using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class BCR02Test : BIRDLineUpdateTest
	{
		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			BCR02 bcr02 = new BCR02();
			bcr02.CountryOfOrigin = "AU";
			bcr02.TariffNumber = "0000000000";
			bcr02.ManufacturerIDCode = "AUABCEXP6390ALE";
			bcr02.UltimateConsignee = "12-1234567CC";

			return new IBIRDLineRecord[] { bcr02 };
		}

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
				{
					"LineItemValue"
				};
		}

		protected override EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new BorderCargoReleaseMessageBuilder(declaration.ActiveEntryHeaders.CargoReleaseEntry, UpdateActionCode.Add);
		}

		protected override Type GetTypeOfMessageBlock()
		{
			return typeof(BCR02);
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUABCEXP6390ALE", GlbCompany.CurrentCompany.Country);

			OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567CC", GlbCompany.CurrentCompany.Country);

			Factory.Save();
		}
	}
}
