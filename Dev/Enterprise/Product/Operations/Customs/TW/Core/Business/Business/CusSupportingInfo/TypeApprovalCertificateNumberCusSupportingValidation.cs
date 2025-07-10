using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class TypeApprovalCertificateNumberCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public TypeApprovalCertificateNumberCusSupportingValidation(TypeApprovalCertificateNumberCusSupporting parent) : base(parent)
		{
		}

		public new TypeApprovalCertificateNumberCusSupporting Parent => (TypeApprovalCertificateNumberCusSupporting)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			if (!referenceNumber.IsEmpty)
			{
				var targetInfo = parent.CSI_ReferenceNumberInfo;
				if (!IsAlphaNumeric(referenceNumber))
				{
					targetInfo.AddMessageError(ValidationConstants.TypeApprovalCertificateNumber.AlphanumericCharactersOnlyForCertificateNo);
				}
				else if (referenceNumber.Length != 14)
				{
					targetInfo.AddMessageError(ValidationConstants.TypeApprovalCertificateNumber.LengthForCertificateNo);
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();

			var parent = Parent;
			var targetInfo = parent.CSI_ReferenceNumber2Info;
			var isReadOnly = parent.Parent?.TypeApprovalAuthorizedPartyInfo?.ReadOnly ?? true;
			if (!isReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			var referenceNumber = parent.CSI_ReferenceNumber2;
			if (!referenceNumber.IsEmpty && !IsAlphaNumeric(referenceNumber))
			{
				targetInfo.AddMessageError(ValidationConstants.TypeApprovalCertificateNumber.AlphanumericCharactersOnlyForAuthorizedParty);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			var parent = Parent;
			var isReadOnly = parent.Parent?.TypeApprovalPartyIdentifierInfo?.ReadOnly ?? true;
			if (!isReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CSI_DescriptionInfo, parent.Factory.GetCachedValue<PartyIdentifierCodeList>());
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo, Parent.Factory.GetCachedValue<ExemptionCodeList>());
		}

		bool IsAlphaNumeric(string val)
		{
			return Regex.IsMatch(val, @"^[A-Z0-9]*$", RegexOptions.IgnoreCase);
		}
	}
}
