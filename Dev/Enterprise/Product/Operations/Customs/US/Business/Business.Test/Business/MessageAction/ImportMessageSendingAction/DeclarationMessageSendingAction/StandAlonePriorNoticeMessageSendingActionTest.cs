using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StandAlonePriorNoticeMessageSendingAction))]
	sealed class StandAlonePriorNoticeMessageSendingActionTest : XmlSerializableNonPersistentBusinessObjectTest<StandAlonePriorNoticeMessageSendingAction>
	{
		public override void TestSchemaPropertiesHaveFields()
		{
			Assert("We need not save this sending action, so it is allowd has one Property hasn't a corresponding field.", true);
		}

		public void TestUS_MessageContents()
		{
			Declaration.JE_MasterBill = "MB111111";
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			Declaration.US_EnableSPN = true;
			Declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";
			tariff.UE_OGACodes = "FD3";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			var action = (StandAlonePriorNoticeMessageSendingAction)GetNewBusinessObject();
			action.US_PNActionCode = ACEPNActionCodeList.Codes.A;
			var declarationWrapper = new ACEPriorNoticeBillWrapper(Declaration.PrimaryMasterBill, Declaration);
			AssertMultilineASCIIEquals("MessageContents", new ACEPriorNoticeMessageBuilder(declarationWrapper, action.US_PNActionCode).GenerateHumanReadableMessageContent(), action.US_MessageContents);
			Declaration.US_EnableCRL = false;
			Declaration.US_EnableENS = false;
			action = (StandAlonePriorNoticeMessageSendingAction)GetNewBusinessObject();
			declarationWrapper = new ACEPriorNoticeBillWrapper(Declaration.PrimaryMasterBill, Declaration);
			AssertMultilineASCIIEquals("MessageContents", new ACEPriorNoticeMessageBuilder(declarationWrapper, action.US_PNActionCode).GenerateHumanReadableMessageContent(), action.US_MessageContents);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var importMessageSendingActionCollection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			return importMessageSendingActionCollection[0];
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				}

				return declaration;
			}
		}
	}
}
