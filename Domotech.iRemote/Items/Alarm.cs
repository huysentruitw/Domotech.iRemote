using System;

namespace Domotech.iRemote.Items
{
    public class Alarm : NamedItem
    {
        #region Declarations

        internal bool active;
        internal bool wasActive;
        internal byte timesOn;
        internal bool beep;

        #endregion

        #region Constructor

        internal Alarm(Client client, int index, string name, bool active, bool wasActive, byte timesOn, bool beep)
            : base(client, index, name)
        {
            this.active = active;
            this.wasActive = wasActive;
            this.timesOn = timesOn;
            this.beep = beep;
        }

        #endregion

        #region Private / protected methods
        
        private void SetState(bool state)
        {
            try
            {
                client.core.SetAlarmState(index, state);
            }
            catch
            {
            }
        }

        private void SetActive()
        {
            try
            {
                client.core.SetAlarmActive(index);
            }
            catch
            {
            }
        }

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetAlarmName(index, name);
            }
            catch
            {
            }
        }
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets the alarm active flag.
        /// This flag can't be set to false, call the Client.AlarmReset() instead.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (value == true)
                    SetActive();
                else
                    throw new Exception("Can't set active to false!");
            }
        }

        /// <summary>
        /// Sets the alarm state.
        /// Increases/decreases the timeson value.
        /// </summary>
        public bool State
        {
            set { SetState(value); }
        }

        /// <summary>
        /// Returns the number of times the alarm is set.
        /// </summary>
        public byte TimesOn
        {
            get { return timesOn; }
        }

        #endregion
    }
}
