using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace AnomalyChecker
{
    public class Material
    {
        private List<string> _copperNames = new List<string>()
            {
                "CUIVRE",
                "Cuivre",
                "cuivre",
                "CUI",
                "Cui",
                "cui",
            };

        private List<string> _steelNames = new List<string>()
            {
                "ACIER",
                "Acier",
                "acier",
                "ACI",
                "Aci",
                "aci",
            };

        private List<string> _PVCNames = new List<string>()
            {
                "PVC",
                "Pvc",
                "pvc",
            };

        private List<string> _castIronNames = new List<string>()
            {
                "FONTE",
                "Fonte",
                "fonte",
                "FON",
                "Fon",
                "fon"
            };

        public string Name;

        public Material(MEPSystemType MEPSysType) 
        {

            string MEPSysName = MEPSysType.Name;

            if (MEPSysName.Contains("Eau chaude sanitaire")) this.Name = "Copper";
            if (MEPSysName.Contains("Eau Chaude Sanitaire")) this.Name = "Copper";


            else if (MEPSysName.Contains("Eau froide sanitaire")) this.Name = "Copper";
            else if (MEPSysName.Contains("Eau Froide Sanitaire")) this.Name = "Copper";

            else if(MEPSysName.Contains("Ventilation Primaire")) this.Name = "PVC";
            else if(MEPSysName.Contains("ventilation primaire")) this.Name = "PVC";
            else if(MEPSysName.Contains("VP")) this.Name = "PVC";

            else if(MEPSysName.Contains("EP")) this.Name = "Cast Iron";
            else if(MEPSysName.Contains("EAUX PLUVIALES")) this.Name = "Cast Iron";
            else if(MEPSysName.Contains("Eau Pluviale")) this.Name = "Cast Iron";
            else if(MEPSysName.Contains("eau pluviale")) this.Name = "Cast Iron";
            else if(MEPSysName.Contains("Eaux Pluviales")) this.Name = "Cast Iron";
            else if(MEPSysName.Contains("eaux pluviales")) this.Name = "Cast Iron";

            else { this.Name = "Non défini"; }
        }

        public Material(Autodesk.Revit.DB.Plumbing.PipeType pipeType) 
        {
            this.Name = ReturnMaterialName(pipeType);
        }

        public Material(FamilyInstance realtedElementFamInst) 
        {
            this.Name = ReturnMaterialName(realtedElementFamInst);
        }

        public Material(Pipe pipe)
        {
            this.Name = ReturnMaterialName(pipe.PipeType);
        }


        private string ReturnMaterialName(FamilyInstance FamInst) 
        {
            string FamilyName = FamInst.Symbol.Family.Name; ;
            string TypeName = FamInst.Symbol.Name;

            foreach (string copperName in _copperNames)
            {
                if (FamilyName.Contains(copperName) || TypeName.Contains(copperName)) return "Copper";
            }

            foreach (string steelName in _steelNames)
            {
                if (FamilyName.Contains(steelName) || TypeName.Contains(steelName)) return "Steel";
            }

            foreach (string PVCName in _PVCNames)
            {
                if (FamilyName.Contains(PVCName) || TypeName.Contains(PVCName)) return "PVC";
            }

            foreach (string castIronName in _castIronNames)
            {
                if (FamilyName.Contains(castIronName) || TypeName.Contains(castIronName)) return "Cast Iron";
            }

            return "Not Defined";
        }
        private string ReturnMaterialName(PipeType pipeType) 
        {
            string FamilyName = pipeType.Name; ;
            string TypeName = pipeType.FamilyName;

            foreach (string copperName in _copperNames)
            {
                if (FamilyName.Contains(copperName) || TypeName.Contains(copperName)) return "Copper";
            }

            foreach (string steelName in _steelNames)
            {
                if (FamilyName.Contains(steelName) || TypeName.Contains(steelName)) return "Steel";
            }

            foreach (string PVCName in _PVCNames)
            {
                if (FamilyName.Contains(PVCName) || TypeName.Contains(PVCName)) return "PVC";
            }

            foreach (string castIronName in _castIronNames)
            {
                if (FamilyName.Contains(castIronName) || TypeName.Contains(castIronName)) return "Cast Iron";
            }

            return "Not Defined";

        }
    }
}
