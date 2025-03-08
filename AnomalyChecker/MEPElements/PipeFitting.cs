using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnomalyChecker.Materials;
using AnomalyChecker.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public class PipeFitting : IPipingElementBase
    {

        //long IPipingElementBase.ElementID => throw new NotImplementedException();

        public long ElementID { get; set;}




        private Material _material;
        private string _relatedSystemMaterial;

        private FamilyInstance _famInst;

        public List<Pipe> _relatedPipes = new List<Pipe>();


        public string AnomalyType { get; set; }

        public string Type { get; set; }



        public bool IsAPotentialAnomaly = false;
        public bool IsACertainAnomaly = false;

        public string mepSystemTypeName;
        public string mepSystemName { get; set; }

        public long RelatedMEPElementID;

        public Material Material { get; set; }

        public bool HasIncorrectMaterial { get; set; }





        public PipeFitting(FamilyInstance pipeFittingFamInst) 
        {
            this.Type = "Raccord de canalisation";
            ElementID = pipeFittingFamInst.Id.Value;


            _famInst = pipeFittingFamInst;
            this.RelatedMEPElementID = pipeFittingFamInst.Id.Value;

            Parameter mepSystemTypeParam = pipeFittingFamInst.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            MEPSystemType mepSystemType = pipeFittingFamInst.Document.GetElement(mepSystemTypeParam.AsElementId()) as MEPSystemType;
            this.mepSystemTypeName = mepSystemType?.Name; 

            Parameter mepSystemNameParam = pipeFittingFamInst.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            this.mepSystemName = (mepSystemNameParam != null && mepSystemNameParam.HasValue) ? mepSystemNameParam.AsString() : "Aucun nom de système défini";


            _material = new Material(pipeFittingFamInst);
            Material = new Material(pipeFittingFamInst);
            _relatedPipes = ReturnRelatedPipes(_famInst);

            //_relatedSystemMaterial = new Material(mepSystemType);

            //HasIncorrectMaterial = _material.Name != _relatedSystemMaterial.Name;
        }


        public void UpdateRelatedMaterial(string materialName)
        {
            string familyName = _famInst.Symbol.Family.Name;
            string typeName = _famInst.Symbol.Name;

            _relatedSystemMaterial = materialName;

            HasIncorrectMaterial = (familyName.Contains(materialName) || typeName.Contains(materialName)) ? false : true;
        }


        private List<Pipe> ReturnRelatedPipes(FamilyInstance famIns)
        {
            List<Pipe> relatedMEPElements = new List<Pipe>();
            ConnectorSet connectors = famIns.MEPModel.ConnectorManager.Connectors;

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    if (connectedConnector.Owner as Pipe == null) continue;

                    relatedMEPElements.Add(connectedConnector.Owner as Pipe);
                }
            }

            return relatedMEPElements;
        }

        public Material ReturnMaterial() 
        {
            return _material;
        }

        List<IPipingElementBase> IPipingElementBase.ReturnConnectedElements()
        {
            List<IPipingElementBase> relatedMEPElements = new List<IPipingElementBase>();
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


            foreach (IPipingElementBase pipe in relatedMEPElements)
            {
                string systemName = pipe.mepSystemName;
                var F = true;

                string systemDesignatedMaterial = materialSpecifications.ReturnRelatedMaterial(systemName);

                pipe.UpdateRelatedMaterial(systemDesignatedMaterial);
            }


            return relatedMEPElements;
        }
    }
}
