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

        private readonly List<Action> _actions;

        public TransactionlessCommand(params Action[] actions)
        {
            this._actions = new List<Action>(actions);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            foreach (var action in _actions)
            {
                action();
                //_action.Invoke(parameter);
            }       
        }
    }
}
