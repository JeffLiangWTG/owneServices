using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PutawayEngineManagerForVASTransferLine : PutawayEngineManager<WhsVASOrder, VASReturnTransferLine>, IPutawayEngineManagerForVASTransferLine
	{
		public PutawayEngineManagerForVASTransferLine(
			IUserHaltableProductionRulesEngineService engine,
			IPutawayLocationFactLoader locationFactLoader,
			IPutawayLocationCacheUpdater putawayLocationCacheUpdater)
			: base(engine, locationFactLoader, putawayLocationCacheUpdater)
		{
		}

		public void Putaway(IEnumerable<WhsVASOrder> vasOrders, IEnumerable<VASReturnTransferLine> linesToPutaway, INotifications notifications, RefEquipment equipment = null)
		{
			Putaway(vasOrders, linesToPutaway, notifications, equipment, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Baseline")]
		protected override IInputFact AddInputFactFromLineCore(
			WhsVASOrder parent,
			VASReturnTransferLine line,
			OrganisationFact client,
			OrgSupplierPart product,
			PutawayProductFact productFact,
			EquipmentFact equipmentFact,
			Dictionary<ZGuid, OrganisationFact> organisations,
			bool isTsaKnownClient,
			bool isTsaPolicyRequired)
		{
			var vasServiceArea = parent.ServiceArea;

			return new InventoryFact(
								parent,
								line,
								line.TransferLine,
								client,
								product,
								productFact,
								equipment: equipmentFact,
								isTsaKnownClient: isTsaKnownClient,
								isTsaPolicyRequired: isTsaPolicyRequired,
								vasServiceAreaName: vasServiceArea.WA_Name,
								vasServiceAreaTypeCode: vasServiceArea.WA_AreaType);
		}

		protected override void AddFetchHintsForGetInvAndLocFactsCore(BusinessObjectFactory factory, IEnumerable<VASReturnTransferLine> lines)
		{
			AddDocketLineFetchHints(factory, lines.Select(l => l.TransferLine.PK).ToArray());
		}

		protected override void AddFetchHintsForProcessResultsCore(BusinessObjectFactory factory, IEnumerable<VASReturnTransferLine> lines, IEnumerable<PutawayResultFact> putawayInstructions)
		{
			foreach (var returnTransferLine in lines)
			{
				var transferLine = returnTransferLine.TransferLine;
				factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, transferLine.PK);
				foreach (var matchingLine in transferLine.MatchingLines)
				{
					factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, matchingLine.PK);
				}
				AddDocketLineFetchHints(factory, transferLine.MatchingLines.Select(l => l.PK).ToArray());
			}
		}

		static void AddDocketLineFetchHints(BusinessObjectFactory factory, ZGuid[] linePKs)
		{
			var childDocketLineQuery = new ZQuery();
			childDocketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WE_ParentDocketLine, linePKs);
			factory.AddFetchHint(WhsDocketLineSchema.Instance, childDocketLineQuery);

			var matchingDocketLineQuery = new ZQuery();
			matchingDocketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WE_MatchingLine, linePKs);
			factory.AddFetchHint(WhsDocketLineSchema.Instance, matchingDocketLineQuery);
		}

		protected override IEnumerable<(WhsVASOrder Parent, IEnumerable<VASReturnTransferLine> Lines)> GetGroupedLinesToPutaway(
			BusinessObjectFactory factory,
			IEnumerable<WhsVASOrder> parents,
			IEnumerable<VASReturnTransferLine> lines,
			WhsWarehouse warehouse)
		{
			var parent = parents.SingleOrDefault(); // VASTransferLines only putaway one order at a time
			var linesToPutaway = lines.Where(l => l.TransferLine.WE_WL.IsEmpty);
			return new[] { (parent, linesToPutaway) };
		}
	}
}
