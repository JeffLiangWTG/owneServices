using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// A combination of container type plus commodity code used on a job,
	/// plus a set of matching rates for the user to choose from.
	/// </summary>
	public class ChooserContainerCommodity
	{
		/// <summary>
		/// For FCL
		/// </summary>
		/// <param name="containerRef"></param>
		/// <param name="containerQuality"></param>
		/// <param name="commodityCode"></param>
		/// <param name="containerCount">number of containers on job of type and commodity</param>
		public ChooserContainerCommodity(RefContainer containerRef, string containerQuality, ZString commodityCode, int containerCount = 1)
		{
			CommodityCode = commodityCode;
			ContainerRef = containerRef;
			ContainerQuality = containerQuality;
			RateCollection = new ChooserRateEntryCollection(null);
			ContainerCount = containerCount;
			ContainerTypeWithCommodityCode = string.Format(CultureInfo.InvariantCulture, "{0} ({1}) ({2})",
				ContainerRef.RC_Code,
				ContainerQuality,
				CommodityCode); // not to be translated
		}

		/// <summary>
		/// For LCL
		/// </summary>
		public ChooserContainerCommodity()
		{
			CommodityCode = string.Empty;
			ContainerQuality = string.Empty;
			RateCollection = new ChooserRateEntryCollection(null);
		}

		public ZString CommodityCode { get; }
		public RefContainer ContainerRef { get; }
		public string ContainerQuality { get; }
		public int ContainerCount { get; }

		public ChooserRateEntryCollection RateCollection { get; }
		public IEnumerable<ChooserRateEntry> Rates => RateCollection.Cast<ChooserRateEntry>();
		public ChooserRateEntry SelectedRate { get; set; }

		public ZString ContainerTypeWithCommodityCode { get; }
	}
}
