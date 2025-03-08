using AnomalyChecker.Materials;

namespace AnomalyChecker.Services
{
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

        private MaterialSpecificationService() { } // Empêche la création directe

        public void LoadSpecification(PipelineMaterialSpecification materialSpecification)
        {
            this.Specification = materialSpecification;
        }
    }
}
