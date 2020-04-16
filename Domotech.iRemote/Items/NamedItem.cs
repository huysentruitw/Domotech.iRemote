namespace Domotech.iRemote.Items
{
    public abstract class NamedItem
    {
        #region Declarations

        protected Client client;
        protected int index;
        internal string name;

        #endregion

        #region Construction

        public NamedItem(Client client, int index, string name)
        {
            this.client = client;
            this.index = index;
            this.name = name;
        }

        #endregion

        #region Abstract methods

        protected abstract void SetName(string name);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of this item in the List.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets or sets the name of this item
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetName(value); }
        }

        #endregion
    }
}
