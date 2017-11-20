using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security;
using System.Threading;
using System.Xml;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

using RCR.SP.Framework.Helper.LogError;

namespace RCR.SP.Framework.Helper.SharePoint
{
    public class SharePointHelper
    {
        #region variables

        private const string APP_NAME = "SharePointHelper Class";
        private string _spSettingList;
        private string _categorySetting;
        private SPWeb _spCurrentWeb;
        private SPWebTemplate _spWebTemplate;

        #endregion

        #region constructor


        /// <summary>
        ///     Initialise class instance
        /// </summary>
        //public SharePointHelper() { }

        /// <summary>
        ///     Initialise class instance
        /// </summary>
        public SharePointHelper(string spSettingList, string categorySetting, SPWeb spSite)
        {
            this._spSettingList = spSettingList;
            this._categorySetting = categorySetting;
            this._spCurrentWeb = spSite;
        }

        #endregion

        #region Members

        public string CategorySettings
        {
            get { return this._categorySetting; }
            set { this._categorySetting = value; }
        }

        public string SPListSettingName
        {
            get { return this._spSettingList; }
            set { this._spSettingList = value; }
        }

        public SPWeb SPCurrentWeb
        {
            get { return this._spCurrentWeb; }
            set { this._spCurrentWeb = value; }
        }

        public SPWebTemplate SPSiteTemplate
        {
            get { return this._spWebTemplate; }
            set { this._spWebTemplate = value; }
        }

        #endregion

        #region InfoPath methods

        /// <summary>
        ///     Get Coma delimitated value set form InfoPath form item
        ///     REFERENCE: http://sujeewaediriweera.wordpress.com/2012/02/12/accessing-infopath-form-data-programmatically/
        /// </summary>
        /// <param name="url">Site URL for the InfoPath form library</param>
        /// <param name="InfoPathFormLibraryName">InfoPath form library Name</param>
        /// <param name="itemID">InfoPath form library item</param>
        /// <returns>Coma delimitated value set</returns>
        public string ReadFromInfoPathForm(string url, string InfoPathFormLibraryName, int itemID, string fieldName)
        {
            string output = string.Empty;

            try
            {
                //Open site which contains InfoPath form library 
                using (SPSite site = new SPSite(url))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        //Open InfoPath form library 
                        SPList list = web.Lists[InfoPathFormLibraryName];
                        //Get Correspondent list item 
                        SPListItem item = list.GetItemById(itemID);
                        //Read InfoPath from
                        SPFile infoPathFrom = item.File;
                        //Get xml Transformation 
                        XmlTextReader infoPathform = new XmlTextReader(infoPathFrom.OpenBinaryStream());
                        infoPathform.WhitespaceHandling = WhitespaceHandling.None;
                        string nodeKey = string.Empty;

                        //Read each node in InfoPath
                        while (infoPathform.Read())
                        {
                            if (!string.IsNullOrEmpty(infoPathform.Value))
                            {                               
                                if (nodeKey.ToLower() == fieldName.ToLower())
                                {
                                    output = infoPathform.Value;
                                }
                            }
                            nodeKey = infoPathform.Name;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                //LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                //objErr.logSysErrorEmail(APP_NAME, err, "Error at ReadFromInfoPathForm function");
                //objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return output;
        }


        /// <summary>
        ///         Function to get multiple user login name from InfoPath XML file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="InfoPathFormLibraryName"></param>
        /// <param name="itemID"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string ReadMultiUserFromInfoPathForm(string url, string InfoPathFormLibraryName, int itemID, string fieldName, string xPath, string searchNodeKey)
        {
            string output = string.Empty;
            //string pcUser = "pc:Person;pc:DisplayName;pc:AccountId";
            string[] arrUser = xPath.Split('/');
            //string searchNodeKey = "pc:AccountId";

            try
            {
                //Open site which contains InfoPath form library 
                using (SPSite site = new SPSite(url))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        //Open InfoPath form library 
                        SPList list = web.Lists[InfoPathFormLibraryName];
                        //Get Correspondent list item 
                        SPListItem item = list.GetItemById(itemID);
                        //Read InfoPath from
                        SPFile infoPathFrom = item.File;
                        //Get xml Transformation 
                        XmlTextReader infoPathform = new XmlTextReader(infoPathFrom.OpenBinaryStream());
                        infoPathform.WhitespaceHandling = WhitespaceHandling.None;
                        string nodeKey = string.Empty;

                        //Read each node in InfoPath
                        int i = 0; //Initialise index
                        while (infoPathform.Read())
                        {
                            if (fieldName.ToLower() == nodeKey.ToLower())
                            {
                                if (!string.IsNullOrEmpty(infoPathform.Value))
                                {//if there is a value in the node key
                                    if (nodeKey.ToLower() == searchNodeKey.ToLower())
                                    {
                                        output = output + infoPathform.Value + ";";
                                    }
                                    else
                                    {
                                        if (arrUser[i] != null)
                                            fieldName = arrUser[i].ToString(); //Read next node key if there is no value
                                    }
                                }
                                else
                                {
                                    if (arrUser[i] != null)
                                        fieldName = arrUser[i].ToString(); //Read next node key if there is no value
                                }

                                i = i + 1; //Increment index counter

                                if (i >= arrUser.Count())
                                    i = 0;
                            }

                            nodeKey = infoPathform.Name; //Read next node

                        }
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("ERROR: " + err.Message.ToString());
                Console.ReadKey();
            }

            return output;
        }

        /// <summary>
        ///         Function to get user login name from InfoPath XML file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="InfoPathFormLibraryName"></param>
        /// <param name="itemID"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string ReadUserFromInfoPathForm(string url, string InfoPathFormLibraryName, int itemID, string fieldName, string xPath, string searchNodeKey)
        {
            string output = string.Empty;
            //string pcUser = "pc:Person;pc:DisplayName;pc:AccountId";
            string[] arrUser = xPath.Split('/');
            //string searchNodeKey = "pc:AccountId";

            try
            {
                //Open site which contains InfoPath form library 
                using (SPSite site = new SPSite(url))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        //Open InfoPath form library 
                        SPList list = web.Lists[InfoPathFormLibraryName];
                        //Get Correspondent list item 
                        SPListItem item = list.GetItemById(itemID);
                        //Read InfoPath from
                        SPFile infoPathFrom = item.File;
                        //Get xml Transformation 
                        XmlTextReader infoPathform = new XmlTextReader(infoPathFrom.OpenBinaryStream());
                        infoPathform.WhitespaceHandling = WhitespaceHandling.None;
                        string nodeKey = string.Empty;

                        //Read each node in InfoPath
                        int i = 0; //Initialise index
                        while (infoPathform.Read())
                        {
                            if (fieldName.ToLower() == nodeKey.ToLower())
                            {
                                if (!string.IsNullOrEmpty(infoPathform.Value))
                                {//if there is a value in the node key
                                    if (nodeKey.ToLower() == searchNodeKey.ToLower())
                                    {
                                        output = infoPathform.Value;
                                    }
                                    else
                                    {
                                        if (arrUser[i] != null)
                                            fieldName = arrUser[i].ToString(); //Read next node key if there is no value
                                    }
                                }
                                else
                                {
                                    if (arrUser[i] != null)
                                        fieldName = arrUser[i].ToString(); //Read next node key if there is no value
                                }

                                i = i + 1; //Increment index counter
                            }

                            nodeKey = infoPathform.Name; //Read next node

                        }
                    }
                }
            }
            catch (Exception err)
            {
                //LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                //objErr.logSysErrorEmail(APP_NAME, err, "Error at ReadUserFromInfoPathForm function");
                //objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return output;
        }

        #endregion

        #region Add methods

        /// <summary>
        ///     Function to add new approver into the custom approver list
        /// </summary>
        /// <param name="spURL"></param>
        /// <param name="userName"></param>
        /// <param name="userJobTitle"></param>
        /// <param name="listName"></param>
        /// <returns></returns>
        public bool addApprover(string spURL, SPFieldUserValue userName, string userJobTitle, string listName)
        {
            bool isApproverAdded = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite destSite = new SPSite(spURL))
                    {
                        using (SPWeb destWeb = destSite.OpenWeb())
                        {
                            destWeb.AllowUnsafeUpdates = true;
                            SPList list = destWeb.Lists[listName];
                            SPListItem listItem = list.Items.Add();
                            listItem["Title"] = userJobTitle;
                            listItem["Approvers"] = userName;

                            listItem.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes

                            destWeb.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes
                            destWeb.AllowUnsafeUpdates = false;

                            isApproverAdded = true;
                        }

                    }

                });

              
            }
            catch (Exception ex)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, ex, "Error at addApprover function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message.ToString(), ex.StackTrace);            
            }


            return isApproverAdded;
        }

        /// <summary>
        ///     Method to insert a list item into a specific SharePoint list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="colName"></param>
        /// <param name="itemValue"></param>
        public void AddListItem(string spSiteUrl, string listName, string colName, string itemValue)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            web.AllowUnsafeUpdates = true;

                            SPList list = web.Lists[listName];
                            SPListItem listItem = list.Items.Add();
                            listItem[colName] = itemValue;
                            listItem.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes

                            web.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes
                            web.AllowUnsafeUpdates = false;

                        }
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at AddListItem function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }
        }

         /// <summary>
        ///     Function to update an existing workflow item into a SharePoint task list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="errTitle"></param>
        /// <param name="errMsg"></param>
        public void UpdateWorkflowListItem(string spSiteUrl, string listName, string Title, string Msg, string docFileName, int itemID, string ApprovalStatus, string TaskStatus)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
               {
                   using (SPSite site = new SPSite(spSiteUrl))
                   {
                       using (SPWeb web = site.OpenWeb())
                       {
                           StringBuilder sb = new StringBuilder();
                           SPFieldUserValue loginUser = GetCurrentUser(web);

                           web.AllowUnsafeUpdates = true;
                           // Fetch the List
                           SPList list = web.Lists[listName];
                           // Update the List item by ID
                           SPListItem itemToUpdate = list.GetItemById(itemID);

                           itemToUpdate["Title"] = Title;
                           
                           if (ApprovalStatus.ToLower() == "pending")
                           {
                               sb.AppendFormat(Msg + "<p><b>Document File:</b> {0}</p>", docFileName);
                              
                           } else {
                                sb.AppendFormat(itemToUpdate["Body"] + "<p>{0}</p>", Msg);
                           }

                           itemToUpdate["Body"] = sb.ToString();
                           itemToUpdate["BusinessApproval"] = ApprovalStatus;
                           itemToUpdate["TaskStatus"] = TaskStatus;

                           if (TaskStatus.ToLower() == "completed")
                           {
                               itemToUpdate["PercentComplete"] = 1;
                           }
                           else
                           {
                               itemToUpdate["PercentComplete"] = 0;
                           }

                           if (loginUser != null)
                               itemToUpdate["Modified By"] = loginUser;

                           itemToUpdate.Update();
                           Thread.Sleep(1000); //Give SharePoint some time to update the changes

                           web.Update();
                           Thread.Sleep(1000); //Give SharePoint some time to update the changes
                           web.AllowUnsafeUpdates = false;
                       }

                   }
               });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at UpdateWorkflowListItem function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }

        /// <summary>
        ///     Function to insert a new workflow item into a SharePoint task list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="errTitle"></param>
        /// <param name="errMsg"></param>
        public void AddWorkflowListItem(string spSiteUrl, string listName, string Title, string Msg, string docFileName, string itemID, int numDays, string ApprovalStatus, string TaskStatus, SPFieldUserValue assignedPerson)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                           
                            SPFieldUserValue loginUser = GetCurrentUser(web);
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat(Msg + "<p><b>Document File:</b> {0}</p>", docFileName);

                            web.AllowUnsafeUpdates = true;
                            SPList list = web.Lists[listName];
                            SPListItem listItem = list.Items.Add();
                            listItem["DocID"] = itemID;
                            listItem["Title"] = Title;
                            listItem["Body"] = sb.ToString();
                            listItem["StartDate"] = DateTime.Now;
                            listItem["DueDate"] = DateTime.Now.AddDays(numDays);
                            listItem["BusinessApproval"] = ApprovalStatus;
                            listItem["TaskStatus"] = TaskStatus;

                            if (TaskStatus.ToLower() == "completed")
                                listItem["PercentComplete"] = 100;

                            if (loginUser != null)
                                listItem["Modified By"] = loginUser;

                            if (assignedPerson != null)
                                listItem["AssignedTo"] = assignedPerson;

                            listItem.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes

                            web.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes
                            web.AllowUnsafeUpdates = false;

                        }
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at AddWorkflowListItem function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }

        /// <summary>
        ///     Function to insert a log item into a SharePoint log list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="errTitle"></param>
        /// <param name="errMsg"></param>
        public void AddLogListItem(string spSiteUrl, string errTitle, string errMsg, string docFileName, string pdfFileName, string itemID, bool convertStatus)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            string listName = GetRCRSettingsItem(_categorySetting, _spSettingList);
                            SPFieldUserValue loginUser = GetCurrentUser(web);

                            web.AllowUnsafeUpdates = true;
                            SPList list = web.Lists[listName];
                            SPListItem listItem = list.Items.Add();
                            listItem["Title"] = errTitle;
                            listItem["DescriptionLog"] = errMsg;
                            listItem["DocFileName"] = docFileName;
                            listItem["PdfFileName"] = pdfFileName;
                            listItem["DocID"] = itemID;
                            listItem["ConvertDate"] = DateTime.Now;

                            if (convertStatus == true)
                            {
                                listItem["ConvertStatus"] = "Success";
                            }
                            else
                            {
                                listItem["ConvertStatus"] = "Failed";
                            }

                            if (loginUser != null)
                                listItem["Modified By"] = loginUser;

                            listItem.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes

                            web.Update();
                            Thread.Sleep(1000); //Give SharePoint some time to update the changes
                            web.AllowUnsafeUpdates = false;

                        }
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at AddLogListItem function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }

        /// <summary>
        ///     Function to create a new document library and set the document template
        ///     REFERENCE:  http://www.c-sharpcorner.com/UploadFile/40e97e/sharepoint-2010-create-document-library/
        /// </summary>
        /// <param name="spSiteURL"></param>
        /// <param name="libTemplateName">The availiable list of template names are:
        ///     1. Document Library
        ///     2. Form Library
        ///     3. Wiki Page Library
        ///     4. Picture Library
        ///     5. Announcements
        ///     6. 
        /// </param>
        /// <param name="newDocLibTitle"></param>
        /// <param name="newDocLibDesc"></param>
        /// <param name="docTemplateId">The availiable list of document template Id are:
        ///     Template ID - Description
        ///     100         - No Template
        ///     101         - Word 2003 document
        ///     103         - Excel 2003 document
        ///     104         - PowerPoint 2003 document
        ///     121         - Word document
        ///     122         - Excel document
        ///     123         - PowerPoint document
        ///     111         - OneNote Notebook
        ///     102         - SharePoint Designer HTML document
        ///     105         - ASPX Web Page
        ///     106         - ASPX Web Part Page
        ///     1000        - InfoPath document
        /// </param>
        /// <returns></returns>
        public bool CreateNewDocumentTemplate(string spSiteURL, string libTemplateName, string newDocLibTitle, string newDocLibDesc, int docTemplateId)
        {
            bool isNewDocLibraryCreated = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteURL))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPListTemplate listTemplate = web.ListTemplates[libTemplateName]; //e.g. ["Document Library"];
                            SPDocTemplate docTemplate = (from SPDocTemplate dt in web.DocTemplates
                                                         where dt.Type == docTemplateId
                                                         select dt).FirstOrDefault();

                            web.AllowUnsafeUpdates = true;
                            Guid docLibGuid = web.Lists.Add(newDocLibTitle, newDocLibDesc, listTemplate, docTemplate);

                            //Add short-cut link onto left side menu
                            SPDocumentLibrary library = web.Lists[docLibGuid] as SPDocumentLibrary;
                            library.OnQuickLaunch = true;
                            library.Update();
                            web.Update();

                            web.AllowUnsafeUpdates = false;
                            isNewDocLibraryCreated = true;
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewDocumentTemplate function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isNewDocLibraryCreated;
        }

        /// <summary>
        ///  Creates a new document library in SharePoint
        ///  REFERENCE:
        ///         1. http://msdn.microsoft.com/en-us/library/ms425818.aspx
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="spSiteURL"></param>
        /// <returns>The GUID of the new created document library</returns>
        public bool CreateNewDocumentLibrary(string spSiteURL, string libTemplateName, string newDocLibTitle, string newDocLibDesc)
        {          
                bool isNewDocLibraryCreated = false;

                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite site = new SPSite(spSiteURL))
                        {//Executes the specified method with Full Control rights even if the user does not otherwise have Full Control.

                            SPListTemplateType listTemplateType = new SPListTemplateType();

                            switch (libTemplateName.ToLower())
                            {
                                case "generic list":
                                    listTemplateType = SPListTemplateType.GenericList;
                                    break;                            

                                case "events":
                                    listTemplateType = SPListTemplateType.Events;
                                    break;                            

                                case "announcements":                         
                                    listTemplateType = SPListTemplateType.Announcements;
                                    break;
                                
                                case "document library":
                                    listTemplateType = SPListTemplateType.DocumentLibrary;
                                    break;  
                                
                                case "picture library":
                                    listTemplateType = SPListTemplateType.PictureLibrary;
                                    break;

                                case "contacts":
                                    listTemplateType = SPListTemplateType.Contacts;
                                    break;

                                default:
                                    listTemplateType = SPListTemplateType.DocumentLibrary;
                                    break;
                            }

                            using (SPWeb web = site.OpenWeb())
                            {                              
                                web.AllowUnsafeUpdates = true;
                                Guid newDocGuid = web.Lists.Add(newDocLibTitle, newDocLibDesc, listTemplateType);

                                //Add short-cut onto the left side menu
                                SPDocumentLibrary library = web.Lists[newDocGuid] as SPDocumentLibrary;
                                library.OnQuickLaunch = true;
                                library.Update();
                                web.Update();

                                web.AllowUnsafeUpdates = false;
                                isNewDocLibraryCreated = true;
                            } // SPWeb object web.Dispose() automatically called.

                        } // SPSite object siteCollection.Dispose() automatically called.


                    });

                }
                catch (Exception err)
                {
                    LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                    objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewDocumentLibrary function");
                    objErr = null;

                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                }

                return isNewDocLibraryCreated;
            }
           
       

        /// <summary>
        ///    Create a new custom document library based on using a custom template
        ///     REFERENCE:  1. http://msdn.microsoft.com/en-us/library/ms425818.aspx
        ///                 2. http://www.learningsharepoint.com/2010/09/05/create-list-from-list-template-sharepoint-2010-programmatically/ 
        /// </summary>
        /// <param name="spSiteURL"></param>
        /// <param name="DocLibTemplate"></param>
        /// <param name="newDocLibTitle"></param>
        /// <param name="newDocLibDesc"></param>
        /// <returns>Returns true if a new custom document library has been created.</returns>
        public bool CreateNewCustomDocLibrary(string spSiteURL, string DocLibTemplate, string newDocLibTitle, string newDocLibDesc)
        {

            bool isNewDocLibCreated = false;

            try
            {

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteURL))
                    {//Executes the specified method with Full Control rights even if the user does not otherwise have Full Control.

                        using (SPWeb web = site.OpenWeb())
                        {
                            web.AllowUnsafeUpdates = true;

                            SPListTemplateCollection listTemplates = site.GetCustomListTemplates(web);
                            SPListTemplate listTemplate = listTemplates[DocLibTemplate];
                            Guid newDocGuid = web.Lists.Add(newDocLibTitle, newDocLibDesc, listTemplate);

                            //Add short-cut onto the left side menu
                            SPDocumentLibrary library = web.Lists[newDocGuid] as SPDocumentLibrary;
                            library.OnQuickLaunch = true;
                            library.Update();
                            web.Update();

                            web.AllowUnsafeUpdates = false;
                            isNewDocLibCreated = true;
                            
                        } // SPWeb object web.Dispose() automatically called.

                    } // SPSite object siteCollection.Dispose() automatically called.


                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewCustomDocLibrary function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return isNewDocLibCreated;
        }


        /// <summary>
        ///     REFERENCE: http://blog.sharedove.com/adisjugo/index.php/2012/07/31/creating-site-collections-in-specific-content-database/
        /// </summary>
        /// <param name="newContentDatabase"></param>
        /// <param name="newSiteUrl"></param>
        /// <param name="newSiteTitle"></param>
        /// <param name="newSiteDesc"></param>
        /// <param name="LCID"></param>
        /// <param name="siteTemplate"></param>
        /// <param name="sitePrimaryOwner"></param>
        /// <param name="siteSecondaryOwner"></param>
        public bool CreateNewSiteWithNewContentDB(SPContentDatabase newContentDatabase, string newSiteUrl, string newSiteTitle, string newSiteDesc, UInt32 LCID, string siteTemplate, SPFieldUserValue sitePrimaryOwner, SPFieldUserValue siteSecondaryOwner, bool activateTemplate)
        {
            bool isNewSiteCreated = false;

            try
            {
                // Set the user for the new site
                string primaryOwnerLogin = sitePrimaryOwner.User.LoginName; //user’s login name
                string primaryOwnerEmail = sitePrimaryOwner.User.Email;
                string primaryOwnerName = sitePrimaryOwner.User.Name; //Display name

                string secondaryOwnerLogin = siteSecondaryOwner.User.LoginName; //user’s login name
                string secondaryOwnerEmail = siteSecondaryOwner.User.Email;
                string secondaryOwnerName = siteSecondaryOwner.User.Name; //Display name

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPUtility.ValidateFormDigest();

                    if (activateTemplate == true)
                    {
                        //Step 4 - Create the new site collection in the new database
                        using (SPSite newSiteCollection = newContentDatabase.Sites.Add(newSiteUrl, newSiteTitle, newSiteDesc, LCID, null, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail))
                        {
                            newSiteCollection.AllowUnsafeUpdates = true;

                            ProcessSiteTemplate(newSiteCollection, siteTemplate);
                          
                            //TODO: Add the upper limit for the site collection
                            //SPQuota quota = new SPQuota();
                            //quota.StorageMaximumLevel = maximumBytesForSiteCollection;
                            //newSiteCollection.Quota = quota;

                            // And update the site collection and the content database
                            newSiteCollection.RootWeb.Update();
                            newContentDatabase.Update();
                            newSiteCollection.AllowUnsafeUpdates = false;

                            isNewSiteCreated = true;
                        }
                    }
                    else
                    {
                        //This is to assume that  site template has already been uploaded to the new site collection and activated
                        using (SPSite newSiteCollection = newContentDatabase.Sites.Add(newSiteUrl, newSiteTitle, newSiteDesc, LCID, siteTemplate, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail))
                        {
                            newSiteCollection.AllowUnsafeUpdates = true;                        

                            // And update the site collection and the content database
                            newSiteCollection.RootWeb.Update();
                            newContentDatabase.Update();
                            newSiteCollection.AllowUnsafeUpdates = false;

                            isNewSiteCreated = true;
                        }
                    }
                    
                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewSiteWithNewContentDB function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);             

            }

            return isNewSiteCreated;
        }


        private void ProcessSiteTemplate(SPSite newSiteCollection, string siteTemplate)
        {
            SPWeb web = newSiteCollection.RootWeb;
            string currentUrl = web.Url;
            web.AllowUnsafeUpdates = true;

             //Check if custom site template has been uploaded and activated to the new site collection
            if (searchSiteTemplate(currentUrl, siteTemplate) == true)
            {
                //Apply web template
                ApplySiteTemplate(currentUrl, siteTemplate);
            }
            else
            {
                //programmatically upload site template and activate it
                string ListName = "Solution Gallery";
                string pathSolution = @GetRCRSettingsItem("SiteTemplateFilePath", _spSettingList).ToString();
                string strTemplateGuid = GetCustomSiteTemplateGuid(siteTemplate);

                if (UploadAndActivateSolution(currentUrl, ListName, pathSolution, strTemplateGuid, true) == true)
                {
                    if (searchSiteTemplate(currentUrl, siteTemplate) == true)
                    {
                        //Apply web template                                          
                        ApplySiteTemplate(currentUrl, siteTemplate);
                    }
                    else
                    {
                        LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                        string msg = "Applying template: " + pathSolution + " was not successful!";
                        objErr.sendUserEmail(APP_NAME, "Unable to apply site template", msg, false);
                        objErr = null;
                    }

                }
                else
                {
                    LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                    string msg = "Uploading template: " + pathSolution + " was not successful!";
                    objErr.sendUserEmail(APP_NAME, "Unable to upload site template", msg, false);
                    objErr = null;
                }    
            }

            // need to reload web. Features in webtemplate have modified it!
            using (web = newSiteCollection.OpenWeb(web.ServerRelativeUrl))
            {
                web.AllowUnsafeUpdates = false;
            }

        }

        /// <summary>
        ///     Create a new site collection. Returns true if a new site collection was successfully created.
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="newSiteUrl"></param>
        /// <param name="newSiteTitle"></param>
        /// <param name="newSiteDesc"></param>
        /// <param name="LCID"></param>
        /// <param name="siteTemplate">
        ///   * For complete list of site template definition see http://msdn.microsoft.com/en-us/library/office/ms411953(v=office.15).aspx
        ///   * You can also get the site template defition for custom site template 
        ///     - http://stackoverflow.com/questions/3240967/sharepoint-2010-create-site-from-code-using-custom-site-template
        ///     - http://www.learningsharepoint.com/2010/07/25/programatically-create-site-from-site-template-sharepoint-2010/
        /// </param>
        /// <param name="siteOwner"></param>
        public bool CreateSite(string spSiteUrl, string newSiteUrl, string newSiteTitle, string newSiteDesc, UInt32 LCID, string siteTemplate, SPFieldUserValue sitePrimaryOwner, SPFieldUserValue siteSecondaryOwner, bool activateTemplate)
        {
            bool isSiteCreated = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPWebApplication webApp = new SPSite(spSiteUrl).WebApplication;
                            SPSiteCollection siteCollection = webApp.Sites;

                            // Set the user for the new site
                            string primaryOwnerLogin = sitePrimaryOwner.User.LoginName; //user’s login name
                            string primaryOwnerEmail = sitePrimaryOwner.User.Email;
                            string primaryOwnerName = sitePrimaryOwner.User.Name; //Display name

                            string secondaryOwnerLogin = siteSecondaryOwner.User.LoginName; //user’s login name
                            string secondaryOwnerEmail = siteSecondaryOwner.User.Email;
                            string secondaryOwnerName = siteSecondaryOwner.User.Name; //Display name

                            //Create new web site
                            SPSite newSiteCollection = null;
                            if (activateTemplate == true)
                            {
                                newSiteCollection = siteCollection.Add("/" + newSiteUrl, newSiteTitle, newSiteDesc, LCID, null, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail);
                                ProcessSiteTemplate(newSiteCollection, siteTemplate);
                            }
                            else
                            {
                                newSiteCollection = siteCollection.Add("/" + newSiteUrl, newSiteTitle, newSiteDesc, LCID, siteTemplate, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail);
                            }


                            if (newSiteCollection != null)
                                isSiteCreated = true;

                            newSiteCollection.Dispose();
                            siteCollection = null;
                            webApp = null;

                        }
                    }
                });
                
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateSite function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);             
            }

            return isSiteCreated;
        }

        public bool CreateSiteWithExistingDB(string spSiteUrl, string newSiteUrl, string newSiteTitle, string newSiteDesc, UInt32 LCID, string siteTemplate, SPFieldUserValue sitePrimaryOwner, SPFieldUserValue siteSecondaryOwner, bool activateTemplate, string DBServer, string DBName)
        {
            bool isSiteCreated = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPWebApplication webApp = new SPSite(spSiteUrl).WebApplication;
                            SPSiteCollection siteCollection = webApp.Sites;

                            // Set the user for the new site
                            string primaryOwnerLogin = sitePrimaryOwner.User.LoginName; //user’s login name
                            string primaryOwnerEmail = sitePrimaryOwner.User.Email;
                            string primaryOwnerName = sitePrimaryOwner.User.Name; //Display name

                            string secondaryOwnerLogin = siteSecondaryOwner.User.LoginName; //user’s login name
                            string secondaryOwnerEmail = siteSecondaryOwner.User.Email;
                            string secondaryOwnerName = siteSecondaryOwner.User.Name; //Display name

                            //Create new web site
                            SPSite newSiteCollection = null;
                            if (activateTemplate == true)
                            {
                                newSiteCollection = siteCollection.Add("/" + newSiteUrl, newSiteTitle, newSiteDesc, LCID, null, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail, DBServer, DBName, null, null);
                                ProcessSiteTemplate(newSiteCollection, siteTemplate);
                            }
                            else
                            {
                                newSiteCollection = siteCollection.Add("/" + newSiteUrl, newSiteTitle, newSiteDesc, LCID, siteTemplate, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail, DBServer, DBName, null, null);
                            }


                            if (newSiteCollection != null)
                                isSiteCreated = true;

                            newSiteCollection.Dispose();
                            siteCollection = null;
                            webApp = null;

                        }
                    }
                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateSiteWithExistingDB function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isSiteCreated;
        }



        /// <summary>
        /// Returns a URL with only the scheme and authority segmenst from a full Url.
        /// This will generate the url of the root site collection
        /// </summary>
        /// <param name="url">The full URL to format.</param>
        /// <returns>Returns the URL without the path and querystring.</returns>
        /// <example>http://devserver</example>
        public string GetAuthorityUrl(string url)
        {
            Uri uri = new Uri(url);
            return string.Format(@"{0}://{1}", uri.Scheme, uri.Authority);
        }

        /// <summary>
        ///     Create a new site collection without a farm level permission
        /// </summary>
        /// <remarks>
        ///     REFERENCE:
        ///         1. http://peterheibrink.wordpress.com/2009/09/09/self-service-site-creation-through-code/
        ///         2. http://msdn.microsoft.com/en-us/library/ms439417.aspx
        /// </remarks>
        /// <param name="activateTemplate"></param>
        /// <param name="spSiteUrl"></param>
        /// <param name="newSiteUrl"></param>
        /// <param name="newSiteTitle"></param>
        /// <param name="newSiteDesc"></param>
        /// <param name="LCID"></param>
        /// <param name="siteTemplate"></param>
        /// <param name="sitePrimaryOwner"></param>
        /// <param name="siteSecondaryOwner"></param>
        /// <param name="DB_Server"></param>
        /// <param name="DB_Name"></param>
        /// <returns></returns>
        public bool SelfServiceCreateSite(bool activateTemplate, string spSiteUrl, string newSiteUrl, string newSiteTitle, string newSiteDesc, UInt32 LCID, string siteTemplate, SPFieldUserValue sitePrimaryOwner, SPFieldUserValue siteSecondaryOwner)
        {
            bool isSiteCreated = false;

            try
            {
                // Set the user for the new site
                string primaryOwnerLogin = sitePrimaryOwner.User.LoginName; //user’s login name
                string primaryOwnerEmail = sitePrimaryOwner.User.Email;
                string primaryOwnerName = sitePrimaryOwner.User.Name; //Display name

                string secondaryOwnerLogin = siteSecondaryOwner.User.LoginName; //user’s login name
                string secondaryOwnerEmail = siteSecondaryOwner.User.Email;
                string secondaryOwnerName = siteSecondaryOwner.User.Name; //Display name

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    SPUtility.ValidateFormDigest();

                    // Make sure we are at the root site collection
                    using (SPSite rootSite = new SPSite(GetAuthorityUrl(spSiteUrl)))
                    {

                        if (activateTemplate == true)
                        {
                            using (SPSite newSiteCollection = rootSite.SelfServiceCreateSite(newSiteUrl, newSiteTitle, newSiteDesc, LCID, null, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail))
                            {
                                SPWeb web = newSiteCollection.RootWeb;
                                string currentUrl = web.Url;
                                web.AllowUnsafeUpdates = true;

                                //Check if custom site template has been uploaded and activated to the new site collection
                                if (searchSiteTemplate(currentUrl, siteTemplate) == true)
                                {                                 
                                    //Apply web template
                                    //newSiteCollection.RootWeb.ApplyWebTemplate(_spWebTemplate.Name);
                                    //or call 
                                    ApplySiteTemplate(currentUrl, siteTemplate);
                                }
                                else
                                {
                                    //programmatically upload site template and activate it
                                    string ListName = "Solution Gallery";
                                    string pathSolution = @GetRCRSettingsItem("SiteTemplateFilePath", _spSettingList).ToString();
                                    string strTemplateGuid = GetCustomSiteTemplateGuid(siteTemplate);

                                    if (UploadAndActivateSolution(currentUrl, ListName, pathSolution, strTemplateGuid, true) == true)
                                    {
                                        if (searchSiteTemplate(currentUrl, siteTemplate) == true)
                                        {
                                            //Apply web template                                          
                                            ApplySiteTemplate(currentUrl, siteTemplate);
                                        }
                                        else
                                        {
                                            LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                                            string msg = "Applying template: " + pathSolution + " was not successful!";
                                            objErr.sendUserEmail(APP_NAME, "Unable to apply site template", msg, false);
                                            objErr = null;
                                        }
                                        
                                    }
                                    else
                                    {
                                        LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                                        string msg = "Uploading template: " + pathSolution + " was not successful!";
                                        objErr.sendUserEmail(APP_NAME, "Unable to upload site template", msg, false);
                                        objErr = null;
                                    }                             
                                   
                                }

                                // need to reload web. Features in webtemplate have modified it!
                                using (web = newSiteCollection.OpenWeb(web.ServerRelativeUrl))
                                {
                                    web.AllowUnsafeUpdates = false;
                                }

                                isSiteCreated = true;
                            }
                        }
                        else
                        {
                            //This is to assume that  site template has already been uploaded to the new site collection and activated
                            using (SPSite newSiteCollection = rootSite.SelfServiceCreateSite(newSiteUrl, newSiteTitle, newSiteDesc, LCID, siteTemplate, primaryOwnerLogin, primaryOwnerName, primaryOwnerEmail, secondaryOwnerLogin, secondaryOwnerName, secondaryOwnerEmail))
                            {
                                isSiteCreated = true;
                            }
                            //newSiteCollection.Dispose();
                        }

                    }
                });

            }
            catch (Exception err)
            {
                
               LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
               objErr.logSysErrorEmail(APP_NAME, err, "Error at SelfServiceCreateSite function");
               objErr = null;
                
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isSiteCreated;
        }

        #endregion

        #region Create column method

        /// <summary>
        ///     Add a new choice column to a specific SharePoint list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="displayColName"></param>
        /// <param name="arrChoices"></param>
        public void CreateNewColumnListChoiceType(string spSiteUrl, string listName, string displayColName, string[] arrChoices, string Description)
        {

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPList list = web.Lists[listName];

                            //check if column exist
                            if (list.Fields.ContainsField(displayColName) == false)
                            {   //Only add new column if one does not yet exist
                                web.AllowUnsafeUpdates = true;
                                list.Fields.Add(displayColName, SPFieldType.Choice, false);

                                var fldChoice = list.Fields[displayColName] as SPFieldChoice;

                                if (fldChoice != null)
                                {
                                    string defaultChoiceValue = "";
                                    for (int i = 0; i <= arrChoices.Length - 1; i++)
                                    {
                                        if (i == 0)
                                            defaultChoiceValue = arrChoices[i];

                                        fldChoice.Choices.Add(arrChoices[i].ToString());
                                    }
                                    fldChoice.DefaultValue = defaultChoiceValue;
                                    fldChoice.Description = Description;
                                    fldChoice.Update();
                                }
                                list.Update();
                                web.Update();
                                web.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewColumnListChoiceType function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }
        }

        /// <summary>
        ///     Add a new Note column into a specific list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="displayColName"></param>
        /// <param name="isMandatory"></param>
        /// <param name="isMultiLine"></param>
        public void CreateNewColumnListNoteType(string spSiteUrl, string listName, string displayColName, bool isMandatory, bool isMultiLine)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPList list = web.Lists[listName];

                            //check if column exist
                            if (list.Fields.ContainsField(displayColName) == false)
                            {//Only add new column if one does not yet exist
                                web.AllowUnsafeUpdates = true;

                                list.Fields.Add(displayColName, SPFieldType.Note, isMandatory);

                                if (isMultiLine)
                                {
                                    var fldMultiLine = list.Fields[displayColName] as SPFieldMultiLineText;

                                    if (fldMultiLine != null)
                                    {
                                        fldMultiLine.NumberOfLines = 6;
                                        fldMultiLine.Update();
                                    }
                                }
                                list.Update();
                                web.Update();
                                web.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewColumnListNoteType function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }
        }

        #endregion

        #region Get methods

        /// <summary>
        ///     Function to search if a site template already exist in site collection
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="siteTemplateName"></param>
        /// <returns></returns>
        public bool searchSiteTemplate(string spSiteUrl, string siteTemplateName)
        {
            bool isSiteTemplateFound = false;

            try
            {
                StringBuilder sb = new StringBuilder();

                using (SPSite site = new SPSite(spSiteUrl))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPWebTemplateCollection templates = web.GetAvailableWebTemplates(1033, true);

                            foreach (SPWebTemplate template in templates)
                            {
                                if (template.Name.ToString().ToLower() == siteTemplateName.ToLower())
                                {
                                    _spWebTemplate = template;
                                    return true;
                                }
                            }                    
                    }

                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetCustomSiteTemplate function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isSiteTemplateFound;
        }

        /// <summary>
        ///     Function to return string GUID of custom template site
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public string GetCustomSiteTemplateGuid(string templateName)
        {
            string [] arrGuid = templateName.Split('#');
            string Guid = "";

            if (arrGuid.Length >0)
            {
                 string GuidValue = arrGuid[0];
                 Guid = GuidValue.Replace("{", "").Replace("}", "");  
            }

            return Guid;
        }

        /// <summary>
        ///     Function to get all site template info
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="getAllTemplate"></param>
        /// <returns></returns>
        public string GetCustomSiteTemplate(string spSiteUrl, bool getAllTemplate)
        {
            string siteTemplate = "";

            try
            {
                StringBuilder sb = new StringBuilder();

                using (SPSite site = new SPSite(spSiteUrl))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPWebTemplateCollection templates = web.GetAvailableWebTemplates(1033, true);

                        if (getAllTemplate == true)
                        {
                            foreach (SPWebTemplate template in templates)
                            {
                                sb.AppendFormat("ID: {0} LCID: {1} Name: {2} Title: {3} <br>", template.ID,  template.Lcid.ToString(), template.Name, template.Title);
                            }
                            siteTemplate = sb.ToString();
                        }
                    }

                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetCustomSiteTemplate function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return siteTemplate;
        }


        /// <summary>
        ///     Get value content of a specific list
        /// </summary>
        /// <param name="spListName"></param>
        /// <param name="spQueryStr"></param>
        /// <param name="spFieldName"></param>
        /// <returns></returns>
        public string GetSharePointListItem(string spListName, string spQueryStr, string spFieldName, bool displayTitle)
        {
            try
            {
                string spListContentItem = "";

                using (SPWeb web = SPContext.Current.Web)
                {

                    SPList spList = web.Lists[spListName];
                    SPQuery spQuery = new SPQuery();
                    spQuery.Query = spQueryStr;
                    SPListItemCollection spListItems = spList.GetItems(spQuery);

                    if (spListItems.Count >= 1)
                    {
                        foreach (SPListItem item in spListItems)
                        {

                            if (displayTitle)
                            {
                                string title = "";
                                title = item["Title"].ToString() + " ";
                                spListContentItem = title + item[spFieldName].ToString();
                            }
                            else
                            {
                                spListContentItem = item[spFieldName].ToString();
                            }

                        }
                    }
                }

                return spListContentItem;
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetSharePointListItem function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return string.Empty;
            }
        }

       
        /// <summary>
        ///     Function to get current login SP user
        /// </summary>
        /// <param name="oWeb"></param>
        /// <returns></returns>
        public SPFieldUserValue GetCurrentUser(SPWeb oWeb)
        {
            try
            {
                //Variable to store the user
                SPUser oUser = oWeb.CurrentUser;
                SPFieldUserValue loginUser = new SPFieldUserValue(oWeb, oUser.ID, oUser.LoginName);
                oUser = null;

                return loginUser;
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetCurrentUser function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                SPFieldUserValue ret = null;

                return ret;
            }

        }

        /// <summary>
        ///     Function to get the list of features GUID 
        /// </summary>
        /// <param name="featureScope"></param>
        /// <param name="SPList"></param>
        /// <param name="viewRowLimit"></param>
        /// <returns>Returns the list of feautures GUID as a list item collection object</returns>
        public SPListItemCollection GetSPFeaturesByScope(string featureScope, string SPList, uint viewRowLimit)
        {
            SPListItemCollection spListItems = null;

            try
            {
                SPList spList = _spCurrentWeb.Lists[SPList];
                SPQuery spQuery = new SPQuery();
                spQuery.Query = @"<Where><Eq><FieldRef Name='Scope'/><Value Type='CHOICE'>" + featureScope + "</Value></Eq></Where>";
                spQuery.ViewFields = String.Concat("<FieldRef Name='FeatureGUID'/>");
                spQuery.ViewFieldsOnly = true;
                spQuery.RowLimit = viewRowLimit;
                spListItems = spList.GetItems(spQuery);
             
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetSPFeaturesByScope function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return spListItems;
        }

        /// <summary>
        ///     Get the approver's name from the custom approver list 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetApproversName(string userName, string SPList)
        {
            string ApproverName = ""; //Default approver

            try
            {
                SPList spList = _spCurrentWeb.Lists[SPList];
                SPQuery spQuery = new SPQuery();
                spQuery.Query = @"<Where><Eq><FieldRef Name='Approvers'/><Value Type='User'>" + userName + "</Value></Eq></Where>";
                spQuery.ViewFields = String.Concat("<FieldRef Name='Approvers'/>");
                spQuery.ViewFieldsOnly = true;
                spQuery.RowLimit = 1;
                SPListItemCollection spListItems = spList.GetItems(spQuery);
                if (spListItems != null)
                {
                    foreach (SPListItem spListItem in spListItems)
                    {
                        if (spListItem["Approvers"] != null)
                        {
                            ApproverName = spListItem["Approvers"].ToString();
                            return ApproverName;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetApproversName function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return ApproverName;
        }

        /// <summary>
        ///     Update the setting list item
        /// </summary>
        /// <param name="category"></param>
        /// <param name="spListName"></param>
        /// <param name="changeValue"></param>
        /// <param name="oSPWeb"></param>
        public void UpdateRCRSettingsItem(string category, string spListName, string changeValue, SPWeb oSPWeb)
        {

            try
            {
                int listItemId = 0;
                string fieldName = "SettingsValue";

                if (_spCurrentWeb == null)
                    _spCurrentWeb = oSPWeb;

                //Get list item ID
                SPList spList = _spCurrentWeb.Lists[spListName];
                SPQuery spQuery = new SPQuery();
                spQuery.Query = @"<Where><Eq><FieldRef Name='SettingsCategory'/><Value Type='CHOICE'>" + category + "</Value></Eq></Where>";
                spQuery.ViewFields = String.Concat("<FieldRef Name='SettingsValue'/>");
                spQuery.ViewFieldsOnly = true;
                spQuery.RowLimit = 1;
                SPListItemCollection spListItems = spList.GetItems(spQuery);
                if (spListItems != null)
                {
                    foreach (SPListItem spListItem in spListItems)
                    {
                        if (spListItem["SettingsValue"] != null)
                        {
                            listItemId = spListItem.ID;
                        }
                    }
                }

                //Update setting item if record was found
                if (listItemId > 0)
                {
                    oSPWeb.AllowUnsafeUpdates = true;

                    // Fetch the List
                    SPList list = oSPWeb.Lists[spListName];

                    // Update the List item by ID
                    SPListItem itemToUpdate = list.GetItemById(listItemId);
                    itemToUpdate[fieldName] = changeValue;
                    itemToUpdate.Update();

                    oSPWeb.AllowUnsafeUpdates = false;
                }
           

            }
            catch(Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at UpdateRCRSettingsItem function. Unable to update item");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

        }

        /// <summary>
        ///     Get configuration settings from the list
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public string GetRCRSettingsItem(string category, string SPList)
        {
            string description = string.Empty;

            try
            {
                if (category == string.Empty)
                {
                    return description;
                }


                //using (SPWeb web = _spCurrentSite)
                {
                    SPList spList = _spCurrentWeb.Lists[SPList];
                    SPQuery spQuery = new SPQuery();
                    spQuery.Query = @"<Where><Eq><FieldRef Name='SettingsCategory'/><Value Type='CHOICE'>" + category + "</Value></Eq></Where>";
                    spQuery.ViewFields = String.Concat("<FieldRef Name='SettingsValue'/>");
                    spQuery.ViewFieldsOnly = true;
                    spQuery.RowLimit = 1;
                    SPListItemCollection spListItems = spList.GetItems(spQuery);
                    if (spListItems != null)
                    {
                        foreach (SPListItem spListItem in spListItems)
                        {
                            if (spListItem["SettingsValue"] != null)
                            {
                                description = spListItem["SettingsValue"].ToString();
                                return description;
                            }
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetRCRSettingsItem function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return description;
        }


        /// <summary>
        ///     Function to get the created by user for a particular item
        /// </summary>
        /// <param name="SPList"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public string GetCreatedByUser(string SPList, string itemID)
        {
            string author = "";

            try
            {
                SPList spList = _spCurrentWeb.Lists[SPList];
                SPQuery spQuery = new SPQuery();
                spQuery.Query = @"<Where><Eq><FieldRef Name='ID'/><Value Type='Counter'>" + itemID + "</Value></Eq></Where>";
                spQuery.ViewFields = String.Concat("<FieldRef Name='Author'/>");
                spQuery.ViewFieldsOnly = true;
                spQuery.RowLimit = 1;
                SPListItemCollection spListItems = spList.GetItems(spQuery);
                if (spListItems != null)
                {
                    foreach (SPListItem spListItem in spListItems)
                    {
                        if (spListItem["Author"] != null)
                        {
                            author = spListItem["Author"].ToString();
                            return author;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetCreatedByUser function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return author;
        }
        
        #endregion

        #region Delete methods

        /// <summary>
        ///     Delete a particular list item
        /// </summary>
        public void DeleteListItem(string spListName, int listItemId, SPWeb oSPWeb)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (_spCurrentWeb == null)
                        _spCurrentWeb = oSPWeb;

                    _spCurrentWeb.AllowUnsafeUpdates = true;
                    // Fetch the List
                    SPList list = _spCurrentWeb.Lists[spListName];

                    SPListItem itemToDelete = list.GetItemById(listItemId);
                    itemToDelete.Delete();

                    _spCurrentWeb.AllowUnsafeUpdates = false;
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DeleteListItem function. Unable to delete item ID: " + listItemId);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }


        /// <summary>
        ///     Function to delete a particular SharePoint list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="spListName"></param>
        public void DeleteList(string spListName, SPWeb oSPWeb)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (_spCurrentWeb == null)
                        _spCurrentWeb = oSPWeb;

                    _spCurrentWeb.AllowUnsafeUpdates = true;
                    SPListCollection lists = _spCurrentWeb.Lists;

                    SPList listToDelete = lists[spListName];
                    System.Guid listGuid = listToDelete.ID;

                    lists.Delete(listGuid);

                    _spCurrentWeb.AllowUnsafeUpdates = false;
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DeleteList function. Unable to delete " + spListName);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }

        #endregion

        #region Update methods

        /// <summary>
        ///     Function to update an existing list item
        /// </summary>
        /// <param name="spListName"></param>
        /// <param name="listItemId"></param>
        /// <param name="fieldName"></param>
        /// <param name="changeValue"></param>
        /// <param name="oSPWeb"></param>
        public void UpdateListItem(string spListName, int listItemId, string fieldName, string changeValue,  SPWeb oSPWeb)
        {
            try
            {
                if (_spCurrentWeb == null)
                    _spCurrentWeb = oSPWeb;

                oSPWeb.AllowUnsafeUpdates = true;

                // Fetch the List
                SPList list = oSPWeb.Lists[spListName];

                // Update the List item by ID
                SPListItem itemToUpdate = list.GetItemById(listItemId);
                itemToUpdate[fieldName] = changeValue;
                itemToUpdate.Update();

                oSPWeb.AllowUnsafeUpdates = false;
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at UpdateListItem function. Unable to update item ID: " + listItemId);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }

        /// <summary>
        /// Update list title using web service.
        /// </summary>
        public void UpdateListTitle(string spSiteUrl, string oldListName, string newListName)
        {
           
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite site = new SPSite(spSiteUrl))
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                web.AllowUnsafeUpdates = true;

                                SPList list = web.Lists[oldListName];
                                list.Title = newListName;
                                list.Update();

                                web.Update();
                                web.AllowUnsafeUpdates = false;
                            }
                        }
                    });
                }
                catch (Exception err)
                {
                    LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                    objErr.logSysErrorEmail(APP_NAME, err, "Error at UpdateListTitle function. Unable to update old list name " + oldListName + " to new name " + newListName);
                    objErr = null;

                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                }
            
        }


        #endregion

        #region Upload methods


        /// <summary>
        ///     Function to automatically upload a list template to the list gallary
        /// </summary>
        /// <param name="siteURL"></param>
        /// <param name="templateListLocation"></param>
        /// <param name="templateDesc"></param>
        /// <returns>Returns true if template list was successfully uploaded</returns>
        public bool uploadListTemplate(string siteURL, string templateListLocation, string templateDesc)
        {
            bool isTemplateListUploaded = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite objSite = new SPSite(siteURL))
                    {
                        SPWeb rootWeb = objSite.RootWeb;
                        SPDocumentLibrary lstListTemplateGallery = (SPDocumentLibrary)rootWeb.Lists["List Template Gallery"];
                        SPFolder rootFolder = lstListTemplateGallery.RootFolder;

                        string strSTPFileName = Path.GetFileName(templateListLocation);
                        string strSTPFileNameWithoutExt = strSTPFileName.Substring(0, strSTPFileName.IndexOf("."));
                        SPFile newFile = rootWeb.GetFile(rootFolder.Url + "/" + strSTPFileName);

                        if (!newFile.Exists)
                        {
                            SPFile spfile = rootFolder.Files.Add(strSTPFileName, File.ReadAllBytes(templateListLocation), true);
                            spfile.Item["TemplateTitle"] = strSTPFileNameWithoutExt;
                            spfile.Item["Description"] = templateDesc;
                            spfile.Item.Update();
                            Thread.Sleep(1000); //Give time for or SharePoint to apply change
                            spfile = null;
                            isTemplateListUploaded = true;
                        }
                        else
                        {
                            //template list already exist
                        }

                        rootFolder = null;
                        lstListTemplateGallery = null;
                        rootWeb.Dispose();
                    }
                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at uploadListTemplate function. Unable to update list template: " + templateListLocation);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return isTemplateListUploaded;

        }

        /// <summary>
        ///     Upload a solution wsp file and activate it
        /// </summary>
        /// <remarks>
        ///     REFERENCES:
        ///         1. http://social.technet.microsoft.com/Forums/en-US/9c9e0203-fee2-4b1e-8e3c-d07774b26863/how-to-import-site-template-programmatically?forum=sharepointdevelopmentprevious
        ///         2. http://sharepoint.stackexchange.com/questions/80990/add-wsp-to-solution-gallery-programmatically
        /// </remarks>
        /// <param name="spSiteUrl"></param>
        /// <param name="ListName"></param>
        /// <param name="pathSolutionFileName"></param>
        /// <param name="solutionGuid"></param>
        public bool UploadAndActivateSolution(string spSiteUrl, string ListName, string pathSolutionFileName, string solutionGuid, bool activateFeature)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite destSite = new SPSite(spSiteUrl))
                    {
                        SPWeb destWeb = destSite.RootWeb;
                        string fileName = Path.GetFileName(pathSolutionFileName);
                        SPDocumentLibrary listTemplateGallery;
                        SPFile file = null;

                        if (ListName.ToLower() == "solution gallery")
                        {
                            listTemplateGallery = (SPDocumentLibrary)destWeb.Site.GetCatalog(SPListTemplateType.SolutionCatalog);
                            file = listTemplateGallery.RootFolder.Files.Add(fileName, File.ReadAllBytes(pathSolutionFileName), true);
                        }
                        else
                        {
                            listTemplateGallery = (SPDocumentLibrary)destWeb.Lists[ListName];
                            SPFolder listFolder = listTemplateGallery.RootFolder;
                            file = listFolder.Files.Add(fileName, File.ReadAllBytes(pathSolutionFileName), true);
                        }

                        if (file != null)
                        {
                            file.Update();
                            Thread.Sleep(1000); //Give time for or SharePoint to apply change

                            //Activate solution
                            SPUserSolution solution = destWeb.Site.Solutions.Add(file.Item.ID);
                            Thread.Sleep(1000); //Give time for or SharePoint to apply change

                            if (activateFeature == true)
                            {
                                destSite.Features.Add(new Guid(solutionGuid), false, SPFeatureDefinitionScope.Site); //Only work if solution is  site scope     
                                Thread.Sleep(1000); //Give time for or SharePoint to apply change
                            }
                        }

                        file = null;
                        listTemplateGallery = null;
                        destWeb.Dispose();
                    }
                });

                return true;
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at UploadAndActivateSolution function. Unable to upload and activate " + pathSolutionFileName);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return false;
            }
        }


        /// <summary>
        ///     Upload a document to a document library
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="destDocLibName"></param>
        /// <param name="uploadFileName"></param>
        /// <param name="fileToUpload"></param>
        public void UploadFileToRootDocumentLibrary(string spSiteUrl, string destDocLibName, string uploadFileName, Stream fileToUpload)
        {

            try
            {

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite oSite = new SPSite(spSiteUrl))
                    {
                        using (SPWeb oWeb = oSite.OpenWeb())
                        {
                            if (fileToUpload.Length > 0)
                            {
                                const bool replaceExistingFiles = true;
                                oWeb.AllowUnsafeUpdates = true;
                                string strFilenName = Path.GetFileName(uploadFileName);
                                var contents = new byte[Convert.ToInt32(fileToUpload.Length)];

                                fileToUpload.Read(contents, 0, Convert.ToInt32(fileToUpload.Length));
                                fileToUpload.Close();

                                SPFolder myLibrary = oWeb.Folders[destDocLibName];
                                if (myLibrary != null)
                                    if (strFilenName != null)
                                        if (myLibrary.Files != null)
                                            myLibrary.Files.Add(strFilenName, contents, replaceExistingFiles);
                                oWeb.AllowUnsafeUpdates = false;

                            }

                        }

                    }
                });
            }
            catch (Exception err)
            {
                //WriteErrorLog("ERROR: Cannot upload file " + uploadFileName + " to SharePoint! " + err.Message);
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at UploadAndActivateSolution function. Cannot upload file " + uploadFileName + " to " + destDocLibName);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

        }

        /// <summary>
        ///     Upload a new or existing file to an existing document library
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="destDocLibName"></param>
        /// <param name="destFolderName"></param>
        /// <param name="uploadFileName"></param>
        /// <param name="fileToUpload"></param>
        public void UploadFileToDocumentLibrary(string spSiteUrl, string destDocLibName, string destFolderName, string uploadFileName, Stream fileToUpload)
        {
            if (uploadFileName == null) return;
            if (!File.Exists(uploadFileName))
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.sendUserEmail(APP_NAME, "Error at UploadFileToDocumentLibrary", "ERROR: File " + uploadFileName + " was not found to be uploaded to a document library!", false);
                objErr = null;
                throw new FileNotFoundException("File not found.", uploadFileName);
            }

            if (destFolderName.Length <= 0)
            {
                //If file is to be uploaded to a document library without a folder
                UploadFileToRootDocumentLibrary(spSiteUrl, destDocLibName, uploadFileName, fileToUpload);
            }
            else
            {

                const bool replaceExistingFiles = true;

                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite site = new SPSite(spSiteUrl))
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                string fileName = Path.GetFileName(uploadFileName);
                                SPFolder folder = web.GetFolder(destDocLibName + "/" + destFolderName);
                                string remoteDestFile = destDocLibName + "/" + destFolderName + "/" + fileName;

                                web.AllowUnsafeUpdates = true;

                                //Get and read the content of the uploaded file
                                var fileBytes = new byte[fileToUpload.Length];
                                fileToUpload.Read(fileBytes, 0, (int)fileToUpload.Length);
                                fileToUpload.Close();

                                //Add uploaded file to document library
                                SPFile newFile = folder.Files.Add(remoteDestFile, fileBytes, replaceExistingFiles);
                                newFile.Item["Title"] = fileName;
                                newFile.Item.Update();
                                newFile.Update();

                                folder.Update();
                                web.AllowUnsafeUpdates = false;
                            }
                        }
                    });
                }
                catch (Exception err)
                {       
                    LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                    objErr.logSysErrorEmail(APP_NAME, err, "Error at UploadFileToDocumentLibrary function. Cannot upload file " + uploadFileName + " to " + destDocLibName);
                    objErr = null;

                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                }


            }
        }
        #endregion

        #region Download methods

        /// <summary>
        ///   Get the byte size of a particular SharePoint document
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="docLibName"></param>
        /// <param name="spFileUrl"></param>
        /// <returns>The byte size</returns>
        public Byte[] GetSharePointFile(string spSiteUrl, string docLibName, string spFileUrl)
        {
            byte[] objByte = new byte[] { };
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (SPSite oSite = new SPSite(spSiteUrl))
                    {
                        using (SPWeb oWeb = oSite.OpenWeb())
                        {
                            SPFolder folder = oWeb.GetFolder(docLibName);
                            string strFilenName = Path.GetFileName(spFileUrl);
                            string fullSPFilePath = spSiteUrl + "/" + docLibName + "/" + spFileUrl;
                            SPFile tempFile = oWeb.GetFile(fullSPFilePath);
                            objByte = (byte[])tempFile.OpenBinary();

                        }

                    }
                });

                return objByte;
            }
            catch (Exception err)
            {              
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetSharePointFile function. Cannot get file " + spFileUrl);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return objByte;
            }
        }

        /// <summary>
        ///    Download a document from SharePoint to a location on the computer
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="docLibName"></param>
        /// <param name="spFileUrl"></param>
        /// <param name="destFolderPath"></param>
        public void DownloadFileFromDocumentLibrary(string spSiteUrl, string docLibName, string spFileUrl, string destFolderPath)
        {
           
            try
            {
                byte[] objByte = new byte[] { };
                objByte = GetSharePointFile(spSiteUrl, docLibName, spFileUrl);
                File.WriteAllBytes(destFolderPath, objByte);
            }
            catch (Exception err)
            {           
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DownloadFileFromDocumentLibrary function. Cannot download file " + spFileUrl);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }
        


        #endregion

        #region feature methods

        /// <summary>
        ///     Function to activate a feature at web level
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="solutionGuid"></param>
        public bool ActivateFeatureWebScope(string spSiteUrl, string solutionGuid, bool sendLog)
        {
            bool isFeatureActivated = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                 {
                     using (SPSite destSite = new SPSite(spSiteUrl))
                     {
                         //using (SPWeb destWeb = destSite.OpenWeb())
                         SPWeb destWeb = destSite.OpenWeb();
                         {
                             Guid featureGuid = new Guid(solutionGuid);

                             if (destWeb.Features[featureGuid] == null)//If feature is not yet activated
                             {
                                 destWeb.AllowUnsafeUpdates = true;
                                 destWeb.Features.Add(featureGuid, true); //Only work if solution is web scope    
                                 destWeb.Update();
                                 Thread.Sleep(1000); //Give time for or SharePoint to apply change
                                 destWeb.AllowUnsafeUpdates = false;

                                 isFeatureActivated = true;
                             }
                             else
                             { //Feature already activated
                                 if (sendLog == true)
                                 {
                                     //send email
                                     LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                                     string msg = "Feature Guid: " + solutionGuid + " has already been activated.";
                                     objErr.sendUserEmail(APP_NAME, "Unable to activate feature", msg, false);
                                     objErr = null;
                                 }
                             }
                         }
                         destWeb.Dispose();
                     }
                 });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at ActivateFeatureWebScope function. Unable to activate feature Guid: " + solutionGuid);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "ActivateFeatureWebScope-" + err.Message, err.StackTrace);
            }

            return isFeatureActivated;
        }

        /// <summary>
        ///     Function to activate a feature at site scope
        /// </summary>
        /// <remarks>
        ///     To activate OOTB feature need to know the GUID. For the list of known GUID see: http://workflowssimplified.wordpress.com/2012/06/26/ootb-feature-and-guid-mapping/
        /// </remarks>
        /// <param name="spSiteUrl"></param>
        /// <param name="solutionGuid"></param>
        public bool ActivateFeatureSiteScope(string spSiteUrl, string solutionGuid, bool sendLog)
        {
            bool isFeatureActivated = false;

            try
            {
              
              SPSecurity.RunWithElevatedPrivileges(delegate()
              {
                  //using (SPSite destSite = new SPSite(spSiteUrl))
                  SPSite destSite = new SPSite(spSiteUrl);
                  {
                      Guid featureGuid = new Guid(solutionGuid);

                      if (destSite.Features[featureGuid] == null)//If feature is not yet activated
                      {
                          destSite.AllowUnsafeUpdates = true;
                          destSite.Features.Add(featureGuid, true); //Only work if solution is  site scope  
                          Thread.Sleep(1000); //Give time for or SharePoint to apply change
                          destSite.AllowUnsafeUpdates = false;

                          isFeatureActivated = true;
                      }
                      else
                      { //Feature already activated
                          if (sendLog == true)
                          {
                              //send email
                              LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                              string msg = "Feature Guid: " + solutionGuid + " has already been activated.";
                              objErr.sendUserEmail(APP_NAME, "Unable to activate feature", msg, false);
                              objErr = null;
                          }
                      }
                  }
                  destSite.Dispose();
              });

            }
            catch (Exception err)
            {               
               LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
               objErr.logSysErrorEmail(APP_NAME, err, "Error at ActivateFeatureSiteScope function. Unable to activate feature Guid: " + solutionGuid);
               objErr = null;

               SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "ActivateFeatureSiteScope-" + err.Message, err.StackTrace);
            }

            return isFeatureActivated;
        }


        /// <summary>
        ///     Function to deactivate a site feature
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="solutionGuid"></param>
        public void DeactivateSiteFeature(string spSiteUrl, string solutionGuid)
        {
            try
            {
                 SPSecurity.RunWithElevatedPrivileges(delegate()
                 {
                     using (SPSite destSite = new SPSite(spSiteUrl))
                     {

                         Guid featureGuid = new Guid(solutionGuid);

                         if (destSite.Features[featureGuid] != null)
                         {
                             destSite.AllowUnsafeUpdates = true;
                             destSite.Features.Remove(featureGuid, true);
                             destSite.AllowUnsafeUpdates = false;
                             Thread.Sleep(1000); //Give time for or SharePoint to apply change
                         }
                         else
                         {
                             //send email
                             LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                             string msg = "Feature Guid: " + solutionGuid + " does not exist for deactivation.";
                             objErr.sendUserEmail(APP_NAME, "Unable to deactivate site feature", msg, false);
                             objErr = null;
                         }
                     }
                 });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DeactivateSiteFeature function. Unable to deactivate feature Guid: " + solutionGuid);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "DeactivateSiteFeature-" + err.Message, err.StackTrace);
            }
        }

        /// <summary>
        ///     Function to deactivate a web feature
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="solutionGuid"></param>
        public void DeactivateWebFeature(string spSiteUrl, string solutionGuid)
        {
            try
            {
                 SPSecurity.RunWithElevatedPrivileges(delegate()
                 {
                     using (SPSite destSite = new SPSite(spSiteUrl))
                     {
                         using (SPWeb destWeb = destSite.OpenWeb())
                         {
                             Guid featureGuid = new Guid(solutionGuid);

                             if (destWeb.Features[featureGuid] != null)
                             {
                                 destWeb.AllowUnsafeUpdates = true;
                                 destWeb.Features.Remove(featureGuid, true);
                                 destWeb.Update();
                                 Thread.Sleep(1000); //Give time for or SharePoint to apply change
                                 destWeb.AllowUnsafeUpdates = false;
                             }
                             else
                             {
                                 //send email
                                 LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                                 string msg = "Feature Guid: " + solutionGuid + " does not exist for deactivation.";
                                 objErr.sendUserEmail(APP_NAME, "Unable to deactivate web feature", msg, false);
                                 objErr = null;
                             }
                         }
                     }
                 });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DeactivateWebFeature function. Unable to deactivate feature Guid: " + solutionGuid);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "DeactivateWebFeature-" + err.Message, err.StackTrace);
            }
        }

        /// <summary>
        ///     Apply the a site template to the site collection
        /// </summary>
        /// <param name="siteURL"></param>
        /// <param name="siteTemplateName"></param>
        public void ApplySiteTemplate(string siteURL, string siteTemplateName)
        {
            try
            {
                using (SPSite destSite = new SPSite(siteURL))
                {
                    SPWeb destWeb = destSite.RootWeb;

                    // Get Web Template
                    SPWebTemplateCollection webTemplates = destSite.RootWeb.GetAvailableWebTemplates(1033);
                    SPWebTemplate webTemplate = (from SPWebTemplate t in webTemplates
                                                 where t.Name == siteTemplateName
                                                 select t).FirstOrDefault();
                    if (webTemplate != null)
                    {
                        destSite.RootWeb.ApplyWebTemplate(webTemplate.Name);
                        Thread.Sleep(2000); //Give time for or SharePoint to apply change
                    }
                    else
                    {
                        LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                        string msg = "Applying template: " + siteTemplateName + " was not successful!";
                        objErr.sendUserEmail(APP_NAME, "Unable to apply site template", msg, false);
                        objErr = null;
                    }

                    destWeb.Dispose();
                  
                }
            }
            catch (Exception err)
            {
              
               LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
               objErr.logSysErrorEmail(APP_NAME, err, "Error at ApplySiteTemplate function");
               objErr = null;
               
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "btnCreateSiteCollection_Click-" + err.Message, err.StackTrace);
            }
        }

        /// <summary>
        ///     Activate a solution feauture
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="site"></param>
        private void EnsureSiteCollectionFeaturesActivated(SPUserSolution solution, SPSite site)
        {
            try
            {
                SPUserSolutionCollection solutions = site.Solutions;
                List<SPFeatureDefinition> oListofFeatures = GetFeatureDefinitionsInSolution(solution, site);
                foreach (SPFeatureDefinition def in oListofFeatures)
                {
                    if (((def.Scope == SPFeatureScope.Site) && def.ActivateOnDefault) && (site.Features[def.Id] == null))
                    {
                        site.Features.Add(def.Id, false, SPFeatureDefinitionScope.Site);
                    }
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at EnsureSiteCollectionFeaturesActivated function.");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
        }


        /// <summary>
        ///     Get feature definition
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        private List<SPFeatureDefinition> GetFeatureDefinitionsInSolution(SPUserSolution solution, SPSite site)
        {
            List<SPFeatureDefinition> list = new List<SPFeatureDefinition>();
            foreach (SPFeatureDefinition definition in site.FeatureDefinitions)
            {
                if (definition.SolutionId.Equals(solution.SolutionId))
                {
                    list.Add(definition);
                }
            }
            return list;
        }


        /// <summary>
        ///     Check if a feature is already activated
        /// </summary>
        /// <param name="web"></param>
        /// <param name="featureId"></param>
        /// <returns>Return true if feature is activated</returns>
        public bool IsFeatureActivated(SPWeb web, Guid featureId)
        {
            return web.Features[featureId] != null;
        }

        /// <summary>
        ///     Check if feature is alrady installed
        /// </summary>
        /// <param name="web"></param>
        /// <param name="featureId"></param>
        /// <returns></returns>
        public bool IsFeatureInstalled(SPWeb web, Guid featureId)
        {
            SPFeatureDefinition featureDefinition = SPFarm.Local.FeatureDefinitions[featureId];
            if (featureDefinition == null)
            {
                return false;
            }

            if (featureDefinition.Scope != SPFeatureScope.Web)
            {              
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.sendUserEmail(APP_NAME, "Error at IsFeatureInstalled function.", string.Format("Feature with the ID {0} was installed but is not scoped at the web level.", featureId), true);
                objErr = null;
            }

            return true;
        }

        #endregion

        #region Document libriary methods


        /// <summary>
        ///     Check if a document ID exist in a particular list
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="itemID"></param>
        /// <returns>
        ///     Returns the item ID if it is found. Otherwise returns empty string
        /// </returns>
        public string isDocIdExit(string listName, string itemID)
        {
            string isItemExist = "";

            try
            {
                SPList spList = _spCurrentWeb.Lists[listName];
                SPQuery spQuery = new SPQuery();
                spQuery.Query = @"<Where><Eq><FieldRef Name='DocID'/><Value Type='Text'>" + itemID + "</Value></Eq></Where>";
                spQuery.ViewFields = String.Concat("<FieldRef Name='DocID'/>");
                spQuery.ViewFieldsOnly = true;
                spQuery.RowLimit = 1;
                SPListItemCollection spListItems = spList.GetItems(spQuery);
                if (spListItems != null)
                {
                    if (spListItems.Count > 0)
                    {
                        foreach (SPListItem spListItem in spListItems)
                        {
                            isItemExist = spListItem["ID"].ToString() ; //Item is found
                        }
                    }
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at isDocIdExit function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return isItemExist;
        }

        /// <summary>
        ///     Function to check if a list already exist
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="currentWeb"></param>
        /// <returns></returns>
        public bool isListExist(string listName, SPWeb currentWeb)
        {
            bool listExist = false;

            try
            {
                SPList list = currentWeb.Lists.TryGetList(listName);

                if (list != null)
                    listExist = true;
                
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at isListExist function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return listExist;
        }

        /// <summary>
        ///     Function determine if an item ID already exist in the list
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="listName"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public bool isItemExist(string listName, int itemID)
        {
            bool isItemExist = false;

            try
            {
                    SPList spList = _spCurrentWeb.Lists[listName];
                    SPQuery spQuery = new SPQuery();
                    spQuery.Query = @"<Where><Eq><FieldRef Name='ID'/><Value Type='Counter'>" + itemID + "</Value></Eq></Where>";
                    spQuery.ViewFields = String.Concat("<FieldRef Name='ID'/>");
                    spQuery.ViewFieldsOnly = true;
                    spQuery.RowLimit = 1;
                    SPListItemCollection spListItems = spList.GetItems(spQuery);
                    if (spListItems != null)
                    {
                        if (spListItems.Count > 0)
                            isItemExist = true; //Item is found
                    }
                
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at isItemExist function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return isItemExist;

        }

         /// <summary>
        ///     Get the document library GUID
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="docLibName"></param>
        /// <returns>GUID</returns>
        public Guid GetDocLibraryGuid(string spSiteUrl, string docLibName)
        {
            Guid docLibGuid = new Guid();
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPFolder folder = web.Folders[docLibName];
                            docLibGuid = folder.UniqueId;
                        }

                    }

                });

                return docLibGuid;
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetDocLibraryGuid function.");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return docLibGuid;
            }
        }


        /// <summary>
        ///     Creates a new folder inside an existing document library. This method can also be used to create a new sub folder inside an existing root folder
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="docLibraryName"></param>
        /// <param name="newFolderTitle"></param>
        public void CreateNewFolder(string spSiteUrl, string docLibraryName, string newFolderTitle)
        {

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {//Executes the specified method with Full Control rights even if the user does not otherwise have Full Control.

                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            SPList docLib = web.Lists[docLibraryName]; //Get the document library
                            SPListItem folder = docLib.Folders.Add(docLib.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, newFolderTitle);
                            folder.Update();
                            Thread.Sleep(1000);
                        }

                    }

                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at CreateNewFolder function.Cannot create new folder " + newFolderTitle + " in " + docLibraryName);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

        }

        /// <summary>
        ///     Delete a particular document from a sub folder in a document library
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="destDocLibName"></param>
        /// <param name="destFolderName"></param>
        /// <param name="deleteFileName"></param>
        /// <returns>True if file was successfully deleted. Otherwise returns false</returns>
        public bool DeleteFileInDocumentLibrary(string spSiteUrl, string destDocLibName, string destFolderName, string deleteFileName)
        {
            bool isFileDeleted = false;

            try
            {

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            string fileName = Path.GetFileName(deleteFileName);
                            SPFolder folder = web.GetFolder(destDocLibName + "/" + destFolderName);
                            SPFileCollection files = folder.Files;

                            web.AllowUnsafeUpdates = true;
                            for (int i = 0; i < files.Count; i++)
                            {
                                SPFile tempFile = files[i];

                                if (tempFile.Name.ToLower() == fileName.ToLower())
                                {
                                    folder.Files.Delete(tempFile.Url);
                                    folder.Update();
                                    isFileDeleted = true;
                                }
                            }
                            web.AllowUnsafeUpdates = false;
                        }
                    }

                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DeleteFileInDocumentLibrary function. Cannot delete " + deleteFileName + " at " + destDocLibName + "/" + destFolderName);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isFileDeleted;
        }

        /// <summary>
        ///   Delete a particular document from a document library
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="destDocLibName"></param>
        /// <param name="deleteFileName"></param>
        /// <returns></returns>
        public bool DeleteFileInRootDocumentLibrary(string spSiteUrl, string destDocLibName, string deleteFileName)
        {
            bool isFileDeleted = false;

            try
            {

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(spSiteUrl))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            string fileName = Path.GetFileName(deleteFileName);
                            SPFolder folder = web.GetFolder(destDocLibName);
                            SPFileCollection files = folder.Files;

                            web.AllowUnsafeUpdates = true;
                            for (int i = 0; i < files.Count; i++)
                            {
                                SPFile tempFile = files[i];

                                if (tempFile.Name.ToLower() == fileName.ToLower())
                                {
                                    folder.Files.Delete(tempFile.Url);
                                    folder.Update();
                                    isFileDeleted = true;
                                }
                            }
                            web.AllowUnsafeUpdates = false;
                        }
                    }

                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at DeleteFileInRootDocumentLibrary function. Cannot delete " + deleteFileName + " at " + destDocLibName);
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isFileDeleted;
        }


        #endregion

        #region Databaes methods

        /// <summary>
        ///     Function to determine the database naming convention
        /// </summary>
        /// <param name="spHelper"></param>
        /// <param name="prefxDBName"></param>
        /// <param name="currentUrl"></param>
        /// <param name="libraryName"></param>
        /// <param name="itemID"></param>
        /// <param name="projectNumber"></param>
        /// <returns></returns>
        public string getDBNamingConvention(SPWebApplication webApp, string prefixDBName, string currentUrl, string libraryName, int itemID, string projectNumber, SPWeb currentWeb, int MAX_SITE_COLLECTION)
        {
            string DBName = "";
            try
            {
                string projSize = ReadFromInfoPathForm(currentUrl, libraryName, itemID, "my:EstimatedProjectSize");

                if ((projSize.Length <= 0) || (projSize == string.Empty))
                    projSize = "small";

                int configMaxSiteCollection = 0;
                string categoryName = "DBSmallMaxSite";          
                StringBuilder sb = new StringBuilder();
                sb.Append(prefixDBName);

                switch (projSize.ToLower())
                {
                    case "small":
                        configMaxSiteCollection = Convert.ToInt32(GetRCRSettingsItem(categoryName, _spSettingList));
                        sb.Append("Small");

                        //Get the maximum no. site collection from DB. The source of truth will always be from the list settings
                        MAX_SITE_COLLECTION = getMaxSiteCollection(currentUrl, sb.ToString());

                        if (configMaxSiteCollection != MAX_SITE_COLLECTION)
                        {
                            MAX_SITE_COLLECTION = configMaxSiteCollection;
                        }

                        DBName = getDBSuffixName(webApp, sb.ToString(), MAX_SITE_COLLECTION, currentUrl);
                        break;

                    case "medium":
                        categoryName = "DBMediumMaxSite";
                        configMaxSiteCollection = Convert.ToInt32(GetRCRSettingsItem(categoryName, _spSettingList));
                        sb.Append("Medium");

                        //Get the maximum no. site collection from DB. The source of truth will always be from the list settings
                        MAX_SITE_COLLECTION = getMaxSiteCollection(currentUrl, sb.ToString());

                        if (configMaxSiteCollection != MAX_SITE_COLLECTION)
                        {
                            MAX_SITE_COLLECTION = configMaxSiteCollection;
                        }

                        DBName = getDBSuffixName(webApp, sb.ToString(), MAX_SITE_COLLECTION, currentUrl);
                        break;

                    default:
                        categoryName = "DBLargeMaxSite";
                        configMaxSiteCollection = Convert.ToInt32(GetRCRSettingsItem(categoryName, _spSettingList));
                        sb.Append("Large");

                        //Get the maximum no. site collection from DB. The source of truth will always be from the list settings
                        MAX_SITE_COLLECTION = getMaxSiteCollection(currentUrl, sb.ToString());

                        if (configMaxSiteCollection != MAX_SITE_COLLECTION)
                        {
                            MAX_SITE_COLLECTION = configMaxSiteCollection;
                        }
                        DBName = prefixDBName + projectNumber;
                        break;
                }

            }
            catch (Exception ex)
            {
                
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message.ToString(), ex.StackTrace);
            }

            return DBName;

        }

        /// <summary>
        ///     Remove integer from character string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string removeIntegerChars(string input)
        {
            String result = Regex.Replace(input, @"[0-9]", String.Empty);

            return result;
        }

        /// <summary>
        ///     Function to get DB suffix name
        /// </summary>
        /// <param name="DBName"></param>
        /// <returns></returns>
        private string getSuffixDBName(string DBName)
        {
            string DBPrefixName = GetRCRSettingsItem("PrefixProjHubDBName", _spSettingList).ToString();
            int indexSuffixDBName = DBPrefixName.Length;
            string suffixDBName = DBName.Substring(indexSuffixDBName, DBName.Length - indexSuffixDBName);

            return suffixDBName;
        }

        /// <summary>
        ///     Function to get suffix DB name by incrementing number 
        /// </summary>
        /// <param name="spHelper"></param>
        /// <param name="DBName"></param>
        /// <returns></returns>
        public string getDBSuffixName(SPWebApplication webApp, string DBName, int maxSiteCount, string spSite)
        {          
            string retDBName = DBName + "1"; //Get actual DB name
           
            try
            {

                bool isDBExist = chkSPContentDBNameExist(webApp, retDBName);   //Check if content DB name already exist in SP farm
                int currentSiteCount = getCurrentSiteCount(spSite, DBName);

                if (currentSiteCount == 0)
                {//New database if no no. of current site exist
                    return retDBName;
                }
                else if (currentSiteCount < maxSiteCount) //if curent number of site collection is less than maximum number of site collection
                { //Do NOT create a new DB. Use existing DB

                    using (SPSite currentSite = new SPSite(spSite))
                    {
                        //for (int i = 1; i <= maxSiteCount; i++)                      
                            retDBName = DBName;
                            string suffixDBName = removeIntegerChars(getSuffixDBName(retDBName));          //Get suffix DB name                          
                            int indexDB = getCurrentDBIndex(suffixDBName, currentSite); //Get index of current DB

                            if (indexDB <= 0)
                            {//If DB index was found
                                retDBName = "";
                            }
                            else
                            {
                                SPContentDatabaseCollection contentDbs = webApp.ContentDatabases;//Get all content database
                                SPContentDatabase lastDatabase = contentDbs[indexDB]; //Find the current DB
                                retDBName = lastDatabase.Name;
                            }
                        
                    }
                }
                else
                {//Create new DB if current no. site collection >= max no. site collection
                    StringBuilder emailMsg = new StringBuilder();
                    LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);

                    //Allow incremental suffix number for DB name
                    if (isDBExist == false)
                    {//if content DB name does not exist then ok to use the name
                        return retDBName;
                    }
                    else
                    {//Increment suffix number if previous number already exist
                       

                        for (int i = 2; i <= maxSiteCount; i++)
                        {
                            retDBName = DBName + i.ToString();
                            //cmdText = "select * from master.dbo.sysdatabases where name=\'" + retDBName + "\'";
                            isDBExist = chkSPContentDBNameExist(webApp, retDBName);  //spHelper.checkSPContentDataBaseNameExist(connString, cmdText);
                            if (isDBExist == false)
                            {
                                return retDBName;
                            }

                            if (i == maxSiteCount)
                            {//If DB naming convention has reached maxinum number of site collection limit

                                //Send email notification                               
                                emailMsg.Append("<p>The Database naming convention has reached the maximum limit number of site collection below.</p>");
                                emailMsg.AppendFormat("<b>Database name: {0}</b><br>", retDBName);
                                emailMsg.AppendFormat("<b>Current Number of site collection:</b> {0}<br>", currentSiteCount);
                                emailMsg.AppendFormat("<b>Maximum Number of site collection:</b> {0}<br>", maxSiteCount);
                                objErr.sendUserEmail(APP_NAME, "Warning: Database Name Limit", emailMsg.ToString(), true);

                                return "";
                            }
                        }
                    }

                    //Send email notification
                    emailMsg.Append("<p>The current number of site collection has reached the maximum limit number of site collection below.</p>");
                    emailMsg.AppendFormat("<b>Current Number of site collection:</b> {0}<br>", currentSiteCount);
                    emailMsg.AppendFormat("<b>Maximum Number of site collection:</b> {0}<br>", maxSiteCount);
                    objErr.sendUserEmail(APP_NAME, "Warning: Site Collection Limit", emailMsg.ToString(), true);

                    //use current database name
                    retDBName = getCurrentDBName(spSite);
                    objErr = null;
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getDBSuffix function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return retDBName;
        }

        /// <summary>
        ///     RERERENCE: http://blog.sharedove.com/adisjugo/index.php/2012/07/31/creating-site-collections-in-specific-content-database/
        /// </summary>
        /// <param name="currentRootSite"></param>
        /// <param name="DBServer"></param>
        /// <param name="newDBName"></param>
        /// <param name="maxSiteCount"></param>
        /// <param name="userName">If empty string was passed then Windows authentication is used by default </param>
        /// <param name="password">If empty string was passed then Windows authentication is used by default</param>
        /// <returns></returns>
        public SPContentDatabase CreateNewContentDB(SPSite currentRootSite, string DBServer, string newDBName, int maxSiteCount, int warningSiteCount)
        {
            SPContentDatabase newContentDatabase = null;
            string suffixDBName = getSuffixDBName(newDBName);          //Get suffix DB name

            try
            {              
                bool createNewDb = false;
                int currentSiteCount = getCurrentSiteCount(currentRootSite.Url, newDBName);
                bool isDBExist = chkSPContentDBNameExist(currentRootSite.WebApplication, newDBName);   //Check if content DB name already exist in SP farm

                if (isDBExist == false) //Create new DB if new DB name does not exist
                {
                    //SPContentDatabase lastDatabase = null;
                    //Step 1: Get all content databases
                    //SPContentDatabaseCollection contentDbs = currentRootSite.WebApplication.ContentDatabases;

                    //int indexDB = getCurrentDBIndex(suffixDBName, currentRootSite);

                    //    //Step 2: Get the status of last working DB
                    //    if (indexDB <= 0)
                    //    {//If DB index was found
                    //        indexDB = 0;
                    //    }
                    //    else
                    //    {
                    //       lastDatabase = contentDbs[indexDB];
                    //    }

                    //if ((currentSiteCount >= lastDatabase.MaximumSiteCount) || (currentSiteCount == 0))
                    if ((currentSiteCount >= maxSiteCount) || (currentSiteCount == 0))
                    {
                        createNewDb = true;
                    }

                    //Step 3: Creat new DB
                    if (createNewDb)
                    {
                        SecurityContext spSecurity = SecurityContext.Capture();
                        SecurityContext.Run(spSecurity, delegate
                        {
                            SPSecurity.RunWithElevatedPrivileges(delegate()
                            {
                                SPUtility.ValidateFormDigest();
                                int status = 0; //0= read; 1=offline
                                int warningSiteCollectionNumber = 0;
                                if (currentSiteCount <= 0)
                                {
                                    if (warningSiteCount < maxSiteCount)
                                        warningSiteCollectionNumber = maxSiteCount - warningSiteCount;

                                }
                                else
                                {
                                    if (warningSiteCount < maxSiteCount)
                                        warningSiteCollectionNumber = maxSiteCount - warningSiteCount; //lastDatabase.MaximumSiteCount - warningSiteCount;
                                }

                                newContentDatabase = currentRootSite.WebApplication.ContentDatabases.Add(DBServer, newDBName, null, null, warningSiteCollectionNumber, maxSiteCount, status);
                                newContentDatabase.Update();
                                currentRootSite.WebApplication.Update();
                            });
                        }, null);
                    }
                }
                else
                {//Use existing content DB
                    //Step 1: Get all content databases
                    SPContentDatabaseCollection contentDbs = currentRootSite.WebApplication.ContentDatabases;
                    suffixDBName = removeIntegerChars(suffixDBName);
                    int indexDB = getCurrentDBIndex(suffixDBName, currentRootSite);
                   
                    //Step 2: Get the status of current working DB index
                    if (indexDB <= 0)
                    {
                        newContentDatabase = contentDbs[0];
                    }
                    else
                    {
                        newContentDatabase = contentDbs[indexDB];
                    }
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at createNewContentDB function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }


            return newContentDatabase;
        }


        /// <summary>
        ///     Function search for the content DB name
        /// </summary>
        /// <param name="webApp"></param>
        /// <param name="DBName"></param>
        /// <returns></returns>
        public bool chkSPContentDBNameExist(SPWebApplication webApp, string DBName)
        {
            bool bRet = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //Get list of all existing content database
                    SPContentDatabaseCollection contentDbs = webApp.ContentDatabases;

                    foreach (SPContentDatabase contentDB in contentDbs)
                    {
                        if (contentDB.Name.ToLower() == DBName.ToLower())
                            bRet = true;
                    }
                });
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at chkSPContentDBNameExist function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return bRet;
        }

        /// <summary>
        ///     Function to determine if a content database name exist on the SharePoint farm server
        /// </summary>
        /// <param name="connString">e.g. server=localhost; database=master; uid=projecthubsa; password=P@ssword; </param>
        /// <param name="cmdText">e.g. select * from master.dbo.sysdatabases where name=\'DBName here\'"</param>
        /// <returns></returns>
        public bool checkSPContentDataBaseNameExist(string connString, string cmdText)
        {
            bool bRet = false;

            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connString))
                    {

                        sqlConnection.Open();

                        using (SqlCommand sqlCmd = new SqlCommand(cmdText, sqlConnection))
                        {
                            //int nRet = sqlCmd.ExecuteNonQuery();
                            //if (nRet <= 0)
                            //{
                            //     bRet = false;
                            // }
                            // else
                            // {
                            //     bRet = true;
                            // }

                            SqlDataReader reader = sqlCmd.ExecuteReader();
                            bRet = reader.HasRows;
                            reader.Close();
                        }

                        sqlConnection.Close();
                    }


                });

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at checkSPContentDatabaseNameExist function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return bRet;

        }

        #endregion

        #region Admin methods

  
        /// <summary>
        ///     Check if sub site exists at given site collection
        /// </summary>
        /// <remarks>
        ///     REFERENCE: http://nikpatel.net/2011/11/30/code-snippet-how-to-check-if-sharepoint-site-collection-or-sub-site-exists/
        /// </remarks>
        /// <param name="spSite"></param>
        /// <param name="webRelativeUrl"></param>
        /// <returns></returns>
        public bool isWebExists(SPSite spSite, string webRelativeUrl)
        {
            bool returnVal = false;
            using (SPWeb currentWeb = spSite.OpenWeb(webRelativeUrl, true))
            {
                returnVal = currentWeb.Exists;
            }
            return returnVal;
        }

        /// <summary>
        ///     Check if site collection exists at given web application
        /// </summary>
        /// <param name="spWebApp"></param>
        /// <param name="siteCollectionRelativeUrl"></param>
        /// <returns></returns>
        public bool isSiteCollectionExists(SPWebApplication spWebApp, string siteCollectionRelativeUrl)
        {
            bool returnVal = false;
            string webAppURL = string.Empty;
            foreach (SPAlternateUrl spWebAppAlternateURL in spWebApp.AlternateUrls)
            {
                if (spWebAppAlternateURL.UrlZone == SPUrlZone.Default)
                {
                    webAppURL = spWebAppAlternateURL.Uri.AbsoluteUri;
                }
            }

            if (webAppURL.ToString().Length != 0)
            {
                Uri siteCollectionUri = new Uri(webAppURL + siteCollectionRelativeUrl);
                returnVal = SPSite.Exists(siteCollectionUri);
            }
            return returnVal;
        }

        /// <summary>
        ///     Function to get the last check in comments for a particular document item
        ///     REFERENCE: http://www.learningsharepoint.com/2010/09/05/programmatically-get-versions-for-files-in-sharepoint-2010-document-library/
        /// </summary>
        /// <param name="currentWeb"></param>
        /// <param name="listName"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public string getLastCheckInComment(SPWeb currentWeb, string listName,  int itemID)
        {
            string checkInComment = "";

            try
            {
                SPList docs = currentWeb.Lists[listName];
                SPFile file = docs.GetItemById(itemID).File;

                foreach (SPFileVersion v in file.Versions)
                {
                    checkInComment = v.CheckInComment;
                }
                
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getLastCheckInComment function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return checkInComment;
        }

        /// <summary>
        ///     Function to determine if a file is currently checked out
        /// </summary>
        /// <param name="listItem"></param>
        /// <returns></returns>
        public bool isFileCheckOut(SPListItem listItem)
        {
            try
            {
                if (listItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                {//If file is already check out by someone else
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at isFileCheckOut function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return false;
            }
        }

        /// <summary>
        ///     Function to detect if site exist
        /// </summary>
        /// <param name="siteURL"></param>
        /// <returns></returns>
        public bool isSiteExist(string siteURL)
        {
            bool isSiteFound = false;

            try
            {
                //using (SPSite site = new SPSite(siteURL))
                //{
                //    isSiteFound = true;
                //}
                Uri siteCollectionUri = new Uri(siteURL);
                isSiteFound = SPSite.Exists(siteCollectionUri);
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at isSiteExist function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return isSiteFound;
        }

        /// <summary>
        ///     Function to get the maxinum number of site collection value from setting list
        /// </summary>
        /// <param name="spHelper"></param>
        /// <param name="currentUrl"></param>
        /// <param name="libraryName"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public int getMaximumSiteCountSetting(string currentUrl, string libraryName, int itemID)
        {
            int configMaxSiteCollection = 0;
            string projSize = ReadFromInfoPathForm(currentUrl, libraryName, itemID, "my:EstimatedProjectSize");
            string category = "DBSmallMaxSite";

            if ((projSize.Length <= 0) || (projSize == string.Empty))
                projSize = "small";

            switch (projSize.ToLower())
            {
                case "small":
                    configMaxSiteCollection = Convert.ToInt32(GetRCRSettingsItem(category,_spSettingList));
                    break;

                case "medium":
                    category = "DBMediumMaxSite";
                    configMaxSiteCollection = Convert.ToInt32(GetRCRSettingsItem(category, _spSettingList));
                    break;

                default:
                    category = "DBLargeMaxSite";
                    configMaxSiteCollection = Convert.ToInt32(GetRCRSettingsItem(category, _spSettingList));
                    break;
            }

            return configMaxSiteCollection;
        }

        /// <summary>
        ///     Function to get maximum site setting from custom list setting
        /// </summary>
        /// <param name="suffixName"></param>
        /// <returns></returns>
        private int getMaxSiteSettings(string suffixName)
        {
            int maxSite = 0;

            try
            {
                switch (suffixName.ToLower())
                {
                    case "small":
                        maxSite = Convert.ToInt32(GetRCRSettingsItem("DBSmallMaxSite", _spSettingList).ToString());     
                        break;

                    case "medium":
                        maxSite = Convert.ToInt32(GetRCRSettingsItem("DBMediumMaxSite", _spSettingList).ToString());
                        break;

                    default:
                        maxSite = Convert.ToInt32(GetRCRSettingsItem("DBLargeMaxSite", _spSettingList).ToString());    
                        break;
                }
            }
             catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getMaxSiteSettings function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return maxSite;

        }

        /// <summary>
        ///     Function to get the maximum number of site collection
        /// </summary>
        /// <param name="siteURL"></param>
        /// <returns></returns>
        public int getMaxSiteCollection(string siteURL, string DBName)
        {
            int maxSiteCount = 0;

            try
            {
                using (SPSite currentSite = new SPSite(siteURL))
                {
                    string suffixDBName = getSuffixDBName(DBName);          //Get suffix DB name

                    //Step 1: Get all content databases
                    SPContentDatabaseCollection contentDbs = currentSite.WebApplication.ContentDatabases;
                    int indexDB = getCurrentDBIndex(suffixDBName, currentSite);

                    //Step 2: Get the status of last working DB
                    SPContentDatabase lastDatabase = contentDbs[indexDB];

                    maxSiteCount = lastDatabase.MaximumSiteCount;
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getMaxSiteCollection function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return maxSiteCount;
        }

        /// <summary>
        ///     Function to get the DB name of the site
        /// </summary>
        /// <param name="siteURL"></param>
        /// <returns></returns>
        public string getCurrentDBName(string siteURL)
        {
            string currentDBName = "";

            try
            {
                using (SPSite currentSite = new SPSite(siteURL))
                {
                    currentDBName = currentSite.ContentDatabase.Name;
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getCurrentDBName function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return currentDBName;
        }

        /// <summary>
        ///     Function to search for DB name and get its DB index
        ///     If no DB name was found, always return 0
        /// </summary>
        /// <param name="suffixName"></param>
        /// <param name="siteURL"></param>
        /// <returns></returns>
        private int getCurrentDBIndex(string suffixName, SPSite currentSite)
        {
            int counter = 0;
            bool isDBNameExist = false;

            try
            {
                //using (SPSite currentSite = new SPSite(siteURL))
                {
                    //Step 1: Get prefix DB name
                    string DBPrefixName = GetRCRSettingsItem("PrefixProjHubDBName", _spSettingList).ToString();

                    //Step 2: Get all content databases
                    SPContentDatabaseCollection contentDbs = currentSite.WebApplication.ContentDatabases;

                    //Step 3: Iterate through eacb content DB
                    if (contentDbs.Count <= 0)
                    {
                        return counter;
                    }
                    else
                    {
                        string DBName = DBPrefixName + suffixName; 
                        foreach (SPContentDatabase contentDB in contentDbs)
                        {
                            if (contentDB.Name.ToLower().Contains(DBName.ToLower()))
                            {
                                isDBNameExist = true;
                                return getMatchContentDBIndex(suffixName, DBName, contentDbs);  //counter;
                            }
                            counter++;
                        }
                    }

                    //Check for out of bound array
                    //if (counter > contentDbs.Count)
                    //{
                    //    int indexDB = contentDbs.Count-1;
                    //    return indexDB;
                    //}

                    if (isDBNameExist == false)
                        counter = 0;

                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getCurrentDBIndex function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return counter;
        }

        /// <summary>
        ///     Function to get the actual DB index for a search DB name
        /// </summary>
        /// <param name="searchDBName"></param>
        /// <param name="contentDBs"></param>
        /// <returns></returns>
        private int getMatchContentDBIndex(string suffixName, string searchDBName, SPContentDatabaseCollection contentDBs)
        {
            int indexDB = 0;
            int maxDBCount = contentDBs.Count;

            try
            {
                int maxSiteSetting = getMaxSiteSettings(suffixName);

                for (int i = 1; i <= maxDBCount; i++)
                {
                    string DBName = searchDBName + i.ToString();

                    foreach (SPContentDatabase contentDB in contentDBs)
                    {
                        if (contentDB.Name.ToLower() == DBName.ToLower())
                        {
                            if (contentDB.CurrentSiteCount < maxSiteSetting)
                                return indexDB;
                        }
                        indexDB++;                
                    }
                    indexDB = 0;
                }

                //Check for out of bound array index
                if (indexDB > maxDBCount)
                    indexDB = maxDBCount - 1;

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getMatchContentDBIndex function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return indexDB-1;
        }
        /// <summary>
        ///     Function to get the current number of site collection
        /// </summary>
        /// <param name="siteURL"></param>
        /// <returns></returns>
        public int getCurrentSiteCount(string siteURL, string DBName)
        {
            int currentSiteCount = 0;
            string suffixDBName = getSuffixDBName(DBName);          //Get suffix DB name

            try
            {
                using (SPSite currentSite = new SPSite(siteURL))
                {
                    //Step 1: Get all content databases
                    SPContentDatabaseCollection contentDbs = currentSite.WebApplication.ContentDatabases;
                    int indexDB = getCurrentDBIndex(suffixDBName, currentSite);
                   
                    //Step 2: Get the status of current working DB index
                    if (indexDB <= 0)
                    {//If DB index was found
                        currentSiteCount = 0;
                    }
                    else
                    {
                        SPContentDatabase lastDatabase = contentDbs[indexDB];
                        currentSiteCount = lastDatabase.CurrentSiteCount;
                    }

                    
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(_spSettingList, _spCurrentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getCurrentSiteCount function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return currentSiteCount;
        }

        #endregion

    }
}
