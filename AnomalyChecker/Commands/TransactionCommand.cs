using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashMarkerAddIn.External_Events_Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClashMarkerAddIn.Commands
{
    public class TransactionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        TransactionExternalEventHandler extEventHandler;

        public TransactionCommand(Func<object, Result> func, string transactionName)
        {
            this.extEventHandler = new TransactionExternalEventHandler(func, transactionName);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.extEventHandler.RaiseEvent();
        }
    }
}
