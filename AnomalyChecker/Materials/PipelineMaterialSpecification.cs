using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace AnomalyChecker.Materials
{
    public class PipelineMaterialSpecification
    {
        private XDocument _xmlDoc;
        private Dictionary<string, string> _specs;
        private string _xmlDocPath;
        public bool HasSpecsBeenUpdated;

        public PipelineMaterialSpecification(string xmlDocPath) 
        {
            HasSpecsBeenUpdated = false;
            _xmlDocPath = xmlDocPath;
            _xmlDoc = XDocument.Load(xmlDocPath);

            _specs = new Dictionary<string, string>();

            var fluids = _xmlDoc.Descendants("Fluide");

            foreach (var fluid in fluids)
            {
                string nom = fluid.Attribute("Systeme")?.Value;
                string materiau = fluid.Attribute("Materiau")?.Value;

                _specs[nom] = materiau;
            }
        }


        public string ReturnRelatedMaterial(string systemName) 
        {
            if (_specs.TryGetValue(systemName, out string materiau))
            {
                return materiau;
            }
            return null;
        }

        public void UpdateSpecs(string systemName, string systemMaterial) 
        {
            _specs[systemName] = systemMaterial;
        }

        public void UpdateXMLFile() 
        {
            XElement root = _xmlDoc.Element("Correspondances");


            foreach (KeyValuePair<string, string> entry in _specs)
            {
                string systemName = entry.Key;
                string materialName = entry.Value;

                bool XmlDocContainssystemName = _xmlDoc.Descendants("Fluide").Any(f => (string)f.Attribute("Systeme") == systemName);

                if (XmlDocContainssystemName) 
                {
                    var element = _xmlDoc.Descendants("Fluide").FirstOrDefault(f => (string)f.Attribute("Systeme") == systemName);

                    string initialMaterialName = (string)element.Attribute("Materiau");


                    if (element != null) element.SetAttributeValue("Materiau", materialName);



                    if (initialMaterialName != materialName) HasSpecsBeenUpdated = true;


                }


                else
                {
                    root.Add(new XElement("Fluide", new XAttribute("Systeme", systemName), new XAttribute("Materiau", materialName)));
                    HasSpecsBeenUpdated = true;
                }
            }

            _xmlDoc.Save(_xmlDocPath);
        }
    }
}
