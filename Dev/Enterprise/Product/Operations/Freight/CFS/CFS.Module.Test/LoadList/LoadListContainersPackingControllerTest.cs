using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(LoadListContainersPackingController))]
	sealed class LoadListContainersPackingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.LoadListContainersPacking;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CFSLoadListConsol obj = Factory.New<CFSLoadListConsol>();
			Factory.Save();
			return obj;
		}
	}
}
