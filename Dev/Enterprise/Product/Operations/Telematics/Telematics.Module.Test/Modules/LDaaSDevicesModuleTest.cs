using System;
using Enterprise.Environment;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Module.Filters;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test
{
	class LDaaSDevicesModuleTest : TestCase
	{
		public void TestAllowsView()
		{
			AssertEquals(true, module.AllowView);
		}

		public void TestAllowsEdit()
		{
			AssertEquals(true, module.AllowEdit);
		}

		public void TestDoesNotAllowNew()
		{
			AssertEquals(false, module.AllowNew);
		}

		public void TestDoesNotAllowDelete()
		{
			AssertEquals(false, module.AllowDelete);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.LDaaSDevices, module.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
		}

		public void TestGridCollectionIsGlbDeviceCollection()
		{
			AssertType<GlbDeviceCollection>(module.GridCollection);
		}

		public void TestFilterBizOIsGlbDeviceFilter()
		{
			AssertType<GlbDeviceFilterBusinessObject>(module.FilterBusinessObject);
		}

		public void TestFilterControlIsGlbDeviceFilter()
		{
			using (var control = module.GetNewFilterControlForGrid() as IDisposable)
			{
				AssertType<GlbDeviceFilterControl>(control);
			}
		}

		LDaaSDevicesModule module;

		protected override void SetUp()
		{
			base.SetUp();

			module = new LDaaSDevicesModule();
		}

		protected override void TearDown()
		{
			module?.Dispose();
			module = null;

			base.TearDown();
		}
	}
}
