using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClashMarkerAddIn.External_Events_Handlers
{
    public abstract class ExternalEventHandler : IExternalEventHandler
    {

        private string _identifier;

        private readonly ExternalEvent _externalEvent;
        // the readonly keyword means that this fields could be assigned a value only in the constructor
        // its value will no be modifiable later on


        public ExternalEventHandler()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        public abstract void Execute(UIApplication app);

        public string GetName()
        {
            return GetType().Name;
        }


        public void Raise()
        {
            _externalEvent.Raise();
        }

    }
}
