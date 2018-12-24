using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace connect.Models.docusign.connect
{
    public class ConnectProcessResponse
    {
        private string envelopeId { get; set; }
        private List<JobDocument> hangFireJobId { get; set; }

        public ConnectProcessResponse(string envelopeId, string jobId)
        {
            this.envelopeId = envelopeId;
            //hangFireJobId = jobId;
        }
    }
}