using System;
using System.Collections.Generic;
using System.Text;
using Domotech.iRemote.Items;
using Timer = Domotech.iRemote.Items.Timer;

namespace Domotech.iRemote
{
    #region ClientStatus event

    public enum ClientStatusValue
    {
        Disconnected,
        Disposed,
        ConnectionLost,
        Connecting,
        Downloading,
        Ready,
        ConfigChanged
    }

    public class ClientStatusEventArgs : EventArgs
    {
        private ClientStatusValue status;
        private int percent;
        public ClientStatusEventArgs(ClientStatusValue status, int percent)
        {
            this.status = status;
            this.percent = percent;
        }
        public ClientStatusValue Status
        {
            get { return status; }
        }
        public int Percent
        {
            get { return percent; }
        }
    }

    public delegate void ClientStatusEventHandler(object sender, ClientStatusEventArgs e);

    #endregion

    #region ClientItemChanged event

    public class ClientItemChangedEventArgs<T> : EventArgs
    {
        private T item;
        private List<string> properties;
        public ClientItemChangedEventArgs(T item, List<string> properties)
        {
            this.item = item;
            this.properties = properties;
        }

        /// <summary>
        /// Get the item that has one or more changed properties.
        /// </summary>
        public T Item
        {
            get { return item; }
        }

        /// <summary>
        /// Get a list of properties that have changed.
        /// </summary>
        public List<string> Properties
        {
            get { return properties; }
        }
    }

    public delegate void ClientItemChangedEventHandler<T>(object sender, ClientItemChangedEventArgs<T> e);

    #endregion

    public class Client : IDisposable
    {
        #region Declarations

        private bool connected = false;

        internal Core core;
        internal Lists lists;
        internal Poll poll;
        private object listLock = new object();

        #endregion

        #region Public events

        /// <summary>
        /// Event fired when the status of the commander object has changed.
        /// </summary>
        public event ClientStatusEventHandler ClientStatus;

        /// <summary>
        /// Event fired when an alarm has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Alarm> AlarmChanged;

        /// <summary>
        /// Event fired when an audio source has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<AudioSource> AudioSourceChanged;

        /// <summary>
        /// Event fired when an audio zone has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<AudioZone> AudioZoneChanged;

        /// <summary>
        /// Event fired when a temperature curve step has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<CurveStep> CurveStepChanged;

        /// <summary>
        /// Event fired when a dimmer has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Dimmer> DimmerChanged;

        /// <summary>
        /// Event fired when a light has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Light> LightChanged;

        /// <summary>
        /// Event fired when a logical variable has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<LogVar> LogVarChanged;

        /// <summary>
        /// Event fired when a room has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Room> RoomChanged;

        /// <summary>
        /// Event fired when a scenario has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Scenario> ScenarioChanged;

        /// <summary>
        /// Event fired when a shutter has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Shutter> ShutterChanged;

        /// <summary>
        /// Event fired when a socket has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Socket> SocketChanged;

        /// <summary>
        /// Event fired when a timer has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<Timer> TimerChanged;

        /// <summary>
        /// Event fired when a named item has changed.
        /// </summary>
        public event ClientItemChangedEventHandler<NamedItem> ItemChanged;

        #endregion

        #region Construction / Destruction

        public Client()
        {
            core = new Core();
            lists = new Lists(this);
            poll = new Poll(this);
        }

        public void Dispose()
        {
            if (core != null)
            {
                if (connected)
                    Disconnect();

                lock (listLock)
                {
                    if (lists != null)
                    {
                        lists.Dispose();
                        lists = null;
                    }
                }

                if (poll != null)
                {
                    poll.Dispose();
                    poll = null;
                }

                core.Dispose();
                core = null;

                OnClientStatus(ClientStatusValue.Disposed);
            }
        }

        #endregion

        #region Internal event invokers

        /// <summary>
        /// Invokes the ClientStatus event.
        /// </summary>
        /// <param name="status">The current status.</param>
        /// <param name="percent">The current progress position in %.</param>
        internal void OnClientStatus(ClientStatusValue status, int percent)
        {
            if (ClientStatus != null)
            {
                try
                {
                    ClientStatus(this, new ClientStatusEventArgs(status, percent));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the ClientStatus event.
        /// </summary>
        /// <param name="status">The current status.</param>
        internal void OnClientStatus(ClientStatusValue status)
        {
            OnClientStatus(status, -1);
        }

        /// <summary>
        /// Invokes the AlarmChanged event.
        /// </summary>
        /// <param name="alarm">The alarm that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnAlarmChanged(Alarm alarm, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(alarm, properties));
                }
                catch
                {
                }
            }

            if (AlarmChanged != null)
            {
                try
                {
                    AlarmChanged(this, new ClientItemChangedEventArgs<Alarm>(alarm, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the AudioSourceChanged event.
        /// </summary>
        /// <param name="audioSource">The audiosource that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnAudioSourceChanged(AudioSource audioSource, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(audioSource, properties));
                }
                catch
                {
                }
            }

            if (AudioSourceChanged != null)
            {
                try
                {
                    AudioSourceChanged(this, new ClientItemChangedEventArgs<AudioSource>(audioSource, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the AudioZoneChanged event.
        /// </summary>
        /// <param name="audioZone">The audiozone that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnAudioZoneChanged(AudioZone audioZone, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(audioZone, properties));
                }
                catch
                {
                }
            }

            if (AudioZoneChanged != null)
            {
                try
                {
                    AudioZoneChanged(this, new ClientItemChangedEventArgs<AudioZone>(audioZone, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the CurveStepChanged event.
        /// </summary>
        /// <param name="curveStep">The curvestep that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnCurveStepChanged(CurveStep curveStep, List<string> properties)
        {
            if (CurveStepChanged != null)
            {
                try
                {
                    CurveStepChanged(this, new ClientItemChangedEventArgs<CurveStep>(curveStep, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the DimmerChanged event.
        /// </summary>
        /// <param name="dimmer">The dimmer that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnDimmerChanged(Dimmer dimmer, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(dimmer, properties));
                }
                catch
                {
                }
            }

            if (DimmerChanged != null)
            {
                try
                {
                    DimmerChanged(this, new ClientItemChangedEventArgs<Dimmer>(dimmer, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the LichtChanged event.
        /// </summary>
        /// <param name="light">The light that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnLightChanged(Light light, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(light, properties));
                }
                catch
                {
                }
            }

            if (LightChanged != null)
            {
                try
                {
                    LightChanged(this, new ClientItemChangedEventArgs<Light>(light, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the LogVarChanged event.
        /// </summary>
        /// <param name="logVar">The logvar that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnLogVarChanged(LogVar logVar, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(logVar, properties));
                }
                catch
                {
                }
            }

            if (LogVarChanged != null)
            {
                try
                {
                    LogVarChanged(this, new ClientItemChangedEventArgs<LogVar>(logVar, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the RoomChanged event.
        /// </summary>
        /// <param name="room">The room that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnRoomChanged(Room room, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(room, properties));
                }
                catch
                {
                }
            }

            if (RoomChanged != null)
            {
                try
                {
                    RoomChanged(this, new ClientItemChangedEventArgs<Room>(room, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the ScenarioChanged event.
        /// </summary>
        /// <param name="scenario">The scenario that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnScenarioChanged(Scenario scenario, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(scenario, properties));
                }
                catch
                {
                }
            }

            if (ScenarioChanged != null)
            {
                try
                {
                    ScenarioChanged(this, new ClientItemChangedEventArgs<Scenario>(scenario, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the ShutterChanged event.
        /// </summary>
        /// <param name="shutter">The shutter that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnShutterChanged(Shutter shutter, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(shutter, properties));
                }
                catch
                {
                }
            }

            if (ShutterChanged != null)
            {
                try
                {
                    ShutterChanged(this, new ClientItemChangedEventArgs<Shutter>(shutter, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the SocketChanged event.
        /// </summary>
        /// <param name="socket">The socket that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnSocketChanged(Socket socket, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(socket, properties));
                }
                catch
                {
                }
            }

            if (SocketChanged != null)
            {
                try
                {
                    SocketChanged(this, new ClientItemChangedEventArgs<Socket>(socket, properties));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Invokes the TimerChanged event.
        /// </summary>
        /// <param name="timer">The timer that has been changed.</param>
        /// <param name="properties">List of propertynames that have been changed.</param>
        internal void OnTimerChanged(Timer timer, List<string> properties)
        {
            if (ItemChanged != null)
            {
                try
                {
                    ItemChanged(this, new ClientItemChangedEventArgs<NamedItem>(timer, properties));
                }
                catch
                {
                }
            }

            if (TimerChanged != null)
            {
                try
                {
                    TimerChanged(this, new ClientItemChangedEventArgs<Timer>(timer, properties));
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Connect / Disconnect

        /// <summary>
        /// Connect to the Domotech iRemote Server over TCP/IP using the COM objects Connect method.
        /// </summary>
        /// <param name="host">The hostname or IP address of the Domotech iRemote Server.</param>
        /// <param name="port">The TCP portnumber where the Domotech iRemote Server is listening on (default: 33999).</param>
        public void Connect(string host, int port)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            if (connected)
                Disconnect();

            OnClientStatus(ClientStatusValue.Connecting);

            // Try to connect
            Core.ConnectResult result;

            try
            {
                result = core.Connect(host, port);
            }
            catch
            {
                // Translate exception into internal COM error return value
                result = Core.ConnectResult.InternalError;
            }

            try
            {
                // Translate error return values into an Exception message
                switch (result)
                {
                    case Core.ConnectResult.InternalError:
                        throw new Exception("Interne Domotech iRemote COM object fout");
                    case Core.ConnectResult.ConnectionFailed:
                        throw new Exception("Kan geen verbinding maken met de Domotech iRemote Server");
                    case Core.ConnectResult.StatusTimeout:
                        throw new Exception("Domotech iRemote Server timeout");
                    case Core.ConnectResult.TooManyConnections:
                        throw new Exception("Te veel verbindingen met de Domotech iRemote Server");
                    case Core.ConnectResult.AccessDenied:
                        throw new Exception("Toegang tot de Domotech iRemote Server werd geweigerd");
                }
            }
            catch (Exception e)
            {
                OnClientStatus(ClientStatusValue.Disconnected);
                throw e;
            }

            // Connect success, set the client name
            core.SetClientName("dotNET Client");

            // Connection established
            connected = true;

            // Download setup from Domotech iRemote Server
            DownloadSetup();
        }

        /// <summary>
        /// Disconnect from the Domotech iRemote Server.
        /// </summary>
        public void Disconnect()
        {
            if (!connected)
                return;

            try
            {
                // Disconnect
                core.Disconnect();
            }
            catch
            {
            }

            connected = false;

            OnClientStatus(ClientStatusValue.Disconnected);
        }

        #endregion

        #region Download method

        /// <summary>
        /// Download the complete setup from the Domotech iRemote Server.
        /// </summary>
        private void DownloadSetup()
        {
            lock (listLock)
            {
                if (lists == null)
                    throw new ObjectDisposedException("Lists");

                OnClientStatus(ClientStatusValue.Downloading, 0);
                lists.DownloadSocketList();
                OnClientStatus(ClientStatusValue.Downloading, 9);
                lists.DownloadLightList();
                OnClientStatus(ClientStatusValue.Downloading, 18);
                lists.DownloadLogVarList();
                OnClientStatus(ClientStatusValue.Downloading, 27);
                lists.DownloadDimmerList();
                OnClientStatus(ClientStatusValue.Downloading, 36);
                lists.DownloadShutterList();
                OnClientStatus(ClientStatusValue.Downloading, 45);
                lists.DownloadRoomList();
                OnClientStatus(ClientStatusValue.Downloading, 55);
                lists.DownloadAlarmList();
                OnClientStatus(ClientStatusValue.Downloading, 64);
                lists.DownloadAudioZoneList();
                OnClientStatus(ClientStatusValue.Downloading, 73);
                lists.DownloadAudioSourceList();
                OnClientStatus(ClientStatusValue.Downloading, 82);
                lists.DownloadScenarioList();
                OnClientStatus(ClientStatusValue.Downloading, 91);
                lists.DownloadTimerList();
                OnClientStatus(ClientStatusValue.Downloading, 100);
                lists.audioInitOk = false;
                OnClientStatus(ClientStatusValue.Ready);
            }
        }

        #endregion

        #region Public wrapped methods

        /// <summary>
        /// Sets the state of all Sockets in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        public void SetAllSocketsState(bool state)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.SetAllSocketsState(state);
        }

        /// <summary>
        /// Sets the state of all Lights in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        public void SetAllLightsState(bool state)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.SetAllLightsState(state);
        }

        /// <summary>
        /// Sets the state of all LogVars in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        public void SetAllLogVarsState(bool state)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.SetAllLogVarsState(state);
        }

        /// <summary>
        /// Sets the state of all Dimmers in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        public void SetAllDimmersState(bool state)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.SetAllDimmersState(state);
        }

        /// <summary>
        /// Reset all alarms.
        /// </summary>
        public void AlarmReset()
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.ResetAlarms();
        }

        /// <summary>
        /// Stops the alarm signal on all LCD panels.
        /// </summary>
        public void AlarmSilence()
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.SetAlarmSilence();
        }

        /// <summary>
        /// Sets the state of all AudioZones in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        public void SetAllAudioZonesState(bool state)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.SetAllAudioZonesState(state);
        }

        /// <summary>
        /// Gets the human readable description for an instruction.
        /// </summary>
        /// <param name="index">The instruction index</param>
        /// <returns>The human readable description</returns>
        public string GetInstructionText(int index)
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            string text;
            core.GetInstructionText(index, out text);
            return text;
        }

        /// <summary>
        /// Forces the Domotech Master to initialize the Audio interface.
        /// </summary>
        public void AudioInit()
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            core.AudioInit();
        }

        #endregion

        #region Poll method

        /// <summary>
        /// Retreives update messages from the Domotech iRemote Server.
        /// </summary>
        /// <returns>A boolean representing the connection state. Returns true when still connected, false when not.</returns>
        public bool Poll()
        {
            if (core == null)
                throw new ObjectDisposedException("Core");

            if (poll == null)
                throw new ObjectDisposedException("Poll");

            try
            {
                lock (listLock)
                {
                    if (lists == null)
                        throw new ObjectDisposedException("Lists");

                    poll.Execute();
                }
                return true;
            }
            catch (ObjectDisposedException e)
            {
                if (e.ObjectName == "Lists")
                    throw e;

                connected = false;

                // TCP link with the Domotech iRemote Server is down.
                OnClientStatus(ClientStatusValue.ConnectionLost);

                return false;
            }
            catch
            {
                connected = false;

                // TCP link with the Domotech iRemote Server is down.
                OnClientStatus(ClientStatusValue.ConnectionLost);

                return false;
            }
        }

        #endregion

        #region Item accessors

        /// <summary>
        /// Gets a socket by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The socket or null</returns>
        public Socket GetSocket(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.socketList.Count ? lists.socketList[index] : null;
            }
        }

        /// <summary>
        /// Gets a light by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The light or null</returns>
        public Light GetLight(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.lightList.Count ? lists.lightList[index] : null;
            }
        }

        /// <summary>
        /// Gets a logical variable by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The logical variable or null</returns>
        public LogVar GetLogVar(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.logVarList.Count ? lists.logVarList[index] : null;
            }
        }

        /// <summary>
        /// Gets a dimmer by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The dimmer or null</returns>
        public Dimmer GetDimmer(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.dimmerList.Count ? lists.dimmerList[index] : null;
            }
        }

        /// <summary>
        /// Gets a shutter by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The shutter or null</returns>
        public Shutter GetShutter(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.shutterList.Count ? lists.shutterList[index] : null;
            }
        }

        /// <summary>
        /// Gets a room by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The room or null</returns>
        public Room GetRoom(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.roomList.Count ? lists.roomList[index] : null;
            }
        }

        /// <summary>
        /// Gets an alarm by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The alarm or null</returns>
        public Alarm GetAlarm(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.alarmList.Count ? lists.alarmList[index] : null;
            }
        }

        /// <summary>
        /// Gets an audio zone by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The audio zone or null</returns>
        public AudioZone GetAudioZone(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.audioZoneList.Count ? lists.audioZoneList[index] : null;
            }
        }

        /// <summary>
        /// Gets an audio source by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The audio source or null</returns>
        public AudioSource GetAudioSource(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.audioSourceList.Count ? lists.audioSourceList[index] : null;
            }
        }

        /// <summary>
        /// Gets a scenario by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The scenario or null</returns>
        public Scenario GetScenario(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.scenarioList.Count ? lists.scenarioList[index] : null;
            }
        }

        /// <summary>
        /// Gets a timer by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The timer or null</returns>
        public Timer GetTimer(int index)
        {
            lock (listLock)
            {
                if ((lists == null) || (index < 0))
                    return null;

                return index < lists.timerList.Count ? lists.timerList[index] : null;
            }
        }

        #endregion

        #region Item enumerators

        /// <summary>
        /// Enumerates all sockets known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of sockets.</returns>
        public IEnumerable<Socket> Sockets()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Socket socket in lists.socketList)
                        yield return socket;
        }

        /// <summary>
        /// Enumerates all lights known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of lights.</returns>
        public IEnumerable<Light> Lights()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Light light in lists.lightList)
                        yield return light;
        }

        /// <summary>
        /// Enumerates all logvars known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of logvars.</returns>
        public IEnumerable<LogVar> LogVars()
        {
            lock (listLock)
                if (lists != null)
                    foreach (LogVar logVar in lists.logVarList)
                        yield return logVar;
        }

        /// <summary>
        /// Enumerates all dimmers known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of dimmers.</returns>
        public IEnumerable<Dimmer> Dimmers()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Dimmer dimmer in lists.dimmerList)
                        yield return dimmer;
        }

        /// <summary>
        /// Enumerates all shutters known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of shutters.</returns>
        public IEnumerable<Shutter> Shutters()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Shutter shutter in lists.shutterList)
                        yield return shutter;
        }

        /// <summary>
        /// Enumerates all rooms known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of rooms.</returns>
        public IEnumerable<Room> Rooms()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Room room in lists.roomList)
                        yield return room;
        }

        /// <summary>
        /// Enumerates all alarms known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of alarms.</returns>
        public IEnumerable<Alarm> Alarms()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Alarm alarm in lists.alarmList)
                        yield return alarm;
        }

        /// <summary>
        /// Enumerates all audiozones known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of audiozones.</returns>
        public IEnumerable<AudioZone> AudioZones()
        {
            lock (listLock)
                if (lists != null)
                    foreach (AudioZone audioZone in lists.audioZoneList)
                        yield return audioZone;
        }

        /// <summary>
        /// Enumerates all audiosources known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of audiosources.</returns>
        public IEnumerable<AudioSource> AudioSources()
        {
            lock (listLock)
                if (lists != null)
                    foreach (AudioSource audioSource in lists.audioSourceList)
                        yield return audioSource;
        }

        /// <summary>
        /// Enumerates all scenarios known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of scenarios.</returns>
        public IEnumerable<Scenario> Scenarios()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Scenario scenario in lists.scenarioList)
                        yield return scenario;
        }

        /// <summary>
        /// Enumerates all timers known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of timers.</returns>
        public IEnumerable<Timer> Timers()
        {
            lock (listLock)
                if (lists != null)
                    foreach (Timer timer in lists.timerList)
                        yield return timer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the number of sockets known by iRemote.
        /// </summary>
        public int SocketCount
        {
            get { lock (listLock) return (lists != null) ? lists.socketList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of lights known by iRemote.
        /// </summary>
        public int LightCount
        {
            get { lock (listLock) return (lists != null) ? lists.lightList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of logvars known by iRemote.
        /// </summary>
        public int LogVarCount
        {
            get { lock (listLock) return (lists != null) ? lists.logVarList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of dimmers known by iRemote.
        /// </summary>
        public int DimmerCount
        {
            get { lock (listLock) return (lists != null) ? lists.dimmerList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of shutters known by iRemote.
        /// </summary>
        public int ShutterCount
        {
            get { lock (listLock) return (lists != null) ? lists.shutterList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of rooms known by iRemote.
        /// </summary>
        public int RoomCount
        {
            get { lock (listLock) return (lists != null) ? lists.roomList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of alarms known by iRemote.
        /// </summary>
        public int AlarmCount
        {
            get { lock (listLock) return (lists != null) ? lists.alarmList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of audiozones known by iRemote.
        /// </summary>
        public int AudioZoneCount
        {
            get { lock (listLock) return (lists != null) ? lists.audioZoneList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of audiosources known by iRemote.
        /// </summary>
        public int AudioSourceCount
        {
            get { lock (listLock) return (lists != null) ? lists.audioSourceList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of scenarios known by iRemote.
        /// </summary>
        public int ScenarioCount
        {
            get { lock (listLock) return (lists != null) ? lists.scenarioList.Count : 0; }
        }

        /// <summary>
        /// Returns the number of timers known by iRemote.
        /// </summary>
        public int TimerCount
        {
            get { lock (listLock) return (lists != null) ? lists.timerList.Count : 0; }
        }

        /// <summary>
        /// Returns the state of the TCP/IP connection with the Domotech iRemote Server
        /// </summary>
        public bool Connected
        {
            get { return connected; }
        }

        #endregion
    }
}
