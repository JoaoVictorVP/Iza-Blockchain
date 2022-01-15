namespace IzaBlockchain
{
    /// <summary>
    /// 16 bytes time stamp (128 bits)
    /// </summary>
    public struct TimeStamp
    {
        public ushort Year;
        public short Month;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public int Milisecond;

        public int SumAll => Year + Month + Day + Hour + Minute + Second;
        public int SumAllWMilisecond => Year + Month + Day + Hour + Minute + Second + Milisecond;

        public int SumYMD => Year + Month + Day;

        public int SumHMS => Hour + Minute + Second;

        public int SumHMSM => Hour + Minute + Second + Milisecond;

        public DateTime GetDateTime() => new DateTime(Year, Month, Day, Hour, Minute, Second, Milisecond);

        /// <summary>
        /// Get's a universal coordinated time stamp for now
        /// </summary>
        /// <returns></returns>
        public static TimeStamp Now() => new TimeStamp(DateTime.UtcNow);

        /// <summary>
        /// Is this timestamp equal to the other?
        /// </summary>
        /// <param name="other">The other timestamp</param>
        /// <param name="tolerant">Milisecond differences tolerant (exclusive)</param>
        /// <returns></returns>
        public bool IsEqual(TimeStamp other, bool tolerant = true)
        {
            return Year == other.Year && Month == other.Month && Day == other.Day &&
                Hour == other.Hour && Minute == other.Minute && Second == other.Second &&
                (tolerant ? true : Milisecond == other.Milisecond);
        }

        /// <summary>
        /// Is this timestamp later in relation to other?
        /// </summary>
        /// <param name="other">The other timestamp</param>
        /// <param name="tolerant">Tolerant from differences in miliseconds? (If true case miliseconds being equal this timestamp will be equal to other)</param>
        /// <returns></returns>
        public bool Later(TimeStamp other, bool tolerant = true)
        {
            return Year > other.Year && Month > other.Month && Day > other.Day &&
                Hour > other.Hour && Minute > other.Minute && Second > other.Second &&
                (tolerant ? true : Milisecond > other.Milisecond);
        }

        public TimeStamp(DateTime date)
        {
            Year = (ushort)date.Year;
            Month = (short)date.Month;
            Day = (short)date.Day;
            Hour = (short)date.Hour;
            Minute = (short)date.Minute;
            Second = (short)date.Second;
            Milisecond = date.Millisecond;
        }
    }
}