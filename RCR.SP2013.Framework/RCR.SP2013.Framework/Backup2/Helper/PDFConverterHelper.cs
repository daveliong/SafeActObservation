using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Word.Server.Service;
using Microsoft.Office.Word.Server.Conversions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

using RCR.SP.Framework.Helper.SharePoint;

namespace RCR.SP.Framework.Helper.PDFConverter
{
    public class PDFConverterHelper
    {

        #region variables

        private const string AutomationServiceName = "Word Automation Services";
        private const string ClassName = "PDFConverterHelper class";
        private string _pdfLogList;

        #endregion

        #region constructor

        /// <summary>
        ///     Initialise class instance
        /// </summary>
        public PDFConverterHelper(string pdfLogList) {

            this._pdfLogList = pdfLogList;
        }

        #endregion


        #region members

        public string PDFLogList
        {
             get {return this._pdfLogList;}
             set {this._pdfLogList = value;}
        }

        #endregion

        #region methods
        /// <summary>
        ///     Timer job to convert doc to PDF
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="dstFile"></param>
        /// <returns></returns>
        public bool PDFConvertJob(string srcFile, string dstFile)
        {

            try
            {
                //create references to the Word Services.
                var wdProxy = (WordServiceApplicationProxy)SPServiceContext.Current.GetDefaultProxy(typeof(WordServiceApplicationProxy));
                var conversionJob = new ConversionJob(wdProxy);

                conversionJob.UserToken = SPContext.Current.Web.CurrentUser.UserToken;
                conversionJob.Name = "PDF Conversion Job " + DateTime.Now.ToString("hhmmss");
                conversionJob.Settings.OutputFormat = SaveFormat.PDF;
                conversionJob.Settings.OutputSaveBehavior = SaveBehavior.AlwaysOverwrite;

                conversionJob.AddFile(srcFile, dstFile);

                conversionJob.Start();
                return (conversionJob.Started);
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(ClassName, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "PDFConverJob - " + ex.Message, ex.StackTrace);
                return false;
            }

        }

        /// <summary>
        ///     Convert document to PDF
        /// </summary>
        public void ConvertDocToPDF(SPListItem listItem)
        {
            try
            {
                //Variables used for PDF conversions
                ConversionJobSettings jobSettings;
                ConversionJob pdfConversionJob;
                string wordFile; //Source Word file
                string pdfFile; //target destination PDF file

                // Initialize the conversion settings.
                jobSettings = new ConversionJobSettings();
                jobSettings.OutputFormat = SaveFormat.PDF;

                // Create the conversion job using the settings.
                pdfConversionJob = new ConversionJob(AutomationServiceName, jobSettings);

                //Set the credentials to use when running the conversion job.
                pdfConversionJob.UserToken = SPContext.Current.Web.CurrentUser.UserToken;

                // Set the file names to use for the source Word document and the destination PDF document.
                wordFile = SPContext.Current.Web.Url + "/" + listItem.Url;
                if (IsFileTypeDoc(listItem.File, "docx"))
                {
                    pdfFile = wordFile.Replace(".docx", ".pdf");
                }
                else
                {
                    pdfFile = wordFile.Replace(".doc", ".pdf");
                }

                // Add the file conversion to the conversion job.
                pdfConversionJob.AddFile(wordFile, pdfFile);

                // Add the conversion job to the Word Automation Services conversion job queue.
                // The conversion does not occurimmediately but is processed during the next run of the document conversion job.
                pdfConversionJob.Start();
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(ClassName, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "ConvertDocToPDF - " + ex.Message, ex.StackTrace);
            }

        }

        /// <summary>
        ///     Convert document to PDF
        /// </summary>
        public void ConvertDocToPDFWithSettings(SPListItem listItem, string SPSettingList, string categorySettingName, SPWeb currentWeb)
        {
            try
            {
                SharePointHelper spHelper = new SharePointHelper(_pdfLogList, categorySettingName, currentWeb);
                string WordAutoSvc = spHelper.GetRCRSettingsItem(categorySettingName, SPSettingList).ToString();

                //Variables used for PDF conversions
                ConversionJobSettings jobSettings;
                ConversionJob pdfConversionJob;
                string wordFile; //Source Word file
                string pdfFile; //target destination PDF file

                // Initialize the conversion settings.
                jobSettings = new ConversionJobSettings();
                jobSettings.OutputFormat = SaveFormat.PDF;

                // Create the conversion job using the settings.
                pdfConversionJob = new ConversionJob(WordAutoSvc, jobSettings);

                //Set the credentials to use when running the conversion job.
                pdfConversionJob.UserToken = SPContext.Current.Web.CurrentUser.UserToken;

                // Set the file names to use for the source Word document and the destination PDF document.
                wordFile = SPContext.Current.Web.Url + "/" + listItem.Url;
                if (IsFileTypeDoc(listItem.File, "docx"))
                {
                    pdfFile = wordFile.Replace(".docx", ".pdf");
                }
                else if (IsFileTypeDoc(listItem.File, "doc"))
                {
                    pdfFile = wordFile.Replace(".doc", ".pdf");
                }
                else
                {
                    pdfFile = "";
                }

                if (pdfFile.Length > 0)
                {
                    // Add the file conversion to the conversion job.
                    pdfConversionJob.AddFile(wordFile, pdfFile);

                    // Add the conversion job to the Word Automation Services conversion job queue.
                    // The conversion does not occurimmediately but is processed during the next run of the document conversion job.
                    pdfConversionJob.Start();
                }

                spHelper = null;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(ClassName, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "ConvertDocToPDF - " + ex.Message, ex.StackTrace);
            }

        }


        /// <summary>
        ///     Verify the document added is a Word Document before starting the conversion.
        /// </summary>
        /// <param name="spFile"></param>
        /// <returns>Returns true if file is valid</returns>
        private bool IsFileTypeDoc(SPFile spFile, string docType)
        {
            bool IsDoc = false;

            switch (docType)
            {
                case "":
                    if (spFile.Name.Contains(".docx") || spFile.Name.Contains(".doc"))
                        IsDoc = true;
                    break;

                case "doc":
                    if (spFile.Name.Contains(".doc"))
                        IsDoc = true;
                    break;

                case "docx":
                    if (spFile.Name.Contains(".docx"))
                        IsDoc = true;
                    break;

                default:
                    IsDoc = false;
                    break;
            }

            return IsDoc;
        }

        /// <summary>
        ///     Check if document is docx file type
        /// </summary>
        /// <param name="srcFileName"></param>
        /// <returns></returns>
        public bool isDocValidated(string srcFileName)
        {
            bool IsDoc = false;

            if (srcFileName.Contains(".docx") || srcFileName.Contains(".doc"))
                IsDoc = true;

            return IsDoc;
        }

        /// <summary>
        ///     Validate if document type is valid for PDF conversion
        /// </summary>
        /// <param name="listItemId"></param>
        /// <param name="list"></param>
        /// <returns>
        ///     Returns true if document type is a word format.
        /// </returns>
        public bool isDocIDValidated(string listItemId, SPList list)
        {
            bool validDoc = false;

            try
            {
                //Get the details of selected documents
                using (SPWeb spWeb = SPContext.Current.Site.OpenWeb(SPContext.Current.Web.ServerRelativeUrl))
                {
                    SPListItem listItem = list.GetItemById(int.Parse(listItemId));
                    PDFConverterHelper PDFConvert = new PDFConverterHelper("");

                    if ((listItem.FileSystemObjectType == SPFileSystemObjectType.File) && PDFConvert.IsFileTypeDoc(listItem.File, ""))
                        validDoc = true;

                    PDFConvert = null;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(ClassName, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, "isDocValidated - " + ex.Message, ex.StackTrace);
            }
            return validDoc;
        }
        #endregion
    }
}
