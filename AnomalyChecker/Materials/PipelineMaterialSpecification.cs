using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using AnomalyChecker.MEPElements;

namespace AnomalyChecker.Materials
{
    public class PipelineMaterialSpecification
    {
        private XDocument _xmlDoc;
        private Dictionary<string, string> _specs;
        private string _xmlDocPath;
        public bool HasBeenUpdated = false;

        public PipelineMaterialSpecification(string xmlDocPath) 
        {
            _xmlDocPath = xmlDocPath;
            _xmlDoc = XDocument.Load(xmlDocPath);
            _specs = new Dictionary<string, string>();

            IEnumerable<XElement> fluids = _xmlDoc.Descendants("Fluide");

            foreach (XElement fluid in fluids)
            {
                string systemName = fluid.Attribute("Systeme")?.Value;
                string systemMaterial = fluid.Attribute("Materiau")?.Value;
                _specs[systemName] = systemMaterial;
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

        public void UpdateSpecs(List<PipingSystemWrapper> pipingSystems)
        {
            foreach (PipingSystemWrapper pipingSys in pipingSystems)
            {
                _specs[pipingSys.Name] = pipingSys.DesignatedMaterial;
            }       
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

                    if (initialMaterialName != materialName) HasBeenUpdated = true;
                }

                else
                {
                    root.Add(new XElement("Fluide", new XAttribute("Systeme", systemName), new XAttribute("Materiau", materialName)));
                    HasBeenUpdated = true;
                }
            }
            _xmlDoc.Save(_xmlDocPath);
        }
    }
}
