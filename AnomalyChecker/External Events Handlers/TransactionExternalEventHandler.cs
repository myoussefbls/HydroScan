using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashMarkerAddIn.External_Events_Handlers
{
    public class TransactionExternalEventHandler : ExternalEventHandler
    {
        Func<object, Result> _transactionFunc;
        private string _transactionName;

        public TransactionExternalEventHandler(Func<object, Result> function, string _transactionName)
        {
            if (_transactionFunc is null) this._transactionFunc = function;
            this._transactionName = _transactionName;
        }

        public override void Execute(UIApplication UIapp)
        {

            if (_transactionFunc == null) return;

            Transaction DocTransaction = new Transaction(UIapp.ActiveUIDocument.Document, _transactionName);
            DocTransaction.Start();

            Result functionResult = _transactionFunc.Invoke(UIapp);

            if (functionResult == Result.Succeeded) DocTransaction.Commit();
            else DocTransaction.RollBack();
        }

        public void RaiseEvent()
        {
            Raise();
        }
    }
}
