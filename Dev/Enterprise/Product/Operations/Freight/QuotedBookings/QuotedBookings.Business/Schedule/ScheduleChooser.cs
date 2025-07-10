using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ScheduleChooser : NonPersistentBusinessObject,
		IObsoleteValidation,
		IMAWBAllocationParent,
		ISailingParentFindBox,
		IVoyageFinderParent
	{
		/// <summary>
		/// This constructor is here only for binding purposes. Like Factory.GetNull() on a BusinessObject.
		/// When there is no booking we still need to create one of these so binding works - otherwise we get null reference exceptions.
		/// </summary>
		/// <param name="factory"></param>
		[Obsolete("Should only be used through reflection because of binding!", false)]
		public ScheduleChooser(BusinessObjectFactory factory)
			: this(new NullSailingChooserParent(factory))
		{
		}

		public ScheduleChooser(ISailingChooserParent parent)
			: base(parent.Factory)
		{
			Parent = parent;
			HookEvents();

			if (IsAir)
			{
				MasterBillMAWB_ReadOnly = Parent.IsNeutralMaster;
				MasterBillAirlinePrefix_ReadOnly = MAWBAllocation.IsMAWBPrinted;
				Parent.SetIsNeutralMasterReadOnly(MAWBAllocation.NeutralReadOnly);
			}
		}

		public ISailingChooserParent Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		ISailingChooserParent parent;

		#region Schema

		public static class Schema
		{
			public const string MasterBillAirlinePrefix = "MasterBillAirlinePrefix";
			public const string MasterBillMAWB = "MasterBillMAWB";
			public const string MasterBillNeutralMAWB = "MasterBillNeutralMAWB";
			public const string AWBServiceLevel = "AWBServiceLevel";
		}

		#endregion

		#region HookEvents

		void HookEvents()
		{
			if (ParentHasUnconvertedBooking)
			{
				Parent.SailingJXChanged += new EventHandler(SailingJXInfo_ValueChanged);
				Parent.AWBServiceLevelChanged += new EventHandler(AWBServiceLevelInfo_ValueChanged);
				Parent.IsNeutralMasterChanged += new EventHandler(IsNeutralMasterInfo_ValueChanged);
				Parent.IsDirectChanged += new EventHandler(IsDirectInfo_ValueChanged);
				Parent.TransportModeChanged += new EventHandler(TransportModeInfo_ValueChanged);
				Parent.BookedDateChanged += new EventHandler(BookedInfo_ValueChanged);
			}
		}

		bool ParentHasUnconvertedBooking => !Parent.Booking?.JS_IsForwardRegistered ?? false;

		#endregion

		#region Events

		void SailingJXInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!ParentHasUnconvertedBooking
				|| Sailing == null)
			{
				return;
			}

			if (Parent.ReservedMasterBill.IsEmpty && (IsLCL || IsAir))
			{
				Parent.ReservedMasterBill = Sailing.JX_ReservedMasterBill;
			}

			if (!MAWBAllocation.IsMAWBPrinted && IsAir && IsValidForNeutralMaster)
			{
				SetMasterBillPrefixWithAirlineCode();
			}

			if (!ImportExportHelper.IsBranchCountry(Parent.LoadPort))
			{
				Parent.IsNeutralMaster = false;
			}
			MAWBAllocation.MarkForReallocation(checkIsNeutralAndNotPrinted: true);

			if ((IsFCL || IsLCL) && Parent.Carrier.IsEmpty && Sailing != null && Sailing.Voyage != null)
			{
				Parent.Carrier = Sailing.Voyage.JV_OH_Line;
			}
		}

		void SetMasterBillPrefixWithAirlineCode()
		{
			RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Factory, TwoLetterAirlineCode);
			if (airline != null)
			{
				MasterBillAirlinePrefix = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
			}
		}

		void AWBServiceLevelInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentHasUnconvertedBooking)
			{
				MAWBAllocation.MarkForReallocation(checkIsNotPrinted: true);
			}
		}

		void IsDirectInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!ParentHasUnconvertedBooking)
			{
				return;
			}

			Parent.MawbNumber = ZString.Empty;

			if (!MAWBAllocation.IsMAWBPrinted)
			{
				Parent.SetIsNeutralMasterReadOnly(MAWBAllocation.NeutralReadOnly);

				if (IsValidForNeutralMaster)
				{
					SetMasterBillPrefixWithAirlineCode();
				}
				else
				{
					Parent.IsNeutralMaster = false;
				}

				IsDirectOrIsNeutralChanged();
			}
		}

		void IsNeutralMasterInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!ParentHasUnconvertedBooking)
			{
				return;
			}

			if (Parent.IsNeutralMaster)
			{
				if (MAWBAllocation.AllocatedMawb != null && MasterBillAirlinePrefix == MAWBAllocation.AllocatedMawb.JM_Airline3DigitPrefix)
				{
					Parent.MawbNumber = MAWBAllocation.AllocatedMAWBNumber;
					MAWBAllocation.PendingAllocation = false;
				}
				else
				{
					if (MAWBAllocation.AllocatedMawb != null)
					{
						MAWBAllocation.PendingAllocation = true;
					}
					else
					{
						MAWBAllocation.MarkForAllocation(checkForNull: true);
						MasterBillMAWB = "";
					}
				}
			}
			else
			{
				MasterBillMAWB = "";
				MAWBAllocation.MarkForDeallocation();
			}
			IsDirectOrIsNeutralChanged();
		}

		void IsDirectOrIsNeutralChanged()
		{
			MasterBillMAWB_ReadOnly = Parent.IsNeutralMaster && IsValidForNeutralMaster;
			if (!Parent.IsNeutralMaster)
			{
				MasterBillAirlinePrefix_ReadOnly = false;
				if (!Parent.IsDirect)
				{
					Parent.MawbNumber = "";
				}
			}
			((BusinessObject)Parent).RefreshBinding();
		}

		void TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentHasUnconvertedBooking)
			{
				MAWBAllocation.MarkForReallocation();
				GetNextSailingOrClearSailings();
			}
		}

		void BookedInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentHasUnconvertedBooking)
			{
				GetNextSailingOrClearSailings();
			}
		}

		#endregion

		#region Non persistent Properties

		#region MasterBillAirlinePrefix

		[BusinessObjectTestExclude()]
		[MaxLength(3)]
		public ZString MasterBillAirlinePrefix
		{
			get { return Parent.MawbNumber.Left(3); }
			set
			{
				if (MasterBillAirlinePrefix != value)
				{
					CheckMaximumLength(MasterBillAirlinePrefixInfo, value);

					Parent.MawbNumber = value + MasterBillMAWB;

					if (Parent.IsNeutralMaster)
					{
						if (MAWBAllocation.MarkForReallocationIfPrefixChanged(value))
						{
							Parent.MawbNumber = value;
						}
						else
						{
							Parent.IsNeutralMaster = false;
						}
					}

					if (!IsValidationSuspended)
					{
						ValidateMasterBillAirlinePrefix();
					}
				}
				MasterBillAirlinePrefixInfo.RefreshBinding();
				MasterBillNeutralMAWBInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MasterBillAirlinePrefixInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillAirlinePrefix); }
		}

		public bool MasterBillAirlinePrefix_ReadOnly
		{
			get;
			set;
		}

		#endregion

		#region MasterBillMAWB

		[BusinessObjectTestExclude()]
		[MaxLength(8)]
		public ZString MasterBillMAWB
		{
			get { return Parent.MawbNumber.SubstringSafe(3); }
			set
			{
				if (MasterBillMAWB != value)
				{
					CheckMaximumLength(MasterBillMAWBInfo, value);
					Parent.MawbNumber = MasterBillAirlinePrefix + value;

					if (!IsValidationSuspended)
					{
						ValidateMasterBillMAWB();
					}
				}
				MasterBillMAWBInfo.RefreshBinding();
				MasterBillNeutralMAWBInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MasterBillMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillMAWB); }
		}

		public bool MasterBillMAWB_ReadOnly
		{
			get;
			set;
		}

		#endregion

		#region AWBServiceLevel

		[List("NeutralAirWaybillServiceLevelList")]
		[MaxLength(CommonShipment.Schema.JS_AWBServiceLevelMaxLength)]
		public ZString AWBServiceLevel
		{
			get { return Parent.AWBServiceLevel; }
			set
			{
				if (AWBServiceLevel != value)
				{
					CheckMaximumLength(AWBServiceLevelInfo, value);
					Parent.AWBServiceLevel = value;

					if (!IsValidationSuspended)
					{
						ValidateAWBServiceLevel();
					}
				}
				AWBServiceLevelInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AWBServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.AWBServiceLevel); }
		}

		#endregion

		#region MasterBillNeutralMAWB

		public ZString MasterBillNeutralMAWB
		{
			get
			{
				return MAWBAllocation.MasterBillNeutralMAWB;
			}
		}

		public ZPropertyInfo MasterBillNeutralMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.MasterBillNeutralMAWB); }
		}

		#endregion

		#endregion

		#region Validation

		#region ValidateMasterBillAirlinePrefix

		public void ValidateMasterBillAirlinePrefix()
		{
			MasterBillAirlinePrefixInfo.ClearAllNotifications();

			ZString flight = Sailing != null ? Sailing.JX_JV_VoyageFlight : ZString.Empty;
			if (IsAir && flight.Length >= 2)
			{
				RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Factory, TwoLetterAirlineCode);
				if (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != MasterBillAirlinePrefix)
				{
					MasterBillAirlinePrefixInfo.AddWarning(Res.GetString("a3e867cb-155d-4424-9963-00fb3a7edb24", "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number."));
				}
			}
		}

		#endregion

		#region ValidateMasterBillMAWB

		bool IsDuplicateMAWB()
		{
			return MasterBillValidator.IsDuplicate(Parent.Booking,
				ZDateTime.Now.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value),
				ZDateTime.Empty);
		}

		public void ValidateMasterBillMAWB()
		{
			MasterBillMAWBInfo.ClearAllNotifications();

			if (IsAir)
			{
				MasterBillValidator.ValidateMAWB(this, IsDuplicateMAWB);
				MasterBillValidator.ValidateNeutralMAWB(this);
				MasterBillNeutralMAWBInfo.RefreshBinding();
			}
		}

		#endregion

		#region ValidateAWBServiceLevel

		public void ValidateAWBServiceLevel()
		{
			AWBServiceLevelInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AWBServiceLevelInfo, NeutralAirWaybillServiceLevelList);
		}

		#endregion

		#endregion

		#region Lists

		public RefUNLOCOCollection RefUNLOCO_List
		{
			get
			{
				if (fRefUNLOCO_List == null)
				{
					fRefUNLOCO_List = new RefUNLOCOCollection(Factory);
				}
				return fRefUNLOCO_List;
			}
		}
		RefUNLOCOCollection fRefUNLOCO_List;

		OrgHeader RelevantCarrier
		{
			get
			{
				OrgHeader result = null;
				if (IsFCL)
				{
					result = Factory.Load<OrgHeader>(Parent.Carrier);
				}
				else if (Sailing != null && Sailing.Voyage != null)
				{
					result = Sailing.Voyage.Line;
				}
				return result;
			}
		}

		public OrgCarrierServiceLevelCollection NeutralAirWaybillServiceLevelList
		{
			get
			{
				if (fCarrierServiceLevels == null || fCarrierServiceLevels.Master != RelevantCarrier)
				{
					if (RelevantCarrier == null)
					{
						fCarrierServiceLevels = new OrgCarrierServiceLevelCollection(Factory);
					}
					else
					{
						fCarrierServiceLevels = new OrgCarrierServiceLevelCollection(RelevantCarrier.MiscServ);
					}
					fCarrierServiceLevels.Load();
				}

				return fCarrierServiceLevels;
			}
		}

		OrgCarrierServiceLevelCollection fCarrierServiceLevels;

		#endregion

		#region Sailing

		public JobSailing Sailing
		{
			get { return Factory.Load<JobSailing>(Parent.SailingJX); }
		}

		#endregion

		#region GetNextSailing

		void GetNextSailingOrClearSailings()
		{
			if (CanSearchForSailing())
			{
				GetNextSailing();
			}
			else
			{
				Parent.SailingJX = ZGuid.Empty;
			}
		}

		bool UserWantsToAutopopulateSailing()
		{
			if (!Globals.CanShowDialogs || Parent.Factory.IsInTransaction)
			{
				return true;
			}

			var context = new DialogDefaultContext(
				new ZGuid("00052C77-2008-46FA-BBEF-F9C39804E184"),
				ResString.GetMultilingualString("DD47FD13-17D7-445A-9840-97A892F941A0", "Automatically populate {0}?", SailingText),
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Question);

			var message = ResString.GetMultilingualString("62B8A5FA-F86E-4ED4-80CB-CBFB41E13FF6", "Would you like to populate the latest {0} schedule?", SailingText);
			return Globals.Message.ShowOrDefault(context, message) == ZDialogResult.Yes; // Cant apply hooks since the parent and ScheduleChooser are swapping around all the time
		}

		bool CanSearchForSailing()
		{
			return (!Parent.LoadPort.IsEmpty &&
				!Parent.DischargePort.IsEmpty &&
				!IsParentBookingImportingData &&
				(Parent.ETD.IsValidSmallDateTime || Parent.BookedDate.IsValidSmallDateTime) &&
				!Parent.TransportMode.IsEmpty);
		}

		void GetNextSailing()
		{
			var relatedSailings = RetrieveRelatedSailings();

			if (!Parent.SailingJX.IsValid || !relatedSailings.Contains(Parent.SailingJX))
			{
				if (relatedSailings != null && relatedSailings.Count > 0 && UserWantsToAutopopulateSailing())
				{
					SetNextSailing(relatedSailings[0].PK);
				}
				else
				{
					Parent.SailingJX = ZGuid.Empty;
				}
			}
		}

		protected virtual void SetNextSailing(ZGuid nextSailingPK)
		{
			Parent.SailingJX = nextSailingPK;
		}

		JobSailingCollection RetrieveRelatedSailings()
		{
			JobSailingCollection sailing_List = null;

			if (!Parent.LoadPort.IsEmpty &&
				!Parent.DischargePort.IsEmpty &&
				(Parent.ETD.IsValidSmallDateTime || Parent.BookedDate.IsValidSmallDateTime))
			{
				SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
				builder.TransportMode = Parent.TransportMode;
				builder.LoadPort = Parent.LoadPort;
				builder.DischargePort = Parent.DischargePort;

				builder.SetDateRange(SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateInRange, Parent.ETD.IsValidSmallDateTime ? Parent.ETD.Date : Parent.BookedDate.Date, ZDateTime.Empty);

				ZQuery cutOffsQuery = new ZQuery();
				if (Parent.BookedDate.IsValidSmallDateTime)
				{
					if (Parent.ContainerMode == Constants.ContainerModes.FCL)
					{
						ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
						originFilter.AddToFilter(JoinCondition.And, JobVoyOriginSchema.JA_CutOff, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.BookedDate);
						originFilter.AddToFilter(JoinCondition.Or, JobVoyOriginSchema.JA_CutOff, SQLComparisonOperator.Equal, null);

						ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
						sailingFilter.AddSubQuery(originFilter, JoinCondition.And);

						cutOffsQuery.AddToFilter(sailingFilter);
					}
					else
					{
						cutOffsQuery.AddToFilter(JoinCondition.And, JobSailingSchema.JX_DepotCutOff, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.BookedDate);
						cutOffsQuery.AddToFilter(JoinCondition.Or, JobSailingSchema.JX_DepotCutOff, SQLComparisonOperator.Equal, null);
					}
				}

				ZQuery filter = builder.ToSailingFilter();
				filter.AddToFilter(cutOffsQuery, JoinCondition.And);
				filter.AddToFilter(JobSailingSchema.JX_IsPublished, ZBool.True);

				sailing_List = new JobSailingCollection(Factory, filter);
				sailing_List.Load();

				sailing_List.Sort(JobSailing.Schema.JX_JA_E_DEP, ListSortDirection.Ascending);
			}

			return sailing_List;
		}

		#endregion

		#region IMAWBAllocationParent Members

		public MAWBAllocation MAWBAllocation
		{
			get { return mawbAllocation ?? (mawbAllocation = new MAWBAllocation(this)); }
		}
		MAWBAllocation mawbAllocation;

		ZString IMAWBAllocationParent.AWBServiceLevel
		{
			get { return Parent.AWBServiceLevel; }
		}

		ZString IMAWBAllocationParent.MawbBookingReference
		{
			get { return ZString.Empty; }
		}

		public ZBool IsValidForNeutralMaster
		{
			get { return Parent.IsDirect; }
		}

		ZBool IMAWBAllocationParent.IsNeutralMaster
		{
			get { return Parent.IsNeutralMaster; }
			set { Parent.IsNeutralMaster = value; }
		}

		ZString IMAWBAllocationParent.MasterBill
		{
			get { return Parent.MawbNumber; }
		}

		public ZString TwoLetterAirlineCode
		{
			get { return Sailing != null ? Sailing.JX_JV_VoyageFlight.Left(2) : ZString.Empty; }
		}

		ZString IMAWBAllocationParent.MawbPortOfLoading
		{
			get
			{
				return !Parent.LoadPort.IsEmpty
					? Parent.LoadPort
					: Sailing?.JX_JA_RL_NKPortOfLoading ?? ZString.Empty;
			}
		}

		ZString IMAWBAllocationParent.MawbPortOfDischarge
		{
			get
			{
				return !Parent.DischargePort.IsEmpty
					? Parent.DischargePort
					: Sailing?.JX_JB_RL_NKPortOfDischarge ?? ZString.Empty;
			}
		}

		ZString IMAWBAllocationParent.Prefix
		{
			get { return JobShipmentSchema.Constants.Prefix; }
		}

		ZGuid IMAWBAllocationParent.PK
		{
			get { return Parent.Booking != null ? Parent.Booking.PK : ZGuid.Empty; }
		}

		bool IMAWBAllocationParent.IsAir
		{
			get { return IsAir; }
		}

		IStmNoteParent IMAWBAllocationParent.NotesParent
		{
			get { return Parent.Booking; }
		}

		#endregion

		#region ISailingParentFindBox Members

		ZString ISailingParentFindBox.Destination
		{
			get { return Parent.Destination; }
		}

		ZString ISailingParentFindBox.DischargePort
		{
			get { return Parent.DischargePort; }
		}

		BusinessObjectFactory ISailingParentFindBox.Factory
		{
			get { return Parent.Factory; }
		}

		ZString ISailingParentFindBox.LoadPort
		{
			get { return Parent.LoadPort; }
		}

		ZString ISailingParentFindBox.Origin
		{
			get { return Parent.Origin; }
		}

		ZGuid ISailingParentFindBox.SailingPK
		{
			get { return Parent.SailingJX; }
			set { Parent.SailingJX = value; }
		}

		ZString ISailingParentFindBox.TransportMode
		{
			get { return Parent.TransportMode; }
		}

		#endregion

		#region IVoyageFinderParent Members

		ZString IVoyageFinderParent.DischargePort
		{
			get { return Parent.DischargePort; }
		}

		ZString IVoyageFinderParent.LoadPort
		{
			get { return Parent.LoadPort; }
		}

		ZString IVoyageFinderParent.TransportMode
		{
			get { return Parent.TransportMode; }
		}

		ZGuid IVoyageFinderParent.CarrierPK
		{
			get { return Parent.Carrier; }
		}

		#endregion

		#region Implementation

		public ZString SailingText
		{
			get
			{
				if (IsAir)
				{
					return Res.GetString("f95ce89d-cd43-4783-a8d7-a4a8988070c3", "Flight");
				}
				if (IsSea)
				{
					return Res.GetString("c07216ec-07c4-425b-998c-bfc94a87a830", "Sailing");
				}

				return Res.GetString("f0db63e3-9022-483a-9ce6-bdff43ba1d87", "Journey");
			}
		}

		public VoyageFinder GetVoyageFinder()
		{
			return new VoyageFinder(this);
		}

		ZBool IsParentBookingImportingData
		{
			get
			{
				var booking = Parent.Booking as ISupportDataImporting;
				if (booking != null)
				{
					return booking.IsImportingData;
				}
				else
				{
					return false;
				}
			}
		}

		ZBool IsAir
		{
			get { return Parent.TransportMode == Constants.TransportModes.Air; }
		}

		ZBool IsSea
		{
			get { return Parent.TransportMode == Constants.TransportModes.Sea; }
		}

		ZBool IsRail
		{
			get { return Parent.TransportMode == Constants.TransportModes.Rail; }
		}

		ZBool IsRoad
		{
			get { return Parent.TransportMode == Constants.TransportModes.Road; }
		}

		ZBool IsLCL
		{
			get { return Parent.ContainerMode == Constants.ContainerModes.LCL; }
		}

		ZBool IsFCL
		{
			get { return Parent.ContainerMode == Constants.ContainerModes.FCL; }
		}

		public IMAWBAllocationParent GetMAWBAllocator()
		{
			var mAWBAllocator = (IMAWBAllocationParent)this;
			if (mAWBAllocator.MAWBAllocation.CanGetMAWBAllocator)
			{
				return mAWBAllocator;
			}
			else
			{
				return null;
			}
		}

		public ZString GetVoyageNoLabelDependingOnTransportMode()
		{
			ZString result = ZString.Empty;

			if (IsSea)
			{
				result = Res.GetString("ScheduleChooserControl|VoyageLabel.Voyage", "Voyage No");
			}
			else if (IsAir)
			{
				result = Res.GetString("ScheduleChooserControl|VoyageLabel.Flight", "Flight No");
			}
			else if (IsRail)
			{
				result = Res.GetString("ScheduleChooserControl|VoyageLabel.Journey", "Journey");
			}
			else if (IsRoad)
			{
				result = Res.GetString("ScheduleChooserControl|VoyageLabel.TruckRef", "Truck Ref.");
			}

			return result;
		}

		#endregion

	}

	class NullSailingChooserParent : ISailingChooserParent
	{
		public NullSailingChooserParent(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		#region ISailingChooserParent Members

		BusinessObjectFactory ISailingChooserParent.Factory
		{
			get { return factory; }
		}

		ZDateTime ISailingChooserParent.BookedDate
		{
			get { return ZDateTime.Empty; }
		}

		event EventHandler ISailingChooserParent.BookedDateChanged
		{
			add { }
			remove { }
		}

		ZDateTime ISailingChooserParent.ETD
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime ISailingChooserParent.RequestedByDate
		{
			get { return ZDateTime.Empty; }
		}

		event EventHandler ISailingChooserParent.RequestedByDateChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.AWBServiceLevel
		{
			get { return ""; }
			set { }
		}

		event EventHandler ISailingChooserParent.AWBServiceLevelChanged
		{
			add { }
			remove { }
		}

		ZBool ISailingChooserParent.IsDirect
		{
			get { return false; }
			set { }
		}

		ZBool ISailingChooserParent.IsDirectEnabled
		{
			get { return false; }
		}

		event EventHandler ISailingChooserParent.IsDirectChanged
		{
			add { }
			remove { }
		}

		ZBool ISailingChooserParent.IsNeutralMaster
		{
			get { return false; }
			set { }
		}

		event EventHandler ISailingChooserParent.IsNeutralMasterChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.MawbNumber
		{
			get { return ""; }
			set { }
		}

		event EventHandler ISailingChooserParent.MawbNumberChanged
		{
			add { }
			remove { }
		}

		ZGuid ISailingChooserParent.Carrier
		{
			get { return ZGuid.Empty; }
			set { }
		}

		event EventHandler ISailingChooserParent.CarrierChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.TransportMode
		{
			get { return ""; }
		}

		event EventHandler ISailingChooserParent.TransportModeChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.LoadPort
		{
			get { return ""; }
		}

		event EventHandler ISailingChooserParent.LoadPortChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.DischargePort
		{
			get { return ""; }
		}

		event EventHandler ISailingChooserParent.DischargePortChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.ContainerMode
		{
			get { return ""; }
		}

		event EventHandler ISailingChooserParent.ContainerModeChanged
		{
			add { }
			remove { }
		}

		ZString ISailingChooserParent.ReservedMasterBill
		{
			get { return ""; }
			set { }
		}

		ZString ISailingChooserParent.Origin
		{
			get { return ""; }
			set { }
		}

		ZString ISailingChooserParent.Destination
		{
			get { return ""; }
			set { }
		}

		ZGuid ISailingChooserParent.SailingJX
		{
			get { return ZGuid.Empty; }
			set { }
		}

		event EventHandler ISailingChooserParent.SailingJXChanged
		{
			add { }
			remove { }
		}

		ScheduleChooser ISailingChooserParent.ScheduleChooser
		{
			get { return null; }
		}

		ForwardingShipment ISailingChooserParent.Booking
		{
			get { return null; }
		}

		void ISailingChooserParent.SetIsNeutralMasterReadOnly(bool value)
		{
		}

		#endregion
	}
}
