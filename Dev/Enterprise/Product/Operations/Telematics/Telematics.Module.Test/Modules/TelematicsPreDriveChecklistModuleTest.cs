using System;
using Enterprise.Environment;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Module.Filters;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test.Modules
{
	class TelematicsPreDriveChecklistModuleTest : TestCase
	{
		public void TestAllowsView()
		{
			AssertEquals(true, module.AllowView);
		}

		public void TestAllowsEdit()
		{
			AssertEquals(false, module.AllowEdit);
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
			AssertEquals(Env.Security.TelematicsPreDriveChecklistHeader, module.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
		}

		public void TestGridCollectionIsTelPreDriveChecklistTemplateHeaderCollection()
		{
			AssertType<TelPreDriveChecklistHeaderCollection>(module.GridCollection);
		}

		public void TestFilterBizOIsTelPreDriveChecklistTemplateHeaderFilterBusinessObject()
		{
			AssertType<TelPreDriveChecklistHeaderFilterBusinessObject>(module.FilterBusinessObject);
		}

		public void TestFilterControlIsTelPreDriveChecklistTemplateHeaderFilterControl()
		{
			using (var control = module.GetNewFilterControlForGrid() as IDisposable)
			{
				AssertType<TelPreDriveChecklistHeaderFilterControl>(control);
			}
		}

		TelematicsPreDriveChecklistModule module;

		protected override void SetUp()
		{
			base.SetUp();

			module = new TelematicsPreDriveChecklistModule();
		}

		protected override void TearDown()
		{
			module?.Dispose();
			module = null;

			base.TearDown();
		}
	}
}
