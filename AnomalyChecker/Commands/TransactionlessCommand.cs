using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnomalyChecker.Commands
{
    public class TransactionlessCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> _action;

        public TransactionlessCommand(Action<object> action)
        {
            this._action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action.Invoke(parameter);
        }
    }
}
