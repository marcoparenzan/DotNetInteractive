using Microsoft.AspNetCore.Http;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Notebook;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoWebAPI
{
    public class NotebookHandler
    {
        private CompositeKernel kernel;
        private CSharpKernel csharpKernel;
        private NotebookDocument notebook;
        private Log log;

        public NotebookHandler(string notebookName, byte[] notebookBytes)
        {
            this.kernel = new CompositeKernel();

            this.csharpKernel = new CSharpKernel()
                .UseDefaultFormatting()
                .UseNugetDirective()
                .UseKernelHelpers()
                .UseWho()
                .UseDotNetVariableSharing()
            ;

            this.kernel.Add(csharpKernel);

            // notebook

            this.notebook = kernel.ParseNotebook($"{notebookName}.ipynb", notebookBytes);

            // logging and handling events

            this.log = new Log();

            kernel.KernelEvents.Subscribe(ev =>
            {
                switch (ev)
                {
                    case ReturnValueProduced rvp:
                        //if (rvp.Command is SubmitCode sc)
                        //{
                        //    log.Code(sc.Code);
                        //}
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
        }

        public async Task Handle(HttpContext context)
        {
            var mre = new ManualResetEvent(false);
            foreach (var cell in notebook.Cells)
            {
                var result = await kernel.SendAsync(new SubmitCode(cell.Contents, cell.Language));
                result.KernelEvents.Subscribe((ev) =>
                {
                    if (ev is CommandSucceeded || ev is CommandFailed)
                    {
                        log.Write(context.Response.BodyWriter);
                        mre.Reset();
                    }
                });
                mre.Set();
            }
        }
    }
}
