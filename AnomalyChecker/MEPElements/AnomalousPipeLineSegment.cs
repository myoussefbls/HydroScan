using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public class AnomalousPipeLineSegment
    {
        private List<IPipingElement> _ContainedElements = new List<IPipingElement> { };
        private List<IPipingElement> _Limits = new List<IPipingElement> { };
        public string AnomalyType { get; set; }

        private bool _areLimitsConnectedToAccessory ;

        public AnomalousPipeLineSegment(IPipingElement pipingElement)
        {
            _ContainedElements.Add(pipingElement);
            ExpandPipeline();
            RemoveDuplicates();
            DefineLimits();
            AreLimitsConnectedToAccessory();
            DefineAnomalyType();
        }

        private void AreLimitsConnectedToAccessory()
        {
            _areLimitsConnectedToAccessory = _Limits.Where(limit => limit is PipeWrapper || limit is PipeFitting)
                                                    .Any(limit => (limit as dynamic).IsConnectedToAccessory());
        }


        private void DefineAnomalyType() 
        {
            if (_ContainedElements.Count == 1)
            {
                var connectedElements = _ContainedElements[0].ReturnConnectedElements();
                int numberOfConnectedElements = connectedElements.Count;

                bool isCertainAnomaly = numberOfConnectedElements == 0 || numberOfConnectedElements > 1;
                this.AnomalyType = isCertainAnomaly ? "Anomalie certaine" : "Anomalie potentielle";
            }

            if (_ContainedElements.Count > 1)
            {
                var firstLimitConnections = _Limits[0].ReturnConnectedElements().Count;
                var secondLimitConnections = _Limits[1].ReturnConnectedElements().Count;

                bool isCertainAnomaly = (firstLimitConnections == 1 && secondLimitConnections == 1) || (firstLimitConnections > 1 && secondLimitConnections > 1);

                if (isCertainAnomaly == true && _areLimitsConnectedToAccessory == true) isCertainAnomaly = false;

                this.AnomalyType = isCertainAnomaly ? "Anomalie certaine" : "Anomalie potentielle";
            }
        }

        private void DefineLimits()
        {

            if (_ContainedElements.Count == 1) return;

            foreach (var pipingElement in _ContainedElements)
            {
                var connectedElements = pipingElement.ReturnConnectedElements();

                if (connectedElements.Count <= 1 || connectedElements.Any(elem => elem.Type == "Accessoire de canalisation"))
                {
                    _Limits.Add(pipingElement);
                    continue;
                }
                if (connectedElements.Any(element => !element.HasIncorrectMaterial)) _Limits.Add(pipingElement);
            }
        }


        private void ExpandPipeline()
        {
            HashSet<long> processedIDs = new HashSet<long>(_ContainedElements.Select(e => e.ElementID));

            bool hasNewElements;

            do
            {
                hasNewElements = false;
                List<IPipingElement> tempList = new List<IPipingElement>();

                foreach (var element in _ContainedElements)
                {
                    foreach (var connectedElement in element.ReturnConnectedElements())
                    {
                        if (connectedElement.Type == "Accessoire de canalisation") continue;
                        if (connectedElement.HasIncorrectMaterial == false) continue;
                        if (!processedIDs.Add(connectedElement.ElementID)) continue;

                        tempList.Add(connectedElement);
                        hasNewElements = true;
                    }
                }

                _ContainedElements.AddRange(tempList);
            } while (hasNewElements);
        }
        private void RemoveDuplicates()
        {
            _ContainedElements = _ContainedElements
                .GroupBy(e => e.ElementID)
                .Select(g => g.First())
                .ToList();
        }
        public bool Contains(IPipingElement pipingElement)
        {
            List<long> IDs = _ContainedElements.Select(pipingElem => pipingElem.ElementID).ToList();
            if (IDs.Contains(pipingElement.ElementID)) return true;
            return false;
        }
    }
}
