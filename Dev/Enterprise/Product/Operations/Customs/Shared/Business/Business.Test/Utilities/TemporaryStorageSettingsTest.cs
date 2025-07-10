using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TemporaryStorageSettingsTest : TestCaseWithFactory
	{
		public void TestIsUsingUCC5()
		{
			AssertEquals("TSD UCC5 not enabled for default country ER", expected: false, temporaryStorageSettings.IsUsingUCC5);

			foreach (var country in new String[] { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.Poland })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertEquals($"TSD UCC5 enabled for {country}", expected: true, temporaryStorageSettings.IsUsingUCC5);
				}
			}
		}

		public void TestIsUsingUCC6()
		{
			var registryPNTSEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabled;
			var registryPNTSEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabledDeveloperOnly;

			using (registryPNTSEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Both PNTSEnabled and registryPNTSEnabledDeveloperOnly are disabled", expected: false, temporaryStorageSettings.IsUsingUCC6);

				using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("PNTSEnabled disabled and registryPNTSEnabledDeveloperOnly enabled", expected: true, temporaryStorageSettings.IsUsingUCC6);
				}
			}

			using (registryPNTSEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (registryPNTSEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("PNTSEnabled enabled and registryPNTSEnabledDeveloperOnly disabled", expected: true, temporaryStorageSettings.IsUsingUCC6);

				using (registryPNTSEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Both PNTSEnabled and registryPNTSEnabledDeveloperOnly are enabled", expected: true, temporaryStorageSettings.IsUsingUCC6);
				}
			}
		}

		public void TestIsUsingTSRegister()
		{
			var registryRegisterEnabled = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabled;
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;

			using (registryRegisterEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Both RegisterEnabled and RegisterEnabledDeveloperOnly are disabled", expected: false, temporaryStorageSettings.IsUsingTSRegister);

				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("RegisterEnabled disabled and RegisterEnabledDeveloperOnly enabled", expected: true, temporaryStorageSettings.IsUsingTSRegister);
				}
			}

			using (registryRegisterEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("RegisterEnabled enabled and RegisterEnabledDeveloperOnly disabled", expected: true, temporaryStorageSettings.IsUsingTSRegister);

				using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Both RegisterEnabled and RegisterEnabledDeveloperOnly are enabled", expected: true, temporaryStorageSettings.IsUsingTSRegister);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			temporaryStorageSettings = new TemporaryStorageSettings();
		}
		TemporaryStorageSettings temporaryStorageSettings;
	}
}
