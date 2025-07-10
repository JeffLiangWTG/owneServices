using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Documents.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVConsignmentDataContextManager : ShipmentDataContextManager<HVLVConsignment>
	{
		public override DataContextType DataContextType => DataContextType.HVLVConsignment;

		protected override bool CanUpdateLogParentFromEventCore(BusinessObject logParent, IXmlEventValueObject xmlEvent, out ZString failureReason)
		{
			var result = base.CanUpdateLogParentFromEventCore(logParent, xmlEvent, out failureReason);
			if (result && logParent is HVLVConsignment consignment)
			{
				result = IsEventDataSourceValid(consignment, xmlEvent, ref failureReason);
			}

			return result;
		}

		bool IsEventDataSourceValid(HVLVConsignment consignment, IXmlEventValueObject xmlEvent, ref ZString failureReason)
		{
			var result = true;
			if (xmlEvent.EventType == AutoEvents.CustomsEntryStatusCode
				|| xmlEvent.EventType == AutoEvents.MessageStatusChangeCode)
			{
				var declarationPK = ZGuid.Empty;
				var direction = GetEventDirection(xmlEvent, consignment);

				if (IsDataSourceCheckNeeded(direction))
				{
					if (direction == Directions.Import)
					{
						declarationPK = consignment.HVC_JE_ImportDeclaration;
					}
					else if (direction == Directions.Export)
					{
						declarationPK = consignment.HVC_JE_ExportDeclaration;
					}

					if (!declarationPK.IsEmpty)
					{
						var declarationExists = consignment.Factory.Exists(typeof(BaseJobDeclaration),
							new ZQuery(JobDeclarationSchema.PK, declarationPK).AddToFilter(JobDeclarationSchema.JE_IsCancelled, false));
						var hasCustomsDeclarationDataSource =
							xmlEvent.GetMatchingDataSource(DataContextType.CustomsDeclaration) != null;
						if (declarationExists && !hasCustomsDeclarationDataSource)
						{
							failureReason = Res.GetString(
								"f052671b-66d4-4905-a507-b8113678274f",
								"Consignment has a standalone declaration and only accept Customs Status Update Event from Data Source of Declaration");

							result = false;
						}
					}
				}
			}

			return result;
		}

		bool IsDataSourceCheckNeeded(Directions direction)
		{
			return !(Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Australia && direction == Directions.Import);
		}

		public override ZString DataContextKey => ParentBO.HVC_ConsignmentId;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new ZQuery(HVLVConsignmentSchema.HVC_ConsignmentId, matchingValues.Key);

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				var isAir = ParentBO.ManifestedOnShipment == null || ParentBO.ManifestedOnShipment.IsAir;
				if (isAir)
				{
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.HAWBNumber, ParentBO.HVC_WaybillNumber);
				}
				else
				{
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLNumber, ParentBO.HVC_WaybillNumber);
				}

				result.AddIfNotEmpty(UniversalEvent.ContextTypes.ShippersReference, ParentBO.HVC_ShipperReference);

				result.AddIfNotEmpty(UniversalEvent.ContextTypes.WarehouseReleaseStatus, ParentBO.HVC_ImportReleaseStatus == HVLVReleaseStatus.None ?
					ParentBO.HVC_ExportReleaseStatus : ParentBO.HVC_ImportReleaseStatus);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.ComplianceStatus, !ParentBO.HVC_ImportCustomsClearanceStatus.IsEmpty ?
					ParentBO.HVC_ImportCustomsClearanceStatus :
					ParentBO.HVC_ExportCustomsClearanceStatus);

				var consolBO = ParentBO.ManifestedOnShipment?.ArrivalConsol;
				if (consolBO != null)
				{
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportMode, consolBO.TransportMode);

					result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLOriginUNLOCO, consolBO.LoadPort.GetUNLOCO());
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLDestinationUNLOCO, consolBO.DischargePort.GetUNLOCO());

					result.AddIfNotEmpty(UniversalEvent.ContextTypes.EstimatedTimeOfArrival, consolBO.JK_JX_JB_E_ARV);
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.EstimatedTimeOfDeparture, consolBO.JK_JX_JA_E_DEP);

					if (isAir)
					{
						result.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBNumber, consolBO.JK_MasterBillNum.FormatAirMAWB());
						result.AddIfNotEmpty(UniversalEvent.ContextTypes.FlightNumber, consolBO.JK_JX_JV_VoyageFlight);

						result.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBOriginIATAAirportCode, consolBO.LoadPort.GetIATACode());
						result.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBDestinationIATAAirportCode, consolBO.DischargePort.GetIATACode());
					}
					else
					{
						result.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, consolBO.JK_MasterBillNum);
						result.AddIfNotEmpty(UniversalEvent.ContextTypes.VoyageNumber, consolBO.JK_JX_JV_VoyageFlight);
						result.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselName, consolBO.JK_JX_JV_NKVessel);
					}
				}
			}

			return result;
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			var referenceParameters = StmALog.GetParametersFromReference(eventAdded.EventReference);

			switch (eventAdded.EventType.Value)
			{
				case AutoEvents.CustomsEntryStatusCode:
				case AutoEvents.MessageStatusChangeCode:
					var direction = GetEventDirection(eventAdded, ParentBO);

					var newStatus = (eventAdded as IXmlEventValueObject)?.Context?.ComplianceStatus.GetValueOrDefault();
					var warehouseReleaseStatus = (eventAdded as IXmlEventValueObject)?.Context?.WarehouseReleaseStatus.GetValueOrDefault();
					if (!newStatus.Value.IsEmpty)
					{
						if (direction == Directions.Export)
						{
							ParentBO.HVC_ExportCustomsClearanceStatus = newStatus.Value;
						}
						else if (direction == Directions.Import)
						{
							ParentBO.HVC_ImportCustomsClearanceStatus = newStatus.Value;
						}
					}
					else if (!warehouseReleaseStatus.Value.IsEmpty && HVLVReleaseStatus.GetAll().ContainsCode(warehouseReleaseStatus.Value))
					{
						if (direction == Directions.Export)
						{
							ParentBO.HVC_ExportReleaseStatus = warehouseReleaseStatus.Value;
						}
						else if (direction == Directions.Import)
						{
							ParentBO.HVC_ImportReleaseStatus = warehouseReleaseStatus.Value;
						}
					}
					else
					{
						if (referenceParameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var newReleaseStatus)
							&& !string.IsNullOrEmpty(newReleaseStatus))
						{
							if (direction == Directions.Export)
							{
								ParentBO.HVC_ExportReleaseStatus = newReleaseStatus;
							}
							else if (direction == Directions.Import)
							{
								ParentBO.HVC_ImportReleaseStatus = newReleaseStatus;
							}
						}
					}

					break;
				case AutoEvents.StatusUpdatedCode:
					if (referenceParameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, out var newStatusFromEvent)
						&& !string.IsNullOrEmpty(newStatusFromEvent))
					{
						ParentBO.HVC_Status = newStatusFromEvent;
					}

					break;
				default:
					break;
			}

			if (IsACASReport(eventAdded))
			{
				var eventType = eventAdded.EventType.Value;
				if (ACASEventTypes.Contains(eventType))
				{
					HVLVACASStatusManager.UpdateACASStatus(ParentBO, eventType, eventAdded.EventParameters?.Reason);
					if (ParentBO != null && ParentBO.HVC_ACASMessageStatus == HVLVACASMessageStatusList.Codes.AcknowledgementRequired)
					{
						var shipment = ParentBO.ManifestedOnShipment;
						var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.HeldCode).AddToFilter(StmALogSchema.SL_Reference, "|DEP=Customs|MST=ACAS Hold Acknowledgement");
						if (shipment != null && eventType == AutoEvents.HeldCode && !shipment.Logs.DatabaseHasLogs(logQuery))
						{
							var eventParameters = new[]
							{
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, (NoResString)"Customs"),
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, (NoResString)"ACAS Hold Acknowledgement")
							};
							shipment.Logs.CreateOrRecreateEventLog(AutoEvents.Held, EstimateActual.Actual, ZDateTimeOffset.UtcNow, ZString.Empty, eventParameters.ToArray());
						}
					}
				}
			}
		}

		bool IsACASReport(UniversalEvent universalEvent)
		{
			var result = false;
			var reportName = universalEvent.DataContext?.DocumentaryOverride?.DocumentName.GetValueOrDefault();
			if (!string.IsNullOrEmpty(reportName))
			{
				var acasReportName = new HVLVAirCargoAdvanceScreeningMessageSender(null).ACASDocumentaryName;
				result = reportName.Equals(acasReportName);
			}

			return result;
		}

		static Directions GetEventDirection(IXmlEventValueObject eventAdded, HVLVConsignment consignment)
		{
			var result = Directions.Unknown;
			var eventCompanyCountry = eventAdded.DataContext.CountryCodeToImportInto;
			var shipmentOriginCountry = consignment.ManifestedOnShipment?.Origin?.RL_RN_NKCountryCode;
			var shipmentDestinationCountry = consignment.ManifestedOnShipment?.Destination?.RL_RN_NKCountryCode;
			if (shipmentOriginCountry.HasValue && eventCompanyCountry == shipmentOriginCountry.Value)
			{
				result = Directions.Export;
			}
			else if (shipmentDestinationCountry.HasValue && eventCompanyCountry == shipmentDestinationCountry.Value)
			{
				result = Directions.Import;
			}

			return result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) =>
			new HVLVConsignmentEventParentFinder(this, factory, logger);

		public override string DefaultOutputDirectory => null;

		public override bool ManagesShipments => true;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => recipientRoles.Any(IsTargeted);

		bool IsTargeted(IRecipientRoleDataObject role)
		{
			switch (role.Code)
			{
				case RecipientRoleType.HVL:
					return true;
				default:
					return false;
			}
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) =>
			new HVLVConsignmentDataObjectReader(universalShipment, logger, factory);

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => new HVLVConsignmentDataObjectWriter(writeManager);

		protected List<string> ACASEventTypes =>
			new List<string>()
			{
				AutoEvents.MessagePendingProcessingCode,
				AutoEvents.ClearanceCompletedCode,
				AutoEvents.HeldCode,
				AutoEvents.ClearedHoldCode,
				AutoEvents.InterchangeSentCode,
				AutoEvents.InterchangeRejectedCode,
				AutoEvents.MessageSentCode
			};
	}
}
