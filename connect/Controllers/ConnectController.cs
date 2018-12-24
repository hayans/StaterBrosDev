using connect.Models.docusign.connect;
using connect.Service.docusign;
using connect.Service.docusign.utils;
using connect.Service.hangfire.tasks;
using DocuSign.Connect;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using connect.Service.ibm.cm.utils;
using System.Configuration;

namespace connect.Controllers
{
    public class ConnectController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConnectController));

        // GET api/connect
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/connect/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/connect
        //[ResponseType(typeof{string})]
        public HttpResponseMessage Post([FromBody]DocuSignEnvelopeInformation dsEnvInfo)
        {
            var dsEnv = ServiceUtil.buildEnvironment(dsEnvInfo);
            Log.Info("Processing envelopeId " + dsEnvInfo.EnvelopeStatus.EnvelopeID);
            // Log.Info("Creating HangFire backgoundJob ");

            //check for valid template
            IDictionary<string, string> localECF = CMServiceUtils.getDocumentProperties(dsEnvInfo);
            IDictionary<string, string> localParams = localECF;

            string[] classList = ConfigurationManager.AppSettings["cmValidClassList"].Split('|');
            string[] docParams = localParams[EnvelopeMetaFields.TemplateName].Split(':');
            string documentClass = null;
            string documentType = null;

            if (docParams.Length > 1)
            {
                documentClass = docParams[0];
                documentType = docParams[1];
                Log.Info("Doc Class : " + documentClass);
                Log.Info("Doc Type : " + documentType);

                if (!String.IsNullOrEmpty(documentClass) && !String.IsNullOrEmpty(documentType))
                {
                    bool jobCreated = false;
                    var jobId = "";

                    foreach (string item in classList)
                    {
                        if (item.Equals(documentClass))
                        {
                            jobId = Hangfire.BackgroundJob.Enqueue(() => ContentManagerUploadTask.uploadDocument(localECF,
                                                                        dsEnvInfo.EnvelopeStatus.EnvelopeID,
                                                                        "combined",
                                                                        DocumentOptions.Combined_No_Cert, documentClass, documentType));

                            Log.Info("Hangfire Job created. Job ID : " + jobId);

                            ConnectProcessResponse response = new ConnectProcessResponse(dsEnvInfo.EnvelopeStatus.EnvelopeID, jobId);
                            Log.Info("Reply HTTP 200 to DocuSign JobId created: " + jobId);
                            jobCreated = true;
                            //save
                            break;
                        }

                    }
                    //    Log.Info("An Invalid Template was sent and ignored by the Connect Application. Document Class : " + documentClass);

                    if (jobCreated)
                    {
                        return this.Request.CreateResponse<string>(HttpStatusCode.OK, dsEnvInfo.EnvelopeStatus.EnvelopeID + " : " + jobId);
                    }
                    else
                        return this.Request.CreateResponse<string>(HttpStatusCode.OK, "INFO :: A Template with an Invalid Document Class was sent and ignored by the Connect Application. Document Class : " + documentClass);
                }
                else
                    return this.Request.CreateResponse<string>(HttpStatusCode.OK, "INFO :: An Invalid Template was sent and ignored by the Connect Application. Document Class Or Document Type or both was not found");
            }
            else
                return this.Request.CreateResponse<string>(HttpStatusCode.OK, "INFO :: An Invalid Template was sent and ignored by the Connect Application.");
        }

        // PUT api/connect/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/connect/5
        public void Delete(int id)
        {
        }
    }
}
