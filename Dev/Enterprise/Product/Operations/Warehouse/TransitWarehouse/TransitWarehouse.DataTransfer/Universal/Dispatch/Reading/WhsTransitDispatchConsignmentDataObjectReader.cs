using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.TransitStoredProcedureHandler;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForDCN;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using MasterExtensions = Enterprise.MasterFiles.Business.Extensions;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitDispatchConsignmentDataObjectReader : WhsTransitConsignmentDataObjectReader<WhsItemDispatchConsignment>
	{
		public WhsTransitDispatchConsignmentDataObjectReader(UniversalShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer warehouseFromConsol = null, bool shouldReadSubShipments = true, Dictionary<PackingLine, DispatchInstructionWarningInfo> dcnWarningInfo = null, bool fromTWPConsol = false, ZString? masterShipmentId = null)
			: base(shipment, logger, factory, warehouseFromConsol, masterShipmentId)
		{
			ShouldReadSubShipments = shouldReadSubShipments;
			DCNWarningInfo = dcnWarningInfo ?? new Dictionary<PackingLine, DispatchInstructionWarningInfo>();
			FromTWPConsol = fromTWPConsol;
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemDispatchConsignment dcn) => dcn == null ? Res.GetString("bc441a38-aae8-4cf1-80b3-7c21138c5ac1", "Dispatch Consignment") : dcn.HumanReadableName.ToString();

		protected readonly bool ShouldReadSubShipments;
		protected readonly bool FromTWPConsol;
		protected readonly Dictionary<PackingLine, DispatchInstructionWarningInfo> DCNWarningInfo;

		WhsItemDispatchConsignment dcnForPostSavedAction;

		UniversalShipment topLeveDO;
		UniversalShipment TopLeveDO => topLeveDO ??= TransitUniversalExtensions.GetTopLevelDataObject(logger);

		#region Context

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransitDispatch; }
		}

		#endregion

		#region Parameterised Schema Columns

		protected override SchemaStringColumn ConsignmentIDSchemaColumn => DispatchConsignmentIDColumn;
		protected override SchemaStringColumn ConsignmentHouseBillColumn => WhsItemDispatchConsignmentSchema.WDC_HouseBillNumber;

		protected override SchemaStringColumn JobIDSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_JobID;

		protected override SchemaStringColumn ServiceLevelSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_RS_NKServiceLevel;

		protected override SchemaGuidColumn WarehouseSchemaColumn => DispatchConsignmentWarehouseColumn;

		static SchemaStringColumn DispatchConsignmentIDColumn => WhsItemDispatchConsignmentSchema.WDC_ConsignmentID;

		static SchemaGuidColumn DispatchConsignmentWarehouseColumn => WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse;

		protected override SchemaGuidColumn ParentIDSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_ParentID;

		protected override SchemaStringColumn ParentTableCodeSchemaColumn => WhsItemDispatchConsignmentSchema.WDC_ParentTableCode;

		protected override SchemaGuidColumn PKSchemaColumn => WhsItemDispatchConsignmentSchema.PK;

		protected override string TablePrefix => WhsItemDispatchConsignmentSchema.Constants.Prefix;

		#endregion

		#region Number Fountain ID

		protected override ZString NumberFountainID => NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseDispatchConsignmentID);

		#endregion

		#region Matching

		protected override WhsTransitConsignmentMatchingHelper<WhsItemDispatchConsignment> GetMatchingHelperCore(UniversalShipment sourceDO, IColumnIndexer warehouseFromConsol)
		{
			return new WhsTransitDispatchConsignmentMatchingHelper(factory, BookingParty, warehouseFromConsol, logger);
		}

		#endregion

		#region PopulateBusinessObjectCore

		protected override void PopulateBusinessObjectCore(UniversalShipment sourceDataObject, WhsItemDispatchConsignment consignment, IColumnIndexer consignmentRow)
		{
			RegisterHandler();
			consignment.WDC_TransportMode = sourceDataObject.GetTransportModeForShipmentLevel();
			consignment.WDC_ShippersReference = sourceDataObject.BookingConfirmationReference ?? ZString.Empty;

			PopulateExpectedDispatchIfValid(consignment, sourceDataObject);
			PopulateOrderReferences(sourceDataObject, consignment);

			logger.Log(LogType.Information, Res.GetString("3ddf9dd3-1982-4bc2-9374-05e72e740f8b", "Populating Packages for {0}...", GetJobTypeForLogs()));
			LinkPackages(consignment, consignmentRow, sourceDataObject);

			TransitUniversalHelper.UpdateAllowPartialLoading(factory, new[] { consignmentRow });

			var forwardingShipmentDS = sourceDataObject.GetMatchingDataSource(DataContextType.ForwardingShipment);
			var shipmentNumber = forwardingShipmentDS?.Key;
			if (shipmentNumber.GetValueOrDefault() != "")
			{
				PopulateGoverningReferencesForReceive(shipmentNumber.Value, consignment, sourceDataObject);
			}

			AuthorizedForDispatch(consignment);

			dcnForPostSavedAction = consignment;
		}

		protected override void PopulateConsignmentDirection(WhsItemDispatchConsignment consignment, UniversalShipment sourceDO)
		{
			var consolDO = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var twhCountryCode = consignment?.Warehouse?.RelatedCompanyBranch?.HomePort?.RL_RN_NKCountryCode ?? ZString.Empty;
			if (twhCountryCode.IsEmpty)
			{
				ConsignmentDirection = ZString.Empty;
			}

			var inboundTransportLeg = TransitUniversalHelper.GetInboundTransportLeg(factory, consignment.Warehouse, consolDO, sourceDO);
			var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, consignment.Warehouse, consolDO, sourceDO);

			var inboundLoadingCountryCode = inboundTransportLeg?.PortOfLoading?.Code?.Substring(0, 2) ?? ZString.Empty;
			var outboundDischargeCountryCode = outboundTransportLeg?.PortOfDischarge?.Code?.Substring(0, 2) ?? ZString.Empty;

			string splCountryCode = ZString.Empty;
			string spdCountryCode = ZString.Empty;

			if (consolDO != null)
			{
				splCountryCode = consolDO?.PortOfLoading?.Code?.Substring(0, 2) ?? ZString.Empty;
				spdCountryCode = consolDO?.PortOfDischarge?.Code?.Substring(0, 2) ?? ZString.Empty;
			}
			else if (sourceDO != null)
			{
				splCountryCode = sourceDO?.PortOfLoading?.Code?.Substring(0, 2) ?? ZString.Empty;
				spdCountryCode = sourceDO?.PortOfDischarge?.Code?.Substring(0, 2) ?? ZString.Empty;
			}

			var destination = consolDO?.PortOfDischarge?.Code ?? sourceDO?.PortOfDischarge?.Code ?? sourceDO?.PortOfDestination?.Code;
			var origin = consolDO?.PortOfLoading?.Code ?? sourceDO?.PortOfLoading?.Code ?? sourceDO?.PortOfOrigin?.Code;

			var destinationCountryCode = destination?.Substring(0, 2) ?? ZString.Empty;
			var originCountryCode = origin?.Substring(0, 2) ?? ZString.Empty;

			if (!outboundDischargeCountryCode.IsEmpty && outboundDischargeCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Export;
			}
			else if (!inboundLoadingCountryCode.IsEmpty && inboundLoadingCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Import;
			}
			else if (inboundTransportLeg is null && spdCountryCode == twhCountryCode && originCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Import;
			}
			else if ((inboundLoadingCountryCode == twhCountryCode && destinationCountryCode == twhCountryCode)
					 || (outboundDischargeCountryCode == twhCountryCode && originCountryCode == twhCountryCode)
					 || (outboundTransportLeg is null && splCountryCode == twhCountryCode && spdCountryCode == twhCountryCode))
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Domestic;
			}
			else if (outboundTransportLeg is null && splCountryCode == twhCountryCode && spdCountryCode != twhCountryCode)
			{
				ConsignmentDirection = TransitWarehouseConsignmentDirections.Codes.Export;
			}
			consignment.WDC_Direction = ConsignmentDirection;
		}

		protected override void PopulateDestination(WhsItemDispatchConsignment consignment, UniversalShipment sourceDO) => consignment.WDC_RL_NKDestination = sourceDO.PortOfDestination?.Code ?? string.Empty;

		void AuthorizedForDispatch(WhsItemDispatchConsignment consignment)
		{
			var isTWP = FromTWPConsol || Manager.IsTWP;
			if (isTWP)
			{
				if (!consignment.IsInDatabase && consignment.WDC_IsAuthorizedForDispatch)
				{
					consignment.WDC_IsAuthorizedForDispatch = false;
				}
			}
			else
			{
				consignment.WDC_IsAuthorizedForDispatch = true;
			}

			if (isTWP && consignment.IsInDatabase && consignment.WDC_IsAuthorizedForDispatch)
			{
				var message = Res.GetString("0e531b4a-7ae7-4723-90b8-77e59d6b8871", "Cannot Prepare '{0} - {1}' for Dispatch because Dispatch has already been authorized.", consignment.WDC_JobID, consignment.WDC_ConsignmentID);
				throw new DataObjectReadFailureException(message);
			}
		}

		protected override void AfterSuccessfullyPopulatingBizOCore()
		{
			if (TopLeveDO.IsTransitWarehouseDispatch() && dcnForPostSavedAction != null)
			{
				var dcnPackageStates = GetDCNPackages(dcnForPostSavedAction);

				var packageStateRCNs = dcnPackageStates.Select(ps => ps.WPS_WRC_TransitReceiveConsignment).Distinct().ToList();

				var rcnPackageStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, packageStateRCNs);
				var rcnPackageStates = factory.BOFactory.Load<WhsItemPackageState>(rcnPackageStateQuery).ToArray();

				if (rcnPackageStates.Length > 0)
				{
					StoredProcedureHandler.AddCachedPackageState([.. rcnPackageStates.Select(p => p.PK)], ProcedureType.CustomStatus);
				}

				if (dcnPackageStates.Length > 0)
				{
					StoredProcedureHandler.AddCachedPackageState([.. dcnPackageStates.Select(p => p.PK)], ProcedureType.SecurityStatus);
				}
			}
		}

		#region Populate Expected Dates

		void PopulateExpectedDispatchIfValid(WhsItemDispatchConsignment consignment, UniversalShipment sourceDataObject)
		{
			var warehouse = consignment.Warehouse;
			var consolDataObject = TransitUniversalExtensions.GetConsolDataObject(logger, factory);
			var outboundTransportLeg = TransitUniversalHelper.GetOutboundTransportLeg(factory, warehouse, consolDataObject, sourceDataObject);
			var expectedDispatch = GetExpectedDispatchDateTimeOffset(outboundTransportLeg, sourceDataObject, warehouse);
			if (expectedDispatch.IsValid)
			{
				consignment.WDC_ExpectedDispatchTime = expectedDispatch;
			}
		}

		void PopulateOrderReferences(UniversalShipment sourceDO, WhsItemDispatchConsignment consignment)
		{
			consignment.OrderReferences.DeleteAll();

			var orderReferencesUXML = sourceDO.LocalProcessing?.OrderNumberCollection ?? Enumerable.Empty<OrderNumber>();

			new TransitConsignmentOrderReferenceCollectionReader(orderReferencesUXML.ToArray(), consignment, logger, factory).ReadIntoCollection();
		}

		ZDateTimeOffset GetExpectedDispatchDateTimeOffset(TransportLeg outboundTransportLeg, UniversalShipment sourceDO, WhsWarehouse warehouse)
		{
			var expectedDispatchDateTimeOffset = ZDateTimeOffset.Empty;
			var isDestinationWarehouse = sourceDO.PortOfDestination != null &&
				TransitUniversalHelper.IsGivenPortARelatedOrExtraPort(warehouse, sourceDO.PortOfDestination.Code);

			if (isDestinationWarehouse && sourceDO.LocalProcessing != null)
			{
				var estimatedDelivery = sourceDO.LocalProcessing.EstimatedDelivery.GetValueOrDefault();
				var expectedDispatchDateTime = estimatedDelivery.IsValid ?
					estimatedDelivery : sourceDO.LocalProcessing.DeliveryRequiredBy.GetValueOrDefault();
				if (expectedDispatchDateTime.IsValid)
				{
					expectedDispatchDateTimeOffset = MasterExtensions.ToDateTimeOffset(expectedDispatchDateTime, warehouse.RelatedCompanyBranch.HomePort);
				}
			}

			if (!expectedDispatchDateTimeOffset.IsValid && outboundTransportLeg != null)
			{
				var ctoCutOff = outboundTransportLeg.FCLCutOff.GetValueOrDefault();
				var expectedDispatchDateTime = ctoCutOff.IsValid ? ctoCutOff : outboundTransportLeg.EstimatedDeparture.GetValueOrDefault();
				if (expectedDispatchDateTime.IsValid)
				{
					expectedDispatchDateTimeOffset = MasterExtensions.ToDateTimeOffset(expectedDispatchDateTime, warehouse.RelatedCompanyBranch.HomePort);
				}
			}

			return expectedDispatchDateTimeOffset;
		}

		#endregion

		#region LinkPackages

		void LinkPackages(WhsItemDispatchConsignment consignment, IColumnIndexer dispatchConsignmentRow, UniversalShipment sourceDataObject)
		{
			var finderByPackageID = new WhsTransitPackageStateBusinessObjectFinderForDCN(sourceDataObject, consignment, logger, factory, FindOption.ByPackageID);
			new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderByPackageID, consignment, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.DCN).ReadIntoCollectionRetainingUnmatchedElements();

			var finderByPackingLineID = new WhsTransitPackageStateBusinessObjectFinderForDCN(sourceDataObject, consignment, logger, factory, FindOption.ByPackingLineID);
			new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderByPackingLineID, consignment, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.DCN).ReadIntoCollectionRetainingUnmatchedElements();

			Func<UniversalShipment, WhsItemReceiveConsignment> findRCN = sourceDataObject => MatchingReceiveConsignment(dispatchConsignmentRow, sourceDataObject);
			var finderByRCN = new WhsTransitPackageStateBusinessObjectFinderForDCN(sourceDataObject, consignment, logger, factory, findRCN, DCNWarningInfo, FindOption.ByRCN);
			new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderByRCN, consignment, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.DCN).ReadIntoCollectionRetainingUnmatchedElements();

			var finderByDCN = new WhsTransitPackageStateBusinessObjectFinderForDCN(consignment, logger, factory, FindOption.ByDCN);
			new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderByDCN, consignment, WhsTransitPackageStatePopulateStrategy.Update, ProcessType.DCN).ReadIntoCollectionRetainingUnmatchedElements();

			new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), dataObject, logger, factory, finderByDCN, consignment, WhsTransitPackageStatePopulateStrategy.Detach, ProcessType.DCN).ReadIntoCollectionRetainingUnmatchedElements();
		}

		WhsItemReceiveConsignment MatchingReceiveConsignment(IColumnIndexer dispatchConsignmentRow, UniversalShipment sourceDataObject, bool requireLoggingAndMatch = true)
		{
			WhsItemReceiveConsignment receiveConsignment = null;
			var consignmentID = GetConsignmentID();
			var houseBill = GetHouseBill();

			if (consignmentID.IsEmpty && houseBill.IsEmpty)
			{
				var dataSourceType = (string)dataObject.FirstDataSource()?.Type;

				if (requireLoggingAndMatch && dataSourceType == nameof(DataContextType.ForwardingShipment))
				{
					var message = Res.GetString("22f1444e-9617-4ec5-af8d-0ad506266523", "Cannot Import Dispatch Consignment as no House Bill or Forwarding Shipment Number was provided.");
					throw new DataObjectReadFailureException(message);
				}
				else if (requireLoggingAndMatch && dataSourceType == nameof(DataContextType.LandTransportConsignment))
				{
					var message = Res.GetString("b8f8e241-9336-47fc-916f-2d5725a8daac", "Cannot Import Dispatch Consignment as no House Bill or Land Transport Consignment Number was provided.");
					throw new DataObjectReadFailureException(message);
				}
				else if (requireLoggingAndMatch)
				{
					var message = Res.GetString("ec60c616-d905-4afe-8cec-70fb292ac073", "Cannot Import Dispatch Consignment as no House Bill was provided.");
					throw new DataObjectReadFailureException(message);
				}
			}
			else
			{
				if (requireLoggingAndMatch)
				{
					var dataSource = sourceDataObject.FirstDataSource();

					if (!string.IsNullOrEmpty(dataSource?.Key))
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("4b7958be-8b54-49de-b98a-b094a05fa3b8",
							"Searching for Receive Consignment for '{0} - {1}'.", dataSource?.Type, dataSource?.Key));
					}
					else
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("5287c59e-7de8-4f27-967a-50bf126cc807", "Searching for Receive Consignment."));
					}
				}

				receiveConsignment = GetReceiveConsignment(GetWarehouse(dispatchConsignmentRow), consignmentID, houseBill, sourceDataObject);

				if (requireLoggingAndMatch)
				{
					if (receiveConsignment == null)
					{
						var warehouseBranchPK = WarehouseFromSource.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
						var allowImportWithoutRCN = WarehouseDataRegistry.Instance.IgnoreDCNOnUXMLImportWhenPackagesCantBeFoundToAttach.GetFallBackValueAtAllLevels(Guid.Empty, warehouseBranchPK.ToGuid(), Guid.Empty);
						if (allowImportWithoutRCN)
						{
							var warningLogMessage = Res.GetString("7170304c-23f1-47fe-bd4e-9b362d44164a",
								"No Receive Consignment could be found matching {0} or {1}.",
								((NoResString)"Shipment " + consignmentID).Trim(),
								((NoResString)"House Bill " + houseBill).Trim());
							var dcnID = dispatchConsignmentRow.GetValue(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID);
							foreach (var packingline in sourceDataObject.PackingLineCollection)
							{
								if (!DCNWarningInfo.ContainsKey(packingline))
								{
									var info = new DispatchInstructionWarningInfo();
									DCNWarningInfo.Add(packingline, info);
								}
								DCNWarningInfo[packingline].DCNIDsCanNotBeCreated.Add(dcnID);
							}
							logger.Log(LogType.Warning, warningLogMessage);
						}
						else
						{
							var message = Res.GetString("f75e665f-f1c6-47b6-ac35-9c307cea9741",
								"No Receive Consignment could be found matching {0} or {1}. Send the Receive Instructions then send the Dispatch Instructions again.",
								((NoResString)"Shipment " + consignmentID).Trim(),
								((NoResString)"House Bill " + houseBill).Trim());
							throw new DataObjectReadFailureException(message);
						}
					}
					else
					{
						logger.Log(LogType.Information, ResString.GetMultilingualString("71cd2dc3-b0a6-4d09-b401-25842318302f",
							"Found matching Receive Consignment {0}.", WhsTransitLogHelper.FormatCodeDescriptionForLogging(receiveConsignment.WRC_JobID, receiveConsignment.WRC_ConsignmentID)));
					}
				}
			}

			return receiveConsignment;
		}

		#endregion

		#region PopulateAdditionalReferences

		protected override void PopulateConsignmentAdditionalReferencesCore(WhsItemDispatchConsignment consignment, UniversalShipment sourceDO, WhsTransitAdditionalReferencesHelper referenceHelper, List<TransitAdditionalReferenceInfo> additionalReferences)
		{
			var consignmentPK = consignment.GetValue(WhsItemDispatchConsignmentSchema.PK);
			var consignmentTableName = WhsItemDispatchConsignmentSchema.Constants.TableName;
			ZString? masterShipmentReference = null;

			if (Manager.Master != null && TransitUniversalHelper.IsMasterShipmentRepresentingAllChildShipments(Manager.Master))
			{
				masterShipmentReference = Manager.Master.WayBillNumber;
			}
			else if (!string.IsNullOrEmpty(MasterShipmentId))
			{
				masterShipmentReference = MasterShipmentId;
			}

			if (masterShipmentReference is not null)
			{
				referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.AssemblyMasterShipment, masterShipmentReference);
			}

			PopulateReferencesUsingMapper(referenceHelper, consignmentPK, consignmentTableName, additionalReferences, sourceDO.IsDepartureTransitWarehouse());
		}

		void PopulateReferencesUsingMapper(WhsTransitAdditionalReferencesHelper referenceHelper, ZGuid consignmentPK, string consignmentTableName, List<TransitAdditionalReferenceInfo> additionalReferences, bool isDepartureTransitWarehouse)
		{
			var warehouseBranchPK = WarehouseFromSource.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
			var warehouseBranch = factory.Load<GlbBranch>(warehouseBranchPK);
			var companyPK = warehouseBranch?.Company.PK;
			var mapper = GetMappedReferences(dataObject, warehouseBranch);

			var countryCode = warehouseBranch.GB_RN_NKCountryCode;
			var additionalReferenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.GetFallBackValueAtAllLevels(companyPK?.ToGuid() ?? Guid.Empty, warehouseBranch?.PK.ToGuid() ?? Guid.Empty, Guid.Empty).GetCodeDescriptionPairList().ToArray();
			var validReferenceCodes = additionalReferenceTypes.Select(c => c.Code);
			var validAdditionalReferences = mapper.additionalReferences.Where(a => a.Type != null && validReferenceCodes.Contains(a.Type.Code.GetValueOrDefault().ToString())
																			&& (a.CountryOfIssue == null || a.CountryOfIssue.Code.GetValueOrDefault().IsEmpty || a.CountryOfIssue.Code.GetValueOrDefault() == countryCode));
			var invalidAdditionalReferences = mapper.additionalReferences.Where(r => !validAdditionalReferences.Contains(r));

			if (invalidAdditionalReferences.Any())
			{
				var logHelper = new CusEntryNumberTransitLogHelper();
				logger.Log(LogType.Warning, logHelper.GetTable(Res.GetString("d66d7236-e321-41a0-afa5-0c03fd6a4f41", "The following Additional References are invalid for this Transit Warehouse and could not be imported:"), invalidAdditionalReferences, TransitLogColumnIDs.CusEntryNumberColumn.Type, TransitLogColumnIDs.CusEntryNumberColumn.Number, TransitLogColumnIDs.CusEntryNumberColumn.CountryCode));

				var noteHelper = new WhsTransitStmNoteHelper(logger, factory);
				noteHelper.AddOrUpdateUnrecognisedAdditionalReferenceNote(consignmentPK, WhsItemDispatchConsignmentSchema.Constants.TableName, invalidAdditionalReferences);
			}

			if (validAdditionalReferences.Any())
			{
				foreach (var additionalReference in validAdditionalReferences)
				{
					if (additionalReference.Type.Code.Value == WarehouseAdditionalReferenceTypes.Codes.MasterBill && !isDepartureTransitWarehouse)
					{
						continue;
					}
					referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, additionalReference.Type.Code.Value, additionalReference.ReferenceNumber, countryCode: additionalReference.CountryOfIssue?.Code ?? ZString.Empty);
				}
			}
			// Governing references (Port/customs) are imported after packages have been imported. RCN required.
		}

		#endregion

		#region PopulateGoverningReferencesForReceive

		void PopulateGoverningReferencesForReceive(ZString shipmentNumber, WhsItemDispatchConsignment consignment, UniversalShipment dataObject)
		{
			var dcnPackages = GetDCNPackages(consignment);
			var matchingRCNs = GetMatchingRCNs(shipmentNumber, dcnPackages);

			var matchingRCNPKs = matchingRCNs.Select(rcn => rcn[WhsItemReceiveConsignmentSchema.Constants.PK]);
			var blindPackageStatePKs = dcnPackages
				.Where(ps => !matchingRCNPKs.Contains(ps.WPS_WRC_TransitReceiveConsignment))
				.Select(ps => ps.PK);

			var matchingRCNBizos = factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.PK, matchingRCNPKs));
			var blindPackageStateBizos = factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, blindPackageStatePKs));

			var warehouseBranchPK = WarehouseFromSource.GetValue(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
			var warehouseBranch = factory.Load<GlbBranch>(warehouseBranchPK);
			var mapper = GetMappedReferences(dataObject, warehouseBranch);

			var countryCode = warehouseBranch.GB_RN_NKCountryCode;
			PopulateValidPortReferences(matchingRCNBizos, blindPackageStateBizos, countryCode, mapper.portReferences);
			PopulateValidCustomsReferences(matchingRCNBizos, blindPackageStateBizos, countryCode, mapper.entryNumbers);

			WhsTransitUniversalEventHelper.GetBizoColumnIndexers(factory, matchingRCNBizos.ToArray());
			WhsTransitUniversalEventHelper.GetBizoColumnIndexers(factory, blindPackageStateBizos.ToArray());

			matchingRCNBizos.ForEach(rcn => rcn.HasChanges = true);
			blindPackageStateBizos.ForEach(ps => ps.HasChanges = true);
		}

		(List<PortReference> portReferences, List<AdditionalReference> additionalReferences, List<(ZString SourceType, EntryNumber EntryNumber)> entryNumbers) GetMappedReferences(UniversalShipment dataObject, GlbBranch warehouseBranch)
		{
			var companyPK = warehouseBranch.Company.PK;
			var referenceConverter = new ReferenceConverter(dataObject.PortReferenceCollection, dataObject.AdditionalReferenceCollection, dataObject.EntryNumberCollection);

			var mappingInfo = WhsTransitAdditionalReferencesHelper.GetTransitReferenceMappingInfo(companyPK.ToGuid(), warehouseBranch.PK.ToGuid());
			var mapper = referenceConverter.ConvertReferences(mappingInfo, ConsignmentDirection);
			return mapper;
		}

		void PopulateValidCustomsReferences(IEnumerable<WhsItemReceiveConsignment> matchingRCNs, IEnumerable<WhsItemPackageState> blindPackageStates, ZString countryCode, List<(ZString SourceType, EntryNumber EntryNumber)> entryNumbers)
		{
			var validEntryNumbers = entryNumbers.Where(c => c.EntryNumber.Type != null && (c.EntryNumber.Type.Code.GetValueOrDefault() == TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber
							|| c.EntryNumber.Type.Code.GetValueOrDefault() == TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber)
							&& (c.EntryNumber.CountryOfIssue == null || c.EntryNumber.CountryOfIssue.Code.GetValueOrDefault().IsEmpty || c.EntryNumber.CountryOfIssue.Code.GetValueOrDefault() == countryCode));

			if (validEntryNumbers.Any())
			{
				var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);
				var rcnAdditionalReferences = new List<TransitAdditionalReferenceInfo>();
				var packageAdditionalReferences = new List<TransitAdditionalReferenceInfo>();

				foreach (var rcn in matchingRCNs)
				{
					rcnAdditionalReferences.Clear();
					foreach (var reference in validEntryNumbers)
					{
						referenceHelper.CollectTransitAdditionalReferenceInfoIfNumberNotExist(rcnAdditionalReferences, rcn.PK, reference.EntryNumber.Type.Code.Value, reference.EntryNumber.Number, reference.SourceType, TransitWarehouseReferenceCategories.Codes.CustomsReference);
					}
					new TransitWarehouseCusEntryNumReferenceCollectionReader(rcnAdditionalReferences.ToArray(), rcn, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
				}

				foreach (var package in blindPackageStates)
				{
					packageAdditionalReferences.Clear();
					foreach (var reference in validEntryNumbers)
					{
						referenceHelper.CollectTransitAdditionalReferenceInfoIfNumberNotExist(packageAdditionalReferences, package.WPS_KP_Package, reference.EntryNumber.Type.Code.Value, reference.EntryNumber.Number, reference.SourceType, TransitWarehouseReferenceCategories.Codes.CustomsReference);
					}
					new TransitWarehouseCusEntryNumReferenceCollectionReader(packageAdditionalReferences.ToArray(), package.Package, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
				}
			}
		}

		void PopulateValidPortReferences(WhsItemReceiveConsignment[] matchingRCNs, WhsItemPackageState[] blindPackageStates, ZString countryCode, List<PortReference> portReferences)
		{
			var validPortReferences = portReferences.Where(p => p.Type != null && p.Type.Code.GetValueOrDefault() == TransitWarehousePortReferenceTypes.Codes.PortAuthority
				&& (p.Country == null || p.Country.Code.GetValueOrDefault().IsEmpty || p.Country.Code.GetValueOrDefault() == countryCode));

			if (validPortReferences.Any())
			{
				foreach (var rcn in matchingRCNs)
				{
					new PortReferenceCollectionReader(validPortReferences.ToArray(), rcn, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
				}

				foreach (var package in blindPackageStates)
				{
					new PortReferenceCollectionReader(validPortReferences.ToArray(), package.Package, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
				}
			}
		}

		DataRow[] GetMatchingRCNs(ZString shipmentNumber, WhsItemPackageState[] dcnPackages)
		{
			var packageRCNPKs = dcnPackages.Select(ps => ps.WPS_WRC_TransitReceiveConsignment).Distinct();
			var packageRCNsQuery = new ZQuery(WhsItemReceiveConsignmentSchema.PK, packageRCNPKs);
			var matchingRCNs = factory.RowFactory.Load(WhsItemReceiveConsignmentSchema.Constants.TableName, packageRCNsQuery)
				.Where(rcn => ShipmentNumberMatches(shipmentNumber, (Guid)rcn[WhsItemReceiveConsignmentSchema.Constants.PK]));

			return matchingRCNs.ToArray();
		}

		bool ShipmentNumberMatches(ZString shipmentNumber, Guid parentPK)
		{
			var cusEntryNumQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, parentPK);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, shipmentNumber);

			return factory.RowFactory.Load(CusEntryNumSchema.Constants.TableName, cusEntryNumQuery).Any();
		}

		WhsItemPackageState[] GetDCNPackages(WhsItemDispatchConsignment consignment)
		{
			var dcnPackagesQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, consignment.PK);
			dcnPackagesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_IsHandlingUnit, false);
			dcnPackagesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_AdjustedOut, "");

			return factory.BOFactory.Load<WhsItemPackageState>(dcnPackagesQuery).ToArray();
		}

		#endregion

		WhsItemReceiveConsignment GetReceiveConsignment(IColumnIndexer warehouse, ZString consignmentID, string houseBill, UniversalShipment sourceDO)
		{
			return WhsTransitReceiveConsignmentDataObjectReader.TryGetReceiveConsignmentInWarehouse(warehouse, BookingParty, consignmentID, houseBill, sourceDO, factory, Manager.IsTWX, logger);
		}

		#endregion

		#region ReadSubShipment

		protected override void ReadSubShipment(UniversalShipment shipment, ZString? masterShipmentId = null)
		{
			if (ShouldReadSubShipments)
			{
				var reader = new WhsTransitDispatchConsignmentDataObjectReader(shipment, logger, factory, WarehouseFromSource, shouldReadSubShipments: false, dcnWarningInfo: DCNWarningInfo, masterShipmentId: masterShipmentId);
				reader.ReadIntoBusinessObject();
			}
		}

		#endregion

		#region GetConsigneeDocAddress

		protected override JobDocAddress GetConsigneeDocAddress(WhsItemDispatchConsignment consignment) => consignment.ConsigneeDocAddress;

		#endregion

		#region TransitJobType

		protected override string TransitJobType => TransitConstants.TransitDispatchJobType; // Event reference parameters

		#endregion

		#region PopulateAdditionalServices

		protected override IEnumerable<WhsJobService> GetRCNServices(UniversalShipment sourceDataObject, WhsItemDispatchConsignment consignment, IColumnIndexer consignmentRow)
		{
			var rcn = MatchingReceiveConsignment(consignmentRow, sourceDataObject, requireLoggingAndMatch: false);
			return rcn != null ? rcn.Services.Cast<WhsJobService>() : Enumerable.Empty<WhsJobService>();
		}

		protected override IEnumerable<WhsJobService> GetDCNServices(WhsItemDispatchConsignment consignment) => consignment.Services.Cast<WhsJobService>();

		#endregion

		#region ResStrings

		protected override ResourceString GetJobTypeForLogs()
		{
			return ResString.GetMultilingualString("09463afd-3e1e-4dda-93c5-d37f33d3720c", "Dispatch Consignment");
		}

		protected override ResourceString GetLogForMatching(IDataSourceDataObject dataSource)
		{
			if (!string.IsNullOrEmpty(dataSource?.Key))
			{
				return ResString.GetMultilingualString("dec6aa5f-fae9-48aa-a40e-7e199f70bfa7", "Searching for Dispatch Consignment for '{0} - {1}'.", dataSource?.Type, dataSource?.Key);
			}
			else
			{
				return ResString.GetMultilingualString("e6457876-e8f5-480d-b03f-710e0c932637", "Searching for Dispatch Consignment.");
			}
		}

		#endregion

		#region RegisterHandler

		protected override void RegisterHandler()
		{
			HandlerManager.Init(logger, factory, this, null);
			base.RegisterHandler();
		}

		#endregion
	}
}
