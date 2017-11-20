using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Microsoft.Office.Server;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;

using RCR.SP.Framework.Helper.LogError;

namespace RCR.SP.Framework.Helper.UserProfileServices
{
    public class UserProfileHelper
    {
        #region constants

        private const string APP_NAME = "UserProfileHelper Class";
        private const string LIST_SETTING_NAME = "User Profile Settings";

        #endregion

        #region Member properties

        private List<string> upFieldName;
        private List<string> upFieldValue;

        private string _accountName = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName =  string.Empty;
        private string _employeeID = string.Empty;
        private string _deptNum = string.Empty;
        private string _division = string.Empty;
        private string _ipPhone = string.Empty;
        private string _workPhone = string.Empty;
        private string _email = string.Empty;
        private string _manager = string.Empty;
        private string _companyName = string.Empty;
        private string _officeAddress = string.Empty;
        private string _jobTitle = string.Empty;
        private DateTime _startDate;

        public string AccountName
        {
            get { return this._accountName; }
            set { this._accountName = value; }
        }

        public string FirstName
        {
            get { return this._firstName; }
            set { this._firstName = value; }
        }

        public string LastName
        {
            get { return this._lastName; }
            set { this._lastName = value; }
        }

        public string JobTitle
        {
            get { return this._jobTitle; }
            set { this._jobTitle = value; }
        }

        public string EmployeeID
        {
            get { return this._employeeID; }
            set { this._employeeID = value; }
        }

        public string DepartmentNumber
        {
            get { return this._deptNum; }
            set { this._deptNum = value; }
        }

        public string Division
        {
            get { return this._division; }
            set { this._division = value; }
        }

        public string IPPhone
        {
            get { return this._ipPhone; }
            set { this._ipPhone = value; }
        }

        public string WorkPhone
        {
            get { return this._workPhone; }
            set { this._workPhone = value; }
        }

        public string emailAddress
        {
            get { return this._email; }
            set { this._email = value; }
        }

        public string Manager
        {
            get { return this._manager; }
            set { this._manager = value; }
        }

        public string CompanyName
        {
            get { return this._companyName; }
            set { this._companyName = value; }
        }

        
        public string OfficeAddress
        {
            get { return this._officeAddress; }
            set { this._officeAddress = value; }
        }

        public DateTime StartDate //TODO:- Find internal field name
        {
             get { return this._startDate; }
            set { this._startDate = value; }
        }

        #endregion

        #region Constructor

        public UserProfileHelper()
        {

        }

        
        #endregion

        #region Update methods

        /// <summary>
        ///     Update a particular user profile property field
        /// </summary>
        /// <param name="acctName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        public void updateProfileDetails(string acctName, string fieldName, string fieldValue, SPWeb currentWeb)
        {
            try
            {
                SPSite site = SPContext.Current.Site;
                SPServiceContext ospServerContext = SPServiceContext.GetContext(site);
                UserProfileManager ospUserProfileManager = new UserProfileManager(ospServerContext);

                UserProfile ospUserProfile = ospUserProfileManager.GetUserProfile(acctName);
                Microsoft.Office.Server.UserProfiles.PropertyCollection propColl = ospUserProfile.ProfileManager.PropertiesWithSection;

                if (ospUserProfile != null && propColl != null)
                {
                    foreach (Property prop in propColl)
                    {
                        if (fieldName == prop.Name)
                        {
                            ospUserProfile[prop.Name].Value = fieldValue;
                        }
                    }
                    ospUserProfile.Commit();
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserProfileDetails function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }
        }

        #endregion

        #region Read methods

        /// <summary>
        ///     Get selecte user information from people editor control
        /// </summary>
        /// <param name="people"></param>
        /// <param name="currentWeb"></param>
        /// <returns></returns>
        public SPFieldUserValue GetSingleUserFromPeopleEditor(PeopleEditor people, SPWeb currentWeb)
        {
            SPFieldUserValue userValue = null;

            try
            {
                if (people.ResolvedEntities.Count <= 1)
                {            
                        PickerEntity user = (PickerEntity)people.ResolvedEntities[0];

                        switch ((string)user.EntityData["PrincipalType"])
                        {
                            case "User":
                                SPUser webUser = currentWeb.EnsureUser(user.Key);
                                userValue = new SPFieldUserValue(currentWeb, webUser.ID, webUser.Name);
                                break;

                            case "SharePointGroup":
                                SPGroup siteGroup = currentWeb.SiteGroups[user.EntityData["AccountName"].ToString()];
                                userValue = new SPFieldUserValue(currentWeb, siteGroup.ID, siteGroup.Name);
                                break;
                        }          
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetSingleUserFromPeopleEditor function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            return userValue;
        }

        /// <summary>
        ///   Get multiple people information from people editor control
        /// </summary>
        /// <remarks>
        ///     References: http://kancharla-sharepoint.blogspot.com.au/2012/10/sometimes-we-need-to-get-all-people-or.html
        /// </remarks>
        /// <param name="people"></param>
        /// <param name="web"></param>
        /// <returns></returns>
        public SPFieldUserValueCollection GetPeopleFromPickerControl(PeopleEditor people, SPWeb web) 
        { 
            SPFieldUserValueCollection values = new SPFieldUserValueCollection();

            try
            {
                if (people.ResolvedEntities.Count > 0)
                {
                    for (int i = 0; i < people.ResolvedEntities.Count; i++)
                    {
                        PickerEntity user = (PickerEntity)people.ResolvedEntities[i];

                        switch ((string)user.EntityData["PrincipalType"])
                        {
                            case "User":
                                SPUser webUser = web.EnsureUser(user.Key);
                                SPFieldUserValue userValue = new SPFieldUserValue(web, webUser.ID, webUser.Name);
                                values.Add(userValue);
                                break;

                            case "SharePointGroup":
                                SPGroup siteGroup = web.SiteGroups[user.EntityData["AccountName"].ToString()];
                                SPFieldUserValue groupValue = new SPFieldUserValue(web, siteGroup.ID, siteGroup.Name);
                                values.Add(groupValue);
                                break;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, web);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at GetPeopleFromPickerControl function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return values; 
        }

        /// <summary>
        ///     Get the login user ID
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="currentWeb"></param>
        /// <returns></returns>
        public int getUserID(string userid, SPWeb currentWeb)
        {
            int userID = 0;

            try
            {
                SPFieldUserValue uservalue;
                SPUser requireduser = currentWeb.EnsureUser(userid);
                uservalue = new SPFieldUserValue(currentWeb, requireduser.ID, requireduser.LoginName);
                userID = uservalue.User.ID;
                //or use userID = currentWeb.AllUsers[userid].ID;
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserID function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

            return userID;
        }

        /// <summary>
        ///     Function to get user by login account
        /// </summary>
        /// <param name="currentURL"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public SPFieldUserValue ConvertLoginAccount(string currentURL, string userid)
        {
            SPFieldUserValue uservalue;
            using (SPSite site = new SPSite(currentURL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPUser requireduser = web.EnsureUser(userid);
                    uservalue = new SPFieldUserValue(web, requireduser.ID, requireduser.LoginName);
                }
            }
            return uservalue;
        }


        /// <summary>
        ///     Function get user value object
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="currentWeb"></param>
        /// <returns></returns>
        public SPFieldUserValue GetUserValue(string loginName, SPWeb currentWeb)
        {
            SPFieldUserValue fuv = null;

            try
            {
                if (!string.IsNullOrEmpty(loginName))
                {
                    SPUser user = currentWeb.SiteUsers[loginName];
                    fuv = new SPFieldUserValue(currentWeb, user.ID, user.LoginName);

                }
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(loginName))
                {
                    SPUser user2 = currentWeb.EnsureUser(loginName); 
                    fuv = new SPFieldUserValue(currentWeb, user2.ID, user2.LoginName);
                }
            }

            return fuv;
        }

        /// <summary>
        ///     Function to get user name
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="currentWeb"></param>
        /// <returns></returns>
        public string GetUserNameByAccountName(string loginName, SPWeb currentWeb)
        {
            string userName = "";

            try
            {
                if (!string.IsNullOrEmpty(loginName))
                {
                    SPUser user = currentWeb.SiteUsers[loginName];
                    SPFieldUserValue fuv = new SPFieldUserValue(currentWeb, user.ID, user.LoginName);
                    userName = fuv.User.Name;
                }
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(loginName))
                {
                    SPUser user2 = currentWeb.EnsureUser(loginName);
                    SPFieldUserValue fuv = new SPFieldUserValue(currentWeb, user2.ID, user2.LoginName);
                    userName = fuv.User.Name;
                }
            }

            return userName;
        }

        /// <summary>
        ///     Get multiple people information from people editor control
        /// </summary>
        /// <remarks>
        ///     References: http://blog.bugrapostaci.com/tag/spfielduservalue/   
        /// </remarks>
        /// <param name="editor"></param>
        /// <returns></returns>
        public SPFieldUserValueCollection GetSelectedUsers(PeopleEditor editor, SPWeb currentWeb)
        {
               string selectedUsers = editor.CommaSeparatedAccounts;
               SPFieldUserValueCollection values = new SPFieldUserValueCollection();

               try
               {
                   // commaseparatedaccounts returns entries that are comma separated. we want to split those up
                   char[] splitter = { ',' };
                   string[] splitPPData = selectedUsers.Split(splitter);
                   // this collection will store the user values from the people editor which we'll eventually use
                   // to populate the field in the list

                   // for each item in our array, create a new sp user object given the loginname and add to our collection
                   for (int i = 0; i < splitPPData.Length; i++)
                   {
                       string loginName = splitPPData[i];
                       if (!string.IsNullOrEmpty(loginName))
                       {
                           SPUser user = currentWeb.SiteUsers[loginName];
                           SPFieldUserValue fuv = new SPFieldUserValue(currentWeb, user.ID, user.LoginName);
                           values.Add(fuv);
                       }
                   }
               }
               catch (Exception err)
               {
                   LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                   objErr.logSysErrorEmail(APP_NAME, err, "Error at GetSelectedUsers function");
                   objErr = null;

                   SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
               }
                return values;
        }


        /// <summary>
        ///     Get user profile by ID
        ///     REFERENCE: http://waelmohamed.wordpress.com/2010/05/10/sharepoint-user-profiles-part-2-working-with-user-profile-with-sharepoint-object-model/
        /// </summary>
        /// <param name="byUserID"></param>
        /// <param name="currentWeb"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool getUserProfileDetailsById(long byUserID, SPWeb currentWeb, SPSite site)
        {
            try
            {             
                //SPServiceContext ospServerContext = SPServiceContext.GetContext(site);
                ServerContext ospServerContext = ServerContext.GetContext(site);
                UserProfileManager ospUserProfileManager = new UserProfileManager(ospServerContext);

                UserProfile ospUserProfile = ospUserProfileManager.GetUserProfile(byUserID);
                //Microsoft.Office.Server.UserProfiles.PropertyCollection propColl = ospUserProfile.ProfileManager.PropertiesWithSection;

                if (ospUserProfile != null)
                {
                    upFieldName = new List<string>();
                    upFieldValue = new List<string>();

                    //Search through all user profile property fields
                    foreach (Property prop in ospUserProfileManager.Properties)
                    {
                        //get user profile values collection
                        UserProfileValueCollection proValCol = ospUserProfile[prop.Name];
                        mapUserProfilePropertyValue(proValCol, prop, prop.Name, ospUserProfile[prop.Name].Value.ToString());
                    }

                    //Send email with any user profile fields not map to a custom list
                    reportUnManagedUserProfileProperties(currentWeb);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserProfileDetailsById function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return false;
            }
        }

        private bool getUserProfileDetailsById_BUG(long byUserID, SPWeb currentWeb, SPSite site)
        {
            try
            {
                //SPSite site = SPContext.Current.Site;
                SPServiceContext ospServerContext = SPServiceContext.GetContext(site);
                UserProfileManager ospUserProfileManager = new UserProfileManager(ospServerContext);

                UserProfile ospUserProfile = ospUserProfileManager.GetUserProfile(byUserID);
                Microsoft.Office.Server.UserProfiles.PropertyCollection propColl = ospUserProfile.ProfileManager.PropertiesWithSection;

                if (ospUserProfile != null && propColl != null)
                {
                    upFieldName = new List<string>();
                    upFieldValue = new List<string>();

                    //Search through all user profile property fields
                    foreach (Property prop in propColl)
                    {
                        //Console.WriteLine("property Name : " + prop.Name);                    
                        //Console.WriteLine("proerpty Value : " + ospUserProfile[prop.Name].Value);
                        UserProfileValueCollection proValCol = ospUserProfile[prop.Name];
                        mapUserProfilePropertyValue(proValCol, prop, prop.Name, ospUserProfile[prop.Name].Value.ToString());
                    }

                    //Send email with any user profile fields not map to a custom list
                    reportUnManagedUserProfileProperties(currentWeb);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserProfileDetailsById function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return false;
            }
        }

        /// <summary>
        ///     Get user account name by searching user profile by user display name
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="currentWeb"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public string getUserAccountName(string displayName, SPWeb currentWeb, SPSite site)
        {
            string acctName = "";

            try
            {
                //SPSite site = SPContext.Current.Site;
                ServerContext ospServerContext = ServerContext.GetContext(site);
                UserProfileManager ospUserProfileManager = new UserProfileManager(ospServerContext);

                if (ospUserProfileManager != null)
                {
                    upFieldName = new List<string>();
                    upFieldValue = new List<string>();

                    //Search through all user profile property fields
                    foreach (UserProfile user in ospUserProfileManager)
                    {
                        if (user.DisplayName.ToLower() == displayName.ToLower())
                        {

                            return user.MultiloginAccounts[0]; //Search user has been found
                        }
                    }
                       

                }
                

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserAccountName function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }

            return acctName;
        }

        /// <summary>
        ///     Method to retreive user profile by account name
        /// </summary>
        /// <reference>
        ///         1. http://firstblogofvarun.blogspot.com.au/2009/02/how-to-access-sharepoint-user-profile.html
        /// </reference>
        /// <param name="byPersonName"></param>
        public bool getUserProfileDetailsByAccountName(string byAccountName, SPWeb currentWeb, SPSite site)
        {
            try
            {               
                    //SPSite site = SPContext.Current.Site;
                    ServerContext ospServerContext = ServerContext.GetContext(site);
                    UserProfileManager ospUserProfileManager = new UserProfileManager(ospServerContext);

                    if (ospUserProfileManager != null)
                    {
                        upFieldName = new List<string>();
                        upFieldValue = new List<string>();

                        //Search through all user profile property fields
                        foreach (UserProfile user in ospUserProfileManager)
                        {
                            if (user.MultiloginAccounts[0].ToLower() == byAccountName.ToLower())
                            {
                                foreach (ProfileSubtypeProperty prop in user.Properties)
                                {
                                    if (prop.Name != null)
                                    {
                                        getUserProfilePropertyValue(prop, user, currentWeb);
                                    }
                                }
                                //Send email with any user profile fields not map to a custom list
                                reportUnManagedUserProfileProperties(currentWeb);
                                return true; //Search user has been found
                            }
                        }

                        return false; //Search user was not found           

                    }
                    else
                    {
                        return false;
                    }


            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserProfileDetailsByAccountName function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return false;
            }
        }

        /// <summary>
        ///     Method to retreive user profile by account name
        /// </summary>
        /// <param name="byPersonName"></param>
        public bool getUserProfileDetailsByAccountNameWithPrivileges(string byAccountName, SPWeb currentWeb, SPSite site)
        {
            try
            {
                bool isUserFound = false;

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    //SPSite site = SPContext.Current.Site;
                    ServerContext ospServerContext = ServerContext.GetContext(site);
                    UserProfileManager ospUserProfileManager = new UserProfileManager(ospServerContext);

                    if (ospUserProfileManager != null)
                    {
                        upFieldName = new List<string>();
                        upFieldValue = new List<string>();

                        //Search through all user profile property fields
                        foreach (UserProfile user in ospUserProfileManager)
                        {
                            if (user.MultiloginAccounts[0].ToLower() == byAccountName.ToLower())
                            {
                                foreach (ProfileSubtypeProperty prop in user.Properties)
                                {
                                    if (prop.Name != null)
                                    {
                                        getUserProfilePropertyValue(prop, user, currentWeb);
                                    }
                                }
                                //Send email with any user profile fields not map to a custom list
                                reportUnManagedUserProfileProperties(currentWeb);
                                isUserFound = true;  //Search user has been found
                            }
                        }  

                    }
                   

                });

                return isUserFound;

            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserProfileDetailsByAccountNameWithPrivileges function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

                return false;
            }
        }


        /// <summary>
        ///     Get the value of all mapped user property fields
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="user"></param>
        private void getUserProfilePropertyValue(ProfileSubtypeProperty prop, UserProfile user, SPWeb currentWeb)
        {
            try
            {
                switch (prop.Name)
                {
                    case "AccountName":
                        if (user[prop.Name].Value != null)
                            _accountName = user[prop.Name].Value.ToString();
                        break;

                    case "FirstName":
                        if (user[prop.Name].Value != null)
                            _firstName = user[prop.Name].Value.ToString();
                        break;

                    case "Title":
                        if (user[prop.Name].Value != null)
                            _jobTitle = user[prop.Name].Value.ToString();
                        break;

                    case "LastName":
                        if (user[prop.Name].Value != null)
                            _lastName = user[prop.Name].Value.ToString();
                        break;

                    case "EmployeeID":
                        if (user[prop.Name].Value != null)
                            _employeeID = user[prop.Name].Value.ToString();
                        break;

                    case "DepartmentNumber"://TODO:- Investigate internal fieldname
                        if (user[prop.Name].Value != null)
                            _deptNum = user[prop.Name].Value.ToString();
                        break;

                    case "Department":
                        if (user[prop.Name].Value != null)
                            _division = user[prop.Name].Value.ToString();
                        break;

                    case "IPPhone":
                        if (user[prop.Name].Value != null)
                            _ipPhone = user[prop.Name].Value.ToString();
                        break;

                    case "WorkPhone":
                        if (user[prop.Name].Value != null)
                            _workPhone = user[prop.Name].Value.ToString();
                        break;

                    case "WorkEmail":
                        if (user[prop.Name].Value != null)
                            _email = user[prop.Name].Value.ToString();
                        break;

                    case "Manager":
                        if (user[prop.Name].Value != null)
                            _manager = user[prop.Name].Value.ToString();
                        break;

                    case "CompanyName": //TODO:- Find internal field name
                        if (user[prop.Name].Value != null)
                            _companyName = user[prop.Name].Value.ToString();
                        break;

                    case "Office":
                        if (user[prop.Name].Value != null)
                            _officeAddress = user[prop.Name].Value.ToString();
                        break;

                    case "StartDate":
                        if (user[prop.Name].Value != null)
                            _startDate = ConvertToDateTime(user[prop.Name].Value.ToString());
                        break;

                    default:
                        upFieldName.Add(prop.Name);
                        //upFieldValue.Add(user[prop.Name].Value.ToString()); //TODO: handle multi-values
                        upFieldValue.Add("");
                        break;
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getUserProfilePropertyValue function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);

            }

        }
    

        /// <summary>
        ///     Function to retreive multiple values for a user profile field
        /// </summary>
        /// <param name="proValCol"></param>
        /// <returns></returns>
        private string getUserProfileMultiValue(UserProfileValueCollection proValCol)
        {            
            StringBuilder sb = new StringBuilder("");
            string multiValue = string.Empty;

            //retrieve all values inisde property values collection
            foreach (Object obj in proValCol)
            {
               sb.AppendFormat("{0}<br/>",obj.ToString());
            }
            multiValue = sb.ToString();

            return multiValue;

        }

        /// <summary>
        ///     Map internal property field value to class members
        /// </summary>
        /// <reference>
        ///     1. http://www.isurinder.com/Blog/Post/2011/02/01/Retrieve-and-Manage-User-Profile-Properties-in-SharePoint-2010
        /// </reference>
        /// <param name="fieldName"></param>
        private void mapUserProfilePropertyValue(UserProfileValueCollection proValCol, Property prop, string internalPropertyFieldName, string fieldValue)
        {
            //Check if this property has a multiple values to retrieve all values as an objects from it
            if (prop.IsMultivalued)
            {
                //TODO: handle multi-value routine
                switch (internalPropertyFieldName)
                {
                    case "AccountName":
                        _accountName = getUserProfileMultiValue(proValCol);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (internalPropertyFieldName)
                {
                    case "AccountName":
                        _accountName = fieldValue;
                        break;

                    case "FirstName":
                        _firstName = fieldValue;
                        break;

                    case "LastName":
                        _lastName = fieldValue;
                        break;

                    case "EmployeeID":
                        _employeeID = fieldValue;
                        break;

                    case "DepartmentNumber"://TODO:- Investigate internal fieldname
                        _deptNum = fieldValue;
                        break;

                    case "Department":
                        _division = fieldValue;
                        break;

                    case "IPPhone":
                        _ipPhone = fieldValue;
                        break;

                    case "WorkPhone":
                        _workPhone = fieldValue;
                        break;

                    case "WorkEmail":
                        _email = fieldValue;
                        break;

                    case "Manager":
                        _manager = fieldValue;
                        break;

                    case "CompanyName": //TODO:- Find internal field name
                        _companyName = fieldValue;
                        break;

                    case "Office":
                        _officeAddress = fieldValue;
                        break;

                    case "StartDate":
                        _startDate = ConvertToDateTime(fieldValue);
                        break;

                    default:
                        upFieldName.Add(internalPropertyFieldName);
                        upFieldValue.Add(fieldValue);
                        break;
                }
            }
        }


        /// <summary>
        ///     Report any unmapped user profile property fields via email
        /// </summary>
        /// <param name="internalPropertyFieldName"></param>
        /// <param name="fieldValue"></param>
        private void reportUnManagedUserProfileProperties(SPWeb currentWeb)
        {
            LogErrorHelper objEmail = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);

            try
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i <= upFieldName.Count - 1; i++)
                {
                    sb.AppendFormat("<b>{0}:</b> {1}<br>", upFieldName[i], upFieldValue[i]);
                }

                string emailMsg = "The following User Profile property fields below was not mapped to a custom list: <br>" + sb.ToString();

                //Send email to admin user
                objEmail.sendUserEmail(APP_NAME, "User Profile Notification", emailMsg, false);

            }
            catch (Exception err)
            {

                objEmail.logSysErrorEmail(APP_NAME, err, "Error at reportUnManagedUserProfileProperties function");           
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
            }
            objEmail = null;

        }

         /// <summary>
        ///     Convert string date to datetime
        /// </summary>
        /// <param name="value"></param>
        private DateTime ConvertToDateTime(string value)
        {
            DateTime convertedDate = DateTime.Now;

            try
            {
                convertedDate = Convert.ToDateTime(value);
               
            }
            catch (FormatException err)
            {
               string errMsg = value + " is not in the proper format.";            
               SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, errMsg, err.StackTrace);

            }

            return convertedDate;
        }

        /// <summary>
        /// Get the localized label of User Profile Properties
        /// Option 1 => Get the specific property to find the label (much better performance)
        /// </summary>
        /// <param name="userProfilePropertyName"></param>
        /// <reference>
        ///       1. http://nikpatel.net/2013/12/26/code-snippet-programmatically-retrieve-localized-user-profile-properties-label/ 
        /// </reference>
        /// <returns></returns>
        public string getLocalizedUserProfilePropertiesLabel(string userProfilePropertyName, SPWeb currentWeb)
        {
            string localizedLabel = string.Empty;

            try
            {
                //Get the handle of User Profile Service Application for current site (web application)
                SPSite site = SPContext.Current.Site;
                SPServiceContext context = SPServiceContext.GetContext(site);
                UserProfileConfigManager upcm = new UserProfileConfigManager(context);

                //Access the User Profile Property manager core properties
                ProfilePropertyManager ppm = upcm.ProfilePropertyManager;
                CorePropertyManager cpm = ppm.GetCoreProperties();

                //Get the core property for user profile property and get the localized value
                CoreProperty cp = cpm.GetPropertyByName(userProfilePropertyName);
                if (cp != null)
                {
                    localizedLabel = cp.DisplayNameLocalized[System.Globalization.CultureInfo.CurrentUICulture.LCID];
                }
            }
            catch (Exception err)
            {
                LogErrorHelper objErr = new LogErrorHelper(LIST_SETTING_NAME, currentWeb);
                objErr.logSysErrorEmail(APP_NAME, err, "Error at getLocalizedUserProfilePropertiesLabel function");
                objErr = null;

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                return string.Empty;
            }
            return localizedLabel;
        }

       

        #endregion
    }
}
