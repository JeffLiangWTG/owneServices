using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PutawayEngineManagerForReceive : PutawayEngineManager<WhsReceive, WhsReceiveLine>, IPutawayEngineManagerForReceive
	{
		public PutawayEngineManagerForReceive(
			IUserHaltableProductionRulesEngineService engine,
			ICrossDockManager crossDockManager,
			IPutawayExistingPalletManager putawayExistingPalletManager,
			IPutawayLocationFactLoader locationFactLoader,
			IPutawayLocationCacheUpdater putawayLocationCacheUpdater)
			: base(engine, locationFactLoader, putawayLocationCacheUpdater)
		{
			CrossDockManager = Argument.NotNull(crossDockManager, nameof(crossDockManager));
			PutawayExistingPalletManager = Argument.NotNull(putawayExistingPalletManager, nameof(putawayExistingPalletManager));
		}

		ICrossDockManager CrossDockManager { get; }
		IPutawayExistingPalletManager PutawayExistingPalletManager { get; }

		protected override IDisposable SuspendForProcessResults(IEnumerable<WhsReceive> parents) => new DisposableList(parents.Select(r => r.DeferUpdateTotalPalletsReceived()));

		protected override IEnumerable<(WhsReceive Parent, IEnumerable<WhsReceiveLine> Lines)> GetGroupedLinesToPutaway(
			BusinessObjectFactory factory,
			IEnumerable<WhsReceive> receives,
			IEnumerable<WhsReceiveLine> lines,
			WhsWarehouse warehouse)
		{
			var groupedLinesToPutaway = new List<(WhsReceive Parent, IEnumerable<WhsReceiveLine> Lines)>();

			AddFetchHints(factory, receives, lines);

			var inventoriesToPutaway = lines.Where(i => ((ILineToPutaway)i).LocationPK.IsEmpty && i.WE_TransactionQuantity > 0m).ToList();
			CrossDockManager.AllocateCrossDockedLines(factory, inventoriesToPutaway);

			inventoriesToPutaway.RemoveAll(i => !((ILineToPutaway)i).LocationPK.IsEmpty);

			var groupedLinesByDocket = inventoriesToPutaway
				.GroupBy(l => l.WE_WD)
				.ToDictionary(g => g.Key, g => g.ToList());

			var palletStockOnOtherReceives = PutawayExistingPalletManager.AllocateExistingPalletLocations(factory, receives, inventoriesToPutaway, warehouse) ?? Enumerable.Empty<WhsInventoryView>();

			var otherDocketsLookup = palletStockOnOtherReceives.ToLookup(i => i.WI_WD);
			AddFetchHintsForOtherInventories(factory, otherDocketsLookup);

			var groupedPalletStockOnOtherReceives = otherDocketsLookup.Select(GetReceiveAndInventoryLines).Where(g => g.Receive != null);

			foreach (var receive in receives)
			{
				if (groupedLinesByDocket.TryGetValue(receive.PK, out var groupedLines))
				{
					var remainingInventoriesToPutaway = groupedLines.Where(i => ((ILineToPutaway)i).LocationPK.IsEmpty).ToArray();

					AddFetchHintsForDocAddresses(receive, remainingInventoriesToPutaway, groupedPalletStockOnOtherReceives);
					AddFetchHintsForCompanyAndBranch(receive, remainingInventoriesToPutaway, groupedPalletStockOnOtherReceives);

					groupedLinesToPutaway.AddRange(new List<(WhsReceive, IEnumerable<WhsReceiveLine>)>() { (receive, remainingInventoriesToPutaway) });
				}
				else
				{
					groupedLinesToPutaway.Add((receive, Enumerable.Empty<WhsReceiveLine>()));
				}
			}

			groupedLinesToPutaway.AddRange(groupedPalletStockOnOtherReceives);

			return groupedLinesToPutaway;
		}

		static (WhsReceive Receive, IEnumerable<WhsReceiveLine> InventoryLines) GetReceiveAndInventoryLines(IEnumerable<WhsInventoryView> inventory)
		{
			WhsReceive receiveResult = null;
			IEnumerable<WhsReceiveLine> inventoryLinesResult = null;

			var firstInventory = inventory.First();
			if (firstInventory.WI_InDocketLineType == DocketType.Codes.Receive)
			{
				receiveResult = firstInventory.Docket as WhsReceive;
				inventoryLinesResult = inventory.Select(i => i.InDocketLine).Cast<WhsReceiveLine>();
			}
			else if (firstInventory.WI_InDocketLineType == DocketType.Codes.Transfer)
			{
				var transferLine = firstInventory.InDocketLine as WhsTransferLine;
				receiveResult = transferLine?.AssociatedReceiveLineOfPutawayTransfer?.Docket;
				inventoryLinesResult = inventory.Select(i => (i.InDocketLine as WhsTransferLine)?.AssociatedReceiveLineOfPutawayTransfer).WhereNotNull();
			}

			return (receiveResult, inventoryLinesResult);
		}

		protected override IInputFact AddInputFactFromLineCore(
			WhsReceive receive,
			WhsReceiveLine receiveLine,
			OrganisationFact client,
			OrgSupplierPart product,
			PutawayProductFact productFact,
			EquipmentFact equipmentFact,
			Dictionary<ZGuid, OrganisationFact> organisations,
			bool isTsaKnownClient,
			bool isTsaPolicyRequired)
		{
			var consignee = receiveLine.Consignee;
			OrganisationFact consigneeFact = null;
			if (consignee != null && !organisations.TryGetValue(consignee.PK, out consigneeFact))
			{
				organisations[consignee.PK] = consigneeFact = WarehouseFactsHelper.GetOrganisationFact(consignee);
			}

			if (!organisations.TryGetValue(receive.SupplierPK, out var supplierFact))
			{
				var supplier = receive.Supplier;
				if (supplier != null)
				{
					organisations[supplier.PK] = supplierFact = WarehouseFactsHelper.GetOrganisationFact(supplier);
				}
			}

			return new InventoryFact(
								receive,
								receiveLine,
								client,
								supplierFact,
								consigneeFact,
								product,
								productFact,
								equipment: equipmentFact,
								isTsaKnownClient: isTsaKnownClient,
								isTsaPolicyRequired: isTsaPolicyRequired,
								hasPutawayTransfer: receiveLine.HasPutawayTransfer);
		}

		void AddFetchHints(BusinessObjectFactory factory, IEnumerable<WhsReceive> receives, IEnumerable<WhsReceiveLine> lines)
		{
			receives.ForEach(receive => _ = receive.Inventory);

			var pickLines = lines.Cast<WhsDocketLine>().Select(d => d.Inventory[0]).SelectMany(i => i.AllPickLines).ToArray();
			foreach (var pickLine in pickLines)
			{
				factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, pickLine.WZ_WE_TransactionLine);
			}

			var rowFactory = ((IBusinessObjectFactoryInternals)factory).RowFactory; // Loading WhsTransferLine as BizO loads the Transfer
			foreach (var pickLine in IEnumerableExtensions.DistinctBy(pickLines, pl => pl.WZ_WE_TransactionLine))
			{
				var transferRow = rowFactory.LoadFromPK(WhsDocketLineSchema.Constants.TableName, pickLine.WZ_WE_TransactionLine);
				factory.AddFetchHint(WhsDocketSchema.Constants.TableName, (Guid)transferRow[WhsDocketLineSchema.Constants.WE_WD]);
			}

			foreach (var putawayTransferLine in lines.Cast<WhsReceiveLine>().Select(i => i?.PutawayTransferLine).WhereNotNull())
			{
				factory.AddFetchHint(WhsDocketLineSchema.WE_WE_MatchingLine, putawayTransferLine.PK);
				factory.AddFetchHint(WhsDocketLineSchema.WE_WE_ParentDocketLine, putawayTransferLine.PK);
				factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, putawayTransferLine.PK);
			}
		}

		static void AddFetchHintsForOtherInventories(BusinessObjectFactory factory, ILookup<ZGuid, WhsInventoryView> otherDocketsLookup)
		{
			var transferInventories = new List<WhsInventoryView>();
			foreach (var group in otherDocketsLookup)
			{
				var firstInventory = group.First();
				if (firstInventory.WI_InDocketLineType == DocketType.Codes.Receive)
				{
					factory.AddFetchHint(WhsDocketSchema.PK, group.Key);
				}
				else if (firstInventory.WI_InDocketLineType == DocketType.Codes.Transfer)
				{
					transferInventories.AddRange(group);
				}
			}

			foreach (var transferInventory in transferInventories)
			{
				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, transferInventory.PK);
			}

			var transferDocketLines = transferInventories.Select(t => t.InDocketLine as WhsTransferLine).WhereNotNull();
			foreach (var transferLine in transferDocketLines)
			{
				var firstPickLine = transferLine.PickLines.FirstOrDefault();
				if (firstPickLine != null)
				{
					factory.AddFetchHint(WhsDocketLineSchema.PK, firstPickLine.WZ_WE_InventoryLine);
					factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, firstPickLine.WZ_WE_InventoryLine);
				}
			}

			foreach (var transferLine in transferDocketLines)
			{
				var receiveLine = transferLine.AssociatedReceiveLineOfPutawayTransfer;
				if (receiveLine != null)
				{
					factory.AddFetchHint(WhsDocketSchema.PK, receiveLine.WE_WD);
				}
			}
		}

		static void AddFetchHintsForDocAddresses(WhsReceive receive, IEnumerable<WhsReceiveLine> inventoryLinesToPutaway, IEnumerable<(WhsReceive Receive, IEnumerable<WhsReceiveLine> ReceiveLines)> groupedPalletStockOnOtherReceives)
		{
			var factory = receive.Factory;

			foreach (var receiveLine in inventoryLinesToPutaway.Concat(groupedPalletStockOnOtherReceives.SelectMany(g => g.ReceiveLines)))
			{
				factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, receiveLine.PK);
			}

			groupedPalletStockOnOtherReceives.Select(g => g.Receive).Append(receive).ForEach(r => r.Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, r.PK));
		}

		static void AddFetchHintsForCompanyAndBranch(WhsReceive receive, IEnumerable<WhsReceiveLine> lines, IEnumerable<(WhsReceive Receive, IEnumerable<WhsReceiveLine> ReceiveLines)> groupedPalletStockOnOtherReceives)
		{
			var factory = receive.Factory;
			var addresses = new List<JobDocAddress>();
			var receives = groupedPalletStockOnOtherReceives.Select(g => g.Receive).Append(receive);
			addresses.AddRange(receives.Select(r => r.LoadJobDocAddressQuickly(r.SupplierDocAddressRequirement.DefaultDocAddressType)).WhereNotNull().ToArray());

			var receiveLines = lines.Concat(groupedPalletStockOnOtherReceives.SelectMany(g => g.ReceiveLines));
			addresses.AddRange(receiveLines.Select(r => r.LoadJobDocAddressQuickly(r.ConsigneeDocAddressRequirement.DefaultDocAddressType)).WhereNotNull().ToArray());

			FetchHintsHelper.AddFetchHintsForGlbCompanyOrgProxy(factory, addresses.Select(address => address.OrganisationPK));
			FetchHintsHelper.AddFetchHintsForGlbBranchOrgProxy(factory, addresses.Select(address => address.OrganisationPK));
		}
	}
}
