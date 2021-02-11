using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DemoWPFNotebook
{
    public class RelayCommand : ICommand
    {
        private Action<object> action;

        public RelayCommand(Action<object> action) => this.action = action;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.action?.Invoke(parameter);
        }
    }
}
