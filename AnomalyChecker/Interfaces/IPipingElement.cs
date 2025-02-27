using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;

namespace AnomalyChecker
{
    public interface IPipingElement<T> : IPipingElementBase
    {


        List<T> ReturnConnectedElements();

    }
}
