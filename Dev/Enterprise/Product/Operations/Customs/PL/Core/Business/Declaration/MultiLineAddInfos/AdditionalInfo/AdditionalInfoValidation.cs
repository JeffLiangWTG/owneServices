namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
{
	public AdditionalInfoValidation(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo parent) : base(parent)
	{
	}

	protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

	protected sealed override void CheckCSI_Status() { }
}
