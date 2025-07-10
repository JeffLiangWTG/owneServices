using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitReceiveConsolHandler : ITransitDataObjectReaderHandler
	{
		TransitDataObjectReaderHandlerManager HandlerManager
		{
			get
			{
				handlerManager ??= ObjectFactory.Get<TransitDataObjectReaderHandlerManager>();
				return handlerManager;
			}
		}
		TransitDataObjectReaderHandlerManager handlerManager;

		public void Execute(UniversalObjectFactory factory, UniversalShipment dataObject, IXmlImportLogger logger)
		{
			RemoveUnusedASNs(factory, logger);
		}

		void RemoveUnusedASNs(UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			if (HandlerManager.GetAffectedASNPKs().Count > 0)
			{
				// Exclude ASNs with remaining packages
				var packageQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRP_ReceiveExpectedPacking, HandlerManager.GetAffectedASNPKs());
				var remainingPackages = factory.BOFactory.Load<WhsItemPackageState>(packageQuery);

				var distinctASNsWithBookedPackages = remainingPackages.Select(p => p.WPS_WRP_ReceiveExpectedPacking).Distinct();
				var asnPKsToDelete = HandlerManager.GetAffectedASNPKs().Except(distinctASNsWithBookedPackages).ToArray();

				var queryForPivots = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPKsToDelete);
				var pivotsToDelete = factory.BOFactory.Load<WhsItemReceiveASNRTUPivot>(queryForPivots);
				var rtuPKsToDelete = GetDetachedRTUs(pivotsToDelete, factory);
				DeleteEntities(asnPKsToDelete, pivotsToDelete, rtuPKsToDelete, factory, logger);
			}
		}

		static IEnumerable<ZGuid> GetDetachedRTUs(IColumnIndexer[] pivotsToDelete, UniversalObjectFactory factory)
		{
			if (pivotsToDelete.Length == 0)
			{
				return Array.Empty<ZGuid>();
			}

			// Exclude RTUs with other planned ASNs
			var rtuPKs = pivotsToDelete.Select(pivotRow => pivotRow.GetValue(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit)).ToArray();
			var pivotPKsToDelete = pivotsToDelete.Select(pivotRow => pivotRow.GetValue(WhsItemReceiveASNRTUPivotSchema.PK));

			var queryForRemainingPivots = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtuPKs);
			queryForRemainingPivots.AddToFilter(new ZQuery(WhsItemReceiveASNRTUPivotSchema.PK, SQLComparisonOperator.NotEqual, pivotPKsToDelete));
			var remainingPivots = factory.BOFactory.Load<WhsItemReceiveASNRTUPivot>(queryForRemainingPivots);
			var rtuPKsToDelete = rtuPKs.Except(remainingPivots.Select(p => p.GetValue(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit))).ToArray();

			if (rtuPKsToDelete.Length > 0)
			{
				// Exclude RTUs with arrived packages
				var packageStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader, rtuPKsToDelete);
				var arrivedPackages = factory.BOFactory.Load<WhsItemPackageState>(packageStateQuery);

				var rtusWithArrivedPackages = arrivedPackages.Select(p => p.GetValue(WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader)).Distinct();
				rtuPKsToDelete = rtuPKsToDelete.Except(rtusWithArrivedPackages).ToArray();
			}

			return rtuPKsToDelete;
		}

		static void DeleteEntities(IEnumerable<ZGuid> asnPKsToDelete, IColumnIndexer[] pivotPKsToDelete, IEnumerable<ZGuid> rtuPKsToDelete, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			pivotPKsToDelete.ForEach(p => factory.DeleteRowAndSetHasChanges<WhsItemReceiveASNRTUPivot>(p, WhsItemReceiveASNRTUPivotSchema.PK));

			if (asnPKsToDelete.Any())
			{
				var asnRowsToDelete = factory.BOFactory.Load<WhsItemReceiveASN>(new ZQuery(WhsItemReceiveASNSchema.PK, asnPKsToDelete)).ToArray();
				var asnReferenceNumbers = asnRowsToDelete.Select(asn => asn.WRP_ReferenceNumber).ToArray();
				logger.Log(LogType.Information, ResString.GetMultilingualString("9cac46a1-5067-4180-b99c-dd83f8a4502c", "Deleted ASNs '{0}' as they have no remaining packages.",
					ZString.Join(", ", asnReferenceNumbers)));
				asnRowsToDelete.ForEach(asn => factory.DeleteRowAndSetHasChanges<WhsItemReceiveASN>(asn, WhsItemReceiveASNSchema.PK));

				// Delete references
				var referenceQuery = new ZQuery();
				referenceQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, WhsItemReceiveASNSchema.Constants.TableName);
				referenceQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, asnPKsToDelete);
				var refs = factory.BOFactory.Load<CusEntryNumber>(referenceQuery).ToArray();
				refs.ForEach(r => factory.DeleteRowAndSetHasChanges<CusEntryNumber>(r, CusEntryNumSchema.PK));
			}

			if (rtuPKsToDelete.Any())
			{
				var unloadTasks = factory.BOFactory.Load<WhsItemUnloadTask>(new ZQuery(WhsItemUnloadTaskSchema.WUT_WRH_ActiveReceiveHeader, rtuPKsToDelete)).ToArray();
				unloadTasks.ForEach(unloadTask => factory.DeleteRowAndSetHasChanges<WhsItemUnloadTask>(unloadTask, WhsItemUnloadTaskSchema.PK));

				var rtuRowsToDelete = factory.BOFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery(WhsItemReceiveTransportationUnitSchema.PK, rtuPKsToDelete)).ToArray();
				var rtuReferenceNumbers = rtuRowsToDelete.Select(asn => asn.GetValue(WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber)).ToArray();
				logger.Log(LogType.Information, ResString.GetMultilingualString("26a5062b-f4c9-4f1a-a2ff-179a4c6182f9", "Deleted RTUs '{0}' as their planned ASNs were deleted.",
					ZString.Join(", ", rtuReferenceNumbers)));
				rtuRowsToDelete.ForEach(rtu => factory.DeleteRowAndSetHasChanges<WhsItemReceiveTransportationUnit>(rtu, WhsItemReceiveTransportationUnitSchema.PK));
			}
		}
	}
}
