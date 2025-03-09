using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AnomalyChecker.Commands;
using AnomalyChecker.Materials;
using AnomalyChecker.MEPElements;
using AnomalyChecker.Services;
using AnomalyChecker.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace AnomalyChecker
{
    public class ViewModel : INotifyPropertyChanged
    {
        Autodesk.Revit.DB.Document _currentDocument;

        private RevitDataService _revitData;

        private PipingSystemType _selectedPipingSystemType;
        public PipingSystemType SelectedPipingSystemType 
        { 
            get {  return _selectedPipingSystemType; }
            set 
            {
                _selectedPipingSystemType = value;
                UpdateDisplayedSystemNames(value);
                OnPropertyChanged();         
            }     
        }

        private ICollection<Element> _pipingSystemTypes;
        public ICollection<Element> PipingSystemTypes 
        {
            get { return _pipingSystemTypes; }
            set {  _pipingSystemTypes = value; }  
        }

        private List<PipingSystemWrapper> _preSelectedSystemNames;
        public List<PipingSystemWrapper> PreSelectedSystemNames 
        {
            get { return _preSelectedSystemNames; }

            set 
            {
                _preSelectedSystemNames = value;
                OnPropertyChanged();
            }
        }

        private List<IPipingElementBase> _selectedSystemElements = new List<IPipingElementBase>();
        public List<IPipingElementBase> SelectedSystemElements 
        {
            get { return _selectedSystemElements; }
            set 
            { 
                _selectedSystemElements = value;
                OnPropertyChanged();
            }                           
        }

        private PipingSystemWrapper _selectedSystem;
        public PipingSystemWrapper SelectedSystem 
        {
            get { return _selectedSystem; }
            set 
            {
                _selectedSystem = value;
                UpdateSelectedSystemItems(value);
                OnPropertyChanged();
            }    
        }

        private List<PipingSystemWrapper> _pipingSystems = new List<PipingSystemWrapper>();
        public List<PipingSystemWrapper> PipingSystems 
        {
            get { return _pipingSystems; }
            set 
            {
                _pipingSystems = value;
                OnPropertyChanged();
            }       
        }

        private WindowService _windowService;

        private MaterialSpecificationService _specService;
        public TransactionlessCommand SelectXMLFileCommand { get; private set; }
        public TransactionlessCommand LaunchMainWindowCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateDisplayedSystemNames(PipingSystemType selectedPipingSystemType) 
        {
            string selectedPipingSystemTypeName = selectedPipingSystemType.SystemClassification.ToString();
            this.PreSelectedSystemNames = _pipingSystems.Where(system => system.SystemClassificationName == selectedPipingSystemTypeName).ToList();
        }

        private void UpdateSelectedSystemItems(PipingSystemWrapper selectedSystem) 
        {
            this.SelectedSystemElements = selectedSystem.Elements;
        }

        private void UpdatePipingSystems()
        {
            this.PipingSystems = _revitData.ReturnPipingSystems().Select(system => new PipingSystemWrapper(system)).ToList();
        }

        private void AnalyzePipingMaterials()
        {
            bool anySystemMissingMaterial = ArePipingSystemsMissingMaterials();

            if (!anySystemMissingMaterial)
            {
                UpdateSpecs();
                UpdatePipingSystemData();
            }

            ShowAnalysisResults(anySystemMissingMaterial);
        }

        private bool ArePipingSystemsMissingMaterials()
        {
            return _pipingSystems.Any(sys => sys.DesignatedMaterial == null);
        }

        private void UpdatePipingSystemData()
        {
            _pipingSystems.ForEach(pipingSys => pipingSys.UpdateElements());
        }

        private void ShowAnalysisResults(bool anySystemMissingMaterial)
        {
            if (anySystemMissingMaterial) UIMessage.SignalUnspecifiedMaterials();
            else { _windowService.ShowMainWindow(); }
        }

        private void UpdateSpecs() 
        {
            _specService.Specification.UpdateSpecs(_pipingSystems);
            _specService.Specification.UpdateXMLFile();

            if (_specService.Specification.HasBeenUpdated) UIMessage.SignalSpecificationUpdate();
        }

        public ViewModel(ExternalCommandData comData) 
        {
            this._currentDocument = comData.Application.ActiveUIDocument.Document;
            this._revitData = new RevitDataService(_currentDocument);
            this.PipingSystemTypes = _revitData.ReturnPipingSystemsTypes();

            this._specService = MaterialSpecificationService.Instance;
            this._windowService = new WindowService(this);

            SelectXMLFileCommand = new TransactionlessCommand(_specService.LoadSpecificationFromXMLFile, UpdatePipingSystems, _windowService.ShowConfigWindow);
            LaunchMainWindowCommand = new TransactionlessCommand(AnalyzePipingMaterials);

            this._windowService.ShowLaunchWindow();
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
