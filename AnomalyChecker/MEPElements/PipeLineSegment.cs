using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public class PipeLineSegment
    {
        private List<Pipe> _pipes;

        private List<PipeFitting> _pipeFittings;

        private List<IPipingElementBase> _ContainedElements = new List<IPipingElementBase> { };

        private List<IPipingElementBase> _Limits = new List<IPipingElementBase> { };

        private Material _relatedMaterial;

        public string AnomalyType { get; set; }


        public PipeLineSegment(IPipingElementBase pipingElement)
        {
            this._relatedMaterial = pipingElement.Material;
            _ContainedElements.Add(pipingElement);
            ExpandPipeline();
            RemoveDuplicates();
            DefineLimits();
            DefineAnomalyType();
        }

        private void DefineAnomalyType() 
        {
            //if (_ContainedElements.Count == 1) 
            //{
            //    List<IPipingElementBase> connectedElements = _ContainedElements[0].ReturnConnectedElements();

            //    if (connectedElements.Count > 1)
            //    {
            //        Material firstConnectedMaterial = connectedElements[0].Material;
            //        Material secondConnectedMaterial = connectedElements[1].Material;

            //        if (this._relatedMaterial.Name != firstConnectedMaterial.Name && this._relatedMaterial.Name == secondConnectedMaterial.Name) this.AnomalyType = "Anomalie potentielle" ;
            //        if (this._relatedMaterial.Name == firstConnectedMaterial.Name && this._relatedMaterial.Name != secondConnectedMaterial.Name) this.AnomalyType = "Anomalie potentielle";
            //        if (this._relatedMaterial.Name != firstConnectedMaterial.Name && this._relatedMaterial.Name != secondConnectedMaterial.Name) this.AnomalyType = "Anomalie certaine";
            //    }

            //    if (connectedElements.Count == 1) 
            //    {
            //        Material firstConnectedMaterial = connectedElements[0].Material;
            //        if (this._relatedMaterial.Name != firstConnectedMaterial.Name) this.AnomalyType = "Anomalie potentielle";
            //    }

            //    if (connectedElements.Count == 0) this.AnomalyType = "Anomalie potentielle";

            //}




            if (_ContainedElements.Count == 1) 
            {
                List<IPipingElementBase> connectedElements = _ContainedElements[0].ReturnConnectedElements();

                if (connectedElements.Count == 0) this.AnomalyType = "Anomalie certaine";

                if (connectedElements.Count == 1)
                {
                    if (connectedElements[0].HasIncorrectMaterial) 
                    {
                        this.AnomalyType = "Anomalie certaine";
                    }

                    else 
                    {
                        this.AnomalyType = "Anomalie potentielle";

                    }
                }

                if (connectedElements.Count > 1) this.AnomalyType = "Anomalie certaine";
            }



            if (_ContainedElements.Count > 1) 
            {

                IPipingElementBase firstLimit = _Limits[0];
                IPipingElementBase secondLimit = _Limits[1];

                if (firstLimit.ReturnConnectedElements().Count == 1 && secondLimit.ReturnConnectedElements().Count == 1) this.AnomalyType = "Anomalie certaine";

                if (firstLimit.ReturnConnectedElements().Count > 1 && secondLimit.ReturnConnectedElements().Count > 1) this.AnomalyType = "Anomalie certaine";

                if (firstLimit.ReturnConnectedElements().Count > 1 && secondLimit.ReturnConnectedElements().Count == 1) this.AnomalyType = "Anomalie potentielle";

                if (firstLimit.ReturnConnectedElements().Count == 1 && secondLimit.ReturnConnectedElements().Count > 1) this.AnomalyType = "Anomalie potentielle";
            }
        }


        private void DefineLimits() 
        {
            if (_ContainedElements.Count == 1) return;

            foreach (IPipingElementBase pipingElement in _ContainedElements) 
            {
                List<IPipingElementBase> connectedElements = pipingElement.ReturnConnectedElements();

                if (connectedElements.Count <= 1) _Limits.Add(pipingElement);

                foreach (IPipingElementBase connectedElement in connectedElements) 
                {
                    //if (connectedElement.Material.Name != this._relatedMaterial.Name) _Limits.Add(pipingElement);
                    if (connectedElement.HasIncorrectMaterial == false) _Limits.Add(pipingElement);
                }
            }           
        }

        private void ExpandPipeline()
        {
            HashSet<long> processedIDs = new HashSet<long>(_ContainedElements.Select(e => e.ElementID));

            bool hasNewElements;

            do
            {
                hasNewElements = false;
                List<IPipingElementBase> tempList = new List<IPipingElementBase>();

                foreach (var element in _ContainedElements)
                {
                    foreach (var connectedElement in element.ReturnConnectedElements())
                    {
                        //if (connectedElement.Material.Name != _relatedMaterial.Name) continue;
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

        public bool Contains(IPipingElementBase pipingElement)
        {
            List<long> IDs = _ContainedElements.Select(pipingElem => pipingElem.ElementID).ToList();
            if (IDs.Contains(pipingElement.ElementID)) return true;
            return false;
        }
        public bool IsAPotentialAnomaly_() 
        {
            return false;
        }
        public bool IsACertainAnomaly_()
        {
            return false;
        }
    }
}
