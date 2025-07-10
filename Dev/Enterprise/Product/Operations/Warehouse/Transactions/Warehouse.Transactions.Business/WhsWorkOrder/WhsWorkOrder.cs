using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[GlowDataDefinition("IWhsWorkOrder")]
	[UniversalDataContext(DataContextType.WarehouseWorkOrder)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.WhsWorkOrder)]
	public class WhsWorkOrder : WhsComponentOrder
	{
		public WhsWorkOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return WD_DocketID.IsEmpty ? Res.GetString("b8b86473-65af-41b9-98e1-4518d239cc8f", "Warehouse Work Order") : Res.GetString("61535bbc-96f9-4955-bd85-586e55d7ada7", "Warehouse Work Order {0}", WD_DocketID); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WD_DocketType = CodeLists.DocketType.Codes.WorkOrder;
			WD_DocketSubType = CodeLists.WorkOrderType.Codes.Assemble;
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsWorkOrderFetchStrategy(this);
		}

		#endregion

		#region Related Entities

		#region RelatedJobs

		protected override bool CanHaveChildWorkOrders => true;

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var result = base.GetRelatedJobsCore();

			var parentDocket = ParentDocket;
			if (parentDocket != null)
			{
				result.Add(parentDocket);
			}

			return result;
		}

		#endregion

		#region Lines

		[ChildEditable]
		public new WhsWorkOrderLineCollection Lines
		{
			get { return (WhsWorkOrderLineCollection)base.Lines; }
		}

		public new WhsWorkOrderLineCollection AllLines => (WhsWorkOrderLineCollection)base.AllLines;

		public WhsWorkOrderLineCollection AssemblyLinesForReceive
		{
			get { return assemblyLinesForReceive ?? (assemblyLinesForReceive = new WhsWorkOrderLineCollection(this, WhsWorkOrderLineFilterStrategy.AssemblyLinesForReceive)); }
		}

		protected override WhsComponentOrderLineCollection DisassemblyLinesForPickCore
		{
			get { return disassemblyLinesForPick ?? (disassemblyLinesForPick = new WhsWorkOrderLineCollection(this, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPick)); }
		}

		protected override WhsComponentOrderLineCollection DisassemblyLinesForPutawayCore
		{
			get { return disassemblyLinesForPutaway ?? (disassemblyLinesForPutaway = new WhsWorkOrderLineCollection(this, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPutaway)); }
		}

		protected override WhsPickableDocketLineCollection GetNewPickableDocketLineCollection()
		{
			return new WhsWorkOrderLineCollection(this);
		}

		protected override WhsPickableDocketLineCollection GetNewAllLines()
		{
			var allLines = new WhsWorkOrderLineCollection(this, WhsWorkOrderLineFilterStrategy.All);
			RegisterEditableChildObject(allLines);

			return allLines;
		}

		WhsWorkOrderLineCollection assemblyLinesForReceive;
		WhsWorkOrderLineCollection disassemblyLinesForPick;
		WhsWorkOrderLineCollection disassemblyLinesForPutaway;

		#endregion

		#region GetLinesToPick

		protected override WhsPickableDocketLineCollection GetLinesToPickCore()
		{
			return (WD_DocketSubType == WorkOrderType.Codes.Assemble)
				? new WhsWorkOrderLineCollection(this, WhsWorkOrderLineFilterStrategy.AssemblyLinesForPick)
				: new WhsWorkOrderLineCollection(this, WhsWorkOrderLineFilterStrategy.DisassemblyLinesForPick);
		}

		#endregion

		#region GetLinesToCalculateShortfall

		protected override WhsDocketLine[] GetLinesToCalculateShortfall()
		{
			return GetLinesToPick().ToArray();
		}

		#endregion

		#region Consignee

		protected override JobDocAddress ConsigneeDocAddressCore
		{
			get
			{
				var result = ParentDocket?.ConsigneeDocAddress ?? base.ConsigneeDocAddressCore;
				result.ReadOnlyStrategy = GetConsigneeDocAddressReadOnlyStrategy();

				return result;
			}
		}

		protected override IJobDocAddressReadOnlyStrategy GetConsigneeDocAddressReadOnlyStrategy()
		{
			Func<bool> readOnly = () => true;
			return new JobDocAddressReadOnlyStrategy(readOnly, readOnly);
		}

		#endregion

		#region TransportCo

		protected override JobDocAddress TransportCoDocAddressCore
		{
			get
			{
				var result = ParentDocket?.TransportCoDocAddress ?? base.TransportCoDocAddressCore;
				result.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(() => true);

				return result;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region Description

		protected override ZString DescriptionCore
		{
			get { return Res.GetString("8cd775f0-a3e3-4dd2-a062-4f98580ee47c", "Work Order"); }
		}

		#endregion

		#region WD_DocketStatus

		public override ZString WD_DocketStatus
		{
			get { return base.WD_DocketStatus; }
			set
			{
				if (value == DocketStatus.Codes.Cancelled && CurrentWorkOrders.Any())
				{
					ZString iDs = "";
					CurrentWorkOrdersIncludingChildren.ForEach(workOrder => iDs += "	" + workOrder.WD_DocketID + "\r\n");

					ZString msg = Res.GetString("309377e1-447e-42a5-9976-dca60994ba14",
						"Cannot cancel this {0} because the following related Work Orders exist:\r\n\r\n{1}\r\nYou must first cancel or delete the related Work Orders.",
						Description, iDs);

					NotificationSubscriber.Notify(new InfoNotification(msg));
				}
				else
				{
					base.WD_DocketStatus = value;
				}
			}
		}

		/// <summary>
		/// These methods will likely do one DB hit per level, so be careful.
		/// </summary>
		List<WhsWorkOrder> CurrentWorkOrdersIncludingChildren
		{
			get { return GetCurrentWorkOrders(this); }
		}

		List<WhsWorkOrder> GetCurrentWorkOrders(WhsWorkOrder parent)
		{
			var result = new List<WhsWorkOrder>(parent.CurrentWorkOrders);
			foreach (var workOrder in parent.CurrentWorkOrders)
			{
				result.AddRange(GetCurrentWorkOrders(workOrder));
			}
			return result;
		}

		#endregion

		#region WD_DocketSubType

		protected override void OnDocketSubTypeChangedCore(ZString previousSubType)
		{
			base.OnDocketSubTypeChangedCore(previousSubType);

			Lines.ResetSupplierPartDocManagerInfo();
			ValidateProducts(); // update disassembly errors
		}

		void ValidateProducts()
		{
			if (!IsValidationSuspended)
			{
				foreach (var line in AllLines)
				{
					line.Validation.ValidateWE_OP();
				}
			}
		}

		#endregion

		#region IsAnyStockAssemblable

		bool IsAnyStockAssemblable
		{
			get
			{
				foreach (WhsWorkOrderLine line in Lines)
				{
					if (line.BOM.IsTopLevelProduct && line.WE_TransactionQuantity > 0 && line.WE_TransactionQuantity > line.WE_ShortfallQuantityCached)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region WD_WW_Whs

		public override ZGuid WD_WW_Whs
		{
			get => base.WD_WW_Whs;
			set
			{
				var previousValue = base.WD_WW_Whs;
				base.WD_WW_Whs = value;

				if (value != previousValue)
				{
					AllLines.Cast<WhsWorkOrderLine>().ForEach(l => l.ClearInVirtualWarehouse());
					if (!WD_AutoFinaliseBOMIntoInventory && (Warehouse?.WW_IsVirtualWarehouse ?? false))
					{
						WD_AutoFinaliseBOMIntoInventory = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateWD_AutoFinaliseBOMIntoInventory();
					}
				}
			}
		}

		#endregion

		#endregion

		#region Properties_ReadOnly

		protected override bool GoodsBillToDocAddress_ReadOnly()
		{
			return true;
		}

		protected override bool TransportBillToDocAddress_ReadOnly()
		{
			return true;
		}

		protected bool WD_AutoFinaliseBOMIntoInventory_ReadOnly
		{
			get { return StandardReadOnly; }
		}

		protected bool WD_OH_Client_ReadOnly
		{
			get { return ParentDocket != null; }
		}

		#endregion

		#region Validation

		public new WhsWorkOrderValidation Validation
		{
			get { return (WhsWorkOrderValidation)base.Validation; }
		}

		protected override WhsDocketValidation GetNewValidation()
		{
			return new WhsWorkOrderValidation(this);
		}

		#endregion

		#region Lookups

		protected override WhsDocketLookups GetNewLookups()
		{
			return new WhsWorkOrderLookups(this);
		}

		public new WhsWorkOrderLookups Lookups
		{
			get { return (WhsWorkOrderLookups)base.Lookups; }
		}

		#endregion

		#region Picking

		protected override void GetPickabilityCore(WhsPick.DocketPickabilityEventArgs pickability, ZStringBuilder errorMessage)
		{
			base.GetPickabilityCore(pickability, errorMessage);

			if (pickability.Message.IsEmpty && !IsAttachedToPickButNotFinalised && Lines.Count > 0 && !IsAnyStockAssemblable && !IsImportingData)
			{
				errorMessage.Append(Res.GetString("7c81dc86-9418-4a2a-bc2f-f2921f3c8aa5", "No Stock can be allocated to this {0} due to shortfalls.", Description));
			}
			else if (pickability.Message.IsEmpty && ProductDefinitionNotMatchesWithDocketLinesDefinition())
			{
				errorMessage.Append(Res.GetString("e01d9292-6bbb-42a5-a8b4-fc33f47a3b50", "One of the component products has been changed. Please cancel the work order {0} and recreate it.", WD_ExternalReference));
			}
		}

		bool ProductDefinitionNotMatchesWithDocketLinesDefinition()
		{
			var lines = Lines.Cast<WhsWorkOrderLine>().Distinct(new WorkOrderLineComparer());
			return lines.Any(l => l.ProductDefinitionDoesNotMatchDocketLineProductDefinition());
		}

		#region WorkOrderLineComparer

		class WorkOrderLineComparer : IEqualityComparer<WhsWorkOrderLine>
		{
			#region IEqualityComparer<WhsWorkOrderLine> Members

			bool IEqualityComparer<WhsWorkOrderLine>.Equals(WhsWorkOrderLine line1, WhsWorkOrderLine line2)
			{
				return line1.SupplierPart.PK == line2.SupplierPart.PK;
			}

			int IEqualityComparer<WhsWorkOrderLine>.GetHashCode(WhsWorkOrderLine line)
			{
				return line.SupplierPart.PK.GetHashCode();
			}

			#endregion
		}

		#endregion

		#region Finalisation

		protected override bool FinaliseConfirmationNotification(ZString finaliseConfirmationMessage)
		{
			var returnValue = true;
			if (Pick.HasUnpickedPickLines)
			{
				var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: new ZGuid("46e7effa-5ef3-497d-916e-2f39c67c7534"),
										caption: Res.GetString("dc49d54d-e84c-472f-8ee8-a21238c8208e", "Continue?"),
										buttons: ZMessageBoxButtons.YesNo,
										icon: ZMessageBoxIcon.Warning,
										context: null,
										resultsNotToSave: new[] { ZDialogResult.No },
										showCheckboxOnly: true);

				var message = Res.GetString("ed686cc1-cbde-494f-a465-dde9d77b3421", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.");
				var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, true);
				NotificationSubscriber.QueryUser(e);
				returnValue = e.Response;
			}
			return returnValue;
		}

		protected override bool RunPreFinaliseValidationCore()
		{
			var success = base.RunPreFinaliseValidationCore();

			if (success)
			{
				var unfinalisedChildWorkOrderIDs = BOM.UnfinalisedChildWorkOrderIDs().ToArray();
				if (unfinalisedChildWorkOrderIDs.Length > 0)
				{
					var msg = Res.GetString("57e9fb82-a6ae-48f6-b4dd-8a081fbea0c0",
						"Cannot finalize this Work Order because the following related Work Orders exist:\r\n\r\n	{0}\r\n\r\nYou must first finalize the related Work Orders.",
						unfinalisedChildWorkOrderIDs.Aggregate((ids, id) => ids + "\r\n	" + id));

					NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.FinaliseZErrorMessageBox, msg));
					success = false;
				}
			}

			return success;
		}

		protected override IReadOnlyCollection<WhsComponentOrderLine> GetLinesToAssemble() => AssemblyLinesForReceive.Cast<WhsComponentOrderLine>().ToArray();

		protected override void AfterAssemblyCore()
		{
			var args = new AssemblyConfirmationQueryUserEventArgs(defaultResponse: true);
			NotificationSubscriber.QueryUser(args);

			if (!args.Response)
			{
				var message = Res.GetString("afbccb04-77ce-49ff-b018-62845e906aae", "Work Order Finalization was canceled.");
				throw new AbortFinalizationException(message);
			}
		}

		protected override void AddFetchHintsForAssembleKits()
		{
			if (WD_IsInwardsProcessingJob)
			{
				IEnumerableExtensions.DistinctBy(AssemblyLinesForReceive, l => l.WE_OP).ForEach(d => Factory.AddFetchHint(OrgSecondaryPartBOMSchema.OSB_OP_MainProduct, d.WE_OP));
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
			if (WD_IsInwardsProcessingJob)
			{
				var componentLineCache = new Dictionary<(ZGuid, ZString), WhsComponentOrderLine>();
				foreach (var componentLine in childComponentLines)
				{
					componentLineCache[(componentLine.WE_OP, componentLine.WE_F3_NKPackType)] = componentLine;
				}

				foreach (var secondaryPart in line.SupplierPart.SecondaryParts)
				{
					var quantity = secondaryPart.OSB_ProductQuantity * line.WE_TransactionQuantity;
					var inventoryLine = WhsWorkOrderHelper.CreateNewInventory(receiveCreatedOnFinalise, secondaryPart.OSB_OP_SecondaryProduct, null, quantity, now);
					inventoryLine.Inventory[0].WI_WL = location;

					SetInwardProcessingData(inventoryLine, isMainItem: false);
					LinkSecondaryProducts(secondaryPart, inventoryLine, componentLineCache);
				}
			}

			void LinkSecondaryProducts(OrgSecondaryPartBOM secondaryPart, WhsReceiveLine inventoryLine, Dictionary<(ZGuid, ZString), WhsComponentOrderLine> componentLineCache)
			{
				if (quantityAssembled > 0m)
				{
					foreach (var componentUsage in secondaryPart.ComponentUsages)
					{
						var component = componentUsage.Component;
						var childLine = componentLineCache[(component.OE_OP_Component, component.OE_F3_NKPackType)];
						var componentQty = componentUsage.OPP_ComponentQuantity * quantityAssembled;
						componentsAssembled[childLine] -= componentQty;

						// Product Master Validation should prevent this occurring
						if (componentsAssembled[childLine] < 0)
						{
							var message = Res.GetString("88d46958-8b8c-44e6-a8bb-4e215709fee8", "Secondary Product Setup is not correct for Component Product: {0}. Total component quantity used exceeds quantity available for Main Product {1}.", component.Component.OP_PartNum, line.ProductCode);
							throw new AbortFinalizationException(message);
						}

						LinkInventoryToComponent(childLine.PK, inventoryLine, componentQty);
					}

					if (secondaryPart.ComponentUsages.Count == 0)
					{
						// Link Secondary Parts with no Component Usages (considered waste products) to the entire Kit.
						LinkInventoryToComponent(line.PK, inventoryLine, quantityAssembled);
					}
				}
			}
		}

		protected override void HandleUnusedAssemblyQuantity(
			WhsPick pick,
			WorkOrderStagingLocationHelper stagingLocationHelper,
			WhsReceive receive,
			WhsComponentOrderLine childLine,
			decimal quantityUnused,
			ZDateTimeOffset arrivalDate)
		{
			foreach (var pickLine in childLine.PickLines.OrderByDescending(pl => pl.WZ_Units))
			{
				var quantityPicked = Math.Min(quantityUnused, pickLine.WZ_Units);

				var newInventory = WhsWorkOrderHelper.AddNewInventoryFromWorkOrderLine(stagingLocationHelper, pick, receive, childLine, null, quantityPicked, arrivalDate);
				WhsWorkOrderHelper.CopyAttributesFromPickedInventory(newInventory, pickLine);

				quantityUnused -= quantityPicked;
				if (quantityUnused <= 0)
				{
					break;
				}
			}
		}

		protected override IReadOnlyCollection<WhsComponentOrderLine> GetChildComponentLines(WhsComponentOrderLine line) => ((WhsWorkOrderLine)line).BOM.ChildComponentLines;

		protected override Type TypeToCloneLinesAs => typeof(WhsWorkOrderLine);

		protected override decimal GetQuantityAssembled(WhsComponentOrderLine line) => ((WhsWorkOrderLine)line).QuantityAssembled;

		protected override void HandleDisassemblyCore(WhsReceive receive, ZDateTimeOffset now, ZStringBuilder messageBuilder)
		{
			new DisassembledInventoryCreator(this, receive, now).CreateDisassembledInventory();
			messageBuilder.Append(" " + Res.GetString("700f7f22-e3f2-4924-9b46-714369be2533", "disassembled components back into inventory."));
		}

		public decimal GetPlannedDisassemblyQuantity() => DisassemblyLinesForPutaway.Cast<WhsWorkOrderLine>().Sum(l => l.WE_TransactionQuantity);

		public decimal GetDisassemblyQuantity()
		{
			if (IsAssembly)
			{
				throw new InvalidOperationException("GetDisassemblyQuantity can only be invoked for disassembly work orders.");
			}

			return DisassembledInventoryQuantityHelper.GetDisassembledQuantity();
		}

		DisassembledInventoryQuantityHelper DisassembledInventoryQuantityHelper => disassembledInventoryQuantityHelper ?? (disassembledInventoryQuantityHelper = new DisassembledInventoryQuantityHelper(this));
		DisassembledInventoryQuantityHelper disassembledInventoryQuantityHelper;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();

			foreach (var childWorkOrder in ChildWorkOrders)
			{
				if (!childWorkOrder.IsFinalisedOrCancelled)
				{
					childWorkOrder.Delete();
				}
			}
		}

		WhsWorkOrder[] ChildWorkOrders => Factory.Load<WhsWorkOrder>(ChildWorkOrdersQuery);

		ZQuery ChildWorkOrdersQuery
		{
			get
			{
				var result = new ZQuery();
				result.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, PK);
				result.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder);
				return result;
			}
		}

		#endregion

		#region Saving

		protected override bool ReloadRelatedJobsOnNextSave
		{
			get => BOM.ReloadRelatedJobsOnNextSave;
			set => BOM.ReloadRelatedJobsOnNextSave = value;
		}

		#endregion

		#region BOM

		public WorkOrderBOMAutoCreateHelper BOM => bom ?? (bom = new WorkOrderBOMAutoCreateHelper(this));
		WorkOrderBOMAutoCreateHelper bom;

		public class WorkOrderBOMAutoCreateHelper : BOMAutoCreateHelper
		{
			public WorkOrderBOMAutoCreateHelper(WhsWorkOrder parent)
				: base(parent)
			{
				isLevelCached = false;
			}

			#region UnfinalisedChildWorkOrderIDs

			public IEnumerable<ZString> UnfinalisedChildWorkOrderIDs()
			{
				var result = new List<ZString>();

				foreach (var workOrder in Parent.CurrentWorkOrders)
				{
					if (!workOrder.IsFinalised)
					{
						yield return workOrder.WD_DocketID;

						foreach (var unfinalisedChildWorkOrderID in workOrder.BOM.UnfinalisedChildWorkOrderIDs())
						{
							yield return unfinalisedChildWorkOrderID;
						}
					}
				}
			}

			#endregion

			#region Expand / Collapse Lines

			public void ExpandAllLines()
			{
				ExpandLines(Parent.Lines.ToArray<WhsWorkOrderLine>());
			}

			public void ExpandLines(params WhsWorkOrderLine[] lines)
			{
				foreach (var line in lines)
				{
					line.BOM.ToggleExpansion(true);
				}
				((IActiveBusinessObjectCollection)Parent.Lines).Refresh();
			}

			public void CollapseAllLines()
			{
				CollapseLines(Parent.Lines.ToArray<WhsWorkOrderLine>());
			}

			public void CollapseLines(params WhsWorkOrderLine[] parentLines)
			{
				foreach (var line in parentLines)
				{
					foreach (var componentLine in line.BOM.ChildComponentLines)
					{
						componentLine.BOM.ToggleExpansion(false);
					}
				}
				((IActiveBusinessObjectCollection)Parent.Lines).Refresh();
			}

			#endregion

			public ZInt Level
			{
				get
				{
					if (!isLevelCached)
					{
						level = 1;
						CalculateNextLevel(Parent);
						isLevelCached = true;
					}
					return level;
				}
			}
			ZInt level;
			bool isLevelCached;

			void CalculateNextLevel(WhsDocket workOrder)
			{
				if (workOrder.ParentDocket != null)
				{
					level++;
					CalculateNextLevel(workOrder.ParentDocket);
				}
			}
		}

		#endregion

		#region TemplateCopy

		protected override void TemplateCopyLines(WhsPickableDocket copy)
		{
			foreach (WhsWorkOrderLine line in AllLines.ToArray())
			{
				if (line.BOM.IsTopLevelProduct)
				{
					var lineCopy = line.Clone<WhsWorkOrderLine>();
					lineCopy.SuspendingUpdatingTotalWeightAndVolume(); // total weight and volume were already set through copy
					copy.Lines.Add(lineCopy);
					lineCopy.WE_WD = copy.PK; // setting WE_WD will also create child component lines
					lineCopy.ResumeUpdatingTotalWeightAndVolume();
				}
			}
		}

		#endregion

		#region IJobInvoicingPlugin

		protected override WhsDocketInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsWorkOrderInvoicingSupporter(this);
		}

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo GetNewDocManagerInfo()
		{
			return new WhsWorkOrderDocManagerInfo(this, Constants.DocManagerCodes.WarehouseWorkOrder);
		}

		#endregion

		#region IDocumentSupportable Members

		protected override DocumentSupporter GetNewDocumentSupporter()
		{
			return documentSupporter ?? (documentSupporter = new WhsWorkOrderDocumentSupporter(this));
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IWorkflowProvider Members

		protected override ZString GetWorkflowType() => WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode;

		protected override WhsDocketProcessTasksCollection GetNewProcessTasksCollection() => new WhsWorkOrderProcessTasksCollection(this);

		#endregion

		#region IRelatedJob Members

		protected override ControllerID JobControllerId => ControllerIDs.WhsWorkOrder;

		#endregion

		#region IWhsLogEventParent Members

		protected override string GetDocketEventReferenceParameterType => Constants.EventReferenceParameterTypes.WorkOrder;

		#endregion
	}

	#region Document Support

	public class WhsWorkOrderDocumentSupporter : DocumentSupporter
	{
		public WhsWorkOrderDocumentSupporter(WhsWorkOrder docket)
			: base(docket)
		{
		}

		protected WhsWorkOrder Docket
		{
			get { return (WhsWorkOrder)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsWorkOrder; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.WhsWorkOrderCustomiseDocuments; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Core.Constants.DataContext.WhsWorkOrder,
				Core.Constants.DataContext.WhsPickableDocket,
				Core.Constants.DataContext.GenericFreightJob
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Docket);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			DocumentWrapper[] result = null;
			switch (dataContext)
			{
				case Core.Constants.DataContext.WhsWorkOrder:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsWorkOrder, Docket) };
					break;

				case Core.Constants.DataContext.WhsPickableDocket:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsPickableDocket, Docket) };
					break;

				default:
					break;
			}
			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(Docket.Client, null);
		}
	}

	#endregion
}
