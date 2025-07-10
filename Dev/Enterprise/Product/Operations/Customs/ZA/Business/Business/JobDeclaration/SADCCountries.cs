using Enterprise.Core;

using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ZA.Business
{
	[CodeAlive("")]
	public sealed class SADCCountries : CodeDescriptionPairList
	{
		public SADCCountries()
		{
			AddPair(Constants.CountryCodes.Angola, "The People's Republic of Angola");
			AddPair(Constants.CountryCodes.Botswana, "Botswana");
			AddPair(Constants.CountryCodes.Lesotho, "Kingdom of Lesotho");
			AddPair(Constants.CountryCodes.Malawi, "Republic of Malawi");
			AddPair(Constants.CountryCodes.Mozambique, "Republic of Mozambique");
			AddPair(Constants.CountryCodes.Namibia, "Republic of Namibia");
			AddPair(Constants.CountryCodes.Swaziland, "Republic of Swaziland");
			AddPair(Constants.CountryCodes.Tanzania, "United Republic of Tanzania");
			AddPair(Constants.CountryCodes.Zambia, "Republic of Zambia");
			AddPair(Constants.CountryCodes.Zimbabwe, "Republic of Zimbabwe");
		}
	}
}
