using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Packing;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using IDtbTransportConsolidation = Enterprise.TransportCommon.Business.IDtbTransportConsolidation;

namespace Enterprise.TransportBookings.Business
{
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.TransportBookingConsolidation)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.DtbBookingConsolidation)]
	[CodeProperty(DtbBookingConsolidationSchema.Constants.KB_JobID), DescriptionProperty(DtbBookingConsolidationSchema.Constants.KB_JobID)]
	public sealed class DtbBookingConsolidation :
		Common.AutoDtbBookingConsolidation,
		IDtbBookingConsolidation,
		IEDocsProvider,
		IJobCostingPlugIn,
		IPackingParent,
		IPackingParentSupportsImportingBookedDimensions,
		IPackingParentCustomDescription,
		IRatingSupporter,
		IWorkflowProvider,
		IWorkflowProviderEvent,
		ITransportParentCore,
		IAdditionalReferenceNumberTypeProvider,
		ISupportDataImporting,
		ITransportAdditionalReferenceNumbers,
		IRelatedJob,
		IAdditionalReferenceNumberValidationProvider,
		IUniversalXMLNoteParent,
		IDtbTransportConsolidation,
		IJobNumber,
		IDocAddresses,
		IDtbMasterBookingEntity
	{
		public DtbBookingConsolidation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConfirmLoadingCorrectType(factory, row);
			MasterBookingHelper = new DtbMasterBookingHelper(this);
		}

		void ConfirmLoadingCorrectType(BusinessObjectFactory factory, DataRow row)
		{
			var loadAs = TypeDecider.GetTypeForLoad(row, factory);
			if (!loadAs.IsAssignableFrom(this.GetType()))
			{
				throw new NotSupportedException(string.Format(Culture.Invariant, "Job type did not match the type of the class. The class type was {0}, but should have been {1}", GetType().ToString(), loadAs.ToString()));
			}
		}

		public static readonly DtbBookingConsolidationTypeDecider TypeDecider = new DtbBookingConsolidationTypeDecider();

		public static DtbBookingConsolidation FindExistingTransportBookingConsolidation(BusinessObjectFactory factory, IDtbBookingParent parentBizO, DtbBookingDirection direction)
		{
			var query = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, parentBizO.BookingParentPK);
			query.AddToFilter(DtbBookingConsolidationSchema.KB_ParentTableCode, parentBizO.BookingParentTablePrefix);
			query.AddToFilter(DtbBookingConsolidationSchema.KB_JobDirection, direction.ToString());

			return factory.LoadTop1<DtbBookingConsolidation>(query);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			KB_JobType = DefaultJobType;
			KB_Status = TransportStatuses.Codes.Available;
		}

		string DefaultJobType
		{
			get { return TransportConsolidationJobTypes.Codes.Booking; }
		}

		public JobDocAddress Address
		{
			get
			{
				if (address == null || address.IsDeleted)
				{
					var requirement = (((IDocAddresses)this).GetDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress));
					address = DocAddresses.FindOrCreateWithRequirement(requirement);
					address.MakePersistentEvenIfEmpty();
				}
				return address;
			}
		}

		JobDocAddress address;

		[ChildEditable]
		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<Integration.Customs.ICusEntryNumAdditionalReferenceCollectionProvider>();
					additionalReferenceNumbers = provider.GetCollection(this);

					var additionalReferenceNumbersBusinessObjectCollection = additionalReferenceNumbers as BusinessObjectCollection;
					if (additionalReferenceNumbersBusinessObjectCollection != null)
					{
						additionalReferenceNumbersBusinessObjectCollection.Load();
					}

					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}

		ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		[ChildEditable]
		public DtbBookingCollection Bookings
		{
			get
			{
				if (bookings == null)
				{
					bookings = GetNewBookingsCollection();

					if (DtbChildEditableService.GetState(Factory) != DtbChildEditableServiceState.Transport)
					{
						RegisterEditableChildObject(bookings);
					}

					bookings.CountChanged += Bookings_CountChanged;
				}

				return bookings;
			}
		}

		DtbBookingCollection GetNewBookingsCollection()
		{
			return new DtbBookingCollection(this);
		}

		void Bookings_CountChanged(object sender, EventArgs e)
		{
			OnBookingsCountChanged();
		}

		void OnBookingsCountChanged()
		{
			UpdateStatus();
		}

		void ClearBookings()
		{
			bookings = null;
		}

		DtbBookingCollection bookings;

		public DtbBookingCollection ActiveBookings
		{
			get
			{
				if (activeBookings == null)
				{
					activeBookings = new DtbBookingCollection(this);
					activeBookings.AdditionalFilter = new ZQuery(DtbBookingSchema.KM_IsActive, true);
					activeBookings.AllowNewCore = false;
				}
				return activeBookings;
			}
		}

		DtbBookingCollection activeBookings;

		public JobDocAddress BookedByAddress
		{
			get
			{
				if (bookedByAddress == null || bookedByAddress.IsDeleted)
				{
					var requirement = (((IDocAddresses)this).GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress));
					bookedByAddress = DocAddresses.FindOrCreateWithRequirement(requirement);
					bookedByAddress.MakePersistentEvenIfEmpty();
				}
				return bookedByAddress;
			}
		}
		JobDocAddress bookedByAddress;

		public DtbBookingConsolidation MasterBookingConsolidation
		{
			get
			{
				if (masterBookingConsolidation == null)
				{
					masterBookingConsolidation = Factory.Load<DtbBookingConsolidation>(KB_KB_MasterBookingConsolidation);
				}

				return masterBookingConsolidation;
			}
		}

		DtbBookingConsolidation masterBookingConsolidation;

		public PkgPackageJob PackageJob
		{
			get
			{
				if (packageJob == null)
				{
					packageJob = PkgPackageJob.LoadOrCreatePackageJob(this);
					packageJob.SetReadOnlyIncludingChildren(((IPackingParent)this).IsReadOnly);
				}

				return packageJob;
			}
		}

		PkgPackageJob packageJob;

		public IDtbParentInfo Parent
		{
			get
			{
				if (parent == null && !KB_ParentID.IsEmpty && !KB_ParentTableCode.IsEmpty)
				{
					var parentLoader = ObjectFactory.Get<IDtbParentInfoLoader>();
					parent = parentLoader.Load(this);
				}
				return parent;
			}
		}

		IDtbParentInfo parent;

		public void SetParentInfo(IDtbParentInfo parentInfo)
		{
			this.parent = parentInfo;
		}

		// persistent

		public override ZString KB_JobType
		{
			get { return base.KB_JobType; }
			set
			{
				if (base.KB_JobType != value)
				{
					if (value == TransportConsolidationJobTypes.Codes.Consignment)
					{
						throw new InvalidOperationException(string.Format(Culture.Invariant, "{0} is not a valid {1} in a Booking Consolidation.", value, DtbBookingConsolidationSchema.Constants.KB_JobType));
					}
					if (Bookings.Count > 0)
					{
						throw new InvalidOperationException(DtbBookingConsolidationSchema.Constants.KB_JobType + " cannot be changed if the Consolidation already has Bookings.");
					}

					ClearBookings(); // get a new collection and relationship
					base.KB_JobType = value;
				}
			}
		}

		[ReadOnly(true)]
		public override ZString KB_Status
		{
			get { return base.KB_Status; }
			set { base.KB_Status = value; }
		}

		public override ZGuid KB_KB_MasterBookingConsolidation
		{
			get { return base.KB_KB_MasterBookingConsolidation; }
			set
			{
				if (base.KB_KB_MasterBookingConsolidation != value)
				{
					base.KB_KB_MasterBookingConsolidation = value;
					masterBookingConsolidation = null;
					docAddresses = null;
				}
			}
		}

		public void UpdateStatus()
		{
			if (Bookings.IsAnyBookingHeld)
			{
				KB_Status = TransportStatuses.Codes.Held;
			}
			else if (Bookings.IsDelivered)
			{
				KB_Status = TransportStatuses.Codes.Delivered;
			}
			else if (Bookings.IsDeliveredEmptyNotReturned)
			{
				KB_Status = TransportStatuses.Codes.DeliveredEmptyNotReturned;
			}
			else if (Bookings.IsPickedUp)
			{
				KB_Status = TransportStatuses.Codes.PickedUp;
			}
			else if (Bookings.IsServiceCommenced)
			{
				KB_Status = TransportStatuses.Codes.ServiceCommenced;
			}
			else if (Bookings.IsActionRequired)
			{
				KB_Status = TransportStatuses.Codes.ActionRequired;
			}
			else
			{
				KB_Status = TransportStatuses.Codes.Available;
			}
		}

		public override ZBool KB_IsOverridden
		{
			get { return base.KB_IsOverridden; }
			set
			{
				base.KB_IsOverridden = value;

				OnConsolidationIsOverrideChanged();
			}
		}

		public void OnConsolidationIsOverrideChanged()
		{
			Bookings.OnConsolidationIsOverrideChanged();

			var pkgJob = PkgPackageJob.LoadPackageJob(this);
			if (pkgJob != null)
			{
				pkgJob.OnParentJobIsReadOnlyChanged();
			}
		}

		public override ZGuid KB_ParentID
		{
			get { return base.KB_ParentID; }
			set
			{
				if (Bookings != null)
				{
					foreach (var booking in Bookings)
					{
						if (KB_ParentID == ZGuid.Empty && booking.Job != null && value != ZGuid.Empty && KB_ParentTableCode != DtbAgentBookingSchema.Constants.Prefix)
						{
							ErrorReporter.ReportOnce("Attempted to attach a booking to a IDtbBookingParent while the booking already has an attached job header.");
						}
					}
				}
				
				base.KB_ParentID = value;
				foreach (var booking in Bookings)
				{
					booking.PurgeInvoicingPlugInCache();
				}
			}
		}

		// calculated

		public bool IsBookingsReadOnly
		{
			get { return ReadOnly || (!KB_IsOverridden && Parent != null); }
		}

		public DtbBookingDirection Direction
		{
			get
			{
				if (!Enum.TryParse<DtbBookingDirection>(KB_JobDirection, out var result))
				{
					result = DtbBookingDirection.LOC;
				}

				return result;
			}
		}

		public ZString JobNumber
		{
			get { return !IsMultiBooking && Bookings.Count == 1 ? Bookings.First().KM_JobID : KB_JobID; }
		}

		public ZString Description
		{
			get { return (DescriptionWithoutJobNo + " " + ((IPackingParent)this).JobNo).TrimEnd(); }
		}

		ZString DescriptionWithoutJobNo
		{
			get
			{
				ZString result;

				if (IsMultiBooking)
				{
					result = Res.GetString("166b7a04-34d4-4574-a67b-0f5deaf9c79b", "Consolidated Transport Booking");
				}
				else
				{
					var consolParent = this.Parent;
					result = (consolParent != null)
						? Res.GetString("512868c2-442c-46a5-967c-d6d2e0b550fc", "Bookings for {0}", consolParent.JobDescription)
						: Res.GetString("8774d86e-5228-48fc-81e0-51b9b824840f", "Bookings for Standalone Packing");
				}

				return result;
			}
		}

		public ZString ParentJobDescription
		{
			get
			{
				var bookingParent = this.Parent;
				return (bookingParent != null) ? bookingParent.JobDescription + " " + bookingParent.JobNumber : Res.GetString("6FAE9DD1-7530-4C7A-9A01-DE6816CF4A48", "Standalone Packing");
			}
		}

		public ZGuid BookedByOrganisationPK
		{
			get
			{
				var bookedBy = BookedByAddress;
				return bookedBy != null && !bookedBy.E2_AddressOverride ? bookedBy.OrganisationPK : ZGuid.Empty;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("1948b8e0-0847-4114-aad4-a80fa30ef9a0", "Transport Booking Consolidation {0}", KB_JobID); }
		}

		public ZDateTime LatestPickedUpConfirmationDate
		{
			get
			{
				var result = ZDateTime.Empty;

				foreach (var booking in Bookings)
				{
					var bookingLatest = booking.LatestPickedUpConfirmationDate;
					if (bookingLatest.IsValid && (result.IsEmpty || bookingLatest > result))
					{
						result = bookingLatest;
					}
				}

				return result;
			}
		}

		public ZDateTime LatestDeliveryNonDehireConfirmationDate
		{
			get
			{
				var result = ZDateTime.Empty;

				foreach (var booking in Bookings)
				{
					var bookingLatest = booking.LatestDeliveryNonDehireConfirmationDate;
					if (bookingLatest.IsValid && (result.IsEmpty || bookingLatest > result))
					{
						result = bookingLatest;
					}
				}

				return result;
			}
		}

		public ZDateTime LatestDeliveryConfirmationDate
		{
			get
			{
				var result = ZDateTime.Empty;

				foreach (var booking in Bookings)
				{
					var bookingLatest = booking.LatestDeliveryConfirmationDate;
					if (bookingLatest.IsValid && (result.IsEmpty || bookingLatest > result))
					{
						result = bookingLatest;
					}
				}

				return result;
			}
		}

		public DtbBookingConfirmation LatestDeliveryNonDehireWithDateAndSignedByConfirmation
		{
			get
			{
				DtbBookingConfirmation result = null;
				var latest = ZDateTime.Empty;

				foreach (var booking in Bookings)
				{
					var curConfirmation = booking.LatestDeliveryNonDehireWithDateAndSignedByConfirmation;
					if (curConfirmation != null)
					{
						var curLatest = curConfirmation.KK_Actual;
						if (curLatest.IsValid && (latest.IsEmpty || curLatest > latest))
						{
							latest = curLatest;
							result = curConfirmation;
						}
					}
				}

				return result;
			}
		}

		public ZDateTime LatestEmptyReturnedConfirmationDate
		{
			get
			{
				var result = ZDateTime.Empty;

				foreach (var booking in Bookings)
				{
					var bookingLatest = booking.LatestEmptyReturnedConfirmationDate;
					if (bookingLatest.IsValid && (result.IsEmpty || bookingLatest > result))
					{
						result = bookingLatest;
					}
				}

				return result;
			}
		}

		public ZString StatusDescription
		{
			get { return Lookups.BindToLists.Statuses.GetDescriptionFromCode(KB_Status); }
		}

		public ZPropertyInfo<ZString> StatusDescriptionInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(nameof(StatusDescription)); }
		}

		public IEnumerable<ZString> TransportBookingPartyReferences
		{
			get
			{
				var result = new List<ZString>();

				foreach (var booking in Bookings)
				{
					var externalTBNumber = booking.TransportBookingPartyReference;
					if (!externalTBNumber.IsEmpty)
					{
						result.Add(externalTBNumber);
					}
				}

				return result;
			}
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);
				foreach (DtbBooking booking in Bookings)
				{
					result.Add(booking);
				}
				return result.ToArray();
			}
		}

		public new DtbBookingConsolidationLookups Lookups
		{
			get { return (DtbBookingConsolidationLookups)base.Lookups; }
		}

		protected override Common.DtbBookingConsolidationLookups GetNewLookups()
		{
			return new DtbBookingConsolidationLookups(this);
		}

		public new DtbBookingConsolidationValidation Validation
		{
			get { return (DtbBookingConsolidationValidation)base.Validation; }
		}

		protected override Common.DtbBookingConsolidationValidation GetNewValidation()
		{
			return new DtbBookingConsolidationValidation(this);
		}

		// overrides

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			KB_ParentID = Guid.Empty;
			KB_ParentTableCode = "";
		}
#endif

		// status

		public bool IsAvailable
		{
			get { return KB_Status.EqualsIgnoringCase(TransportStatuses.Codes.Available); }
		}

		public bool IsDeliveredEmptyNotReturned
		{
			get { return KB_Status.EqualsIgnoringCase(TransportStatuses.Codes.DeliveredEmptyNotReturned); }
		}

		public bool IsDelivered
		{
			get { return KB_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered); }
		}

		public bool IsHeld
		{
			get { return KB_Status.EqualsIgnoringCase(TransportStatuses.Codes.Held); }
		}

		public bool IsParentHidden
		{
			get
			{
				var parentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(KB_ParentTableCode);
				return Attribute.IsDefined(parentType, typeof(HiddenBookingParentAttribute));
			}
		}

		public bool IsPickedUp
		{
			get { return KB_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickedUp); }
		}

		public ZBool IsMultiBooking
		{
			get { return KB_JobType == TransportConsolidationJobTypes.Codes.BookingTransportConsolidation; }
		}

		public bool IsSub
		{
			get { return KB_KB_MasterBookingConsolidation != ZGuid.Empty; }
		}

		public bool CheckIfPUPNeedsToBeSent { get; set; }

		public bool CheckIfDLVNeedsToBeSent { get; set; }

		// considering status

		public bool HasEmptyDehireInstructions
		{
			get { return Bookings.Any(b => b.HasEmptyDehireInstructions); }
		}

		public ZBool IsParentSupportsRouting
		{
			get
			{
				var parentBO = ParentBO;
				return parentBO != null && typeof(ITransportParentCommon).IsAssignableFrom(parentBO.GetType());
			}
		}

		public bool IsParentSupportsDirectSailing
		{
			get { return Parent?.SupportsDirectSailing ?? false; }
		}

		public BusinessObject ParentBO
		{
			get { return Parent?.ParentWithWorkflow; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingConsolidationFetchStrategy(this);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			// tested by workflow tests
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);

			AddAndDelayPublishingStatusEvents();
			AddAndDelayPublishingContainerEvents();
			AddAndDelayPublishingBookingRequestedEventsToParent();
		}

		void AddAndDelayPublishingStatusEvents()
		{
			if (MarkedAsNeedingStatusCheck)
			{
				PublishEventsOnSaved.Add(FirePickupEventIfNeeded());
				PublishEventsOnSaved.Add(FireDeliveredEmptyNotReturnedEventIfNeeded());
				PublishEventsOnSaved.Add(FireEmptyYardGateInEventIfNeeded());
				PublishEventsOnSaved.Add(FireDeliveredEventIfNeeded());

				MarkedAsNeedingStatusCheck = false;
			}
			PublishEventsOnSaved.Add(FirePUPEventIfNeeded());
			PublishEventsOnSaved.Add(FireDLVEventIfNeeded());
		}

		/// <summary>
		/// Fire PUP Event if All CNR Confirmations has KK_Actual date.
		/// </summary>
		StmALog FirePUPEventIfNeeded()
		{
			StmALog result = null;
			if (CheckIfPUPNeedsToBeSent)
			{
				result = AddUpdateOrRemoveEvents(Events.PickedUp, i => i.IsPickUp || i.IsMulti, c => c.IsPickUp, OrganisationTypesList.Codes.CNR, CargoWise.EventReference.Constants.Facilities.Code.Consignor);
				CheckIfPUPNeedsToBeSent = false;
			}
			return result;
		}

		/// <summary>
		/// Fire DLV Event if All CNE Confirmations has KK_Actual date.
		/// </summary>
		StmALog FireDLVEventIfNeeded()
		{
			StmALog result = null;

			if (CheckIfDLVNeedsToBeSent)
			{
				result = AddUpdateOrRemoveEvents(Events.Delivered, i => i.IsDelivery || i.IsMulti, c => c.IsDelivery, OrganisationTypesList.Codes.CNE, CargoWise.EventReference.Constants.Facilities.Code.Consignee);
				CheckIfDLVNeedsToBeSent = false;
			}

			return result;
		}

		StmALog AddUpdateOrRemoveEvents(Event eventCode, Func<DtbBookingInstruction, bool> isValidInstruction, Func<DtbBookingConfirmation, bool> isValidConfirmation,
			string orgType, string facility)
		{
			var instructions = Bookings.SelectMany(b => b.Instructions).Where(i => isValidInstruction(i));
			var confirmations = instructions.SelectMany(i => i.Confirmations).Where(c => isValidConfirmation(c) && c.OrganisationType == orgType);
			var isAllConfirmationsCompleted = confirmations.Any() && confirmations.All(c => c.KK_Actual.IsValid);
			var eventParams = new[] { new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, facility) };
			return Logs.CreateRecreateOrUpdateEventLog(eventCode, EstimateActual.Actual, (isAllConfirmationsCompleted ? confirmations.OrderByDescending(c => c.KK_Actual).First().KK_Actual.ToOffset() : ZDateTimeOffset.Empty), "", eventParams); // get the latest time
		}

		/// <summary>
		/// Fire Picked Up Event if Picked Up. Return Log, if to be logged on Parent, but only after Saved.
		/// </summary>
		StmALog FirePickupEventIfNeeded()
		{
			StmALog result = null;
			if (IsPickedUp || IsDeliveredEmptyNotReturned || IsDelivered)
			{
				var latestPickedUpDate = LatestPickedUpConfirmationDate;
				if (latestPickedUpDate.IsValid)
				{
					var log = AddLogIfNotExistsOrNotMatching(Events.PickupCartageCompleteFinalised, latestPickedUpDate);
					if (Direction == DtbBookingDirection.PIC || Direction == DtbBookingDirection.LOC)
					{
						result = log;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Fire Delivered Event if Delivered excluding Empty Dehires. Return Log, if to be logged on Parent, but only after Saved.
		/// </summary>
		StmALog FireDeliveredEmptyNotReturnedEventIfNeeded()
		{
			StmALog result = null;
			if (IsDeliveredEmptyNotReturned || IsDelivered)
			{
				var latestConfirmationWithDateAndSignedBy = LatestDeliveryNonDehireWithDateAndSignedByConfirmation;
				var latestDateOnBooking = (latestConfirmationWithDateAndSignedBy != null) ? latestConfirmationWithDateAndSignedBy.KK_Actual : LatestDeliveryNonDehireConfirmationDate;
				if (latestDateOnBooking.IsValid)
				{
					var latestSignedByOnBooking = (latestConfirmationWithDateAndSignedBy != null) ? latestConfirmationWithDateAndSignedBy.KK_ReceivedBy : ZString.Empty;
					var log = AddLogIfNotExistsOrNotMatching(Events.DeliveryCartageCompleteFinalised, latestDateOnBooking, latestSignedByOnBooking);
					if (Direction == DtbBookingDirection.DLV || Direction == DtbBookingDirection.LOC)
					{
						result = log;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Fire Dehire Gate In Event if Booking has Empties and booking is Delivered. Return Log, if to be logged on Parent, but only after Saved.
		/// </summary>
		StmALog FireEmptyYardGateInEventIfNeeded()
		{
			StmALog result = null;
			if (HasEmptyDehireInstructions && IsDelivered)
			{
				var latestEmptyReturnedDate = LatestEmptyReturnedConfirmationDate;
				if (latestEmptyReturnedDate.IsValid)
				{
					var parameters = new[]
					{
						new KeyValuePair<string, string>
						(
							CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, CargoWise.EventReference.Constants.Facilities.Code.ContainerYard
						)
					};

					result = AddLogIfNotExistsOrNotMatching(AutoEvents.GateIn, latestEmptyReturnedDate, string.Empty, parameters);
				}
			}

			return result;
		}

		/// <summary>
		/// Fire Complete Event if everything is Delivered. Return Log, if to be logged on Parent, but only after Saved.
		/// </summary>
		StmALog FireDeliveredEventIfNeeded()
		{
			StmALog result = null;
			if (IsDelivered)
			{
				var latestCompletionDate = LatestDeliveryConfirmationDate;
				if (latestCompletionDate.IsValid)
				{
					result = AddLogIfNotExistsOrNotMatching(Events.CartageCompleteFinalised, latestCompletionDate, "");
				}
			}

			return result;
		}

		StmALog AddLogIfNotExistsOrNotMatching(Event eventType, ZDateTime date, string referenceFreeText = "", params KeyValuePair<string, string>[] parameters)
		{
			StmALog result = null;
			var existingLog = GetLatestLog(eventType);
			var reference = StmALog.GenerateEventReference(referenceFreeText, parameters);

			if (existingLog == null || existingLog.SL_EventTime != date || existingLog.SL_Reference != reference)
			{
				result = Logs.AddNew(eventType, reference, date.ToOffset());
			}

			return result;
		}

		public StmALog GetLatestLog(Event eventType)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, this.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " desc";

			return Factory.LoadTop1<StmALog>(query);
		}

		void AddAndDelayPublishingContainerEvents()
		{
			foreach (var container in ContainersWithIDChange.Where(p => !p.IsDeleted && p.IsContainer))
			{
				var oldContainerID = container.IsInDatabase ? container.OriginalPackageID.ToString() : "";
				var newContainerID = container.KP_PackageID;

				var packageView = Bookings.Where(b => b.IsPickupDirection).Select(b => b.Packages_PackageView.Find(container)).FirstOrDefault(d => d != null);
				var confirmation = packageView?.Confirmations.Cast<DtbBookingConfirmation>()
					.FirstOrDefault(c => c.KK_IsEmptyContainer && c.KK_ConfirmationType == c.Instruction.KN_InstructionType && !c.KK_ReferenceNum.IsEmpty);
				var referenceNumber = (confirmation != null) ? confirmation.KK_ReferenceNum : ZString.Empty;

				if (oldContainerID != newContainerID)
				{
					var parameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ContainerID),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, oldContainerID),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, newContainerID),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, referenceNumber)
					};

					var eventInfo = new EventValue(Events.ChangeOfIdentifier, false, false, ZDateTimeOffset.Now, "", parameters: parameters.ToDictionary(x => x.Key, x => x.Value));
					var existingLog = FindActiveLogNotInDBForContainerIDEvent(parameters);

					PublishEventsOnSaved.Add(Logs.CreateRecreateOrUpdateEventLog(eventInfo, existingLog));
				}
			}

			ContainersWithIDChange.Clear();
		}

		StmALog FindActiveLogNotInDBForContainerIDEvent(params KeyValuePair<string, string>[] parameters)
		{
			Func<KeyValuePair<string, string>, bool> inNewParameters = pair =>
			{
				return parameters != null && parameters.Any(p => p.Key == pair.Key && p.Value == pair.Value);
			};

			foreach (StmALog log in Logs.LogsNotInDB)
			{
				if (log.SL_SE_NKEvent == Events.ChangeOfIdentifier.Code &&
					!log.SL_IsEstimate &&
					log.Parameters.All(inNewParameters) &&
					!log.SL_IsCancelled)
				{
					return log;
				}
			}
			return null;
		}

		void AddAndDelayPublishingBookingRequestedEventsToParent()
		{
			// Need to separate condition checking "RequiredToAddBookingRequestedEvent" and log generating "CreateRecreateOrUpdateEventLog".
			// Condition checking should happen before "Saving", so that there is no "BookingRequested" log for booking yet.
			// Log generating should happen after "Saving", so that booking's KM_JobID will be available then.
			var bookingRequestedEventService = Factory.ServiceContainer.GetAfterOnSavingService<BookingsAfterOnSavingEventService>();
			if (bookingRequestedEventService == null)
			{
				bookingRequestedEventService = new BookingsAfterOnSavingEventService(bookingsToProcess =>
				{
					foreach (var booking in bookingsToProcess)
					{
						PublishEventsOnSaved.Add(
							Logs.CreateRecreateOrUpdateEventLog(
								Events.BookingRequested,
								EstimateActual.Actual,
								booking.KM_BookingOfTransportRequestedDate.IsEmpty ? ZDateTimeOffset.Now : booking.KM_BookingOfTransportRequestedDate.ToOffset(),
								booking.KM_JobID,
								new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, booking.GetBookingRequestedEventTypeParameter())));
					}
				});
				Factory.ServiceContainer.AddAfterOnSavingService(bookingRequestedEventService);
			}

			var bookingsToSubscribe = Bookings.Where(booking => booking.RequiredToAddBookingRequestedEventForNonShipmentParent()).ToArray();
			bookingRequestedEventService.SubscribeBookingsToPublishBookingRequestedEvent(bookingsToSubscribe);
		}

		class BookingsAfterOnSavingEventService : IAfterOnSavingBOProcessingService
		{
			public BookingsAfterOnSavingEventService(Action<IEnumerable<DtbBooking>> actionAfterOnSaving)
			{
				ActionAfterOnSaving = actionAfterOnSaving;
			}

			Action<IEnumerable<DtbBooking>> ActionAfterOnSaving { get; }
			HashSet<DtbBooking> Bookings { get; } = new HashSet<DtbBooking>();

			public void SubscribeBookingsToPublishBookingRequestedEvent(DtbBooking[] bookings)
			{
				foreach (var booking in bookings)
				{
					Bookings.Add(booking);
				}
			}

			public void Clear() => Bookings.Clear();

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				try
				{
					ActionAfterOnSaving.Invoke(Bookings);
				}
				finally
				{
					Clear();
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!isInOnFactorySaved)
			{
				// Send events if successful
				if (saveSucceeded && PublishEventsOnSaved.Any(l => l != null))
				{
					if (Parent != null && Parent.ParentWithWorkflow != null)
					{
						var old = isInOnFactorySaved;
						isInOnFactorySaved = true;
						try
						{
							var factory = new BusinessObjectFactory();
							using (factory.AddDisposableService())
							{
								foreach (var log in PublishEventsOnSaved.Where(l => !(l?.IsDeleted ?? true)))
								{
									PublishUniversalEventCore.PublishUniversalEvent(factory, this, Parent.ParentWithWorkflow, log);
								}
								factory.Save();
							}
						}
						finally
						{
							isInOnFactorySaved = old;
						}
					}

					PublishEventsOnSaved.Clear();
				}

				if (!saveSucceeded)
				{
					Factory.ServiceContainer.GetAfterOnSavingService<BookingsAfterOnSavingEventService>()?.Clear();
				}
			}
		}

		[ThreadStatic]
		static bool isInOnFactorySaved;

		public void MarkAsNeedingStatusCheck()
		{
			MarkedAsNeedingStatusCheck = true;
		}

		bool MarkedAsNeedingStatusCheck;

		List<StmALog> PublishEventsOnSaved
		{
			get { return publishEventsOnSaved ?? (publishEventsOnSaved = new List<StmALog>()); }
		}
		List<StmALog> publishEventsOnSaved;

		void BeforeOnSaving()
		{
			if (!IsMultiBooking && (!IsInDatabase || (ZGuid)KB_ParentIDInfo.OriginalValue != KB_ParentID))
			{
				foreach (var booking in Bookings)
				{
					var isAttachingEventRequiredForThisBooking = booking.IsInDatabase && !booking.HasChanges; // normally this event is added from booking.OnSaving, but this booking already exists and doesn't have changes
					if (isAttachingEventRequiredForThisBooking)
					{
						booking.AddBookingAttachedToParentEvent();
					}
				}
			}
		}

		public override void OnSaving()
		{
			PopulateUniqueIDIfNeeded();
			UpdateSubMasterBookingVersionIfNewAndNotFilled();

			BeforeOnSaving();
			MasterBookingHelper.UpdateMasterBookingVersion();
			base.OnSaving();
		}

		void PopulateUniqueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted)
			{
				KB_JobID = NumberFountainForUniqueID.GetNextFormatted(Factory);
			}
		}

		void UpdateSubMasterBookingVersionIfNewAndNotFilled()
		{
			if (!IsInDatabase && !IsDeleted && KB_KB_MasterBookingConsolidation != ZGuid.Empty && KB_MasterBookingVersion == ZShort.Zero)
			{
				KB_MasterBookingVersion = (short)1;
			}
		}

		public override void Delete()
		{
			if (SubBookingConsolidations.Any())
			{
				return;
			}

			// tested by SaveAndDeleteBusinessObject()
			DeleteBookings();
			// tested by SaveAndDeleteBusinessObject()
			DocAddresses.RemoveAndDeleteAll();
			// test by WorkflowProvider tests
			WorkflowItems.RemoveAndDeleteAll();
			// tested by PackingParentTestCase
			DeletePackageJob();

			base.Delete();
		}

		ActiveBusinessObjectCollection<DtbBookingConsolidation> SubBookingConsolidations
		{
			get
			{
				var subBookingConsolidationQuery = new ZQuery(DtbBookingConsolidationSchema.KB_KB_MasterBookingConsolidation, this.PK);
				var subBookingConsolidations = new ActiveBusinessObjectCollection<DtbBookingConsolidation>(Factory, subBookingConsolidationQuery);
				return subBookingConsolidations;
			}
		}

		void DeletePackageJob()
		{
			var pkgJob = PkgPackageJob.LoadPackageJob(this);
			if (pkgJob != null)
			{
				pkgJob.Delete();
			}
		}

		void DeleteBookings()
		{
			if (IsMultiBooking)
			{
				Array.ForEach(Bookings.ToArray(), b => Bookings.RemoveFromRelationship(b));
			}
			else
			{
				Bookings.DeleteAll();
			}
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DtbBookingConsolidationFountainUniqueIndexFailureHandler(this)); }
		}

		class DtbBookingConsolidationFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DtbBookingConsolidationFountainUniqueIndexFailureHandler(DtbBookingConsolidation consolidation)
				: base(DtbBookingConsolidationSchema.Constants.Indexes.NR_UC__KB_JobID, consolidation)
			{
				Consolidation = consolidation;
			}

			readonly DtbBookingConsolidation Consolidation;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Consolidation.NumberFountainForUniqueID; }
			}
		}

		INumberFountainProxy NumberFountainForUniqueID
		{
			get { return IsMultiBooking ? Env.NumberFountains.DtbBookingConsolidationMultiJobID : Env.NumberFountains.DtbBookingConsolidationID; }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		string IJobNumber.JobNumber
		{
			get { return KB_JobID; }
		}

		public Stack<INotifications> NotificationManager
		{
			get
			{
				if (notificationManager == null)
				{
					notificationManager = new Stack<INotifications>();
					notificationManager.Push(new NotificationBuffer());
				}
				return notificationManager;
			}
		}
		Stack<INotifications> notificationManager;

		public INotifications NotificationSubscriber
		{
			get { return NotificationManager.Peek(); }
		}

		public void RunOnSelectDtbBookingsToPrint(object sender, DtbBookingsToPrintEventArgs e)
		{
			OnSelectDtbBookingsToPrint?.Invoke(this, e);
		}

		public event EventHandler<DtbBookingsToPrintEventArgs> OnSelectDtbBookingsToPrint;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				BusinessObject[] result;

				if (!IsDeleted)
				{
					var relatedObjects = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

					foreach (var transportBooking in Bookings)
					{
						relatedObjects.Add(transportBooking);
					}

					result = relatedObjects.ToArray();
				}
				else
				{
					result = base.BusinessObjectsWithRelatedEventsCore;
				}

				return result;
			}
		}

		// interfaces

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DtbBookingConsolidationDocManagerInfo(this, Constants.DocManagerCodes.DomesticTransportBookingConsolidation)); }
		}

		DocManagerInfo docManagerInfo;

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DtbBookingConsolidationDocumentSupporter(this)); }
		}

		DocumentSupporter documentSupporter;

		IDtbBooking[] IDtbBookingConsolidation.Bookings
		{
			get { return Bookings.Cast<IDtbBooking>().ToArray(); }
		}

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new EDocsProviderSupporter(this);
		}

		RefCurrency IJobCostingPlugIn.ConsolCurrency
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ConsolExchangeRate
		{
			get { return 0m; }
		}

		RefUNLOCO IJobCostingPlugIn.DischargePort
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK)
		{
			return 0m;
		}

		void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference)
		{
			Logs.AddNew(@event, reference);
		}

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob)
		{
			return ZString.Empty;
		}

		bool IJobCostingPlugIn.IsMasterCollect
		{
			get { return false; }
		}

		ZString IJobCostingPlugIn.JK_UniqueConsignRef
		{
			get { return KB_JobID; }
		}

		RefUNLOCO IJobCostingPlugIn.LoadPort
		{
			get { return null; }
		}

		JobProfitLossCollection IJobCostingPlugIn.ProfitLossContainer
		{
			get { return null; }
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

		ZString IJobCostingPlugIn.ContainerMode => ZString.Empty;

		ZString IJobCostingPlugIn.ConsolType => ZString.Empty;

		ZString IJobCostingPlugIn.Module => ApportionmentMethodModules.TransportBooking;

		ZString IJobCostingPlugIn.Direction => ZString.Empty;

		public CodeDescriptionPairList PrepaidCollectList
		{
			get { return new CodeDescriptionPairList(); }
		}

		IGenericJobCostSupporter IGenericJobCostPlugIn.CostSupporter
		{
			get { return new DtbBookingConsolidationJobCostSupporter(this); }
		}

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return false; }
		}

		public override bool CanDelete
		{
			get
			{
				return !Bookings.Any();
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (Bookings.Any())
				{
					return ResString.GetMultilingualString("ee4110a1-4f42-40b0-8947-a9ca2e33001d", @"Transport Booking Consolidation ""{0}"" cannot be deleted. Detach existing Transport Booking(s) before deleting.", KB_JobID);
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			var restrictedAction = PackageAction.Delete;

			foreach (var booking in Bookings)
			{
				foreach (var instruction in booking.Instructions)
				{
					if (instruction.DivotsWithPackages.Contains(package))
					{
						string reason = "\r\n" + Res.GetString("51193d99-bed7-483e-97ef-6a8a1fbdd666",
							"Package '{0} {1}' is assigned to one or more Instructions and cannot be deleted.\r\n\r\nIf you want to delete this package, first un-assign it from all instructions.",
							package.ToStringPackageSummary(), package.KP_PackageID);

						return new PackageActionStrategy(package, restrictedAction, reason);
					}

					//ensure no child package is linked
					foreach (var childPackage in package.GetAllPackages())
					{
						if (instruction.DivotsWithPackages.Contains(childPackage))
						{
							string reason = "\r\n" + Res.GetString("8e7effaf-0f2f-4e57-8b0c-dc6f4e4e2675",
								"Package '{0} {1}' has child packages that are assigned to one or more Instructions and cannot be deleted.\r\n\r\nIf you want to delete this package, first un-assign child packages from all instructions.",
								package.ToStringPackageSummary(), package.KP_PackageID);

							return new PackageActionStrategy(package, restrictedAction, reason);
						}
					}
				}
			}

			return new PackageActionStrategy(package);
		}

		ControllerID IPackingParent.ControllerID
		{
			get { return ControllerIDs.DtbBookingConsolidation; }
		}

		DocumentOptions IPackingParent.DocumentOptions
		{
			get { return DocumentOptions.None; }
		}

		ZString IPackingParent.JobDescription
		{
			get { return DescriptionWithoutJobNo; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return Parent != null ? (ZString)Parent.TransportReference : ZString.Empty; }
		}

		ZString IPackingParent.JobNo
		{
			get
			{
				var bookingParent = Parent;
				return bookingParent != null ? bookingParent.JobNumber : JobNumber;
			}
		}

		bool IList.IsReadOnly
		{
			get { return IsBookingsReadOnly; }
		}

		bool IPackingParent.IsPackingJobReadOnly
		{
			get { return IsBookingsReadOnly; }
		}

		bool IPackingParent.IsScanEventsVisible
		{
			get { return false; }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			return "";
		}

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob job)
		{
		}

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
			ContainersWithIDChange.Add(container);
		}

		List<PkgPackage> ContainersWithIDChange
		{
			get { return containersWithIDChange ?? (containersWithIDChange = new List<PkgPackage>()); }
		}
		List<PkgPackage> containersWithIDChange;

		bool IPackingParent.IsAutoPrintAllowed
		{
			get { return false; }
		}

		bool IPackingParent.IsParentJobFinalised
		{
			get { return false; }
		}

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package)
		{
			var carrierServiceLevel = Bookings.Where(b => b.AssignedPackages.Contains(package)).Select(b => b.KM_PL_NKCarrierServiceLevel).Distinct().ToArray();
			return carrierServiceLevel.Length == 1 ? carrierServiceLevel.Single() : ZString.Empty;
		}

		OrgHeader IPackingParent.CarrierBookingAgent => null;

		ZString IPackingParent.TransportReference { get => ZString.Empty; set { } }

		OrgHeader IPackingParent.GetCarrier(PkgPackage package)
		{
			var carriers = Bookings.Where(b => b.AssignedPackages.Contains(package)).Select(b => b.Address.Organisation).Distinct().ToArray();
			return carriers.Length == 1 ? carriers.Single() : null;
		}

		bool IPackingParent.IsLoosePackageIDsSupported
		{
			get { return false; }
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Outer;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber
		{
			get
			{
				if (Bookings.Any(b => b.IsSendingXUSToCTO))
				{
					return NotificationTypes.MessageError;
				}
				else
				{
					return NotificationTypes.Warning;
				}
			}
		}

		ZString IPackingParentCustomDescription.FullJobDescription
		{
			get
			{
				ZString result;

				var bookingParent = Parent;
				if (bookingParent != null)
				{
					result = Res.GetString("052094a0-4fa9-4a42-bffa-343745699abe", "Bookings for {0} {1}", bookingParent.JobDescription, bookingParent.JobNumber);
				}
				else if (Bookings.Count == 1)
				{
					result = Res.GetString("d897dcba-eec3-487d-a61e-2ddbe0622aaf", "Packing Job for Transport Booking {0}", Bookings.First().KM_JobID);
				}
				else
				{
					result = Res.GetString("3578c6a3-0347-4b0e-9535-82b4f52e4ad0", "Packing Job for Multiple Bookings");
				}

				return result;
			}
		}

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new DtbBookingConsolRatingAdaptersProvider(this); }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new DtbBookingConsolidationProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		CargoWise.Integration.IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode; }
		}

		OrgHeader[] IWorkflowProviderEvent.RecipientOrganisations
		{
			get { return new OrgHeader[] { GlbBranch.CurrentBranch.OrgProxy }; }
		}

		[ChildEditable()]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					if (IsSub)
					{
						docAddresses = MasterBookingConsolidation.DocAddresses;
					}
					else
					{
						docAddresses = new JobDocAddressDependentCollection(this);
						docAddresses.Load();
						RegisterEditableChildObject(docAddresses);
					}
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new[] { DocAddressType.BookingPartyDocumentaryAddress, DocAddressType.TransportCompanyDocumentaryAddress };
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.BookingPartyDocumentaryAddress && !docAddress.E2_AddressOverride)
			{
				var clientRequestedBillToParty = FindClientRequestedBillToPartyForBookingPartyDocumentaryAddress(docAddress);
				if (clientRequestedBillToParty != null)
				{
					foreach (DtbBooking booking in Bookings)
					{
						var billingPartyAddress = booking.BillingPartyAddress;
						if (!billingPartyAddress.E2_AddressOverride && !billingPartyAddress.E2_OA_Address.IsValid)
						{
							billingPartyAddress.E2_OA_Address = clientRequestedBillToParty.MainAddress.PK;
						}
					}
				}
			}
		}

		OrgHeader FindClientRequestedBillToPartyForBookingPartyDocumentaryAddress(JobDocAddress docAddress)
		{
			OrgHeader result = null;
			var organisation = docAddress.Organisation;
			if (organisation != null)
			{
				var debtorFromRelatedParty = organisation.GetRelatedParty(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
				result = debtorFromRelatedParty ?? (organisation.OH_IsDebtor ? docAddress.Organisation : null);
			}
			return result;
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress)
			{
				foreach (var booking in Bookings)
				{
					booking.Validation.ValidateTransportCoAgainstMultiJobConsolidation();
				}
			}
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return docAddress == null || docAddress.DocAddressType != DocAddressType.TransportCompanyDocumentaryAddress;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.TransportJobMISCDetails;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result = null;

			if (addressType == DocAddressType.TransportCompanyDocumentaryAddress)
			{
				result = new JobDocAddressRequirement(addressType, AddressType.OFC, ContactType.LocalTransport);
				result.IsMandatory = IsMultiBooking;
			}
			else
			{
				result = new JobDocAddressRequirement(addressType, AddressType.OFC, ContactType.LocalTransport);
			}

			return result;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			return AdditionalReferenceHelper.GetAdditionalReferenceNumberTypeList();
		}

		public void ValidateEntryType(ZPropertyInfo entryTypeInfo, ZString category, ZString countryCode)
		{
		}

		public bool EntryTypeShouldBeUnique(ZString entryType, ZString category, ZString countryCode)
		{
			var registryRow = ((TransportReferenceNumberType)TransportRegistry.Instance.AdditionalReferenceNumbers.Value.FindByCode(entryType));
			if (registryRow != null)
			{
				return registryRow.IsUnique;
			}
			else
			{
				return true;
			}
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.TransportBooking; }
		}

		ZString ITransportParentCore.BillOfLading
		{
			get
			{
				var firstBooking = Bookings.FirstOrDefault();
				return firstBooking != null ? firstBooking.WayBillNumber : ZString.Empty;
			}
		}

		ZString ITransportParentCore.ConsignmentRef
		{
			get
			{
				var firstBooking = Bookings.FirstOrDefault();
				return firstBooking != null ? firstBooking.KM_TransportReference : ZString.Empty;
			}
		}

		ZString ITransportParentCore.ContainerMode
		{
			get { return ""; }
		}

		ZString ITransportParentCore.Description
		{
			get
			{
				var firstAndOnlyBooking = Bookings.Count == 1 ? Bookings.First() : null;
				return firstAndOnlyBooking != null ? firstAndOnlyBooking.KM_JobID : KB_JobID;
			}
		}

		SecurityCheckpoint ITransportParentCore.DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLandTransport; }
		}

		ZString ITransportParentCore.TransportMode
		{
			get { return ""; }
		}

		public bool IsImportingData
		{
			get;
			set;
		}

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return KB_JobID; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return StatusDescription; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.DtbBookingConsolidation; }
		}

		bool IDtbMasterBookingEntity.IsMaster => KB_IsMaster;

		short IDtbMasterBookingEntity.GetMasterBookingVersion() => KB_MasterBookingVersion;

		void IDtbMasterBookingEntity.SetMasterBookingVersion(short newMasterBookingVersion)
		{
			KB_MasterBookingVersion = newMasterBookingVersion;
		}

		IEnumerable<ZPropertyInfo> IDtbMasterBookingEntity.ReplicationFieldInfos
		{
			get
			{
				if (replicationFieldInfos == null)
				{
					replicationFieldInfos = DtbMasterBookingReplication.GetPropertyInfosFromListOfColumns(this, DtbMasterBookingReplication.DtbBookingConsolidationReplicatedColumns);
				}

				return replicationFieldInfos;
			}
		}
		IEnumerable<ZPropertyInfo> replicationFieldInfos;

		DtbMasterBookingHelper MasterBookingHelper { get; }
	}

	public static class DtbBookingDirectionDescription
	{
		public static string GetDescription(DtbBookingDirection direction)
		{
			return GetDescription(direction.ToString());
		}

		public static string GetDescription(string direction)
		{
			string result = "";

			if (direction == nameof(DtbBookingDirection.DLV))
			{
				result = Res.GetString("f7f696f2-9071-4a8a-8365-8397aeeab2d7", "Delivery");
			}
			else if (direction == nameof(DtbBookingDirection.PIC))
			{
				result = Res.GetString("34318368-5cd7-4af9-a451-44c01767a2b2", "Pickup");
			}

			return result;
		}
	}
}
