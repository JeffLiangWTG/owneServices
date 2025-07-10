using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Business
{
	public interface ISupplementaryCodeSupporter : ICusCodeDataWithOrderSupporter
	{
		IEnumerable<BaseSupplementaryCode> SupplementaryCodes { get; }
		ZString SupplementaryCodesFieldType { get; }
		ICusCodeDataCollection<BaseSupplementaryCode> AdditionalSupplementaryCodes { get; }
		ResourceStringData SupplementaryCodeCaption { get; }
		ZString GetCountryCodeFromAdditionalCode(ZString additionalCode);
	}
}
