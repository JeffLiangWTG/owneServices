using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitConsignmentPivot))]
sealed class CusExitConsignmentPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestPackage() => AssertType<CusExitConsignmentPackage>(pivot.Package);

	public void TestContainer() => AssertType<CusExitContainer>(pivot.Container);

	public void TestConsignmentItem() => AssertType<CusExitConsignmentItem>(pivot.ConsignmentItem);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).pivot;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => pivot;

	protected override BusinessObject GetNewBusinessObject() => pivot;

	public static (CusExitHeader header, CusExitConsignment consignment, CusExitConsignmentItem item, CusExitConsignmentPivot pivot) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var (header, consignment, item) = CusExitConsignmentItemTest.GetNewBusinessObject(factory);
		var pivot = (CusExitConsignmentPivot)item.CusExitConsignmentPackagePivots.AddNew();
		pivot.CNP_CXN_Container = header.CusExitContainers.AddNew().PK;
		return (header, consignment, item, pivot);
	}

	protected override void SetUp()
	{
		base.SetUp();
		pivot = GetNewBusinessObject(Factory).pivot;
	}

	CusExitConsignmentPivot pivot;
}
