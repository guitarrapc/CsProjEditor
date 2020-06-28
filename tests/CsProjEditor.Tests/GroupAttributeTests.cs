using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Xunit;

namespace CsProjEditor.Tests
{
    public class GroupAttributeTests
    {
        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj", new[] { null, "Condition", "Condition", null })]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj", new[] { null, "Condition", "Condition", null })]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj", new[] { null, "Condition" })]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj", new[] { null, "Condition" })] 
        public void GetTest(string csprojPath, string[] expected)
        {
            // <Project>
            //   <Property />
            //   <Property Condition="hoge" />
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.GetAttribute("PropertyGroup").Select(x => x?.Name).Should().BeEquivalentTo(expected);
            csproj.GetAttribute("PropertyGroup", onlyAttributed: true).Select(x => x.Name).Should().BeEquivalentTo(expected.Where(x => x != null).ToArray());
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void GetFailest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("PropertyGroup").Should().BeTrue();
            csproj.GetAttribute("PropertyGroup", "NoneX").Should().BeEmpty();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ExistsTest(string csprojPath)
        {
            // <Project>
            //   <PropertyGroup Condition="" />
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("PropertyGroup").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", new CsProjAttribute("Condition")).Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ExistsFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("PropertyGroup").Should().BeTrue();
            csproj.ExistsAttribute("PropertyGroup", new CsProjAttribute("ConditionX")).Should().BeFalse();
        }

        [Theory]
        //[InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        //[InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void InsertTest(string csprojPath)
        {
            // <Project>
            //   <PropertyGroup>
            //     <Hogemoge>value</Hogemoge>
            //     <Hogemoge Fugafuga="Value"/>
            //     <OutputType Fugafuga="Value"/>
            //   </PropertyGroup>
            // </Project>

            // Insert will generate group and attribute
            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("Import").Should().BeFalse();
            csproj.InsertAttribute("Import", new[] { new CsProjAttribute("Project", @".pack\package.csproj"), new CsProjAttribute("Condition", @"Exists('.pack\package.csproj')") }, e => !e.HasAttributes);
            var x = csproj.ToString();
            csproj.ExistsAttribute("Import", new CsProjAttribute("Project")).Should().BeTrue();
            csproj.ExistsAttribute("Import", new CsProjAttribute("Condition")).Should().BeTrue();

            csproj.GetAttribute("Import").Select(attribute => attribute.Name).Should().BeEquivalentTo(new[] { "Project" });
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void InsertFailTest(string csprojPath)
        {
            // never throw
            _ = Project.Load(csprojPath);
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void SetTest(string csprojPath)
        {
            // <Project>
            //   <Piyopiyo Fugafuga="value" />
            // </Project>

            var csproj = Project.Load(csprojPath);
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
            csproj.InsertAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), e => !e.HasAttributes);
            var x = csproj.ToString();
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeTrue();

            // <Project>
            //   <Piyopiyo Fugafuga="Value2" />
            // </Project>

            csproj.SetAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "Value2"), e => e.HasAttributes && e.FirstAttribute.Name == "Fugafuga" && e.FirstAttribute.Value == "value" );
            var y = csproj.ToString();
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "Value2")).Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void SetFailTest(string csprojPath)
        {
            // none existing group set will ignore
            var csproj = Project.Load(csprojPath);
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeFalse();
            csproj.SetAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), e => !e.HasAttributes);
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeFalse();
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
            //   <Piyopiyo Fugafuga="value" />
            // </Project>

            // after
            // <Project>
            //   <Piyopiyo Piyopiyo="value" />
            // </Project>

            var csproj = Project.Load(csprojPath);
            // simple replacement
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
            csproj.InsertAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), e => !e.HasAttributes);
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeTrue();
            var x = csproj.ToString();
            csproj.ReplaceAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), "Piyopiyo");
            var y = csproj.ToString();
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Piyopiyo", "value")).Should().BeTrue();
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
            //   <Piyopiyo Fugafuga="value" />
            // </Project>

            // after
            // <Project>
            //   <Piyopiyo HurohurogaHurohuroga="value" />
            // </Project>


            var csproj = Project.Load(csprojPath);
            // replacement can specify which letter to replace with via `pattern`.
            // In this case, node name `Fugafuga` will replace `Fuga` with `Hurohuro`, so the resuld must be `HurohurogaHurohuroga`.
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
            csproj.InsertAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), e => !e.HasAttributes);
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeTrue();
            var x = csproj.ToString();
            csproj.ReplaceAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), "Fu", "Hurohuro");
            var y = csproj.ToString();
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("HurohurogaHurohuroga", "value")).Should().BeTrue();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void ReplaceFailTest(string csprojPath)
        {
            var csproj = Project.Load(csprojPath);

            // not exists attribute will not do any.
            csproj.ExistsGroup("ItemGroup").Should().BeTrue();
            csproj.ReplaceAttribute("ItemGroup", new CsProjAttribute("IncludeX", "Microsoft.VCLibs, Version=14.0"), "Hogemoge");
            csproj.ExistsAttribute("ItemGroup", new CsProjAttribute("IncludeX", "Hogemoge")).Should().BeFalse();

            // not exists group will not do any
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
            csproj.ReplaceAttribute("Piyopiyo", new CsProjAttribute("Include", "Microsoft.VCLibs, Version=14.0"), "Hogemoge");
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
        }

        [Theory]
        [InlineData("testdata/SimpleOldCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleOldCsProjUtf8_LF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_CRLF.csproj")]
        [InlineData("testdata/SimpleNewCsProjUtf8_LF.csproj")]
        public void RemoveTest(string csprojPath)
        {
            // before
            // <Project>
            //   <ItemGroup Include="project.json" />
            // </Project>

            // after
            // <Project>
            //   <ItemGroup/>
            // </Project>
            var csproj = Project.Load(csprojPath);

            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
            csproj.InsertAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"), e => !e.HasAttributes);
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeTrue();
            csproj.RemoveAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"));
            var x = csproj.ToString();
            csproj.ExistsAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value")).Should().BeFalse();
            csproj.GetAttribute("Piyopiyo").Should().BeEquivalentTo(new CsProjAttribute[] { null });
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
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
            csproj.RemoveAttribute("Piyopiyo", new CsProjAttribute("Fugafuga", "value"));
            var x = csproj.ToString();
            csproj.ExistsGroup("Piyopiyo").Should().BeFalse();
        }
    }
}
