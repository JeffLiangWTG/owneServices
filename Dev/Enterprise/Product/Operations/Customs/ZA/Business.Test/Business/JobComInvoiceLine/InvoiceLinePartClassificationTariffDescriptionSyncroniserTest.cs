using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class InvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode => "00000001";

		protected override ZString TariffCode2 => "00000002";

		// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
		protected override ZString TariffDescription => string.Empty;

		// TODO: ToBeChecked: Logic Changed with the removal of AdditionalDuties in AddInfo, Need re-implementation #Victor 20160322
		protected override ZString TariffDescription2 => string.Empty;

		protected override ZString ExpectedDescriptionFromMergeOfOneLine => "LINE";

		protected override void DoMerge(Customs.Business.BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

		void SetupDataEligibleForMerging(Customs.Business.BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = testInstruction.PK;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
		}
	}
}
