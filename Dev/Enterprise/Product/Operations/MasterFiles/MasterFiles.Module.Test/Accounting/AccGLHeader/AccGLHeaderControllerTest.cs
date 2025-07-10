using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGLHeaderController))]
	sealed class AccGLHeaderControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccGLHeader;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var gLHeader = Factory.NewWithValidTestData<AccGLHeader>();

			Factory.Save();
			return gLHeader;
		}
	}
}
