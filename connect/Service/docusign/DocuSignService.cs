using connect.Service.docusign.utils;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace connect.Service.docusign
{

    public static class DocuSignService
    {
        private static EnvelopesApi envelopesApi;
        private static readonly ILog Log = LogManager.GetLogger(typeof(DocuSignService));

        public static MemoryStream GetDocument(string domain, string account, string envelopeId, string documentId, DocumentOptions documentOptions)
        {
            DocuSign.eSign.Api.EnvelopesApi.GetDocumentOptions options = null;

            if (documentOptions == DocumentOptions.Combined)
            {
                options = new EnvelopesApi.GetDocumentOptions();
                documentId = "combined";
                options.certificate = "true";

            }
            if (documentOptions == DocumentOptions.Combined_No_Cert)
            {
                options = new EnvelopesApi.GetDocumentOptions();
                documentId = "combined";
                options.certificate = "false";

            }
            MemoryStream docStream = null;
            try
            {
                // GetDocument() API call returns a MemoryStream
                Log.Info("Retriving Document " + documentId + " for envelope: " + envelopeId +
                    " environment: " + domain + ":" + account);
                ServiceUtil.ConfigureApiClient(domain);
                envelopesApi = new EnvelopesApi();
                docStream = (MemoryStream)envelopesApi.GetDocument(account, envelopeId, documentId, options);
            }
            catch (ApiException ex)
            {
                Log.Error(ex);
            }
            return docStream;
        }

        
    }
}