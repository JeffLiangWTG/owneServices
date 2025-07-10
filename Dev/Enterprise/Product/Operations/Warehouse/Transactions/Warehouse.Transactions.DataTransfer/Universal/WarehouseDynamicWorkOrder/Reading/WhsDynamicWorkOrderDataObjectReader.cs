using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using BusinessCodeLists = Enterprise.Warehouse.Transactions.CodeLists;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsDynamicWorkOrderDataObjectReader : WhsComponentOrderDataObjectReader<WhsDynamicWorkOrder, WhsDynamicWorkOrderLine>
	{
		public WhsDynamicWorkOrderDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected override void PopulateBusinessObjectCore(WhsDynamicWorkOrder dynamicWorkOrder)
		{
			base.PopulateBusinessObjectCore(dynamicWorkOrder);

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				ValidateRestrictedDynamicWorkOrderFields(orderDataObject);
			}
		}

		void ValidateRestrictedDynamicWorkOrderFields(Order orderDataObject)
		{
			var errorMessage = new ZStringBuilder();

			var isInwardsProcessingJob = orderDataObject.IsInwardsProcessingJob ?? true;
			if (!isInwardsProcessingJob)
			{
				errorMessage.Append(Res.GetString("019fc74d-c923-4e8a-851c-a64f77f7f8f4", "Dynamic Work Orders must be an Inward Processing Jobs."));
			}

			var autoFinaliseInventory = orderDataObject.AutoFinaliseBOMIntoInventory ?? true;
			if (!autoFinaliseInventory)
			{
				errorMessage.Append(Res.GetString("a9f15083-d099-49f3-b191-7a3e97e87ff7", "Dynamic Work Orders must always auto finalize bill of materials into inventory."));
			}

			var dynamicWorkOrderSubType = orderDataObject.Type?.Code ?? WorkOrderType.Codes.Assemble;
			if (!dynamicWorkOrderSubType.EqualsIgnoringCase(DynamicWorkOrderType.Codes.Assemble)
				&& !dynamicWorkOrderSubType.EqualsIgnoringCase(DynamicWorkOrderType.Codes.Disassemble))
			{
				errorMessage.Append(Res.GetString("c00f18bc-65d8-41f6-bb8c-063bc8b3f031", "Cannot assign an invalid docket sub type for Dynamic Work Orders."));
			}

			this.ThrowImportFailureExceptionIfNotEmpty(errorMessage);
		}

		protected override void CalculateTotalUnitsIfNeeded(WhsDynamicWorkOrder dynamicWorkOrder)
		{
			// We cannot calculate Disassembly Total until after allocation
			if (dataObject.Order?.TotalUnits == null && dynamicWorkOrder.IsAssembly)
			{
				SetValueIfNotReadOnly(dynamicWorkOrder, WhsDocketSchema.WD_TotalUnits, dynamicWorkOrder.Lines.Sum(line => line.WE_TransactionQuantity));
			}
		}

		protected override IEnumerable<WhsDocketLine> GetLinesForTotalWeightAndVolumeCalculation(WhsDynamicWorkOrder dynamicWorkOrder)
		{
			return dynamicWorkOrder.IsAssembly
				? dynamicWorkOrder.MainProductLine_UnsafeAfterFinalization?.ChildComponentLinesCollection ?? Enumerable.Empty<WhsDocketLine>()
				: dynamicWorkOrder.Lines;
		}

		protected override DataObjectReader<OrderLine, WhsDynamicWorkOrderLine> GetNewLineReader(WhsDynamicWorkOrder docket, OrderLine orderLineDataObject, IEnumerable<WhsDynamicWorkOrderLine> matchedLines)
			=> new WhsDynamicWorkOrderLineDataObjectReader(orderLineDataObject, logger, factory, docket, matchedLines);

		protected override WhsImportStrategy GetNewImportStrategy() => new WorkDynamicWorkOrderImportStrategy(this);

		class WorkDynamicWorkOrderImportStrategy : ComponentOrderImportStrategy
		{
			public WorkDynamicWorkOrderImportStrategy(WhsDynamicWorkOrderDataObjectReader reader)
				: base(reader)
			{
			}

			protected override void AfterPopulateCore(WhsDynamicWorkOrder docket)
			{
				base.AfterPopulateCore(docket);
				ValidateDynamicWorkOrderLines(docket);
				SetMatchingLinesOnComponentLines(docket);
				FinaliseComponentOrder(docket);
			}

			void ValidateDynamicWorkOrderLines(WhsDynamicWorkOrder docket)
			{
				var lines = docket.AllLines;
				if (lines.Count > 0)
				{
					var errorMessage = new ZStringBuilder();
					var validationCache = new WhsDynamicWorkOrderValidationCache(lines);
					var dynamicWorkOrderLines = lines.Cast<WhsDynamicWorkOrderLine>();

					ValidateLineQuantity(errorMessage, dynamicWorkOrderLines);

					ValidateParentLines(
						errorMessage,
						dynamicWorkOrderLines.Where(line => line.WE_WE_ParentDocketLine.IsEmpty),
						validationCache);

					ValidateComponentLines(
						errorMessage,
						dynamicWorkOrderLines.Where(line => line.WE_WE_ParentDocketLine.IsValid),
						validationCache);

					ValidateSecondaryLineComponentProducts(errorMessage, dynamicWorkOrderLines);

					Reader.ThrowImportFailureExceptionIfNotEmpty(errorMessage);
				}
			}

			static void ValidateLineQuantity(ZStringBuilder errorMessage, IEnumerable<WhsDynamicWorkOrderLine> lines)
			{
				if (lines.Any(line => line.WE_TransactionQuantity <= 0m))
				{
					errorMessage.Append(WhsDynamicWorkOrderLineValidation.DynamicWorkOrderLineQuantityMustBeGreaterZero);
				}
				else if (lines.Any(line => line.WE_TransactionQuantity % 1 != 0 && line.IsMainInwardProcessedItem))
				{
					errorMessage.Append(WhsDynamicWorkOrderLineValidation.MainProductLineQuantityMustBeAnInteger);
				}
			}

			static void ValidateParentLines(ZStringBuilder errorMessage, IEnumerable<WhsDynamicWorkOrderLine> parentLines, WhsDynamicWorkOrderValidationCache validationCache)
			{
				if (validationCache.DoesDynamicWorkOrderHaveNoMainProcessedItems())
				{
					errorMessage.Append(WhsBondedWarehouseAttributeValidationForDynamicWorkOrders.AtLeastOneMainInwardsProcessedItem);
				}

				if (validationCache.DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem())
				{
					errorMessage.Append(WhsBondedWarehouseAttributeValidationForDynamicWorkOrders.OnlyOneMainInwardsProcessedItem);
				}

				parentLines.ForEach(line =>
				{
					if (line
						.ChildComponentLines
						.GroupBy(componentLine => componentLine.WE_OP)
						.Any(groupedComponentLines => groupedComponentLines.Count() > 1))
					{
						errorMessage.Append(WhsDynamicWorkOrderLineValidation.ComponentLineMustHaveUniqueProductErrorMessage);
					}

					if (!(line.IsMainInwardProcessedItem ^ line.IsSecondaryInwardProcessedItem))
					{
						errorMessage.Append(WhsBondedWarehouseAttributeValidationForDynamicWorkOrders.AtLeastOneInwardProcessedItemTypeIsSet);
					}

					if ((line.IsMainInwardProcessedItem || line.IsSecondaryInwardProcessedItem) && line.IsBOMProduct)
					{
						errorMessage.Append(Res.GetString("5bed8360-ecd3-4018-91b9-11c2025234f1", "Main or Secondary Product cannot be a Bill of Materials."));
					}
				});
			}

			static void ValidateComponentLines(ZStringBuilder errorMessage, IEnumerable<WhsDynamicWorkOrderLine> componentLines, WhsDynamicWorkOrderValidationCache validationCache)
			{
				componentLines.ForEach(line =>
				{
					if (validationCache.IsSecondaryComponentSumMoreThanMainComponent(line.WE_OP))
					{
						errorMessage.Append(WhsDynamicWorkOrderLineValidation.DynamicWorkOrderComponentsSumIncorrect);
					}
				});
			}

			static void ValidateSecondaryLineComponentProducts(ZStringBuilder errorMessage, IEnumerable<WhsDynamicWorkOrderLine> lines)
			{
				var mainComponentProducts = lines
					.Where(line => line.IsMainInwardProcessedItem)
					.SelectMany(line => line.ChildComponentLines)
					.Select(line => line.WE_OP)
					.ToHashSet();

				var secondaryComponentProducts = lines
					.Where(line => line.IsSecondaryInwardProcessedItem)
					.SelectMany(line => line.ChildComponentLines)
					.Select(line => line.WE_OP)
					.ToHashSet();

				if (secondaryComponentProducts.Any(product => !mainComponentProducts.Contains(product)))
				{
					errorMessage.AppendLine(Res.GetString("23ba795c-73fe-4bc2-bcfd-84aee41fe59e", "Component Lines for Secondary Products must have a Product on the Main Component Line Collection."));
				}
			}

			static void SetMatchingLinesOnComponentLines(WhsDynamicWorkOrder docket)
			{
				try
				{
					docket.SetMatchingLinesIfRequired();
				}
				catch (ArgumentException ex)
				{
					throw new DataObjectReadFailureException(ex.Message);
				}
			}

			protected override bool ShouldRejectShortfallCore(WhsDynamicWorkOrder docket) => docket.IsAssembly;

			protected override void OnSuccessfulAllocationCore(WhsDynamicWorkOrder docket)
			{
				base.OnSuccessfulAllocationCore(docket);

				if (!docket.IsAssembly && docket.WD_TotalUnits == 0m)
				{
					docket.WD_TotalUnits = docket.CalculateTotalUnitsForDisassembly();
				}
			}

			protected override void OnAutoPickAttemptedCore(WhsDynamicWorkOrder docket, WhsPick.AutoPickEventArgs args)
			{
				base.OnAutoPickAttemptedCore(docket, args);

				if (!docket.IsAssembly && !args.StockWasAllocated)
				{
					throw new DataObjectReadFailureException(WhsDynamicWorkOrder.DisassemblyDynamicWorkOrderCannotBeFinalisedWithoutSomeAllocation);
				}
			}
		}

		#region Matching Job

		public override DataContextType DataContextType => DataContextType.WarehouseDynamicWorkOrder;

		protected override string DocketTypeCode => BusinessCodeLists.DocketType.Codes.DynamicWorkOrder;

		protected override string DocketType => DynamicWorkOrderDocketType;

		internal static string DynamicWorkOrderDocketType => Res.GetString("724464c1-f68d-4328-92a8-fde85ee607a1", "Dynamic Work Order");

		#endregion
	}
}
