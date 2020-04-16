namespace Domotech.iRemote.Items
{
    public class Dimmer : NamedItem
    {
        #region Declarations

        internal byte value;
        internal bool state;

        #endregion

        #region Construction

        internal Dimmer(Client client, int index, string name, byte value, bool state)
            : base(client, index, name)
        {
            this.value = value;
            this.state = state;
        }

        #endregion

        #region Private / protected methods

        private void SetValue(byte value)
        {
            try
            {
                client.core.SetDimmerValue(index, value);
            }
            catch
            {
            }
        }

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetDimmerName(index, name);
            }
            catch
            {
            }
        }

        private void SetState(bool state)
        {
            try
            {
                client.core.SetDimmerState(index, state);
            }
            catch
            {
            }
        }

        private void SetActive(bool active)
        {
            try
            {
                client.core.SetDimmerActive(index, active);
            }
            catch
            {
            }
        }
        
        /// <summary>
        /// Force the Domotech Master to update the value of this dimmer on all LCD panels.
        /// </summary>
        private void SlaveUpdate()
        {
            try
            {
                client.core.DimmerSlaveUpdate(index);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of this dimmer.
        /// </summary>
        public bool State
        {
            get { return state; }
            set { SetState(value); }
        }

        /// <summary>
        /// Gets or sets the value of this dimmer in percent (0...100).
        /// </summary>
        public byte Value
        {
            get { return value; }
            set
            {
                if (value > 100) value = 100;
                
                SetValue(value);
                SlaveUpdate();
            }
        }

        /// <summary>
        /// Sets this dimmer to active (100%) or inactive (0%).
        /// </summary>
        public bool Active
        {
            set { SetActive(value); }
        }

        #endregion
    }
}
