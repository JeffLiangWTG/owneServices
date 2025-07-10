using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business
{
	class PackageTotals
	{
		public PackageTotals(DynamicBusinessObject bizO, IEnumerable<PackTypeCount> packTypeCounts, GroupedPackTypeCounts bookedPackTypeCounts)
		{
			BizO = Argument.NotNull(bizO, "bizO");
			BookedPackTypeCounts = Argument.NotNull(bookedPackTypeCounts, "bookedPackTypeCounts");
			PackTypeCounts = Argument.NotNull(packTypeCounts, "packTypeCounts");
		}

		readonly GroupedPackTypeCounts BookedPackTypeCounts;
		readonly IEnumerable<PackTypeCount> PackTypeCounts;
		readonly DynamicBusinessObject BizO;

		#region GetCompletePackageSummary

		public static ZString GetCompletePackageSummary(IEnumerable<GroupedPackTypeCounts> groupedPackTypeCounts)
		{
			ZString result = ZString.Empty;

			if (groupedPackTypeCounts != null && groupedPackTypeCounts.Any())
			{
				result = GetCompletePackageSummary(IEnumerableExtensions.DistinctBy(groupedPackTypeCounts.Where(g => !g.IsEmpty), g => g.Key).SelectMany(g => g.PackTypeCounts));
			}

			return result;
		}

		public static ZString GetCompletePackageSummary(IEnumerable<PackTypeCount> packTypeCounts)
		{
			ZString result = ZString.Empty;

			if (packTypeCounts != null && packTypeCounts.Any())
			{
				var packTypeQuantities = GetPackTypeQuantities(packTypeCounts);
				result = GetPackageSummaryCore(packTypeQuantities.OrderBy(kvp => kvp.Key).Select(kvp => new PackTypeCount(kvp.Key, kvp.Value)));
			}

			return result;
		}

		static Dictionary<ZString, ZInt> GetPackTypeQuantities(IEnumerable<PackTypeCount> packTypeCounts)
		{
			var packTypeQuantities = new Dictionary<ZString, ZInt>();

			foreach (var packTypeCount in packTypeCounts)
			{
				ZInt quantity;
				if (!packTypeQuantities.TryGetValue(packTypeCount.PackType, out quantity))
				{
					quantity = 0;
					packTypeQuantities.Add(packTypeCount.PackType, quantity);
				}

				packTypeQuantities[packTypeCount.PackType] = quantity + packTypeCount.Quantity;
			}

			return packTypeQuantities;
		}

		#endregion

		#region Cache Indexer

		public object this[ZString cachePropertyName]
		{
			get { return BizO[cachePropertyName]; }
		}

		#endregion

		#region BookedPickupPackageList

		public GroupedPackTypeCounts BookedPickupPackageList
		{
			get { return BookedPackTypeCounts; }
		}

		#endregion

		#region PackageSummary

		public ZString PackageSummary
		{
			get { return GetPackageSummaryCore(PackTypeCounts); }
		}

		static ZString GetPackageSummaryCore(IEnumerable<PackTypeCount> packTypeQuantities)
		{
			return string.Join(", ", packTypeQuantities);
		}

		#endregion

		#region PackageSummaryList

		public IEnumerable<PackTypeCount> PackageSummaryList
		{
			get { return PackTypeCounts; }
		}

		#endregion
	}
}
