using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class SupportingDocumentsValidation : Customs.Business.CusSupportingInfoValidation
	{
		public SupportingDocumentsValidation(SupportingDocuments parent)
			: base(parent)
		{
		}
		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (!Parent.CSI_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();
			if (!Parent.CSI_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_StatusInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_StatusInfo);
		}

		protected new SupportingDocuments Parent => (SupportingDocuments)base.Parent;
	}
}
