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
    public class PipeWrapper : IPipingElementBase
    {
        public string AnomalyType { get; set; }

        public string Type { get; set; }
        public long ElementID { get; set; }

        private Pipe _relatedPipe;
        public Material Material { get; set; }


        private string _relatedSystemMaterial;

        MEPSystemType _MEPSystemType;

        public string mepSystemName { get; set; }



        public bool HasIncorrectMaterial { get; set; }

        public PipeWrapper(Pipe relatedPipe) 
        {
            this.Type = "Canalisation";

            Autodesk.Revit.DB.Document pipeDocument = relatedPipe.Document;

            _relatedPipe = relatedPipe;

            ElementID = relatedPipe.Id.Value;

            this._MEPSystemType = pipeDocument.GetElement(relatedPipe.MEPSystem.GetTypeId()) as MEPSystemType;

            PipeType pipeType = pipeDocument.GetElement(relatedPipe.GetTypeId()) as PipeType;

            this.Material = new Material(relatedPipe);

            //this.mepSystemName = (mepSystemNameParam != null && mepSystemNameParam.HasValue) ? mepSystemNameParam.AsString() : "Aucun nom de système défini";

            this.mepSystemName = relatedPipe.MEPSystem.Name;
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

        public List<IPipingElementBase> ReturnConnectedElements()
        {
            List<IPipingElementBase> relatedPipeFittings = new List<IPipingElementBase>();

            ConnectorSet connectors = _relatedPipe.ConnectorManager.Connectors;

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    if (connectedConnector.Owner as FamilyInstance == null) continue;
                    relatedPipeFittings.Add(new PipeFitting(connectedConnector.Owner as FamilyInstance));
                }
            }




            PipelineMaterialSpecification materialSpecifications = MaterialSpecificationService.Instance.Specification;


            foreach (IPipingElementBase pipeFitting in relatedPipeFittings) 
            {
                string systemName = pipeFitting.mepSystemName;

                string systemDesignatedMaterial = materialSpecifications.ReturnRelatedMaterial(systemName);

                pipeFitting.UpdateRelatedMaterial(systemDesignatedMaterial);
            }

            return relatedPipeFittings;
        }
    }
}
