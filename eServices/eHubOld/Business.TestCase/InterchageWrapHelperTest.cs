using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Business.TestCase
{
	[TestClass]
	public class InterchageWrapHelperTest
	{
		[TestMethod]
		public void WrapIntoInterchange()
		{
			string document = "Just any text.";

			string id = Guid.NewGuid().ToString();
			var interchange = InterchageWrapHelper.WrapIntoInterchange(document, "BLAH", id);
			Assert.AreEqual(string.Format("<ns0:Interchange xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><InterchangeHeader><SenderID></SenderID><RecipientID></RecipientID><InterchangeVersion>1.0</InterchangeVersion><SenderApplicationVersion>1.0</SenderApplicationVersion><InterchangeID>{0}</InterchangeID></InterchangeHeader><Payload><ns0:Document xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\" DocumentType=\"BLAH\"><DocumentContent>Just any text.</DocumentContent></ns0:Document></Payload></ns0:Interchange>", id),
				interchange.OuterXml);
		}

		[TestMethod]
		public void RetrievePayloadToDocumentEnvelope()
		{
			string interchange = "<ns0:Interchange xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><InterchangeHeader><SenderID>SenderID_0</SenderID><RecipientID>RecipientID_0</RecipientID><InterchangeVersion>InterchangeVersion_0</InterchangeVersion><SenderApplicationVersion>SenderApplicationVersion_0</SenderApplicationVersion><InterchangeID>InterchangeID_0</InterchangeID></InterchangeHeader><Payload><ns0:Document DocumentType=\"AAA\" xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><DocumentContent>Content_0</DocumentContent></ns0:Document><ns0:Document DocumentType=\"BBB\" xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><DocumentContent>Content_1</DocumentContent></ns0:Document></Payload></ns0:Interchange>";

			var documentEnvelope = InterchageWrapHelper.RetrievePayloadToDocumentEnvelope(interchange);
			Assert.AreEqual(string.Format("<ns0:DocumentEnvelope xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><Documents><ns0:Document DocumentType=\"AAA\" xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><DocumentContent>Content_0</DocumentContent></ns0:Document><ns0:Document DocumentType=\"BBB\" xmlns:ns0=\"http://CargoWise.eServices.eHub.Schema\"><DocumentContent>Content_1</DocumentContent></ns0:Document></Documents></ns0:DocumentEnvelope>"),
				documentEnvelope.OuterXml);
		}
	}
}
