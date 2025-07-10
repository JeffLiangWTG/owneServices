using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using static Enterprise.MasterFiles.Business.RefZoneHeaderLookups;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListHelper
	{
		public HVLVOriginLoadListHelper(ILogger logger)
		{
			this.logger = logger;
		}

		readonly ILogger logger;

		public static ZString GetPortUNLOCO(OrgAddress address)
		{
			if (address != null)
			{
				return !address.OA_RL_NKRelatedPortCode.IsEmpty ? address.OA_RL_NKRelatedPortCode : address.Header.OH_RL_NKClosestPort;
			}

			return ZString.Empty;
		}

		public static ZString GetPortUNLOCOFromConsignment(HVLVConsignment consignment, HVLVOriginLoadList loadList)
		{
			var result = string.Empty;

			var query = new ZDBOnlyQuery(typeof(RefUNLOCO));
			query.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, consignment.DestinationCountry);

			var zoneHeaderQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.PK);
			zoneHeaderQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, ZoneTypeCodes.HVLVGateway);
			zoneHeaderQuery.AddToFilter(RefZoneHeaderSchema.FZ_OH_RelatedParty, loadList.DestinationDepot?.Header?.PK);

			var zonePivotQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_ParentID);
			zonePivotQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefUNLOCOSchema.Constants.Prefix);

			zonePivotQuery.AddSubQuery(RefZonePivotSchema.F2_FZ, RefZoneHeaderSchema.PK, zoneHeaderQuery, JoinCondition.And);
			query.AddSubQuery(zonePivotQuery, JoinCondition.And);

			query.MaximumRows = 2;
			var unlocos = loadList.Factory.Load<RefUNLOCO>(query);

			if (unlocos.Length == 1)
			{
				result = unlocos.First().Code;
			}

			return result;
		}

		#region Create Consol

		public IForwardingConsol CreateConsol(IHVLVOriginLoadList loadList)
		{
			logger.Log(LogType.Debug, string.Format("No matching consol found, trying to create a new consol for loadlist {0}...", loadList.HVL_UniqueReference));

			var originLoadList = (HVLVOriginLoadList)loadList;

			var consol = originLoadList.Factory.New<ForwardingConsol>();

			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = originLoadList.HVL_TransportMode;
			consol.JK_ConsolMode = GetConsolContainerMode(originLoadList);
			consol.JK_RL_NKLoadPort = originLoadList.HVL_RL_NKOrigin;
			consol.JK_RL_NKDischargePort = originLoadList.HVL_RL_NKDestination;
			consol.JK_MasterBillNum = originLoadList.HVL_MasterBillNumber;
			consol.JK_OA_PackDepotAddress = originLoadList.HVL_OA_OriginDepot;
			consol.JK_OA_UnpackDepotAddress = originLoadList.HVL_OA_DestinationDepot;
			consol.JK_IsNeutralMaster = originLoadList.HVL_IsNeutralMaster;

			if (originLoadList.Carrier != null)
			{
				consol.SetDefaultShippingLineAddress(originLoadList.Carrier);
			}

			var transport = consol.Transports.Cast<Transport>().Single();

			transport.JW_RL_NKLoadPort = originLoadList.HVL_RL_NKOrigin;
			transport.JW_RL_NKDiscPort = originLoadList.HVL_RL_NKDestination;
			transport.JW_ETD = originLoadList.HVL_E_Dep;
			transport.JW_ETA = originLoadList.HVL_E_Arv;
			transport.JW_Vessel = originLoadList.HVL_VesselName;
			transport.JW_VoyageFlight = originLoadList.HVL_VoyageFlight;

			logger.Log(LogType.Debug, string.Format("Successfully created a consol for loadlist {0}...", loadList.HVL_UniqueReference));

			return consol;
		}

		public static string GetConsolContainerMode(HVLVOriginLoadList originLoadList)
		{
			switch (originLoadList.HVL_TransportMode)
			{
				case TransportModes.Sea:
					return ContainerModes.Groupage;
				case TransportModes.Air:
					return HasContainerSpecified(originLoadList) ? ContainerModes.ULD : ContainerModes.Loose;
				case TransportModes.Road:
					return ContainerModes.LTL;
				case TransportModes.Rail:
					return ContainerModes.LCL;
			}

			return string.Empty;
		}

		#endregion

		#region Attach to Consol

		public bool TryAttachToConsol(IForwardingConsol consol, IEnumerable<IHVLVOriginLoadList> loadLists, out string errorMessage)
		{
			var success = true;
			errorMessage = string.Empty;
			var forwardingConsol = (ForwardingConsol)consol;

			var consignmentComparer = new HVLVConsignmentComparerForShipmentCreation();
			var headerAndItemsLookupMaster = new Dictionary<HVLVConsignment, HeaderAndItems>(consignmentComparer);

			var bookingComparer = new BookingHeaderComparerForShipmentCreation();
			var headerAndItemsLookup = new Dictionary<HVLVBookingHeader, HeaderAndItems>(bookingComparer);

			ForwardingShipment masterShipment = null;

			var factory = forwardingConsol.Factory;

			foreach (HVLVOriginLoadList loadList in loadLists)
			{
				ForwardingContainer container = null;
				loadList.SetLoadedOnConsolForOuterPackages(forwardingConsol);

				if (HasContainerSpecified(loadList))
				{
					container = PopulateContainerDetails(loadList, forwardingConsol);
				}

				SetBookingHeadersIsProcessedAtOriginDepot(loadList);

				if (loadList.HVL_IsMasterHouse)
				{
					success = TryProcessMasterLoadList(container, forwardingConsol, loadList, consignmentComparer, headerAndItemsLookupMaster, out errorMessage, out masterShipment);
				}
				else
				{
					success = TryProcessLoadList(container, forwardingConsol, loadList, bookingComparer, headerAndItemsLookup, out errorMessage);
				}

				if (!success)
				{
					return success;
				}

				loadList.HVL_Status = ELoadListStatuses.Consolidated;

				using (Logs.DeferFiringWorkflow(true))
				{
					var eventParameters = new[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, loadList.HVL_UniqueReference)
					};

					forwardingConsol.Logs.CreateOrRecreateEventLog(AutoEvents.ELoadListConsolidated, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, eventParameters);
				}

				PopulateMasterBillNumberForNeutralMasterLoadListService.AddPopulateMasterBillNumberForNeutralMasterLoadListServiceIfRequired(forwardingConsol, loadList);
			}

			foreach (var shipmentAndItems in headerAndItemsLookup)
			{
				PopulateShipmentAndPackingDetails(shipmentAndItems, forwardingConsol, loadLists.Cast<HVLVOriginLoadList>().First());
			}

			var countriesWithoutMatchedDestinationDepot = new List<string>();
			foreach (var shipmentAndItems in headerAndItemsLookupMaster)
			{
				PopulateShipmentAndPackingDetailsForMaster(shipmentAndItems, forwardingConsol, loadLists.Cast<HVLVOriginLoadList>().First());

				if (shipmentAndItems.Value.Header.Shipment.JS_RL_NKDestination.IsEmpty)
				{
					countriesWithoutMatchedDestinationDepot.Add(shipmentAndItems.Key.DestinationCountry);
				}
			}

			if (countriesWithoutMatchedDestinationDepot.Any())
			{
				logger.Warning(Res.GetString("28015a6b-ae69-4be1-adf5-5838eac0f28d",
					"Cannot determine suitable Destination Port for HVL Shipment(s) on Consol '{0}'. There are zero or multiple UNLOCOs for country(s) '{1}'.",
					consol.JK_MasterBillNum, string.Join(",", countriesWithoutMatchedDestinationDepot)));
			}

			#region Factory Saving/Saved Hooks

			void AddTransferedLogHook(BusinessObjectFactory f)
			{
				if (forwardingConsol != null)
				{
					forwardingConsol.PopulateJK_UniqueConsignRefIfNeeded();
					var eventParameters = new[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode, forwardingConsol.JK_ConsolMode)
					};

					foreach (var loadList in loadLists)
					{
						loadList.Logs.CreateOrRecreateEventLog(AutoEvents.Transferred, EstimateActual.Actual, ZDateTimeOffset.Now, $"|RFN={forwardingConsol.JK_UniqueConsignRef}", eventParameters);
					}
				}
			}

			void PopulateBillAndShipmentNumberIfNeeded(BusinessObjectFactory f)
			{
				masterShipment?.PopulateBillAndShipmentNumberIfNeeded();
			}

			void AddSavingHooks(BusinessObjectFactory f)
			{
				AddTransferedLogHook(f);
				PopulateBillAndShipmentNumberIfNeeded(f);
			}

			void RemoveSavingHooks(BusinessObjectFactory f, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					f.Saving -= AddTransferedLogHook;
					f.Saving -= PopulateBillAndShipmentNumberIfNeeded;
					f.Saved -= RemoveSavingHooks;
				}
			}

			#endregion

			factory.Saving -= AddSavingHooks;
			factory.Saving += AddSavingHooks;
			factory.Saved -= RemoveSavingHooks;
			factory.Saved += RemoveSavingHooks;

			return success;
		}

		#region Master Loadlist

		bool TryProcessMasterLoadList(ForwardingContainer container,
			ForwardingConsol forwardingConsol,
			HVLVOriginLoadList loadList,
			HVLVConsignmentComparerForShipmentCreation consignmentComparer,
			Dictionary<HVLVConsignment, HeaderAndItems> headerAndItemsLookupMaster,
			out string errorMessage,
			out ForwardingShipment masterShipment)
		{
			logger.Log(LogType.Debug, string.Format("Trying to process master loadlist {0}", loadList.HVL_UniqueReference));

			var success = true;
			errorMessage = string.Empty;
			masterShipment = null;
			var loadlistConsignments = GetLoadListConsignments(loadList);
			var consignmentGroups = loadlistConsignments.GroupBy(x => x, consignmentComparer);
			success = TryPopulateMasterShipmentForMasterHouseLoadList(loadList, forwardingConsol, out errorMessage, out masterShipment);
			if (!success)
			{
				return success;
			}

			foreach (var consignmentGroup in consignmentGroups)
			{
				if (!TryCreateContainerItemGroupsForMaster(container, loadList, consignmentGroup, forwardingConsol, masterShipment, headerAndItemsLookupMaster, out var repeatedWaybillNumbersAndHeaders, out var repeatedShipperReferencesAndHeaders))
				{
					success = false;
					errorMessage = GenerateErrorMessageForRepeatedReferences(repeatedWaybillNumbersAndHeaders, repeatedShipperReferencesAndHeaders, loadList);
					logger.Error(errorMessage);
					return success;
				}
			}

			logger.Log(LogType.Debug, string.Format("Finishing processing master loadlist {0}", loadList.HVL_UniqueReference));

			return success;
		}

		HVLVConsignment[] GetLoadListConsignments(HVLVOriginLoadList loadList)
		{
			var itemsSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_HVC_Consignment);
			itemsSubQuery.AddToFilter(HVLVItemSchema.HVI_HVL_LoadList, loadList.PK);

			var query = new ZDBOnlyQuery(typeof(HVLVConsignment));
			query.AddSubQuery(HVLVConsignmentSchema.PK, itemsSubQuery, JoinCondition.And);

			return loadList.Factory.Load<HVLVConsignment>(query);
		}

		class HVLVConsignmentComparerForShipmentCreation : IEqualityComparer<HVLVConsignment>
		{
			bool IEqualityComparer<HVLVConsignment>.Equals(HVLVConsignment x, HVLVConsignment y)
			{
				return x.BookingHeader.BillToParty?.PK == y.BookingHeader.BillToParty?.PK &&
						x.DestinationCountry == y.DestinationCountry &&
						x.BookingHeader.HVH_RS_NKBookingServiceLevel == y.BookingHeader.HVH_RS_NKBookingServiceLevel;
			}

			int IEqualityComparer<HVLVConsignment>.GetHashCode(HVLVConsignment consignment)
			{
				return consignment.BookingHeader.BillToParty?.PK != null
					? consignment.BookingHeader.BillToParty.PK.GetHashCode() ^ consignment.BookingHeader.HVH_RS_NKBookingServiceLevel.GetHashCode() ^ consignment.DestinationCountry.GetHashCode()
					: consignment.BookingHeader.HVH_RS_NKBookingServiceLevel.GetHashCode() ^ consignment.DestinationCountry.GetHashCode();
			}
		}

		HVLVItem[] GetHVLVItems(HVLVOriginLoadList loadList, IEnumerable<HVLVConsignment> consignments)
		{
			var outputShipment = new StringBuilder(string.Format((NoResString)"Trying to get HVLV items in loadlist {0}", loadList.HVL_UniqueReference));
			var query = new ZQuery(HVLVItemSchema.HVI_HVL_LoadList, loadList.PK);
			query.AddToFilter(HVLVItemSchema.HVI_HVC_Consignment, consignments.Select(x => x.PK));
			return loadList.Factory.Load<HVLVItem>(query);
		}

		bool TryCreateContainerItemGroupsForMaster(ForwardingContainer container,
			HVLVOriginLoadList loadList,
			IGrouping<HVLVConsignment, HVLVConsignment> consignmentGroup,
			ForwardingConsol consol,
			ForwardingShipment masterShipment,
			Dictionary<HVLVConsignment, HeaderAndItems> headerAndItemsLookupMaster,
			out List<ZString[]> repeatedWaybillNumbersAndHeaders,
			out List<ZString[]> repeatedShipperReferencesAndHeaders
		)
		{
			logger.Log(LogType.Debug, string.Format("Trying to create container item groups for loadlist {0}...", loadList.HVL_UniqueReference));
			var success = true;
			var groupKey = consignmentGroup.Key;
			var hvlvShipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault(shipment =>
				shipment.IsHighVolumeLowValue &&
				shipment.CoLoadMasterShipment == masterShipment &&
				shipment.Destination?.Country?.Code == groupKey?.DestinationCountry &&
				shipment.ConsignorDocumentaryAddress?.E2_OA_Address == groupKey.BookingHeader.HVH_OA_BillToParty &&
				shipment.JS_RS_NKServiceLevel == groupKey.BookingHeader.HVH_RS_NKBookingServiceLevel);

			var outputShipment = new StringBuilder((NoResString)"Trying to find an existing HVL shipment on the consol having");
			if (!masterShipment.JS_UniqueConsignRef.IsEmpty)
			{
				outputShipment.Append(string.Format((NoResString)" coload master shipment is {0},", masterShipment.JS_UniqueConsignRef.ToString()));
			}

			if (groupKey?.DestinationCountry.Length > 0)
			{
				outputShipment.Append(string.Format((NoResString)" destination is {0},", groupKey?.DestinationCountry));
			}

			outputShipment.Append(string.Format((NoResString)" consignor address is {0}, service level is {1}", groupKey.BookingHeader.HVH_OA_BillToParty_ZAddress.Address, groupKey.BookingHeader.HVH_RS_NKBookingServiceLevel));
			logger.Log(LogType.Debug, outputShipment.ToString());

			if (hvlvShipment == null)
			{
				logger.Log(LogType.Debug, "No matching HVL shipment found, trying to create a new HVL shipment for the consol");
				hvlvShipment = loadList.Factory.New<ForwardingShipment>();
				hvlvShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			}
			else
			{
				logger.Log(LogType.Debug, string.Format("Successfully found a matching HVL shipment {0} on the consol {1}", hvlvShipment.JS_UniqueConsignRef, consol.JK_UniqueConsignRef));
			}

			if (!headerAndItemsLookupMaster.TryGetValue(groupKey, out var headerAndItems))
			{
				var consignmentHeader = hvlvShipment.GetOrCreateHVLVConsignmentHeader();
				consignmentHeader.Logs.CreateOrRecreateEventLog(AutoEvents.ELoadListConsolidated, EstimateActual.Actual, ZDateTimeOffset.Now);

				headerAndItems = new HeaderAndItems(consignmentHeader);
				headerAndItemsLookupMaster.Add(groupKey, headerAndItems);
			}

			var items = GetHVLVItems(loadList, consignmentGroup);
			logger.Log(LogType.Debug, string.Format("Finishing getting HVLV items in loadlist {0}", loadList.HVL_UniqueReference));
			headerAndItems.ContainerItemGroups.Add(Tuple.Create(container, items));

			repeatedWaybillNumbersAndHeaders = GetRepeatedWaybillNumbersAndBookingHeaderReferences(items, hvlvShipment);
			repeatedShipperReferencesAndHeaders = GetRepeatedShipperReferencesAndBookingHeaderReferences(items, hvlvShipment);

			if (repeatedWaybillNumbersAndHeaders.Count > 0 || repeatedShipperReferencesAndHeaders.Count > 0)
			{
				success = false;
			}

			logger.Log(LogType.Debug, string.Format("Finishing creating container item groups for loadlist {0}...", loadList.HVL_UniqueReference));
			return success;
		}

		void PopulateShipmentAndPackingDetailsForMaster(KeyValuePair<HVLVConsignment, HeaderAndItems> headerAndItems, ForwardingConsol consol, HVLVOriginLoadList loadList)
		{
			var consignment = headerAndItems.Key;
			var consignmentHeader = headerAndItems.Value.Header;
			var shipment = consignmentHeader.Shipment;
			var containerItemGroups = headerAndItems.Value.ContainerItemGroups;

			PopulateShipmentDetails(shipment, consol, consignment.BookingHeader, loadList, containerItemGroups.SelectMany(x => x.Item2));
			logger.Log(LogType.Debug, string.Format("Trying to get Port UNLOCO from the destionation country of loadlist {0}", loadList.HVL_UniqueReference));
			shipment.JS_RL_NKDestination = GetPortUNLOCOFromConsignment(consignment, loadList);
			logger.Log(LogType.Debug, string.Format("Finishing getting Port UNLOCO from the destionation country of loadlist {0}", loadList.HVL_UniqueReference));

			consol.Shipments.Add(shipment);

			if (!shipment.IsInDatabase)
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
			}

			foreach (var items in containerItemGroups)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				PopulatePackingDetails(packLine, consignmentHeader, items.Item1, items.Item2);
			}

			shipment.UpdateShipmentFromOuterPackLines();
		}

		bool TryPopulateMasterShipmentForMasterHouseLoadList(HVLVOriginLoadList loadList, ForwardingConsol consol, out string errorMessage, out ForwardingShipment masterShipment)
		{
			errorMessage = string.Empty;
			masterShipment = null;
			logger.Log(LogType.Debug, string.Format("Trying to find a matching master shipment for the master house loadlist {0}...", loadList.HVL_UniqueReference));
			if (loadList.HVL_HouseBillNumber.IsEmpty)
			{
				var matchedHVMShipments = consol.Shipments.OfType<ForwardingShipment>().Where(shipment =>
					shipment.IsHighVolumeLowValueMaster &&
					shipment.JS_RS_NKServiceLevel == loadList.HVL_RS_NKServiceLevel
				);
				if (matchedHVMShipments.Count() == 1)
				{
					masterShipment = matchedHVMShipments.First();
					logger.Log(LogType.Debug, string.Format("Successfully found a matching master shipment {0} for the master house loadlist {1}", masterShipment.JS_UniqueConsignRef, loadList.HVL_UniqueReference));
				}
				else if (matchedHVMShipments.Count() > 1)
				{
					errorMessage = Res.GetString("00dd6773-49d2-4970-a332-20c4762bd34c", "There are multiple HVM shipments on consol '{0}'. Load list '{1}' failed to be merged.", consol.JK_UniqueConsignRef, loadList.HVL_UniqueReference);
					logger.Error(errorMessage);
					return false;
				}
			}
			else
			{
				masterShipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault(shipment =>
					shipment.IsHighVolumeLowValueMaster &&
					shipment.JS_HouseBill == loadList.HVL_HouseBillNumber.SubstringSafe(0, 20) &&
					shipment.JS_RS_NKServiceLevel == loadList.HVL_RS_NKServiceLevel
				);
			}

			if (masterShipment == null)
			{
				masterShipment = consol.Shipments.AddNew();
				logger.Log(LogType.Debug, string.Format("No matching master shipment found, trying to create a new master shipment for loadlist {0}", consol.JK_UniqueConsignRef));
			}

			masterShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			masterShipment.ConsignorDocumentaryAddress.E2_OA_Address = loadList.HVL_OA_OriginDepot;
			masterShipment.ConsigneeDocumentaryAddress.E2_OA_Address = loadList.HVL_OA_DestinationDepot;
			masterShipment.JS_RL_NKOrigin = loadList.HVL_RL_NKOrigin;
			masterShipment.JS_RL_NKDestination = loadList.HVL_RL_NKDestination;
			masterShipment.JS_HouseBill = loadList.HVL_HouseBillNumber.SubstringSafe(0, 20);
			masterShipment.JS_RS_NKServiceLevel = loadList.HVL_RS_NKServiceLevel;
			masterShipment.JS_INCO = loadList.HVL_INCO;

			PopulatePackingDetailsOnMasterShipment(masterShipment, consol, loadList.OuterPackages);

			PopulateInnerPackagesOnMasterShipment(masterShipment, loadList.Items);
			masterShipment.JS_TotalPackageCount = masterShipment.JS_TotalPackageCount += loadList.Items.Count;

			if (masterShipment.OuterPackLines.Count > 0)
			{
				masterShipment.JS_GoodsDescription = ResString.GetMultilingualString("faa1efa8-bdeb-4e61-8c77-d271358b246f", "Various Cargo");
			}

			logger.Log(LogType.Debug, string.Format("Successfully populate the master shipment for the master house loadlist {0}...", loadList.HVL_UniqueReference));
			return true;
		}

		void PopulatePackingDetailsOnMasterShipment(ForwardingShipment masterShipment, ForwardingConsol consol, HVLVOuterPackagesInOriginLoadListCollection outerPackages)
		{
			logger.Log(LogType.Debug, "Trying to populate packing details on the master shipment");
			foreach (HVLVOuterPackage outerPackage in outerPackages)
			{
				var packLine = masterShipment.OuterPackLines.AddNew();
				var undgs = outerPackage.Items.SelectMany(item => ((HVLVItem)item).UNDGs);
				foreach (var undg in undgs)
				{
					var newundg = packLine.UNDGs.AddNew();
					var args = new BusinessObjectCloneArgs(new string[] { UNDGDataItemSchema.Constants.DI_ParentID, UNDGDataItemSchema.Constants.DI_ParentTableCode });
					newundg.CopyPersistentValuesFrom(undg, args);
				}

				var container = consol.Containers.FindAnyByContainerNumber(outerPackage.HVO_ContainerNumber);
				if (container != null)
				{
					packLine.JL_JC = container.PK;
				}

				packLine.JL_ActualWeight = outerPackage.HVO_Weight;
				packLine.JL_ActualWeightUQ = outerPackage.HVO_WeightUQ;
				packLine.JL_ActualVolume = outerPackage.HVO_Volume;
				packLine.JL_ActualVolumeUQ = outerPackage.HVO_VolumeUQ;

				packLine.JL_F3_NKPackType = outerPackage.HVO_F3_NKPackageType;
				if (packLine.JL_F3_NKPackType.IsEmpty)
				{
					packLine.JL_F3_NKPackType = PkgUnit.Package;
				}

				packLine.JL_Height = outerPackage.HVO_Height;
				packLine.JL_Length = outerPackage.HVO_Length;
				packLine.JL_Width = outerPackage.HVO_Width;
				packLine.JL_UnitOfDimension = outerPackage.HVO_UnitOfDimension;
				packLine.JL_RH_NKCommodityCode = outerPackage.HVO_RH_NKCommodityCode;
				packLine.JL_RefNumber = outerPackage.HVO_PackageBarcode.SubstringSafe(0, 46);
				packLine.JL_OA_LastKnownTransitWarehouseAddress = outerPackage.HVO_OA_DestinationDepot;
				packLine.JL_PackageCount = 1;
			}

			masterShipment.UpdateShipmentFromOuterPackLines();
			logger.Log(LogType.Debug, "Finishing populating packing details on the master shipment");
		}

		void PopulateInnerPackagesOnMasterShipment(ForwardingShipment masterShipment, HVLVItemInOriginLoadListCollection loadListItems)
		{
			logger.Log(LogType.Debug, "Trying to populate inner packages on the master shipment");
			var innerPackage = masterShipment.InnerPackLines.AddNew();

			var innerPackWeight = ZDecimal.Zero;
			var innerPackVolume = ZDecimal.Zero;
			foreach (HVLVItem item in loadListItems)
			{
				var packWeight = item.HVI_ActualWeight;
				var packVolume = item.HVI_ActualVolume;

				if (item.Consignment.HVC_WeightUQ != masterShipment.JS_UnitOfWeight)
				{
					packWeight = Weight.Convert(packWeight, item.Consignment.HVC_WeightUQ, masterShipment.JS_UnitOfWeight);
				}

				innerPackWeight += packWeight;

				if (item.Consignment.HVC_WeightUQ != masterShipment.JS_UnitOfVolume)
				{
					packVolume = Volume.Convert(packVolume, item.Consignment.HVC_VolumeUQ, masterShipment.JS_UnitOfVolume);
				}

				innerPackVolume += packVolume;
			}

			innerPackage.JL_PackageCount = loadListItems.Count;
			innerPackage.JL_F3_NKPackType = masterShipment.JS_F3_NKTotalCountPackType;

			innerPackage.JL_ActualWeight = innerPackWeight;
			innerPackage.JL_ActualWeightUQ = masterShipment.JS_UnitOfWeight;

			innerPackage.JL_ActualVolume = innerPackVolume;
			innerPackage.JL_ActualVolumeUQ = masterShipment.JS_UnitOfVolume;

			innerPackage.JL_LoadingMeters = masterShipment.JS_LoadingMeters;
			logger.Log(LogType.Debug, "Finishing populating inner packages on the master shipment");
		}

		#endregion

		#region HVLV LoadList

		bool TryProcessLoadList(ForwardingContainer container,
			ForwardingConsol forwardingConsol,
			HVLVOriginLoadList loadList,
			BookingHeaderComparerForShipmentCreation bookingComparer,
			Dictionary<HVLVBookingHeader, HeaderAndItems> headerAndItemsLookup,
			out string errorMessage)
		{
			logger.Log(LogType.Debug, string.Format("Trying to process loadList {0}", loadList.HVL_UniqueReference));
			var success = true;
			errorMessage = string.Empty;
			var loadListBookingHeaders = GetLoadListBookingHeaders(loadList);
			var bookingHeaderGroups = loadListBookingHeaders.GroupBy(x => x, bookingComparer);

			foreach (var bookingHeaderGroup in bookingHeaderGroups)
			{
				if (!TryCreateContainerItemGroups(container, loadList, bookingHeaderGroup, forwardingConsol, headerAndItemsLookup, out var repeatedWaybillNumbersAndHeaders, out var repeatedShipperReferencesAndHeaders))
				{
					success = false;
					errorMessage = GenerateErrorMessageForRepeatedReferences(repeatedWaybillNumbersAndHeaders, repeatedShipperReferencesAndHeaders, loadList);
					logger.Error(errorMessage);
					return success;
				}
			}

			logger.Log(LogType.Debug, string.Format("Finishing processing loadList {0}", loadList.HVL_UniqueReference));

			return success;
		}

		void SetBookingHeadersIsProcessedAtOriginDepot(HVLVOriginLoadList loadList)
		{
			var bookingHeaders = GetLoadListBookingHeaders(loadList);
			bookingHeaders.ForEach(x => x.HVH_IsProcessedAtOriginDepot = true);
		}

		HVLVBookingHeader[] GetLoadListBookingHeaders(HVLVOriginLoadList loadList)
		{
			var itemsSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_HVC_Consignment);
			itemsSubQuery.AddToFilter(HVLVItemSchema.HVI_HVL_LoadList, loadList.PK);

			var consignmentsSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.HVC_HVH_BookingHeader);
			consignmentsSubQuery.AddSubQuery(HVLVConsignmentSchema.PK, itemsSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(HVLVBookingHeader));
			query.AddSubQuery(HVLVBookingHeaderSchema.PK, consignmentsSubQuery, JoinCondition.And);

			return loadList.Factory.Load<HVLVBookingHeader>(query);
		}

		bool TryCreateContainerItemGroups(ForwardingContainer container,
			HVLVOriginLoadList loadList,
			IGrouping<HVLVBookingHeader, HVLVBookingHeader> bookingHeaderGroup,
			ForwardingConsol consol,
			Dictionary<HVLVBookingHeader, HeaderAndItems> headerAndItemsLookup,
			out List<ZString[]> repeatedWaybillNumbersAndHeaders,
			out List<ZString[]> repeatedShipperReferencesAndHeaders)
		{
			logger.Log(LogType.Debug, string.Format("Trying to create container item groups for loadList {0}", loadList.HVL_UniqueReference));
			var success = true;
			var groupKey = bookingHeaderGroup.Key;
			if (!headerAndItemsLookup.TryGetValue(groupKey, out var headerAndItems))
			{
				var hvlvShipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault(s =>
					s.IsHighVolumeLowValue
					&& s.ConsignorDocumentaryAddress?.E2_OA_Address == groupKey.BillToParty.PK
					&& s.JS_RS_NKServiceLevel == groupKey.HVH_RS_NKBookingServiceLevel);

				if (!groupKey.HVH_RS_NKBookingServiceLevel.IsEmpty)
				{
					logger.Log(LogType.Debug, string.Format("Trying to find matching HVL Shipments for creating container item groups when service level is {0} and consignor address is {1}", groupKey.HVH_RS_NKBookingServiceLevel, groupKey.HVH_OA_BillToParty_ZAddress.Address));
				}
				else
				{
					logger.Log(LogType.Debug, string.Format("Trying to find matching HVL Shipments for creating container item groups when consignor address is {0}", groupKey.HVH_OA_BillToParty_ZAddress.Address));
				}

				hvlvShipment = hvlvShipment ?? loadList.Factory.New<ForwardingShipment>();

				var consignmentHeader = hvlvShipment.GetOrCreateHVLVConsignmentHeader();
				consignmentHeader.Logs.CreateOrRecreateEventLog(AutoEvents.ELoadListConsolidated, EstimateActual.Actual, ZDateTimeOffset.Now);

				headerAndItems = new HeaderAndItems(consignmentHeader);
				headerAndItemsLookup.Add(bookingHeaderGroup.Key, headerAndItems);
			}

			var items = GetHVLVItems(loadList, bookingHeaderGroup);
			headerAndItems.ContainerItemGroups.Add(Tuple.Create(container, items));

			repeatedWaybillNumbersAndHeaders = GetRepeatedWaybillNumbersAndBookingHeaderReferences(items);
			repeatedShipperReferencesAndHeaders = GetRepeatedShipperReferencesAndBookingHeaderReferences(items);

			if (repeatedWaybillNumbersAndHeaders.Count > 0 || repeatedShipperReferencesAndHeaders.Count > 0)
			{
				success = false;
			}

			logger.Log(LogType.Debug, string.Format("Finishing creating container item groups for loadList {0}", loadList.HVL_UniqueReference));

			return success;
		}

		void PopulateShipmentAndPackingDetails(KeyValuePair<HVLVBookingHeader, HeaderAndItems> headerAndItems, ForwardingConsol consol, HVLVOriginLoadList loadList)
		{
			var bookingHeader = headerAndItems.Key;
			var consignmentHeader = headerAndItems.Value.Header;
			var shipment = consignmentHeader.Shipment;
			var containerItemGroups = headerAndItems.Value.ContainerItemGroups;

			PopulateShipmentDetails(shipment, consol, bookingHeader, loadList, containerItemGroups.SelectMany(x => x.Item2));
			shipment.JS_RL_NKDestination = GetPortUNLOCO(shipment.ConsigneeDocumentaryAddress.Address);

			consol.Shipments.Add(shipment);

			if (!shipment.IsInDatabase)
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
			}

			foreach (var items in containerItemGroups)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				PopulatePackingDetails(packLine, consignmentHeader, items.Item1, items.Item2);
			}

			shipment.UpdateShipmentFromOuterPackLines();
		}

		HVLVItem[] GetHVLVItems(HVLVOriginLoadList loadList, IEnumerable<HVLVBookingHeader> headers)
		{
			var headersSubQuery = new ZDBOnlySubQuery(typeof(HVLVBookingHeader), HVLVBookingHeaderSchema.PK);
			headersSubQuery.AddToFilter(HVLVBookingHeaderSchema.PK, headers.Select(x => x.PK));

			var consignmentsSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.PK);
			consignmentsSubQuery.AddSubQuery(HVLVConsignmentSchema.HVC_HVH_BookingHeader, headersSubQuery, JoinCondition.And);

			var itemsQuery = new ZDBOnlyQuery(typeof(HVLVItem));
			itemsQuery.AddToFilter(HVLVItemSchema.HVI_HVL_LoadList, loadList.PK);
			itemsQuery.AddSubQuery(HVLVItemSchema.HVI_HVC_Consignment, consignmentsSubQuery, JoinCondition.And);

			return loadList.Factory.Load<HVLVItem>(itemsQuery);
		}

		#endregion

		ForwardingContainer PopulateContainerDetails(HVLVOriginLoadList loadList, ForwardingConsol consol)
		{
			logger.Log(LogType.Debug, "Trying to populate details of a container for the consol");
			ForwardingContainer container = null;
			if (!consol.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == loadList.HVL_ContainerNumber))
			{
				container = consol.Containers.AddNew();
				container.JC_ContainerNum = loadList.HVL_ContainerNumber;
				container.JC_RC = loadList.HVL_RC_ContainerType;
				container.JC_ContainerMode = consol.JK_ConsolMode;

				if (loadList.HVL_TransportMode != TransportModes.Air)
				{
					container.JC_DeliveryMode = DeliveryModes.Codes.CFS_CFS;
				}
			}

			logger.Log(LogType.Debug, "Finishing populating details of a container for the consol");

			return container;
		}

		List<ZString[]> GetRepeatedWaybillNumbersAndBookingHeaderReferences(IEnumerable<HVLVItem> items, ForwardingShipment shipment = null)
		{
			if (shipment != null && shipment.IsInDatabase)
			{
				items = items.Concat(shipment.HVLVItems.OfType<HVLVItem>());
			}

			var result = items.Select(i => i.Consignment)
				.Where(c => !c.HVC_WaybillNumber.IsEmpty)
				.Distinct()
				.OrderBy(c => c.BookingHeader.HVH_BookingReference)
				.GroupBy(c => c.HVC_WaybillNumber)
				.Where(g => g.Count() > 1)
				.Select(y => new[] { y.Key, ZString.Join(", ", y.Select(c => c.BookingHeader.HVH_BookingReference).ToArray()) })
				.ToList();

			return result;
		}

		List<ZString[]> GetRepeatedShipperReferencesAndBookingHeaderReferences(IEnumerable<HVLVItem> items, ForwardingShipment shipment = null)
		{
			if (shipment != null && shipment.IsInDatabase)
			{
				items = items.Concat(shipment.HVLVItems.OfType<HVLVItem>());
			}

			var result = items.Select(i => i.Consignment)
				.Where(c => !c.HVC_ShipperReference.IsEmpty)
				.Distinct()
				.OrderBy(c => c.BookingHeader.HVH_BookingReference)
				.GroupBy(c => c.HVC_ShipperReference)
				.Where(g => g.Count() > 1)
				.Select(y => new[] { y.Key, ZString.Join(", ", y.Select(c => c.BookingHeader.HVH_BookingReference).ToArray()) })
				.ToList();

			return result;
		}

		void PopulateShipmentDetails(ForwardingShipment shipment, ForwardingConsol consol, HVLVBookingHeader header, HVLVOriginLoadList loadList, IEnumerable<HVLVItem> items)
		{
			logger.Log(LogType.Debug, string.Format("Trying to populate {0} shipment details", ShipmentTypes.HighVolumeLowValue));
			shipment.JS_TransportMode = consol.JK_TransportMode;
			shipment.JS_PackingMode = GetShipmentContainerMode(shipment.JS_TransportMode, loadList);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = header.HVH_OA_BillToParty;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = loadList.HVL_OA_DestinationDepot;
			shipment.JS_RL_NKOrigin = GetPortUNLOCO(shipment.ConsignorDocumentaryAddress.Address);
			shipment.JS_GoodsDescription = Res.GetString("faa1efa8-bdeb-4e61-8c77-d271358b246f", "Various Cargo");
			shipment.JS_RS_NKServiceLevel = header.HVH_RS_NKBookingServiceLevel;
			shipment.JS_UnitOfWeight = Env.Registry.FreightWeightUnit;
			shipment.JS_UnitOfVolume = Env.Registry.FreightVolumeUnit;

			var consignmentsByCurrency = items.GroupBy(x => x.HVI_HVC_Consignment).Select(x => shipment.Factory.Load<HVLVConsignment>(x.Key)).GroupBy(x => x.HVC_RX_NKGoodsValueCurrency).ToArray();

			if (consignmentsByCurrency.Length == 1)
			{
				shipment.JS_GoodsValue = consignmentsByCurrency[0].Sum(x => x.HVC_GoodsValue);
				shipment.JS_RX_NKGoodsValueCurr = consignmentsByCurrency[0].Key;
			}
			else
			{
				var localCurrency = GetShipmentCurrency(shipment);
				shipment.JS_RX_NKGoodsValueCurr = localCurrency.RX_Code;

				foreach (var consignmentGroup in consignmentsByCurrency)
				{
					var consignmentCurrency = shipment.Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, consignmentGroup.Key));

					if (consignmentCurrency != null)
					{
						var consignmentValue = consignmentGroup.Sum(x => x.HVC_GoodsValue);
						shipment.JS_GoodsValue += consignmentCurrency.ConvertUsingSellRate(ZDateTime.Now, consignmentValue, localCurrency);
					}
				}
			}

			logger.Log(LogType.Debug, string.Format("Finishing populating {0} shipment details", shipment.JS_ShipmentType));
		}

		RefCurrency GetShipmentCurrency(ForwardingShipment shipment)
		{
			if (!shipment.JS_RL_NKDestination.IsEmpty)
			{
				var localCountry = shipment.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, shipment.JS_RL_NKDestination.SubstringSafe(0, 2)));

				if (localCountry != null)
				{
					return localCountry.LocalCurrency;
				}
			}

			return shipment.Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, CurrencyCodes.UnitedStates));
		}

		void PopulatePackingDetails(ForwardingPackLine packLine, HVLVConsignmentHeader consignmentHeader, ForwardingContainer container, HVLVItem[] items)
		{
			logger.Log(LogType.Debug, string.Format("Trying to populate packing details for the packline `{0}`", packLine.JL_Description));
			packLine.JL_JC = container?.PK ?? ZGuid.Empty;
			packLine.JL_PackageCount = items.Length;

			var shipment = consignmentHeader.Shipment;

			var weightUnit = shipment.JS_UnitOfWeight;
			var volumeUnit = shipment.JS_UnitOfVolume;

			packLine.JL_ActualWeightUQ = weightUnit;
			packLine.JL_ActualVolumeUQ = volumeUnit;
			packLine.JL_ActualWeight = TotalCalculation.GetTotalWeight(items, x => x.HVI_ActualWeight == 0 ? x.HVI_ManifestedWeight : x.HVI_ActualWeight, x => x.Consignment.HVC_WeightUQ, weightUnit);
			packLine.JL_ActualVolume = TotalCalculation.GetTotalVolume(items, x => x.HVI_ActualVolume == 0 ? x.HVI_ManifestedVolume : x.HVI_ActualVolume, x => x.Consignment.HVC_VolumeUQ, volumeUnit);

			var numberOfDifferentPackTypes = items.Select(i => i.HVI_F3_NKPackType).Distinct().Where(t => !string.IsNullOrEmpty(t)).Take(2).ToArray();
			packLine.JL_F3_NKPackType = numberOfDifferentPackTypes.Length == 1 ? numberOfDifferentPackTypes[0].ToString() : PkgUnit.Package;

			foreach (var item in items)
			{
				if (!item.HVI_JS_LoadedOnShipment.IsEmpty)
				{
					logger.Warning(Res.GetString("be2722e7-1e5f-4d45-9e30-ef67b62d8172", "HVLV Item with ID {0} has been skipped as it is already loaded on Shipment with ID {1}", item.HVI_ItemId, item.Shipment.JS_UniqueConsignRef));
				}

				if (item.Consignment.HVC_HCH_Header.IsEmpty)
				{
					item.Consignment.HVC_HCH_Header = consignmentHeader.PK;
				}
			}

			logger.Log(LogType.Debug, string.Format("Finishing populating packing details for the packline `{0}`", packLine.JL_Description));
		}

		string GetShipmentContainerMode(string transportMode, HVLVOriginLoadList loadList)
		{
			switch (transportMode)
			{
				case TransportModes.Sea:
				case TransportModes.Rail:
					return ContainerModes.LCL;
				case TransportModes.Air:
					return HasContainerSpecified(loadList) ? ContainerModes.ULD : ContainerModes.Loose;
				case TransportModes.Road:
					return ContainerModes.LTL;
			}

			return string.Empty;
		}

		string GenerateErrorMessageForRepeatedReferences(List<ZString[]> repeatedWaybillNumbersAndHeaders, List<ZString[]> repeatedShipperReferencesAndHeaders, HVLVOriginLoadList loadList)
		{
			var messages = new ZStringBuilder();

			foreach (var repeatedWaybillNumber in repeatedWaybillNumbersAndHeaders)
			{
				messages.AppendLine(Res.GetString("2a7d5b26-dc0d-47cf-8ba5-4a233dbbc994",
					"Error processing the HVLV Origin Load List '{0}': Consignment Waybill number '{1}' is repeated on booking headers: {2}",
					loadList.HVL_UniqueReference, repeatedWaybillNumber[0], repeatedWaybillNumber[1]));
			}

			foreach (var repeatedShipperReference in repeatedShipperReferencesAndHeaders)
			{
				messages.AppendLine(Res.GetString("228544e2-8af3-4f98-85c8-afe1e0442728",
					"Error processing the HVLV Origin Load List '{0}': Consignment Shipper Reference '{1}' is repeated on booking headers: {2}",
					loadList.HVL_UniqueReference, repeatedShipperReference[0], repeatedShipperReference[1]));
			}

			return messages.ToString().Trim();
		}

		#endregion

		#region Implementation

		static bool HasContainerSpecified(HVLVOriginLoadList originLoadList)
		{
			return !originLoadList.HVL_ContainerNumber.IsEmpty || !originLoadList.HVL_RC_ContainerType.IsEmpty;
		}

		class HeaderAndItems
		{
			public HeaderAndItems(HVLVConsignmentHeader header)
			{
				Header = header;
				ContainerItemGroups = new List<Tuple<ForwardingContainer, HVLVItem[]>>();
			}

			public HVLVConsignmentHeader Header { get; }
			public List<Tuple<ForwardingContainer, HVLVItem[]>> ContainerItemGroups { get; }
		}

		class BookingHeaderComparerForShipmentCreation : IEqualityComparer<HVLVBookingHeader>
		{
			bool IEqualityComparer<HVLVBookingHeader>.Equals(HVLVBookingHeader x, HVLVBookingHeader y)
			{
				return x.BillToParty?.PK == y.BillToParty?.PK && x.HVH_RS_NKBookingServiceLevel == y.HVH_RS_NKBookingServiceLevel;
			}

			int IEqualityComparer<HVLVBookingHeader>.GetHashCode(HVLVBookingHeader header)
			{
				return header.BillToParty?.PK != null
					? header.BillToParty.PK.GetHashCode() ^ header.HVH_RS_NKBookingServiceLevel.GetHashCode()
					: header.HVH_RS_NKBookingServiceLevel.GetHashCode();
			}
		}

		#endregion
	}
}
