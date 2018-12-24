using DocuSign.Connect;
using DocuSign.eSign.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using connect.Service.util;

namespace connect.Service.ibm.cm.utils
{

    public class CMServiceUtils
    {
        public static void ConfigureApiClient(string repo)
        {
            ApiClient apiClient = new ApiClient(ConfigurationManager.AppSettings["documentumUrl"].Replace("{REPO}", repo));
            string authHeader = " Basic " + CreateBasicBearToken(ConfigurationManager.AppSettings["dsUserName"],
                ConfigurationManager.AppSettings["dsUserPassword"]);
            // set client in global config so we don't need to pass it to each API object.
            DocuSign.eSign.Client.Configuration.Default.ApiClient = apiClient;

            if (DocuSign.eSign.Client.Configuration.Default.DefaultHeader.ContainsKey("X-DocuSign-Authentication"))
            {
                DocuSign.eSign.Client.Configuration.Default.DefaultHeader.Remove("X-DocuSign-Authentication");
            }
            if (DocuSign.eSign.Client.Configuration.Default.DefaultHeader.ContainsKey("Authorization"))
            {
                DocuSign.eSign.Client.Configuration.Default.DefaultHeader.Remove("Authorization");
            }
            DocuSign.eSign.Client.Configuration.Default.AddDefaultHeader("Authorization", authHeader);
            DocuSign.eSign.Client.Configuration.Default.AddDefaultHeader("Content-Type", "application/vnd.emc.documentum+json");
        }


        public static Dictionary<string, string> getDocumentProperties(DocuSignEnvelopeInformation envelopeInfo)
        {
            Dictionary<string, string> ecf = new Dictionary<string, string>();

            Predicate<DocumentStatus> docfinder = (DocumentStatus p) => { return p.ID == "1"; };
            DocumentStatus templateName = envelopeInfo.EnvelopeStatus.DocumentStatuses.DocumentStatus.Find(docfinder);
            string docClassNType = templateName == null ? null : templateName.TemplateName;
            ecf.Add(EnvelopeMetaFields.TemplateName, docClassNType);

            Predicate<CustomField> finder = (CustomField p) => { return p.Name == EnvelopeMetaFields.AccountId; };
            CustomField customField = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(finder);
            string accountId = customField == null ? null : customField.Value;
            ecf.Add(EnvelopeMetaFields.AccountId, accountId);

            finder = (CustomField p) => { return p.Name == EnvelopeMetaFields.Environment; };
            customField = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(finder);
            string environment = customField == null ? null : customField.Value;
            ecf.Add(EnvelopeMetaFields.Environment, environment);

            finder = (CustomField p) => { return p.Name == EnvelopeMetaFields.EID; };
            customField = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(finder);
            string employeeId = customField == null ? null : customField.Value;
            ecf.Add(EnvelopeMetaFields.EID, employeeId);

            finder = (CustomField p) => { return p.Name == EnvelopeMetaFields.FirstName; };
            customField = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(finder);
            string firstName = customField == null ? null : customField.Value;
            ecf.Add(EnvelopeMetaFields.FirstName, firstName);

            finder = (CustomField p) => { return p.Name == EnvelopeMetaFields.LastName; };
            customField = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(finder);
            string lastName = customField == null ? null : customField.Value;
            ecf.Add(EnvelopeMetaFields.LastName, lastName);

            return ecf;
        }

        public static string CreateBasicBearToken(string userName = null, string password = null)
        {
            string cmUserName = null;
            string cmPassword = null;
            if (userName == null) userName = ConfigurationManager.AppSettings["cmUserName"];
            if (password == null) password = ConfigurationManager.AppSettings["cmPassword"];
            string baseOauth = userName + ":" + password;
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(baseOauth));
        }
    }
}