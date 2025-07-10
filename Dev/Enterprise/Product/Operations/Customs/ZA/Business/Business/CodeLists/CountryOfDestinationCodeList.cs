using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CountryOfDestinationCodeList : CodeDescriptionPairList
	{
		public CountryOfDestinationCodeList()
		{
			AddPair("ZA", "South Africa");
			AddPair("BW", "Botswana");
			AddPair("LS", "Lesotho");
			AddPair("SZ", "Swaziland");
			AddPair("NA", "Namibia");
		}
	}
}
