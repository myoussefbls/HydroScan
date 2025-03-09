using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnomalyChecker.UI
{
    public class UIMessage
    {
        public static void SignalUnspecifiedMaterials() 
        {
            MessageBox.Show("Certains matériaux ne sont pas renseignés", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void SignalSpecificationUpdate()
        {
            MessageBox.Show("Le fichiers des spécifications a été mis a jour avec vos dernières modifications", "Attention", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}
