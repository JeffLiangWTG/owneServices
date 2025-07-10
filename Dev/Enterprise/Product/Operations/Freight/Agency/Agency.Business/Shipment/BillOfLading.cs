using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business
{
	[UniversalDataContext(DataContextType.BillOfLading)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.BillOfLading)]
	[VisualizableDocumentsSupportable("BillOfLadingVisualizableDocumentSupporter")]
	public class BillOfLading : AgencyShipment,
		Integration.Agency.IBillOfLading,
		ITransportParent,
		IModuleToModule,
		ICustomFieldProvider,
		IComplianceCommodityRiskStatusProvider
	{
		public BillOfLading(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			AgencyBooking.CheckBillOfLadingIsNotUsedForCurrentBooking(
				factory,
				new ZGuid(row[JobShipmentSchema.PK.Name]),
				new ZString(row[JobShipmentSchema.JS_UniqueConsignRef.Name]),
				new ZString(row[JobShipmentSchema.JS_ShipmentStatus.Name]));
		}

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			return new BillOfLadingValidation(this);
		}

		public new BillOfLadingValidation Validation
		{
			get { return (BillOfLadingValidation)base.Validation; }
		}

		#endregion

		#region Properties

		#region CustomsStatus

		public ZString CustomsStatus
		{
			get { return CustomsMessageStatusProvider.GetCustomsStatus(this); }
		}

		public ZPropertyInfo CustomsStatusInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsStatus)); }
		}

		ICustomsMessageStatusProvider CustomsMessageStatusProvider
		{
			get { return customsMessageStatusProvider ?? (customsMessageStatusProvider = CustomsMessageStatusProviderFactory.New()); }
		}
		ICustomsMessageStatusProvider customsMessageStatusProvider;

		#endregion

		#region MessageStatus

		public ZString MessageStatus
		{
			get { return CustomsMessageStatusProvider.GetMessageStatus(this); }
		}

		public ZPropertyInfo MessageStatusInfo
		{
			get { return GetZPropertyInfo(nameof(MessageStatus)); }
		}

		#endregion

		#region UserFriendlyStatusMessage

		public ZString UserFriendlyStatusMessage
		{
			get { return CustomsMessageStatusProvider.GetUserFriendlyStatusMessage(this); }
		}

		public ZPropertyInfo UserFriendlyStatusMessageInfo
		{
			get { return GetZPropertyInfo(nameof(UserFriendlyStatusMessage)); }
		}

		#endregion

		#region ShouldShowCustomsDetail

		public bool ShouldShowCustomsDetail
		{
			get { return CustomsMessageStatusProvider.ShouldShow(this); }
		}

		#endregion

		public ZString OBLType
		{
			get
			{
				var principalBranding = DocumentsDataRegistry.Instance.PrincipalDocumentBrand.FindBrandingForPrincipal(JS_OH_DeliveryAgent);
				return principalBranding != null ? principalBranding.Code : ZString.Empty;
			}
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
		}

		public override void OnJobCreated(JobHeader job)
		{
			base.OnJobCreated(job);
			_ = ShipmentJobHeader;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (JS_IsCancelled)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (JS_IsCancelled)
				{
					SetReadOnlyIncludingChildren(true);
				}

				if (enforcePropagationOfRealContainersEventsOnSaving)
				{
					enforcePropagationOfRealContainersEventsOnSaving = false;
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (JS_JXInfo.HasChanges)
			{
				UpdateAllRealContainerMovementsEvents();
			}

			if (enforcePropagationOfRealContainersEventsOnSaving)
			{
				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					PropagateRealContainersEvents();
				}
			}

			CascadeFreshFreightLoadedUnloadedEvents();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (HasTransportsIncludingRelatedCountChange || TransportSailingHasChanges)
			{
				UpdateAllRealContainerMovementsEvents();
				TransportSailingHasChanges = false;
				HasTransportsIncludingRelatedCountChange = false;
			}
		}

		void UpdateAllRealContainerMovementsEvents()
		{
			foreach (AgencyShipmentContainer container in RealContainers)
			{
				container.UpdateContainerMovementEvents();
			}
		}

		void CascadeFreshFreightLoadedUnloadedEvents()
		{
			if (!IsFCL)
			{
				FreightEventsHelper.CascadeIfApplicable(this,
					Events.FreightLoadedCode,
					RealContainers.Cast<CommonContainer>().Where(container => container.JC_FCLOnBoardVessel.IsEmpty));

				FreightEventsHelper.CascadeIfApplicable(this,
					Events.FreightUnloadedCode,
					RealContainers);
			}
		}

		#endregion

		public override DocumentSupporter DocumentSupporter
		{
			get { return new AgencyShipmentDocumentSupporter(this); }
		}

		public ZBool UseNewFormBuilderBillOfLading => AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.Value;

		#region Related Business Objects

		#region PackLines

		public new BillOfLadingPackLineCollection OuterPackLines
		{
			get { return (BillOfLadingPackLineCollection)base.OuterPackLines; }
		}

		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new BillOfLadingPackLineCollection(this);
		}

		#endregion

		#region Containers

		public new BillOfLadingContainerDependentCollection BookedContainers
		{
			get { return (BillOfLadingContainerDependentCollection)base.BookedContainers; }
		}

		public new BillOfLadingContainerDependentCollection RealContainers
		{
			get { return (BillOfLadingContainerDependentCollection)base.RealContainers; }
		}

		protected override AgencyShipmentContainerDependentCollection NewContainersCollection(ZString purpose)
		{
			BillOfLadingContainerDependentCollection result = new BillOfLadingContainerDependentCollection(this, purpose);

			if (!IsDeleted && purpose == ContainerBookedStatus.Codes.Booked)
			{
				result.SetReadOnlyIncludingChildren(true);
			}

			return result;
		}

		#endregion

		#endregion

		#region Confirm

		protected override void ConfirmCore()
		{
			base.ConfirmCore();

			WorkflowItems.RemoveAndDeleteAll();
			enforcePropagationOfRealContainersEventsOnSaving = true;
		}

		bool enforcePropagationOfRealContainersEventsOnSaving;

		void PropagateRealContainersEvents()
		{
			var mostRecentContainerEvents = new Dictionary<ZString, StmALog>();
			foreach (AgencyShipmentContainer realContainer in RealContainers)
			{
				var nonCancelledEventLogs = realContainer.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_IsCancelled, false));
				foreach (StmALog log in nonCancelledEventLogs)
				{
					string key = log.SL_SE_NKEvent + log.SL_IsEstimate;
					if (!mostRecentContainerEvents.ContainsKey(key))
					{
						mostRecentContainerEvents.Add(key, log);
					}
					else if (mostRecentContainerEvents[key].SL_EventTime < log.SL_EventTime)
					{
						mostRecentContainerEvents[key] = log;
					}
				}
			}

			var propagationHandler = new PropagationHandler(Factory);
			foreach (var eventLog in mostRecentContainerEvents.Values)
			{
				var handlerProvider = eventLog.Master as IProcessHandlingInfoProvider;
				if (handlerProvider != null)
				{
					propagationHandler.Propagate(handlerProvider.ProcessHandlingInfo, eventLog);
				}
			}
		}

		#endregion

		#region Implementation

		public bool HasTransportsIncludingRelatedCountChange { get; set; }

		public bool TransportSailingHasChanges { get; internal set; }

		protected override bool IsConfirmed
		{
			get { return true; }
			set { }
		}

		protected override ShipmentDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new BillOfLadingDocAddressValidation(addressToValidate);
		}

		protected override void OnTransportsIncludingRelatedCreated()
		{
			base.OnTransportsIncludingRelatedCreated();
			TransportsIncludingRelated.CountChanged += delegate
			{
				HasTransportsIncludingRelatedCountChange = true;
			};
		}

		protected override ControllerID GetDtbBookingParentControllerID()
		{
			return ControllerIDs.AgencyBillOfLading;
		}

		#region IComplianceRiskStatusProvider

		protected override List<ScreeningParty> GetParties()
		{
			var parties = base.GetParties();

			if (Sailing?.Vessel != null)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("4c12458c-8bb4-47af-a186-8b89f89e1feb", "Vessel"), Sailing.Vessel));
			}

			foreach (AgencyShipmentContainer container in RealContainers)
			{
				var arrivalYard = container.ArrivalContainerYardAddress?.Header;
				var departureYard = container.DepartureContainerYardAddress?.Header;

				parties.Add(new ScreeningParty(this, Res.GetString("ac1c2a75-d830-4296-afcd-d58188c9d2c8", "Empty Pickup From"), departureYard));
				parties.Add(new ScreeningParty(this, Res.GetString("6a0f7c49-c8ac-449f-8152-c15ff6806fe2", "Empty Return To"), arrivalYard));
			}

			return parties;
		}

		protected override ZString GetCommoditySource()
		{
			switch (JS_PackingMode)
			{
				case Constants.ContainerModes.RollOnRollOff:
					return Res.GetString("E41BDC8D-DE30-405C-AEE5-6ED222F4995F", "Vehicles");
				case Constants.ContainerModes.BreakBulk:
				case Constants.ContainerModes.Bulk:
				case Constants.ContainerModes.Liquid:
					return Res.GetString("6eb18e78-e78c-40a4-a9fd-68bc49937a8b", "Packs");
				default:
					return Res.GetString("61f7ebdc-2df3-48e6-a3b8-b40d8d89b317", "Containers > Pack Lines");
			}
		}

		#endregion

		public override ZString JS_ShipmentStatus
		{
			get => base.JS_ShipmentStatus;
			set
			{
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && IsInDatabase)
				{
					if (JS_ShipmentStatus != value)
					{
						switch (value)
						{
							case ShipmentStatusList.Codes.Confirmed:
								LogStatusChangedEvent(value, (NoResString)"Shipping Instruction Confirmed");
								break;
							case ShipmentStatusList.Codes.ElectronicShippingInstruction:
								LogStatusChangedEvent(value, Constants.EventReferenceParameterReasons.ElectronicShippingInstructionReceived);
								break;
							case ShipmentStatusList.Codes.SIRejected:
								{
									if (!isSuspendAutomaticCreationOfStatusChangedLog)
									{
										var reason = ZString.Empty;
										if (IsOnShipmentStatusUpdateRunnable())
										{
											reason = GetShipmentStatusUpdateReason(value);

											if (reason.IsEmpty)
											{
												return;
											}
										}

										LogStatusChangedEvent(value, (NoResString)"Shipping Instruction Rejected" + (reason.IsEmpty ? string.Empty : ", " + reason));
									}

									break;
								}
						}

						base.JS_ShipmentStatus = value;
					}
				}
				else
				{
					base.JS_ShipmentStatus = value;
				}
			}
		}

		public bool JS_ShipmentStatus_ReadOnly
		{
			get
			{
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
				{
					return JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed || IsReceivedElectronicShippingInstruction() && JS_ShipmentStatus == ShipmentStatusList.Codes.SIRejected;
				}

				return false;
			}
		}

		public void LogStatusChangedEvent(ZString newValue, ZString? reason = null)
		{
			if (!isSuspendAutomaticCreationOfStatusChangedLog)
			{
				var parameters = new KeyValuePair<string, string>[]
				{
						new KeyValuePair<string, string>(Params.New, newValue),
						new KeyValuePair<string, string>(Params.Old, IsInDatabase ? JS_ShipmentStatusInfo.OriginalValue.ToString() : null),
						new KeyValuePair<string, string>(Params.Reason, reason),
						new KeyValuePair<string, string>(Params.Type, Constants.EventReferenceMessageTypes.ShipmentStatus)
				};

				LogStatusChangedEvent(parameters);
			}
		}

		public IDisposable SuspendAutomaticCreationOfStatusChangedLog()
		{
			isSuspendAutomaticCreationOfStatusChangedLog = true;
			return new DisposableAction(() => isSuspendAutomaticCreationOfStatusChangedLog = false);
		}

		bool isSuspendAutomaticCreationOfStatusChangedLog;

		#endregion

		#region Send Email

		protected override AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = base.GetAddressBookSelection();
			result.AddRecipient(NotifyParty);
			result.AddRecipient(NotifyParty2DocumentaryAddress.Organisation);
			result.AddRecipient(NotifyParty3DocumentaryAddress.Organisation);

			return result;
		}

		protected override string GetEmailSubject()
		{
			return Res.GetString("1d3f588c-dd2c-4eb6-8553-20b180237cc4", "Bill Of Lading - {0}", JS_UniqueConsignRef);
		}

		protected override Type GetDocWrapperType()
		{
			return ObjectFactory.GetType<DocumentWrappers.IDocAgencyShipment>();
		}

		protected override string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.AgencyBillOfLading;
		}

		#endregion

		#region IJobInvoicingPlugin

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new BillOfLadingInvoicingSupporter(this);
		}

		#endregion

		#region Invoicing/Rating

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new BillOfLadingRatingAdapter(this);
		}

		#endregion

		#region Workflow

		protected override ZString GetWorkflowType()
		{
			return WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;
		}

		protected override AgencyShipmentProcessTaskCollection NewWorkflowItemsCollection()
		{
			return new BillOfLadingProcessTaskCollection(this);
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new BillOfLadingTransportSupporter(this); }
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.AgencyShipment; }
		}

		#endregion

		#region IModuleToModule

		IOrgHeader IModuleToModule.RecipientOrganisation
		{
			get { return GlbBranch.CurrentBranch.OrgProxy; }
		}

		bool IModuleToModule.CanExportData(out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			return true;
		}

		BusinessObject IModuleToModule.GetRelatedObject()
		{
			var matchingCriteria = new Dictionary<RelatedJobMatcherKeys, ZString>();
			var consignor = Consignor;
			if (consignor != null)
			{
				matchingCriteria.Add(RelatedJobMatcherKeys.Consignor, consignor.PK.ToString());
			}
			var consignee = Consignee;
			if (consignee != null)
			{
				matchingCriteria.Add(RelatedJobMatcherKeys.Consignee, consignee.PK.ToString());
			}
			if (!JS_HouseBill.IsEmpty)
			{
				matchingCriteria.Add(RelatedJobMatcherKeys.BillOfLading, JS_HouseBill);
			}
			if (!JS_CFSReference.IsEmpty)
			{
				matchingCriteria.Add(RelatedJobMatcherKeys.BookingRefNumber, JS_CFSReference);
			}
			if (CustomsEntryNumberType == Enterprise.Customs.Common.US.CusEntryNumberTypeList.Codes.ITN &&
				!CustomsEntryNumber.IsEmpty)
			{
				matchingCriteria.Add(RelatedJobMatcherKeys.ITN, CustomsEntryNumber);
			}

			var matcher = RelatedBrokerageJobMatcher.New();
			return matcher.GetMatchingJob(matchingCriteria);
		}

		void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
		{
		}

		#endregion

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.BillsOfLadingComplianceAllowOverrideOverallRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.BillsOfLadingComplianceAllowResynchronizeRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => new DeniedSecurityCheckpoint();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.BillsOfLadingComplianceEditHarmonizedCode;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.BillsOfLadingComplianceEditComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.BillsOfLadingComplianceAllowComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.BillsOfLadingComplianceDeclineComplianceAssessment;
	}
}
