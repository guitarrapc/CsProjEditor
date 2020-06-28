using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class GroupTests
    {
        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj",
            new[] { "PropertyGroup", "PropertyGroup", "PropertyGroup", "PropertyGroup" },
            new[] { "Import", "Import", "PropertyGroup", "PropertyGroup", "ItemGroup", "ItemGroup", "ItemGroup", "PropertyGroup", "Import", "Target", "Target", "PropertyGroup" })]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj",
            new[] { "PropertyGroup", "PropertyGroup", "PropertyGroup", "PropertyGroup" },
            new[] { "Import", "Import", "PropertyGroup", "PropertyGroup", "ItemGroup", "ItemGroup", "ItemGroup", "PropertyGroup", "Import", "Target", "Target", "PropertyGroup" })]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj",
            new[] { "PropertyGroup", "PropertyGroup" },
            new[] { "PropertyGroup", "PropertyGroup", "ItemGroup", "ItemGroup", "Target", "Target" })]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj",
            new[] { "PropertyGroup", "PropertyGroup" },
            new[] { "PropertyGroup", "PropertyGroup", "ItemGroup", "ItemGroup", "Target", "Target" })]
        public void GetTest(string csprojPath, string[] expected, string[] expected2)
        {
            // <Project>
            //   <PropertyGroup>
            //   </PropertyGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            var x = csproj.GetGroup("PropertyGroup");
            csproj.GetGroup("PropertyGroup").Should().BeEquivalentTo(expected);

            csproj.GetGroups().Should().BeEquivalentTo(expected2);
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void GetFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.GetGroup("PropertyGroupX").Should().BeEmpty();
            csproj.GetGroup("ItemGroupX").Should().BeEmpty();
            csproj.GetGroup("TargetX").Should().BeEmpty();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ExistsTest(string csprojPath)
        {
            // <Project>
            //   <PropertyGroup>
            //   </PropertyGroup>
            //   <ItemGroup>
            //   </ItemGroup>
            //   <Target>
            //   </Target>
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("PropertyGroup").Should().BeTrue();
            csproj.ExistsGroup("ItemGroup").Should().BeTrue();
            csproj.ExistsGroup("Target").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ExistsFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("PropertyGroupX").Should().BeFalse();
            csproj.ExistsGroup("ItemGroupX").Should().BeFalse();
            csproj.ExistsGroup("TargetX").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj", new[] { "PropertyGroup", "PropertyGroup", "PropertyGroup", "PropertyGroup", "PropertyGroup" })]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj", new[] { "PropertyGroup", "PropertyGroup", "PropertyGroup", "PropertyGroup", "PropertyGroup" })]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj", new[] { "PropertyGroup", "PropertyGroup", "PropertyGroup" })]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj", new[] { "PropertyGroup", "PropertyGroup", "PropertyGroup" })]
        public void InsertExistingTest(string csprojPath, string[] expected)
        {
            // <Project>
            //   .... existings....
            //   <PropertyGroup>
            //   </PropertyGroup>
            // </Project>

            var csproj = Project.Load(csprojPath);
            // additional existing group
            csproj.ExistsGroup("PropertyGroup").Should().BeTrue();
            csproj.InsertGroup("PropertyGroup");
            var x = csproj.ToString();
            csproj.GetGroup("PropertyGroup").Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void InsertNewTest(string csprojPath)
        {
            // <Project>
            //   .... existings....
            //   <Test>
            //     <Hoge>Value</Hoge>
            //   </Test>
            // </Project>

            var csproj = Project.Load(csprojPath);
            // new group
            csproj.ExistsGroup("Test").Should().BeFalse();
            csproj.InsertGroup("Test");
            csproj.ExistsGroup("Test").Should().BeTrue();
            csproj.GetGroup("Test").Should().BeEquivalentTo("Test");
            csproj.InsertNode("Test", "Hoge", "Value");
            var x = csproj.ToString();
            csproj.ExistsNodeValue("Test", "Hoge", "Value").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void InsertFailTest(string csprojPath)
        {
            // no test.
            _ = Project.Load(csprojPath);
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceTest(string csprojPath)
        {
            // before
            // <Project>
            //   <PropertyGroup>
            //   </PropertyGroup>
            // </Project>

            // after
            // <Project>
            //   <Hogemoge>
            //   </Hogemoge>
            // </Project>
            var csproj = Project.Load(csprojPath);
            // simple replacement
            csproj.ExistsGroup("PropertyGroup").Should().BeTrue();
            csproj.ReplaceGroup("PropertyGroup", "Hogemoge");
            var x = csproj.ToString();
            csproj.ExistsGroup("Hogemoge").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceWithPatternTest(string csprojPath)
        {
            // before
            // <Project>
            //   <ItemGroup>
            //   </ItemGroup>
            // </Project>

            // after
            // <Project>
            //   <PiyoGroup>
            //   </PiyoGroup>
            // </Project>
            var csproj = Project.Load(csprojPath);
            // replacement can specify which letter to replace with via `pattern`.
            // In this case, node name `ProjectGuid` will replace `Guid` with `Hogemoge`, so the resuld must be `ProjectHogemoge`.
            csproj.ExistsGroup("ItemGroup").Should().BeTrue();
            csproj.ReplaceGroup("ItemGroup", "Item", "Piyo");
            var x = csproj.ToString();
            csproj.ExistsGroup("PiyoGroup").Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsGroup("HogemogeGroup").Should().BeFalse();
            csproj.ReplaceGroup("HogemogeGroup", "FugaFuga");
            csproj.ExistsGroup("FugaFuga").Should().BeFalse();

            csproj.ExistsGroup("HogemogeGroup").Should().BeFalse();
            csproj.ReplaceGroup("HogemogeGroup", "Hogemoge", "FugaFuga");
            csproj.ExistsGroup("HogemogeGroup").Should().BeFalse();
            csproj.ExistsGroup("FugaFugaGroup").Should().BeFalse();

            csproj.ExistsGroup("ItemFoo").Should().BeFalse();
            csproj.ReplaceGroup("ItemFoo", "Hoge", "Fuga");
            csproj.ExistsGroup("ItemFoo").Should().BeFalse();
            csproj.ExistsGroup("Hoge").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void RemoveTest(string csprojPath)
        {
            // CRLF test will only run on windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && csprojPath.Contains("CRLF"))
                return;

            var csproj = Project.Load(csprojPath);
            csproj.InsertGroup("Test");
            csproj.InsertNode("Test", "Foo", "value");
            var beforeCount = csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length;
            var x = csproj.ToString();

            // before
            // <Project>
            //   <Test>
            //     <Foo>
            //       value
            //     </Foo>
            //   </Test>
            // </Project>

            // after
            // <Project>
            //
            // </Project>

            // Remove with leave blank line test.
            csproj.ExistsGroup("Test").Should().BeTrue();
            var nodeCounts = csproj.GetNodes("Test").Length;
            csproj.RemoveGroup("Test", true);
            var y = csproj.ToString();
            csproj.ExistsGroup("Test").Should().BeFalse();
            var afterCount = csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length;
            afterCount.Should().Be(beforeCount - nodeCounts - 1);
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void RemoveAndBlankLineTest(string csprojPath)
        {
            // CRLF test will only run on windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && csprojPath.Contains("CRLF"))
                return;

            var csproj = Project.Load(csprojPath);
            csproj.InsertGroup("Test");
            csproj.InsertNode("Test", "Foo", "value");
            var beforeCount = csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length;
            var x = csproj.ToString();

            // before
            // <Project>
            //   <Test>
            //     <Foo>
            //       value
            //     </Foo>
            //   </Test>
            // </Project>

            // after
            // <Project>
            // </Project>

            // Remove node and remove blank line test.
            csproj.ExistsGroup("Test").Should().BeTrue();
            csproj.RemoveGroup("Test");
            var y = csproj.ToString();
            csproj.ExistsGroup("Test").Should().BeFalse();
            // remove blankline will remove all blank line. In this case, previous remove's blank line also removed.
            // 3 = Test + Foo + /Test
            // 1 = original space
            csproj.Root.ToString().Split(csproj.Eol.ToEolString()).Length.Should().Be(beforeCount - 3 - 1);
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void RemoveFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);

            // not exists node will not do any.
            csproj.ExistsGroup("Test").Should().BeFalse();
            csproj.RemoveGroup("Test", true);
        }
    }
}
