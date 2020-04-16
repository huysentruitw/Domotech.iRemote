namespace Domotech.iRemote.Items
{
    public class AudioZone : NamedItem
    {
        #region Declarations

        internal bool state;
        internal bool mute;
        internal byte inputSource;
        internal int volume;
        internal int bass;
        internal int treble;

        #endregion

        #region Constructor

        internal AudioZone(Client client, int index, string name, bool state, bool mute, byte inputSource, int volume, int bass, int treble)
            : base(client, index, name)
        {
            this.state = state;
            this.mute = mute;
            this.inputSource = inputSource;
            this.volume = volume;
            this.bass = bass;
            this.treble = treble;
        }

        #endregion

        #region Private / protected methods

        private void SetState(bool state)
        {
            try
            {
                client.core.SetAudioZoneState(index, state);
            }
            catch
            {
            }
        }

        private void SetVolume(int volume, bool execute, bool lcdUpdate)
        {
            try
            {
                client.core.SetAudioParam(index, false, AudioParamType.Volume, volume, execute, lcdUpdate);
            }
            catch
            {
            }
        }

        private void SetBass(int bass, bool execute, bool lcdUpdate)
        {
            try
            {
                client.core.SetAudioParam(index, false, AudioParamType.Bass, bass, execute, lcdUpdate);
            }
            catch
            {
            }
        }

        private void SetTreble(int treble, bool execute, bool lcdUpdate)
        {
            try
            {
                client.core.SetAudioParam(index, false, AudioParamType.Treble, treble, execute, lcdUpdate);
            }
            catch
            {
            }
        }

        private void SetMute(bool mute, bool execute, bool lcdUpdate)
        {
            try
            {
                client.core.SetAudioParam(index, false, AudioParamType.Mute, mute ? 1 : 0, execute, lcdUpdate);
            }
            catch
            {
            }
        }
        
        private void SetInputSource(byte inputSource, bool execute, bool lcdUpdate)
        {
            try
            {
                client.core.SetAudioParam(index, false, AudioParamType.InputSource, (int)inputSource, execute, lcdUpdate);
            }
            catch
            {
            }
        }
        
        protected override void SetName(string name)
        {
            try
            {
                client.core.SetAudioZoneName(index, name);
            }
            catch
            {
            }
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the power state of this audio zone.
        /// </summary>
        public bool State
        {
            get { return state; }
            set { SetState(value); }
        }

        /// <summary>
        /// Gets or sets the mute state of this audio zone.
        /// </summary>
        public bool Mute
        {
            get { return mute; }
            set { SetMute(value, true, true); }
        }

        /// <summary>
        /// Gets or sets the current input source of this audio zone.
        /// </summary>
        public byte Source
        {
            get { return inputSource; }
            set
            {
                if (value < client.AudioSourceCount)
                    SetInputSource(value, true, true);
            }
        }

        /// <summary>
        /// Gets or sets the volume of this audio zone.
        /// </summary>
        public int Volume
        {
            get { return volume; }
            set
            {
                if (value < -80) value = -80;
                if (value > 0) value = 0;

                SetVolume(value, true, true);
            }
        }

        /// <summary>
        /// Gets or sets the bass level of this audio zone.
        /// </summary>
        public int Bass
        {
            get { return bass; }
            set
            {
                if (value < -12) value = -12;
                if (value > 12) value = 12;

                SetBass(value, true, true);
            }
        }

        /// <summary>
        /// Gets or sets the treble level of this audio zone.
        /// </summary>
        public int Treble
        {
            get { return treble; }
            set
            {
                if (value < -12) value = -12;
                if (value > 12) value = 12;

                SetTreble(value, true, true);
            }
        }
        
        #endregion
    }
}
