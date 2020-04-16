namespace Domotech.iRemote.Items
{
    public class Light : NamedItem
    {
        #region Declarations

        internal bool state;

        #endregion

        #region Constructor

        internal Light(Client client, int index, string name, bool state)
            : base(client, index, name)
        {
            this.state = state;
        }

        #endregion

        #region Private / protected methods

        private void SetState(bool state)
        {
            try
            {
                client.core.SetLightState(index, state);
            }
            catch
            {
            }
        }

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetLightName(index, name);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of this light.
        /// </summary>
        public bool State
        {
            get { return state; }
            set { SetState(value); }
        }

        #endregion
    }
}
