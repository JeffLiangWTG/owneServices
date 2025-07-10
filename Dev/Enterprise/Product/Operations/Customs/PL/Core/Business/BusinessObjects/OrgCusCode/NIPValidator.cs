using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class NIPValidator
{
	internal ZBool Validate(ZPropertyInfo codeInfo)
	{
		var result = true;
		var value = (ZString)codeInfo.Value;

		if (value.Length != OrgCusCodesLength.NipAcceptedLength || !value.IsNumbersOnlyOrEmpty)
		{
			codeInfo.AddError(Res.GetString("PLNIPValidator|InvalidLengthMessage", "The NIP number entered is invalid (must be {0} digits long).", OrgCusCodesLength.NipAcceptedLength));
			result = false;
		}

		return result;
	}
}
