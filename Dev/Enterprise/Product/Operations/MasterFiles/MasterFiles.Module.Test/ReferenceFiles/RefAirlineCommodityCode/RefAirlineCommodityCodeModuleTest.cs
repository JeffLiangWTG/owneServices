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
	[TestedType(typeof(RefAirlineCommodityCodeModule))]
	class RefAirlineCommodityCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefAirlineCommodityCode;
		}

		public void TestModuleID()
		{
			using (RefAirlineCommodityCodeModule module = new RefAirlineCommodityCodeModule())
			{
				AssertEquals("ModuleID", ModuleIDs.RefAirlineCommodityCode, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (RefAirlineCommodityCodeModule module = new RefAirlineCommodityCodeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Commodity, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected virtual RefAirlineCommodityCodeModule GetNewAirlineCommodityCodeModule()
		{
			return new RefAirlineCommodityCodeModule();
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefAirlineCommodityCodeModuleForTest module = new RefAirlineCommodityCodeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefAirlineCommodityCodeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefAirlineCommodityCodeModuleForTest module = new RefAirlineCommodityCodeModuleForTest())
			{
				IBusinessObjectCollection airlineCommodityCodesCollection = module.NewGridCollection;
				Assert("Invalid type", airlineCommodityCodesCollection is RefAirlineCommodityCodeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefAirlineCommodityCodeModuleForTest module = new RefAirlineCommodityCodeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefAirlineCommodityCodeFilterBusinessObject);
			}
		}

		#endregion
	}
}
