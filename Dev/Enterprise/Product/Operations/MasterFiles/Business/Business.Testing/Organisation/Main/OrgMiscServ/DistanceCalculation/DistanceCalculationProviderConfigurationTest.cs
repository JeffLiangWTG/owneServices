using CargoWise.ComponentModel;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DistanceCalculationProviderConfiguration))]
	class DistanceCalculationProviderConfigurationTest : RegistryBusinessObjectTemplateTestCase<DistanceCalculationProviderConfiguration>
	{
		#region Implementation

		protected override DistanceCalculationProviderConfiguration GetBusinessObjectToClone()
		{
			return new DistanceCalculationProviderConfiguration();
		}

		protected override DistanceCalculationProviderConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new DistanceCalculationProviderConfiguration BizObj
		{
			get { return base.BizObj; }
		}

		#endregion

		public void TestGetDefault()
		{
			DistanceCalculationProviderConfiguration config = DistanceCalculationProviderConfiguration.GetDefault();
			AssertEquals("Defaul provider", DistanceCalculationConstants.Providers.CargoWise, config.Provider);
			AssertEquals("Defaul version", "", config.Version);
			AssertEquals("Defaul method", "", config.CalculationMethod);
		}

		public void TestProviderValidation()
		{
			DistanceCalculationProviderConfiguration config = DistanceCalculationProviderConfiguration.GetDefault();
			AssertEquals("Precondition - no validation errors", false, config.ProviderInfo.HasErrors());

			config.Provider = DistanceCalculationConstants.Providers.PCMiler;
			AssertEquals("No errors", false, config.ProviderInfo.HasErrors());

			config.Provider = "XXX";
			AssertEquals("Validation error", true, config.ProviderInfo.HasErrors());
		}

		public void TestVersionValidation()
		{
			DistanceCalculationProviderConfiguration config = DistanceCalculationProviderConfiguration.GetDefault();
			AssertEquals("Precondition - no validation errors", false, config.VersionInfo.HasErrors());

			config.Provider = DistanceCalculationConstants.Providers.PCMiler;
			foreach (CodeDescriptionPair pair in DistanceCalculationLists.Instance.PCMilerVersions)
			{
				config.Version = pair.Code;
				AssertEquals("No errors", false, config.VersionInfo.HasErrors());
			}

			config.Version = "99";
			AssertEquals("No error - users are allowed to enter any version", false, config.VersionInfo.HasErrors());
		}

		public void TestCalculationMethodValidation()
		{
			DistanceCalculationProviderConfiguration config = DistanceCalculationProviderConfiguration.GetDefault();
			AssertEquals("Precondition - no validation errors", false, config.CalculationMethodInfo.HasErrors());

			config.Provider = DistanceCalculationConstants.Providers.PCMiler;
			foreach (CodeDescriptionPair pair in DistanceCalculationLists.Instance.PCMilerCalculationMethods)
			{
				config.CalculationMethod = pair.Code;
				AssertEquals("No errors", false, config.CalculationMethodInfo.HasErrors());
			}

			config.CalculationMethod = "XXX";
			AssertEquals("Validation error", true, config.CalculationMethodInfo.HasErrors());
		}

		public void TestPropertiesReadOnly()
		{
			DistanceCalculationProviderConfiguration config = DistanceCalculationProviderConfiguration.GetDefault();

			foreach (CodeDescriptionPair pair in DistanceCalculationLists.Instance.Providers)
			{
				config.Provider = pair.Code;
				AssertEquals("Provider is not readonly", false, config.ProviderInfo.ReadOnly);
				AssertEquals("Version readonly depends on provider selected", config.Provider != DistanceCalculationConstants.Providers.PCMiler, config.VersionInfo.ReadOnly);
				AssertEquals("Method readonly depends on provider selected", config.Provider != DistanceCalculationConstants.Providers.PCMiler, config.CalculationMethodInfo.ReadOnly);
			}
		}

		public void TestDefaultsWhenSettingProvider()
		{
			DistanceCalculationProviderConfiguration config = new DistanceCalculationProviderConfiguration();

			config.Version = "99";
			config.CalculationMethod = "XXX";

			AssertEquals("Precondition", "99", config.Version);
			AssertEquals("Precondition", "XXX", config.CalculationMethod);

			config.Provider = DistanceCalculationConstants.Providers.PCMiler;
			AssertEquals("Default code from the list", config.VersionList.DefaultCode, config.Version);
			AssertEquals("Default code from the list", config.CalculationMethodList.DefaultCode, config.CalculationMethod);

			config.Provider = DistanceCalculationConstants.Providers.CargoWise;
			AssertEquals("Must be blank", "", config.Version);
			AssertEquals("Must be blank", "", config.CalculationMethod);
		}
	}
}
