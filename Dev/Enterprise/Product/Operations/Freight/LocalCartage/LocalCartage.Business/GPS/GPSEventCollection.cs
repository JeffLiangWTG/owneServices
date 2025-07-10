using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	public class GPSEventCollection : NonPersistentBusinessObjectCollection<GPSEvent>
	{
		public GPSEventCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GPSEvent();
		}

		public bool GPSEventExists(ZGuid eventPK)
		{
			bool result = false;
			foreach (GPSEvent existingEvent in this)
			{
				if (existingEvent.EventPK == eventPK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public bool GPSLegExists(CommonCartageLeg leg)
		{
			bool result = false;
			foreach (GPSEvent existingEvent in this)
			{
				if (existingEvent.EventLeg.PK == leg.PK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			GPSEvent added = bizOAdded as GPSEvent;
			if (added != null)
			{
				added.EventPK = added.EventPK;
			}
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == GPSEvent.Schema.EventTime)
			{
				return new GPSEventComparer(direction);
			}
			else
			{
				return base.GetComparerForSort(property, direction);
			}
		}
	}

	public class GPSEventComparer : IComparer
	{
		public GPSEventComparer(ListSortDirection direction)
		{
			Direction = direction;
		}

		ListSortDirection Direction { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public int Compare(object x, object y)
		{
			GPSEvent event1 = x as GPSEvent;
			GPSEvent event2 = y as GPSEvent;

			if (Direction == ListSortDirection.Ascending)
			{
				if (event1.EventTime < event2.EventTime)
				{
					return 2;
				}
				else if (event1.EventTime > event2.EventTime)
				{
					return 1;
				}
				else
				{
					if (event1.EventTypeCode.Contains("IN", System.StringComparison.Ordinal))
					{
						return 0;
					}
					else
					{
						return 1;
					}
				}
			}
			else
			{
				if (event1 != null && event2 != null)
				{
					if (event1.EventTime < event2.EventTime)
					{
						return 1;
					}
					else if (event1.EventTime > event2.EventTime)
					{
						return 2;
					}
					else
					{
						if (event1.EventTypeCode.Contains("IN", System.StringComparison.Ordinal))
						{
							return 0;
						}
						else
						{
							return 1;
						}
					}
				}
			}

			return 0;
		}
	}
}
