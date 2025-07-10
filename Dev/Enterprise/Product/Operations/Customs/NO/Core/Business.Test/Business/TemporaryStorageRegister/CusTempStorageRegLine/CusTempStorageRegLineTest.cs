using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusTempStorageRegLine))]
sealed class CusTempStorageRegLineTest : EnterpriseBusinessObjectTestCase
{
	public void TestSRL_CustomsStatus_Caption()
	{
		AssertEquals("Line Status", DataBoundResourceStrings.GetDataForProperty(regLine.SRL_CustomsStatusInfo).Caption);
	}

	public void TestSRL_CustomsStatus_ListAttribute()
	{
		CombineAssertions(() =>
			AssertEntity<CusTempStorageRegLine>()
				.HasProperty(x => x.SRL_CustomsStatus)
				.WithList($"{nameof(CusTempStorageRegLine.Lookups)}.{nameof(CusTempStorageRegLineLookups.CustomsStatusList)}"));
	}

	public void TestSRL_CustodianIdentifier_Caption()
	{
		AssertEquals("Custodian EORI", DataBoundResourceStrings.GetDataForProperty(regLine.SRL_CustodianIdentifierInfo).Caption);
	}

	public void TestSRL_GoodsOwnerIdentifier_Caption()
	{
		AssertEquals("Disp. Ent. Trader EORI", DataBoundResourceStrings.GetDataForProperty(regLine.SRL_GoodsOwnerIdentifierInfo).Caption);
	}

	public void TestCusTempStorageRegLineTransactions_CollectionCountChanged()
	{
		var line = Factory.New<CusTempStorageRegLine>();
		var transaction1 = Factory.New<CusTempStorageRegLineTransaction>();
		transaction1.SRT_PackageQty = 1;
		var transaction2 = Factory.New<CusTempStorageRegLineTransaction>();
		transaction2.SRT_PackageQty = 5;

		AssertEquals("Initially SRL_PackagesRemaining = 0.", 0, line.SRL_PackagesRemaining);
		line.CusTempStorageRegLineTransactions.Add(transaction1);
		AssertEquals("SRL_PackagesRemaining should be shifted by 1.", 1, line.SRL_PackagesRemaining);
		line.CusTempStorageRegLineTransactions.Add(transaction2);
		AssertEquals("SRL_PackagesRemaining should be shifted by 5.", 6, line.SRL_PackagesRemaining);
		transaction1.SRT_SRL = ZGuid.Empty;
		AssertEquals("SRL_PackagesRemaining should be reduced by 1.", 5, line.SRL_PackagesRemaining);
		line.CusTempStorageRegLineTransactions.Delete(transaction2);
		AssertEquals("SRL_PackagesRemaining should be reduced by 5.", 0, line.SRL_PackagesRemaining);
	}

	public void TestDependentBusinessObjectAttribute()
	{
		var attributes = TypeDescriptor.GetAttributes(GetExpectedBusinessObjectType());
		var attribute = attributes[typeof(DependentBusinessObjectAttribute)] as DependentBusinessObjectAttribute;
		CombineAssertions(() =>
		{
			AssertEquals("MasterType", typeof(CusTempStorageRegHeader), attribute.MasterType);
			AssertEquals("DetailRelationshipCollectionProperty", nameof(CusTempStorageRegHeader.CusTempStorageRegLines), attribute.DetailRelationshipCollectionProperty);
		});
	}

	public void TestSRL_PackagesRemaining_Caption()
	{
		AssertEquals("Package Count", DataBoundResourceStrings.GetDataForProperty(regLine.SRL_PackagesRemainingInfo).Caption);
	}

	public void TestHeaderRegNo() =>
			AssertEquals("Should equal Header.SRH_Reference", "TEST", regLine.HeaderRegNo);

	public void TestCodePropertyAttribute()
	{
		var attribute = typeof(CusTempStorageRegLine).GetCustomAttributes(typeof(CodePropertyAttribute), false).Single() as CodePropertyAttribute;
		AssertEquals(nameof(CusTempStorageRegLine.HeaderRegNo), attribute.PropertyName);
	}

	public void TestDescriptionPropertyAttribute()
	{
		var attribute = typeof(CusTempStorageRegLine).GetCustomAttributes(typeof(DescriptionPropertyAttribute), false).Single() as DescriptionPropertyAttribute;
		AssertEquals(nameof(CusTempStorageRegLine.HeaderRegNo), attribute.PropertyName);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetNewBusinessObject(Factory);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		return GetNewBusinessObject(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetNewBusinessObject(factory);
	}

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_ArrivalDate = ZDate.Today;
		header.SRH_PresentationDate = ZDate.Today;
		header.SRH_AppCode = "123";
		header.SRH_Status = "OK";
		header.SRH_Reference = "TEST";

		var line = factory.New<CusTempStorageRegLine>();
		header.CusTempStorageRegLines.Add(line);
		line.SRL_SRH = header.PK;
		line.SRL_LimitDate = ZDate.Today;
		line.SRL_LineNumber = 1;
		line.SRL_PackagesRemaining = 0;
		line.SRL_LocationOfGoods = "DE";

		return line;
	}

	protected override void SetUp()
	{
		base.SetUp();

		regLine = GetNewBusinessObject(Factory) as CusTempStorageRegLine;
	}
	CusTempStorageRegLine regLine;
}
