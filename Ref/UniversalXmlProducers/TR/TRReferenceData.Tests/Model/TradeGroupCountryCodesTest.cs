using System.Collections.Generic;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using CargoWise.RefDbRepo.TRReferenceData.Tests.Model;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class TradeGroupCountryCodesTest : NameAttributeBaseTest<TradeGroupCountryCodes>
	{
		protected override Dictionary<string, string> ExpectedPropertyNameAndAttributeValue
			=> new Dictionary<string, string>()
			{
				{"TradeGroupCode","Trade Group Code"},
				{"TradeGroupDescription","Trade Group Description"},
				{"CountryCode","Country Code"},
				{"CountryName","Country Name"},
				{"OriginControl","Origin Control"},
				{"ExitCountryControl","Exit Country Control"}
			};
	}
}
