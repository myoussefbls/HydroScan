using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public static class PipeTypeExtensions
    {
        public static List<PipeFitting> ReturnRelatedPipeFittings(this Pipe selectedPipe) 
        {
            List<PipeFitting> relatedPipeFittings = new List<PipeFitting>();

            ConnectorSet connectors = selectedPipe.ConnectorManager.Connectors;

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    if (connectedConnector.Owner as FamilyInstance == null) continue;
                    relatedPipeFittings.Add(new PipeFitting(connectedConnector.Owner as FamilyInstance));
                }
            }

            return relatedPipeFittings;
        }

    }
}
