using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5167;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class N5167DataTestHelper : TestCaseWithFactory
	{
		public N5167DataTestHelper() : base()
		{ }

		public N5167DataTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
			GenerateTestData();
		}
		readonly BusinessObjectFactory factory;

		protected override BusinessObjectFactory NewFactory()
		{
			return factory ?? base.NewFactory();
		}

		void GenerateTestData()
		{
			var classification = Factory.NewWithValidTestData<Customs.Business.BaseCusClassification>();
			classification.CC_Description = "CUCKOO SQUEAKERS";
			classification.CC_LookupCode = "CKSQKS";
			classification.CC_TariffNum = "0000.00.00.00Y";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_CustomsProfile = "AAA-BBB";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_CustomsOffice = "AA";

			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 15);
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CC = classification.PK;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CC = classification.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			messageSending = new N5167MessageSendingObject(entryHeader);
			Provider = messageSending;
			Factory.Save();
		}

		public N5167MessageSendingObject messageSending;
		public IN5167Declaration Provider;
	}
}
