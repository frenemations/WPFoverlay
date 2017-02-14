using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfOverlay
{
    public class Command
    {
        public string ButtonName { get; set; }
        public string CommandContent { get; set; }
        public string VoiceTrigger { get; set; } 
        public bool RequireAuth { get; set; }
    }
}
