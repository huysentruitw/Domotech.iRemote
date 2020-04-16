using System;
using System.Collections.Generic;
using System.Text;

namespace Domotech.iRemote.Items
{
    public class CurveStep
    {
        #region Declarations

        private Room room;
        private int index;
        internal byte day;
        internal byte hour;
        internal byte minute;
        internal float temperature;

        #endregion

        #region Construction

        internal CurveStep(Room room, int index, byte day, byte hour, byte minute, float temperature)
        {
            this.room = room;
            this.index = index;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.temperature = temperature;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the room this curvestep is assigned to.
        /// </summary>
        public Room Room
        {
            get { return room; }
        }

        /// <summary>
        /// Gets the index of this curvestep in the room's CurveStepList.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the starting day of week of this curvestep.
        /// </summary>
        public byte Day
        {
            get { return day; }
        }

        /// <summary>
        /// Gets or sets the starting hour of this curvestep.
        /// </summary>
        public byte Hour
        {
            get { return hour; }
            set
            {
                room.SetCurveStepTime(index, value, minute);
                hour = value;   // Don't wait for callback
            }
        }

        /// <summary>
        /// Gets or sets the starting minute of this curvestep.
        /// </summary>
        public byte Minute
        {
            get { return minute; }
            set
            {
                room.SetCurveStepTime(index, hour, value);
                minute = value; // Don't wait for callback
            }
        }

        /// <summary>
        /// Gets or sets the requested temperature of this curvestep.
        /// </summary>
        public float Temperature
        {
            get { return temperature; }
            set { room.SetCurveStepTemp(index, value); }
        }

        #endregion
    }
}
