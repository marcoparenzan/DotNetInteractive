using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DemoNotebook
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        Dispatcher dispatcher;

        public BaseViewModel(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T property, T value, [CallerMemberName] string propertyName = default)
        {
            property = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected async Task InvokeAsync(Action a)
        {
            await dispatcher.InvokeAsync(a);
        }
    }
}
