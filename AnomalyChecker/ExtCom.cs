using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AnomalyChecker
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ExtCom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            ViewModel viewModel = new ViewModel(commandData);

            return Result.Succeeded;
        }
    }
}
