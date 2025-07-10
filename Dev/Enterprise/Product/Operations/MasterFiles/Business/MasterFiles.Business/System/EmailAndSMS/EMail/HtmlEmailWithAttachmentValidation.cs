using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.MasterFiles.Business
{
	public class HtmlEmailWithAttachmentValidation : EmailWithAttachmentValidation
	{
		public HtmlEmailWithAttachmentValidation(HtmlEmailWithAttachment parent) : base(parent)
		{
		}

		new HtmlEmailWithAttachment Parent
		{
			get { return (HtmlEmailWithAttachment)base.Parent; }
		}

		protected override void CheckBody()
		{
			if (Parent.FilledBodyIsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.BodyInfo);

				if (Parent.ShouldCheckFormatOfEmailBody && !DocumentParser.IsDocumentTextWellFormedBySimpleCheck(Parent.Body))
				{
					Parent.BodyInfo.AddError(DocumentParser.DocumentIsMalformedMessage);
				}
			}
		}

		#region Bcc

		public void ValidateBcc()
		{
			ValidateCalculatedProperty(Parent.BccInfo);
		}

		protected void CheckBcc()
		{
			ValidateEmailAddress(Parent.BccInfo, Parent.BccRecipients);
		}

		#endregion

		#region Template

		public void ValidateTemplate()
		{
			ValidateCalculatedProperty(Parent.TemplateInfo);
		}

		protected virtual void CheckTemplate()
		{
			ListValidation.ErrorIfInvalidPK(Parent.TemplateInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateBcc();
			ValidateTemplate();
			ValidateBody();
		}
	}
}
