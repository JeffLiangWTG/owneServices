using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;

namespace Enterprise.Customs.Module
{
	public sealed class FilterIsInEuropeanCustomsUnionOrInheritsFromEUConstraint : IFilterConstraint
	{
		#region IFilterConstraint Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => "IsInEuropeanCustomsUnionOrInheritsFromEU";
		}

		public string Description => Res.GetString("OperationalActionsFilter|IsInEuropeanCustomsUnionOrInheritsFromEU|Description",
				"Matches if the country code of the current login company is a European customs country.\r\ne.g. '{0} == \"Y\"' will only match if the current country is a European customs country.",
				"IsInEuropeanCustomsUnionOrInheritsFromEU");

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			return new EuropeanUnionCustomsMembersProvider().IsInEuropeanCustomsUnionOrInheritsFromEU(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode)) ? "Y" : "N";
		}

		public string PluralValueName => Res.GetString("0DBBEB91-69E3-4738-8F48-7C10D021B148", "values");

		public string SingularValueName => Res.GetString("8B82E7E8-A3FC-464C-AA98-2FDC412A9352", "value");

		#endregion

	}
}

