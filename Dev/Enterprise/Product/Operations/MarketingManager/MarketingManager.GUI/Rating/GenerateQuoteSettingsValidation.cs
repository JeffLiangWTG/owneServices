
namespace Enterprise.MarketingManager.GUI
{
	public class GenerateQuoteSettingsValidation : AutoGenerateQuoteSettingsValidation
	{
		public GenerateQuoteSettingsValidation(AutoGenerateQuoteSettings parent)
			: base(parent)
		{
		}

		public new GenerateQuoteSettings Parent
		{
			get { return (GenerateQuoteSettings)base.Parent; }
		}

		protected override void CheckShouldCreateAmendment()
		{
			base.CheckShouldCreateAmendment();

			if (Parent.ShouldCreateAmendment && Parent.GetQuoteToAmend() == null)
			{
				Parent.ShouldCreateAmendmentInfo.AddError(Res.GetString("831d9459-6e61-451d-828d-7376ad0b5c67", "Please select a quotation to amend."));
			}
		}
	}
}
