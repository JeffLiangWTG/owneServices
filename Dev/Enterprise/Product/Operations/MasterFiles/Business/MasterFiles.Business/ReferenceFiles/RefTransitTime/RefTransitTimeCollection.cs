using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTimeCollection : ActiveBusinessObjectCollection<RefTransitTime>
	{
		public RefTransitTimeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == RefTransitTime.Schema.TransitTimeFormatted)
			{
				return new TransitTimeComparer(property, direction);
			}

			return base.GetSortComparerForProperty(property, direction);
		}

		class TransitTimeComparer : PropertyComparer
		{
			public TransitTimeComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				var result = 0;
				var transitTime1 = x as RefTransitTime;
				var transitTime2 = y as RefTransitTime;

				if (transitTime1 != null && transitTime2 != null)
				{
					result = transitTime1.RTT_TransitHours.CompareTo(transitTime2.RTT_TransitHours);

					if (Direction == ListSortDirection.Descending)
					{
						result *= -1;
					}
				}

				return result;
			}
		}
	}
}
