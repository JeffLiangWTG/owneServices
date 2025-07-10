using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class AMSExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_ExportCertificateNo = "ABC1234567";
		}

		protected override ZString ExpectedResult
		{
			get { return "PGAAM1ABC1234567                                                                "; }
		}
	}
}
