using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AnomalyChecker.Materials;
using AnomalyChecker.MEPElements;
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
            this.Type = "Pipe";

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
            var connectedElements = new List<IPipingElement>();

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    FamilyInstance connectedElementFamInst = connectedConnector.Owner as FamilyInstance;
                    if (connectedElementFamInst == null) continue;

                    if (connectedElementFamInst.Category.Id.Value == (int)BuiltInCategory.OST_PipeAccessory) 
                    {
                        var pipeAccessory = new PipeAccessory(connectedElementFamInst);
                        pipeAccessory.UpdateRelatedMaterial(materialSpecifications);
                        connectedElements.Add(pipeAccessory);
                    }

                    if (connectedElementFamInst.Category.Id.Value == (int)BuiltInCategory.OST_PipeFitting) 
                    {
                        var pipeFitting = new PipeFitting(connectedElementFamInst);
                        pipeFitting.UpdateRelatedMaterial(materialSpecifications);
                        connectedElements.Add(pipeFitting);
                    }
                }
            }

            return connectedElements;
        }

        public bool IsConnectedToAccessory() 
        {
            var connectors = _relatedPipe.ConnectorManager.Connectors;

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    FamilyInstance connectedElement = connectedConnector.Owner as FamilyInstance;

                    if (connectedElement == null) continue;

                    if (connectedElement.Category.Id.Value == (int)BuiltInCategory.OST_PipeAccessory) return true;
                }
            }

            return false;
        }


    }
}
