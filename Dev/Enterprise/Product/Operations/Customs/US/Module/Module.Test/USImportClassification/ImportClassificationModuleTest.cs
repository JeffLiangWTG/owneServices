using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ImportClassificationModule))]
	sealed class ImportClassificationModuleTest : Customs.Module.Testing.ImportClassificationModuleTest
	{
		public void TestGetNewFilterControl()
		{
			using (var module = new ImportClassificationModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is USImportClassificationFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new ImportClassificationModuleForTest())
			{
				var collection = module.NewGridCollection;
				Assert("Invalid type", collection is ImportClassificationCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new ImportClassificationModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is USImportClassificationFilterBusinessObject);
			}
		}

		public void TestUSModuleID()
		{
			AssertEquals(ModuleIDs.Customs.US.USImportClassification, TestImportClassificationModule.ID);
		}

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.ImportBroker;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override Customs.Module.ImportClassificationModule GetNewImportClassificationModule() => new ImportClassificationModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USImportClassification;

		ImportClassificationModule TestImportClassificationModule => (ImportClassificationModule)testImportClassificationModule;

		sealed class ImportClassificationModuleForTest : ImportClassificationModule
		{
			public ImportClassificationModuleForTest()
			{
			}

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
