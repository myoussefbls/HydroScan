using AnomalyChecker.Materials;
using Microsoft.Win32;

namespace AnomalyChecker.Services
{
    //        // The Pipeline Material Specification defines rules linking pipe systems to their corresponding materials.  
    //        // It helps verify if the model's piping system uses the correct material.  
    //        // Since we need access to this specification throughout the program, we use the Singleton pattern.  
    //        // This ensures a centralized and consistent reference to the specification.  
    //        // The Singleton is implemented through a service: `MaterialSpecificationService`.  
    //        // - It provides a single instance of itself during the entire program execution.  
    //        // - This instance holds a single reference to the specification object.  
    //        // This way, the specification remains accessible and modifiable from anywhere in the program  
    //        // without needing to pass it through multiple constructors.  

    public class MaterialSpecificationService
    {
        private static MaterialSpecificationService _instance;
        public static MaterialSpecificationService Instance
        {
            get
            {
                if (_instance == null) _instance = new MaterialSpecificationService();
                return _instance;
            }
        }
        public PipelineMaterialSpecification Specification { get; private set; }
        private MaterialSpecificationService() { } // Prevents direct creation
        public void LoadSpecification(PipelineMaterialSpecification materialSpecification)
        {
            this.Specification = materialSpecification;
        }

        public void LoadSpecificationFromXMLFile() 
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Fichiers XML | *.xml";

            bool? selectionSuccess = fileDialog.ShowDialog();
            if (selectionSuccess == true) 
            {
                string xmlFilePath = fileDialog.FileName;
                this.Specification = new PipelineMaterialSpecification(xmlFilePath);
            }
        }
    }
}
