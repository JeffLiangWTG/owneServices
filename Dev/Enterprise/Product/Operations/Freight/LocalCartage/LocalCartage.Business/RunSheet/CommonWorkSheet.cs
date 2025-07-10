using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.LocalCartage.Business
{
	[UniversalDataContext(DataContextType.LocalTransportRunSheet)]
	[CodeProperty(CommonWorkSheet.Schema.EY_RunSheetNumber), DescriptionProperty(CommonWorkSheet.Schema.EY_RunSheetNumber)]
	public class CommonWorkSheet :
		AutoJobCartageRunSheet,
		ICommonWorkSheet,
		IEDocsProvider,
		ICreditControlledDocumentDelivery,
		IJobCostingPlugIn,
		IRatingSupporter,
		IJobNumber
	{
		public CommonWorkSheet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new class Schema : AutoJobCartageRunSheet.Schema
		{
			public const string VehicleType = "VehicleType";
			public const string LegCompletionStatus = "LegCompletionStatus";
			public const string Heading = "Heading";
			public const string Status = "Status";
			public const string StatusDescription = "StatusDescription";
			public const string ErrorStatus = "ErrorStatus";
			public const string ErrorReason = "ErrorReason";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BehaviorStrategy.SetDefaultValues(this);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				EY_RunSheetNumber = ZString.Empty;
			}
		}

		public override void OnSaving()
		{
			PopulateEY_RunSheetNumberIfNeeded();
			base.OnSaving();
		}

		protected override void OnFactorySaving()
		{
			AddCancelledLogEvents();
			base.OnFactorySaving();
		}

		void AddCancelledLogEvents()
		{
			if (HasChanges)
			{
				var errorStatusAndReason = ErrorStatus + " - " + ErrorReason;
				var errorLog = GetLastErrorStatusLog();

				if ((errorLog == null && ErrorStatus != ErrorStatuses.Working)
					|| (errorLog != null && errorLog.SL_Reference != errorStatusAndReason))
				{
					Logs.AddNew(Events.Cancelled, errorStatusAndReason);
				}
			}
		}

		public override string ToString()
		{
			return EY_RunSheetNumber;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public new CommonWorkSheetValidation Validation
		{
			get { return (CommonWorkSheetValidation)GetNewValidation(); }
		}

		protected override JobCartageRunSheetValidation GetNewValidation()
		{
			return new CommonWorkSheetValidation(this);
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(CartageLegs);
				return result.ToArray();
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);

				return types;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return EY_RunSheetNumber.IsEmpty ?
					Res.GetString("662b78ae-89c1-451c-b079-9c1ce7bbe203", "Run Sheet") :
					Res.GetString("d3203066-0e19-4698-8819-3b6253f2735c", "Run Sheet {0}", EY_RunSheetNumber);
			}
		}

		public override ZDateTime EY_StartTime
		{
			get { return base.EY_StartTime; }
			set
			{
				base.EY_StartTime = value;
				Validation.ValidateEY_EndTime();
			}
		}

		public override ZDateTime EY_EndTime
		{
			get { return base.EY_EndTime; }
			set
			{
				base.EY_EndTime = value;
				Validation.ValidateEY_StartTime();
			}
		}

		public override ZGuid EY_RQ_Truck
		{
			get { return base.EY_RQ_Truck; }
			set
			{
				if (EY_RQ_Truck == value)
				{
					base.EY_RQ_Truck = value;
				}
				else
				{
					if (!base.EY_RQ_Truck.IsEmpty)
					{
						BehaviorStrategy.VehicleRemoved(this);
						EY_TruckRegistration = "";
					}

					base.EY_RQ_Truck = value;

					AllocateTransportCoFromTruck();

					if (!EY_RQ_Truck.IsEmpty)
					{
						BehaviorStrategy.VehicleAttached(this);
					}
				}
			}
		}

		void AllocateTransportCoFromTruck()
		{
			if (Truck != null)
			{
				if (EY_OH_TransportCo.IsEmpty && Truck.RQ_OH_Owner.IsValid)
				{
					EY_OH_TransportCo = Truck.RQ_OH_Owner;

					foreach (CommonCartageLeg leg in CartageLegs)
					{
						leg.QuickOHTransportCompanyInfo.RefreshBinding();
					}
				}
			}
		}

		[List("Drivers")]
		public override ZString EY_GS_NKTruckDriver
		{
			get { return base.EY_GS_NKTruckDriver; }
			set
			{
				if (base.EY_GS_NKTruckDriver != value)
				{
					EY_DriversName = "";
					EY_DriversLicence = "";
				}
				base.EY_GS_NKTruckDriver = value;
			}
		}

		public StaffDriverCollection Drivers
		{
			get
			{
				if (fDrivers == null)
				{
					fDrivers = new StaffDriverCollection(Factory);
				}
				return fDrivers;
			}
		}

		StaffDriverCollection fDrivers;

		public override ZString EY_DriversName
		{
			get { return TruckDriver != null ? TruckDriver.GS_FullName : base.EY_DriversName; }
			[DebuggerStepThrough()]
			set { base.EY_DriversName = value; }
		}

		protected bool EY_DriversName_ReadOnly
		{
			get { return TruckDriver != null; }
		}

		public override ZString EY_TruckRegistration
		{
			get { return Truck != null ? Truck.RQ_Registration : base.EY_TruckRegistration; }
			[DebuggerStepThrough()]
			set { base.EY_TruckRegistration = value; }
		}

		protected bool EY_TruckRegistration_ReadOnly
		{
			get { return Truck != null; }
		}

		public override ZString EY_DriversLicence
		{
			get { return TruckDriver != null ? TruckDriver.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.CA1) : base.EY_DriversLicence; }
			[DebuggerStepThrough()]
			set { base.EY_DriversLicence = value; }
		}

		protected bool EY_DriversLicence_ReadOnly
		{
			get { return TruckDriver != null; }
		}

		[List("BindToLists.TransportProviders")]
		public override ZGuid EY_OH_TransportCo
		{
			get { return base.EY_OH_TransportCo; }
			set
			{
				if (base.EY_OH_TransportCo != value)
				{
					EY_TransportCoName = "";
				}
				base.EY_OH_TransportCo = value;
			}
		}

		void PopulateEY_RunSheetNumberIfNeeded()
		{
			if (EY_RunSheetNumber.IsEmpty && !IsInDatabase)
			{
				EY_RunSheetNumber = RunSheetNumberFountain.GetNextFormatted(Factory);
			}
		}

		INumberFountainProxy RunSheetNumberFountain
		{
			get { return Env.NumberFountains.JobCartageRunSheetNumber; }
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new RunSheetNumberFountainUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class RunSheetNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public RunSheetNumberFountainUniqueIndexFailureHandler(CommonWorkSheet runSheet)
				: base(JobCartageRunSheetSchema.Constants.Indexes.NR_UX__EY_RunSheetNumber, runSheet)
			{
				RunSheet = runSheet;
			}
			readonly CommonWorkSheet RunSheet;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return RunSheet.RunSheetNumberFountain; }
			}
		}

		[ChildEditableTestExclude()]
		[ChildEditable()]
		public CommonCartageLegCollection CartageLegs
		{
			get
			{
				if (cartageLegs == null)
				{
					cartageLegs = new CommonCartageLegCollection(this);
					if (IsRoot)
					{
						RegisterEditableChildObject(cartageLegs);
					}
				}
				return cartageLegs;
			}
		}
		CommonCartageLegCollection cartageLegs;

		public CommonCartageLegCollection FilteredCartageLegs
		{
			get
			{
				if (filteredCartageLegs == null)
				{
					filteredCartageLegs = new CommonCartageLegCollection(Factory);
					filteredCartageLegs.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("RunSheet Allocated", "Property", new ZString((NoResString)"Unallocated")));
					filteredCartageLegs.AdditionalFilter = new ZQuery(JobContainerLegsSchema.JU_EY_RunSheet, SQLComparisonOperator.Equal, null);
				}
				return filteredCartageLegs;
			}
		}
		CommonCartageLegCollection filteredCartageLegs;

		public CommonCartageLeg[] GetCartageLegsInOrder(ListSortDirection direction)
		{
			CommonCartageLeg[] result = CartageLegs.ToArray();
			Array.Sort(result, delegate(CommonCartageLeg lhs, CommonCartageLeg rhs)
			{ return direction == ListSortDirection.Ascending ? lhs.JU_RunSheetSequence.CompareTo(rhs.JU_RunSheetSequence) : rhs.JU_RunSheetSequence.CompareTo(lhs.JU_RunSheetSequence); });
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public List<CommonCartageLeg> GetGroupedLegs(CommonCartageLeg mainLeg)
		{
			List<CommonCartageLeg> groupedLegs = new List<CommonCartageLeg>();
			if (mainLeg != null)
			{
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (leg.JU_RunSheetSequence == mainLeg.JU_RunSheetSequence)
					{
						groupedLegs.Add(leg);
					}
				}
			}
			return groupedLegs;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public List<CommonCartageLeg> ActiveLegs
		{
			get
			{
				CommonCartageLeg lastLegWithATimeOrError = null;
				foreach (CommonCartageLeg leg in GetCartageLegsInOrder(ListSortDirection.Descending))
				{
					if (leg.IsPartComplete || leg.IsStatusInError)
					{
						lastLegWithATimeOrError = leg;
						break;
					}
				}

				return GetGroupedLegs(lastLegWithATimeOrError);
			}
		}

		public CommonCartageLeg PrimaryActiveLeg
		{
			get { return ActiveLegs.Count > 0 ? ActiveLegs[0] : null; }
		}

		public string MixedContainerModeErrorMessage
		{
			get { return Res.GetString("CommonWorkSheet|MixedContainerModeCostingErrorMessage", "Costing cannot be run on a run sheet that contains Containerized and Loose Transport Legs. Please create separate run sheets."); }
		}

		public bool HasMixedContainerMode
		{
			get { return !CartageLegs.All(leg => leg.IsContainerised) && !CartageLegs.All(leg => !leg.IsContainerised); }
		}

		public ZString VehicleType
		{
			get { return Truck == null || Truck.RoadContainerType == null ? ZString.Empty : Truck.RoadContainerType.RC_Code; }
		}

		public ZPropertyInfo VehicleTypeInfo
		{
			get { return GetZPropertyInfo(Schema.VehicleType); }
		}

		public GPS.GPSEventCollection GPSEvents
		{
			get
			{
				if (fGPSEvents == null)
				{
					fGPSEvents = new GPS.GPSEventCollection(Factory);
				}
				return fGPSEvents;
			}
		}
		GPS.GPSEventCollection fGPSEvents;

		[MaxLength(9)]
		public ZString LegCompletionStatus
		{
			get
			{
				int completeLegs = 0;

				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (leg.IsComplete)
					{
						completeLegs++;
					}
				}

				return ZString.Format("{0} / {1}", completeLegs, CartageLegs.Count).SubstringSafe(0, LegCompletionStatusInfo.MaxLength);
			}
		}

		public ZPropertyInfo LegCompletionStatusInfo
		{
			get { return GetZPropertyInfo(Schema.LegCompletionStatus); }
		}

		ZString DriverTruckCompany
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(EY_DriversName);
				result.AppendIfNotEmpty(EY_TruckRegistration);
				if (result.IsEmpty)
				{
					result.AppendIfNotEmpty(TransportCompanyName);
				}

				return result.ToStringWithDelimiterBetweenAppends(" - ").ToUpper(CultureInfo.CurrentCulture);
			}
		}

		ZString TimeRangeDescription
		{
			get
			{
				if (!EY_StartTime.IsValid || !EY_EndTime.IsValid)
				{
					return "";
				}

				bool startIsMidnight = EY_StartTime.Hour == 0 && EY_StartTime.Minute == 0;
				bool endIsEndOfDay = EY_EndTime.Hour == 23 && EY_EndTime.Minute == 59;
				bool bothHaveSameDay = EY_StartTime.Date == EY_EndTime.Date;
				bool isDefaultPeriod = bothHaveSameDay && startIsMidnight && endIsEndOfDay;

				if (isDefaultPeriod)
				{
					return GetDay(EY_StartTime);
				}
				else if (bothHaveSameDay)
				{
					return ZString.Format("{0} {1} - {2}", GetDay(EY_StartTime), GetTime(EY_StartTime), GetTime(EY_EndTime));
				}
				else
				{
					return ZString.Format("{0} {1} - {2} {3}", GetDay(EY_StartTime), GetTime(EY_StartTime), GetDay(EY_EndTime), GetTime(EY_EndTime));
				}
			}
		}

		[MaxLength(50)]
		public ZString Heading
		{
			get
			{
				ZString vehicleType = Truck != null && Truck.RoadContainerType != null ? Truck.RoadContainerType.RC_Code : ZString.Empty;

				ZStringBuilder builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(DriverTruckCompany);
				builder.AppendIfNotEmpty(TimeRangeDescription);
				builder.AppendIfNotEmpty(vehicleType);

				ZString result = builder.ToStringWithDelimiterBetweenAppends(" - ");
				return result.SubstringSafe(0, HeadingInfo.MaxLength);
			}
		}

		public ZPropertyInfo HeadingInfo
		{
			get { return GetZPropertyInfo(Schema.Heading); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class StatusDescriptions
		{
			public static string Idle
			{
				get { return ResString.GetMultilingualString("12d324ec-8a38-45c5-b430-48dd43a30fbc", "Idle"); }
			}
			public static string Delivered
			{
				get { return ResString.GetMultilingualString("b5356b6c-cb33-4b44-9f75-f619efa2ebb9", "Delivered"); }
			}
			public static string Delivering
			{
				get { return ResString.GetMultilingualString("3a47c8c1-d47a-41a2-8a11-4251b344616b", "Delivering"); }
			}
			public static string WaitDelivered
			{
				get { return ResString.GetMultilingualString("c81e3573-392f-41f8-a569-7448b28e2dc5", "Wait Delivered"); }
			}
			public static string WaitDelivering
			{
				get { return ResString.GetMultilingualString("fdd0e5d5-c212-43f8-9398-7f9d3594d97b", "Wait Delivering"); }
			}
			public static string PickedUp
			{
				get { return ResString.GetMultilingualString("45882ba9-8fc8-47e7-afab-efd1c3169f1b", "Picked Up"); }
			}
			public static string PickingUp
			{
				get { return ResString.GetMultilingualString("8801b039-b5c1-40fc-8c80-b1aad9b79223", "Picking Up"); }
			}
		}

		[ResourceStringData("CommonWorkSheet|StatusDescription", Caption = "Status")]
		[MaxLength(50)]
		public ZString StatusDescription
		{
			get
			{
				ZString runSheetErrorStatus = ZString.Empty;
				ZString runSheetErrorReason = ZString.Empty;
				ZString legError = ZString.Empty;
				ZString legStatus = ZString.Empty;
				ZString location = ZString.Empty;

				if (ErrorStatus != ErrorStatuses.Working)
				{
					runSheetErrorStatus = ErrorStatus;
					runSheetErrorReason = ErrorReason;
				}
				else
				{
					legStatus = StatusDescriptions.Idle;

					if (PrimaryActiveLeg != null)
					{
						if (AreAnyActiveLegsInError)
						{
							legError = PrimaryActiveLeg.ErrorStatusDescription;
						}

						if (PrimaryActiveLeg.JU_DeliverTimeOut.IsValid)
						{
							legStatus = StatusDescriptions.Delivered;
						}
						else if (PrimaryActiveLeg.JU_DeliverTimeIn.IsValid)
						{
							legStatus = StatusDescriptions.Delivering;
						}
						else if (PrimaryActiveLeg.JU_WaitPointTimeOut.IsValid)
						{
							legStatus = StatusDescriptions.WaitDelivered;
						}
						else if (PrimaryActiveLeg.JU_WaitPointTimeIn.IsValid)
						{
							legStatus = StatusDescriptions.WaitDelivering;
						}
						else if (PrimaryActiveLeg.JU_PickupTimeOut.IsValid)
						{
							legStatus = StatusDescriptions.PickedUp;
						}
						else if (PrimaryActiveLeg.JU_PickupTimeIn.IsValid)
						{
							legStatus = StatusDescriptions.PickingUp;
						}
					}

					if (location.IsEmpty && PrimaryActiveLeg != null)
					{
						if (PrimaryActiveLeg.JU_DeliverTimeIn.IsValid)
						{
							location = PrimaryActiveLeg.DeliveryAddressCode;
						}
						else if (PrimaryActiveLeg.JU_WaitPointTimeIn.IsValid)
						{
							location = "";
						}
						else if (PrimaryActiveLeg.JU_PickupTimeIn.IsValid)
						{
							location = PrimaryActiveLeg.PickupAddressCode;
						}
					}
				}

				ZStringBuilder builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(legError);
				builder.AppendIfNotEmpty(legStatus);
				builder.AppendIfNotEmpty(location);
				builder.AppendIfNotEmpty(runSheetErrorStatus);
				builder.AppendIfNotEmpty(runSheetErrorReason);

				ZString result = builder.ToStringWithDelimiterBetweenAppends(" - ");
				return result.SubstringSafe(0, StatusDescriptionInfo.MaxLength);
			}
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.StatusDescription); }
		}

		public void ResetErrorStatus()
		{
			errorStatus = null;
			errorReason = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class ErrorStatuses
		{
			public static string Working
			{
				get { return ResString.GetMultilingualString("a9e5b54c-48dd-45b1-8c85-3eb055b598a1", "Working"); }
			}

			public static string Canceled
			{
				get { return ResString.GetMultilingualString("448fa590-60a5-4c50-bd21-53c19e185f01", "Canceled"); }
			}

			public static string VehicleOutOfAction
			{
				get { return ResString.GetMultilingualString("0e424b5b-a95d-41de-937f-90f5e6a49c3e", "Vehicle - Out Of Action"); }
			}

			public static string DriverOutOfAction
			{
				get { return ResString.GetMultilingualString("382a82ac-59f7-4e69-8382-a2d56ed5ede5", "Driver - Out Of Action"); }
			}

			public static CodeDescriptionPairList List
			{
				get
				{
					if (list == null)
					{
						list = new CodeDescriptionPairList();
						list.AddPair(Working);
						list.AddPair(Canceled);
						list.AddPair(VehicleOutOfAction);
						list.AddPair(DriverOutOfAction);
					}

					return list;
				}
			}

			[ThreadStatic]
			static CodeDescriptionPairList list;
		}

		[ResourceStringData("CommonWorkSheet|ErrorStatus", Caption = "Override")]
		[List("ErrorStatusList")]
		[MaxLength(25)]
		public ZString ErrorStatus
		{
			get
			{
				if (!errorStatus.HasValue)
				{
					errorStatus = GetErrorStatus();
				}

				return errorStatus.Value;
			}
			set
			{
				var status = ZString.Empty;
				SetNonPersistentPropertyValue(ErrorStatusInfo, ref status, value);
				errorStatus = status;
				ErrorReason = "";

				if (!IsValidationSuspended)
				{
					Validation.ValidateErrorStatus();
				}

				StatusInfo.RefreshBinding();
				StatusDescriptionInfo.RefreshBinding();
			}
		}
		ZString? errorStatus;

		public CodeDescriptionPairList ErrorStatusList
		{
			get { return ErrorStatuses.List; }
		}

		public ZPropertyInfo ErrorStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ErrorStatus); }
		}

		ZString GetErrorStatus()
		{
			var code = ErrorStatuses.Working;

			var cancelledLog = GetLastErrorStatusLog();
			if (cancelledLog != null)
			{
				foreach (CodeDescriptionPair error in ErrorStatuses.List)
				{
					if (cancelledLog.SL_Reference.StartsWith(error.Code, StringComparison.CurrentCulture))
					{
						code = error.Code;
						break;
					}
				}
			}
			return code;
		}

		StmALog GetLastErrorStatusLog()
		{
			var logs = Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == Events.CancelledCode);
			return logs.OrderBy(l => l.SL_EventTime).LastOrDefault();
		}

		[MaxLength(25)]
		[ResourceStringData("CommonWorkSheet|ErrorReason", Caption = "Reason")]
		public ZString ErrorReason
		{
			get
			{
				if (!errorReason.HasValue)
				{
					errorReason = GetErrorReason();
				}
				return errorReason.Value;
			}
			set
			{
				var reason = ZString.Empty;
				SetNonPersistentPropertyValue(ErrorReasonInfo, ref reason, value);
				errorReason = reason;
				StatusInfo.RefreshBinding();
				StatusDescriptionInfo.RefreshBinding();
			}
		}
		ZString? errorReason;

		public ZPropertyInfo ErrorReasonInfo
		{
			get { return GetZPropertyInfo(Schema.ErrorReason); }
		}

		protected bool ErrorReason_ReadOnly
		{
			get { return ErrorStatus == ErrorStatuses.Working; }
		}

		ZString GetErrorReason()
		{
			errorReason = "";
			if (ErrorStatus != ErrorStatuses.Working)
			{
				var cancelledLog = GetLastErrorStatusLog();
				if (cancelledLog != null)
				{
					errorReason = cancelledLog.SL_Reference.SubstringSafe(ErrorStatus.Length + 3);
				}
			}
			return errorReason.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
		[CodeAlive("Provides status of the run sheet")]
		public enum RunSheetStatuses
		{
			None,
			OK,
			NearCompletion,
			Completed,
			Warning,
			LegError,
			RunSheetError
		}

		[MaxLength(20)]
		public ZString Status
		{
			get
			{
				if (ErrorStatus != ErrorStatuses.Working)
				{
					return nameof(RunSheetStatuses.RunSheetError);
				}
				else if (CartageLegs.Count == 0)
				{
					return nameof(RunSheetStatuses.None);
				}
				else if (AreAnyActiveLegsInError)
				{
					return nameof(RunSheetStatuses.LegError);
				}
				else if (AreCartageLegsComplete)
				{
					return nameof(RunSheetStatuses.Completed);
				}
				else if (AreCartageLegsNearlyComplete)
				{
					return nameof(RunSheetStatuses.NearCompletion);
				}

				return nameof(RunSheetStatuses.OK);
			}
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		public ZBool HasRunSheetError
		{
			get { return Status == nameof(RunSheetStatuses.RunSheetError); }
		}

		[ReadOnlyMember(nameof(HasTransportCo))]
		[ResourceStringData("CommonWorkSheet|TransportCompanyName", Caption = "Transport Company Name", MediumCaption = "Trans. Co. Name", ShortCaption = "Trans. Name")]
		public ZString TransportCompanyName
		{
			get => TransportCo?.OH_FullNameTruncated ?? EY_TransportCoName;
			set
			{
				if (TransportCo == null)
				{
					EY_TransportCoName = value;
				}
			}
		}

		public ZPropertyInfo TransportCompanyNameInfo => GetWrappedZPropertyInfo(nameof(TransportCompanyName), sender => EY_TransportCoNameInfo);

		bool HasTransportCo => TransportCo != null;

		ZBool AreAnyActiveLegsInError
		{
			get
			{
				foreach (CommonCartageLeg leg in ActiveLegs)
				{
					if (leg.LegStatus == CommonCartageLeg.LegStatuses.Error)
					{
						return true;
					}
				}
				return false;
			}
		}

		ZBool AreCartageLegsComplete
		{
			get
			{
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (!leg.IsComplete)
					{
						return false;
					}
				}
				return true;
			}
		}

		ZBool AreCartageLegsNearlyComplete
		{
			get
			{
				CommonCartageLeg[] legs = GetCartageLegsInOrder(ListSortDirection.Descending);

				if (legs.Length == 0)
				{
					return false;
				}

				CommonCartageLeg lastLeg = lastLeg = legs[0];

				CommonCartageLeg secondLastLeg = null;
				for (int i = 1; i < legs.Length; i++)
				{
					if (legs[i].JU_RunSheetSequence != lastLeg.JU_RunSheetSequence)
					{
						secondLastLeg = legs[i];
						break;
					}
				}

				if (lastLeg.IsPartComplete || (secondLastLeg == null || secondLastLeg.IsComplete))
				{
					return true;
				}

				return false;
			}
		}

		public ZBool IsRoot { get; set; }

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return BehaviorStrategy.DocumentSupporter(this); }
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = BehaviorStrategy.DocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return ResString.GetMultilingualString("B043BAC6-2523-4683-9988-C0C94D90B4A1", "Consignee, Consignor or Local Client for Billing"); }
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
			get { return CartageLegs.Cast<ICreditControlledDocumentDelivery>().SelectMany(l => l.OrganisationsForCreditChecks).ToArray(); }
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
			get { return CartageLegs.Select(l => (string)l.Cartage.JJ_ConsignmentID).ToArray(); }
		}

		IGenericJobCostSupporter IGenericJobCostPlugIn.CostSupporter
		{
			get { return costSupporter ?? (costSupporter = new CommonWorkSheetCostSupporter(this)); }
		}

		IGenericJobCostSupporter costSupporter;

		decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK)
		{
			return 0m;
		}

		void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference)
		{
			Logs.AddNew(@event, reference);
		}

		ZString IJobCostingPlugIn.GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob)
		{
			return ZString.Empty;
		}

		ZString IJobCostingPlugIn.JK_UniqueConsignRef
		{
			get { return EY_RunSheetNumber; }
		}

		RefUNLOCO IJobCostingPlugIn.LoadPort
		{
			get { return null; }
		}

		RefUNLOCO IJobCostingPlugIn.DischargePort
		{
			get { return null; }
		}

		JobProfitLossCollection IJobCostingPlugIn.ProfitLossContainer
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ConsolExchangeRate
		{
			get { return 0m; }
		}

		RefCurrency IJobCostingPlugIn.ConsolCurrency
		{
			get { return null; }
		}

		bool IJobCostingPlugIn.IsMasterCollect
		{
			get { return false; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgent
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentAPInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentARInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgent
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentAPInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentARInvoicingParty
		{
			get { return null; }
		}

		ZString IJobCostingPlugIn.TransportMode
		{
			get { return ZString.Empty; }
		}

		ZString IJobCostingPlugIn.Module
		{
			get { return ApportionmentMethodModules.TransportBooking; }
		}

		ZString IJobCostingPlugIn.ContainerMode => ZString.Empty;

		ZString IJobCostingPlugIn.ConsolType => ZString.Empty;

		ZString IJobCostingPlugIn.Direction => ZString.Empty;

		CodeDescriptionPairList IJobCostingPlugIn.PrepaidCollectList
		{
			get { return null; }
		}

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return adaptersProvider ?? (adaptersProvider = new CommonWorkSheetRatingAdapterProvider(this)); }
		}

		RatingAdaptersProvider adaptersProvider;

		CommonWorkSheetBehaviorStrategy BehaviorStrategy
		{
			get { return CommonCartageBehaviorStrategyProvider.GetCartageProvider(Factory).WorkSheetBehaviorStrategy; }
		}

		public string JobNumber => EY_RunSheetNumber;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<DocumentCartageLegEventArgs> OnGetCartageLegsToPrint;
		public void RaiseOnGetCartageLegsToPrint(DocumentCartageLegEventArgs e)
		{
			if (OnGetCartageLegsToPrint != null)
			{
				OnGetCartageLegsToPrint(this, e);
			}
		}

		public event EventHandler ActiveLegChanged;
		public void RaiseActiveLegChanged(EventArgs e)
		{
			if (ActiveLegChanged != null)
			{
				ActiveLegChanged(this, e);
			}
		}

		public event EventHandler OrderChanged;
		public void RaiseOrderChanged(EventArgs e)
		{
			if (OrderChanged != null)
			{
				OrderChanged(this, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<WorkSheetLegLinkEventArgs> OnCartageLegAdded;
		public void RaiseCartageLegAdded(WorkSheetLegLinkEventArgs e)
		{
			if (OnCartageLegAdded != null)
			{
				OnCartageLegAdded(this, e);
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CommonWorkSheetFetchStrategy(this);
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

			if (dateTime.Hour == 0 && dateTime.Minute == 0)
			{
				return ResString.GetMultilingualString("01da4810-840d-4383-a860-91764e52e4c2", "Midnight");
			}
			else
			{
				return dateTime.ToShortTimeString();
			}
		}

		public void LegUpdated()
		{
			StatusDescriptionInfo.RefreshBinding();
			StatusInfo.RefreshBinding();
			CheckActiveLeg();
			RaiseOrderChanged(new EventArgs());
		}

		void CheckActiveLeg()
		{
			bool hasDifference = activeLegs.Count != ActiveLegs.Count;

			if (!hasDifference)
			{
				foreach (CommonCartageLeg leg in ActiveLegs)
				{
					if (!activeLegs.Contains(leg))
					{
						hasDifference = true;
					}
				}
			}

			if (hasDifference)
			{
				activeLegs.Clear();
				activeLegs.AddRange(ActiveLegs);
				RaiseActiveLegChanged(new EventArgs());

				foreach (CommonCartageLeg leg in CartageLegs)
				{
					leg.SequenceAndActiveInfo.RefreshBinding();
				}
			}
		}
		readonly List<CommonCartageLeg> activeLegs = new List<CommonCartageLeg>();
	}
}
