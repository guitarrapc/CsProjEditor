using ConsoleAppFramework;
using System;
using System.IO;
using System.Threading.Tasks;
using CsProjEditor;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace csprojcli
{
    // make UWP Store associated csproj.
    // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
    class Program
    {
        static async Task Main(string[] args)
            => await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<EditCsProj>(args);
    }

    public class EditCsProj : ConsoleAppBase
    {
        private void Save(Project csproj, string path, string output, bool allowoverwrite)
        {
            if (string.IsNullOrEmpty(output))
            {
                output = Path.Combine(Path.GetDirectoryName(path), "modify_" + Path.GetFileName(path));
            }
            if (File.Exists(output) && !allowoverwrite)
                throw new IOException($"Output path {output} already exists. Please use `-allowoverwrite true`.");
            csproj.Save(output);
            Console.WriteLine($"complete! new csproj generated at {output}");
        }

        [Command(new[] { "version", "-v", "-version", "--version" }, "show version")]
        public void Version()
        {
            var version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion
                .ToString();
            Console.WriteLine($"csprojcli v{version}");
        }

        [Command(new[] { "help", "list", "-h", "-help", "--help" }, "show help")]
        public void Help()
        {
            Console.WriteLine("Usage: csprojcli [version] [help] [batch.loadandrun|batch.run|groups.get|group.get|group.exists|group.insert|group.replace|group.remove|nodes.get|node.get|node.exists|node.insert|node.replace|node.remove|nodevalue.get|nodevalue.exists|nodevalue.set|nodevalue.append|nodevalue.prepend|nodevalue.replace|nodevalue.remove|attribute.get|attribute.exists|attribute.insert|attribute.replace|attribute.remove|attributevalue.get|attributevalue.exists|attributevalue.set|attributevalue.append|attributevalue.prepend|attributevalue.replace|attributevalue.remove] [parameters]");
            Console.WriteLine();
            Console.WriteLine("E.g., run this for Batch execution. see JSON sample at https://raw.githubusercontent.com/guitarrapc/CsProjEditor/master/src/csprojcli/sample.json ");
            Console.WriteLine("--------------------------");
            Console.WriteLine("$ csprojcli batch.loadandrun -jsonPath examples/csprojcli/uwp_storepublish.json");
            Console.WriteLine("$ csprojcli batch.run -json JSON");
            Console.WriteLine();
            Console.WriteLine("E.g., run this for group execution.:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("$ csprojcli groups.get -p SimpleNewCsProjUtf8_CRLF.csproj");
            Console.WriteLine("$ csprojcli group.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup");
            Console.WriteLine("$ csprojcli group.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup");
            Console.WriteLine("$ csprojcli group.insert -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli group.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -pattern Property -replacement Foo -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli group.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Console.WriteLine("E.g., run this for node execution.:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("$ csprojcli nodes.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup");
            Console.WriteLine("$ csprojcli node.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework");
            Console.WriteLine("$ csprojcli node.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework");
            Console.WriteLine("$ csprojcli node.insert -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n PackageCertificateThumbprint -v 1234567890ABCDEF -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli node.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n LangVersion -pattern latest -replacement preview -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli node.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n LangVersion -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Console.WriteLine("E.g., run this for node value execution.:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("$ csprojcli nodevalue.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework");
            Console.WriteLine("$ csprojcli nodevalue.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework -v netstandard2.0");
            Console.WriteLine("$ csprojcli nodevalue.set -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n PackageCertificateKeyFile -v hogehoge.pfx -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli nodevalue.append -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n RootNamespace -v SimpleCsProj -append ect -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli nodevalue.prepend -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n RootNamespace -v SimpleCsProj -prepend Very -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli nodevalue.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n OutputType -v Exe -pattern Exe -replacement AppContainer -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli nodevalue.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n AssemblyName -v SimpleCsProj -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Console.WriteLine("E.g., run this for attribute execution.:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("$ csprojcli attribute.get -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None");
            Console.WriteLine("$ csprojcli attribute.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include");
            Console.WriteLine("$ csprojcli attribute.insert -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v example.json -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli attribute.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -pattern None -replacement Content -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli attribute.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Exclude -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Console.WriteLine("E.g., run this for attribute value execution.:");
            Console.WriteLine("--------------------------");
            Console.WriteLine("$ csprojcli attributevalue.get -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include");
            Console.WriteLine("$ csprojcli attributevalue.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n Compile -a Include -v App.cs");
            Console.WriteLine("$ csprojcli attributevalue.set -p SimpleNewCsProjUtf8_CRLF.csproj -g Target -n Message -a Importance -v low - -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli attributevalue.append -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -append ect -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli attributevalue.prepend -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -prepend Very -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli attributevalue.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -pattern project -replacement example -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine("$ csprojcli attributevalue.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
        }

        [Command("batch.loadandrun", "load json definition and run.")]
        public void LoadAndRun(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);
            Run(json);
        }
        [Command("batch.run", "run as json definition.")]
        public void Run(string json)
        {
            var scheme = Utf8Json.JsonSerializer.Deserialize<Scheme>(json);

            // validate
            if (string.IsNullOrWhiteSpace(scheme.path)) throw new ArgumentNullException($"{nameof(scheme.path)} is missing. please add `path` to specify input csproj path.");
            if (string.IsNullOrWhiteSpace(scheme.output)) throw new ArgumentNullException($"{nameof(scheme.output)} is missing. please add `output` to specify output csproj path.");
            if (scheme.dry) Console.WriteLine("Detected Dry-run mode.");
            if (scheme.path == scheme.output && scheme.allowoverwrite) Console.WriteLine("Detected overwrite csproj.");

            // run
            var csproj = Project.Load(scheme.path);

            foreach (var command in scheme.commands.OrderBy(x => x.order))
            {
                Console.WriteLine($"#{command.order}: Running {command.type}.{command.command}. group: {command.parameter.group}, node: {command.parameter.node}");

                // validate required parameter
                if (string.IsNullOrWhiteSpace(command.parameter.group))
                {
                    Console.WriteLine($"#{command.order}: {command.parameter.group} was empty, please specify node. skip and run next.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(command.parameter.node))
                {
                    Console.WriteLine($"#{command.order}: {command.parameter.node} was empty, please specify node. skip and run next.");
                    continue;
                }

                if (command.type == "group")
                {
                    switch (command.command)
                    {
                        case "insert":
                            csproj.InsertGroup(command.parameter.group);
                            break;
                        case "replace":
                            csproj.ReplaceGroup(command.parameter.group, command.parameter.pattern, command.parameter.replacement);
                            break;
                        case "remove":
                            csproj.RemoveGroup(command.parameter.group, command.parameter.leaveBrankLine);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (command.type == "node")
                {
                    switch (command.command)
                    {
                        case "insert":
                            csproj.InsertNode(command.parameter.group, command.parameter.node, command.parameter.value);
                            break;
                        case "replace":
                            csproj.ReplaceNode(command.parameter.group, command.parameter.node, command.parameter.pattern, command.parameter.replacement);
                            break;
                        case "remove":
                            csproj.RemoveNode(command.parameter.group, command.parameter.node, command.parameter.leaveBrankLine);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (command.type == "nodevalue")
                {
                    switch (command.command)
                    {
                        case "set":
                            if (string.IsNullOrWhiteSpace(command.parameter.newvalue))
                            {
                                csproj.SetNodeValue(command.parameter.group, command.parameter.node, command.parameter.value);
                            }
                            else
                            {
                                csproj.SetNodeValue(command.parameter.group, command.parameter.node, command.parameter.value, command.parameter.newvalue);
                            }
                            break;
                        case "append":
                            csproj.AppendNodeValue(command.parameter.group, command.parameter.node, command.parameter.value, command.parameter.newvalue);
                            break;
                        case "prepend":
                            csproj.PrependNodeValue(command.parameter.group, command.parameter.node, command.parameter.value, command.parameter.newvalue);
                            break;
                        case "replace":
                            csproj.ReplaceNodeValue(command.parameter.group, command.parameter.node, command.parameter.value, command.parameter.pattern, command.parameter.replacement);
                            break;
                        case "remove":
                            csproj.RemoveNodeValue(command.parameter.group, command.parameter.node);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (command.type == "attribute")
                {
                    switch (command.command)
                    {
                        case "insert":
                            if (string.IsNullOrEmpty(command.parameter.node))
                            {
                                csproj.InsertAttribute(command.parameter.group, new CsProjAttribute(command.parameter.attribute, command.parameter.value), e => !e.HasAttributes);
                            }
                            else
                            {
                                csproj.InsertAttribute(command.parameter.group, command.parameter.node, new CsProjAttribute(command.parameter.attribute, command.parameter.value), e => !e.HasAttributes);
                            }
                            break;
                        case "replace":
                            if (string.IsNullOrEmpty(command.parameter.node))
                            {
                                csproj.ReplaceAttribute(command.parameter.group, new CsProjAttribute(command.parameter.attribute, command.parameter.value), command.parameter.pattern, command.parameter.replacement);
                            }
                            else
                            {
                                csproj.ReplaceAttribute(command.parameter.group, command.parameter.node, new CsProjAttribute(command.parameter.attribute, command.parameter.value), command.parameter.pattern, command.parameter.replacement);
                            }
                            break;
                        case "remove":
                            if (string.IsNullOrEmpty(command.parameter.node))
                            {
                                csproj.RemoveAttribute(command.parameter.group, new CsProjAttribute(command.parameter.attribute, command.parameter.value));
                            }
                            else
                            {
                                csproj.RemoveAttribute(command.parameter.group, command.parameter.node, new CsProjAttribute(command.parameter.attribute, command.parameter.value));
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else if (command.type == "attributevalue")
                {
                    switch (command.command)
                    {
                        case "set":
                            csproj.SetAttributeValue(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value);
                            break;
                        case "append":
                            csproj.AppendAttributeValue(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value, command.parameter.newvalue);
                            break;
                        case "prepend":
                            csproj.PrependAttributeValue(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value, command.parameter.newvalue);
                            break;
                        case "replace":
                            csproj.ReplaceAttributeValue(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value, command.parameter.pattern, command.parameter.replacement);
                            break;
                        case "remove":
                            csproj.RemoveAttributeValue(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            if (scheme.dry)
            {
                Console.WriteLine($"Complete all commands. Showing evaluate result.");
                Console.WriteLine($"--------------------------");
                Console.WriteLine(csproj.ToString());
                return;
            }

            Console.WriteLine($"saving generated csproj to {scheme.output} (Override: {File.Exists(scheme.output)})");
            if (File.Exists(scheme.output) && !scheme.allowoverwrite)
            {
                throw new IOException($"Output path {scheme.output} already exists. Please use `-allowoverwrite true`.");
            }
            csproj.Save(scheme.output);
            Console.WriteLine($"complete! new csproj generated at {scheme.output}");
        }

        [Command("groups.get", "get group.")]
        public void GroupGet([Option("p", "path of csproj.")]string path)
        {
            var results = Project.Load(path).GetGroups();

            if (!results.Any()) Console.WriteLine($"group not found.");
            foreach (var item in results)
            {
                Console.WriteLine(item);
            }
        }
        [Command("group.get", "get group.")]
        public void GroupGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group)
        {
            var results = Project.Load(path).GetGroup(group);

            if (!results.Any()) Console.WriteLine($"group `{group}` not found.");
            foreach (var item in results)
            {
                Console.WriteLine($"{group}: {item}");
            }
        }
        [Command("group.exists", "check specified group is exists.")]
        public void GroupExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group)
        {
            var item = Project.Load(path).ExistsGroup(group);
            Console.WriteLine(item.ToString());
        }
        [Command("group.insert", "insert specified group.")]
        public void GroupInsert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertGroup(group);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("group.replace", "replace specified group.")]
        public void GroupReplace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            string pattern,
            string replacement,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceGroup(group, pattern, replacement);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("group.remove", "remove specified group.")]
        public void GroupRemove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("l", "leave brank line")]bool leaveBrankLine = false,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveGroup(group, leaveBrankLine);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("nodes.get", "get nodes for the group.")]
        public void NodesGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            int index = -1)
        {
            var results = index < 0
                ? Project.Load(path).GetNodes(group)
                : Project.Load(path).GetNodes(group, index);

            if (!results.Any()) Console.WriteLine($"index `{index}` not found.");
            foreach (var item in results)
            {
                Console.WriteLine($"{group}[{index}]: {item}");
            }
        }
        [Command("node.get", "get node for the group.")]
        public void NodeGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetNode(group, node);

            if (!results.Any()) Console.WriteLine($"node `{node}` not found.");
            foreach (var item in results)
            {
                Console.WriteLine($"{group}: {item}");
            }
        }
        [Command("node.exists", "check specified node is exists.")]
        public void NodeExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var item = Project.Load(path).ExistsNode(group, node);
            Console.WriteLine(item.ToString());
        }
        [Command("node.insert", "insert specified node.")]
        public void NodeInsert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value = "",
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertNode(group, node, value);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("node.replace", "replace specified node.")]
        public void NodeReplace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            string pattern,
            string replacement,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceNode(group, node, pattern, replacement);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("node.remove", "remove specified node.")]
        public void NodeRemove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("l", "leave brank line")]bool leaveBrankLine = false,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveNode(group, node, leaveBrankLine);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("nodevalue.get", "get value for the node.")]
        public void NodeValueGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetNodeValue(group, node);

            if (!results.Any()) Console.WriteLine($"node `{node}` not found.");
            foreach (var item in results)
            {
                Console.WriteLine($"{group}.{node}: {item}");
            }
        }
        [Command("nodevalue.exists", "check specified node's value is exists.")]
        public void NodeValueExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value)
        {
            var item = Project.Load(path).ExistsNodeValue(group, node, value);
            Console.WriteLine(item.ToString());
        }
        [Command("nodevalue.set", "set specified node value.")]
        public void NodeValueSet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.SetNodeValue(group, node, value);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("nodevalue.append", "append specified node value.")]
        public void NodeValueAppend([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            string append,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.AppendNodeValue(group, node, value, append);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("nodevalue.prepend", "preppend specified node value.")]
        public void NodeValuePrepend(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            string prepend,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.PrependNodeValue(group, node, value, prepend);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("nodevalue.replace", "replace specified node value.")]
        public void NodeValueReplace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            string pattern,
            string replacement,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceNodeValue(group, node, value, pattern, replacement);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("nodevalue.remove", "remove specified node value.")]
        public void NodeValueRemove([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveNodeValue(group, node);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("attribute.get", "get attribute for the group.")]
        public void AttributeGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node = "")
        {
            var results = string.IsNullOrEmpty(node)
                ? Project.Load(path).GetAttribute(group)
                : Project.Load(path).GetAttribute(group, node);

            if (!results.Any())
            {
                if (string.IsNullOrEmpty(node))
                {
                    Console.WriteLine($"group `{group}` not found.");
                }
                else
                {
                    Console.WriteLine($"node `{node}` not found.");
                }
            }
            foreach (var item in results)
            {
                Console.WriteLine($"{group}: {item}");
            }
        }
        [Command("attribute.exists", "check specified attribute is exists.")]
        public void AttributeExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("a", "attribute of node")]string attribute,
            [Option("n", "name of node")] string node = "")
        {
            var item = string.IsNullOrEmpty(node)
                ? Project.Load(path).ExistsAttribute(group, new CsProjAttribute(attribute))
                : Project.Load(path).ExistsAttribute(group, node, new CsProjAttribute(attribute));
            Console.WriteLine(item.ToString());
        }
        [Command("attribute.insert", "insert specified attribute.")]
        public void AttributeInsert(
            [Option("p", "path of csproj.")] string path,
            [Option("g", "group of nodes. eg. PropertyGroup")] string group,
            [Option("a", "attribute of node")] string[] attribute,
            [Option("n", "name of node")] string node = "",
            [Option("v", "value of attribute")] string[] value = null,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            if (value == null)
                value = Enumerable.Range(0, attribute.Length).Select(x => "").ToArray();
            var attributes = attribute.Zip(value, (a, v) => new CsProjAttribute(a, v)).ToArray();
            if (string.IsNullOrEmpty(node))
            {
                csproj.InsertAttribute(group, attributes, e => !e.HasAttributes);
            }
            else
            {
                csproj.InsertAttribute(group, node, attributes, e => !e.HasAttributes);
            }
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attribute.replace", "replace specified attribute with node.")]
        public void AttributeReplace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string pattern,
            string replacement,
            [Option("n", "name of node")] string node = "",
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            if (string.IsNullOrEmpty(node))
            {
                csproj.ReplaceAttribute(group, new CsProjAttribute(attribute, value), pattern, replacement);
            }
            else
            {
                csproj.ReplaceAttribute(group, node, new CsProjAttribute(attribute, value), pattern, replacement);
            }
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attribute.remove", "remove specified attribute.")]
        public void AttributeRemove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("a", "attribute of node")]string attribute,
            [Option("n", "name of node")] string node = "",
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            if (string.IsNullOrEmpty(node))
            {
                csproj.RemoveAttribute(group, new CsProjAttribute(attribute));
            }
            else
            {
                csproj.RemoveAttribute(group, node, new CsProjAttribute(attribute));
            }
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("attributevalue.get", "get value for the attribute.")]
        public void AttributeValueGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute)
        {
            var results = Project.Load(path).GetAttributeValue(group, node, attribute);

            if (!results.Any()) Console.WriteLine($"node `{node}` not found.");
            foreach (var item in results)
            {
                Console.WriteLine($"{group}.{node}: {item}");
            }
        }
        [Command("attributevalue.exists", "check specified attribute's value is exists.")]
        public void AttributeValueExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value)
        {
            var item = Project.Load(path).ExistsAttributeValue(group, node, attribute, value);
            Console.WriteLine(item.ToString());
        }
        [Command("attributevalue.set", "set specified attribute value.")]
        public void AttributeValueSet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.SetAttributeValue(group, node, attribute, value);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attributevalue.append", "append specified attribute value.")]
        public void AttributeValueAppend([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string append = "",
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.AppendAttributeValue(group, node, attribute, value, append);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attributevalue.prepend", "preppend specified attribute value.")]
        public void AttributeValuePrepend(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string prepend = "",
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.PrependAttributeValue(group, node, attribute, value, prepend);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attributevalue.replace", "replace specified attribute value.")]
        public void AttributeValueReplace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string pattern,
            string replacement,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceAttributeValue(group, node, attribute, value, pattern, replacement);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attributevalue.remove", "remove specified attribute value.")]
        public void AttributeValueRemove([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            bool dry = false,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveAttributeValue(group, node, attribute, value);
            if (dry)
            {
                Console.WriteLine(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
    }
}
