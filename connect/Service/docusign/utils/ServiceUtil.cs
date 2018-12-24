using DocuSign.Connect;
using DocuSign.eSign.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace connect.Service.docusign.utils
{
    public static class ServiceUtil
    {
        
        public static DocuSignEnvironment buildEnvironment(DocuSignEnvelopeInformation envelopeInfo)
        {
            DocuSignEnvironment env = new DocuSignEnvironment();
            Predicate<CustomField> accountFinder = (CustomField p) => { return p.Name == "AccountId"; };
            env.AccountId = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(accountFinder).Value;
            Predicate<CustomField> AccountSiteFinder = (CustomField p) => { return p.Name == "AccountSite"; };
            env.Environment = envelopeInfo.EnvelopeStatus.CustomFields.CustomField.Find(AccountSiteFinder).Value;
            return env;
        }


        public static void ConfigureApiClient(string domain)
        {
            ApiClient apiClient = new ApiClient(ConfigurationManager.AppSettings["baseUrl"].Replace("{AccountSite}", domain));
            string authHeader = CreateAuthHeader(ConfigurationManager.AppSettings["dsUserName"],
                ConfigurationManager.AppSettings["dsUserPassword"],
                ConfigurationManager.AppSettings["dsIntegratorKey"]);
            // set client in global config so we don't need to pass it to each API object.
            DocuSign.eSign.Client.Configuration.Default.ApiClient = apiClient;

            if (DocuSign.eSign.Client.Configuration.Default.DefaultHeader.ContainsKey("X-DocuSign-Authentication"))
            {
                DocuSign.eSign.Client.Configuration.Default.DefaultHeader.Remove("X-DocuSign-Authentication");
            }
            DocuSign.eSign.Client.Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);
        }

        public static string CreateAuthHeader(string userName, string password, string integratorKey)
        {
            DocuSignCredentials dsCreds = new DocuSignCredentials()
            {
                Username = userName,
                Password = password,
                IntegratorKey = integratorKey
            };

            string authHeader = Newtonsoft.Json.JsonConvert.SerializeObject(dsCreds);
            return authHeader;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}