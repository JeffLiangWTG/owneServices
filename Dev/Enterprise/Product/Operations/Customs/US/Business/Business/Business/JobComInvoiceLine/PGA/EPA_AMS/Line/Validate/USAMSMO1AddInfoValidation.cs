using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USAMSMO1AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSMO1AddInfoValidation(AutoUSAMSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Packages()
		{
			base.CheckUS_Packages();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackagesInfo);
			}
			if (Parent.US_Packages < 0m)
			{
				Parent.US_PackagesInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_InspecDateTime()
		{
			base.CheckUS_InspecDateTime();
			if (IsPGAValidation)
			{
				if (Parent.US_InspecDateTime.IsValid && Parent.US_InspecDateTime <= ZDateTime.Now.AddDays(2))
				{
					Parent.US_InspecDateTimeInfo.AddWarning(InspecDateTimeMessageError);
				}
			}
		}

		internal const string InspecDateTimeMessageError = "Date of Inspection should be at least 48 hours after entry filing.";
	}
}
