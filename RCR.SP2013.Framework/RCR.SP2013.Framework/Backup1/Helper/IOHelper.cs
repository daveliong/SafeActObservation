using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;

namespace RCR.SP.Framework.Helper.IO
{
    public class IOHelper
    {

        #region variables

            private const string APP_NAME = "IOHelper Class";

        #endregion

        #region constructor
        
            public IOHelper() { }

        #endregion

        #region methods

            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Create a new folder on the server hard drive. 
            /// </summary> 
            /// <param name="iFolderName">INPUT - The folder location name.</param> 
            /// <returns> 
            /// Returns true if the new folder was successfully created. Otherwise returns false. 
            /// </returns> 
            /// <remarks> 
            /// </remarks> 
            /// ----------------------------------------------------------------------------- 
            public bool CreateFolder(string iFolderName)
            {
                DirectoryInfo fileDir = new DirectoryInfo(iFolderName);

                try
                {
                    fileDir.Create();
                    return true;
                }
                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    return false;
                }

            }

            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Delete an existing folder from the server. 
            /// </summary> 
            /// <param name="iFolderName">INPUT - The folder location name.</param> 
            /// <returns> 
            /// This function will return true if the folder was successfully deleted. Otherwise 
            /// it will return false for unsuccessful folder deletion. 
            /// </returns> 
            /// <remarks> 
            /// </remarks> 
            /// ----------------------------------------------------------------------------- 
            public bool DeleteFolder(string iFolderName)
            {

                DirectoryInfo fileDir = new DirectoryInfo(iFolderName);

                try
                {
                    fileDir.Delete(true);
                    return true;
                }
                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    return false;
                }

            }

            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Create a new file name on the hard drive if it does not exist. Otherwise append new content onto existing file. 
            /// </summary> 
            /// <param name="iFileName">The name of a file (including the folder path) to be created.</param> 
            /// <returns> 
            /// Returns true if a new file was successfully created. Otherwise returns false. 
            /// </returns> 
            /// <remarks> 
            /// Assume that file does not exist. Use isFileExist method first to check if file exist before 
            /// using the CreateFile method. 
            /// </remarks> 
            /// ----------------------------------------------------------------------------- 
            public bool WriteExistingFile(string iFileName, string iFileData)
            {

                try
                {
                    if (!isFileExist(iFileName))
                    {
                        if (CreateNewFile(iFileName, iFileData))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //Create a writer for an existing file 
                        StreamWriter oWriter = File.AppendText(iFileName);

                        oWriter.WriteLine(iFileData);
                        //Write the content with carriage new line return 
                        oWriter.Flush();
                        //Clear buffer 
                        oWriter.Close();
                        //Close writer 

                    }

                    return true;
                }
                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    return false;
                }

            }


            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Create a new file and write the data into that new file 
            /// </summary> 
            /// <param name="strData"></param> 
            /// <param name="FullPath"></param> 
            /// <param name="ErrInfo"></param> 
            /// <returns> 
            /// This function will return true if writing data to a new file was successful. 
            /// Otherwise this function will return false 
            /// </returns> 
            /// ----------------------------------------------------------------------------- 
            public bool SaveTextToFile(string strData, string FullPath, ref string ErrInfo)
            {

                bool bAns = false;
                StreamWriter objReader;

                try
                {
                    objReader = new StreamWriter(FullPath);
                    objReader.Write(strData);
                    objReader.Close();
                    bAns = true;
                }
                catch (Exception err)
                {
                    ErrInfo = err.Message;
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                }

                return bAns;

            }
            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Write the content to a new file. 
            /// </summary> 
            /// <param name="iFileData">INPUT - The content to be written.</param> 
            /// <param name="iFileName">INPUT - The file name.</param> 
            /// <returns> 
            /// Returns true if the new file was successfully written. 
            /// Otherwise returns false for unsuccessful writing. 
            /// </returns> 
            /// <remarks> 
            /// Assume that iFileName does not exist. Use isFileExist method first to check if file exist before 
            /// using this method. 
            /// </remarks> 
            /// ----------------------------------------------------------------------------- 
            public bool CreateNewFile(string iFileName, string iFileData)
            {

                try
                {
                    //Create the new file if it does not exist (assuming that it does not exist 
                    FileStream newFile = new FileStream(iFileName, FileMode.CreateNew, FileAccess.ReadWrite);

                    //Create a writer for the new file 
                    StreamWriter oWriter = new StreamWriter(newFile);

                    oWriter.WriteLine(iFileData);
                    //Write the content with a carriage return 
                    oWriter.Flush();
                    //Clear buffer 
                    oWriter.Close();
                    //Close writer 
                    newFile.Close();
                    //Close file 

                    return true;
                }
                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    return false;
                }

            }
            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Delete an existing file. 
            /// </summary> 
            /// <param name="iFileName">INPUT - The name of a file to be deleted. 
            /// </param> 
            /// <returns> 
            /// Returns true if the file was successfully deleted. Otherwise returns fasle. 
            /// </returns> 
            /// <remarks> 
            /// Assume that iFileName does exist. Use isFileExist method first to check if file exist before 
            /// using this method. 
            /// </remarks>  
            /// ----------------------------------------------------------------------------- 
            public bool DeleteFile(string iFileName)
            {

                try
                {
                    File.Delete(iFileName);

                    return true;
                }
                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    return false;
                }

            }

            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Checks to determine if a folder already exist on the hard drive. 
            /// </summary> 
            /// <param name="iFolderName">INPUT - The folder location name.</param> 
            /// <returns></returns> 
            /// <remarks> 
            /// This function will return true if the specific folder path exist. Otherwise it will return false. 
            /// </remarks>  
            /// ----------------------------------------------------------------------------- 
            public bool isFolderExist(string iFolderName)
            {
                DirectoryInfo fileDir = new DirectoryInfo(iFolderName);

                if (fileDir.Exists)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Determine if a file already exist 
            /// </summary> 
            /// <param name="iFileName"></param> 
            /// <returns></returns> 
            /// <remarks> 
            /// Returns true if a file exist. Otherwise return false. 
            /// </remarks> 
            /// ----------------------------------------------------------------------------- 
            public bool isFileExist(string iFileName)
            {

                if (File.Exists(iFileName))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// Check if file is currently in use by another user 
            /// </summary> 
            /// <param name="iFileName"></param> 
            /// <returns> 
            /// Returns True if the file is open (currently in use). Otherwise returns False 
            /// </returns> 
            /// ----------------------------------------------------------------------------- 
            public bool isFileInUse(string iFileName)
            {

                // If the file is already opened by another process and the specified type of access 
                // is not allowed, then the Open operation will fail and an error occurs. 
                try
                {
                    FileStream fs;
                    fs = File.OpenRead(iFileName);
                    fs.Close();
                    return false;
                    //File is not in use by another user 
                }

                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    return true;
                    //File is still in use 
                }


            }

            /// ----------------------------------------------------------------------------- 
            /// <summary> 
            /// NAME: readFileContents 
            /// </summary> 
            /// <param name="iFileName">The location of the filel, which in this case is limited to either a
            ///     text or html file type.
            /// </param> 
            /// <returns> 
            /// This function will read a particular file from the server and return the string content. 
            /// </returns> 
            /// <remarks> 
            /// PRE: A file (iFileName) is specified to be read. This procedure will need to handle 
            /// invalid file that does not exist on the server. 
            /// POST: A valid file is read successfully 
            /// </remarks> 
            /// ----------------------------------------------------------------------------- 
            public string readFileContents(string iFileName)
            {
                string functionReturnValue = string.Empty;

                try
                {
                    StreamReader filestream;
                    string readcontents;

                    filestream = File.OpenText(iFileName);
                    readcontents = filestream.ReadToEnd();
                    functionReturnValue = readcontents;

                    filestream.Close();
                    return functionReturnValue;
                }
                catch (Exception err)
                {
                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory(APP_NAME, TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, err.Message.ToString(), err.StackTrace);
                    functionReturnValue = err.Message.ToString();
                }

                return functionReturnValue;

            }



        #endregion

    }
}
