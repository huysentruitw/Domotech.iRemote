using System;
using System.Collections.Generic;
using System.Text;

namespace Domotech.iRemote.Items
{
    public class Timer : NamedItem
    {
        #region Declarations

        internal int instructionIndex;
        internal string instructionText;
        internal byte day;
        internal byte hour;
        internal byte minute;
        internal bool state;

        #endregion

        #region Construction

        internal Timer(Client client, int index, int instructionIndex, string instructionText, byte day, byte hour, byte minute, bool state)
            : base(client, index, "Timer")
        {
            this.instructionIndex = instructionIndex;
            this.instructionText = instructionText;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.state = state;
        }

        #endregion

        #region Private / protected methods

        private void SetState(bool state)
        {
            try
            {
                client.core.SetTimerState(index, state);
            }
            catch
            {
            }
        }
        
        private void SetTime(byte day, byte hour, byte minute)
        {
            try
            {
                client.core.SetTimerTime(index, day, hour, minute);
            }
            catch
            {
            }
        }

        protected override void SetName(string name)
        {
            throw new Exception("Can't modify timer name");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of this timer.
        /// </summary>
        public bool State
        {
            get { return state; }
            set { SetState(value); }
        }

        /// <summary>
        /// Gets the day of week of this timer.
        /// </summary>
        public byte Day
        {
            get { return day; }
        }

        /// <summary>
        /// Gets or sets the hour part of this timer.
        /// </summary>
        public byte Hour
        {
            get { return hour; }
            set
            {
                SetTime(day, value, minute);
                hour = value;   // Don't wait for callback
            }
        }

        /// <summary>
        /// Gets or sets the minute part of this timer.
        /// </summary>
        public byte Minute
        {
            get { return minute; }
            set
            {
                SetTime(day, hour, value);
                minute = value; // Don't wait for callback
            }
        }

        #endregion
    }
}
