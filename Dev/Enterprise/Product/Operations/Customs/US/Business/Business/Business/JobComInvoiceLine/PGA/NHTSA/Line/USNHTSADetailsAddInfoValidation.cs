//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSADetailsAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNHTSADetailsAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USNHTSADetailsAddInfoValidation : AutoUSNHTSADetailsAddInfoValidation
	{
		public USNHTSADetailsAddInfoValidation(AutoUSNHTSADetailsAddInfo parent) : base(parent)
		{
		}

		new USNHTSADetailsAddInfo Parent
		{
			get { return (USNHTSADetailsAddInfo)base.Parent; }
		}

		NHTSAHeader Header
		{
			get { return Parent.Details.Header; }
		}

		protected override void CheckUS_NHTBrandName()
		{
			base.CheckUS_NHTBrandName();

			if (Header != null && Header.IsPGAValidationOn && (Header.IsMotorVehicles || Header.US_NHTProgramCode == NHTSAProgramCodeList.Codes.REI))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTBrandNameInfo);
			}
		}

		protected override void CheckUS_NHTModel()
		{
			base.CheckUS_NHTModel();

			if (Header != null && Header.IsPGAValidationOn && Header.IsMotorVehicles)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTModelInfo);
			}
		}

		protected override void CheckUS_NHTYearOfMFR()
		{
			base.CheckUS_NHTYearOfMFR();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTYearOfMFRInfo, Parent.Lookups.ManufacturerYearList);

			if (Header != null && Header.IsPGAValidationOn && (Header.US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._01 &&
				Header.US_NHTProgramCode != NHTSAProgramCodeList.Codes.OEI) || !Parent.Details.US_NHTMonthOfMFR.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTYearOfMFRInfo);
			}

			ValidateUS_NHTMonthOfMFR();
		}

		protected override void CheckUS_NHTMonthOfMFR()
		{
			base.CheckUS_NHTMonthOfMFR();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTMonthOfMFRInfo, Parent.Lookups.MonthList);

			if (Header != null && Header.IsPGAValidationOn && (Header.US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._01 &&
				Header.US_NHTProgramCode != NHTSAProgramCodeList.Codes.OEI) || !Parent.Details.US_NHTYearOfMFR.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTMonthOfMFRInfo);
			}

			ValidateUS_NHTYearOfMFR();
		}

		protected override void CheckUS_NHTCategoryCode()
		{
			base.CheckUS_NHTCategoryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTCategoryCodeInfo, Parent.Lookups.CategoryCodes);

			if (Header != null && Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTCategoryCodeInfo);
				Header.AddInfo.Validation.ValidateUS_NHTFabricatingMFRAddress();
			}
		}

		protected override void CheckUS_NHTModelYear()
		{
			base.CheckUS_NHTModelYear();
			ListValidation.WarnIfInvalidCode(Parent.US_NHTModelYearInfo, Parent.Lookups.ModelYearList);
		}

		protected override void CheckUS_NHTDriveSide()
		{
			base.CheckUS_NHTDriveSide();

			if (Header != null && Header.IsPGAValidationOn && (Header.US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._2B || Header.US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._03))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDriveSideInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTDriveSideInfo, Parent.Lookups.DriveSides);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.US_NHTDriveSideInfo, Parent.Lookups.DriveSides);
			}
		}
	}
}
