using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class ETailCusSCAHouseDataObjectReader : CusSCAHouseDataObjectReader
	{
		public ETailCusSCAHouseDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(oceanBill, hvlvConsolidatorShipmentWrapper, dataObject, logger, factory)
		{
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusSCAHouse houseBill)
		{
			var reason = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(houseBill);
			if (reason.IsEmpty && IsMessagingActive(houseBill))
			{
				reason = Res.GetString("5BAC3977-E823-4CDF-8A88-633816C1BCBF", "Messaging is active for {0}", houseBill.HumanReadableName);
			}
			return reason;
		}

		protected override void PopulatePackingLineCollection(IColumnIndexer houseBill)
		{
			var lineNo = 0;

			foreach (var packingLine in dataObject.PackingLineCollection)
			{
				var packedItems = packingLine?.PackedItemCollection;
				if (packedItems != null && packedItems.Count > 0)
				{
					foreach (var packedItem in packedItems)
					{
						lineNo++;
						var reader = new ETailCusSCAPackingLineDataObjectReader(lineNo, houseBill, hvlvConsolidatorShipmentWrapper, dataObject, packingLine, packedItem, logger, factory);
						reader.ReadIntoBusinessObject();
					}
				}
				else
				{
					lineNo++;
					var reader = new ETailCusSCAPackingLineDataObjectReader(lineNo, houseBill, hvlvConsolidatorShipmentWrapper, dataObject, packingLine, null, logger, factory);
					reader.ReadIntoBusinessObject();
				}
			}
		}

		protected override void PopulateCountrySpecificDetails(IColumnIndexer houseBill)
		{
			base.PopulateCountrySpecificDetails(houseBill);
			SetValue(houseBill, CusSCAHouseSchema.CA_GoodsValue, GetGoodsValueFallback());
			SetValue(houseBill, CusSCAHouseSchema.CA_JS, hvlvConsolidatorShipmentWrapper?.ShipmentPK ?? ZGuid.Empty);
		}

		ZDecimal? GetGoodsValueFallback() => GetFallbackValue(
			dataObject?.GoodsValue,
			SumItemsGoodsValue());

		ZDecimal? SumItemsGoodsValue()
		{
			var packedItems = dataObject
				?.PackingLineCollection
				?.Where(item => item.PackedItemCollection != null)
				.SelectMany(item => item.PackedItemCollection);
			if (packedItems == null)
			{
				return null;
			}
			else
			{
				var commercialInvoiceLines = packedItems.Select(line => line.FindMatchingCommercialInvoiceLine(dataObject));
				return commercialInvoiceLines?.Sum(itemLine => itemLine.CustomsValue);
			}
		}

		TValue GetFallbackValue<TValue>(params TValue[] values)
		{
			return values.FirstOrDefault(v =>
				v is ZString stringValue && !string.IsNullOrEmpty(stringValue)
				|| v is ICodeDataObject codeDataObject && !string.IsNullOrEmpty(codeDataObject?.Code)
				|| v is ZDecimal decimalValue && decimalValue > ZDecimal.Zero
				|| v is ZInt intValue && intValue > ZInt.Zero);
		}

		public bool IsMessagingActive(CusSCAHouse houseBill)
		{
			return
				houseBill != null &&
				houseBill.IsInDatabase &&
				!IsMessagingBeingNotActive(houseBill.CA_MessageStatus);
		}

		HashSet<string> MessagingBeingNotActiveList => messagingBeingNotActiveList ??=
			new HashSet<string>
			{
				LowValueManifestStatusList.Codes.ManifestRejected,
				LowValueManifestStatusList.Codes.NotSentToCustoms,
				LowValueManifestStatusList.Codes.ManifestInError,
				LowValueManifestStatusList.Codes.ManifestCancelled
			};
		HashSet<string> messagingBeingNotActiveList;

		bool IsMessagingBeingNotActive(string status)
		{
			return string.IsNullOrEmpty(status) || MessagingBeingNotActiveList.Contains(status);
		}
	}
}
