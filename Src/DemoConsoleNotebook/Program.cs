using DemoConsoleNotebook;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;

// command line args

var notebookName = Environment.GetCommandLineArgs()[1];

// setup

var kernel = new CompositeKernel();

var csharpKernel = new CSharpKernel()
    .UseDefaultFormatting()
    .UseNugetDirective()
    .UseKernelHelpers()
    .UseWho()
    .UseDotNetVariableSharing()
;

kernel.Add(csharpKernel);

// logging and handling events

var log = new Log();

kernel.KernelEvents.Subscribe(ev => {
    switch (ev)
    {
        case ReturnValueProduced rvp:
            if (rvp.Command is SubmitCode sc)
            {
                log.Code(sc.Code);
            }
            log.Value(rvp.Value.ToString());
            break;
        case CommandFailed cf:
            log.Error(cf.Message);
            break;
        case CommandSucceeded cs:
            //log.Info(cs.Command.ToString());
            break;
        default:
            //log.Info(ev.Command.ToString());
            break;
    }
});

// load notebook

var notebookBytes = await File.ReadAllBytesAsync(notebookName);
var text = Encoding.UTF8.GetString(notebookBytes);

var notebook = kernel.ParseNotebook(notebookName, notebookBytes);

// run notebook

var mre = new ManualResetEvent(false);
foreach(var cell in notebook.Cells)
{
    var result = await kernel.SendAsync(new SubmitCode(cell.Contents, cell.Language));
    result.KernelEvents.Subscribe((ev) =>
    {
        if (ev is CommandSucceeded || ev is CommandFailed)
        {
            log.Write();
            mre.Reset();
        }
    });
    mre.Set();
}

Console.Write("Press return...");
Console.ReadLine();
