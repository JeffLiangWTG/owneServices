using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAMSMO2AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSMO2AddInfoValidation(AutoUSAMSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_CertNumber()
		{
			base.CheckUS_CertNumber();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertNumberInfo);
			}
		}

		protected override void CheckUS_IssueDate()
		{
			base.CheckUS_IssueDate();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IssueDateInfo);
			}
		}

		protected override void CheckUS_InspectionLocation()
		{
			base.CheckUS_InspectionLocation();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InspectionLocationInfo, AMSLineDetail.AddInfoLookups.InspectionLocationCodeList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InspectionLocationInfo);
			}
		}

		protected override void CheckUS_Party()
		{
			base.CheckUS_Party();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_PartyInfo, AMSLineDetail.AddInfoLookups.InspectionAgencyList);

			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PartyInfo);
			}
		}

		protected override void CheckUS_Weight()
		{
			base.CheckUS_Weight();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_WeightInfo);
			}
			if (Parent.US_Weight < 0m)
			{
				Parent.US_WeightInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		protected override void CheckUS_WeightUQ()
		{
			base.CheckUS_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_WeightUQInfo, AMSLineDetail.AddInfoLookups.UnitOfMeasureList);
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_WeightUQInfo);
			}
		}

		protected override void CheckUS_CertType()
		{
			base.CheckUS_CertType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CertTypeInfo, AMSLineDetail.AddInfoLookups.CertTypeCodeList);

			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CertTypeInfo);
			}
		}
	}
}
