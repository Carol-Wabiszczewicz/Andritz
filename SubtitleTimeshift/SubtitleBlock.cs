using System;
using System.Collections.Generic;

namespace SubtitleTimeshift
{
    public class SubtitleBlock
    {
        public int Index { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<string> Texts { get; set; } = new List<string>();
    }
}