using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AnomalyChecker.Materials;
using AnomalyChecker.MEPElements;
using AnomalyChecker.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public class PipeFitting : IPipingElement
    {
        public long ElementID { get; set;}

        public string UserComment { get; set; }

        public bool HasIncorrectMaterial { get; set; }
        public string AnomalyType { get; set; }

        private bool _isAnomalyVerified;
        public bool IsAnomalyVerified
        {
            get { return _isAnomalyVerified; }
            set { _isAnomalyVerified = value; }
        }
        public string Type { get; set; }
        public string Type_French { get; set; }
        public string mepSystemName { get; set; }

        public string RelatedSystemMaterial { get; set; }

        private FamilyInstance _famInst;

        public List<Pipe> _relatedPipes = new List<Pipe>();

        public string mepSystemTypeName;

        public long RelatedMEPElementID;



        public PipeFitting(FamilyInstance pipeFittingFamInst) 
        {
            this.Type = "PipeFitting";
            this.Type_French = "Raccord de canalisation";

            _famInst = pipeFittingFamInst;
            ElementID = pipeFittingFamInst.Id.Value;

            this.RelatedMEPElementID = pipeFittingFamInst.Id.Value;

            Parameter mepSystemTypeParam = pipeFittingFamInst.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            MEPSystemType mepSystemType = pipeFittingFamInst.Document.GetElement(mepSystemTypeParam.AsElementId()) as MEPSystemType;
            this.mepSystemTypeName = mepSystemType?.Name; 

            Parameter mepSystemNameParam = pipeFittingFamInst.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            this.mepSystemName = (mepSystemNameParam != null && mepSystemNameParam.HasValue) ? mepSystemNameParam.AsString() : "Aucun nom de système défini";

            _relatedPipes = ReturnRelatedPipes(_famInst);
        }


        public void UpdateRelatedMaterial(string materialName)
        {
            string familyName = _famInst.Symbol.Family.Name;
            string typeName = _famInst.Symbol.Name;

            RelatedSystemMaterial = materialName;
            HasIncorrectMaterial = (familyName.Contains(materialName) || typeName.Contains(materialName)) ? false : true;
        }

        public void UpdateRelatedMaterial(PipelineMaterialSpecification spec) 
        {
            string familyName = _famInst.Symbol.Family.Name;
            string typeName = _famInst.Symbol.Name;

            RelatedSystemMaterial = spec.ReturnRelatedMaterial(mepSystemName);

            if (RelatedSystemMaterial != null)
            {
                HasIncorrectMaterial = (familyName.Contains(RelatedSystemMaterial) || typeName.Contains(RelatedSystemMaterial)) ? false : true;
            }

            else { HasIncorrectMaterial = false; }
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

        List<IPipingElement> IPipingElement.ReturnConnectedElements()
        {
            List<IPipingElement> relatedMEPElements = new List<IPipingElement>();
            ConnectorSet connectors = _famInst.MEPModel.ConnectorManager.Connectors;

            foreach (Connector connector in connectors)
            {
                foreach (Connector connectedConnector in connector.AllRefs)
                {
                    if (connectedConnector.Owner as Pipe != null) 
                    {
                        PipeWrapper pipeWrapper = new PipeWrapper(connectedConnector.Owner as Pipe);
                        relatedMEPElements.Add(pipeWrapper);
                    }

                    if (connectedConnector.Owner as FamilyInstance != null && (connectedConnector.Owner as FamilyInstance).Category?.Id.Value == (int)BuiltInCategory.OST_PipeAccessory)
                    {
                        PipeAccessory pipeAccessory = new PipeAccessory(connectedConnector.Owner as FamilyInstance);
                        relatedMEPElements.Add(pipeAccessory);
                    }

                    if (connectedConnector.Owner as FamilyInstance != null && (connectedConnector.Owner as FamilyInstance).Category?.Id.Value == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        PipeFitting pipeFitting = new PipeFitting(connectedConnector.Owner as FamilyInstance);
                        relatedMEPElements.Add(pipeFitting);
                    }
                }            
            }

            PipelineMaterialSpecification materialSpecifications = MaterialSpecificationService.Instance.Specification;

            foreach (IPipingElement pipingElement in relatedMEPElements)
            {

                string systemName = pipingElement.mepSystemName;
                string systemDesignatedMaterial = materialSpecifications.ReturnRelatedMaterial(systemName);

                pipingElement.UpdateRelatedMaterial(systemDesignatedMaterial);
            }

            return relatedMEPElements;
        }


        public bool IsConnectedToAccessory()
        {
            var connectors = _famInst.MEPModel.ConnectorManager.Connectors;

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

        public void UpdateElementAnomalyParameters()
        {
            Parameter isCheckedParam = this._famInst.LookupParameter("BAL_HYDRSC_Statut");
            if (_isAnomalyVerified) isCheckedParam.Set(1);
            else {isCheckedParam.Set(0);}

            Parameter commentParam = this._famInst.LookupParameter("BAL_HYDRSC_Commentaire");
            commentParam.Set(UserComment);
        }
    }
}
