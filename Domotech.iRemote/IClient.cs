using Domotech.iRemote.Items;
using System;
using System.Collections.Generic;

namespace Domotech.iRemote
{
    public interface IClient : IDisposable
    {
        /// <summary>
        /// Event fired when the status of the commander object has changed.
        /// </summary>
        event ClientStatusEventHandler ClientStatus;

        /// <summary>
        /// Event fired when an alarm has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Alarm> AlarmChanged;

        /// <summary>
        /// Event fired when an audio source has changed.
        /// </summary>
        event ClientItemChangedEventHandler<AudioSource> AudioSourceChanged;

        /// <summary>
        /// Event fired when an audio zone has changed.
        /// </summary>
        event ClientItemChangedEventHandler<AudioZone> AudioZoneChanged;

        /// <summary>
        /// Event fired when a temperature curve step has changed.
        /// </summary>
        event ClientItemChangedEventHandler<CurveStep> CurveStepChanged;

        /// <summary>
        /// Event fired when a dimmer has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Dimmer> DimmerChanged;

        /// <summary>
        /// Event fired when a light has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Light> LightChanged;

        /// <summary>
        /// Event fired when a logical variable has changed.
        /// </summary>
        event ClientItemChangedEventHandler<LogVar> LogVarChanged;

        /// <summary>
        /// Event fired when a room has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Room> RoomChanged;

        /// <summary>
        /// Event fired when a scenario has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Scenario> ScenarioChanged;

        /// <summary>
        /// Event fired when a shutter has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Shutter> ShutterChanged;

        /// <summary>
        /// Event fired when a socket has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Socket> SocketChanged;

        /// <summary>
        /// Event fired when a timer has changed.
        /// </summary>
        event ClientItemChangedEventHandler<Timer> TimerChanged;

        /// <summary>
        /// Event fired when a named item has changed.
        /// </summary>
        event ClientItemChangedEventHandler<NamedItem> ItemChanged;

        /// <summary>
        /// Connect to the Domotech iRemote Server over TCP/IP using the COM objects Connect method.
        /// </summary>
        /// <param name="host">The hostname or IP address of the Domotech iRemote Server.</param>
        /// <param name="port">The TCP portnumber where the Domotech iRemote Server is listening on (default: 33999).</param>
        void Connect(string host, int port);

        /// <summary>
        /// Disconnect from the Domotech iRemote Server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sets the state of all Sockets in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        void SetAllSocketsState(bool state);

        /// <summary>
        /// Sets the state of all Lights in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        void SetAllLightsState(bool state);

        /// <summary>
        /// Sets the state of all LogVars in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        void SetAllLogVarsState(bool state);

        /// <summary>
        /// Sets the state of all Dimmers in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        void SetAllDimmersState(bool state);

        /// <summary>
        /// Reset all alarms.
        /// </summary>
        void AlarmReset();

        /// <summary>
        /// Stops the alarm signal on all LCD panels.
        /// </summary>
        void AlarmSilence();

        /// <summary>
        /// Sets the state of all AudioZones in one operation.
        /// </summary>
        /// <param name="state">The state true (on) or false (off)</param>
        void SetAllAudioZonesState(bool state);

        /// <summary>
        /// Gets the human readable description for an instruction.
        /// </summary>
        /// <param name="index">The instruction index</param>
        /// <returns>The human readable description</returns>
        string GetInstructionText(int index);

        /// <summary>
        /// Forces the Domotech Master to initialize the Audio interface.
        /// </summary>
        void AudioInit();

        /// <summary>
        /// Retreives update messages from the Domotech iRemote Server.
        /// </summary>
        /// <returns>A boolean representing the connection state. Returns true when still connected, false when not.</returns>
        bool Poll();

        /// <summary>
        /// Gets a socket by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The socket or null</returns>
        Socket GetSocket(int index);

        /// <summary>
        /// Gets a light by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The light or null</returns>
        Light GetLight(int index);

        /// <summary>
        /// Gets a logical variable by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The logical variable or null</returns>
        LogVar GetLogVar(int index);

        /// <summary>
        /// Gets a dimmer by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The dimmer or null</returns>
        Dimmer GetDimmer(int index);

        /// <summary>
        /// Gets a shutter by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The shutter or null</returns>
        Shutter GetShutter(int index);

        /// <summary>
        /// Gets a room by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The room or null</returns>
        Room GetRoom(int index);

        /// <summary>
        /// Gets an alarm by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The alarm or null</returns>
        Alarm GetAlarm(int index);

        /// <summary>
        /// Gets an audio zone by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The audio zone or null</returns>
        AudioZone GetAudioZone(int index);

        /// <summary>
        /// Gets an audio source by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The audio source or null</returns>
        AudioSource GetAudioSource(int index);

        /// <summary>
        /// Gets a scenario by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The scenario or null</returns>
        Scenario GetScenario(int index);

        /// <summary>
        /// Gets a timer by its index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The timer or null</returns>
        Timer GetTimer(int index);

        /// <summary>
        /// Enumerates all sockets known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of sockets.</returns>
        IEnumerable<Socket> Sockets();

        /// <summary>
        /// Enumerates all lights known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of lights.</returns>
        IEnumerable<Light> Lights();

        /// <summary>
        /// Enumerates all logvars known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of logvars.</returns>
        IEnumerable<LogVar> LogVars();

        /// <summary>
        /// Enumerates all dimmers known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of dimmers.</returns>
        IEnumerable<Dimmer> Dimmers();

        /// <summary>
        /// Enumerates all shutters known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of shutters.</returns>
        IEnumerable<Shutter> Shutters();

        /// <summary>
        /// Enumerates all rooms known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of rooms.</returns>
        IEnumerable<Room> Rooms();

        /// <summary>
        /// Enumerates all alarms known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of alarms.</returns>
        IEnumerable<Alarm> Alarms();

        /// <summary>
        /// Enumerates all audiozones known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of audiozones.</returns>
        IEnumerable<AudioZone> AudioZones();

        /// <summary>
        /// Enumerates all audiosources known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of audiosources.</returns>
        IEnumerable<AudioSource> AudioSources();

        /// <summary>
        /// Enumerates all scenarios known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of scenarios.</returns>
        IEnumerable<Scenario> Scenarios();

        /// <summary>
        /// Enumerates all timers known by iRemote.
        /// </summary>
        /// <returns>IEnumerable of timers.</returns>
        IEnumerable<Timer> Timers();

        /// <summary>
        /// Returns the number of sockets known by iRemote.
        /// </summary>
        int SocketCount { get; }

        /// <summary>
        /// Returns the number of lights known by iRemote.
        /// </summary>
        int LightCount { get; }

        /// <summary>
        /// Returns the number of logvars known by iRemote.
        /// </summary>
        int LogVarCount { get; }

        /// <summary>
        /// Returns the number of dimmers known by iRemote.
        /// </summary>
        int DimmerCount { get; }

        /// <summary>
        /// Returns the number of shutters known by iRemote.
        /// </summary>
        int ShutterCount { get; }

        /// <summary>
        /// Returns the number of rooms known by iRemote.
        /// </summary>
        int RoomCount { get; }

        /// <summary>
        /// Returns the number of alarms known by iRemote.
        /// </summary>
        int AlarmCount { get; }

        /// <summary>
        /// Returns the number of audiozones known by iRemote.
        /// </summary>
        int AudioZoneCount { get; }

        /// <summary>
        /// Returns the number of audiosources known by iRemote.
        /// </summary>
        int AudioSourceCount { get; }

        /// <summary>
        /// Returns the number of scenarios known by iRemote.
        /// </summary>
        int ScenarioCount { get; }

        /// <summary>
        /// Returns the number of timers known by iRemote.
        /// </summary>
        int TimerCount { get; }

        /// <summary>
        /// Returns the state of the TCP/IP connection with the Domotech iRemote Server
        /// </summary>
        bool Connected { get; }
    }
}