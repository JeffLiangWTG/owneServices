using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Freight;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class BuildConsolHelper
	{
		#region Checking Methods

		public bool CheckCanBuildConsolFromSailing(JobSailing sailing)
		{
			return !sailing.Containers.Any()
				|| (sailing.Containers.IsCountEqualTo(1) && !sailing.Containers[0].PackLines.Any());
		}

		public bool CheckHasConsolidatedContainer(CommonContainer[] selectedContainers)
		{
			return selectedContainers.Any(c => !c.JC_JK.IsEmpty);
		}

		#endregion

		#region Build Consol Methods

		#region Make Consol from Booking or Standalone Shipment

		public void MakeConsolFromBookingOrStandaloneShipment(CommonConsol consol, ZGuid shipmentPK, ZGuid? quotedBookingPK = null)
		{
			var shipment = consol.Factory.Load<CommonShipment>(shipmentPK);
			if (shipment == null)
			{
				ErrorReporter.ReportOnce(
					"{0D63660A-E28A-4f61-99C1-61FAEA92BC92}",
					"Attempting to create a Consol from a non-existant CommonShipment (PK: '" + shipmentPK.ToString() + "')"
					);
			}
			else
			{
				var oldAutomaticallyUpdatePackLineContainersValue = consol.AutomaticallyUpdatePackLineContainers;
				consol.AutomaticallyUpdatePackLineContainers = false;
				try
				{
					SetConsolDefaults(consol, shipment, quotedBookingPK);
				}
				finally
				{
					consol.AutomaticallyUpdatePackLineContainers = oldAutomaticallyUpdatePackLineContainersValue;
				}
			}
		}

		void SetConsolDefaults(CommonConsol consol, CommonShipment shipment, ZGuid? quotedBookingPK)
		{
			UpdateShipmentCFSDepot(shipment);
			SetConsolAgentTypeFromShipment(consol, shipment);
			SetConsolTransportModeFromShipment(consol, shipment);
			SetConsolNumbersFromShipment(consol, shipment);

			if (shipment.JS_IsBooking && !shipment.JS_IsForwardRegistered)
			{
				consol.JK_CarrierContractNumber = shipment.JS_CarrierContractNumber;
				consol.JK_RCA_AllocationLine = shipment.JS_RCA_BookingAllocationLine;
			}

			var mostInterestingTransport = consol.Transports.MostInterestingTransport;
			mostInterestingTransport.JW_ETA = shipment.JS_E_ARV;

			consol.JK_BookingReference = shipment.JS_CFSReference;

			if (shipment.Sailing != null)
			{
				SetConsolDefaultsFromSailingOrShipment(consol, shipment.Sailing, shipment);
			}
			else
			{
				SetConsolPortsAndDatesFromShipment(consol, shipment);
				ResetIsNeutralMaster(consol);
			}

			SetupConsolCFSFromShipment(consol, shipment);

			var isStandAloneShipmentFromBooking = shipment.IsStandAloneShipmentFromBooking; // We need to save this first since it will become false when new consol is added.
			if (isStandAloneShipmentFromBooking)
			{
				ConvertStandAloneShipmentDataToConsol(shipment, consol);
			}
			else
			{
				TurnBookingIntoShipmentOnConsol(shipment, consol, quotedBookingPK: quotedBookingPK);
			}

			SetConsolDefaultsFromDirectBooking(consol, shipment);
			SetConsolForwardersFromShipment(consol, shipment);

			if (shipment.BookedShippingLineAddress != null)
			{
				consol.SetDefaultShippingLineAddress(shipment.BookedShippingLineAddress);
				if (!shipment.JS_PL_NKCarrierServiceLevel.IsEmpty)
				{
					if (consol.Transports.Any() && consol.Transports[0].CarrierPK == shipment.BookedShippingLinePK)
					{
						consol.Transports[0].JW_PL_NKCarrierServiceLevel = shipment.JS_PL_NKCarrierServiceLevel;
					}

					consol.JK_AWBServiceLevel = shipment.JS_PL_NKCarrierServiceLevel;
				}
			}

			if (shipment.Creditor != null)
			{
				consol.SetDefaultCreditor(shipment.Creditor);
			}
		}

		void SetConsolAgentTypeFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			if (shipment.IsCourier)
			{
				consol.JK_AgentType = Constants.AgentType.Courier;
			}
			else if (shipment.JS_IsDirectBooking)
			{
				consol.JK_AgentType = Constants.AgentType.Direct;
			}
		}

		void SetConsolTransportModeFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			switch (shipment.JS_TransportMode)
			{
				case Constants.TransportModes.SeaAir:
					consol.JK_TransportMode = Constants.TransportModes.Sea;
					break;

				case Constants.TransportModes.AirSea:
					consol.JK_TransportMode = Constants.TransportModes.Air;
					break;

				default:
					consol.JK_TransportMode = shipment.IsCourier ? Constants.TransportModes.Air : shipment.JS_TransportMode.ToString();
					break;
			}
		}

		void SetConsolPortsAndDatesFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort.IsEmpty
				? shipment.JS_RL_NKOrigin
				: shipment.JS_RL_NKLoadPort;

			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort.IsEmpty
				? shipment.JS_RL_NKDestination
				: shipment.JS_RL_NKDischargePort;

			consol.Transports.MostInterestingTransport.JW_ETD = shipment.JS_E_DEP;
			consol.Transports.MostInterestingTransport.JW_ETA = shipment.JS_E_ARV;
		}

		void SetConsolDefaultsFromDirectBooking(CommonConsol consol, CommonShipment shipment)
		{
			if (shipment.JS_IsDirectBooking)
			{
				consol.JK_AWBServiceLevel = shipment.JS_AWBServiceLevel;
				consol.JK_IsNeutralMaster = shipment.JS_IsNeutralMaster;
				consol.JK_MasterBillNum = shipment.JS_HouseBill;
				shipment.JS_HouseBill = ZString.Empty;
				var mawbAllocationConsol = consol as IMAWBAllocationParent;
				if (mawbAllocationConsol != null)
				{
					mawbAllocationConsol.MAWBAllocation.ReallocateMAWBToNewParent(shipment.PK, shipment.TablePrefix);
				}
			}
		}

		void SetConsolForwardersFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			var receivingForwarderAddressPK = shipment.FindRelatedReceivingForwarderAddressPK();
			var sendingForwarderAddressPK = shipment.FindRelatedSendingForwarderAddressPK();

			if (!receivingForwarderAddressPK.IsEmpty)
			{
				consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddressPK;
			}

			if (!sendingForwarderAddressPK.IsEmpty)
			{
				consol.JK_OA_SendingForwarderAddress = sendingForwarderAddressPK;
			}
		}

		void SetConsolNumbersFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			consol.Numbers.RemoveAndDeleteAll();

			foreach (CusEntryNumber number in shipment.Numbers)
			{
				var provider = consol as IAdditionalReferenceNumberTypeProvider;
				if (provider.GetAdditionalReferenceNumberTypeList(number.CE_Category, number.CE_RN_NKCountryCode).IndexOfCode(number.CE_EntryType) >= 0)
				{
					if (number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON ||
						number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount)
					{
						var consolNumber = consol.Numbers.AddNew();

						consolNumber.CE_EntryNum = number.CE_EntryNum;
						consolNumber.CE_EntryType = number.CE_EntryType;
						consolNumber.CE_EntryLineReference = number.CE_EntryLineReference;
						consolNumber.CE_EntryStatus = number.CE_EntryStatus;
						consolNumber.CE_Category = number.CE_Category;
						consolNumber.CE_IssueDate = number.CE_IssueDate;
						consolNumber.CE_RN_NKCountryCode = number.CE_RN_NKCountryCode;
						consolNumber.CE_ExpiryDate = number.CE_ExpiryDate;

						if (number.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON)
						{
							consol.JK_CarrierContractNumber = number.CE_EntryNum;
						}
					}
				}
			}
		}

		#endregion

		#region Build Consol from Sailing

		public void BuildConsolFromSailing(ICommonConsol newConsol, IJobSailing sailing)
		{
			BuildConsolFromSailing(newConsol as CommonConsol, sailing as JobSailing);
		}

		public void BuildConsolFromSailing(CommonConsol newConsol, JobSailing sailing)
		{
			newConsol.AutomaticallyUpdatePackLineContainers = false;

			var filter = GetShipmentsBySailingFilter(sailing);
			var shipment = newConsol.Factory.LoadTop1<CommonShipment>(filter);
			SetConsolDefaultsFromSailingOrShipment(newConsol, sailing, shipment);

			foreach (var booking in newConsol.Factory.Load<CommonShipment>(filter))
			{
				TurnBookingIntoShipmentOnConsol(booking, newConsol);
			}
		}

		#endregion

		#region Build Consol From Pack Containers

		public void BuildConsolFromPackContainers(CommonConsol newConsol, CommonContainer[] selectedContainers, JobSailing sailing)
		{
			newConsol.AutomaticallyUpdatePackLineContainers = false;
			foreach (var sailingContainer in selectedContainers)
			{
				var consolContainer = newConsol.Factory.Load<IForwardingContainer>(sailingContainer.PK);
				if (consolContainer != null)
				{
					consolContainer.JC_JK = newConsol.PK;
					consolContainer.JC_JX = ZGuid.Empty;
					newConsol.Containers.Add(consolContainer as BusinessObject);
				}
			}

			var filter = GetShipmentsBySailingFilter(sailing);
			var shipment = newConsol.Factory.LoadTop1<CommonShipment>(filter);
			SetConsolDefaultsFromSailingOrShipment(newConsol, sailing, shipment);
			SetupShipmentsFromPackLines(newConsol);
		}

		void SetupShipmentsFromPackLines(CommonConsol consol)
		{
			foreach (CommonContainer consolContainer in consol.Containers)
			{
				foreach (var line in consolContainer.PackLines.Cast<PackLine>().Where(p => !p.JL_JS.IsEmpty && !consol.Shipments.Contains(p.JL_JS)))
				{
					var shipment = consol.Factory.Load<CommonShipment>(line.JL_JS);
					if (shipment != null)
					{
						if (shipment.OuterPackLines.IsCountEqualTo(1))
						{
							TurnBookingIntoShipmentOnConsol(shipment, consol);
						}
						else if (shipment.OuterPackLines.IsCountMoreThan(1))
						{
							ExtractPackLinesInContainersToOwnShipment(shipment, consol);
						}
					}
				}
			}
		}

		void ExtractPackLinesInContainersToOwnShipment(CommonShipment existingShipment, CommonConsol consol)
		{
			var siblingLinesOnThisConsol = GetCommonShipmentAndConsolPackLines(existingShipment, consol);
			if (siblingLinesOnThisConsol.Count < existingShipment.OuterPackLines.Count)
			{
				existingShipment.JS_IsSplitShipment = true;

				var newShipment = (CommonShipment)existingShipment.Clone();
				newShipment.JS_UniqueConsignRef = "";
				newShipment.JS_JS_SplitSwitchShipment = existingShipment.PK;

				foreach (PackLine line in existingShipment.OuterPackLines.ToList())
				{
					if (!siblingLinesOnThisConsol.Contains(line.PK))
					{
						existingShipment.OuterPackLines.Remove(line);
						line.JL_JS = newShipment.PK;
						newShipment.OuterPackLines.Add(line);
					}
				}

				UpdateShipmentTotals(newShipment);
			}

			TurnBookingIntoShipmentOnConsol(existingShipment, consol);
		}

		PackLineNonDependentCollection GetCommonShipmentAndConsolPackLines(CommonShipment shipment, CommonConsol consol)
		{
			var siblingLinesOnThisConsol = new PackLineNonDependentCollection(consol.Factory);
			foreach (PackLine siblingLine in shipment.OuterPackLines)
			{
				foreach (CommonContainer con in consol.Containers)
				{
					if (siblingLine.Containers.Contains(con))
					{
						siblingLinesOnThisConsol.Add(siblingLine);
					}
				}
			}

			return siblingLinesOnThisConsol;
		}

		void UpdateShipmentTotals(CommonShipment shipment)
		{
			shipment.JS_OuterPacks = shipment.OuterPackLines.TotalPackages;
			shipment.JS_ActualVolume = shipment.OuterPackLines.TotalVolume;
			shipment.JS_ActualWeight = shipment.OuterPackLines.TotalWeight;
		}

		#endregion

		#region Turn Booking into Shipment

		public void TurnBookingIntoShipment(CommonShipment shipment, CommonConsol consol, ZGuid? quotedBookingPK = null)
		{
			if (!shipment.JS_IsBooking)
			{
				throw new NotSupportedException("This method can only be applied to bookings");
			}
			UpdateShipmentCFSDepot(shipment);

			var previousImportBrokerPK = shipment.JS_OH_ImportBroker;
			var previousExportBrokerPK = shipment.JS_OH_ExportBroker;

			PopulateShipmentExpectedDatesFromConsol(shipment, consol);

			if (shipment.JS_InspectionTypeCode.IsEmpty)
			{
				using (shipment.SuspendSettingHasChanges())
				using (shipment.GetValidationSuspender())
				{
					shipment.JS_InspectionTypeCode = shipment.SupplyChainSecurityConfiguration.InspectionTypeDefault;
				}
			}

			DisableProcessTasksInBooking(shipment);

			shipment.JS_IsForwardRegistered = ZBool.True;

			if (shipment.Consols.Count > 0)
			{
				shipment.JS_JX = ZGuid.Empty;
			}

			ChangeBookingConsolidationParent(shipment);
			UpdateDefaultContainerModeFromRegistry(shipment);
			MoveDocumentaryOverrides(shipment, quotedBookingPK ?? shipment.PK);
			ObjectFactory.Get<IGlobalCommercialInvoiceJobProcessor>().ConvertBookingWithQuoteToShipment(quotedBookingPK ?? ZGuid.Empty, shipment.PK, shipment.Factory);
			ObjectFactory.Get<IComplianceRiskStatusSupporter>().CopyAssessmentDetailsFromBookingToShipment(quotedBookingPK ?? ZGuid.Empty, shipment.PK, shipment.Factory);
			CopyCustomFields(shipment.PK, shipment);

			if (quotedBookingPK.HasValue && quotedBookingPK.Value != shipment.PK)
			{
				CopyCustomFields(quotedBookingPK.Value, shipment);
			}

			if (shipment.JS_OH_ImportBroker != previousImportBrokerPK)
			{
				shipment.JS_OH_ImportBroker = previousImportBrokerPK;
			}

			if (shipment.JS_OH_ExportBroker != previousExportBrokerPK)
			{
				shipment.JS_OH_ExportBroker = previousExportBrokerPK;
			}

			shipment.FieldChangeTrackerStateChanged(BusinessObjectFieldStateChangeEvent.ObjectNowRoot);
		}

		void PopulateShipmentExpectedDatesFromConsol(CommonShipment shipment, CommonConsol consol)
		{
			if (consol != null)
			{
				if (!shipment.JS_E_DEP.IsValid)
				{
					shipment.JS_E_DEP = consol.JK_JX_JA_E_DEP;
				}

				if (!shipment.JS_E_ARV.IsValid)
				{
					shipment.JS_E_ARV = consol.JK_JX_JB_E_ARV;
				}
			}
		}

		void UpdateDefaultContainerModeFromRegistry(CommonShipment shipment)
		{
			var defaultContainerType = shipment.GetRegistryDefaultContainerMode();
			if (!defaultContainerType.IsEmpty)
			{
				shipment.JS_PackingMode = defaultContainerType;
			}
		}

		void MoveDocumentaryOverrides(CommonShipment shipment, ZGuid quotedBookingPK)
		{
			var query = new ZQuery(JobDocumentDataSchema.JDD_ParentTableCode, ViewQuotedBookingSchema.Constants.Prefix);
			query.AddToFilter(JobDocumentDataSchema.JDD_ParentID, quotedBookingPK);

			var documentaryOverrides = shipment.Factory.Load<VisualizerDocumentData>(query);

			foreach (var documentaryOverride in documentaryOverrides)
			{
				documentaryOverride.JDD_ParentID = shipment.PK;
				documentaryOverride.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}
		}

		#endregion

		#region Add Bookings to Consol

		public void AddBookingsToConsol(CommonConsol consol, ZGuid[] shipmentPKs, bool needUpdateConsolMode = true, ZGuid? quotedBookingPK = null)
		{
			PreAddBookingsToConsol(consol);

			foreach (var shipmentPK in shipmentPKs)
			{
				AddBookingsToConsolCore(consol, shipmentPK, quotedBookingPK, needUpdateConsolMode);
			}
		}

		public void AddBookingsToConsol(CommonConsol consol, Tuple<ZGuid, ZGuid>[] shipmentPKsAndQuotedBookingPKs, bool needUpdateConsolMode = true)
		{
			PreAddBookingsToConsol(consol);

			foreach (var (shipmentPK, quotedBookingPK) in shipmentPKsAndQuotedBookingPKs)
			{
				AddBookingsToConsolCore(consol, shipmentPK, quotedBookingPK, needUpdateConsolMode);
			}
		}

		void PreAddBookingsToConsol(CommonConsol consol)
		{
			consol.AutomaticallyUpdatePackLineContainers = false;
		}

		void AddBookingsToConsolCore(CommonConsol consol, ZGuid shipmentPK, ZGuid? quotedBookingPK, bool needUpdateConsolMode)
		{
			var shipment = consol.Factory.Load<CommonShipment>(shipmentPK);
			TurnBookingIntoShipmentOnConsol(shipment, consol, needUpdateConsolMode, quotedBookingPK);
			consol.AllocateShipment(shipment);
		}

		#endregion

		#region Add Stand Alone Shipment to Consol

		public void AddStandAloneShipmentToConsol(CommonConsol consol, CommonShipment standaloneShipment)
		{
			var shipment = consol.Factory == standaloneShipment.Factory
				? standaloneShipment
				: consol.Factory.Load<CommonShipment>(standaloneShipment.PK);

			consol.AutomaticallyUpdatePackLineContainers = false;

			ConvertStandAloneShipmentDataToConsol(shipment, consol);
			consol.AllocateShipment(shipment);

			if (shipment.BookedShippingLineAddress != null)
			{
				if (!consol.IsInDatabase)
				{
					consol.SetDefaultShippingLineAddress(shipment.BookedShippingLineAddress);
				}
			}

			consol.IsAttachedToStandAloneShipment = true;
		}

		#endregion

		#endregion

		#region Helper methods

		void ConvertStandAloneShipmentDataToConsol(CommonShipment shipment, CommonConsol consol)
		{
			if (!shipment.JS_IsBooking || !shipment.JS_IsForwardRegistered)
			{
				throw new NotSupportedException("This method can only be applied to standalone shipment.");
			}

			if (!consol.IsInDatabase)
			{
				SetConsolModeFromShipment(consol, shipment);
			}

			if (consol.JK_BookingReference.IsEmpty)
			{
				consol.JK_BookingReference = shipment.JS_CFSReference;
			}

			SetupConsolCFSFromShipment(consol, shipment);

			if (shipment.Sailing != null)
			{
				if (!consol.IsInDatabase)
				{
					SetNewConsolDefaultsFromSailing(consol, shipment.Sailing);
				}

				ResetIsNeutralMaster(consol);
			}

			shipment.JS_JX = ZGuid.Empty;

			ChangeBookingConsolidationParent(shipment);
			DisableProcessTasksInBooking(shipment);

			CopyCustomFields(shipment.PK, shipment);

			consol.Shipments.Add(consol.Factory.Load<CommonShipment>(shipment.PK));
			TurnBookingContainersIntoConsolContainers(shipment, consol);
		}

		void SetConsolModeFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			if (shipment.IsCourier)
			{
				consol.JK_ConsolMode = Constants.ContainerModes.Other;
			}
			else if (shipment.IsAir && shipment.JS_PackingMode == Constants.ContainerModes.LCL)
			{
				consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			}
			else if (shipment.IsSea && (shipment.JS_PackingMode == Constants.ContainerModes.Loose || shipment.JS_PackingMode == Constants.ContainerModes.ULD))
			{
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			}
			else if (!consol.JK_ConsolMode_List.ContainsCode(shipment.JS_PackingMode))
			{
				consol.JK_ConsolMode = "";
			}
			else
			{
				consol.JK_ConsolMode = shipment.JS_PackingMode;
			}
		}

		void SetupConsolCFSFromShipment(CommonConsol consol, CommonShipment shipment)
		{
			if (shipment.JS_TransportMode == Constants.TransportModes.Air
				|| shipment.JS_PackingMode == Constants.ContainerModes.LCL
				|| shipment.JS_PackingMode == Constants.ContainerModes.LTL
				|| shipment.JS_PackingMode == Constants.ContainerModes.FTL)
			{
				if (consol.JK_OA_PackDepotAddress.IsEmpty)
				{
					consol.JK_OA_PackDepotAddress = shipment.JS_OA_ExportReceivingDepot;
				}
			}
			else if (consol.JK_OA_DepartureCTOAddress.IsEmpty)
			{
				consol.JK_OA_DepartureCTOAddress = shipment.JS_OA_ExportReceivingDepot;
			}
		}

		void SetNewConsolDefaultsFromSailing(CommonConsol consol, JobSailing sailing)
		{
			consol.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol.JK_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
			consol.SetDefaultShippingLineAddress(sailing.JX_JV_OH_Line);

			consol.Transports[0].JW_IsLinked = true;
			consol.Transports[0].JW_JX = sailing.PK;
		}

		void ResetIsNeutralMaster(CommonConsol consol)
		{
			if (!consol.IsAir)
			{
				consol.JK_IsNeutralMaster = false;
			}
		}

		void ChangeBookingConsolidationParent(CommonShipment shipment)
		{
			foreach (var consolidatedBooking in LoadBookingConsolidation(shipment))
			{
				consolidatedBooking.KB_ParentID = shipment.PK;
				consolidatedBooking.KB_ParentTableCode = shipment.TablePrefix;
			}
		}

		IDtbBookingConsolidation[] LoadBookingConsolidation(CommonShipment shipment)
		{
			var quotedBooking = shipment.Factory.Load<IQuotedBooking>(shipment.PK) as IDtbBookingParent;
			return quotedBooking != null ? TransportBookingLoader.GetBookingConsolidations(quotedBooking) : Array.Empty<IDtbBookingConsolidation>();
		}

		void DisableProcessTasksInBooking(CommonShipment shipment)
		{
			var quotedBooking = shipment.Factory.Load<IQuotedBooking>(shipment.PK) as IWorkflowProvider;
			quotedBooking?.WorkflowItems.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);
			quotedBooking?.DisableProcessTasks();
		}

		void CopyCustomFields(ZGuid fromBizoPK, CommonShipment shipment)
		{
			if (!(shipment is null))
			{
				CustomFieldsConversionHelper.CopyCustomFields(shipment.Factory,
					ViewQuotedBookingSchema.Constants.Prefix, fromBizoPK, shipment.TablePrefix, shipment.PK);
			}
		}

		void TurnBookingContainersIntoConsolContainers(CommonShipment shipment, CommonConsol consol)
		{
			if (shipment != null
				&& (shipment.JS_PackingMode == Constants.ContainerModes.FCL || shipment.JS_PackingMode == Constants.ContainerModes.FTL))
			{
				var quotedBooking = shipment.Factory.Load<IQuotedBooking>(shipment.PK);

				if (quotedBooking != null)
				{
					PopulateConsolContainers(shipment, consol, quotedBooking);
				}

				shipment.JS_ActualVolume = shipment.OuterPackLines.TotalVolume;
				shipment.JS_ActualWeight = shipment.OuterPackLines.TotalWeight;
			}
		}

		void PopulateConsolContainers(CommonShipment shipment, CommonConsol consol, IQuotedBooking quotedBooking)
		{
			var nextFreePackLine = shipment.OuterPackLines.Count - 1;
			foreach (CommonContainer qbContainer in quotedBooking.QuotedBookingContainers)
			{
				PackLine line;
				if (nextFreePackLine < 0)
				{
					line = shipment.OuterPackLines.AddNew();
					line.JL_FreightMode = FreightConstants.OuterPackType;
					line.JL_ActualWeight = qbContainer.JC_GrossWeight - qbContainer.JC_TareWeight;
					line.JL_ActualWeightUQ = Constants.Weight.Kilograms;
				}
				else
				{
					line = shipment.OuterPackLines[nextFreePackLine--];
				}

				qbContainer.JC_ContainerMode = shipment.JS_PackingMode;

				shipment.OuterPackLines.Add(line);
				AddLineToContainerAndConsol(consol, qbContainer.PK, line);
			}
		}

		protected virtual void AddLineToContainerAndConsol(CommonConsol consol, ZGuid containerPK, PackLine line)
		{
			var container = consol.Factory.Load<CommonContainer>(containerPK);

			var consolContainer = AddNewOrGetExistingContainerFromConsolContainers(consol, container);
			line.SetContainer(consol, consolContainer);
		}

		protected CommonContainer AddNewOrGetExistingContainerFromConsolContainers(CommonConsol consol, CommonContainer container)
		{
			var existingContainer = consol.Containers.Cast<CommonContainer>().FirstOrDefault(cnt => !cnt.JC_ContainerNum.EqualsIgnoringCase(ZString.Empty) && cnt.JC_ContainerNum.EqualsIgnoringCase(container.JC_ContainerNum));

			if (existingContainer != null)
			{
				return existingContainer;
			}
			else
			{
				try
				{
					consol.Containers.Add(container);
				}
				catch (CannotAddToCollectionException ex)
				{
					ReportCannotAddToCollectionException(consol, ex);
				}
				return container;
			}
		}

		void ReportCannotAddToCollectionException(CommonConsol consol, CannotAddToCollectionException ex)
		{
			var shipment = consol.Shipments.First();
			var quotedBooking = shipment.Factory.Load<IQuotedBooking>(shipment.PK);
			var containerDetails = (CommonContainer container) => $"Container:\r\n{container.GetAllPropertyValues()}\r\n\r\n{container.GetParentCollectionsInfo()}";
			var message = string.Join("\r\n", quotedBooking.QuotedBookingContainers.ToArray<CommonContainer>().Select(containerDetails));
			ErrorReporter.ReportOnce("Could not add container to Consol", message, ex);
		}

		void SetConsolDefaultsFromSailingOrShipment(CommonConsol consol, JobSailing sailing, CommonShipment shipment = null)
		{
			if (sailing != null)
			{
				if (shipment != null)
				{
					consol.JK_BookingReference = shipment.JS_CFSReference;
					SetupConsolCFSFromShipment(consol, shipment);
				}

				consol.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;

				consol.JK_RL_NKLoadPort = string.IsNullOrEmpty(shipment?.JS_RL_NKLoadPort)
					? sailing.JX_JA_RL_NKPortOfLoading
					: shipment.JS_RL_NKLoadPort;

				consol.JK_RL_NKDischargePort = string.IsNullOrEmpty(shipment?.JS_RL_NKDischargePort)
					? sailing.JX_JB_RL_NKPortOfDischarge
					: shipment.JS_RL_NKDischargePort;

				consol.SetDefaultShippingLineAddress(sailing.JX_JV_OH_Line);

				consol.Transports[0].JW_IsLinked = true;
				consol.Transports[0].JW_JX = sailing.PK;
			}

			ResetIsNeutralMaster(consol);
		}

		void TurnBookingIntoShipmentOnConsol(CommonShipment shipment, CommonConsol consol, bool needUpdateConsolMode = true, ZGuid? quotedBookingPK = null)
		{
			if (needUpdateConsolMode && !consol.IsInDatabase)
			{
				SetConsolModeFromShipment(consol, shipment);
			}

			AddBookingToConsol(shipment, consol, quotedBookingPK);
			shipment.JS_JX = ZGuid.Empty;
		}

		public event EventHandler<BuildConsolEventArgs> ShipmentCannotBeAttachedToConsol;

		protected virtual void AddBookingToConsol(CommonShipment shipment, CommonConsol consol, ZGuid? quotedBookingPK = null)
		{
			if (shipment.JS_IsBooking && shipment.JS_IsForwardRegistered)
			{
				var attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(consol, shipment);
				if (!attachRequest.Errors.IsEmpty)
				{
					ShipmentCannotBeAttachedToConsol?.Invoke(this, new BuildConsolEventArgs(attachRequest.Errors));
				}
				else
				{
					consol.Shipments.Add(shipment);
				}
				return;
			}
			TurnBookingIntoShipment(shipment, consol, quotedBookingPK);
			consol.Shipments.Add(consol.Factory.Load(consol.Shipments.TypeOfElements, shipment.PK));
			TurnBookingContainersIntoConsolContainers(shipment, consol);
		}

		ZQuery GetShipmentsBySailingFilter(JobSailing sailing)
		{
			var filter = new ZQuery(JobShipmentSchema.JS_JX, sailing.PK);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsBooking, ZBool.True), JoinCondition.And);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, ZBool.False), JoinCondition.And);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsCancelled, ZBool.False), JoinCondition.And);

			if (sailing.Voyage != null && sailing.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air)
			{
				filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_PackingMode, Enterprise.Core.Constants.ContainerModes.Loose));
			}
			else
			{
				filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_PackingMode, Enterprise.Core.Constants.ContainerModes.LCL));
			}

			return filter;
		}

		void UpdateShipmentCFSDepot(CommonShipment shipment)
		{
			if (shipment.JS_IsCFSRegistered || !shipment.JS_IsBooking || shipment.JS_IsForwardRegistered)
			{
				return;
			}

			if (IsDepotProxyOrg(shipment.ExportReceivingDepot) || IsDepotProxyOrg(shipment.ImportReleaseDepot))
			{
				shipment.JS_IsCFSRegistered = true;
			}
		}

		bool IsDepotProxyOrg(OrgAddress depot)
		{
			return depot != null && depot.Header != null && depot.Header.IsProxyOrgOfAnyCompany();
		}

		#endregion
	}

	public class BuildConsolEventArgs : EventArgs
	{
		public BuildConsolEventArgs(ZString message)
		{
			Message = message;
		}

		public ZString Message { get; }
	}
}
