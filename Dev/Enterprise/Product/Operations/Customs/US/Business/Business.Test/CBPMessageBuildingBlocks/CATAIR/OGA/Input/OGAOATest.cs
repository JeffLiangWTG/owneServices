using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAOATest : TestCaseWithFactory
	{
		public void TestIBIRDOGAStartRecord()
		{
			var oa = new OGAOA();
			oa.OtherAgencyDeclaration = "FC0";
			oa.OtherAgencyDeclaration1 = "DT0";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			((IBIRDOGAStartRecord)oa).SetOGAIndicator(invoiceLine);

			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FCCIndicator);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_DOTIndicator);
			AssertEquals(ZString.Empty, invoiceLine.US_FDAIndicator);
		}
	}
}
