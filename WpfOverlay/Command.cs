using System.Collections.Generic;

namespace WpfOverlay
{
    public class Command
    {
        public string ButtonName { get; set; } //The name the command's button should have
        public string CommandContent { get; set; } //The code to be compiled
        public string VoiceTrigger { get; set; } //The voice trigger of the command
        public bool RequireAuth { get; set; }//Does the command Require verification?
        public List<string> VoiceTrigIntName { get; set; }//Name of int to search for in to be compiled code
        public List<string> VoiceTrigStringName { get; set; } //Name of string to search for in to be compiled code
        public List<int> VoiceTrigStringNamePosition { get; set; }//used for voicetrigString string extraction from voice recognized text
    }
}
