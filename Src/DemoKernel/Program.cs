using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;

var kernel = new CompositeKernel();

var csharpKernel = new CSharpKernel()
    .UseDefaultFormatting()
    .UseNugetDirective()
    .UseKernelHelpers()
    .UseWho()
    .UseDotNetVariableSharing()
;

csharpKernel.AddMiddleware(async (KernelCommand command, KernelInvocationContext context, KernelPipelineContinuation next) =>
{
    context.DisplayStandardOut(command.ToString());
    await next(command, context);
});

kernel.Add(csharpKernel);

kernel.KernelEvents.Subscribe(ev =>
{
    if (ev is ReturnValueProduced rvp)
    {
        foreach (var xx in rvp.FormattedValues)
        {
            Console.WriteLine(xx);
        }
    }
    else if (ev is CommandFailed cf)
    {
        Console.Error.WriteLine(ev);
    }
    else
    {
        Console.WriteLine($"{ev.Command}");
    }
});

var notebookName = @"sampleNotebook.ipynb";
var notebookBytes = await File.ReadAllBytesAsync(notebookName);

try
{
    var notebook = kernel.ParseNotebook(notebookName, notebookBytes);
    foreach (var cell in notebook.Cells)
    {
        var command = new SubmitCode(cell.Contents, cell.Language);
        var result = await kernel.SendAsync(command);
    }
}
catch (Exception ex)
{ 
}
