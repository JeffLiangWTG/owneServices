using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAMSEG1AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSEG1AddInfoValidation(AutoUSAMSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_InnerAmount()
		{
			base.CheckUS_InnerAmount();

			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InnerAmountInfo);
			}
			if (Parent.US_InnerAmount < 0m)
			{
				Parent.US_InnerAmountInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_InnerAmountUQ()
		{
			base.CheckUS_InnerAmountUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InnerAmountUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InnerAmountUQInfo);
			}
		}

		protected override void CheckUS_InnerWeightUQ()
		{
			base.CheckUS_InnerWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InnerWeightUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				if (!Parent.US_InnerWeight.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InnerWeightUQInfo);
				}
			}
		}

		protected override void CheckUS_InnerPackage()
		{
			base.CheckUS_InnerPackage();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InnerPackageInfo);
			}
			if (Parent.US_InnerPackage < 0m)
			{
				Parent.US_InnerPackageInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_InnerPackageUQ()
		{
			base.CheckUS_InnerPackageUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InnerPackageUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InnerPackageUQInfo);
			}
		}

		protected override void CheckUS_OuterPackage()
		{
			base.CheckUS_OuterPackage();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OuterPackageInfo);
			}

			if (Parent.US_OuterPackage < 0m)
			{
				Parent.US_OuterPackageInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_OuterPackageUQ()
		{
			base.CheckUS_OuterPackageUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_OuterPackageUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OuterPackageUQInfo);
			}
		}

		protected override void CheckUS_TotalQuantity()
		{
			base.CheckUS_TotalQuantity();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TotalQuantityInfo);
			}

			if (Parent.US_TotalQuantity < 0m)
			{
				Parent.US_TotalQuantityInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_TotalQuantityUQ()
		{
			base.CheckUS_TotalQuantityUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TotalQuantityUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TotalQuantityUQInfo);
			}
		}

		protected override void CheckUS_TotalWeight()
		{
			base.CheckUS_TotalWeight();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TotalWeightInfo);
			}
			if (Parent.US_TotalWeight < 0m)
			{
				Parent.US_TotalWeightInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_TotalWeightUQ()
		{
			base.CheckUS_TotalWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TotalWeightUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_TotalWeightUQInfo);
			}
		}
	}
}
