using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Customs.Business
{
	public static class CusIntegrationTypeDecider
	{
		public static ICusIntegration CusIntegration(BaseJobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration can not be null");

			ICusIntegration result = null;
			var countryCode = declaration.CountryCode;

			if (IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(declaration.CompanyPK))
			{
				result = new LocalCountryCustomsInterfaceIntegrationProvider();
			}
			else if (IntegratedCountryHelper.CustomsWareInstallations(countryCode))
			{
				result = ObjectFactory.Get<Integration.Customs.CustomsWare.ICustomsWareRegistry>().SubmitOutOfLine
					? (CusIntegrationProvider)ObjectFactory.Get("CustomWare.ICusIntegration.OutOfLine")
					: (CusIntegrationProvider)ObjectFactory.Get("CustomWare.ICusIntegration");
			}

			return result;
		}
	}
}
