using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class ATFExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "THIS IS EXPORT ATF";
			invoiceLine.ExportATF.US_FFLNumber = "1234-BCD9832-12349OK";
			invoiceLine.ExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			invoiceLine.ExportATF.US_Quantity = 3412m;
			invoiceLine.ExportATF.US_CategoryCode = ATFCategoryCodeList.Codes.AW;
		}

		protected override ZString ExpectedResult
		{
			get { return "PGAAT61234-BCD9832-12349OK            2000003412AW  THIS IS EXPORT ATF          "; }
		}
	}
}
