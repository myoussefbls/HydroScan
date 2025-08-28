using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnomalyChecker.UI;
using Autodesk.Revit.DB;

namespace AnomalyChecker.Services
{
    public class WindowService
    {
        private LaunchWindow _launchWindow;
        private ConfigWindow _configWindow;  


        private MainWindow _mainWindow;

        private ViewModel _viewModel;
        public WindowService(ViewModel viewModel) 
        {
            _viewModel = viewModel;
        }

        public void ShowLaunchWindow() 
        {

            var ff = _viewModel;
            var fddf = true;

            _launchWindow = new LaunchWindow() { DataContext = _viewModel };
            _launchWindow.Show();
        }
        public void ShowConfigWindow()
        {
            _launchWindow.Close();

            _configWindow = new ConfigWindow() { DataContext = _viewModel };
            _configWindow.Show();
        }
        public void ShowMainWindow()
        {
            _configWindow.Close();

            _mainWindow = new MainWindow() { DataContext = _viewModel };
            _mainWindow.Show();
        }
    }
}
