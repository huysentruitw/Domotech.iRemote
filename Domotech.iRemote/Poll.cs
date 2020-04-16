using System;
using System.Collections.Generic;
using System.Text;
using Domotech.iRemote.Items;

namespace Domotech.iRemote
{
    internal class Poll : IDisposable
    {
        #region Declarations

        private Client client;
        private string broadcastClientName = "unknown";

        #endregion

        #region Construction / Destruction

        public Poll(Client client)
        {
            this.client = client;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Execute method

        /// <summary>
        /// Retreives update messages from the Domotech iRemote Server.
        /// </summary>
        public void Execute()
        {
            // Query the number of update messages from the Domotech iRemote Server
            int messageCount = client.core.Poll();

            // Query and parse the update messages
            for (int i = 0; i < messageCount; i++)
                ParsePollMessage(client.core.GetPollMessage());
        }

        #endregion

        #region ParseMessage method

        /// <summary>
        /// Parse the content of one update message.
        /// </summary>
        /// <param name="message">The update message as received from the Domotech iRemote Server</param>
        private void ParsePollMessage(string message)
        {
            // The update message is a token string with a colon as separator
            string[] msg = message.Split(':');  // Split the message into tokens

            int index;

            try
            {
                // The second number mostly represents the item index
                index = int.Parse(msg[1]);
            }
            catch
            {
                index = 0;
            }

            int stepIndex;
            
            List<string> properties = new List<string>();
            
            switch (msg[0]) // The first number always represents the message type
            {
                #region ACHANGED
                case "ACHANGED":
                    {   // Alarm parameter(s) changed
                        Alarm alarm = client.lists.alarmList[index];
                        
                        bool active = (byte.Parse(msg[2]) != 0);
                        bool wasActive = (byte.Parse(msg[3]) != 0);
                        byte timesOn = byte.Parse(msg[4]);

                        if (alarm.active != active)
                        {
                            alarm.active = active;
                            properties.Add("Active");
                        }

                        if (alarm.wasActive != wasActive)
                        {
                            alarm.wasActive = wasActive;
                            properties.Add("WasActive");
                        }

                        if (alarm.timesOn != timesOn)
                        {
                            alarm.timesOn = timesOn;
                            properties.Add("TimesOn");
                        }

                        client.OnAlarmChanged(alarm, properties);
                    }
                    break;
                #endregion
                #region AIRCOCHANGED
                case "AIRCOCHANGED":
                    {   // Room airco temperature changed
                        Room room = client.lists.roomList[index];

                        room.aircoTemp = Core.TempToDegrees(int.Parse(msg[2]));
                        properties.Add("AircoTemp");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region AIRCOONOFFCHANGED
                case "AIRCOONOFFCHANGED":
                    {   // Room airco state changed
                        Room room = client.lists.roomList[index];

                        room.aircoEnabled = (byte.Parse(msg[2]) != 0);
                        properties.Add("AircoEnabled");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region AIRCOTCHANGED
                case "AIRCOTCHANGED":
                    {   // Room airco type changed
                        Room room = client.lists.roomList[index];

                        room.aircoType = (RoomAircoType)byte.Parse(msg[2]);
                        properties.Add("AircoType");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                /*case "ALARMNROK":
                    break;*/
                /*case "ALARMOFF":
                    break;*/
                #region ALARMTEXTUPD
                case "ALARMTEXTUPD":
                    {   // Alarm text changed
                        Alarm alarm = client.lists.alarmList[index];

                        alarm.name = msg[2];
                        properties.Add("Name");

                        client.OnAlarmChanged(alarm, properties);
                    }
                    break;
                #endregion
                #region ALLCHANGED
                case "ALLCHANGED":
                    // Recall init
                    client.OnClientStatus(ClientStatusValue.ConfigChanged);
                    client.Disconnect();    // Thread will reconnect & redownload
                    break;
                #endregion
                /*case "ALSIL":
                    break;*/
                /*case "ARESET":
                    break;*/
                #region ASCHANGED
                case "ASCHANGED":
                    {   // Room airco activity changed
                        Room room = client.lists.roomList[index];

                        room.aircoActive = (byte.Parse(msg[2]) != 0);
                        properties.Add("AircoActive");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region AU_BASCHANGED
                case "AU_BASCHANGED":
                    {   // AudioZone bass value changed
                        AudioZone audioZone = client.lists.audioZoneList[index];
                        
                        audioZone.bass = Core.ValueToBassTreble(byte.Parse(msg[2]));
                        properties.Add("Bass");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region AU_MUTECHANGED
                case "AU_MUTECHANGED":
                    {   // AudioZone mute value changed
                        AudioZone audioZone = client.lists.audioZoneList[index];

                        audioZone.mute = (byte.Parse(msg[2]) != 0);
                        properties.Add("Mute");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region AU_ONOFFCHANGED
                case "AU_ONOFFCHANGED":
                    {   // AudioZone state changed
                        AudioZone audioZone = client.lists.audioZoneList[index];

                        audioZone.state = (byte.Parse(msg[2]) != 0);
                        properties.Add("State");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region AU_SRCCHANGED
                case "AU_SRCCHANGED":
                    {   // AudioZone source changed
                        AudioZone audioZone = client.lists.audioZoneList[index];

                        audioZone.inputSource = byte.Parse(msg[2]);
                        properties.Add("InputSource");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region AU_TRECHANGED
                case "AU_TRECHANGED":
                    {   // AudioZone treble changed
                        AudioZone audioZone = client.lists.audioZoneList[index];

                        audioZone.treble = Core.ValueToBassTreble(byte.Parse(msg[2]));
                        properties.Add("Treble");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region AU_VOLCHANGED
                case "AU_VOLCHANGED":
                    {   // AudioZone volume changed
                        AudioZone audioZone = client.lists.audioZoneList[index];

                        audioZone.volume = Core.ValueToVolume(byte.Parse(msg[2]));
                        properties.Add("Volume");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region AUDIOINIT
                case "AUDIOINIT":
                    // Return value of AudioInit
                    // 1 when audiocontroller found, 0 when not
                    client.lists.audioInitOk = (byte.Parse(msg[1]) != 0);
                    break;
                #endregion
                /*case "AUDIONROK":
                    break;*/
                #region AUSRCTEXTUPD
                case "AUSRCTEXTUPD":
                    {   // AudioSrouce text changed
                        AudioSource audioSource = client.lists.audioSourceList[index];

                        audioSource.name = msg[2];
                        properties.Add("Name");

                        client.OnAudioSourceChanged(audioSource, properties);
                    }
                    break;
                #endregion
                #region AUZONTEXTUPD
                case "AUZONTEXTUPD":
                    {   // AudioZone text changed
                        AudioZone audioZone = client.lists.audioZoneList[index];
                        
                        audioZone.name = msg[2];
                        properties.Add("Name");

                        client.OnAudioZoneChanged(audioZone, properties);
                    }
                    break;
                #endregion
                #region B
                case "B":
                    // Broadcast message from broadcastClientName
                    break;
                #endregion
                #region CLIENTNAME
                case "CLIENTNAME":
                    // Set the client name for the next broadcast message
                    broadcastClientName = msg[1];
                    break;
                #endregion
                #region CLKCHANGED
                case "CLKCHANGED":
                    // Domotech Master clock changed
                    client.lists.clockHour = byte.Parse(msg[1]);
                    client.lists.clockMinute = byte.Parse(msg[2]);
                    break;
                #endregion
                #region DCHANGED
                case "DCHANGED":
                    {   // Dimmer parameter(s) changed
                        Dimmer dimmer = client.lists.dimmerList[index];
                        byte value = byte.Parse(msg[2]);
                        bool state = (byte.Parse(msg[3]) != 0);

                        if (dimmer.value != value)
                        {
                            dimmer.value = value;
                            properties.Add("Value");
                        }

                        if (dimmer.state != state)
                        {
                            dimmer.state = state;
                            properties.Add("State");
                        }

                        client.OnDimmerChanged(dimmer, properties);
                    }
                    break;
                #endregion
                #region DIMMERTEXTUPD
                case "DIMMERTEXTUPD":
                    {   // Dimmer text changed
                        Dimmer dimmer = client.lists.dimmerList[index];
                        
                        dimmer.name = msg[2];
                        properties.Add("Name");

                        client.OnDimmerChanged(dimmer, properties);
                    }
                    break;
                #endregion
                #region GOODBYE
                case "GOODBYE":
                    // Domotech iRemote Server is shutting down
                    // Stop polling & warn user
                    break;
                #endregion
                #region INSTRNROK
                case "INSTRNROK":
                    break;
                #endregion
                #region LCHANGED
                case "LCHANGED":
                    {   // Light state changed
                        Light light = client.lists.lightList[index];

                        light.state = (byte.Parse(msg[2]) != 0);
                        properties.Add("State");

                        client.OnLightChanged(light, properties);
                    }
                    break;
                #endregion
                #region LIGHTTEXTUPD
                case "LIGHTTEXTUPD":
                    {   // Light text changed
                        Light light = client.lists.lightList[index];

                        light.name = msg[2];
                        properties.Add("Name");

                        client.OnLightChanged(light, properties);
                    }
                    break;
                #endregion
                #region LOGVARTEXTUPD
                case "LOGVARTEXTUPD":
                    {   // LogVar text changed
                        LogVar logVar = client.lists.logVarList[index];
                        
                        logVar.name = msg[2];
                        properties.Add("Name");

                        client.OnLogVarChanged(logVar, properties);
                    }
                    break;
                #endregion
                #region MASTERINIT
                case "MASTERINIT":
                    // Domotech iRemote Server started to download the
                    // configuration from the Domotech Master.
                    // Disable GUI until this operation has finished.
                    break;
                #endregion
                #region MASTERLOST
                case "MASTERLOST":
                    // Serial connection between the Domotech iRemote Server and
                    // Domotech Master is lost.
                    // Disable GUI until the connection is fixed.
                    break;
                #endregion
                /*case "OK":
                    break;*/
                /*case "ROOMNROK":
                    break;*/
                #region ROOMTEXTUPD
                case "ROOMTEXTUPD":
                    {   // Room text changed
                        Room room = client.lists.roomList[index];

                        room.name = msg[2];
                        properties.Add("Name");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region SCENNROK
                case "SCENNROK":
                    {   // Scenario has been executed
                        Scenario scenario = client.lists.scenarioList[index];

                        scenario.busy = false;
                        properties.Add("Busy");

                        client.OnScenarioChanged(scenario, properties);
                    }
                    break;
                #endregion
                #region SCENTEXTUPD
                case "SCENTEXTUPD":
                    {   // Scenario text changed
                        Scenario scenario = client.lists.scenarioList[index];

                        scenario.name = msg[2];
                        properties.Add("Name");

                        client.OnScenarioChanged(scenario, properties);
                    }
                    break;
                #endregion
                #region SCHANGED
                case "SCHANGED":
                    {   // Socket state changed
                        Socket socket = client.lists.socketList[index];

                        socket.state = (byte.Parse(msg[2]) != 0);
                        properties.Add("State");

                        client.OnSocketChanged(socket, properties);
                    }
                    break;
                #endregion
                #region SHUTTERTEXTUPD
                case "SHUTTERTEXTUPD":
                    {   // Shutter text changed
                        Shutter shutter = client.lists.shutterList[index];

                        shutter.name = msg[2];
                        properties.Add("Name");

                        client.OnShutterChanged(shutter, properties);
                    }
                    break;
                #endregion
                #region SOCKETTEXTUPD
                case "SOCKETTEXTUPD":
                    {   // Socket text changed
                        Socket socket = client.lists.socketList[index];

                        socket.name = msg[2];
                        properties.Add("Name");

                        client.OnSocketChanged(socket, properties);
                    }
                    break;
                #endregion
                #region TATIMCHANGED
                case "TATIMCHANGED":
                    {   // Time of step in temp curve changed
                        stepIndex = int.Parse(msg[2]);

                        Room room = client.lists.roomList[index];
                        CurveStep curveStep = room.CurveSteps[stepIndex];

                        byte hour = byte.Parse(msg[3]);
                        byte minute = byte.Parse(msg[4]);

                        if (curveStep.hour != hour)
                        {
                            curveStep.hour = hour;
                            properties.Add("Hour");
                        }

                        if (curveStep.minute != minute)
                        {
                            curveStep.minute = minute;
                            properties.Add("Minute");
                        }

                        client.OnCurveStepChanged(curveStep, properties);
                    }
                    break;
                #endregion
                #region TATMPCHANGED
                case "TATMPCHANGED":
                    {   // Temperature of step in temp curve changed
                        stepIndex = int.Parse(msg[2]);

                        Room room = client.lists.roomList[index];
                        CurveStep curveStep = room.CurveSteps[stepIndex];

                        curveStep.temperature = Core.TempToDegrees(int.Parse(msg[3]));
                        properties.Add("Temperature");

                        client.OnCurveStepChanged(curveStep, properties);
                    }
                    break;
                #endregion
                #region TCCHANGED
                case "TCCHANGED":
                    {   // Room temperature correction changed
                        Room room = client.lists.roomList[index];
                        
                        room.tempCorrection = Core.CorrectionToDegrees(byte.Parse(msg[2]));
                        properties.Add("TempCorrection");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region TCHANGED
                case "TCHANGED":
                    {   // Room measured temperature changed
                        Room room = client.lists.roomList[index];

                        room.measuredTemp = Core.TempToDegrees(int.Parse(msg[2]));
                        properties.Add("MeasuredTemp");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region TDCHANGED
                case "TDCHANGED":
                    {   // Room day temperature changed
                        Room room = client.lists.roomList[index];

                        room.dayTemp = Core.TempToDegrees(int.Parse(msg[2]));
                        properties.Add("DayTemp");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region TIMCHANGED
                case "TIMCHANGED":
                    {   // Timer parameter(s) changed
                        Timer timer = client.lists.timerList[index];
                        
                        byte day = byte.Parse(msg[2]);
                        byte hour = byte.Parse(msg[3]);
                        byte minute = byte.Parse(msg[4]);

                        if (timer.day != day)
                        {
                            timer.day = day;
                            properties.Add("Day");
                        }

                        if (timer.hour != hour)
                        {
                            timer.hour = hour;
                            properties.Add("Hour");
                        }

                        if (timer.minute != minute)
                        {
                            timer.minute = minute;
                            properties.Add("Minute");
                        }

                        client.OnTimerChanged(timer, properties);
                    }
                    break;
                #endregion
                /*case "TIMERADDED":
                    break;*/
                /*case "TIMERNROK":
                    break;*/
                /*case "TIMERREMOVED":
                    break;*/
                #region TIMONOFFCHANGED
                case "TIMONOFFCHANGED":
                    {   // Timer state changed
                        Timer timer = client.lists.timerList[index];

                        timer.state = (byte.Parse(msg[2]) != 0);
                        properties.Add("State");

                        client.OnTimerChanged(timer, properties);
                    }
                    break;
                #endregion
                #region TNCHANGED
                case "TNCHANGED":
                    {   // Room night temperature changed
                        Room room = client.lists.roomList[index];

                        room.nightTemp = Core.TempToDegrees(int.Parse(msg[2]));
                        properties.Add("NightTemp");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region TONOFFCHANGED
                case "TONOFFCHANGED":
                    {   // Room temperature control state changed
                        Room room = client.lists.roomList[index];

                        room.tempEnabled = (byte.Parse(msg[2]) != 0);
                        properties.Add("TempEnabled");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region TSCHANGED
                case "TSCHANGED":
                    {   // Room temperature control activity changed
                        Room room = client.lists.roomList[index];

                        room.tempActive = (byte.Parse(msg[2]) != 0);
                        properties.Add("TempActive");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region TTCHANGED
                case "TTCHANGED":
                    {   // Room temperature control type changed
                        Room room = client.lists.roomList[index];

                        room.tempType = (RoomTempType)byte.Parse(msg[2]);
                        properties.Add("TempType");

                        client.OnRoomChanged(room, properties);
                    }
                    break;
                #endregion
                #region VCHANGED
                case "VCHANGED":
                    {   // LogVar state changed
                        LogVar logVar = client.lists.logVarList[index];

                        logVar.state = (byte.Parse(msg[2]) != 0);
                        properties.Add("State");

                        client.OnLogVarChanged(logVar, properties);
                    }
                    break;
                #endregion
            }
        }

        #endregion
    }
}
