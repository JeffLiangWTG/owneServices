using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using BusinessCodeLists = Enterprise.Warehouse.Transactions.CodeLists;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsWorkOrderDataObjectReader : WhsComponentOrderDataObjectReader<WhsWorkOrder, WhsWorkOrderLine>
	{
		public WhsWorkOrderDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		#region Create / Update Job

		protected override void PopulateBusinessObjectCore(WhsWorkOrder workOrder)
		{
			base.PopulateBusinessObjectCore(workOrder);

			SetValueIfNotReadOnly(workOrder, WhsDocketSchema.WD_PackagesSent, dataObject.OuterPacks);

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				SetValueIfNotReadOnly(workOrder, WhsDocketSchema.WD_AutoFinaliseBOMIntoInventory, orderDataObject.AutoFinaliseBOMIntoInventory);
				SetValueIfNotReadOnly(workOrder, WhsDocketSchema.WD_IsInwardsProcessingJob, orderDataObject.IsInwardsProcessingJob);
				SetValueIfNotReadOnly(workOrder, WhsDocketSchema.WD_TotalPallets, orderDataObject.PalletsSent);
			}
		}

		protected override IEnumerable<WhsDocketLine> GetLinesForTotalWeightAndVolumeCalculation(WhsWorkOrder workOrder) => workOrder.GetLinesToPick();

		protected override void CalculateTotalUnitsIfNeeded(WhsWorkOrder workOrder)
		{
			if (dataObject.Order?.TotalUnits == null)
			{
				var totalUnits = workOrder.IsAssembly
					? workOrder.AssemblyLinesForReceive.Sum(l => GetPlannedAssemblyQuantity(l))
					: workOrder.GetPlannedDisassemblyQuantity();

				SetValueIfNotReadOnly(workOrder, WhsDocketSchema.WD_TotalUnits, totalUnits);
			}

			ZDecimal GetPlannedAssemblyQuantity(WhsDocketLine line)
			{
				var quantity = line.WE_TransactionQuantity;

				if (workOrder.WD_IsInwardsProcessingJob)
				{
					quantity += line.SupplierPart.SecondaryParts.Sum(sp => sp.OSB_ProductQuantity) * line.WE_TransactionQuantity;
				}

				return quantity;
			}
		}

		protected override DataObjectReader<OrderLine, WhsWorkOrderLine> GetNewLineReader(
			WhsWorkOrder docket,
			OrderLine orderLineDataObject,
			IEnumerable<WhsWorkOrderLine> matchedLines)
		{
			return new WhsWorkOrderLineDataObjectReader(orderLineDataObject, logger, factory, docket, matchedLines);
		}

		protected override WhsImportStrategy GetNewImportStrategy() => new WorkOrderImportStrategy(this);

		class WorkOrderImportStrategy : ComponentOrderImportStrategy
		{
			public WorkOrderImportStrategy(WhsWorkOrderDataObjectReader reader)
				: base(reader)
			{
			}

			protected override void AfterPopulateCore(WhsWorkOrder docket)
			{
				base.AfterPopulateCore(docket);
				FinaliseComponentOrder(docket);
			}
		}

		#endregion

		#region Matching Job

		public override DataContextType DataContextType => DataContextType.WarehouseWorkOrder;

		protected override string DocketTypeCode => BusinessCodeLists.DocketType.Codes.WorkOrder;

		protected override string DocketType => WorkOrderDocketType;

		internal static string WorkOrderDocketType => Res.GetString("e7969736-0aa0-4a6d-8b03-76033d708494", "Work Order");

		#endregion
	}
}
