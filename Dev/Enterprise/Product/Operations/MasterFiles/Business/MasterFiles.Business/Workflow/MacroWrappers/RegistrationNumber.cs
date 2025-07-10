using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class RegistrationNumber : IRegistrationNumber
	{
		public RegistrationNumber(OrgCusCode orgCustCode)
		{
			this.orgCustCode = orgCustCode;
		}
		readonly OrgCusCode orgCustCode;

		public ICountry CountryOfIssue => countryOfIssue ?? (countryOfIssue = new Country(orgCustCode?.CodeCountry));
		ICountry countryOfIssue;

		public ICodeDescription Type => type
			?? (type = new RegistrationNumberType
			{
				Code = orgCustCode?.OK_CodeType ?? ZString.Empty,
				Description = orgCustCode?.Lookups.OK_CodeType_List.GetDescriptionFromCode(orgCustCode?.OK_CodeType),
				Codes = orgCustCode?.Lookups.OK_CodeType_List
			});
		RegistrationNumberType type;

		public ZString Value
		{
			get
			{
				if (!numberValue.HasValue)
				{
					numberValue = orgCustCode?.OK_CustomsRegNo ?? ZString.Empty;
				}
				return numberValue.Value;
			}
			set => numberValue = value;
		}

		ZString? numberValue;
	}
}
