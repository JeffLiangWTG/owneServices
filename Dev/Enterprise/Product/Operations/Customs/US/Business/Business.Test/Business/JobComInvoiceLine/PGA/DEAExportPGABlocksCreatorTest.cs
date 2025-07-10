using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class DEAExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			declaration.US_InbondType = InbondTypeList.Codes.MerchandiseNOTShippedInbond;
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var exportDEA = invoiceLine.DEAHeaders.AddNew();
			exportDEA.US_DrugCode = "87EF";
			exportDEA.US_Weight = 1000.25m;
			exportDEA.US_UnitOfMeasure = Core.Constants.Weight.Kilograms;
			exportDEA.US_PermitNumber = "AB12937";
			exportDEA.US_RegistrationNumber = "83741FL34";
		}

		protected override ZString ExpectedResult
		{
			get { return "PGADEA87EF00000010002500KG EAB1293783741FL34                                    "; }
		}
	}
}
