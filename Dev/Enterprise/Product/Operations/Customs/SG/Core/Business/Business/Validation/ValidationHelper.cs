using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ValidationHelper
	{
		public const string UENDoesNotExist = "does not have a UEN reference, (set up in Organisation > Config)";

		public bool HasUEN(OrgHeader organisation) => !organisation?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber).IsEmpty ?? true;

		public bool IsNonAdValoremDutyRatedGoods(JobDeclaration declaration)
		{
			var placeOfRelease = declaration.PlaceOfRelease;
			var placeOfReceipt = declaration.PlaceOfReceipt;
			return
				placeOfRelease.IsLicencedPremise() && !placeOfRelease.IsBWCY()
				|| placeOfReceipt.IsShortPayment()
				|| declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT
				|| placeOfReceipt.IsRecoveryPayment()
				|| declaration.SG_GoodsPreviouslyExemptedFromDuties
				|| placeOfReceipt.IsExemptPlaceCodePresident();
		}
	}
}
