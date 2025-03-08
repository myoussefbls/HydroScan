using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyChecker
{
    public interface IPipingElementBase
    {
        Material Material { get; }
        long ElementID { get; }
        bool HasIncorrectMaterial { get;}

        string AnomalyType { get; set; }

        string Type { get; set; }

        string mepSystemName {  get; set; }

        List<IPipingElementBase> ReturnConnectedElements();

        void UpdateRelatedMaterial(string materialName);
    }
}
