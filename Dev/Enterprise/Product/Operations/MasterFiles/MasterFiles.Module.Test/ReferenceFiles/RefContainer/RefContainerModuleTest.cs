using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefContainerModule))]
	sealed class RefContainerModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefContainer;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			refContainer = new RefContainerModuleForTest();
			IFilterControl controlForTest = refContainer.GetNewFilterControlForTest();
			Assert(controlForTest is RefContainerFilterControl);
			controlForTest.Dispose();
			refContainer.Dispose();
		}

		public void TestGridCollection()
		{
			refContainer = new RefContainerModuleForTest();
			IBusinessObjectCollection collectionForTest = refContainer.GetNewGridCollectionForTest();
			Assert(collectionForTest is ActiveBusinessObjectCollection<RefContainer>);
			refContainer.Dispose();
		}

		public void TestFilterBusinessObject()
		{
			refContainer = new RefContainerModuleForTest();
			FilterBusinessObject businessForTest = refContainer.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
			refContainer.Dispose();
		}

		#region Implementation

		RefContainerModuleForTest refContainer;

		#endregion
	}
}
