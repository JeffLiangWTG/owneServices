using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;

namespace Enterprise.Customs.Module
{
	sealed class FilterIsIntegratedCountryConstraint : IFilterConstraint
	{
		#region IFilterConstraint Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => "IsIntegratedCountry";
		}

		public string Description => Res.GetString("OperationalActionsFilter|IsIntegratedCountryConstraint|Description",
			"Matches if the country code of the current login company is an integrated country.\r\ne.g. '{0} == \"Y\"' will only match if the current country is an integrated country.",
			"IsIntegratedCountry");

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			var company = GlbCompany.CurrentCompany;
			var countryCode = company.GC_RN_NKCountryCode;
			return IntegratedCountryHelper.IsInterfaceEnabledCompany(company.PK, countryCode) ? "Y" : "N";
		}

		public string PluralValueName => Res.GetString("527df665-cd65-4021-81e9-bbee76b65451", "values");

		public string SingularValueName => Res.GetString("06909d96-9762-4882-9442-fd51112e2875", "value");

		#endregion
	}
}
