using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

using System.Net.Mail;

using RCR.SP.Framework.Helper.SharePoint;

namespace RCR.SP.Framework.Helper.LogError
{
    public class LogErrorHelper
    {

        #region Variables

        private const string APP_NAME = "LogErrorHelper Class";
        private string _spSettingList;
        private SPWeb _spCurrentWeb;

        #endregion

        #region Constructor

        /// <summary>
        ///      Initializes a new instance of class
        /// </summary>
        //public LogErrorHelper() {}

        /// <summary>
        ///      Initializes a new instance of class
        /// </summary>
        public LogErrorHelper(string spSettingList, SPWeb spWeb)
        {
            this._spSettingList = spSettingList;
            this._spCurrentWeb = spWeb;
        }


        #region members

        public string SPSettingList
        {
            get { return this._spSettingList; }
            set { this._spSettingList = value; }
        }


        public SPWeb SPCurrentWeb
        {
            get { return this._spCurrentWeb; }
            set { this._spCurrentWeb = value; }
        }

        #endregion

        #endregion

        #region method


        /// <summary>
        ///     Function to email system error messages 
        /// </summary>
        /// <param name="errSysMsg"></param>
        public bool logSysErrorEmail(string APP_NAME, Exception errSysMsg, string errTitle)
        {
            try
            {
                string categorySetting = "EmailTo";
                SharePointHelper spHelper = new SharePointHelper(_spSettingList, categorySetting, _spCurrentWeb);

                string EmailTo = spHelper.GetRCRSettingsItem(categorySetting, _spSettingList).ToString();
                string strError = System.DateTime.Now + "<br>Application: " + APP_NAME + "<br>Error Message: " + errSysMsg.Message + "<br>";

                //Check the InnerException 
                while ((errSysMsg.InnerException != null))
                {
                    strError += errSysMsg.InnerException.ToString();
                    errSysMsg = errSysMsg.InnerException;
                }

                //Send error log via email
                SPUtility.SendEmail(_spCurrentWeb, false, false, EmailTo, errTitle, strError);

                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, strError, errSysMsg.StackTrace);
                spHelper = null;

                return true;
            }
            catch
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, errSysMsg.Message.ToString(), errSysMsg.StackTrace);
                return false;
            }


        }

        /// <summary>
        ///     Function to email system error messages 
        /// </summary>
        /// <param name="errSysMsg"></param>
        public bool sendUserEmail(string APP_NAME, string Title, string emailBody, bool isHTMLFormat)
        {
            try
            {
                string categorySetting = "EmailTo";
                SharePointHelper spHelper = new SharePointHelper(_spSettingList, categorySetting, _spCurrentWeb);
                string EmailTo = spHelper.GetRCRSettingsItem(categorySetting, _spSettingList).ToString();

                //Send error log via email
                SPUtility.SendEmail(_spCurrentWeb, isHTMLFormat, false, EmailTo, Title, emailBody);
                spHelper = null;

                return true;
            }
            catch (Exception errSysMsg)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, errSysMsg.Message.ToString(), errSysMsg.StackTrace);
                return false;
            }


        }

        /// <summary>
        ///    Email and log error message using custom SMTP server
        /// </summary>
        /// <param name="APP_NAME"></param>
        /// <param name="errSysMsg"></param>
        /// <param name="errTitle"></param>
        /// <returns>Returns true if email was sucessfully sent</returns>
        public bool logErrorSMTPEmail(string APP_NAME, Exception errSysMsg, string errTitle, bool isHTMLFormat)
        {
            try
            {
                SharePointHelper spHelper = new SharePointHelper(_spSettingList, "", _spCurrentWeb);

                string EmailTo = spHelper.GetRCRSettingsItem("EmailTo", _spSettingList).ToString();
                string EmailFrom = spHelper.GetRCRSettingsItem("EmailFrom", _spSettingList).ToString();
                string SMPTServer = spHelper.GetRCRSettingsItem("SMTPServer", _spSettingList).ToString();
                string useProxyEmail = spHelper.GetRCRSettingsItem("UseProxyEmail", _spSettingList).ToString();

                string strError = System.DateTime.Now + "<br>Application: " + APP_NAME + "<br>Error Message: " + errSysMsg.Message + "<br>";

                //Check the InnerException 
                while ((errSysMsg.InnerException != null))
                {
                    strError += errSysMsg.InnerException.ToString();
                    errSysMsg = errSysMsg.InnerException;
                }

                //Send error log via email
                MailMessage message = new MailMessage();
                message.From = new MailAddress(EmailFrom);

                if (isHTMLFormat)
                    message.IsBodyHtml = true;

                message.ReplyTo = new MailAddress(EmailFrom);
                message.Sender = new MailAddress(EmailFrom);

                message.To.Add(new MailAddress(EmailTo));
                // message.CC.Add(new MailAddress("copy@domain.com"));  
                message.Subject = errTitle;
                message.Body = strError;

                SmtpClient client = new SmtpClient();
                client.Host = SMPTServer;

                if (useProxyEmail == "true")
                {
                    client.UseDefaultCredentials = true;
                    string Domain = spHelper.GetRCRSettingsItem("Domain", _spSettingList).ToString();
                    string SysAcct = spHelper.GetRCRSettingsItem("ProxyUser", _spSettingList).ToString();
                    string SysAcctPassword = spHelper.GetRCRSettingsItem("ProxyPassword", _spSettingList).ToString();

                    client.Credentials = new System.Net.NetworkCredential(Domain + "\\" + SysAcct, SysAcctPassword);
                }

                client.Send(message);
                spHelper = null;

                return true;
            }
            catch
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, errSysMsg.Message.ToString(), errSysMsg.StackTrace);
                return false;
            }

        }

        /// <summary>
        ///     Logging function that enters updates to the Workflow History list
        /// </summary>
        /// <param name="logMessage"></param>
        public void LogWFHistoryComment(string logMessage, SPWorkflowActivationProperties workflowProperties, Guid WorkflowInstanceId)
        {
            SPWorkflow.CreateHistoryEvent(workflowProperties.Web, WorkflowInstanceId, 0, workflowProperties.Web.CurrentUser, new TimeSpan(), "Update", logMessage, string.Empty);
        }

        #endregion

        
    }
}
