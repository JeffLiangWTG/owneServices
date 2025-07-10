using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class LocatorPLXTest : LocatorTestBase<LocatorPLX, EDIMessage>
{
	protected override string ExpectedApplicationCodes => ApplicationCodeList.Codes.PLCustomsExitControl;

	protected override string MessageApplicationCode => ApplicationCodeList.Codes.PLCustomsExitControl;

	protected override BusinessObject CreateLinkedObject()
	{
		var cusExitHeader = Factory.New<CusExitHeader>();
		var cusExitReport = cusExitHeader.CusExitReports.AddNew();
		var cusExitConsignment = cusExitHeader.CusExitConsignments.AddNew();
		cusExitReport.CER_CXC_Consignment = cusExitConsignment.PK;

		return cusExitReport;
	}

	protected override void AllocateLRN(BusinessObject linkedObject, string lrn) =>
		((CusExitReport)linkedObject).Consignment.CXC_LocalReference = lrn;

	protected override void AllocateMRN(BusinessObject linkedObject, string mrn) =>
		((CusExitReport)linkedObject).Consignment.CXC_MovementReference = mrn;
}
