using System;
using Xunit;

namespace CsProjEditor.Tests
{
    public class CsProjEditorUtf8CRLFUnitTests
    {
        // * Before all test
        //      * Gen temp path
        //      * Copy csproj to temp
        // * After all test
        //      * Remove temp path

        [Fact]
        public void ValidCsprojFormatLoadTest()
        {
            // Load Should be success
            // Encoding Should be utf8
            // Eol Should be CRLF
            // Initialized Should be true
        }

        [Fact]
        public void InvalidCsprojFormatLoadTest()
        {
            // Load Should throw
            // Initialized Should be false
        }

        [Fact]
        public void InvalidCsprojPathLoadTest()
        {
            // Load Should throw
            // Initialized Should be false
        }
    }
}
