using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitDataObjectReaderHandlerManager;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitStoredProcedureHandler;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveConsolDataObjectReader : WhsTransitDataObjectReader<WhsTransitReceiveConsol>
	{
		public WhsTransitReceiveConsolDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			RegisterHandler();
		}

		TransitReceiveConsolHandler ConsolHandler => (TransitReceiveConsolHandler)HandlerManager.GetHandler<TransitReceiveConsolHandler>();

		#region Warehouse

		IColumnIndexer Warehouse
		{
			get
			{
				warehouse ??= WarehouseMatchingHelper.GetWarehouse(dataObject, factory, logger);
				if (warehouse == null)
				{
					WarehouseMatchingHelper.ThrowForNoMatchingWarehouse(dataObject, logger);
				}
				return warehouse;
			}
		}
		IColumnIndexer warehouse;

		#endregion

		protected override void PopulateBusinessObject(WhsTransitReceiveConsol targetBO)
		{
			var subShipmentDOs = dataObject.SubShipmentCollection;
			if (subShipmentDOs == null || !subShipmentDOs.Any())
			{
				throw new DataObjectReadFailureException(Res.GetString("91fd2dbe-0995-4893-b77a-7febb9d510ee", "No Receive Consignments (e.g. Forwarding Shipments) were included in the Receive Instruction."));
			}

			if (dataObject.IsTransitWarehouseCombined())
			{
				WhsTransitLogHelper.LogStartOfReceiveInstruction(logger);
			}

			var matchingReceiveConsignmentsDict = new Dictionary<WhsItemReceiveConsignment, bool>();
			WhsTransitLogHelper.LogRecipientRole(dataObject, logger);

			var houseBills = subShipmentDOs.Where(p => !string.IsNullOrEmpty(p.WayBillNumber)).ToLookup(p => p.WayBillNumber.GetValueOrDefault().ToUpper());
			var duplicatedHSBs = houseBills.Where(g => g.Count() > 1).Select(g => g.Key).Distinct();

			if (duplicatedHSBs.Any())
			{
				throw new DataObjectReadFailureException(Res.GetString("70178531-6115-48de-b125-08ec51b4ed96", "Duplicate House Bill Number(s) found in the Receive Instruction: {0}.", string.Join(", ", duplicatedHSBs)));
			}

			// Filter out Gate Booking shipment
			subShipmentDOs = new DataObjectList<UniversalShipment>(subShipmentDOs.Where(subShipment => !subShipment.IsFromGateBooking()));

			var shipmentExceptions = new List<(UniversalShipment, DataObjectReadFailureException)>();
			foreach (var shipment in subShipmentDOs.OrderBy(s => !string.IsNullOrEmpty(s.WayBillNumber) ? s.WayBillNumber.Value.ToString() : ""))
			{
				try
				{
					var reader = new WhsTransitReceiveConsignmentDataObjectReader(shipment, logger, factory, Warehouse);
					var rcn = reader.ReadIntoBusinessObject();
					if (!matchingReceiveConsignmentsDict.ContainsKey(rcn))
					{
						matchingReceiveConsignmentsDict.Add(rcn, reader.IsRecalculatePackageStateSecurityStatus);
					}
				}
				catch (DataObjectReadFailureException ex)
				{
					shipmentExceptions.Add((shipment, ex));
					logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
				}
			}
			TransitUniversalHelper.ThrowIfShipmentsHaveExceptions(dataObject, shipmentExceptions);

			var rcnCreatedByHigherPrioritySource = HandlerManager.GetPackagesByContainerLink().Count == 0;

			HandlerManager.BuildHandler(HandlerType.Container, dataObject).Execute(factory, dataObject, logger);

			HandlerManager.BuildHandler(HandlerType.Vehicle, dataObject).Execute(factory, dataObject, logger);

			ConsolHandler.Execute(factory, dataObject, logger);

			if (rcnCreatedByHigherPrioritySource)
			{
				ImportASNAdditionalReferences(matchingReceiveConsignmentsDict.Keys.ToList());
			}

#if DEBUG
			targetBO.PopulatedConsignmentsForTesting = matchingReceiveConsignmentsDict.Keys.ToArray();
#endif

			if (dataObject.IsTransitWarehouseCombined())
			{
				factory.SaveAtEndOfImport(logger);

				WhsTransitLogHelper.LogStartOfDispatchInstruction(logger);

				var dispatchConsolReader = new WhsTransitDispatchConsolDataObjectReader(dataObject, logger, factory);
				dispatchConsolReader.ReadIntoBusinessObject();

				var warehouseBO = factory.BOFactory.Load<WhsWarehouse>(Warehouse.GetValue(WhsWarehouseSchema.PK));
				var packageStates = matchingReceiveConsignmentsDict.Keys.SelectMany(rcn => rcn.PackageStates).Where(p => !p.IsDeleted).ToArray();
				if (packageStates.Length > 0)
				{
					StoredProcedureHandler.AddCachedPackageState([.. packageStates.Select(p => p.PK)], ProcedureType.CustomStatus);

					var matchingRCNPKs = matchingReceiveConsignmentsDict.Where(item => item.Value).Select(r => r.Key.PK.ToGuid()).ToArray();
					if (matchingRCNPKs.Length > 0)
					{
						StoredProcedureHandler.AddCachedPackageState([.. packageStates.Select(p => p.PK)], ProcedureType.SecurityStatus);
					}
				}
			}

			ExecuteStoreProcedure();
		}

		void ImportASNAdditionalReferences(List<WhsItemReceiveConsignment> matchingReceiveConsignments)
		{
			var warehouse = HandlerManager.GetWarehouse();
			foreach (var rcn in matchingReceiveConsignments)
			{
				var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger);
				var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
				if (consolDO.ContainerCollection != null && consolDO.ContainerCollection.Count <= 1)
				{
					var reader = new WhsTransitReceiveASNDataObjectReader(dataObject, consolDO.ContainerCollection, null, bookingParty, warehouse, logger, factory, true);
					reader.ReadIntoBusinessObject();
				}
			}
		}

		protected override WhsTransitReceiveConsol GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return new WhsTransitReceiveConsol();
		}

		protected override IMatchingBusinessEntityFinder<WhsTransitReceiveConsol> GetCombinedReferenceMatcher()
		{
			return null;
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitReceiveConsol; }
		}

		#region Log Message

		protected override void LogSuccessfullyLoadedMessage(string typeName)
		{
		}

		protected override void LogPopulatingMessage(string typeName)
		{
		}

		#endregion

		#region RegisterHandler

		protected override void RegisterHandler()
		{
			var warehouse = factory.Load<WhsWarehouse>(Warehouse.GetValue(WhsWarehouseSchema.PK));
			HandlerManager.Init(logger, factory, this, warehouse);
			var consolHander = HandlerManager.BuildHandler(HandlerType.Consol, dataObject);
			HandlerManager.RegisterHandler(consolHander);

			base.RegisterHandler();
		}

		#endregion
	}
}
