using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.eTail.Business
{
	class HVLVOriginLoadListItemGrouping
	{
		public static IEnumerable<IGrouping<GroupKey, HVLVItem>> GetGroupings(HVLVOriginLoadList loadList)
		{
			Func<HVLVItem, GroupKey> keySelector;
			if (loadList.HVL_IsMasterHouse)
			{
				keySelector = item =>
				{
					var consignment = item.Consignment;
					var bookingHeader = consignment.BookingHeader;
					return new GroupKey(bookingHeader.BillToParty.PK, bookingHeader.HVH_RS_NKBookingServiceLevel, consignment.DestinationCountry);
				};
			}
			else
			{
				keySelector = item =>
				{
					var bookingHeader = item.Consignment.BookingHeader;
					return new GroupKey(bookingHeader.BillToParty.PK, bookingHeader.HVH_RS_NKBookingServiceLevel);
				};
			}

			return loadList.Items.OfType<HVLVItem>().GroupBy(keySelector);
		}

		public class GroupKey
		{
			public GroupKey(ZGuid billToPartyPK, ZString serviceLevel, ZString destinationCountry = default)
			{
				BillToPartyPK = billToPartyPK;
				ServiceLevel = serviceLevel;
				DestinationCountry = destinationCountry;
			}

			public ZGuid BillToPartyPK { get; }
			public ZString ServiceLevel { get; }
			public ZString DestinationCountry { get; }

			public override bool Equals(object other)
			{
				return other is GroupKey otherKey
						&& BillToPartyPK == otherKey.BillToPartyPK
						&& ServiceLevel == otherKey.ServiceLevel
						&& (string.IsNullOrEmpty(DestinationCountry) || string.IsNullOrEmpty(otherKey.DestinationCountry) || DestinationCountry == otherKey.DestinationCountry);
			}

			public override int GetHashCode()
			{
				return BillToPartyPK.GetHashCode() ^ ServiceLevel.GetHashCode() ^ DestinationCountry.GetHashCode();
			}
		}
	}
}
