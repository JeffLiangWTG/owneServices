using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaBillValidationForRegularBill : AsycudaBillValidation
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCustomsCPC();
			ValidateMRN();
			ValidateLRN();
		}

		protected override void CheckCustomsCPC()
		{
			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR)
				{
					var customsCPC = Parent.CustomsCPC;

					if (!Parent.CustomsCPC.IsEmpty
						&& (customsCPC.Length != AsycudaBill.Schema.CustomsCPCMaxLength
						|| !char.IsLetter(customsCPC[0])
						|| !customsCPC.SubstringSafe(1).IsNumbersOnlyOrEmpty))
					{
						Parent.CustomsCPCInfo.AddMessageError("Export CPP should be one letter followed by 4 numeric.");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.CustomsCPCInfo);
				}
			}
		}

		protected override void CheckLRN()
		{
			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR)
				{
					var loadPortCountryCode = header.MasterBill?.PortOfLoading?.Country.Code ?? ZString.Empty;
					if (loadPortCountryCode == CountryCodes.SouthAfrica)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.LRNInfo);
					}
					else
					{
						MandatoryValidation.MessageErrorIfIsEntered(Parent.LRNInfo);
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.LRNInfo);
				}
			}
		}

		protected override void CheckMRN()
		{
			if (HeaderNotNullWithType)
			{
				if (!header.IsCOSTCO && !header.IsTGO && !header.IsTGI)
				{
					if (!Parent.MRN.IsEmpty && !Parent.MRN.IsLettersAndNumbersOnlyOrEmpty)
					{
						Parent.MRNInfo.AddMessageError("MRN should be alphanumeric only.");
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.MRNInfo);
				}
			}
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			if (HeaderNotNullWithType)
			{
				if (header.IsGOVGIO)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillNumberInfo);
				}
				else if (header.IsVOR || header.IsEOR)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_BillNumberInfo);
				}
			}
		}

		protected override void CheckABL_BillIssuer()
		{
			base.CheckABL_BillIssuer();

			if (HeaderNotNullWithType)
			{
				if (header.IsDOR || header.IsBBB || header.IsAOR || header.IsALD || header.IsDCI || header.IsDGO || header.IsDGI)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_BillIssuerInfo);
				}
				else if (header.IsVOR || header.IsEOR)
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.ABL_BillIssuerInfo);
				}
			}
		}
	}
}
