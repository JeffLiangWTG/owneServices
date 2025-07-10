using System;
using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class LocationComparer<TObject> : PropertyComparer
		where TObject : BusinessObject
	{
		public LocationComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction, Func<TObject, WhsLocation> getLocation)
				: base(propertyDescriptor, direction)
		{
			GetLocation = getLocation;
		}

		Func<TObject, WhsLocation> GetLocation { get; }

		public override int Compare(BusinessObject x, BusinessObject y)
		{
			var result = 0;
			var location1 = GetLocation((TObject)x);
			var location2 = GetLocation((TObject)y);

			var isLocation1Empty = location1 == null;
			var isLocation2Empty = location2 == null;

			if (!isLocation1Empty && !isLocation2Empty)
			{
				result = Direction == ListSortDirection.Ascending
						? CompareNonEmptyLocations(location1, location2)
						: CompareNonEmptyLocations(location2, location1);
			}
			else if (isLocation1Empty || isLocation2Empty)
			{
				result = Direction == ListSortDirection.Ascending
						? isLocation2Empty.CompareTo(isLocation1Empty)
						: isLocation1Empty.CompareTo(isLocation2Empty);
			}

			return result;
		}

		int CompareNonEmptyLocations(WhsLocation location1, WhsLocation location2)
		{
			var result = location1.WLV_RowName.CompareTo(location2.WLV_RowName);
			if (result == 0)
			{
				result = location1.WLV_Column.CompareTo(location2.WLV_Column);
			}
			if (result == 0)
			{
				result = location1.WLV_Level.CompareTo(location2.WLV_Level);
			}
			if (result == 0)
			{
				result = location1.WLV_Tray.CompareTo(location2.WLV_Tray);
			}

			return result;
		}
	}
}
