namespace Enterprise.Customs.PL.ExitControl.Business;

public class AdditionalInfoValidation(AdditionalInfo parent) : EU.ExitControl.Business.AdditionalInfoValidation(parent)
{
	protected override void CheckCSI_Status()
	{
		if (parent.CSI_SubType != EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation)
		{
			base.CheckCSI_Status();
		}
	}
}
