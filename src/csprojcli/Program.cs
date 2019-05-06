using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using CsProjEditor;
using System.Linq;
using System.Reflection;

namespace csprojcli
{
    // make UWP Store associated csproj.
    // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
    class Program
    {
        static async Task Main(string[] args)
            => await BatchHost.CreateDefaultBuilder()
                .RunBatchEngineAsync<EditCsProj>(args);
    }

    public class EditCsProj : BatchBase
    {
        private void Save(Project csproj, string path, string output, bool allowoverwrite)
        {
            if (string.IsNullOrEmpty(output)) output = "modify_" + path;
            if (File.Exists(output) && !allowoverwrite)
                throw new IOException($"Output path {output} already exists. Please use `-allowoverwrite true`.");
            csproj.Save(output);
            this.Context.Logger.LogInformation($"complete! new csproj generated at {output}");
        }

        [Command(new[] { "version", "-v", "-version", "--version" }, "show version")]
        public void Version()
        {
            var version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion
                .ToString();
            Context.Logger.LogInformation($"csprojcli v{version}");
        }

        [Command(new[] { "help", "list", "-h", "-help", "--help" }, "show help")]
        public void Help()
        {
            Context.Logger.LogInformation("Usage: csprojcli [version] [help] [batch.loadandrun|batch.run|groups.get|group.get|group.exists|group.insert|group.replace|group.remove|nodes.get|node.get|node.exists|node.insert|node.replace|node.remove|nodevalue.get|nodevalue.exists|nodevalue.set|nodevalue.append|nodevalue.prepend|nodevalue.replace|nodevalue.remove|attribute.get|attribute.exists|attribute.insert|attribute.replace|attribute.remove|attributevalue.get|attributevalue.exists|attributevalue.set|attributevalue.append|attributevalue.prepend|attributevalue.replace|attributevalue.remove] [parameters]");
            Console.WriteLine();
            Context.Logger.LogInformation("E.g., run this for Batch execution. see JSON sample at https://raw.githubusercontent.com/guitarrapc/CsProjEditor/master/src/csprojcli/sample.json ");
            Context.Logger.LogInformation("--------------------------");
            Context.Logger.LogInformation("$ csprojcli batch.loadandrun -jsonPath examples/csprojcli/uwp_storepublish.json");
            Context.Logger.LogInformation("$ csprojcli batch.run -json JSON");
            Console.WriteLine();
            Context.Logger.LogInformation("E.g., run this for group execution.:");
            Context.Logger.LogInformation("--------------------------");
            Context.Logger.LogInformation("$ csprojcli groups.get -p SimpleNewCsProjUtf8_CRLF.csproj");
            Context.Logger.LogInformation("$ csprojcli group.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup");
            Context.Logger.LogInformation("$ csprojcli group.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup");
            Context.Logger.LogInformation("$ csprojcli group.insert -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli group.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -pattern Property -replacement Foo -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli group.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Context.Logger.LogInformation("E.g., run this for node execution.:");
            Context.Logger.LogInformation("--------------------------");
            Context.Logger.LogInformation("$ csprojcli nodes.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup");
            Context.Logger.LogInformation("$ csprojcli node.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework");
            Context.Logger.LogInformation("$ csprojcli node.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework");
            Context.Logger.LogInformation("$ csprojcli node.insert -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n PackageCertificateThumbprint -v 1234567890ABCDEF -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli node.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n LangVersion -pattern latest -replacement preview -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli node.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n LangVersion -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Context.Logger.LogInformation("E.g., run this for node value execution.:");
            Context.Logger.LogInformation("--------------------------");
            Context.Logger.LogInformation("$ csprojcli nodevalue.get -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework");
            Context.Logger.LogInformation("$ csprojcli nodevalue.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n TargetFramework -v netstandard2.0");
            Context.Logger.LogInformation("$ csprojcli nodevalue.set -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n PackageCertificateKeyFile -v hogehoge.pfx -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli nodevalue.append -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n RootNamespace -v SimpleCsProj -append ect -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli nodevalue.prepend -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n RootNamespace -v SimpleCsProj -prepend Very -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli nodevalue.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n OutputType -v Exe -pattern Exe -replacement AppContainer -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli nodevalue.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g PropertyGroup -n AssemblyName -v SimpleCsProj -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Context.Logger.LogInformation("E.g., run this for attribute execution.:");
            Context.Logger.LogInformation("--------------------------");
            Context.Logger.LogInformation("$ csprojcli attribute.get -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None");
            Context.Logger.LogInformation("$ csprojcli attribute.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include");
            Context.Logger.LogInformation("$ csprojcli attribute.insert -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v example.json -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli attribute.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -pattern None -replacement Content -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli attribute.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Exclude -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Console.WriteLine();
            Context.Logger.LogInformation("E.g., run this for attribute value execution.:");
            Context.Logger.LogInformation("--------------------------");
            Context.Logger.LogInformation("$ csprojcli attributevalue.get -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include");
            Context.Logger.LogInformation("$ csprojcli attributevalue.exists -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n Compile -a Include -v App.cs");
            Context.Logger.LogInformation("$ csprojcli attributevalue.set -p SimpleNewCsProjUtf8_CRLF.csproj -g Target -n Message -a Importance -v low - -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli attributevalue.append -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -append ect -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli attributevalue.prepend -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -prepend Very -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli attributevalue.replace -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -pattern project -replacement example -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
            Context.Logger.LogInformation("$ csprojcli attributevalue.remove -p SimpleNewCsProjUtf8_CRLF.csproj -g ItemGroup -n None -a Include -v project.json -dry false -output result_SimpleNewCsProjUtf8_CRLF.csproj -allowoverwrite true");
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
            if (scheme.dry) Context.Logger.LogInformation("Detected Dry-run mode.");
            if (scheme.path == scheme.output && scheme.allowoverwrite) Context.Logger.LogInformation("Detected overwrite csproj.");

            // run
            var csproj = Project.Load(scheme.path);

            foreach (var command in scheme.commands.OrderBy(x => x.order))
            {
                Context.Logger.LogInformation($"#{command.order}: Running {command.type}.{command.command}. group: {command.parameter.group}, node: {command.parameter.node}");

                // validate required parameter
                if (string.IsNullOrWhiteSpace(command.parameter.group))
                {
                    Context.Logger.LogWarning($"#{command.order}: {command.parameter.group} was empty, please specify node. skip and run next.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(command.parameter.node))
                {
                    Context.Logger.LogWarning($"#{command.order}: {command.parameter.node} was empty, please specify node. skip and run next.");
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
                            csproj.InsertAttribute(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value, e => !e.HasAttributes);
                            break;
                        case "replace":
                            csproj.ReplaceAttribute(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value, command.parameter.pattern, command.parameter.replacement);
                            break;
                        case "remove":
                            csproj.RemoveAttribute(command.parameter.group, command.parameter.node, command.parameter.attribute, command.parameter.value);
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
                Context.Logger.LogInformation($"Complete all commands. Showing evaluate result.");
                Context.Logger.LogInformation($"--------------------------");
                Context.Logger.LogInformation(csproj.ToString());
                return;
            }

            this.Context.Logger.LogInformation($"saving generated csproj to {scheme.output} (Override: {File.Exists(scheme.output)})");
            if (File.Exists(scheme.output) && !scheme.allowoverwrite)
            {
                throw new IOException($"Output path {scheme.output} already exists. Please use `-allowoverwrite true`.");
            }
            csproj.Save(scheme.output);
            this.Context.Logger.LogInformation($"complete! new csproj generated at {scheme.output}");
        }

        [Command("groups.get", "get group.")]
        public void GroupGet([Option("p", "path of csproj.")]string path)
        {
            var results = Project.Load(path).GetGroups();

            if (!results.Any()) this.Context.Logger.LogInformation($"group not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation(item);
            }
        }
        [Command("group.get", "get group.")]
        public void GroupGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group)
        {
            var results = Project.Load(path).GetGroup(group);

            if (!results.Any()) this.Context.Logger.LogInformation($"group `{group}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}: {item}");
            }
        }
        [Command("group.exists", "check specified group is exists.")]
        public void GroupExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group)
        {
            var item = Project.Load(path).ExistsGroup(group);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("group.insert", "insert specified group.")]
        public void GroupInsert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertGroup(group);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceGroup(group, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("group.remove", "remove specified group.")]
        public void GroupRemove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("l", "leave brank line")]bool leaveBrankLine = false,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveGroup(group, leaveBrankLine);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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

            if (!results.Any()) this.Context.Logger.LogInformation($"index `{index}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}[{index}]: {item}");
            }
        }
        [Command("node.get", "get node for the group.")]
        public void NodeGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetNode(group, node);

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}: {item}");
            }
        }
        [Command("node.exists", "check specified node is exists.")]
        public void NodeExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var item = Project.Load(path).ExistsNode(group, node);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("node.insert", "insert specified node.")]
        public void NodeInsert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value = "",
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertNode(group, node, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceNode(group, node, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveNode(group, node, leaveBrankLine);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}.{node}: {item}");
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
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("nodevalue.set", "set specified node value.")]
        public void NodeValueSet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.SetNodeValue(group, node, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.AppendNodeValue(group, node, value, append);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.PrependNodeValue(group, node, value, prepend);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceNodeValue(group, node, value, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("nodevalue.remove", "remove specified node value.")]
        public void NodeValueRemove([Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("v", "value of node")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveNodeValue(group, node);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("attribute.get", "get attribute for the group.")]
        public void AttributeGet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node)
        {
            var results = Project.Load(path).GetAttribute(group, node);

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}: {item}");
            }
        }
        [Command("attribute.exists", "check specified attribute is exists.")]
        public void AttributeExists(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute)
        {
            var item = Project.Load(path).ExistsAttribute(group, node, attribute);
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("attribute.insert", "insert specified attribute.")]
        public void AttributeInsert(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value = "",
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.InsertAttribute(group, node, attribute, value, e => !e.HasAttributes);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
        [Command("attribute.replace", "replace specified attribute with node.")]
        public void AttributeReplace(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            string pattern,
            string replacement,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceAttribute(group, node, attribute, value, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }

        [Command("attribute.remove", "remove specified attribute.")]
        public void AttributeRemove(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveAttribute(group, node, attribute);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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

            if (!results.Any()) this.Context.Logger.LogInformation($"node `{node}` not found.");
            foreach (var item in results)
            {
                this.Context.Logger.LogInformation($"{group}.{node}: {item}");
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
            this.Context.Logger.LogInformation(item.ToString());
        }
        [Command("attributevalue.set", "set specified attribute value.")]
        public void AttributeValueSet(
            [Option("p", "path of csproj.")]string path,
            [Option("g", "group of nodes. eg. PropertyGroup")]string group,
            [Option("n", "name of node")]string node,
            [Option("a", "attribute of node")]string attribute,
            [Option("v", "value of attribute")]string value,
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.SetAttributeValue(group, node, attribute, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.AppendAttributeValue(group, node, attribute, value, append);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.PrependAttributeValue(group, node, attribute, value, prepend);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.ReplaceAttributeValue(group, node, attribute, value, pattern, replacement);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
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
            bool dry = true,
            string output = "",
            bool allowoverwrite = false)
        {
            var csproj = Project.Load(path);
            csproj.RemoveAttributeValue(group, node, attribute, value);
            if (dry)
            {
                this.Context.Logger.LogInformation(csproj.ToString());
                return;
            }
            Save(csproj, path, output, allowoverwrite);
        }
    }
}
