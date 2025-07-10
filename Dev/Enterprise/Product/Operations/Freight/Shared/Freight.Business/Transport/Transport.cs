using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Shared;
using WTG.MachineLearning.NLP;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = nameof(OnUniversalCopyFinish))]
	[UniversalCopyIgnoreElement(nameof(JW_IsLinked), nameof(IsDomestic))]
	[DebuggerDisplay("Transport ({JW_RL_NKLoadPort}->{JW_RL_NKDiscPort})")]
	[CodeProperty(Transport.Schema.JW_Vessel)]
	public class Transport : AutoJobConsolTransport,
		ITransport,
		ISailingManaged,
		IFlightDetailsSuppression,
		IMovementLeg,
		IWorkflowTriggerFieldChangeSource,
		IProcessHandlingInfoProvider,
		IShouldUpdateScreeningStatus,
		IEventDatePropertyChecker,
		IWorkflowTriggerEventSource,
		ISupplyChainSecurityImportExportSupporter,
		IScreeningPartyProvider,
		IFlightInformationProvider,
		IVesselMovementsUrlSupporter,
		ICO2eLegProvider,
		IScreeningPartyForVessel,
		IUniversalEventAddedHandler,
		IUpdateEventDateSupporter
	{
		#region Schema

		public new class Schema : AutoJobConsolTransport.Schema
		{
			public const string DescriptionForMostInterestingTransport = "DescriptionForMostInterestingTransport";
			public const string JW_VesselFieldType = "JW_VesselFieldType";
			public const string JW_ParentDescription = "JW_ParentDescription";
			public const string JW_ParentContainerMode = "JW_ParentContainerMode";
			public const string JW_ParentBillOfLading = "JW_ParentBillOfLading";
			public const string JW_ParentConsignmentRef = "JW_ParentConsignmentRef";
			public const string JW_TransportType_List = "JW_TransportType_List";
			public const string JW_TransportMode_List = "JW_TransportMode_List";

			public const string JW_JX_JV_RegistrationNo = "JW_JX_JV_RegistrationNo";
			public const string JW_JX_JV_VoyageType = "JW_JX_JV_VoyageType";
			public const string JW_JX_DepartOrArriveReference = "JW_JX_DepartOrArriveReference";
			public const string JW_JX_DepartOrArriveBerth = "JW_JX_DepartOrArriveBerth";
			public const string JW_JX_IsPublished = "JW_JX_IsPublished";
			public const string JW_JX_Load_ATA = "JW_JX_Load_ATA";
			public const string JW_JX_Load_ETA = "JW_JX_Load_ETA";

			public const string JW_Calc_Status = "JW_Calc_Status";

			public const string IsDomestic = "IsDomestic";
			public const string VoyageFlightWithSuppression = "VoyageFlightWithSuppression";
			public const string BillOfLadingWithSuppression = "BillOfLadingWithSuppression";
			public const string ETDWithSuppression = "ETDWithSuppression";
			public const string ETAWithSuppression = "ETAWithSuppression";
			public const string ATDWithSuppression = "ATDWithSuppression";
			public const string ATAWithSuppression = "ATAWithSuppression";
			public const string CarrierWithSuppression = "CarrierWithSuppression";
			public const string CarrierPK = "CarrierPK";
			public const string CreditorPK = "CreditorPK";

			public const string JW_ATAForBinding = "JW_ATAForBinding";
			public const string JW_ATDForBinding = "JW_ATDForBinding";
			public const string JW_STAForBinding = "JW_STAForBinding";
			public const string JW_STDForBinding = "JW_STDForBinding";
			public const string JW_ETAForBinding = "JW_ETAForBinding";
			public const string JW_ETDForBinding = "JW_ETDForBinding";
			public const string JW_VoyageFlightForBinding = "JW_VoyageFlightForBinding";
			public const string JW_VesselForBinding = "JW_VesselForBinding";
			public const string JW_RL_NKDiscPortForBinding = "JW_RL_NKDiscPortForBinding";
			public const string JW_RL_NKLoadPortForBinding = "JW_RL_NKLoadPortForBinding";
			public const string JW_IsCharterForBinding = "JW_IsCharterForBinding";
			public const string JW_AircraftTypeForBinding = "JW_AircraftTypeForBinding";

			public const string JW_DepotReceivalCommencesForBinding = "JW_DepotReceivalCommencesForBinding";
			public const string JW_DocumentaryCutOffForBinding = "JW_DocumentaryCutOffForBinding";
			public const string JW_VGMCutOffForBinding = "JW_VGMCutOffForBinding";
			public const string JW_TerminalCutOffForBinding = "JW_TerminalCutOffForBinding";
			public const string JW_TerminalReceivalCommencesForBinding = "JW_TerminalReceivalCommencesForBinding";
			public const string JW_TerminalAvailabilityDateForBinding = "JW_TerminalAvailabilityDateForBinding";
			public const string JW_TerminalStorageDateForBinding = "JW_TerminalStorageDateForBinding";
			public const string JW_DepotCutOffForBinding = "JW_DepotCutOffForBinding";
			public const string JW_DepotAvailabilityDateForBinding = "JW_DepotAvailabilityDateForBinding";
			public const string JW_DepotStorageDateForBinding = "JW_DepotStorageDateForBinding";
			public const string JW_EmptyReceivalCommencesForBinding = "JW_EmptyReceivalCommencesForBinding";
			public const string JW_EmptyCutOffForBinding = "JW_EmptyCutOffForBinding";
			public const string JW_ReeferReceivalCommencesForBinding = "JW_ReeferReceivalCommencesForBinding";
			public const string JW_ReeferCutOffForBinding = "JW_ReeferCutOffForBinding";
			public const string JW_DGReceivalCommencesForBinding = "JW_DGReceivalCommencesForBinding";
			public const string JW_DGCutOffForBinding = "JW_DGCutOffForBinding";
			public const string JW_ServiceStringForBinding = "JW_ServiceStringForBinding";
			public const string JW_ArrivalPortRouteIdForBinding = "JW_ArrivalPortRouteIdForBinding";
			public const string JW_DeparturePortRouteIdForBinding = "JW_DeparturePortRouteIdForBinding";
		}

		#endregion

		public Transport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			constructorLog = System.Environment.StackTrace;
			ParentTypeDebugLog.AppendLine(constructorLog);
			transportInfoCollector = Factory.GetCachedValue("TransportInfoCollector", () => new TransportInfoCollector(this), CacheStalenessPolicy.NeverStale);
		}

		readonly string constructorLog;
		readonly TransportInfoCollector transportInfoCollector;
		bool IsLastAircraftUpdatedFromGSS;

		public ZStringBuilder ParentTypeDebugLog
		{
			get
			{
				if (parentTypeDebugLog == null)
				{
					parentTypeDebugLog = new ZStringBuilder();
				}

				return parentTypeDebugLog;
			}
		}
		ZStringBuilder parentTypeDebugLog;

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unknown;
			JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			JW_AdditionalTransportMode = ZString.Empty;
		}

#endif
		#endregion

		#region Loading

		void StoreOriginalValuesForProxiedFields(bool reset = false)
		{
			if (reset)
			{
				requireResetOfOriginalValuesForProxiedFields = true;
			}

			if (requireResetOfOriginalValuesForProxiedFields || !savedOriginalValuesForProxiedFields)
			{
				if (JW_IsLinked && SailingManager.IsInitialising)
				{
					return;
				}

				savedOriginalValuesForProxiedFields = true;
				requireResetOfOriginalValuesForProxiedFields = false;

				PreviousJW_STD = JW_STD;
				PreviousJW_STA = JW_ATA;
				PreviousJW_ETA = JW_ETA;
				PreviousJW_RL_NKLoadPort = JW_RL_NKLoadPort;
				PreviousJW_OA_DepartureLocation = JW_OA_DepartureLocation;

				PreviousJW_ETD = JW_ETD;
				PreviousJW_ATA = JW_ATA;
				PreviousJW_ATD = JW_ATD;
				PreviousJW_RL_NKDiscPort = JW_RL_NKDiscPort;
				PreviousJW_OA_ArrivalLocation = JW_OA_ArrivalLocation;

				PreviousJW_TerminalCutOff = JW_TerminalCutOff;
				PreviousJW_DepotCutOff = JW_DepotCutOff;
				PreviousJW_TerminalAvailabilityDate = JW_TerminalAvailabilityDate;
				PreviousJW_DepotAvailabilityDate = JW_DepotAvailabilityDate;
				PreviousJW_TerminalReceivalCommences = JW_TerminalReceivalCommences;
				PreviousJW_DepotReceivalCommences = JW_DepotReceivalCommences;
				PreviousJW_TerminalStorageDate = JW_TerminalStorageDate;
				PreviousJW_EmptyReceivalCommences = JW_EmptyReceivalCommences;
				PreviousJW_EmptyCutOff = JW_EmptyCutOff;
				PreviousJW_ReeferReceivalCommences = JW_ReeferReceivalCommences;
				PreviousJW_ReeferCutOff = JW_ReeferCutOff;
				PreviousJW_DGReceivalCommences = JW_DGReceivalCommences;
				PreviousJW_DGCutOff = JW_DGCutOff;

				PreviousJW_VoyageFlight = JW_VoyageFlight;
				PreviousJW_Vessel = JW_Vessel;
				PreviousJW_IsCharter = JW_IsCharter;
				PreviousJW_IsCargoOnly = JW_IsCargoOnly;
				PreviousJW_AircraftType = JW_AircraftType;
				PreviousJW_ServiceString = JW_ServiceString;
				PreviousJW_ArrivalPortRouteId = JW_ArrivalPortRouteId;
				PreviousJW_DeparturePortRouteId = JW_DeparturePortRouteId;
			}
		}

		bool savedOriginalValuesForProxiedFields;
		bool requireResetOfOriginalValuesForProxiedFields;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new string[]
			{
				Transport.Schema.JW_IsLinked,
				Transport.Schema.JW_JX
			});

			var factory = args.AlternativeFactoryToInstantiateCloneIn ?? Factory;
			Transport newTransport = factory.New<Transport>();
			using (newTransport.GetValidationSuspender())
			{
				newTransport.ParentType = ParentType;
				newTransport.CopyPersistentValuesFrom(this, args);
				newTransport.SetIsDomestic();
				if (JW_IsLinked)
				{
					newTransport.JW_IsLinked = true;
					newTransport.JW_JX = JW_JX;
				}
				else
				{
					newTransport.JW_IsLinked = false;
					this.CopyJobCO2eTo(newTransport);
					this.UpdateCO2eStatusToNotCurrent();
				}
			}
			return newTransport;
		}

		public override void CopyTransientProperties(BusinessObject copy)
		{
			((Transport)copy).parentType = parentType;
		}

		internal Transport TemplateCopy(BusinessObjectFactory alternativeFactory = null)
		{
			var columnNamesToExclude = new List<string> { Transport.Schema.JW_ParentGUID };
			var cloneArgs = alternativeFactory == null
				? new BusinessObjectCloneArgs(columnNamesToExclude)
				: new BusinessObjectCloneArgs(alternativeFactory, columnNamesToExclude, null, true);
			var clonedTransport = (Transport)Clone(cloneArgs);

			using (clonedTransport.GetValidationSuspender())
			{
				clonedTransport.JW_Vessel = ZString.Empty;
				clonedTransport.JW_VoyageFlight = ZString.Empty;

				var excludedPropertyNames = GetPropertyNamesExcludedForCloning();

				foreach (ZPropertyInfo propertyInfo in clonedTransport.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Empty;
					}

					if (excludedPropertyNames.Contains(propertyInfo.Name))
					{
						propertyInfo.Value = propertyInfo.DefaultValue;
					}
				}
			}

			return clonedTransport;
		}

		HashSet<ZString> GetPropertyNamesExcludedForCloning()
		{
			var excluded = new HashSet<ZString>();

			if (Parent is CommonConsol consol && consol.JK_RL_NKDischargePort.Left(2) == CountryCodes.Israel)
			{
				excluded.Add(JW_ArrivalPortRouteIdInfo.Name);
			}

			return excluded;
		}

		#endregion

		#region Universal Copy Handling

#if DEBUG
		internal
#endif
		protected void OnUniversalCopyFinish()
		{
			base.JW_IsLinked = !JW_JX.IsEmpty; // Bypass business logic, just set field value
		}

		#endregion

		#region Parent

		public ITransportParentCommon Parent
		{
			get
			{
				// DONT ATTEMPT TO GUESS THE PARENT TYPE HERE, YOU WILL GET IT WRONG!
				// If you get the transport from the parent then the correct parent should be set by the transport collection.
				// If you load the transport yourself then you should set the parent type as approperate for the context
				// in which you plan to use it.
				if ((ParentType == null && JW_ParentGUID.IsEmpty)
#if DEBUG
 || ForceParentNullIssueReporting
#endif
)
				{
					ReportParentNullIssue();

					throw new InvalidOperationException("Transport Parent Type and JW_ParentGUID Not Set Yet");
				}

				CheckParentTypeIsSet();

				try
				{
					if(ParentType == null)
					{
						var rowFactory = new RowFactory(Factory);
						var row = rowFactory.LoadFromPK(JobConsolTransportSchema.Constants.TableName, PK);
						var parentType = ParentTypeDecider.GetTypeForLoad(row, Factory);
						return (ITransportParentCommon)Factory.Load(parentType, JW_ParentGUID);
					}
					return (ITransportParentCommon)Factory.Load(ParentType, JW_ParentGUID);
				}
				catch (Exception e)
				{
					var message = FormattableString.Invariant($@"Error Loading Transport Parent for Transport: {this.GetType()}
ParentType: {ParentType.ToString()} / parentType: {parentType.ToString()}
Exception: {e.Message}
constructor Stacktrace:
{ParentTypeDebugLog.ToString()}");

					ErrorReporter.ReportOnce("TransportParentLoadError", message);

					throw;
				}
			}
		}

#if DEBUG

		[ThreadStatic]
		public static bool ForceParentNullIssueReporting;

#endif

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public Type ParentType
		{
			get { return GetParentTypeCore(); }
			set
			{
				if (value == null)
				{
					ParentTypeDebugLog.AppendLine((NoResString)"Throwing ArgumentNullException('value')");
					throw new ArgumentNullException(nameof(value));
				}

				if (!typeof(ITransportParentCommon).IsAssignableFrom(value))
				{
					ParentTypeDebugLog.AppendLine((NoResString)"Throwing ArgumentException()");
					throw new ArgumentException();
				}

				parentType = value;
				ParentTypeDebugLog.Clear();
			}
		}

		public ITransportParentCommon GetParentSafe()
		{
			return ParentType == null ? null : Parent;
		}

		protected virtual Type GetParentTypeCore()
		{
			return parentType;
		}

		Type parentType;

		protected virtual TypeDecider ParentTypeDecider => new TransportParentTypeDecider();

		void CheckParentTypeIsSet()
		{
			if (ParentType == null && JW_ParentType.IsEmpty)
			{
				string key = "UnableToCreateOrLoadTransportWithoutParentType";
				string errorMessage = (NoResString)@"Cannot create or load transport without setting parent type.
Transport constructor stacktrace:
{0}";

				ErrorReporter.ReportOnce(key, string.Format(errorMessage, ParentTypeDebugLog.ToString()));

				throw new InvalidOperationException("Transport Parent Type Not Set Yet");
			}
		}

		internal void ReportParentNullIssue()
		{
			string key = (NoResString)"WI00033391 International Logistics";
			string parentCollectionsTypes = "";

			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
			{
				parentCollectionsTypes = parentCollectionsTypes + collection.GetType() + ", ";
			}

			#region SuppressResourceStringsCheckRegion
			string message = "PK: " + PK
						+ "\r\nthis.GetType(): " + this.GetType()
						+ "\r\nJW_ParentGUID: " + JW_ParentGUID
						+ "\r\nJW_ParentType: " + JW_ParentType
						+ "\r\nJW_IsLinked: " + JW_IsLinked
						+ "\r\nJW_ETD: " + GetValueFromRowSafely(JobConsolTransportSchema.JW_ETD)
						+ "\r\nfJW_ETD: " + fJW_ETD
						+ "\r\nJW_ATD: " + GetValueFromRowSafely(JobConsolTransportSchema.JW_ATD)
						+ "\r\nfJW_ATD: " + fJW_ATD
						+ "\r\nJW_ETA: " + GetValueFromRowSafely(JobConsolTransportSchema.JW_ETA)
						+ "\r\nfJW_ETA: " + fJW_ETA
						+ "\r\nJW_ATA: " + GetValueFromRowSafely(JobConsolTransportSchema.JW_ATA)
						+ "\r\nfJW_ATA: " + fJW_ATA
						+ "\r\nPreviousJW_ETD: " + PreviousJW_ETD
						+ "\r\nPreviousJW_ATD: " + PreviousJW_ATD
						+ "\r\nPreviousJW_ETA: " + PreviousJW_ETA
						+ "\r\nPreviousJW_ATA: " + PreviousJW_ATA
						+ "\r\nHasChanges: " + HasChanges
						+ "\r\nIsInDatabase: " + IsInDatabase
						+ "\r\nParentCollections Types: " + parentCollectionsTypes
						+ "\r\nParentTypeDebugLog: " + ParentTypeDebugLog;
			#endregion

			ErrorReporter.ReportOnce(key, message);
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			if (parentType != null)
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifySave();
					SynchroniseProxiedFieldsIfNeeded();

					if (Sailing != null && (JW_JXInfo.HasChanges || JW_TerminalAvailabilityDateInfo.HasChanges || !IsInDatabase))
					{
						Sailing.CalculateJC_EmptyReturnedBy_ForContainers();
					}
				}
				if (!JW_JX.IsValid)
				{
					// something has gone horribly wrong and we should unlink the transport in an attempt to preserve the data entered.
					JW_IsLinked = false;
				}

				if (!JW_IsLinked && JW_JX.IsValid)
				{
					// something has gone horribly wrong and we should unlink the transport in an attempt to preserve the data entered and prevent triggering TG_JobConsolTransport.sql.
					JW_JX = ZGuid.Empty;
					ResetSailingManager();
				}

				if (hasClientULVATAProblemOccurred)
				{
					if (Math.Abs((JW_ATA - ZDateTime.Now).TotalMinutes) > 1)
					{
						hasClientULVATAProblemOccurred = false;
						Factory.ClearCachedValue<string>("ClientULVATAUpdateStackTrace");
					}
					else
					{
						ErrorReporter.ReportOnce("ClientULVATAUpdateStackTrace", "Contact the International Team (Work Item WI00104187).\r\n" + GetClientULVATAUpdateStackTrace());
					}
				}

				if (IsAir)
				{
					FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(this);
				}

				if (Parent is IContainerTrackingProvider trackingProvider && IsSea)
				{
					new ContainerTrackingSubscriptionRequestedManager(trackingProvider).UpdateIfNecessary();
				}

				if (JW_ATAInfo.HasChanges)
				{
					if (Parent is CommonConsol consol)
					{
						foreach (CommonContainer container in consol.Containers)
						{
							container.CalculateJC_EmptyReturnedBy();
						}
					}
				}

				if (JW_ETAInfo.HasChanges || JW_RL_NKDiscPortInfo.HasChanges || JW_ETDInfo.HasChanges || JW_RL_NKLoadPortInfo.HasChanges)
				{
					if (Parent is CommonConsol consol)
					{
						foreach (CommonShipment shipment in consol.Shipments)
						{
							if (shipment.Consols.Count == 1)
							{
								shipment.CalculateDeliveryDueDateIfNecessary();
							}
						}
					}
				}

				if (Parent is CommonConsol synchronizable && synchronizable.Syncroniser != null)
				{
					synchronizable.Syncroniser.Syncronise(synchronizable);
				}
			}
			else
			{
				// If we don't have a parent then there is nothing we can do to save ourselves.
				// (OnSaving should never be called if parentType is not set. If it does happen
				// then we will need to find out what flagged it as needing to be saved)
			}
			AddDateEvents();
			base.OnSaving();
		}

		void SynchroniseProxiedFieldsIfNeeded()
		{
			var row = ((INeedRow)this).Row;
			var propertiesChangedByDataRefresh = SailingManager.TransportPropertiesChangedByDataRefresh;

			new List<Tuple<BusinessObject, ZPropertyInfo>>()
			{
				Tuple.Create((BusinessObject)Voyage, JW_VesselInfo),
				Tuple.Create((BusinessObject)Voyage, JW_VoyageFlightInfo),
				Tuple.Create((BusinessObject)Voyage, JW_IsCharterInfo),
				Tuple.Create((BusinessObject)Voyage, JW_IsCargoOnlyInfo),
				Tuple.Create((BusinessObject)Voyage, JW_AircraftTypeInfo),

				Tuple.Create((BusinessObject)Sailing?.Origin, JW_RL_NKLoadPortInfo),
				Tuple.Create((BusinessObject)Sailing?.Origin, JW_OA_DepartureLocationInfo),
				Tuple.Create((BusinessObject)Sailing?.Origin, JW_STDInfo),
				Tuple.Create((BusinessObject)Sailing?.Origin, JW_ETDInfo),
				Tuple.Create((BusinessObject)Sailing?.Origin, JW_ATDInfo),

				Tuple.Create((BusinessObject)Sailing?.Destination, JW_RL_NKDiscPortInfo),
				Tuple.Create((BusinessObject)Sailing?.Destination, JW_OA_ArrivalLocationInfo),
				Tuple.Create((BusinessObject)Sailing?.Destination, JW_STAInfo),
				Tuple.Create((BusinessObject)Sailing?.Destination, JW_ETAInfo),
				Tuple.Create((BusinessObject)Sailing?.Destination, JW_ATAInfo)
			}.ForEach(pair =>
			{
				var itemContainedInPropertiesChangedByDataRefresh = propertiesChangedByDataRefresh.ContainsKey(pair.Item2.Name);

				var item2Value = pair.Item2.Value;
				var item2ValueInRow = row[pair.Item2.Name];
				var item2ValueEqualsValueInRow = (item2Value.IsEmpty && item2ValueInRow == DBNull.Value) || item2Value.Equals(item2ValueInRow);

				if (
						(
							!IsInDatabase
							|| (pair.Item1 != null && !pair.Item1.IsInDatabase)
							|| (!JW_JXInfo.HasChanges && !item2ValueEqualsValueInRow)
						)
						&&
						(
							itemContainedInPropertiesChangedByDataRefresh && !propertiesChangedByDataRefresh[pair.Item2.Name].Equals(item2Value)
							|| !itemContainedInPropertiesChangedByDataRefresh
						)
					)
				{
					pair.Item2.PushValueIntoRow();
					propertiesChangedByDataRefresh.Remove(pair.Item2.Name);
				}
			});
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				StoreOriginalValuesForProxiedFields(true);
				UnsubscribeConcurrencyMergeHandlers();
			}
			else
			{
				SubscribeConcurrencyMergeHandlers();
			}
			ResetDateAdded();
			ResetSkipAddDateEvents();
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			UnhookSailingManagerWithoutReleasingSailing();

			if (!JW_IsLinked)
			{
				fJW_JX_IsPublished = false;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			if (JW_IsLinked)
			{
				SailingManager.NotifySave();
				SynchroniseProxiedFieldsIfNeeded();
			}
			base.RunPreSaveValidationCore();
		}

		public override bool IsSavedByFactory
		{
			get { return persistent && base.IsSavedByFactory; }
		}

		bool ShouldAddDateEvents => JW_ParentType != Constants.TransportParentTypes.ImporterSecurityFiling;

		void ResetSkipAddDateEvents()
		{
			SkipAddATDEvent = false;
			SkipAddATAEvent = false;
		}

		public bool SkipAddATDEvent { get; set; }

		public bool SkipAddATAEvent { get; set; }

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				UpdateOriginalValues();
			}
		}

		void UpdateOriginalValues()
		{
			UpdateVoyageRelatedOriginalValues();
			UpdateOriginRelatedOriginalValues();
			UpdateDestinationRelatedOriginalValues();
			UpdateSailingRelatedOriginalValues();
		}

		public void UpdateVoyageRelatedOriginalValues()
		{
			if (IsDeleted)
			{
				return;
			}

			UpdateJW_VesselOriginalValue();
			UpdateJW_VoyageFlightOriginalValue();
			UpdateJW_OA_CarrierAddressOriginalValue();
		}

		public void UpdateOriginRelatedOriginalValues()
		{
			UpdateJW_RL_NKLoadPortOriginalValue();
			UpdateJW_ETDOriginalValue();
			UpdateJW_TerminalReceivalCommencesOriginalValue();
			UpdateJW_TerminalCutOffOriginalValue();
			UpdateJW_DocumentaryCutOffOriginalValue();
			UpdateJW_VGMCutOffOriginalValue();
		}

		public void UpdateDestinationRelatedOriginalValues()
		{
			UpdateJW_RL_NKDiscPortOriginalValue();
			UpdateJW_ETAOriginalValue();
		}

		public void UpdateSailingRelatedOriginalValues()
		{
			UpdateJW_DepotReceivalCommencesOriginalValue();
			UpdateJW_DepotCutOffOriginalValue();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TransportFetchStrategy(this);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			isSettingDefaultValues = true;
			try
			{
				base.SetDefaultValues();
				JW_ParentType = "";
				JW_Status = FreightDataRegistry.Instance.DefaultRoutingLegStatus.Value;
			}
			finally
			{
				isSettingDefaultValues = false;
			}
		}

		#endregion

		#region Related Business Objects

		#region Sailing

		public JobSailing Sailing
		{
			get
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
				}

				return Factory.Load<JobSailing>(JW_JX);
			}
		}

		#endregion

		#region Vessel

		public RefVessel Vessel
		{
			get { return RefVessel.LookupVesselByFK(this, JobConsolTransportSchema.JW_Vessel); }
		}

		public IStringSimilarity StringSimilarity
		{
			get
			{
				return stringSimilarity ?? (stringSimilarity = new JaroWinklerStringSimilarity());
			}
			set
			{
				stringSimilarity = value;
			}
		}

		IStringSimilarity stringSimilarity;
		const float vesselNameSimilarityThreshold = 0.85f;

		public bool IsVesselMatched(ZString vesselLloyds, ZString vesselName)
		{
			var lloydsNumber = Vessel?.RV_LloydsNumber ?? ZString.Empty;

			if (!lloydsNumber.IsEmpty && !vesselLloyds.IsEmpty)
			{
				return lloydsNumber.EqualsIgnoringCase(vesselLloyds);
			}

			if (JW_Vessel.IsEmpty || vesselName.IsEmpty)
			{
				return false;
			}

			if (JW_Vessel.EqualsIgnoringCase(vesselName))
			{
				return true;
			}

			var similarity = StringSimilarity.CalculateSimilarity(JW_Vessel, vesselName);
			return similarity > vesselNameSimilarityThreshold;
		}

		#endregion

		#region Voyage

		public JobVoyage Voyage
		{
			get
			{
				return Sailing?.Voyage;
			}
		}

		#endregion

		#region TransportSupporter

		#region TransportSupporter

		public TransportSupporterCommon TransportSupporter
		{
			get
			{
				if (Parent == null)
				{
					// always return a NULL supporter if Parent is NULL - keeps existing functionality
					return null;
				}
				else if (transportSupporterCommon == null)
				{
					var transportParent = Parent as ITransportParent;
					if (transportParent != null)
					{
						transportSupporterCommon = transportParent.TransportSupporter;
					}
					else
					{
						var parentCore = Parent as ITransportParentCore;
						transportSupporterCommon = parentCore != null ? new TransportSupporterWithParent(parentCore) : null;
					}
				}

				return transportSupporterCommon;
			}
		}
		TransportSupporterCommon transportSupporterCommon;

		#region TransportSupporterWithParent

		/// <summary>
		/// Used by Transport Booking
		/// </summary>
		class TransportSupporterWithParent : TransportSupporterCommon
		{
			public TransportSupporterWithParent(ITransportParentCore parentCore)
			{
				this.parentCore = parentCore;
			}
			readonly ITransportParentCore parentCore;

			public override ZString BillOfLading
			{
				get { return parentCore.BillOfLading; }
			}

			public override ZString ConsignmentRef
			{
				get { return parentCore.ConsignmentRef; }
			}

			public override ZString ContainerMode
			{
				get { return parentCore.ContainerMode; }
			}

			public override ZString Description
			{
				get { return parentCore.Description; }
			}

			public override SecurityCheckpoint DistanceCalculationCheckpoint
			{
				get { return parentCore.DistanceCalculationCheckpoint; }
			}

			public override ZString TransportMode
			{
				get { return parentCore.TransportMode; }
			}
		}

		#endregion

		#endregion

		#region TransportSupporterWithSchedule

		/// <summary>
		/// Used by Foprwarding / Agency / Customs
		/// </summary>
		public TransportSupporter TransportSupporterWithSchedule
		{
			get { return TransportSupporter as TransportSupporter; }
		}

		#endregion

		#endregion

		#region OtherParentTransports

		public IEnumerable<Transport> OtherParentTransports
		{
			get
			{
				IEnumerable<Transport> parentTransports;

				var transportParent = Parent as ITransportParent;
				if (transportParent != null)
				{
					parentTransports = transportParent.Transports.Cast<Transport>();
				}
				else if (Parent != null)
				{
					parentTransports = Parent.Factory.Load<Transport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, Parent.PK));

					foreach (var transport in parentTransports)
					{
						if (transport.ParentType == null)
						{
							transport.ParentType = ParentType;
						}
					}
				}
				else
				{
					parentTransports = Array.Empty<Transport>();
				}

				return parentTransports.Where(t => t.PK != PK);
			}
		}

		#endregion

		#endregion

		#region Binding Lists

		#region JW_TransportMode_List

		public CodeDescriptionPairList JW_TransportMode_List
		{
			get
			{
				return Factory.GetCachedValue(
					"Transport.JW_TransportMode_List",
					FreightCodePairLists.RoutingTransportModeList);
			}
		}

		#endregion

		#region JW_AdditionalTransportMode_List

		public CodeDescriptionPairList JW_AdditionalTransportMode_List
		{
			get
			{
				var cacheKey = ZString.Empty;
				if (JW_TransportType == Constants.TransportPlanningType.PreCarriage
					|| JW_TransportType == Constants.TransportPlanningType.OnForwarding)
				{
					if (JW_TransportMode == Constants.TransportModes.Rail)
					{
						cacheKey = (NoResString)"Rail";
					}
					else if (JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport)
					{
						cacheKey = "InlandWaterwayTransport";
					}
				}

				if (!cacheKey.IsEmpty)
				{
					return Factory.GetCachedValue("Transport.JW_AdditionalTransportMode_List_" + cacheKey,
						this.AdditionalTransportModeList);
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		#endregion

		#region JW_TransportType_List

		public virtual CodeDescriptionPairList JW_TransportType_List
		{
			get
			{
				ZString cacheKey = ZString.Empty;
				switch (JW_TransportMode)
				{
					case Constants.TransportModes.Air:
						cacheKey = (NoResString)"Air";
						break;
					case Constants.TransportModes.InlandWaterwayTransport:
						cacheKey = "InlandWaterwayTransport";
						break;
					case Constants.TransportModes.Sea:
					case Constants.TransportModes.Road:
					case Constants.TransportModes.Rail:
						cacheKey = (NoResString)"Other";
						break;
				}

				if (!cacheKey.IsEmpty)
				{
					return Factory.GetCachedValue("Transport.JW_TransportType_List_" + cacheKey,
					delegate
					{
						return FreightCodePairLists.RoutingTransportTypeList(JW_TransportMode);
					});
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		#endregion

		#region JW_Status_List

		public CodeDescriptionPairList JW_Status_List
		{
			get
			{
				if (IsAir)
				{
					return Factory.GetCachedValue("Transport.JW_Status_List_Air", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Constants.TransportStatus.Planned, Constants.TransportStatusDescriptions.Planned);
						result.AddPair(Constants.TransportStatus.Confirmed, Constants.TransportStatusDescriptions.Confirmed);
						result.AddPair(Constants.TransportStatus.Requested, Constants.TransportStatusDescriptions.Requested);
						result.AddPair(Constants.TransportStatus.CancellationRequested, Constants.TransportStatusDescriptions.CancellationRequested);
						result.AddPair(Constants.TransportStatus.Unable, Constants.TransportStatusDescriptions.Unable);
						result.AddPair(Constants.TransportStatus.Queued, Constants.TransportStatusDescriptions.Queued);
						result.AddPair(Constants.TransportStatus.FlightNotOperating, Constants.TransportStatusDescriptions.FlightNotOperating);
						result.AddPair(Constants.TransportStatus.Cancelled, Constants.TransportStatusDescriptions.Cancelled);

						return result;
					});
				}

				return Factory.GetCachedValue("Transport.JW_Status_List", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.TransportStatus.Planned, Constants.TransportStatusDescriptions.Planned);
					result.AddPair(Constants.TransportStatus.Confirmed, Constants.TransportStatusDescriptions.Confirmed);
					result.AddPair(Constants.TransportStatus.Held, Constants.TransportStatusDescriptions.Held);

					return result;
				});
			}
		}

		#endregion

		#region CarrierServiceLevel

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, JW_PL_NKCarrierServiceLevel)); }
		}

		public OrgCarrierServiceLevelCollection CarrierServiceLevel_List
		{
			get
			{
				return (carrierServiceLevelsCache ?? (carrierServiceLevelsCache = new CachedProperty<OrgCarrierServiceLevelCollection>(Factory, GetCarrierServiceLevel_List))).Value;
			}
		}

		CachedProperty<OrgCarrierServiceLevelCollection> carrierServiceLevelsCache;

		OrgCarrierServiceLevelCollection GetCarrierServiceLevel_List()
		{
			OrgCarrierServiceLevelCollection carrierServiceLevels = Carrier != null
																	? new OrgCarrierServiceLevelCollection(Carrier.MiscServ)
																	: new OrgCarrierServiceLevelCollection(Factory);
			carrierServiceLevels.Load();
			return carrierServiceLevels;
		}

		#endregion

		#region UNLOCOCollection

		public RefUNLOCOCollection UNLOCOCollection
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		#endregion

		#region ShippingProvider_List

		public ShippingProviderCollection ShippingProvider_List
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		#endregion

		#region DepartureAddressOrgs

		public OrgHeaderCollection DepartureAddressOrgs
		{
			get
			{
				OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
				SetOrgFilters(collection, JW_RL_NKLoadPort);
				return collection;
			}
		}

		#endregion

		#region ArrivalAddressOrgs

		public OrgHeaderCollection ArrivalAddressOrgs
		{
			get
			{
				OrgHeaderCollection collection = new OrgHeaderCollection(Factory);
				SetOrgFilters(collection, JW_RL_NKDiscPort);
				return collection;
			}
		}

		#endregion

		#region RefVessels

		public RefVesselCollection RefVessels
		{
			get
			{
				var refVessels = new RefVesselCollection(Factory);

				if (TransportMode.EqualsIgnoringCase(Core.Constants.TransportModes.InlandWaterwayTransport))
				{
					refVessels.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vessel Type", "Property", new ZString(Core.Constants.VesselType.Barge), true));
				}

				return refVessels;
			}
		}

		#endregion

		#endregion

		#region ChangeConcurrencyForProxiedFieldIfNeeded

		ZPropertyInfo ChangeConcurrencyForProxiedFieldIfNeeded(ZPropertyInfo transportField)
		{
			//sorry, I'm ready to put up with a lot, but I am NOT figuring out to to refactor this to not happen on property load LOL. someone else can look at it if they want
			ConcurrencyInfo.SetConcurrencyPolicy(this, transportField.Name, !IsDeleted && (ZBool)JW_IsLinkedInfo.OriginalValue ? ConcurrencyPolicy.Ignore : ConcurrencyPolicy.Default);
			return transportField;
		}

		#endregion

		#region Properties

		#region Vessel Name

		public ZString VesselLabel
		{
			get { return VesselVoyageCaptionProvider.GetVesselCaption(JW_TransportMode); }
		}

		public ZPropertyInfo VesselLabelInfo
		{
			get { return GetZPropertyInfo(nameof(VesselLabel)); }
		}

		#endregion

		#region Voyage Name

		public ZString VoyageLabel
		{
			get { return VesselVoyageCaptionProvider.GetVoyageCaption(JW_TransportMode); }
		}

		public ZPropertyInfo VoyageLabelInfo
		{
			get { return GetZPropertyInfo(nameof(VoyageLabel)); }
		}

		#endregion

		#region JW_ParentGUID

		public override ZGuid JW_ParentGUID
		{
			[DebuggerStepThrough]
			get { return base.JW_ParentGUID; }
			set
			{
				base.JW_ParentGUID = value;
				MarkParentAsNeedingValidation();

				if (ParentType != null && Parent == null)
				{
					parentGUIDStackTrace = System.Environment.StackTrace;
				}
			}
		}

		string parentGUIDStackTrace;

		#endregion

		#region JW_IsLinked

		public enum DebugLogContext
		{
			LogIfSetJW_IsLinkedFromTrueToFalse
		}

		public ZStringBuilder SetJW_IsLinkedFromTrueToFalseLog => setJW_IsLinkedFromTrueToFalseLog;
		readonly ZStringBuilder setJW_IsLinkedFromTrueToFalseLog = new ZStringBuilder();

		public override ZBool JW_IsLinked
		{
			[DebuggerStepThrough]
			get { return base.JW_IsLinked; }
			set
			{
				IsSettingIsLinked = true;

				if (JW_IsLinked && !value)
				{
					JW_JX = ZGuid.Empty;
					ResetSailingManager();

					if (Factory.HasContext(DebugLogContext.LogIfSetJW_IsLinkedFromTrueToFalse))
					{
						SetJW_IsLinkedFromTrueToFalseLog.Append($"JW_IsLinked was updated from true to false and JW_JX was updated to empty. call stack is: {System.Environment.StackTrace}");
					}
				}

				bool changed = JW_IsLinked != value;

				base.JW_IsLinked = value;

				if (changed)
				{
					if (IsLinkedChanging)
					{
						throw new ApplicationException("IsLinked already changing");
					}

					using (GetValidationSuspender())
					{
						IsLinkedChanging = true;
						try
						{
							if (value)
							{
								MoveValuesFromPersisted();
								SailingManager.Dirty = true;
								SailingManager.NotifyRead();
							}
							else
							{
								MoveValuesToPersisted();
							}
						}
						finally
						{
							IsLinkedChanging = false;
						}
					}

					if (!value)
					{
						fJW_JX_IsPublished = false;
						JW_JX_IsPublishedInfo.RefreshBinding();
					}
					else
					{
						JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
					}

					((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;
					jobCO2eCollection = null;

					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_IsLinked();
						Validation.ValidateJW_TerminalCutOff();
						Validation.ValidateJW_DepotCutOff();
						Validation.ValidateJW_Vessel();
						Validation.ValidateJW_ServiceString();
						Validation.ValidateTotalCO2eForSorting();
						Validation.ValidateJW_JX_Load_ATA();
						Validation.ValidateJW_JX_Load_ETA();
						Validation.ValidateJW_EmptyReceivalCommences();
						Validation.ValidateJW_EmptyCutOff();
						Validation.ValidateJW_ReeferReceivalCommences();
						Validation.ValidateJW_ReeferCutOff();
						Validation.ValidateJW_DGReceivalCommences();
						Validation.ValidateJW_DGCutOff();
						Validation.ValidateJW_ArrivalPortRouteId();
						Validation.ValidateJW_DeparturePortRouteId();

						if (value)
						{
							Validation.ValidateJW_VoyageFlight();
						}

						if (IsSea)
						{
							Validation.ValidateCarrierPK();
						}
					}
				}

				IsSettingIsLinked = false;
			}
		}

		bool IsLinkedChanging;
		public bool IsSettingIsLinked { get; private set; }

		protected bool JW_IsLinked_ReadOnly => IsStorageTransportMode || !JW_AdditionalTransportMode.IsEmpty;

		#endregion

		#region JW_TransportMode
		[List("JW_TransportMode_List")]
		public override ZString JW_TransportMode
		{
			[DebuggerStepThrough]
			get { return base.JW_TransportMode; }
			set
			{
				var previousValue = JW_TransportMode;
				if (value != previousValue)
				{
					ReleaseSailing();
					UnhookSailingManagerWithoutReleasingSailing();

					try
					{
						base.JW_TransportMode = value;
						ResetSailingDetails();
						if (JW_IsLinked)
						{
							SailingManager.NotifyRead();
						}
					}
					finally
					{
						if (Parent is CommonConsol && Voyage != null && JW_IsLinked)
						{
							var voyageVessel = Voyage.JV_RV_NKVessel;
							var transportVessel = JW_Vessel;
							var voyageVoyageFlight = Voyage.JV_VoyageFlight;
							var transportVoyageFlight = JW_VoyageFlight;

							var vesselComparison = string.Compare(voyageVessel, transportVessel, StringComparison.OrdinalIgnoreCase);
							var voyageFlightComparison = string.Compare(voyageVoyageFlight, transportVoyageFlight, StringComparison.OrdinalIgnoreCase);
							var vesselVoyageSyncLog = new ZStringBuilder();

							if (vesselComparison != 0)
							{
								vesselVoyageSyncLog.AppendLine(FormattableString.Invariant($"TransportMode: Transport.JW_Vessel != Voyage.JV_RV_NKVessel for linked Transport: {PK}, Voyage: {Voyage.PK}, Comparison: {vesselComparison}"));
								vesselVoyageSyncLog.AppendLine("'" + transportVessel + "'");
								vesselVoyageSyncLog.AppendLine("'" + voyageVessel + "'");
							}

							if (voyageFlightComparison != 0)
							{
								vesselVoyageSyncLog.AppendLine(FormattableString.Invariant($"TransportMode: Transport.JW_VoyageFlight != Voyage.JV_VoyageFlight for linked Transport: {PK}, Voyage: {Voyage.PK}, Comparison: {voyageFlightComparison}"));
								vesselVoyageSyncLog.AppendLine("'" + transportVoyageFlight + "'");
								vesselVoyageSyncLog.AppendLine("'" + voyageVoyageFlight + "'");
							}

							if (vesselComparison != 0 || voyageFlightComparison != 0)
							{
								ErrorReporter.ReportOnce("Unsynced Vessel/Voyage", vesselVoyageSyncLog.ToString());
							}
						}
					}

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyTransportModeChanged(this, previousValue);
					}

					if (!IsValidationSuspended)
					{
						ValidateLoadPortsForAllTransports();
					}

					if (IsAir && !IsInDatabase)
					{
						JW_Status = Constants.TransportStatus.Planned;
					}

					UpdateTransportCO2eStatusToNotCurrent(JW_TransportModeInfo);
				}

				if (!JW_TransportType_List.ContainsCode(JW_TransportType))
				{
					SetTransportTypeBasedOnTransportMode();
				}

				if (JW_TransportMode == Constants.TransportModes.Road)
				{
					JW_DistanceUnit = DistanceCalculationRegistry.Instance.DefaultDistanceUnit.Value;
				}

				ClearAdditionalTransportMode();

				MarkParentAsNeedingValidation(true);
			}
		}

		protected bool IsStorageTransportMode
		{
			get { return JW_TransportMode == Constants.TransportModes.Storage; }
		}

		#endregion

		#region JW_TransportType

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		[List("JW_TransportType_List")]
		public override ZString JW_TransportType
		{
			[DebuggerStepThrough]
			get { return base.JW_TransportType; }
			set
			{
				ZString previousValue = JW_TransportType;

				if (value != previousValue)
				{
					base.JW_TransportType = value;

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyTransportTypeChanged(this, previousValue);
					}

					ClearAdditionalTransportMode();

					MarkParentAsNeedingValidation(true);
				}
			}
		}

		#endregion

		#region ServiceLevel

		[List("CarrierServiceLevel_List")]
		[ResourceStringData("JobConsolTransport|JW_PL_NKCarrierServiceLevel", Caption = "Carrier Service Level", FullDescription = "The Service Level represents the type of service booked with the Carrier responsible for this routing leg.", MediumCaption = "", ShortCaption = "Car.Svc. Lvl.")]
		public override ZString JW_PL_NKCarrierServiceLevel
		{
			get { return base.JW_PL_NKCarrierServiceLevel; }
			set
			{
				if (base.JW_PL_NKCarrierServiceLevel != value)
				{
					base.JW_PL_NKCarrierServiceLevel = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_PL_NKCarrierServiceLevel();
					}
				}
			}
		}

		#endregion

		#region JW_Status

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		[List(nameof(JW_Status_List))]
		public override ZString JW_Status
		{
			get { return base.JW_Status; }
			set
			{
				if (base.JW_Status != value)
				{
					base.JW_Status = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_CarrierBookingReference();
					}
				}
			}
		}

		#endregion

		#region JW_IsCharter

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		public ZBool JW_IsCharterForBinding
		{
			get => JW_IsCharter;
			set => JW_IsCharter = value;
		}

		public ZPropertyInfo JW_IsCharterForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_IsCharterForBinding, p => JW_IsLinked && Sailing?.Voyage != null ? Sailing?.Voyage.JV_IsCharteredInfo : JW_IsCharterInfo);

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		public override ZBool JW_IsCharter
		{
			[DebuggerStepThrough]
			get { return JW_IsCharterCore; }
			set
			{
				var previousValue = JW_IsCharterCore;

				if (value != previousValue)
				{
					JW_IsCharterCore = value;
					MarkParentAsNeedingValidation();

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyIsCharterChanged(this, previousValue);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_IsCharter();
					}
				}
			}
		}

		bool JW_IsCharterCore
		{
			get
			{
				bool result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_IsCharter;
				}
				else
				{
					result = base.JW_IsCharter;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					fJW_IsCharter = value;
					HasChanges = true;

					SailingManager.IsCharterDirty = true;
					SailingManager.NotifyRead();

					JW_IsCharterInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_IsCharter();
					}
				}
				else
				{
					base.JW_IsCharter = value;
				}
			}
		}

		public override ZPropertyInfo JW_IsCharterInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_IsCharterInfo);

		ZBool fJW_IsCharter;

		public ZBool PreviousJW_IsCharter { get; private set; }

		#endregion

		#region JW_IsCargoOnly

		public override ZBool JW_IsCargoOnly
		{
			[DebuggerStepThrough]
			get { return JW_IsCargoOnlyCore; }
			set
			{
				var previousValue = JW_IsCargoOnlyCore;

				if (value != previousValue)
				{
					JW_IsCargoOnlyCore = value;

					MarkParentAsNeedingValidation(true);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_IsCargoOnly();
					}
				}
			}
		}

		bool JW_IsCargoOnlyCore
		{
			get
			{
				bool result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_IsCargoOnly;
				}
				else
				{
					result = base.JW_IsCargoOnly;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					fJW_IsCargoOnly = value;
					HasChanges = true;

					SailingManager.IsCargoOnlyDirty = true;
					SailingManager.NotifyRead();

					JW_IsCargoOnlyInfo.RefreshBinding();

					if (Voyage != null && IsAir)
					{
						Voyage.JV_IsCargoOnly = value;
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_IsCargoOnly();
					}
				}
				else
				{
					base.JW_IsCargoOnly = value;
				}
			}
		}

		public override ZPropertyInfo JW_IsCargoOnlyInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_IsCargoOnlyInfo);

		ZBool fJW_IsCargoOnly;

		public ZBool PreviousJW_IsCargoOnly { get; private set; }

		protected bool JW_IsCargoOnly_ReadOnly
		{
			get { return !IsAir; }
		}

		#endregion

		#region JW_AircraftType

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		public ZString JW_AircraftTypeForBinding
		{
			get => JW_AircraftType;
			set
			{
				if (JW_AircraftType != value)
				{
					IsLastAircraftUpdatedFromGSS = false;
				}
				JW_AircraftType = value;
			}
		}

		public ZPropertyInfo JW_AircraftTypeForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_AircraftTypeForBinding, p => JW_IsLinked && Sailing?.Voyage != null ? Sailing?.Voyage.JV_AircraftTypeInfo : JW_AircraftTypeInfo);

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		public override ZString JW_AircraftType
		{
			[DebuggerStepThrough]
			get { return JW_AircraftTypeCore; }
			set
			{
				var previousValue = JW_AircraftTypeCore;

				if (value != previousValue)
				{
					CheckMaximumLength(JW_AircraftTypeInfo, value);

					JW_AircraftTypeCore = value;
					MarkParentAsNeedingValidation();

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyAircraftTypeChanged(this, previousValue);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_AircraftType();
					}

					UpdateTransportCO2eStatusToNotCurrent(JW_AircraftTypeInfo);
				}
			}
		}

		ZString JW_AircraftTypeCore
		{
			get
			{
				ZString result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_AircraftType;
				}
				else
				{
					result = base.JW_AircraftType;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					fJW_AircraftType = value;
					HasChanges = true;

					SailingManager.IsAircraftTypeDirty = true;
					SailingManager.NotifyRead();

					JW_AircraftTypeInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_AircraftType();
					}
				}
				else
				{
					base.JW_AircraftType = value;
				}
			}
		}

		public override ZPropertyInfo JW_AircraftTypeInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_AircraftTypeInfo);

		ZString fJW_AircraftType;

		public ZString PreviousJW_AircraftType { get; private set; }

		#endregion

		#region JW_LegOrder

		public override ZByte JW_LegOrder
		{
			[DebuggerStepThrough]
			get { return base.JW_LegOrder; }
			set
			{
				ZByte previousValue = JW_LegOrder;
				base.JW_LegOrder = value;
				if (previousValue != JW_LegOrder)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ETD();
						Validation.ValidateJW_ETA();
					}

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyLegOrderChanged(this, previousValue);
					}
				}
			}
		}

		#endregion

		#region JW_JX

		public override ZGuid JW_JX
		{
			[DebuggerStepThrough]
			get { return base.JW_JX; }
			set
			{
				ZGuid previousValue = JW_JX;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
				}

				if (value != previousValue)
				{
					base.JW_JX = value;

					UnsubscribeConcurrencyMergeHandlers();
					if (!value.IsEmpty)
					{
						if (JW_IsLinked)
						{
							SailingManager.Sailing = Factory.Load<JobSailing>(JW_JX);
							SailingManager.Dirty = false;
						}
					}

					SetAllScheduleFields(Sailing);
					SetIsDomestic();

					MarkParentAsNeedingValidation();

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifySailingChanged(this, previousValue);
					}
					jobCO2eCollection = null;
				}
			}
		}

		#endregion

		#region JW_RL_NKLoadPort

		[List("UNLOCOCollection")]
		public ZString JW_RL_NKLoadPortForBinding
		{
			get => JW_RL_NKLoadPort;
			set => JW_RL_NKLoadPort = value;
		}

		public ZPropertyInfo JW_RL_NKLoadPortForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_RL_NKLoadPortForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing?.Origin.JA_RL_NKPortOfLoadingInfo : JW_RL_NKLoadPortInfo);

		[List("UNLOCOCollection")]
		public override ZString JW_RL_NKLoadPort
		{
			[DebuggerStepThrough]
			get
			{
				var result = JW_RL_NKLoadPortCore;

				if (!JW_RL_NKLoadPortOriginalValue.HasValue)
				{
					JW_RL_NKLoadPortOriginalValue = result;
				}

				return result;
			}
			set
			{
				ZString previousValue = JW_RL_NKLoadPortCore;

				if (value != previousValue)
				{
					JW_RL_NKLoadPortCore = value;

					var consolParent = Parent as CommonConsol;
					if (consolParent != null && JW_TransportMode == Constants.TransportModes.Sea)
					{
						consolParent.Containers.Cast<CommonContainer>().Where(x => !x.IsValidationSuspended).ForEach(x => x.Validation.ValidateJC_GrossWeightVerificationType());
					}
					else if (JW_TransportMode == Constants.TransportModes.Storage)
					{
						JW_RL_NKDiscPortCore = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ATD();

						if (consolParent != null)
						{
							consolParent.Validation.ValidateJK_RL_NKLoadPort();
						}
					}

					MarkParentAsNeedingValidation(true);
					if (!value.IsEmpty)
					{
						SetIsDomestic();
					}

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyLoadChanged(this, previousValue);
					}

					TryMatchAgainstOnlineFlights();

					UpdateTransportCO2eStatusToNotCurrent(JW_RL_NKLoadPortInfo);

					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.TransportLeg);
				}
			}
		}

		public ZString? JW_RL_NKLoadPortOriginalValue { get; private set; }

		void UpdateJW_RL_NKLoadPortOriginalValue()
		{
			if (JW_RL_NKLoadPortOriginalValue.HasValue)
			{
				JW_RL_NKLoadPortOriginalValue = JW_RL_NKLoadPort;
			}
		}

		public ZBool JW_RL_NKLoadPortHasChanges()
		{
			return IsInDatabase && JW_RL_NKLoadPortOriginalValue.HasValue && JW_RL_NKLoadPortOriginalValue.Value != JW_RL_NKLoadPort;
		}

		ZString JW_RL_NKLoadPortCore
		{
			get
			{
				ZString result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_RL_NKLoadPort;
				}
				else
				{
					result = base.JW_RL_NKLoadPort;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_RL_NKLoadPortInfo, value);
					SailingManager.NotifyRead();

					ZString oldValue = fJW_RL_NKLoadPort;
					fJW_RL_NKLoadPort = value;
					HasChanges = true;

					SailingManager.LoadDirty = true;
					SailingManager.NotifyRead();

					JW_RL_NKLoadPortInfo.RefreshBinding(oldValue);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_RL_NKLoadPort();
					}
					PropertyChangeSubscription.NotifyPropertyChanged(JW_RL_NKLoadPortInfo, oldValue);
				}
				else
				{
					base.JW_RL_NKLoadPort = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_RL_NKDiscPort();
				}

				MarkParentAsNeedingValidation(true);
			}
		}

		public override ZPropertyInfo JW_RL_NKLoadPortInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_RL_NKLoadPortInfo);

		ZString fJW_RL_NKLoadPort;

		public ZString PreviousJW_RL_NKLoadPort { get; private set; }

		#endregion

		#region JW_RL_NKDiscPort

		[List("UNLOCOCollection")]
		public ZString JW_RL_NKDiscPortForBinding
		{
			get => JW_RL_NKDiscPort;
			set => JW_RL_NKDiscPort = value;
		}

		public ZPropertyInfo JW_RL_NKDiscPortForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_RL_NKDiscPortForBinding, p => JW_IsLinked && Sailing?.Destination != null ? Sailing?.Destination.JB_RL_NKPortOfDischargeInfo : JW_RL_NKDiscPortInfo);

		[List("UNLOCOCollection")]
		public override ZString JW_RL_NKDiscPort
		{
			[DebuggerStepThrough]
			get
			{
				var result = JW_RL_NKDiscPortCore;

				if (!JW_RL_NKDiscPortOriginalValue.HasValue)
				{
					JW_RL_NKDiscPortOriginalValue = result;
				}

				return result;
			}
			set
			{
				ZString previousValue = JW_RL_NKDiscPort;

				if (value != previousValue)
				{
					JW_RL_NKDiscPortCore = value;

					if (JW_TransportMode == Core.Constants.TransportModes.Storage)
					{
						JW_RL_NKLoadPortCore = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ATA();
						ValidateLoadPortsForAllTransports();

						var consolParent = Parent as CommonConsol;
						if (consolParent != null)
						{
							consolParent.Validation.ValidateJK_RL_NKLoadPort();
						}
					}

					MarkParentAsNeedingValidation(true);
					if (!value.IsEmpty)
					{
						SetIsDomestic();
					}

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyDischargeChanged(this, previousValue);
					}

					TryMatchAgainstOnlineFlights();

					UpdateTransportCO2eStatusToNotCurrent(JW_RL_NKDiscPortInfo);

					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.TransportLeg);
				}
			}
		}

		public ZString? JW_RL_NKDiscPortOriginalValue { get; private set; }

		void UpdateJW_RL_NKDiscPortOriginalValue()
		{
			if (JW_RL_NKDiscPortOriginalValue.HasValue)
			{
				JW_RL_NKDiscPortOriginalValue = JW_RL_NKDiscPort;
			}
		}

		public ZBool JW_RL_NKDiscPortHasChanges()
		{
			return IsInDatabase && JW_RL_NKDiscPortOriginalValue.HasValue && JW_RL_NKDiscPortOriginalValue.Value != JW_RL_NKDiscPort;
		}

		ZString JW_RL_NKDiscPortCore
		{
			get
			{
				ZString result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_RL_NKDiscPort;
				}
				else
				{
					result = base.JW_RL_NKDiscPort;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_RL_NKDiscPortInfo, value);
					SailingManager.NotifyRead();

					ZString oldValue = fJW_RL_NKDiscPort;
					fJW_RL_NKDiscPort = value;
					HasChanges = true;

					SailingManager.DischargeDirty = true;
					SailingManager.NotifyRead();

					JW_RL_NKDiscPortInfo.RefreshBinding(oldValue);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_RL_NKDiscPort();
					}
					PropertyChangeSubscription.NotifyPropertyChanged(JW_RL_NKDiscPortInfo, oldValue);
				}
				else
				{
					base.JW_RL_NKDiscPort = value;
				}

				MarkParentAsNeedingValidation(true);
			}
		}

		public override ZPropertyInfo JW_RL_NKDiscPortInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_RL_NKDiscPortInfo);

		ZString fJW_RL_NKDiscPort;

		public ZString PreviousJW_RL_NKDiscPort { get; private set; }

		void ValidateLoadPortsForAllTransports()
		{
			var transportParent = Parent as ITransportParent;
			if (transportParent != null)
			{
				foreach (Transport transport in transportParent.Transports)
				{
					transport.Validation.ValidateJW_RL_NKLoadPort();
				}
			}
		}

		#endregion

		#region JW_Vessel

		[List("RefVessels")]
		public ZString JW_VesselForBinding
		{
			get => JW_Vessel;
			set => JW_Vessel = value;
		}

		public ZPropertyInfo JW_VesselForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_VesselForBinding, p => JW_IsLinked && Sailing?.Voyage != null ? Sailing?.Voyage.JV_RV_NKVesselInfo : JW_VesselInfo);

		[RequiresSuppression]
		[List("RefVessels")]
		public override ZString JW_Vessel
		{
			get
			{
				var result = JW_VesselCore;

				if (!JW_VesselOriginalValue.HasValue)
				{
					JW_VesselOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_VesselCore;
				var newValue = value.TrimEnd();

				if (newValue != previousValue)
				{
					JW_VesselCore = newValue;

					((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;

					if (TransportSupporterWithSchedule != null && IsSea)
					{
						TransportSupporterWithSchedule.NotifyVesselChanged(this, previousValue);
					}

					UpdateTransportCO2eStatusToNotCurrent(JW_VesselInfo);
				}
			}
		}

		public ZString? JW_VesselOriginalValue { get; private set; }

		void UpdateJW_VesselOriginalValue()
		{
			if (JW_VesselOriginalValue.HasValue)
			{
				JW_VesselOriginalValue = JW_Vessel;
			}
		}

		public ZBool JW_VesselHasChanges()
		{
			return IsInDatabase && JW_VesselOriginalValue.HasValue && JW_VesselOriginalValue.Value != JW_Vessel;
		}

		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus { get; set; }

		ZString JW_VesselCore
		{
			get
			{
				ZString result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_Vessel;
				}
				else
				{
					result = base.JW_Vessel;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_VesselInfo, value);
					SailingManager.NotifyRead();

					ZString oldValue = fJW_Vessel;
					fJW_Vessel = value;
					HasChanges = true;

					if (Voyage != null && IsRoad)
					{
						Voyage.JV_RV_NKVessel = value;
					}

					SailingManager.VesselDirty = true;
					SailingManager.NotifyRead();

					JW_VesselInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_Vessel();
					}
					PropertyChangeSubscription.NotifyPropertyChanged(JW_VesselInfo, oldValue);

					if (IsSea && CarrierPK.IsEmpty && Vessel != null && !Vessel.RV_OH.IsEmpty)
					{
						CarrierPK = Vessel.RV_OH;
					}
				}
				else
				{
					base.JW_Vessel = value;
					JW_VesselScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				}
			}
		}

		public override ZPropertyInfo JW_VesselInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_VesselInfo);

		protected bool JW_Vessel_ReadOnly
		{
			get { return (JW_TransportMode == Constants.TransportModes.Air || JW_TransportMode == Constants.TransportModes.Storage); }
		}

		ZString fJW_Vessel;

		public ZString PreviousJW_Vessel { get; private set; }

		#endregion

		#region JW_VoyageFlight

		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		public ZString JW_VoyageFlightForBinding
		{
			get => JW_VoyageFlight;
			set => JW_VoyageFlight = value;
		}

		public ZPropertyInfo JW_VoyageFlightForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_VoyageFlightForBinding, p => JW_IsLinked && Sailing?.Voyage != null ? Sailing?.Voyage.JV_VoyageFlightInfo : JW_VoyageFlightInfo);

		[RequiresSuppression]
		[ReadOnlyMember(nameof(IsStorageTransportMode))]
		public override ZString JW_VoyageFlight
		{
			[DebuggerStepThrough]
			get
			{
				var result = JW_VoyageFlightCore;

				if (!JW_VoyageFlightOriginalValue.HasValue)
				{
					JW_VoyageFlightOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_VoyageFlight;
				var newValue = value.TrimEnd();

				if (newValue != previousValue)
				{
					JW_VoyageFlightCore = newValue;

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyVoyageFlightChanged(this, previousValue);
					}

					TryMatchAgainstOnlineFlights();

					UpdateTransportCO2eStatusToNotCurrent(JW_VoyageFlightInfo);
				}
			}
		}

		public ZString? JW_VoyageFlightOriginalValue { get; private set; }

		void UpdateJW_VoyageFlightOriginalValue()
		{
			if (JW_VoyageFlightOriginalValue.HasValue)
			{
				JW_VoyageFlightOriginalValue = JW_VoyageFlight;
			}
		}

		public ZBool JW_VoyageFlightHasChanges()
		{
			return IsInDatabase && JW_VoyageFlightOriginalValue.HasValue && JW_VoyageFlightOriginalValue.Value != JW_VoyageFlight;
		}

		ZString JW_VoyageFlightCore
		{
			get
			{
				ZString result;

				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_VoyageFlight;
				}
				else
				{
					result = base.JW_VoyageFlight;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_VoyageFlightInfo, value);
					SailingManager.NotifyRead();

					ZString oldValue = fJW_VoyageFlight;
					fJW_VoyageFlight = value;
					HasChanges = true;

					if (Voyage != null && IsAir && JW_IsCharter)
					{
						Voyage.JV_VoyageFlight = value;
					}
					else
					{
						SailingManager.VoyageDirty = true;
						SailingManager.NotifyRead();
					}

					JW_VoyageFlightInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_VoyageFlight();
					}
					PropertyChangeSubscription.NotifyPropertyChanged(JW_VoyageFlightInfo, oldValue);
				}
				else
				{
					base.JW_VoyageFlight = value;
				}
			}
		}

		public override ZPropertyInfo JW_VoyageFlightInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_VoyageFlightInfo);

		ZString fJW_VoyageFlight;

		public ZString PreviousJW_VoyageFlight { get; private set; }

		public bool IsVoyageFlightMatched(ZString flightNumber)
		{
			return AreTwoFlightNumbersMatched(JW_VoyageFlight, flightNumber);
		}

		bool AreTwoFlightNumbersMatched(ZString flightNumber1, ZString flightNumber2)
		{
			if (flightNumber1.EqualsIgnoringCase(flightNumber2))
			{
				return true;
			}

			var airlineCode1 = flightNumber1.SubstringSafe(0, 2);
			var airlineCode2 = flightNumber2.SubstringSafe(0, 2);

			var flightNoWithoutLeadingZero1 = flightNumber1.SubstringSafe(2).TrimStart('0');
			var flightNoWithoutLeadingZero2 = flightNumber2.SubstringSafe(2).TrimStart('0');

			return airlineCode1.EqualsIgnoringCase(airlineCode2) && flightNoWithoutLeadingZero1.EqualsIgnoringCase(flightNoWithoutLeadingZero2);
		}

		public bool IsVoyageFlightFuzzyMatched(ZString voyageFlight)
		{
			var suffix = voyageFlight.Right(1);
			return !suffix.IsEmpty
				&& suffix.IsLettersOnlyOrEmpty
				&& IsVoyageFlightMatched(voyageFlight.Left(voyageFlight.Length - 1));
		}

		#endregion

		#region JW_ETD

		public ZDateTime JW_ETDForBinding
		{
			get => JW_ETD;
			set => JW_ETD = value;
		}

		public ZPropertyInfo JW_ETDForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ETDForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing?.Origin.JA_E_DEPInfo : JW_ETDInfo);

		[RequiresSuppression]
		[EventDateProperty(Events.DepartureCode, EstimateActual.Estimate)]
		public override ZDateTime JW_ETD
		{
			[DebuggerStepThrough]
			get
			{
				var etd = new ZDateTime(JW_ETDCore, DateTimeKind.Unspecified);
				StoreOriginalValuesForProxiedFields();

				if (!JW_ETDOriginalValue.HasValue)
				{
					JW_ETDOriginalValue = etd;
				}

				return etd;
			}
			set
			{
				ZDateTime previousValue = JW_ETD;

				if (value != previousValue)
				{
					JW_ETDCore = value;

					if (JW_ETDCore.IsValid && !JW_STD.IsValid)
					{
						JW_STD = JW_ETDCore;
					}

					MarkParentAsNeedingValidation();

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyETDChanged(this, PreviousJW_ETD);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ETA();
					}

					TryMatchAgainstOnlineFlights();

					if (value.IsEmpty)
					{
						JW_ETD_ResetStackTrace = System.Environment.StackTrace;
						if (eventDatePropertyCancelledContext != null && Parent is CommonConsol consolParent)
						{
							consolParent.TryLogWhileClearTimeOfTransports(null, "_CAN", eventDatePropertyCancelledContext);
						}
					}

					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.TransportLeg);
				}
			}
		}

		public ZDateTime? JW_ETDOriginalValue { get; private set; }

		void UpdateJW_ETDOriginalValue()
		{
			if (JW_ETDOriginalValue.HasValue)
			{
				JW_ETDOriginalValue = JW_ETD;
			}
		}

		public ZBool JW_ETDHasChanges()
		{
			return IsInDatabase && JW_ETDOriginalValue.HasValue && JW_ETDOriginalValue.Value != JW_ETD;
		}

		public string JW_ETD_ResetStackTrace
		{
			get;
			private set;
		}

		public override ZPropertyInfo JW_ETDInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ETDInfo);

		public ZDateTime JW_ETD_UTC
		{
			get { return JW_ETD.IsValid ? Env.Time.GetUtcFromUnlocoTime(JW_RL_NKLoadPort.ToString(), JW_ETD.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime PreviousJW_ETD
		{
			get;
			private set;
		}

		ZDateTime JW_ETDCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ETD;
				}
				else
				{
					result = base.JW_ETD;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					fJW_ETD = value;
					HasChanges = true;

					SailingManager.DepartureDirty = true;
					SailingManager.NotifyRead();

					JW_ETDInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ETD();
					}
				}
				else
				{
					base.JW_ETD = value;
				}
			}
		}

		ZDateTime fJW_ETD;

		#endregion

		#region JW_ETA

		public ZDateTime JW_ETAForBinding
		{
			get => JW_ETA;
			set => JW_ETA = value;
		}

		public ZPropertyInfo JW_ETAForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ETAForBinding, p => JW_IsLinked && Sailing?.Destination != null ? Sailing?.Destination.JB_E_ARVInfo : JW_ETAInfo);

		[RequiresSuppression]
		[EventDateProperty(Events.ArrivalCode, EstimateActual.Estimate)]
		public override ZDateTime JW_ETA
		{
			[DebuggerStepThrough]
			get
			{
				var eta = new ZDateTime(JW_ETACore, DateTimeKind.Unspecified);
				StoreOriginalValuesForProxiedFields();

				if (!JW_ETAOriginalValue.HasValue)
				{
					JW_ETAOriginalValue = eta;
				}

				return eta;
			}
			set
			{
				ZDateTime previousValue = JW_ETA;

				if (value != previousValue)
				{
					JW_ETACore = value;

					if (JW_ETACore.IsValid && !JW_STA.IsValid)
					{
						JW_STA = JW_ETACore;
					}

					MarkParentAsNeedingValidation();

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyETAChanged(this, PreviousJW_ETA);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ETD();
					}

					TryMatchAgainstOnlineFlights();

					if (value.IsEmpty)
					{
						JW_ETA_ResetStackTrace = System.Environment.StackTrace;
					}

					DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor.TransportLeg);
				}
			}
		}

		public ZDateTime? JW_ETAOriginalValue { get; private set; }

		void UpdateJW_ETAOriginalValue()
		{
			if (JW_ETAOriginalValue.HasValue)
			{
				JW_ETAOriginalValue = JW_ETA;
			}
		}

		public ZBool JW_ETAHasChanges()
		{
			return IsInDatabase && JW_ETAOriginalValue.HasValue && JW_ETAOriginalValue.Value != JW_ETA;
		}

		public string JW_ETA_ResetStackTrace
		{
			get;
			private set;
		}

		public ZDateTime JW_ETA_UTC
		{
			get { return JW_ETA.IsValid ? Env.Time.GetUtcFromUnlocoTime(JW_RL_NKDiscPort.ToString(), JW_ETA.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime PreviousJW_ETA
		{
			get;
			private set;
		}

		ZDateTime JW_ETACore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ETA;
				}
				else
				{
					result = base.JW_ETA;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					fJW_ETA = value;
					HasChanges = true;

					SailingManager.ArrivalDirty = true;
					SailingManager.NotifyRead();

					JW_ETAInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ETA();
					}
				}
				else
				{
					base.JW_ETA = value;
				}
			}
		}

		public override ZPropertyInfo JW_ETAInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ETAInfo);

		ZDateTime fJW_ETA;

		#endregion

		#region JW_STD

		public ZDateTime JW_STDForBinding
		{
			get => JW_STD;
			set => JW_STD = value;
		}

		public ZPropertyInfo JW_STDForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_STDForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing?.Origin.JA_S_DEPInfo : JW_STDInfo);

		public override ZDateTime JW_STD
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_IsLinked ? fJW_STD : base.JW_STD;
			}
			set
			{
				var previousValue = JW_STD;
				if (value != previousValue)
				{
					if (JW_IsLinked)
					{
						fJW_STD = value;
						SailingManager.DepartureDirty = true;
						SailingManager.NotifyRead();
					}
					else
					{
						base.JW_STD = value;
					}

					JW_STDInfo.RefreshBinding();
				}
			}
		}

		public override ZPropertyInfo JW_STDInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_STDInfo);

		ZDateTime fJW_STD;

		public ZDateTime JW_STD_UTC
		{
			get { return JW_STD.IsValid ? Env.Time.GetUtcFromUnlocoTime(JW_RL_NKLoadPort.ToString(), JW_STD.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime PreviousJW_STD { get; private set; }

		#endregion

		#region JW_STA

		public ZDateTime JW_STAForBinding
		{
			get => JW_STA;
			set => JW_STA = value;
		}

		public ZPropertyInfo JW_STAForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_STAForBinding, p => JW_IsLinked && Sailing?.Destination != null ? Sailing?.Destination.JB_S_ARVInfo : JW_STAInfo);

		public override ZDateTime JW_STA
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_IsLinked ? fJW_STA : base.JW_STA;
			}
			set
			{
				var previousValue = JW_STA;
				if (value != previousValue)
				{
					if (JW_IsLinked)
					{
						fJW_STA = value;
						SailingManager.ArrivalDirty = true;
						SailingManager.NotifyRead();
					}
					else
					{
						base.JW_STA = value;
					}

					JW_STAInfo.RefreshBinding();
				}
			}
		}

		public override ZPropertyInfo JW_STAInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_STAInfo);

		ZDateTime fJW_STA;

		public ZDateTime JW_STA_UTC
		{
			get { return JW_STA.IsValid ? Env.Time.GetUtcFromUnlocoTime(JW_RL_NKDiscPort.ToString(), JW_STA.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime PreviousJW_STA { get; private set; }

		public DisposableAction PreserveScheduleDates()
		{
			return JW_IsLinked ? SailingManager.PreserveParentScheduleDates() : DisposableAction.NoAction;
		}

		#endregion

		#region Load Port Estimated Time of Arrival

		public virtual ZDateTime JW_JX_Load_ETA
		{
			get
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
				}
				return fJW_JX_Load_ETA;
			}
			set
			{
				if (value != fJW_JX_Load_ETA)
				{
					if (Sailing != null && !Sailing.IsDeleted && Sailing.Origin != null)
					{
						Sailing.Origin.JA_E_ARV = value;
					}

					fJW_JX_Load_ETA = value;
					HasChanges = true;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_JX_Load_ETA();
					}

					JW_JX_Load_ETAInfo.RefreshBinding();
				}
			}
		}
		ZDateTime fJW_JX_Load_ETA;

		public ZPropertyInfo JW_JX_Load_ETAInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_Load_ETA); }
		}

		#endregion

		#region JW_ATD

		public ZDateTime JW_ATDForBinding
		{
			get => JW_ATD;
			set => JW_ATD = value;
		}

		public ZPropertyInfo JW_ATDForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ATDForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing?.Origin.JA_A_DEPInfo : JW_ATDInfo);

		[RequiresSuppression]
		[EventDateProperty(Events.DepartureCode, EstimateActual.Actual)]
		public override ZDateTime JW_ATD
		{
			[DebuggerStepThrough]
			get
			{
				var atd = new ZDateTime(JW_ATDCore, DateTimeKind.Unspecified);
				StoreOriginalValuesForProxiedFields();
				return atd;
			}
			set
			{
				ZDateTime previousValue = JW_ATD;

				if (value != previousValue)
				{
					JW_ATDCore = value;

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyATDChanged(this, previousValue);
					}

					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ATA();
					}
				}
			}
		}

		public override ZPropertyInfo JW_ATDInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ATDInfo);

		public ZDateTime JW_ATD_UTC
		{
			get { return JW_ATD.IsValid ? Env.Time.GetUtcFromUnlocoTime(JW_RL_NKLoadPort.ToString(), JW_ATD.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime PreviousJW_ATD
		{
			get;
			private set;
		}

		ZDateTime JW_ATDCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ATD;
				}
				else
				{
					result = base.JW_ATD;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing != null)
					{
						Sailing.Origin.JA_A_DEP = value;
					}

					fJW_ATD = value;
					HasChanges = true;

					JW_ATDInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ATD();
					}
				}
				else
				{
					base.JW_ATD = value;
				}
			}
		}

		ZDateTime fJW_ATD;

		#endregion

		#region JW_ATA

		public ZDateTime JW_ATAForBinding
		{
			get => JW_ATA;
			set => JW_ATA = value;
		}

		public ZPropertyInfo JW_ATAForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ATAForBinding, p => JW_IsLinked && Sailing?.Destination != null ? Sailing?.Destination.JB_A_ARVInfo : JW_ATAInfo);

		[RequiresSuppression]
		[EventDateProperty(Events.ArrivalCode, EstimateActual.Actual)]
		public override ZDateTime JW_ATA
		{
			[DebuggerStepThrough]
			get
			{
				var ata = new ZDateTime(JW_ATACore, DateTimeKind.Unspecified);
				StoreOriginalValuesForProxiedFields();
				return ata;
			}
			set
			{
				ZDateTime previousValue = JW_ATA;

				if (value != previousValue)
				{
					JW_ATACore = value;

					SaveStackTraceForClientULVIfNeed();

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyATAChanged(this, previousValue);
					}

					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ATD();
					}
				}
			}
		}

		public ZDateTime JW_ATA_UTC
		{
			get { return JW_ATA.IsValid ? Env.Time.GetUtcFromUnlocoTime(JW_RL_NKDiscPort.ToString(), JW_ATA.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime PreviousJW_ATA
		{
			get;
			private set;
		}

		ZDateTime JW_ATACore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ATA;
				}
				else
				{
					result = base.JW_ATA;
				}

				return result;
			}
			set
			{
				ZDateTime previousValue = JW_ATA;

				if (value != JW_ATA)
				{
					if (JW_IsLinked)
					{
						if (Sailing != null)
						{
							Sailing.Destination.JB_A_ARV = value;
						}

						fJW_ATA = value;
						HasChanges = true;

						JW_ATAInfo.RefreshBinding();

						if (!IsValidationSuspended)
						{
							Validation.ValidateJW_ATA();
						}
					}
					else
					{
						base.JW_ATA = value;
					}

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyATAChanged(this, previousValue);
					}
				}
			}
		}

		public override ZPropertyInfo JW_ATAInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ATAInfo);

		ZDateTime fJW_ATA;

		#region Client ULV ATA Problem

		bool hasClientULVATAProblemOccurred;

		void SaveStackTraceForClientULVIfNeed()
		{
			if (!hasClientULVATAProblemOccurred
				&& JW_ATA.IsValid
				&& Math.Abs((JW_ATA - ZDateTime.Now).TotalMinutes) <= 1
				&& EnvProxy.Instance.CurrentUser.IsBatchProcessor
				&& EnvProxy.Instance.CurrentCompany.GetLicenceCode() == "ULVSOFVAR")
			{
				hasClientULVATAProblemOccurred = true;
				GetClientULVATAUpdateStackTrace();
			}
		}

		string GetClientULVATAUpdateStackTrace()
		{
			return Factory.GetCachedValue(
				"ClientULVATAUpdateStackTrace",
				() => string.Format(CultureInfo.InvariantCulture,
					(NoResString)"Transport PK: {0}\r\nStackTrace:\r\n{1}",
					PK,
					new StackTrace()));
		}

		#endregion

		#endregion

		#region Load Port Actual Time of Arrival

		public virtual ZDateTime JW_JX_Load_ATA
		{
			get
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
				}
				return fJW_JX_Load_ATA;
			}
			set
			{
				if (value != fJW_JX_Load_ATA)
				{
					if (Sailing != null && !Sailing.IsDeleted && Sailing.Origin != null)
					{
						Sailing.Origin.JA_A_ARV = value;
					}

					fJW_JX_Load_ATA = value;
					HasChanges = true;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_JX_Load_ATA();
					}

					JW_JX_Load_ATAInfo.RefreshBinding();
				}
			}
		}
		ZDateTime fJW_JX_Load_ATA;

		public ZPropertyInfo JW_JX_Load_ATAInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_Load_ATA); }
		}

		#endregion

		#region JW_OA_CarrierAddress

		public OrgHeader Carrier
		{
			get { return CarrierAddress != null ? CarrierAddress.Header : null; }
		}

		public ZString CarrierName
		{
			get { return Carrier?.OH_FullName ?? ZString.Empty; }
		}

		public ZString CarrierCode
		{
			get { return Carrier?.OH_Code ?? ZString.Empty; }
		}

		[RelatedBusinessObject("Carrier")]
		[List("Lookups.Carriers")]
		[ResourceStringData("JobConsolTransport|JW_OA_CarrierAddress", Caption = "Carrier / Provider", FullDescription = "The carrier or storage provider handling this leg.", MediumCaption = "", ShortCaption = "Carrier")]
		public ZGuid CarrierPK
		{
			get { return JW_OA_CarrierAddress_ZAddress.OrgPK; }
			set
			{
				CheckParentTypeIsSet();

				JW_OA_CarrierAddress_ZAddress.OrgPK = value;
				CarrierPKInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrierPK();
				}
			}
		}

		public ZPropertyInfo CarrierPKInfo
		{
			get { return GetZPropertyInfo(Schema.CarrierPK); }
		}

		public override ZGuid JW_OA_CarrierAddress
		{
			get
			{
				var result = JW_OA_CarrierAddressCore;

				if (!JW_OA_CarrierAddressOriginalValue.HasValue)
				{
					JW_OA_CarrierAddressOriginalValue = result;
				}

				return result;
			}
			set
			{
				ZGuid previousValue = JW_OA_CarrierAddressCore;
				if (previousValue != value)
				{
					JW_OA_CarrierAddressCore = value;

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyCarrierAddressChanged(this, previousValue);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_CarrierBookingReference();
						Validation.ValidateJW_PL_NKCarrierServiceLevel();
					}

					((IShouldUpdateScreeningStatus)this).ShouldUpdateScreeningStatus = true;

					SetDefaultRoutingCreditorFromRoutingCarrier(value);

					PropertyChangeSubscription.NotifyPropertyChanged(JW_OA_CarrierAddressInfo, previousValue);

					UpdateTransportCO2eStatusToNotCurrent(JW_OA_CarrierAddressInfo);
				}
			}
		}

		public ZGuid? JW_OA_CarrierAddressOriginalValue { get; private set; }

		void UpdateJW_OA_CarrierAddressOriginalValue()
		{
			if (JW_OA_CarrierAddressOriginalValue.HasValue)
			{
				JW_OA_CarrierAddressOriginalValue = JW_OA_CarrierAddress;
			}
		}

		public ZBool JW_OA_CarrierAddressHasChanges()
		{
			return IsInDatabase && JW_OA_CarrierAddressOriginalValue.HasValue && JW_OA_CarrierAddressOriginalValue.Value != JW_OA_CarrierAddress;
		}

		void SetDefaultRoutingCreditorFromRoutingCarrier(ZGuid carrierAddressPK)
		{
			if (carrierAddressPK.IsEmpty)
			{
				return;
			}

			var consolParent = GetParentSafe() as CommonConsol;
			if (consolParent == null || !consolParent.JK_IsForwarding)
			{
				return;
			}

			var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter
			{
				OrgAddress = carrierAddressPK,
				OrgPartyAddress = ZGuid.Empty,
				CreditorType = DefaultCreditorHelper.CreditorType.ForwardingConsol,
				UNLOCO = GetRelatedPartyLocation(consolParent),
				TransportMode = JW_TransportMode,
				ContainerMode = consolParent.JK_ConsolMode,
				PaymentType = consolParent.JK_PrepaidCollect
			};

			// Is there any valid related party (to the carrier) as a creditor?
			var newCreditorPK = DefaultCreditorHelper.GetCreditorAddress(orgRelatedPartyFilter, Factory);
			if (!newCreditorPK.IsEmpty)
			{
				JW_OA_CreditorAddress = newCreditorPK;

				return;
			}

			// Fallback to the carrier itself if it is an AP org.
			var orgHeader = DefaultCreditorHelper.GetOrgHeaderFromAddress(carrierAddressPK, Factory);
			if (orgHeader?.OH_IsCreditor ?? false)
			{
				JW_OA_CreditorAddress = carrierAddressPK;
			}
		}

		ZString GetRelatedPartyLocation(CommonConsol consol)
		{
			if (consol is not IRoutingSupport routingSupport || routingSupport.TransportsIncludingRelated.Count == 0)
			{
				return ZString.Empty;
			}

			var routeSet = routingSupport.TransportsIncludingRelated.RouteSets.FirstOrDefault(x => x.RouteSetNumber == RouteSetNumber);
			if (routeSet == null)
			{
				return ZString.Empty;
			}

			if (consol.JK_PrepaidCollect == Constants.PaymentType.Collect)
			{
				return routeSet.Destination?.Code ?? ZString.Empty;
			}

			if (consol.JK_PrepaidCollect == Constants.PaymentType.Prepaid)
			{
				return routeSet.Origin?.Code ?? ZString.Empty;
			}

			// If the Payment Type for the consol is not specified,
			// the process for determining the routing's location to find SPC remains similar to the existing method used for the consol.
			return GetLocationFromRouteSetDirection(routeSet);
		}

		static ZString GetLocationFromRouteSetDirection(RouteSet routeSet)
		{
			var origin = routeSet.Origin?.Code ?? ZString.Empty;
			var destination = routeSet.Destination?.Code ?? ZString.Empty;
			if (origin.IsEmpty || destination.IsEmpty)
			{
				return ZString.Empty;
			}

			var direction = ImportExportHelper.GetJobDirection(origin, destination);
			return direction switch
			{
				Directions.Export or Directions.Domestic => origin,
				Directions.Import or Directions.CrossTrade => destination,
				_ => ZString.Empty
			};
		}

		/// <summary>
		/// This is a proxy property to access whether:
		/// - If this transport is a linked SEA transport: non-persistent cached carrier address.
		/// - Otherwise, the base address directly.
		/// </summary>
		ZGuid JW_OA_CarrierAddressCore
		{
			get
			{
				ZGuid result;

				if (JW_IsLinked && IsSea)
				{
					SailingManager.NotifyRead();
					result = fJW_OA_CarrierAddress;
				}
				else
				{
					result = base.JW_OA_CarrierAddress;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked && IsSea)
				{
					SailingManager.NotifyRead();

					fJW_OA_CarrierAddress = value;
					HasChanges = true;

					SailingManager.CarrierDirty = true;
					SailingManager.NotifyRead();

					JW_OA_CarrierAddressInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_OA_CarrierAddress();
					}
				}
				else
				{
					base.JW_OA_CarrierAddress = value;
				}
			}
		}

		/// <summary>
		/// This non-persistent carrier address PK caches the value when this transport is linked and in SEA mode.
		/// Its value is transferred to persistent with <cref see="MoveValuesToPersisted" />.
		/// Accessing this property directly to avoid SailingManager.NotifyRead which reloads the linked sailing and reassigns multiple properties in this class from the sailing.
		/// </summary>
		ZGuid fJW_OA_CarrierAddress;

		protected override ZAddress GetNewJW_OA_CarrierAddress_ZAddress()
		{
			ZAddress result = base.GetNewJW_OA_CarrierAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		public void RedefaultCreditor(bool shouldOverrideExistingCreditor = false)
		{
			if (shouldOverrideExistingCreditor || JW_OA_CreditorAddress.IsEmpty)
			{
				SetDefaultRoutingCreditorFromRoutingCarrier(JW_OA_CarrierAddressCore);
			}
		}

		#region JW_OA_CreditorAddress

		public OrgHeader Creditor
		{
			get { return CreditorAddress != null ? CreditorAddress.Header : null; }
		}

		[List("Lookups.CreditorList")]
		[ResourceStringData("JobConsolTransport|JW_OA_CreditorAddress", Caption = "Creditor", FullDescription = "The Creditor will be paid for the transport service provided by the carrier for this leg.", MediumCaption = "", ShortCaption = "")]
		public ZGuid CreditorPK
		{
			get { return JW_OA_CreditorAddress_ZAddress.OrgPK; }
			set
			{
				CheckParentTypeIsSet();

				JW_OA_CreditorAddress_ZAddress.OrgPK = value;
				CreditorPKInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateCreditorPK();
				}
			}
		}

		public ZPropertyInfo CreditorPKInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorPK); }
		}

		protected override ZAddress GetNewJW_OA_CreditorAddress_ZAddress()
		{
			ZAddress result = base.GetNewJW_OA_CreditorAddress_ZAddress();
			result.DefaultAddressType = AddressType.APM;
			result.GetDefaultAddress = GetDefaultAddressForCreditor;
			return result;
		}

		ZGuid GetDefaultAddressForCreditor(IOrgHeader org)
		{
			OrgAddress result = null;
			OrgHeader header = org as OrgHeader;

			if (header != null)
			{
				result = header.Addresses.DefaultAddressOfType(OrgAddressType.Payables)
						 ?? header.Addresses.DefaultAddressOfType(OrgAddressType.Postal)
						 ?? header.Addresses.DefaultAddressOfType(OrgAddressType.Office)
						 ?? header.MainAddress;
			}

			return result == null ? ZGuid.Empty : result.PK;
		}

		#endregion

		#region JW_CarrierBookingReference

		public override ZString JW_CarrierBookingReference
		{
			get { return base.JW_CarrierBookingReference; }
			set
			{
				ZString previousValue = JW_CarrierBookingReference;

				base.JW_CarrierBookingReference = value;

				if (TransportSupporterWithSchedule != null)
				{
					TransportSupporterWithSchedule.NotifyCarrierBookingRefChanged(this, previousValue);
				}
			}
		}

		#endregion

		#region JW_Distance

		[ReadOnlyMember(nameof(IsTransportModeNotRoad))]
		[MeasureUnit(Schema.JW_DistanceUnit, MeasureUnitType.Length)]
		public override ZDecimal JW_Distance
		{
			get { return base.JW_Distance; }
			set { base.JW_Distance = value; }
		}

		#endregion

		#region JW_DistanceUnit

		[ReadOnlyMember(nameof(IsTransportModeNotRoad))]
		[List("Lookups.DistanceUnit_List")]
		public override ZString JW_DistanceUnit
		{
			get { return base.JW_DistanceUnit; }
			set { base.JW_DistanceUnit = value; }
		}

		bool IsTransportModeNotRoad
		{
			get { return (JW_TransportMode != Constants.TransportModes.Road); }
		}

		#endregion

		#region CO2e

		[DecimalPlaces(3)]
		public ZDecimal TotalCO2eForSorting => (this as ICO2eLegProvider).DynamicTotalCO2e;

		public ZPropertyInfo TotalCO2eForSortingInfo => GetZPropertyInfo(nameof(TotalCO2eForSorting));

		#endregion

		#region JW_ParentContainerMode

		public ZString JW_ParentContainerMode
		{
			get { return (TransportSupporter == null) ? ZString.Empty : TransportSupporter.ContainerMode; }
		}

		public ZPropertyInfo JW_ParentContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.JW_ParentContainerMode); }
		}

		#endregion

		#region JW_ParentBillOfLading

		public ZString BillOfLadingWithSuppression
		{
			get
			{
				if (Parent == null)
				{
					return ZString.Empty;
				}

				return Parent is CommonConsol
							? Suppression.GetValue(TransportSupporter.BillOfLading, this, SuppressFields.MasterBill, null)
							: TransportSupporter.BillOfLading;
			}
		}

		public ZPropertyInfo BillOfLadingWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.BillOfLadingWithSuppression); }
		}

		public ZString JW_ParentBillOfLading
		{
			get { return (TransportSupporter == null) ? ZString.Empty : TransportSupporter.BillOfLading; }
		}

		public ZPropertyInfo JW_ParentBillOfLadingInfo
		{
			get { return GetZPropertyInfo(Schema.JW_ParentBillOfLading); }
		}

		#endregion

		#region JW_ParentConsignmentRef

		public ZString JW_ParentConsignmentRef
		{
			get { return TransportSupporter == null ? ZString.Empty : TransportSupporter.ConsignmentRef; }
		}

		public ZPropertyInfo JW_ParentConsignmentRefInfo
		{
			get { return GetZPropertyInfo(Schema.JW_ParentConsignmentRef); }
		}

		#endregion

		#region JW_ParentDescription

		public ZString JW_ParentDescription
		{
			get { return (TransportSupporter == null) ? ZString.Empty : TransportSupporter.Description; }
		}

		public ZPropertyInfo JW_ParentDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JW_ParentDescription); }
		}

		#endregion

		#region JW_VesselFieldType

		public ZString JW_VesselFieldType
		{
			get
			{
				return (JW_TransportMode == Constants.TransportModes.Sea || JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport) ?
					nameof(FieldType.TextCodeFindBox) :
					nameof(FieldType.Text);
			}
		}

		public ZPropertyInfo JW_VesselFieldTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JW_VesselFieldType); }
		}

		#endregion

		#region JW_JX_JV_RegistrationNo

		[MaxLength(JobVoyage.Schema.JV_RegistrationNoMaxLength)]
		public ZString JW_JX_JV_RegistrationNo
		{
			get
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
				}
				return fJW_JX_JV_RegistrationNo;
			}
			set
			{
				if (value != fJW_JX_JV_RegistrationNo)
				{
					CheckMaximumLength(JW_JX_JV_RegistrationNoInfo, value);

					if (JW_IsLinked)
					{
						SailingManager.NotifyRead();

						fJW_JX_JV_RegistrationNo = value;
						HasChanges = true;

						if (Voyage != null && !(IsAir && JW_IsCharter))
						{
							Voyage.JV_RegistrationNo = value;
						}
						else
						{
							SailingManager.RegistrationDirty = true;
							SailingManager.NotifyRead();
						}
					}
					else
					{
						fJW_JX_JV_RegistrationNo = value;
						HasChanges = true;
					}

					JW_JX_JV_RegistrationNoInfo.RefreshBinding();
					if (Voyage != null)
					{
						Voyage.JV_IsCargoOnlyInfo.RefreshBinding();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_JX_JV_RegistrationNo();
					}
				}
			}
		}

		ZString fJW_JX_JV_RegistrationNo;

		public ZPropertyInfo JW_JX_JV_RegistrationNoInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_JV_RegistrationNo); }
		}

		#endregion

		#endregion

		#region JW_JX_JV_VoyageType

		[ReadOnly(true)]
		public ZString JW_JX_JV_VoyageType
		{
			get
			{
				if (IsSea
					&& JW_IsLinked
					&& Sailing != null
					&& !Sailing.IsDeleted
					&& Sailing.Voyage != null
					&& !Sailing.Voyage.IsDeleted)
				{
					return Sailing.Voyage.JV_VoyageType;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo JW_JX_JV_VoyageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_JV_VoyageType); }
		}

		#endregion

		#region JW_TerminalReceivalCommences

		ZDateTime PreviousJW_TerminalReceivalCommences { get; set; }

		public ZDateTime JW_TerminalReceivalCommencesForBinding
		{
			get => JW_TerminalReceivalCommences;
			set => JW_TerminalReceivalCommences = value;
		}

		public ZPropertyInfo JW_TerminalReceivalCommencesForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_TerminalReceivalCommencesForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_ReceivalCommencesInfo : JW_TerminalReceivalCommencesInfo);

		public override ZDateTime JW_TerminalReceivalCommences
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				var result = JW_TerminalReceivalCommencesCore;

				if (!JW_TerminalReceivalCommencesOriginalValue.HasValue)
				{
					JW_TerminalReceivalCommencesOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_TerminalReceivalCommencesCore;
				if (value != previousValue)
				{
					JW_TerminalReceivalCommencesCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalReceivalCommences();
					}
				}
			}
		}

		public ZDateTime? JW_TerminalReceivalCommencesOriginalValue { get; private set; }

		void UpdateJW_TerminalReceivalCommencesOriginalValue()
		{
			if (JW_TerminalReceivalCommencesOriginalValue.HasValue)
			{
				JW_TerminalReceivalCommencesOriginalValue = JW_TerminalReceivalCommences;
			}
		}

		public ZBool JW_TerminalReceivalCommencesHasChanges()
		{
			return IsInDatabase && JW_TerminalReceivalCommencesOriginalValue.HasValue && JW_TerminalReceivalCommencesOriginalValue.Value != JW_TerminalReceivalCommences;
		}

		public override ZPropertyInfo JW_TerminalReceivalCommencesInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_TerminalReceivalCommencesInfo);

		ZDateTime JW_TerminalReceivalCommencesCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_TerminalReceivalCommences;
				}
				else
				{
					result = base.JW_TerminalReceivalCommences;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_ReceivalCommences = value;
					}

					HasChanges = true;
					fJW_TerminalReceivalCommences = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalReceivalCommences();
					}

					JW_TerminalReceivalCommencesInfo.RefreshBinding();
				}
				else
				{
					base.JW_TerminalReceivalCommences = value;
				}
			}
		}
		ZDateTime fJW_TerminalReceivalCommences;

		#endregion

		#region JW_DepotReceivalCommences

		ZDateTime PreviousJW_DepotReceivalCommences { get; set; }

		public ZDateTime JW_DepotReceivalCommencesForBinding
		{
			get => JW_DepotReceivalCommences;
			set => JW_DepotReceivalCommences = value;
		}

		public ZPropertyInfo JW_DepotReceivalCommencesForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DepotReceivalCommencesForBinding, p => JW_IsLinked && Sailing != null ? Sailing.JX_DepotReceivalCommencesInfo : JW_DepotReceivalCommencesInfo);

		public override ZDateTime JW_DepotReceivalCommences
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				var result = JW_DepotReceivalCommencesCore;

				if (!JW_DepotReceivalCommencesOriginalValue.HasValue)
				{
					JW_DepotReceivalCommencesOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_DepotReceivalCommencesCore;
				if (value != previousValue)
				{
					JW_DepotReceivalCommencesCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotReceivalCommences();
					}
				}
			}
		}

		public override ZPropertyInfo JW_DepotReceivalCommencesInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DepotReceivalCommencesInfo);

		public ZDateTime? JW_DepotReceivalCommencesOriginalValue { get; private set; }

		void UpdateJW_DepotReceivalCommencesOriginalValue()
		{
			if (JW_DepotReceivalCommencesOriginalValue.HasValue)
			{
				JW_DepotReceivalCommencesOriginalValue = JW_DepotReceivalCommences;
			}
		}

		public ZBool JW_DepotReceivalCommencesHasChanges()
		{
			return IsInDatabase && JW_DepotReceivalCommencesOriginalValue.HasValue && JW_DepotReceivalCommencesOriginalValue.Value != JW_DepotReceivalCommences;
		}

		ZDateTime JW_DepotReceivalCommencesCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DepotReceivalCommences;
				}
				else
				{
					result = base.JW_DepotReceivalCommences;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing != null)
					{
						Sailing.JX_DepotReceivalCommences = value;
					}

					HasChanges = true;
					fJW_DepotReceivalCommences = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotReceivalCommences();
					}

					JW_DepotReceivalCommencesInfo.RefreshBinding();
				}
				else
				{
					base.JW_DepotReceivalCommences = value;
				}
			}
		}
		ZDateTime fJW_DepotReceivalCommences;

		#endregion

		#region JW_TerminalCutOff

		ZDateTime PreviousJW_TerminalCutOff { get; set; }

		public ZDateTime JW_TerminalCutOffForBinding
		{
			get => JW_TerminalCutOff;
			set => JW_TerminalCutOff = value;
		}

		public ZPropertyInfo JW_TerminalCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_TerminalCutOffForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_CutOffInfo : JW_TerminalCutOffInfo);

		public override ZDateTime JW_TerminalCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				var result = JW_TerminalCutOffCore;

				if (!JW_TerminalCutOffOriginalValue.HasValue)
				{
					JW_TerminalCutOffOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_TerminalCutOffCore;
				if (value != previousValue)
				{
					JW_TerminalCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_TerminalCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_TerminalCutOffInfo);

		public ZDateTime? JW_TerminalCutOffOriginalValue { get; private set; }

		void UpdateJW_TerminalCutOffOriginalValue()
		{
			if (JW_TerminalCutOffOriginalValue.HasValue)
			{
				JW_TerminalCutOffOriginalValue = JW_TerminalCutOff;
			}
		}

		public ZBool JW_TerminalCutOffHasChanges()
		{
			return IsInDatabase && JW_TerminalCutOffOriginalValue.HasValue && JW_TerminalCutOffOriginalValue.Value != JW_TerminalCutOff;
		}

		ZDateTime JW_TerminalCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_TerminalCutOff;
				}
				else
				{
					result = base.JW_TerminalCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_CutOff = value;
					}

					HasChanges = true;
					fJW_TerminalCutOff = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalCutOff();
					}

					JW_TerminalCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_TerminalCutOff = value;
				}
			}
		}
		ZDateTime fJW_TerminalCutOff;

		#endregion

		#region JW_DepotCutOff

		ZDateTime PreviousJW_DepotCutOff { get; set; }

		public ZDateTime JW_DepotCutOffForBinding
		{
			get => JW_DepotCutOff;
			set => JW_DepotCutOff = value;
		}

		public ZPropertyInfo JW_DepotCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DepotCutOffForBinding, p => JW_IsLinked && Sailing != null ? Sailing.JX_DepotCutOffInfo : JW_DepotCutOffInfo);

		public override ZDateTime JW_DepotCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				var result = JW_DepotCutOffCore;

				if (!JW_DepotCutOffOriginalValue.HasValue)
				{
					JW_DepotCutOffOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_DepotCutOffCore;
				if (value != previousValue)
				{
					JW_DepotCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_DepotCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DepotCutOffInfo);

		public ZDateTime? JW_DepotCutOffOriginalValue { get; private set; }

		void UpdateJW_DepotCutOffOriginalValue()
		{
			if (JW_DepotCutOffOriginalValue.HasValue)
			{
				JW_DepotCutOffOriginalValue = JW_DepotCutOff;
			}
		}

		public ZBool JW_DepotCutOffHasChanges()
		{
			return IsInDatabase && JW_DepotCutOffOriginalValue.HasValue && JW_DepotCutOffOriginalValue.Value != JW_DepotCutOff;
		}

		ZDateTime JW_DepotCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DepotCutOff;
				}
				else
				{
					result = base.JW_DepotCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing != null)
					{
						Sailing.JX_DepotCutOff = value;
					}

					HasChanges = true;
					fJW_DepotCutOff = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotCutOff();
					}

					JW_DepotCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_DepotCutOff = value;
				}
			}
		}
		ZDateTime fJW_DepotCutOff;

		#endregion

		#region JW_DocumentaryCutOff

		ZDateTime PreviousJW_DocumentaryCutOff { get; set; }

		public ZDateTime JW_DocumentaryCutOffForBinding
		{
			get => JW_DocumentaryCutOff;
			set => JW_DocumentaryCutOff = value;
		}

		public ZPropertyInfo JW_DocumentaryCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DocumentaryCutOffForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_DocumentaryCutoffInfo : JW_DocumentaryCutOffInfo);

		public override ZDateTime JW_DocumentaryCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				var result = JW_DocumentaryCutOffCore;

				if (!JW_DocumentaryCutOffOriginalValue.HasValue)
				{
					JW_DocumentaryCutOffOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_DocumentaryCutOffCore;
				if (value != previousValue)
				{
					JW_DocumentaryCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DocumentaryCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_DocumentaryCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DocumentaryCutOffInfo);

		public ZDateTime? JW_DocumentaryCutOffOriginalValue { get; private set; }

		void UpdateJW_DocumentaryCutOffOriginalValue()
		{
			if (JW_DocumentaryCutOffOriginalValue.HasValue)
			{
				JW_DocumentaryCutOffOriginalValue = JW_DocumentaryCutOff;
			}
		}

		public ZBool JW_DocumentaryCutOffHasChanges()
		{
			return IsInDatabase && JW_DocumentaryCutOffOriginalValue.HasValue && JW_DocumentaryCutOffOriginalValue.Value != JW_DocumentaryCutOff;
		}

		ZDateTime JW_DocumentaryCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DocumentaryCutOff;
				}
				else
				{
					result = base.JW_DocumentaryCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_DocumentaryCutoff = value;
					}

					HasChanges = true;
					fJW_DocumentaryCutOff = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DocumentaryCutOff();
					}
					JW_DocumentaryCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_DocumentaryCutOff = value;
				}
			}
		}
		ZDateTime fJW_DocumentaryCutOff;

		#endregion

		#region JW_VGMCutOff

		ZDateTime PreviousJW_VGMCutOff { get; set; }

		public ZDateTime JW_VGMCutOffForBinding
		{
			get => JW_VGMCutOff;
			set => JW_VGMCutOff = value;
		}

		public ZPropertyInfo JW_VGMCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_VGMCutOffForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_VGMCutOffInfo : JW_VGMCutOffInfo);

		public override ZDateTime JW_VGMCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				var result = JW_VGMCutOffCore;

				if (!JW_VGMCutOffOriginalValue.HasValue)
				{
					JW_VGMCutOffOriginalValue = result;
				}

				return result;
			}
			set
			{
				var previousValue = JW_VGMCutOffCore;
				if (value != previousValue)
				{
					JW_VGMCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_VGMCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_VGMCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_VGMCutOffInfo);

		public ZDateTime? JW_VGMCutOffOriginalValue { get; private set; }

		void UpdateJW_VGMCutOffOriginalValue()
		{
			if (JW_VGMCutOffOriginalValue.HasValue)
			{
				JW_VGMCutOffOriginalValue = JW_VGMCutOff;
			}
		}

		public ZBool JW_VGMCutOffHasChanges()
		{
			return IsInDatabase && JW_VGMCutOffOriginalValue.HasValue && JW_VGMCutOffOriginalValue.Value != JW_VGMCutOff;
		}

		ZDateTime JW_VGMCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_VGMCutOff;
				}
				else
				{
					result = base.JW_VGMCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_VGMCutOff = value;
					}

					HasChanges = true;
					fJW_VGMCutOff = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_VGMCutOff();
					}
					JW_VGMCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_VGMCutOff = value;
				}
			}
		}
		ZDateTime fJW_VGMCutOff;

		#endregion

		#region JW_TerminalAvailabilityDate

		ZDateTime PreviousJW_TerminalAvailabilityDate { get; set; }

		public ZDateTime JW_TerminalAvailabilityDateForBinding
		{
			get => JW_TerminalAvailabilityDate;
			set => JW_TerminalAvailabilityDate = value;
		}

		public ZPropertyInfo JW_TerminalAvailabilityDateForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_TerminalAvailabilityDateForBinding, p => JW_IsLinked && Sailing?.Destination != null ? Sailing.Destination.JB_AvailabilityDateInfo : JW_TerminalAvailabilityDateInfo);

		public override ZDateTime JW_TerminalAvailabilityDate
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_TerminalAvailabilityDateCore;
			}
			set
			{
				var previousValue = JW_TerminalAvailabilityDateCore;
				if (value != previousValue)
				{
					JW_TerminalAvailabilityDateCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalAvailabilityDate();
					}

					TransportSupporterWithSchedule?.NotifyTerminalAvailabilityDateChanged(this, previousValue);
				}
			}
		}

		public override ZPropertyInfo JW_TerminalAvailabilityDateInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_TerminalAvailabilityDateInfo);

		ZDateTime JW_TerminalAvailabilityDateCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_TerminalAvailabilityDate;
				}
				else
				{
					result = base.JW_TerminalAvailabilityDate;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing?.Destination != null)
					{
						Sailing.Destination.JB_AvailabilityDate = value;
					}

					HasChanges = true;
					fJW_TerminalAvailabilityDate = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalAvailabilityDate();
					}
					JW_TerminalAvailabilityDateInfo.RefreshBinding();
				}
				else
				{
					base.JW_TerminalAvailabilityDate = value;
				}
			}
		}
		ZDateTime fJW_TerminalAvailabilityDate;

		#endregion

		#region JW_DepotAvailabilityDate

		ZDateTime PreviousJW_DepotAvailabilityDate { get; set; }

		public ZDateTime JW_DepotAvailabilityDateForBinding
		{
			get => JW_DepotAvailabilityDate;
			set => JW_DepotAvailabilityDate = value;
		}

		public ZPropertyInfo JW_DepotAvailabilityDateForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DepotAvailabilityDateForBinding, p => JW_IsLinked && Sailing != null ? Sailing.JX_DepotAvailabilityDateInfo : JW_DepotAvailabilityDateInfo);

		public override ZDateTime JW_DepotAvailabilityDate
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_DepotAvailabilityDateCore;
			}
			set
			{
				var previousValue = JW_DepotAvailabilityDateCore;
				if (value != previousValue)
				{
					JW_DepotAvailabilityDateCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotAvailabilityDate();
					}

					TransportSupporterWithSchedule?.NotifyDepotAvailabilityDateChanged(this, previousValue);
				}
			}
		}

		public override ZPropertyInfo JW_DepotAvailabilityDateInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DepotAvailabilityDateInfo);

		ZDateTime JW_DepotAvailabilityDateCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DepotAvailabilityDate;
				}
				else
				{
					result = base.JW_DepotAvailabilityDate;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing != null)
					{
						Sailing.JX_DepotAvailabilityDate = value;
					}

					SailingManager.NotifyRead();

					HasChanges = true;
					fJW_DepotAvailabilityDate = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotAvailabilityDate();
					}
					JW_DepotAvailabilityDateInfo.RefreshBinding();
				}
				else
				{
					base.JW_DepotAvailabilityDate = value;
				}
			}
		}
		ZDateTime fJW_DepotAvailabilityDate;

		#endregion

		#region JW_OA_ArrivalLocation

		public ZGuid JW_OA_ArrivalLocationForBinding
		{
			get => JW_OA_ArrivalLocation;
			set => JW_OA_ArrivalLocation = value;
		}

		public ZPropertyInfo JW_OA_ArrivalLocationForBindingInfo => JW_OA_ArrivalLocationInfo;

		public ZAddress JW_OA_ArrivalLocationForBinding_ZAddress => JW_OA_ArrivalLocation_ZAddress;

		public override ZGuid JW_OA_ArrivalLocation
		{
			[DebuggerStepThrough]
			get { return JW_OA_ArrivalLocationCore; }
			set
			{
				var previousValue = JW_OA_ArrivalLocation;
				if (previousValue != value)
				{
					JW_OA_ArrivalLocationCore = value;

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyArrivalLocationChanged(this, previousValue);
					}
				}
			}
		}

		ZGuid JW_OA_ArrivalLocationCore
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					return fJW_OA_ArrivalLocation;
				}

				return base.JW_OA_ArrivalLocation;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing != null && Sailing.Destination != null)
					{
						Sailing.Destination.JB_OA_ArrivalCTOAddress = value;
					}

					fJW_OA_ArrivalLocation = value;
					HasChanges = true;

					JW_OA_ArrivalLocationInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_OA_ArrivalLocation();
					}
				}
				else
				{
					base.JW_OA_ArrivalLocation = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_RL_NKDiscPort();
				}
			}
		}

		public override ZPropertyInfo JW_OA_ArrivalLocationInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_OA_ArrivalLocationInfo);

		ZGuid fJW_OA_ArrivalLocation;

		public ZGuid PreviousJW_OA_ArrivalLocation { get; private set; }

		#endregion

		#region JW_TerminalStorageDate

		ZDateTime PreviousJW_TerminalStorageDate { get; set; }

		public ZDateTime JW_TerminalStorageDateForBinding
		{
			get => JW_TerminalStorageDate;
			set => JW_TerminalStorageDate = value;
		}

		public ZPropertyInfo JW_TerminalStorageDateForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_TerminalStorageDateForBinding, p => JW_IsLinked && Sailing?.Destination != null ? Sailing.Destination.JB_StorageDateInfo : JW_TerminalStorageDateInfo);

		public override ZDateTime JW_TerminalStorageDate
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_TerminalStorageDateCore;
			}
			set
			{
				var previousValue = JW_TerminalStorageDateCore;
				if (value != previousValue)
				{
					JW_TerminalStorageDateCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalStorageDate();
					}

					TransportSupporterWithSchedule?.NotifyTerminalStorageDateChanged(this, previousValue);
				}
			}
		}

		public override ZPropertyInfo JW_TerminalStorageDateInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_TerminalStorageDateInfo);

		ZDateTime JW_TerminalStorageDateCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_TerminalStorageDate;
				}
				else
				{
					result = base.JW_TerminalStorageDate;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing?.Destination != null)
					{
						Sailing.Destination.JB_StorageDate = value;
					}

					SailingManager.NotifyRead();

					HasChanges = true;
					fJW_TerminalStorageDate = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_TerminalStorageDate();
					}
					JW_TerminalStorageDateInfo.RefreshBinding();
				}
				else
				{
					base.JW_TerminalStorageDate = value;
				}
			}
		}
		ZDateTime fJW_TerminalStorageDate;

		#endregion

		#region JW_DepotStorageDate

		ZDateTime PreviousJW_DepotStorageDate { get; set; }

		public ZDateTime JW_DepotStorageDateForBinding
		{
			get => JW_DepotStorageDate;
			set => JW_DepotStorageDate = value;
		}

		public ZPropertyInfo JW_DepotStorageDateForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DepotStorageDateForBinding, p => JW_IsLinked && Sailing != null ? Sailing.JX_DepotStorageDateInfo : JW_DepotStorageDateInfo);

		public override ZDateTime JW_DepotStorageDate
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_DepotStorageDateCore;
			}
			set
			{
				var previousValue = JW_DepotStorageDateCore;
				if (value != previousValue)
				{
					JW_DepotStorageDateCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotStorageDate();
					}

					TransportSupporterWithSchedule?.NotifyDepotStorageDateChanged(this, previousValue);
				}
			}
		}

		public override ZPropertyInfo JW_DepotStorageDateInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DepotStorageDateInfo);

		ZDateTime JW_DepotStorageDateCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DepotStorageDate;
				}
				else
				{
					result = base.JW_DepotStorageDate;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing != null)
					{
						Sailing.JX_DepotStorageDate = value;
					}

					SailingManager.NotifyRead();

					HasChanges = true;
					fJW_DepotStorageDate = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DepotStorageDate();
					}

					JW_DepotStorageDateInfo.RefreshBinding();
				}
				else
				{
					base.JW_DepotStorageDate = value;
				}
			}
		}
		ZDateTime fJW_DepotStorageDate;

		#endregion

		#region JW_ServiceString

		ZString PreviousJW_ServiceString { get; set; }

		public ZString JW_ServiceStringForBinding
		{
			get => JW_ServiceString;
			set => JW_ServiceString = value;
		}

		public ZPropertyInfo JW_ServiceStringForBindingInfo => GetSailingPropertyIfLinked(Schema.JW_ServiceStringForBinding, Sailing?.JX_ServiceStringInfo, JW_ServiceStringInfo);

		ZPropertyInfo GetSailingPropertyIfLinked(string schemaColumn, ZPropertyInfo sailingProperty, ZPropertyInfo transportProperty)
		{
			return GetWrappedZPropertyInfo(schemaColumn, p => JW_IsLinked && Sailing != null ? sailingProperty : transportProperty);
		}

		public override ZString JW_ServiceString
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_ServiceStringCore;
			}
			set
			{
				var previousValue = JW_ServiceStringCore;
				if (value != previousValue)
				{
					JW_ServiceStringCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ServiceString();
					}
				}
			}
		}

		public override ZPropertyInfo JW_ServiceStringInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ServiceStringInfo);

		ZString JW_ServiceStringCore
		{
			get
			{
				ZString result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ServiceString;
				}
				else
				{
					result = base.JW_ServiceString;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_ServiceStringInfo, value);
					if (Sailing != null)
					{
						Sailing.JX_ServiceString = value;
					}

					SailingManager.NotifyRead();

					HasChanges = true;
					fJW_ServiceString = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ServiceString();
					}

					JW_ServiceStringInfo.RefreshBinding();
				}
				else
				{
					base.JW_ServiceString = value;
				}
			}
		}
		ZString fJW_ServiceString;

		#endregion

		#region JW_ArrivalPortRouteId

		ZString PreviousJW_ArrivalPortRouteId { get; set; }

		public ZString JW_ArrivalPortRouteIdForBinding
		{
			get => JW_ArrivalPortRouteId;
			set => JW_ArrivalPortRouteId = value;
		}

		public ZPropertyInfo JW_ArrivalPortRouteIdForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ArrivalPortRouteIdForBinding, p => JW_IsLinked && Sailing != null ? Sailing.JX_ArrivalPortRouteIdInfo : JW_ArrivalPortRouteIdInfo);

		public override ZString JW_ArrivalPortRouteId
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_ArrivalPortRouteIdCore;
			}
			set
			{
				var previousValue = JW_ArrivalPortRouteIdCore;
				if (value != previousValue)
				{
					JW_ArrivalPortRouteIdCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ArrivalPortRouteId();
					}
				}
			}
		}

		public override ZPropertyInfo JW_ArrivalPortRouteIdInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ArrivalPortRouteIdInfo);

		ZString JW_ArrivalPortRouteIdCore
		{
			get
			{
				ZString result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ArrivalPortRouteId;
				}
				else
				{
					result = base.JW_ArrivalPortRouteId;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_ArrivalPortRouteIdInfo, value);
					if (Sailing != null)
					{
						Sailing.JX_ArrivalPortRouteId = value;
					}

					SailingManager.NotifyRead();

					HasChanges = true;
					fJW_ArrivalPortRouteId = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ArrivalPortRouteId();
					}

					JW_ArrivalPortRouteIdInfo.RefreshBinding();
				}
				else
				{
					base.JW_ArrivalPortRouteId = value;
				}
			}
		}
		ZString fJW_ArrivalPortRouteId;

		#endregion

		#region JW_DeparturePortRouteId

		ZString PreviousJW_DeparturePortRouteId { get; set; }

		public ZString JW_DeparturePortRouteIdForBinding
		{
			get => JW_DeparturePortRouteId;
			set => JW_DeparturePortRouteId = value;
		}

		public ZPropertyInfo JW_DeparturePortRouteIdForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DeparturePortRouteIdForBinding, p => JW_IsLinked && Sailing != null ? Sailing.JX_DeparturePortRouteIdInfo : JW_DeparturePortRouteIdInfo);

		public override ZString JW_DeparturePortRouteId
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_DeparturePortRouteIdCore;
			}
			set
			{
				var previousValue = JW_DeparturePortRouteIdCore;
				if (value != previousValue)
				{
					JW_DeparturePortRouteIdCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DeparturePortRouteId();
					}
				}
			}
		}

		public override ZPropertyInfo JW_DeparturePortRouteIdInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DeparturePortRouteIdInfo);

		ZString JW_DeparturePortRouteIdCore
		{
			get
			{
				ZString result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DeparturePortRouteId;
				}
				else
				{
					result = base.JW_DeparturePortRouteId;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					CheckMaximumLength(JW_DeparturePortRouteIdInfo, value);
					if (Sailing != null)
					{
						Sailing.JX_DeparturePortRouteId = value;
					}

					SailingManager.NotifyRead();

					HasChanges = true;
					fJW_DeparturePortRouteId = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DeparturePortRouteId();
					}

					JW_DeparturePortRouteIdInfo.RefreshBinding();
				}
				else
				{
					base.JW_DeparturePortRouteId = value;
				}
			}
		}
		ZString fJW_DeparturePortRouteId;

		#endregion

		#region JW_OA_DepartureLocation

		public ZGuid JW_OA_DepartureLocationForBinding
		{
			get => JW_OA_DepartureLocation;
			set => JW_OA_DepartureLocation = value;
		}

		public ZPropertyInfo JW_OA_DepartureLocationForBindingInfo => JW_OA_DepartureLocationInfo;

		public ZAddress JW_OA_DepartureLocationForBinding_ZAddress => JW_OA_DepartureLocation_ZAddress;

		public override ZGuid JW_OA_DepartureLocation
		{
			[DebuggerStepThrough]
			get { return JW_OA_DepartureLocationCore; }
			set
			{
				var previousValue = JW_OA_DepartureLocation;
				if (previousValue != value)
				{
					JW_OA_DepartureLocationCore = value;

					if (TransportSupporterWithSchedule != null)
					{
						TransportSupporterWithSchedule.NotifyDepartureLocationChanged(this, previousValue);
					}
				}
			}
		}

		ZGuid JW_OA_DepartureLocationCore
		{
			get
			{
				StoreOriginalValuesForProxiedFields();

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					return fJW_OA_DepartureLocation;
				}

				return base.JW_OA_DepartureLocation;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing != null && Sailing.Origin != null)
					{
						Sailing.Origin.JA_OA_DepartureCTOAddress = value;
					}

					fJW_OA_DepartureLocation = value;
					HasChanges = true;

					JW_OA_DepartureLocationInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_OA_DepartureLocation();
					}
				}
				else
				{
					base.JW_OA_DepartureLocation = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_RL_NKLoadPort();
				}
			}
		}

		public override ZPropertyInfo JW_OA_DepartureLocationInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_OA_DepartureLocationInfo);

		ZGuid fJW_OA_DepartureLocation;

		public ZGuid PreviousJW_OA_DepartureLocation { get; private set; }

		#endregion

		#region JW_JX_DepartOrArriveReference

		public ZString JW_JX_DepartOrArriveReference
		{
			get
			{
				var result = "";

				var sailingBO = Sailing;
				if (sailingBO != null)
				{
					if (ImportExportHelper.IsBranchCountry(JW_RL_NKLoadPort))
					{
						result = sailingBO.Origin.JA_DepartReference;
					}
					else if (ImportExportHelper.IsBranchCountry(JW_RL_NKDiscPort))
					{
						result = sailingBO.Destination.JB_ArrivalReference;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JW_JX_DepartOrArriveReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_DepartOrArriveReference); }
		}

		#endregion

		#region JW_JX_DepartOrArriveBerth

		public ZString JW_JX_DepartOrArriveBerth
		{
			get
			{
				var result = "";
				var sailingBO = Sailing;
				if (sailingBO != null)
				{
					if (ImportExportHelper.IsBranchCountry(JW_RL_NKLoadPort))
					{
						result = sailingBO.Origin.JA_Berth;
					}
					else if (ImportExportHelper.IsBranchCountry(JW_RL_NKDiscPort))
					{
						result = sailingBO.Destination.JB_Berth;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JW_JX_DepartOrArriveBerthInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_DepartOrArriveBerth); }
		}

		#endregion

		#region JW_JX_IsPublished

		public ZBool JW_JX_IsPublished
		{
			get
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
				}
				if (Sailing != null)
				{
					fJW_JX_IsPublished = Sailing.JX_IsPublished;
				}
				return fJW_JX_IsPublished;
			}
			set
			{
				if (value != fJW_JX_IsPublished)
				{
					if (Sailing != null)
					{
						Sailing.JX_IsPublished = value;
					}
					fJW_JX_IsPublished = value;

					HasChanges = true;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_JX_IsPublished();
					}

					JW_JX_IsPublishedInfo.RefreshBinding();
				}
			}
		}
		ZBool fJW_JX_IsPublished;

		public ZPropertyInfo JW_JX_IsPublishedInfo
		{
			get { return GetZPropertyInfo(Schema.JW_JX_IsPublished); }
		}

		#endregion

		#region JW_Calc_Status

		static string Pending
		{
			get { return Res.GetString("b2cfd53f-6df4-46b2-85e4-dd6c69656844", "Pending"); }
		}
		static string InTransit
		{
			get { return Res.GetString("1d0939c8-0846-43bd-9281-a3646a50e78f", "In Transit"); }
		}
		static string Delayed
		{
			get { return Res.GetString("0fa43029-d585-4ffb-9e40-5a887045f525", "Delayed"); }
		}
		static string Arrived
		{
			get { return Res.GetString("5e6ad5fa-4a51-493f-aeb8-662553d4451c", "Arrived"); }
		}

		public ZString JW_Calc_Status
		{
			get
			{
				ZString result = Pending;

				if (JW_ATA_UTC <= ZDateTime.UtcNow)
				{
					result = Arrived;
				}
				else if (JW_ETA_UTC <= ZDateTime.UtcNow)
				{
					result = Delayed;
				}
				else if (JW_ATD_UTC <= ZDateTime.UtcNow || (JW_ATD_UTC.IsEmpty && JW_ETD_UTC <= ZDateTime.UtcNow))
				{
					result = InTransit;
				}

				return result;
			}
		}

		#endregion

		#region VoyageFlightWithSuppression

		public ZString VoyageFlightWithSuppression
		{
			get { return Suppression.GetValue(JW_VoyageFlight, this, SuppressFields.FlightNumber, null); }
		}

		#endregion

		#region ETDWithSuppression

		public ZDateTime ETDWithSuppression
		{
			get { return Suppression.GetValue(JW_ETD, this, SuppressFields.ETD, null); }
		}

		#endregion

		#region ETAWithSuppression

		public ZDateTime ETAWithSuppression
		{
			get { return Suppression.GetValue(JW_ETA, this, SuppressFields.ETA, null); }
		}

		#endregion

		#region CarrierWithSuppression

		public ZString CarrierWithSuppression
		{
			get
			{
				if (Carrier == null)
				{
					return ZString.Empty;
				}
				if (!string.IsNullOrEmpty(Carrier.OH_Code))
				{
					return Suppression.GetValue(Carrier.OH_Code, this, SuppressFields.Carrier, null);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region ATDWithSuppression

		public ZDateTime ATDWithSuppression
		{
			get { return Suppression.GetValue(JW_ATD, this, SuppressFields.ATD, null); }
		}

		#endregion

		#region ATAWithSuppression

		public ZDateTime ATAWithSuppression
		{
			get { return Suppression.GetValue(JW_ATA, this, SuppressFields.ATA, null); }
		}

		#endregion

		#region Is Air / Sea / Rail / Road

		public bool IsAir
		{
			get { return JW_TransportMode == Constants.TransportModes.Air; }
		}

		public bool IsSea
		{
			get { return JW_TransportMode == Constants.TransportModes.Sea; }
		}

		public bool IsRail
		{
			get { return JW_TransportMode == Constants.TransportModes.Rail; }
		}

		public bool IsRoad
		{
			get { return JW_TransportMode == Constants.TransportModes.Road; }
		}

		public bool IsRailOrRoadOrInlandWaterway
		{
			get
			{
				return JW_TransportMode == Constants.TransportModes.Rail || JW_TransportMode == Constants.TransportModes.Road || JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport;
			}
		}

		#endregion

		#region IsDomestic

		protected virtual void SetIsDomestic()
		{
			var direction = ImportExportHelper.GetJobDirection(JW_RL_NKLoadPort, JW_RL_NKDiscPort);

			if (direction != Directions.Unknown)
			{
				if (ParentType != null)
				{
					IsDomestic = direction == Directions.Domestic;
				}
				else
				{
					isDomestic = direction == Directions.Domestic;
				}
			}
		}

		public ZBool IsDomestic
		{
			get
			{
				if (isDomestic == null)
				{
					using (SuspendSettingHasChanges())
					{
						SetIsDomestic();
					}
				}

				return isDomestic == null ? ZBool.False : (ZBool)isDomestic;
			}
			set
			{
				if (!isSettingDomestic)
				{
					isSettingDomestic = true;
					try
					{
						SetNonPersistentPropertyValue(IsDomesticInfo, ref isDomestic, value);

						if (value)
						{
							if (JW_RL_NKLoadPort.IsEmpty)
							{
								JW_RL_NKLoadPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}

							if (JW_RL_NKDiscPort.IsEmpty)
							{
								JW_RL_NKDiscPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
							}
						}

						if (!IsValidationSuspended && !IsValidationSuspendedForCargoReport)
						{
							Validation.ValidateIsDomestic();
						}
					}
					finally
					{
						isSettingDomestic = false;
					}
				}
			}
		}
		ZBool? isDomestic;
		bool isSettingDomestic;

		public ZPropertyInfo IsDomesticInfo
		{
			get { return GetZPropertyInfo(nameof(IsDomestic)); }
		}

		public ZBool IsValidationSuspendedForCargoReport
		{
			get;
			set;
		}

		#endregion

		public bool IsFeeder { get; set; }

		public bool CreateDeniedBySecurity
		{
			get
			{
				SecurityCheckpoint checkpoint;

				return JW_IsLinked
					&& SailingManager.HasSufficientInformation
					&& Sailing == null
					&& (checkpoint = FreightUtilities.GetCreateScheduleFromJobSecurityCheckpoint(JW_TransportMode)) != null
					&& !checkpoint.IsAllowed;
			}
		}

		void SetTransportTypeBasedOnTransportMode()
		{
			ZString nextBestTransportType = GetNextBestTransportType();
			if (nextBestTransportType.IsEmpty || JW_TransportType_List.ContainsCode(nextBestTransportType))
			{
				using (GetValidationSuspender())
				{
					JW_TransportType = nextBestTransportType;
				}
			}
		}

		#region ReadOnly Properties

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		public ZInt RouteSetNumber
		{
			get
			{
				var transportParent = Parent as ITransportParent;
				if (transportParent != null)
				{
					var routingSupport = transportParent.Transports.Master as IRoutingSupport;
					if (routingSupport != null)
					{
						return routingSupport.TransportsIncludingRelated.RouteSets.GetRouteSetNumberForTransport(this);
					}
				}
				return 0;
			}
		}

		public ZPropertyInfo RouteSetNumberInfo
		{
			get { return GetZPropertyInfo(nameof(RouteSetNumber)); }
		}

		#endregion

		string deletedStackTrace;
		public override void Delete()
		{
			if (deletedStackTrace == null)
			{
				deletedStackTrace = new StackTrace().ToString();
			}
			else
			{
				deletedStackTrace += "\n" + new StackTrace().ToString();
			}

			FlightMonitoringSystemManager.UpdateFlightSubscriptionEventAfterTransportRemoved(this);

			base.Delete();
			ResetSailingManager();
		}

		protected override StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			string voyageEventStackTrace = fSailingManager != null && !string.IsNullOrEmpty(fSailingManager.voyageEventStackTrace)
				? fSailingManager.voyageEventStackTrace
				: (NoResString)"Voyage Hook/UnHook stack trace never collected.";

			var rowMessages = new StringBuilder()
				.AppendLine(HasWarnings ? (NoResString)"Warnings: " + string.Join((NoResString)"\r\n", RowWarnings) : (NoResString)"No warnings")
				.AppendLine(HasErrors ? (NoResString)"Errors: " + string.Join((NoResString)"\r\n", RowErrors) : (NoResString)"No errors")
				.AppendLine(HasMessageErrors ? (NoResString)"Message Errors: " + string.Join((NoResString)"\r\n", RowMessageErrors) : (NoResString)"No message errors")
				.AppendLine(HasNotifications() ? (NoResString)"Notifications: " + string.Join((NoResString)"\r\n", RowNotifications) : (NoResString)"No notifications");

			return base.BuildRowDeletedReport(columnName, ex, versionToUse, version, message)
				.AppendLine((NoResString)"Deletion Stack Trace: ").AppendLine(deletedStackTrace ?? (NoResString)"Deletion stack trace never collected.")
				.AppendLine((NoResString)"Hook/UnHook Voyage Stack Trace: ").AppendLine(voyageEventStackTrace)
				.AppendLine((NoResString)"Movement Leg Sort Debug Log: ").AppendLine(MovementLegSortDebugLog)
				.AppendLine((NoResString)"Transport Constructor Stack Trace: ").AppendLine(constructorLog)
				.AppendLine((NoResString)"Transport Row Messages: ").AppendLine(rowMessages.ToString())
				.AppendLine((NoResString)"Transport IsDeleted: ").AppendLine(IsDeleted ? (NoResString)"true" : (NoResString)"false")
				.AppendLine((NoResString)"Transport RowState: ").AppendLine(((INeedRow)this).Row.RowState.ToString())
				.AppendLine((NoResString)"Transport IsDataInRowAccessibleForDelete: ").AppendLine(ZDataUtils.IsDataInRowAccessible(((INeedRow)this).Row) ? (NoResString)"true" : (NoResString)"false")
				.AppendLine((NoResString)"Transport IsRowDeletedOrDetachedOrNull: ").AppendLine(IsRowDeletedOrDetachedOrNull ? (NoResString)"true" : (NoResString)"false")
				.AppendLine((NoResString)"Transport IsRowDeletedOrNull: ").AppendLine(IsRowDeletedOrNull ? (NoResString)"true" : (NoResString)"false")
				.AppendLine((NoResString)"Transport Property Values: ").AppendLine(GetPropertyValuesForDeletedRowReporting())
				.AppendLine((NoResString)"Row Deleted Stack Trace: ").AppendLine(transportInfoCollector.GetLastRowDeletedStackTrace(PK.ToGuid()));
		}

#if DEBUG
		internal
#endif
		string GetPropertyValuesForDeletedRowReporting()
		{
			if (!IsInDatabase)
			{
				return (NoResString)"Transport property values cannot be collected as IsInDataBase is false.";
			}

			var properties = new[]
			{
				JW_ATAInfo, JW_ATDInfo, JW_ETAInfo, JW_ETDInfo,
				JW_DepotCutOffInfo, JW_ServiceStringInfo,
				JW_DepotStorageDateInfo, JW_IsLinkedInfo,
				JW_ParentBillOfLadingInfo, JW_ParentConsignmentRefInfo,
				JW_ParentContainerModeInfo, JW_ParentGUIDInfo, JW_ParentTypeInfo,
				JW_StatusInfo, JW_STDInfo, JW_TransportModeInfo,
				JW_TransportTypeInfo, JW_VoyageFlightInfo, JW_TerminalStorageDateInfo,
				JW_TerminalReceivalCommencesInfo, JW_DepotReceivalCommencesInfo,
				JW_TerminalCutOffInfo, JW_DepotAvailabilityDateInfo, JW_JXInfo,
				JW_TerminalAvailabilityDateInfo, JW_RL_NKDiscPortInfo,
				JW_OA_ArrivalLocationInfo, JW_VesselInfo, JW_RL_NKLoadPortInfo,
				JW_OA_DepartureLocationInfo, JW_IsCharterInfo, JW_AircraftTypeInfo,
				JW_ArrivalPortRouteIdInfo, JW_DeparturePortRouteIdInfo,
			};

			var result = new StringBuilder();
			foreach (var property in properties)
			{
				result.Append(property.Name).Append(": ").AppendLine(property.OriginalValue.ToString());
			}

			return result.ToString();
		}

		internal ZString MovementLegSortDebugLog { get; set; }

		protected override void DeleteForDataRefresh()
		{
			if (deletedStackTrace == null)
			{
				deletedStackTrace = new StackTrace().ToString();
			}
			else
			{
				deletedStackTrace += "\n" + new StackTrace().ToString();
			}

			base.DeleteForDataRefresh();
			ResetSailingManager();
		}

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
			if (IsDeleted)
			{
				ResetSailingManager();
			}
		}

		void ResetSailingManager()
		{
			if (fSailingManager != null)
			{
				fSailingManager.ResetSailing();
				fSailingManager = null;
			}
		}

		void ReleaseSailing()
		{
			fSailingManager?.ResetSailing();
		}

		public bool IsFlightDateMatched(bool isCompareETA, ZDate flightDate)
		{
			var compareDate = isCompareETA ? JW_ETA.Date : JW_ETD.Date;

			return compareDate.IsValid && compareDate == flightDate;
		}

		#region SailingManager

		public bool SailingManagerHasSufficientInformation()
		{
			return fSailingManager?.HasSufficientInformation ?? false;
		}

		BaseSailingManager SailingManager
		{
			get
			{
				if (!JW_IsLinked)
				{
					ErrorReporter.ReportOnce("{B2665CCC-9598-4d6a-ACF9-AFAE980DA3B8}", "Trying to access the SailingManager for a non-linked Transport");
				}

				if (fSailingManager == null)
				{
					fSailingManager = BaseSailingManager.New(this);
				}
				return fSailingManager;
			}
		}

		BaseSailingManager fSailingManager;

		#endregion

		#region Distance Calculation

		public void SetCalculatedDistance(INotifications notifications)
		{
			if (TransportSupporter != null)
			{
				if (JW_TransportMode != Constants.TransportModes.Road)
				{
					notifications.Notify(new InfoNotification(Res.GetString("967b1d4f-20f0-4445-9b88-7fc002efa0fa", "Distance calculation functionality is only available for Road transport mode.")));
					return;
				}

				DistanceCalculationConfiguration distanceCalculationConfig = DistanceCalculationHelper.GetConfigurationFromRegistry();
				if (!JW_DistanceUnit.IsEmpty)
				{
					distanceCalculationConfig.UnitsForCalculation = (JW_DistanceUnit == Constants.Length.Miles) ? DistanceCalculationConstants.UnitsForCalculation.Miles : DistanceCalculationConstants.UnitsForCalculation.Kilometres;
				}
				else
				{
					distanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Default;
				}

				DistanceCalculationAddress originAddress = GetAddress(JW_RL_NKLoadPort, JW_OA_DepartureLocation);
				DistanceCalculationAddress destinationAddress = GetAddress(JW_RL_NKDiscPort, JW_OA_ArrivalLocation);

				if (JW_ParentType == Constants.TransportParentTypes.Shipment && JW_TransportType == Constants.TransportPlanningType.MainVessel)
				{
					CommonShipment shipment = Factory.Load<CommonShipment>(JW_ParentGUID);
					if (shipment.ServiceLevel != null && shipment.ServiceLevel.RS_IsDoorToDoor)
					{
						originAddress = DistanceCalculationHelper.GetAddressFromAddress(shipment.ConsignorPickupAddress);
						destinationAddress = DistanceCalculationHelper.GetAddressFromAddress(shipment.ConsigneeDeliveryAddress);
					}
				}

				var manager = new DistanceCalculationManager();

				DistanceCalculationResult result = manager.Calculate(TransportSupporter.DistanceCalculationCheckpoint, Guid.NewGuid(), distanceCalculationConfig, originAddress, destinationAddress);

				if (string.IsNullOrEmpty(result.StatusMessage))
				{
					ZString resultUnit = result.DistanceUnit == DistanceCalculationConstants.UnitsForCalculation.Miles ? Constants.Length.Miles : Constants.Length.Kilometres;

					if (JW_DistanceUnit.IsEmpty)
					{
						JW_DistanceUnit = DistanceCalculationRegistry.Instance.DefaultDistanceUnit.Value;
					}

					bool isConsumerUnitValid = Constants.Length.ContainsCode(JW_DistanceUnit);

					if (isConsumerUnitValid)
					{
						JW_Distance = Constants.Length.Convert((ZDecimal)result.Distance, resultUnit, JW_DistanceUnit);
					}
					else
					{
						JW_Distance = result.Distance;
						JW_DistanceUnit = resultUnit;
					}
				}
				else
				{
					notifications.Notify(new WarningNotification(result.StatusMessage));
				}
			}
		}

		DistanceCalculationAddress GetAddress(ZString portCode, ZGuid addressPK)
		{
			DistanceCalculationAddress result = new DistanceCalculationAddress();

			OrgAddress orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, addressPK));
			if (orgAddress != null)
			{
				result = DistanceCalculationHelper.GetAddressFromAddress(orgAddress);
			}
			else
			{
				RefUNLOCO port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, portCode));
				if (port != null)
				{
					result = DistanceCalculationHelper.GetAddressFromUNLOCO(port);
				}
			}

			return result;
		}

		#endregion

		#region Proxied Property Merge Implementation

		void SubscribeConcurrencyMergeHandlers()
		{
			if (JW_IsLinked && concurrencyMergeSubscribedSailingPk.IsEmpty)
			{
				concurrencyMergeSubscribedSailingPk = JW_JX;
				synchroniseConcurrencyMergedPropertiesHasBeenRun = false;

				foreach (var propertyInfo in GetProxiedPropertyInfos(concurrencyMergeSubscribedSailingPk))
				{
					if (propertyInfo != null)
					{
						propertyInfo.ConcurrencyMerged += SynchroniseConcurrencyMergedProperties;
					}
				}
			}
		}

		void UnsubscribeConcurrencyMergeHandlers()
		{
			if (!concurrencyMergeSubscribedSailingPk.IsEmpty)
			{
				foreach (var propertyInfo in GetProxiedPropertyInfos(concurrencyMergeSubscribedSailingPk))
				{
					if (propertyInfo != null)
					{
						propertyInfo.ConcurrencyMerged -= SynchroniseConcurrencyMergedProperties;
					}
				}

				concurrencyMergeSubscribedSailingPk = ZGuid.Empty;
			}
		}
#if DEBUG
		public delegate void DelegateForSynchroniseConcurrencyMergedProperties();
		public DelegateForSynchroniseConcurrencyMergedProperties BeforeSynchroniseConcurrencyMergedPropertiesForTest { get; set; }
#endif

		void SynchroniseConcurrencyMergedProperties(object sender, EventArgs eventArgs)
		{
#if DEBUG
			BeforeSynchroniseConcurrencyMergedPropertiesForTest?.Invoke();
#endif
			if (IsDeleted)
			{
				ResetSailingManager();
				return;
			}

			if (concurrencyMergeSubscribedSailingPk == JW_JX && !synchroniseConcurrencyMergedPropertiesHasBeenRun)
			{
				MergeProxyFieldsIfRequiredByConcurrency();
				StoreOriginalValuesForProxiedFields(true);

				synchroniseConcurrencyMergedPropertiesHasBeenRun = true;
			}
		}

		ZGuid concurrencyMergeSubscribedSailingPk;
		bool synchroniseConcurrencyMergedPropertiesHasBeenRun;

		void MergeProxyFieldsIfRequiredByConcurrency()
		{
			if (JW_IsLinked)
			{
				#region MergeProxyField

				void MergeProxyField(ZPropertyInfo proxyInfo, ZPropertyInfo sourceInfo, IZType proxyValue, IZType originalValue)
				{
					if (!proxyValue.Equals(sourceInfo.Value) && originalValue.Equals(sourceInfo.Value))
					{
						sourceInfo.Value = proxyValue;
					}
				}

				#endregion

				if (Sailing != null && Sailing.IsInDatabase && !Sailing.IsDeleted)
				{
					Sailing.ReloadSafe();

					MergeProxyField(JW_DepotReceivalCommencesInfo, Sailing.JX_DepotReceivalCommencesInfo, fJW_DepotReceivalCommences, PreviousJW_DepotReceivalCommences);
					MergeProxyField(JW_DepotCutOffInfo, Sailing.JX_DepotCutOffInfo, fJW_DepotCutOff, PreviousJW_DepotCutOff);
					MergeProxyField(JW_DepotAvailabilityDateInfo, Sailing.JX_DepotAvailabilityDateInfo, fJW_DepotAvailabilityDate, PreviousJW_DepotAvailabilityDate);
					MergeProxyField(JW_DepotStorageDateInfo, Sailing.JX_DepotStorageDateInfo, fJW_DepotStorageDate, PreviousJW_DepotStorageDate);
					MergeProxyField(JW_ServiceStringInfo, Sailing.JX_ServiceStringInfo, fJW_ServiceString, PreviousJW_ServiceString);
					MergeProxyField(JW_ArrivalPortRouteIdInfo, Sailing.JX_ArrivalPortRouteIdInfo, fJW_ArrivalPortRouteId, PreviousJW_ArrivalPortRouteId);
					MergeProxyField(JW_DeparturePortRouteIdInfo, Sailing.JX_DeparturePortRouteIdInfo, fJW_DeparturePortRouteId, PreviousJW_DeparturePortRouteId);
				}

				if (Sailing?.Origin != null && Sailing.Origin.IsInDatabase && !Sailing.Origin.IsDeleted)
				{
					Sailing.Origin.ReloadSafe();

					MergeProxyField(JW_STDInfo, Sailing.Origin.JA_S_DEPInfo, fJW_STD, PreviousJW_STD);
					MergeProxyField(JW_ETDInfo, Sailing.Origin.JA_E_DEPInfo, fJW_ETD, PreviousJW_ETD);
					MergeProxyField(JW_ATDInfo, Sailing.Origin.JA_A_DEPInfo, fJW_ATD, PreviousJW_ATD);
					MergeProxyField(JW_RL_NKLoadPortInfo, Sailing.Origin.JA_RL_NKPortOfLoadingInfo, fJW_RL_NKLoadPort, PreviousJW_RL_NKLoadPort);
					MergeProxyField(JW_OA_DepartureLocationInfo, Sailing.Origin.JA_OA_DepartureCTOAddressInfo, fJW_OA_DepartureLocation, PreviousJW_OA_DepartureLocation);
					MergeProxyField(JW_TerminalReceivalCommencesInfo, Sailing.Origin.JA_ReceivalCommencesInfo, fJW_TerminalReceivalCommences, PreviousJW_TerminalReceivalCommences);
					MergeProxyField(JW_TerminalCutOffInfo, Sailing.Origin.JA_CutOffInfo, fJW_TerminalCutOff, PreviousJW_TerminalCutOff);
					MergeProxyField(JW_DocumentaryCutOffInfo, Sailing.Origin.JA_DocumentaryCutoffInfo, fJW_DocumentaryCutOff, PreviousJW_DocumentaryCutOff);
					MergeProxyField(JW_VGMCutOffInfo, Sailing.Origin.JA_VGMCutOffInfo, fJW_VGMCutOff, PreviousJW_VGMCutOff);
					MergeProxyField(JW_EmptyReceivalCommencesInfo, Sailing.Origin.JA_EmptyReceivalCommencesInfo, fJW_EmptyReceivalCommences, PreviousJW_EmptyReceivalCommences);
					MergeProxyField(JW_EmptyCutOffInfo, Sailing.Origin.JA_EmptyCutOffInfo, fJW_EmptyCutOff, PreviousJW_EmptyCutOff);
					MergeProxyField(JW_ReeferReceivalCommencesInfo, Sailing.Origin.JA_ReeferReceivalCommencesInfo, fJW_ReeferReceivalCommences, PreviousJW_ReeferReceivalCommences);
					MergeProxyField(JW_ReeferCutOffInfo, Sailing.Origin.JA_ReeferCutOffInfo, fJW_ReeferCutOff, PreviousJW_ReeferCutOff);
					MergeProxyField(JW_DGReceivalCommencesInfo, Sailing.Origin.JA_DGReceivalCommencesInfo, fJW_DGReceivalCommences, PreviousJW_DGReceivalCommences);
					MergeProxyField(JW_DGCutOffInfo, Sailing.Origin.JA_DGCutOffInfo, fJW_DGCutOff, PreviousJW_DGCutOff);
				}

				if (Sailing?.Destination != null && Sailing.Destination.IsInDatabase && !Sailing.Destination.IsDeleted)
				{
					Sailing.Destination.ReloadSafe();

					MergeProxyField(JW_STAInfo, Sailing.Destination.JB_S_ARVInfo, fJW_STA, PreviousJW_STA);
					MergeProxyField(JW_ETAInfo, Sailing.Destination.JB_E_ARVInfo, fJW_ETA, PreviousJW_ETA);
					MergeProxyField(JW_ATAInfo, Sailing.Destination.JB_A_ARVInfo, fJW_ATA, PreviousJW_ATA);
					MergeProxyField(JW_RL_NKDiscPortInfo, Sailing.Destination.JB_RL_NKPortOfDischargeInfo, fJW_RL_NKDiscPort, PreviousJW_RL_NKDiscPort);
					MergeProxyField(JW_OA_ArrivalLocationInfo, Sailing.Destination.JB_OA_ArrivalCTOAddressInfo, fJW_OA_ArrivalLocation, PreviousJW_OA_ArrivalLocation);
					MergeProxyField(JW_TerminalAvailabilityDateInfo, Sailing.Destination.JB_AvailabilityDateInfo, fJW_TerminalAvailabilityDate, PreviousJW_TerminalAvailabilityDate);
					MergeProxyField(JW_TerminalStorageDateInfo, Sailing.Destination.JB_StorageDateInfo, fJW_TerminalStorageDate, PreviousJW_TerminalStorageDate);
				}

				if (Voyage != null && Voyage.IsInDatabase && !Voyage.IsDeleted)
				{
					Voyage.ReloadSafe();

					MergeProxyField(JW_VoyageFlightInfo, Voyage.JV_VoyageFlightInfo, fJW_VoyageFlight, PreviousJW_VoyageFlight);
					MergeProxyField(JW_VesselInfo, Voyage.JV_RV_NKVesselInfo, fJW_Vessel, PreviousJW_Vessel);
					MergeProxyField(JW_IsCharterInfo, Voyage.JV_IsCharteredInfo, fJW_IsCharter, PreviousJW_IsCharter);
					MergeProxyField(JW_IsCargoOnlyInfo, Voyage.JV_IsCargoOnlyInfo, fJW_IsCargoOnly, PreviousJW_IsCargoOnly);
					MergeProxyField(JW_AircraftTypeInfo, Voyage.JV_AircraftTypeInfo, fJW_AircraftType, PreviousJW_AircraftType);
				}

				SetAllScheduleFields(Sailing);
			}
		}

		IEnumerable<ZPropertyInfo> GetProxiedPropertyInfos(ZGuid sailingPK)
		{
			var sailing = Factory.Load<JobSailing>(sailingPK);

			yield return sailing?.JX_DepotReceivalCommencesInfo;
			yield return sailing?.JX_DepotCutOffInfo;
			yield return sailing?.JX_DepotAvailabilityDateInfo;
			yield return sailing?.JX_DepotStorageDateInfo;
			yield return sailing?.JX_ServiceStringInfo;
			yield return sailing?.JX_ArrivalPortRouteIdInfo;
			yield return sailing?.JX_DeparturePortRouteIdInfo;

			yield return sailing?.Origin?.JA_S_DEPInfo;
			yield return sailing?.Origin?.JA_E_DEPInfo;
			yield return sailing?.Origin?.JA_A_DEPInfo;
			yield return sailing?.Origin?.JA_RL_NKPortOfLoadingInfo;
			yield return sailing?.Origin?.JA_OA_DepartureCTOAddressInfo;
			yield return sailing?.Origin?.JA_DocumentaryCutoffInfo;
			yield return sailing?.Origin?.JA_VGMCutOffInfo;
			yield return sailing?.Origin?.JA_ReceivalCommencesInfo;
			yield return sailing?.Origin?.JA_CutOffInfo;
			yield return sailing?.Origin?.JA_EmptyReceivalCommencesInfo;
			yield return sailing?.Origin?.JA_EmptyCutOffInfo;
			yield return sailing?.Origin?.JA_ReeferReceivalCommencesInfo;
			yield return sailing?.Origin?.JA_ReeferCutOffInfo;
			yield return sailing?.Origin?.JA_DGReceivalCommencesInfo;
			yield return sailing?.Origin?.JA_DGCutOffInfo;

			yield return sailing?.Destination?.JB_S_ARVInfo;
			yield return sailing?.Destination?.JB_E_ARVInfo;
			yield return sailing?.Destination?.JB_A_ARVInfo;
			yield return sailing?.Destination?.JB_RL_NKPortOfDischargeInfo;
			yield return sailing?.Destination?.JB_OA_ArrivalCTOAddressInfo;
			yield return sailing?.Destination?.JB_AvailabilityDateInfo;
			yield return sailing?.Destination?.JB_StorageDateInfo;

			yield return sailing?.Voyage?.JV_VoyageFlightInfo;
			yield return sailing?.Voyage?.JV_RV_NKVesselInfo;
			yield return sailing?.Voyage?.JV_IsCharteredInfo;
			yield return sailing?.Voyage?.JV_IsCargoOnlyInfo;
			yield return sailing?.Voyage?.JV_AircraftTypeInfo;

			yield return JW_STDInfo;
			yield return JW_ETDInfo;
			yield return JW_ATDInfo;
			yield return JW_RL_NKLoadPortInfo;
			yield return JW_OA_DepartureLocationInfo;
			yield return JW_DocumentaryCutOffInfo;
			yield return JW_VGMCutOffInfo;
			yield return JW_TerminalReceivalCommencesInfo;
			yield return JW_TerminalCutOffInfo;
			yield return JW_EmptyReceivalCommencesInfo;
			yield return JW_EmptyCutOffInfo;
			yield return JW_ReeferReceivalCommencesInfo;
			yield return JW_ReeferCutOffInfo;
			yield return JW_DGReceivalCommencesInfo;
			yield return JW_DGCutOffInfo;

			yield return JW_STAInfo;
			yield return JW_ETAInfo;
			yield return JW_ATAInfo;
			yield return JW_RL_NKDiscPortInfo;
			yield return JW_OA_ArrivalLocationInfo;
			yield return JW_TerminalAvailabilityDateInfo;
			yield return JW_TerminalStorageDateInfo;

			yield return JW_VoyageFlightInfo;
			yield return JW_VesselInfo;
			yield return JW_ServiceStringInfo;
			yield return JW_IsCharterInfo;
			yield return JW_IsCargoOnlyInfo;
			yield return JW_AircraftTypeInfo;
			yield return JW_ArrivalPortRouteIdInfo;
			yield return JW_DeparturePortRouteIdInfo;

			yield return JW_DepotReceivalCommencesInfo;
			yield return JW_DepotCutOffInfo;
			yield return JW_DepotStorageDateInfo;
			yield return JW_DepotAvailabilityDateInfo;
		}

		#endregion

		#region Validation

		protected sealed override JobConsolTransportValidation GetNewValidation()
		{
			TransportValidation result = TransportValidation.New(this);

			if (TransportSupporterWithSchedule != null)
			{
				JobConsolTransportValidation validation = TransportSupporterWithSchedule.GetNewTransportValidator(this);
				if (validation != null)
				{
					result.Add(validation);
				}
			}

			return result;
		}

		#endregion

		#region Name

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = ZString.Empty;

				CommonConsol parentConsol = null;
				if (ParentType == null)
				{
					parentConsol = Factory.Load<CommonConsol>(JW_ParentGUID);
				}
				else if (typeof(CommonConsol).IsAssignableFrom(ParentType))
				{
					parentConsol = Parent as CommonConsol;
				}

				if (IsAir)
				{
					result = parentConsol != null
						? Res.GetString("d65dd44d-da77-423f-8c4e-3c76fa52cbe2", "Transport Leg (Consol='{0}', Flight='{1}')", parentConsol.JK_UniqueConsignRef, JW_VoyageFlight)
						: Res.GetString("fdf019c6-5f0b-4814-8a9f-27c55242bafa", "Transport Leg (Flight='{0}')", JW_VoyageFlight);
				}
				else if (IsSea)
				{
					var carrierName = ZString.Empty;
					if (!CarrierPK.IsEmpty)
					{
						var carrier = Factory.Load<OrgHeader>(CarrierPK);
						if (carrier != null)
						{
							carrierName = carrier.OH_Code;
						}
					}

					result = parentConsol != null
						? Res.GetString("263d90d4-7598-4624-adb4-ca86ab36e2aa", "Transport Leg (Consol='{0}', Vessel='{1}', Voyage='{2}', Carrier='{3}')", parentConsol.JK_UniqueConsignRef, JW_Vessel, JW_VoyageFlight, carrierName)
						: Res.GetString("f6c46681-0411-41d2-a33f-ae7eb4b5ab67", "Transport Leg (Vessel='{0}', Voyage='{1}', Carrier='{2}')", JW_Vessel, JW_VoyageFlight, carrierName);
				}
				else if (IsRail)
				{
					result = parentConsol != null
						? Res.GetString("22b13faf-1f09-4586-b3bd-a3949775d1ed", "Transport Leg (Consol='{0}', Journey='{1}')", parentConsol.JK_UniqueConsignRef, JW_VoyageFlight)
						: Res.GetString("c341c308-ade9-498f-8c7e-02222618d48b", "Transport Leg (Journey='{0}')", JW_VoyageFlight);
				}
				else if (IsRoad)
				{
					result = parentConsol != null
						? Res.GetString("489e90d6-bf09-44f9-b8af-419e0484dcf8", "Transport Leg (Consol='{0}', Truck='{1}')", parentConsol.JK_UniqueConsignRef, JW_VoyageFlight)
						: Res.GetString("b03c07d5-841d-41aa-b075-b723d8a597d6", "Transport Leg (Truck='{0}')", JW_VoyageFlight);
				}
				else
				{
					result = parentConsol != null
						? Res.GetString("38bc8070-16cb-4556-a455-7df5e053917f", "Transport Leg (Consol='{0}')", parentConsol.JK_UniqueConsignRef)
						: Res.GetString("e03dbb5c-895f-47c7-b4b8-221d10cd3159", "Transport Leg");
				}

				return result;
			}
		}

		#endregion

		#region Persistance

		public void MakeNonPersistent()
		{
			if (IsInDatabase)
			{
				throw new InvalidOperationException("This transport has already been saved.");
			}

			persistent = false;
		}

		public bool IsPersistent
		{
			[DebuggerStepThrough]
			get { return persistent; }
		}

		#endregion

		#region Implementation

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			if (isSettingDefaultValues)
			{
				return base.GetValueSetStrategy();
			}
			else
			{
				return new ThrowIfNoParentTransportStrategy();
			}
		}

		public bool IsDepartureContainerModeFCLorULD
		{
			get
			{
				var result = TransportSupporterWithSchedule != null && TransportSupporterWithSchedule.IsDepartureContainerModeFCLorULD;

				return result;
			}
		}

		public bool IsArrivalContainerModeFCLorULD
		{
			get
			{
				var result = TransportSupporterWithSchedule != null && TransportSupporterWithSchedule.IsArrivalContainerModeFCLorULD;

				return result;
			}
		}

		#region Move Values To/From Persisted

		void MoveValuesToPersisted()
		{
			base.JW_RL_NKLoadPort = fJW_RL_NKLoadPort;
			base.JW_RL_NKDiscPort = fJW_RL_NKDiscPort;
			base.JW_Vessel = fJW_Vessel;
			base.JW_VoyageFlight = fJW_VoyageFlight;
			base.JW_ServiceString = fJW_ServiceString;
			base.JW_ArrivalPortRouteId = fJW_ArrivalPortRouteId;
			base.JW_DeparturePortRouteId = fJW_DeparturePortRouteId;
			base.JW_IsCharter = fJW_IsCharter;
			base.JW_IsCargoOnly = fJW_IsCargoOnly;
			base.JW_ATA = fJW_ATA;
			base.JW_ATD = fJW_ATD;
			base.JW_ETA = fJW_ETA;
			base.JW_ETD = fJW_ETD;
			base.JW_STA = fJW_STA;
			base.JW_STD = fJW_STD;
			base.JW_OA_DepartureLocation = fJW_OA_DepartureLocation;
			base.JW_OA_ArrivalLocation = fJW_OA_ArrivalLocation;
			base.JW_AircraftType = fJW_AircraftType;
			base.JW_TerminalReceivalCommences = fJW_TerminalReceivalCommences;
			base.JW_DepotAvailabilityDate = fJW_DepotAvailabilityDate;
			base.JW_VGMCutOff = fJW_VGMCutOff;
			base.JW_TerminalCutOff = fJW_TerminalCutOff;
			base.JW_TerminalAvailabilityDate = fJW_TerminalAvailabilityDate;
			base.JW_TerminalStorageDate = fJW_TerminalStorageDate;
			base.JW_DepotStorageDate = fJW_DepotStorageDate;
			base.JW_DocumentaryCutOff = fJW_DocumentaryCutOff;
			base.JW_DepotCutOff = fJW_DepotCutOff;
			base.JW_DepotReceivalCommences = fJW_DepotReceivalCommences;
			base.JW_OnlineScheduleStatus = fJW_OnlineScheduleStatus;
			base.JW_EmptyReceivalCommences = fJW_EmptyReceivalCommences;
			base.JW_EmptyCutOff = fJW_EmptyCutOff;
			base.JW_ReeferReceivalCommences = fJW_ReeferReceivalCommences;
			base.JW_ReeferCutOff = fJW_ReeferCutOff;
			base.JW_DGReceivalCommences = fJW_DGReceivalCommences;
			base.JW_DGCutOff = fJW_DGCutOff;

			if (IsSea)
			{
				base.JW_OA_CarrierAddress = fJW_OA_CarrierAddress;
				fJW_OA_CarrierAddress = ZGuid.Empty;
			}
			fJW_RL_NKLoadPort = "";
			fJW_RL_NKDiscPort = "";
			fJW_Vessel = "";
			fJW_VoyageFlight = "";
			fJW_ServiceString = "";
			fJW_ArrivalPortRouteId = "";
			fJW_DeparturePortRouteId = "";
			fJW_IsCharter = false;
			fJW_IsCargoOnly = false;
			fJW_ATA = ZDateTime.Empty;
			fJW_ATD = ZDateTime.Empty;
			fJW_ETA = ZDateTime.Empty;
			fJW_ETD = ZDateTime.Empty;
			fJW_STA = ZDateTime.Empty;
			fJW_STD = ZDateTime.Empty;
			fJW_OA_DepartureLocation = ZGuid.Empty;
			fJW_OA_ArrivalLocation = ZGuid.Empty;
			fJW_AircraftType = ZString.Empty;
			fJW_DepotAvailabilityDate = ZDateTime.Empty;
			fJW_TerminalReceivalCommences = ZDateTime.Empty;
			fJW_VGMCutOff = ZDateTime.Empty;
			fJW_TerminalCutOff = ZDateTime.Empty;
			fJW_TerminalAvailabilityDate = ZDateTime.Empty;
			fJW_TerminalStorageDate = ZDateTime.Empty;
			fJW_DepotStorageDate = ZDateTime.Empty;
			fJW_DocumentaryCutOff = ZDateTime.Empty;
			fJW_DepotCutOff = ZDateTime.Empty;
			fJW_DepotReceivalCommences = ZDateTime.Empty;
			fJW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unknown;
			fJW_EmptyReceivalCommences = ZDateTime.Empty;
			fJW_EmptyCutOff = ZDateTime.Empty;
			fJW_ReeferReceivalCommences = ZDateTime.Empty;
			fJW_ReeferCutOff = ZDateTime.Empty;
			fJW_DGReceivalCommences = ZDateTime.Empty;
			fJW_DGCutOff = ZDateTime.Empty;
		}

		void MoveValuesFromPersisted()
		{
			fJW_RL_NKLoadPort = base.JW_RL_NKLoadPort;
			fJW_RL_NKDiscPort = base.JW_RL_NKDiscPort;
			fJW_Vessel = base.JW_Vessel;
			fJW_VoyageFlight = base.JW_VoyageFlight;
			fJW_ServiceString = base.JW_ServiceString;
			fJW_ArrivalPortRouteId = base.JW_ArrivalPortRouteId;
			fJW_DeparturePortRouteId = base.JW_DeparturePortRouteId;
			fJW_IsCharter = base.JW_IsCharter;
			fJW_IsCargoOnly = base.JW_IsCargoOnly;
			fJW_ATA = base.JW_ATA;
			fJW_ATD = base.JW_ATD;
			fJW_ETA = base.JW_ETA;
			fJW_ETD = base.JW_ETD;
			fJW_STA = base.JW_STA;
			fJW_STD = base.JW_STD;
			fJW_OA_DepartureLocation = base.JW_OA_DepartureLocation;
			fJW_OA_ArrivalLocation = base.JW_OA_ArrivalLocation;
			fJW_AircraftType = base.JW_AircraftType;
			fJW_DepotAvailabilityDate = base.JW_DepotAvailabilityDate;
			fJW_TerminalReceivalCommences = base.JW_TerminalReceivalCommences;
			fJW_VGMCutOff = base.JW_VGMCutOff;
			fJW_TerminalCutOff = base.JW_TerminalCutOff;
			fJW_TerminalAvailabilityDate = base.JW_TerminalAvailabilityDate;
			fJW_TerminalStorageDate = base.JW_TerminalStorageDate;
			fJW_DepotStorageDate = base.JW_DepotStorageDate;
			fJW_DocumentaryCutOff = base.JW_DocumentaryCutOff;
			fJW_DepotCutOff = base.JW_DepotCutOff;
			fJW_DepotReceivalCommences = base.JW_DepotReceivalCommences;
			fJW_OnlineScheduleStatus = base.JW_OnlineScheduleStatus;
			fJW_EmptyReceivalCommences = base.JW_EmptyReceivalCommences;
			fJW_EmptyCutOff = base.JW_EmptyCutOff;
			fJW_ReeferReceivalCommences = base.JW_ReeferReceivalCommences;
			fJW_ReeferCutOff = base.JW_ReeferCutOff;
			fJW_DGReceivalCommences = base.JW_DGReceivalCommences;
			fJW_DGCutOff = base.JW_DGCutOff;

			if (IsSea)
			{
				fJW_OA_CarrierAddress = base.JW_OA_CarrierAddress;
				base.JW_OA_CarrierAddress = ZGuid.Empty;
			}
			base.JW_RL_NKLoadPort = "";
			base.JW_RL_NKDiscPort = "";
			base.JW_Vessel = "";
			base.JW_VoyageFlight = "";
			base.JW_ServiceString = "";
			base.JW_ArrivalPortRouteId = "";
			base.JW_DeparturePortRouteId = "";
			base.JW_IsCharter = false;
			base.JW_IsCargoOnly = false;
			base.JW_ATA = ZDateTime.Empty;
			base.JW_ATD = ZDateTime.Empty;
			base.JW_ETA = ZDateTime.Empty;
			base.JW_ETD = ZDateTime.Empty;
			base.JW_STA = ZDateTime.Empty;
			base.JW_STD = ZDateTime.Empty;
			base.JW_OA_DepartureLocation = ZGuid.Empty;
			base.JW_OA_ArrivalLocation = ZGuid.Empty;
			base.JW_AircraftType = ZString.Empty;
			base.JW_TerminalReceivalCommences = ZDateTime.Empty;
			base.JW_DepotAvailabilityDate = ZDateTime.Empty;
			base.JW_VGMCutOff = ZDateTime.Empty;
			base.JW_TerminalCutOff = ZDateTime.Empty;
			base.JW_TerminalAvailabilityDate = ZDateTime.Empty;
			base.JW_TerminalStorageDate = ZDateTime.Empty;
			base.JW_DepotStorageDate = ZDateTime.Empty;
			base.JW_DocumentaryCutOff = ZDateTime.Empty;
			base.JW_DepotCutOff = ZDateTime.Empty;
			base.JW_DepotReceivalCommences = ZDateTime.Empty;
			base.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unknown;
			base.JW_EmptyReceivalCommences = ZDateTime.Empty;
			base.JW_EmptyCutOff = ZDateTime.Empty;
			base.JW_ReeferReceivalCommences = ZDateTime.Empty;
			base.JW_ReeferCutOff = ZDateTime.Empty;
			base.JW_DGReceivalCommences = ZDateTime.Empty;
			base.JW_DGCutOff = ZDateTime.Empty;
		}

		#endregion

		#region Set ALL Schedule Fields

		void SetAllScheduleFields(JobSailing sailingBO)
		{
			if (!IsDefaultingFromSchedule)
			{
				IsDefaultingFromSchedule = true;
				try
				{
					SetVoyageFields(sailingBO?.Voyage);
					SetVoyOriginFields(sailingBO?.Origin);
					SetVoyDestinationFields(sailingBO?.Destination);
					SetScheduleFields(sailingBO);

					RefreshSailingBindings();
				}
				finally
				{
					IsDefaultingFromSchedule = false;
				}
			}
		}

		bool IsDefaultingFromSchedule;

		void SetVoyageFields(JobVoyage voyage)
		{
			if (voyage != null)
			{
				voyage.ParentConsol = Parent as CommonConsol;

				if (JW_TransportMode.IsEmpty)
				{
					base.JW_TransportMode = voyage.JV_AirSeaRoad;
					UnhookSailingManagerWithoutReleasingSailing();
				}

				fJW_Vessel = voyage.JV_RV_NKVessel;
				fJW_VoyageFlight = voyage.JV_VoyageFlight;
				fJW_JX_JV_RegistrationNo = voyage.JV_RegistrationNo;
				fJW_IsCargoOnly = voyage.JV_IsCargoOnly;
				fJW_IsCharter = voyage.JV_IsChartered;
				fJW_AircraftType = voyage.JV_AircraftType;

				if (IsSea)
				{
					var carrier = Factory.Load<OrgHeader>(voyage.JV_OH_Line);
					fJW_OA_CarrierAddress = GetMainAddressPK(carrier);
				}

				if (!voyage.JV_OH_Line.IsEmpty && CarrierPK.IsEmpty)
				{
					CarrierPK = voyage.JV_OH_Line;
				}

				if (IsAir && CarrierPK == ZGuid.Empty && voyage.ParentConsol != null)
				{
					var mawbPrefix = voyage.ParentConsol.JK_MasterBillNum.Left(3);
					var airline = RefAirline.LoadFromAirlinePrefix(Factory, mawbPrefix);
					var carrierOrg = airline?.GetCorrespondingCarrierOrganisation();
					CarrierPK = carrierOrg?.PK ?? ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_Vessel();
					Validation.ValidateJW_VoyageFlight();
				}
			}
		}

		void SetVoyOriginFields(VoyageOrigin origin)
		{
			if (origin != null && !origin.IsDeleted)
			{
				ZDateTime old_E_DEP = PreviousJW_ETD.IsEmpty ? JW_ETD : PreviousJW_ETD;

				fJW_RL_NKLoadPort = origin.JA_RL_NKPortOfLoading;
				fJW_ETD = origin.JA_E_DEP;
				fJW_ATD = origin.JA_A_DEP;
				fJW_STD = origin.JA_S_DEP;

				if (origin.IsInDatabase || !IsLinkedChanging)
				{
					fJW_DocumentaryCutOff = origin.JA_DocumentaryCutoff;
					fJW_TerminalReceivalCommences = origin.JA_ReceivalCommences;
					fJW_TerminalCutOff = origin.JA_CutOff;
					fJW_VGMCutOff = origin.JA_VGMCutOff;
					fJW_JX_Load_ATA = origin.JA_A_ARV;
					fJW_JX_Load_ETA = origin.JA_E_ARV;
					fJW_OA_DepartureLocation = origin.JA_OA_DepartureCTOAddress;
					fJW_EmptyReceivalCommences = origin.JA_EmptyReceivalCommences;
					fJW_EmptyCutOff = origin.JA_EmptyCutOff;
					fJW_ReeferReceivalCommences = origin.JA_ReeferReceivalCommences;
					fJW_ReeferCutOff = origin.JA_ReeferCutOff;
					fJW_DGReceivalCommences = origin.JA_DGReceivalCommences;
					fJW_DGCutOff = origin.JA_DGCutOff;

					if (fJW_OA_DepartureLocation.IsEmpty)
					{
						JW_OA_DepartureLocation_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
					}
				}
				else
				{
					origin.JA_DocumentaryCutoff = fJW_DocumentaryCutOff;
					origin.JA_ReceivalCommences = fJW_TerminalReceivalCommences;
					origin.JA_CutOff = fJW_TerminalCutOff;
					origin.JA_VGMCutOff = fJW_VGMCutOff;
					origin.JA_A_ARV = fJW_JX_Load_ATA;
					origin.JA_E_ARV = fJW_JX_Load_ETA;
					origin.JA_OA_DepartureCTOAddress = fJW_OA_DepartureLocation;
					origin.JA_EmptyReceivalCommences = fJW_EmptyReceivalCommences;
					origin.JA_EmptyCutOff = fJW_EmptyCutOff;
					origin.JA_ReeferReceivalCommences = fJW_ReeferReceivalCommences;
					origin.JA_ReeferCutOff = fJW_ReeferCutOff;
					origin.JA_DGReceivalCommences = fJW_DGReceivalCommences;
					origin.JA_DGCutOff = fJW_DGCutOff;
				}

				if (TransportSupporterWithSchedule != null)
				{
					TransportSupporterWithSchedule.ETDSetFromSailing(this, old_E_DEP, fJW_ETD);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_RL_NKLoadPort();
					Validation.ValidateJW_ETD();
					Validation.ValidateJW_ATD();
					Validation.ValidateJW_STD();
					Validation.ValidateJW_OA_DepartureLocation();
				}
			}
		}

		void SetVoyDestinationFields(VoyageDestination destination)
		{
			if (destination != null)
			{
				ZDateTime old_E_ARV = PreviousJW_ETA.IsEmpty ? JW_ETA : PreviousJW_ETA;

				fJW_RL_NKDiscPort = destination.JB_RL_NKPortOfDischarge;
				fJW_ETA = destination.JB_E_ARV;
				fJW_ATA = destination.JB_A_ARV;
				fJW_STA = destination.JB_S_ARV;

				if (destination.IsInDatabase || !IsLinkedChanging)
				{
					fJW_TerminalAvailabilityDate = destination.JB_AvailabilityDate;
					fJW_TerminalStorageDate = destination.JB_StorageDate;

					fJW_OA_ArrivalLocation = destination.JB_OA_ArrivalCTOAddress;
					if (fJW_OA_ArrivalLocation.IsEmpty)
					{
						JW_OA_ArrivalLocation_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
					}
				}
				else
				{
					destination.JB_AvailabilityDate = fJW_TerminalAvailabilityDate;
					destination.JB_StorageDate = fJW_TerminalStorageDate;
					destination.JB_OA_ArrivalCTOAddress = fJW_OA_ArrivalLocation;
				}

				if (TransportSupporterWithSchedule != null)
				{
					TransportSupporterWithSchedule.ETASetFromSailing(this, old_E_ARV, fJW_ETA);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_RL_NKDiscPort();
					Validation.ValidateJW_ETA();
					Validation.ValidateJW_ATA();
					Validation.ValidateJW_STA();
					Validation.ValidateJW_OA_ArrivalLocation();
				}
			}
		}

		void SetScheduleFields(JobSailing sailingBO)
		{
			if (sailingBO != null && !sailingBO.IsDeleted)
			{
				if (sailingBO.IsInDatabase || !IsLinkedChanging)
				{
					fJW_DepotReceivalCommences = sailingBO.JX_DepotReceivalCommences;
					fJW_DepotCutOff = sailingBO.JX_DepotCutOff;
					fJW_DepotAvailabilityDate = sailingBO.JX_DepotAvailabilityDate;
					fJW_DepotStorageDate = sailingBO.JX_DepotStorageDate;
					fJW_JX_IsPublished = sailingBO.JX_IsPublished;
					fJW_OnlineScheduleStatus = sailingBO.JX_OnlineScheduleStatus;
					fJW_ServiceString = sailingBO.JX_ServiceString;
					fJW_ArrivalPortRouteId = sailingBO.JX_ArrivalPortRouteId;
					fJW_DeparturePortRouteId = sailingBO.JX_DeparturePortRouteId;
				}
				else
				{
					sailingBO.JX_DepotReceivalCommences = fJW_DepotReceivalCommences;
					sailingBO.JX_DepotCutOff = fJW_DepotCutOff;
					sailingBO.JX_DepotAvailabilityDate = fJW_DepotAvailabilityDate;
					sailingBO.JX_DepotStorageDate = fJW_DepotStorageDate;
					sailingBO.JX_IsPublished = fJW_JX_IsPublished;
					sailingBO.JX_OnlineScheduleStatus = fJW_OnlineScheduleStatus;
					sailingBO.JX_ServiceString = fJW_ServiceString;
					sailingBO.JX_ArrivalPortRouteId = fJW_ArrivalPortRouteId;
					sailingBO.JX_DeparturePortRouteId = fJW_DeparturePortRouteId;
				}

				if (
					JW_CarrierBookingReference.IsEmpty &&
					ImportExportHelper.IsExport(JW_RL_NKLoadPort, JW_RL_NKDiscPort)
				)
				{
					JW_CarrierBookingReference = sailingBO.JX_ReservedMasterBill;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJW_TerminalReceivalCommences();
					Validation.ValidateJW_DepotReceivalCommences();
					Validation.ValidateJW_TerminalCutOff();
					Validation.ValidateJW_DepotCutOff();
					Validation.ValidateJW_DocumentaryCutOff();
					Validation.ValidateJW_VGMCutOff();
					Validation.ValidateJW_TerminalAvailabilityDate();
					Validation.ValidateJW_DepotAvailabilityDate();
					Validation.ValidateJW_TerminalStorageDate();
					Validation.ValidateJW_DepotStorageDate();
					Validation.ValidateJW_JX_IsPublished();
					Validation.ValidateJW_JX_Load_ETA();
					Validation.ValidateJW_JX_Load_ATA();
					Validation.ValidateJW_OnlineScheduleStatus();
					Validation.ValidateJW_EmptyReceivalCommences();
					Validation.ValidateJW_EmptyCutOff();
					Validation.ValidateJW_ReeferReceivalCommences();
					Validation.ValidateJW_ReeferCutOff();
					Validation.ValidateJW_DGReceivalCommences();
					Validation.ValidateJW_DGCutOff();
					Validation.ValidateJW_ServiceString();
					Validation.ValidateJW_ArrivalPortRouteId();
					Validation.ValidateJW_DeparturePortRouteId();
				}
			}
		}

		#endregion

		#region ResetSailingDetails

		void ResetSailingDetails()
		{
			UnhookSailingManagerWithoutReleasingSailing();

			if (JW_TransportMode == Core.Constants.TransportModes.Storage)
			{
				JW_IsLinked = false;
				JW_IsCharter = false;
				JW_IsCargoOnly = false;
				JW_Status = "";
				JW_RL_NKDiscPortCore = JW_RL_NKLoadPortCore;
				JW_AircraftType = ZString.Empty;
			}

			if (JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport)
			{
				JW_IsLinked = false;
			}

			base.JW_JX = ZGuid.Empty;
			JW_Vessel = "";
			JW_VoyageFlight = "";
			JW_ServiceString = "";
			JW_ArrivalPortRouteId = "";
			JW_DeparturePortRouteId = "";
			fJW_DepotReceivalCommences = ZDateTime.Empty;
			fJW_TerminalReceivalCommences = ZDateTime.Empty;
			fJW_DepotCutOff = ZDateTime.Empty;
			fJW_TerminalCutOff = ZDateTime.Empty;
			fJW_JX_Load_ETA = ZDateTime.Empty;
			fJW_JX_Load_ATA = ZDateTime.Empty;
			fJW_DocumentaryCutOff = ZDateTime.Empty;
			fJW_VGMCutOff = ZDateTime.Empty;
			fJW_TerminalAvailabilityDate = ZDateTime.Empty;
			fJW_DepotAvailabilityDate = ZDateTime.Empty;
			fJW_TerminalStorageDate = ZDateTime.Empty;
			fJW_DepotStorageDate = ZDateTime.Empty;
			fJW_JX_IsPublished = ZBool.False;
			fJW_EmptyReceivalCommences = ZDateTime.Empty;
			fJW_EmptyCutOff = ZDateTime.Empty;
			fJW_ReeferReceivalCommences = ZDateTime.Empty;
			fJW_ReeferCutOff = ZDateTime.Empty;
			fJW_DGReceivalCommences = ZDateTime.Empty;
			fJW_DGCutOff = ZDateTime.Empty;
		}

		#endregion

		#region RefreshSailingBindings

		void RefreshSailingBindings()
		{
			JW_RL_NKLoadPortInfo.RefreshBinding();
			JW_RL_NKDiscPortInfo.RefreshBinding();
			JW_VesselInfo.RefreshBinding();
			JW_VoyageFlightInfo.RefreshBinding();
			JW_ATDInfo.RefreshBinding();
			JW_ATAInfo.RefreshBinding();
			JW_ETDInfo.RefreshBinding();
			JW_ETAInfo.RefreshBinding();
			JW_STDInfo.RefreshBinding();
			JW_STAInfo.RefreshBinding();
			JW_JX_JV_RegistrationNoInfo.RefreshBinding();
			JW_JX_JV_VoyageTypeInfo.RefreshBinding();
			JW_TerminalReceivalCommencesInfo.RefreshBinding();
			JW_DepotReceivalCommencesInfo.RefreshBinding();
			JW_TerminalCutOffInfo.RefreshBinding();
			JW_DepotCutOffInfo.RefreshBinding();
			JW_JX_Load_ETAInfo.RefreshBinding();
			JW_JX_Load_ATAInfo.RefreshBinding();
			JW_DocumentaryCutOffInfo.RefreshBinding();
			JW_TerminalAvailabilityDateInfo.RefreshBinding();
			JW_DepotAvailabilityDateInfo.RefreshBinding();
			JW_TerminalStorageDateInfo.RefreshBinding();
			JW_DepotStorageDateInfo.RefreshBinding();
			JW_JX_IsPublishedInfo.RefreshBinding();
			JW_OA_DepartureLocationInfo.RefreshBinding();
			JW_OA_ArrivalLocationInfo.RefreshBinding();
			JW_OA_CarrierAddressInfo.RefreshBinding();
			JW_EmptyReceivalCommencesInfo.RefreshBinding();
			JW_EmptyCutOffInfo.RefreshBinding();
			JW_ReeferReceivalCommencesInfo.RefreshBinding();
			JW_ReeferCutOffInfo.RefreshBinding();
			JW_DGReceivalCommencesInfo.RefreshBinding();
			JW_DGCutOffInfo.RefreshBinding();
			JW_ServiceStringInfo.RefreshBinding();
			JW_ArrivalPortRouteIdInfo.RefreshBinding();
			JW_DeparturePortRouteIdInfo.RefreshBinding();
		}

		#endregion

		#region SetOrgFilters

		void SetOrgFilters(OrgHeaderCollection collection, ZString port)
		{
			if (!port.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", port));
			}
		}

		#endregion

		#region MarkParentAsNeedingValidation

		void MarkParentAsNeedingValidation(bool includingChildren = false)
		{
			if (Parent != null)
			{
				Parent.MarkAsNeedingValidation();
				if (includingChildren)
				{
					foreach (var otherTransport in OtherParentTransports)
					{
						otherTransport.MarkAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		#endregion

		#region GetNextBestTransportType

		ZString GetNextBestTransportType()
		{
			switch (JW_TransportMode)
			{
				case Constants.TransportModes.Air:
					return GetNextBestTransportTypeForAir();

				case Constants.TransportModes.InlandWaterwayTransport:
					return GetNextBestTransportTypeForInlandWaterway();

				case Constants.TransportModes.Storage:
					return ZString.Empty;

				default:
					return GetNextBestTransportTypeForNonAir();
			}
		}

		#endregion

		#region GetNextBestTransportTypeForInlandWaterway

		ZString GetNextBestTransportTypeForInlandWaterway()
		{
			ZString result;
			if (this.IsMainLeg)
			{
				result = this.JW_TransportType;
			}
			else
			{
				if (Parent == null)
				{
					result = Constants.TransportPlanningType.PreCarriage;
				}
				else
				{
					var mainVessel = OtherParentTransports.FirstOrDefault(transport =>
						transport.JW_TransportMode == Constants.TransportModes.Sea &&
						transport.JW_TransportType == Constants.TransportPlanningType.MainVessel);
					if (mainVessel == null)
					{
						result = Constants.TransportPlanningType.PreCarriage;
					}
					else if (this.JW_LegOrder > mainVessel.JW_LegOrder)
					{
						result = Constants.TransportPlanningType.OnForwarding;
					}
					else
					{
						result = Constants.TransportPlanningType.PreCarriage;
					}
				}
			}

			return result;
		}

		public bool IsMainLeg
		{
			get
			{
				return this.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel ||
					this.JW_TransportType == Core.Constants.TransportPlanningType.Flight1 ||
					this.JW_TransportType == Core.Constants.TransportPlanningType.Flight2 ||
					this.JW_TransportType == Core.Constants.TransportPlanningType.Flight3;
			}
		}

		#endregion

		#region GetNextBestTransportTypeForAir

		ZString GetNextBestTransportTypeForAir()
		{
			ZString result;
			if (Parent == null)
			{
				result = Core.Constants.TransportPlanningType.Flight1;
			}
			else
			{
				bool flight1Used = false;
				bool flight2Used = false;
				bool flight3Used = false;

				foreach (Transport transport in OtherParentTransports)
				{
					switch (transport.JW_TransportType)
					{
						case Core.Constants.TransportPlanningType.Flight1:
							flight1Used = true;
							break;

						case Core.Constants.TransportPlanningType.Flight2:
							flight2Used = true;
							break;

						case Core.Constants.TransportPlanningType.Flight3:
							flight3Used = true;
							break;
					}
				}

				if (!flight1Used)
				{
					result = Core.Constants.TransportPlanningType.Flight1;
				}
				else if (!flight2Used)
				{
					result = Core.Constants.TransportPlanningType.Flight2;
				}
				else if (!flight3Used)
				{
					result = Core.Constants.TransportPlanningType.Flight3;
				}
				else
				{
					result = Core.Constants.TransportPlanningType.Other;
				}
			}

			return result;
		}

		#endregion

		#region GetNextBestTransportTypeForNonAir

		ZString GetNextBestTransportTypeForNonAir()
		{
			var result = Core.Constants.TransportPlanningType.MainVessel;

			foreach (Transport transport in OtherParentTransports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
				{
					result = Core.Constants.TransportPlanningType.Other;
					break;
				}
			}

			return result;
		}

		#endregion

		#region AddDateEvents

		bool originDateEventsAdded;
		bool destinationDateEventsAdded;
		bool sailingDateEventsAdded;

		void ResetDateAdded()
		{
			originDateEventsAdded = false;
			destinationDateEventsAdded = false;
			sailingDateEventsAdded = false;
		}

		void AddOriginDateEvents()
		{
			if (!ShouldAddDateEvents || ParentType == null || originDateEventsAdded)
			{
				return;
			}

			var loadUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JW_RL_NKLoadPort);

			if (IsDepartureContainerModeFCLorULD)
			{
				AddDateEvent(PreviousJW_TerminalCutOff.ToDateTimeOffset(loadUnloco), JW_TerminalCutOff.ToDateTimeOffset(loadUnloco), Events.CutOffDate, true);
			}

			AddOceanCarrierBookingByTEUEvent();

			AddDateEvent(PreviousJW_ETD.ToDateTimeOffset(loadUnloco), JW_ETD.ToDateTimeOffset(loadUnloco), Events.Departure, true);

			if (!SkipAddATDEvent)
			{
				AddDateEvent(PreviousJW_ATD.ToDateTimeOffset(loadUnloco), JW_ATD.ToDateTimeOffset(loadUnloco), Events.Departure, false);
			}

			AddDateEvent(PreviousJW_TerminalReceivalCommences.ToDateTimeOffset(loadUnloco), JW_TerminalReceivalCommences.ToDateTimeOffset(loadUnloco), Events.ReceiptCommenced, false);

			originDateEventsAdded = true;
		}

		void AddDestinationDateEvents()
		{
			if (!ShouldAddDateEvents || ParentType == null || destinationDateEventsAdded)
			{
				return;
			}

			var dischargeUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JW_RL_NKDiscPort);

			if (IsArrivalContainerModeFCLorULD && JW_ParentContainerMode != Constants.ContainerModes.Groupage)
			{
				AddDateEvent(PreviousJW_TerminalAvailabilityDate.ToDateTimeOffset(dischargeUnloco), JW_TerminalAvailabilityDate.ToDateTimeOffset(dischargeUnloco), Events.CargoAvailable, false);
			}

			if (JW_ParentContainerMode == Constants.ContainerModes.Groupage)
			{
				var ctoParameters = new Dictionary<string, string>();
				ctoParameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKDiscPort;
				ctoParameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				AddDateEvent(PreviousJW_TerminalAvailabilityDate.ToDateTimeOffset(dischargeUnloco), JW_TerminalAvailabilityDate.ToDateTimeOffset(dischargeUnloco), Events.CargoAvailable, false, parameters: ctoParameters, checkExistingLogs: false);
			}

			AddDateEvent(PreviousJW_ETA.ToDateTimeOffset(dischargeUnloco), JW_ETA.ToDateTimeOffset(dischargeUnloco), Events.Arrival, true);

			if (!SkipAddATAEvent)
			{
				AddDateEvent(PreviousJW_ATA.ToDateTimeOffset(dischargeUnloco), JW_ATA.ToDateTimeOffset(dischargeUnloco), Events.Arrival, false);
			}
			AddDateEvent(PreviousJW_TerminalStorageDate.ToDateTimeOffset(dischargeUnloco), JW_TerminalStorageDate.ToDateTimeOffset(dischargeUnloco), Events.StorageCommenced, false);

			destinationDateEventsAdded = true;
		}

		void AddSailingDateEvents()
		{
			if (!ShouldAddDateEvents || ParentType == null || sailingDateEventsAdded)
			{
				return;
			}

			var dischargeUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JW_RL_NKDiscPort);
			var loadUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JW_RL_NKLoadPort);

			if (!IsArrivalContainerModeFCLorULD && JW_ParentContainerMode != Constants.ContainerModes.Groupage)
			{
				AddDateEvent(PreviousJW_DepotAvailabilityDate.ToDateTimeOffset(dischargeUnloco), JW_DepotAvailabilityDate.ToDateTimeOffset(dischargeUnloco), Events.CargoAvailable, false);
			}

			if (!IsDepartureContainerModeFCLorULD)
			{
				var eventParameters = new Dictionary<string, string>();
				eventParameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKLoadPort;
				eventParameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Depot;

				AddDateEvent(PreviousJW_DepotCutOff.ToDateTimeOffset(loadUnloco), JW_DepotCutOff.ToDateTimeOffset(loadUnloco), Events.CutOffDate, true, eventParameters);
			}

			if (JW_ParentContainerMode == Constants.ContainerModes.Groupage)
			{
				var cfsParameters = new Dictionary<string, string>();
				cfsParameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKDiscPort;
				cfsParameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Depot;
				AddDateEvent(PreviousJW_DepotAvailabilityDate.ToDateTimeOffset(dischargeUnloco), JW_DepotAvailabilityDate.ToDateTimeOffset(dischargeUnloco), Events.CargoAvailable, false, parameters: cfsParameters, checkExistingLogs: false);
			}

			sailingDateEventsAdded = true;
		}

		void AddDateEvents()
		{
			AddOriginDateEvents();
			AddDestinationDateEvents();
			AddSailingDateEvents();
		}

		GlbBranch GetBranchForOceanCarrierBookingEvent(ZGuid sendingForwarderPK) => sendingForwarderPK.IsValid ? Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, sendingForwarderPK)) : null;

		void AddOceanCarrierBookingByTEUEvent()
		{
			var newValue = JW_ATD.ToOffset();
			var oldValue = PreviousJW_ATD.ToOffset();

			if ((newValue != oldValue || (!IsInDatabase && !newValue.IsEmpty))
				&& Parent is CommonConsol consol
				&& !consol.IsCoLoad && consol.ShippingLine is OrgHeader orgHeader && consol.ShippingLineIsShippingLine)
			{
				var branch = GetBranchForOceanCarrierBookingEvent(consol.SendingForwarderPK);
				var refShippingLine = orgHeader?.ShippingLine ?? OCBEventParameterHelper.GetRefShippingLineFromCarrierWithFallback(consol);
				if (refShippingLine != null
					&& (refShippingLine.RSL_BookingRequestAvailable || refShippingLine.RSL_ShippingInstructionAvailable || refShippingLine.RSL_ShippingOrderAvailable && consol.JK_RL_NKLoadPort.StartsWith(Constants.CountryCodes.China))
					&& branch != null)
				{
					var totalTEU = OCBEventParameterHelper.GetTotalTEU(consol);
					var totalTEUString = totalTEU.ToString();
					var scac = OCBEventParameterHelper.GetSCAC(consol);

					var lastOCBlog = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU, log => !log.IsCancelled);

					var previousNew = lastOCBlog?.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.New) ?? "0";
					decimal.TryParse(lastOCBlog?.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Maximum) ?? "0", out decimal previousMax);
					var previousQty = lastOCBlog?.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Quantity) ?? "0";
					var max = Math.Max(totalTEU, previousMax);

					var hasDepartureOCBLog = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU, log => !log.IsCancelled && string.Compare(log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type), FreightConstants.EventParameterDescriptions.Departure, StringComparison.OrdinalIgnoreCase) == 0) != null;

					var referenceParameters = OCBEventParameterHelper.GetParametersForEvent(FreightConstants.EventParameterDescriptions.Departure, totalTEUString, previousNew, max.ToString(), Math.Max(max - previousMax, 0).ToString(), hasDepartureOCBLog ? MessagePurposes.Codes.Amendment : MessagePurposes.Codes.Original, scac);

					var newLog = consol.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.OceanCarrierBookingByTEU, isEstimate: false, eventTime: newValue, reference: GetReferenceFreeTextForEvent(Events.OceanCarrierBookingByTEU, oldValue, newValue), parameters: referenceParameters));
					if (newLog != null)
					{
						newLog.SL_GB_NKBranch = branch.GB_Code;
					}
				}
			}
		}

		public string GetReferenceFreeTextForEvent(Event eventType, ZDateTimeOffset oldValue, ZDateTimeOffset newValue)
		{
			string reference;

			if (DateEvents.Contains(eventType))
			{
				var builder = new ZStringBuilder();

				if (newValue != oldValue)
				{
					if (IsInDatabase && !oldValue.IsEmpty)
					{
						builder.Append((NoResString)"Changed From: ");
						builder.Append(oldValue.ToShortDateString());
						builder.Append(" ");

						builder.Append((NoResString)"To: ");
					}
					else
					{
						builder.Append((NoResString)"Changed To: ");
					}

					builder.Append(newValue.ToShortDateString());
				}

				reference = builder.ToString();
			}
			else
			{
				reference = string.Empty;
			}

			return reference;
		}

		List<Event> DateEvents => dateEvents ?? (dateEvents = new List<Event>
		{
			Events.Arrival,
			Events.Departure,
			Events.CutOffDate,
			Events.StorageCommenced,
			Events.ReceiptCommenced
		});

		List<Event> dateEvents;

		void AddDateEvent(ZDateTimeOffset oldValue, ZDateTimeOffset newValue, Event eventType, bool isEstimate, IDictionary<string, string> parameters = null, bool checkExistingLogs = true)
		{
			if (newValue != oldValue || (!IsInDatabase && !newValue.IsEmpty))
			{
				ZQuery existingLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code);
				existingLogsQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				existingLogsQuery.AddToFilter(StmALogSchema.SL_IsEstimate, isEstimate);

				var existingLogs = Logs.Find(existingLogsQuery);
				var referenceParameters = parameters ?? GetParametersForEvent(eventType);

				if (checkExistingLogs && existingLogs.Any())
				{
					var oldLog = existingLogs.OrderByDescending(log => log.SL_PostedTimeUtc).First();
					if (oldLog.SL_EventTimeOffset == newValue && referenceParameters.All(x => oldLog.Parameters.ContainsKey(x.Key) && oldLog.Parameters[x.Key] == x.Value))
					{
						return;
					}

					var existingLogsNotInDatabase = existingLogs.Where(log => !log.IsInDatabase);
					if (existingLogsNotInDatabase.Any())
					{
						var log = existingLogsNotInDatabase.FirstOrDefault();

						using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
						{
							log.ReferenceFreeText = GetReferenceFreeTextForEvent(eventType, oldValue, newValue);

							foreach (var param in referenceParameters)
							{
								log.Parameters[param.Key] = param.Value;
							}
						}

						return;
					}
				}

				var logParent = Parent as IStmALogParent;

				var createdEvent = CreateEvent(oldValue, newValue, eventType, isEstimate, referenceParameters, logParent);
				if (createdEvent != null)
				{
					foreach (StmALog log in existingLogs.Where(log => log.IsInDatabase))
					{
						log.Cancel();
					}
				}
			}
		}

		protected virtual StmALog CreateEvent(ZDateTimeOffset oldValue, ZDateTimeOffset newValue, Event eventType, bool isEstimate, IDictionary<string, string> referenceParameters, IStmALogParent logParent)
		{
			return Logs.CreateRecreateOrUpdateEventLog(new EventValue(eventType,
								isEstimate: isEstimate,
								eventTime: newValue,
								reference: GetReferenceFreeTextForEvent(eventType, oldValue, newValue),
								parameters: referenceParameters,
								deferFiringWorkflow: logParent != null && logParent.DeferFiringWorkflow));
		}

		public IDictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.Arrival)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKDiscPort;

				if (JW_ParentType == Constants.TransportParentTypes.Consol
					|| JW_ParentType == Constants.TransportParentTypes.AgencyShipment)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Mode] = JW_TransportMode;
				}

				if (IsAir)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber] = JW_VoyageFlight;
					parameters[EventConstants.EventReferenceParameters.Codes.FlightDate] = JW_ETA.IsValid
																					? JW_ETA.ToISO8601ShortDateString()
																					: string.Empty;
				}
			}
			else if (eventType == Events.Departure)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKLoadPort;

				if (JW_ParentType == Constants.TransportParentTypes.Consol
					|| JW_ParentType == Constants.TransportParentTypes.AgencyShipment)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.Mode] = JW_TransportMode;
				}

				if (IsAir)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber] = JW_VoyageFlight;
					parameters[EventConstants.EventReferenceParameters.Codes.FlightDate] = JW_ETD.IsValid
																					? JW_ETD.ToISO8601ShortDateString()
																					: string.Empty;
				}
			}
			else if (eventType == Events.CutOffDate || eventType == Events.ReceiptCommenced)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKLoadPort;
			}
			else if (eventType == Events.StorageCommenced)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKDiscPort;
			}
			else if (eventType == Events.CargoAvailable)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Location] = JW_RL_NKDiscPort;
				parameters[EventConstants.EventReferenceParameters.Codes.Facility] = IsArrivalContainerModeFCLorULD ? EventConstants.Facilities.Code.Terminal : EventConstants.Facilities.Code.Depot;
			}

			return parameters;
		}

		public RefUNLOCO GetUNLOCOForEvent(Event eventType)
		{
			ZString port;
			if (eventType == Events.Arrival || eventType == Events.StorageCommenced ||
				eventType == Events.CargoAvailable)
			{
				port = JW_RL_NKDiscPort;
			}
			else if (eventType == Events.Departure || eventType == Events.CutOffDate ||
					 eventType == Events.ReceiptCommenced)
			{
				port = JW_RL_NKLoadPort;
			}
			else
			{
				return null;
			}

			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, port);
		}

		#endregion

		#region IFlightDetailsSuppression Members

		ZBool IFlightDetailsSuppression.HasActualRCVPassed
		{
			get
			{
				return !JW_DepotReceivalCommences.IsEmpty && ZDateTime.Now > JW_DepotReceivalCommences ||
						!JW_TerminalReceivalCommences.IsEmpty && ZDateTime.Now > JW_TerminalReceivalCommences;
			}
		}

		ZBool IFlightDetailsSuppression.HasETDPassed
		{
			get { return !JW_ETD.IsEmpty && ZDateTime.Now > JW_ETD; }
		}

		ZBool IFlightDetailsSuppression.IsPassengerFlight
		{
			get { return !JW_IsCargoOnly; }
		}

		ZBool IFlightDetailsSuppression.HasFinalRoutingLegATDPassed
		{
			get { return !JW_ATD.IsEmpty && ZDateTime.Now > JW_ATD; }
		}

		public Directions JobDirection
		{
			get
			{
				var transportParent = Parent as ITransportParent;
				return transportParent != null ? transportParent.JobDirection : Directions.Unknown;
			}
		}

		#endregion

		bool isSettingDefaultValues;
		bool persistent = true;

		#endregion

		#region ISailingManaged Members

		bool ISailingManaged.AllowScheduleCreation
		{
			get { return FreightUtilities.AllowCreateScheduleFromJob(JW_TransportMode); }
		}

		bool ISailingManaged.AllowScheduleDatesChanging
		{
			get
			{
				var checkpoint = FreightUtilities.GetEditScheduleSecurityCheckpoint(JW_TransportMode);
				return checkpoint != null && checkpoint.IsAllowed;
			}
		}

		public ZString TransportMode
		{
			get { return this.JW_TransportMode; }
		}

		ZString ISailingManaged.Load
		{
			get { return fJW_RL_NKLoadPort; }
			set
			{
				fJW_RL_NKLoadPort = value;
				JW_RL_NKLoadPortInfo.RefreshBinding();
			}
		}

		ZString ISailingManaged.Discharge
		{
			get { return fJW_RL_NKDiscPort; }
			set
			{
				fJW_RL_NKDiscPort = value;
				JW_RL_NKDiscPortInfo.RefreshBinding();
			}
		}

		ZString ISailingManaged.Vessel
		{
			get { return fJW_Vessel; }
			set
			{
				fJW_Vessel = value;
				JW_VesselInfo.RefreshBinding();
			}
		}

		ZString ISailingManaged.Voyage
		{
			get { return fJW_VoyageFlight; }
			set
			{
				fJW_VoyageFlight = value;
				JW_VoyageFlightInfo.RefreshBinding();
			}
		}

		ZString ISailingManaged.OnlineScheduleStatus
		{
			get { return fJW_OnlineScheduleStatus; }
			set
			{
				fJW_OnlineScheduleStatus = value;
				JW_OnlineScheduleStatusInfo.RefreshBinding();
			}
		}

		ZDateTime ISailingManaged.ATD
		{
			get { return fJW_ATD; }
			set
			{
				fJW_ATD = value;
				JW_ATDInfo.RefreshBinding();
			}
		}

		ZDateTime ISailingManaged.ATA
		{
			get { return fJW_ATA; }
			set
			{
				fJW_ATA = value;
				JW_ATAInfo.RefreshBinding();
			}
		}

		ZDateTime ISailingManaged.ETD
		{
			get { return fJW_ETD; }
			set
			{
				fJW_ETD = value;
				JW_ETDInfo.RefreshBinding();
			}
		}

		ZDateTime ISailingManaged.ETA
		{
			get { return fJW_ETA; }
			set
			{
				fJW_ETA = value;
				JW_ETAInfo.RefreshBinding();
			}
		}

		ZDateTime ISailingManaged.STD
		{
			get { return fJW_STD; }
			set
			{
				fJW_STD = value;
				JW_STDInfo.RefreshBinding();
			}
		}

		ZDateTime ISailingManaged.STA
		{
			get { return fJW_STA; }
			set
			{
				fJW_STA = value;
				JW_STAInfo.RefreshBinding();
			}
		}

		ZGuid ISailingManaged.ShippingLine
		{
			get
			{
				ZGuid addressPK = JW_IsLinked && IsSea
					? fJW_OA_CarrierAddress
					: JW_OA_CarrierAddress;

				var address = Factory.Load<OrgAddress>(addressPK);
				return address != null ? address.Header.PK : ZGuid.Empty;
			}
			set
			{
				if (!IsDeleted)
				{
					var carrier = Factory.Load<OrgHeader>(value);
					ZGuid addressPK = GetMainAddressPK(carrier);

					if (JW_IsLinked && IsSea)
					{
						fJW_OA_CarrierAddress = addressPK;
					}
					else
					{
						JW_OA_CarrierAddress = addressPK;
					}

					JW_OA_CarrierAddressInfo.RefreshBinding();
				}
			}
		}

		ZGuid ISailingManaged.SailingPK
		{
			get { return JW_JX; }
			set { JW_JX = value; }
		}

		ZBool ISailingManaged.IsCharter
		{
			get { return fJW_IsCharter; }
			set
			{
				fJW_IsCharter = value;
				JW_IsCharterInfo.RefreshBinding();
			}
		}

		ZString ISailingManaged.AircraftType
		{
			get { return fJW_AircraftType; }
			set
			{
				fJW_AircraftType = value;
				JW_AircraftTypeInfo.RefreshBinding();
			}
		}

		ZBool ISailingManaged.IsImportingData
		{
			get { return Parent is ISupportDataImporting && ((ISupportDataImporting)Parent).IsImportingData; }
		}

		ZString ISailingManaged.RegistrationNo
		{
			get { return fJW_JX_JV_RegistrationNo; }
			set
			{
				fJW_JX_JV_RegistrationNo = value;
				JW_JX_JV_RegistrationNoInfo.RefreshBinding();
			}
		}

		ZBool ISailingManaged.IsCargoOnly
		{
			get { return fJW_IsCargoOnly; }
			set
			{
				fJW_IsCargoOnly = value;
				JW_IsCargoOnlyInfo.RefreshBinding();
			}
		}

		void ISailingManaged.SetFCLReceivalCommences(ZDateTime value)
		{
			fJW_TerminalReceivalCommences = value;
			JW_TerminalReceivalCommencesInfo.RefreshBinding();
		}

		void ISailingManaged.SetLCLReceivalCommences(ZDateTime value)
		{
			fJW_DepotReceivalCommences = value;
			JW_DepotReceivalCommencesInfo.RefreshBinding();
		}

		void ISailingManaged.SetFCLCutOff(ZDateTime value)
		{
			fJW_TerminalCutOff = value;
			JW_TerminalCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetLCLCutOff(ZDateTime value)
		{
			fJW_DepotCutOff = value;
			JW_DepotCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetDocsCutOff(ZDateTime value)
		{
			fJW_DocumentaryCutOff = value;
			JW_DocumentaryCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetVGMCutOff(ZDateTime value)
		{
			fJW_VGMCutOff = value;
			JW_VGMCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetAvailabilityDate(ZDateTime value)
		{
			fJW_TerminalAvailabilityDate = value;
			JW_TerminalAvailabilityDateInfo.RefreshBinding();
		}

		void ISailingManaged.SetLCLAvailabilityDate(ZDateTime value)
		{
			fJW_DepotAvailabilityDate = value;
			JW_DepotAvailabilityDateInfo.RefreshBinding();
		}

		void ISailingManaged.SetStorageDate(ZDateTime value)
		{
			fJW_TerminalStorageDate = value;
			JW_TerminalStorageDateInfo.RefreshBinding();
		}

		void ISailingManaged.SetLCLStorageDate(ZDateTime value)
		{
			fJW_DepotStorageDate = value;
			JW_DepotStorageDateInfo.RefreshBinding();
		}

		void ISailingManaged.SetLoadETA(ZDateTime value)
		{
			fJW_JX_Load_ETA = value;
			JW_JX_Load_ETAInfo.RefreshBinding();
		}

		void ISailingManaged.SetLoadATA(ZDateTime value)
		{
			fJW_JX_Load_ATA = value;
			JW_JX_Load_ATAInfo.RefreshBinding();
		}

		void ISailingManaged.SetEmptyCutOff(ZDateTime value)
		{
			fJW_EmptyCutOff = value;
			JW_EmptyCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetEmptyReceivalCommences(ZDateTime value)
		{
			fJW_EmptyReceivalCommences = value;
			JW_EmptyReceivalCommencesInfo.RefreshBinding();
		}

		void ISailingManaged.SetDGCutOff(ZDateTime value)
		{
			fJW_DGCutOff = value;
			JW_DGCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetDGReceivalCommences(ZDateTime value)
		{
			fJW_DGReceivalCommences = value;
			JW_DGReceivalCommencesInfo.RefreshBinding();
		}

		void ISailingManaged.SetReeferCutOff(ZDateTime value)
		{
			fJW_ReeferCutOff = value;
			JW_ReeferCutOffInfo.RefreshBinding();
		}

		void ISailingManaged.SetReeferReceivalCommences(ZDateTime value)
		{
			fJW_ReeferReceivalCommences = value;
			JW_ReeferReceivalCommencesInfo.RefreshBinding();
		}

		void ISailingManaged.SetOA_DepartureLocation(ZGuid value)
		{
			fJW_OA_DepartureLocation = value;
			if (fJW_OA_DepartureLocation.IsEmpty)
			{
				JW_OA_DepartureLocation_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			}

			JW_OA_DepartureLocationInfo.RefreshBinding();
		}

		void ISailingManaged.SetOA_ArrivalLocation(ZGuid value)
		{
			fJW_OA_ArrivalLocation = value;
			if (fJW_OA_ArrivalLocation.IsEmpty)
			{
				JW_OA_ArrivalLocation_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
			}

			JW_OA_ArrivalLocationInfo.RefreshBinding();
		}

		void ISailingManaged.SetServiceString(ZString value)
		{
			fJW_ServiceString = value;
			JW_ServiceStringInfo.RefreshBinding();
		}

		void ISailingManaged.SetArrivalPortRouteId(ZString value)
		{
			fJW_ArrivalPortRouteId = value;
			JW_ArrivalPortRouteIdInfo.RefreshBinding();
		}

		void ISailingManaged.SetDeparturePortRouteId(ZString value)
		{
			fJW_DeparturePortRouteId = value;
			JW_DeparturePortRouteIdInfo.RefreshBinding();
		}

		void ISailingManaged.LogDateEvents(object sender)
		{
			if (sender is VoyageOrigin)
			{
				AddOriginDateEvents();
			}
			else if (sender is VoyageDestination)
			{
				AddDestinationDateEvents();
			}
			else if (sender is JobSailing)
			{
				AddSailingDateEvents();
			}
		}

		JobSailingValidation ISailingManaged.SailingAdditionalValidation => JW_IsLinked && SailingManager.Sailing != null
					? new TransportSailingAdditionalValidation(SailingManager.Sailing, this)
					: null;

		JobVoyOriginValidation ISailingManaged.VoyOriginAdditionalValidation => JW_IsLinked && SailingManager.VoyOrigin != null
					? new TransportVoyOriginAdditionalValidation(SailingManager.VoyOrigin, this)
					: null;

		JobVoyDestinationValidation ISailingManaged.VoyDestinationAdditionalValidation => JW_IsLinked && SailingManager.VoyDestination != null
						? new TransportVoyDestinationAdditionalValidation(SailingManager.VoyDestination, this)
						: null;

		#endregion

		#region IMovementLeg Members

		ZString IMovementLeg.Load
		{
			get { return JW_RL_NKLoadPort; }
		}

		ZString IMovementLeg.Discharge
		{
			get { return JW_RL_NKDiscPort; }
		}

		ZDateTime IMovementLeg.DepartureDate
		{
			get
			{
				ZDateTime aTD = JW_ATD;
				return aTD.IsValid ? aTD : JW_ETD;
			}
		}

		ZDateTime IMovementLeg.ArrivalDate
		{
			get
			{
				ZDateTime aTA = JW_ATA;
				return aTA.IsValid ? aTA : JW_ETA;
			}
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				Type parentType = ParentType;
				if (parentType == null)
				{
					Hashtable parentTypes = Factory.GetCachedValue("TransportParentWorkflowProviderList", delegate
						{
							return (Hashtable)ObjectFactory.Get("TransportParentWorkflowProviderList");
						});
					ObjectHandle objectHandle = (ObjectHandle)parentTypes[JW_ParentType.ToString()];
					parentType = objectHandle != null ? objectHandle.GetObjectType() : null;
				}

				IWorkflowProvider parent = parentType != null ? Factory.Load(parentType, JW_ParentGUID) as IWorkflowProvider : null;
				return parent == null ? Array.Empty<IWorkflowProvider>() : new IWorkflowProvider[] { parent };
			}
		}

		#endregion

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new TransportProcessHandlingInfo(this); }
		}

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return IsPersistent || !ReadOnly; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("05bd970f-b8f1-40dd-a3af-39ca8a068f79", "This leg is read-only and cannot be deleted."); }
		}

		#endregion

		#region S8 Service

		[ReadOnly(true)]
		[List("Lookups.FlightStatus_List")]
		public override ZString JW_OnlineScheduleStatus
		{
			get
			{
				return JW_OnlineScheduleStatusCore;
			}
			set
			{
				var previousValue = JW_OnlineScheduleStatusCore;

				if (value != previousValue)
				{
					CheckMaximumLength(JW_OnlineScheduleStatusInfo, value);

					JW_OnlineScheduleStatusCore = value;
					MarkParentAsNeedingValidation();
					PropertyChangeSubscription.NotifyPropertyChanged(JW_OnlineScheduleStatusInfo, previousValue);
					OnlineScheduleStatusDescriptionInfo.RefreshBinding();
				}
			}
		}

		ZString JW_OnlineScheduleStatusCore
		{
			get
			{
				ZString result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_OnlineScheduleStatus;
				}
				else
				{
					result = base.JW_OnlineScheduleStatus;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					if (Sailing != null)
					{
						Sailing.JX_OnlineScheduleStatus = value;
					}

					SailingManager.NotifyRead();

					fJW_OnlineScheduleStatus = value;
					HasChanges = true;

					SailingManager.IsOnlineScheduleStatusDirty = true;
					SailingManager.NotifyRead();

					JW_OnlineScheduleStatusInfo.RefreshBinding();
				}
				else
				{
					base.JW_OnlineScheduleStatus = value;
				}
			}
		}

		ZString fJW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Unknown;

		public override ZPropertyInfo JW_OnlineScheduleStatusInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_OnlineScheduleStatusInfo);

		public ZString OnlineScheduleStatusDescription => Lookups.FlightStatus_List.GetDescriptionFromCode(JW_OnlineScheduleStatus);

		public ZPropertyInfo OnlineScheduleStatusDescriptionInfo => GetZPropertyInfo(nameof(OnlineScheduleStatusDescription));

		public ScheduleInfo MatchedSchedule { get; private set; }

		public void TryMatchAgainstOnlineFlights()
		{
			MatchErrorMessage = string.Empty;
			if (ShouldTryMatchOnlineFlights())
			{
				using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
				{
					(MatchErrorMessage, MatchedSchedule, JW_OnlineScheduleStatus) = OnlineFlightMatchingHelper.MatchAgainstOnlineFlights(this);

					if (JW_AircraftType.IsEmpty || IsLastAircraftUpdatedFromGSS)
					{
						if ((JW_OnlineScheduleStatus == Constants.FlightScheduleStatus.Matched) && !MatchedSchedule.AircraftType.IsEmpty)
						{
							IsLastAircraftUpdatedFromGSS = true;
							JW_AircraftType = MatchedSchedule.AircraftType;
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_OnlineScheduleStatus();
					}
				}
			}
		}

		OnlineFlightMatchingHelper OnlineFlightMatchingHelper
		{
			get
			{
				if (onlineFlightMatchingHelper == null)
				{
					onlineFlightMatchingHelper = new OnlineFlightMatchingHelper(Factory);
				}
				return onlineFlightMatchingHelper;
			}
		}
		OnlineFlightMatchingHelper onlineFlightMatchingHelper;

		bool ShouldTryMatchOnlineFlights()
			=> IsAir && !IsDefaultingFromSchedule && Globals.IsUserInteractive
#if DEBUG
					&& (!Globals.IsTest || Enterprise.MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTestAttribute.Enabled)
#endif
			;

		public string MatchErrorMessage { get; private set; }

		#region IOnlineFlightMatchingUser

		ZString IFlightInformationProvider.OnlineScheduleStatus => JW_OnlineScheduleStatus;
		ZString IFlightInformationProvider.VoyageFlight => JW_VoyageFlight;
		ZDateTime IFlightInformationProvider.ETD => JW_ETD;
		ZDateTime IFlightInformationProvider.ETA => JW_ETA;
		IS8Matcher IFlightInformationProvider.Matcher => OnlineFlightMatchingHelper.Matcher;

		#endregion

		#endregion

		#region IEventDatePropertyChecker

		bool IEventDatePropertyChecker.CanUpdateProperty(IStmALog log, ZPropertyInfo property)
		{
			var locationOnTransport = GetEventLocation(Events.All[log.SL_SE_NKEvent]);
			string locationInEvent;

			log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Location, out locationInEvent);

			return string.IsNullOrEmpty(locationInEvent) || locationOnTransport == locationInEvent;
		}

		protected virtual ZString GetEventLocation(Event evnt)
		{
			ZString location = ZString.Empty;

			if (evnt == Events.Arrival)
			{
				location = JW_RL_NKDiscPort;
			}
			else if (evnt == Events.Departure)
			{
				location = JW_RL_NKLoadPort;
			}

			return location;
		}

		#endregion

		#region IWorkflowTriggerEventSource

		public IGlbCompany JobHeaderCompany
		{
			get { return Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK); }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var providers = new List<IWorkflowProviderCore>();
				var allowedParents = new ZString[]
				{
					Constants.TransportParentTypes.Consol,
					Constants.TransportParentTypes.AgencyShipment,
					Constants.TransportParentTypes.ShipmentPreAdvice
				};

				if (ParentType != null)
				{
					if (allowedParents.Contains(JW_ParentType) && Parent is IWorkflowProviderCore workflowProvider)
					{
						providers.Add(workflowProvider);
					}
					return providers;
				}

				if (JW_ParentType == Constants.TransportParentTypes.Consol && !JW_ParentGUID.IsEmpty)
				{
					var rowFactory = new RowFactory(Factory);
					var row = rowFactory.LoadFromPK(JobConsolSchema.Constants.TableName, JW_ParentGUID);
					var consolType = new ConsolTypeDecider().GetTypeForLoad(row, Factory);
					var consolParent = Factory.Load(consolType, JW_ParentGUID);

					if (consolParent is IWorkflowProviderCore workflowProvider)
					{
						providers.Add(workflowProvider);
					}
				}
				else
				{
					ReportParentNullIssue();
				}

				return providers;
			}
		}

		#endregion

		#region Supply Chain Security

		public ISupplyChainSecurityConfiguration DestinationSupplyChainSecurityConfiguration
		{
			get
			{
				var countryCode = JW_RL_NKDiscPort.SubstringSafe(0, 2);
				var cacheKey = "Transport.DestinationSupplyChainSecurityConfiguration." + countryCode;
				return Factory.GetCachedValue(cacheKey, delegate
				{
					return ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfigurationForCountry(countryCode);
				});
			}
		}

		#region ISupplyChainSecurityImportExportSupporter

		ZString ISupplyChainSecurityImportExportSupporter.LoadCountryForSupplyChainSecurity => JW_RL_NKLoadPort.SubstringSafe(0, 2);

		ZString ISupplyChainSecurityImportExportSupporter.DischargeCountryForSupplyChainSecurity => JW_RL_NKDiscPort.SubstringSafe(0, 2);

		IEnumerable<ITransport> ISupplyChainSecurityImportExportSupporter.SupplyChainSecurityRelatedTransports
		{
			get
			{
				var parent = Parent as ISupplyChainSecurityImportExportSupporter;
				if (parent != null)
				{
					return parent.SupplyChainSecurityRelatedTransports;
				}

				return new List<ITransport>();
			}
		}

		#endregion

		#region IScreeningPartyProvider

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			return JW_VesselScreeningStatus;
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get { return new ScreeningParty[] { new ScreeningParty(this, Res.GetString("2AAB6763-0714-4642-B8CA-3F3103224852", "Vessel"), this) }; }
		}

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return JW_VesselScreeningStatus; }
			set { JW_VesselScreeningStatus = value; }
		}

		#endregion

		#endregion

		#region AircraftType_List

		public CodeDescriptionPairList AircraftType_List => new CodeDescriptionPairList();

		#endregion

		#region IVesselMovementsUrlSupporter
		(VesselMovementsUrlModel model, string errorMessage) IVesselMovementsUrlSupporter.GetVesselMovementsUrlModel()
		{
			if (!IsSea)
			{
				return (null, Res.GetString("4ffe087e-62ec-45f8-ac0b-c3ee560e0119", "Routing leg Transport Mode must be SEA."));
			}

			if (!JW_RL_NKLoadPort.IsEmpty && LoadPort == null)
			{
				return (null, Res.GetString("e5afae13-60d4-4e90-b178-2e8a31efcc89", "Routing leg has an invalid load port."));
			}

			if (!JW_RL_NKDiscPort.IsEmpty && DiscPort == null)
			{
				return (null, Res.GetString("92e25361-2d58-4f81-ae56-587969de5ad4", "Routing leg has an invalid discharge port."));
			}

			var departureTime = JW_ATD.IsValid ? JW_ATD
							: JW_ETD.IsValid ? JW_ETD
							: ZDateTime.Empty;
			var arrivalTime = JW_ATA.IsValid ? JW_ATA
							: JW_ETA.IsValid ? JW_ETA
							: ZDateTime.Empty;

			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = Vessel?.RV_LloydsNumber ?? ZString.Empty,
				DepartureTime = departureTime,
				ArrivalTime = arrivalTime,
				DeparturePortUnloco = JW_RL_NKLoadPort,
				ArrivalPortUnloco = JW_RL_NKDiscPort,
				CarrierCode = Carrier?.SCACCode ?? ZString.Empty,
				VoyageNumber = JW_VoyageFlight,
			};

			return (model, null);
		}
		#endregion

		void UnhookSailingManagerWithoutReleasingSailing()
		{
			if (fSailingManager != null)
			{
				fSailingManager.UnhookWithoutReleasingSailing();
				fSailingManager = null;
			}
		}

		#region JW_AdditionalTransportMode

		[List("JW_AdditionalTransportMode_List")]
		public override ZString JW_AdditionalTransportMode
		{
			get => base.JW_AdditionalTransportMode;
			set
			{
				if (JW_AdditionalTransportMode != value)
				{
					base.JW_AdditionalTransportMode = value;

					if (!value.IsEmpty)
					{
						JW_IsLinked = false;
					}
				}
			}
		}

		public bool IsAdditionalTransportModeApplicable
		{
			get
			{
				return (JW_TransportMode == Constants.TransportModes.Rail || JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport)
					&& (JW_TransportType == Constants.TransportPlanningType.PreCarriage || JW_TransportType == Constants.TransportPlanningType.OnForwarding);
			}
		}

		void ClearAdditionalTransportMode()
		{
			if (!IsAdditionalTransportModeApplicable)
			{
				JW_AdditionalTransportMode = ZString.Empty;
			}
		}

		protected bool JW_AdditionalTransportMode_ReadOnly
		{
			get { return !IsAdditionalTransportModeApplicable; }
		}

		#endregion

		#region JW_EmptyReceivalCommences

		ZDateTime PreviousJW_EmptyReceivalCommences { get; set; }

		public ZDateTime JW_EmptyReceivalCommencesForBinding
		{
			get => JW_EmptyReceivalCommences;
			set => JW_EmptyReceivalCommences = value;
		}

		public ZPropertyInfo JW_EmptyReceivalCommencesForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_EmptyReceivalCommencesForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_EmptyReceivalCommencesInfo : JW_EmptyReceivalCommencesInfo);

		public override ZDateTime JW_EmptyReceivalCommences
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_EmptyReceivalCommencesCore;
			}
			set
			{
				var previousValue = JW_EmptyReceivalCommencesCore;
				if (value != previousValue)
				{
					JW_EmptyReceivalCommencesCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_EmptyReceivalCommences();
					}
				}
			}
		}

		public override ZPropertyInfo JW_EmptyReceivalCommencesInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_EmptyReceivalCommencesInfo);

		ZDateTime JW_EmptyReceivalCommencesCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_EmptyReceivalCommences;
				}
				else
				{
					result = base.JW_EmptyReceivalCommences;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_EmptyReceivalCommences = value;
					}

					HasChanges = true;
					fJW_EmptyReceivalCommences = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_EmptyReceivalCommences();
					}

					JW_EmptyReceivalCommencesInfo.RefreshBinding();
				}
				else
				{
					base.JW_EmptyReceivalCommences = value;
				}
			}
		}
		ZDateTime fJW_EmptyReceivalCommences;

		#endregion

		#region JW_EmptyCutOff

		ZDateTime PreviousJW_EmptyCutOff { get; set; }

		public ZDateTime JW_EmptyCutOffForBinding
		{
			get => JW_EmptyCutOff;
			set => JW_EmptyCutOff = value;
		}

		public ZPropertyInfo JW_EmptyCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_EmptyCutOffForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_EmptyCutOffInfo : JW_EmptyCutOffInfo);

		public override ZDateTime JW_EmptyCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_EmptyCutOffCore;
			}
			set
			{
				var previousValue = JW_EmptyCutOffCore;
				if (value != previousValue)
				{
					JW_EmptyCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_EmptyCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_EmptyCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_EmptyCutOffInfo);

		ZDateTime JW_EmptyCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_EmptyCutOff;
				}
				else
				{
					result = base.JW_EmptyCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_EmptyCutOff = value;
					}

					HasChanges = true;
					fJW_EmptyCutOff = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_EmptyCutOff();
					}

					JW_EmptyCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_EmptyCutOff = value;
				}
			}
		}
		ZDateTime fJW_EmptyCutOff;

		#endregion

		#region JW_ReeferReceivalCommences

		ZDateTime PreviousJW_ReeferReceivalCommences { get; set; }

		public ZDateTime JW_ReeferReceivalCommencesForBinding
		{
			get => JW_ReeferReceivalCommences;
			set => JW_ReeferReceivalCommences = value;
		}

		public ZPropertyInfo JW_ReeferReceivalCommencesForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ReeferReceivalCommencesForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_ReeferReceivalCommencesInfo : JW_ReeferReceivalCommencesInfo);

		public override ZDateTime JW_ReeferReceivalCommences
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_ReeferReceivalCommencesCore;
			}
			set
			{
				var previousValue = JW_ReeferReceivalCommencesCore;
				if (value != previousValue)
				{
					JW_ReeferReceivalCommencesCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ReeferReceivalCommences();
					}
				}
			}
		}

		public override ZPropertyInfo JW_ReeferReceivalCommencesInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ReeferReceivalCommencesInfo);

		ZDateTime JW_ReeferReceivalCommencesCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ReeferReceivalCommences;
				}
				else
				{
					result = base.JW_ReeferReceivalCommences;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_ReeferReceivalCommences = value;
					}

					HasChanges = true;
					fJW_ReeferReceivalCommences = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ReeferReceivalCommences();
					}

					JW_ReeferReceivalCommencesInfo.RefreshBinding();
				}
				else
				{
					base.JW_ReeferReceivalCommences = value;
				}
			}
		}
		ZDateTime fJW_ReeferReceivalCommences;

		#endregion

		#region JW_ReeferCutOff

		ZDateTime PreviousJW_ReeferCutOff { get; set; }

		public ZDateTime JW_ReeferCutOffForBinding
		{
			get => JW_ReeferCutOff;
			set => JW_ReeferCutOff = value;
		}

		public ZPropertyInfo JW_ReeferCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_ReeferCutOffForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_ReeferCutOffInfo : JW_ReeferCutOffInfo);

		public override ZDateTime JW_ReeferCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_ReeferCutOffCore;
			}
			set
			{
				var previousValue = JW_ReeferCutOffCore;
				if (value != previousValue)
				{
					JW_ReeferCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ReeferCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_ReeferCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_ReeferCutOffInfo);

		ZDateTime JW_ReeferCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_ReeferCutOff;
				}
				else
				{
					result = base.JW_ReeferCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_ReeferCutOff = value;
					}

					HasChanges = true;
					fJW_ReeferCutOff = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_ReeferCutOff();
					}

					JW_ReeferCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_ReeferCutOff = value;
				}
			}
		}
		ZDateTime fJW_ReeferCutOff;

		#endregion

		#region JW_DGReceivalCommences

		ZDateTime PreviousJW_DGReceivalCommences { get; set; }

		public ZDateTime JW_DGReceivalCommencesForBinding
		{
			get => JW_DGReceivalCommences;
			set => JW_DGReceivalCommences = value;
		}

		public ZPropertyInfo JW_DGReceivalCommencesForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DGReceivalCommencesForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_DGReceivalCommencesInfo : JW_DGReceivalCommencesInfo);

		public override ZDateTime JW_DGReceivalCommences
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_DGReceivalCommencesCore;
			}
			set
			{
				var previousValue = JW_DGReceivalCommencesCore;
				if (value != previousValue)
				{
					JW_DGReceivalCommencesCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DGReceivalCommences();
					}
				}
			}
		}

		public override ZPropertyInfo JW_DGReceivalCommencesInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DGReceivalCommencesInfo);

		ZDateTime JW_DGReceivalCommencesCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DGReceivalCommences;
				}
				else
				{
					result = base.JW_DGReceivalCommences;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_DGReceivalCommences = value;
					}

					HasChanges = true;
					fJW_DGReceivalCommences = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DGReceivalCommences();
					}

					JW_DGReceivalCommencesInfo.RefreshBinding();
				}
				else
				{
					base.JW_DGReceivalCommences = value;
				}
			}
		}
		ZDateTime fJW_DGReceivalCommences;

		#endregion

		#region JW_DGCutOff

		ZDateTime PreviousJW_DGCutOff { get; set; }

		public ZDateTime JW_DGCutOffForBinding
		{
			get => JW_DGCutOff;
			set => JW_DGCutOff = value;
		}

		public ZPropertyInfo JW_DGCutOffForBindingInfo => GetWrappedZPropertyInfo(Schema.JW_DGCutOffForBinding, p => JW_IsLinked && Sailing?.Origin != null ? Sailing.Origin.JA_DGCutOffInfo : JW_DGCutOffInfo);

		public override ZDateTime JW_DGCutOff
		{
			get
			{
				StoreOriginalValuesForProxiedFields();
				return JW_DGCutOffCore;
			}
			set
			{
				var previousValue = JW_DGCutOffCore;
				if (value != previousValue)
				{
					JW_DGCutOffCore = value;
					MarkParentAsNeedingValidation();

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DGCutOff();
					}
				}
			}
		}

		public override ZPropertyInfo JW_DGCutOffInfo => ChangeConcurrencyForProxiedFieldIfNeeded(base.JW_DGCutOffInfo);

		ZDateTime JW_DGCutOffCore
		{
			get
			{
				ZDateTime result;

				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();
					result = fJW_DGCutOff;
				}
				else
				{
					result = base.JW_DGCutOff;
				}

				return result;
			}
			set
			{
				if (JW_IsLinked)
				{
					SailingManager.NotifyRead();

					if (Sailing?.Origin != null)
					{
						Sailing.Origin.JA_DGCutOff = value;
					}

					HasChanges = true;
					fJW_DGCutOff = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJW_DGCutOff();
					}

					JW_DGCutOffInfo.RefreshBinding();
				}
				else
				{
					base.JW_DGCutOff = value;
				}
			}
		}
		ZDateTime fJW_DGCutOff;

		#endregion

		#region ICO2eLegProvider

		ZDecimal ICO2eLegProvider.DynamicTotalCO2e
		{
			get
			{
				if (currentCO2eCalcSupporter == null || !HaveJobCO2e)
				{
					return ZDecimal.Zero;
				}

				return (this as ICO2eProvider).RequireTEU
					? currentCO2eCalcSupporter.GetNumberOfTEUForLeg(this) * this.GetCO2ePerTEUInKg()
					: Constants.Weight.ConvertSafe(currentCO2eCalcSupporter.Weight, currentCO2eCalcSupporter.UnitOfWeight, Constants.Weight.Tonnes) * this.GetCO2ePerTonneInKg();
			}
			set
			{
				ErrorReporter.ReportOnce("SetTotalCO2eForTransport", "Attempt to set total CO2e for Transport, it's not allowed.");
			}
		}

		ZString ICO2eLegProvider.LoadPort => JW_RL_NKLoadPort;

		ZString ICO2eLegProvider.DiscPort => JW_RL_NKDiscPort;

		ZString ICO2eLegProvider.VoyageFlight => JW_VoyageFlight;

		ZString ICO2eLegProvider.AircraftType => JW_AircraftType;

		ZString ICO2eLegProvider.VesselLloydsNumber => Vessel?.RV_LloydsNumber ?? ZString.Empty;

		OrgHeader ICO2eLegProvider.Carrier => Carrier;

		ICO2eLegBasedSupporter ICO2eLegProvider.CurrentCO2eCalcSupporter
		{
			get => currentCO2eCalcSupporter;
			set => currentCO2eCalcSupporter = value;
		}
		ICO2eLegBasedSupporter currentCO2eCalcSupporter;

		bool ICO2eProvider.RequireTEU
		{
			get
			{
				if (GetParentSafe() is ICO2eLegBasedSupporter parent)
				{
					return parent.RequireTEUForTransportMode(JW_TransportMode);
				}

				return false;
			}
		}

		void ICO2eProvider.RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue)
		{
			this.LogGHGEvent(type, extra, previousCO2eValue);
			if (JW_IsLinked && Sailing != null && Sailing is ICO2eProvider provider)
			{
				provider.RecordLog(type, extra);
			}
		}

		public void UpdateTransportCO2eStatusToNotCurrent(ZPropertyInfo changedPropertyInfo)
		{
			this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(changedPropertyInfo), IsDefaultingFromSchedule || IsCopying);
		}

		public event EventHandler CO2eStatusChangedEvent;

		#endregion

		#region ICO2eParent

		public IJobCO2eCollection JobCO2eCollection
		{
			get
			{
				if (jobCO2eCollection == null)
				{
					jobCO2eCollection = new JobCO2eCollection(this);
					jobCO2eCollection.JobCO2e_StatusChanged += CO2eStatusChanged;
					jobCO2eCollection.JobCO2e_UpdatedByDataRefresh += CO2eOnUpdatedByDataRefresh;
				}
				return jobCO2eCollection;
			}
		}
		IJobCO2eCollection jobCO2eCollection;

		void CO2eStatusChanged(object sender, EventArgs e)
		{
			CO2eStatusChangedEvent?.Invoke(sender, e);
			RefreshCO2eBinding();
		}

		void CO2eOnUpdatedByDataRefresh(object sender, EventArgs e) => RefreshCO2eBinding();

		public bool HaveJobCO2e => this.JobCO2eExists();

		public ZGuid JobCO2eParentID => JW_IsLinked && !JW_JX.IsEmpty ? JW_JX : PK;

		public ZString JobCO2eParentTableCode => JW_IsLinked && !JW_JX.IsEmpty
			? JobSailingSchema.Constants.Prefix
			: JobConsolTransportSchema.Constants.Prefix;

		public void RefreshCO2e() => RefreshCO2eBinding();

		void RefreshCO2eBinding()
		{
			if (ParentType != null && HaveJobCO2e)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalCO2eForSorting();
				}
				TotalCO2eForSortingInfo.RefreshBinding();
			}
		}

		#endregion

		#region Delivery Due Date

		void DeliveryDueDateFactorHasChanged(DeliveryDueDateChangedFactor factor = DeliveryDueDateChangedFactor.TransportLeg)
		{
			if (Parent is CommonConsol consolParent && consolParent.JK_IsForwarding)
			{
				foreach (CommonShipment shipment in consolParent.Shipments)
				{
					shipment.DeliveryDueDateFactorHasChanged(factor);
				}
			}
		}

		#endregion

		#region IScreeningPartyForVessel

		ZString IScreeningPartyForVessel.Code
		{
			get { return JW_Vessel; }
		}

		ZString IScreeningPartyForVessel.CurrentScreeningStatus
		{
			get { return JW_VesselScreeningStatus; }
		}

		#endregion

		#region TotalShipmentPieces

		public ZInt TotalShipmentPieces =>
			GetParentSafe() is CommonConsol consol
				? consol.JK_TotalShipmentQuantity.ToZInt()
				: (ZInt)0;

		#endregion

		#region TotalShipmentWeight

		public ZString TotalShipmentWeight =>
			GetParentSafe() is CommonConsol consol
				? ZString.Format("{0} {1}", consol.JK_TotalShipmentWeight, consol.JK_TotalShipmentWeightUnit)
				: ZString.Empty;

		#endregion

		#region TotalShipmentVolume

		public ZString TotalShipmentVolume =>
			GetParentSafe() is CommonConsol consol
				? ZString.Format("{0} {1}", consol.JK_TotalShipmentVolume, consol.JK_TotalShipmentVolumeUnit)
				: ZString.Empty;

		#endregion

		#region ShipmentDeliveredTime

		public ZDateTime ShipmentDeliveredTime =>
			GetParentSafe() is CommonConsol consol
				? consol.ShipmentDeliveredTime
				: ZDateTime.Empty;

		#endregion

		#region BookingConfirmations

		public BookingConfirmationCollection BookingConfirmations =>
			GetParentSafe() is CommonConsol consol
				? consol.BookingConfirmations
				: new BookingConfirmationCollection(Factory);

		#endregion

		#region ActualEvents

		public ActualEventCollection ActualEvents =>
			GetParentSafe() is CommonConsol consol
				? consol.ActualEvents
				: new ActualEventCollection(Factory);

		#endregion

		#region ProcessLogCore

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			this.ProcessOneStopEvent(log);
			if (GetParentSafe() is CommonConsol consol && consol is IAirlineTrackingEventProvider provider)
			{
				provider.PropagateTransportEventToConsol(log);
			}
		}

		#endregion

		#region Date Change Logging

		void IUniversalEventAddedHandler.UniversalEventAdded(IXmlImportLogger logger, IXmlEventValueObject eventDataObject)
		{
			if (!string.IsNullOrEmpty(JW_ETD_ResetStackTrace))
			{
				if (GetParentSafe() is CommonConsol consol)
				{
					consol.TryLogWhileClearTimeOfTransports(logger, "_UE");
				}
			}
		}

		IDisposable IUpdateEventDateSupporter.SetCancellingEventDatePropertyContext(IStmALog log)
		{
			eventDatePropertyCancelledContext = log;

			return new DisposableAction(() =>
			{
				eventDatePropertyCancelledContext = null;
			});
		}

		IStmALog eventDatePropertyCancelledContext;

		#endregion
	}
}
