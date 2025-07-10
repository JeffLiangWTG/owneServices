using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SupplierPart;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var part = base.GetBusinessObjectWithoutValidationErrors() as OrgSupplierPart;
			part.OP_Desc = "Desc";
			return part;
		}
	}
}
