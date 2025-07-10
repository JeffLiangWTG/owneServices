using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(EventsModule))]
	sealed class EventsModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Workflow, eventsModule.LicenceCheckPointCoreForTest);
		}

		public void TestAllowNew()
		{
			AssertEquals("Should not be able to create new Customizable Workflow Event", false, eventsModule.AllowNew);
		}

		public void TestAllowDelete()
		{
			AssertEquals("Should not be able to delete existing Customizable Workflow Event", false, eventsModule.AllowDelete);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Events;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			var controlForTest = eventsModule.GetNewFilterControlForTest();
			AssertEquals("Type of the FilterControl", typeof(EventsFilterControl), controlForTest.GetType());
			controlForTest.Dispose();
		}

		public void TestGridCollection()
		{
			var collectionForTest = eventsModule.GetNewGridCollectionForTest();

			Assert(collectionForTest is ActiveBusinessObjectCollection<StmEvent>);
		}

		public void TestFilterBusinessObject()
		{
			var businessForTest = eventsModule.GetNewFilterBusinessObjectForTest();
			AssertEquals("Type of the FilterBusinessObject", typeof(EventsFilterBusinessObject), businessForTest.GetType());
		}

		#region Implementation

		EventsModuleForTest eventsModule;

		protected override void SetUp()
		{
			base.SetUp();

			eventsModule = new EventsModuleForTest();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (eventsModule != null)
			{
				eventsModule.Dispose();
			}
		}

		#endregion
	}
}
