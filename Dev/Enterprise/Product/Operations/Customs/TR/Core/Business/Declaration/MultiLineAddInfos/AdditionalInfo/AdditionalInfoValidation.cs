namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected override bool IsCodeMandatory => false;

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;
	}
}
