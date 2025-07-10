using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;

namespace Enterprise.Customs.Module
{
	public sealed class FilterIsUnderFrenchCustomsJurisdictionConstraint : IFilterConstraint
	{
		#region IFilterConstraint Members

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => "IsUnderFrenchCustomsJurisdiction";
		}

		public string Description => Res.GetString("OperationalActionsFilter|IsUnderFrenchCustomsJurisdiction|Description",
				"Matches if the country code of the current login company is under French customs jurisdiction.\r\ne.g. '{0} == \"Y\"' will only match if the current country is under French customs jurisdiction.",
				"IsUnderFrenchCustomsJurisdiction");

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			return Core.Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(countryCode) ? "Y" : "N";
		}

		public string PluralValueName => Res.GetString("9E4DD449-ABDF-439E-BA9A-89F4D8489768", "values");

		public string SingularValueName => Res.GetString("5F1447B8-F5F1-434E-916D-DF3CBA8F178F", "value");

		#endregion

	}
}

