using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Agency.Business.Shipment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business
{
	[UniversalDataContext(DataContextType.AgencyBooking)]
	[VisualizableDocumentsSupportable("BookingVisualizableDocumentSupporter")]
	public class AgencyBooking : AgencyShipment,
		Integration.Agency.IAgencyBooking,
		ICustomFieldProvider,
		IUniversalCopyValidationStrategy
	{
		public AgencyBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (IsBillOfLadingStage || JS_IsCancelled)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public override void OnJobCreated(JobHeader job)
		{
			base.OnJobCreated(job);
			_ = ShipmentJobHeader;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && (IsBillOfLadingStage || JS_IsCancelled))
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
		}

		#endregion

		#region JS_ShipmentStatus

		public override ZString JS_ShipmentStatus
		{
			get => base.JS_ShipmentStatus;
			set
			{
				if (JS_ShipmentStatus != value)
				{
					switch (value)
					{
						case ShipmentStatusList.Codes.ElectronicBooking:
							LogStatusChangedEvent(value, Core.Constants.EventReferenceParameterReasons.ElectronicBookingReceived);
							break;
						case ShipmentStatusList.Codes.Booked:
							LogStatusChangedEvent(value, (NoResString)"Booking Confirmed"); // Event Reason Prefix
							break;
						case ShipmentStatusList.Codes.BookingRejected:
							{
								if (!ShouldLogStatusChangesOnSaving() && !isSuspendAutomaticCreationOfStatusChangedLog)
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

									LogStatusChangedEvent(value, (NoResString)"Booking Rejected" + (reason.IsEmpty ? string.Empty : (NoResString)", " + reason));  // Event Reason Prefix
								}
								break;
							}
						case ShipmentStatusList.Codes.EBookingCancellationRequest:
						case ShipmentStatusList.Codes.WebBooking:
						case ShipmentStatusList.Codes.BookingCancelled:
						case ShipmentStatusList.Codes.WaitListed:
							LogStatusChangedEvent(value);
							break;
					}

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
					return JS_ShipmentStatus == ShipmentStatusList.Codes.EBookingCancellationRequest || JS_ShipmentStatus == ShipmentStatusList.Codes.BookingCancelled
						|| (IsReceivedElectronicBooking() && (JS_ShipmentStatus == ShipmentStatusList.Codes.Booked || JS_ShipmentStatus == ShipmentStatusList.Codes.BookingRejected));
				}

				return false;
			}
		}

		public void LogStatusChangedEvent(ZString newValue, ZString? reason = null)
		{
			if (!ShouldLogStatusChangesOnSaving() && !isSuspendAutomaticCreationOfStatusChangedLog)
			{
				var parameters = new KeyValuePair<string, string>[]
				{
						new KeyValuePair<string, string>(Params.New, newValue),
						new KeyValuePair<string, string>(Params.Old, IsInDatabase ? JS_ShipmentStatusInfo.OriginalValue.ToString() : null),
						new KeyValuePair<string, string>(Params.Reason, reason ?? null),
						new KeyValuePair<string, string>(Params.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus)
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

		#region Related Business Objects

		public new AgencyBookingPackLineCollection OuterPackLines
		{
			get { return (AgencyBookingPackLineCollection)base.OuterPackLines; }
		}
		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new AgencyBookingPackLineCollection(this);
		}

		public new AgencyBookingContainerDependentCollection BookedContainers
		{
			get { return (AgencyBookingContainerDependentCollection)base.BookedContainers; }
		}
		public new AgencyBookingContainerDependentCollection RealContainers
		{
			get { return (AgencyBookingContainerDependentCollection)base.RealContainers; }
		}
		protected override AgencyShipmentContainerDependentCollection NewContainersCollection(ZString purpose)
		{
			return new AgencyBookingContainerDependentCollection(this, purpose);
		}

		#endregion

		#region IJobInvoicingPlugin

		protected override CommonShipmentInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new AgencyBookingInvoicingSupporter(this);
		}

		#endregion

		#region Invoicing/Rating

		protected override IAutoRating GetRatingAdapterCore()
		{
			return new AgencyBookingRatingAdapter(this);
		}

		#endregion

		#region DocumentSupporter

		public override DocumentSupporter DocumentSupporter
		{
			get { return new AgencyShipmentDocumentSupporter(this); }
		}

		#endregion

		#region Workflow

		protected override ZString GetWorkflowType()
		{
			return WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
		}

		protected override AgencyShipmentProcessTaskCollection NewWorkflowItemsCollection()
		{
			return new AgencyBookingProcessTaskCollection(this);
		}

		#endregion

		#region Implementation

		protected override ZString DefaultWeightUnit
		{
			get { return AgencyRegistry.Instance.DefaultBookingWeightUnit.Value; }
		}

		protected override ZString DefaultVolumeUnit
		{
			get { return AgencyRegistry.Instance.DefaultBookingVolumeUnit.Value; }
		}

		protected override void ConfirmCore()
		{
			base.ConfirmCore();
			SetReadOnlyIncludingChildren(true);
		}

		protected override void GenerateNumbers(NumberGenerator generator)
		{
			OceanBookingNumberGeneratorTarget bookingRefTarget = null;

			if (JS_CFSReference.IsEmpty && NeedsHouseBill)
			{
				bookingRefTarget = new OceanBookingNumberGeneratorTarget();
				generator.AdditionalTargets.Add(bookingRefTarget);
			}

			base.GenerateNumbers(generator);

			if (bookingRefTarget != null)
			{
				JS_CFSReference = bookingRefTarget.Value;
			}
		}

		protected override ShipmentDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new AgencyBookingDocAddressValidation(addressToValidate);
		}

		protected override string GetEmailSubject()
		{
			return Res.GetString("ea78578a-aa7f-4886-97e8-bb491805fa6d", "Agency Booking - {0}", JS_UniqueConsignRef);
		}

		protected override Type GetDocWrapperType()
		{
			return ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocAgencyShipment>();
		}

		protected override string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.AgencyBooking;
		}

		protected override ControllerID GetDtbBookingParentControllerID()
		{
			return ControllerIDs.AgencyBooking;
		}

		protected override void OnTransportsIncludingRelatedCreated()
		{
			base.OnTransportsIncludingRelatedCreated();

			TransportsIncludingRelatedOriginalCount = TransportsIncludingRelated.Count;
		}

		#endregion

		#region Catch Wrong Workflow Type

		sealed class CurrentBookingService
		{
			public Guid BookingPK { get; set; }
			public string BookingStatus { get; set; }
			public string BookingRegistrationStackTrace { get; set; }

			public Dictionary<Guid, ContainerInfo> ContainerInfos { get; set; }
		}

		sealed class ContainerInfo
		{
			public List<string> ContainerTypes { get; set; }
			public List<string> LoadedAsOtherTypeStackTraces { get; set; }
		}

		public static void SetCurrentBookingInfo(BusinessObjectFactory factory, ZGuid pk, ZString status)
		{
			if (factory == null
				|| !pk.IsValid)
			{
				return;
			}

			var service = factory.GetCachedValue<CurrentBookingService>();
			service.BookingPK = pk.ToGuid();
			service.BookingStatus = status;
			service.BookingRegistrationStackTrace = $"SetCurrentBookingInfo Factory={factory._Instance},{factory.NameForDebugging},{factory.RefreshEnabled}" + System.Environment.NewLine + new StackTrace().ToString();
		}

		public static void NotifyContainerLoadedWithOtherType(BusinessObjectFactory factory, BusinessObject container)
		{
			if (factory == null
				|| container == null
				|| !(factory.GetCachedValue<CurrentBookingService>() is CurrentBookingService service))
			{
				return;
			}

			var containerPK = container.PK.ToGuid();

			var containerInfos = service.ContainerInfos ?? new Dictionary<Guid, ContainerInfo>();

			if (containerInfos.TryGetValue(containerPK, out var existingContainerInfo))
			{
				existingContainerInfo.ContainerTypes.Add(container.GetType().Name);
				existingContainerInfo.LoadedAsOtherTypeStackTraces.Add(new StackTrace().ToString());

				return;
			}

			var containersInFactoryCache = factory.GetBizOsForPK(containerPK);

			if (containersInFactoryCache.Length < 2)
			{
				return;
			}

			var containerTypes = containersInFactoryCache
				.OrderBy(containerInFactory => containerInFactory != container ? 0 : 1)
				.Select(containerInFactory => containerInFactory.GetType().Name);

			var containerStackTraces = new List<string>
			{
				(NoResString)"[not recorded for first container]",
				new StackTrace().ToString()
			};

			var containerInfo = new ContainerInfo
			{
				ContainerTypes = new List<string>(containerTypes),
				LoadedAsOtherTypeStackTraces = containerStackTraces
			};

			containerInfos[container.PK.ToGuid()] = containerInfo;
			service.ContainerInfos = containerInfos;
		}

		public static void CheckBillOfLadingIsNotUsedForCurrentBooking(BusinessObjectFactory factory, ZGuid billOfLadingPK, ZString? billOfLadingNumber = null, ZString? billOfLadingStatus = null)
		{
			var service = factory.GetCachedValue<CurrentBookingService>();

			if (service == null
				|| !billOfLadingPK.IsValid
				|| service.BookingPK != billOfLadingPK)
			{
				return;
			}

			var booking = factory.GetBizOsForPK(service.BookingPK)
					.Select(x => x as AgencyBooking)
					.FirstOrDefault();
			if (booking == null)
			{
				return;
			}

			var dataRefreshStatus = $@"Booking IsRefreshingByDataRefreshBus = {booking.IsRefreshingByDataRefreshBus}
Numbers IsRefreshingByDataRefreshBus = {booking.Numbers.IsRefreshingByDataRefreshBus}
BookedContainers IsRefreshingByDataRefreshBus = {booking.BookedContainers.IsRefreshingByDataRefreshBus}
RealContainers IsRefreshingByDataRefreshBus = {booking.RealContainers.IsRefreshingByDataRefreshBus}";

			if (booking.IsFactoryPerformingDataRefresh)
			{
				return;
			}

			var message = $@"BillOfLading was used from the Booking form.
Factory={factory._Instance},{factory.NameForDebugging},{factory.RefreshEnabled}
Number={billOfLadingNumber}
PK={service.BookingPK}
Booking JS_ShipmentStatus={service.BookingStatus}
BillOfLading JS_ShipmentStatus={billOfLadingStatus}
Booking Load Stack Trace={service.BookingRegistrationStackTrace}
Container Infos={CreateContainerInfoMessage()}
Booking CusEntryNumbers={CreateCusEntryNumbersInfoMessage()}
{dataRefreshStatus}";

			ErrorReporter.ReportOnce("BillOfLadingFromBookingContext", message);

			#region helpers

			string CreateContainerInfoMessage()
			{
				if (service.ContainerInfos == null)
				{
					return null;
				}

				var containerInfosForMessage = new List<string>();

				foreach (var containerPKAndInfo in service.ContainerInfos)
				{
					var containerInfo = containerPKAndInfo.Value;

					var containerTypes = string.Join(", ", containerInfo.ContainerTypes);
					var containerStackTraces = string.Join($"\r\n---\r\n", containerInfo.LoadedAsOtherTypeStackTraces);

					var containerInfoForMessage = $@"Container PK={containerPKAndInfo.Key}
Container Types={containerTypes}
Container Stack Traces={containerStackTraces}";

					containerInfosForMessage.Add(containerInfoForMessage);
				}

				return string.Join("\r\n", containerInfosForMessage);
			}

			string CreateCusEntryNumbersInfoMessage()
			{
				var numberInfoMessage = string.Empty;
				var cusEntryNumbers = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, service.BookingPK));

				foreach (var number in cusEntryNumbers)
				{
					numberInfoMessage += $"\nPK={number.PK}, CE_EntryNum={number.CE_EntryNum}, CE_Category={number.CE_Category}, CE_EntryType={number.CE_EntryType}, CE_ParentID={number.CE_ParentID}, CE_ParentTable={number.CE_ParentTable}, CE_RN_NKCountryCode={number.CE_RN_NKCountryCode}";
				}

				return numberInfoMessage;
			}

			#endregion
		}

		#endregion

		#region GetStrategies

		IBusinessObjectStrategy[] strategies;
		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			if (strategies == null)
			{
				var baseStrategies = base.GetStrategies();
				var strategyList = new List<IBusinessObjectStrategy>(baseStrategies);
				strategyList.Add(new AgencyBookingLoggingStrategy());
				strategies = strategyList.ToArray();
			}

			return strategies;
		}

		#endregion

		#region IUniversalCopyValidationStrategy Member

		public string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree)
		{
			if (this.IsBillOfLadingStage)
			{
				return Res.GetString("64D67EC5-0BEF-4B31-9D81-5F824EC9EF5A", "This Booking has now been confirmed to Bill of Lading and is no longer accessible from the Booking module. Please access from Bill of Lading module.");
			}

			return null;
		}

		#endregion

		#region TransportsIncludingRelatedOriginalCount

		public ZInt? TransportsIncludingRelatedOriginalCount { get; set; }

		#endregion

		bool IsFactoryPerformingDataRefresh
		{
			get
			{
				return IsRefreshingByDataRefreshBus || Numbers.IsRefreshingByDataRefreshBus;
			}
		}

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}
	}
}
