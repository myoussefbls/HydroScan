using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker.MEPElements
{
    public class PipingSystemWrapper : INotifyPropertyChanged
    {
        private PipingSystem _mepSystem;
        private ElementSet _pipingNetwork;
        private MEPSystemType _relatedType;
        public string Name => _mepSystem.Name;

        public string SystemClassificationName;

        public List<IPipingElementBase> Elements;
        private List<PipeLineSegment> AnomalousPipeSegments;

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

        public event PropertyChangedEventHandler PropertyChanged;

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

            List<string> elementsAnomalies =  this.Elements.Select(c => c.AnomalyType).ToList();

            if (elementsAnomalies.Contains("Anomalie certaine")) AnomalyType = "Anomalie certaine";

            else if (elementsAnomalies.Contains("Anomalie potentielle")) AnomalyType = "Anomalie potentielle";

            else { AnomalyType = "RAS"; }


            var Z = this.Name;
            var A = this.AnomalyType;
            var B = true;


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
