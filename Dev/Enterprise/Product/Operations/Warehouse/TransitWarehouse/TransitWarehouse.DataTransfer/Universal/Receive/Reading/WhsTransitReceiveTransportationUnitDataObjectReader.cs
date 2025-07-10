using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Definitions.Customs;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveTransportationUnitDataObjectReader : DataObjectReader<Container, WhsItemReceiveTransportationUnit>
	{
		public WhsTransitReceiveTransportationUnitDataObjectReader(IColumnIndexer warehouse, Container dataObject, UniversalShipment consolDO, IColumnIndexer asnRow, IXmlImportLogger logger, UniversalObjectFactory factory, IOrgHeader bookingParty, string masterBill = "", bool isRejectedByHigherPriority = false, bool shouldCreateContainerPackageState = true)
			: base(dataObject, logger, factory)
		{
			Argument.NotNullOrEmpty(dataObject.ContainerNumber, nameof(dataObject.ContainerNumber));
			this.warehouse = Argument.NotNull(warehouse, nameof(warehouse));
			// ConsolDO could be loaded from the logger and factory
			this.consolDO = consolDO;
			this.masterBill = consolDO?.WayBillNumber ?? masterBill;
			this.consolNumber = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key ?? "";
			this.asnRow = asnRow;
			this.bookingParty = bookingParty;
			this.isRejectedByHigherPriority = isRejectedByHigherPriority;
			this.shouldCreateContainerPackageState = shouldCreateContainerPackageState;
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemReceiveTransportationUnit rtu) => rtu == null ? Res.GetString("99be46fd-8509-4371-a30d-d532ac291853", "Receive Transportation Unit") : rtu.HumanReadableName.ToString();

		readonly IColumnIndexer warehouse;
		readonly ZString masterBill;
		readonly ZString consolNumber;
		readonly UniversalShipment consolDO;
		readonly IColumnIndexer asnRow;
		readonly IOrgHeader bookingParty;
		readonly bool isRejectedByHigherPriority;
		readonly bool shouldCreateContainerPackageState;

		#region GetExistingBusinessObject

		bool IsFromSeaCargoOutturn => consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;
		ZString VesselLloyds => consolDO.LloydsIMO ?? "";
		ZString VoyageFlightNo => consolDO.VoyageFlightNo ?? "";
		ZString PremiseID => consolDO.AdditionalReferenceCollection?.Where(a => a.Type.Code.Equals(CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID)).FirstOrDefault()?.ReferenceNumber ?? "";

		protected override WhsItemReceiveTransportationUnit GetExistingBusinessObject()
		{
			if (IsFromSeaCargoOutturn)
			{
				return WhsTransitReceiveTransportationUnitMatchingHelper.GetExistingRTUForSeaCargo(factory, logger, dataObject.ContainerNumber.Value, warehouse, VesselLloyds, VoyageFlightNo, PremiseID, isRejectedByHigherPriority);
			}
			else
			{
				return WhsTransitReceiveTransportationUnitMatchingHelper.GetExistingRTUForConsol(factory, logger, dataObject.ContainerNumber.Value, warehouse, masterBill, consolNumber);
			}
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemReceiveTransportationUnit unit)
		{
			var headerRow = GetColumnIndexer(unit);
			PopulateAdditionalReferences(unit);
			CreateUniversalLink(unit);

			if (IsNewBO)
			{
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber, NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseReceiveID));
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, dataObject.ContainerNumber);
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, warehouse.GetValue(WhsWarehouseSchema.PK));
			}

			if (!warehouse.GetValue(WhsWarehouseSchema.WW_DefaultInboundDockDoor).IsEmpty && unit.WRH_WL_StagingLocation.IsEmpty)
			{
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation, warehouse.GetValue(WhsWarehouseSchema.WW_DefaultInboundDockDoor));
			}

			if (!isRejectedByHigherPriority)
			{
				var packageCollection = unit.PackageJob.Packages;
				var isSeaCargoOutturn = consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;
				if (shouldCreateContainerPackageState)
				{
					if (!isSeaCargoOutturn)
					{
						// This is only for container
						var originalContainerType = unit.Container?.ContainerType;
						var packageContainerReader = new PkgPackageContainerDataObjectReader(dataObject, logger, factory, packageCollection, unit.Container?.Package);
						var containerPackage = packageContainerReader.ReadIntoBusinessObject();
						if (containerPackage.Container != null)
						{
							var containerType = containerPackage.Container.ContainerType;
							var unitType = TransportUnitTypes.ConvertTypeToTransportUnitType(containerType);
							SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_UnitType, unitType);
							TransitUniversalHelper.CreatePackageStateForContainerizedTransportationUnit(factory, containerPackage, unit, warehouse.GetValue(WhsWarehouseSchema.PK), unitType);

							if (originalContainerType != null && containerPackage.Container.K0_RC_ContainerType != originalContainerType.PK)
							{
								var links = UniversalJobLinkHelper.GetMatchingJobLinkEntities(unit);
								if (links.Any(l => l.UCL_SourceType == "GateMovementBooking"))
								{
									var rtuEventReference = WhsTransitLogHelper.GetEventReferenceString(
									new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "ContainerType"),
									new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, containerType.RC_Code),
									new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalContainerType.RC_Code));

									WhsTransitLogHelper.AddStmALogToBizoObject(unit, AutoEvents.ContainerTypeUpdated, rtuEventReference);

									logger.Log(LogType.Warning, $"RTU container type updated from: {originalContainerType.RC_Code} to {containerType.RC_Code}");
								}
							}
						}
						else
						{
							var containerNumber = dataObject.ContainerNumber;
							throw new DataObjectReadFailureException(Res.GetString("31bbdae4-6304-4233-b52f-96b4a40ad0f5", "Container {0} does not have a supported Container Type.", containerNumber));
						}
					}
					else
					{
						SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_UnitType, TransportUnitTypes.Container);
						var package = TransitUniversalHelper.CreatePkgPackageAndExtension(unit.PackageJob, logger, factory, dataObject.ContainerNumber);
						TransitUniversalHelper.CreatePackageStateForContainerizedTransportationUnit(factory, package, unit, warehouse.GetValue(WhsWarehouseSchema.PK), TransportUnitTypes.Container);
					}
				}

				if (unit.PackageStates.Any(pkg => !pkg.WPS_UnloadedTime.IsEmpty))
				{
					var errorMessage = Res.GetString("5ccdc84b-eca3-4db1-991f-b3c12a7c9e54", "Cannot update the RTU Transport Company Address. Some packages have already been unloaded.");
					var existingTransportCompanyAddress = unit.DocAddresses?.Where(address => address.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress).SingleOrDefault();

					var newDepartureAddress = consolDO.OrganizationAddressCollection?.SingleOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.DepartureCFSLocalTransportAddress));
					var newArrivalAddress = consolDO.OrganizationAddressCollection?.SingleOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ArrivalCFSLocalTransportAddress));

					if (IsUpdatingAddress(existingTransportCompanyAddress, newDepartureAddress) ||
						IsUpdatingAddress(existingTransportCompanyAddress, newArrivalAddress))
					{
						throw new DataObjectReadFailureException(errorMessage);
					}
				}

				OrgAddressImportHelper.PopulateTransportOrg(consolDO, null, bookingParty as OrgHeader, unit, logger, factory);
				OrgAddressImportHelper.PopulateBillToPartyOrg(consolDO, unit, logger, factory);
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_TransportProviderIsKnown, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(unit));

				LinkASNAndRTU(unit.PK);
			}
		}

		bool IsUpdatingAddress(JobDocAddress jobDocAddress, OrganizationAddress orgAddress)
		{
			return jobDocAddress != null && orgAddress != null &&
					((jobDocAddress.Address1 != ZString.Empty && jobDocAddress.Address1 != orgAddress.Address1.GetValueOrDefault()) ||
					(jobDocAddress.Address2 != ZString.Empty && jobDocAddress.Address2 != orgAddress.Address2.GetValueOrDefault()));
		}

		void CreateUniversalLink(WhsItemReceiveTransportationUnit unit)
		{
			// if UXML is coming from the same system, booking party will be org proxy.
			// It is possible booking pary could not be determined if UXML is coming from an external system.
			if (bookingParty != null && consolDO != null)
			{
				var dataContext = consolDO.DataContext;
				var linkCreator = new UniversalJobLinkCreator(unit.Factory, unit, bookingParty, dataContext, logger);
				linkCreator.TryCreateJobLink(DataContextType.SeaCargoOutturn);
			}
		}

		#endregion

		#region PopulateAdditionalReferences

		protected void PopulateAdditionalReferences(WhsItemReceiveTransportationUnit unit)
		{
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);

			var additionalReferences = new List<TransitAdditionalReferenceInfo>();
			if (!IsFromSeaCargoOutturn)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.MasterBill, masterBill);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, consolNumber);
			}
			else
			{
				// sea cargo outturn references
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.MasterBill, masterBill);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VesselLloyds, consolDO.LloydsIMO);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, consolDO.VoyageFlightNo);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.Vessel, consolDO.VesselName);
			}
			new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), unit, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
		}

		#endregion

		#region Link ASN

		void LinkASNAndRTU(ZGuid rtuPK)
		{
			if (asnRow != null)
			{
				var asnPK = asnRow.GetValue(WhsItemReceiveASNSchema.PK);
				var pivotQuery = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtuPK);
				pivotQuery.AddToFilter(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPK));

				if (factory.RowFactory.Load(WhsItemReceiveASNRTUPivotSchema.Constants.TableName, pivotQuery).Length == 0)
				{
					var pivotRow = factory.RowFactory.NewRowWithPK(WhsItemReceiveASNRTUPivotSchema.Instance);

					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtuPK);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPK);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemCreateTimeUtc, ZDateTime.UtcNow);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemCreateUser, User.InterchangeUserCode);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemLastEditTimeUtc, ZDateTime.UtcNow);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemLastEditUser, User.InterchangeUserCode);
				}
			}
		}

		#endregion
	}
}
