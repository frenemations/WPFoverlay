using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkCommsDotNet.Tools;
using ProtoBuf;
using NetworkCommsDotNet;
using System.Diagnostics;
namespace WpfOverlay
{
    [ProtoContract]
    class CustomObject
    {
        [ProtoMember(1)]
        public String StringValue { get; set; }

        [ProtoMember(2)]
        public int IntValue { get; set; }

        [ProtoMember(3)]
        public bool BoolValue { get; set; }

       [ProtoMember(4)]
       public Process ProcessValue { get; set; }
        
    }
}
