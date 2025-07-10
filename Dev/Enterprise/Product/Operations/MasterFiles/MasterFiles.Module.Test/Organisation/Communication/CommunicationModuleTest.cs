using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CommunicationModule))]
	sealed class CommunicationModuleTest : ZModuleBasherTest
	{
		public void TestShowRecentItems()
		{
			using (var communicationModule = new CommunicationModuleForTesting())
			{
				communicationModule.Header = null;
				AssertEquals(true, communicationModule.ShowRecentItems_Exposed);

				communicationModule.Header = Factory.New<OrgHeader>();
				AssertEquals(false, communicationModule.ShowRecentItems_Exposed);
			}
		}

		#region Implementation

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			var factory = collection.Factory;
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			org1.SalesCalls.AddNew();
			org1.SalesCalls.AddNew();

			var org2 = factory.NewWithValidTestData<OrgHeader>();
			org2.SalesCalls.AddNew();

			factory.Save();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Communication;
		}

		class CommunicationModuleForTesting : CommunicationModule
		{
			internal bool ShowRecentItems_Exposed
			{
				get { return ShowRecentItems; }
			}
		}

		#endregion
	}
}
