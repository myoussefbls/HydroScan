using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnomalyChecker.Materials;
using AnomalyChecker.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker.MEPElements
{
    public class PipeAccessory : IPipingElement
    {
        public string UserComment { get; set; }
        public long ElementID {get; set;}
        public bool HasIncorrectMaterial { get; set; }
        public string AnomalyType { get; set; }

        public bool IsAnomalyVerified { get; set; }
        public string Type { get; set; }
        public string mepSystemName { get; set; }

        private FamilyInstance _famInst;

        public long RelatedMEPElementID;

        public string mepSystemTypeName;

        public List<IPipingElement> _relatedPipes = new List<IPipingElement>();

        private string _relatedSystemMaterial;


        public PipeAccessory(FamilyInstance pipeAccessoryFamInst) 
        {
            this.Type = "Accessoire de canalisation";


            _famInst = pipeAccessoryFamInst;
            ElementID = pipeAccessoryFamInst.Id.Value;

            this.RelatedMEPElementID = pipeAccessoryFamInst.Id.Value;

            Parameter mepSystemTypeParam = pipeAccessoryFamInst.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            MEPSystemType mepSystemType = pipeAccessoryFamInst.Document.GetElement(mepSystemTypeParam.AsElementId()) as MEPSystemType;
            this.mepSystemTypeName = mepSystemType?.Name;

            Parameter mepSystemNameParam = pipeAccessoryFamInst.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            this.mepSystemName = (mepSystemNameParam != null && mepSystemNameParam.HasValue) ? mepSystemNameParam.AsString() : "Aucun nom de système défini";

            _relatedPipes = ReturnConnectedElements();
 
        }

        //private List<Pipe> ReturnRelatedPipes(FamilyInstance famIns)
        //{
        //    List<Pipe> relatedMEPElements = new List<Pipe>();
        //    ConnectorSet connectors = famIns.MEPModel.ConnectorManager.Connectors;

        //    foreach (Connector connector in connectors)
        //    {
        //        foreach (Connector connectedConnector in connector.AllRefs)
        //        {
        //            if (connectedConnector.Owner as Pipe == null) continue;

        //            relatedMEPElements.Add(connectedConnector.Owner as Pipe);
        //        }
        //    }

        //    return relatedMEPElements;
        //}

        public List<IPipingElement> ReturnConnectedElements()
        {
            List<IPipingElement> relatedMEPElements = new List<IPipingElement>();
            ConnectorSet connectors = _famInst.MEPModel.ConnectorManager.Connectors;

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    if (connectedConnector.Owner as Pipe == null) continue;

                    PipeWrapper pipeWrapper = new PipeWrapper(connectedConnector.Owner as Pipe);

                    relatedMEPElements.Add(pipeWrapper);
                }
            }

            PipelineMaterialSpecification materialSpecifications = MaterialSpecificationService.Instance.Specification;

            foreach (IPipingElement pipe in relatedMEPElements)
            {
                string systemName = pipe.mepSystemName;
                string systemDesignatedMaterial = materialSpecifications.ReturnRelatedMaterial(systemName);

                pipe.UpdateRelatedMaterial(systemDesignatedMaterial);
            }

            return relatedMEPElements;
        }

        public void UpdateRelatedMaterial(string materialName)
        {
            string familyName = _famInst.Symbol.Family.Name;
            string typeName = _famInst.Symbol.Name;
            _relatedSystemMaterial = materialName;
        }

        public void UpdateRelatedMaterial(PipelineMaterialSpecification spec)
        {
            string familyName = _famInst.Symbol.Family.Name;
            string typeName = _famInst.Symbol.Name;

            _relatedSystemMaterial = spec.ReturnRelatedMaterial(mepSystemName);

            if (_relatedSystemMaterial != null)
            {
                HasIncorrectMaterial = (familyName.Contains(_relatedSystemMaterial) || typeName.Contains(_relatedSystemMaterial)) ? false : true;
            }

            else { HasIncorrectMaterial = false; }
        }

        public void UpdateElementAnomalyParameters()
        {
            throw new NotImplementedException();
        }
    }
}
