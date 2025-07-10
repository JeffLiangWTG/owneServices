using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class OrgSupplierPartControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get { return "ER"; }
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			Factory.Save();
			return part;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.SupplierPart;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var part = base.GetBusinessObjectWithoutValidationErrors() as OrgSupplierPart;
			part.OP_Desc = "Desc";
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "Supplier";
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = supplier.PK;
			return part;
		}

		public void TestSecurityCheckpoints()
		{
			var controller = GetOrgSupplierPartController();

			Env.Security.CustomsSupplierPart.IsAllowed = false;
			Env.Security.CustomsSupplierPartModify.IsAllowed = false;

			AssertEquals("For View", Env.Security.WhsConfigProductView, controller.CheckPointForViewForTesting);
			AssertEquals("For Edit", Env.Security.WhsConfigProductEdit, controller.CheckPointForEditForTesting);
			AssertEquals("For New", Env.Security.WhsConfigProductNew, controller.CheckPointForNewForTesting);
			AssertEquals("For Delete", Env.Security.WhsConfigProductDelete, controller.CheckPointForDeleteForTesting);

			Env.Security.CustomsSupplierPart.IsAllowed = true;
			Env.Security.CustomsSupplierPartModify.IsAllowed = true;

			AssertEquals("For View", Env.Security.CustomsSupplierPart, controller.CheckPointForViewForTesting);
			AssertEquals("For Edit", Env.Security.CustomsSupplierPartModify, controller.CheckPointForEditForTesting);
			AssertEquals("For New", Env.Security.CustomsSupplierPartModify, controller.CheckPointForNewForTesting);
			AssertEquals("For Delete", Env.Security.CustomsSupplierPartModify, controller.CheckPointForDeleteForTesting);
		}

		protected abstract OrgSupplierPartController GetOrgSupplierPartController();
	}
}
