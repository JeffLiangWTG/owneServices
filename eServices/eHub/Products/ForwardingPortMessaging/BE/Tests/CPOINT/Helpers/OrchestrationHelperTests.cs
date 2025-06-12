using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XmlDiffPatch;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
	[TestClass]
	public class OrchestrationHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCreateRequestMessage()
		{
			var expectedMessage = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""><SOAP-ENV:Header><wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" mustUnderstand=""1""><wsse:UsernameToken xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" wsu:Id=""UsernameToken-1""><wsse:Username>WISETECH-CARGO</wsse:Username><wsse:Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">TsMPsM1bkBPkzsQ9XX1K</wsse:Password></wsse:UsernameToken></wsse:Security></SOAP-ENV:Header><SOAP-ENV:Body><por:checkpointAndRetrieveMessages xmlns:por=""http://portcommunity.haven.antwerpen.be/""><checkpoint>3</checkpoint><criteria><maxNumberOfMessages>20</maxNumberOfMessages><maxSize>9000000</maxSize></criteria></por:checkpointAndRetrieveMessages></SOAP-ENV:Body></SOAP-ENV:Envelope>";
			var requestMessage = OrchestrationHelper.CreateRequestMessage("WISETECH-CARGO", "TsMPsM1bkBPkzsQ9XX1K", "3");
			Assert.AreEqual(expectedMessage, requestMessage.InnerXml);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCheckMoreMessages()
		{
			var xmlString = @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
<soap:Body> 
    <ns2:checkpointAndRetrieveMessagesResponse xmlns:ns2=""http://portcommunity.haven.antwerpen.be/""> 
      <batch> 
        <moreMessages>false</moreMessages> 
      </batch> 
    </ns2:checkpointAndRetrieveMessagesResponse> 
  </soap:Body> 
</soap:Envelope>";
			var response = new XmlDocument();
			response.LoadXml(xmlString);
			var moreMessages = OrchestrationHelper.GetMoreMessagesValue(response);
			Assert.AreEqual("false", moreMessages);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetWcfSqlUpdateTest()
		{
			var codeMapsTestingContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			var eHubClients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CC_ID = "CPOINT" }
			};
			codeMapsTestingContext.eHubClients = eHubClients;
			codeMapsTestingContext.eHubTransformationSets = new TestDbSet<eHubTransformationSet>();
			codeMapsTestingContext.eHubCodeSets = new TestDbSet<eHubCodeSet>();
			codeMapsTestingContext.eHubCodeSetResults = new TestDbSet<eHubCodeSetResult>();
			codeMapsTestingContext.eHubCodeMapKeys = new TestDbSet<eHubCodeMapKey>();
			codeMapsTestingContext.eHubCodeMapValues = new TestDbSet<eHubCodeMapValue>();

			codeMapsTestingContext.eHubTransformationSets.Add(new eHubTransformationSet { TS_Name = "CPOINT System Configuration", eHubClient_Sender = eHubClients.ElementAt(0), eHubClient_Recipient = codeMapsTestingContext.eHubClients.ElementAt(0)});

			codeMapsTestingContext.eHubCodeSets.Add(new eHubCodeSet {CS_PK = Guid.Parse("6D6FD7C8-DAA9-4B7F-99F0-DF50BB3FF371"),CS_Name = "Connection Details", eHubClient_Sender = eHubClients.ElementAt(0) , eHubClient_Recipient = codeMapsTestingContext.eHubClients.ElementAt(0), eHubTransformationSet = codeMapsTestingContext.eHubTransformationSets.ElementAt(0)});

			codeMapsTestingContext.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CR_PK = Guid.Parse("8561D6C7-FA67-4B38-A71A-137E2C9E0901"), CR_Order = 1, CR_Name = "Username", CR_CS = Guid.Parse("6D6FD7C8-DAA9-4B7F-99F0-DF50BB3FF371") });
			codeMapsTestingContext.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CK_Order = 1,CK_PK  = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CK_Key1Value = "EBADEC" });
			codeMapsTestingContext.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = codeMapsTestingContext.eHubCodeMapKeys.Last(), eHubCodeSetResult = codeMapsTestingContext.eHubCodeSetResults.Last(), CV_CR = Guid.Parse("8561D6C7-FA67-4B38-A71A-137E2C9E0901"), CV_CK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CV_OutputCode = "WISETECH-EBALIE" });

			codeMapsTestingContext.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CR_PK = Guid.Parse("0161988F-6077-461F-B15B-D0A3612CC13B"), CR_Order = 1, CR_Name = "Password", CR_CS = Guid.Parse("6D6FD7C8-DAA9-4B7F-99F0-DF50BB3FF371") });
			codeMapsTestingContext.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CK_Order = 1, CK_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CK_Key1Value = "EBADEC" });
			codeMapsTestingContext.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = codeMapsTestingContext.eHubCodeMapKeys.Last(), eHubCodeSetResult = codeMapsTestingContext.eHubCodeSetResults.Last(), CV_CR = Guid.Parse("0161988F-6077-461F-B15B-D0A3612CC13B"), CV_CK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CV_OutputCode = "testpassword" });

			codeMapsTestingContext.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CR_PK = Guid.Parse("163E97AE-7409-4B77-AFC7-D2C71E58903F"), CR_Order = 1, CR_Name = "Checkpoint" ,CR_CS = Guid.Parse("6D6FD7C8-DAA9-4B7F-99F0-DF50BB3FF371") });
			codeMapsTestingContext.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CK_Order = 1, CK_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CK_Key1Value = "EBADEC" });
			codeMapsTestingContext.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = codeMapsTestingContext.eHubCodeMapKeys.Last(), eHubCodeSetResult = codeMapsTestingContext.eHubCodeSetResults.Last(), CV_CR = Guid.Parse("163E97AE-7409-4B77-AFC7-D2C71E58903F"), CV_CK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CV_OutputCode = "12" });

			codeMapsTestingContext.eHubCodeSetResults.Add(new eHubCodeSetResult { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(),CR_PK = Guid.Parse("A9A5C1DA-0A2D-444C-B2F7-DEB2E35DD070"), CR_Order = 1, CR_Name = "IsProd", CR_CS = Guid.Parse("6D6FD7C8-DAA9-4B7F-99F0-DF50BB3FF371") });
			codeMapsTestingContext.eHubCodeMapKeys.Add(new eHubCodeMapKey { eHubCodeSet = codeMapsTestingContext.eHubCodeSets.Last(), CK_Order = 1, CK_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CK_Key1Value = "EBADEC" });
			codeMapsTestingContext.eHubCodeMapValues.Add(new eHubCodeMapValue { eHubCodeMapKey = codeMapsTestingContext.eHubCodeMapKeys.Last(), eHubCodeSetResult = codeMapsTestingContext.eHubCodeSetResults.Last(), CV_CR = Guid.Parse("A9A5C1DA-0A2D-444C-B2F7-DEB2E35DD070"), CV_CK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CV_OutputCode = "0" });

			OrchestrationHelper.InternalContextFactory = () => codeMapsTestingContext;
			var actualUpdateCheckpointSqlXml = OrchestrationHelper.GetWcfSqlUpdateCheckPoint("EBADEC", "12", "1532");

			AssertXmlAreEqual(GetEmbeddedResource("CheckpointSqlXml.xml"), actualUpdateCheckpointSqlXml);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIsKnownError()
		{
			foreach (var error in Errors)
			{
				Assert.IsTrue(OrchestrationHelper.IsKnownError(error));
			}
		}

		Stream GetEmbeddedResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType().Namespace + ".CPOINT.Helpers.TestFiles." + name);
		}

		void AssertXmlAreEqual(Stream expectedXmlStream, XmlDocument actualXmlDocument)
		{
			var xmlDiff = new XmlDiff(XmlDiffOptions.IgnoreNamespaces);
			var xmlDiffgram = new XDocument();
			var expectedXDoc = XDocument.Load(expectedXmlStream);
			foreach (var node in expectedXDoc.Descendants())
				node.Attributes("xmlns").Remove();

			using (var expectedRdr = expectedXDoc.CreateReader())
			using (var actualRdr = new XmlNodeReader(actualXmlDocument))
			using (var diffWrtr = xmlDiffgram.CreateWriter())
				if (xmlDiff.Compare(expectedRdr, actualRdr, diffWrtr))
					return;

			var tempFile = Path.GetTempFileName();
			actualXmlDocument.Save(tempFile);
			Console.WriteLine("Actual XML: " + tempFile);
			Console.WriteLine("XML Diff:");
			Console.WriteLine(xmlDiffgram.ToString());
			Assert.Fail("AssertXmlAreEqual failed.");
		}

		static string[] Errors = new string[]
		{
			"java.net.NoRouteToHostException: No route to host (Host unreachable)",
			"SC_INTERNAL_SERVER_ERROR"
		};
	}
}
