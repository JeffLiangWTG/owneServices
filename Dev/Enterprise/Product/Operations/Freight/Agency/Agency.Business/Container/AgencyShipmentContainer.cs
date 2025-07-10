using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using ReadOnlyAttribute = System.ComponentModel.ReadOnlyAttribute;

namespace Enterprise.Freight.Agency.Business
{
	[UniversalDataContext(DataContextType.AgencyShipmentContainer)]
	[CodeProperty(AgencyShipmentContainer.Schema.JC_ContainerCode)]
	public class AgencyShipmentContainer : CommonContainer,
		Integration.Agency.IAgencyShipmentContainer,
		IEDIMessageCollectionProvider,
		IUNDGDataItemProvider,
		IProcessHandlingInfoProvider
	{
		#region Schema

		public new class Schema : CommonContainer.Schema
		{
			public const string JC_ImportReleaseOrderStatus = "JC_ImportReleaseOrderStatus";

			public const string JC_Calc_ImportDetentionOverdueDays = "JC_Calc_ImportDetentionOverdueDays";
			public const string JC_Calc_ExportDetentionOverdueDays = "JC_Calc_ExportDetentionOverdueDays";

			public const string JC_Calc_ImportDetentionFreeDays = "JC_Calc_ImportDetentionFreeDays";
			public const string JC_Calc_ExportDetentionFreeDays = "JC_Calc_ExportDetentionFreeDays";

			public const string CustomsEntryNumberType = "CustomsEntryNumberType";
			public const string CustomsEntryNumber = "CustomsEntryNumber";
			public const string BillContainersEntryNumberType = "BillContainersEntryNumberType";
			public const string BillContainersEntryNumber = "BillContainersEntryNumber";
			public const string GrossWeightVerifiedByPK = "GrossWeightVerifiedByPK";
			public const string VerifiedByCompany = "VerifiedByCompany";
			public const string VerifiedByPerson = "VerifiedByPerson";
			public const string VerifiedByPhone = "VerifiedByPhone";
			public const string VerifiedByEmail = "VerifiedByEmail";
			public const string VerifiedMethod = "VerifiedMethod";
		}

		#endregion

		#region Constructors

		public AgencyShipmentContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			var containersInFactoryCache = factory.GetBizOsForPK(PK.ToGuid());

			if (containersInFactoryCache.Length > 1)
			{
				AgencyBooking.NotifyContainerLoadedWithOtherType(factory, this);
			}
		}

		#endregion

		#region Properties

		#region VerifiedMethod

		public ZString VerifiedMethod
		{
			get { return Lookups.GrossWeightVerificationTypeList.GetDescriptionFromCode(JC_GrossWeightVerificationType); }
		}

		public ZPropertyInfo VerifiedMethodInfo
		{
			get { return GetZPropertyInfo(Schema.VerifiedMethod); }
		}

		#endregion

		#region VerifiedByCompany

		public ZString VerifiedByCompany
		{
			get { return GrossWeightVerifiedByAddress.E2_CompanyName; }
		}

		public ZPropertyInfo VerifiedByCompanyInfo
		{
			get { return GetZPropertyInfo(Schema.VerifiedByCompany); }
		}

		#endregion

		#region VerifiedByPerson

		public ZString VerifiedByPerson
		{
			get { return GrossWeightVerifiedByAddress.E2_Contact; }
		}

		public ZPropertyInfo VerifiedByPersonInfo
		{
			get { return GetZPropertyInfo(Schema.VerifiedByPerson); }
		}

		#endregion

		#region VerifiedByPhone

		public ZString VerifiedByPhone
		{
			get { return GrossWeightVerifiedByAddress.E2_Phone; }
		}

		public ZPropertyInfo VerifiedByPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.VerifiedByPhone); }
		}

		#endregion

		#region VerifiedByEmail

		public ZString VerifiedByEmail
		{
			get { return GrossWeightVerifiedByAddress.E2_Email; }
		}

		public ZPropertyInfo VerifiedByEmailInfo
		{
			get { return GetZPropertyInfo(Schema.VerifiedByEmail); }
		}

		#endregion

		public override ZString JC_ContainerNum
		{
			get { return base.JC_ContainerNum; }
			set
			{
				if (JC_ContainerNum != value)
				{
					StockManager.ContainerNumberChanging(base.JC_ContainerNum, value);
					base.JC_ContainerNum = value;

					if (Booking != null && !Booking.IsDeleted)
					{
						AgencyShipmentContainerDependentCollection containers =
							JC_Purpose == ContainerBookedStatus.Codes.Booked ? Booking.BookedContainers : Booking.RealContainers;

						foreach (AgencyShipmentContainer container in containers)
						{
							if (!container.IsValidationSuspended)
							{
								container.Validation.ValidateJC_ContainerNum();
							}
						}
					}

					RefreshMovements();
				}
			}
		}

		public override ZShort JC_ContainerCount
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_ContainerCount; }
			set
			{
				base.JC_ContainerCount = value < 1 ? (ZShort)1 : value;
				SetGrossVolumeFromDimensions();
			}
		}

		protected bool JC_ContainerCount_ReadOnly
		{
			get { return !JC_ReleaseNum.IsEmpty; }
		}

		[ReadOnly(true)]
		public override ZString JC_ContainerImportDORelease
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_ContainerImportDORelease; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.JC_ContainerImportDORelease = value; }
		}

		#region JC_ImportReleaseOrderStatus

		[MaxLength(13)]
		public ZString JC_ImportReleaseOrderStatus
		{
			get
			{
				ZString status;

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
				{
					status = EIDOStatus;
				}
				else
				{
					status = ImportReleaseOrderStatus;
				}

				return status;
			}
		}

		public ZPropertyInfo JC_ImportReleaseOrderStatusInfo
		{
			get { return GetZPropertyInfo(Schema.JC_ImportReleaseOrderStatus); }
		}

		#region EIDO

		ZString EIDOStatus
		{
			get
			{
				return (eidoStatusCache ?? (eidoStatusCache = new CachedProperty<ZString>(Factory, () => CalculateEIDOStatus(false)))).Value;
			}
		}
		CachedProperty<ZString> eidoStatusCache;

		ZString CalculateEIDOStatus(bool ignoreFailedAndRejectedMessages)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.EIDO);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

			if (ignoreFailedAndRejectedMessages)
			{
				filter.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, new ZString[]
				{
					EDIMessage.Status.Rejected,
					EDIMessage.Status.Failed,
				});
			}

			EDIMessage lastEIDOMessageSent = null;

			foreach (EDIMessage message in Messages.Find(filter))
			{
				if (lastEIDOMessageSent == null || message.EM_SystemCreateTimeUtc > lastEIDOMessageSent.EM_SystemCreateTimeUtc)
				{
					lastEIDOMessageSent = message;
				}
			}

			if (lastEIDOMessageSent == null)
			{
				return ReleaseImportOrderMessageStatusList.Codes.NotSent;
			}
			else
			{
				switch (lastEIDOMessageSent.EM_MessageSubType)
				{
					case EIDOMessageTypes.Codes.Original:
						switch (lastEIDOMessageSent.EM_Status)
						{
							case EIDOMessage.Status.Sent:
							case EIDOMessage.Status.Queued:
								return ReleaseImportOrderMessageStatusList.Codes.OriginalSent;

							case EIDOMessage.Status.Received:
								return ReleaseImportOrderMessageStatusList.Codes.Accepted;

							case EIDOMessage.Status.Acknowledged:
								return ReleaseImportOrderMessageStatusList.Codes.Acknowledged;

							case EIDOMessage.Status.Rejected:
								return ReleaseImportOrderMessageStatusList.Codes.Rejected;

							case EIDOMessage.Status.Failed:
								return ReleaseImportOrderMessageStatusList.Codes.Failed;

							default:
								return ReleaseImportOrderMessageStatusList.Codes.Error;
						}

					case EIDOMessageTypes.Codes.Cancellation:
						switch (lastEIDOMessageSent.EM_Status)
						{
							case EIDOMessage.Status.Sent:
							case EIDOMessage.Status.Queued:
								return ReleaseImportOrderMessageStatusList.Codes.WithdrawSent;

							case EIDOMessage.Status.Received:
							case EIDOMessage.Status.Acknowledged:
								return ReleaseImportOrderMessageStatusList.Codes.Withdrawn;

							case EIDOMessage.Status.Rejected:
								return ReleaseImportOrderMessageStatusList.Codes.Rejected;

							case EIDOMessage.Status.Failed:
								return ReleaseImportOrderMessageStatusList.Codes.Failed;

							default:
								return ReleaseImportOrderMessageStatusList.Codes.Error;
						}

					default:
						return ReleaseImportOrderMessageStatusList.Codes.Error;
				}
			}
		}

		#endregion

		#region Import Release Order

		ZString ImportReleaseOrderStatus
		{
			get
			{
				if (importReleaseOrderStatus == null)
				{
					importReleaseOrderStatus = new CachedProperty<ZString>(Factory, GetImportReleaseOrderStatus);
				}

				return importReleaseOrderStatus.Value;
			}
		}
		CachedProperty<ZString> importReleaseOrderStatus;

		ZString GetImportReleaseOrderStatus()
		{
			var logs = new[]
			{
				Logs.MostRecentLogByEventTime(Events.MessageSent,                  WhereMessageTypeIs(Constants.EventReferenceMessageTypes.ImportReleaseOrder)),
				Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelRequest, WhereMessageTypeIs(Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal)),
				Logs.MostRecentLogByEventTime(Events.InterchangeRejected,          WhereMessageTypeIs(Constants.EventReferenceMessageTypes.ImportReleaseOrder))
			};

			var mostRecentLog = logs.Where(l => l != null).OrderByDescending(l => l.SL_EventTime).FirstOrDefault();
			if (mostRecentLog != null)
			{
				switch (mostRecentLog.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						return ReleaseImportOrderMessageStatusList.Codes.OriginalSent;

					case Events.MessageWithdrawCancelRequestCode:
						return ReleaseImportOrderMessageStatusList.Codes.WithdrawSent;

					case Events.InterchangeRejectedCode:
						return ReleaseImportOrderMessageStatusList.Codes.Rejected;

					default:
						return ReleaseImportOrderMessageStatusList.Codes.NotSent;
				}
			}
			else
			{
				return ReleaseImportOrderMessageStatusList.Codes.NotSent;
			}
		}

		Func<StmALog, bool> WhereMessageTypeIs(string type)
		{
			Func<StmALog, bool> result = log =>
			{
				string val;

				return log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out val)
					&& StringComparer.InvariantCultureIgnoreCase.Compare(val, type) == 0;
			};

			return result;
		}

		#endregion

		#endregion

		[ReadOnly(true)]
		public override ZString JC_ReleaseNum
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_ReleaseNum; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.JC_ReleaseNum = value; }
		}

		[List("Lookups.Containers")]
		public override ZGuid JC_RC
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_RC; }
			set
			{
				StockManager.ContainerTypeChanging(base.JC_RC, value);
				base.JC_RC = value;
				Validation.ValidateJC_AirVentFlow();
				Validation.ValidateJC_HumidityPercent();
				Validation.ValidateJC_SetPointTemp();
				if (Booking != null)
				{
					Booking.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool JC_RC_ReadOnly
		{
			get { return !JC_ReleaseNum.IsEmpty; }
		}

		public override ZBool JC_IsShipperOwned
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_IsShipperOwned; }
			set
			{
				StockManager.IsShipperOwnedChanging(base.JC_IsShipperOwned, value);
				base.JC_IsShipperOwned = value;
			}
		}

		#region JC_GrossWeight

		public override ZDecimal JC_GrossWeight
		{
			get { return base.JC_GrossWeight; }
			set
			{
				base.JC_GrossWeight = value;
				if (Booking != null)
				{
					Booking.MarkAsNeedingValidation();
				}
			}
		}

		[List("JC_GrossWeightUQ_List")]
		public override ZString JC_GrossWeightUQ
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_GrossWeightUQ; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.JC_GrossWeightUQ = value;
				if (Booking != null)
				{
					Booking.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JC_GrossVolume

		public override ZDecimal JC_GrossVolume
		{
			get { return base.JC_GrossVolume; }
			set
			{
				base.JC_GrossVolume = this.GetRoundedValue(JobContainerSchema.JC_GrossVolume, JC_GrossVolumeInfo, value);
				if (Booking != null)
				{
					Booking.MarkAsNeedingValidation();
				}
			}
		}

		[List("JC_GrossVolumeUQ_List")]
		public override ZString JC_GrossVolumeUQ
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JC_GrossVolumeUQ; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.JC_GrossVolumeUQ = value;

				this.SetRoundedValue(JobContainerSchema.JC_GrossVolume, JC_GrossVolumeInfo);

				if (Booking != null)
				{
					Booking.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Dimensions

		[List("JC_TotalUnitOfMeasure_List")]
		public override ZString JC_TotalUnitOfMeasure
		{
			get { return base.JC_TotalUnitOfMeasure; }
			set
			{
				base.JC_TotalUnitOfMeasure = value;
				SetGrossVolumeFromDimensions();
			}
		}

		public override ZDecimal JC_TotalLength
		{
			get { return base.JC_TotalLength; }
			set
			{
				base.JC_TotalLength = value;
				SetGrossVolumeFromDimensions();
			}
		}

		public override ZDecimal JC_TotalHeight
		{
			get { return base.JC_TotalHeight; }
			set
			{
				base.JC_TotalHeight = value;
				SetGrossVolumeFromDimensions();
			}
		}

		public override ZDecimal JC_TotalWidth
		{
			get { return base.JC_TotalWidth; }
			set
			{
				base.JC_TotalWidth = value;
				SetGrossVolumeFromDimensions();
			}
		}

		void SetGrossVolumeFromDimensions()
		{
			if (IsTopLevelPack && JC_TotalLength > 0 && JC_TotalHeight > 0 && JC_TotalWidth > 0 && JC_ContainerCount > 0)
			{
				JC_GrossVolume = FreightUtilities.CalculateVolume(JC_GrossVolume, JC_ContainerCount, JC_TotalLength, JC_TotalWidth, JC_TotalHeight, JC_TotalUnitOfMeasure, JC_GrossVolumeUQ, JobContainerSchema.JC_GrossVolume.Scale);
			}
		}

		#endregion

		#region JC_F3_NKPackType

		[List("JC_F3_NKPackType_List")]
		public override ZString JC_F3_NKPackType
		{
			get { return base.JC_F3_NKPackType; }
			set { base.JC_F3_NKPackType = value; }
		}

		#endregion

		#region ReadOnly Booleans

		protected bool JC_ContainerQuality_ReadOnly
		{
			get { return !JC_ReleaseNum.IsEmpty; }
		}

		protected bool JC_ContainerStatus_ReadOnly
		{
			get { return !JC_ReleaseNum.IsEmpty; }
		}

		protected bool JC_OA_DepartureContainerYardAddress_ReadOnly
		{
			get { return !JC_ReleaseNum.IsEmpty; }
		}

		#endregion

		public ZInt JC_Calc_ExportDetentionOverdueDays
		{
			get { return GetDetentionDays(DepartureVoyagePK(), ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Export)); }
		}

		public ZPropertyInfo JC_Calc_ExportDetentionOverdueDaysInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ExportDetentionOverdueDays); }
		}

		public ZInt JC_Calc_ImportDetentionOverdueDays
		{
			get { return GetDetentionDays(ArrivalVoyagePK(), ContainerMovementTypes.GetMovementCodesForDetention(DetentionInvoiceType.Codes.Import)); }
		}

		public ZPropertyInfo JC_Calc_ImportDetentionOverdueDaysInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ImportDetentionOverdueDays); }
		}

		IContainerPenaltyMatcherFactory penaltyMatcherFactory;
		protected IContainerPenaltyMatcherFactory PenaltyMatcherFactory => penaltyMatcherFactory ??= new ContainerPenaltyMatcherFactory();

		public ZInt JC_Calc_ExportDetentionFreeDays
		{
			get
			{
				if (jc_calc_exportDetentionFreeDays == null)
				{
					jc_calc_exportDetentionFreeDays = new CachedProperty<ZInt>(Factory, delegate
					{
						if (Booking == null)
						{
							return FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.Value.ValidFreeDays ?? ZInt.Zero;
						}
						else
						{
							var filter = new ContainerPenaltyMatchFilter
							{
								Carrier = Booking.BookedShippingLine,
								Client = Booking.Consignor,
								OriginPort = Booking.JS_RL_NKOrigin,
								DetentionPort = Booking.JS_NKLoadPort,
								ContainerClass = Container?.RC_StorageClass ?? ZString.Empty,
								Direction = ContainerDetentionDirection.Export,
								ProcessType = Core.Constants.ContainerPenaltyProcessType.Export,
								Company = GlbCompany.CurrentCompany,
								Container = this
							};

							return PenaltyMatcherFactory.MatchDetention(filter)?.FreeDays ?? ZInt.Zero;
						}
					});
				}

				return jc_calc_exportDetentionFreeDays.Value;
			}
		}
		public ZPropertyInfo JC_Calc_ExportDetentionFreeDaysInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ExportDetentionFreeDays); }
		}
		CachedProperty<ZInt> jc_calc_exportDetentionFreeDays;

		public ZInt JC_Calc_ImportDetentionFreeDays
		{
			get
			{
				if (jc_calc_ImportDetentionFreeDays == null)
				{
					jc_calc_ImportDetentionFreeDays = new CachedProperty<ZInt>(Factory, delegate
					{
						if (Booking == null)
						{
							return FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.Value.ValidFreeDays ?? ZInt.Zero;
						}
						else
						{
							var filter = new ContainerPenaltyMatchFilter
							{
								Carrier = Booking.BookedShippingLine,
								Client = Booking.Consignee,
								OriginPort = Booking.JS_RL_NKOrigin,
								DetentionPort = Booking.JS_RL_NKDestination,
								ContainerClass = Container?.RC_StorageClass ?? ZString.Empty,
								Direction = ContainerDetentionDirection.Import,
								ProcessType = Core.Constants.ContainerPenaltyProcessType.Import,
								Company = GlbCompany.CurrentCompany,
								Container = this
							};

							return PenaltyMatcherFactory.MatchDetention(filter)?.FreeDays ?? ZInt.Zero;
						}
					});
				}

				return jc_calc_ImportDetentionFreeDays.Value;
			}
		}
		public ZPropertyInfo JC_Calc_ImportDetentionFreeDaysInfo
		{
			get { return GetZPropertyInfo(Schema.JC_Calc_ImportDetentionFreeDays); }
		}
		CachedProperty<ZInt> jc_calc_ImportDetentionFreeDays;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsContainerised")]
		[ReadOnlyMember("IsContainerised")]
		[BusinessObjectTestExclude]
		[EventDateProperty(Events.GateInCode, EstimateActual.Actual)]
		public override ZDateTime JC_FCLWharfGateIn
		{
			get
			{
				ZDateTime date;

				if (IsContainerised)
				{
					var movement = GetFCLWharfGateInRelatedMovement();

					date = movement == null ? ZDateTime.Empty : movement.E9_MovementDate;
				}
				else
				{
					date = base.JC_FCLWharfGateIn;
				}

				return date;
			}
			set
			{
				if (IsContainerised)
				{
					JC_FCLWharfGateInInfo.RefreshBinding();
				}
				else
				{
					base.JC_FCLWharfGateIn = value;
					LogEvent(AutoEvents.GateIn, value.ToOffset());
				}
			}
		}

		public ZString JC_RL_NKFCLWharfGateInPort
		{
			get
			{
				var movement = GetFCLWharfGateInRelatedMovement();

				return movement == null ? ZString.Empty : movement.DepotPort;
			}
		}

		ContainerMovement GetFCLWharfGateInRelatedMovement()
		{
			ContainerMovement movement = null;

			if (Booking != null && Booking.TransportsIncludingRelated.Any())
			{
				movement = GetFirstMovement(ContainerMovementTypes.FCLReturnToWharfMovements);
			}

			return movement;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsContainerised")]
		[ReadOnlyMember("IsContainerised")]
		[BusinessObjectTestExclude]
		[EventDateProperty(AutoEvents.GateOutCode, EstimateActual.Actual)]
		public override ZDateTime JC_FCLWharfGateOut
		{
			get
			{
				ZDateTime date;
				if (IsContainerised)
				{
					var movement = GetWharfGateOutRelatedMovement();

					date = movement == null ? ZDateTime.Empty : movement.E9_MovementDate;
				}
				else
				{
					date = base.JC_FCLWharfGateOut;
				}

				return date;
			}
			set
			{
				if (IsContainerised)
				{
					JC_FCLWharfGateOutInfo.RefreshBinding();
				}
				else
				{
					base.JC_FCLWharfGateOut = value;
					LogEvent(AutoEvents.GateOut, value.ToOffset());
				}
			}
		}

		public ZString JC_RL_NKFCLWharfGateOutPort
		{
			get
			{
				var movement = GetWharfGateOutRelatedMovement();
				return movement == null ? ZString.Empty : movement.DepotPort;
			}
		}

		ContainerMovement GetWharfGateOutRelatedMovement()
		{
			ContainerMovement movement = null;
			if (Booking != null && Booking.TransportsIncludingRelated.Any())
			{
				movement = GetLastMovement(ContainerMovementTypes.Codes.WharfGateOut);
			}

			return movement;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsContainerised")]
		[ReadOnlyMember("IsContainerised")]
		[BusinessObjectTestExclude]
		public override ZDateTime JC_FCLOnBoardVessel
		{
			get
			{
				if (IsContainerised)
				{
					ContainerMovement movement = GetFirstMovement(ContainerMovementTypes.Codes.Load);
					return movement == null ? ZDateTime.Empty : movement.E9_MovementDate;
				}
				else
				{
					return base.JC_FCLOnBoardVessel;
				}
			}
			set
			{
				if (IsContainerised)
				{
					JC_FCLOnBoardVesselInfo.RefreshBinding();
				}
				else
				{
					base.JC_FCLOnBoardVessel = value;
				}
			}
		}

		public ZString JC_RL_NKFCLOnBoardVesselPort
		{
			get
			{
				ContainerMovement movement = GetFirstMovement(ContainerMovementTypes.Codes.Load);
				return movement == null ? ZString.Empty : movement.DepotPort;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsContainerised")]
		[ReadOnlyMember("IsContainerised")]
		[BusinessObjectTestExclude]
		public override ZDateTime JC_FCLUnloadFromVessel
		{
			get
			{
				if (IsContainerised)
				{
					ContainerMovement movement = GetLastMovement(ContainerMovementTypes.Codes.Discharge);
					return movement == null ? ZDateTime.Empty : movement.E9_MovementDate;
				}
				else
				{
					return base.JC_FCLUnloadFromVessel;
				}
			}
			set
			{
				if (IsContainerised)
				{
					JC_FCLUnloadFromVesselInfo.RefreshBinding();
				}
				else
				{
					base.JC_FCLUnloadFromVessel = value;
				}
			}
		}

		public ZString JC_RL_NKFCLUnloadFromVesselPort
		{
			get
			{
				ContainerMovement movement = GetLastMovement(ContainerMovementTypes.Codes.Discharge);
				return movement == null ? ZString.Empty : movement.DepotPort;
			}
		}

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZDateTime JC_ContainerYardEmptyReturnGateIn
		{
			get
			{
				var movement = ArrivalContainerYardAddress == null
					? null
					: GetLastMovement(ContainerMovementTypes.EmptyReturnMovements, ArrivalContainerYardAddress.OA_RL_NKRelatedPortCode);

				return movement == null ? ZDateTime.Empty : movement.E9_MovementDate;
			}
		}

		public ZString JC_RL_NKContainerYardEmptyReturnGateInPort
		{
			get
			{
				var movement = ArrivalContainerYardAddress == null
					? null
					: GetLastMovement(ContainerMovementTypes.EmptyReturnMovements, ArrivalContainerYardAddress.OA_RL_NKRelatedPortCode);

				return movement == null ? ZString.Empty : movement.DepotPort;
			}
		}

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public override ZDateTime JC_ContainerYardEmptyPickupGateOut
		{
			get
			{
				var movement = DepartureContainerYardAddress == null
					? null
					: GetFirstMovement(ContainerMovementTypes.Codes.YardGateOut, DepartureContainerYardAddress.OA_RL_NKRelatedPortCode);

				return movement == null ? ZDateTime.Empty : movement.E9_MovementDate;
			}
		}

		public ZString JC_RL_NKContainerYardEmptyPickupGateOutPort
		{
			get
			{
				var movement = DepartureContainerYardAddress == null
					? null
					: GetFirstMovement(ContainerMovementTypes.Codes.YardGateOut, DepartureContainerYardAddress.OA_RL_NKRelatedPortCode);

				return movement == null ? ZString.Empty : movement.DepotPort;
			}
		}

		[EventDateProperty(AutoEvents.PickedUpCode, EstimateActual.Actual, true)]
		public override ZDateTime JC_DepartureCartageComplete
		{
			get { return base.JC_DepartureCartageComplete; }
			set
			{
				if (base.JC_DepartureCartageComplete != value)
				{
					base.JC_DepartureCartageComplete = value;
					LogEvent(AutoEvents.PickedUp, value.ToOffset());
				}
			}
		}

		[List("CustomsEntryNumberType_List")]
		[MaxLength(4)]
		public ZString CustomsEntryNumberType
		{
			get { return customsEntryNumberType; }
			set
			{
				if (customsEntryNumberType != value)
				{
					CusEntryNumber cusEntryNumber = GetCusEntryNumber();
					if (value.IsEmpty)
					{
						cusEntryNumber.Delete();
						OnElementChanged();

						customsEntryNumberType = ZString.Empty;
						customsEntryNumber = ZString.Empty;
					}
					else
					{
						CheckMaximumLength(CustomsEntryNumberTypeInfo, value);
						customsEntryNumberType = value;

						if (CusEntryNumberTypes.IsExemptionCode(value))
						{
							CustomsEntryNumber = ZString.Empty;
						}

						ZString valueToSet = CMRExportExemptionCodes.Get3CharCode(value);
						cusEntryNumber.CE_EntryType = valueToSet.Left(3);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateCustomsEntryNumberType();
						Validation.ValidateCustomsEntryNumber();
					}

					CustomsEntryNumberTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CustomsEntryNumberTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsEntryNumberType); }
		}

		protected bool CustomsEntryNumberType_ReadOnly
		{
			get
			{
				return CusEntryNumbers.Count == 1 && CusEntryNumbers[0].CE_EntryIsSystemGenerated || CustomsEntryNumberType_List.Count == 0;
			}
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString CustomsEntryNumber
		{
			get { return customsEntryNumber; }
			set
			{
				if (customsEntryNumber != value)
				{
					CusEntryNumber cusEntryNumber = GetCusEntryNumber();

					CheckMaximumLength(CustomsEntryNumberInfo, value);
					customsEntryNumber = value;

					cusEntryNumber.CE_EntryNum = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateCustomsEntryNumber();
					}

					CustomsEntryNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CustomsEntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsEntryNumber); }
		}

		protected bool CustomsEntryNumber_ReadOnly
		{
			get { return CustomsEntryNumberType_ReadOnly || CusEntryNumberTypes.IsExemptionCode(CustomsEntryNumberType); }
		}

		[MaxLength(4)]
		public ZString BillContainersEntryNumberType
		{
			get
			{
				ZString result = ZString.Empty;

				if (CusEntryNumbers.Count > 0)
				{
					result = CustomsEntryNumberType;
				}
				else if (Booking.CusEntryNumbers.Count > 0)
				{
					result = Booking.CustomsEntryNumberType;
				}

				return result;
			}
		}

		public ZPropertyInfo BillContainersEntryNumberTypeInfo
		{
			get { return GetZPropertyInfo(Schema.BillContainersEntryNumberType); }
		}

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString BillContainersEntryNumber
		{
			get { return CusEntryNumbers.Count > 0 ? CustomsEntryNumber : Booking.CustomsEntryNumber; }
		}

		public ZPropertyInfo BillContainersEntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.BillContainersEntryNumber); }
		}

		protected bool BillContainersEntryNumber_ReadOnly
		{
			get { return CusEntryNumberTypes.IsExemptionCode(CustomsEntryNumberType); }
		}

		#region JC_ContainerMode

		[BusinessObjectTestExclude]
		public override ZString JC_ContainerMode
		{
			get { return base.JC_ContainerMode; }
			set
			{
				if (base.JC_ContainerMode != value)
				{
					base.JC_ContainerMode = JC_ContainerMode_List.ContainsCode(value) ? value : ZString.Empty;

					if (!IsValidationSuspended)
					{
						Validation.ValidateJC_ContainerNum();
					}

					if (Booking != null)
					{
						Booking.MarkAsNeedingValidation();
					}
				}
			}
		}

		public bool IsRollOnRollOff
		{
			get { return JC_ContainerMode == Constants.ContainerModes.RollOnRollOff; }
		}

		public bool IsTopLevelPack
		{
			get { return AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes.Contains(JC_ContainerMode); }
		}

		public bool SupportsPackLines
		{
			get { return IsFCL; }
		}

		#endregion

		#region Stowage Position

		public override ZString JC_StowagePosition
		{
			get { return base.JC_StowagePosition; }
			set
			{
				ZString result = value;

				if (!IsTopLevelPack && (result.Length == 5 || result.Length == 6))
				{
					result = result.PadLeft(7, '0');
				}

				base.JC_StowagePosition = result;
			}
		}

		#endregion

		#region JC_ArrivalCartageComplete

		[EventDateProperty(AutoEvents.DeliveredCode, EstimateActual.Actual)]
		public override ZDateTime JC_ArrivalCartageComplete
		{
			get { return base.JC_ArrivalCartageComplete; }
			set
			{
				if (base.JC_ArrivalCartageComplete != value)
				{
					base.JC_ArrivalCartageComplete = value;
					// Assuming that value is local to current branches timezone
					Logs.CreateRecreateOrUpdateEventLog(AutoEvents.Delivered, EstimateActual.Actual, value.ToOffset());
				}
			}
		}

		#endregion

		#region VGM Fields

		protected bool JC_GrossWeightVerificationType_ReadOnly
		{
			get
			{
				if (Globals.IsWeb)
				{
					return false;
				}
				else
				{
					return HasEmptyContainerNumber;
				}
			}
		}

		[ReadOnlyMember(nameof(HasEmptyContainerNumber))]
		public override ZString GrossWeightVerifiedByNameOrPK
		{
			get { return GrossWeightVerifiedByAddress.OrganisationNameOrPK; }
			set { GrossWeightVerifiedByAddress.OrganisationNameOrPK = value; }
		}

		protected bool JC_GrossWeightVerificationDateTime_ReadOnly
		{
			get
			{
				if (Globals.IsWeb)
				{
					return false;
				}
				else
				{
					return HasEmptyContainerNumber;
				}
			}
		}

		public bool HasEmptyContainerNumber
		{
			get { return JC_ContainerNum.IsEmpty; }
		}

		[List("Lookups.GrossWeightVerifiedByList")]
		public ZGuid GrossWeightVerifiedByPK
		{
			get { return GrossWeightVerifiedByAddress.OrganisationPK; }
			set { GrossWeightVerifiedByAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo GrossWeightVerifiedByPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GrossWeightVerifiedByPK, x => GrossWeightVerifiedByAddress.OrganisationPKInfo); }
		}

		#endregion

		#region JC_EmptyReturnedBy

		public override bool RequireCalculateJC_EmptyReturnedBy
		{
			get
			{
				var transport = Booking?.TransportsIncludingRelated?.ArrivalTransport;
				return JC_FCLWharfGateOutInfo.HasChanges || JC_FCLUnloadFromVesselInfo.HasChanges || (transport != null && transport.JW_TerminalAvailabilityDateInfo.HasChanges);
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override JobContainerValidation GetNewValidation()
		{
			AgencyShipmentContainerValidation result = null;
			if (IsRollOnRollOff)
			{
				result = new AgencyRORContainerValidation(this);
			}
			else if (IsTopLevelPack)
			{
				result = new AgencyTopLevelPackValidation(this);
			}
			else
			{
				result = GetNewContainerValidation();
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia)
				{
					result.Add(new AgencyShipmentContainerAUValidation(this));
				}
			}

			return result;
		}

		protected virtual AgencyShipmentContainerValidation GetNewContainerValidation()
		{
			return new AgencyShipmentContainerValidation(this);
		}

		public new AgencyShipmentContainerValidation Validation
		{
			get { return (AgencyShipmentContainerValidation)base.Validation; }
		}

		#endregion

		#region Related Business Objects

		public RefContainerStock Stock
		{
			get { return RefContainerStock.Load(Factory, JC_ContainerNum); }
		}

		public new AgencyShipment Booking
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyShipment)base.Booking; }
		}
		protected override Type ShipmentType
		{
			get { return typeof(AgencyShipment); }
		}

		[ChildEditable(true)]
		public new AgencyShipmentPackLineManyToManyCollection PackLines
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (AgencyShipmentPackLineManyToManyCollection)base.PackLines; }
		}
		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new AgencyShipmentPackLineManyToManyCollection(this);
		}

		public ContainerMovementCollection Movements
		{
			get
			{
				if (movements == null)
				{
					movementsRelationship = new ContainerMovementRelationship(this);
					movements = new ContainerMovementCollection(Factory, false, movementsRelationship);
				}
				return movements;
			}
		}
		public void RefreshMovements()
		{
			if (movementsRelationship != null)
			{
				movementsRelationship.UpdateFilters();
			}

			if (movements != null)
			{
				movements.Deactivate();
			}
		}
		ContainerMovementRelationship movementsRelationship;
		ContainerMovementCollection movements;

		[ChildEditable(false)]
		public CusEntryNumCollection CusEntryNumbers
		{
			get
			{
				if (cusEntryNumbers == null)
				{
					cusEntryNumbers = GetLoadedCusEntryNumbers();

					cusEntryNumbers.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusEntryNumbers);
				}

				return cusEntryNumbers;
			}
		}

		#region NoteTypes

		protected override NoteTypeCollection AdditionalNoteTypes
		{
			get { return new NoteTypeCollection(); }
		}

		#endregion

		#endregion

		#region Lookups

		#region JC_GrossWeightUQ_List

		public CodeDescriptionPairList JC_GrossWeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region JC_GrossVolumeUQ_List

		public CodeDescriptionPairList JC_GrossVolumeUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region JC_TotalUnitOfMeasure_List

		public CodeDescriptionPairList JC_TotalUnitOfMeasure_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		#endregion

		#region ContainerCommodityCode_List

		public override RefCommodityCodeCollection ContainerCommodityCode_List
		{
			get { return IsTopLevelPack ? BindingLists.RefCommodityCode_List : base.ContainerCommodityCode_List; }
		}

		protected override RefCommodityCodeCollection GetNewContainerCommodityCodeListCore()
		{
			return new AgencyContainerCommodityCodeCollection(Factory);
		}

		#endregion

		public CodeDescriptionPairList CustomsEntryNumberType_List
		{
			get
			{
				if (customsEntryNumberTypeList == null)
				{
					customsEntryNumberTypeList = new CodeDescriptionPairList();
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia && Booking != null)
					{
						customsEntryNumberTypeList.AddRange(CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Booking.IsImport()));
					}

					customsEntryNumberTypeList.AddPairsIfNotExist(FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value);
				}
				return customsEntryNumberTypeList;
			}
		}

		public override CodeDescriptionPairList JC_ContainerMode_List
		{
			get
			{
				return Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "AgencyShipmentContainer.JC_ContainerMode_List_{0}", Booking != null ? Booking.JS_PackingMode : ZString.Empty), // Factory cache key
					() => new AgencyShipmentContainerModeList(Booking != null ? Booking.JS_PackingMode : ZString.Empty));
			}
		}

		public RefPackTypeCollection JC_F3_NKPackType_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		protected override ICodeDescriptionPairList GetDeliveryMode_ListCore()
		{
			return FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ToCodeDescription();
		}

		protected override ICodeDescriptionPairList GetUserDefinedDeliveryMode_List()
		{
			return FreightDataRegistry.Instance.ContainerDeliveryModeList.Value.ToUserDefinedCodeDescription();
		}

		#endregion

		#region NewContainerYardDefaultStrategy

		protected override IContainerDefaultingStrategy NewContainerDefaultingStrategyCore()
		{
			return new AgencyShipmentContainerDefaultingStrategy(this);
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				MultilingualString reason;
				return CheckCanDelete(out reason);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason;
				return CheckCanDelete(out reason) ? (NoResString)string.Empty : reason;
			}
		}

		bool CheckCanDelete(out MultilingualString reason)
		{
			reason = (NoResString)string.Empty;
			return CanDelete_EIDO(ref reason)
				&& CanDelete_ContainerRelease(ref reason);
		}
		bool CanDelete_ContainerRelease(ref MultilingualString reason)
		{
			if (!JC_ReleaseNum.IsEmpty)
			{
				reason = ResString.GetMultilingualString("058878f3-5d3d-4ceb-8379-a8bb6e219438", "This container has been released, you will need to replace the release without this container before you can delete it.");
				return false;
			}

			return true;
		}
		bool CanDelete_EIDO(ref MultilingualString reason)
		{
			switch (CalculateEIDOStatus(true))
			{
				case ReleaseImportOrderMessageStatusList.Codes.Withdrawn:
				case ReleaseImportOrderMessageStatusList.Codes.NotSent:
					return true;

				default:
					reason = ResString.GetMultilingualString("e73ed665-b552-47ff-8aec-2fe1ade40a62", "A container can only be deleted if no E-IDO messages have been successfully sent for it or if it has been successfully withdrawn.");
					return false;
			}
		}

		#endregion

		#region IProcessHandlingInfoProvider

		public ProcessHandlingInfo ProcessHandlingInfo
		{
			get { return new AgencyShipmentContainerProcessHandlingInfo(this); }
		}

		#endregion

		#region Implementation

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase || JC_ContainerNumInfo.HasChanges)
			{
				UpdateContainerMovementEvents();
			}
		}

		internal void UpdateContainerMovementEvents(ContainerMovement movementDeleting = null)
		{
			var containerSynchroniser = new AgencyShipmentContainerMovementEventSynchroniser(this);
			containerSynchroniser.UpdateContainerMovementEvents(movementDeleting);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new AgencyShipmentContainerFetchStrategy(this);
		}

		internal ContainerStockManager StockManager
		{
			get { return stockManager ?? (stockManager = ContainerStockManager.New(this)); }
		}
		ContainerStockManager stockManager;

		#region Movements

		ContainerMovement GetFirstMovement(string typeOfMovement, string depot = null)
		{
			return GetFirstMovement(new[] { typeOfMovement }, depot);
		}

		ContainerMovement GetFirstMovement(string[] typesOfMovement, string depot = null)
		{
			return OrderedMovementDateOfType((m1, m2) => m1.E9_MovementDate.CompareTo(m2.E9_MovementDate), typesOfMovement, depot);
		}

		ContainerMovement GetLastMovement(string typeOfMovement, string depot = null)
		{
			return GetLastMovement(new[] { typeOfMovement }, depot);
		}

		ContainerMovement GetLastMovement(string[] typesOfMovement, string depot = null)
		{
			return OrderedMovementDateOfType((m1, m2) => m2.E9_MovementDate.CompareTo(m1.E9_MovementDate), typesOfMovement, depot);
		}

		ContainerMovement OrderedMovementDateOfType(Comparison<ContainerMovement> comparison, string[] movementTypes, string location = null)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, movementTypes);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementDate, SQLComparisonOperator.NotEqual, null);

			ContainerMovement selected = null;
			foreach (ContainerMovement current in Movements.Find(filter))
			{
				if (location == null || current.DepotPort == location)
				{
					if (selected == null || comparison(selected, current) > 0)
					{
						selected = current;
					}
				}
			}

			return selected;
		}

		#endregion

		ZInt GetDetentionDays(ZGuid voyagePK, string[] movementTypes)
		{
			ZInt result = 0;

			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobContainerMoveSchema.E9_JV, voyagePK);
			filter.AddToFilter(JobContainerMoveSchema.E9_MovementType, movementTypes);

			foreach (ContainerMovement movement in Movements.Find(filter))
			{
				ZInt detentionDays = movement.E9_DetentionDays;
				if (detentionDays > result)
				{
					result = detentionDays;
				}
			}

			return result;
		}

		ZGuid DepartureVoyagePK()
		{
			AgencyShipment shipment;

			if ((shipment = Booking) == null)
			{
				return ZGuid.Empty;
			}
			else
			{
				return GetVoyagePK(shipment.TransportsIncludingRelated.DepartureTransport);
			}
		}

		ZGuid ArrivalVoyagePK()
		{
			AgencyShipment shipment;

			if ((shipment = Booking) == null)
			{
				return ZGuid.Empty;
			}
			else
			{
				return GetVoyagePK(shipment.TransportsIncludingRelated.ArrivalTransport);
			}
		}

		ZGuid GetVoyagePK(Transport transport)
		{
			JobVoyage voyage;
			JobSailing sailing;

			if (transport == null ||
				!transport.JW_IsLinked ||
				(sailing = transport.Sailing) == null ||
				(voyage = sailing.Voyage) == null)
			{
				return ZGuid.Empty;
			}
			else
			{
				return voyage.PK;
			}
		}

		CusEntryNumCollection cusEntryNumbers;
		ZString customsEntryNumberType;
		CodeDescriptionPairList customsEntryNumberTypeList;
		ZString customsEntryNumber;

		CusEntryNumCollection GetLoadedCusEntryNumbers()
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			CusEntryNumCollection result = new CusEntryNumCollection(Factory, query);
			result.Load();

			if (result.Count > 0)
			{
				customsEntryNumberType = CMRExportExemptionCodes.Get4CharCode(result[0].CE_EntryType);
				customsEntryNumber = result[0].CE_EntryNum;
			}

			return result;
		}

		CusEntryNumber GetCusEntryNumber()
		{
			CusEntryNumber result;

			if (CusEntryNumbers.Count == 0)
			{
				result = CusEntryNumbers.AddNew();
				result.CE_ParentTable = TableName;
				result.CE_EntryType = CusEntryNumberTypes.CountrySpecificDefaultEntryNumberType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Booking.IsImport());
				result.CE_EntryIsSystemGenerated = false;
				result.CE_ParentID = PK;
				result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				customsEntryNumberType = CMRExportExemptionCodes.Get4CharCode(result.CE_EntryType);
				customsEntryNumber = result.CE_EntryNum;
			}
			else
			{
				result = CusEntryNumbers[0];
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JC_ContainerMode = Constants.ContainerModes.FCL;
			JC_GrossWeightUQ = DefaultWeightUnit;
			JC_GrossVolumeUQ = DefaultVolumeUnit;
		}

		protected virtual ZString DefaultWeightUnit
		{
			get { return AgencyRegistry.Instance.DefaultBillWeightUnit.Value; }
		}

		protected virtual ZString DefaultVolumeUnit
		{
			get { return AgencyRegistry.Instance.DefaultBillVolumeUnit.Value; }
		}

		public override void Delete()
		{
			if (cusEntryNumbers != null)
			{
				UnRegisterEditableChildObject(cusEntryNumbers);
				cusEntryNumbers.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo NewDocManagerInfo()
		{
			return new DocManagerInfo(this, Constants.DocManagerCodes.AgencyBillContainers);
		}

		#endregion

		#region IWorkflowProvider overrides

		protected override ZString WorkFlowTypeCore
		{
			get { return WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode; }
		}

		protected override ContainerProcessTaskCollection NewWorkflowItemsCollection()
		{
			return new AgencyContainerProcessTaskCollection(this);
		}

		protected override IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			ColumnValueRanker result = new ColumnValueRanker();

			if (JC_Purpose == ContainerBookedStatus.Codes.Booked && Booking != null && Booking.IsBillOfLadingStage)
			{
				result.Add(ProcessTaskTemplateSchema.PK, ZGuid.Empty); // No templates to be matched for booked containers on BOL
			}
			else
			{
				JobHeader job = null;

				if (Booking != null)
				{
					job = new JobHeader.Loader(Booking).Load(true, false);
				}

				ZString loadPort = Booking != null ? Booking.JS_NKLoadPort : ZString.Empty;
				ZString dischargePort = Booking != null ? Booking.JS_NKDischargePort : ZString.Empty;

				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, Booking != null ? Booking.JS_PackingMode : ZString.Empty, ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_GB, job != null ? job.JH_GB : GlbBranch.CurrentBranch.PK, ZGuid.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_GE, job != null ? job.JH_GE : GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, loadPort, loadPort.SubstringSafe(0, 2), ZString.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, dischargePort, dischargePort.SubstringSafe(0, 2), ZString.Empty);
			}

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			List<IZType> result = new List<IZType>();

			if (Booking != null)
			{
				result.AddRange(Booking.GetClientsInTemplateSelectionOrder());
			}

			return result.ToArray();
		}

		#endregion

		#region Dangerous goods

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (dangerousGoods == null)
				{
					dangerousGoods = Booking != null
						? new AgencyUNDGDataItemCollection(this, Booking.Consignee, Booking.Consignor)
						: new AgencyUNDGDataItemCollection(this);

					RegisterEditableChildObject(dangerousGoods);
				}
				return dangerousGoods;
			}
		}

		UNDGDataItemCollection dangerousGoods;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public override ZBool HasHazardous
		{
			get { return base.HasHazardous || UNDGs.Count > 0; }
		}

		#endregion

		#region Event Parameters

		public override IDictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = base.GetParametersForEvent(eventType);

			if (eventType == Events.FreightLoaded || eventType == Events.FreightUnloaded)
			{
				parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] = GetEventLocation(eventType.Code);
			}
			else if (eventType == Events.GateIn)
			{
				parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] = GetEventLocation(Events.GateIn.Code);
				parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility] = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
			}
			else if (eventType == Events.GateOut)
			{
				parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] = GetEventLocation(Events.GateOut.Code);
				parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility] = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
			}

			return parameters;
		}

		protected override string GetReferenceFreeTextForEvent(string eventCode)
		{
			if (eventCode != Events.FreightLoadedCode && eventCode != Events.FreightUnloadedCode && eventCode != Events.GateInCode && eventCode != Events.GateOutCode)
			{
				return GetEventLocation(eventCode);
			}

			return base.GetReferenceFreeTextForEvent(eventCode);
		}

		ZString GetEventLocation(string eventCode)
		{
			ZString result = ZString.Empty;

			switch (eventCode)
			{
				case Events.PickedUpCode: // PUP
					if (Booking != null)
					{
						result = Booking.JS_RL_NKOrigin;
					}
					break;

				case Events.DeliveredCode: // DLV
					if (Booking != null)
					{
						result = Booking.JS_RL_NKDestination;
					}
					break;

				case Events.GateInCode:

					var firstSeaLeg = Booking != null ? Booking.TransportsIncludingRelated.FirstLegMatching(l => l.IsSea) : null;
					result = firstSeaLeg != null ? firstSeaLeg.JW_RL_NKLoadPort : null;

					break;

				case Events.GateOutCode:

					var lastSeaLeg = Booking != null ? Booking.TransportsIncludingRelated.LastLegMatching(l => l.IsSea) : null;
					result = lastSeaLeg != null ? lastSeaLeg.JW_RL_NKDiscPort : null;

					break;

				default:
					result = IsFCL ? GetFCLEventLocation(eventCode) : GetTopLevelPackEventLocation(eventCode);
					break;
			}

			return result;
		}

		ZString GetFCLEventLocation(string eventCode)
		{
			ZString result = ZString.Empty;

			switch (eventCode)
			{
				case Events.FreightLoadedCode: // FLO
					result = JC_RL_NKFCLOnBoardVesselPort;
					break;

				case Events.FreightUnloadedCode: // FUL
					result = JC_RL_NKFCLUnloadFromVesselPort;
					break;
			}

			return result;
		}

		ZString GetTopLevelPackEventLocation(string eventCode)
		{
			ZString result = ZString.Empty;

			switch (eventCode)
			{
				case Events.FreightLoadedCode: // FLO
					var firstSeaLeg = Booking != null
						? Booking.TransportsIncludingRelated.FirstLegMatching(leg => leg.IsSea)
						: null;

					if (firstSeaLeg != null)
					{
						result = firstSeaLeg.JW_RL_NKLoadPort;
					}
					break;

				case Events.FreightUnloadedCode: // FUL
					var lastSeaLeg = Booking != null
						? Booking.TransportsIncludingRelated.LastLegMatching(leg => leg.IsSea)
						: null;

					if (lastSeaLeg != null)
					{
						result = lastSeaLeg.JW_RL_NKDiscPort;
					}
					break;
			}

			return result;
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		protected override ZDecimal GetRoundedValueCore(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForShipping.GetRoundedValue(this, column, property, value);
		}

		#endregion
	}
}
