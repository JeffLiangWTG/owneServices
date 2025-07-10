using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPackTypeModule))]
	sealed class RefPackTypeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefPackType;
		}

		public void TestCheckpoints()
		{
			using (RefPackTypeModule module = new RefPackTypeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.RefPackType, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefPackTypeModuleForTest module = new RefPackTypeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefPackTypeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefPackTypeModuleForTest module = new RefPackTypeModuleForTest())
			{
				IBusinessObjectCollection packTypeCollection = module.NewGridCollection;
				Assert("Invalid type", packTypeCollection is RefPackTypeCollection);
				AssertEquals(true, ((RefPackTypeCollection)packTypeCollection).ContainsCode(RefPackTypeCollection.ReservedContainerType));
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefPackTypeModuleForTest module = new RefPackTypeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefPackTypeFilterBusinessObject);
			}
		}

		#endregion

	}
}
