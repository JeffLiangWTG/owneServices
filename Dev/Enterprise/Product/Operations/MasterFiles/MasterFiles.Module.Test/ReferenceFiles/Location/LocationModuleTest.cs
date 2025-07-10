using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(LocationModule))]
	sealed class LocationModuleTest : ZModuleBasherTest
	{
		public void TestAllowExcelExport()
		{
			using (TestLocationModule module = new TestLocationModule())
			{
				AssertEquals("This is not required because we're using this in find boxes only", false, module.ModuleDecisionProvider.AllowExcelExport);
			}
		}

		public void TestSecurityCheckpointForPopups()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var refCountryModule = ZModuleFactory.Instance.Create(ModuleIDs.RefCountry))
			using (var refUnlocoModule = ZModuleFactory.Instance.Create(ModuleIDs.RefUNLOCO))
			using (var zoneModule = ZModuleFactory.Instance.Create(ModuleIDs.InternationalZone))
			{
				SecurityCheckpoint[] expectedSecuritycheckpoints = new SecurityCheckpoint[] {
					refCountryModule.SecurityCheckpoint,
					refUnlocoModule.SecurityCheckpoint,
					zoneModule.SecurityCheckpoint
				};

				AssertContainsExactElementsInAnyOrder(expectedSecuritycheckpoints, module.GetSecurityCheckpointForPopups());
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Location;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.Add(collection.Factory.New(typeof(RefUNLOCO)));
			collection.Add(collection.Factory.New(typeof(RefCountry)));
			collection.Add(collection.Factory.New(typeof(RefZoneHeader)));
		}

		class TestLocationModule : LocationModule
		{
			public new IModuleDecisionProvider ModuleDecisionProvider
			{
				get { return base.ModuleDecisionProvider; }
			}
		}
	}
}
