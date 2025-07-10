using System;
using Enterprise.Environment;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Module.Filters;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test
{
	class TelematicsPreDriveChecklistTemplatesModuleTest : TestCase
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
			AssertEquals(Env.Security.TelematicsPreDriveChecklistHeaderTemplate, module.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
		}

		public void TestGridCollectionIsTelPreDriveChecklistTemplateHeaderCollection()
		{
			AssertType<TelPreDriveChecklistTemplateHeaderCollection>(module.GridCollection);
		}

		public void TestFilterBizOIsTelPreDriveChecklistTemplateHeaderFilterBusinessObject()
		{
			AssertType<TelPreDriveChecklistTemplateHeaderFilterBusinessObject>(module.FilterBusinessObject);
		}

		public void TestFilterControlIsTelPreDriveChecklistTemplateHeaderFilterControl()
		{
			using (var control = module.GetNewFilterControlForGrid() as IDisposable)
			{
				AssertType<TelPreDriveChecklistTemplateHeaderFilterControl>(control);
			}
		}

		TelematicsPreDriveChecklistTemplatesModule module;

		protected override void SetUp()
		{
			base.SetUp();

			module = new TelematicsPreDriveChecklistTemplatesModule();
		}

		protected override void TearDown()
		{
			module?.Dispose();
			module = null;

			base.TearDown();
		}
	}
}
