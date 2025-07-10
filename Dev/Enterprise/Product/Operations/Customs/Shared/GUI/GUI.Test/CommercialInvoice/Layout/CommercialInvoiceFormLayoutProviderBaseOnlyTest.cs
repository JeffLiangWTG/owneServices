using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.GUI.CommercialInvoice;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceFormLayoutProvider))]
	sealed class CommercialInvoiceFormLayoutProviderBaseOnlyTest : CommercialInvoiceFormLayoutProviderAbstractTest<CommercialInvoiceFormLayoutProvider, BaseJobComInvoiceHeader>
	{
		[AsycudaCustomsCountries(Core.Constants.CountryCodes.Congo)]
		public void TestObjectFactoryContents()
		{
			CombineAssertions(() =>
			{
				var provider = CommercialInvoiceFormLayoutProvider.GetLayoutProvider(null);
				AssertNull("Provider for NULL", provider);

				provider = CommercialInvoiceFormLayoutProvider.GetLayoutProvider(Factory.New<BaseJobComInvoiceHeader>());
				AssertType<CommercialInvoiceFormLayoutProvider>("Provider for BaseJobComInvoiceHeader", provider);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					AssertType<CommercialInvoiceFormLayoutProvider>("Provider for AU", provider);
				}

				foreach (var expectedProviderType in expectedCommercialInvoiceFormLayoutProviders)
				{
					var countryCode = expectedProviderType.Key;
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						provider = CommercialInvoiceFormLayoutProvider.GetLayoutProvider(Factory.New<BaseJobComInvoiceHeader>());
						AssertEquals($"{countryCode} has expected provider", expectedProviderType.Value, provider.GetType().FullName);
					}
				}
			});
		}

		readonly Dictionary<string, string> expectedCommercialInvoiceFormLayoutProviders = new Dictionary<string, string>()
		{
			{ Core.Constants.CountryCodes.Belgium, "Enterprise.Customs.EU.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Brazil, "Enterprise.Customs.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.China, "Enterprise.Customs.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Congo, "Enterprise.Customs.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.France, "Enterprise.Customs.EU.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Germany, "Enterprise.Customs.EU.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Ireland, "Enterprise.Customs.IE.GUI.CommercialInvoiceFormLayoutProvider" },
			{ Core.Constants.CountryCodes.Italy, "Enterprise.Customs.EU.GUI.CommercialInvoiceFormLayoutProvider" },
		};

		protected override Type ExpectedCommercialInvoiceHeaderDetailsLayoutType => typeof(InvoiceHeaderDetailsLayout);
	}
}
