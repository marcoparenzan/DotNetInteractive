using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Server;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DemoNotebook
{
    public class MainWindowViewModel : BaseViewModel
    {
        private CompositeKernel kernel;

        public MainWindowViewModel(Dispatcher dispatcher): base(dispatcher)
        {
            kernel = new CompositeKernel();

            //_Kernel.UseLog();

            //var dispatcherCommand = new Command("#!dispatcher", "Enable or disable running code on the Dispatcher")
            //{
            //    new Option<bool>("--enabled", getDefaultValue:() => true)
            //};
            //dispatcherCommand.Handler = CommandHandler.Create<bool>(enabled =>
            //{
            //});
            //_Kernel.AddDirective(dispatcherCommand);
            ////Start named pipe
            //_Kernel.UseNamedPipeKernelServer("InteractiveWpf", new DirectoryInfo("."));

            var csharpKernel = new CSharpKernel()
                .UseDefaultFormatting()
                .UseNugetDirective()
                .UseKernelHelpers()
                .UseWho()
                .UseDotNetVariableSharing()
            ;
            csharpKernel.SetVariableAsync("App", this);

            csharpKernel.AddMiddleware(async (KernelCommand command, KernelInvocationContext context, KernelPipelineContinuation next) =>
            {
                await InvokeAsync(async () => await next(command, context));
            });

            kernel.Add(csharpKernel);

            kernel.KernelEvents.Subscribe(ev =>
            {
                if (ev is ReturnValueProduced rvp)
                {
                    foreach (var xx in rvp.FormattedValues)
                    {
                        Result += xx.Value;
                    }
                }
                else if (ev is CommandFailed cf)
                {
                    MessageBox.Show(cf.Message);
                }
                else
                {
                    
                }
            });

            Exec = new RelayCommand(async (arg) =>
            {
                var command = new SubmitCode(Text, csharpKernel.Name);
                await kernel.SendAsync(command);

            });
        }

        private string _text = @"
#r ""nuget:Newtonsoft.Json""
using System.Text.Json;
public class X
        {
            public int Y { get; set; }
            public int Z => Y * 3;
        }
        var x = new X { Y = 5 };
        JsonSerializer.Serialize(x)
";

        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }

        private string _result;
        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }

        public RelayCommand Exec { get; set; }
    }
}
