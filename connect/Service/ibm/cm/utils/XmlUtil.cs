using connect.CMWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace connect.Service.ibm.cm.utils
{
    public static class XmlUtil
    {
        public static XmlElement createItem(string documentClass, string documentType, string employeeId, string firstName, string lastName)
        {
            DateTime currentDate = DateTime.Today;

            var doc = new XmlDocument();
            XDocument itemXml = new XDocument();
            XElement itemType = new XElement(documentClass);
            itemType.SetAttributeValue("HRDocumentType", documentType);
            itemType.SetAttributeValue("HREmployeeID", employeeId);
            itemType.SetAttributeValue("HRFirstName", firstName);
            itemType.SetAttributeValue("HRLastName", lastName);
            itemType.SetAttributeValue("HRDate", currentDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.GetCultureInfo("en-US")));

            XElement icmBaseElement = new XElement("ICMBASE");
            itemType.AddFirst(icmBaseElement);

            XNamespace xmlns = XNamespace.Get("http://www.ibm.com/xmlns/db2/cm/api/1.0/schema");
            XElement labelElement = new XElement(xmlns + "label");
            labelElement.SetAttributeValue("name", "doc");

            XElement resourceObjectElement = new XElement(xmlns + "resourceObject", labelElement);
            resourceObjectElement.SetAttributeValue("MIMEType", "application/pdf");
            icmBaseElement.AddFirst(resourceObjectElement);

            doc.Load(itemType.CreateReader());

            return doc.DocumentElement;
        }
    }
}