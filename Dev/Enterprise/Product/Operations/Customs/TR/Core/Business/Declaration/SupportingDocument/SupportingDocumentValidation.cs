using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_StatusInfo);
		}
	}
}
