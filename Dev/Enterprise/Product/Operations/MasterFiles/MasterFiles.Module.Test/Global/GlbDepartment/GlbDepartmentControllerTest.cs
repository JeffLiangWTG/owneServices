using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbDepartmentController))]
	sealed class GlbDepartmentControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbDepartment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			GlbDepartment dept = Factory.New<GlbDepartment>();
			Factory.Save();
			return dept;
		}
	}
}
