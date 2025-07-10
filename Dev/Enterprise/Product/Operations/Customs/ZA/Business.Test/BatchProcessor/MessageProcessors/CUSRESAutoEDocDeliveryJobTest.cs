using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CUSRESAutoEDocDeliveryJobTest : AutoDocumentDeliveryJobTest
	{
		public new void TestDeliver_WithIncompleteDeliveryInstructions()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliver_ByEmail()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliver_ByFax()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliver_WhenJobNotApplicable()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliver_WithInvalidFaxDetails()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliver_WithNoRecipient()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliverInNonUserInteractiveEnvironmentDoesNotShowAnyForms()
		{
			Assert("Not Supported", true);
		}

		protected override DocumentCommand DocumentCommand
		{
			get
			{
				var filter = new DocumentZQuery(BusinessContext.CusEntryHeader, "Customs Declaration Response");
				return Factory.LoadTop1<DocumentCommand>(filter);
			}
		}

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine1 = invHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			return declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
		}

		protected override AutoDocumentDeliveryJob NewDocumentDeliveryJob(bool sendToDocManager)
		{
			return new CUSRESAutoEDocDeliveryJob(BusinessObjectToDeliver as CusEntryHeader, DocumentCommand.PK, ZGuid.Empty);
		}
	}
}
