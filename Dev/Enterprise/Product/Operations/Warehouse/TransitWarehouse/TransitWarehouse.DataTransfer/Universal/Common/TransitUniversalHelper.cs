using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class TransitUniversalHelper
	{
		#region GetBookingParty

		public static IOrgHeader GetBookingParty(UniversalObjectFactory factory, IXmlImportLogger logger, bool isRequired = true)
		{
			var dataContext = logger.TopLevelDataContext;
			var role = dataContext?.RecipientRoleCollection?.FirstOrDefault();

			IOrgHeader bookingParty;
			if (role != null && (role.Code == RecipientRoleType.DTW || role.Code == RecipientRoleType.ATW))
			{
				bookingParty = GetBookingPartyWithRecipientType(factory, logger, isRequired);
			}
			else
			{
				bookingParty = GetBookingPartyWithoutRecipientType(factory, logger, isRequired);
			}

			return bookingParty;
		}

		static IOrgHeader GetBookingPartyWithRecipientType(UniversalObjectFactory factory, IXmlImportLogger logger, bool isRequired)
		{
			var dataContext = logger.TopLevelDataContext;
			var role = dataContext?.RecipientRoleCollection?.FirstOrDefault();
			var addressType = role.Code == RecipientRoleType.DTW ? DocAddressType.SendingForwarderAddress : DocAddressType.ReceivingForwarderAddress;

			var (bookingParty, orgCodeByConsolOrgs) = GetBookingPartyByConsolOrgs(factory, logger, addressType.ToString());

			if (bookingParty != null)
			{
				var message = role.Code == RecipientRoleType.DTW ? Res.GetString("4f63f381-6c26-42e7-beb3-3e8d2a4e9c89", "Matching 'Booking Party':- Matched to '{0}' by code from '{1}' for Departure Transit Warehouse. Successfully loaded Booking Party.", bookingParty.OH_Code, addressType.ToString()) : Res.GetString("a9bd0ae5-6adf-4156-bcb5-3fb7ac9047d6", "Matching 'Booking Party':- Matched to '{0}' by code from '{1}' for Arrival Transit Warehouse. Successfully loaded Booking Party.", bookingParty.OH_Code, addressType.ToString());
				logger.Log(LogType.Information, message);
			}
			else
			{
				bookingParty = GetBookingPartyFromTheSystem(factory, logger);

				if (orgCodeByConsolOrgs.IsNullOrEmpty())
				{
					if (bookingParty != null)
					{
						var message = role.Code == RecipientRoleType.DTW ? Res.GetString("252cfa9b-31a7-489a-aa7d-1875267cab46", "Matching 'Booking Party':- Organization Code is empty in '{0}' for this Departure Transit Warehouse, but UXML generated from the same system. Matched to '{1}' by code. Successfully loaded Booking Party.", addressType.ToString(), bookingParty.OH_Code) : Res.GetString("8d4f243f-b48e-455d-9482-3f07a792ba40", "Matching 'Booking Party':- Organization Code is empty in '{0}' for this Arrival Transit Warehouse, but UXML generated from the same system. Matched to '{1}' by code. Successfully loaded Booking Party.", addressType.ToString(), bookingParty.OH_Code);
						logger.Log(LogType.Information, message);
					}
					else
					{
						var message = role.Code == RecipientRoleType.DTW ? Res.GetString("ce6a8693-fda4-4d09-8917-581b7c41efac", "Could not find 'Booking Party':- Organization Code is empty in '{0}' for this Departure Transit Warehouse, and UXML does not originate from this system.", addressType.ToString()) : Res.GetString("71ef9690-5003-49ba-a07a-88d01011fabe", "Could not find 'Booking Party':- Organization Code is empty in '{0}' for this Arrival Transit Warehouse, and UXML does not originate from this system.", addressType.ToString());
						logger.Log(isRequired ? LogType.Error : LogType.Warning, message);
					}
				}
				else
				{
					if (bookingParty != null)
					{
						var message = role.Code == RecipientRoleType.DTW ? Res.GetString("11697cfa-4c76-4288-8db3-553ebfffc77f", "Matching 'Booking Party':- Organization Code '{0}' does not exist for this Departure Transit Warehouse, but UXML generated from the same system. Matched to '{1}' by code. Successfully loaded Booking Party.", orgCodeByConsolOrgs, bookingParty.OH_Code) : Res.GetString("b7730dd7-03cc-4ccd-9235-cc4a54f5a07c", "Matching 'Booking Party':- Organization Code '{0}' does not exist for this Arrival Transit Warehouse, but UXML generated from the same system. Matched to '{1}' by code. Successfully loaded Booking Party.", orgCodeByConsolOrgs, bookingParty.OH_Code);
						logger.Log(LogType.Information, message);
					}
					else
					{
						var message = role.Code == RecipientRoleType.DTW ? Res.GetString("bb978075-ffeb-4763-b3ed-70740469cdac", "Could not find 'Booking Party':- Organization Code '{0}' does not exist for this Departure Transit Warehouse, and UXML does not originate from this system.", orgCodeByConsolOrgs) : Res.GetString("51d9abf0-2bb1-487b-b781-099d11692405", "Could not find 'Booking Party':- Organization Code '{0}' does not exist for this Arrival Transit Warehouse, and UXML does not originate from this system.", orgCodeByConsolOrgs);
						logger.Log(isRequired ? LogType.Error : LogType.Warning, message);
					}
				}
			}

			return bookingParty;
		}

		static (IOrgHeader org, string orgCodeByConsolOrgs) GetBookingPartyByConsolOrgs(UniversalObjectFactory factory, IXmlImportLogger logger, string addressType)
		{
			var consolObject = logger.GetTopLevelDataObject();
			var address = consolObject.OrganizationAddressCollection.FirstOrDefault(addressType);

			IOrgHeader bookingParty = null;
			var orgCodeByConsolOrgs = "";
			if (address != null && address.OrganizationCode.HasValue)
			{
				bookingParty = factory.LoadFromUniqueKey<IOrgHeader>(OrgHeaderSchema.OH_Code, (ZString)address.OrganizationCode);
				orgCodeByConsolOrgs = address.OrganizationCode;
			}

			return (bookingParty, orgCodeByConsolOrgs);
		}

		static IOrgHeader GetBookingPartyWithoutRecipientType(UniversalObjectFactory factory, IXmlImportLogger logger, bool isRequired)
		{
			var bookingParty = GetBookingPartyFromTheSystem(factory, logger);

			if (bookingParty != null)
			{
				logger.Log(LogType.Information,
						Res.GetString("73843c4d-2562-40f0-a8fb-575090f88c2f", "Matching 'Booking Party':- Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse, but UXML generated from the same system. Matched to '{0}' by code. Successfully loaded Booking Party.", bookingParty.OH_Code));
			}
			else
			{
				logger.Log(isRequired ? LogType.Error : LogType.Warning,
					Res.GetString("027c27d1-c66b-4f67-a39a-cf9ded21b0b9", "Could not find 'Booking Party':- Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse, and UXML does not originate from this system."));
			}

			return bookingParty;
		}

		static IOrgHeader GetBookingPartyFromTheSystem(UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var dataContext = logger.TopLevelDataContext;

			var bookingParty = dataContext.GetSourceOrganisation(logger, factory.BOFactory);
			if (bookingParty == null && dataContext.IsFromSameSystem())
			{
				bookingParty = GlbBranch.CurrentBranch.OrgProxy;
			}

			return bookingParty;
		}

		#endregion

		public static IEnumerable<UniversalShipment> GetShipmentsToCreateConsignments(IEnumerable<UniversalShipment> shipmentDOs, Guid branchPK)
		{
			if (shipmentDOs != null && shipmentDOs.Any())
			{
				// Filter out subShipments from Gate Booking
				shipmentDOs = shipmentDOs.Where(subShipment => !subShipment.IsFromGateBooking());
			}

			if (WarehouseDataRegistry.Instance.EnableCoLoadAndAssemblyMasterSubConsignmentsCreation.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty) && shipmentDOs != null)
			{
				var shipments = new List<UniversalShipment>();

				foreach (var shipment in shipmentDOs)
				{
					shipments.AddRange(GetAllLevelSubShipmentsToCreateConsignments(shipment, branchPK));
				}
				return shipments;
			}
			else
			{
				return shipmentDOs;
			}
		}

		static bool IsColoadMaster(UniversalShipment shipment)
		{
			var coloadMasterTypes = new[] { Constants.ShipmentTypes.BlindCoLoadMaster, Constants.ShipmentTypes.CoLoadMaster };
			var shipmentType = shipment.ShipmentType?.Code ?? ZString.Empty;
			return coloadMasterTypes.Contains(shipmentType.ToString());
		}

		static bool IsEmptyMaster(UniversalShipment shipment)
		{
			return shipment.SubShipmentCollection == null || !shipment.SubShipmentCollection.Any();
		}

		public static void CreatePackageStateForContainerizedTransportationUnit(UniversalObjectFactory factory, PkgPackage containerPackage, IPackingParentSupportsPackageExtensions parentSupportPackageExtension, ZGuid warehousePK, string unitType)
		{
			if (unitType == TransportUnitTypes.ULD || unitType == TransportUnitTypes.Container)
			{
				var extension = parentSupportPackageExtension.PackageExtension;
				var query = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, extension.KPN_KP_Package);
				query.AddToFilter(WhsItemPackageStateSchema.WPS_WW_Warehouse, warehousePK);
				var packageState = factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, query).FirstOrDefault();
				var uldRow = packageState == null
					? factory.RowFactory.NewRowWithPK(WhsItemPackageStateSchema.Instance)
					: DataObjectReader.GetColumnIndexerFromRow(packageState);
				if (packageState == null)
				{
					uldRow.SetValue(WhsItemPackageStateSchema.WPS_Status, (ZString)TransitWarehouseStatuses.Codes.Booked);
					uldRow.SetValue(WhsItemPackageStateSchema.WPS_SystemCreateTimeUtc, ZDateTime.UtcNow);
					uldRow.SetValue(WhsItemPackageStateSchema.WPS_SystemCreateUser, new ZString(User.InterchangeUserCode));
					uldRow.SetValue(WhsItemPackageStateSchema.WPS_SecurityStatus, new ZString(TransitWarehouseSecurityStatuses.Codes.NotRequired));
					uldRow.SetValue(WhsItemPackageStateSchema.WPS_CustomsStatus, new ZString(TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired));
				}
				uldRow.SetValue(WhsItemPackageStateSchema.WPS_WW_Warehouse, warehousePK);
				uldRow.SetValue(WhsItemPackageStateSchema.WPS_KP_Package, containerPackage.PK);
				uldRow.SetValue(WhsItemPackageStateSchema.WPS_IsHandlingUnit, ZBool.True);
				uldRow.SetValue(WhsItemPackageStateSchema.WPS_SystemLastEditTimeUtc, ZDateTime.UtcNow);
				uldRow.SetValue(WhsItemPackageStateSchema.WPS_SystemLastEditUser, new ZString(User.InterchangeUserCode));
				uldRow.SetValue(WhsItemPackageStateSchema.WPS_UnitType, new ZString(unitType));
			}
		}

		public static PkgPackage CreatePkgPackageAndExtension(PkgPackageJob packageJob, IXmlImportLogger logger, UniversalObjectFactory factory, string uldNumber)
		{
			var packLine = new PackingLine { PackType = new PackageType { Code = PkgUnit.Unit }, PackQty = 1, ReferenceNumber = uldNumber };
			var packageReader = new PkgPackageDataObjectReader(packLine, logger, factory, packageJob, packageJob.Packages, ImportOption.MatchOnPackageIDs, shouldCreatePackageExtension: true);
			var package = packageReader.ReadIntoBusinessObject();
			return package;
		}

		public static string GetUnitTypeFromContainerDataObject(UniversalObjectFactory factory, Container container)
		{
			var unitType = "";
			var containerTypeCode = container.ContainerType?.GetCodeAsUpperCase().Trim() ?? "";
			if (!string.IsNullOrEmpty(containerTypeCode))
			{
				var refContainer = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, containerTypeCode);
				if (refContainer != null)
				{
					unitType = TransportUnitTypes.ConvertTypeToTransportUnitType(refContainer);
				}
			}
			return unitType;
		}

		static IEnumerable<UniversalShipment> GetAllLevelSubShipmentsToCreateConsignments(UniversalShipment shipment, Guid branchPK)
		{
			var shipments = new List<UniversalShipment>();

			if (IsMasterShipmentRepresentingAllChildShipments(shipment))
			{
				if (IsBuyersConsolLead(shipment) ||
					(IsColoadMaster(shipment) && IsEmptyMaster(shipment) &&
					WarehouseDataRegistry.Instance.EnableImportingCoLoadMastersWithoutSubs.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty)))
				{
					shipments.Add(shipment);
				}

				if (shipment.SubShipmentCollection != null)
				{
					var subShipmentsToCreateConsignment = shipment.SubShipmentCollection;
					foreach (var sub in subShipmentsToCreateConsignment)
					{
						shipments.AddRange(GetAllLevelSubShipmentsToCreateConsignments(sub, branchPK));
					}
				}
			}
			else
			{
				shipments.Add(shipment);
			}
			return shipments;
		}

		public static bool CanShipmentCreateConsignment(UniversalShipment shipment, ZGuid branchPK)
		{
			var isStandardShipment = !IsMasterShipmentRepresentingAllChildShipments(shipment);
			return isStandardShipment
				|| IsBuyersConsolLead(shipment)
				|| IsColoadMaster(shipment) && IsEmptyMaster(shipment) && WarehouseDataRegistry.Instance.EnableImportingCoLoadMastersWithoutSubs.GetFallBackValueAtAllLevels(Guid.Empty, branchPK.ToGuid(), Guid.Empty);
		}

		public static bool IsMasterShipmentRepresentingAllChildShipments(UniversalShipment shipment)
		{
			var shipmentTypesWithSubs = new[] { Constants.ShipmentTypes.AssemblyMaster, Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypes.BlindCoLoadMaster, Constants.ShipmentTypes.CoLoadMaster };
			var shipmentType = shipment.ShipmentType?.Code ?? ZString.Empty;
			return shipmentTypesWithSubs.Contains(shipmentType.ToString());
		}

		public static void UpdateAllowPartialLoading(UniversalObjectFactory factory, IReadOnlyCollection<IColumnIndexer> dispatchConsignmentRows)
		{
			foreach (var consignmentRow in dispatchConsignmentRows)
			{
				var packages = factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, consignmentRow.GetValue(WhsItemDispatchConsignmentSchema.PK))).ToArray();
				if (packages.Length > 0 && packages.Any(p => p[WhsItemPackageStateSchema.Constants.WPS_WDL_LoadList] != null))
				{
					var isMixedDLLs = (ZBool)(packages.Select(p => p[WhsItemPackageStateSchema.Constants.WPS_WDL_LoadList]).Distinct().Count() > 1);
					if (isMixedDLLs)
					{
						consignmentRow.SetValue(WhsItemDispatchConsignmentSchema.WDC_AllowPartialLoading, isMixedDLLs);
					}
					else
					{
						consignmentRow.SetValue(WhsItemDispatchConsignmentSchema.WDC_AllowPartialLoading, (ZBool)GetDispatchConsignmentWarehouseAllowPartialLoadingDefault(factory, consignmentRow));
					}
				}
				else
				{
					consignmentRow.SetValue(WhsItemDispatchConsignmentSchema.WDC_AllowPartialLoading, (ZBool)GetDispatchConsignmentWarehouseAllowPartialLoadingDefault(factory, consignmentRow));
				}
			}
		}

		static bool GetDispatchConsignmentWarehouseAllowPartialLoadingDefault(UniversalObjectFactory factory, IColumnIndexer dispatchConsignmentRow)
		{
			bool result = false;
			var warehouse = factory.RowFactory.Load(WhsWarehouseSchema.Constants.TableName, new ZQuery(WhsWarehouseSchema.PK, dispatchConsignmentRow.GetValue(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse))).FirstOrDefault();
			if (warehouse != null)
			{
				result = (bool)warehouse[WhsWarehouseSchema.Constants.WW_AllowPartialLoadingDefault];
			}

			return result;
		}

		static bool IsBuyersConsolLead(UniversalShipment dataObject)
		{
			var shipmentType = dataObject.ShipmentType?.Code ?? ZString.Empty;
			return shipmentType == Constants.ShipmentTypes.BuyersConsolLead;
		}

		public static void ThrowIfShipmentsHaveExceptions(UniversalShipment dataObject, List<(UniversalShipment, DataObjectReadFailureException)> shipmentExceptions)
		{
			if (shipmentExceptions.Any())
			{
				var helper = new ShipmentTransitLogHelper();
				var dataSource = dataObject.FirstDataSource();
				var errorMessage = Res.GetString("7391e14f-253b-463a-8b7e-b94973d9d2a9", "The following Sub Shipments encountered errors when importing {0} - {1}:", dataSource?.Type, dataSource?.Key);
				var errorLog = new ZStringBuilder(helper.GetTable(errorMessage, shipmentExceptions.Select(t => t.Item1).ToArray(), TransitLogColumnIDs.ShipmentColumn.Source));

				for (var i = 0; i < shipmentExceptions.Count; i++)
				{
					var shipment = shipmentExceptions[i].Item1;
					var exception = shipmentExceptions[i].Item2;
					var shipmentDataSource = shipment.FirstDataSource();
					errorLog.AppendLine();
					errorLog.AppendLine(Res.GetString("e157d4b6-3be4-4e9f-b022-e28b8415e410", "Error importing {0} - {1}:", shipmentDataSource?.Type, shipmentDataSource?.Key));
					errorLog.AppendLine(exception.Message);
				}

				throw new DataObjectReadFailureException(errorLog.ToString());
			}
		}

		public static bool IsGivenPortARelatedOrExtraPort(WhsWarehouse warehouse, string givenPort)
		{
			var homePortUNLOCO = warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var result = givenPort == homePortUNLOCO;
			if (!result)
			{
				var extraPorts = warehouse.RelatedCompanyBranch.ExtraPorts.Cast<GlbBranchExtraPorts>();
				result = extraPorts.Any(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort == givenPort);
			}

			return result;
		}

		#region GetAddOnValuesByName

		public static IEnumerable<GenCustomAddOnValue> GetAddOnValuesByName(UniversalObjectFactory factory, ZGuid parentPK, ZString name)
		{
			var query = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, parentPK);
			query.AddToFilter(GenCustomAddOnValueSchema.XV_Name, name);
			return factory.Load<GenCustomAddOnValue>(query);
		}

		#endregion

		#region Load List Matching

		public static void RemovePackagesFromLoadList(IEnumerable<WhsItemPackageState> removedPackages, WhsItemDispatchLoadList loadList, UniversalShipment dataObject, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			if (removedPackages.Any())
			{
				logger.Log(LogType.Information, Res.GetString("0a8dda70-c203-45e9-b5bb-f8dbae7345a3", "Packages were found on Load List {0} but were either assigned to another Load List or were not included in the UXML.", loadList.GetValue(WhsItemDispatchLoadListSchema.WDL_JobID)));

				var isLoadListStarted = loadList.WDL_IsReadyToStage;
				if (isLoadListStarted)
				{
					logger.Log(LogType.Warning, Res.GetString("67d571e0-e528-4701-b48d-483f6303f5f0", "Stopping Load List {0} as some of its packages were removed.", loadList.GetValue(WhsItemDispatchLoadListSchema.WDL_JobID)));
					LoadListController.StopLoadList(factory, logger, loadList, dataObject);
				}
			}

			foreach (var removedPackage in removedPackages)
			{
				var packageStatus = removedPackage.GetValue(WhsItemPackageStateSchema.WPS_Status);
				if (packageStatus.ToString().IsPackageDeparted())
				{
					throw new DataObjectReadFailureException(Res.GetString("a1a4f3a1-8643-42d1-b747-37c0cba7fd24", "At least one package is departed already. Therefore cannot be removed from load list."));
				}
				new WhsItemPackageStateDataObjectReader(dataObject, logger, factory, removedPackage, null, WhsTransitPackageStatePopulateStrategy.Detach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, loadList).ReadIntoBusinessObject();
			}
		}

		public static IEnumerable<WhsItemDispatchLoadList> FindMatchingLoadLists(BusinessObjectFactory factory, ZString consolNumber, ZString masterBill, IEnumerable<ZGuid> warehousePKs, IEnumerable<ZGuid> alreadyMatchedLoadListPKs, IXmlImportLogger logger, bool ignoreTransportMode = true)
		{
			var matchingLoadLists = Enumerable.Empty<WhsItemDispatchLoadList>();
			var loadListLogHelper = new DispatchLoadListTransitLogHelper();
			if (!consolNumber.IsEmpty)
			{
				var loadListQuery = GetLoadListsMatchingAdditionalReferenceQuery(consolNumber, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, warehousePKs, alreadyMatchedLoadListPKs, ignoreTransportMode);
				matchingLoadLists = factory.Load<WhsItemDispatchLoadList>(loadListQuery);
				if (matchingLoadLists.IsNullOrEmpty())
				{
					var loadListQueryByReferenceNumber = GetLoadListsMatchingReferenceNumberQuery(consolNumber, warehousePKs, alreadyMatchedLoadListPKs, ignoreTransportMode);
					matchingLoadLists = factory.Load<WhsItemDispatchLoadList>(loadListQueryByReferenceNumber);
				}
				if (matchingLoadLists.Any())
				{
					logger.Log(LogType.Information, loadListLogHelper.GetTable(Res.GetString("e15d8bac-44eb-4bba-a357-29169716e80c", "The following load list(s) matching consol number '{0}'.", consolNumber), matchingLoadLists, TransitLogColumnIDs.LoadListColumn.LoadList));
				}
			}
			else if (!masterBill.IsEmpty)
			{
				var loadListQuery = GetLoadListsMatchingAdditionalReferenceQuery(masterBill, WarehouseAdditionalReferenceTypes.Codes.MasterBill, warehousePKs, alreadyMatchedLoadListPKs, ignoreTransportMode);
				matchingLoadLists = factory.Load<WhsItemDispatchLoadList>(loadListQuery);
				if (matchingLoadLists.Any())
				{
					logger.Log(LogType.Information, loadListLogHelper.GetTable(Res.GetString("75a39d8b-c382-4799-b638-b4b07743dbc7", "The following load list(s) matching master bill number '{0}'.", masterBill), matchingLoadLists, TransitLogColumnIDs.LoadListColumn.LoadList));
				}
			}

			return matchingLoadLists;
		}

		public static ZDBOnlyQuery GetLoadListsMatchingReferenceNumberQuery(ZString consolNumber, IEnumerable<ZGuid> warehousePKs, IEnumerable<ZGuid> alreadyMatchedLoadListPKs, bool ignoreTransportMode = true)
		{
			var loadListQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadList));
			loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, warehousePKs);
			loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_IsActive, true);
			if (!ignoreTransportMode)
			{
				loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_TransportMode, TransportModes.Air);
			}

			loadListQuery.AddToFilter(JoinCondition.And, WhsItemDispatchLoadListSchema.WDL_ReferenceNumber, consolNumber);

			loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.PK, SQLComparisonOperator.NotEqual, alreadyMatchedLoadListPKs);

			return loadListQuery;
		}

		public static ZDBOnlyQuery GetLoadListsMatchingAdditionalReferenceQuery(ZString referenceNumber, ZString referenceType, IEnumerable<ZGuid> warehousePKs, IEnumerable<ZGuid> alreadyMatchedLoadListPKs, bool ignoreTransportMode = true)
		{
			var loadListQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchLoadList));
			loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_WW_Warehouse, warehousePKs);
			loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_IsActive, true);
			if (!ignoreTransportMode)
			{
				loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.WDL_TransportMode, TransportModes.Air);
			}
			var additionalReferencesQuery = GetAdditionalReferenceSubQuery(referenceNumber, referenceType);
			loadListQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);
			loadListQuery.AddToFilter(WhsItemDispatchLoadListSchema.PK, SQLComparisonOperator.NotEqual, alreadyMatchedLoadListPKs);

			return loadListQuery;
		}

		public static ZDBOnlySubQuery GetAdditionalReferenceSubQuery(ZString referenceNumber, ZString referenceType)
		{
			var additionalReferencesQuery = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);

			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, referenceType);
			additionalReferencesQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, referenceNumber);

			if (referenceType == WarehouseAdditionalReferenceTypes.Codes.MasterBill)
			{
				var formattedMABNumber = referenceNumber.Replace("-", "");
				additionalReferencesQuery.AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, formattedMABNumber);
			}
			return additionalReferencesQuery;
		}

		public static ZDBOnlyQuery GetASNsMatchingAdditionalReferenceQuery(ZString referenceNumber, ZString referenceType, IEnumerable<ZGuid> warehousePKs)
		{
			var asnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));
			asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, warehousePKs);
			asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_CompleteTime, null);
			var additionalReferencesQuery = GetAdditionalReferenceSubQuery(referenceNumber, referenceType);
			asnQuery.AddSubQuery(additionalReferencesQuery, JoinCondition.And);

			return asnQuery;
		}

		public static WhsItemDispatchTransportationUnit[] GetLoadCompleteDTUs(IEnumerable<IColumnIndexer> packageStates, UniversalObjectFactory factory)
		{
			var dtuPKs = packageStates.Where(p => p.GetValue(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader).IsValid && p.GetValue(WhsItemPackageStateSchema.WPS_Status) == TransitWarehouseStatuses.Codes.FreightLoaded)
				.Select(p => p.GetValue(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader))
				.Distinct();

			var loadCompleteDTURows = Array.Empty<WhsItemDispatchTransportationUnit>();

			if (dtuPKs.Any())
			{
				var loadCompleteDTUQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchTransportationUnit));
				loadCompleteDTUQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.PK, dtuPKs);
				loadCompleteDTUQuery.AddToFilter(WhsItemDispatchTransportationUnitSchema.WDH_LoadCompleteTime, SQLComparisonOperator.NotEqual, ZDateTimeOffset.Empty);
				loadCompleteDTURows = factory.BOFactory.Load<WhsItemDispatchTransportationUnit>(loadCompleteDTUQuery).ToArray();
			}

			return loadCompleteDTURows;
		}

		public static IEnumerable<IColumnIndexer> GetDCNsNotAuthorizedByPackagesOnLoadList(IEnumerable<IColumnIndexer> loadListPackages, UniversalObjectFactory factory)
		{
			var dcnNotAuthorizedQuery = new ZDBOnlyQuery(typeof(WhsItemDispatchConsignment));
			dcnNotAuthorizedQuery.AddToFilter(WhsItemDispatchConsignmentSchema.PK, loadListPackages.Select(p => p.GetValue(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment)).Distinct());
			dcnNotAuthorizedQuery.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_IsAuthorizedForDispatch, false);
			var dcnsNotAuthorized = factory.RowFactory.Load(WhsItemDispatchConsignmentSchema.Constants.TableName, dcnNotAuthorizedQuery).Select(d => DataObjectReader.GetColumnIndexerFromRow(d));

			return dcnsNotAuthorized;
		}

		#endregion

		#region TransportMode

		public static ZString GetTransportModeForShipmentLevel(this UniversalShipment dataObject)
		{
			var transportMode = dataObject.TransportMode?.GetCodeAsUpperCase() ?? ZString.Empty;
			if (transportMode == TransportModes.Air ||
				transportMode == TransportModes.Sea ||
				transportMode == TransportModes.Courier ||
				transportMode == TransportModes.Road ||
				transportMode == TransportModes.Rail ||
				transportMode == TransportModes.AirSea ||
				transportMode == TransportModes.SeaAir)
			{
				return transportMode;
			}
			return ZString.Empty;
		}

		public static ZString GetTransportModeForConsolLevel(this UniversalShipment dataObject)
		{
			var transportMode = dataObject.TransportMode?.GetCodeAsUpperCase() ?? ZString.Empty;
			if (transportMode == TransportModes.Air ||
				transportMode == TransportModes.Sea ||
				transportMode == TransportModes.Road ||
				transportMode == TransportModes.Rail)
			{
				return transportMode;
			}
			return ZString.Empty;
		}

		public static void PopulateTransportMode(this UniversalShipment dataObject, ZString transportMode)
		{
			if (!string.IsNullOrEmpty(transportMode))
			{
				dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportMode, new ZArchitecture.Core.CodeDescriptionPairList(ZArchitecture.Core.OLookUpEditType.LocalCartageTransportModes));
			}
		}

		#endregion

		#region Outbound Tranport Leg

		public static TransportLeg GetOutboundTransportLeg(UniversalObjectFactory factory, WhsWarehouse warehouse, UniversalShipment consolDO, UniversalShipment sourceDO)
		{
			return consolDO != null ? GetOutboundTransportLeg(factory, consolDO, warehouse) : GetOutboundTransportLeg(factory, sourceDO, warehouse);
		}

		static TransportLeg GetOutboundTransportLeg(UniversalObjectFactory factory, UniversalShipment uShipment, WhsWarehouse warehouse)
		{
			var homePortUNLOCO = warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var result = uShipment?.TransportLegCollection?.FirstOrDefault(d => d.PortOfLoading.GetCodeAsUpperCase() == homePortUNLOCO);

			if (result == null)
			{
				var legCollection = uShipment?.TransportLegCollection;
				if (legCollection != null && legCollection.Count > 0)
				{
					result = legCollection.OrderBy(l => l.PortOfLoading.GetCodeAsUpperCase())
						.FirstOrDefault(
							d => warehouse.RelatedCompanyBranch.ExtraPorts.Cast<GlbBranchExtraPorts>()
							.Any(p => p.GY_RL_NKAdditionalBranchRelatedPort == d.PortOfLoading.GetCodeAsUpperCase())
						);
				}
			}

			return result;
		}

		#endregion

		#region Inbound Transport Leg

		public static TransportLeg GetInboundTransportLeg(UniversalObjectFactory factory, WhsWarehouse warehouse, UniversalShipment consolDO, UniversalShipment sourceDO)
		{
			return consolDO != null ? GetInboundTransportLeg(factory, consolDO, warehouse) : GetInboundTransportLeg(factory, sourceDO, warehouse);
		}

		static TransportLeg GetInboundTransportLeg(UniversalObjectFactory factory, UniversalShipment uShipment, WhsWarehouse warehouse)
		{
			var homePortUNLOCO = warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var result = uShipment?.TransportLegCollection?.FirstOrDefault(d => d.PortOfDischarge.GetCodeAsUpperCase() == homePortUNLOCO);

			if (result == null)
			{
				var legCollection = uShipment?.TransportLegCollection;
				if (legCollection != null && legCollection.Count > 0)
				{
					result = legCollection.OrderBy(l => l.PortOfDischarge.GetCodeAsUpperCase())
						.FirstOrDefault(
							d => warehouse.RelatedCompanyBranch.ExtraPorts.Cast<GlbBranchExtraPorts>()
							.Any(p => p.GY_RL_NKAdditionalBranchRelatedPort == d.PortOfDischarge.GetCodeAsUpperCase())
						);
				}
			}

			return result;
		}

		#endregion

		#region ExecuteUpdatePackageStateSecurityStatusProcedure

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void ExecuteUpdatePackageStateSecurityStatusProcedure(WhsWarehouse warehouse, IEnumerable<Guid> packageStatePKs)
		{
			if (warehouse != null && packageStatePKs.Any() && warehouse.WW_GB_RelatedCompanyBranch.IsValid)
			{
				var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
				var staffCode = Convert.ToString(((IGlbStaff)Env.CurrentUser).GS_Code);
				var registryValue = WarehouseDataRegistry.Instance.MandatoryPackageScreeningForAirAndUnknownTransportMode.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty);
				var securityProcessingRequired = Convert.ToBoolean(warehouse.WW_TransitSecurityProcessingRequired);
				try
				{
					using (var manager = Db.Connection.BeginTransactionWithManager())
					using (var command = Db.Connection.Command("dbo.UpdatePackageStateSecurityStatus"))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.AddParameter("@CompanyBranchPK", SqlDbType.UniqueIdentifier, branchPK);
						command.AddParameter("@SystemLastEditUser", SqlDbType.Char, staffCode);
						command.AddParameter("@RegistryValue", SqlDbType.Bit, registryValue);
						command.AddParameter("@WarehouseConfigurationValue", SqlDbType.Bit, securityProcessingRequired);
						command.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", packageStatePKs);

						command.ExecuteNonQuery();
						manager.CommitTransaction();
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		#endregion

		#region ExecuteUpdatePackageStateAndRCNCustomStatusProcedure

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void ExecuteUpdatePackageStateAndRCNCustomStatusProcedure(WhsWarehouse warehouse, IEnumerable<Guid> packageStatePKs)
		{
			if (warehouse != null && warehouse.WW_GB_RelatedCompanyBranch.IsValid)
			{
				var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
				var staffCode = Convert.ToString(((IGlbStaff)Env.CurrentUser).GS_Code);
				var registryValue = WarehouseDataRegistry.Instance.ApplyPackageQuantityCountingAlgorithmForCustomsStatusCalculation.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty);
				try
				{
					using (var manager = Db.Connection.BeginTransactionWithManager())
					using (var command = Db.Connection.Command("dbo.UpdatePackageStateAndRCNCustomStatus"))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.AddParameter("@CompanyBranchPK", SqlDbType.UniqueIdentifier, branchPK);
						command.AddParameter("@SystemLastEditUser", SqlDbType.Char, staffCode);
						command.AddParameter("@CurrentUTC", SqlDbType.DateTime, DBNull.Value);
						command.AddParameter("@WarehouseConfigCustomControlled", SqlDbType.Bit, Convert.ToBoolean(warehouse.WW_IsCustomsControlled));
						command.AddParameter("@WarehouseConfigPortControlled", SqlDbType.Bit, Convert.ToBoolean(warehouse.WW_IsPortAuthorityControlled));
						command.AddParameter("@IsApplyPackageQuantityCountingAlgorithm", SqlDbType.Bit, registryValue);
						command.AddTableValuedParameter("@PackageStatePKs", "dbo.TVP_uniqueidentifier", packageStatePKs);
						command.ExecuteNonQuery();
						manager.CommitTransaction();
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		#endregion

		#region Gate Integration

		public static IColumnIndexer[] GetRelatedPackageStates(UniversalObjectFactory factory, SchemaGuidColumn colToMatch, ZGuid[] colPKsToMatch)
		{
			var pkgStateQuery = new ZQuery();
			pkgStateQuery.AddToFilter(colToMatch, colPKsToMatch);
			return Array.ConvertAll(factory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName, pkgStateQuery), DataObjectReader.GetColumnIndexerFromRow);
		}

		#endregion
	}
}
