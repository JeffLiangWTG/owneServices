using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS52Test : BIRDLineUpdateTest
	{
		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS52 ens52 = new ENS52();
			ens52.ChinaHongKongSWPMIndicator = SWPMList.Codes._2;
			ens52.CanadianExportCertificateSugar = "11111111";
			ens52.WoolLicense = "333333333";
			ens52.CBTPACertificationNumber = "444444444";
			ens52.MiscellaneousPermitLicenseNumber = "555555555";
			ens52.OtherDataIndicator1 = "01";
			ens52.OtherDataElement1 = "666666666";
			ens52.OtherDataIndicator2 = "01";
			ens52.OtherDataElement2 = "Y77777777777";
			ens52.MiscellaneousIndicator = "";

			return new IBIRDLineRecord[] { ens52 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = USCTariff.LumberPermitApplicable;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS52);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
