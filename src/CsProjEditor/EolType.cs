using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CsProjEditor
{
    public enum EolType { CRLF, LF, }

    public static class EolTypeExtensions
    {
        public static string ToEolString(this EolType eol)
            => eol == EolType.CRLF ? "\r\n" : "\n";
    }
}
