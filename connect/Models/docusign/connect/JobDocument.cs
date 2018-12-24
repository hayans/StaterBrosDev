using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace connect.Models.docusign.connect
{
    public class JobDocument
    {
        private string jobId { get; set; }
        private string envelopeId { get; set; }

        public JobDocument(string jobId, string envelopeId)
        {
            this.jobId = jobId;
            this.envelopeId = envelopeId;
        }
    }
}