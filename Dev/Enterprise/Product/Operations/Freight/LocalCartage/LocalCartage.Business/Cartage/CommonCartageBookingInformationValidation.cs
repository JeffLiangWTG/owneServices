using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageBookingInformationValidation : ZValidation
	{
		public CommonCartageBookingInformationValidation(CommonCartageBookingInformation parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly CommonCartageBookingInformation Parent;

		public override void ValidateAll()
		{
			ValidateBookingStatus();
		}

		public void ValidateBookingStatus()
		{
			ValidateCalculatedProperty(Parent.BookingStatusInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestValidateBookingStatus (CommonCartageBookingInformationValidationTest)")]
		void CheckBookingStatus()
		{
			if (Parent.CanSelectStatus)
			{
				ListValidation.ErrorIfInvalidCode(Parent.BookingStatusInfo, Parent.BookingStatusList);
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(CommonCartageBookingInformationValidation); }
		}
	}
}
