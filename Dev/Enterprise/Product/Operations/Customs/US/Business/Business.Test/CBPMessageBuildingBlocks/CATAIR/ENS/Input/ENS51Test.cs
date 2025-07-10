using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS51Test : BIRDLineUpdateTest
	{
		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS51 ens51 = new ENS51();
			ens51.DateOfExportationTextiles = new ZDate(2009, 1, 3);
			ens51.VisaNumber = "9AU123456";
			ens51.CategoryNumber = "339";
			ens51.VisaQuantity = 50m;
			ens51.VisaUnitOfMeasure = "KG";
			ens51.AgricultureLicenseNumber = "9-AA-333-4";
			ens51.CottonCertificateNumberOrganicExemptionCertificateNumber = "999999999";

			return new IBIRDLineRecord[] { ens51 };
		}

		protected override System.Type GetTypeOfMessageBlock() => typeof(ENS51);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"LumberPermitNumber",//This is not populated and the permit number along with other details are sent in a different record (52)
			};
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
