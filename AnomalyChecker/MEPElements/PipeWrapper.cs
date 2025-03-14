using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AnomalyChecker.Materials;
using AnomalyChecker.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public class PipeWrapper : IPipingElement
    {
        public string AnomalyType { get; set; }
        public string Type { get; set; }
        public long ElementID { get; set; }

        private Pipe _relatedPipe;

        private string _relatedSystemMaterial;

        MEPSystemType _MEPSystemType;
        public string mepSystemName { get; set; }
        public bool HasIncorrectMaterial { get; set; }

        public PipeWrapper(Pipe relatedPipe) 
        {
            this.Type = "Canalisation";

            _relatedPipe = relatedPipe;
            ElementID = relatedPipe.Id.Value;

            this._MEPSystemType = relatedPipe.Document.GetElement(relatedPipe.MEPSystem.GetTypeId()) as MEPSystemType;
            this.mepSystemName = relatedPipe.MEPSystem.Name;

            PipeType pipeType = relatedPipe.Document.GetElement(relatedPipe.GetTypeId()) as PipeType;
        }

        public void UpdateRelatedMaterial(string materialName) 
        {
            _relatedSystemMaterial = materialName;
            HasIncorrectMaterial = this._relatedPipe.Name.Contains(_relatedSystemMaterial) ? false : true;
        }

        public Pipe ReturnRelatedPipe() 
        {   
            return _relatedPipe;     
        }

        public List<IPipingElement> ReturnConnectedElements()
        {
            var materialSpecifications = MaterialSpecificationService.Instance.Specification;
            var connectors = _relatedPipe.ConnectorManager.Connectors;
            var relatedPipeFittings = new List<IPipingElement>();

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    FamilyInstance pipeFittingFamInst = connectedConnector.Owner as FamilyInstance;
                    if (pipeFittingFamInst == null) continue;

                    var pipeFitting = new PipeFitting(pipeFittingFamInst);
                    pipeFitting.UpdateRelatedMaterial(materialSpecifications);
                    relatedPipeFittings.Add(pipeFitting);
                }
            }

            return relatedPipeFittings;
        }



    }
}
