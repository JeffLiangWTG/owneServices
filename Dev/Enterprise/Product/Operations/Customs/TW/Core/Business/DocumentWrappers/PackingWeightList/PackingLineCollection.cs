using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class PackingLineCollection : NonPersistentBusinessObjectCollection<PackingLine>
	{
		internal PackingLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PackingLine(Factory);
		}

		internal ZString PackTypeSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(PackTypeSummaryInfo);

		internal ZString QuantitySummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(QuantitySummaryInfo);

		internal ZString NetWeightSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(NetWeightSummaryInfo);

		internal ZString GrossWeightSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(GrossWeightSummaryInfo);

		internal ZString VolumeSummary => DocumentWrapperHelper.BuildKeyValueStringFromDictionary(VolumeSummaryInfo);

		internal IDictionary<ZString, ZDecimal> PackTypeSummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> QuantitySummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> NetWeightSummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> GrossWeightSummaryInfo { get; set; }

		IDictionary<ZString, ZDecimal> VolumeSummaryInfo { get; set; }

		internal void SetPackingSummaries()
		{
			PackTypeSummaryInfo = new Dictionary<ZString, ZDecimal>();
			QuantitySummaryInfo = new Dictionary<ZString, ZDecimal>();
			NetWeightSummaryInfo = new Dictionary<ZString, ZDecimal>();
			GrossWeightSummaryInfo = new Dictionary<ZString, ZDecimal>();
			VolumeSummaryInfo = new Dictionary<ZString, ZDecimal>();

			foreach (PackingLine packingLine in this)
			{
				if (packingLine.OnlyGoodsDescriptionAndQuantity.IsEmpty)
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(PackTypeSummaryInfo, packingLine.PackTypeInfo, (ZDecimal)packingLine.PackQtyInfo);
					DocumentWrapperHelper.AddOrUpdateDictionary(NetWeightSummaryInfo, packingLine.NetWeightUQ, packingLine.NetWeight);
					DocumentWrapperHelper.AddOrUpdateDictionary(GrossWeightSummaryInfo, packingLine.GrossWeightUQ, packingLine.GrossWeight);
					DocumentWrapperHelper.AddOrUpdateDictionary(VolumeSummaryInfo, packingLine.VolumeUQ, packingLine.Volume);
				}
				foreach (var uq in packingLine.PivotQuantityInfo.Keys)
				{
					DocumentWrapperHelper.AddOrUpdateDictionary(QuantitySummaryInfo, uq, packingLine.PivotQuantityInfo[uq]);
				}
			}
		}
	}
}
