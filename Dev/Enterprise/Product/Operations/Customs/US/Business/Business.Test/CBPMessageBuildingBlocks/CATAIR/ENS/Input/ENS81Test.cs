using System;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS81Test : BIRDSecondaryLineUpdateTest
	{
		protected override IBIRDSecondaryLineRecord[] GetPopulatedSecondaryLineRecords()
		{
			var ens81 = new ENS81();
			ens81.AdditionalTariffNumber = "0000000000";
			ens81.Quantity1 = 123456m;
			ens81.Duty = 30.78m;
			ens81.Unit1 = "KG";
			ens81.Quantity2 = 23456m;
			ens81.Unit2 = "L";
			ens81.Quantity3 = 234m;
			ens81.Unit3 = "BO";
			ens81.Value = 9999m;
			ens81.SpecialProgramsIndicatorPrimaryOrCountry = "E";
			ens81.SpecialProgramsIndicatorSecondary = "F";

			return new IBIRDSecondaryLineRecord[] { ens81 };
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

			var secondSecondaryLine = parentLine.AddSecondaryInvoiceLine();
			secondSecondaryLine.JI_LineNo = 3;

			secondaryLine.JI_LineNo = 4;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS81);

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
