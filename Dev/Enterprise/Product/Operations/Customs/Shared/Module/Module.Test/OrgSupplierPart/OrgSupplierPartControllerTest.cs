using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class OrgSupplierPartControllerTest : MasterFiles.Module.Testing.OrgSupplierPartControllerTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgSupplierPart part = (OrgSupplierPart)Factory.New(GetBusinessObjectType());
			part.OP_PartNum = "PartNum";
			Factory.Save();
			return part;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SupplierPart;

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

		protected override MasterFiles.Module.OrgSupplierPartController GetOrgSupplierPartController() => new OrgSupplierPartController();
	}
}
