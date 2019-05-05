using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class FileTest
    {
        [Theory]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/HololensUnityUwpNetAppUtf8_LF.csproj", EolType.LF, "\n")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj", EolType.LF, "\n")]
        public async Task ValidCsprojFormatPathLoadUtf8Test(string csprojPath, EolType eol, string eolString)
        {
            // delay to avoid file read conflict
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Load Should be success
            var csproj = Project.Load(csprojPath);
            csproj.Root.ToString().Should().NotBeNullOrEmpty();

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
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj", EolType.LF, "\n")]
        public async Task ValidCsprojFormatStreamLoadUtf8Test(string csprojPath, EolType eol, string eolString)
        {
            // delay to avoid file read conflict
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Load Should be success
            using (var stream = File.Open(csprojPath, FileMode.Open, FileAccess.Read))
            {
                var csproj = Project.Load(stream);
                csproj.Root.ToString().Should().NotBeNullOrEmpty();

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
        [InlineData("testdata/SimpleNewCsProjUtf8Bom_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/SimpleNewCsProjUtf8Bom_LF.csproj", EolType.LF, "\n")]
        public async Task ValidCsprojFormatPathLoadUtf8BomTest(string csprojPath, EolType eol, string eolString)
        {
            // delay to avoid file read conflict
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Load Should be success
            var csproj = Project.Load(csprojPath);
            csproj.Root.ToString().Should().NotBeNullOrEmpty();

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
        [InlineData("testdata/SimpleNewCsProjUtf8Bom_CRLF.csproj", EolType.CRLF, "\r\n")]
        [InlineData("testdata/SimpleNewCsProjUtf8Bom_LF.csproj", EolType.LF, "\n")]
        public async Task ValidCsprojFormatStreamLoadUtf8BomTest(string csprojPath, EolType eol, string eolString)
        {
            // delay to avoid file read conflict
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Load Should be success
            using (var stream = File.Open(csprojPath, FileMode.Open, FileAccess.Read))
            {
                var csproj = Project.Load(stream);
                csproj.Root.ToString().Should().NotBeNullOrEmpty();

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
        [InlineData("testdata/InvalidFormatOld.csproj")]
        [InlineData("testdata/InvalidFormatNew.csproj")]
        public void InvalidXmLFormatFailLoadTest(string csprojPath)
        {
            // Load Should throw
            Assert.Throws<XmlException>(() => Project.Load(csprojPath));
        }

        [Theory]
        [InlineData("testdata/NotExist.csproj")]
        public void CsprojPathNotFoundTest(string csprojPath)
        {
            // Load Should throw
            Assert.Throws<FileNotFoundException>(() => Project.Load(csprojPath));
        }
    }
}
