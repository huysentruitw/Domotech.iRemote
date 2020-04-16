namespace Domotech.iRemote
{
    public enum AudioParamType : byte
    {
        Volume = 0,
        Bass = 1,
        Treble = 2,
        Mute = 3,
        InputSource = 4
    }

    public enum AudioSourceType : byte
    {
        CD = 0,
        DVD = 1,
        PHONO = 2,
        VCR = 3,
        CAS = 4,
        MD = 5,
        MP3 = 6,
        TUN = 7,
        TV = 8,
        SAT = 9,
        CAM = 10,
        PC = 11,
        AUX = 12,
        FREE13 = 13,
        FREE14 = 14,
        FREE15 = 15
    }

    public enum RoomTempType : byte
    {
        Day = 1,
        Night = 2,
        Auto = 3
    }

    public enum RoomAircoType : byte
    {
        Temperature = 1,
        Continuous = 2
    }

    public enum RoomControlMode
    {
        Off,
        Day,
        Night,
        Auto,
        AircoTemp,
        AircoContinu
    }

    public enum ScenarioType : byte
    {
        Once = 0,
        Repeated = 1
    }
}
