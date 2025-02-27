using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker.MEPElements
{
    public class PipingSystemWrapper
    {

        private PipingSystem _mepSystem;
        private ElementSet _pipingNetwork;
        private MEPSystemType _relatedType;

        public string Name => _mepSystem.Name;

        public string SystemClassificationName;




        public List<IPipingElementBase> Elements;
        private List<PipeLineSegment> AnomalousPipeSegments;

        public PipingSystemWrapper(PipingSystem pipingSystem) 
        {



            this._mepSystem = pipingSystem;
            this._pipingNetwork = pipingSystem.PipingNetwork;
            this._relatedType = _mepSystem.Document.GetElement(_mepSystem.GetTypeId()) as MEPSystemType;

            this.SystemClassificationName = (pipingSystem.Document.GetElement(pipingSystem.GetTypeId()) as PipingSystemType).SystemClassification.ToString();

            PipingSystemType pipingSystemType = pipingSystem.Document.GetElement(pipingSystem.GetTypeId()) as PipingSystemType;

            this.Elements = FindSystemElements();
            this.AnomalousPipeSegments = FindAnomalousPipeSegments();

            foreach (IPipingElementBase pipingElement in this.Elements)
            {
                foreach (PipeLineSegment anomalousPipeSegment in AnomalousPipeSegments) 
                {
                    if (anomalousPipeSegment.Contains(pipingElement)) 
                    {
                       pipingElement.AnomalyType = anomalousPipeSegment.AnomalyType;
                    }
                }
            }
        }


        private List<IPipingElementBase> FindSystemElements() 
        {

            List<IPipingElementBase> output = new List<IPipingElementBase>();

            List<Element> selectedSystemElements = new FilteredElementCollector(_mepSystem.Document)
            .WhereElementIsNotElementType().Where(e => e is Pipe || e is FamilyInstance) // Filtre les canalisations et raccords
            .Where(e => e.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)?.AsString() == _mepSystem.Name) // Associer au système
            .ToList();

            foreach (Element element in selectedSystemElements)
            {

                if (element is Pipe) output.Add(new PipeWrapper(element as Pipe));
                if (element is FamilyInstance) output.Add(new PipeFitting(element as FamilyInstance));
            }

            return output;
        }

        //private List<IPipingElementBase> FindIncorrectElements()
        //{

        //    List<IPipingElementBase> IncorrectPipingElements = new List<IPipingElementBase>();

        //    foreach (Element elem in _pipingNetwork)
        //    {
        //        if (elem is FamilyInstance)
        //        {
        //            var pipeFitting = new PipeFitting(elem as FamilyInstance);

        //            if (pipeFitting.IsRelatedMaterialDifferentFromRelatedSystemMaterial())
        //            {
        //                IncorrectPipingElements.Add(pipeFitting);
        //            }
        //        }

        //        if (elem is Pipe)
        //        {
        //            PipeWrapper pipeWrapper = new PipeWrapper(elem as Pipe);

        //            Material PipeMaterial = new Material(elem as Pipe);

        //            MEPSystem mepSystem = pipeWrapper.ReturnRelatedPipe().MEPSystem;

        //            Material PipeSystemMaterial = new Material(_relatedType);

        //            if (PipeMaterial.Name != PipeSystemMaterial.Name)
        //            {
        //                IncorrectPipingElements.Add(pipeWrapper);
        //            }
        //        }
        //    }

        //    return IncorrectPipingElements;


        //}

        private List<PipeLineSegment> FindAnomalousPipeSegments()
        {

            var incorrectElements = this.Elements.Where(obj => obj.HasIncorrectMaterial).ToList();

            List<PipeLineSegment> PipeLineSections = new List<PipeLineSegment>();

            foreach (IPipingElementBase pipingElement in incorrectElements)
            {
                bool IsElementProcessed = false;

                foreach (PipeLineSegment pipeLineSection in PipeLineSections)
                {
                    if (pipeLineSection.Contains(pipingElement))
                    {
                        IsElementProcessed = true;
                    }
                }

                if (IsElementProcessed) { continue; }
                PipeLineSections.Add(new PipeLineSegment(pipingElement));
            }

            return PipeLineSections;

        }




    }
}
