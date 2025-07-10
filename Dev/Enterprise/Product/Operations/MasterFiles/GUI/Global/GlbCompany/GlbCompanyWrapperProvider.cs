using System.Collections;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class GlbCompanyWrapperProvider : IGlbCompanyWrapperProvider
	{
		public static GlbCompanyWrapperProvider GetProvider(ZString actualCountry)
		{
			var result = new GlbCompanyWrapperProvider();

			var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(actualCountry);
			var writers = ObjectFactory.Get<Hashtable>("GlbCompanyWrapperProviders");
			var objectHandle = (ObjectHandle)writers[countryCode];
			if (objectHandle == null && ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCode))
			{
				objectHandle = (ObjectHandle)writers[Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda];
			}
			if (objectHandle != null)
			{
				result = (GlbCompanyWrapperProvider)objectHandle.GetObject();
			}

			return result;
		}

		public GlbCompanyWrapper GetWrapper(GlbCompany company) => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);

		public virtual IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();

		public virtual string PluginTextOverride => null;

		IGlbCompanyWrapper IGlbCompanyWrapperProvider.GetWrapper(IGlbCompany company)
		{
			return GetWrapper(company as GlbCompany);
		}
	}
}
