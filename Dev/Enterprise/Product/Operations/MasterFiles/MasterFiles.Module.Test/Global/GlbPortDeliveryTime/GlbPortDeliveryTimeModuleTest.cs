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
	[TestedType(typeof(GlbPortDeliveryTimeModule))]
	sealed class GlbPortDeliveryTimeModuleTest : ZModuleBasherTest
	{
		public GlbPortDeliveryTimeModuleTest() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbPortDeliveryTime;
		}

		public void TestCheckpoints()
		{
			using (GlbPortDeliveryTimeModule module = new GlbPortDeliveryTimeModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GlbPortDeliveryTime, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (GlbPortDeliveryTimeModuleForTest module = new GlbPortDeliveryTimeModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is GlbPortDeliveryTimeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (GlbPortDeliveryTimeModuleForTest module = new GlbPortDeliveryTimeModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is GlbPortDeliveryTimeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (GlbPortDeliveryTimeModuleForTest module = new GlbPortDeliveryTimeModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is GlbPortDeliveryTimeFilterBusinessObject);
			}
		}

		#endregion
	}
}
