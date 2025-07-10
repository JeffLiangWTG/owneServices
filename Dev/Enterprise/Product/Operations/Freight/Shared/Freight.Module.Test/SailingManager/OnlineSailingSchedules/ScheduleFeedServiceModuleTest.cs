using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(OnlineSailingSchedulesModuleForTest))]
	sealed class ScheduleFeedServiceModuleTest : ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.OnlineSailingSchedules, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.OnlineSailingSchedules, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(OnlineSailingSchedulesController), Module.GetNewController().GetType());
		}

		public void TestUri_FromRegistry()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost"))
			{
				var glowURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertNotEquals("GlowRegistry.Instance.GlowPortalsUri value should be not be null", null, glowURL);

				using (var module = new OnlineSailingSchedulesModule())
				{
					var url = module.Url;
					AssertNotEquals("OnlineSailingSchedulesModule.Url should be not null", null, url);
				}
			}
		}

		public void TestUri_FromBase()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			{
				var glowURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				AssertEquals("GlowRegistry.Instance.GlowPortalsUri value should be null", null, glowURL);

				using (var module = new OnlineSailingSchedulesModule())
				{
					var url = module.Url;
					AssertEquals("OnlineSailingSchedulesModule.Url should be null", null, url);
				}
			}
		}

		#region Test Classes

		class OnlineSailingSchedulesModuleForTest : OnlineSailingSchedulesModule
		{
			public new ZPopupController GetNewController()
			{
				return base.GetNewController();
			}
		}

		#endregion

		#region Implementation

		OnlineSailingSchedulesModuleForTest Module
		{
			get { return module ?? (module = new OnlineSailingSchedulesModuleForTest()); }
		}
		OnlineSailingSchedulesModuleForTest module;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OnlineSailingSchedules;
		}

		protected override void TearDown()
		{
			base.TearDown();

			module?.Dispose();
		}

		#endregion
	}
}
