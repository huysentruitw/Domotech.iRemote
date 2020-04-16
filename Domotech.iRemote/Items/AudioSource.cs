namespace Domotech.iRemote.Items
{
    public class AudioSource : NamedItem
    {
        #region Declarations

        internal AudioSourceType type;

        #endregion

        #region Constructor

        internal AudioSource(Client client, int index, string name, AudioSourceType type)
            : base(client, index, name)
        {
            this.type = type;
        }

        #endregion

        #region Private / protected methods

        protected override void SetName(string name)
        {
            try
            {
                client.core.SetAudioSourceName(index, name);
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of this audio source.
        /// </summary>
        public AudioSourceType Type
        {
            get { return type; }
        }

        #endregion
    }
}
