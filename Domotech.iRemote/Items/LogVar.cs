using System;
using System.Collections.Generic;
using System.Text;
using Domotech.iRemote;

namespace Domotech.iRemote.Items
{
    public class LogVar : NamedItem
    {
        #region Declarations

        internal bool state;

        #endregion

        #region Constructor

        internal LogVar(Client client, int index, string name, bool state)
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
                client.core.SetLogVarState(index, state);
            }
            catch
            {
            }
        }

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetLogVarName(index, name);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of this logical variable.
        /// </summary>
        public bool State
        {
            get { return state; }
            set { SetState(value); }
        }

        #endregion
    }
}
