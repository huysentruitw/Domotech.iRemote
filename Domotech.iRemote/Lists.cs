using Domotech.iRemote.Items;
using System;
using System.Collections.Generic;

namespace Domotech.iRemote
{
    internal class Lists : IDisposable
    {
        #region Declarations

        private Client client;

        public byte clockHour;
        public byte clockMinute;
        public bool audioInitOk = false;

        public List<Socket> socketList = new List<Socket>();
        public List<Light> lightList = new List<Light>();
        public List<LogVar> logVarList = new List<LogVar>();
        public List<Dimmer> dimmerList = new List<Dimmer>();
        public List<Shutter> shutterList = new List<Shutter>();
        public List<Room> roomList = new List<Room>();
        public List<Alarm> alarmList = new List<Alarm>();
        public List<AudioZone> audioZoneList = new List<AudioZone>();
        public List<AudioSource> audioSourceList = new List<AudioSource>();
        public List<Scenario> scenarioList = new List<Scenario>();
        public List<Timer> timerList = new List<Timer>();

        #endregion

        #region Construction / Destruction

        public Lists(Client client)
        {
            this.client = client;
        }

        public void Dispose()
        {
            socketList.Clear();
            lightList.Clear();
            logVarList.Clear();
            dimmerList.Clear();
            shutterList.Clear();
            roomList.Clear();
            alarmList.Clear();
            audioZoneList.Clear();
            audioSourceList.Clear();
            scenarioList.Clear();
            timerList.Clear();
        }

        #endregion

        #region List retreival methods

        /// <summary>
        /// Download the Socket list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadSocketList()
        {
            socketList.Clear();

            int count = client.core.SocketCount;

            for (int i = 0; i < count; i++)
            {
                string name;
                bool state;

                client.core.GetSocketInfo(i, out name, out state);

                socketList.Add(new Socket(client, i, name, state));
            }
        }

        /// <summary>
        /// Download the Light list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadLightList()
        {
            lightList.Clear();

            int count = client.core.LightCount;

            for (int i = 0; i < count; i++)
            {
                string name;
                bool state;

                client.core.GetLightInfo(i, out name, out state);

                lightList.Add(new Light(client, i, name, state));
            }
        }

        /// <summary>
        /// Download the LogVar list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadLogVarList()
        {
            logVarList.Clear();

            int count = client.core.LogVarCount;

            for (int i = 0; i < count; i++)
            {
                string name;
                bool state;

                client.core.GetLogVarInfo(i, out name, out state);

                logVarList.Add(new LogVar(client, i, name, state));
            }
        }

        /// <summary>
        /// Download the Dimmer list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadDimmerList()
        {
            dimmerList.Clear();

            int count = client.core.DimmerCount;
            for (int i = 0; i < count; i++)
            {
                string name;
                byte value;
                bool state;

                client.core.GetDimmerInfo(i, out name, out value, out state);

                dimmerList.Add(new Dimmer(client, i, name, value, state));
            }
        }

        /// <summary>
        /// Download the Shutter list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadShutterList()
        {
            shutterList.Clear();

            int count = client.core.ShutterCount;
            for (int i = 0; i < count; i++)
            {
                string name;

                client.core.GetShutterInfo(i, out name);

                shutterList.Add(new Shutter(client, i, name));
            }
        }

        /// <summary>
        /// Download the Room list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadRoomList()
        {
            roomList.Clear();

            int count = client.core.RoomCount;
            for (int i = 0; i < count; i++)
            {
                string name;
                float tempCorrection;
                float measuredTemp;
                float dayTemp;
                float nightTemp;
                RoomTempType tempType;
                bool tempEnabled;
                bool tempActive;
                float aircoTemp;
                RoomAircoType aircoType;
                bool aircoEnabled;
                bool aircoActive;
                int curveSteps;
                bool outside;

                client.core.GetRoomInfo(i, out name, out tempCorrection,
                    out measuredTemp, out dayTemp, out nightTemp,
                    out tempType, out tempEnabled, out tempActive,
                    out aircoTemp, out aircoType, out aircoEnabled,
                    out aircoActive, out curveSteps, out outside);

                Room room = new Room(client, i, name, tempCorrection,
                    measuredTemp, dayTemp, nightTemp,
                    tempType, tempEnabled, tempActive,
                    aircoTemp, aircoType, aircoEnabled,
                    aircoActive, outside);

                for (int j = 0; j < (int)curveSteps; j++)
                {
                    byte day;
                    byte hour;
                    byte minute;
                    float temp;

                    client.core.GetCurveStepInfo(i, j, out day, out hour, out minute, out temp);

                    room.CurveSteps.Add(new CurveStep(room, j, day, hour, minute, temp));
                }

                roomList.Add(room);
            }
        }

        /// <summary>
        /// Download the Alarm list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadAlarmList()
        {
            alarmList.Clear();

            int count = client.core.AlarmCount;
            for (int i = 0; i < count; i++)
            {
                string name;
                bool active;
                bool wasActive;
                byte timesOn;
                bool beep;

                client.core.GetAlarmInfo(i, out name, out active,
                    out wasActive, out timesOn, out beep);

                alarmList.Add(new Alarm(client, i, name, active,
                    wasActive, timesOn, beep));
            }
        }

        /// <summary>
        /// Download the AudioZone list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadAudioZoneList()
        {
            audioZoneList.Clear();

            int count = client.core.AudioZoneCount;
            for (int i = 0; i < count; i++)
            {
                string name;
                bool state;
                bool mute;
                byte inputSource;
                int volume;
                int bass;
                int treble;

                client.core.GetAudioZoneInfo(i, out name, out state, out mute,
                    out inputSource, out volume, out bass, out treble);

                audioZoneList.Add(new AudioZone(client, i, name, state, mute,
                    inputSource, volume, bass, treble));
            }
        }

        /// <summary>
        /// Download the AudioSource list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadAudioSourceList()
        {
            audioSourceList.Clear();

            int count = client.core.AudioSourceCount;
            for (int i = 0; i < count; i++)
            {
                string name;
                AudioSourceType type;

                client.core.GetAudioSourceInfo(i, out name, out type);

                audioSourceList.Add(new AudioSource(client, i, name, type));
            }
        }

        /// <summary>
        /// Download the Scenario list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadScenarioList()
        {
            scenarioList.Clear();

            int count = client.core.ScenarioCount;
            for (int i = 0; i < count; i++)
            {
                string name;
                ScenarioType type;

                client.core.GetScenarioInfo(i, out name, out type);

                scenarioList.Add(new Scenario(client, i, name, type));
            }
        }

        /// <summary>
        /// Download the Timer list. This method is called from the DownloadSetup method.
        /// </summary>
        public void DownloadTimerList()
        {
            timerList.Clear();

            int count = client.core.TimerCount;
            for (int i = 0; i < count; i++)
            {
                int instructionIndex;
                string instructionText;
                byte day;
                byte hour;
                byte minute;
                bool state;

                client.core.GetTimerInfo(i, out instructionIndex,
                    out day, out hour, out minute, out state);

                client.core.GetInstructionText(instructionIndex, out instructionText);

                timerList.Add(new Timer(client, i, instructionIndex,
                    instructionText, day, hour, minute, state));
            }
        }

        #endregion
    }
}
