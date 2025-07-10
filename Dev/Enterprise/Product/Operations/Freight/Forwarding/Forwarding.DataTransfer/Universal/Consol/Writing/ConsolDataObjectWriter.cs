using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ConsolDataObjectWriter : TopLevelDataObjectWriter<ForwardingConsol, UniversalShipment>, IContainerParentDataObjectWriter
	{
		public ConsolDataObjectWriter(IDataWritingManager manager)
			: this(manager, null, new DataWriterOptions())
		{
		}

		public ConsolDataObjectWriter(IDataWritingManager manager, IContainerLinkManager<ForwardingConsol> linkManager, DataWriterOptions dataWriterOptions)
			: base(manager)
		{
			this.linkManager = linkManager;
			this.dataWriterOptions = dataWriterOptions;
		}

		readonly DataWriterOptions dataWriterOptions;
		protected IContainerLinkManager<ForwardingConsol> linkManager;

		protected override void PopulateDataObject(ForwardingConsol consolBO, UniversalShipment shipmentData)
		{
			var listCache = BindToLists.GetCachedLists(consolBO.Factory);
			var linkManagerInitializedExplicitly = EnsureLinkManagerIsInitialized(consolBO);

			PopulateShipmentData(shipmentData, consolBO);

			if (dataWriterOptions.IncludeContainers)
			{
				PopulateContainerData(shipmentData, consolBO, listCache, CollectionContent.Complete);
			}

			shipmentData.SetTransportLegCollection(() => ProcessCollection(consolBO.Transports, new TransportLegDataObjectWriter(writeManager, consolBO), CollectionContent.Complete, true));
			var notes = consolBO.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			shipmentData.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
			shipmentData.SetOrganizationAddressCollection(() => ProcessCollection(consolBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));

			shipmentData.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(Date.New(DateType.ShippedOnBoard, ZBool.False, consolBO.JK_ShippedOnBoardDate));
				dates.Add(Date.New(DateType.BillIssued, ZBool.False, consolBO.JK_MasterBillIssueDate));
				dates.Add(Date.New(DateType.FirstArrivalInCountry, ZBool.False, consolBO.JK_DatePortOfFirstArrival));
				dates.Add(Date.New(DateType.FirstForeignArrival, ZBool.False, consolBO.JK_DateFirstForeignPort));
				dates.Add(Date.New(DateType.LastForeignDeparture, ZBool.False, consolBO.JK_DateLastForeignPort));
				dates.Add(Date.New(DateType.CutOffDate, ZBool.False, consolBO.JK_ConsolCutOffDate));
				dates.Add(Date.New(DateType.DepartureReceiptRequested, ZBool.False, consolBO.JK_PackDepotReceiptRequested));
				dates.Add(Date.New(DateType.ArrivalReceiptRequested, ZBool.False, consolBO.JK_UnpackDepotReceiptRequested));
				dates.Add(Date.New(DateType.DepartureDispatchRequested, ZBool.False, consolBO.JK_PackDepotDispatchRequested));
				dates.Add(Date.New(DateType.ArrivalDispatchRequested, ZBool.False, consolBO.JK_UnpackDepotDispatchRequested));
				return dates;
			});
			shipmentData.IsHazardous = consolBO.JK_IsHazardous;
			shipmentData.SetPreallocatedUNDGCollection(() => ProcessCollection(consolBO.ConsolDGRestrictionCollection, new ConsolRestrictionUNDGDataObjectWriter(writeManager), CollectionContent.Complete));

			shipmentData.AddOrgAddress(writeManager, consolBO.ArrivalUnpackCFSTransportAddress, DocAddressType.ArrivalCFSLocalTransportAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.DeparturePackCFSTransportAddress, DocAddressType.DepartureCFSLocalTransportAddress);

			shipmentData.AddOrgAddress(writeManager, consolBO.CreditorAddress, DocAddressType.Creditor);
			if (consolBO.IsCoLoad)
			{
				shipmentData.AddOrgAddress(writeManager, consolBO.CreditorAddress, DocAddressType.CoLoadWith);
			}

			shipmentData.AddOrgAddress(writeManager, consolBO.ReceivingForwarderAddress, DocAddressType.ReceivingForwarderAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.ShippingLineAddress, DocAddressType.ShippingLineAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.SendingForwarderAddress, DocAddressType.SendingForwarderAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.ArrivalCTOAddress, DocAddressType.ArrivalCTOAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.DepartureCTOAddress, DocAddressType.DepartureCTOAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.UnpackDepotAddress, DocAddressType.ArrivalCFSAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.PackDepotAddress, DocAddressType.DepartureCFSAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.ContainerYardEmptyPickupAddress, DocAddressType.ContainerYardEmptyPickupAddress);
			shipmentData.AddOrgAddress(writeManager, consolBO.ContainerYardEmptyReturnAddress, DocAddressType.ContainerYardEmptyReturnAddress);

			CustomLabelsCustomizedFieldDataObjectWriter.Write(JobConsolSchema.Instance, consolBO, shipmentData, new ForwardingConsol.CustomLabelsProvider(consolBO));

			shipmentData.SetEntryNumberCollection(() => ProcessCollection(consolBO.CusEntryNums, new EntryNumberDataObjectWriter(writeManager)));
			shipmentData.SetAdditionalReferenceCollection(() => ProcessCollection(consolBO.Numbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
			if (!consolBO.JK_CarrierContractNumber.IsEmpty)
			{
				var additionalReferenceCollection = shipmentData.AdditionalReferenceCollection ?? new DataObjectList<AdditionalReference>();
				additionalReferenceCollection.Add(new AdditionalReference { Type = new EntryType { Code = "CON", Description = (NoResString)"Carrier Contract Number" }, ReferenceNumber = consolBO.JK_CarrierContractNumber });
				shipmentData.SetAdditionalReferenceCollection(() => additionalReferenceCollection);
			}

			if (dataWriterOptions.IncludeSubShipments)
			{
				if (consolBO.JK_AgentType == Constants.AgentType.AWBMaster)
				{
					var coloadConsolDataObjectWriter = new ConsolDataObjectWriter(writeManager, null, dataWriterOptions);
					var data = ProcessCollection(consolBO.ColoadConsols, coloadConsolDataObjectWriter);
					shipmentData.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
				}
				else
				{
					shipmentData.SetSubShipmentCollection(() => PopulateSubShipmentCollection(consolBO));
				}
			}

			if (linkManagerInitializedExplicitly)
			{
				linkManager = null;
			}

			PopulateSpecialHandling(consolBO, shipmentData);

			PopulateCO2eFields(consolBO, shipmentData);

			PopulateAdditionalAddressInfoCollection(consolBO, shipmentData);

			//Please keep this one the last to call as it will override Freight data.
			MergeAUCargoDataIfNeeded(consolBO, shipmentData);
		}

		void PopulateCO2eFields(ForwardingConsol consolBO, UniversalShipment shipmentData)
		{
			shipmentData.GreenhouseGasEmission = new GreenhouseGasEmission
			{
				CO2e = consolBO.GetTotalCO2e(),
				CO2eUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, consolBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)),
				CO2eDescriptiveStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.GetCO2eStatus(), new CO2eStatusList())
					.AdditionalSetup(cdp => cdp.Description = CO2eHelper.GetCO2eStatusShortDescription(cdp.Code)),
			};
		}

		void PopulateAdditionalAddressInfoCollection(ForwardingConsol consolBO, UniversalShipment shipmentData)
		{
			shipmentData.SetAdditionalAddressInfoCollection(() => ProcessCollection(consolBO.JobAddressAdditionalInfoCollection, new JobAddressAdditionalInfoDataObjectWriter(writeManager)));
		}

		void PopulateContainerData(UniversalShipment shipmentData, ForwardingConsol consolBO, BindToLists listCache, CollectionContent contentType)
		{
			shipmentData.SetContainerCollection(() => ProcessCollection(GetContainersToWrite(consolBO), new ForwardingContainerWithPackLinesDataObjectWriter(linkManager, listCache, writeManager), contentType, true));
		}

		IEnumerable<ForwardingContainer> GetContainersToWrite(ForwardingConsol consol)
		{
			return ContainerOverride != null ? new[] { ContainerOverride } : consol.Containers.Cast<ForwardingContainer>();
		}

		ForwardingContainer ContainerOverride { get; set; }

		void MergeAUCargoDataIfNeeded(ForwardingConsol consolBO, UniversalShipment shipmentData)
		{
			var hasHVLVClearanceAgent = HasRecipientRole(RecipientRoleType.HCA) || HasRecipientRole(RecipientRoleType.HSA);
			var shouldMergeWithAUCusMAWB = HasRecipientRole(RecipientRoleType.HCA) || HasRecipientRole(RecipientRoleType.AAD);
			var shouldMergeWithAUOceanBill = HasRecipientRole(RecipientRoleType.HSA);
			if (shouldMergeWithAUCusMAWB)
			{
				MergeDataObjectWriterManager.AddBizObjData(writeManager, shipmentData, (BusinessObject)consolBO.AUCusMAWB, includeParent: !hasHVLVClearanceAgent, includeChildren: !hasHVLVClearanceAgent);
			}
			else if (shouldMergeWithAUOceanBill)
			{
				MergeDataObjectWriterManager.AddBizObjData(writeManager, shipmentData, (BusinessObject)consolBO.AUCMRCusSCAOceanBill, includeParent: !hasHVLVClearanceAgent, includeChildren: !hasHVLVClearanceAgent);
			}
		}

		void CreateCarrierDocumentsSection(UniversalShipment shipmentData)
		{
			if (shipmentData.CarrierDocumentsOverride == null)
			{
				shipmentData.CarrierDocumentsOverride = new CarrierDocumentsOverride();
			}
		}

		void PopulateSpecialHandling(ForwardingConsol consolBO, UniversalShipment shipmentDO)
		{
			if (consolBO.JK_TransportMode == Constants.TransportModes.Air)
			{
				shipmentDO.SetSpecialHandlingCollection(() => ProcessCollection(consolBO.AWBSpecialHandlingItems, new SpecialHandlingDataObjectWriter(writeManager)));
			}
		}

		void PopulateShipmentData(UniversalShipment shipmentData, ForwardingConsol consolBO)
		{
			shipmentData.ShipmentType = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_AgentType, consolBO.JK_AgentType_List);
			shipmentData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_TransportMode, consolBO.JK_TransportMode_List);
			shipmentData.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(consolBO.JK_ConsolMode, consolBO.JK_ConsolMode_List);

			shipmentData.SendingForwarderHandlingType = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_SendingForwarderHandlingType, consolBO.GatewayHandlingTypeList);
			shipmentData.ReceivingForwarderHandlingType = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_ReceivingForwarderHandlingType, consolBO.GatewayHandlingTypeList);

			shipmentData.AgentsReference = consolBO.JK_AgentsReference;
			shipmentData.BookingConfirmationReference = consolBO.JK_BookingReference;
			shipmentData.CarrierContractNumber = consolBO.JK_CarrierContractNumber;
			shipmentData.ConsolCommodity = ListHelper.GetWithDescription<Commodity>(consolBO.JK_RH_NKConsolCommodity, consolBO.JK_RH_NKConsolCommodity_List);

			shipmentData.PortOfDischarge = ListHelper.GetWithName(consolBO.JK_RL_NKDischargePort, consolBO.RefUNLOCO_List);
			shipmentData.PlaceOfDelivery = ListHelper.GetWithName(consolBO.JK_RL_NKDischargePort, consolBO.RefUNLOCO_List);
			shipmentData.PlaceOfIssue = ListHelper.GetWithName(consolBO.JK_RL_NKMasterBillIssuePlace, consolBO.RefUNLOCO_List);
			shipmentData.PortOfFirstArrival = ListHelper.GetWithName(consolBO.JK_RL_NKPortOfFirstArrival, consolBO.RefUNLOCO_List);

			shipmentData.PortOfLoading = ListHelper.GetWithName(consolBO.JK_RL_NKLoadPort, consolBO.RefUNLOCO_List);
			shipmentData.PlaceOfReceipt = ListHelper.GetWithName(consolBO.JK_RL_NKLoadPort, consolBO.RefUNLOCO_List);

			shipmentData.PortFirstForeign = ListHelper.GetWithName(consolBO.JK_RL_NKFirstForeignPort, consolBO.RefUNLOCO_List);
			shipmentData.PortLastForeign = ListHelper.GetWithName(consolBO.JK_RL_NKLastForeignPort, consolBO.RefUNLOCO_List);

			if (!string.IsNullOrEmpty(GlbBranch.CurrentBranch?.GB_RL_NKHomePort))
			{
				shipmentData.EventBranchHomePort = ListHelper.GetWithName(GlbBranch.CurrentBranch.GB_RL_NKHomePort, consolBO.RefUNLOCO_List);
			}

			shipmentData.VesselName = consolBO.JK_JX_JV_NKVessel;
			shipmentData.LloydsIMO = consolBO.Vessel.GetLloydsIMO();
			shipmentData.VoyageFlightNo = consolBO.JK_JX_JV_VoyageFlight;

			if (consolBO.JK_TransportMode == Constants.TransportModes.Air)
			{
				shipmentData.WayBillNumber = consolBO.JK_MasterBillNum.FormatAirMAWB();
			}
			else
			{
				shipmentData.WayBillNumber = consolBO.JK_MasterBillNum;
			}

			try
			{
				if (consolBO.IsAir
					&& consolBO.AWBHeader != null
					&& !suppressCarrierDocumentsOverride)
				{
					CreateCarrierDocumentsSection(shipmentData);
					shipmentData.CarrierDocumentsOverride.AWBHeader = new AWBHeaderDataObjectWriter(writeManager).GetDataObject(consolBO.AWBHeader);
				}
			}
			catch (ExportAWBHeaderReplaceMacrosException e)
			{
				throw new DataObjectValidationException(e.Message);
			}
			catch (Exception e) when (e.InnerException is ExportAWBHeaderReplaceMacrosException)
			{
				throw new DataObjectValidationException(e.InnerException.Message);
			}

			shipmentData.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.Master, new WayBillTypeList());
			shipmentData.AWBServiceLevel = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_AWBServiceLevel, consolBO.NeutralAirWaybillServiceLevelList);
			shipmentData.GatewayServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(consolBO.JK_RS_NKGatewayServiceLevel, consolBO.Lookups.GatewayServiceLevels);
			shipmentData.PaymentMethod = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_PrepaidCollect, consolBO.JK_PrepaidCollect_List);
			shipmentData.ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_ScreeningStatus, consolBO.ScreeningStatusesList);
			shipmentData.ReleaseType = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_ReleaseType, consolBO.JK_ReleaseType_List);

			shipmentData.ContainerCount = consolBO.JK_Calc_ContainerCount;
			shipmentData.DocumentedChargeable = consolBO.JK_TotalDocumentedChargeable;
			shipmentData.DocumentedVolume = consolBO.JK_TotalDocumentedVolume;
			shipmentData.DocumentedWeight = consolBO.JK_TotalDocumentedWeight;
			shipmentData.FreightRate = consolBO.JK_ConsolChargeableRate;

			if (consolBO.AWBCurrency != null)
			{
				shipmentData.FreightRateCurrency = ListHelper.GetWithDescription<Currency>(consolBO.AWBCurrency.RX_Code, consolBO.RefCurrency_List);
			}

			shipmentData.IsCFSRegistered = consolBO.JK_IsCFS;
			shipmentData.IsNeutralMaster = new IsNeutralMaster { Value = consolBO.JK_IsNeutralMaster };
			shipmentData.IsForwardRegistered = consolBO.JK_IsForwarding;
			shipmentData.IsDirectBooking = consolBO.IsDirect;

			shipmentData.ManifestedChargeable = consolBO.JK_TotalManifestedChargeable;
			shipmentData.ManifestedVolume = consolBO.JK_TotalManifestedVolume;
			shipmentData.ManifestedWeight = consolBO.JK_TotalManifestedWeight;
			shipmentData.NoCopyBills = consolBO.JK_NoCopyBills;
			shipmentData.NoOriginalBills = consolBO.JK_NoOriginalBills;
			ReplaceOverFlowExceptionWithDataObjectValidationException(
				ResString.GetMultilingualString("1C795C09-7091-45F1-AEBD-723EE78EB8BB", "Error: Total shipments package count ({0}) exceeds the maximum allowed quantity ({1}).", consolBO.JK_TotalShipmentPackageCount, int.MaxValue),
				() => shipmentData.OuterPacks = consolBO.JK_TotalShipmentQuantity.ToZInt());
			shipmentData.TotalNoOfPacksPackageType = new PackageType() { Code = Constants.PkgUnit.Package, Description = Constants.PkgUnit.GetDescription(Constants.PkgUnit.Package) };
			ReplaceOverFlowExceptionWithDataObjectValidationException(
				ResString.GetMultilingualString("c089b97f-5790-4d58-8a3f-d7ce27fb7955", "Error: Total shipments inner package count ({0}) exceeds the maximum allowed quantity ({1}).", consolBO.JK_TotalShipmentQuantity, int.MaxValue),
				() => shipmentData.TotalNoOfPacks = consolBO.JK_TotalShipmentPackageCount.ToZInt());
			shipmentData.TotalVolume = consolBO.JK_TotalShipmentVolume;
			shipmentData.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(consolBO.JK_TotalShipmentVolumeUnit, consolBO.JK_Calc_ShipmentVolumeUnit_List);
			shipmentData.TotalWeight = consolBO.JK_TotalShipmentWeight;
			shipmentData.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(consolBO.JK_TotalShipmentWeightUnit, consolBO.JK_Calc_ShipmentWeightUnit_List);
			shipmentData.TotalPreallocatedWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(consolBO.WeightVerificationUnit, consolBO.JK_Calc_ShipmentWeightUnit_List);
			shipmentData.TotalPreallocatedWeight = consolBO.JK_TotalShipmentActWeightCheck;
			shipmentData.TotalPreallocatedVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(consolBO.VolumeVerificationUnit, consolBO.JK_Calc_ShipmentVolumeUnit_List);
			shipmentData.TotalPreallocatedVolume = consolBO.JK_TotalShipmentActVolumeCheck;
			shipmentData.TotalPreallocatedChargeable = consolBO.JK_TotalShipmentChargableCheck;
			shipmentData.ChargeableRate = consolBO.JK_ConsolChargeableRate;

			if (consolBO.JK_TotalShipmentLoadingMeters > 0)
			{
				shipmentData.TotalLoadingMeters = consolBO.JK_TotalShipmentLoadingMeters;
			}

			shipmentData.RequiresTemperatureControl = consolBO.JK_RequiresTemperatureControl;
			if (consolBO.JK_RequiresTemperatureControl)
			{
				shipmentData.RequiredTemperatureMinimum = consolBO.JK_RequiredTemperatureMinimum;
				shipmentData.RequiredTemperatureMaximum = consolBO.JK_RequiredTemperatureMaximum;
				shipmentData.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Constants.Temperature.Centigrade, Description = Constants.Temperature.GetDescription(Constants.Temperature.Centigrade) };
			}

			if (consolBO.IsCoLoad)
			{
				shipmentData.CoLoadMasterBillNumber = consolBO.JK_CoLoadMasterBill;
				shipmentData.CoLoadBookingConfirmationReference = consolBO.JK_CoLoadBookingReference;
			}

			if (consolBO.JK_OverrideConsolChargeable)
			{
				shipmentData.CarrierCorrectedWeight = consolBO.JK_CorrectedConsolWeight;
				shipmentData.CarrierCorrectedWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(consolBO.JK_CorrectedConsolWeightUnit, consolBO.JK_Calc_ShipmentWeightUnit_List);
				shipmentData.CarrierCorrectedVolume = consolBO.JK_CorrectedConsolVolume;
				shipmentData.CarrierCorrectedVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(consolBO.JK_CorrectedConsolVolumeUnit, consolBO.JK_Calc_ShipmentVolumeUnit_List);
				shipmentData.CarrierCorrectedChargeable = consolBO.JK_ConsolChargeable;
			}

			shipmentData.MaximumAllowablePackageLength = consolBO.JK_MaximumAllowablePackageLength;
			shipmentData.MaximumAllowablePackageWidth = consolBO.JK_MaximumAllowablePackageWidth;
			shipmentData.MaximumAllowablePackageHeight = consolBO.JK_MaximumAllowablePackageHeight;
			shipmentData.MaximumAllowablePackageLengthUnit = ListHelper.GetWithDescription<UnitOfLength>(consolBO.JK_MaximumAllowablePackageUnit, consolBO.JK_PackageUnit_List);

			if (consolBO.IsSea)
			{
				shipmentData.CarrierBookingOffice = ListHelper.GetWithName(consolBO.JK_RL_NKCarrierBookingOffice, consolBO.RefUNLOCO_List);
				shipmentData.CarrierBookingLatestStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(consolBO.JK_Calc_CarrierBookingLatestStatus, consolBO.JK_CarrierBookingStatus_List);
				shipmentData.CarrierBookingLatestDate = consolBO.JK_Calc_CarrierBookingLatestDate;
			}

			shipmentData.ElectronicBillOfLadingReference = consolBO.JK_ElectronicBillOfLadingReference;
		}

		/// <summary>
		///		Checks wether the <see cref="ListManager"/> is initialized, and initialize it if it wasn't.
		/// </summary>
		/// <returns>
		///		true, if the object was explicitly initialized; otherwise (the object is already initialized), false.
		/// </returns>
		bool EnsureLinkManagerIsInitialized(ForwardingConsol consolBO)
		{
			if (linkManager == null)
			{
				// TODO: Implement factory for creating instances
				linkManager = new ContainerLinkManager<ForwardingConsol>(consolBO);

				return true;
			}
			else
			{
				return false;
			}
		}

		protected virtual DataObjectList<UniversalShipment> PopulateSubShipmentCollection(ForwardingConsol consolBO)
		{
			var shipments = consolBO.TopLevelShipments;
			List<UniversalShipment> data = null;
			if (writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
			{
				data = ProcessCollection(shipments, new ShipmentDataObjectWriter(writeManager, linkManager, true, true, consolBO));
			}
			else
			{
				data = ProcessCollection(shipments, new ShipmentDataObjectWriter(writeManager, linkManager, true, false));
			}

			return data != null ? new DataObjectList<UniversalShipment>(data) : null;
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingConsol;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(ForwardingConsol consolBO)
		{
			return consolBO.GetUserDefinedValues();
		}

		protected override bool ShouldSendConsolCostsData(ForwardingConsol sourceBO)
		{
			var recipientDetails = writeManager.Action.RecipientRoleDetails;
			var sentToOrpOnly = recipientDetails != null && recipientDetails.Length == 1 && recipientDetails[0].Type == RecipientRoleType.ORP;
			return sentToOrpOnly || eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.Value;
		}

		#region IContainerParentDataObjectWriter

		void IContainerParentDataObjectWriter.PopulateContainer(BusinessObject sourceBO, ICommonContainer containerBO, ITopLevelDataObject dataObject)
		{
			ContainerOverride = containerBO as ForwardingContainer;

			var consolBO = sourceBO as ForwardingConsol;
			var shipmentData = dataObject as UniversalShipment;
			var listCache = BindToLists.GetCachedLists(consolBO.Factory);

			EnsureLinkManagerIsInitialized(consolBO);

			if (consolBO != null && shipmentData != null)
			{
				PopulateContainerData(shipmentData, consolBO, listCache, CollectionContent.Partial);
			}
		}

		ITopLevelDataObject IContainerParentDataObjectWriter.GetContainerParentDataObject(BusinessObject sourceBO)
		{
			bool includeContainers = dataWriterOptions.IncludeContainers;
			bool includeSubShipments = dataWriterOptions.IncludeSubShipments;

			ITopLevelDataObject dataObject = null;
			try
			{
				dataWriterOptions.IncludeContainers = false;
				dataWriterOptions.IncludeSubShipments = false;
				suppressCarrierDocumentsOverride = true;

				dataObject = GetDataObject(sourceBO as ForwardingConsol);
			}
			finally
			{
				dataWriterOptions.IncludeContainers = includeContainers;
				dataWriterOptions.IncludeSubShipments = includeSubShipments;
				suppressCarrierDocumentsOverride = false;
			}

			return dataObject;
		}

		bool suppressCarrierDocumentsOverride;

		#endregion
	}
}

