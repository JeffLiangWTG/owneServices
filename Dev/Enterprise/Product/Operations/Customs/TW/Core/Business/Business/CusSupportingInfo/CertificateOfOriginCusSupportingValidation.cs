using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CertificateOfOriginCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public CertificateOfOriginCusSupportingValidation(CertificateOfOriginCusSupporting parent) : base(parent)
		{
		}
		public new CertificateOfOriginCusSupporting Parent => (CertificateOfOriginCusSupporting)base.Parent;

		JobComInvoiceLine InvoiceLine => Parent.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var invoiceLine = InvoiceLine;
			if (!Parent.CSI_LineNo.IsEmpty || (invoiceLine != null && (invoiceLine.JI_PrimaryPreference == Constants.PreferenceCodes.Preference2 || invoiceLine.JI_PrimaryPreference == Constants.PreferenceCodes.ProvisionalPreference2)))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}

			if (Parent.CSI_ReferenceNumber.IsEmpty && invoiceLine != null && invoiceLine.HasTariffCustomsRequirementsAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeValue.SPartially))
			{
				Parent.CSI_ReferenceNumberInfo.AddWarning(ValidationConstants.InvoiceLine.CustomsRequirementSPartially);
			}
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();

			if (!Parent.CSI_ReferenceNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_LineNoInfo);
			}
		}
	}
}
