using System;
using System.Collections.Generic;
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
    await next(command, context);
});

kernel.Add(csharpKernel);

var msgQueue = new Queue<string>();

kernel.KernelEvents.Subscribe(ev =>
{
    if (ev is ReturnValueProduced rvp)
    {
        foreach (var xx in rvp.FormattedValues)
        {
            //Console.WriteLine(xx);
            msgQueue.Enqueue(xx.ToString());
        }
    }
    else if (ev is CommandFailed cf)
    {
        msgQueue.Enqueue($"!{cf.Message}");
    }
    else
    {
        msgQueue.Enqueue(ev.ToString());
    }
    
});

var notebookName = @"sampleNotebook.ipynb";
var notebookBytes = await File.ReadAllBytesAsync(notebookName);

try
{
    msgQueue.Clear();
    var notebook = kernel.ParseNotebook(notebookName, notebookBytes);
    var cmdQueue = new Queue<KernelCommand>();
    foreach (var cell in notebook.Cells)
    {
        cmdQueue.Enqueue(new SubmitCode(cell.Contents, cell.Language));
    }
    await ExecuteCommand(kernel, cmdQueue, msgQueue);
}
catch (Exception ex)
{ 
}

Console.ReadLine();

static async Task ExecuteCommand(Kernel kernel, Queue<KernelCommand> commands, Queue<string> msgs)
{
    if (commands.Count == 0) return;
    var cmd = commands.Dequeue();
    var result = await kernel.SendAsync(cmd);
    result.KernelEvents.Subscribe(async (ev) =>
    {
        if (ev is CommandSucceeded || ev is CommandFailed)
        {
            while (true)
            {
                if (msgs.Count == 0) break;
                var msg = msgs.Dequeue();
                Console.WriteLine(msg);
            }
            await ExecuteCommand(kernel, commands, msgs);
        }
    });
}