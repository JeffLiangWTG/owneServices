using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusClassPartPivot))]
class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestSupportingDocumentsCorrectType()
	{
		AssertType<SupportingDocumentCollection>(pivot.SupportingDocuments);
	}

	protected override BusinessObject GetNewBusinessObject() => pivot;

	protected override void SetUp()
	{
		var product = Factory.New<OrgSupplierPart>();
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
	}
	CusClassPartPivot pivot;
}
