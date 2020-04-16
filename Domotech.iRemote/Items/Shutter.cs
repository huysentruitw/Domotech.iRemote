namespace Domotech.iRemote.Items
{
    public class Shutter : NamedItem
    {
        #region Declarations

        #endregion

        #region Constructor

        internal Shutter(Client client, int index, string name)
            : base(client, index, name)
        {
        }

        #endregion

        #region Private / protected methods

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetShutterName(index, name);
            }
            catch
            {
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Open this shutter until Stop() is called. (or the motor stops by itself)
        /// </summary>
        public void Open()
        {
            try
            {
                client.core.ShutterOpen(index);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Close this shutter until Stop() is called. (or the motor stops by itself)
        /// </summary>
        public void Close()
        {
            try
            {
                client.core.ShutterClose(index);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Stops the shutter motor.
        /// </summary>
        public void Stop()
        {
            try
            {
                client.core.ShutterStop(index);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        #endregion
    }
}
