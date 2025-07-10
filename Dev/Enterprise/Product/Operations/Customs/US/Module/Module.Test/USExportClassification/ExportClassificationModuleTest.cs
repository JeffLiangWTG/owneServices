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
	[TestedType(typeof(ExportClassificationModuleForTest))]
	sealed class ExportClassificationModuleTest : Customs.Module.Testing.ExportClassificationModuleTest
	{
		public void TestGetNewFilterControl()
		{
			using (var module = new ExportClassificationModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is USExportClassificationFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new ExportClassificationModuleForTest())
			{
				var collection = module.NewGridCollection;
				Assert("Invalid type", collection is ExportClassificationCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new ExportClassificationModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is USExportClassificationFilterBusinessObject);
			}
		}

		public void TestUSModuleID()
		{
			AssertEquals(ModuleIDs.Customs.US.USExportClassification, TestExportClassificationModule.ID);
		}

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.ExportBroker;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override Customs.Module.ExportClassificationModule GetNewExportClassificationModule() => new ExportClassificationModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USExportClassification;

		ExportClassificationModule TestExportClassificationModule => (ExportClassificationModule)testExportClassificationModule;

		sealed class ExportClassificationModuleForTest : ExportClassificationModule
		{
			public ExportClassificationModuleForTest()
			{
			}

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
