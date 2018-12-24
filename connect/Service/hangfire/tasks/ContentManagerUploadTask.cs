using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocuSign.Connect;
using connect.Service.docusign;
using DocuSign.eSign.Model;
using connect.Service.docusign.utils;
using System.ComponentModel;
using System.IO;
using log4net;
using connect.Service.ibm.cm;
using connect.CMWebService;
using connect.Service.ibm.cm.utils;
using System.Configuration;

namespace connect.Service.hangfire.tasks
{
    public static class ContentManagerUploadTask
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ContentManagerUploadTask));

        [DisplayName("Uploading document {3} for envelope {2}")]
        public static void uploadDocument(IDictionary<string, string> localParams, string envelopeId, string documentId, DocumentOptions options, string docClass, string docType)
        {
            Log.Info("Environment : " + localParams[EnvelopeMetaFields.Environment] + " - Account Id : " + localParams[EnvelopeMetaFields.AccountId] + " - Envelope Id : " + envelopeId + " - document Id : " + documentId + " - Option : " + options);

            //string[] docParams = localParams[EnvelopeMetaFields.TemplateName].Split(':');
            //string documentClass = docParams[0];
            //string documentType = docParams[1];

            //Now retrieve the documents for the given envelope from the accountId hosted in environment as combined
            MemoryStream docStream = DocuSignService.GetDocument(localParams[EnvelopeMetaFields.Environment],
                localParams[EnvelopeMetaFields.AccountId],
                envelopeId,
                documentId,
                options);

            // Now upload the bytes for the document that we just retrieved in cm
            byte[] buffer = ServiceUtil.ReadFully(docStream);
            
            string exFolderUri = exFolderUri = ConfigurationManager.AppSettings["exFolderUri"];
            
            AuthenticationData authData = CMWebServiceClient.setupAuthData();
            MTOMAttachment[] mtomAttachment = CMWebServiceClient.setupAttachments(new string[,] { { "doc", "application/pdf" } }, buffer);
            
            //check for cach 
            //check for  hangfire flag
            string itemUri = CMWebServiceClient.createItem(authData, mtomAttachment, docClass, docType, localParams[EnvelopeMetaFields.EID], localParams[EnvelopeMetaFields.FirstName], localParams[EnvelopeMetaFields.LastName]);
            
            //setcach
            //set a hangfire flag
            Log.Info("The Following CM Item was Created. Item Uri :: " + itemUri);
            string folderUri = CMWebServiceClient.getItemUri(authData, "HrEmpFolder", localParams[EnvelopeMetaFields.EID]);
            if (folderUri == null) { 
                CMWebServiceClient.addItemToFolder(authData, exFolderUri, itemUri);
                Log.Info("No Folder Matching the Employee ID was found. Item was placed in the following Exception Folder - Folder URI :: " + folderUri);
            }
            else { 
                CMWebServiceClient.addItemToFolder(authData, folderUri, itemUri);
                Log.Info("Item was placed in the following Folder - Folder URI ::" + folderUri);
            } 
        }
    }
}