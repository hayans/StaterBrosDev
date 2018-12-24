using connect.CMWebService;
using connect.Service.hangfire.tasks;
using connect.Service.ibm.cm.utils;
using log4net;
using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace connect.Service.ibm.cm
{
    public class CMWebServiceClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ContentManagerUploadTask));

        static string workingDir = "";
        static CMWebService.CMWebServicePortTypeClient client = new CMWebService.CMWebServicePortTypeClient("CMWebServicePort");

        public static AuthenticationData setupAuthData()
        {
            AuthenticationData authData = new AuthenticationData();
            ServerDef serverDef = new ServerDef();
            serverDef.ServerName = ConfigurationManager.AppSettings["cmServerName"]; // The CM server name
            authData.ServerDef = serverDef;
            AuthenticationDataLoginData loginData = new AuthenticationDataLoginData();
            loginData.UserID = ConfigurationManager.AppSettings["cmUserName"]; // The CM server user id
            loginData.Password = ConfigurationManager.AppSettings["cmUserPassword"]; // The CM server password
            authData.LoginData = loginData;

            return authData;

        }

        public static string getItemUri(AuthenticationData authData, string className, string query)
        {
            RunQueryRequest request = new RunQueryRequest();
            request.AuthenticationData = authData;
            request.QueryCriteria = new QueryCriteria();

            // set the query string to search for all policies
            request.QueryCriteria.QueryString = "/" + className +" [ @HREmployeeID = \"" + query + "\"]";
            request.maxResults = "1";
            request.version = "latest-version(.)";

            request.retrieveOption = RunQueryRequestRetrieveOption.CONTENT;
            request.contentOption = RunQueryRequestContentOption.URL;

            // now call the request
            RunQueryReply reply = client.RunQuery(request);
            // check to see if operation was successful
            if (reply.RequestStatus.success == true && reply.ResultSet.count > 0)
            {
               return reply.ResultSet.Item[0].URI;               
            }
            else
                return null;

        }

        public static string createItem(AuthenticationData authData, MTOMAttachment[] attachments, string documentClass, string documentType, string employeeId, string firstName, string lastName)
        {
            CreateItemRequest request = new CreateItemRequest();
            request.AuthenticationData = authData;
            request.Item = new CreateItemRequestItem();

            request.Item.ItemXML = XmlUtil.createItem(documentClass, documentType, employeeId, firstName, lastName);

            // Add mtom attachments
            if (attachments != null)
            {
                request.mtomRef = new MTOMAttachment[attachments.Length];
                for (int i = 0; i < attachments.Length; i++)
                {
                    request.mtomRef[i] = attachments[i];
                }
            }

            CreateItemReply reply = client.CreateItem(request);

            if (reply.RequestStatus.success == true)
            {
                return reply.Item.URI;
            }
            else
            {
                Log.Error("CreateItem Failed");
                return null;
            }
        }

        public static bool addItemToFolder(AuthenticationData authData, string folderUri, string itemUri)
        {

            AddItemToFolderRequest request = new AddItemToFolderRequest();

            request.newVersion = false;
            request.checkout = true;
            request.checkin = true;

            request.AuthenticationData = authData;

            request.Folder = new AddItemToFolderRequestFolder();
            request.Folder.URI = folderUri;
            request.Item = new AddItemToFolderRequestItem();
            request.Item.URI = itemUri;

            AddItemToFolderReply reply = client.AddItemToFolder(request);

            if (reply.RequestStatus.success == true)
            {
                return reply.RequestStatus.success;
            }
            else
            {
                Log.Error("Add Item to Folder Call Failed.");
                return false;
            }

        }

        public static MTOMAttachment[] setupAttachments(string[,] resources, byte[] document)
        {
            MTOMAttachment[] attachments = new MTOMAttachment[resources.GetLength(0)];
            for (int i = 0; i < resources.GetLength(0); i++)
            {
                attachments[i] = new MTOMAttachment();
                attachments[i].ID = resources[i, 0];
                attachments[i].MimeType = resources[i, 1];
                attachments[i].Value = document;
                //attachments[i].Value = File.ReadAllBytes(getWorkingDir() + resources[i, 2]);
            }
            return attachments;
        }
    }
}