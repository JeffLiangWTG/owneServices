using System;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public static class CustomsMessageStatusProviderFactory
	{
		public static ICustomsMessageStatusProvider New()
		{
			GlbCompany company;
			RefCountry country;

			if ((company = GlbCompany.CurrentCompany) != null && (country = company.Country) != null)
			{
				return New(country.RN_Code);
			}
			else
			{
				return New("");
			}
		}

		public static ICustomsMessageStatusProvider New(string countryCode)
		{
#if DEBUG
			if (Globals.IsTest && useCustomsMessageStatusProviderFactory_ForTesting)
			{
				return DummyCustomsMessageStatusProvider.Instance;
			}
#endif

			switch (countryCode)
			{
				case Constants.CountryCodes.Australia:
					return (ICustomsMessageStatusProvider)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusSeaManOBLHeaderMessageStatus>(), Array.Empty<object>());

				default:
					return new NullCustomsMessageStatusProvider();
			}
		}

		[ThreadStatic]
		public static bool useCustomsMessageStatusProviderFactory_ForTesting;
	}
}
