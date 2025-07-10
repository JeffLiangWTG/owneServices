using System.Collections;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GUI
{
	internal class CommercialInvoiceFormLayoutProvider : ICommercialInvoiceFormLayoutProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Pending adjustment of CW1161")]
		internal static ICommercialInvoiceFormLayoutProvider GetLayoutProvider(BaseJobComInvoiceHeader invoice)
		{
			ICommercialInvoiceFormLayoutProvider provider = null;

			if (invoice != null)
			{
				var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(invoice.CountryCode);
				var providers = ObjectFactory.Get<Hashtable>("CommercialInvoiceFormLayoutProviders");

				if (string.IsNullOrWhiteSpace(countryCode) || countryCode == Core.Constants.CountryCodes._TemplateCountryName_ || invoice.GetType() == typeof(BaseJobComInvoiceHeader))
				{
					var objectHandle = (ObjectHandle)providers?["Default"];
					provider = (ICommercialInvoiceFormLayoutProvider)objectHandle?.GetObject();
				}
				else
				{
					var objectHandle = (ObjectHandle)providers?[countryCode];
					provider = (ICommercialInvoiceFormLayoutProvider)objectHandle?.GetObject();

					if (provider == null && ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode))
					{
						objectHandle = (ObjectHandle)providers[Core.Constants.CountryCodes.EuropeanUnion];
						provider = (ICommercialInvoiceFormLayoutProvider)objectHandle?.GetObject();
					}
					else if (provider == null && ObjectFactory.Get<Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
					{
						objectHandle = (ObjectHandle)providers["AsycudaCustoms"];
						provider = (ICommercialInvoiceFormLayoutProvider)objectHandle?.GetObject();
					}
					if (provider == null)
					{
						objectHandle = (ObjectHandle)providers?["Default"];
						provider = (ICommercialInvoiceFormLayoutProvider)objectHandle?.GetObject();
					}
				}
			}
			return provider;
		}

		public IPanelLayoutProvider GetInvoiceHeaderDetailsLayout(BaseJobComInvoiceHeader invoice) => new CommercialInvoice.InvoiceHeaderDetailsLayout();
	}
}
