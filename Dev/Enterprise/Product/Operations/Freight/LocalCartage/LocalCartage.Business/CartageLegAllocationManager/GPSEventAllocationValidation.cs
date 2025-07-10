using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class GPSEventAllocationValidation : ZValidation
	{
		public GPSEventAllocationValidation(GPSEventAllocation parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly GPSEventAllocation Parent;

		public override void ValidateAll()
		{
			ValidateSelected();
		}

		public void ValidateSelected()
		{
			ValidateCalculatedProperty(Parent.SelectedInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for tests TestValidateAllSelectedGpsEvent_Invalid, TestAllocateAndSetTimeAsSelected_Invalid (CartageLegAllocationManagerTests)")]
		void CheckSelected()
		{
			var allocationManager = Parent.Parent;
			var clientActivity = Parent.GPSEvent != null ? allocationManager.Factory.Load<GPSSupporterActivity>(Parent.GPSEvent.EventPK) : null;

			if (clientActivity != null)
			{
				var inOut = clientActivity.EN_ActivityType == GPSConstants.GPSInOutActivityType.Codes.GIN ? InOut.In : InOut.Out;

				var validGpsEventAllocations = allocationManager.GPSEventAllocations.Cast<GPSEventAllocation>().Where(g => g.Selected).Where(g => allocationManager.IsValidAddressAndDirection(g, Parent.GPSEvent.EventShortInfo, inOut));

				if (validGpsEventAllocations.Count() > 1)
				{
					string segmentDirection = inOut == InOut.In ? Res.GetString("B3DD861A-CF64-4D95-9473-9F89C7B378D3", "In") : Res.GetString("94585EF2-E8CE-4760-8C75-5EBFD1421785", "Out");
					var error = Res.GetString("9A94C7EE-632C-402A-8F1F-0832BFD2BEB3", "Too many events have been selected for {0} - {1}. Please select only one {1} event per {0}.", allocationManager.GetSegmentTypeDescriptionFromAddress(Parent.GPSEvent.EventShortInfo), segmentDirection);
					Parent.SelectedInfo.AddError(error);
				}
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(GPSEventAllocationValidation); }
		}
	}
}
