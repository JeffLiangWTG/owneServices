//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISInspectionAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISInspectionAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISInspectionAddInfoValidation : AutoUSAPHISInspectionAddInfoValidation
	{
		public USAPHISInspectionAddInfoValidation(AutoUSAPHISInspectionAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Date()
		{
			base.CheckUS_Date();
			if (Parent.US_Date.IsEmpty && IsPGAValidation)
			{
				Parent.US_DateInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Inspection Date"));
			}
		}

		protected override void CheckUS_Location()
		{
			base.CheckUS_Location();
			if (IsPGAValidation)
			{
				if (Parent.US_Location.IsEmpty)
				{
					Parent.US_LocationInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Inspection Port Code"));
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_LocationInfo, Parent.Lookups.PortCodes);
				}
			}
		}

		protected override void CheckUS_TestingStatus()
		{
			base.CheckUS_TestingStatus();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TestingStatusInfo, Parent.Lookups.InspectionStatusList);
			}
		}

		protected new USAPHISInspectionAddInfo Parent
		{
			get { return (USAPHISInspectionAddInfo)base.Parent; }
		}

		protected APHISInspection Inspection
		{
			get { return Parent.Parent; }
		}

		protected APHISHeader Header
		{
			get
			{
				var inspection = Inspection;
				return inspection == null ? null : inspection.Header;
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var aphisHeader = Header;
				if (aphisHeader != null)
				{
					var invoiceLine = aphisHeader.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
