using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAMSEG2AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSEG2AddInfoValidation(AutoUSAMSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_PermitNumber()
		{
			base.CheckUS_PermitNumber();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PermitNumberInfo);
			}
		}

		protected override void CheckUS_IsDocSubmitted()
		{
			base.CheckUS_IsDocSubmitted();
			var ams = AMSLineDetail.Parent;
			if (IsPGAValidation)
			{
				if (!Parent.US_IsDocSubmitted && ams != null && (ams.US_Program == AMSProgramList.Codes.EG1 || ams.US_Program == AMSProgramList.Codes.EG2 || ams.US_Program == AMSProgramList.Codes.MO2))
				{
					Parent.US_IsDocSubmittedInfo.AddMessageError(ConfirmSubmittedALLDocument);
				}
			}
		}
	}
}
