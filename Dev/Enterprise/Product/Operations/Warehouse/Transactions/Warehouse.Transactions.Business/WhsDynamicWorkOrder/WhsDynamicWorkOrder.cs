using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[UniversalDataContext(DataContextType.WarehouseDynamicWorkOrder)]
	public class WhsDynamicWorkOrder : WhsComponentOrder
	{
		public WhsDynamicWorkOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
			=> new WhsDynamicWorkOrderFetchStrategy(this);

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WD_DocketType = DocketType.Codes.DynamicWorkOrder;
			WD_DocketSubType = WorkOrderType.Codes.Assemble; // will likely end up sharing both WorkOrder Sub Types
			WD_AutoFinaliseBOMIntoInventory = true;
			WD_IsInwardsProcessingJob = true;
		}

		#endregion

		#region Properties

		#region WD_IsInwardsProcessingJob

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZBool WD_IsInwardsProcessingJob
		{
			get => base.WD_IsInwardsProcessingJob;
			set => base.WD_IsInwardsProcessingJob = value;
		}

		#endregion

		#endregion

		#region DocAddresses

		protected override IJobDocAddressReadOnlyStrategy GetConsigneeDocAddressReadOnlyStrategy() => new JobDocAddressReadOnlyStrategy(() => true);

		#endregion

		#region RelatedJobs

		protected override bool CanHaveChildWorkOrders => false;

		#endregion

		#region Lines

		[ChildEditable]
		public new WhsDynamicWorkOrderLineCollection Lines
			=> (WhsDynamicWorkOrderLineCollection)base.Lines;

		/// <summary>
		/// After finalization, there can be multiple main lines due to splitting. It is not correct to use this property after finalization.
		/// </summary>
		public WhsDynamicWorkOrderLine MainProductLine_UnsafeAfterFinalization
			=> Lines.Cast<WhsDynamicWorkOrderLine>().FirstOrDefault(l => l.IsMainInwardProcessedItem);

		#endregion

		#region Description

		protected override ZString DescriptionCore => Res.GetString("defa3656-1bc8-4bc0-bec3-dbb9abdcdc3f", "Dynamic Work Order");

		#endregion

		#region JobControllerId

		protected override ControllerID JobControllerId => ControllerIDs.WhsDynamicWorkOrder;

		#endregion

		#region EventReferenceParameterType

		protected override string GetDocketEventReferenceParameterType => Constants.EventReferenceParameterTypes.DynamicWorkOrder;

		#endregion

		#region Lines

		protected override WhsDocketLine[] GetLinesToCalculateShortfall() => Array.Empty<WhsDocketLine>();

		protected override WhsPickableDocketLineCollection GetLinesToPickCore()
		{
			return IsAssembly
				? GetLinesToPickForAssembly()
				: Lines;
		}

		WhsPickableDocketLineCollection GetLinesToPickForAssembly()
		{
			return !IsFinalised
				? MainProductLine_UnsafeAfterFinalization?.ChildComponentLinesCollection ?? new WhsDynamicWorkOrderLineCollection(this, ZQuery.NoResultQuery)
				: GetPostFinalizationLinesToPickCollection();

			WhsDynamicWorkOrderLineCollection GetPostFinalizationLinesToPickCollection()
			{
				var masterLinePKs = Lines.Cast<WhsDynamicWorkOrderLine>().Where(l => l.IsMainInwardProcessedItem).Select(l => l.PK);
				var childComponentLinesQuery = new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, masterLinePKs);
				return new WhsDynamicWorkOrderLineCollection(this, childComponentLinesQuery);
			}
		}

		protected override WhsPickableDocketLineCollection GetNewAllLines() => new WhsDynamicWorkOrderLineCollection(this);

		protected override WhsPickableDocketLineCollection GetNewPickableDocketLineCollection()
		{
			return new WhsDynamicWorkOrderLineCollection(this, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, null));
		}

		protected override WhsComponentOrderLineCollection DisassemblyLinesForPickCore
			=> new WhsDynamicWorkOrderLineCollection(this);

		protected override WhsComponentOrderLineCollection DisassemblyLinesForPutawayCore
			=> new WhsDynamicWorkOrderLineCollection(this);

		#endregion

		#region CalculateTotalUnitsForDisassembly

		public decimal CalculateTotalUnitsForDisassembly()
		{
			var result = 0m;
			var pickLines = Lines.SelectMany(l => l.PickLines);

			foreach (var pickLine in pickLines)
			{
				result += CalculateComponentQuantityForLinks(pickLine);
			}

			return result;

			decimal CalculateComponentQuantityForLinks(WhsPickLine pickLineForKit)
			{
				var componentQty = 0m;
				var componentLinks = pickLineForKit.InventoryLine.BOMComponentLinks;
				foreach (var link in componentLinks)
				{
					var componentLine = (WhsComponentOrderLine)link.ComponentLine;
					var parentLine = componentLine.ParentLine;

					var quantityInOneAssembly = componentLine.WE_TransactionQuantity / parentLine.WE_TransactionQuantity;
					componentQty += pickLineForKit.WZ_Units * quantityInOneAssembly;
				}
				return componentQty;
			}
		}

		#endregion

		#region Customs Stuff

		protected override ZString DefaultDocketSubTypeForCustomsTransactionCore => WorkOrderType.Codes.Assemble;

		#endregion

		#region Documents

		protected override DocumentSupporter GetNewDocumentSupporter() => new WhsDynamicWorkOrderDocumentSupporter(this);

		#endregion

		#region Lookups

		protected override WhsDocketLookups GetNewLookups() => new WhsDynamicWorkOrderLookups(this);

		#endregion

		#region Finalisation

		protected override void HandleDisassemblyCore(WhsReceive receive, ZDateTimeOffset now, ZStringBuilder messageBuilder)
		{
			new DisassembledInventoryCreator(this, receive, now).CreateDisassembledInventory();
			messageBuilder.Append(" " + Res.GetString("700f7f22-e3f2-4924-9b46-714369be2533", "disassembled components back into inventory."));
		}

		protected override void HandleUnusedAssemblyQuantity(
			WhsPick pick,
			WorkOrderStagingLocationHelper stagingLocationHelper,
			WhsReceive receive,
			WhsComponentOrderLine childLine,
			decimal quantityUnused,
			ZDateTimeOffset arrivalDate)
		{
			throw new InvalidOperationException("Child component line had unused assembly quantity for a Dynamic Work Order.");
		}

		protected override bool RunPreFinaliseValidationCore()
		{
			var result = base.RunPreFinaliseValidationCore();

			if (!IsAssembly && !Lines.SelectMany(l => l.PickLines).Any())
			{
				AddRowError(DisassemblyDynamicWorkOrderCannotBeFinalisedWithoutSomeAllocation);
				result = false;
			}

			return result;
		}

		public static string DisassemblyDynamicWorkOrderCannotBeFinalisedWithoutSomeAllocation => Res.GetString("02c1394e-494f-47c3-bcac-e2b646b974f0", "Dynamic Work Order cannot be finalized until some inventory is allocated.");

		protected override void OnFinaliseSucceeded()
		{
			try
			{
				mainLineToSecondaryLineMappings = new Dictionary<ZGuid, IEnumerable<WhsDynamicWorkOrderLine>>
				{
					{ MainProductLine_UnsafeAfterFinalization.PK, Lines.Cast<WhsDynamicWorkOrderLine>().Where(l => l.IsSecondaryInwardProcessedItem).ToArray() },
				};

				base.OnFinaliseSucceeded();
			}
			finally
			{
				mainLineToSecondaryLineMappings = null;
			}
		}

		Dictionary<ZGuid, IEnumerable<WhsDynamicWorkOrderLine>> mainLineToSecondaryLineMappings;

		protected override IReadOnlyCollection<WhsComponentOrderLine> GetLinesToAssemble()
			=> IsAssembly ? Lines.Cast<WhsDynamicWorkOrderLine>().Where(l => l.IsMainInwardProcessedItem).ToArray() : throw new InvalidOperationException("Attempt to access assembly lines for receive for disassembly Dynamic Work Order.");

		protected override HashSet<WhsComponentOrderLine> SplitAssemblyLines()
		{
			var mainPreSplitLine = MainProductLine_UnsafeAfterFinalization;
			var preSplitQuantities = AllLines.ToDictionary(l => l.PK, l => l.WE_TransactionQuantity);
			var preSplitSecondaryLines = Lines.Cast<WhsDynamicWorkOrderLine>().Where(l => l.IsSecondaryInwardProcessedItem).ToArray();

			// Shared logic splits Main line accurately, but regular work orders will create secondary products on finalisation. We should clone now.
			var originalMainLinesThatWereSplit = base.SplitAssemblyLines();

			SplitSecondaryLinesBasedOnMainLineSplits();

			// clear Pack Qty Cache on all originallines so that it recalculates the correct value on next access
			preSplitSecondaryLines.ForEach(l => l.ClearPackQuantity());
			preSplitSecondaryLines.SelectMany(l => l.ChildComponentLinesCollection).ForEach(l => l.ClearPackQuantity());

			FailFinalizationIfBadSplitOccurred();

			return originalMainLinesThatWereSplit;

			void SplitSecondaryLinesBasedOnMainLineSplits()
			{
				foreach (var newSplitMainLine in Lines.Cast<WhsDynamicWorkOrderLine>().Where(l => l.IsMainInwardProcessedItem && l.PK != mainPreSplitLine.PK).ToArray())
				{
					var componentLineDictionary = newSplitMainLine.ChildComponentLinesCollection.ToDictionary(l => l.WE_OP);
					var splitRatio = newSplitMainLine.WE_TransactionQuantity / preSplitQuantities[mainPreSplitLine.PK];

					var splitSecondaryLines = new List<WhsDynamicWorkOrderLine>(preSplitSecondaryLines.Length);
					foreach (var secondaryLine in preSplitSecondaryLines)
					{
						var splitSecondaryQuantity = Utilities.Round(splitRatio * secondaryLine.WE_TransactionQuantity, secondaryLine.SupplierPart.OP_CountDecimalPlaces);
						var splitSecondaryLine = (WhsDynamicWorkOrderLine)CloneLine(secondaryLine, splitSecondaryQuantity);
						splitSecondaryLine.IsSecondaryInwardProcessedItem = true;

						SetQuantityViaRow(secondaryLine, secondaryLine.WE_TransactionQuantity - splitSecondaryLine.WE_TransactionQuantity);

						splitSecondaryLines.Add(splitSecondaryLine);

						foreach (var secondaryComponentLine in secondaryLine.ChildComponentLinesCollection.Cast<WhsDynamicWorkOrderLine>())
						{
							var splitComponentQuantity = Utilities.Round(splitRatio * secondaryComponentLine.WE_TransactionQuantity, secondaryComponentLine.SupplierPart.OP_CountDecimalPlaces);
							var splitComponentLine = CloneLine(secondaryComponentLine, splitComponentQuantity);
							splitComponentLine.WE_WE_ParentDocketLine = splitSecondaryLine.PK;
							splitComponentLine.WE_WE_MatchingLine = componentLineDictionary[splitComponentLine.WE_OP].PK;

							SetQuantityViaRow(secondaryComponentLine, secondaryComponentLine.WE_TransactionQuantity - splitComponentLine.WE_TransactionQuantity);
						}
					}

					mainLineToSecondaryLineMappings.Add(newSplitMainLine.PK, splitSecondaryLines);
				}
			}

			void FailFinalizationIfBadSplitOccurred()
			{
				var badlySplitProducts = IEnumerableExtensions.DistinctBy(AllLines.Where(l => l.WE_TransactionQuantity <= 0m), l => l.WE_OP).Select(l => l.ProductCode).ToArray();
				if (badlySplitProducts.Length > 0)
				{
					throw new AbortFinalizationException(Res.GetString("a6d53aa5-fc87-45a5-a9aa-e5be082e12e0",
@"Attempted to split lines into a fractional quantity beyond what is allowed with the Product's configured decimal places.

To finalize this Dynamic Work Order, allocate uniform components, create separate Dynamic Work Orders for each unit of the Main Product or alter the configured decimal places on the Product master file.

Affected Product(s): {0}", string.Join(", ", badlySplitProducts)));
				}
			}
		}

		protected override void HandleSecondaryProducts(
			WhsReceive receiveCreatedOnFinalise,
			WhsComponentOrderLine line,
			IReadOnlyCollection<WhsComponentOrderLine> childComponentLines,
			Dictionary<WhsComponentOrderLine, decimal> componentsAssembled,
			ZGuid location,
			decimal quantityAssembled,
			ZDateTimeOffset now)
		{
			var secondaryProductsToCreate = mainLineToSecondaryLineMappings[line.PK];
			var childComponentLinesByPK = childComponentLines.ToDictionary(l => l.PK);

			foreach (var secondaryProductLine in secondaryProductsToCreate)
			{
				var inventoryLine = WhsWorkOrderHelper.CreateNewInventory(receiveCreatedOnFinalise, secondaryProductLine.WE_OP, null, secondaryProductLine.WE_TransactionQuantity, now);
				inventoryLine.Inventory[0].WI_WL = location;

				SetInwardProcessingData(inventoryLine, isMainItem: false);

				if (secondaryProductLine.ChildComponentLinesCollection.Count > 0)
				{
					foreach (var secondaryComponentLine in secondaryProductLine.ChildComponentLinesCollection)
					{
						var matchingLine = childComponentLinesByPK[secondaryComponentLine.WE_WE_MatchingLine];
						componentsAssembled[matchingLine] -= secondaryComponentLine.WE_TransactionQuantity;

						LinkInventoryToComponent(matchingLine.PK, inventoryLine, secondaryComponentLine.WE_TransactionQuantity);
					}
				}
				else
				{
					// Link Secondary Parts with no Component Usages (considered waste products) to the entire Kit.
					LinkInventoryToComponent(line.PK, inventoryLine, quantityAssembled);
				}
			}
		}

		protected override IReadOnlyCollection<WhsComponentOrderLine> GetChildComponentLines(WhsComponentOrderLine line) => ((WhsDynamicWorkOrderLine)line).ChildComponentLinesCollection.Cast<WhsDynamicWorkOrderLine>().ToArray();

		protected override Type TypeToCloneLinesAs => typeof(WhsDynamicWorkOrderLine);

		protected override decimal GetQuantityAssembled(WhsComponentOrderLine line)
			=> IsAssembly ? line.WE_TransactionQuantity : throw new InvalidOperationException("Attempt to access asembly quantity for disassembly Dynamic Work Order.");

		#endregion

		#region Validation

		protected override WhsDocketValidation GetNewValidation() => new WhsDynamicWorkOrderValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			SetMatchingLinesIfRequired();
		}

		public void SetMatchingLinesIfRequired()
		{
			var mainProductLine = new Lazy<WhsDynamicWorkOrderLine>(() => MainProductLine_UnsafeAfterFinalization);

			if (!IsFinalised && !HasErrorsIncludingLines && mainProductLine.Value != null)
			{
				var mainProductComponentsDictionary =
					mainProductLine.Value
					.ChildComponentLinesCollection
					.ToDictionary(l => l.WE_OP);

				var secondaryComponentLines =
					Lines
					.Cast<WhsDynamicWorkOrderLine>()
					.Where(l => l.IsSecondaryInwardProcessedItem)
					.SelectMany(l => l.ChildComponentLinesCollection);

				foreach (var secondaryComponentLine in secondaryComponentLines)
				{
					if (mainProductComponentsDictionary.TryGetValue(secondaryComponentLine.WE_OP, out var mainComponentLine))
					{
						secondaryComponentLine.WE_WE_MatchingLine = mainComponentLine.PK;
						secondaryComponentLine.SetAttributes(mainComponentLine);
					}
					else
					{
						throw new ArgumentException("No matching component line found.");
					}
				}
			}
		}

		#region PreSaveValidationCache

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableAction(
				() => dynamicWorkOrderValidationCache = new WhsDynamicWorkOrderValidationCache(AllLines),
				() => dynamicWorkOrderValidationCache = null);
		}

		internal WhsDynamicWorkOrderValidationCache GetPreSaveValidationCache() => dynamicWorkOrderValidationCache;
		WhsDynamicWorkOrderValidationCache dynamicWorkOrderValidationCache;

		#endregion

		#region AddFetchHintsForPreFinaliseValidation

		protected override void AddFetchHintsForPreFinaliseValidation_Core()
		{
			base.AddFetchHintsForPreFinaliseValidation_Core();
			AllLines.ForEach(l =>
			{
				Factory.AddFetchHint(WhsDocketLineSchema.WE_WE_ParentDocketLine, l.PK);
			});
		}

		#endregion

		#endregion

		#region Workflow

		protected override ZString GetWorkflowType()
			=> WorkflowDescriptors.WhsDynamicWorkOrderWorkflowDescriptorCode;

		protected override WhsDocketProcessTasksCollection GetNewProcessTasksCollection()
			=> new WhsDynamicWorkOrderProcessTasksCollection(this);

		#endregion

		#region IsRecalculateOrderPricingCore

		protected override ZBool IsRecalculateOrderPricingCore => false;

		#endregion

		#region CustomFieldsSupportedCore

		protected override bool CustomFieldsSupportedCore => false;

		#endregion

		#region IsBondedEntryKeyVisibleForCustomsTransactionsCore

		protected override bool IsBondedEntryKeyVisibleForCustomsTransactionsCore => true;

		#endregion

		#region Properties

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBool WD_AutoFinaliseBOMIntoInventory
		{
			get => base.WD_AutoFinaliseBOMIntoInventory;
			set => base.WD_AutoFinaliseBOMIntoInventory = value;
		}

		#endregion

		#region TemplateCopy

		protected override void TemplateCopyLines(WhsPickableDocket copy)
		{
			foreach (WhsDynamicWorkOrderLine line in Lines.ToArray())
			{
				var lineCopy = line.Clone<WhsDynamicWorkOrderLine>();
				copy.Lines.Add(lineCopy);

				foreach (var childComponentLine in line.ChildComponentLinesCollection)
				{
					var childComponentLineCopy = childComponentLine.Clone<WhsDynamicWorkOrderLine>();
					childComponentLineCopy.WE_WE_ParentDocketLine = lineCopy.PK;
					copy.AllLines.Add(childComponentLineCopy);
				}
			}

			((WhsDynamicWorkOrder)copy).SetMatchingLinesIfRequired();
		}

		#endregion

		#region BusinessObjectOverrides

		protected override ZString HumanReadableNameCore
			=> WD_DocketID.IsEmpty
				? Res.GetString("4feb5a9c-012f-4049-bcbf-bc23c4096e54", "Warehouse Dynamic Work Order")
				: Res.GetString("31cc4109-5df4-4ac6-bc7e-20efecd7a121", "Warehouse Dynamic Work Order {0}", WD_DocketID);

		#endregion

		#region DocManagerInfo

		protected override DocManagerInfo GetNewDocManagerInfo()
			=> new WhsDynamicWorkOrderDocManagerInfo(this, Constants.DocManagerCodes.WarehouseDynamicWorkOrder);

		#endregion
	}

	#region Document Support

	public class WhsDynamicWorkOrderDocumentSupporter : DocumentSupporter
	{
		public WhsDynamicWorkOrderDocumentSupporter(WhsDynamicWorkOrder docket)
			: base(docket)
		{
		}

		protected WhsDynamicWorkOrder Docket => (WhsDynamicWorkOrder)BusinessObject;

		public override CargoWise.Definitions.BusinessContext BusinessContext
			=> CargoWise.Definitions.BusinessContext.WhsDynamicWorkOrder;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			=> Env.Security.WhsDynamicWorkOrderCustomiseDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts() =>
			new Constants.DataContext[]
			{
				Constants.DataContext.WhsDynamicWorkOrder,
				Constants.DataContext.WhsPickableDocket,
				Constants.DataContext.GenericFreightJob
			};

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Docket);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper[] result = null;
			switch (dataContext)
			{
				case Constants.DataContext.WhsDynamicWorkOrder:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.WhsDynamicWorkOrder, Docket) };
					break;

				case Constants.DataContext.WhsPickableDocket:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.WhsPickableDocket, Docket) };
					break;

				default:
					break;
			}
			return result;
		}
	}

	#endregion
}
