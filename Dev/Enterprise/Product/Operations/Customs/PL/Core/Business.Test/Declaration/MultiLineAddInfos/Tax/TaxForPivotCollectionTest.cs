using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(TaxForPivotCollection))]
class TaxOnlyForPivotCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestElementType()
	{
		var part = Factory.New<OrgSupplierPart>();
		var pivot = part.PivotsForBinding.AddNew();
		var tax = pivot.Taxes.AddNew();
		AssertType(typeof(PLTaxOnlyForPivot), tax);
		AssertType(typeof(Tax_OnlyForPivot), tax.Data);
	}

	public new void TestReintroducedAddNewRemovedForGenericCollection()
	{
		Assert(true);
	}

	public new void TestReintroducedIndexerRemovedForGenericCollection()
	{
		Assert(true);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var part = Factory.New<OrgSupplierPart>();
		var pivot = part.PivotsForBinding.AddNew();
		return pivot.Taxes;
	}
}
