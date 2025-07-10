using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefEquipmentModule))]
	sealed class RefEquipmentModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefEquipment;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			refEquipment = new RefEquipmentModuleForTest();
			IFilterControl controlForTest = refEquipment.GetNewFilterControlForTest();
			Assert(controlForTest is RefEquipmentFilterControl);
			controlForTest.Dispose();
			refEquipment.Dispose();
		}

		public void TestGridCollection()
		{
			refEquipment = new RefEquipmentModuleForTest();
			Assert(refEquipment.GetNewGridCollectionForTest() is ActiveBusinessObjectCollection<RefEquipment>);
			refEquipment.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			refEquipment = new RefEquipmentModuleForTest();
			Assert(refEquipment.GetNewFilterBusinessObjectForTest() is FilterBusinessObject);
			refEquipment.Dispose();
		}

		#region Implementation

		RefEquipmentModuleForTest refEquipment;

		#endregion
	}
}
