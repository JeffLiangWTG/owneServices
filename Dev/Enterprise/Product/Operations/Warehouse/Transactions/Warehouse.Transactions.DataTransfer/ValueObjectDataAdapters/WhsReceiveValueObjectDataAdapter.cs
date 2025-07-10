using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsReceiveValueObjectDataAdapter : WhsDocketValueObjectDataAdapter<WhsReceive>
	{
		#region Constructors

		public WhsReceiveValueObjectDataAdapter()
			: base(EventsWithSourceType.Empty)
		{
		}

		public WhsReceiveValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#endregion

		#region Data Formatter

		protected override WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsReceiveDataFormatter();
		}

		#endregion

		#region Error Handler

		protected override IErrorHandler GetNewDocketErrorHandler()
		{
			return new WhsReceiveErrorHandler();
		}

		#endregion

		#region Overrides

		#region AllowDocketModifications

		protected override bool AllowDocketModifications(WhsReceive receive)
		{
			return base.AllowDocketModifications(receive) && !receive.IsReadyForPlanningOrPlanned;
		}

		#endregion

		#region Cross Dock

		protected override WhsPickLineCollection GetExportLineReservedPickLines(WhsDocketLine line)
		{
			return (line.Inventory.Count == 0 ? null : line.Inventory[0].ReservedPickLines);
		}

		protected override void SetExportLineReservedPickLineReferences(WhsPickLine reservedPickLine, Xsd.WhsDocketLineCrossDockLine xsdCrossDockLine)
		{
			base.SetExportLineReservedPickLineReferences(reservedPickLine, xsdCrossDockLine);

			var orderLine = reservedPickLine.DocketLine;
			xsdCrossDockLine.Reference = orderLine.Docket.WD_ExternalReference;
			xsdCrossDockLine.LineNumber = orderLine.WE_LineNo;
			xsdCrossDockLine.SubLineNumber = orderLine.WE_SubLineNo;
		}

		protected override WhsOrderLine GetCrossDockLinkOrderLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			return (WhsOrderLine)importLineCrossDockLine;
		}

		protected override WhsInventoryView GetGrossDockLinkInventoryLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			WhsReceiveLine receiveLine = (WhsReceiveLine)importLine;
			return (receiveLine.Inventory.Count == 0 ? null : receiveLine.Inventory[0]);
		}

		protected override ZString GetExpectedCrossDockDocketType()
		{
			return Transactions.CodeLists.DocketType.Codes.Order;
		}

		#endregion

		#region ImportLineQuantity

		protected override void ImportLineQuantity(WhsDocketLine line, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
			ZDecimal quantity = (xsdLine.QuantityFromClientOrder == 0) ? xsdLine.QuantityActuallyOrdered : xsdLine.QuantityFromClientOrder;

			OrgSupplierPart part = line.SupplierPart;
			if (part != null)
			{
				ZDecimal units = part.UnitConverter.Convert(quantity, xsdLine.ProductUQ, line.ProductUQ);
				units = (units != 0m) ? units : quantity;
				quantity = Utilities.Round(units, part.OP_CountDecimalPlaces);
			}
			line.WE_ClientOrderedUnits = quantity;
		}

		#endregion

		protected override ZString GetXsdWhsDocketType()
		{
			return DocketTypes.Codes.WhsASN;
		}

		protected override void ImportWhsDocketOtherDetails(WhsReceive bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			base.ImportWhsDocketOtherDetails(bizObj, value, context);
			if (value.DocketDetail.Item is Xsd.WhsCustomerInwardsDetail)
			{
				bizObj.CalculateTotalsEnabled = false;
				bizObj.WD_TotalUnits = value.DocketDetail.Units;
				bizObj.WD_TotalPallets = (ZShort)value.DocketDetail.Pallets;
				bizObj.CalculateTotalsEnabled = true;

				var customerInwardsDetail = (Xsd.WhsCustomerInwardsDetail)value.DocketDetail.Item;
				var calculationTimeZone = bizObj.Warehouse?.RelatedCompanyBranch?.HomePort?.TimeZoneSet?.GetCalculationTimeZone();

				ImportAddress(bizObj, customerInwardsDetail.Supplier, context, (NoResString)"Supplier"); // May be an identifier or GUID.

				if (customerInwardsDetail.ArrivalDate.IsValid && !customerInwardsDetail.ArrivalDate.IsEmpty)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(bizObj.WD_ArrivalDateInfo, ConvertLocalTimeToOffset(customerInwardsDetail.ArrivalDate));
				}
				else
				{
					bizObj.WD_ArrivalDate = ZDateTimeOffset.Empty;
				}

				if (customerInwardsDetail.BookingDate.IsValid && !customerInwardsDetail.BookingDate.IsEmpty)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(bizObj.WD_BookingDateInfo, ConvertLocalTimeToOffset(customerInwardsDetail.BookingDate));
				}
				else
				{
					bizObj.WD_BookingDate = ZDateTimeOffset.Empty;
				}

				if (customerInwardsDetail.ETD.IsValid && !customerInwardsDetail.ETD.IsEmpty)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(bizObj.WD_ETDInfo, ConvertLocalTimeToOffset(customerInwardsDetail.ETD));
				}
				else
				{
					bizObj.WD_ETD = ZDateTimeOffset.Empty;
				}

				if (customerInwardsDetail.ETA.IsValid && !customerInwardsDetail.ETA.IsEmpty)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(bizObj.WD_ETAInfo, ConvertLocalTimeToOffset(customerInwardsDetail.ETA));
				}
				else
				{
					bizObj.WD_ETA = ZDateTimeOffset.Empty;
				}

				ZDateTimeOffset ConvertLocalTimeToOffset(ZDateTime localDateTime)
				{
					var dateTimeOffset = ZDateTimeOffset.Now;
					if (calculationTimeZone != null)
					{
						var offset = calculationTimeZone.GetUtcOffsetBasedOnLocal(localDateTime.ToDateTime());
						dateTimeOffset = new ZDateTimeOffset(localDateTime, DateTimeKind.Local, offset);
					}
					return dateTimeOffset;
				}
			}
		}

		protected override void ImportLineAdditionalDetails(WhsReceive bizObj, WhsDocketLine line, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
			base.ImportLineAdditionalDetails(bizObj, line, xsdLine, context);
			UpdateLineRelatedInventory(bizObj, line);
		}

		protected override void ProcessExistingLineNotIncludedInImport(WhsReceive bizObj, WhsDocketLine line, IValueObjectImportContext context)
		{
			base.ProcessExistingLineNotIncludedInImport(bizObj, line, context);

			if (bizObj.Client.MiscServ.OM_WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage)
			{
				line.WE_ClientOrderedUnits = 0;
				UpdateLineRelatedInventory(bizObj, line);
			}
		}

		protected override void ExportAdditionalToValueObjectCore(WhsReceive bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			base.ExportAdditionalToValueObjectCore(bizObj, value, context);
			value.DocketDetail.Item = new Xsd.WhsCustomerInwardsDetail();
			var xsdInwards = (Xsd.WhsCustomerInwardsDetail)value.DocketDetail.Item;
			xsdInwards.Supplier = new DocAddressValueObjectHelper((NoResString)"Supplier And Address").ExportToValueObject(bizObj.SupplierDocAddress, context); // May be an identifier or GUID.
			if (!bizObj.WD_ArrivalDate.IsEmpty)
			{
				xsdInwards.ArrivalDate = bizObj.WD_ArrivalDate.ToLocalZDateTime();
			}
			if (!bizObj.WD_BookingDate.IsEmpty)
			{
				xsdInwards.BookingDate = bizObj.WD_BookingDate.ToLocalZDateTime();
			}
			if (!bizObj.WD_ETA.IsEmpty)
			{
				xsdInwards.ETA = bizObj.WD_ETA.ToLocalZDateTime();
			}
			if (!bizObj.WD_ETD.IsEmpty)
			{
				xsdInwards.ETD = bizObj.WD_ETD.ToLocalZDateTime();
			}
			value.DocketDetail.Units = bizObj.WD_TotalUnits;
			value.DocketDetail.UnitsSpecified = true;
			value.DocketDetail.Pallets = bizObj.WD_TotalPallets;
			value.DocketDetail.PalletsSpecified = true;
		}

		protected override bool IncludeeDocs => SystemDataRegistry.Instance.IncludeWhsReceipteDocs.Value;

		#endregion

		#region Implementation

		void UpdateLineRelatedInventory(WhsReceive bizObj, WhsDocketLine line)
		{
			if (line.WE_TransactionQuantity == 0m)
			{
				line.WE_TransactionQuantity = line.WE_ClientOrderedUnits;
			}

			if (line.WE_F3_NKPackType.IsEmpty && line.SupplierPart != null)
			{
				line.WE_F3_NKPackType = line.SupplierPart.OP_StockKeepingUnit;
			}

			line.WE_AdjustmentArrivalDate = bizObj.WD_ArrivalDate;
			line.WE_StockOnHand = line.WE_TransactionQuantity;
		}

		#endregion
	}
}
