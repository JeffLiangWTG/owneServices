using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS80Test : BIRDSecondaryLineUpdateTest
	{
		protected override IBIRDSecondaryLineRecord[] GetPopulatedSecondaryLineRecords()
		{
			var ens80 = new ENS80();
			ens80.TariffNumber3 = "0000000000";
			ens80.Duty = 79.77m;
			ens80.Quantity1 = 123456m;
			ens80.Unit1 = "KG";
			ens80.Quantity2 = 23456m;
			ens80.Unit2 = "L";
			ens80.Quantity3 = 234m;
			ens80.Unit3 = "BO";
			ens80.Value = 9999m;
			ens80.SpecialProgramsIndicatorPrimaryOrCountry = "E";
			ens80.SpecialProgramsIndicatorSecondary = "F";

			return new IBIRDSecondaryLineRecord[] { ens80 };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine secondaryLine, IBIRDSecondaryLineRecord lineRecord)
		{
			base.PrepareData(declaration, invoice, secondaryLine, lineRecord);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "L";
			tariff.UE_Unit3 = "BO";

			var parentLine = secondaryLine.ParentTariffLine;

			var firstSecondaryLine = parentLine.AddSecondaryInvoiceLine();
			firstSecondaryLine.JI_LineNo = 2;
			secondaryLine.JI_LineNo = 3;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS80);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
