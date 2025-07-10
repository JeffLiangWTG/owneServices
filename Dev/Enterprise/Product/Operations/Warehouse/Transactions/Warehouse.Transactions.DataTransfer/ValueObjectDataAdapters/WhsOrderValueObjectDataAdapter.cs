using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.DataTransfer.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderValueObjectDataAdapter : WhsDocketValueObjectDataAdapter<WhsOrder>
	{
		#region Constructors

		public WhsOrderValueObjectDataAdapter()
			: base(EventsWithSourceType.Empty)
		{
		}

		public WhsOrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#endregion

		#region AllowDocketModifications

		protected override bool AllowDocketModifications(WhsOrder order)
		{
			return base.AllowDocketModifications(order) && !(order.Pick?.IsReadyForPlanningOrPlanned ?? false);
		}

		#endregion

		#region Cross Dock

		protected override WhsPickLineCollection GetExportLineReservedPickLines(WhsDocketLine line)
		{
			return ((WhsOrderLine)line).ReservedPickLines;
		}

		protected override void SetExportLineReservedPickLineReferences(WhsPickLine reservedPickLine, Xsd.WhsDocketLineCrossDockLine xsdCrossDockLine)
		{
			base.SetExportLineReservedPickLineReferences(reservedPickLine, xsdCrossDockLine);
			xsdCrossDockLine.Reference = reservedPickLine.Inventory.InDocketLine.Docket.WD_ExternalReference;
			xsdCrossDockLine.LineNumber = reservedPickLine.Inventory.WI_LineNo;
			xsdCrossDockLine.SubLineNumber = reservedPickLine.Inventory.WI_SubLineNo;
		}

		protected override WhsOrderLine GetCrossDockLinkOrderLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			return (WhsOrderLine)importLine;
		}

		protected override WhsInventoryView GetGrossDockLinkInventoryLine(WhsDocketLine importLine, WhsDocketLine importLineCrossDockLine)
		{
			var receiveLine = (WhsReceiveLine)importLineCrossDockLine;
			return (receiveLine.Inventory.Count == 0 ? null : receiveLine.Inventory[0]);
		}

		protected override void ExamineAndUpdateIfNeededExistingCrossDockLinks(WhsDocketLine importLine)
		{
			base.ExamineAndUpdateIfNeededExistingCrossDockLinks(importLine);
			var orderLine = (WhsOrderLine)importLine;

			foreach (var pickLine in orderLine.ReservedPickLines)
			{
				pickLine.ReservedQuantity = 0m;
			}
		}

		protected override void ClearUpUnusedCrossDockLinks(WhsDocketLine importLine)
		{
			base.ClearUpUnusedCrossDockLinks(importLine);

			var orderLine = (WhsOrderLine)importLine;
			foreach (var pickLine in orderLine.ReservedPickLines.ToArray())
			{
				if (pickLine.ReservedQuantity == 0m)
				{
					pickLine.Delete();
				}
			}
		}

		protected override ZString GetExpectedCrossDockDocketType()
		{
			return Transactions.CodeLists.DocketType.Codes.Receive;
		}

		#endregion

		#region Data Formatter

		protected override WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsOrderDataFormatter();
		}

		#endregion

		#region Error Handler

		protected override IErrorHandler GetNewDocketErrorHandler()
		{
			return new WhsOrderErrorHandler();
		}

		#endregion

		#region Export

		protected override void ExportAdditionalToValueObjectCore(WhsOrder bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			base.ExportAdditionalToValueObjectCore(bizObj, value, context);

			value.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			var xsdOrder = (Xsd.WhsCustomerOrderDetail)value.DocketDetail.Item;
			xsdOrder.Consignee = new DocAddressValueObjectHelper((NoResString)"Consignee And Address").ExportToValueObject(bizObj.ConsigneeDocAddress, context); // May be an identifier
			xsdOrder.GoodsBilledTo = new DocAddressValueObjectHelper((NoResString)"Goods Bill To And Address").ExportToValueObject(bizObj.GoodsBillToDocAddress, context); // May be an identifier

			if (!bizObj.WD_RequiredDate.IsEmpty)
			{
				xsdOrder.DateRequired = bizObj.WD_RequiredDate.ToZDateTime();
			}

			xsdOrder.OrderType = bizObj.WD_DocketSubType;
			value.DocketDetail.Units = (bizObj.WD_UnitsSent > 0) ? bizObj.WD_UnitsSent : bizObj.WD_TotalUnitsFromLines;
			value.DocketDetail.UnitsSpecified = true;
			value.DocketDetail.Pallets = bizObj.WD_PalletsSent;
			value.DocketDetail.PalletsSpecified = true;
		}

		protected override void ExportDocketLineAdditionalInfo(WhsDocketLine line, Xsd.WhsDocketLine value, INotifications notifications)
		{
			base.ExportDocketLineAdditionalInfo(line, value, notifications);
			var xsdOrderLine = new Xsd.WhsCustomerOrderLineDetail();
			xsdOrderLine.Pricing.ExtendedPrice = line.WE_ExtendedLinePrice;
			xsdOrderLine.Pricing.RecommendedUnitPrice = line.WE_RecommendedUnitPrice;
			xsdOrderLine.Pricing.UnitDiscount = line.WE_UnitDiscountPercent;
			xsdOrderLine.Pricing.UnitDiscountAmount = line.WE_UnitDiscountAmount;
			xsdOrderLine.Pricing.UnitPriceAfterDiscount = line.WE_UnitPriceAfterDiscount;
			xsdOrderLine.ProjectedShortfallQuantity = ((WhsOrderLine)line).WE_ShortfallQuantityCached;
			xsdOrderLine.ProjectedShortfallQuantitySpecified = true;
			value.Item = xsdOrderLine;
		}

		#endregion

		#region GetXsdWhsDocketType

		protected override ZString GetXsdWhsDocketType()
		{
			return DocketTypes.Codes.WhsOrder;
		}

		#endregion

		#region Import

		protected override void ImportWhsDocketOtherDetails(WhsOrder order, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			base.ImportWhsDocketOtherDetails(order, value, context);

			context.SetPropertyInfoValue(order.WD_ShipperCODAmountInfo, value.DocketDetail.ShipperCODAmount, WhsDocketSchema.WD_ShipperCODAmount);
			context.SetPropertyInfoValue(order.WD_CODPayMethodInfo, value.DocketDetail.ShipperCODType, value.DocketDetail.ShipperCODTypeSpecified, (NoResString)"Shipper COD Type"); // May be an identifier or GUID.
			context.SetPropertyInfoValue(order.WD_LocalCartInsuranceCostInfo, value.DocketDetail.TransportInsurance, WhsDocketSchema.WD_LocalCartInsuranceCost);

			order.CalculateTotalsEnabled = false;
			context.SetPropertyInfoValue(order.WD_TotalUnitsInfo, value.DocketDetail.Units, WhsDocketSchema.WD_TotalUnits);
			order.WD_PalletsSent = (ZShort)value.DocketDetail.Pallets;

			if (value.DocketDetail.Weight.IsSpecified)
			{
				context.SetPropertyInfoValue(order.WD_TotalWeightInfo, value.DocketDetail.Weight.Value, WhsDocketSchema.WD_TotalWeight);
				order.WD_WeightSent = order.WD_TotalWeight;

				if (!value.DocketDetail.Weight.DimensionType.IsEmpty)
				{
					context.SetPropertyInfoValue(order.WD_TotalWeightUnitInfo, value.DocketDetail.Weight.DimensionType, value.DocketDetail.Weight.DimensionTypeSpecified, (NoResString)"Total Weight UQ"); // May be an identifier or GUID.
				}
			}
			if (order.WD_TotalWeightUnit.IsEmpty)
			{
				context.SetPropertyInfoValue(order.WD_TotalWeightUnitInfo, Env.Registry.PackageWeightUnit, true, (NoResString)"Total Weight UQ"); // May be an identifier or GUID.
			}

			if (value.DocketDetail.Cubic.IsSpecified)
			{
				context.SetPropertyInfoValue(order.WD_TotalCubicInfo, value.DocketDetail.Cubic.Value, WhsDocketSchema.WD_TotalCubic);
				order.WD_CubicSent = order.WD_TotalCubic;

				if (!value.DocketDetail.Cubic.DimensionType.IsEmpty)
				{
					context.SetPropertyInfoValue(order.WD_TotalCubicUnitInfo, value.DocketDetail.Cubic.DimensionType, value.DocketDetail.Cubic.DimensionTypeSpecified, (NoResString)"Total Cube UQ"); // May be an identifier or GUID.
				}
			}
			if (order.WD_TotalCubicUnit.IsEmpty)
			{
				context.SetPropertyInfoValue(order.WD_TotalCubicUnitInfo, Env.Registry.PackageVolumeUnit, true, (NoResString)"Total Cube UQ"); // May be an identifier or GUID.
			}
			order.CalculateTotalsEnabled = true;

			if (value.DocketDetail.Weight.Value != 0 || value.DocketDetail.Cubic.Value != 0)
			{
				order.WD_WeightVolSetFromImport = ZBool.True;
			}
			else
			{
				order.WD_WeightVolSetFromImport = ZBool.False;
			}

			if (value.DocketDetail.Item is Xsd.WhsCustomerOrderDetail)
			{
				var dataObjectHelper = new WhsDataObjectReaderHelper(order.Warehouse);
				var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)value.DocketDetail.Item;
				context.SetPropertyInfoValue(order.WD_DocketSubTypeInfo, customerOrderDetail.OrderType, customerOrderDetail.OrderTypeSpecified, (NoResString)"Order Type"); // May be an identifier or GUID.
				if (customerOrderDetail.DateRequired.IsValid && !customerOrderDetail.DateRequired.IsEmpty)
				{
					context.SetPropertyInfoValueIfValueNotEmpty(order.WD_RequiredDateInfo, dataObjectHelper.ConvertToZDateTimeOffset(customerOrderDetail.DateRequired).Value);
				}
				else
				{
					order.WD_RequiredDate = ZDateTimeOffset.Empty;
				}

				ImportAddress(order, customerOrderDetail.Consignee, context, (NoResString)"Consignee"); // May be an identifier

				ImportAddress(order, customerOrderDetail.GoodsBilledTo, context, (NoResString)"Bill to"); // May be an identifier
			}

			if (ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(order))
			{
				new JobHeader.Loader(order).TryLoadOrCreateWithMutex();
			}
		}

		protected override void ImportLineAdditionalDetails(WhsOrder bizObj, WhsDocketLine line, Xsd.WhsDocketLine xsdLine, IValueObjectImportContext context)
		{
			base.ImportLineAdditionalDetails(bizObj, line, xsdLine, context);
			if (xsdLine.Item is Xsd.WhsCustomerOrderLineDetail)
			{
				var customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)xsdLine.Item;
				line.WE_UnitDiscountAmount = customerOrderLineDetail.Pricing.UnitDiscountAmount;
				line.WE_UnitDiscountPercent = customerOrderLineDetail.Pricing.UnitDiscount;
				line.WE_UnitPriceAfterDiscount = customerOrderLineDetail.Pricing.UnitPriceAfterDiscount;
				line.WE_RecommendedUnitPrice = customerOrderLineDetail.Pricing.RecommendedUnitPrice;
				line.WE_ExtendedLinePrice = customerOrderLineDetail.Pricing.ExtendedPrice;
			}
		}

		#endregion

		#region IncludeeDocs

		protected override bool IncludeeDocs => SystemDataRegistry.Instance.IncludeWhsOrdereDocs.Value;

		#endregion
	}
}
