using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitConsignmentItem))]
sealed class CusExitConsignmentItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestCusExitReportItems()
	{
		var (_, _, item) = GetNewBusinessObject(Factory);
		AssertType<CusExitReportItemCollection<CusExitReportItem>>(item.CusExitReportItems);
	}

	public void TestCusExitConsignmentPivots()
	{
		var (_, _, item) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentPivots);
	}

	public void TestCusExitConsignmentPackagePivots() => CombineAssertions(() =>
	{
		var (_, _, item) = GetNewBusinessObject(Factory);
		var cusExitConsignmentPackagePivots = item.CusExitConsignmentPackagePivots;
		AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(cusExitConsignmentPackagePivots);
		AssertEquals("SupportsSorting", true, cusExitConsignmentPackagePivots.SupportsSorting);
		AssertEquals("SortDirection", System.ComponentModel.ListSortDirection.Ascending, cusExitConsignmentPackagePivots.SortDirection);
		AssertEquals("SortInformation.PropertyName", nameof(CusExitConsignmentPivot.SequenceNumber), cusExitConsignmentPackagePivots.SortInformation.PropertyName);
	});

	public void TestCusExitConsignmentContainerPivots()
	{
		var (_, _, item) = GetNewBusinessObject(Factory);
		AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentContainerPivots);
	}

	public void TestAdditionalInfos() => CombineAssertions(() =>
	{
		var (_, _, item) = GetNewBusinessObject(Factory);
		AssertType<AdditionalInfoCollection<AdditionalInfo>>("AdditionalInfoCollection", item.AdditionalInfos);
		AssertType<AdditionalInfo>("AdditionalInfoCollection should use PL AdditionalInfo", item.AdditionalInfos.AddNew());
	});

	public void TestGetCusSupportingInfoTypes()
	{
		var (_, _, item) = GetNewBusinessObject(Factory);
		var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)item).GetCusSupportingInfoTypes();
		AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).item;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).item;

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).item;

	public static (CusExitHeader header, CusExitConsignment consignment, CusExitConsignmentItem item) GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var (consignment, header) = CusExitConsignmentTest.GetNewBusinessObject(factory);
		var item = consignment.CusExitConsignmentItems.AddNew();
		item.CCI_LineNumber = 1;
		return (header, consignment, item);
	}
}
