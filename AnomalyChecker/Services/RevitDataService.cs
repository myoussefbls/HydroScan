using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnomalyChecker.MEPElements;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker.Services
{
    public class RevitDataService
    {

        private Autodesk.Revit.DB.Document _revitDoc;
        public RevitDataService(Autodesk.Revit.DB.Document document) 
        {
            _revitDoc = document;
        }


        public IList<Element> ReturnPipingSystemsTypes() 
        {
            return new FilteredElementCollector(_revitDoc).OfClass(typeof(PipingSystemType)).ToElements();
        }

        public List<MEPSystem> ReturnMEPSystems()
        {
            return new FilteredElementCollector(_revitDoc).OfClass(typeof(MEPSystem)).Cast<MEPSystem>().ToList();
        }
        public List<PipingSystem> ReturnPipingSystems() 
        {
            return new FilteredElementCollector(_revitDoc).OfClass(typeof(PipingSystem)).Cast<PipingSystem>().ToList();
        }


    }
}
