using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

using RCR.SP.Framework.Helper.SharePoint;
using RCR.SP.Framework.Helper.LogError;

namespace RCR.SafeActObservation.Features.Safe_Act_Observation
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    /// <reference>
    ///     1. http://borderingdotnet.blogspot.com.au/2013/03/how-to-deploy-display-templates-via.html
    ///     2. http://blog.mastykarz.nl/automatically-publishing-files-provisioned-sandboxed-solutions/
    /// </reference>

    [Guid("9ff7217c-e65a-4941-a145-bcde0166c4b1")]
    public class Safe_Act_ObservationEventReceiver : SPFeatureReceiver
    {
        private const string APP_NAME = "RCR.SafeActObservation_ControlEventReceiver class";
        private const string LIST_SETTING = "SAO Settings";
        private string[] folderUrls = { "_catalogs/masterpage/Display Templates/Content Web Parts" };

        /// <summary>
        ///     Function to get folder url
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="folderUrl"></param>
        /// <returns></returns>
        private static  SPFolder GetFolderByUrl(SPListItemCollection folders, string folderUrl)
        {
            SPFolder folder = null;

            try
            {
                if (folders == null)
                {
                    throw new ArgumentNullException("folders");
                }


                if (String.IsNullOrEmpty(folderUrl))
                {
                    throw new ArgumentNullException("folderUrl");
                }


               
                SPListItem item = (from SPListItem i
                                   in folders
                                   where i.Url.Equals(folderUrl, StringComparison.InvariantCultureIgnoreCase)
                                   select i).FirstOrDefault();

                if (item != null)
                {
                    folder = item.Folder;
                }

            }
            catch (Exception err)
            {
                using (SPSite oSPSite = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb oSPWeb = oSPSite.OpenWeb())
                    {
                        LogErrorHelper logHelper = new LogErrorHelper(LIST_SETTING, oSPWeb);
                        logHelper.logSysErrorEmail(APP_NAME, err, "GetFolderByUrl Error!");
                        logHelper = null;
                    }
                }
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return folder;

        }

        /// <summary>
        ///     Function to publish all files inside folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="featureId"></param>
        private static void PublishFiles(SPFolder folder, string featureId)
        {
            try
            {
                if (folder == null)
                {
                    throw new ArgumentNullException("folder");
                }


                if (String.IsNullOrEmpty(featureId))
                {
                    throw new ArgumentNullException("featureId");
                }


                SPFileCollection files = folder.Files;
                var drafts = from SPFile f
                                      in files
                             where String.Equals(f.Properties["FeatureId"] as string, featureId, StringComparison.InvariantCultureIgnoreCase) &&
                             f.Level == SPFileLevel.Draft
                             select f;


                foreach (SPFile f in drafts)
                {
                    f.Publish("");
                    f.Update();
                }
            }
            catch (Exception err)
            {
                using (SPSite oSPSite = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb oSPWeb = oSPSite.OpenWeb())
                    {
                        LogErrorHelper logHelper = new LogErrorHelper(LIST_SETTING, oSPWeb);
                        logHelper.logSysErrorEmail(APP_NAME, err, "PublishFiles Error!");
                        logHelper = null;
                    }
                }
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

        }


        /// <summary>
        ///     Function to activate the SAO feature
        /// </summary>
        /// <param name="properties"></param>
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSite site = properties.Feature.Parent as SPSite;

                if (site != null)
                {
                    SPWeb rootWeb = site.RootWeb;
                    SPList gallery = site.GetCatalog(SPListTemplateType.MasterPageCatalog);


                    if (gallery != null)
                    {
                        SPListItemCollection folders = gallery.Folders;
                        string featureId = properties.Feature.Definition.Id.ToString();

                        foreach (string folderUrl in folderUrls)
                        {
                            SPFolder folder = GetFolderByUrl(folders, folderUrl);

                            if (folder != null)
                            {
                                PublishFiles(folder, featureId);

                            }

                        }

                    }

                }

            }
            catch (Exception err)
            {
                using (SPSite oSPSite = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb oSPWeb = oSPSite.OpenWeb())
                    {
                        LogErrorHelper logHelper = new LogErrorHelper(LIST_SETTING, oSPWeb);
                        logHelper.logSysErrorEmail(APP_NAME, err, "FeatureActivated Error!");
                        logHelper = null;
                    }
                }
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

        }



        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
