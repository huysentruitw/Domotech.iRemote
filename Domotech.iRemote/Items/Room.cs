using System.Collections.Generic;

namespace Domotech.iRemote.Items
{
    public class Room : NamedItem
    {
        #region Declarations

        private List<CurveStep> curveStepList = new List<CurveStep>();
        internal float tempCorrection;
        internal float measuredTemp;
        internal float dayTemp;
        internal float nightTemp;
        internal RoomTempType tempType;
        internal bool tempEnabled;
        internal bool tempActive;
        internal float aircoTemp;
        internal RoomAircoType aircoType;
        internal bool aircoEnabled;
        internal bool aircoActive;
        internal bool outside;

        #endregion

        #region Constructor

        internal Room(Client client, int index, string name, float tempCorrection, float measuredTemp, float dayTemp, float nightTemp, RoomTempType tempType, bool tempEnabled, bool tempActive, float aircoTemp, RoomAircoType aircoType, bool aircoEnabled, bool aircoActive, bool outside)
            : base(client, index, name)
        {
            this.tempCorrection = tempCorrection;
            this.measuredTemp = measuredTemp;
            this.dayTemp = dayTemp;
            this.nightTemp = nightTemp;
            this.tempType = tempType;
            this.tempEnabled = tempEnabled;
            this.tempActive = tempActive;
            this.aircoTemp = aircoTemp;
            this.aircoType = aircoType;
            this.aircoEnabled = aircoEnabled;
            this.aircoActive = aircoActive;
            this.outside = outside;
        }

        #endregion

        #region Private / protected methods

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetRoomName(index, name);
            }
            catch
            {
            }
        }

        private void SetAircoState(bool aircoState)
        {
            try
            {
                client.core.SetRoomAircoState(index, aircoState);
            }
            catch
            {
            }
        }

        private void SetAircoTemp(float temperature)
        {
            try
            {
                client.core.SetRoomAircoTemp(index, temperature);
            }
            catch
            {
            }
        }

        private void SetAircoType(RoomAircoType type)
        {
            try
            {
                client.core.SetRoomAircoType(index, type);
            }
            catch
            {
            }
        }

        private void SetDayTemp(float temperature)
        {
            try
            {
                client.core.SetRoomDayTemp(index, temperature);
            }
            catch
            {
            }
        }
        
        private void SetNightTemp(float temperature)
        {
            try
            {
                client.core.SetRoomNightTemp(index, temperature);
            }
            catch
            {
            }
        }

        private void SetCorrection(float correction)
        {
            try
            {
                client.core.SetRoomTempCorrection(index, correction);
            }
            catch
            {
            }
        }

        private void SetTempState(bool state)
        {
            try
            {
                client.core.SetRoomTempState(index, state);
            }
            catch
            {
            }
        }

        private void SetTempType(RoomTempType type)
        {
            try
            {
                client.core.SetRoomTempType(index, type);
            }
            catch
            {
            }
        }
        
        #endregion

        #region Internal methods

        internal void SetCurveStepTime(int stepIndex, byte hour, byte minute)
        {
            try
            {
                client.core.SetCurveStepTime(index, stepIndex, hour, minute);
            }
            catch
            {
            }
        }

        internal void SetCurveStepTemp(int stepIndex, float temperature)
        {
            try
            {
                client.core.SetCurveStepTemp(index, stepIndex, temperature);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the curve step list of this room.
        /// </summary>
        public List<CurveStep> CurveSteps
        {
            get { return curveStepList; }
        }

        /// <summary>
        /// Gets the measured temperature in °C of this room.
        /// </summary>
        public float MeasuredTemp
        {
            get { return measuredTemp; }
        }

        /// <summary>
        /// Gets or sets the requested day temperature in °C of this room.
        /// </summary>
        public float DayTemp
        {
            get { return dayTemp; }
            set { if (dayTemp != value) SetDayTemp(value); }
        }

        /// <summary>
        /// Gets or sets the requested night temperature in °C of this room.
        /// </summary>
        public float NightTemp
        {
            get { return nightTemp; }
            set { if (nightTemp != value) SetNightTemp(value); }
        }

        /// <summary>
        /// Gets or sets the requested airco temperature in °C of this room.
        /// </summary>
        public float AircoTemp
        {
            get { return aircoTemp; }
            set { if (aircoTemp != value) SetAircoTemp(value); }
        }

        /// <summary>
        /// Gets or sets the temperature correction that must be applied to the measured temperature of this room.
        /// </summary>
        public float TempCorrection
        {
            get { return tempCorrection; }
            set { if (tempCorrection != value) SetCorrection(value); }
        }

        /// <summary>
        /// Gets or sets heating/airco control mode of this room.
        /// </summary>
        public RoomControlMode ControlMode
        {
            get
            {
                if (tempEnabled)
                {
                    switch (tempType)
                    {
                        case RoomTempType.Day:
                            return RoomControlMode.Day;
                        case RoomTempType.Night:
                            return RoomControlMode.Night;
                        case RoomTempType.Auto:
                            return RoomControlMode.Auto;
                    }
                }

                if (aircoEnabled)
                {
                    switch (aircoType)
                    {
                        case RoomAircoType.Temperature:
                            return RoomControlMode.AircoTemp;
                        case RoomAircoType.Continuous:
                            return RoomControlMode.AircoContinu;
                    }
                }

                return RoomControlMode.Off;
            }
            set
            {
                switch (value)
                {
                    case RoomControlMode.Off:
                        SetTempState(false);
                        SetAircoState(false);
                        break;
                    case RoomControlMode.Day:
                        SetTempState(true);
                        SetTempType(RoomTempType.Day);
                        break;
                    case RoomControlMode.Night:
                        SetTempState(true);
                        SetTempType(RoomTempType.Night);
                        break;
                    case RoomControlMode.Auto:
                        SetTempState(true);
                        SetTempType(RoomTempType.Auto);
                        break;
                    case RoomControlMode.AircoTemp:
                        SetAircoState(true);
                        SetAircoType(RoomAircoType.Temperature);
                        break;
                    case RoomControlMode.AircoContinu:
                        SetAircoState(true);
                        SetAircoType(RoomAircoType.Continuous);
                        break;
                }
            }
        }

        #endregion
    }
}
