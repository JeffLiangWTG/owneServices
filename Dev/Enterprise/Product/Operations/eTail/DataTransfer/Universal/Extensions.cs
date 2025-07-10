using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public static class Extensions
	{
		public static void AttachMatchingItems(this HVLVOuterPackage outerPackageBO, PackingLine outerPackageDataObject, UniversalObjectFactory factory)
		{
			var packingLines = outerPackageDataObject.PackingLineCollection;

			if (packingLines != null)
			{
				var itemIds = packingLines.Where(p => !string.IsNullOrEmpty(p.ReferenceNumber)).Select(p => p.ReferenceNumber);

				if (itemIds.Any())
				{
					var query = new ZQuery(HVLVItemSchema.HVI_ItemId, itemIds);
					var items = factory.Load<HVLVItem>(query);
					items?.ForEach(item => item.HVI_HVO_OuterPackage = outerPackageBO.PK);
				}
			}
		}

		public static void AttachMatchingItems(this HVLVOriginLoadList originLoadListBO, PackingLine packingLine, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (!string.IsNullOrEmpty(packingLine.ReferenceNumber))
			{
				var hvlvItemBO = factory.LoadTop1<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_ItemId, packingLine.ReferenceNumber));

				if (hvlvItemBO != null)
				{
					if (hvlvItemBO.HVI_JS_LoadedOnShipment.IsEmpty)
					{
						hvlvItemBO.HVI_HVL_LoadList = originLoadListBO.PK;
					}
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("affe8db7-998c-4ffa-943e-2b5d74b1036e", "The HVLV Item with ID {0} has been skipped as it is already attached to Shipment with ID {1}.", hvlvItemBO.HVI_ItemId, hvlvItemBO.Shipment.JS_UniqueConsignRef));
					}
				}
			}
		}

		public static void AttachMatchingItems(this HVLVOriginLoadList originLoadListBO, List<PackingLine> packingLines, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var itemIds = packingLines.Where(p => !string.IsNullOrEmpty(p.ReferenceNumber)).Select(p => p.ReferenceNumber);

			if (itemIds.Any())
			{
				var query = new ZQuery(HVLVItemSchema.HVI_ItemId, itemIds);
				var items = factory.Load<HVLVItem>(query);
				foreach (var item in items)
				{
					if (item.HVI_JS_LoadedOnShipment.IsEmpty)
					{
						item.HVI_HVL_LoadList = originLoadListBO.PK;
					}
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("c229075e-b1df-4c7f-a64d-c82f9bded864", "The HVLV Item with ID {0} has been skipped as it is already attached to Shipment with ID {1}.", item.HVI_ItemId, item.Shipment.JS_UniqueConsignRef));
					}
				}
			}
		}
	}
}
