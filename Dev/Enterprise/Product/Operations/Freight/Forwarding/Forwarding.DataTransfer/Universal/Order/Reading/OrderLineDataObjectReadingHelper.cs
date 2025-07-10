using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class OrderLineDataObjectReadingHelper : DataObjectWithWorkflowCustomFieldsReader<UniversalOrderLine>
	{
		internal OrderLineDataObjectReadingHelper(UniversalOrderLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Order parent, bool useLineReferenceMatching) : base(dataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, "Order parent");
			this.useLineReferenceMatching = useLineReferenceMatching;
			targetDataObject = null;
		}

		internal OrderLineDataObjectReadingHelper(UniversalOrderLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Order parent, IDataTargetDataObject targetDataObject) : base(dataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, "Order parent");
			this.targetDataObject = targetDataObject;
			useLineReferenceMatching = (targetDataObject?.Key ?? ZString.Empty).SplitIgnoringEscapedDelimiter('~', '!').Length == 4;
		}

		readonly Order parent;
		readonly bool useLineReferenceMatching;
		readonly IDataTargetDataObject targetDataObject;

		internal bool UseLineReferenceMatching => useLineReferenceMatching;

		internal ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(OrderLine orderLineBO, bool isNewBO)
		{
			if (!(targetDataObject?.Key ?? ZString.Empty).IsEmpty && orderLineBO != null)
			{
				if (useLineReferenceMatching)
				{
					if ((dataObject.LineReference ?? ZString.Empty) != orderLineBO.JO_LineReference)
					{
						return Res.GetString("f758bfdf-ee27-4f0b-8078-086eca6849fb", "Line reference {0} in the Order Line is different from key {1} in the data context.", dataObject.LineReference, targetDataObject.Key.Value);
					}
				}
				else
				{
					if (dataObject.LineNumber != orderLineBO.JO_LineNo && (dataObject.SubLineNumber ?? (ZInt)1) != orderLineBO.JO_SubLineNo)
					{
						return Res.GetString("fe69882e-532a-4bad-9b36-731d8b38ff18", "Line ({0}-{1}) in the Order Line are different from key {2} in the data context.", dataObject.LineNumber, dataObject.SubLineNumber, targetDataObject.Key.Value);
					}
				}
			}

			if (!useLineReferenceMatching)
			{
				if (dataObject.LineNumber <= 0)
				{
					return Res.GetString("{A3A57882-F993-4584-91C3-B3EB683B13D5}", "The line number must be greater than 0.");
				}

				//mainly used to prevent triggering unique index constraint FK_UX__JO_JD_JO_LineReference in database
				if (dataObject.LineReference is ZString reference
					&& !reference.IsEmpty
					&& parent.OrderLines.OfType<OrderLine>().Any(line => (isNewBO || line.PK != orderLineBO.PK) && line.JO_LineReference == reference))
				{
					if (orderLineBO?.IsInDatabase ?? false)
					{
						return Res.GetString("{85D0D1A6-E588-4F96-AAC7-C443B4CE81AD}",
							"Matching order line's (order line {0} and sub-line {1}) Line Reference cannot be updated to {2} because {2} already exists on another order line.",
							orderLineBO.JO_LineNo,
							orderLineBO.JO_SubLineNo,
							reference);
					}
					else
					{
						return Res.GetString("{5FE463DF-4070-4CFC-B914-DE5778B02737}", "New order line cannot be created because Line Reference {0} already exists on another order line.", reference);
					}
				}
			}

			return ZString.Empty;
		}

		#region PopulateBusinessObject

		internal OrderLine PopulateBusinessObject(OrderLine orderLine, bool isNewBO)
		{
			ISupportDataImporting iSupportDataImporting = orderLine;

			using (new DisposableAction(
				() => iSupportDataImporting.IsImportingData = true,
				() => iSupportDataImporting.IsImportingData = false))
			{
				PopulateBusinessObjectCore(orderLine, isNewBO);
				PopulateDangerousGoods(orderLine);
				PopulateOrganisations(orderLine);

				OrderImportValidationHelper.ValidateOrderLine(logger, orderLine);

				orderLine.LogEventForMismatchedShipWindow(false);
			}

			return orderLine;
		}

		void PopulateBusinessObjectCore(OrderLine orderLine, bool isNewBO)
		{
			if (useLineReferenceMatching)
			{
				if (isNewBO)
				{
					SetValue(orderLine, JobOrderLineSchema.JO_LineNo, ProcessLineNumber(null));
					SetValue(orderLine, JobOrderLineSchema.JO_SubLineNo, (ZInt)1);
					SetValue(orderLine, JobOrderLineSchema.JO_LineReference, dataObject.LineReference);
				}
			}
			else
			{
				if (isNewBO)
				{
					SetValue(orderLine, JobOrderLineSchema.JO_LineNo, ProcessLineNumber(dataObject.LineNumber));
					SetValue(orderLine, JobOrderLineSchema.JO_SubLineNo, dataObject.SubLineNumber);
				}

				SetValue(orderLine, JobOrderLineSchema.JO_LineReference, dataObject.LineReference);
			}

			if (isNewBO)
			{
				SetValue(orderLine, JobOrderLineSchema.JO_JD, parent.PK);
			}

			SetValue(orderLine, JobOrderLineSchema.JO_UnitOfVolume, dataObject.VolumeUnit);
			SetValue(orderLine, JobOrderLineSchema.JO_UnitOfWeight, dataObject.WeightUnit);
			SetValue(orderLine, JobOrderLineSchema.JO_ActualVolume, dataObject.Volume);
			SetValue(orderLine, JobOrderLineSchema.JO_ActualWeight, dataObject.Weight);
			SetValue(orderLine, JobOrderLineSchema.JO_AdditionalInformation, dataObject.AdditionalInformation);
			SetValue(orderLine, JobOrderLineSchema.JO_AdditionalTerms, dataObject.AdditionalTerms);
			SetValue(orderLine, JobOrderLineSchema.JO_CommercialInvoiceNo, dataObject.CommercialInvoiceNumber);
			SetValue(orderLine, JobOrderLineSchema.JO_ConfirmationDate, dataObject.SupplierConfirmedAcceptance);
			SetValue(orderLine, JobOrderLineSchema.JO_ConfirmationNum, dataObject.ConfirmationNumber);
			SetValue(orderLine, JobOrderLineSchema.JO_ContainerNumber, dataObject.ContainerNumber);
			SetValue(orderLine, JobOrderLineSchema.JO_ContainerPackingOrder, dataObject.ContainerPackingOrder);
			SetValue(orderLine, JobOrderLineSchema.JO_Description, dataObject.LineComment);
			SetValue(orderLine, JobOrderLineSchema.JO_ExWorksDate, dataObject.RequiredExWorks);
			SetValue(orderLine, JobOrderLineSchema.JO_F3_NKPackType, dataObject.OrderedQtyUnit);
			SetValue(orderLine, JobOrderLineSchema.JO_INCO, dataObject.IncoTerm);
			SetValue(orderLine, JobOrderLineSchema.JO_InnerPacks, dataObject.InnerPacksQty);
			SetValue(orderLine, JobOrderLineSchema.JO_InnerPacksUQ, dataObject.InnerPacksQtyUnit);
			SetValue(orderLine, JobOrderLineSchema.JO_ItemPrice, dataObject.UnitPriceRecommended);
			SetValue(orderLine, JobOrderLineSchema.JO_LineDropDate, dataObject.RequiredInStore);
			SetValue(orderLine, JobOrderLineSchema.JO_LinePrice, dataObject.ExtendedLinePrice);
			SetValue(orderLine, JobOrderLineSchema.JO_LineSplitNumber, dataObject.LineSplitNumber);
			SetValue(orderLine, JobOrderLineSchema.JO_LineStatus, dataObject.Status);
			SetValue(orderLine, JobOrderLineSchema.JO_OuterPacks, dataObject.PackageQty);
			SetValue(orderLine, JobOrderLineSchema.JO_OuterPacksUQ, dataObject.PackageQtyUnit);
			SetValue(orderLine, JobOrderLineSchema.JO_OuterPackLength, dataObject.PackageLength);
			SetValue(orderLine, JobOrderLineSchema.JO_OuterPackHeight, dataObject.PackageHeight);
			SetValue(orderLine, JobOrderLineSchema.JO_OuterPackWidth, dataObject.PackageWidth);

			if (dataObject.PackageLengthUnit != null)
			{
				SetValue(orderLine, JobOrderLineSchema.JO_OuterPackUnitOfDimension, dataObject.PackageLengthUnit);
			}

			SetValue(orderLine, JobOrderLineSchema.JO_PartAttrib1, dataObject.PartAttribute1);
			SetValue(orderLine, JobOrderLineSchema.JO_PartAttrib2, dataObject.PartAttribute2);
			SetValue(orderLine, JobOrderLineSchema.JO_PartAttrib3, dataObject.PartAttribute3);
			SetValue(orderLine, JobOrderLineSchema.JO_SerialNumber, dataObject.SerialNumber);

			if (dataObject.Product != null)
			{
				SetValue(orderLine, JobOrderLineSchema.JO_Partno, dataObject.Product);
				SetValue(orderLine, JobOrderLineSchema.JO_Description, dataObject.Product.Description);
			}

			if (dataObject.CustomsData != null)
			{
				SetValue(orderLine, JobOrderLineSchema.JO_RN_NKCountryOfOrigin, dataObject.CustomsData.CountryOfOrigin);
			}

			if (dataObject.Status.GetCodeAsUpperCase() == Core.Constants.OrderStatus.Cancelled)
			{
				SetValue(orderLine, JobOrderLineSchema.JO_QtyInvoiced, 0m);
				SetValue(orderLine, JobOrderLineSchema.JO_QtyReceived, 0m);
				SetValue(orderLine, JobOrderLineSchema.JO_Quantity, 0m);
			}
			else
			{
				SetValue(orderLine, JobOrderLineSchema.JO_QtyInvoiced, dataObject.ExpectedQuantity);
				SetValue(orderLine, JobOrderLineSchema.JO_QtyReceived, dataObject.QuantityMet);
				SetValue(orderLine, JobOrderLineSchema.JO_Quantity, dataObject.OrderedQty);
			}

			SetValue(orderLine, JobOrderLineSchema.JO_SpecialInstructions, dataObject.SpecialInstructions);
			SetValue(orderLine, JobOrderLineSchema.JO_UnderQuantityPercentageLimit, dataObject.UnderQuantityPercentageLimit);
			SetValue(orderLine, JobOrderLineSchema.JO_OverQuantityPercentageLimit, dataObject.OverQuantityPercentageLimit);
			SetValue(orderLine, JobOrderLineSchema.JO_LateShipmentLimitDays, dataObject.LateShipmentLimitDays);
			SetValue(orderLine, JobOrderLineSchema.JO_EarlyShipmentLimitDays, dataObject.EarlyShipmentLimitDays);
			SetValue(orderLine, JobOrderLineSchema.JO_ShipmentWindowStart, dataObject.ShipmentWindowStart);
			SetValue(orderLine, JobOrderLineSchema.JO_ShipmentWindowEnd, dataObject.ShipmentWindowEnd);
			SetValue(orderLine, JobOrderLineSchema.JO_HSCode, dataObject.HarmonisedCode);
			SetValue(orderLine, JobOrderLineSchema.JO_RH_NKCommodityCode, dataObject.Commodity?.Code.GetValueOrDefault() ?? ZString.Empty);

			PopulateCustomFields(orderLine);
		}

		ZInt? ProcessLineNumber(ZInt? lineNumber)
		{
			return lineNumber.HasValue ? lineNumber : (ZInt)(parent.OrderLines.MaxOrDefault(line => line.JO_LineNo) + 1);
		}

		#region PopulateCustomFields

		void PopulateCustomFields(OrderLine orderLine)
		{
			var usedCustomField = new CustomLabelsCustomizedFieldDataObjectReader(logger).PopulateCustomFields(JobOrderLineSchema.Instance, orderLine, dataObject, OrderLine.NewCustomLabelsProvider(parent));
			PopulateWorkflowCustomFields(orderLine, dataObject, usedCustomField);
		}

		#endregion

		#region PopulateDangerousGoods

		void PopulateDangerousGoods(OrderLine orderLine)
		{
			if (dataObject.UNDGCollection != null)
			{
				var reader = new UNDGDataObjectCollectionReader(logger, factory, orderLine, dataObject.UNDGCollection.ToArray());
				reader.ReadIntoCollection();
			}
		}

		#endregion

		#region PopulateOrganisations

		void PopulateOrganisations(OrderLine orderLine)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				foreach (var organisationDataObject in dataObject.OrganizationAddressCollection)
				{
					if ((organisationDataObject.AddressType ?? ZString.Empty) == nameof(DocAddressType.ConsignorPickupDeliveryAddress))
					{
						new OrganisationDataObjectReader(organisationDataObject, logger, factory).GetMatchedOrNew(orderLine, DocAddressType.GoodsAvailableAt);
						continue;
					}

					if ((organisationDataObject.AddressType ?? ZString.Empty) == nameof(DocAddressType.ConsigneePickupDeliveryAddress))
					{
						new OrganisationDataObjectReader(organisationDataObject, logger, factory).GetMatchedOrNew(orderLine, DocAddressType.GoodsDeliveredTo);
						continue;
					}

					new OrganisationDataObjectReader(organisationDataObject, logger, factory).GetMatchedOrNew(orderLine);
				}
			}
		}

		#endregion

		#endregion

		public static OrderLineDataObjectReadingHelper GetHelper(UniversalShipment dataObject, OrderLine orderLineBO, DataContextType dataContextType, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (dataObject?.Order?.OrderLineCollection?.Count == 1 && orderLineBO?.Order != null)
			{
				return new OrderLineDataObjectReadingHelper(dataObject.Order.OrderLineCollection[0], logger, factory, orderLineBO.Order, dataObject.DataContext?.GetMatchingDataTarget(dataContextType));
			}

			return null;
		}
	}
}
