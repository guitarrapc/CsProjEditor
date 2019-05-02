using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class CsProjEditorUtf8CRLFUnitTests //: IDisposable
    {
        private readonly string tempFolder;
        private readonly List<string> tempPaths = new List<string>();

        ///// <summary>
        ///// Setup
        ///// </summary>
        //public CsProjEditorUtf8CRLFUnitTests()
        //{
        //    // ready temp folder
        //    var testData = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "testdata"), "*.*proj");
        //    var tempFolder = Path.Combine(Path.GetTempPath(), nameof(CsProjEditorUtf8CRLFUnitTests));
        //    if (!Directory.Exists(tempFolder))
        //        Directory.CreateDirectory(tempFolder);
        //    this.tempFolder = tempFolder;

        //    // Copy csproj to temp
        //    foreach (var item in testData)
        //    {
        //        // Gen temp path
        //        var temp = Path.Combine(tempFolder, Path.GetFileName(item));
        //        tempPaths.Add(temp);
        //        if (!File.Exists(temp))
        //        {
        //            File.Copy(item, temp, true);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Teardown
        ///// </summary>
        //public void Dispose()
        //{
        //    // Remove temp path
        //    Directory.Delete(tempFolder, true);
        //}

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8LF.csproj", EolType.LF, "\n")]
        public void ValidCsprojFormatPathLoadUtf8Test(string csprojPath, EolType eol, string eolString)
        {
            // Load Should be success
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.Root.ToString().Should().NotBeNullOrEmpty();
            csproj.ToString().Should().Be(File.ReadAllText(csprojPath));

            // Encoding
            csproj.Encoding.Should().Be(new UTF8Encoding(false));

            // Eol
            csproj.Eol.Should().Be(eol);
            csproj.Eol.ToEolString().Should().Be(eolString);

            // Initialized Should be true
            csproj.Initialized.Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8LF.csproj", EolType.LF, "\n")]
        public void ValidCsprojFormatStreamLoadUtf8Test(string csprojPath, EolType eol, string eolString)
        {
            // Load Should be success
            using (var stream = File.Open(csprojPath, FileMode.Open, FileAccess.Read))
            {
                var csproj = CsProjEditor.Load(stream);
                csproj.Root.ToString().Should().NotBeNullOrEmpty();
                csproj.ToString().Should().Be(File.ReadAllText(csprojPath));

                // Encoding
                csproj.Encoding.Should().Be(new UTF8Encoding(false));

                // Eol
                csproj.Eol.Should().Be(eol);
                csproj.Eol.ToEolString().Should().Be(eolString);

                // Initialized Should be true
                csproj.Initialized.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8BomCRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8BomLF.csproj", EolType.LF, "\n")]
        public void ValidCsprojFormatPathLoadUtf8BomTest(string csprojPath, EolType eol, string eolString)
        {
            // Load Should be success
            var csproj = CsProjEditor.Load(csprojPath);
            csproj.Root.ToString().Should().NotBeNullOrEmpty();
            csproj.ToString().Should().Be(File.ReadAllText(csprojPath));

            // Encoding
            csproj.Encoding.Should().Be(new UTF8Encoding(true));

            // Eol
            csproj.Eol.Should().Be(eol);
            csproj.Eol.ToEolString().Should().Be(eolString);

            // Initialized Should be true
            csproj.Initialized.Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8BomCRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8BomLF.csproj", EolType.LF, "\n")]
        public void ValidCsprojFormatStreamLoadUtf8BomTest(string csprojPath, EolType eol, string eolString)
        {
            // Load Should be success
            using (var stream = File.Open(csprojPath, FileMode.Open, FileAccess.Read))
            {
                var csproj = CsProjEditor.Load(stream);
                csproj.Root.ToString().Should().NotBeNullOrEmpty();
                csproj.ToString().Should().Be(File.ReadAllText(csprojPath));

                // Encoding
                csproj.Encoding.Should().Be(new UTF8Encoding(true));

                // Eol
                csproj.Eol.Should().Be(eol);
                csproj.Eol.ToEolString().Should().Be(eolString);

                // Initialized Should be true
                csproj.Initialized.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("testdata/InvalidFormat.csproj")]
        public void InvalidXmlFormatFailLoadTest(string csprojPath)
        {
            // Load Should throw
            Assert.Throws<XmlException>(() => CsProjEditor.Load(csprojPath));
        }

        [Theory]
        [InlineData("testdata/NotExist.csproj")]
        public void InvalidCsprojPathLoadTest(string csprojPath)
        {
            // Load Should throw
            Assert.Throws<FileNotFoundException>(() => CsProjEditor.Load(csprojPath));
        }
    }
}
