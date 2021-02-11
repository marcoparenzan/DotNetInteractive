using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting;

using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace CSVExtension
{
    public class CSVKernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            System.Diagnostics.Debugger.Launch();

            var loadCsvCommand = new Command("#!loadcsv", "Load and parse a CSV file")
                {
                    new Option<string>(new[]{"-p","--path"},
                                    "The path of the file"),
                    new Option<char?>(new[]{"-s","--separatorChar"},
                                    "The separator char"),
                    new Option<char?>(new[]{"-i","--itemTypeName"},
                                    "The separator char"),
                    new Option<string>(new[]{"-v","--variableName"},
                                    "The name of the variable")
                };

            loadCsvCommand.Handler = CommandHandler.Create(
                async (string path, char? separatorChar, string itemTypeName, string variableName, KernelInvocationContext invocationContext) =>
                {
                    var sc = separatorChar ?? ';';
                    using (var reader = new StreamReader(path))
                    {
                        var fieldNames = reader.ReadLine().Split(sc);

                        // define class
                        var itemClassDef = $@"
                            class {itemTypeName} 
                            {{ 
                                { string.Join("\r\n", fieldNames.Select(xx => $"public string {xx} {{ get; set; }}").ToArray()) }
                            }}
                        ";
                        var itemClassDefCommand = new SubmitCode(itemClassDef);
                        await invocationContext.HandlingKernel.SendAsync(itemClassDefCommand);

                        // define list
                        var declareItemListDef = $"var {variableName} = new System.Collections.Generic.List<{itemTypeName}>();";
                        var declareItemListDefCommand = new SubmitCode(declareItemListDef);
                        await invocationContext.HandlingKernel.SendAsync(declareItemListDefCommand);

                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line)) break;
                            var fieldValues = line.Split(sc);
                            var fields = fieldNames.Zip(fieldValues); // zip together name/value as name has to be repeated on each item class instance

                            // add item
                            var addItemListDef = $"{variableName}.Add(new {itemTypeName} {{ {string.Join(",", fields.Select(xx => $"{xx.First} = \"{xx.Second}\"").ToArray()) } }});";
                            var addItemListDefCommand = new SubmitCode(addItemListDef);
                            await invocationContext.HandlingKernel.SendAsync(addItemListDefCommand);
                        }
                    }
                });

            kernel.AddDirective(loadCsvCommand);

            if (KernelInvocationContext.Current is { } context)
            {
                PocketView view = div(
                    code(nameof(CSVKernelExtension)),
                    " is loaded."
                );
                context.Display(view);
            }

            return Task.CompletedTask;
        }
    }
}