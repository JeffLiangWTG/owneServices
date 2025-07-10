using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class GeneralCountrySingleProductLookupControllerTest : MasterFiles.Module.Testing.OrgSupplierPartControllerTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgSupplierPart part = (OrgSupplierPart)Factory.New(GetBusinessObjectType());
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			Factory.Save();
			return part;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SupplierPart;

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

		protected override MasterFiles.Module.OrgSupplierPartController GetOrgSupplierPartController() => new OrgSupplierPartController();
	}
}
