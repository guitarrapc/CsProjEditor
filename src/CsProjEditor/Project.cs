using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public partial class Project
    {
        private readonly string _path;
        private readonly string _xml;

        public EolType Eol { get; private set; }
        public Encoding Encoding { get; private set; }
        public XElement Root { get; private set; }
        public bool Initialized { get; private set; }

        private Project(string path, string xml) => (_path, _xml) = (path, xml);

        /// <summary>
        /// Load csproj from path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Project Load(string path, LoadOptions options = LoadOptions.PreserveWhitespace)
        {
            var editor = new Project(path, File.ReadAllText(path));
            editor.Root = XElement.Load(path, options);
            editor.Encoding = Project.GetUtf8Encoding(path);
            editor.Eol = Project.GetEndOfLine(path);
            editor.Initialized = true;
            return editor;
        }

        /// <summary>
        /// Load csproj from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Project Load(Stream stream, LoadOptions options = LoadOptions.PreserveWhitespace)
        {
            StreamReader streamReader = null;
            BinaryReader binaryReader = null;
            try
            {
                string xml = "";
                // no `using statement` to re-use stream
                // manual read stream to get string
                streamReader = new StreamReader(stream);
                xml = streamReader.ReadToEnd();
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

                // manual read stream as binary to get bom
                byte[] bytes = new byte[3];
                binaryReader = new BinaryReader(stream);
                binaryReader.Read(bytes, 0, 3);
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

                var editor = new Project("", xml);
                editor.Root = XElement.Parse(xml, options);
                editor.Encoding = GetUtf8Encoding(bytes);
                editor.Eol = Project.GetEndOfLineFromXml(xml);
                editor.Initialized = true;
                return editor;
            }
            finally
            {
                streamReader?.Close();
                binaryReader?.Close();
            }
        }

        public override string ToString()
        {
            var eol = Eol.ToEolString();
            return ToXmlString(Root, eol);
        }

        private string ToXmlString(XElement root, string eol)
        {
            var declare = GetDeclarationFromXml(_xml);

            // gen xml
            string xml;
            if (declare == null)
            {
                xml = root.ToString();
                xml = xml.Replace(EolType.CRLF.ToEolString(), eol);
            }
            else
            {
                xml = declare.ToString();
                xml += eol;
                xml += root.ToString();
                xml = xml.Replace(EolType.CRLF.ToEolString(), eol);
            }
            // add line end
            xml += eol;
            return xml;
        }

        #region File Operation

        /// <summary>
        /// determine utf8 contains bom from first 3 bytes of file.
        /// </summary>
        /// <remarks>
        /// UTF8 BOM == EFBBBF == 239, 187, 191
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Encoding GetUtf8Encoding(string path)
        {
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[3];
                reader.Read(bytes, 0, 3);
                return GetUtf8Encoding(bytes);
            }
        }
        public static Encoding GetUtf8Encoding(byte[] bytes)
        {
            var isBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            return new System.Text.UTF8Encoding(isBom);
        }

        /// <summary>
        /// check file contains crlf or not.
        /// </summary>
        /// <remarks>
        /// lf == 0A == 10
        /// crlf == 0D0A == 1310
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public static EolType GetEndOfLine(string path)
        {
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bits = new byte[1024];
                reader.Read(bits, 0, 1024);
                var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                    .Select((e, i) => (left: e, right: bits[i + 1]))
                    .Where(x => x.left == 13 && x.right == 10)
                    .Any();
                return isCRLF ? EolType.CRLF : EolType.LF;
            }
        }
        public static EolType GetEndOfLine(Stream stream)
        {
            var bits = new byte[1024];
            stream.Read(bits, 0, 1024);
            var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                .Select((e, i) => (left: e, right: bits[i + 1]))
                .Where(x => x.left == 13 && x.right == 10)
                .Any();
            return isCRLF ? EolType.CRLF : EolType.LF;
        }
        public static EolType GetEndOfLineFromXml(string xml)
        {
            var bits = xml.Take(1024).Select(x => Convert.ToByte(x)).ToArray();
            var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                .Select((e, i) => (left: e, right: bits[i + 1]))
                .Where(x => x.left == 13 && x.right == 10)
                .Any();
            return isCRLF ? EolType.CRLF : EolType.LF;
        }

        /// <summary>
        /// Save to destination path
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Save(Root, path, Eol.ToEolString(), Encoding);
        }
        public void Save(XElement root, string path)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Save(root, path, Eol.ToEolString(), Encoding);
        }
        public void Save(XElement root, string path, string eol, Encoding encoding)
        {
            var xml = ToXmlString(root, eol);

            // write
            var bytes = encoding.GetBytes(xml);
            File.WriteAllBytes(path, bytes);
        }

        #endregion
    }
}
