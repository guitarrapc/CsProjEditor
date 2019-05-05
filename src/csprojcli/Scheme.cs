using System;
using System.Collections.Generic;
using System.Text;

namespace csprojcli
{
    public class Scheme
    {
        public string path { get; set; }
        public string output { get; set; }
        public bool dry { get; set; }
        public bool allowoverwrite { get; set; }
        public Command[] commands { get; set; }

        public class Command
        {
            public int order { get; set; }
            public string type { get; set; }
            public string command { get; set; }
            public Parameter parameter { get; set; }
        }

        public class Parameter
        {
            public string group { get; set; }
            public string node { get; set; }
            public string value { get; set; }
            public string pattern { get; set; }
            public string replacement { get; set; }
            public bool leaveBrankLine { get; set; }
            public string newvalue { get; set; }
            public string attribute { get; set; }
        }
    }
}
