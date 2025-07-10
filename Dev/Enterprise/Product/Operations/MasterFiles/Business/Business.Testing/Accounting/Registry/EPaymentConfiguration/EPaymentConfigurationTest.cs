using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentConfiguration))]
	sealed class EPaymentConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestPropertyReadOnlyness()
		{
			var config = new EPaymentConfiguration();
			Assert(config.CountryCodeInfo.ReadOnly);
			Assert(config.CountryDescriptionInfo.ReadOnly);
			Assert(!config.OFXEPaymentEnabledInfo.ReadOnly);
		}

		public void TestCanDelete()
		{
			var config = new EPaymentConfiguration();
			Assert(!config.CanDelete);
			AssertEquals("Cannot delete the default E-Payment Configuration.", config.ReasonForNotAbleToDelete);
		}

		public void TestInvalidCompanyCountryDoesNotCreateDefaultConfiguration()
		{
			var badCompany = Factory.NewWithValidTestData<GlbCompany>();
			badCompany.GC_Code = "BAD";
			badCompany.GC_RN_NKCountryCode = "00";
			Factory.Save();

			var registry = new EPaymentConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
			var badCompanyConfigurationCollection = registry.GetValueWithoutFallback(badCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertEquals("Should not default a config if no country is found", 0, badCompanyConfigurationCollection.Count);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EPaymentConfiguration
			{
				CountryCode = Core.Constants.CountryCodes.Australia,
				CountryDescription = "Australia",
				OFXEPaymentEnabled = true
			};
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectToClone();

		#endregion
	}
}
