using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

using RCR.SP.Framework.Helper.LogError;

namespace RCR.SP.Framework.Helper
{
    /// <summary>
    ///     Disable item events scope
    ///     REFERENCE: 
    ///         1. http://adrianhenke.wordpress.com/2010/01/29/disable-item-events-firing-during-item-update/ 
    ///         2. http://buyevich.blogspot.co.uk/2010/10/disableeventfiring-is-obsolete-in.html
    /// </summary>
    public class DisabledItemEventsScope : SPItemEventReceiver, IDisposable
    {
        private bool oldValue;

        public DisabledItemEventsScope()
        {
            this.oldValue = base.EventFiringEnabled;
            base.EventFiringEnabled = false;// base.DisableEventFiring();
        }

        #region IDisposable Members

        public void Dispose()
        {
            base.EventFiringEnabled = oldValue; //base.EnableEventFiring();
        }

        #endregion

    }
}
