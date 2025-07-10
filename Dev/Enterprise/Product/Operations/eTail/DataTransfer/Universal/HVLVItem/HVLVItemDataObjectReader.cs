using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVItemDataObjectReader : DataObjectReader<PackingLine, HVLVItem>
	{
		public HVLVItemDataObjectReader(UniversalShipment consignmentDataObject, HVLVConsignment consignmentBO, PackingLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipmentBO = null)
			: base(dataObject, logger, factory)
		{
			this.consignmentDataObject = Argument.NotNull(consignmentDataObject, nameof(consignmentDataObject));
			this.consignmentBO = Argument.NotNull(consignmentBO, nameof(consignmentBO));
			this.shipmentBO = shipmentBO;
		}

		readonly UniversalShipment consignmentDataObject;
		readonly HVLVConsignment consignmentBO;
		readonly ForwardingShipment shipmentBO;

		protected override HVLVItem GetExistingBusinessObject()
		{
			var existingItem = default(HVLVItem);
			if (consignmentBO.IsInDatabase)
			{
				var itemShipperReference = dataObject.OrderReference.GetValueOrDefault();
				if (!itemShipperReference.IsEmpty)
				{
					existingItem = consignmentBO.Items.Cast<HVLVItem>().FirstOrDefault(item => item.HVI_ShipperReference == itemShipperReference);
				}

				if (existingItem == null)
				{
					var itemId = dataObject.ReferenceNumber.GetValueOrDefault();
					if (!itemId.IsEmpty)
					{
						existingItem = consignmentBO.Items.Cast<HVLVItem>().FirstOrDefault(item => item.HVI_ItemId == itemId);
					}
				}
			}

			return existingItem;
		}

		protected override void PopulateBusinessObject(HVLVItem itemBO)
		{
			if (dataObject.PackQty.GetValueOrDefault() > 1)
			{
				throw new DataObjectReadFailureException("Cannot import multi-piece HVLV Item, separate into separate PackingLine entries each with 1 piece.");
			}

			SetValue(itemBO, HVLVItemSchema.HVI_ShipperReference, dataObject.OrderReference);
			SetValue(itemBO, HVLVItemSchema.HVI_CurrentBarcode, dataObject.Barcode);
			SetValue(itemBO, HVLVItemSchema.HVI_F3_NKPackType, dataObject.PackType?.Code);
			SetValue(itemBO, HVLVItemSchema.HVI_GoodsDescription, dataObject.GetCleanSingleLineGoodsDescription()?.SubstringSafe(0, AutoHVLVItem.Schema.HVI_GoodsDescriptionMaxLength));
			SetValue(itemBO, HVLVItemSchema.HVI_ContainerNumber, dataObject.ContainerNumber);

			SetValue(itemBO, HVLVItemSchema.HVI_UnitOfDimension, dataObject.LengthUnit?.Code);
			SetValue(itemBO, HVLVItemSchema.HVI_Height, dataObject.Height);
			SetValue(itemBO, HVLVItemSchema.HVI_Length, dataObject.Length);
			SetValue(itemBO, HVLVItemSchema.HVI_Width, dataObject.Width);
			SetValue(itemBO, HVLVItemSchema.HVI_HVC_Consignment, consignmentBO.PK);

			if (!itemBO.IsInDatabase && itemBO.HVI_JS_LoadedOnShipment.IsEmpty && shipmentBO != null)
			{
				SetValue(itemBO, HVLVItemSchema.HVI_JS_LoadedOnShipment, shipmentBO.PK);
			}

			var hasPackingLineMeasurements = dataObject.ManifestedWeight.HasValue || dataObject.Weight.HasValue || dataObject.ManifestedVolume.HasValue || dataObject.Volume.HasValue;
			var isOnlyPackingLineOnShipment = consignmentDataObject.PackingLineCollection.Count == 1;

			if (hasPackingLineMeasurements)
			{
				SetValue(itemBO, HVLVItemSchema.HVI_ManifestedWeight, dataObject.ManifestedWeight);
				SetValue(itemBO, HVLVItemSchema.HVI_ActualWeight, dataObject.Weight);
				SetValue(itemBO, HVLVItemSchema.HVI_ManifestedVolume, dataObject.ManifestedVolume);
				SetValue(itemBO, HVLVItemSchema.HVI_ActualVolume, dataObject.Volume);
			}
			else if (isOnlyPackingLineOnShipment)
			{
				SetValue(itemBO, HVLVItemSchema.HVI_ManifestedWeight, consignmentDataObject.ManifestedWeight);
				SetValue(itemBO, HVLVItemSchema.HVI_ActualWeight, consignmentDataObject.TotalWeight);
				SetValue(itemBO, HVLVItemSchema.HVI_ManifestedVolume, consignmentDataObject.ManifestedVolume);
				SetValue(itemBO, HVLVItemSchema.HVI_ActualVolume, consignmentDataObject.TotalVolume);
			}
			else
			{
				var itemId = dataObject.ReferenceNumber.GetValueOrDefault();
				var warning = Res.GetString("a6b14ede-c633-4dee-b5b3-42a8f77d3856",
					"HVLV Item ({0}): Could not import measurements as they were not specified on the {1}, and the Consignment is multi-piece.",
					!itemId.IsEmpty ? itemId.ToString() : itemBO.PK.ToString(),
					nameof(PackingLine));
				logger.Log(LogType.Warning, warning);
			}

			itemBO.RecalculateVolume(HVLVItemSchema.HVI_ActualVolume, true);
			itemBO.RecalculateVolume(HVLVItemSchema.HVI_ManifestedVolume, true);

			if (!consignmentBO.Items.Contains(itemBO))
			{
				consignmentBO.Items.Add(itemBO);
			}

			if (dataObject.UNDGCollection != null)
			{
				if (itemBO.IsInDatabase)
				{
					itemBO.UNDGs.DeleteAll();
				}

				dataObject.UNDGCollection.ForEach(undg =>
				{
					var itemUndg = itemBO.UNDGs.AddNew();
					var undgReader = new ShipmentUNDGDataObjectReader(consignmentBO.ShipmentTransportMode, undg, logger, factory, () => itemUndg);
					undgReader.ReadIntoBusinessObject();
				});
			}

			if (itemBO.IsInDatabase
			&& (consignmentDataObject.PackingLineCollection.Content == null || consignmentDataObject.PackingLineCollection.Content == CollectionContent.Complete || (consignmentDataObject.PackingLineCollection.Content == CollectionContent.Partial && dataObject.PackedItemCollection != null && dataObject.PackedItemCollection.Any())))
			{
				itemBO.Lines.RemoveAndDeleteAll();
			}

			dataObject.PackedItemCollection?.ForEach(pi =>
				{
					var line = factory.New<HVLVItemLine>();
					SetValue(line, HVLVItemLineSchema.HVS_ClusterKey, itemBO.HVI_ClusterKey);
					SetValue(line, HVLVItemLineSchema.HVS_HVI_HVLVItem, itemBO.PK);
					itemBO.Lines.Load();

					if (pi.PackedQuantity < 1)
					{
						throw new DataObjectReadFailureException($"{pi.PackedQuantity} is an invalid HVLV Item Line quantity, value must be equal to or greater than 1.");
					}

					SetValue(line, HVLVItemLineSchema.HVS_CustomsValue, pi.CIFValue);
					SetValue(line, HVLVItemLineSchema.HVS_IntrinsicValue, pi.GoodsValue);
					SetValue(line, HVLVItemLineSchema.HVS_ItemURL, pi.ItemSpecificationUrl);
					SetValue(line, HVLVItemLineSchema.HVS_GoodsDescription, pi.Description);
					SetValue(line, HVLVItemLineSchema.HVS_GrossWeight, pi.GrossWeight);
					SetValue(line, HVLVItemLineSchema.HVS_NetWeight, pi.NetWeight);
					SetValue(line, HVLVItemLineSchema.HVS_Quantity, pi.PackedQuantity?.ToZInt());
					SetValue(line, HVLVItemLineSchema.HVS_WeightUnit, pi.GrossWeightUnit);
					SetValue(line, HVLVItemLineSchema.HVS_ProductCode, pi.Product?.Code);

					var commercialInvoiceLine = GetCommercialInvoiceLine(pi.CommercialInvoiceLineLink);
					if (commercialInvoiceLine != null)
					{
						SetValue(line, HVLVItemLineSchema.HVS_OriginGoodsDescription, commercialInvoiceLine.LocalDescription);
						SetValue(line, HVLVItemLineSchema.HVS_DestinationTariff, commercialInvoiceLine.HarmonisedCode);
						SetValue(line, HVLVItemLineSchema.HVS_RN_NKOriginCountryCode, commercialInvoiceLine.CountryOfOrigin);

						if (commercialInvoiceLine.CustomsSupportingInformationCollection != null && commercialInvoiceLine.CustomsSupportingInformationCollection.Any())
						{
							var customsSupportingInformation = commercialInvoiceLine.CustomsSupportingInformationCollection.First();
							var tariff = customsSupportingInformation?.Tariff ?? string.Empty;
							var countryCode = customsSupportingInformation.Country?.Code ?? string.Empty;
							if (!tariff.IsEmpty && countryCode.IsEmpty)
							{
								throw new DataObjectReadFailureException(string.Format("There is an HVLVItemLine with Origin HS Code: {0} but no Origin Country", tariff));
							}
							else
							{
								SetValue(line, HVLVItemLineSchema.HVS_OriginTariff, tariff);
								SetValue(line, HVLVItemLineSchema.HVS_RN_NKOriginCountryCode, countryCode);
							}
						}
					}
				});
		}

		CommercialInvoiceLine GetCommercialInvoiceLine(ZInt? commercialInvoiceLineLink)
		{
			var commercialInvoiceLineCollection = consignmentDataObject
				.CommercialInfo?
				.CommercialInvoiceCollection?
				.FirstOrDefault()?
				.CommercialInvoiceLineCollection;
			return commercialInvoiceLineCollection?.FirstOrDefault(c => c.Link == commercialInvoiceLineLink);
		}
	}
}
