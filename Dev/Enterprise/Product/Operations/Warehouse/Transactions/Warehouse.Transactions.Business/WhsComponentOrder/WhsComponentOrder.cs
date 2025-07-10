using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrder : WhsPickableDocket, INumberFountainConsumer
	{
		protected WhsComponentOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider

		public new static readonly WhsComponentOrderTypeDecider TypeDecider = new WhsComponentOrderTypeDecider();

		#endregion

		#region Business Object Overrides

		[ReadOnly(true)]
		public override ZString WD_TransportReference
		{
			get => base.WD_TransportReference;
			set => base.WD_TransportReference = value;
		}

		[ReadOnly(true)]
		public override ZString WD_RS_NKServiceLevel
		{
			get => base.WD_RS_NKServiceLevel;
			set => base.WD_RS_NKServiceLevel = value;
		}

		[ReadOnly(true)]
		public override ZString WD_PL_NKCarrierServiceLevel
		{
			get => base.WD_PL_NKCarrierServiceLevel;
			set => base.WD_PL_NKCarrierServiceLevel = value;
		}

		protected override bool TransportCompanyReadOnly => true;

		#endregion

		#region CustomizableNumber

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehouseDocketID;

		#endregion

		#region Notes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				var result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.I;
				result.FreightMode |= StmNoteContextFreightMode.W;

				return result;
			}
		}

		#endregion

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel => null;

		#endregion

		#region TransportCoDocAddress

		protected override JobDocAddress TransportCoDocAddressCore
		{
			get
			{
				var result = this.GetTransportCoDocAddress(ref transportCoDocAddress);
				result.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(() => true);
				return result;
			}
		}

		JobDocAddress transportCoDocAddress;

		#endregion

		#region IsAssembly

		public bool IsAssembly => WD_DocketSubType == WorkOrderType.Codes.Assemble;

		#endregion

		#region DocketTypeSupportsAllocationKeyCore

		protected override bool DocketTypeSupportsAllocationKeyCore => !IsAssembly;

		#endregion

		#region Receive

		public WhsReceive Receive => Factory.LoadTop1<WhsReceive>(ReceiveQuery);

		ZQuery ReceiveQuery
		{
			get
			{
				var receiveQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
				receiveQuery.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, PK);
				return receiveQuery;
			}
		}

		#endregion

		#region GetRelatedJobs

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var result = base.GetRelatedJobsCore();

			var receive = Receive;
			if (receive != null)
			{
				result.Add(receive);
			}

			return result;
		}

		#endregion

		#region ShouldUpdateWeightAndVolumeOnRemove

		protected override bool ShouldUpdateWeightAndVolumeOnRemove => !IsAssembly; // When it's assembly, total weight and volume were already substracted through components's deletion

		#endregion

		#region CanDeleteRelatedData

		public override bool CanDeleteRelatedData => base.CanDeleteRelatedData && !IsAttachedToPick;

		#endregion

		#region Finalisation

		protected override void OnFinaliseSucceeded()
		{
			// It is important to figure out the Unique Combination of Components used to Assemble Kits.
			// This is so we know exactly which components were used for a particular Inventory Line.
			// To achieve this, we split up the Kits Lines based on Pick Line Allocations.
			var originalMainLinesThatWereSplit = SplitAssemblyLines();
			base.OnFinaliseSucceeded();
			CreateReceive(originalMainLinesThatWereSplit);
		}

		#region SplitAssemblyLines

		protected virtual HashSet<WhsComponentOrderLine> SplitAssemblyLines()
		{
			var originalLinesThatWereSplit = new HashSet<WhsComponentOrderLine>();

			if (IsAssembly)
			{
				var orderedInventoriesCache = new Lazy<Dictionary<WhsPickableDocketLine, WhsPickOrderedInventory>>(() =>
					Pick.OrderedInventories
						.Cast<WhsPickOrderedInventory>()
						.SelectMany(o => o.Owners.Select(l => new { Line = l, OrderedInv = o }))
						.ToDictionary(o => o.Line, o => o.OrderedInv));

				foreach (var line in GetLinesToAssemble().Where(l => l.WE_TransactionQuantity > 0m).ToArray())
				{
					SplitUpKits(line, GetChildComponentLines(line), orderedInventoriesCache);
				}
			}

			return originalLinesThatWereSplit;

			void SplitUpKits(WhsComponentOrderLine line, IReadOnlyCollection<WhsComponentOrderLine> childComponentLines, Lazy<Dictionary<WhsPickableDocketLine, WhsPickOrderedInventory>> orderedInventoriesCache)
			{
				var allSplits = new List<List<int>>();
				var kitQuantitiesPerComponentLine = new Dictionary<WhsComponentOrderLine, decimal>();

				foreach (var childLine in childComponentLines)
				{
					var quantityUsedInOneAssembly = childLine.WE_TransactionQuantity / line.WE_TransactionQuantity;
					kitQuantitiesPerComponentLine.Add(childLine, quantityUsedInOneAssembly);

					// Figure out the distribution of kits based on the Component's Line Allocated PickLines.
					// We will use this to split up Kit / Component Lines into unique Inventory Combinations.
					var kitSplits = GetKitSplitsCore(childLine, quantityUsedInOneAssembly);
					if (kitSplits.Count == 0)
					{
						allSplits.Clear();
						break;
					}
					else
					{
						kitSplits.Sort();
						allSplits.Add(kitSplits);
					}
				}

				SplitUpKitsIfNecessary();

				List<int> GetKitSplitsCore(WhsPickableDocketLine childLine, decimal quantityUsedInOneAssembly)
				{
					var result = new List<int>();
					var currentRemainder = 0m;

					foreach (var pickLineGroup in childLine.PickLines.GroupBy(pl => pl.InventoryLinePKForAvailableInventory))
					{
						var units = pickLineGroup.Sum(pl => pl.WZ_Units);
						var numberOfKits = (int)Math.Floor(units / quantityUsedInOneAssembly);
						var remainder = units % quantityUsedInOneAssembly;
						if (remainder == 0m)
						{
							result.Add(numberOfKits);
						}
						else
						{
							if (numberOfKits > 0)
							{
								result.Add(numberOfKits);
							}

							currentRemainder += remainder;
						}

						if (currentRemainder >= quantityUsedInOneAssembly)
						{
							currentRemainder -= quantityUsedInOneAssembly;
							result.Add(1);
						}
					}

					return result;
				}

				void SplitUpKitsIfNecessary()
				{
					var (minimumPossibleSplit, splitsRequired) = InitialiseValuesAndCheckIfRequiresSplitting();
					if (splitsRequired)
					{
						originalLinesThatWereSplit.Add(line);

						var allComponentLinesWithPickLines = GetComponentLinesWithPickLines();
						SplitLines(allComponentLinesWithPickLines);
					}

					// Find out if we need to do any splitting.
					// If each component picks one uniform inventory there is no need to split anything.
					(int FirstMinimumPossibleSplit, bool SplitsRequired) InitialiseValuesAndCheckIfRequiresSplitting()
					{
						var firstMinimumPossibleSplit = 0;
						var needsSplits = false;

						if (allSplits.Count > 0)
						{
							firstMinimumPossibleSplit = allSplits[0][0];

							foreach (var kitSplits in allSplits)
							{
								if (kitSplits.Count > 1)
								{
									needsSplits = true;
								}

								var firstValue = kitSplits[0];
								if (firstValue != firstMinimumPossibleSplit)
								{
									firstMinimumPossibleSplit = Math.Min(firstMinimumPossibleSplit, firstValue);
									needsSplits = true;
								}

								kitSplits.Reverse(); // reverse the list for efficient modification later
							}
						}

						return (firstMinimumPossibleSplit, needsSplits);
					}

					List<(WhsComponentOrderLine, Stack<WhsPickLine>)> GetComponentLinesWithPickLines()
					{
						var allComponentLinesWithPickLines = new List<(WhsComponentOrderLine, Stack<WhsPickLine>)>(childComponentLines.Count);

						foreach (var childLine in childComponentLines)
						{
							// Stack is LIFO and we want to put smallest units first
							var pickLineStack = new Stack<WhsPickLine>(childLine.PickLines.OrderByDescending(pl => pl.WZ_Units));
							allComponentLinesWithPickLines.Add((childLine, pickLineStack));
						}

						return allComponentLinesWithPickLines;
					}

					void SplitLines(List<(WhsComponentOrderLine, Stack<WhsPickLine>)> allComponentLinesWithPickLines)
					{
						var currentMasterLineQuantity = line.WE_TransactionQuantity - minimumPossibleSplit;

						// no need to split the line if it has the quantity we want already.
						while (minimumPossibleSplit > 0 && currentMasterLineQuantity > 0m)
						{
							// cloning needs to set WE_WD & WE_TransactionQuantity via row setter to avoid the existing code that autocreates Component Lines
							var newMasterLine = CloneLine(line, minimumPossibleSplit);
							SetQuantityViaRow(line, currentMasterLineQuantity);

							var areAllPickLinesOnAComponentLineUsed = false;

							// move picklines across to the new Component Lines
							foreach (var (childLine, pickLineStack) in allComponentLinesWithPickLines)
							{
								var quantityUsedInOneAssembly = kitQuantitiesPerComponentLine[childLine];
								var componentQuantityToUse = minimumPossibleSplit * quantityUsedInOneAssembly;

								SplitComponentLine(childLine, newMasterLine, componentQuantityToUse, pickLineStack);

								if (pickLineStack.Count == 0)
								{
									areAllPickLinesOnAComponentLineUsed = true;
								}
							}

							// if a Component Line has no PickLines left, no need to split further
							if (areAllPickLinesOnAComponentLineUsed)
							{
								break;
							}

							var previousMinimum = minimumPossibleSplit;
							minimumPossibleSplit = CalculateNewMinimumPossibleSplit(previousMinimum);

							currentMasterLineQuantity -= minimumPossibleSplit;
						}

						// clear Pack Qty Cache on all original kit lines & component lines so that it recalculates the correct value on next access
						line.ClearPackQuantity();

						foreach (var childLine in childComponentLines)
						{
							childLine.ClearPackQuantity();
						}
					}

					void SplitComponentLine(WhsComponentOrderLine childLine, WhsComponentOrderLine newMasterLine, decimal componentQuantityToUse, Stack<WhsPickLine> pickLineStack)
					{
						var quantityUsed = 0m;
						var pickLinesToUse = new List<WhsPickLine>();

						while (pickLineStack.Count > 0)
						{
							var pickLine = pickLineStack.Pop();
							var newQuantity = pickLine.WZ_Units + quantityUsed;
							if (newQuantity <= componentQuantityToUse)
							{
								quantityUsed = newQuantity;
								pickLinesToUse.Add(pickLine);
							}
							else
							{
								var splitQuantity = newQuantity - componentQuantityToUse;
								var newPickLine = pickLine.Split(splitQuantity);
								quantityUsed = componentQuantityToUse;
								pickLinesToUse.Add(pickLine);

								pickLineStack.Push(newPickLine); // place unused quantity back on the Stack.
							}

							if (quantityUsed == componentQuantityToUse)
							{
								CreateNewComponentLine();
								break;
							}

							void CreateNewComponentLine()
							{
								var newComponentLine = CloneLine(childLine, componentQuantityToUse);
								newComponentLine.WE_WE_ParentDocketLine = newMasterLine.PK;

								// Add new Component Line as Owner to the existing Ordered Inventory
								// Tested in TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory
								var orderedInventory = orderedInventoriesCache.Value[childLine];
								if (!orderedInventory.Owners.Contains(newComponentLine))
								{
									orderedInventory.Owners.Add(newComponentLine);
								}

								foreach (var pickLineToUse in pickLinesToUse)
								{
									pickLineToUse.WZ_WE_TransactionLine = newComponentLine.PK;
								}
							}
						}

						SetQuantityViaRow(childLine, childLine.WE_TransactionQuantity - componentQuantityToUse);
					}
				}

				// This algorithm takes a greedy approach to figure out the correct splits across all Components.
				// It attempts to minimise the number of splits required, technically a bruteforce/backtracking algorithm
				// could find a more optimal split, however, the performance would degrade significantly and would not be
				// worth the complexity required as the main use case for Kit Assembly should have little variance in Components allocated.
				int CalculateNewMinimumPossibleSplit(int previousMinimum)
				{
					var minimumPossibleSplit = 0;

					foreach (var kitSplits in allSplits)
					{
						// since the list is in descending order, the last element is always the smallest and we can 'pop' it off efficiently
						var lastIndex = kitSplits.Count - 1;
						if (lastIndex >= 0)
						{
							var remainder = kitSplits[lastIndex] -= previousMinimum;
							if (remainder > 0)
							{
								minimumPossibleSplit = minimumPossibleSplit > 0 ? Math.Min(minimumPossibleSplit, remainder) : remainder;
							}
							else
							{
								kitSplits.RemoveAt(lastIndex);
								if (lastIndex > 0)
								{
									var nextValue = kitSplits[lastIndex - 1];
									minimumPossibleSplit = minimumPossibleSplit > 0 ? Math.Min(minimumPossibleSplit, nextValue) : nextValue;
								}
							}
						}
						else
						{
							return 0;
						}
					}

					return minimumPossibleSplit;
				}
			}
		}

		protected WhsComponentOrderLine CloneLine(WhsComponentOrderLine lineToClone, decimal quantity)
		{
			var clonedLine = (WhsComponentOrderLine)lineToClone.Clone(new BusinessObjectCloneArgs(new[] { WhsDocketLineSchema.Constants.WE_TransactionQuantity }, TypeToCloneLinesAs));
			SetQuantityViaRow(clonedLine, quantity);
			((IBusinessObjectInternals)clonedLine).Row[WhsDocketLineSchema.Constants.WE_WD] = PK.ToGuid();
			clonedLine.WE_DocketLineStatus = lineToClone.WE_DocketLineStatus;
			clonedLine.WE_FinalisedDate = lineToClone.WE_FinalisedDate;
			return clonedLine;
		}

		protected void SetQuantityViaRow(IBusinessObjectInternals lineToSet, decimal quantity)
		{
			lineToSet.Row[WhsDocketLineSchema.Constants.WE_TransactionQuantity] = quantity;
		}

		protected abstract IReadOnlyCollection<WhsComponentOrderLine> GetChildComponentLines(WhsComponentOrderLine line);

		protected abstract Type TypeToCloneLinesAs { get; }

		#endregion

		#region Auto-Create Receive

		void CreateReceive(HashSet<WhsComponentOrderLine> originalMainLinesThatWereSplit)
		{
			var now = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			var messageBuilder = new ZStringBuilder();

			// create and setup the receive
			ReceiveCreatedOnFinalise = CreateReceiveHeader(now);

			// add the necessary inventory
			messageBuilder.Append(Res.GetString("5a01ba1e-7f87-447c-8b74-22c2344a13b5", "Receive {0}-{1} has been created to place the",
				ReceiveCreatedOnFinalise.WD_ExternalReference, ReceiveCreatedOnFinalise.WD_ExternalReferenceSplit));

			if (IsAssembly)
			{
				AssembleKits(Pick, new WorkOrderStagingLocationHelper(Warehouse), now, originalMainLinesThatWereSplit);
				messageBuilder.Append(" " + Res.GetString("1d61e682-1e79-416b-88ef-1cc1b7a0c290", "assembled Work Order Product into inventory."));

				AfterAssemblyCore();
			}
			else
			{
				HandleDisassemblyCore(ReceiveCreatedOnFinalise, now, messageBuilder);
			}

			// finalise the receive if necessary
			if (WD_AutoFinaliseBOMIntoInventory)
			{
				ReceiveCreatedOnFinalise.FinaliseDocketWithoutUserConfirmation();
			}

			// notify of the result
			messageBuilder.Append("\r\n\r\n" + Res.GetString("dd4812f7-4b5e-48a1-8535-1b33ddd2d3d7", "After Saving the Work Order the Receive Job will be available on the Related Jobs tab."));

			if (WD_AutoFinaliseBOMIntoInventory && !ReceiveCreatedOnFinalise.IsFinalised)
			{
				messageBuilder.Append("\r\n\r\n" + Res.GetString("9f88f178-e1ae-48b7-914a-302d49685bc4", "The Receive could not be automatically finalized. You can manually Finalize the Receive."));
			}

			NotificationSubscriber.Notify(new InfoNotification(messageBuilder.ToString()));
		}

		protected virtual void AfterAssemblyCore()
		{
		}

		protected abstract void HandleDisassemblyCore(WhsReceive receive, ZDateTimeOffset now, ZStringBuilder messageBuilder);

		WhsReceive CreateReceiveHeader(ZDateTimeOffset now)
		{
			var receive = Factory.New<WhsReceive>();
			RegisterEditableChildObject(receive);

			receive.WD_WW_Whs = WD_WW_Whs;
			receive.WD_OH_Client = WD_OH_Client;
			receive.WD_WD_ParentDocket = PK;

			receive.WD_ExternalReferenceSplit = WD_ExternalReferenceSplit;

			receive.WD_ExternalReference = WD_ExternalReference;
			if (receive.CheckExternalReferenceForDuplicates())
			{
				receive.WD_ExternalReference = GenerateNewUniqueExternalReference(WD_ExternalReference);
			}

			receive.WD_TotalUnits = WD_TotalUnits;
			receive.WD_PackagesSent = WD_PackagesSent;
			receive.WD_TotalPallets = WD_TotalPallets;

			receive.WD_ArrivalDate = now;
			receive.WD_BookingDate = now;

			if (WD_IsInwardsProcessingJob)
			{
				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				receive.WD_IsInwardsProcessingJob = true;
			}

			return receive;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String is used internally in a query.")]
		ZString GenerateNewUniqueExternalReference(string externalReference)
		{
			const string resultColumnName = "Reference";
			var uniqueExternalReference = ZString.Empty;

			var rawSql = @"
				WITH ExternalRefList (Reference) AS
				(
				    SELECT TOP 998 @WorkOrderExternalReference + '-' + CAST((ROW_NUMBER() OVER (ORDER BY object_id)) + 1 as VARCHAR) as Reference FROM sys.objects
				)
				SELECT TOP 1 Reference FROM ExternalRefList WHERE Reference NOT IN (SELECT WD_ExternalReference from dbo.WhsDocket)";

			var sqlResults = new DynamicBusinessObjectCollection(Factory);
			sqlResults.Load(rawSql, new[]
			{
				ZSqlParameter.New("@WorkOrderExternalReference", externalReference, WhsDocketSchema.WD_ExternalReference),
			});

			var generatedExternalReference = sqlResults?.FirstOrDefault()?[resultColumnName]?.ToString();
			if (generatedExternalReference != null && generatedExternalReference.Length <= WhsDocketSchema.WD_ExternalReference.MaxLength)
			{
				uniqueExternalReference = generatedExternalReference;
			}
			// else, get ExternalReference from WD_DocketID in OnFactorySaving

			return uniqueExternalReference;
		}

		void AssembleKits(WhsPick pick, WorkOrderStagingLocationHelper stagingLocationHelper, ZDateTimeOffset now, HashSet<WhsComponentOrderLine> splitKitLines)
		{
			AddFetchHintsForAssembleKits();

			foreach (var line in GetLinesToAssemble())
			{
				var quantityAssembled = GetQuantityAssembled(line);
				var componentsAssembled = AddNewInventoryFromComponentLines(pick, stagingLocationHelper, ReceiveCreatedOnFinalise, line, quantityAssembled, now);

				// The new Kit lines Split off from the Original Kit Line will have all valid picklines moved to them.
				// If there is a shortfall, the Original Kit Line will be left with partial or no kits allocated. In this case,
				// it is unnecessary to create an extra Inventory Line on the receive with 0 Stock on Hand. Otherwise if no full kits
				// were allocated, then the Kit Line does not get split, and we create just one 0 Stock on Hand Inventory Line.
				if (quantityAssembled > 0m || !splitKitLines.Contains(line))
				{
					var receiveLine = WhsWorkOrderHelper.AddNewInventoryFromWorkOrderLine(stagingLocationHelper, pick, ReceiveCreatedOnFinalise, line, null, quantityAssembled, now);
					SetInwardProcessingData(receiveLine, isMainItem: true);

					var childComponentLines = GetChildComponentLines(line);
					HandleSecondaryProducts(ReceiveCreatedOnFinalise, line, childComponentLines, componentsAssembled, receiveLine.WE_WL, quantityAssembled, now);

					if (quantityAssembled > 0m)
					{
						foreach (var componentLine in childComponentLines)
						{
							var componentsAssembledQuantity = componentsAssembled[componentLine];
							if (componentsAssembledQuantity > 0m)
							{
								LinkInventoryToComponent(componentLine.PK, receiveLine, componentsAssembledQuantity);
							}
						}
					}
				}
			}
		}

		protected abstract IReadOnlyCollection<WhsComponentOrderLine> GetLinesToAssemble();

		protected virtual void AddFetchHintsForAssembleKits()
		{
		}

		protected abstract void HandleSecondaryProducts(
			WhsReceive receiveCreatedOnFinalise,
			WhsComponentOrderLine line,
			IReadOnlyCollection<WhsComponentOrderLine> childComponentLines,
			Dictionary<WhsComponentOrderLine, decimal> componentsAssembled,
			ZGuid location,
			decimal quantityAssembled,
			ZDateTimeOffset now);

		protected abstract decimal GetQuantityAssembled(WhsComponentOrderLine line);

		protected void SetInwardProcessingData(WhsDocketLine inventoryLine, bool isMainItem)
		{
			if (WD_IsInwardsProcessingJob)
			{
				var customsData = inventoryLine.CustomsData;

				if (isMainItem)
				{
					customsData.WB_IsMainInwardsProcessedItem = true;
				}
				else
				{
					customsData.WB_IsSecondaryInwardsProcessedItem = true;
				}
			}
		}

		protected void LinkInventoryToComponent(ZGuid componentLinePK, WhsReceiveLine receiveLine, decimal componentQty)
		{
			var pivot = Factory.New<WhsBOMInventoryPivot>();
			pivot.WIP_WE_ComponentLine = componentLinePK;
			pivot.WIP_WE_InventoryLine = receiveLine.PK;
			pivot.WIP_ComponentQuantity = componentQty;
		}

		#region AddNewInventoryFromWorkOrderLine

		Dictionary<WhsComponentOrderLine, decimal> AddNewInventoryFromComponentLines(WhsPick pick, WorkOrderStagingLocationHelper stagingLocationHelper, WhsReceive receive, WhsComponentOrderLine line, ZDecimal quantityAssembled, ZDateTimeOffset arrivalDate)
		{
			var componentsAssembled = new Dictionary<WhsComponentOrderLine, decimal>();

			foreach (WhsComponentOrderLine childLine in line.ChildComponentLines)
			{
				var quantityUsedInAssembly = 0m;
				if (line.WE_TransactionQuantity > 0)
				{
					// we do not use FindByComponentPKandPackType may user change after creating childLine
					var quantityUsedInOneAssembly = childLine.WE_TransactionQuantity / line.WE_TransactionQuantity; // OE_ComponentQty
					quantityUsedInAssembly = quantityUsedInOneAssembly * quantityAssembled;
				}

				componentsAssembled.Add(childLine, quantityUsedInAssembly);

				var quantityUnused = childLine.PickLineQuantity - quantityUsedInAssembly;

				// receive component if it wasn't used in assembly
				if (quantityUnused > 0)
				{
					HandleUnusedAssemblyQuantity(pick, stagingLocationHelper, receive, childLine, quantityUnused, arrivalDate);
				}
			}

			return componentsAssembled;
		}

		protected abstract void HandleUnusedAssemblyQuantity(
			WhsPick pick,
			WorkOrderStagingLocationHelper stagingLocationHelper,
			WhsReceive receive,
			WhsComponentOrderLine childLine,
			decimal quantityUnused,
			ZDateTimeOffset arrivalDate);

		#endregion

		WhsReceive ReceiveCreatedOnFinalise;

		#endregion

		#endregion

		#region Lines

		public WhsComponentOrderLineCollection DisassemblyLinesForPutaway => DisassemblyLinesForPutawayCore;
		protected abstract WhsComponentOrderLineCollection DisassemblyLinesForPutawayCore { get; }

		public WhsComponentOrderLineCollection DisassemblyLinesForPick => DisassemblyLinesForPickCore;
		protected abstract WhsComponentOrderLineCollection DisassemblyLinesForPickCore { get; }

		#endregion

		#region Validation

		public new WhsComponentOrderValidation Validation => (WhsComponentOrderValidation)base.Validation;

		#endregion

		#region Saving

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && ReceiveCreatedOnFinalise != null)
			{
				AutoCreatedReceiveSaved?.Invoke(this, new AutoCreatedReceiveSavedEventArgs(ReceiveCreatedOnFinalise));
			}
		}

		public event EventHandler<AutoCreatedReceiveSavedEventArgs> AutoCreatedReceiveSaved;

		public class AutoCreatedReceiveSavedEventArgs : EventArgs
		{
			public AutoCreatedReceiveSavedEventArgs(WhsReceive receive)
			{
				Receive = Argument.NotNull(receive, nameof(receive));
			}

			public WhsReceive Receive { get; }
		}

		#endregion

		#region IsCustomsTransaction

		public override bool IsCustomsTransaction => WD_IsInwardsProcessingJob;

		protected override bool IsCustomsDataVisibleCore => false;

		#endregion
	}
}
