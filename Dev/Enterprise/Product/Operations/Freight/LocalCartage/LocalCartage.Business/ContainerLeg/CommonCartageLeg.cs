using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.LocalCartage.Business
{
	[UniversalDataContext(DataContextType.LocalTransportLeg)]
	[UserDefinedValues]
	[CodeProperty("UniqueIDWithJobNumber"), DescriptionProperty("UniqueIDWithJobNumber")]
	[DebuggerDisplay("Seq.:{JU_RunSheetSequence} PU:{PickupFromDocAddress.E2_CompanyName} DL:{DeliverToDocAddress.E2_CompanyName} PI:{JU_PickupTimeIn} PO:{JU_PickupTimeOut} DI:{JU_DeliverTimeIn} DO:{JU_DeliverTimeOut}")]
	[DeferTriggerAndRunBeforeCommit(
		"TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob",
		"JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob",
		JobContainerLegsSchema.Constants.PK,
		typeof(IJobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobStrategy_JobContainerLegsInsert))]
	public class CommonCartageLeg :
		AutoJobContainerLegs,
		IDocumentSupportable,
		Integration.ICommonCartageLeg,
		IDocManagerSupport,
		IWorkflowProvider,
		IWorkflowProviderEvent,
		IDistanceCalculationConsumer,
		ICustomFieldProvider,
		IJobNumber,
		ICreditControlledDocumentDelivery
	{
		public CommonCartageLeg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZBool IsRoot
		{
			get;
			set;
		} = true;

		public void UpdateReadOnlyForWhenCancelled()
		{
			if (Cartage != null && !IsDeleted)
			{
				SetReadOnlyIncludingChildren(Cartage.JJ_IsCancelled);
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return (NoResString)"Leg for " + WhatSummary; }
		}

		public new class Schema : AutoJobContainerLegs.Schema
		{
			public const string IsLoose = "IsLoose";
			public const string IsContainerised = "IsContainerised";
			public const string HasWaitPoint = "HasWaitPoint";
			public const string PickupWaitTime = "PickupWaitTime";
			public const string DeliveryWaitTime = "DeliveryWaitTime";
			public const string PostcodeDistance = "PostcodeDistance";
			public const string LegOfBookedMove = "LegOfBookedMove";
			public const string FullGatePass = "FullGatePass";
			public const string ServiceLevel = "ServiceLevel";
			public const string TotalWeight = "TotalWeight";
			public const string TotalWeightUnit = "TotalWeightUnit";
			public const string PickupFromAddress = "PickupFromAddress";
			public const string PickupFromCity = "PickupFromCity";
			public const string WaitPointAddress = "WaitPointAddress";
			public const string WaitPointCity = "WaitPointCity";
			public const string DeliverToAddress = "DeliverToAddress";
			public const string DeliverToCity = "DeliverToCity";
			public const string TotalPackages = "TotalPackages";
			public const string TotalPackagesUnit = "TotalPackagesUnit";
			public const string TotalVolume = "TotalVolume";
			public const string TotalVolumeUnit = "TotalVolumeUnit";
			public const string GPSMessageText = "GPSMessageText";
			public const string VesselVoyage = "VesselVoyage";
			public const string TruckDescription = "TruckDescription";
			public const string QuickGSDriver = "QuickGSDriver";
			public const string QuickRQTruck = "QuickRQTruck";
			public const string QuickOHTransportCompany = "QuickOHTransportCompany";
			public const string QuickPlannedPickupTime = "QuickPlannedPickupTime";
			public const string QuickEstimatedDeliveryTime = "QuickEstimatedDeliveryTime";
			public const string PickupAddressCode = "PickupAddressCode";
			public const string WaitPointAddressCode = "WaitPointAddressCode";
			public const string DeliveryAddressCode = "DeliveryAddressCode";
			public const string PickupTimeSummary = "PickupTimeSummary";
			public const string WaitPointTimeSummary = "WaitPointTimeSummary";
			public const string DeliveryTimeSummary = "DeliveryTimeSummary";
			public const string WhatSummary = "WhatSummary";
			public const string PickupStatus = "PickupStatus";
			public const string DeliveryStatus = "DeliveryStatus";
			public const string WaitPointStatus = "WaitPointStatus";
			public const string SequenceAndActive = "SequenceAndActive";
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			// HasChanges is true even after refreshing each field from the DB. Need to reload for DragDrop from Module
			if (HasChanges)
			{
				Reload();
			}

			// Run Sheet Dashboard
			SequenceAndActiveInfo.RefreshBinding();
			PickupAddressCodeInfo.RefreshBinding();
			DeliveryAddressCodeInfo.RefreshBinding();
			PickupTimeSummaryInfo.RefreshBinding();
			DeliveryTimeSummaryInfo.RefreshBinding();
			WhatSummaryInfo.RefreshBinding();
			JU_E2WaitPointAddressIDInfo.RefreshBinding();

			if (WorkSheet != null)
			{
				WorkSheet.LegUpdated();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges || !IsInDatabase)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges(!IsInDatabase));
				if (WorkSheet == null)
				{
					TryAllocateToExistingRunSheet(true);
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (JU_SplitDeliverySuffix.IsEmpty)
			{
				SetUniqueID();
			}
		}

		public void SetUniqueID()
		{
			JU_SplitDeliverySuffix = GetNextUniqueID();
		}

		ZString GetNextUniqueID()
		{
			int numRep = 0;

			if (Cartage != null)
			{
				foreach (CommonCartageLeg leg in Cartage.CartageLegs)
				{
					if (PK != leg.PK)
					{
						int legNumRep = CommonUtils.GetNumberRepresentation(leg.JU_SplitDeliverySuffix.ToUpper());
						if (legNumRep > numRep)
						{
							numRep = legNumRep;
						}
					}
				}
			}

			numRep++;
			if (numRep > CommonUtils.MaxNumberRepresentation - 1)
			{
				throw new ZCannotSaveException(
					ResString.GetMultilingualString(
						"EC94152C-1BD3-45F9-AA1C-351C13F018A4",
						"Cannot add any more booking legs to this transport job. The maximum number of legs allowed in a transport job is {0}. Cannot assign a booking leg a Unique ID greater than '{1}'.",
						CommonUtils.MaxNumberRepresentation - 1,
						CommonUtils.GetLetterRepresentation(CommonUtils.MaxNumberRepresentation - 1)),
					"Cannot add any more booking legs to this transport job",
					false,
					null);
			}
			return CommonUtils.GetLetterRepresentation(numRep);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JU_DistanceUnit = DistanceCalculationRegistry.Instance.DefaultDistanceUnit.Value;
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			// Tested in Transport booking confirmation
			base.OnFactorySaved(saveSucceeded);

			//Send events if successful
			if (saveSucceeded && Cartage != null && Cartage.ParentBooking != null && PublishEventsOnSaved.Any(l => l != null))
			{
				var parentBookingAsBizObj = Cartage.ParentBooking as BusinessObject;
				if (parentBookingAsBizObj != null)
				{
					var factory = new BusinessObjectFactory();
					using (factory.AddDisposableService())
					{
						foreach (var log in PublishEventsOnSaved.Where(l => l != null))
						{
							try
							{
								Cartage.CurrentlyPublishingAnEventToTheOrgProxy = true;
								UniversalXmlWorkflowProcessor.PublishUniversalEvent(factory, new[] { RecipientRoleType.ORP }, this, log);
							}
							finally
							{
								Cartage.CurrentlyPublishingAnEventToTheOrgProxy = false;
							}
						}
						factory.Save();
					}
				}

				PublishEventsOnSaved.Clear();
			}
		}

		List<StmALog> PublishEventsOnSaved
		{
			get { return publishEventsOnSaved ?? (publishEventsOnSaved = new List<StmALog>()); }
		}
		List<StmALog> publishEventsOnSaved;

		[BusinessObjectTestExclude()]
		[RelatedBusinessObject("BookedCtgMove")]
		public override ZGuid JU_EW
		{
			get { return base.JU_EW; }
			set
			{
				if (base.JU_EW == value)
				{
					base.JU_EW = value;
				}
				else
				{
					if (!base.JU_EW.IsEmpty)
					{
						BehaviorStrategy.BookedMoveCartageLegLinkBroken(this);
					}

					using (SuspendSettingHasChanges())
					{
						base.JU_EW = value;
					}

					if (!JU_EW.IsEmpty)
					{
						BehaviorStrategy.BookedMoveCartageLegLinkCreated(this);
					}
				}
			}
		}

		[RelatedBusinessObject("WorkSheet")]
		[List("Lookups.WorkSheets")]
		public override ZGuid JU_EY_RunSheet
		{
			get { return base.JU_EY_RunSheet; }
			set
			{
				if (base.JU_EY_RunSheet == value)
				{
					base.JU_EY_RunSheet = value;
				}
				else
				{
					if (value.IsValid)
					{
						TryAuthorisedIfRequired();
					}

					if (IsRunSheetAuthorised && WorkSheet != null)
					{
						BehaviorStrategy.WorkSheetCartageLegLinkBroken(this);
					}

					base.JU_EY_RunSheet = value;
					ResetQuickWorkSheetAllocationFields();

					if (IsRunSheetAuthorised && WorkSheet != null)
					{
						BehaviorStrategy.WorkSheetCartageLegLinkCreated(this);
					}

					SetIsDispatchedIfUsingWTGTelematics();
					MarkAsNeedingValidation();

					Validation.ValidateJU_EstimatedDeliveryTime();
					Validation.ValidateJU_PlannedPickupTime();
					Validation.ValidateQuickGSDriver();
					Validation.ValidateQuickRQTruck();
					RefreshBindingIncludingChildren();
				}
			}
		}

		void SetIsDispatchedIfUsingWTGTelematics()
		{
			if (JU_EY_RunSheet.IsEmpty && JU_MessageStatus == Constants.CartageLegDispatchStatusList.Codes.NotStarted)
			{
				JU_MessageStatus = "";
				CancelDispatchEvent();
			}
			else if (JU_MessageStatus.IsEmpty && Vehicle != null && Vehicle.HasTelematics)
			{
				JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.NotStarted;

				CancelDispatchEvent();
				Logs.AddNew(Events.ContainerLegDispatched, DispatchTimeReference);
			}
		}

		void CancelDispatchEvent()
		{
			var dispatchEvent = GetDispatchEvent();
			if (dispatchEvent != null)
			{
				dispatchEvent.Cancel();
			}
		}

		StmALog GetDispatchEvent()
		{
			StmALog[] dispatchMessages = Logs.Find(new ZQuery(StmALogSchema.SL_Reference, DispatchTimeReference));
			return dispatchMessages.Length > 0 ? dispatchMessages[0] : null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		const string DispatchTimeReference = "Dispatch Time";

		[BusinessObjectTestExclude()]
		[List("Lookups.CartageAddressList")]
		public override ZGuid JU_E2PickupAddressID
		{
			[DebuggerStepThrough()]
			get { return base.JU_E2PickupAddressID; }
			set
			{
				ZGuid originalValue = base.JU_E2PickupAddressID;

				if (Cartage == null || Cartage.DocAddresses.Contains(value))
				{
					base.JU_E2PickupAddressID = value;
				}
				else
				{
					JobDocAddress docAddress = Cartage.AddJobDocAddress(value);
					base.JU_E2PickupAddressID = docAddress != null ? docAddress.PK : ZGuid.Empty;
				}

				BehaviorStrategy.PickupAddressIDChanged(this, originalValue);
				SetIsEmptyContainer();
				JU_E2PickupAddressIDInfo.RefreshBinding();
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CartageAddressList")]
		public override ZGuid JU_E2WaitPointAddressID
		{
			[DebuggerStepThrough()]
			get { return base.JU_E2WaitPointAddressID; }
			set
			{
				ZGuid originalValue = base.JU_E2WaitPointAddressID;

				if (Cartage == null || Cartage.DocAddresses.Contains(value))
				{
					base.JU_E2WaitPointAddressID = value;
				}
				else
				{
					JobDocAddress docAddress = Cartage.AddJobDocAddress(value);
					base.JU_E2WaitPointAddressID = docAddress != null ? docAddress.PK : ZGuid.Empty;
				}

				BehaviorStrategy.WaitPointAddressIDChanged(this, originalValue);
				SetIsEmptyContainer();
				JU_E2WaitPointAddressIDInfo.RefreshBinding();
				fHasWaitPoint_null = null;  // reset this then HasWaitPoint can get refreshed
			}
		}

		[BusinessObjectTestExclude()]
		[List("Lookups.CartageAddressList")]
		public override ZGuid JU_E2DeliveryAddressID
		{
			[DebuggerStepThrough()]
			get { return base.JU_E2DeliveryAddressID; }
			set
			{
				ZGuid originalValue = base.JU_E2DeliveryAddressID;

				if (Cartage == null || Cartage.DocAddresses.Contains(value))
				{
					base.JU_E2DeliveryAddressID = value;
				}
				else
				{
					JobDocAddress docAddress = Cartage.AddJobDocAddress(value);
					base.JU_E2DeliveryAddressID = docAddress != null ? docAddress.PK : ZGuid.Empty;
				}

				BehaviorStrategy.DeliveryAddressIDChanged(this, originalValue);
				SetIsEmptyContainer();
				JU_E2DeliveryAddressIDInfo.RefreshBinding();
			}
		}

		public void ClearDeletedDocAddress(ZGuid deletedDocAddressPK)
		{
			if (!IsDeleted)
			{
				if (JU_E2PickupAddressID == deletedDocAddressPK)
				{
					JU_E2PickupAddressID = ZGuid.Empty;
				}

				if (JU_E2WaitPointAddressID == deletedDocAddressPK)
				{
					JU_E2WaitPointAddressID = ZGuid.Empty;
				}

				if (JU_E2DeliveryAddressID == deletedDocAddressPK)
				{
					JU_E2DeliveryAddressID = ZGuid.Empty;
				}
			}
		}

		public override ZDateTime JU_PickupTimeIn
		{
			get
			{
				return base.JU_PickupTimeIn;
			}
			set
			{
				base.JU_PickupTimeIn = value;
				SetDemurrage(DemurrageTime.Pickup);
				CheckJobCompletion();
				SetGroupedLegsTime(JobContainerLegsSchema.JU_PickupTimeIn, value);

				AddInOutPicDlvEvents(PickupFromDocAddress, PickupFromCity, Events.Arrival, JU_PickupTimeInInfo, Events.PickedUp, JU_PickupTimeInInfo, JU_PickupTimeOutInfo, Constants.EventReferenceParameterReasons.Pickup);
			}
		}

		public override ZDateTime JU_PickupTimeOut
		{
			get { return base.JU_PickupTimeOut; }
			set
			{
				base.JU_PickupTimeOut = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJU_PickupTimeOut();
				}
				SetMessageStatus();
				SetDemurrage(DemurrageTime.Pickup);
				if (Cartage != null)
				{
					Cartage.PickupCompleted(this, PickupDocAddressType, value);
				}
				CheckJobCompletion();
				SetGroupedLegsTime(JobContainerLegsSchema.JU_PickupTimeOut, value);

				AddInOutPicDlvEvents(PickupFromDocAddress, PickupFromCity, Events.Departure, JU_PickupTimeOutInfo, Events.PickedUp, JU_PickupTimeInInfo, JU_PickupTimeOutInfo, Constants.EventReferenceParameterReasons.Pickup);
				if (PickupDocAddressType == DocAddressType.LocalCartageExporter)
				{
					if (IsContainerised)
					{
						AddPickUpOrDeliveryEvent(PickupFromDocAddress, PickupFromCity, Events.PickupCartageCompleteFinalised, JU_PickupTimeOutInfo, Constants.EventReferenceParameterReasons.Pickup);
					}
					else if (IsLoose && Cartage != null && AllLoosePickedFromConsignor)
					{
						Cartage.Logs.CreateRecreateOrUpdateEventLog(Events.PickupCartageCompleteFinalised, EstimateActual.Actual, value.ToOffset(),
							"",
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Desc.Consignor),
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, Constants.EventReferenceParameterReasons.Pickup));
					}
				}
			}
		}

		bool AllLoosePickedFromConsignor
		{
			get
			{
				return Cartage != null
					&& Cartage.LooseBookedMoves.All(move => move.CartageLegs.Where(l => l.PickupDocAddressType == DocAddressType.LocalCartageExporter).All(l => !l.JU_PickupTimeOut.IsEmpty))
					&& Cartage.LooseBookedMoves.All(move => move.CartageLegs.Where(l => l.WaitPointDocAddressType == DocAddressType.LocalCartageExporter).All(l => !l.JU_WaitPointTimeOut.IsEmpty));
			}
		}

		public override ZDateTime JU_WaitPointTimeIn
		{
			get { return base.JU_WaitPointTimeIn; }
			set
			{
				base.JU_WaitPointTimeIn = value;
				SetDemurrage(DemurrageTime.WaitPoint);
				SetGroupedLegsTime(JobContainerLegsSchema.JU_WaitPointTimeIn, value);

				AddInOutPicDlvEventsForWaitPoint(WaitPointDocAddress, WaitPointCity, Events.Arrival, JU_WaitPointTimeInInfo);
			}
		}

		public override ZDateTime JU_WaitPointTimeOut
		{
			get { return base.JU_WaitPointTimeOut; }
			set
			{
				base.JU_WaitPointTimeOut = value;
				SetDemurrage(DemurrageTime.WaitPoint);
				if (Cartage != null)
				{
					Cartage.DeliveryCompleted(this, WaitPointDocAddressType, value);
					Cartage.PickupCompleted(this, WaitPointDocAddressType, value);
				}
				SetGroupedLegsTime(JobContainerLegsSchema.JU_WaitPointTimeOut, value);
				CheckIsDelivered();

				AddInOutPicDlvEventsForWaitPoint(WaitPointDocAddress, WaitPointCity, Events.Departure, JU_WaitPointTimeOutInfo);
				if (IsContainerised)
				{
					if (WaitPointDocAddressType == DocAddressType.LocalCartageExporter)
					{
						AddPickUpOrDeliveryEvent(WaitPointDocAddress, WaitPointCity, Events.PickupCartageCompleteFinalised, JU_WaitPointTimeOutInfo, Constants.EventReferenceParameterReasons.Pickup);
					}
					else if (WaitPointDocAddressType == DocAddressType.LocalCartageImporter)
					{
						AddPickUpOrDeliveryEvent(WaitPointDocAddress, WaitPointCity, Events.DeliveryCartageCompleteFinalised, JU_WaitPointTimeOutInfo, Constants.EventReferenceParameterReasons.Delivery);
					}
				}
				else if (IsLoose && Cartage != null)
				{
					if (WaitPointDocAddressType == DocAddressType.LocalCartageExporter && AllLoosePickedFromConsignor)
					{
						Cartage.Logs.CreateRecreateOrUpdateEventLog(Events.PickupCartageCompleteFinalised, EstimateActual.Actual, value.ToOffset(), "",
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Desc.Consignor),
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, Constants.EventReferenceParameterReasons.Pickup));
					}
					else if (WaitPointDocAddressType == DocAddressType.LocalCartageImporter && AllLooseDeliveredToConsignee)
					{
						Cartage.Logs.CreateRecreateOrUpdateEventLog(Events.DeliveryCartageCompleteFinalised, EstimateActual.Actual, value.ToOffset(),
							"",
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Desc.Consignee),
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, Constants.EventReferenceParameterReasons.Delivery));
					}
				}
			}
		}

		public override ZDateTime JU_DeliverTimeIn
		{
			get { return base.JU_DeliverTimeIn; }
			set
			{
				var original = base.JU_DeliverTimeIn;
				base.JU_DeliverTimeIn = value;
				SetDemurrage(DemurrageTime.Delivery);
				CheckJobCompletion();
				UpdateContainerEmptyReturned(original, JU_DeliverTimeOut);
				SetGroupedLegsTime(JobContainerLegsSchema.JU_DeliverTimeIn, value);

				AddInOutPicDlvEvents(DeliverToDocAddress, DeliverToCity, Events.Arrival, JU_DeliverTimeInInfo, Events.Delivered, JU_DeliverTimeInInfo, JU_DeliverTimeOutInfo, Constants.EventReferenceParameterReasons.Delivery);
			}
		}

		public override ZString JU_DeliverySignedFor
		{
			get
			{
				return base.JU_DeliverySignedFor;
			}
			set
			{
				base.JU_DeliverySignedFor = value;
				CheckIsDelivered();
				AddSignatureCapturedEvent();
			}
		}

		public override ZDateTime JU_DeliverTimeOut
		{
			get { return base.JU_DeliverTimeOut; }
			set
			{
				var original = base.JU_DeliverTimeOut;
				var cartage = Cartage;
				var lastLegTimeOutBeforeChange = cartage != null ? cartage.LastLegDeliveryTimeOut : ZDateTime.Empty;
				base.JU_DeliverTimeOut = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJU_DeliverTimeOut();
				}

				SetMessageStatus();
				SetDemurrage(DemurrageTime.Delivery);
				UpdateContainerEmptyReturned(JU_DeliverTimeIn, original);
				if (cartage != null)
				{
					cartage.DeliveryCompleted(this, DeliverToDocAddressType, value);
				}
				CheckJobCompletion(lastLegTimeOutBeforeChange);
				SetGroupedLegsTime(JobContainerLegsSchema.JU_DeliverTimeOut, value);
				CheckIsDelivered();

				AddInOutPicDlvEvents(DeliverToDocAddress, DeliverToCity, Events.Departure, JU_DeliverTimeOutInfo, Events.Delivered, JU_DeliverTimeInInfo, JU_DeliverTimeOutInfo, Constants.EventReferenceParameterReasons.Delivery);

				if (IsContainerised)
				{
					if (DeliverToDocAddressType == DocAddressType.LocalCartageImporter)
					{
						AddPickUpOrDeliveryEvent(DeliverToDocAddress, DeliverToCity, Events.DeliveryCartageCompleteFinalised, JU_DeliverTimeOutInfo, Constants.EventReferenceParameterReasons.Delivery);
					}
					else if (DeliverToDocAddressType == DocAddressType.LocalCartageYard)
					{
						AddPickUpOrDeliveryEvent(DeliverToDocAddress, DeliverToCity, Events.GateIn, JU_DeliverTimeOutInfo, Constants.EventReferenceParameterReasons.Delivery);
					}
				}
				else if (IsLoose && DeliverToDocAddressType == DocAddressType.LocalCartageImporter && AllLooseDeliveredToConsignee)
				{
					Cartage.Logs.CreateRecreateOrUpdateEventLog(Events.DeliveryCartageCompleteFinalised, EstimateActual.Actual, value.ToOffset(),
							"",
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, EventConstants.Facilities.Desc.Consignee),
							new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, Constants.EventReferenceParameterReasons.Delivery));
				}

				if (AllLegsComplete)
				{
					Cartage.Logs.CreateRecreateOrUpdateEventLog(Events.CartageCompleteFinalised, EstimateActual.Actual, value.ToOffset());
				}
			}
		}

		bool AllLooseDeliveredToConsignee
		{
			get
			{
				return Cartage != null
					&& Cartage.LooseBookedMoves.All(move => move.CartageLegs.Where(l => l.DeliverToDocAddressType == DocAddressType.LocalCartageImporter).All(l => !l.JU_DeliverTimeOut.IsEmpty))
					&& Cartage.LooseBookedMoves.All(move => move.CartageLegs.Where(l => l.WaitPointDocAddressType == DocAddressType.LocalCartageImporter).All(l => !l.JU_WaitPointTimeOut.IsEmpty));
			}
		}

		bool AllLegsComplete
		{
			get
			{
				return Cartage != null
					&& Cartage.BookedMovesCollection.All(move => move.CartageLegs.All(l => !l.JU_DeliverTimeOut.IsEmpty));
			}
		}

		void UpdateContainerEmptyReturned(ZDateTime originalValueIN, ZDateTime originalValueOUT)
		{
			var container = Container;
			if (container != null && DeliverToDocAddressType == DocAddressType.LocalCartageYard)
			{
				if (container.JC_ContainerYardEmptyReturnGateIn.IsEmpty ||
					container.JC_ContainerYardEmptyReturnGateIn == originalValueIN ||
					container.JC_ContainerYardEmptyReturnGateIn == originalValueOUT)
				{
					container.JC_ContainerYardEmptyReturnGateIn = JU_DeliverTimeIn.IsValid ? JU_DeliverTimeIn : JU_DeliverTimeOut;
				}
			}
		}

		public override ZDateTime JU_EstimatedDeliveryTime
		{
			get { return base.JU_EstimatedDeliveryTime; }
			set
			{
				base.JU_EstimatedDeliveryTime = value;
				CheckIsDelivered();
			}
		}

		[List("BindToLists.AdditionalServices")]
		public override ZString JU_AdditionalService
		{
			get { return base.JU_AdditionalService; }
			set
			{
				var originalValue = base.JU_AdditionalService;
				var lastLegTimeOutBeforeChange = Cartage != null ? Cartage.LastLegDeliveryTimeOut : ZDateTime.Empty;
				base.JU_AdditionalService = value;

				if (JU_AdditionalService == Constants.CartageAdditional.Futile || originalValue == Constants.CartageAdditional.Futile)
				{
					SetMessageStatus();
				}
				CheckJobCompletion(lastLegTimeOutBeforeChange);
			}
		}

		[List("BindToLists.MessageStatuses")]
		public override ZString JU_MessageStatus
		{
			get
			{
				return base.JU_MessageStatus;
			}
			set
			{
				var lastLegTimeOutBeforeChange = Cartage != null ? Cartage.LastLegDeliveryTimeOut : ZDateTime.Empty;
				var originalValue = base.JU_MessageStatus;
				base.JU_MessageStatus = value;
				if (originalValue != value)
				{
					var eventToUse = Events.WorkflowTriggerEvent; //None

					if (value == Constants.CartageLegDispatchStatusList.Codes.Runsheet)
					{
						eventToUse = Events.ContainerLegRunSheet;
					}

					if (eventToUse != Events.WorkflowTriggerEvent)
					{
						var vehicleCode = Vehicle != null ? Vehicle.RQ_ShortCode : ZString.Empty;
						Logs.AddNew(eventToUse, "dispatched to vehicle: " + vehicleCode);
					}

					UpdateLegAcceptanceStatus(value);
				}

				CheckJobCompletion(lastLegTimeOutBeforeChange);
			}
		}

		void UpdateLegAcceptanceStatus(string messageStatus)
		{
			if (messageStatus == Constants.CartageLegDispatchStatusList.Codes.WIP)
			{
				AddNewLegAcceptanceStatus(Events.MessageAccepted, Constants.EventReferenceParameterReasons.Accepted);
			}
			else if (messageStatus == Constants.CartageLegDispatchStatusList.Codes.Rejected)
			{
				AddNewLegAcceptanceStatus(Events.MessageRejected, Constants.EventReferenceParameterReasons.Rejected);
			}
			else if (messageStatus == Constants.CartageLegDispatchStatusList.Codes.Futile)
			{
				AddNewLegAcceptanceStatus(Events.MessageWithdrawCancelAccepted, Constants.EventReferenceParameterReasons.Futile);
			}
		}

		void AddNewLegAcceptanceStatus(Event eventCode, string reason)
		{
			if (IsMessageStatusUpdatedViaWebservice)
			{
				Logs.AddNew(eventCode,
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, reason),
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Department, (NoResString)"Driver"));
			}
			else
			{
				Logs.AddNew(eventCode,
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, reason));
			}
		}

		public bool IsMessageStatusUpdatedViaWebservice { private get; set; }

		[MeasureUnit(AutoJobContainerLegs.Schema.JU_DistanceUnit, MeasureUnitType.Length)]
		public override ZDecimal JU_Distance
		{
			get { return base.JU_Distance; }
			set { base.JU_Distance = value; }
		}

		[List("BindToLists.DistanceUnits")]
		public override ZString JU_DistanceUnit
		{
			get { return base.JU_DistanceUnit; }
			set
			{
				base.JU_DistanceUnit = value;
				PostcodeDistanceInfo.RefreshBinding();
			}
		}

		[ZDateTimeDurationValue]
		public override ZDateTime JU_CartagePickupDemurrage
		{
			get { return base.JU_CartagePickupDemurrage; }
			set
			{
				base.JU_CartagePickupDemurrage = value.ConvertToDurationBasedDate(JU_CartagePickupDemurrageInfo);

				var cartage = Cartage;
				if (cartage != null)
				{
					cartage.MarkAsNeedingTotalDemurrageCheck();
				}
			}
		}

		[ZDateTimeDurationValue]
		public override ZDateTime JU_CartageWaitPointDemurrage
		{
			get { return base.JU_CartageWaitPointDemurrage; }
			set
			{
				base.JU_CartageWaitPointDemurrage = value.ConvertToDurationBasedDate(JU_CartageWaitPointDemurrageInfo);

				var cartage = Cartage;
				if (cartage != null)
				{
					cartage.MarkAsNeedingTotalDemurrageCheck();
				}
			}
		}

		[ZDateTimeDurationValue]
		public override ZDateTime JU_CartageDeliveryDemurrage
		{
			get { return base.JU_CartageDeliveryDemurrage; }
			set
			{
				base.JU_CartageDeliveryDemurrage = value.ConvertToDurationBasedDate(JU_CartageDeliveryDemurrageInfo);

				var cartage = Cartage;
				if (cartage != null)
				{
					cartage.MarkAsNeedingTotalDemurrageCheck();
				}
			}
		}

		public ZDateTime StartTime
		{
			get
			{
				ZDateTime result = JU_PickupTimeOut;

				if (result.IsEmpty)
				{
					result = JU_PickupTimeIn;
				}
				if (result.IsEmpty)
				{
					result = JU_PlannedPickupTime;
				}
				if (result.IsEmpty && WorkSheet != null)
				{
					result = WorkSheet.EY_StartTime;
				}

				return result;
			}
		}

		public ZDateTime EndTime
		{
			get
			{
				ZDateTime result = JU_DeliverTimeOut;

				if (result.IsEmpty)
				{
					result = JU_DeliverTimeIn;
				}
				if (result.IsEmpty)
				{
					result = JU_EstimatedDeliveryTime;
				}
				if (result.IsEmpty && WorkSheet != null)
				{
					result = WorkSheet.EY_EndTime;
				}

				return result;
			}
		}

		public ZString UniqueID
		{
			get { return JU_SplitDeliverySuffix; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "CargoWise.Types.ZString.IndexOf(CargoWise.Types.ZString)")]
		public ZString UniqueIDWithJobNumber
		{
			get
			{
				ZString jobNumber = Cartage != null ? Cartage.JJ_ConsignmentID : ZString.Empty;
				return !jobNumber.IsEmpty || !UniqueID.IsEmpty ? ZString.Format("{0}/{1}", jobNumber, UniqueID) : ZString.Empty;
			}
		}

		bool HasContainerYard
		{
			get { return PickupDocAddressType == DocAddressType.LocalCartageYard || DeliverToDocAddressType == DocAddressType.LocalCartageYard; }
		}

		public ZString VesselVoyage
		{
			get
			{
				var result = ZString.Empty;
				var cartage = Cartage;
				if (cartage != null)
				{
					if (cartage.IsAir)
					{
						result = cartage.VoyageFlight;
					}
					else
					{
						result = new ZString(cartage.Vessel + " / " + cartage.VoyageFlight);
					}
				}
				return result;
			}
		}

		public ZPropertyInfo VesselVoyageInfo
		{
			get { return GetZPropertyInfo(Schema.VesselVoyage); }
		}

		public ZString ServiceLevel
		{
			get
			{
				var cartage = Cartage;
				return cartage != null ? cartage.JJ_RS_NKServiceLevel : ZString.Empty;
			}
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceLevel); }
		}

		[MaxLength(CommonShipment.Schema.JS_UniqueConsignRefMaxLength + 2)]
		public ZString FullGatePass
		{
			get
			{
				//CommonShipment Shipment = ShipmentParent;
				//ZString ShipmentID = (CommonShipment == null) ? ZString.Empty : Shipment.JS_UniqueConsignRef;
				//return ShipmentID + GatePassIDSeparator + JU_GatePassNumber;
				return JU_GatePassNumber;
			}
		}

		public ZPropertyInfo FullGatePassInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.FullGatePass);
			}
		}

		//Non-persistent property to have read-only field on the form.
		//It's used on the form to send a message to the vehicle through GPS.
		[MaxLength(400)]
		public ZString GPSMessageText
		{
			get
			{
				return fGPSMessageText;
			}
			set
			{
				CheckMaximumLength(GPSMessageTextInfo, value);
				SetNonPersistentPropertyValue(GPSMessageTextInfo, ref fGPSMessageText, value);
				fGPSMessageText = value;
			}
		}
		ZString fGPSMessageText;

		public ZPropertyInfo GPSMessageTextInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.GPSMessageText);
			}
		}

		public OrgHeader Client
		{
			get
			{
				var cartage = Cartage;
				return cartage != null ? cartage.LocalClient : null;
			}
		}

		public ZInt TotalPackages
		{
			get
			{
				var move = BookedCtgMove;
				return move != null ? move.EW_BookedPackCount : ZInt.Zero;
			}
		}

		public ZPropertyInfo TotalPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackages); }
		}

		public ZString TotalPackagesUnit
		{
			get
			{
				var move = BookedCtgMove;
				return move != null ? move.EW_F3_NKPackType : ZString.Empty;
			}
		}

		public ZPropertyInfo TotalPackagesUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackagesUnit); }
		}

		public ZDecimal TotalVolume
		{
			get
			{
				var move = BookedCtgMove;
				return move != null ? move.EW_BookedVolume : ZDecimal.Zero;
			}
		}

		public ZPropertyInfo TotalVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalVolume); }
		}

		public ZString TotalVolumeUnit
		{
			get
			{
				var move = BookedCtgMove;
				return move != null ? move.EW_VolumeUQ : ZString.Empty;
			}
		}

		public ZPropertyInfo TotalVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalVolumeUnit); }
		}

		public ZBool HasWaitPoint
		{
			get
			{
				if (fHasWaitPoint_null == null)
				{
					fHasWaitPoint_null = !JU_E2WaitPointAddressID.IsEmpty;
					fHasWaitPoint = (ZBool)fHasWaitPoint_null;
				}
				return fHasWaitPoint;
			}
			set
			{
				SetNonPersistentPropertyValue(HasWaitPointInfo, ref fHasWaitPoint, value);

				if (!value && !JU_E2WaitPointAddressID.IsEmpty)
				{
					JU_E2WaitPointAddressID = ZGuid.Empty;
				}
			}
		}
		ZBool? fHasWaitPoint_null;
		ZBool fHasWaitPoint;

		public ZPropertyInfo HasWaitPointInfo
		{
			get { return GetZPropertyInfo(Schema.HasWaitPoint); }
		}

		public ZBool HasLegAreadyCommenced
		{
			get { return JU_EY_RunSheet.IsValid || !JU_PickupTimeIn.IsEmpty || !JU_PickupTimeOut.IsEmpty || !JU_DeliverTimeIn.IsEmpty || !JU_DeliverTimeOut.IsEmpty; }
		}

		public bool IsTransportingGoods
		{
			get
			{
				return (PickupDocAddressType != DocAddressType.LocalCartageCTO)
					&& (WaitPointDocAddressType != DocAddressType.LocalCartageCTO)
					&& (DeliverToDocAddressType != DocAddressType.LocalCartageCTO);
			}
		}

		public DocAddressType PickupDocAddressType
		{
			get
			{
				var pickupFromAddress = PickupFromDocAddress;
				return pickupFromAddress != null ? pickupFromAddress.DocAddressType : DocAddressType.None;
			}
		}

		public DocAddressType WaitPointDocAddressType
		{
			get
			{
				var waitPointDocAddress = WaitPointDocAddress;
				return waitPointDocAddress != null ? waitPointDocAddress.DocAddressType : DocAddressType.None;
			}
		}

		public DocAddressType DeliverToDocAddressType
		{
			get
			{
				var deliverToDocAddress = DeliverToDocAddress;
				return deliverToDocAddress != null ? deliverToDocAddress.DocAddressType : DocAddressType.None;
			}
		}

		public ZDateTime DispatchTime
		{
			get
			{
				StmALog dispatchEvent = GetDispatchEvent();
				return dispatchEvent == null ? WorkSheet.EY_StartTime : dispatchEvent.SL_EventTime;
			}
		}

		public bool HasDeliveryStarted
		{
			get { return WorkSheet != null && IsPartComplete; }
		}

		public ZBool IsContainerised
		{
			get { return Container != null; }
		}

		public ZPropertyInfo IsContainerisedInfo
		{
			get { return GetZPropertyInfo(Schema.IsContainerised); }
		}

		public ZBool IsLoose
		{
			get { return Container == null; }
		}

		public ZPropertyInfo IsLooseInfo
		{
			get { return GetZPropertyInfo(Schema.IsLoose); }
		}

		public ZBool IsFirstLeg
		{
			get { return IsFirstLegCore(false); }
		}

		/// <summary>
		/// If Cartage is Mixed:
		///   And Export, Loose Legs are first.
		///   Otherwise Container Legs are first.
		/// If Cartage is NOT Mixed, find the first leg for Container or Loose.
		/// </summary>
		ZBool IsFirstLegCore(ZBool isFullLeg)
		{
			var result = false;

			if (Cartage.IsMixed)
			{
				if (Cartage.IsExportOrOrigin)
				{
					if (IsLoose)
					{
						result = GetFirstLooseLeg() == this;
					}
				}
				else if (IsContainerised)
				{
					result = GetFirstContainerLeg(isFullLeg) == this;
				}
			}
			else if (IsLoose)
			{
				result = GetFirstLooseLeg() == this;
			}
			else
			{
				result = GetFirstContainerLeg(isFullLeg) == this;
			}

			return result;
		}

		CommonCartageLeg GetFirstLooseLeg()
		{
			var firstLooseMove = Cartage.LooseBookedMoves.OrderBy(m => m.EW_DisplayOrder).FirstOrDefault();
			return firstLooseMove != null ? firstLooseMove.CartageLegs.OrderBy(m => m.JU_DisplayOrder).FirstOrDefault() : null;
		}

		CommonCartageLeg GetFirstContainerLeg(bool findFirstFullLegOnly)
		{
			var containerMove = BookedCtgMove;
			var legsInOrder = containerMove != null ? containerMove.CartageLegs.OrderBy(m => m.JU_DisplayOrder) : null;
			var firstFullLeg = legsInOrder != null ? legsInOrder.FirstOrDefault(l => !findFirstFullLegOnly || !l.JU_IsEmptyContainer) : null;
			return firstFullLeg ?? (legsInOrder != null ? containerMove.CartageLegs.OrderBy(m => m.JU_DisplayOrder).FirstOrDefault() : null);
		}

		public ZBool IsFirstFullLeg
		{
			get { return IsFirstLegCore(true); }
		}

		public ZBool IsRunSheetAuthorised
		{
			get { return isRunSheetAuthorised || (IsInDatabase && JU_EY_RunSheetInfo.OriginalValue.IsValid); }
		}

		ZBool isRunSheetAuthorised;

		void TryAuthorisedIfRequired()
		{
			if (!IsRunSheetAuthorised)
			{
				RunSheetSecurityProvider.GetProvider(Factory).TryAuthorise(this);
			}
		}

		public void AuthoriseRunSheet(ZString authorisedBy)
		{
			isRunSheetAuthorised = true;
			if (!authorisedBy.IsEmpty)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				this.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.CurrentCulture, "A non Customs Cleared Leg added to a Run Sheet by {0}.", authorisedBy));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		[ResourceStringData("CommonCartageLeg|PickupWaitTime", ShortCaption = "Pic. Wait", MediumCaption = "Pickup Wait", Caption = "Pickup Wait Time", FullDescription = "Actual Wait Time At Pickup.")]
		public ZString PickupWaitTime
		{
			get
			{
				ZString result = ZString.Empty;
				if (JU_PickupTimeIn.IsValid && JU_PickupTimeOut.IsValid)
				{
					result = (JU_PickupTimeOut - JU_PickupTimeIn).ToString();
				}
				return result;
			}
		}

		public ZPropertyInfo PickupWaitTimeInfo
		{
			get { return GetZPropertyInfo(Schema.PickupWaitTime); }
		}

		[ResourceStringData("CommonCartageLeg|DeliveryWaitTime", ShortCaption = "Dlv. Wait", MediumCaption = "Delivery Wait", Caption = "Delivery Wait Time", FullDescription = "Actual Wait Time At Delivery.")]
		public ZString DeliveryWaitTime
		{
			get
			{
				ZString result = ZString.Empty;
				if (JU_DeliverTimeIn.IsValid && JU_DeliverTimeOut.IsValid)
				{
					result = (JU_DeliverTimeOut - JU_DeliverTimeIn).ToString();
				}
				return result;
			}
		}

		public ZPropertyInfo DeliveryWaitTimeInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryWaitTime); }
		}

		[ResourceStringData("CommonCartageLeg|PostcodeDistance", ShortCaption = "P/C Dist.", MediumCaption = "P/C Distance", Caption = "Postcode Distance", FullDescription = "The Calculated Distance between the Pickup Address Postcode and Delivery Address Postcode.")]
		public ZDecimal PostcodeDistance
		{
			get
			{
				ZDecimal result = new ZDecimal(RefLatLongPostcode.CalculateDistance(PickupFromDocAddress, DeliverToDocAddress));
				if (JU_DistanceUnit != Constants.Length.Kilometres && Constants.Length.ContainsCode(JU_DistanceUnit))
				{
					result = Constants.Length.Convert(result, Constants.Length.Kilometres, JU_DistanceUnit);
				}

				return result;
			}
		}

		public ZPropertyInfo PostcodeDistanceInfo
		{
			get { return GetZPropertyInfo(Schema.PostcodeDistance); }
		}

		public ZString LegOfBookedMove
		{
			get
			{
				var move = BookedCtgMove;
				ZInt total = move != null ? move.CartageLegs.Count : 0;
				return Res.GetString("ab5e68ee-489b-4621-b5fb-9a67736cf648", "{0} of {1}", JU_DisplayOrder, total);
			}
		}

		public ZPropertyInfo LegOfBookedMoveInfo
		{
			get { return GetZPropertyInfo(Schema.LegOfBookedMove); }
		}

		public ZDecimal TotalWeight
		{
			get
			{
				ZDecimal result;
				var container = Container;
				if (container != null)
				{
					result = (JU_IsEmptyContainer || container.JC_IsEmptyContainer) ? container.JC_TareWeight : container.JC_GrossWeight;
				}
				else
				{
					var move = BookedCtgMove;
					result = move != null ? move.EW_BookedWeight : ZDecimal.Zero;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalWeight); }
		}

		public ZString TotalWeightUnit
		{
			get
			{
				ZString result;
				var container = Container;
				if (container != null)
				{
					if (container.JC_IsEmptyContainer)
					{
						result = "";
					}
					else
					{
						result = container.JC_GrossWeightUQ;
					}
				}
				else
				{
					var move = BookedCtgMove;
					result = move != null ? move.EW_WeightUQ : ZString.Empty;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalWeightUnit); }
		}

		[ResourceStringData("CommonCartageLeg|TruckDescription", ShortCaption = "Vehicle Desc.", Caption = "Vehicle Description")]
		public ZString TruckDescription
		{
			get
			{
				var runSheet = WorkSheet;
				return runSheet != null && runSheet.Truck != null ? runSheet.Truck.RQ_DescriptionMultilingual : ZString.Empty;
			}
		}

		public ZPropertyInfo TruckDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.TruckDescription); }
		}

		public ZString GetInstructionDropMode(JobDocAddress addressOnLeg)
		{
			var dropMode = ZString.Empty;

			if (IsRequestedAddress(addressOnLeg))
			{
				dropMode = BookedCtgMove.EW_DropMode;
			}

			return dropMode;
		}

		public ZBool IsRequestedAddress(JobDocAddress addressOnLeg)
		{
			return addressOnLeg != null && BookedCtgMove.RequestedAddressType == addressOnLeg.DocAddressType;
		}

		[ResourceStringData("CommonCartageLeg|QuickGSDriver", Caption = "Driver", FullDescription = "Quick Allocation Driver.")]
		[List("BindToLists.StaffDrivers")]
		[MaxLength(GlbStaff.Schema.GS_CodeMaxLength)]
		public ZString QuickGSDriver
		{
			get
			{
				if (!quickGSDriverHasValue)
				{
					var runSheet = WorkSheet;
					quickGSDriver = runSheet != null ? runSheet.EY_GS_NKTruckDriver : ZString.Empty;
					quickGSDriverHasValue = true;
				}
				return quickGSDriver;
			}
			set
			{
				if (QuickGSDriver != value)
				{
					CheckMaximumLength(QuickGSDriverInfo, value);

					if (!value.IsEmpty)
					{
						TryAuthorisedIfRequired();
					}

					SetNonPersistentPropertyValue(QuickGSDriverInfo, ref quickGSDriver, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuickGSDriver();
					}

					if (IsRunSheetAuthorised)
					{
						TryAllocateToExistingRunSheet(false);

						if (QuickRQTruck.IsEmpty)
						{
							GlbStaff driver = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, QuickGSDriver));

							if (driver != null)
							{
								ZQuery driverPreferredTruckQuery = new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true);
								driverPreferredTruckQuery.AddToFilter(RefEquipmentSchema.RQ_GS_NKPreferredDriver, QuickGSDriver);
								driverPreferredTruckQuery.OrderBy = RefEquipmentSchema.RQ_ShortCode.Name;

								RefEquipment driverPreferredTruck = Factory.LoadTop1<RefEquipment>(driverPreferredTruckQuery);

								if (driverPreferredTruck != null && !settingQuickDriverAndTruck)
								{
									settingQuickDriverAndTruck = true;
									try
									{
										QuickRQTruck = driverPreferredTruck.PK;
									}
									finally
									{
										settingQuickDriverAndTruck = false;
									}
								}
							}
						}
					}
				}
				quickGSDriverHasValue = true;

				QuickGSDriverInfo.RefreshBinding();
			}
		}
		bool quickGSDriverHasValue;
		bool settingQuickDriverAndTruck;
		ZString quickGSDriver;

		public ZPropertyInfo QuickGSDriverInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.QuickGSDriver);
				info.HumanReadableName = Res.GetString("A5776646-FF12-4EFE-A157-4E575806F721", "Driver");
				return info;
			}
		}

		protected bool QuickGSDriver_ReadOnly
		{
			get { return HasDeliveryStarted; }
		}

		[ResourceStringData("CommonCartageLeg|QuickRQTruck", Caption = "Vehicle", FullDescription = "Quick Allocation Vehicle.")]
		[List("Vehicles")]
		public ZGuid QuickRQTruck
		{
			get
			{
				if (quickRQTruck.IsMissing)
				{
					var runSheet = WorkSheet;
					quickRQTruck = runSheet != null ? runSheet.EY_RQ_Truck : ZGuid.Empty;
				}
				return quickRQTruck;
			}
			set
			{
				if (QuickRQTruck != value)
				{
					if (value.IsValid)
					{
						TryAuthorisedIfRequired();
					}

					SetNonPersistentPropertyValue(QuickRQTruckInfo, ref quickRQTruck, value.IsMissing ? ZGuid.Invalid : value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuickRQTruck();
					}

					if (IsRunSheetAuthorised)
					{
						TryAllocateToExistingRunSheet(false);
						AllocateTransportCoFromTruck();

						if (QuickGSDriver.IsEmpty)
						{
							RefEquipment truck = Factory.Load<RefEquipment>(QuickRQTruck);

							if (truck != null && !truck.RQ_GS_NKPreferredDriver.IsEmpty && !settingQuickDriverAndTruck)
							{
								settingQuickDriverAndTruck = true;
								try
								{
									QuickGSDriver = truck.RQ_GS_NKPreferredDriver;
								}
								finally
								{
									settingQuickDriverAndTruck = false;
								}
							}
						}
					}
				}

				QuickRQTruckInfo.RefreshBinding();
			}
		}
		ZGuid quickRQTruck = ZGuid.Missing;

		public ZPropertyInfo QuickRQTruckInfo
		{
			get { return GetZPropertyInfo(Schema.QuickRQTruck); }
		}

		protected bool QuickRQTruck_ReadOnly
		{
			get { return HasDeliveryStarted; }
		}

		void AllocateTransportCoFromTruck()
		{
			if (!QuickRQTruck.IsEmpty)
			{
				RefEquipment quickTruck = Factory.Load<RefEquipment>(QuickRQTruck);
				if (quickTruck != null)
				{
					if (QuickOHTransportCompany.IsEmpty && quickTruck.RQ_OH_Owner.IsValid)
					{
						QuickOHTransportCompany = quickTruck.RQ_OH_Owner;

						if (WorkSheet != null)
						{
							WorkSheet.EY_OH_TransportCo = quickTruck.RQ_OH_Owner;
							WorkSheet.EY_OH_TransportCoInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public RefEquipmentCollection Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					vehicles = new RefEquipmentCollection(Factory, new ZQuery(RefEquipmentSchema.RQ_IsVehicle, true));
					vehicles.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vehicle Status", "Property", (ZString)(NoResString)"Is a Vehicle"));
					vehicles.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Driver", "Property", delegate
					{ return QuickGSDriver; }));
				}
				return vehicles;
			}
		}

		RefEquipmentCollection vehicles;

		[ResourceStringData("CommonCartageLeg|QuickOHTransportCompany", Caption = "Transport Company", FullDescription = "Quick Allocation Transport Company.")]
		[List("BindToLists.TransportProviders")]
		public ZGuid QuickOHTransportCompany
		{
			get
			{
				if (quickOHTransportCompany.IsMissing)
				{
					var runSheet = WorkSheet;
					quickOHTransportCompany = runSheet != null ? runSheet.EY_OH_TransportCo : ZGuid.Empty;
				}
				return quickOHTransportCompany;
			}
			set
			{
				if (QuickOHTransportCompany != value)
				{
					if (value.IsValid)
					{
						TryAuthorisedIfRequired();
					}

					SetNonPersistentPropertyValue(QuickOHTransportCompanyInfo, ref quickOHTransportCompany, value.IsMissing ? ZGuid.Invalid : value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuickOHTransportCompany();
					}

					if (IsRunSheetAuthorised)
					{
						TryAllocateToExistingRunSheet(false);
					}
				}

				QuickOHTransportCompanyInfo.RefreshBinding();
			}
		}
		ZGuid quickOHTransportCompany = ZGuid.Missing;

		public ZPropertyInfo QuickOHTransportCompanyInfo
		{
			get { return GetZPropertyInfo(Schema.QuickOHTransportCompany); }
		}

		protected bool QuickOHTransportCompany_ReadOnly
		{
			get { return HasDeliveryStarted; }
		}

		[ResourceStringData("CommonCartageLeg|QuickPlannedPickupTime", ShortCaption = "Plan. Pic.", MediumCaption = "Plan. Pickup", Caption = "Planned Pickup", FullDescription = "Planned (Estimated) Pickup Time.")]
		public ZDateTime QuickPlannedPickupTime
		{
			get { return JU_PlannedPickupTime; }
			set
			{
				if (JU_PlannedPickupTime != value)
				{
					JU_PlannedPickupTime = value;

					TryAllocateToExistingRunSheet(false);

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuickPlannedPickupTime();
					}
				}

				JU_PlannedPickupTimeInfo.RefreshBinding();
				QuickPlannedPickupTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo QuickPlannedPickupTimeInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.QuickPlannedPickupTime);
				info.HumanReadableName = Res.GetString("962789F3-6431-48E2-AAA8-1D8B83D3B407", "Planned Pickup");
				return info;
			}
		}

		protected bool QuickPlannedPickupTime_ReadOnly
		{
			get { return HasDeliveryStarted; }
		}

		[ResourceStringData("CommonCartageLeg|QuickEstimatedDeliveryTime", ShortCaption = "Plan. Dlv.", MediumCaption = "Plan. Delivery", Caption = "Planned Delivery", FullDescription = "Planned (Estimated) Delivery Time.")]
		public ZDateTime QuickEstimatedDeliveryTime
		{
			get { return JU_EstimatedDeliveryTime; }
			set
			{
				if (JU_EstimatedDeliveryTime != value)
				{
					JU_EstimatedDeliveryTime = value;

					TryAllocateToExistingRunSheet(false);

					if (!IsValidationSuspended)
					{
						Validation.ValidateQuickEstimatedDeliveryTime();
					}
				}

				JU_EstimatedDeliveryTimeInfo.RefreshBinding();
				QuickEstimatedDeliveryTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo QuickEstimatedDeliveryTimeInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.QuickEstimatedDeliveryTime);
				info.HumanReadableName = Res.GetString("EC614104-25A6-4C44-A449-AAEAB438210B", "Planned Delivery");
				return info;
			}
		}

		protected bool QuickEstimatedDeliveryTime_ReadOnly
		{
			get { return HasDeliveryStarted; }
		}

		internal ZDateTime QuickEstimatedRunSheetTime
		{
			get { return JU_EstimatedDeliveryTime.IsValid ? JU_EstimatedDeliveryTime : JU_PlannedPickupTime; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void TryAllocateToExistingRunSheet(bool createIfDoesNotExist)
		{
			var currentRunSheet = WorkSheet;
			var hasQuickWorkSheetAllocation = HasQuickWorkSheetAllocation;
			var quickEstimatedRunSheetTime = QuickEstimatedRunSheetTime;
			var quickTruck = QuickRQTruck;
			var quickDriver = QuickGSDriver;
			var quickTransportCo = QuickOHTransportCompany;

			if (currentRunSheet != null && (!hasQuickWorkSheetAllocation || !quickEstimatedRunSheetTime.IsValid))
			{
				JU_EY_RunSheet = ZGuid.Empty;
				ResetQuickWorkSheetAllocationFields();
			}
			else if (hasQuickWorkSheetAllocation && quickEstimatedRunSheetTime.IsValid)
			{
				// Run simple query ... match other data after, so we reduce db hits.
				var nearRunSheetQuery = new ZQuery();
				nearRunSheetQuery.AddToFilter(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, quickEstimatedRunSheetTime);
				nearRunSheetQuery.AddToFilter(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, quickEstimatedRunSheetTime.AddDays(1));

				CommonWorkSheet exactMatchRunSheet = null;
				CommonWorkSheet nearMatchRunSheet = null;
				var nearRunSheets = Factory.Load<CommonWorkSheet>(nearRunSheetQuery);
				foreach (var possibleMatch in nearRunSheets)
				{
					if (quickEstimatedRunSheetTime >= possibleMatch.EY_StartTime && (quickEstimatedRunSheetTime <= possibleMatch.EY_EndTime || AreBothAlmostMidnightOnSameDate(quickEstimatedRunSheetTime, possibleMatch.EY_EndTime)))
					{
						if (possibleMatch.EY_RQ_Truck == quickTruck
							&& possibleMatch.EY_GS_NKTruckDriver == quickDriver
							&& possibleMatch.EY_OH_TransportCo == quickTransportCo)
						{
							exactMatchRunSheet = possibleMatch;
							break;
						}
						else if (nearMatchRunSheet == null &&
							(quickTruck.IsEmpty || possibleMatch.EY_RQ_Truck == quickTruck) &&
							(quickDriver.IsEmpty || possibleMatch.EY_GS_NKTruckDriver == quickDriver) &&
							(quickTransportCo.IsEmpty || possibleMatch.EY_OH_TransportCo == quickTransportCo))
						{
							nearMatchRunSheet = possibleMatch;
						}
					}
				}

				if (exactMatchRunSheet != null && exactMatchRunSheet.PK != JU_EY_RunSheet)
				{
					JU_EY_RunSheet = exactMatchRunSheet.PK;
				}
				else if (nearMatchRunSheet != null && nearMatchRunSheet.PK != JU_EY_RunSheet)
				{
					// Found a better AND DIFFERENT match, so assign new RunSheet.
					JU_EY_RunSheet = nearMatchRunSheet.PK;
					ResetQuickWorkSheetAllocationFields();
				}
				else if (currentRunSheet != null)
				{
					// Didn't find a better match, maybe update current RunSheet?
					if (currentRunSheet.CartageLegs.Count == 1)
					{
						// Because this is the only Leg on the RunSheet, update it.
						if (quickEstimatedRunSheetTime < currentRunSheet.EY_StartTime || (quickEstimatedRunSheetTime > currentRunSheet.EY_EndTime && !AreBothAlmostMidnightOnSameDate(quickEstimatedRunSheetTime, currentRunSheet.EY_EndTime)))
						{
							currentRunSheet.EY_StartTime = quickEstimatedRunSheetTime.Date;
							currentRunSheet.EY_EndTime = quickEstimatedRunSheetTime.EndOfDay().AddSeconds(-59);
						}
						currentRunSheet.EY_RQ_Truck = quickTruck;
						currentRunSheet.EY_GS_NKTruckDriver = quickDriver;
						currentRunSheet.EY_OH_TransportCo = quickTransportCo;
					}
					else
					{
						// Other legs are to be affected.
						if (IsAddingInfoToRunSheet && quickEstimatedRunSheetTime > currentRunSheet.EY_StartTime && (quickEstimatedRunSheetTime < currentRunSheet.EY_EndTime || AreBothAlmostMidnightOnSameDate(quickEstimatedRunSheetTime, currentRunSheet.EY_EndTime)))
						{
							// Adding value to RunSheet and is still in time is deemed OK.
							currentRunSheet.EY_RQ_Truck = quickTruck;
							currentRunSheet.EY_GS_NKTruckDriver = quickDriver;
							currentRunSheet.EY_OH_TransportCo = quickTransportCo;
						}
						else
						{
							// Changing or REMOVING values, so unassign RunSheet, but don't clear quick allocation fields. Assigning of new RunSheet to occur OnSaving.
							try
							{
								dontReset = true;
								JU_EY_RunSheet = ZGuid.Empty;
							}
							finally
							{
								dontReset = false;
							}
						}
					}
				}
				else if (createIfDoesNotExist)
				{
					var newRunSheet = Factory.New<CommonWorkSheet>();
					newRunSheet.EY_StartTime = quickEstimatedRunSheetTime.Date;
					newRunSheet.EY_EndTime = quickEstimatedRunSheetTime.EndOfDay().AddSeconds(-59);
					newRunSheet.EY_RQ_Truck = quickTruck;
					newRunSheet.EY_GS_NKTruckDriver = quickDriver;
					newRunSheet.EY_OH_TransportCo = quickTransportCo;
					JU_EY_RunSheet = newRunSheet.PK;
				}
			}
		}

		void ResetQuickWorkSheetAllocationFields()
		{
			if (!dontReset)
			{
				quickGSDriverHasValue = false;
				quickRQTruck = ZGuid.Missing;
				quickOHTransportCompany = ZGuid.Missing;

				QuickGSDriverInfo.RefreshBinding();
				QuickRQTruckInfo.RefreshBinding();
				QuickOHTransportCompanyInfo.RefreshBinding();
			}
		}
		bool dontReset;

		public bool HasQuickWorkSheetAllocation
		{
			get { return !QuickGSDriver.IsEmpty || QuickRQTruck.IsValid || QuickOHTransportCompany.IsValid; }
		}

		bool IsAddingInfoToRunSheet
		{
			get
			{
				var runSheet = WorkSheet;
				return runSheet != null &&
					(runSheet.EY_GS_NKTruckDriverInfo.OriginalValue.IsEmpty || runSheet.EY_GS_NKTruckDriver == QuickGSDriver) &&
					(runSheet.EY_RQ_TruckInfo.OriginalValue.IsEmpty || runSheet.EY_RQ_Truck == QuickRQTruck) &&
					(runSheet.EY_OH_TransportCoInfo.OriginalValue.IsEmpty || runSheet.EY_OH_TransportCo == QuickOHTransportCompany);
			}
		}

		public ZString PickupAddressCode
		{
			get
			{
				ZString result = "";
				var pickupFromAddress = PickupFromDocAddress;
				if (pickupFromAddress != null)
				{
					var pickupAddress = pickupFromAddress.Address;
					var pickupOrganisation = pickupAddress != null ? pickupAddress.Header : null;

					var pickupOrgCode = pickupOrganisation != null ? pickupOrganisation.OH_Code : ZString.Empty;
					var pickupAddressCode = pickupAddress != null ? pickupAddress.OA_Code : ZString.Empty;
					result = ZString.Format("{0} - {1}", pickupOrgCode, pickupAddressCode).ToUpper();
				}

				return result;
			}
		}

		public ZPropertyInfo PickupAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PickupAddressCode); }
		}

		public ZString DeliveryAddressCode
		{
			get
			{
				ZString result = "";
				var deliverToDocAddress = DeliverToDocAddress;
				if (deliverToDocAddress != null)
				{
					var deliveryAddress = deliverToDocAddress.Address;
					var deliveryOrganisation = deliveryAddress != null ? deliveryAddress.Header : null;

					var deliveryOrgCode = deliveryOrganisation != null ? deliveryOrganisation.OH_Code : ZString.Empty;
					var deliveryAddressCode = deliveryAddress != null ? deliveryAddress.OA_Code : ZString.Empty;
					return ZString.Format("{0} - {1}", deliveryOrgCode, deliveryAddressCode).ToUpper();
				}
				return result;
			}
		}

		public ZPropertyInfo DeliveryAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryAddressCode); }
		}

		public ZString WaitPointAddressCode
		{
			get
			{
				ZString result = "";
				var waitPointDocAddress = WaitPointDocAddress;
				if (waitPointDocAddress != null)
				{
					var waitPointAddress = waitPointDocAddress.Address;
					var waitPointOrganisation = waitPointAddress != null ? waitPointAddress.Header : null;

					var waitPointOrgCode = waitPointOrganisation != null ? waitPointOrganisation.OH_Code : ZString.Empty;
					var waitPointAddressCode = waitPointAddress != null ? waitPointAddress.OA_Code : ZString.Empty;
					return ZString.Format("{0} - {1}", waitPointOrgCode, waitPointAddressCode).ToUpper();
				}
				return result;
			}
		}

		public ZPropertyInfo WaitPointAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.WaitPointAddressCode); }
		}

		public ZString PickupTimeSummary
		{
			get
			{
				ZString result = "";

				if (JU_PickupTimeIn.IsValid || JU_PickupTimeOut.IsValid)
				{
					result = AddDate(result, false, Res.GetString("670dbb82-9006-472e-af53-49b79a1d6e5c", "@"), JU_PickupTimeIn);
					result = AddDate(result, HaveSameDay(JU_PickupTimeIn, JU_PickupTimeOut), "-", JU_PickupTimeOut);
				}
				else
				{
					result = AddDate(result, false, Res.GetString("9883dc1e-85b0-41e3-957a-379b7d96c266", "@"), JU_PlannedPickupTime);
				}

				return result.Trim();
			}
		}

		public ZPropertyInfo PickupTimeSummaryInfo
		{
			get { return GetZPropertyInfo(Schema.PickupTimeSummary); }
		}

		public ZString WaitPointTimeSummary
		{
			get
			{
				ZString result = "";
				if (JU_WaitPointTimeIn.IsValid || JU_WaitPointTimeOut.IsValid)
				{
					result = AddDate(result, false, Res.GetString("859200f3-08d6-4316-865b-e066f02f6e71", "@"), JU_WaitPointTimeIn);
					result = AddDate(result, HaveSameDay(JU_WaitPointTimeIn, JU_WaitPointTimeOut), "-", JU_WaitPointTimeOut);
				}
				else
				{
					result = AddDate(result, false, Res.GetString("c7a56577-be19-4929-8a5e-353e39f51bfb", "@"), JU_EstimatedDeliveryTime);
				}
				return result.Trim();
			}
		}

		public ZPropertyInfo WaitPointTimeSummaryInfo
		{
			get { return GetZPropertyInfo(Schema.WaitPointTimeSummary); }
		}

		public ZString DeliveryTimeSummary
		{
			get
			{
				ZString result = "";

				if (JU_DeliverTimeIn.IsValid || JU_DeliverTimeOut.IsValid)
				{
					result = AddDate(result, false, Res.GetString("670dbb82-9006-472e-af53-49b79a1d6e5c", "@"), JU_DeliverTimeIn);
					result = AddDate(result, HaveSameDay(JU_DeliverTimeIn, JU_DeliverTimeOut), "-", JU_DeliverTimeOut);
				}
				else
				{
					result = AddDate(result, false, Res.GetString("9883dc1e-85b0-41e3-957a-379b7d96c266", "@"), JU_EstimatedDeliveryTime);
					//if (DeliverToDocAddressType == DocAddressType.LocalCartageCTO && Container != null)
					//{
					//    result = AddDate(result, HaveSameDay(JU_EstimatedDeliveryTime, Container.JC_DepartureSlotDateTime), "Slt:", Container.JC_DepartureSlotDateTime);
					//}
				}

				return result.Trim();
			}
		}

		public ZPropertyInfo DeliveryTimeSummaryInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryTimeSummary); }
		}

		public ZString SequenceAndActive
		{
			get
			{
				var runSheet = WorkSheet;
				var selected = runSheet != null && runSheet.ActiveLegs.Contains(this) ? Res.GetString("fd90fd87-aa7f-4a40-945a-f68bb502abdf", "♦") : "";
				return ZString.Format("{0} {1}", selected, JU_RunSheetSequence).Trim();
			}
		}

		public ZPropertyInfo SequenceAndActiveInfo
		{
			get { return GetZPropertyInfo(Schema.SequenceAndActive); }
		}

		public ZString WhatSummary
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				var move = BookedCtgMove;
				if (move != null)
				{
					if (IsContainerised)
					{
						result.Append(ContainerDescription);
					}
					else
					{
						result.Append(LooseCargoGoodsDescription);
					}

					result.AppendIfNotEmpty(move.EW_DropMode);
					result.AppendIfNotEmpty(UniqueIDWithJobNumber);

					var trailer = Trailer;
					if (trailer != null)
					{
						result.Append(trailer.RQ_ShortCode);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends("  ").ToUpper(CultureInfo.CurrentCulture);
			}
		}

		public ZPropertyInfo WhatSummaryInfo
		{
			get { return GetZPropertyInfo(Schema.WhatSummary); }
		}

		ZString LooseCargoGoodsDescription
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				if (BookedCtgMove != null)
				{
					result.Append(string.Format(CultureInfo.CurrentCulture, "{0} {1}", BookedCtgMove.EW_BookedPackCount, BookedCtgMove.EW_F3_NKPackType));

					if (BookedCtgMove.EW_BookedVolume > 0)
					{
						result.Append(string.Format(CultureInfo.CurrentCulture, "{0:N1} {1}", BookedCtgMove.EW_BookedVolume, BookedCtgMove.EW_VolumeUQ));
					}

					if (BookedCtgMove.EW_BookedWeight > 0)
					{
						result.Append(string.Format(CultureInfo.CurrentCulture, "{0:N0} {1}", BookedCtgMove.EW_BookedWeight, BookedCtgMove.EW_WeightUQ));
					}
				}

				return result.ToStringWithDelimiterBetweenAppends("  ");
			}
		}

		public ZString ContainerDescription
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				if (BookedCtgMove != null && Container != null)
				{
					result.Append(Container.JC_ContainerNum);

					if (Container.Container != null)
					{
						result.Append(Container.Container.RC_Code);
					}
				}

				return result.ToStringWithDelimiterBetweenAppends("  ");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
		public enum LegStatuses
		{
			None,
			Dispatched,
			TimeIn,
			TimeOut,
			Error
		}

		public LegStatuses LegStatus
		{
			get
			{
				switch (JU_MessageStatus)
				{
					case Constants.CartageLegDispatchStatusList.Codes.NotStarted:
					case Constants.CartageLegDispatchStatusList.Codes.Runsheet:
					case Constants.CartageLegDispatchStatusList.Codes.WIP:
						return LegStatuses.Dispatched;

					case Constants.CartageLegDispatchStatusList.Codes.Rejected:
					case Constants.CartageLegDispatchStatusList.Codes.Futile:
						return LegStatuses.Error;

					case Constants.CartageLegDispatchStatusList.Codes.PickedUp:
					case Constants.CartageLegDispatchStatusList.Codes.Delivered:
					case Constants.CartageLegDispatchStatusList.Codes.PickingUp:
					case Constants.CartageLegDispatchStatusList.Codes.Delivering:
						return WasDispatchedViaAttachmentToRunSheet || WasDispatchedViaRunSheetDocument ? LegStatuses.Dispatched : LegStatuses.None;

					default:
						return LegStatuses.None;
				}
			}
		}

		public ZString ErrorStatusDescription
		{
			get
			{
				switch (JU_MessageStatus)
				{
					case Constants.CartageLegDispatchStatusList.Codes.Rejected:
						return Res.GetString("ead891da-9f1d-4692-baeb-a9d7574b203c", "Rejected");
					case Constants.CartageLegDispatchStatusList.Codes.Futile:
						return Res.GetString("df66596b-2538-4a04-a6a7-f127fdce5bbe", "Futile");
					default:
						return "";
				}
			}
		}

		public ZString PickupStatus
		{
			get
			{
				if (LegStatus == LegStatuses.Error)
				{
					return nameof(LegStatuses.Error);
				}

				if (JU_PickupTimeOut.IsValid)
				{
					return nameof(LegStatuses.TimeOut);
				}
				else if (JU_PickupTimeIn.IsValid)
				{
					return nameof(LegStatuses.TimeIn);
				}
				else
				{
					return LegStatus.ToString();
				}
			}
		}

		public ZPropertyInfo PickupStatusInfo
		{
			get { return GetZPropertyInfo(Schema.PickupStatus); }
		}

		public ZString DeliveryStatus
		{
			get
			{
				if (LegStatus == LegStatuses.Error)
				{
					return nameof(LegStatuses.Error);
				}

				if (JU_DeliverTimeOut.IsValid)
				{
					return nameof(LegStatuses.TimeOut);
				}
				else if (JU_DeliverTimeIn.IsValid)
				{
					return nameof(LegStatuses.TimeIn);
				}
				else
				{
					return LegStatus.ToString();
				}
			}
		}

		public ZPropertyInfo DeliveryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryStatus); }
		}

		public ZString WaitPointStatus
		{
			get
			{
				if (LegStatus == LegStatuses.Error)
				{
					return nameof(LegStatuses.Error);
				}

				if (JU_WaitPointTimeOut.IsValid)
				{
					return nameof(LegStatuses.TimeOut);
				}
				else if (JU_WaitPointTimeIn.IsValid)
				{
					return nameof(LegStatuses.TimeIn);
				}
				else
				{
					return LegStatus.ToString();
				}
			}
		}

		public ZPropertyInfo WaitPointStatusInfo
		{
			get { return GetZPropertyInfo(Schema.WaitPointStatus); }
		}

		bool HaveSameDay(ZDateTime date1, ZDateTime date2)
		{
			return date1.IsValid && date2.IsValid && date1.Date == date2.Date;
		}

		ZString AddDate(ZString main, bool timeOnly, ZString separator, ZDateTime dateTime)
		{
			ZString result = main;
			ZString dayTime = "";
			timeOnly = false;

			if (dateTime.IsValid)
			{
				if (!timeOnly)
				{
					dayTime = GetDay(dateTime) + " ";
				}
				dayTime += GetTime(dateTime);

				result += ZString.Format(" {0} {1}", separator, dayTime);
				result = result.Trim();
			}
			else
			{
				result += ZString.Format(" {0} {1}", separator, "                      ");
			}

			return result;
		}

		ZString GetDay(ZDateTime dateTime)
		{
			return dateTime.IsValid ? dateTime.ToString("dd MMM", CultureInfo.InvariantCulture) : "";
		}

		ZString GetTime(ZDateTime dateTime)
		{
			if (!dateTime.IsValid)
			{
				return "";
			}

			return dateTime.ToShortTimeString();
		}

		public ZBool IsComplete
		{
			get
			{
				return
					JU_PickupTimeIn.IsValid &&
					JU_PickupTimeOut.IsValid &&
					JU_DeliverTimeIn.IsValid &&
					JU_DeliverTimeOut.IsValid;
			}
		}

		void CheckJobCompletion(ZDateTime lastLegTimeOutBeforeChange)
		{
			var cartage = Cartage;
			if (cartage != null)
			{
				cartage.CheckJobCompletion(lastLegTimeOutBeforeChange);
			}
		}

		void CheckJobCompletion()
		{
			var cartage = Cartage;
			if (cartage != null)
			{
				cartage.CheckJobCompletion();
			}
		}

		public ZBool IsPartComplete
		{
			get
			{
				return
					JU_PickupTimeIn.IsValid ||
					JU_PickupTimeOut.IsValid ||
					JU_DeliverTimeIn.IsValid ||
					JU_DeliverTimeOut.IsValid;
			}
		}

		public ZBool IsFutile
		{
			get { return JU_MessageStatus == Constants.CartageLegDispatchStatusList.Codes.Futile || JU_AdditionalService == Constants.CartageAdditional.Futile; }
		}

		public SignatureData Signature
		{
			get { return new SignatureData(this); }
		}

		public void UpdateSignature(byte[] signatureData, Guid departmentGuid)
		{
			Signature.SetSignatureData(signatureData, departmentGuid);
			AddSignatureCapturedEvent();
		}

		public bool ShowSignature
		{
			get { return Signature.ShowSignature; }
		}

		public TimeSpan GetTotalDemurrage()
		{
			var totalDemurrageTime = new TimeSpan();

			totalDemurrageTime += GetDemurrageIncludingFreeTime(JU_CartagePickupDemurrage, PickupFromDocAddress);
			totalDemurrageTime += GetDemurrageIncludingFreeTime(JU_CartageWaitPointDemurrage, WaitPointDocAddress);
			totalDemurrageTime += GetDemurrageIncludingFreeTime(JU_CartageDeliveryDemurrage, DeliverToDocAddress);

			return totalDemurrageTime.TotalMinutes > 0 ? totalDemurrageTime : new TimeSpan();
		}

		public TimeSpan GetDemurrageIncludingFreeTime(ZDateTime demurrageTime, JobDocAddress address)
		{
			IOrgFreeWaitingTime freeWaitingTime = null;
			var useCumulative = false;

			var localClientAddress = Cartage != null ? Cartage.LocalClientAddress : null;
			if (localClientAddress != null)
			{
				var collection = localClientAddress.FreeWaitingCollection.Where(a => a.Company.PK == GlbCompany.CurrentCompany.PK);
				freeWaitingTime = GetFreeWaitingTimeItemFromCollection(collection.ToArray());
				useCumulative = freeWaitingTime != null && localClientAddress.OA_UseCumulativeFreeWaitingTime;
			}

			if (freeWaitingTime == null)
			{
				var collection = TransportRegistry.Instance.AmountOfFreeWaitingTime.Value.Cast<IOrgFreeWaitingTime>().ToArray();
				freeWaitingTime = GetFreeWaitingTimeItemFromCollection(collection.ToArray());
				useCumulative = TransportRegistry.Instance.UseCumulativeFreeWaitingTime.Value;
			}

			var docAddressType = address != null ? address.DocAddressType : DocAddressType.None;
			var timeAtAddress = !demurrageTime.IsEmpty && demurrageTime.IsValid ? new TimeSpan(demurrageTime.DayOfYear - 1, demurrageTime.Hour, demurrageTime.Minute, 0) : new TimeSpan();
			var demurrageAtAddress = timeAtAddress - GetFreeTimeSpan(freeWaitingTime, docAddressType);

			return useCumulative || demurrageAtAddress.TotalMinutes > 0 ? demurrageAtAddress : new TimeSpan();
		}

		TimeSpan GetFreeTimeSpan(IOrgFreeWaitingTime freeWaitingTimeItem, DocAddressType addressType)
		{
			ZDateTime freeTimeDate = GetFreeDateTime(freeWaitingTimeItem, addressType);
			return !freeTimeDate.IsEmpty && freeTimeDate.IsValid ? new TimeSpan(freeTimeDate.DayOfYear - 1, freeTimeDate.Hour, freeTimeDate.Minute, 0) : new TimeSpan();
		}

		IOrgFreeWaitingTime GetFreeWaitingTimeItemFromCollection(IOrgFreeWaitingTime[] freeWaitingTimeCollection)
		{
			var containerType = Container != null ? Container.Container : null;
			var containerTypePk = containerType != null ? containerType.PK : ZGuid.Empty;
			var dropMode = BookedCtgMove != null ? BookedCtgMove.EW_DropMode : ZString.Empty;
			var freeWaitingTimeItem = freeWaitingTimeCollection.FirstOrDefault(i => i.DropMode == dropMode && i.CNTType == containerTypePk);
			freeWaitingTimeItem = freeWaitingTimeItem ?? freeWaitingTimeCollection.FirstOrDefault(i => i.DropMode == dropMode && i.CNTType.IsEmpty);
			freeWaitingTimeItem = freeWaitingTimeItem ?? freeWaitingTimeCollection.FirstOrDefault(i => i.DropMode == Constants.EquipmentNeeded.Any && i.CNTType == containerTypePk);
			freeWaitingTimeItem = freeWaitingTimeItem ?? freeWaitingTimeCollection.FirstOrDefault(i => i.DropMode == Constants.EquipmentNeeded.Any && i.CNTType.IsEmpty);
			return freeWaitingTimeItem;
		}

		ZDateTime GetFreeDateTime(IOrgFreeWaitingTime freeWaitingTimeItem, DocAddressType addressType)
		{
			var result = ZDateTime.Empty;

			if (freeWaitingTimeItem != null)
			{
				switch (addressType)
				{
					case DocAddressType.LocalCartageCTO:
						result = freeWaitingTimeItem.CTO;
						break;
					case DocAddressType.LocalCartageCFS:
						result = freeWaitingTimeItem.CFS;
						break;
					case DocAddressType.LocalCartageExporter:
						result = freeWaitingTimeItem.CNR;
						break;
					case DocAddressType.LocalCartageImporter:
						result = freeWaitingTimeItem.CNE;
						break;
					case DocAddressType.LocalCartageYard:
						result = freeWaitingTimeItem.CYD;
						break;
					default:
						result = freeWaitingTimeItem.Other;
						break;
				}
			}
			return result;
		}

		[ChildEditableTestExclude()]
		[ChildEditable()]
		public CommonWorkSheet WorkSheet
		{
			get
			{
				CommonWorkSheet workSheet = null;
				if (!IsDeleted)
				{
					workSheet = Factory.Load<CommonWorkSheet>(JU_EY_RunSheet);

					if (workSheet != null && !workSheet.IsRoot && !IsRegisteredEditableChildObject(workSheet))
					{
						RegisterEditableChildObject(workSheet);
					}
				}

				return workSheet;
			}
		}

		[ChildEditableTestExclude()]
		[ChildEditable()]
		public CommonBookedCtgMove BookedCtgMove
		{
			get
			{
				CommonBookedCtgMove ctgMove = null;
				if (!IsDeleted)
				{
					ctgMove = Factory.Load<CommonBookedCtgMove>(JU_EW);
					if (ctgMove != null)
					{
						var cartage = ctgMove.Cartage;
						if (cartage != null && !cartage.IsRoot && !IsRegisteredEditableChildObject(ctgMove))
						{
							RegisterEditableChildObject(ctgMove);
						}
					}
				}

				return ctgMove;
			}
		}

		public CommonCartage Cartage
		{
			get
			{
				var move = BookedCtgMove;
				return move != null ? move.Cartage : null;
			}
		}

		public CommonContainer Container
		{
			get
			{
				var move = BookedCtgMove;
				return move != null ? move.Container : null;
			}
		}

		public OrgHeader TransportCo
		{
			get
			{
				var runSheet = WorkSheet;
				return runSheet != null ? runSheet.TransportCo : null;
			}
		}

		public RefEquipment Truck
		{
			get
			{
				var runSheet = WorkSheet;
				return runSheet != null ? runSheet.Truck : null;
			}
		}

		public RefEquipment Vehicle
		{
			get
			{
				var runSheet = WorkSheet;
				return runSheet != null ? runSheet.Truck : null;
			}
		}

		public CartageBindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new CartageBindToLists(Factory)); }
		}
		CartageBindToLists bindToLists;

		public JobDocAddress PickupFromDocAddress
		{
			get { return Factory.Load<JobDocAddress>(JU_E2PickupAddressID); }
		}

		public OrgHeader PickupOrganisation
		{
			get
			{
				var pickupFromAddress = PickupFromDocAddress;
				return pickupFromAddress != null ? pickupFromAddress.Organisation : null;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_Address1MaxLength)]
		public ZString PickupFromAddress
		{
			get
			{
				var pickupFromAddress = PickupFromDocAddress;
				return pickupFromAddress != null ? pickupFromAddress.E2_Address1 : ZString.Empty;
			}
		}

		public ZPropertyInfo PickupFromAddressInfo
		{
			get { return GetZPropertyInfo(Schema.PickupFromAddress); }
		}

		[MaxLength(JobDocAddress.Schema.E2_CityMaxLength)]
		public ZString PickupFromCity
		{
			get
			{
				var pickupFromAddress = PickupFromDocAddress;
				return pickupFromAddress != null ? pickupFromAddress.E2_City : ZString.Empty;
			}
		}

		public ZPropertyInfo PickupFromCityInfo
		{
			get { return GetZPropertyInfo(Schema.PickupFromCity); }
		}

		public JobDocAddress WaitPointDocAddress
		{
			get { return Factory.Load<JobDocAddress>(JU_E2WaitPointAddressID); }
		}

		public OrgHeader WaitPointOrganisation
		{
			get
			{
				var waitPointAddress = WaitPointDocAddress;
				return waitPointAddress != null ? waitPointAddress.Organisation : null;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_Address1MaxLength)]
		public ZString WaitPointAddress
		{
			get
			{
				var waitPointAddress = WaitPointDocAddress;
				return waitPointAddress != null ? waitPointAddress.E2_Address1 : ZString.Empty;
			}
		}

		public ZPropertyInfo WaitPointAddressInfo
		{
			get { return GetZPropertyInfo(Schema.WaitPointAddress); }
		}

		[MaxLength(JobDocAddress.Schema.E2_CityMaxLength)]
		public ZString WaitPointCity
		{
			get
			{
				var waitPointAddress = WaitPointDocAddress;
				return waitPointAddress != null ? waitPointAddress.E2_City : ZString.Empty;
			}
		}

		public ZPropertyInfo WaitPointCityInfo
		{
			get { return GetZPropertyInfo(Schema.WaitPointCity); }
		}

		public JobDocAddress DeliverToDocAddress
		{
			get { return Factory.Load<JobDocAddress>(JU_E2DeliveryAddressID); }
		}

		public OrgHeader DeliverOrganisation
		{
			get
			{
				var deliverToAddress = DeliverToDocAddress;
				return deliverToAddress != null ? deliverToAddress.Organisation : null;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_Address1MaxLength)]
		public ZString DeliverToAddress
		{
			get
			{
				var deliverToAddress = DeliverToDocAddress;
				return deliverToAddress != null ? deliverToAddress.E2_Address1 : ZString.Empty;
			}
		}

		public ZPropertyInfo DeliverToAddressInfo
		{
			get { return GetZPropertyInfo(Schema.DeliverToAddress); }
		}

		[MaxLength(JobDocAddress.Schema.E2_CityMaxLength)]
		public ZString DeliverToCity
		{
			get
			{
				var deliverToAddress = DeliverToDocAddress;
				return deliverToAddress != null ? deliverToAddress.E2_City : ZString.Empty;
			}
		}

		public ZPropertyInfo DeliverToCityInfo
		{
			get { return GetZPropertyInfo(Schema.DeliverToCity); }
		}

		void AddPickUpOrDeliveryEvent(JobDocAddress docAddress, ZString city, Event eventType, ZPropertyInfo timeOutPropertyInfo, ZString reason)
		{
			var reference = EventReference;
			var facility = DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(docAddress);
			var suburb = city;
			AddInOutEventIfRequired(eventType, timeOutPropertyInfo, reference, facility, suburb, reason);
		}

		void AddInOutPicDlvEvents(JobDocAddress docAddress, ZString city, Event inOrOutEventType, ZPropertyInfo inOrOutInfo, Event picOrDlvEventType, ZPropertyInfo inInfo, ZPropertyInfo outInfo, ZString reason)
		{
			var reference = EventReference;
			var facility = DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(docAddress);
			var suburb = city;
			AddInOutEventIfRequired(inOrOutEventType, inOrOutInfo, reference, facility, suburb, reason);
			AddPickupOrDeliveryEventIfRequired(picOrDlvEventType, inInfo, outInfo, reference, facility, suburb);
		}

		void AddInOutPicDlvEventsForWaitPoint(JobDocAddress docAddress, ZString city, Event inOrOutEventType, ZPropertyInfo inOrOutInfo)
		{
			var reference = EventReference;
			var facility = DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(docAddress);
			var suburb = city;
			var reason = Cartage != null && Cartage.IsExport ? Constants.EventReferenceParameterReasons.Pack : Constants.EventReferenceParameterReasons.Unpack;
			AddInOutEventIfRequired(inOrOutEventType, inOrOutInfo, reference, facility, suburb, reason);
			AddPickupOrDeliveryEventIfRequired(Events.PickedUp, JU_WaitPointTimeInInfo, JU_WaitPointTimeOutInfo, reference, facility, suburb);
			AddPickupOrDeliveryEventIfRequired(Events.Delivered, JU_WaitPointTimeInInfo, JU_WaitPointTimeOutInfo, reference, facility, suburb);
		}

		void AddInOutEventIfRequired(Event eventToLog, ZPropertyInfo dateTimeInfo, ZString reference, ZString facility, ZString suburb, ZString reason)
		{
			var oldDate = IsInDatabase ? (ZDateTime)dateTimeInfo.OriginalValue : ZDateTime.Empty;
			var newDate = (ZDateTime)dateTimeInfo.Value;
			if (oldDate != newDate)
			{
				Logs.CreateRecreateOrUpdateEventLog(eventToLog, EstimateActual.Actual, newDate.ToOffset(),
					reference,
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, facility),
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, suburb),
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, reason));
			}
		}

		void AddPickupOrDeliveryEventIfRequired(Event eventToLog, ZPropertyInfo inDateTimeInfo, ZPropertyInfo outDateTimeInfo, ZString reference, ZString facility, ZString suburb)
		{
			if (!IsFutile && inDateTimeInfo.Value.IsValid && outDateTimeInfo.Value.IsValid)
			{
				var oldDate = IsInDatabase ? (ZDateTime)outDateTimeInfo.OriginalValue : ZDateTime.Empty;
				var newDate = (ZDateTime)outDateTimeInfo.Value;

				var keyValuePairs = new List<KeyValuePair<string, string>>();
				keyValuePairs.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, facility));
				keyValuePairs.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, suburb));

				ZString containerType = GetContainerType(this, eventToLog, facility);
				if (!containerType.IsEmpty)
				{
					keyValuePairs.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, containerType));
				}

				if (oldDate != newDate)
				{
					var stmALogPickedupOrDelivered = Logs.CreateRecreateOrUpdateEventLog(eventToLog, EstimateActual.Actual, newDate.ToOffset(),
						reference,
						keyValuePairs.ToArray());

					PublishEventsOnSaved.Add(stmALogPickedupOrDelivered);
				}
			}
		}

		string GetContainerType(CommonCartageLeg leg, Event eventToLog, ZString facility)
		{
			return leg.IsContainerised
						? IsContainerEmpty(eventToLog, facility)
							? Constants.EventReferenceParameterTypes.EmptyContainer
							: Constants.EventReferenceParameterTypes.FullContainer
						: "";
		}

		bool IsContainerEmpty(Event eventToLog, ZString facility)
		{
			var result = JU_IsEmptyContainer;

			if (HasWaitPoint)
			{
				//	Direction
				//	Export/Origin:		CYD[E] – [E]CFS[F] – [F]CTO
				//	Import/Destination:		CTO[F] – [F]CFS[E] – [E]CYD
				if (facility == EventConstants.Facilities.Code.Terminal)     // CTO
				{
					result = false;         // has wait point, at CTO always full Container
				}
				else if (facility == EventConstants.Facilities.Code.ContainerYard)
				{
					result = true;          // has wait point, at CYD always empty
				}
				else
				{
					if (Cartage.IsExportOrOrigin)
					{
						result = (eventToLog == Events.Delivered);      // export or origin, delivery to CFS empty container
					}
					else if (Cartage.IsImportOrDestination)
					{
						result = (eventToLog == Events.PickedUp);       // import or destination, picked up at CFS empty container
					}
				}
			}

			return result;
		}

		ZString EventReference
		{
			get
			{
				ZString result;

				if (IsContainerised)
				{
					result = Container.JC_ContainerNum;
					if (result.IsEmpty)
					{
						var containerType = Container.Container;
						var containerTypeCode = containerType != null ? containerType.RC_Code.ToString() : Core.Constants.PkgUnit.Container;
						result = Res.GetString("51132250-30c7-4b76-89fc-8b60a81bb4e2", "{0}x{1}", Container.JC_ContainerCount, containerTypeCode);
					}
				}
				else
				{
					result = LooseCargoGoodsDescription;
				}

				return result;
			}
		}

		public CommonCartageLegBehaviorStrategy BehaviorStrategy
		{
			get { return CommonCartageBehaviorStrategyProvider.GetCartageProvider(Factory).CartageLegBehaviorStrategy; }
		}

		[ChildEditable(true)]
		public GPSSupporterActivityCollection GPSActivities
		{
			get
			{
				if (fGPSActivities == null)
				{
					fGPSActivities = new GPSSupporterActivityCollection((Integration.ICommonCartageLeg)this);
					fGPSActivities.Load();
					RegisterEditableChildObject(fGPSActivities);
				}

				return fGPSActivities;
			}
		}
		GPSSupporterActivityCollection fGPSActivities;

		public new CommonCartageLegLookups Lookups
		{
			get { return lookups ?? (lookups = (CommonCartageLegLookups)GetNewLookups()); }
		}
		CommonCartageLegLookups lookups;

		protected override JobContainerLegsLookups GetNewLookups()
		{
			return new CommonCartageLegLookups(this);
		}

		public new CommonCartageLegValidation Validation
		{
			get { return (CommonCartageLegValidation)GetNewValidation(); }
		}

		protected override JobContainerLegsValidation GetNewValidation()
		{
			return new CommonCartageLegValidation(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CommonCartageLegFetchStrategy(this);
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return BehaviorStrategy.DocumentSupporter(this); }
		}

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			GPSActivities.RemoveAndDeleteAll();
			base.Delete();
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new CommonCartageLegDocManagerInfo(this, Core.Constants.DocManagerCodes.LocalTransportLeg);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			if (!IsDeleted)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, Client != null ? Client.PK : ZGuid.Empty, ZGuid.Empty);
			}
			return result;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CartageLegProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		CartageLegProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.CartageLegWorkflowDescriptorCode; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return (Cartage as IWorkflowProvider).GetWorkflowInformationProvider();
		}

		OrgHeader[] IWorkflowProviderEvent.RecipientOrganisations
		{
			get { return new OrgHeader[] { GlbBranch.CurrentBranch.OrgProxy }; }
		}

		void CheckIsDelivered()
		{
			bool isWaitPointConsignee = WaitPointDocAddressType == DocAddressType.LocalCartageImporter;
			bool isDeliveryConsignee = DeliverToDocAddressType == DocAddressType.LocalCartageImporter;

			if (!JU_EstimatedDeliveryTime.IsEmpty && (isWaitPointConsignee || isDeliveryConsignee))
			{
				Logs.CreateRecreateOrUpdateEventLog(Events.DeliveryCartageCompleteFinalised, EstimateActual.Estimate, JU_EstimatedDeliveryTime.ToOffset());
			}

			if (!JU_DeliverySignedFor.IsEmpty)
			{
				bool isWaitPointConsigneeAndDelivered = isWaitPointConsignee && !JU_WaitPointTimeOut.IsEmpty;
				bool isDeliveryConsigneeAndDelivered = isDeliveryConsignee && !JU_DeliverTimeOut.IsEmpty;

				if (isWaitPointConsigneeAndDelivered)
				{
					Logs.CreateRecreateOrUpdateEventLog(Events.DeliveryCartageCompleteFinalised, EstimateActual.Actual, JU_WaitPointTimeOut.ToOffset());
				}
				else if (isDeliveryConsigneeAndDelivered)
				{
					Logs.CreateRecreateOrUpdateEventLog(Events.DeliveryCartageCompleteFinalised, EstimateActual.Actual, JU_DeliverTimeOut.ToOffset());
				}
			}
		}

		void AddSignatureCapturedEvent()
		{
			if (!JU_DeliverySignedFor.IsEmpty && Signature.HasValidSignature)
			{
				var eventParams = new List<KeyValuePair<string, string>>();
				eventParams.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Name, JU_DeliverySignedFor));
				eventParams.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, Constants.EventReferenceParameterReasons.Delivery));

				var deliverToAddress = DeliverToDocAddress;
				if (deliverToAddress != null)
				{
					var location = deliverToAddress.City;
					if (!location.IsEmpty)
					{
						eventParams.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Location, location));
					}

					var facility = DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(deliverToAddress);
					if (!facility.IsEmpty)
					{
						eventParams.Add(new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Facility, facility));
					}
				}

				Logs.CreateRecreateOrUpdateEventLog(Events.SignatureCaptured, EstimateActual.Actual, ZDateTimeOffset.Now, "", eventParams.ToArray());
			}
		}

		void SetGroupedLegsTime(SchemaDateTimeColumn dateTimeColumn, ZDateTime time)
		{
			if (!SettingGroupedLegsTime && WorkSheet != null)
			{
				foreach (CommonCartageLeg leg in WorkSheet.CartageLegs)
				{
					if (leg.PK != PK && leg.JU_RunSheetSequence == JU_RunSheetSequence && GroupedLegHasSameAddress(leg, dateTimeColumn))
					{
						try
						{
							leg.SettingGroupedLegsTime = true;
							leg[dateTimeColumn] = time;
						}
						finally
						{
							leg.SettingGroupedLegsTime = false;
						}
					}
				}
			}
		}

		bool GroupedLegHasSameAddress(CommonCartageLeg leg, SchemaDateTimeColumn dateTimeColumn)
		{
			bool result = false;

			if (dateTimeColumn.Name == JobContainerLegsSchema.JU_PickupTimeIn.Name || dateTimeColumn.Name == JobContainerLegsSchema.JU_PickupTimeOut.Name)
			{
				result = DoAddressesMatch(leg.PickupFromDocAddress, PickupFromDocAddress);
			}
			else if (dateTimeColumn.Name == JobContainerLegsSchema.JU_WaitPointTimeIn.Name || dateTimeColumn.Name == JobContainerLegsSchema.JU_WaitPointTimeOut.Name)
			{
				result = DoAddressesMatch(leg.WaitPointDocAddress, WaitPointDocAddress);
			}
			else if (dateTimeColumn.Name == JobContainerLegsSchema.JU_DeliverTimeIn.Name || dateTimeColumn.Name == JobContainerLegsSchema.JU_DeliverTimeOut.Name)
			{
				result = DoAddressesMatch(leg.DeliverToDocAddress, DeliverToDocAddress);
			}

			return result;
		}

		bool DoAddressesMatch(JobDocAddress first, JobDocAddress second)
		{
			return first != null && second != null ? first.IsTheSameAddressAs(second) : first == null && second == null;
		}

		internal bool SettingGroupedLegsTime;

		void SetDemurrage(DemurrageTime demurrageTime)
		{
			if (TransportRegistry.Instance.AutoPopulateDemurrage.Value)
			{
				switch (demurrageTime)
				{
					case DemurrageTime.Pickup:
						if (JU_PickupTimeIn.IsValid && JU_PickupTimeOut.IsValid)
						{
							JU_CartagePickupDemurrage = GetDemurrage(JU_PickupTimeIn, JU_PickupTimeOut);
						}
						break;
					case DemurrageTime.WaitPoint:
						if (JU_WaitPointTimeIn.IsValid && JU_WaitPointTimeOut.IsValid)
						{
							JU_CartageWaitPointDemurrage = GetDemurrage(JU_WaitPointTimeIn, JU_WaitPointTimeOut);
						}
						break;
					case DemurrageTime.Delivery:
						if (JU_DeliverTimeIn.IsValid && JU_DeliverTimeOut.IsValid)
						{
							JU_CartageDeliveryDemurrage = GetDemurrage(JU_DeliverTimeIn, JU_DeliverTimeOut);
						}
						break;
					default:
						break;
				}
			}
		}

		ZDateTime GetDemurrage(ZDateTime timeIn, ZDateTime timeOut) => timeOut - timeIn;

		enum DemurrageTime
		{
			Pickup,
			WaitPoint,
			Delivery
		}

		public bool WasDispatchedViaAttachmentToRunSheet
		{
			get { return Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ContainerLegDispatched.Code)).Length > 0; }
		}

		public bool WasDispatchedViaRunSheetDocument
		{
			get { return Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ContainerLegRunSheet.Code)).Length > 0; }
		}

		public bool IsStatusInError
		{
			get
			{
				return JU_MessageStatus == Constants.CartageLegDispatchStatusList.Codes.Futile
				|| JU_MessageStatus == Constants.CartageLegDispatchStatusList.Codes.Rejected;
			}
		}

		void SetMessageStatus()
		{
			if (JU_AdditionalService == Constants.CartageAdditional.Futile)
			{
				JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Futile;
			}
			else if (!JU_DeliverTimeOut.IsEmpty)
			{
				JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Delivered;
			}
			else if (!JU_PickupTimeOut.IsEmpty)
			{
				JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.PickedUp;
			}
			else if (Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ContainerLegRunSheet.Code)).Length > 0)
			{
				JU_MessageStatus = Constants.CartageLegDispatchStatusList.Codes.Runsheet;
			}
			else
			{
				JU_MessageStatus = JU_PickupTimeIn.IsEmpty ? Constants.CartageLegDispatchStatusList.Codes.NotStarted : "";
			}
		}

		void SetIsEmptyContainer()
		{
			JU_IsEmptyContainer = HasContainerYard && JU_E2WaitPointAddressID.IsEmpty;
		}

		ZString Integration.ICommonCartageLeg.MessageStatus
		{
			get { return JU_MessageStatus; }
		}

		SecurityCheckpoint IDistanceCalculationConsumer.Checkpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLocalTransport; }
		}

		ZDecimal IDistanceCalculationConsumer.Distance
		{
			get { return JU_Distance; }
			set { JU_Distance = value; }
		}

		ZString IDistanceCalculationConsumer.DistanceUnit
		{
			get { return JU_DistanceUnit; }
			set { JU_DistanceUnit = value; }
		}

		DistanceCalculationConfiguration IDistanceCalculationConsumer.DistanceCalculationConfig
		{
			get { return DistanceCalculationHelper.GetConfigurationFromJobDocAddress(null, Cartage.LocalClient); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.OriginAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(PickupFromDocAddress); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.DestinationAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(DeliverToDocAddress); }
		}

		public void SetCalculatedDistance(INotifications notifications)
		{
			new FreightDistanceCalculator(this, notifications).SetCalculatedDistance();
		}

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		string IJobNumber.JobNumber
		{
			get { return UniqueIDWithJobNumber; }
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("F25A10D5-DC80-42C8-9EC8-DADB0BBC922C", "Consignee, Consignor or Local Client for Billing"); }
		}

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { documentLogin += value; }
			remove { documentLogin -= value; }
		}

		event EventHandler<SecurityLoginEventArgs> documentLogin;

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (documentLogin != null)
			{
				documentLogin(this, e);
			}
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				return GetOrganisationsForCreditChecks();
			}
		}

		OrgHeader[] GetOrganisationsForCreditChecks()
		{
			var orgsToCheck = new List<OrgHeader>();
			var docAddresses = new[] { PickupFromDocAddress, WaitPointDocAddress, DeliverToDocAddress };
			if (Cartage.IsCreditLimitCheckRequired(OrgCodes.LocalClient))
			{
				orgsToCheck.Add(Cartage.LocalClient);
			}

			if (Cartage.IsCreditLimitCheckRequired(OrgCodes.Consignor))
			{
				orgsToCheck.AddRange(docAddresses.Where(d => d != null && d.DocAddressType == DocAddressType.LocalCartageExporter).Select(d => d.Organisation));
			}

			if (Cartage.IsCreditLimitCheckRequired(OrgCodes.Consignee))
			{
				orgsToCheck.AddRange(docAddresses.Where(d => d != null && d.DocAddressType == DocAddressType.LocalCartageImporter).Select(d => d.Organisation));
			}

			return orgsToCheck.ToArray();
		}

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get { return false; }
		}
		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return Array.Empty<ScreeningParty>();
		}

		string[] IRelatedJobNumber.JobNumber
		{
			get { return new string[] { Cartage.JJ_ConsignmentID }; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<WorkSheetLegLinkEventArgs> OnWorkSheetAdded;
		public void RaiseWorkSheetAdded(WorkSheetLegLinkEventArgs e)
		{
			if (OnWorkSheetAdded != null)
			{
				OnWorkSheetAdded(this, e);
			}
		}

		static bool AreBothAlmostMidnightOnSameDate(ZDateTime dateTime1, ZDateTime dateTime2)
		{
			return dateTime1.Date == dateTime2.Date &&
				dateTime1.Hour == dateTime2.Hour &&
				dateTime1.Hour == 23 &&
				dateTime1.Minute == dateTime2.Minute &&
				dateTime1.Minute == 59;
		}
	}
}
