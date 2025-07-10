using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(Tax_OnlyForPivot))]
class Tax_OnlyForPivotTest : BusinessObjectBaseTestCase
{
	public void TestLookups()
	{
		var tax = pivot.Taxes.AddNew().Data;
		AssertType<PLAddInfoTaxLookups>(tax.Lookups);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot.Taxes.AddNew().Data;
	}

	protected override void SetUp()
	{
		org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
	}
	OrgHeader org;
	CusClassPartPivot pivot;
}
