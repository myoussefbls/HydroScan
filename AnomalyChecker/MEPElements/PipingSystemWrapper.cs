using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AnomalyChecker.Materials;
using AnomalyChecker.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker.MEPElements
{
    public class PipingSystemWrapper : INotifyPropertyChanged
    {
        private PipingSystem _mepSystem;
        public string Name => _mepSystem.Name;

        public string SystemClassificationName;

        public List<IPipingElement> Elements;
        private List<AnomalousPipeLineSegment> AnomalousPipeSegments;

        private string _anomalyType;
        public string AnomalyType
        {
            get => _anomalyType;
            set
            {
                if (_anomalyType != value)
                {
                    _anomalyType = value;
                    OnPropertyChanged(nameof(AnomalyType));
                }
            }
        }

        private string _designatedMaterial;
        public string DesignatedMaterial
        {
            get { return _designatedMaterial; }
            set
            {
                _designatedMaterial = value;
            }     
        }

        public PipingSystemType pipingSystemType;

        public event PropertyChangedEventHandler PropertyChanged;
        public PipingSystemWrapper(PipingSystem pipingSystem) 
        {
            this._mepSystem = pipingSystem;
            this.SystemClassificationName = (pipingSystem.Document.GetElement(pipingSystem.GetTypeId()) as PipingSystemType).SystemClassification.ToString();

            PipelineMaterialSpecification materialSpecifications = MaterialSpecificationService.Instance.Specification;
            this._designatedMaterial = materialSpecifications.ReturnRelatedMaterial(pipingSystem.Name);

            pipingSystemType = pipingSystem.Document.GetElement(pipingSystem.GetTypeId()) as PipingSystemType;
        }

        private List<IPipingElement> FindSystemElements()
        {
            List<IPipingElement> output = new List<IPipingElement>();

            List<Element> selectedSystemElements = new FilteredElementCollector(_mepSystem.Document)
            .WhereElementIsNotElementType()
            .Where(e => e is Pipe ||
                  (e is FamilyInstance famInst && famInst.Category.Id.Value == (int)BuiltInCategory.OST_PipeFitting)) // Filtre uniquement les raccords de canalisations
            .Where(e => e.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)?.AsString() == _mepSystem.Name) // Associer au système
            .ToList();


            foreach (Element element in selectedSystemElements)
            {
                IPipingElement pipingElement = null;

                if (element is Pipe) pipingElement = new PipeWrapper(element as Pipe);
                else if (element is FamilyInstance)
                {
                    if ((element as FamilyInstance).Category?.Id.Value != (int)BuiltInCategory.OST_PipeFitting) continue;
                    pipingElement = new PipeFitting(element as FamilyInstance);
                }

                pipingElement.UpdateRelatedMaterial(_designatedMaterial);
                output.Add(pipingElement);
            }

            return output;
        }
        public void AnalyzeAnomalies() 
        {
            this.Elements = FindSystemElements();
            this.AnomalousPipeSegments = FindAnomalousPipeSegments();

            foreach (var pipingElement in this.Elements)
            {
                pipingElement.AnomalyType = this.AnomalousPipeSegments.FirstOrDefault(segment => segment.Contains(pipingElement))?.AnomalyType;
            }

            List<string> elementsAnomalies = this.Elements.Select(element => element.AnomalyType).ToList();

            AnomalyType = elementsAnomalies.Contains("Anomalie certaine") ? "Anomalie certaine" :
                          elementsAnomalies.Contains("Anomalie potentielle") ? "Anomalie potentielle" :
                          "RAS";
        }
        private List<AnomalousPipeLineSegment> FindAnomalousPipeSegments()
        {
            List<AnomalousPipeLineSegment> anomalousPipeLineSegments = new List<AnomalousPipeLineSegment>();

            var incorrectElements = this.Elements.Where(obj => obj.HasIncorrectMaterial).ToList();

            foreach (IPipingElement pipingElement in incorrectElements)
            {
                bool IsElementProcessed = anomalousPipeLineSegments.Any(segment =>  segment.Contains(pipingElement));
                if (IsElementProcessed) { continue; }

                anomalousPipeLineSegments.Add(new AnomalousPipeLineSegment(pipingElement));
            }
            return anomalousPipeLineSegments;
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // when the CurrentDocument_LinkedDocuments property is set, this line raises the event "PropertyChanged". This event has been created once we applied the INotifyPropertyChanged interface to the Viewmodel class
            // The "?" checks wether the PropertyChanged event has any suscribers. Meaning : should any parts of the code be notified that the ViewModel's field changed.
            // The suscribers are all the parts of the View code which are bound to the ViewModel class' fields.
            // Invoke is the method that raises the event. It takes two arguments : 
            //   First : The object that is raising the event. In this case : The ViewModel itself.
            //   Second : The information about which property of the object has changed. The PropertyChangedEventArgs gives this information.
            // Once The PropertyChanged event has been raised, a notification is sent to its suscriber : The View. The View will then updates the data it shows, using the binding.
        }
    }
}
