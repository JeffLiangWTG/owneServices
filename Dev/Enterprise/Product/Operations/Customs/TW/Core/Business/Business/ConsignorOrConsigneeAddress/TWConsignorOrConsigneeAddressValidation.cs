using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class TWConsignorOrConsigneeAddressValidation : JobDocAddressValidation
	{
		public TWConsignorOrConsigneeAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		public new TWConsignorOrConsigneeAddress Parent => (TWConsignorOrConsigneeAddress)base.Parent;

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();
			var parent = Parent;
			var targetInfo = parent.E2_GovRegNumInfo;
			var code = parent.E2_GovRegNum;
			if (!code.IsEmpty)
			{
				if (parent.Organisation != null)
				{
					var type = parent.E2_GovRegNumType;
					var countryCode = parent.E2_RN_NKCountryCode;
					if (countryCode == Core.Constants.CountryCodes.Taiwan && type != Constants.CCPPrefix)
					{
						OrgCusCodeValidation.ValidateCustomsCode(new DocAddressCustomsRegistrationNumberValidation(parent), countryCode, type, code, targetInfo);
					}
					else if (countryCode != Core.Constants.CountryCodes.Taiwan && type == Constants.CCPPrefix && !code.StartsWith(Constants.CCPPrefix))
					{
						targetInfo.AddMessageError(Res.GetString("567B55A8-8948-4AAC-9208-4FD4FCA3C99D", "Bonded ID for the foreign company must be 'FFF' + Bonded ID."));
					}
					else
					{
						CheckCodeMaxLenth();
					}
				}
				else
				{
					CheckCodeMaxLenth();
				}
			}
			else if (!parent.E2_GovRegNumType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			void CheckCodeMaxLenth()
			{
				if (code.Length > 14)
				{
					targetInfo.AddMessageError(Res.GetString("567B55A8-8948-4AAC-9208-4FD4FCA3C99B", "The length of Number shouldn't be more than 14."));
				}
			}
		}

		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();
			var parent = Parent;
			var targetInfo = parent.E2_GovRegNumTypeInfo;
			if (parent.E2_GovRegNum.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			}
		}
	}
}
