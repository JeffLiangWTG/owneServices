using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.LocalCartage.Business.GPS;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class GPSEventAllocation : NonPersistentBusinessObject
	{
		public GPSEventAllocation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GPSEventAllocation(GPSEvent gpsEvent, CartageLegAllocationManager parent)
			: this(gpsEvent.Factory)
		{
			this.gpsEvent = gpsEvent;
			Parent = parent;
		}

		internal readonly CartageLegAllocationManager Parent;

		readonly GPSEvent gpsEvent;

		public GPSEvent GPSEvent { get { return gpsEvent; } }

		[ResourceStringData("GPSEventAllocation|Selected", Caption = "Selected")]
		public ZBool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				SelectedInfo.RefreshBinding();

				if (Parent != null)
				{
					Parent.SetupAllSelectedGpsEvent();

					if (!IsValidationSuspended)
					{
						Validation.ValidateSelected();

						foreach (var gpsEventAllocation in Parent.GPSEventAllocations.Cast<GPSEventAllocation>().Where(g => g.Selected && g != this))
						{
							gpsEventAllocation.Validation.ValidateSelected();
						}
					}
				}
			}
		}

		ZBool selected = false;

		public ZPropertyInfo SelectedInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(Selected));
			}
		}

		public GPSEventAllocationValidation Validation
		{
			get { return new GPSEventAllocationValidation(this); }
		}
	}
}
