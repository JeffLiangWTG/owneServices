using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class EBondMssageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			AssertEquals("Should only focus on these eBond messages.", EDIMessage.ApplicationCodes.USeBond, MessageProcessorFactory.ApplicationCode);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("Should only focus on these eBond messages.", "US Customs eBond Message Processor", MessageProcessorFactory.MessageFriendlyName);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage()
		{
			CreateDeclarationForEBondMessage(Factory);
			var document = GetEBondInterchangeBodyXml();

			var validMessage = Factory.New<EBondEDIMessage>();
			validMessage.EM_MessageNum = "EBOND190320";
			validMessage.EM_Status = CBPEDIMessage.Status.Queued;
			validMessage.EM_MessageText = document.GetElementsByTagName("UniversalEvent")[0].OuterXml;
			validMessage.EM_SystemCreateTimeUtc = new ZDateTime(2019, 02, 20, 12, 00, 00);

			MessageProcessorFactory.ProcessMessage(validMessage);
			AssertEquals(EDIMessage.Status.Received, validMessage.EM_Status);

			var invalidMessage = Factory.New<EBondEDIMessage>();
			invalidMessage.EM_MessageNum = "EBOND190321";
			invalidMessage.EM_Status = CBPEDIMessage.Status.Queued;
			invalidMessage.EM_MessageText = "<!--INVALID TEXT-->";
			invalidMessage.EM_SystemCreateTimeUtc = new ZDateTime(2019, 02, 20, 12, 00, 00);

			MessageProcessorFactory.ProcessMessage(invalidMessage);
			AssertEquals(EDIMessage.Status.Failed, invalidMessage.EM_Status);
		}

		public static EDIInterchange CreateInterchangeForEBondMessage(BusinessObjectFactory factory)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_InterchangeType = "XDC";
			interchange.EI_Status = "QUE";
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.USeBond;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_To = "EBondMessageTest";
			interchange.EI_From = GlbCompany.CurrentCompany.LicenceEnterpriseCode;
			interchange.EI_InterchangeNum = "EBond190319054146";
			interchange.EI_SystemCreateTimeUtc = ZDateTime.Now;
			interchange.EI_BodyText = GetEBondInterchangeBodyXml().InnerXml;

			return interchange;
		}

		public static JobDeclaration CreateDeclarationForEBondMessage(BusinessObjectFactory factory)
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001398";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "00000592";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			factory.Save();

			return declaration;
		}

		public static XmlDocument GetEBondInterchangeBodyXml()
		{
			var document = new XmlDocument();
			document.Load(typeof(EBondMssageProcessorFactoryTest).Assembly.GetManifestResourceStream("Enterprise.Customs.US.Business.Testing.BatchProcessor.Testing.EBondInterchangeBody.xml"));

			return document;
		}

		EBondMssageProcessorFactory MessageProcessorFactory => messageProcessorFactory ?? (messageProcessorFactory = new EBondMssageProcessorFactory(new LoggingInformation()));
		EBondMssageProcessorFactory messageProcessorFactory;
	}
}
