using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using AnomalyChecker.Commands;
using AnomalyChecker.Materials;
using AnomalyChecker.MEPElements;
using AnomalyChecker.Services;
using AnomalyChecker.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Microsoft.Win32;

namespace AnomalyChecker
{
    public class ViewModel : INotifyPropertyChanged
    {

        public TransactionlessCommand SelectXMLFileCommand { get; private set; }

        public TransactionlessCommand LaunchMainWindowCommand { get; private set; }

        Autodesk.Revit.DB.Document _currentDocument;

        private List<PipeFitting> _pipeFittings = new List<PipeFitting>();
        private IList<Element> _pipesSegments;

        private PipingSystemType _selectedPipingSystemType;
        public PipingSystemType SelectedPipingSystemType 
        { 
            get {  return _selectedPipingSystemType; }
            set 
            {
                _selectedPipingSystemType = value;
                UpdateSelectedSystemNames(value);
                OnPropertyChanged();         
            }     
        }

        private List<MEPSystem> _selectedMEPsystems;

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

        private List<PipingSystemWrapper> _modelPipingSystems = new List<PipingSystemWrapper>();
        public List<PipingSystemWrapper> ModelSystems 
        {
            get { return _modelPipingSystems; }
            set 
            {
                _modelPipingSystems = value;
                OnPropertyChanged();
            }       
        }

        private PipelineMaterialSpecification _pipelineMaterialSpecification;

        private LaunchWindow _launchWindow;

        private MainWindow _mainWindow;

        private ConfigWindow _configWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateSelectedSystemNames(PipingSystemType selectedPipingSystemType) 
        {
            string selectedpipingSystemTypeName = selectedPipingSystemType.SystemClassification.ToString();

            List<PipingSystemWrapper> selectedSystems= new List<PipingSystemWrapper>();

            FilteredElementCollector documentMEPSystems = new FilteredElementCollector(_currentDocument).OfClass(typeof(MEPSystem));

            foreach (PipingSystemWrapper pipingSystem in ModelSystems)
            {
                string pipingSystemTypeName = pipingSystem.SystemClassificationName;
                if (pipingSystemTypeName == selectedpipingSystemTypeName ) selectedSystems.Add(pipingSystem);
            }

            this.PreSelectedSystemNames = selectedSystems;
        }
        private void UpdateSelectedSystemItems(PipingSystemWrapper selectedSystem) 
        {
            this.SelectedSystemElements = selectedSystem.Elements;
        }




        private void SelectXML(object parameter)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Fichiers XML | *.xml";

            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                string xmlFilePath = fileDialog.FileName;


                PipelineMaterialSpecification pipelineMaterialSpecification = new PipelineMaterialSpecification(xmlFilePath);

                MaterialSpecificationService.Instance.LoadSpecification(pipelineMaterialSpecification); //Using the singleton Pattern
            }

            AnalyzeModelSystems();

            this._launchWindow.Close();

            this._configWindow = new ConfigWindow() { DataContext = this };
            this._configWindow.Show();


        }

        private void LaunchMainWindow(object parameter) 
        {

            bool AnySystemMissingMateria = this.ModelSystems.Any(sys => sys.DesignatedMaterial == null);

            if (AnySystemMissingMateria)
            {
                MessageBox.Show("Certains materiaux ne sont pas renseignés", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            else 
            {

                PipelineMaterialSpecification materialSpecifications = MaterialSpecificationService.Instance.Specification;

                foreach (PipingSystemWrapper pipingSys in this.ModelSystems)
                {
                    materialSpecifications.UpdateSpecs(pipingSys.Name, pipingSys.DesignatedMaterial);
                }

                foreach (PipingSystemWrapper pipingSys in this.ModelSystems)
                {
                    pipingSys.UpdateElements();
                }

                materialSpecifications.UpdateXMLFile();

                if (materialSpecifications.HasSpecsBeenUpdated)
                {
                    MessageBox.Show("Le fichiers des spécifications a été mis a jour avec vos dernières modifications", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this._configWindow.Close();
                this._mainWindow = new MainWindow() { DataContext = this };
                this._mainWindow.Show();
            }
        }

        public ViewModel(ExternalCommandData comData) 
        {
            SelectXMLFileCommand = new TransactionlessCommand(SelectXML);
            LaunchMainWindowCommand = new TransactionlessCommand(LaunchMainWindow);

            this._currentDocument = comData.Application.ActiveUIDocument.Document;
            View activeView = _currentDocument.ActiveView;

            this._launchWindow = new LaunchWindow() { DataContext = this };
            this._launchWindow.Show();         
        }

        private void AnalyzeModelSystems() 
        {
            this.PipingSystemTypes = new FilteredElementCollector(_currentDocument).OfClass(typeof(PipingSystemType)).ToElements();
            List<MEPSystem> mepSystems = new FilteredElementCollector(_currentDocument).OfClass(typeof(MEPSystem)).Cast<MEPSystem>().ToList();

            foreach (MEPSystem system in mepSystems)
            {
                this.ModelSystems.Add(new PipingSystemWrapper(system as PipingSystem));
            }
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
