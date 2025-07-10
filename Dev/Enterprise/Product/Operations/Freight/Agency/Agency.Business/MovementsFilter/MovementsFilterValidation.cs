using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class MovementsFilterValidation : AutoMovementsFilterValidation
	{
		public MovementsFilterValidation(AutoMovementsFilter parent)
			: base(parent) { }

		protected override void CheckMovementType()
		{
			base.CheckMovementType();
			ListValidation.ErrorIfInvalidCode(Parent.MovementTypeInfo, Parent.Lookups.MovementTypes);
		}

		protected override void CheckVessel()
		{
			base.CheckVessel();
			ListValidation.ErrorIfInvalidCode(Parent.VesselInfo, Parent.Lookups.Vessels);
		}

		protected override void CheckFromDate()
		{
			base.CheckFromDate();

			if (SpecifiedOutOfOrder(Parent.FromDate, Parent.ToDate))
			{
				Parent.FromDateInfo.AddError(Res.GetString("b2c3ed35-4a12-4edc-8027-b6e86bf86aed", "From Date cannot be after To Date."));
			}
		}

		protected override void CheckFromDateIsValidZDateTimeRange()
		{
			// don't call base, we don't want the default range validation.
		}

		protected override void CheckToDate()
		{
			base.CheckToDate();

			if (SpecifiedOutOfOrder(Parent.FromDate, Parent.ToDate))
			{
				Parent.ToDateInfo.AddError(Res.GetString("7ff8bdaa-25fa-47a1-893a-c4017d179c14", "To Date cannot be before From Date."));
			}

			if (SpecifiedOutOfOrder(Parent.ToDate, ZDateTime.Now))
			{
				Parent.ToDateInfo.AddWarning(Res.GetString("1129c7df-f266-4c76-8557-71198e7000fc", "To Date is in the future."));
			}
		}

		protected override void CheckToDateIsValidZDateTimeRange()
		{
			// don't call base, we don't want the default range validation.
		}

		#region Implementation

		bool SpecifiedOutOfOrder(ZDateTime fromDate, ZDateTime toDate)
		{
			return !fromDate.IsEmpty && !toDate.IsEmpty && fromDate > toDate;
		}

		public new MovementsFilter Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (MovementsFilter)base.Parent; }
		}

		#endregion
	}
}


