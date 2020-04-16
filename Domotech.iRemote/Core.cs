using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Domotech.iRemote
{
    /// <summary>
    /// This class implements all low-level Domotech iRemote methods
    /// </summary>
    internal class Core : IDisposable
    {
        #region Declarations / Consts / Enums

        private TcpClient socket = null;
        private NetworkStream stream = null;
        private bool pollingActive = false;
        private Queue<string> pollMessages = new Queue<string>();

        private const byte versionHi = 2;
        private const byte versionLo = 0;

        private class ExceptionMessages
        {
            public const string ValueOutOfRange = "Value out of range";
            public const string CommunicationError = "Communication error";
            public const string PollingNotActive = "Polling not active";
            public const string InterfaceNotImplemented = "Interface not implemented";
        }

        public enum ConnectResult : int
        {
            Success = 0,
            InternalError = 1,
            ConnectionFailed = 2,
            StatusTimeout = 3,
            TooManyConnections = 4,
            AccessDenied = 5,
        }
        
        #endregion

        #region Construction / Destruction

        public Core()
        {
        }

        public void Dispose()
        {
            Disconnect();
        }

        #endregion

        #region Private methods

        private void SendCommand(string command)
        {
            byte[] buffer = new byte[128];

            byte[] encodedCommand = ASCIIEncoding.ASCII.GetBytes(command);

            Array.Copy(encodedCommand, buffer, Math.Min(128, encodedCommand.Length));

            try
            {
                stream.Write(buffer, 0, 128);
            }
            catch
            {
                throw new Exception(ExceptionMessages.CommunicationError);
            }
        }

        #endregion

        #region Static internal methods

        /// <summary>
        /// Converts a temperature to a value that can be send to the Domotech Master.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        internal static int DegreesToTemp(float degrees)
        {	// -55 <= Temp <= 125
	        if (degrees < -55) degrees = -55;
	        if (degrees > 125) degrees = 125;
	        degrees *= 2.0f;
	        int temp = (int)degrees;
	        if (temp >= 0) return temp; // Positive
            temp = 256 + ((((-temp) - 1) ^ 0xFF) & 0xFF);
            return temp;	// Negative
        }

        /// <summary>
        /// Converts a temperature value received from the Domotech Master into a float value.
        /// </summary>
        /// <param name="hi">The higher part of the temperature value</param>
        /// <param name="lo">The lower part of the temperature value</param>
        /// <returns>The converted temperature</returns>
        internal static float TempToDegrees(byte hi, byte lo)
        {
            int temperature = lo;
            if (hi != 0) temperature += 256;
            return TempToDegrees(temperature);
        }
        
        /// <summary>
        /// Converts a temperature correction value received from the Domotech Master into a float value.
        /// </summary>
        /// <param name="correction">The temperature correction value to convert</param>
        /// <returns>The converted temperature correction</returns>
        internal static float CorrectionToDegrees(byte correction)
        {
            return (float)correction / 2.0f - 7.5f;
        }

        /// <summary>
        /// Converts a temperature value received from the Domotech Master into a float value.
        /// </summary>
        /// <param name="temp">The temperature value to convert</param>
        /// <returns>The converted temperature</returns>
        internal static float TempToDegrees(int temp)
        {
            if (temp >= 0x100)
                temp = -(0xFF ^ (temp & 0xFF)) + 1;
            return (float)temp / 2.0f;
        }

        /// <summary>
        /// Converts the audio volume value received from the Domotech Master into an int value.
        /// </summary>
        /// <param name="value">The audio volume value to convert</param>
        /// <returns>The converted audio volume</returns>
        internal static int ValueToVolume(byte value)
        {
            return -80 + (int)value * 2;
        }

        /// <summary>
        /// Converts the audio bass or treble value received from the Domotech Master into an int value.
        /// </summary>
        /// <param name="value">The audio bass or treble value to convert</param>
        /// <returns>The converted audio bass or treble value</returns>
        internal static int ValueToBassTreble(byte value)
        {
            return -12 + (int)value * 2;
        }

        #endregion

        #region Connect / Disconnect

        public ConnectResult Connect(string hostname, int port)
        {
            Disconnect();

            lock (this)
            {
                try
                {
                    socket = new TcpClient();

                    socket.Connect(hostname, port);

                    // Connection established, get stream
                    stream = socket.GetStream();

                    stream.WriteTimeout = 1000;
                    stream.ReadTimeout = 5000;

                    try
                    {
                        // Receive status
                        int status = stream.ReadByte();

                        if (status != 0)
                        {
                            Disconnect();

                            switch (status)
                            {
                                case -1:
                                    return ConnectResult.StatusTimeout;
                                case 1:
                                    return ConnectResult.TooManyConnections;
                                case 2:
                                    return ConnectResult.AccessDenied;
                                default:
                                    return ConnectResult.InternalError;
                            }
                        }
                    }
                    catch
                    {
                        Disconnect();
                        return ConnectResult.InternalError;
                    }
                }
                catch
                {
                    return ConnectResult.ConnectionFailed;
                }

                pollingActive = true;

                return ConnectResult.Success;
            }
        }

        public void Disconnect()
        {
            lock (this)
            {
                pollMessages.Clear();

                if (socket != null)
                {
                    if (socket.Client.Connected)
                        socket.Client.Close();

                    socket.Close();
                    socket = null;
                }

                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
        }

        #endregion

        #region Public methods

        public void AudioInit()
        {
            lock (this)
            {
                SendCommand("AUDIOINIT");
            }
        }

        public void Broadcast(string message)
        {
            if (message.Length > 125)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("B:" + message);
            }
        }

        public void DimmerSlaveUpdate(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("DIMUPDATE:" + index.ToString());
            }
        }

        public void GetAlarmInfo(int index, out string name, out bool active, out bool wasActive, out byte timesOn, out bool beep)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ALARMTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 4) != 4)
                        throw new Exception();

                    active = (buffer[0] != 0);
                    wasActive = (buffer[1] != 0);
                    timesOn = buffer[2];
                    beep = (buffer[3] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetAudioSourceInfo(int index, out string name, out AudioSourceType type)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("AUSRCTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 1) != 1)
                        throw new Exception();

                    type = (AudioSourceType)(buffer[0] & 15);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetAudioZoneInfo(int index, out string name, out bool state, out bool mute, out byte inputSource, out int volume, out int bass, out int treble)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("AUZONTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 6) != 6)
                        throw new Exception();

                    state = (buffer[0] != 0);
                    mute = (buffer[1] != 0);
                    inputSource = buffer[2];
                    volume = ValueToVolume(buffer[3]);
                    bass = ValueToBassTreble(buffer[4]);
                    treble = ValueToBassTreble(buffer[5]);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetCurveStepInfo(int roomIndex, int stepIndex, out byte day, out byte hour, out byte minute, out float temperature)
        {
            if ((roomIndex < 0) || (stepIndex < 0))
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("VERLOOPTAB:" + roomIndex.ToString() + ":" + stepIndex.ToString());

                try
                {
                    byte[] buffer = new byte[5];

                    // Ontvang info
                    if (stream.Read(buffer, 0, 5) != 5)
                        throw new Exception();

                    day = buffer[0];
                    hour = buffer[1];
                    minute = buffer[2];
                    temperature = TempToDegrees(buffer[3], buffer[4]);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetDimmerInfo(int index, out string name, out byte value, out bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("DIMMERTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 2) != 2)
                        throw new Exception();

                    value = buffer[0];
                    state = (buffer[1] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetInstructionText(int index, out string text)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("BUILDPROGTEXT:" + index.ToString());

                byte[] buffer = new byte[2048];

                try
                {
                    // Get stringlength
                    if (stream.Read(buffer, 0, 2) != 2)
                        throw new Exception();

                    int length = ((int)buffer[0] << 8) + buffer[1];

                    if (length > 0)
                    {
                        if (stream.Read(buffer, 0, length) != length)
                            throw new Exception();

                        text = ASCIIEncoding.ASCII.GetString(buffer, 0, length);
                    }
                    else
                        text = "";
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetLightInfo(int index, out string name, out bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("LAMPTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 1) != 1)
                        throw new Exception();

                    state = (buffer[0] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetLogVarInfo(int index, out string name, out bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("LOGVARSTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 1) != 1)
                        throw new Exception();

                    state = (buffer[0] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public string GetPollMessage()
        {
            if (!pollingActive)
                throw new Exception(ExceptionMessages.PollingNotActive);

            lock (this)
            {
                return (pollMessages.Count > 0) ? pollMessages.Dequeue() : "";
            }
        }

        public void GetRoomInfo(int index, out string name, out float tempCorrection, out float measuredTemp, out float dayTemp, out float nightTemp, out RoomTempType tempType, out bool tempEnabled, out bool tempActive, out float aircoTemp, out RoomAircoType aircoType, out bool aircoEnabled, out bool aircoActive, out int curveSteps, out bool outside)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTETAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 18) != 18)
                        throw new Exception();

                    tempCorrection = CorrectionToDegrees(buffer[0]);
                    measuredTemp = TempToDegrees(buffer[1], buffer[2]);
                    dayTemp = TempToDegrees(buffer[3], buffer[4]);
                    nightTemp = TempToDegrees(buffer[5], buffer[6]);
                    tempType = (buffer[7] == 0) ? RoomTempType.Auto : (RoomTempType)buffer[7];
                    tempEnabled = (buffer[8] != 0);
                    tempActive = (buffer[9] != 0);
                    aircoTemp = TempToDegrees(buffer[10], buffer[11]);
                    aircoType = (RoomAircoType)buffer[12];
                    aircoEnabled = (buffer[13] != 0);
                    aircoActive = (buffer[14] != 0);
                    curveSteps = (buffer[15] << 8) + buffer[16];
                    outside = (buffer[17] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetScenarioInfo(int index, out string name, out ScenarioType type)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("SFEERTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 1) != 1)
                        throw new Exception();

                    type = (buffer[0] == 1) ? ScenarioType.Repeated : ScenarioType.Once;
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetShutterInfo(int index, out string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ROLTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetSocketInfo(int index, out string name, out bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("STOPTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[100];

                    // Get stringlength
                    int length = stream.ReadByte();
                    if (length < 0)
                        throw new Exception();

                    // Ontvang Naam
                    if (stream.Read(buffer, 0, length) != length)
                        throw new Exception();

                    name = ASCIIEncoding.ASCII.GetString(buffer, 0, length);

                    // Ontvang info
                    if (stream.Read(buffer, 0, 1) != 1)
                        throw new Exception();

                    state = (buffer[0] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetTelephoneInfo(int index, out int instructionIndex)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("TELEFOONTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[2];

                    // Ontvang info
                    if (stream.Read(buffer, 0, 2) != 2)
                        throw new Exception();

                    instructionIndex = ((int)buffer[0] << 8) + (int)buffer[1];
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void GetTimerInfo(int index, out int instructionIndex, out byte day, out byte hour, out byte minute, out bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("TIMERTAB:" + index.ToString());

                try
                {
                    byte[] buffer = new byte[6];

                    // Ontvang info
                    if (stream.Read(buffer, 0, 6) != 6)
                        throw new Exception();

                    instructionIndex = ((int)buffer[0] << 8) + (int)buffer[1];
                    day = buffer[2];
                    hour = buffer[3];
                    minute = buffer[4];
                    state = (buffer[5] != 0);
                }
                catch (Exception)
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }
            }
        }

        public void InstructionExecute(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("PROGEXEC:" + index.ToString());
            }
        }

        public void Ping()
        {
            lock (this)
            {
                SendCommand("PING");
            }
        }

        public int Poll()
        {
            if (!pollingActive)
                throw new Exception(ExceptionMessages.PollingNotActive);

            byte[] buffer = new byte[128];

            lock (this)
            {
                SendCommand("POLL");

                try
                {
                    if (stream.Read(buffer, 0, 2) != 2)
                        throw new Exception();

                    int count = ((int)buffer[0] << 8) + (int)buffer[1];

                    for (int i = 0; i < count; i++)
                    {
                        if (stream.Read(buffer, 0, 128) != 128)
                            throw new Exception();

                        string message = ASCIIEncoding.ASCII.GetString(buffer, 0, 128).TrimEnd(new char[] { '\0', ' ' });

                        pollMessages.Enqueue(message);
                    }
                }
                catch
                {
                    throw new Exception(ExceptionMessages.CommunicationError);
                }

                return pollMessages.Count;
            }
        }

        public void ResetAlarms()
        {
            lock (this)
            {
                SendCommand("ALHST");
            }
        }

        public void ScenarioExecute(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("SCENARIO:" + index.ToString());
            }
        }

        public void ScenarioSave(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("SCENSAVE" + index.ToString());
            }
        }

        public void SetAlarmActive(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ALACT:" + index.ToString());
            }
        }

        public void SetAlarmName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ALTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetAlarmSilence()
        {
            lock (this)
            {
                SendCommand("ALSIL");
            }
        }

        public void SetAlarmState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "ALAAN:" : "ALUIT:") + index.ToString());
            }
        }

        public void SetAllAlarmsOff()
        {
            lock (this)
            {
                SendCommand("ALALLESUIT");
            }
        }

        public void SetAllAudioZonesState(bool state)
        {
            lock (this)
            {
                SendCommand(state ? "AUALLESAAN" : "AUALLESUIT");
            }
        }

        public void SetAllDimmersState(bool state)
        {
            lock (this)
            {
                SendCommand(state ? "DIMALLESAAN" : "DIMALLESUIT");
            }
        }

        public void SetAllLightsState(bool state)
        {
            lock (this)
            {
                SendCommand(state ? "LAMPALLESAAN" : "LAMPALLESUIT");
            }
        }

        public void SetAllLogVarsState(bool state)
        {
            lock (this)
            {
                SendCommand(state ? "LOGVARALLESAAN" : "LOGVARALLESUIT");
            }
        }

        public void SetAllSocketsState(bool state)
        {
            lock (this)
            {
                SendCommand(state ? "STOPALLESAAN" : "STOPALLESUIT");
            }
        }

        public void SetAudioParam(int index, bool allZones, AudioParamType type, int value, bool execute, bool lcdUpdate)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            switch (type)
            {
                case AudioParamType.Volume:
                    if (value < -80) value = -80;
                    if (value > 0) value = 0;
                    value = (value + 80) / 2;
                    break;
                case AudioParamType.Bass:
                case AudioParamType.Treble:
                    if (value < -12) value = -12;
                    if (value > 12) value = 12;
                    value = (value + 12) / 2;
                    break;
                case AudioParamType.Mute:
                    if (value != 0) value = 1;
                    break;
                case AudioParamType.InputSource:
                    if (value < 0) value = 0;
                    if (value > 7) value = 7;
                    break;
            }

            lock (this)
            {
                SendCommand("AUINST:" + index.ToString() + ":" +
                    (allZones ? "1" : "0") + ":" +
                    ((byte)type).ToString() + ":" +
                    value.ToString() + ":" +
                    (execute ? "1" : "0") + ":" +
                    (lcdUpdate ? "1" : "0"));
            }
        }

        public void SetAudioSourceName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("AUSRCTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetAudioZoneName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("AUZONTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetAudioZoneState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "AUAAN:" : "AUUIT:") + index.ToString());
            }
        }

        public void SetClientName(string name)
        {
            if (name.Length > 24)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("CLIENTNAME:" + name);
            }
        }

        public void SetCurveStepTemp(int roomIndex, int stepIndex, float temperature)
        {
            if ((roomIndex < 0) || (stepIndex < 0))
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("VERLOOPTEMP:" + roomIndex.ToString() + ":" + stepIndex.ToString() + ":" + DegreesToTemp(temperature).ToString());
            }
        }

        public void SetCurveStepTime(int roomIndex, int stepIndex, byte hour, byte minute)
        {
            if ((roomIndex < 0) || (stepIndex < 0))
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (hour > 23)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (minute > 59)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("VERLOOPTIME:" + roomIndex.ToString() + ":" + stepIndex.ToString() + ":" + hour.ToString() + ":" + minute.ToString());
            }
        }

        public void SetDimmerActive(long index, bool active)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((active ? "DIMHND:" : "DIMNUL:") + index.ToString());
            }
        }

        public void SetDimmerName(long index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("DIMTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetDimmerState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "DIMAAN:" : "DIMUIT:") + index.ToString());
            }
        }

        public void SetDimmerValue(int index, byte value)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (value > 100) value = 100;

            lock (this)
            {
                SendCommand("DIMINST:" + index.ToString() + ":" + value.ToString());
            }
        }

        public void SetLightName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("LAMPTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetLightState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "LAMPAAN:" : "LAMPUIT:") + index.ToString());
            }
        }

        public void SetLogVarName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("LOGVARTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetLogVarState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "LOGVARAAN:" : "LOGVARUIT:") + index.ToString());
            }
        }

        public void SetPollState(bool state)
        {
            lock (this)
            {
                SendCommand(state ? "POLLON" : "POLLOFF");

                pollingActive = state;

                // Clear list when polling is disabled
                if (!state) pollMessages.Clear();
            }
        }

        public void SetRoomAircoState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTEAIRCOONOFF:" + index.ToString() + ":" + (state ? "1" : "0"));
            }
        }

        public void SetRoomAircoTemp(int index, float temperature)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTEAIRCOTEMP:" + index.ToString() + ":" + DegreesToTemp(temperature).ToString());
            }
        }

        public void SetRoomAircoType(int index, RoomAircoType type)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTEAIRCOTYPE:" + index.ToString() + ":" + ((byte)type).ToString());
            }
        }

        public void SetRoomDayTemp(int index, float temperature)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTEDAGTEMP:" + index.ToString() + ":" + DegreesToTemp(temperature).ToString());
            }
        }

        public void SetRoomName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTETEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetRoomNightTemp(int index, float temperature)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTENACHTTEMP:" + index.ToString() + ":" + DegreesToTemp(temperature).ToString());
            }
        }

        public void SetRoomTempCorrection(int index, float correction)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (correction > 7.5f) correction = 7.5f;
            if (correction < -7.5f) correction = -7.5f;

            lock (this)
            {
                SendCommand("RUIMTECORRECTIE:" + index.ToString() + ":" + ((int)(correction * 2) + 15).ToString());
            }
        }

        public void SetRoomTempState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTETEMPONOFF:" + index.ToString() + ":" + (state ? "1" : "0"));
            }
        }

        public void SetRoomTempType(int index, RoomTempType type)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("RUIMTETEMPTYPE:" + index.ToString() + ":" + ((byte)type).ToString());
            }
        }

        public void SetScenarioName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("SCENTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetShutterName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ROLTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetSocketName(int index, string name)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("STOPTEXTUPD:" + index.ToString() + ":" + name);
            }
        }

        public void SetSocketState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "STOPAAN:" : "STOPUIT:") + index.ToString());
            }
        }

        public void SetTimerState(int index, bool state)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand((state ? "TIMAAN:" : "TIMUIT:") + index.ToString());
            }
        }

        public void SetTimerTime(int index, byte day, byte hour, byte minute)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (day > 6)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (hour > 23)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            if (minute > 59)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("TIMINST:" + index.ToString() + ":" + day.ToString() + ":" + hour.ToString() + ":" + minute.ToString());
            }
        }

        public void ShutterClose(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ROLNEER:" + index.ToString());
            }
        }

        public void ShutterOpen(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ROLOP:" + index.ToString());
            }
        }

        public void ShutterStop(int index)
        {
            if (index < 0)
                throw new Exception(ExceptionMessages.ValueOutOfRange);

            lock (this)
            {
                SendCommand("ROLSTOP:" + index.ToString());
            }
        }

        #endregion

        #region Properties

        public int AlarmCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("ALARMTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int AudioSourceCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("AUSRCTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int AudioZoneCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("AUZONTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int DimmerCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("DIMMERTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int InstructionCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("PROGTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int LightCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("LAMPTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int LogVarCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("LOGVARSTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public byte MasterVersion
        {
            get
            {
                byte[] buffer = new byte[1];

                lock (this)
                {
                    SendCommand("MASTERVERSIE");

                    if (stream.Read(buffer, 0, 1) != 1)
                        throw new Exception(ExceptionMessages.CommunicationError);
                }

                return buffer[0];
            }
        }

        public int RoomCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("RUIMTETAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int ScenarioCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("SFEERTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int ShutterCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("ROLTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int SocketCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("STOPTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int TelephoneInstructionCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("TELEFOONTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public int TimerCount
        {
            get
            {
                byte[] buffer = new byte[2];

                lock (this)
                {
                    SendCommand("TIMERTAB");

                    try
                    {
                        if (stream.Read(buffer, 0, 2) != 2)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        throw new Exception(ExceptionMessages.CommunicationError);
                    }
                }

                return ((int)buffer[0] << 8) + (int)buffer[1];
            }
        }

        public byte VersionHi
        {
            get { return versionHi; }
        }

        public byte VersionLo
        {
            get { return versionLo; }
        }

        #endregion
    }
}