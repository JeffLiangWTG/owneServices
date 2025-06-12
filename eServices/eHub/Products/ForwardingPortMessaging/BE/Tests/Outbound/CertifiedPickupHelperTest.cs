using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests.Outbound
{

    [TestClass]
    public class CertifiedPickupHelperTest
    {
        [TestMethod]
        public void TestGetCertifiedPickupBodyJsonString()
        {
            var xmlString = @"
<CertifiedPickUpRequest xmlns='http://www.cargowise.com/Schemas/FPM/CPOINT'>
  <Header>
    <externalReferenceId>NXP0000000062</externalReferenceId>
    <parameters>
      <portLoCode>BEANR</portLoCode>
      <equipmentNumber>MSCU2104261</equipmentNumber>
      <endPoint>import/release-rights</endPoint>
      <Branch>HAM</Branch>
   </parameters>
 </Header>
  <Body xmlns:json='http://james.newtonking.com/projects/json'>
      <releaseIdentification>REL210707</releaseIdentification>
      <terminalCode>01700</terminalCode>
      <actionType>Accept</actionType>
      <actionReason>test accept message</actionReason>
         <billOfLadingNumbers json:Array='true'>123456777</billOfLadingNumbers>
           <releaseFrom>
             <identificationType>Tin</identificationType>
             <identificationCode>AU231129369</identificationCode>
          </releaseFrom>
           <releaseTo>
             <identificationType>Tin</identificationType>
             <identificationCode>BE452814806</identificationCode>
          </releaseTo>
           <equipmentNumber>MSCU2104261</equipmentNumber>
           <portLoCode>BEANR</portLoCode>
        </Body>
      </CertifiedPickUpRequest>";
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var jsonString = CertifiedPickupHelper.GetCertifiedPickupBodyJsonString(xmlDoc);
            var jObject = JObject.Parse(jsonString);
            Assert.AreEqual("01700", jObject["terminalCode"]);


        }
    }
}
