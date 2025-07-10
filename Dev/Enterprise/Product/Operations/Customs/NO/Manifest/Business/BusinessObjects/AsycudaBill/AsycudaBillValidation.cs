using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Manifest.Business;

sealed class AsycudaBillValidation
{
	public AsycudaBillValidation(AsycudaBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
	}

	public void ValidateForwarderForCusCodes()
	{
		if (bill.ABL_OA_Forwarder.IsEmpty)
		{
			return;
		}

		var forwarderOrgAddress = bill.Factory.Load<OrgAddress>(bill.ABL_OA_Forwarder);
		if (!IsOrgAddressContainsRequiredCodeType(forwarderOrgAddress, RequiredCodeTypeListForForwarder))
		{
			bill.ABL_OA_ForwarderInfo.AddMessageError(
				Res.GetString("B29A3FB9-AB5F-4EED-9256-C7F7535E3AF9",
				"The Representative must have a valid EORI or Norwegian “organization number”. (Organization’s Config tab Registration Code EOR, ORG or MVA must exist.)"));
		}
	}

	static bool IsOrgAddressContainsRequiredCodeType(OrgAddress orgAddress, ImmutableArray<string> codeTypes)
	{
		if (orgAddress?.Header is null)
		{
			return false;
		}

		var orgCusCodeCollection = orgAddress.Header.CustomsCodes;
		return orgCusCodeCollection is not null && orgCusCodeCollection.Any(customCode => !customCode.OK_CodeType.IsEmpty && codeTypes.Contains(customCode.OK_CodeType));
	}

	public void ValidateForwarderEmail() =>
		CheckEmailOrPhoneIsEntered(
			bill.ABL_ForwarderEmailInfo,
			bill.ABL_ForwarderPhoneInfo,
			bill.ABL_ForwarderEmailInfo,
			GetMessageErrorForForwarderEmailAndPhone());

	public void ValidateForwarderPhone() =>
		CheckEmailOrPhoneIsEntered(
			bill.ABL_ForwarderEmailInfo,
			bill.ABL_ForwarderPhoneInfo,
			bill.ABL_ForwarderPhoneInfo,
			GetMessageErrorForForwarderEmailAndPhone());

	void CheckEmailOrPhoneIsEntered(ZPropertyInfo emailInfo, ZPropertyInfo phoneInfo, ZPropertyInfo target, string messageError)
	{
		if (emailInfo.Value.IsEmpty && phoneInfo.Value.IsEmpty)
		{
			target.AddMessageError(messageError);
		}
	}

	static string GetMessageErrorForForwarderEmailAndPhone() =>
		Res.GetString("3B443B82-39B3-4384-97FD-F95280765F50", "You have not entered a Phone Number or an Email address for Representative.");

	static ImmutableArray<string> RequiredCodeTypeListForForwarder => ImmutableArray.Create(
		OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
		OrgCusCode.CodeTypes.OrganizationNumber,
		OrgCusCode.NorwayCodeTypes.MVA
	);

	readonly AsycudaBill bill;
}
