using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitConsignmentPackage))]
sealed class CusExitConsignmentPackageTest : EnterpriseBusinessObjectTestCase
{
	public void TestCusExitReportItems()
	{
		var consignmentPackage = (CusExitConsignmentPackage)GetNewBusinessObject();
		AssertType<ExitControlBase.Business.CusExitReportItemCollection<CusExitReportItem>>(consignmentPackage.CusExitReportItems);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewConsignmentPackage();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewConsignmentPackage();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => Factory.NewWithValidTestData<CusExitConsignmentPackage>();

	CusExitConsignmentPackage GetNewConsignmentPackage()
	{
		var exitHeader = CusExitHeaderTest.GetNewBusinessObject(Factory);
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		var exitConsignmentItem = exitConsignment.CusExitConsignmentItems.AddNew();
		exitConsignmentItem.CCI_LineNumber = 1;
		var exitConsignmentPackagePivots = exitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		return (CusExitConsignmentPackage)exitConsignmentPackagePivots.Package;
	}
}
