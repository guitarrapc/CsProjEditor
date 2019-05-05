using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class FileTest
    {
        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8_LF.csproj", EolType.LF, "\n")]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj", EolType.LF, "\n")]
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

            // TODO: Save test
        }

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8_LF.csproj", EolType.LF, "\n")]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj", EolType.LF, "\n")]
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

                // TODO: Save test
            }
        }

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8Bom_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8Bom_LF.csproj", EolType.LF, "\n")]
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

            // TODO: Save test
        }

        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8Bom_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8Bom_LF.csproj", EolType.LF, "\n")]
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

                // TODO: Save test
            }
        }

        [Theory]
        [InlineData("testdata/InvalidFormat.csproj")]
        public void InvalidXmLFormatFailLoadTest(string csprojPath)
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
