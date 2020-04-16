namespace Domotech.iRemote.Items
{
    public class Scenario : NamedItem
    {
        #region Declarations

        internal ScenarioType type;
        internal bool busy;

        #endregion

        #region Constructor

        internal Scenario(Client client, int index, string name, ScenarioType type)
            : base(client, index, name)
        {
            this.type = type;
            busy = false;
        }

        #endregion

        #region Private / protected methods

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetScenarioName(index, name);
            }
            catch
            {
            }
        }

        #endregion

        #region Public methods

        public void Execute()
        {
            try
            {
                client.core.ScenarioExecute(index);
                busy = true;
            }
            catch
            {
            }
        }

        public void Save()
        {
            try
            {
                client.core.ScenarioSave(index);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that indicates the execution of this scenario is busy or not.
        /// </summary>
        public bool Busy
        {
            get { return busy; }
        }

        #endregion
    }
}
