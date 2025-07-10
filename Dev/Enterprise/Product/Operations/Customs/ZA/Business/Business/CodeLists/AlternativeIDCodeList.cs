using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class AlternativeIDCodeList : CodeDescriptionPairList
	{
		public AlternativeIDCodeList()
		{
			AddPair("VA", "VAT registration number");
			AddPair("CRN", "Company Registration Number (Used for CC as well)");
			AddPair("IDN", "ID Number");
			AddPair("PSN", "Passport Number");
			AddPair("AHP", "Tax Registration Number");
		}
	}
}
