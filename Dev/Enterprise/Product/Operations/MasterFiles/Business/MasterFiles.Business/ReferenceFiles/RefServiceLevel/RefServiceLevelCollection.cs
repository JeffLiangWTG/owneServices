using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.ServiceLevel)]
	public class RefServiceLevelCollection : ActiveBusinessObjectCollection<RefServiceLevel>, IRefServiceLevelCollection
	{
		public RefServiceLevelCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefServiceLevelCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public RefServiceLevelCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == RefServiceLevel.Schema.DefaultTransitTimeFormatted)
			{
				return new DefaultTransitTimeComparer(property, direction);
			}

			return base.GetSortComparerForProperty(property, direction);
		}

		class DefaultTransitTimeComparer : PropertyComparer
		{
			public DefaultTransitTimeComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				var result = 0;
				var transitTime1 = x as RefServiceLevel;
				var transitTime2 = y as RefServiceLevel;

				if (transitTime1 != null && transitTime2 != null)
				{
					result = transitTime1.RS_DefaultTransitHours.CompareTo(transitTime2.RS_DefaultTransitHours);

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
