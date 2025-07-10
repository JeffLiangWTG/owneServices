using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class PreAllocationValidation : ZValidation
	{
		public PreAllocationValidation(PreAllocation parent)
			: base(parent)
		{
			this.Parent = parent;
			this.ParentListInternals = parent;
		}

		#region Overrides

		public override Type AutoValidationType
		{
			get { return typeof(PreAllocationValidation); }
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				Parent.ClearRowNotifications();
				ValidateHouseBillCount();
				ValidateHouseBillFrom();
				ValidateHouseBillTo();
				ValidateBranchOriginDestination();
			}
		}

		#endregion

		#region HouseBillCount

		public void ValidateHouseBillCount()
		{
			ValidateCalculatedProperty(Parent.HouseBillCountInfo);
		}

		protected virtual void CheckHouseBillCount()
		{
			if (Parent.HouseBillCount > PreAllocation.MaxHouseBillCount)
			{
				Parent.HouseBillCountInfo.AddError(Res.GetString("4e7b46a8-b29b-4a2c-b383-c527dab32a82", "Count needs to be less than or equal to {0}.", PreAllocation.MaxHouseBillCount));
			}
		}

		#endregion

		#region HouseBillFrom

		public void ValidateHouseBillFrom()
		{
			ValidateCalculatedProperty(Parent.HouseBillFromInfo);
		}

		protected virtual void CheckHouseBillFrom()
		{
			if (Parent.HouseBillFrom.Contains("?", StringComparison.Ordinal))
			{
				Parent.HouseBillFromInfo.AddError(Res.GetString("977a9e95-220e-43da-a8ff-9f92937e275a", "Client does not have a Pre-Allocation Prefix. See Client Organization -> Shipper / Consignor Tab"));
			}
			if (Parent.HouseBillFrom.IsEmpty)
			{
				Parent.HouseBillFromInfo.AddError(Res.GetString("6844dfc8-711f-4d18-8585-31628b2263a4", "House bill number range 'from' cannot be empty."));
			}
		}

		#endregion

		#region HouseBillTo

		public void ValidateHouseBillTo()
		{
			ValidateCalculatedProperty(Parent.HouseBillToInfo);
		}

		protected virtual void CheckHouseBillTo()
		{
			if (Parent.HouseBillTo.Contains("?", StringComparison.Ordinal))
			{
				Parent.HouseBillToInfo.AddError(Res.GetString("56b3d4c9-88c5-4e7e-8e25-c32e53498d9e", "Client does not have a Pre-Allocation Prefix. See Client Organization -> Shipper / Consignor Tab"));
			}
			if (Parent.HouseBillTo.IsEmpty)
			{
				Parent.HouseBillToInfo.AddError(Res.GetString("3970e7bc-9aef-433b-9fed-4ea1549a5c66", "House bill number range 'to' cannot be empty."));
			}
		}

		#endregion

		public void ValidateBranchOriginDestination()
		{
			if (Parent.QuotedBooking.Job != null)
			{
				Parent.QuotedBooking.Job.Validation.ValidateJH_GE();
				if (Parent.QuotedBooking.Job.JH_GE.IsEmpty || Parent.QuotedBooking.Job.JH_GEInfo.HasErrors())
				{
					Parent.AddRowError(Res.GetString("7a24e6df-e129-41cb-993b-8ac3a0fc20e6", "A Department cannot be determined. Please ensure you're logged into an operations branch with 'Default Departments > Default to Current Login Dept.' registry item enabled, or enter a Mode, Origin and Destination."));
				}
			}
		}

		#region Implementation

		readonly PreAllocation Parent;
		readonly ISingleElementListInternal ParentListInternals;

		#endregion
	}
}
