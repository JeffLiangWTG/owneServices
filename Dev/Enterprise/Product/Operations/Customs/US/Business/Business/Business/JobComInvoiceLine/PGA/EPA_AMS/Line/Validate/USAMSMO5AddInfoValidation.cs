using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAMSMO5AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSMO5AddInfoValidation(AutoUSAMSLineAddInfo parent)
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

		protected override void CheckUS_PackagesUQ()
		{
			base.CheckUS_PackagesUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PackagesUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackagesUQInfo);
			}
		}

		protected override void CheckUS_PackageWeight()
		{
			base.CheckUS_PackageWeight();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackageWeightInfo);
			}
			if (Parent.US_PackageWeight < 0m)
			{
				Parent.US_PackageWeightInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_PackageWeightUQ()
		{
			base.CheckUS_PackageWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PackageWeightUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PackageWeightUQInfo);
			}
		}
	}
}
