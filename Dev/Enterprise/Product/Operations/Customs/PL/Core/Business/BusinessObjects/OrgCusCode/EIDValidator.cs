using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class EIDValidator
{
	internal ZBool Validate(ZPropertyInfo codeInfo)
	{
		var result = true;
		var value = (ZString)codeInfo.Value;

		if (value.Length != OrgCusCodesLength.EidAcceptedLength)
		{
			codeInfo.AddError(Res.GetString("PLEIDValidator|InvalidLengthMessage", "EID number must be {0} chars long. Numbers with other lengths will not be used in customs messages.", OrgCusCodesLength.EidAcceptedLength));
			result = false;
		}

		return result;
	}
}
