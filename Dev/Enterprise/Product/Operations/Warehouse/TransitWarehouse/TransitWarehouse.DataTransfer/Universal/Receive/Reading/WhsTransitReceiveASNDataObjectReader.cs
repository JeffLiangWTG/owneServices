using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.Definitions.Customs;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveASNDataObjectReader : WhsTransitDataObjectReader<WhsItemReceiveASN>
	{
		#region Constructor

		public WhsTransitReceiveASNDataObjectReader(UniversalShipment topLevelDO, IEnumerable<Container> containers, IEnumerable<IColumnIndexer> packageStates, IOrgHeader bookingParty, WhsWarehouse intendedWarehouse, IXmlImportLogger logger, UniversalObjectFactory factory, bool populateAdditionalReferenceOnly = false, Vehicle vehicle = null, ZString? masterBill = null, IEnumerable<IColumnIndexer> existingAdditionalReferences = null, WhsTransitPackageStateBusinessObjectFinderForASN finder = null)
			: base(topLevelDO, logger, factory)
		{
			this.topLevelDO = Argument.NotNull(topLevelDO, nameof(topLevelDO));
			this.consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			this.containers = containers ?? Enumerable.Empty<Container>();
			this.bookingParty = bookingParty;
			this.intendedWarehouse = Argument.NotNull(intendedWarehouse, nameof(intendedWarehouse));
			this.populateAdditionalReferenceOnly = populateAdditionalReferenceOnly;
			this.vehicle = vehicle;
			this.masterBill = masterBill;
			this.existingAdditionalReferences = existingAdditionalReferences ?? Enumerable.Empty<IColumnIndexer>();
			this.finder = finder;
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemReceiveASN asn) => asn == null ? Res.GetString("6df0275a-88a9-4b5c-8c39-4f16b3205365", "Receive ASN") : asn.HumanReadableName.ToString();

		readonly UniversalShipment topLevelDO;
		readonly UniversalShipment consolDO;
		readonly IEnumerable<Container> containers;
		readonly IOrgHeader bookingParty;
		readonly WhsWarehouse intendedWarehouse;
		readonly bool populateAdditionalReferenceOnly;
		readonly Vehicle vehicle;
		readonly ZString? masterBill;
		readonly IEnumerable<IColumnIndexer> existingAdditionalReferences;
		readonly WhsTransitPackageStateBusinessObjectFinderForASN finder;

		ZString ContainerNumber => CreateSingleASNForAllContainers ? "" : containers?.FirstOrDefault()?.ContainerNumber ?? "";
		ZString HouseBill => topLevelDO?.WayBillNumber ?? "";
		ZString MasterBill => masterBill ?? GetMasterBillNumberForConsolDO();

		ZString? consolNumber;
		ZString ConsolNumber => (ZString)(consolNumber ?? (consolNumber = consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol)?.Key ?? ""));
		ZString? shipmentNumber;
		ZString ShipmentNumber => (ZString)(shipmentNumber ?? (shipmentNumber = topLevelDO?.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key ?? ""));
		ZString VesselLloyds => consolDO.LloydsIMO ?? "";
		ZString VoyageFlightNo => consolDO.VoyageFlightNo ?? "";
		ZString PremiseID => consolDO.AdditionalReferenceCollection?.Where(a => a.Type.Code.Equals(CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID)).FirstOrDefault()?.ReferenceNumber ?? "";
		UniversalShipment topLeveDO;
		UniversalShipment TopLevelDO => topLeveDO ?? (topLeveDO = TransitUniversalExtensions.GetTopLevelDataObject(logger));
		bool IsArrivalTransitWarehouse => TopLevelDO.IsArrivalTransitWarehouse();
		bool CreateSingleASNForAllContainers => (WarehouseDataRegistry.Instance.CreateSingleASNForAllContainers.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty) && IsFromForwarding);

		public override DataContextType DataContextType => DataContextType.TransitReceiveASN;

		ZString GetMasterBillNumberForConsolDO()
		{
			if (IsFromSeaCargoOutturn)
			{
				var masterBillNumber = consolDO.SubShipmentCollection.Select(s => GetMasterBillNumbers(s)).Where(s => !string.IsNullOrEmpty(s)).OrderBy(s => s).FirstOrDefault();
				return masterBillNumber;
			}
			return consolDO?.WayBillNumber ?? "";
		}

		ZString GetMasterBillNumbers(UniversalShipment dataObject)
		{
			var result = "";
			var additionalBills = dataObject.AdditionalBillCollection;
			var hasContainer = dataObject.ContainerCollection?.Any(c => c.ContainerNumber.HasValue && c.ContainerNumber.Value == ContainerNumber) ?? false;
			if (hasContainer && additionalBills != null && additionalBills.Count > 0)
			{
				var additionalBill = additionalBills.FirstOrDefault(x => x.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.House);
				if (additionalBill != null)
				{
					result = additionalBill.ParentBillNumber ?? "";
				}
			}
			return result;
		}

		#endregion

		#region Matching

		protected override WhsItemReceiveASN GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (IsFromSeaCargoOutturn)
			{
				if (!VesselLloyds.IsEmpty && !VoyageFlightNo.IsEmpty && !PremiseID.IsEmpty && !ContainerNumber.IsEmpty)
				{
					var intendedWarehousePremiseID = TransitWarehouseHelper.GetPremiseIDFortWarehouse(intendedWarehouse);
					if (!intendedWarehousePremiseID.Equals(PremiseID, StringComparison.OrdinalIgnoreCase))
					{
						throw new DataObjectReadFailureException(Res.GetString("03573f2b-4e86-47b9-97e7-b1d80dcf1ea8",
							"The premise ID '{0}' did not match the intended warehouse '{1}' premise ID (CCP) '{2}'.", PremiseID, intendedWarehouse.WW_WarehouseNameMultilingual, intendedWarehousePremiseID));
					}
					else
					{
						var asnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));
						asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, intendedWarehouse.PK);
						asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_VehicleReference, ContainerNumber);
						asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddDays(-30));

						if (!populateAdditionalReferenceOnly)
						{
							var voyageNumberReferenceQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), WhsItemReceiveASNSchema.PK, CusEntryNumSchema.CE_ParentID);
							voyageNumberReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
							voyageNumberReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, VoyageFlightNo);

							var vesselLloydsReferenceQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), WhsItemReceiveASNSchema.PK, CusEntryNumSchema.CE_ParentID);
							vesselLloydsReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
							vesselLloydsReferenceQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, VesselLloyds);

							asnQuery.AddSubQuery(voyageNumberReferenceQuery, JoinCondition.And);
							asnQuery.AddSubQuery(vesselLloydsReferenceQuery, JoinCondition.And);
						}

						var asnsFromQuery = Array.ConvertAll(factory.RowFactory.Load(WhsItemReceiveASNSchema.Constants.TableName, asnQuery), DataObjectReader.GetColumnIndexerFromRow);
						var matchingASNs = asnsFromQuery;

						if (matchingASNs.Any())
						{
							foreach (var asn in matchingASNs)
							{
								factory.RowFactory.AddFetchHint(new FetchHint(CusEntryNumSchema.CE_ParentID, asn.GetValue(WhsItemReceiveASNSchema.PK)));
							}

							logger.Log(LogType.Information, ResString.GetMultilingualString(
								"1f45cc0c-26f2-4341-90b3-963806e7be5a",
								"ASN '{0}' was matched for the Consignment - It was created within the last 30 days and has the provided Vessel Lloyds/IMO '{1}' and Voyage Flight Number '{2}' and Container Number '{3}'.",
								matchingASNs[0].GetValue(WhsItemReceiveASNSchema.WRP_ReferenceNumber),
								VesselLloyds,
								VoyageFlightNo,
								ContainerNumber));

							var asnToReturn = matchingASNs[0]; // grab first element as they are ordered by latest Create Time
							return factory.Load<WhsItemReceiveASN>(asnToReturn.GetValue(WhsItemReceiveASNSchema.PK)); // need to return a BizO to the architecture
						}
						else
						{
							logger.Log(LogType.Information, ResString.GetMultilingualString(
								"2db533bc-6ede-4236-a58b-5295e39c72bd",
								"ASN matching for the Consignment failed - No ASN created within the last 30 days could be found with the provided Vessel Lloyds/IMO '{0}', Voyage Flight Number '{1}' and Container Number '{2}'.",
								VesselLloyds,
								VoyageFlightNo,
								ContainerNumber));

							return null;
						}
					}
				}

				throw new DataObjectReadFailureException(Res.GetString("6395b7a3-c5c0-49dd-b406-be707765942b",
@"UXML received could not be used to match to an ASN because of below errors. Correct them and try again.
{0}{1}{2}{3}",
!ContainerNumber.IsEmpty ? "" : "Container Number is not found.\r\n",
!VesselLloyds.IsEmpty ? "" : "Vessel Lloyds is not found.\r\n",
!PremiseID.IsEmpty ? "" : "Premise ID is not found.\r\n",
!VoyageFlightNo.IsEmpty ? "" : "Voyage Flight Number is not found."));
			}
			else
			{
				var isFromGateBooking = TopLevelDO.IsFromDataSource(DataContextType.GateBooking);
				if (!isFromGateBooking)
				{
					if (consolDO != null && MasterBill.IsEmpty && ConsolNumber.IsEmpty)
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("7a8ca872-2c72-43f2-8742-be337a3899aa", "ASN matching failed - A Master Bill or Consol Number was not provided for the Consolidation of Consignments."));
						return null;
					}
					else if (consolDO == null && HouseBill.IsEmpty)
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("89ba71dd-473e-451e-b9e0-2c2ff9ffbdb2", "ASN matching failed - A House Bill was not provided for the Consignment."));
						return null;
					}
				}

				var reference = GetExternalReference();
				var asnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));
				asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, intendedWarehouse.PK);
				asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_VehicleReference, reference);
				asnQuery.AddToFilter(WhsItemReceiveASNSchema.WRP_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddDays(-30));
				asnQuery.OrderBy = WhsItemReceiveASNSchema.Constants.WRP_SystemCreateTimeUtc + OrderByClause.Descending;

				if (isFromGateBooking)
				{
					var foundASNs = factory.Load<WhsItemReceiveASN>(asnQuery);
					var matchedASNPKs = foundASNs.Where(asn => !asn.ReceiveTransportationUnits.Any(rtu => rtu.WRH_GateInTime != ZDateTimeOffset.Empty))
						.Where(asn => !asn.PackageStates.Any(ps => ps.WPS_Status != TransitWarehouseStatuses.Codes.Booked)).Select(asn => asn.PK);

					asnQuery = new ZDBOnlyQuery(typeof(WhsItemReceiveASN));
					asnQuery.AddToFilter(WhsItemReceiveASNSchema.PK, matchedASNPKs);
				}

				var asnsFromQuery = Array.ConvertAll(factory.RowFactory.Load(WhsItemReceiveASNSchema.Constants.TableName, asnQuery), DataObjectReader.GetColumnIndexerFromRow);
				var matchingASNs = asnsFromQuery;

				if (matchingASNs.Any() && consolDO != null)
				{
					foreach (var asn in matchingASNs)
					{
						factory.RowFactory.AddFetchHint(new FetchHint(CusEntryNumSchema.CE_ParentID, asn.GetValue(WhsItemReceiveASNSchema.PK)));
					}

					if (!MasterBill.IsEmpty && !ConsolNumber.IsEmpty)
					{
						matchingASNs = GetASNsMatchingReferencesWithFallback(asnsFromQuery, reference);
					}
					else if (!ConsolNumber.IsEmpty)
					{
						matchingASNs = GetASNsMatchingConsolNumberWithNoMasterBill(asnsFromQuery, reference);
					}
					else if (!MasterBill.IsEmpty)
					{
						matchingASNs = GetASNsMatchingMasterBillWithNoConsolNumber(asnsFromQuery, reference);
					}
				}
				else if (matchingASNs.Any())
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString(
						"6016ef67-a35f-4503-aa39-f727c37dcc8b",
						"ASN {0} was matched for the Consignment - It was created within the last 30 days and has the provided Warehouse {1}, and reference {2}.",
						matchingASNs[0].GetValue(WhsItemReceiveASNSchema.WRP_ReferenceNumber),
						intendedWarehouse.WW_WarehouseCode,
						reference));
				}

				return GetLatestASN(matchingASNs, reference);
			}
		}

		protected override IMatchingBusinessEntityFinder<WhsItemReceiveASN> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#region GetLatestASN

		WhsItemReceiveASN GetLatestASN(IColumnIndexer[] orderedMatchingASNs, string reference)
		{
			if (orderedMatchingASNs.Length >= 1)
			{
				var asn = orderedMatchingASNs[0]; // grab first element as they are ordered by latest Create Time
				return factory.Load<WhsItemReceiveASN>(asn.GetValue(WhsItemReceiveASNSchema.PK)); // need to return a BizO to the architecture
			}

			if (consolDO != null)
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"9cebe47b-4307-47bb-a8cc-de6cafe84390",
					"ASN matching for the Consolidation of Consignments failed - No ASN created within the last 30 days could be found with the provided Warehouse {0}, Master Bill {1}, Consol Number {2}, and reference {3}.",
					intendedWarehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					MasterBill,
					ConsolNumber,
					reference));
			}
			else
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"c99f201d-c3c8-46fd-83f0-696429bb4809",
					"ASN matching for the Consignment failed - No ASN created within the last 30 days could be found with the provided Warehouse {0}, and reference {1}.",
					intendedWarehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					reference));
			}

			return null;
		}

		#endregion

		#region GetASNsMatchingReferencesOrdered

		IColumnIndexer[] GetASNsMatchingReferencesWithFallback(IColumnIndexer[] asns, string reference)
		{
			var matchingASNs = GetASNsMatchingSpecifiedReferencesOrdered(asns, isMasterBillRequired: true, isConsolNumberRequired: true, reference);

			if (matchingASNs.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"03c29d57-4a20-4800-9703-f31902ead59d",
					"ASN {0} was matched for the Consolidation of Consignments - It was created within the last 30 days and has the provided Warehouse {1}, Master Bill {2}, Consol Number {3}, and reference {4}.",
					matchingASNs[0].GetValue(WhsItemReceiveASNSchema.WRP_ReferenceNumber),
					intendedWarehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					MasterBill,
					ConsolNumber,
					reference));
			}
			else
			{
				matchingASNs = GetASNsMatchingConsolNumberWithNoMasterBill(asns, reference);
				if (!matchingASNs.Any())
				{
					matchingASNs = GetASNsMatchingMasterBillWithNoConsolNumber(asns, reference);
				}
			}

			return matchingASNs;
		}

		IColumnIndexer[] GetASNsMatchingConsolNumberWithNoMasterBill(IColumnIndexer[] asns, string reference)
		{
			var matchingASNs = GetASNsMatchingSpecifiedReferencesOrdered(asns, isMasterBillRequired: false, isConsolNumberRequired: true, reference);
			if (matchingASNs.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"9454006e-8b6a-484b-b89e-d308af059824",
					"ASN {0} was matched for the Consolidation of Consignments - It was created within the last 30 days and has the provided Warehouse {1}, Consol Number {2}, and reference {3}.",
					matchingASNs[0].GetValue(WhsItemReceiveASNSchema.WRP_ReferenceNumber),
					intendedWarehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					ConsolNumber,
					reference));
			}

			return matchingASNs;
		}

		IColumnIndexer[] GetASNsMatchingMasterBillWithNoConsolNumber(IColumnIndexer[] asns, string reference)
		{
			var matchingASNs = GetASNsMatchingSpecifiedReferencesOrdered(asns, isMasterBillRequired: true, isConsolNumberRequired: false, reference);
			if (matchingASNs.Any())
			{
				logger.Log(LogType.Information, ResString.GetMultilingualString(
					"794a9067-2414-4296-a989-d80ba10b17ca",
					"ASN {0} was matched for the Consolidation of Consignments - It was created within the last 30 days and has the provided Warehouse {1}, Master Bill {2}, and reference {3}.",
					matchingASNs[0].GetValue(WhsItemReceiveASNSchema.WRP_ReferenceNumber),
					intendedWarehouse.GetValue(WhsWarehouseSchema.WW_WarehouseCode),
					MasterBill,
					reference));
			}

			return matchingASNs;
		}

		IColumnIndexer[] GetASNsMatchingSpecifiedReferencesOrdered(IColumnIndexer[] asns, bool isMasterBillRequired, bool isConsolNumberRequired, string reference)
		{
			var matchingASNs = (
				from asn in asns
				let additionalReferences = GetAdditionalReferencesOnJob(asn)
				let masterbillOnASN = additionalReferences.FirstOrDefault(IsMasterBill)
				let consolNumberOnASN = additionalReferences.FirstOrDefault(IsConsolNumber)
				where MatchReferences(MasterBill, ConsolNumber, masterbillOnASN, consolNumberOnASN, isMasterBillRequired, isConsolNumberRequired)
				orderby asn.GetValue(WhsItemReceiveASNSchema.WRP_SystemCreateTimeUtc) descending
				select asn
			).ToArray();

			return matchingASNs;
		}

		IColumnIndexer[] GetAdditionalReferencesOnJob(IColumnIndexer additionalReferenceParent)
		{
			var additionalReferenceQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, additionalReferenceParent.GetValue(WhsItemReceiveASNSchema.PK));

			var additionalReferences = factory.RowFactory.Load(CusEntryNumSchema.Constants.TableName, additionalReferenceQuery);
			return Array.ConvertAll(additionalReferences, DataObjectReader.GetColumnIndexerFromRow);
		}

		static bool IsMasterBill(IColumnIndexer additionalReference)
		{
			return IsAdditionalReferenceOfType(additionalReference, AdditionalReferenceTypes.Codes.MasterBill);
		}

		static bool IsConsolNumber(IColumnIndexer additionalReference)
		{
			return IsAdditionalReferenceOfType(additionalReference, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
		}

		static bool IsAdditionalReferenceOfType(IColumnIndexer additionalReference, string additionalReferenceType)
		{
			return additionalReference.GetValue(CusEntryNumSchema.CE_EntryType).EqualsIgnoringCase(additionalReferenceType);
		}

		bool IsFromSeaCargoOutturn => consolDO?.GetMatchingDataSource(DataContextType.SeaCargoOutturn) != null;

		bool IsFromForwarding => consolDO?.GetMatchingDataSource(DataContextType.ForwardingConsol) != null;

		static bool MatchReferences(ZString masterBill, ZString consolNumber, IColumnIndexer masterBillOnASN, IColumnIndexer consolNumberOnASN, bool isMasterBillRequired, bool isConsolNumberRequired)
		{
			var hasMatchingMasterBill = HasMatchingReference(masterBill, masterBillOnASN, isMasterBillRequired);
			var hasMatchingConsolNumber = HasMatchingReference(consolNumber, consolNumberOnASN, isConsolNumberRequired);

			return hasMatchingMasterBill && hasMatchingConsolNumber;
		}

		static bool HasMatchingReference(ZString reference, IColumnIndexer referenceOnASN, bool isReferenceRequired)
		{
			var actualValue = referenceOnASN?.GetValue(CusEntryNumSchema.CE_EntryNum) ?? "";
			var isActualValueSpecified = !actualValue.IsEmpty;

			return (isActualValueSpecified && reference == actualValue)
				|| (!isActualValueSpecified && !isReferenceRequired);
		}

		#endregion

		#endregion

		#region Create / Update

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemReceiveASN asn)
		{
			var asnRow = GetColumnIndexer(asn);
			var reference = GetExternalReference();

			ImportPlannedContainers(intendedWarehouse, containers, consolDO, asnRow);
			ImportPlannedVehicle(intendedWarehouse, asnRow);
			PopulateUniversalJobLinks(asn, consolDO);

			if (populateAdditionalReferenceOnly)
			{
				PopulateAdditionalReferences(asn, asnRow, intendedWarehouse);
			}
			else
			{
				if (!IsNewBO)
				{
					var finderForDetach = new WhsTransitPackageStateBusinessObjectFinderForASN(WhsTransitPackageStateBusinessObjectFinderForASN.FinderType.ByASN, factory, asn: asn);
					PopulatePackageStates(asn, WhsTransitPackageStatePopulateStrategy.Detach, finderForDetach);
				}
				else
				{
					AddLogsForSettingASNReference(reference);
					SetValue(asnRow, WhsItemReceiveASNSchema.WRP_ReferenceNumber, NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseReceiveExpectedPackingID));
					SetValue(asnRow, WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse, intendedWarehouse.PK);
					SetValue(asnRow, WhsItemReceiveASNSchema.WRP_VehicleReference, reference);
				}

				SetValue(asnRow, WhsItemReceiveASNSchema.WRP_TransportMode, dataObject.GetTransportModeForConsolLevel());

				PopulateETA(intendedWarehouse, asnRow);
				PopulateBookingParty(asnRow);
				PopulateASNTransportLegs(asnRow);
				PopulateASNTransport(consolDO, TopLevelDO, asn);
				PopulateAdditionalReferences(asn, asnRow, intendedWarehouse);
				PopulatePackageStates(asn, WhsTransitPackageStatePopulateStrategy.Attach, finder);
				PopulateNotes(asn);
				LinkConsolAndContainer(consolDO, asnRow);
			}
		}

		#endregion

		#region Logging

		void AddLogsForSettingASNReference(string reference)
		{
			if (IsNewBO)
			{
				if (!ContainerNumber.IsEmpty)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("1ae141c2-cacc-4266-a03c-5f0de967f718", "ASN reference set to Container Number {0}.", reference));
				}
				else if (!MasterBill.IsEmpty)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("a76095cb-ab2d-477e-aed1-301f7432fdd5", "ASN reference set to Master Bill {0}.", reference));
				}
				else if (!ConsolNumber.IsEmpty)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("9496d2bd-8f4c-4055-a6bb-6e5860d85fb4", "ASN reference set to Consol Number {0}.", reference));
				}
				else if (!string.IsNullOrEmpty(consolDO?.VoyageFlightNo))
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("5a043708-a1d7-41a3-92a0-320aea65f33b", "ASN reference set to Voyage Flight Number {0}.", reference));
				}
				else if (!HouseBill.IsEmpty)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("ea3bbc01-ed20-47fd-bcb5-27e5e149abea", "ASN reference set to House Bill {0}.", reference));
				}
				else if (!ShipmentNumber.IsEmpty)
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("2ec710a7-333e-4913-a171-94bc94f749e8", "ASN reference set to Shipment Number {0}.", reference));
				}
				else
				{
					logger.Log(LogType.Information, ResString.GetMultilingualString("efb0d1ac-0e77-443e-bb4c-2f96be2fd059", "A Container Number, Consol Number, Master Bill, House Bill, Shipment Number, or Voyage Flight Number is required to set the ASN reference."));
				}
			}
		}

		#endregion

		#region GetExternalReference

		protected ZString GetExternalReference()
		{
			if (vehicle != null)
			{
				return vehicle.Registration?.Number ?? ZString.Empty;
			}

			if (!ContainerNumber.IsEmpty)
			{
				return ContainerNumber;
			}

			if (!MasterBill.IsEmpty)
			{
				return MasterBill;
			}

			if (!ConsolNumber.IsEmpty)
			{
				return ConsolNumber;
			}

			var voyageFlightNumber = consolDO?.VoyageFlightNo ?? "";
			if (!voyageFlightNumber.IsEmpty)
			{
				return voyageFlightNumber;
			}

			if (!HouseBill.IsEmpty)
			{
				return HouseBill;
			}

			return ShipmentNumber;
		}

		#endregion

		#region PopulateBookingParty

		void PopulateBookingParty(IColumnIndexer asnRow)
		{
			if (!IsFromSeaCargoOutturn && bookingParty != null)
			{
				var bookingPartyDocAddress = GetBookingPartyDocAddress(asnRow);
				new OrganisationDataObjectReader(new OrganizationAddress(), logger, factory).PopulateJobDocAddress(bookingParty.MainAddress, bookingPartyDocAddress); // needs BizOs to work
			}
		}

		JobDocAddress GetBookingPartyDocAddress(IColumnIndexer asnRow)
		{
			var jobDocAddressQuery = new ZQuery();
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentID, asnRow.GetValue(WhsItemReceiveASNSchema.PK));
			jobDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			jobDocAddressQuery.MaximumRows = 1;

			var addresses = factory.RowFactory.Load(JobDocAddressSchema.Constants.TableName, jobDocAddressQuery);
			var jobDocAddressRow = addresses.Length == 1 ? GetColumnIndexerFromRow(addresses[0]) : null;
			return jobDocAddressRow != null ? factory.Load<JobDocAddress>(jobDocAddressRow.GetValue(JobDocAddressSchema.PK)) : GetNewBookingPartyDocAddress(asnRow);
		}

		JobDocAddress GetNewBookingPartyDocAddress(IColumnIndexer asnRow)
		{
			var jobDocAddress = factory.New<JobDocAddress>();
			SetValue(jobDocAddress, JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			SetValue(jobDocAddress, JobDocAddressSchema.E2_ParentTableCode, WhsItemReceiveASNSchema.Constants.Prefix);
			SetValue(jobDocAddress, JobDocAddressSchema.E2_ParentID, asnRow.GetValue(WhsItemReceiveASNSchema.PK));

			return jobDocAddress;
		}

		#endregion

		#region LinkConsolAndContainer

		void LinkConsolAndContainer(UniversalShipment consolDO, IColumnIndexer asn)
		{
			if (logger.IsInternalImport() && consolDO != null)
			{
				var consolDataSource = consolDO.GetMatchingDataSource(DataContextType.ForwardingConsol);
				var parent = consolDataSource?.GetLoadedJobFromDataContextType(dataObject, factory.BOFactory);

				if (parent != null && parent is IForwardingConsol)
				{
					if (string.IsNullOrEmpty(ContainerNumber))
					{
						SetValue(asn, WhsItemReceiveASNSchema.WRP_ParentID, parent.PK);
						SetValue(asn, WhsItemReceiveASNSchema.WRP_ParentTableCode, JobConsolSchema.Constants.Prefix);
					}
					else
					{
						var query = new ZQuery(JobContainerSchema.JC_ContainerNum, ContainerNumber);
						query.AddToFilter(JobContainerSchema.JC_JK, parent.PK);
						var container = (BusinessObject)factory.BOFactory.Load<IForwardingContainer>(query).SingleOrDefault();

						if (container != null)
						{
							SetValue(asn, WhsItemReceiveASNSchema.WRP_ParentID, container.PK);
							SetValue(asn, WhsItemReceiveASNSchema.WRP_ParentTableCode, JobContainerSchema.Constants.Prefix);
						}
					}
				}
			}
		}

		#endregion

		#region Populate Transport Legs

		void PopulateASNTransportLegs(IColumnIndexer asn)
		{
			var asnBizO = factory.Load<WhsItemReceiveASN>(asn.GetValue(WhsItemReceiveASNSchema.PK));
			TransportLegImportHelper.ReadTransportLegs(logger, consolDO, factory, asnBizO, asnBizO.IntendedWarehouse);
		}

		#endregion

		#region PopulateETA

		void PopulateETA(WhsWarehouse warehouse, IColumnIndexer asnRow)
		{
			// sourceDO and consolDO are the same for ASNs, hence we only need consolDO to obtain our TransportLegs.
			var inboundTransportLeg = TransitUniversalHelper.GetInboundTransportLeg(factory, warehouse, consolDO, sourceDO: null);
			var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, warehouse, consolDO, sourceDO: null);
			var eta = GetExpectedArrivalTime(inboundTransportLeg, outboundTransportLeg, topLevelDO, warehouse);
			SetValue(asnRow, WhsItemReceiveASNSchema.WRP_ETA, eta);
		}

		ZDateTime GetExpectedArrivalTime(
			TransportLeg inboundTransportLeg,
			TransportLeg outboundTransportLeg,
			UniversalShipment topLevelDO,
			WhsWarehouse warehouse
		)
		{
			var expectedArrivalTime = inboundTransportLeg?.EstimatedArrival.GetValueOrDefault() ?? ZDateTime.Empty;
			if (!expectedArrivalTime.IsValid && topLevelDO.SubShipmentCollection != null)
			{
				var shipmentsWithPickupDetails = topLevelDO.SubShipmentCollection.Where(shipment => ShipmentHasPickupDetails(shipment, warehouse));
				expectedArrivalTime = GetEtaFromShipmentPickupDetails(shipmentsWithPickupDetails);
			}

			if (!expectedArrivalTime.IsValid)
			{
				expectedArrivalTime = outboundTransportLeg?.LCLReceivalCommences.GetValueOrDefault() ?? ZDateTime.Empty;
			}

			return expectedArrivalTime;
		}

		bool ShipmentHasPickupDetails(UniversalShipment shipment, WhsWarehouse warehouse)
		{
			var isOriginWarehouse = shipment.PortOfOrigin != null && TransitUniversalHelper.IsGivenPortARelatedOrExtraPort(warehouse, shipment.PortOfOrigin.Code);
			return isOriginWarehouse && shipment.LocalProcessing != null;
		}

		ZDateTime GetEtaFromShipmentPickupDetails(IEnumerable<UniversalShipment> shipmentsWithPickUpDetails)
		{
			var earliestEstimatedPickup = ZDateTime.Empty;
			var earliestPickupRequiredFrom = ZDateTime.Empty;

			foreach (var shipment in shipmentsWithPickUpDetails)
			{
				var estimatedPickup = shipment.LocalProcessing.EstimatedPickup.GetValueOrDefault();
				if (estimatedPickup.IsValid && (earliestEstimatedPickup.IsEmpty || estimatedPickup < earliestEstimatedPickup))
				{
					earliestEstimatedPickup = estimatedPickup;
				}

				var pickupRequiredFrom = shipment.LocalProcessing.PickupRequiredFrom.GetValueOrDefault();
				if (pickupRequiredFrom.IsValid && (earliestPickupRequiredFrom.IsEmpty || pickupRequiredFrom < earliestPickupRequiredFrom))
				{
					earliestPickupRequiredFrom = pickupRequiredFrom;
				}
			}

			return earliestEstimatedPickup.IsValid ? earliestEstimatedPickup : earliestPickupRequiredFrom;
		}

		#endregion

		#region Populate Transport Company

		void PopulateASNTransport(UniversalShipment consolDO, UniversalShipment sourceDO, WhsItemReceiveASN asn)
		{
			OrgAddressImportHelper.PopulateTransportOrg(consolDO, sourceDO, bookingParty as OrgHeader, asn, logger, factory);
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(WhsItemReceiveASN asn, IColumnIndexer asnRow, WhsWarehouse warehouse)
		{
			var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, warehouse, consolDO, sourceDO: null);
			var runSheetDS = consolDO?.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);

			var additionalReferences = new List<TransitAdditionalReferenceInfo>();
			if (IsArrivalTransitWarehouse)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.MasterBill, MasterBill);
			}
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, ConsolNumber);
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, runSheetDS?.Key);

			// if UXML is from gate booking, BookingConfirmationReference could be different meanings, it is not a CarrierBookingReference
			if (!dataObject.IsFromDataSource(DataContextType.GateBooking))
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, consolDO?.BookingConfirmationReference ?? "");
			}

			if (!IsFromSeaCargoOutturn)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.Vessel, outboundTransportLeg?.VesselName);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, outboundTransportLeg?.VoyageFlightNo);
			}
			else
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VesselLloyds, dataObject.LloydsIMO);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, dataObject.VoyageFlightNo);
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.Vessel, dataObject.VesselName);
			}

			foreach (var existingAdditionalReference in existingAdditionalReferences)
			{
				if (!additionalReferences.Any(a => (ZString)a.Type == existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryType) && (ZString)a.Value == existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryNum) && (ZString)a.Category == existingAdditionalReference.GetValue(CusEntryNumSchema.CE_Category)))
				{
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryType), existingAdditionalReference.GetValue(CusEntryNumSchema.CE_EntryNum), existingAdditionalReference.GetValue(CusEntryNumSchema.CE_Category));
				}
			}

			new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), asn, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
		}

		#endregion

		#region PopulatePackageStates

		void PopulatePackageStates(WhsItemReceiveASN asn, WhsTransitPackageStatePopulateStrategy populateStrategy, WhsTransitPackageStateBusinessObjectFinderForASN finder)
		{
			if (finder != null)
			{
				new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finder, asn, populateStrategy, ProcessType.ASN).ReadIntoCollectionRetainingUnmatchedElements();
			}
		}

		#endregion

		#region Create RTU

		void ImportPlannedContainers(IColumnIndexer warehouseRow, IEnumerable<Container> containers, UniversalShipment consolDO, IColumnIndexer asnRow)
		{
			if (IsArrivalTransitWarehouse)
			{
				foreach (var container in containers)
				{
					if (container != null && container.ContainerNumber.HasValue &&
						!container.ContainerNumber.Value.IsEmpty)
					{
						new WhsTransitReceiveTransportationUnitDataObjectReader(warehouseRow, container, consolDO,
								asnRow, logger, factory, bookingParty, MasterBill, populateAdditionalReferenceOnly)
							.ReadIntoBusinessObject();
					}
				}
			}
		}

		void ImportPlannedVehicle(IColumnIndexer warehouseRow, IColumnIndexer asnRow)
		{
			if (vehicle != null)
			{
				new WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(dataObject, logger, factory, warehouseRow, asnRow, vehicle).ReadIntoBusinessObject();
			}
		}

		#endregion

		#region PopulateNotes

		void PopulateNotes(WhsItemReceiveASN asn)
		{
			if (consolDO?.NoteCollection != null)
			{
				var notesToImport = WhsTransitStmNoteHelper.GetSupportedNotesByVisibilityTypes(asn, consolDO.NoteCollection, StmNoteDescription.Pub, StmNoteDescription.Agv, StmNoteDescription.Prv, StmNoteDescription.Int);
				if (notesToImport.Any())
				{
					new NotesCollectionReader(notesToImport, logger, factory, asn).ReadIntoCollection();
				}
			}
		}

		#endregion

		#region Populate Universal Job Links to Consol

		void PopulateUniversalJobLinks(WhsItemReceiveASN asn, UniversalShipment consolDO)
		{
			if (bookingParty != null && consolDO != null)
			{
				var sourceDataContext = consolDO.DataContext;
				var linkCreator = new UniversalJobLinkCreator(asn.Factory, asn, bookingParty, sourceDataContext, logger);
				linkCreator.TryCreateJobLink(DataContextType.ForwardingConsol);
			}
		}

		#endregion

		#endregion
	}
}
