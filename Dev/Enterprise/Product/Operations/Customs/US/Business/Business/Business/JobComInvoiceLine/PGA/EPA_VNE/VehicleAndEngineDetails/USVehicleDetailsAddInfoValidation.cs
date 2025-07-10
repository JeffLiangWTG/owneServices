//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSVehicleDetailsAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSVehicleDetailsAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USVehicleDetailsAddInfoValidation : AutoUSVehicleDetailsAddInfoValidation
	{
		public USVehicleDetailsAddInfoValidation(AutoUSVehicleDetailsAddInfo parent) : base(parent)
		{
		}

		new USVehicleDetailsAddInfo Parent
		{
			get { return (USVehicleDetailsAddInfo)base.Parent; }
		}

		protected override void CheckUS_EngineBuildDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckUS_IdentityNumberQualifier()
		{
			base.CheckUS_IdentityNumberQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_IdentityNumberQualifierInfo, Parent.Lookups.IdentityQualifier);

			if (Parent.US_IdentityNumberQualifier.IsEmpty && IsVehicleDetailsRequired)
			{
				Parent.US_IdentityNumberQualifierInfo.AddMessageError(QualifierRequired);
			}

			if (IsForm_1 &&
				!Parent.US_IdentityNumberQualifier.IsEmpty &&
				Parent.US_IdentityNumberQualifier != ItemIdentityNumberQualifierList.Codes.SerialNumber &&
				Parent.US_IdentityNumberQualifier != ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN
			)
			{
				Parent.US_IdentityNumberQualifierInfo.AddMessageError(ShouldBeVIN);
			}
		}
		internal const string QualifierRequired = "Vehicle Identification Type is required.";
		internal const string ShouldBeVIN = "Only VIN (Vehicle Identification Number) and Serial Number are allowed for Form 3520-1.";

		bool IsVehicleDetailsRequired
		{
			get
			{
				var details = Parent.VehicleDetails;
				return SomeVehicleDetailsEntered || (IsForm_1 && details != null && !details.AllEngineDetailsEntered());
			}
		}

		bool SomeVehicleDetailsEntered
		{
			get
			{
				var details = Parent.VehicleDetails;
				return details != null && !details.IsEngineOnly();
			}
		}

		protected override void CheckUS_IdentityNumber()
		{
			base.CheckUS_IdentityNumber();

			if (IsVehicleDetailsRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IdentityNumberInfo);
			}

			if (Parent.US_IdentityNumberQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN && Parent.US_IdentityNumber.Length != 17)
			{
				Parent.US_IdentityNumberInfo.AddMessageError(VNEAdditionalNumberValidation.NumberLengthText);
			}
		}

		protected override void CheckUS_BuildMonth()
		{
			base.CheckUS_BuildMonth();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BuildMonthInfo, Parent.Lookups.MonthList);

			if (IsVehicleDetailsRequired || Parent.US_MfrDateType == ManufactureDateTypeList.Codes.VEH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_BuildMonthInfo);
			}
		}

		protected override void CheckUS_BuildYear()
		{
			base.CheckUS_BuildYear();
			if (IsVehicleDetailsRequired || Parent.US_MfrDateType == ManufactureDateTypeList.Codes.VEH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_BuildYearInfo);
			}

			if (!Parent.US_BuildYear.IsEmpty)
			{
				if (!Parent.US_BuildYear.IsNumbersOnlyOrEmpty)
				{
					Parent.US_BuildYearInfo.AddMessageError(YearFormat);
				}
				else
				{
					var buildYear = ZInt.ParseSafe(Parent.US_BuildYear, 0);
					if (buildYear > ZDateTime.Today.Year)
					{
						Parent.US_BuildYearInfo.AddMessageError(YearInFuture);
					}
					else if (buildYear < 1900)
					{
						Parent.US_BuildYearInfo.AddMessageError(YearInPast);
					}
				}
			}
		}
		internal const string YearFormat = "Build Year should be 4 digits, in a format CCYY, where CC is century and YY is year.";
		internal const string YearInFuture = "Build Year cannot be in the future.";
		internal const string YearInPast = "Build Year cannot be earlier than 1900.";

		protected override void CheckUS_EngineManufacturer()
		{
			base.CheckUS_EngineManufacturer();
			if (IsForm_21 && SomeVehicleDetailsEntered)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EngineManufacturerInfo);
			}
		}

		protected override void CheckUS_EngineModel()
		{
			base.CheckUS_EngineModel();
			if (IsForm_21)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EngineModelInfo);
			}
		}

		protected override void CheckUS_EngineNumber()
		{
			base.CheckUS_EngineNumber();
			var details = Parent.VehicleDetails as IVNEDetails;
			if (IsForm_21)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EngineNumberInfo);
			}
			else
			{
				if (details != null && details.EngineNumber.IsEmpty && details.EngineAdditionalNumbers.Any())
				{
					Parent.US_EngineNumberInfo.AddMessageError(EngineNumberMessageText);
				}
			}
		}
		internal const string EngineNumberMessageText = "You have not entered an engine number if exist additional engine number.";

		protected override void CheckUS_EngineBuildDate()
		{
			base.CheckUS_EngineBuildDate();

			if (IsForm_21)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_EngineBuildDateInfo);
			}
		}

		protected override void CheckUS_MfrDateType()
		{
			base.CheckUS_MfrDateType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_MfrDateTypeInfo, Parent.Lookups.DateTypes);

			if (IsForm_21)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_MfrDateTypeInfo);
			}

			ValidateUS_BuildYear();
			ValidateUS_BuildMonth();
			ValidateUS_EngineBuildDate();
			ValidateUS_BuildDateExplanation();
		}

		protected override void CheckUS_BuildDateExplanation()
		{
			base.CheckUS_BuildDateExplanation();

			var manufactureDateType = Parent.US_MfrDateType;
			if (IsForm_21 && manufactureDateType == ManufactureDateTypeList.Codes.OTH && Parent.US_BuildDateExplanation.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_BuildDateExplanationInfo);
			}
		}

		bool IsForm_1
		{
			get
			{
				var vehicle = Vehicle;
				return vehicle != null && vehicle.IsForm_1;
			}
		}

		bool IsForm_21
		{
			get
			{
				var vehicle = Vehicle;
				return vehicle != null && vehicle.IsForm_21;
			}
		}

		Vehicle Vehicle
		{
			get
			{
				var details = Parent.VehicleDetails;
				return details != null ? details.Vehicle : null;
			}
		}
	}
}
