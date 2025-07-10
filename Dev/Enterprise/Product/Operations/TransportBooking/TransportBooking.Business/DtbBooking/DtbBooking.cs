using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.Packing;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using Common = Enterprise.TransportCommon.Business.Common;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using IDtbTransport = Enterprise.TransportCommon.Business.IDtbTransport;

namespace Enterprise.TransportBookings.Business
{
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.TransportBooking)]
	[VisualizableDocumentsSupportable("DtbBookingVisualizableDocumentSupporter")]
	[CodeProperty(DtbBookingSchema.Constants.KM_JobID)]
	[DescriptionProperty(DtbBookingSchema.Constants.KM_Description)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.DtbTransport)]
	public sealed partial class DtbBooking :
		Common.AutoDtbBooking,
		IDtbBooking,
		ICustomFieldProvider,
		IJobNumberForWorkflow,
		IRelatableActivity,
		ISupportDataImporting,
		IRelatedJob,
		ISupportRelatedJobs,
		ICreditControlledDocumentDelivery,
		IImportExport,
		IAdditionalReferenceNumberValidationProvider,
		IJobCostingPlugIn,
		IAdditionalReferenceNumberTypeProvider,
		IDocAddresses,
		IDtbTransport,
		IEDocsProvider,
		IJobInvoicingPlugIn,
		IRatingSupporter,
		IWorkflowProvider,
		IUniversalXMLNoteParent,
		IAddress,
		ITransportAdditionalReferenceNumbers,
		IDtbMasterBookingEntity,
		ICO2eLocationBasedSupporter,
		IAddressesValidation
	{
		public DtbBooking(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
			ConfirmLoadingCorrectType(factory, dataRow);
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

		public static readonly DtbBookingTypeDecider TypeDecider = new DtbBookingTypeDecider();

		public JobDocAddress Address
		{
			get
			{
				var address = GetAddress(DocAddressType.TransportCompanyDocumentaryAddress);
				if (address != null)
				{
					address.ReadOnly = ReadOnly || IsOnMultiJobConsolidation;
				}

				return address;
			}
		}

		public DtbAgentBooking AgentBooking => Factory.LoadFromUniqueKey<DtbAgentBooking>(DtbAgentBookingSchema.LTB_KM_TransportBooking, PK);

		bool IsOKToChangeBillingParty()
		{
			var result = true;

			if (ConsolidationSingleJob != null && ConsolidationSingleJob.Parent != null)
			{
				var queryArgs = GetChangeBillingPartyOptionQueryArgs();
				ConsolidationSingleJob.NotificationSubscriber.QueryUser(queryArgs);

				result = queryArgs.Response;
			}
			return result;
		}

		QueryUserYesNoEventArgs GetChangeBillingPartyOptionQueryArgs()
		{
			return new QueryUserYesNoEventArgs(Res.GetString("cd483a54-ed06-40de-8145-ba67c029a9ad", @"Your Transport Booking has a parent job, changing the Billing Party will change the local client on the parent job.
Are you sure you want to continue?"), false);
		}

		public DtbBookingTmpl BookingTemplate
		{
			get { return (DtbBookingTmpl)Factory.LoadFromNaturalKey(BookingTemplateType, DtbBookingTmplSchema.KT_Code, KM_KT_NKBookingTemplate); }
		}

		Type BookingTemplateType
		{
			get { return typeof(DtbBookingTmpl); }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var relatedObjects = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				var job = GetJobHeaderForWorkflow();
				if (job != null)
				{
					relatedObjects.Add(job);
				}

				var dtbBookingInstructions = Instructions.Cast<DtbBookingInstruction>();
				relatedObjects.AddRange(dtbBookingInstructions);
				relatedObjects.AddRange(dtbBookingInstructions.SelectMany(i => i.Confirmations.Cast<DtbBookingConfirmation>()));

				if (ConsolidationSingleJob != null) // tested in IDtbBookingParentTestCase.TestRelatedTransportBookingObjectsWithEDocs
				{
					var parentInfo = ConsolidationSingleJob.Parent;
					if (parentInfo != null)
					{
						var parentJob = parentInfo.ParentWithWorkflow;
						if (parentJob != null)
						{
							relatedObjects.Add(parentJob);
						}
					}
				}

				if (LandTransportConsignment != null)
				{
					relatedObjects.Add((BusinessObject)LandTransportConsignment);
				}

				if (IsSub && MasterBooking != null)
				{
					relatedObjects.Add(MasterBooking);
				}

				if (KM_IsMaster && SubBookings != null)
				{
					relatedObjects.AddRange(SubBookings);
				}

				return relatedObjects.ToArray();
			}
		}

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, KM_PL_NKCarrierServiceLevel)); }
		}

		public override ZString KM_RS_NKServiceLevel
		{
			get
			{
				return base.KM_RS_NKServiceLevel;
			}
			set
			{
				base.KM_RS_NKServiceLevel = value;
				UpdateConfirmationsEstimateTime();
			}
		}

		[List("Lookups.CarrierAccounts")]
		public override ZGuid KM_OAN_CarrierAccount
		{
			get { return base.KM_OAN_CarrierAccount; }
			set { base.KM_OAN_CarrierAccount = value; }
		}

		public JobDocAddress CarrierBookingAgentDocAddress
		{
			get
			{
				if (carrierBookingAgentDocAddress == null || carrierBookingAgentDocAddress.IsDeleted)
				{
					var requirement = ((IDocAddresses)this).GetDocAddressRequirement(DocAddressType.CarrierBookingAgent);
					carrierBookingAgentDocAddress = DocAddresses.FindOrCreateWithRequirement(requirement);
				}
				return carrierBookingAgentDocAddress;
			}
		}
		JobDocAddress carrierBookingAgentDocAddress;

		public OrgHeader CarrierBookingAgent => GetAddress(DocAddressType.CarrierBookingAgent)?.Organisation;

		public DtbTransportConsolidation ConsignmentConsol
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, PK);
				query.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, TransportConsolidationJobTypes.Codes.Consignment);
				return Factory.LoadTop1<DtbTransportConsolidation>(query);
			}
		}

		public DtbBooking MasterBooking
		{
			get
			{
				if (masterBooking == null)
				{
					masterBooking = Factory.Load<DtbBooking>(KM_KM_MasterBooking);
				}

				return masterBooking;
			}
		}

		DtbBooking masterBooking;

		public PkgPackageJob PackageJob
		{
			get { return ConsolidationSingleJob != null ? ConsolidationSingleJob.PackageJob : null; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public DtbBookingConsolidation ConsolidationSingleJob
		{
			get
			{
				if (consolidationSingleJob == null)
				{
					consolidationSingleJob = (DtbBookingConsolidation)Factory.Load(ConsolidationType, KM_KB_Booking);
					if (consolidationSingleJob != null && DtbChildEditableService.GetState(Factory) == DtbChildEditableServiceState.Transport)
					{
						RegisterEditableChildObject(consolidationSingleJob);
					}
				}
				return consolidationSingleJob;
			}
		}

		DtbBookingConsolidation consolidationSingleJob;

		IDtbBookingConsolidation IDtbBooking.ConsolidationSingleJob => ConsolidationSingleJob;

		public IEnumerable<StmUniversalJobLink> JobLinks
		{
			get
			{
				if (jobLinks == null)
				{
					var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, PK);
					query.AddToFilter(StmUniversalJobLinkSchema.UCL_ParentTableCode, TablePrefix);
					jobLinks = Factory.Load<StmUniversalJobLink>(query);
				}
				return jobLinks;
			}
		}

		IEnumerable<StmUniversalJobLink> jobLinks;

		Type ConsolidationType
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		public DtbBookingConsolidation ConsolidationMultiJob
		{
			get { return Factory.Load<DtbBookingConsolidation>(KM_KB_BookingConsolidationMultiJob); }
		}

		[ChildEditable]
		public DtbBookingInstructionCollection Instructions
		{
			get
			{
				if (instructions == null)
				{
					instructions = GetNewInstructionsCollection();
					RegisterEditableChildObject(instructions);
					instructions.CountChanged += Instructions_CountChanged;
					instructions.CollectionCountChange += Instructions_CollectionCountChanged;
				}

				return instructions;
			}
		}

		void Instructions_CountChanged(object sender, EventArgs e)
		{
			OnInstructionsCountChanged();
		}

		void OnInstructionsCountChanged()
		{
			if (UpdateStatusOnInstructionsCountChanged)
			{
				UpdateStatus();
			}
		}

		void Instructions_CollectionCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e, (NoResString)"Instruction"), IsCopying);
		}

		DtbBookingInstructionCollection instructions;

		bool IsInstructionsInstatiated
		{
			get
			{
				return instructions != null;
			}
		}

		DtbBookingInstructionCollection GetNewInstructionsCollection()
		{
			return new DtbBookingInstructionCollection(this);
		}

		bool UpdateStatusOnInstructionsCountChanged
		{
			get { return !IsDeleted && !IsHeld; }
		}

		[ChildEditableTestExclude]
		[ChildEditable]
		public JobHeader Job
		{
			get
			{
				var job = new JobHeader.Loader(this).Load();
				if (job != null && !IsRegisteredEditableChildObject(job))
				{
					RegisterEditableChildObject(job);
					job.LocalChargesAddrChanged += Job_LocalChargesAddrChanged;
				}

				return job;
			}
		}

		void Job_LocalChargesAddrChanged(object sender, EventArgs e)
		{
			UpdateConsigneeATL();
		}

		void UpdateConsigneeATL()
		{
			if (instructions != null)
			{
				foreach (DtbBookingInstruction i in instructions)
				{
					i.UpdateAuthorisedToLeave();
				}
			}
		}

		IJobHeader IDtbTransport.Job
		{
			get { return Job; }
		}

		JobHeader GetJobHeaderForWorkflow()
		{
			return new JobHeader.Loader(this).Load(true, false);
		}

		public DtbBookingInstructionPkgDivotCollection PackageDivots
		{
			get { return packageDivots ?? (packageDivots = GetNewInstructionPkgDivotCollection()); }
		}

		DtbBookingInstructionPkgDivotCollection packageDivots;

		DtbBookingInstructionPkgDivotCollection GetNewInstructionPkgDivotCollection()
		{
			return new DtbBookingInstructionPkgDivotCollection(this);
		}

		public PackageCollectionForBooking AssignedPackages
		{
			get { return packages ?? (packages = new PackageCollectionForBooking(this)); }
		}

		PackageCollectionForBooking packages;

		[ChildEditable]
		public DtbBookingPackageCollection_PackageView Packages_PackageView
		{
			get
			{
				if (packages_PackageView == null)
				{
					InitialisePackages_PackageView();
				}
				else if (Factory.AreCollectionListChangedEventsDelayed)
				{
					packages_PackageView.RefreshCollection();
				}

				return packages_PackageView;
			}
		}

		void InitialisePackages_PackageView()
		{
			packages_PackageView = GetNewPackages_PackageView();
			packages_PackageView.Initialise();
			RegisterEditableChildObject(packages_PackageView);
		}

		DtbBookingPackageCollection_PackageView packages_PackageView;

		DtbBookingPackageCollection_PackageView GetNewPackages_PackageView()
		{
			return new DtbBookingPackageCollection_PackageView(this);
		}

		public IDtbParentInfo ParentJob
		{
			get { return ConsolidationSingleJob != null ? ConsolidationSingleJob.Parent : null; }
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);
				foreach (DtbBookingInstruction instruction in Instructions)
				{
					var address = instruction.Address;
					if (!address.E2_AddressOverride)
					{
						result.Add(address.Organisation);
					}
				}
				if (KM_IsMaster)
				{
					result.AddRange(SubBookings);
				}
				if (KM_KM_MasterBooking != ZGuid.Empty)
				{
					result.Add(MasterBooking);
				}
				AddBusinessObjectsWithRelatedNotes(result);
				var parentJob = ParentJob;
				var parent = parentJob != null ? parentJob.ParentWithWorkflow : null;
				if (parent != null)
				{
					result.Add(parent);
				}

				if (Address.Organisation != null)
				{
					result.Add(Address.Organisation);
				}

				return result.ToArray();
			}
		}

		public RelatedJobCollection RelatedJobs
		{
			get { return relatedJobs ?? (relatedJobs = GetRelatedJobs()); }
		}

		RelatedJobCollection GetRelatedJobs()
		{
			var result = new RelatedJobCollection(Factory);

			// add ConsolidationMultiJob
			if (ConsolidationMultiJob != null)
			{
				result.Add(ConsolidationMultiJob);
			}

			// add parent
			var parentJob = ParentJob;
			if (parentJob?.ParentWithWorkflow != null)
			{
				result.Add(parentJob.ParentWithWorkflow);
			}

			// add master
			if (MasterBooking != null)
			{
				result.Add(MasterBooking);
			}

			// add subs
			if (KM_IsMaster)
			{
				result.AddRange(SubBookings);
			}

			// add port transports
			result.AddRange((BusinessObject[])GetPortTransportJobs());

			// add Land transports
			result.AddRange((BusinessObject[])GetLandTransportJobs());

			// add DCN
			result.AddRange((BusinessObject[])GetDispatchConsignmentJobs());

			return result;
		}

		public ICommonCartage[] GetPortTransportJobs()
		{
			var query = new ZQuery(JobCartageSchema.JJ_ParentID, PK);
			query.IgnoreActiveFilter = true;
			return Factory.Load<ICommonCartage>(query);
		}

		public IDtbConsignment[] GetLandTransportJobs()
		{
			return Factory.Load<IDtbConsignment>(new ZQuery(DtbConsignmentSchema.LTC_KM_Booking, PK));
		}

		public IWhsItemDispatchConsignment[] GetDispatchConsignmentJobs()
		{
			var dispatchConsignmentQuery = new ZDBOnlyQuery(typeof(IWhsItemDispatchConsignment));
			var subStmJobLinkQuery = new ZDBOnlySubQuery(typeof(IStmUniversalJobLink), StmUniversalJobLinkSchema.UCL_SourceKey, WhsItemDispatchConsignmentSchema.WDC_JobID);
			subStmJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_ParentID, this.PK);
			dispatchConsignmentQuery.AddSubQuery(subStmJobLinkQuery, JoinCondition.And);
			return Factory.Load<IWhsItemDispatchConsignment>(new ZQuery(dispatchConsignmentQuery));
		}

		RelatedJobCollection relatedJobs;

		public IDtbConsignment LandTransportConsignment
		{
			get { return Factory.LoadTop1<IDtbConsignment>(new ZQuery(DtbConsignmentSchema.LTC_KM_Booking, PK)); }
		}

		public ViewTransportBookingParents ParentView
		{
			get
			{
				if (parentView == null && ConsolidationSingleJob != null)
				{
					parentView = Factory.LoadTop1<ViewTransportBookingParents>(new ZQuery(ViewTransportBookingParentsSchema.PK, ConsolidationSingleJob.KB_ParentID));
				}
				return parentView;
			}
		}

		ViewTransportBookingParents parentView;

		public ViewTransportBookingParentLegs Schedule
		{
			get
			{
				var bookingParentView = ParentView;
				if (schedule == null && bookingParentView != null && ConsolidationSingleJob != null)
				{
					var query = new ZQuery(ViewTransportBookingParentLegsSchema.PK, ConsolidationSingleJob.KB_ParentID);
					if (IsPickupDirection)
					{
						query.AddToFilter(ViewTransportBookingParentLegsSchema.VL_RL_NKLoad, bookingParentView.VP_RL_NKOrigin);
					}
					else
					{
						query.AddToFilter(ViewTransportBookingParentLegsSchema.VL_RL_NKDischarge, bookingParentView.VP_RL_NKDestination);
					}

					schedule = Factory.LoadTop1<ViewTransportBookingParentLegs>(query);
				}

				return schedule;
			}
		}

		ViewTransportBookingParentLegs schedule;

		public IEnumerable<DtbBookingPackage_PackageView> Containers
		{
			get { return Packages_PackageView.Cast<DtbBookingPackage_PackageView>().Where(p => p.Package.IsContainer); }
		}

		public IEnumerable<DtbBookingPackage_PackageView> LoosePackages
		{
			get { return Packages_PackageView.Cast<DtbBookingPackage_PackageView>().Where(p => !p.Package.IsContainer); }
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				var noteContexts = base.NoteContextsForRelatedNotes;
				noteContexts.Module |= StmNoteContextModule.T; // transport
				AddNoteContextsForRelatedNotes_Directions(noteContexts);
				AddNoteContextsForRelatedNotes_FreightModes(noteContexts);

				return noteContexts;
			}
		}

		void AddNoteContextsForRelatedNotes_Directions(StmNoteContexts noteContexts)
		{
			noteContexts.Direction |= StmNoteContextDirection.D; // domestic

			if (IsDeliveryDirection)
			{
				noteContexts.Direction |= StmNoteContextDirection.I; // import
				noteContexts.Direction |= StmNoteContextDirection.B; // destination
			}
			else if (IsPickupDirection)
			{
				noteContexts.Direction |= StmNoteContextDirection.E; // export
				noteContexts.Direction |= StmNoteContextDirection.B; // origin
			}
		}

		void AddNoteContextsForRelatedNotes_FreightModes(StmNoteContexts noteContexts)
		{
			noteContexts.FreightMode |= StmNoteContextFreightMode.R; // road

			if (IsContainerised)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.F; // fcl
			}

			if (IsLoose)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.L; // lcl
			}
		}

		// persistent

		public override ZBool KM_IsActive
		{
			get { return base.KM_IsActive; }
			set
			{
				base.KM_IsActive = value;
				UpdateStatus();
				UpdateReadOnlyForWhenIsActive();
			}
		}

		[List("Lookups.BookingTemplates")]
		public override ZString KM_KT_NKBookingTemplate
		{
			get { return KM_KT_NKBookingTemplateCache ?? base.KM_KT_NKBookingTemplate; }
			set
			{
				DefaultTemplateWithNewValue(value);
				base.KM_KT_NKBookingTemplate = value;
			}
		}

		void DefaultTemplateWithNewValue(ZString newValue)
		{
			if (!IsSub)
			{
				using (new DisposableAction(
					() => KM_KT_NKBookingTemplateCache = newValue,
					() => KM_KT_NKBookingTemplateCache = null))
				{
					DefaultFromTemplate();
				}
			}
		}

		ZString? KM_KT_NKBookingTemplateCache;

		void DefaultFromTemplate()
		{
			var bookingTemplate = BookingTemplate;
			if (bookingTemplate != null)
			{
				var existingPackages = GetPreExistingAssignedPackages();

				KM_Description = bookingTemplate.KT_Description;
				KM_Direction = bookingTemplate.KT_Direction; // required to default confirmations
				KM_RatingFreightMode = bookingTemplate.KT_RatingFreightMode;

				if (!AddingInstructionsFromTemplateSemaphore.IsSuspended)
				{
					DeleteAllInstructions();

					bookingTemplate.Instructions.ApplySort(DtbBookingInstructionTmplSchema.Constants.K2_Sequence, ListSortDirection.Ascending);
					Array.ForEach(bookingTemplate.Instructions.ToArray(), t => AddInstructionFromTemplate(t, existingPackages));
				}

				var packageJobHasPackages = (PackageJob != null) && PackageJob.Packages.Any();
				if (!AssignedPackages.Any() && packageJobHasPackages)
				{
					var message = Res.GetString("650ca84d-1f1d-4cb4-a032-c6249875f64f", @"Could not find Containers/Outer Packages based on Instruction Template Package Type. See Packages Tab.");
					ConsolidationSingleJob.NotificationSubscriber.Notify(new WarningNotification(message));
				}
			}
			else
			{
				KM_Description = "";
			}
		}

		Collection<PkgPackage> GetPreExistingAssignedPackages()
		{
			var existingPackages = new Collection<PkgPackage>();
			var assignedPackageExist = AssignedPackages.Count > 0;
			if (assignedPackageExist)
			{
				foreach (PkgPackage package in AssignedPackages)
				{
					existingPackages.Add(package);

					if (package.IsContainer)
					{
						foreach (var subPackage in package.Packages)
						{
							existingPackages.Add(subPackage);
						}
					}
					else
					{
						if (package.ParentPackage != null)
						{
							existingPackages.Add(package.ParentPackage);
						}
					}
				}
			}
			return existingPackages;
		}

		public Semaphore AddingInstructionsFromTemplateSemaphore
		{
			get { return addingInstructionsFromTemplateSemaphore ?? (addingInstructionsFromTemplateSemaphore = new Semaphore()); }
		}

		Semaphore addingInstructionsFromTemplateSemaphore;

		void AddInstructionFromTemplate(DtbBookingInstructionTmpl instructionTemplate, Collection<PkgPackage> existingPackages)
		{
			var instruction = Instructions.AddNew();
			instruction.KN_Sequence = instructionTemplate.K2_Sequence;
			instruction.KN_InstructionType = instructionTemplate.K2_InstructionType;
			instruction.OrganisationType = instructionTemplate.K2_OrgType;

			using (SuspendSettingPackages()) // don't default packages when PackageCategory set
			{
				instruction.PackageCategory = instructionTemplate.K2_PackageType;
			}

			instruction.DefaultPackages(existingPackages);

			instruction.KN_IsContainerRateable = instructionTemplate.K2_IsContainerRateable;
			instruction.KN_IsLooseRateable = instructionTemplate.K2_IsLooseRateable;
			instruction.DefaultDropMode(instructionTemplate);
		}

		public void DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions()
		{
			if (!KM_IsMaster)
			{
				return;
			}

			var currentSubBookingsPackages = SubBookingPackages;
			foreach (var instruction in Instructions)
			{
				using (SuspendSettingPackages())
				{
					instruction.DefaultPackageCategoryIfEmpty();
				}

				instruction.DefaultPackagesWithoutFallback(currentSubBookingsPackages);
			}
		}

		public void DefaultPackagesIfMasterFromSubBookingsForAllInstructions(List<DtbBooking> attachedSubBookings)
		{
			if (!KM_IsMaster)
			{
				return;
			}
			var subBookingsBeforeAttach = SubBookings.Where(sb => !attachedSubBookings.Contains(sb));
			var subBookingsBeforeAttachPackages = subBookingsBeforeAttach.SelectMany(sb => sb.AssignedPackages);

			var newPackagesOnBooking = attachedSubBookings.SelectMany(booking => booking.AssignedPackages).Except(subBookingsBeforeAttachPackages);
			foreach (var instruction in Instructions)
			{
				using (SuspendSettingPackages())
				{
					instruction.DefaultPackageCategoryIfEmpty();
				}

				instruction.DefaultPackagesWithoutFallback(newPackagesOnBooking, shouldRemoveExistingPackages: false);
			}
		}

		public void DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(List<DtbBooking> detachedSubBookings)
		{
			if (!KM_IsMaster)
			{
				return;
			}

			var formerlyAttachedSubBookings = detachedSubBookings.FindAll(b => (ZGuid)b.KM_KM_MasterBookingInfo.OriginalValue != ZGuid.Empty);

			var divotsToCreatePerInstruction = GetDivotsThatWillNeedToBeRecreatedPerInstruction(detachedSubBookings);

			foreach (var subBooking in formerlyAttachedSubBookings)
			{
				foreach (var subInstruction in subBooking.Instructions)
				{
					RecreateMasterInstructionDocAddressInDetachedInstruction(subInstruction);
					DetachConfirmationsInDetachedSubBookingsInstructions(subInstruction);
					PersistJobDocAddress(subInstruction);
				}
			}

			RecreateSubBookingInstructionPackageDivots(divotsToCreatePerInstruction);

			var packageIdsToRemoveFromMaster = divotsToCreatePerInstruction
				.SelectMany(kvp => kvp.Value)
				.Select(t => t.KD_KP_Package)
				.ToHashSet();

			RemovePackageDivotsFromMasterBookingInstructions(packageIdsToRemoveFromMaster);
		}

		Dictionary<DtbBookingInstruction, List<(ZGuid KD_KP_Package, ZInt KD_Quantity)>> GetDivotsThatWillNeedToBeRecreatedPerInstruction(List<DtbBooking> detachedSubBookings)
		{
			Dictionary<DtbBookingInstruction, List<(ZGuid KD_KP_Package, ZInt KD_Quantity)>> divotsCreatedPerInstruction = new();
			foreach (var subBooking in detachedSubBookings)
			{
				foreach (var instruction in subBooking.Instructions)
				{
					var divotsToBeCreated =
						instruction.DivotsWithPackages.Select(x =>
						{
							return (x.KD_KP_Package, x.KD_Quantity);
						})
						.ToList();
					divotsCreatedPerInstruction[instruction] = divotsToBeCreated;
				}
			}
			return divotsCreatedPerInstruction;
		}

		void RecreateMasterInstructionDocAddressInDetachedInstruction(DtbBookingInstruction detachedInstruction)
		{
			var masterInstruction = detachedInstruction.MasterBookingInstruction;
			detachedInstruction.KN_KN_MasterBookingInstruction = ZGuid.Empty;
			detachedInstruction.KN_MasterBookingVersion = (short)0;
			foreach (var docAddress in masterInstruction.DocAddresses)
			{
				detachedInstruction.DocAddresses.Add(docAddress.Clone());
			}
		}

		void DetachConfirmationsInDetachedSubBookingsInstructions(DtbBookingInstruction detachedInstruction)
		{
			foreach (var confirmation in detachedInstruction.Confirmations)
			{
				confirmation.KK_KK_MasterBookingConfirmation = ZGuid.Empty;
				confirmation.KK_MasterBookingVersion = (short)0;
			}
		}

		void PersistJobDocAddress(DtbBookingInstruction detachedInstruction)
		{
			_ = detachedInstruction.Address; // Calls MakePersistentEvenIfEmpty, ensuring the JobDocAddress for Address gets persisted}
		}

		void RecreateSubBookingInstructionPackageDivots(
			IDictionary<DtbBookingInstruction, List<(ZGuid KD_KP_Package, ZInt KD_Quantity)>> divotsToRecreatePerInstruction)
		{
			foreach (var instructionDivots in divotsToRecreatePerInstruction)
			{
				var instruction = instructionDivots.Key;
				if ((ZGuid)instruction.Booking.KM_KM_MasterBookingInfo.OriginalValue != ZGuid.Empty)
				{
					var divotsToCreate = instructionDivots.Value;
					foreach (var (kD_KP_Package, kD_Quantity) in divotsToCreate)
					{
						var recreatedDivot = instruction.PackageDivots.AddNew();
						recreatedDivot.KD_KP_Package = kD_KP_Package;
						recreatedDivot.KD_Quantity = kD_Quantity;
					}
				}
			}
		}

		void RemovePackageDivotsFromMasterBookingInstructions(IEnumerable<ZGuid> packageIDsToRemove)
		{
			var packagesToRemoveFromDivots = AssignedPackages.Except(SubBookingPackages)
				.Where(p => packageIDsToRemove.Contains(p.PK));

			foreach (var instruction in Instructions)
			{
				foreach (var package in packagesToRemoveFromDivots)
				{
					instruction.DivotsWithPackages.RemovePackage(package);
				}
			}
		}

		public void DefaultPackages(IEnumerable<PkgPackage> packages, IReadOnlyDictionary<PkgPackage, ZString> releaseNumbersByPackage)
		{
			foreach (DtbBookingInstruction instruction in Instructions)
			{
				using (instruction.SetReleaseNumbersByPackage(releaseNumbersByPackage))
				{
					instruction.DefaultPackages(packages);
				}
			}
		}

		// testing in DtbBookingConsolidationDataObjectReader
		public void DefaultPackages(IEnumerable<ZString> containerIDs)
		{
			var packagesAllowedToBeDefaulted = new List<PkgPackage>();

			// add containers that match container IDs
			var containers = ConsolidationSingleJob.PackageJob.Packages.Where(p => p.IsContainer && containerIDs.Contains(p.PackageIDWithFallback));
			packagesAllowedToBeDefaulted.AddRange(containers);

			// add all free loose (those not packed into a container) This occurs in Customs where they don't always specify how a container is packed :(
			var freeLoose = ConsolidationSingleJob.PackageJob.Packages.Where(p => !p.IsContainer);
			packagesAllowedToBeDefaulted.AddRange(freeLoose);

			DefaultPackages(packagesAllowedToBeDefaulted, null);
		}

		[RelatedBusinessObject("ConsolidationSingleJob")]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZGuid KM_KB_Booking
		{
			get { return base.KM_KB_Booking; }
			set
			{
				base.KM_KB_Booking = value;
				SetDefaultValuesFromParent(); // reset even if no changes
			}
		}

		void SetDefaultValuesFromParent()
		{
			var consolidation = ConsolidationSingleJob;
			if (consolidation != null)
			{
				var parentInfo = consolidation.Parent;
				if (parentInfo != null)
				{
					if (!IsSettingServiceLevelSuspended)
					{
						KM_RS_NKServiceLevel = parentInfo.ClientServiceLevel;
					}

					parentInfo.UpdateAddress(Address, true, null, null);

					if (parentInfo.CarrierServiceLevel.HasValue)
					{
						KM_PL_NKCarrierServiceLevel = parentInfo.CarrierServiceLevel.Value;
					}

					if (parentInfo.TransportReference.HasValue)
					{
						KM_TransportReference = parentInfo.TransportReference.Value;
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString KM_Status
		{
			get { return base.KM_Status; }
			set
			{
				if (base.KM_Status != value)
				{
					base.KM_Status = value;
					if (KM_IsActive)
					{
						SetReadOnlyIncludingChildren(IsHeld);
					}
					UpdateConsolidatedBookingStatus();
				}
			}
		}

		public bool DelayInstructionUpdatesFromAddingDivots { get; set; }

		public override ZGuid KM_KM_MasterBooking
		{
			get { return base.KM_KM_MasterBooking; }
			set
			{
				if (base.KM_KM_MasterBooking != value)
				{
					if (MasterBooking != null)
					{
						MasterBooking.PackageJob.ShouldReloadPackagesCollection = true;
						((DtbBookingConsolidationPkgPackageJob)MasterBooking.PackageJob).SubBookingPKsToInclude.Remove(PK);
						((DtbBookingConsolidationPkgPackageJob)MasterBooking.PackageJob).SubConsolidationPKsToExclude.Add(ConsolidationSingleJob.PK);
					}

					base.KM_KM_MasterBooking = value;
					masterBooking = null;
					docAddresses = null;

					if (MasterBooking != null)
					{
						MasterBooking.PackageJob.ShouldReloadPackagesCollection = true;
						((DtbBookingConsolidationPkgPackageJob)MasterBooking.PackageJob).SubBookingPKsToInclude.Add(PK);
						((DtbBookingConsolidationPkgPackageJob)MasterBooking.PackageJob).SubConsolidationPKsToExclude.Remove(ConsolidationSingleJob.PK);
					}
				}
			}
		}

		public void ToggleHeldStatus(INotifications notify)
		{
			if (IsAvailable)
			{
				KM_Status = TransportStatuses.Codes.Held;
			}
			else if (IsHeld)
			{
				UpdateStatus();
			}
			else
			{
				notify.AddError(Res.GetString("6c812dd3-294e-42d0-8974-d6e77896854c",
					"This Booking cannot be put on hold because the status is {0}. Only Bookings with a status of {1} can be held.",
					StatusDescription, TransportStatuses.Descriptions.Available));
			}
		}

		ZString GetExpectedStatus()
		{
			ZString result;

			if (IsActionRequired)
			{
				result = TransportStatuses.Codes.ActionRequired;
			}
			else if (IsConsideredDelivered)
			{
				result = TransportStatuses.Codes.Delivered;
			}
			else if (IsConsideredDelivered_EmptiesNotReturned)
			{
				result = TransportStatuses.Codes.DeliveredEmptyNotReturned;
			}
			else if (IsConsideredPickedUp)
			{
				result = TransportStatuses.Codes.PickedUp;
			}
			else if (IsConsideredAvailable)
			{
				result = TransportStatuses.Codes.Available;
			}
			else if (!KM_IsActive)
			{
				result = TransportStatuses.Codes.Deactivated;
			}
			else if (IsPickupCommenced)
			{
				result = TransportStatuses.Codes.PickUpCommenced;
			}
			else if (IsPickUpConfirmed)
			{
				result = TransportStatuses.Codes.PickUpConfirmed;
			}
			else if (IsServiceCommenced)
			{
				result = TransportStatuses.Codes.ServiceCommenced;
			}
			else if (HasServiceCommencedLogs || IsConsideredServiceCommenced)
			{
				result = TransportStatuses.Codes.ServiceCommenced;
			}
			else
			{
				result = TransportStatuses.Codes.Available;
			}

			return result;
		}

		public bool HasServiceCommencedLogs
		{
			get
			{
				Func<EnterpriseBusinessObject, bool> hasServiceCommencedLogs =
					(bizo) =>
					{
						var mostRecentServiceCommenced = bizo.Logs.MostRecentLogByEventTime(Events.ServiceCommenced);
						var mostRecentServiceCancelled = bizo.Logs.MostRecentLogByEventTime(Events.ServiceCancelled);
						return mostRecentServiceCommenced != null
							&& (mostRecentServiceCancelled == null ||
								mostRecentServiceCommenced.SL_EventTime > mostRecentServiceCancelled.SL_EventTime);
					};

				var result = hasServiceCommencedLogs(this);

				if (!result)
				{
					var confirmations = Instructions.SelectMany(i => i.Confirmations).ToList();
					confirmations.ForEach(c => Factory.AddFetchHint(StmALogSchema.SL_Parent, c.PK));
					result = confirmations.Any(hasServiceCommencedLogs);
				}

				return result;
			}
		}

		void UpdateConsolidatedBookingStatus()
		{
			var consolidation = ConsolidationSingleJob;
			var consolidationMultiJob = ConsolidationMultiJob;

			if (consolidation != null)
			{
				consolidation.UpdateStatus();
			}

			if (consolidationMultiJob != null)
			{
				consolidationMultiJob.UpdateStatus();
			}
		}

		public void UpdateStatus()
		{
			if (KM_Status != TransportStatuses.Codes.Incomplete && KM_Status != TransportStatuses.Codes.Quote)
			{
				var result = GetExpectedStatus();
				if (result.IsEmpty)
				{
					result = TransportStatuses.Codes.Available;
				}
				KM_Status = result;
			}
		}

		[List("Lookups.RatingFreightModes")]
		public override ZString KM_RatingFreightMode
		{
			get { return base.KM_RatingFreightMode; }
			set { base.KM_RatingFreightMode = value; }
		}

		public override ZString KM_TransportReference
		{
			get { return base.KM_TransportReference; }
			set
			{
				var originalCostReference = ChargeCostReference;
				base.KM_TransportReference = value;
				UpdateChargeCostReferences(originalCostReference);
			}
		}

		void UpdateChargeCostReferences(ZString originalCostReference)
		{
			if (!((ISupportDataImporting)this).IsImportingData)
			{
				ObjectFactory.Get<IJobProviderForInvoicing>().ParentOperationalJobRefChanged(originalCostReference, this, IsInDatabase);
			}
		}

		public override ZString KM_JobID
		{
			get { return base.KM_JobID; }
			set
			{
				var originalCostReference = ChargeCostReference;
				base.KM_JobID = value;
				UpdateChargeCostReferences(originalCostReference);
			}
		}

		[ReadOnlyMember(nameof(IsKM_ChargeableReadOnly))]
		public override ZDecimal KM_Chargeable
		{
			get { return KM_OverrideChargeable ? base.KM_Chargeable : CalculateChargeableVolumeWeight(TotalWeight, TotalVolume, ChargeableUnits); }
			set { base.KM_Chargeable = value; }
		}

		bool IsKM_ChargeableReadOnly
		{
			get
			{
				return !KM_OverrideChargeable;
			}
		}

		public ZDecimal KM_Calc_ActualVolumeWeight
		{
			get
			{
				var unit = KM_Calc_ActualVolumeWeightUnit;
				if (unit.IsEmpty)
				{
					return 0m;
				}

				var conversionFactor = GetBookingConversionFactor();
				var quantityToConvert = IsBookingChargeableByWeight
					? new ZVolume(TotalVolume.Amount, TotalVolume.Unit)
					: (IQuantity)new ZWeight(TotalWeight.Amount, TotalWeight.Unit);

				if (conversionFactor.IsEmpty || !quantityToConvert.IsValid)
				{
					return 0m;
				}

				var conversion = quantityToConvert.Convert(unit, new[] { conversionFactor }, true)?.Amount ?? 0m;
				return conversion;
			}
		}

		ConversionFactor GetBookingConversionFactor()
		{
			var chargeableFactor = ChargeableFactorFromRegistry;
			if (chargeableFactor == null)
			{
				return ConversionFactor.Empty;
			}

			var isImperial = IsBookingChargeableByWeight
				? Constants.Weight.IsImperial(KM_ChargeableUnit)
				: Constants.Volume.IsImperial(KM_ChargeableUnit);

			return isImperial ? chargeableFactor.ImperialFactor : chargeableFactor.MetricFactor;
		}

		ChargeableFactor ChargeableFactorFromRegistry
		{
			get { return TransportRegistry.Instance.TransportBookingChargeableFactor.Value; }
		}

		public bool IsBookingChargeableByWeight
		{
			get { return Constants.Weight.ContainsCode(KM_ChargeableUnit); }
		}

		public ZString KM_ChargeableUnit
		{
			get { return ChargeableAmountCalculator.GetChargeableUnit(Constants.TransportModes.Road, TotalWeight.Unit, TotalVolume.Unit); }
		}

		public ZString KM_Calc_ActualVolumeWeightUnit
		{
			get
			{
				if (IsBookingChargeableByWeight && Constants.Weight.ContainsCode(TotalWeight.Unit))
				{
					return TotalWeight.Unit;
				}
				else if (Constants.Volume.ContainsCode(TotalVolume.Unit))
				{
					return TotalVolume.Unit;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public override ZDateTime KM_BookingOfTransportRequestedDate
		{
			get => base.KM_BookingOfTransportRequestedDate;
			set
			{
				base.KM_BookingOfTransportRequestedDate = value;
				if (ConsolidationSingleJob?.ParentBO is IForwardingShipment shipment)
				{
					using (shipment.DocsAndCartage.TemporarilySetTransportBookingEventReference(KM_JobID))
					{
						if (ConsolidationSingleJob?.Direction == DtbBookingDirection.PIC)
						{
							shipment.DocsAndCartage.JP_PickupCartageAdvised = value;
						}
						else if (ConsolidationSingleJob?.Direction == DtbBookingDirection.DLV)
						{
							shipment.DocsAndCartage.JP_DeliveryCartageAdvised = value;
						}
					}
				}
			}
		}

		[List("Lookups.BookingTransportModes")]
		[ResourceStringData("DtbBooking|KM_TransportMode", ShortCaption = "Mode", MediumCaption = "Transport Mode", Caption = "Booking Transport Mode")]
		public override ZString KM_TransportMode { get => base.KM_TransportMode; set => base.KM_TransportMode = value; }

		public List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)> addressDetails, bool shouldFallbackToShipmentContainerYard)> ParentContainerLinksAndAddresses { get; set; }

		public IDictionary<ZInt, PkgPackage> PackageContainerLinks { get; set; }

		// calculated

		public bool BookingIsBeingManagedByAuthorisedCarrierBookingAgent
		{
			get
			{
				return CarrierBookingAgentIsAuthorised && IsServiceCommenced;
			}
		}

		bool CarrierBookingAgentIsAuthorised
		{
			get
			{
				var result = false;

				if (CarrierBookingAgent != null)
				{
					foreach (EDICommunicationsMode mode in CarrierBookingAgent.EDICommunicationsModes)
					{
						if (mode.EK_Module == RelatableActivityTypeList.Codes.TransportBooking && mode.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService && DtbAgentBooking.IsAuthorisedCarrierBookingAgent(mode.EK_Destination))
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		public TransportBookingAdditionalReferenceCollection AdditionalReferencesForBinding
		{
			get
			{
				if (additionalReferencesForBinding == null)
				{
					additionalReferencesForBinding = new TransportBookingAdditionalReferenceCollection(this);
					additionalReferencesForBinding.BuildCollection();
				}
				return additionalReferencesForBinding;
			}
		}
		TransportBookingAdditionalReferenceCollection additionalReferencesForBinding;

		public ZString ChargeableUnits
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		ZDecimal CalculateChargeableVolumeWeight(ZWeight weight, ZVolume volume, string targetUnit)
		{
			var result = ZDecimal.Zero;

			var chargeableWeight = GetChargeableWeight(weight, volume);
			if (chargeableWeight.IsValid)
			{
				var chargeableAmount = Constants.Weight.Convert(chargeableWeight.Amount, chargeableWeight.Unit, targetUnit, false);
				return Utilities.Round(chargeableAmount, 3);
			}

			return result;
		}

		ZDecimal CalculateChargeableVolumeWeightRoundedFromRegistrySettings(ZWeight weight, ZVolume volume, string targetUnit)
		{
			var result = ZDecimal.Zero;

			var chargeableWeight = GetChargeableWeight(weight, volume);
			if (chargeableWeight.IsValid)
			{
				var chargeableAmount = Constants.Weight.Convert(chargeableWeight.Amount, chargeableWeight.Unit, targetUnit, false);

				return Utilities.Round(chargeableAmount, RoundingFactorFromRegistry.RoundingType, RoundingFactorFromRegistry.RoundingFactor);
			}

			return result;
		}

		DefaultRoundings RoundingFactorFromRegistry => DataRegistryRating.Instance.DefaultRounding.GetDefaultRounding(RatingConstants.RateCategory.TBC);

		[ReadOnlyMember(nameof(IsKM_ChargeableReadOnly))]
		public ZDecimal KM_Calc_RoundedChargeable {
			get
			{
				if (KM_OverrideChargeable)
				{
					return Utilities.Round(base.KM_Chargeable, RoundingFactorFromRegistry.RoundingType, RoundingFactorFromRegistry.RoundingFactor);
				}
				else
				{
					return CalculateChargeableVolumeWeightRoundedFromRegistrySettings(TotalWeight, TotalVolume, ChargeableUnits);
				}
			}
			set
			{
				base.KM_Chargeable = value;
			}
		}

		ZWeight GetChargeableWeight(ZWeight weight, ZVolume volume)
		{
			var volumeWeight = ZWeight.Empty;
			if (volume.IsValid)
			{
				var showChargeableUnitInMetric = !Constants.Weight.IsImperial(TotalWeight.Unit) || !Constants.Volume.IsImperial(TotalVolume.Unit);

				var factor = showChargeableUnitInMetric
					? ChargeableFactorFromRegistry.MetricFactor
					: ChargeableFactorFromRegistry.ImperialFactor;

				var volumeConvertedToWeight = factor.Convert(volume);
				if (volumeConvertedToWeight != null && !volumeConvertedToWeight.IsEmpty)
				{
					volumeWeight = (ZWeight)volumeConvertedToWeight;
				}
			}

			return weight.IsValid && volumeWeight > weight ? volumeWeight : weight;
		}

		[ResourceStringData("DtbBooking|CreatedByUserName", Caption = "Created By Full Name")]
		public ZString CreatedByUserName
		{
			get
			{
				var user = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, KM_SystemCreateUser);
				return user != null ? user.GS_FullName : ZString.Empty;
			}
		}

		public ZWeight TotalWeight
		{
			get { return GetTotalLoosePackageWeight() + GetTotalContainerisedWeight(); }
		}

		public ZWeight TotalWeightExcludingDuplicatePackages
		{
			get
			{
				var containerisedPackageIDs = new List<ZGuid>();

				foreach (var container in Containers)
				{
					foreach (var package in container.Package.Packages)
					{
						containerisedPackageIDs.Add(package.PK);
					}
				}

				var loosePackageWeight = GetTotalWeight(LoosePackages.Where(p => !containerisedPackageIDs.Contains(p.Package.PK)));
				return loosePackageWeight + GetTotalContainerisedWeight();
			}
		}

		public ZVolume TotalVolume
		{
			get { return GetTotalLoosePackageVolume() + GetTotalContainerisedVolume(); }
		}

		public ZVolume TotalVolumeExcludingDuplicatePackages
		{
			get
			{
				var containerisedPackageIDs = new List<ZGuid>();

				foreach (var container in Containers)
				{
					foreach (var package in container.Package.Packages)
					{
						containerisedPackageIDs.Add(package.PK);
					}
				}

				var loosePackageVolume = GetTotalVolume(LoosePackages.Where(p => !containerisedPackageIDs.Contains(p.Package.PK)));
				return loosePackageVolume + GetTotalContainerisedVolume();
			}
		}

		[ResourceStringData("DtbBooking|ParentID", Caption = "Parent ID")]
		public ZString ParentID
		{
			get
			{
				return ConsolidationSingleJob != null && ConsolidationSingleJob.Parent != null ? ConsolidationSingleJob.Parent.JobNumber :
				FindAdditionalReference(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference);
			}
		}

		ZString FindAdditionalReference(string additionalReferenceCode)
		{
			return AdditionalReferenceNumbers.GetAllReferenceNumbersByType(additionalReferenceCode).FirstOrDefault();
		}

		[ResourceStringData("DtbBooking|WayBillNumber", ShortCaption = "Waybill #", MediumCaption = "Waybill #", Caption = "Waybill Number")]
		public ZString WayBillNumber
		{
			get
			{
				return FirstNonEmptyString(FindAdditionalReferenceFromConsolidationSingleJob(TransportCommonAdditionalReferenceTypes.Codes.HouseBill), FindAdditionalReferenceFromConsolidationSingleJob(TransportCommonAdditionalReferenceTypes.Codes.MasterBill));
			}
		}

		ZString FirstNonEmptyString(params ZString[] strings)
		{
			return strings.FirstOrDefault(s => !s.IsEmpty);
		}

		ZString FindAdditionalReferenceFromConsolidationSingleJob(string additionalReferenceCode)
		{
			return ConsolidationSingleJob != null ? ConsolidationSingleJob.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(additionalReferenceCode).FirstOrDefault() : ZString.Empty;
		}

		[ResourceStringData("DtbBooking|Origin", ShortCaption = "Origin", MediumCaption = "Origin", Caption = "Origin")]
		public ZString Origin
		{
			get
			{
				return ParentView?.VP_RL_NKOrigin ?? ZString.Empty;
			}
		}

		[ResourceStringData("DtbBooking|Destination", ShortCaption = "Destination", MediumCaption = "Destination", Caption = "Destination")]
		public ZString Destination
		{
			get
			{
				return ParentView?.VP_RL_NKDestination ?? ZString.Empty;
			}
		}

		public DtbBookingInstruction FirstPickup
		{
			get
			{
				DtbBookingInstruction firstPickup = null;

				foreach (DtbBookingInstruction instruction in Instructions.PickUpInstructions)
				{
					if (firstPickup == null || instruction.KN_Sequence < firstPickup.KN_Sequence)
					{
						firstPickup = instruction;
					}
				}

				return firstPickup;
			}
		}

		public DtbBookingInstruction FirstConsignor
		{
			get
			{
				return Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CNR).OrderBy(i => i.KN_Sequence).FirstOrDefault();
			}
		}

		public DtbBookingInstruction FirstConsignee
		{
			get
			{
				return Instructions.Where(i => i.OrganisationType == OrganisationTypesList.Codes.CNE).OrderBy(i => i.KN_Sequence).FirstOrDefault();
			}
		}

		public DtbBookingInstruction LastDelivery
		{
			get
			{
				DtbBookingInstruction lastDelivery = null;

				foreach (DtbBookingInstruction instruction in Instructions.DeliveryInstructions)
				{
					if (lastDelivery == null || instruction.KN_Sequence > lastDelivery.KN_Sequence)
					{
						lastDelivery = instruction;
					}
				}

				return lastDelivery;
			}
		}

		[ResourceStringData("DtbBooking|TotalPickups", Caption = "Total Pickups")]
		public ZInt TotalPickups
		{
			get { return Instructions.PickUpInstructions.Count(); }
		}

		[ResourceStringData("DtbBooking|TotalDeliveries", Caption = "Total Deliveries")]
		public ZInt TotalDeliveries
		{
			get { return Instructions.DeliveryInstructions.Count(); }
		}

		public IEnumerable<DtbBookingConfirmation> PickupConfirmations
		{
			get
			{
				return Instructions.Where(i => i.IsPickUp).SelectMany(i => i.Confirmations).Where(c => c.IsPickUp);
			}
		}

		public IEnumerable<DtbBookingConfirmation> DeliveryConfirmations
		{
			get
			{
				return Instructions.Where(i => i.IsDelivery).SelectMany(i => i.Confirmations).Where(c => c.IsDelivery);
			}
		}

		public ZDateTime LatestPickedUpConfirmationDate
		{
			get
			{
				DtbBookingConfirmation latestConfirm;
				if (IsDeliveryDirection)
				{
					latestConfirm = LatestConfirmation(Instructions.PickUpInstructionsExcludeMultis, false);
				}
				else
				{
					latestConfirm = LatestConfirmation(Instructions.PickUpInstructions, false);
				}
				return latestConfirm != null ? latestConfirm.KK_Actual : ZDateTime.Empty;
			}
		}

		public ZDateTime LatestDeliveryNonDehireConfirmationDate
		{
			get
			{
				var latestConfirm = LatestConfirmation(Instructions.DeliveryInstructions_ExcludeEmpties, true);
				return latestConfirm != null ? latestConfirm.KK_Actual : ZDateTime.Empty;
			}
		}

		public ZDateTime LatestDeliveryConfirmationDate
		{
			get
			{
				var latestConfirm = LatestConfirmation(Instructions.DeliveryInstructions, true);
				return latestConfirm != null ? latestConfirm.KK_Actual : ZDateTime.Empty;
			}
		}

		public DtbBookingConfirmation LatestDeliveryNonDehireWithDateAndSignedByConfirmation
		{
			get { return LatestConfirmation(Instructions.DeliveryInstructions_ExcludeEmpties, true, true); }
		}

		public ZDateTime LatestEmptyReturnedConfirmationDate
		{
			get
			{
				var latestConfirm = LatestConfirmation(Instructions.DeliveryInstructions_EmptiesOnly, true);
				return latestConfirm != null ? latestConfirm.KK_Actual : ZDateTime.Empty;
			}
		}

		DtbBookingConfirmation LatestConfirmation(IEnumerable<DtbBookingInstruction> instructions, bool useDeliveryConfirmations, bool receivedByRequired = false)
		{
			DtbBookingConfirmation result = null;
			var latest = ZDateTime.Empty;

			foreach (var instruction in instructions)
			{
				var filteredConfirmations = (useDeliveryConfirmations) ? instruction.Confirmations.Deliveries : instruction.Confirmations.PickUps;
				foreach (var confirmation in filteredConfirmations)
				{
					if (confirmation.KK_Actual.IsValid &&
						(latest.IsEmpty || confirmation.KK_Actual > latest) &&
						(!receivedByRequired || !confirmation.KK_ReceivedBy.IsEmpty))
					{
						latest = confirmation.KK_Actual;
						result = confirmation;
					}
				}
			}

			return result;
		}

		ZString ChargeCostReference
		{
			get { return KM_TransportReference.IsEmpty ? KM_JobID : KM_TransportReference; }
		}

		public ZString SplitBookingDescription
		{
			get
			{
				if (ConsolidationSingleJob == null)
				{
					return ZString.Empty;
				}

				if (RefreshSplitBookingDescriptionHandler == null)
				{
					RefreshSplitBookingDescriptionHandler = delegate
					{ SplitBookingDescriptionInfo.RefreshBinding(); };
					ConsolidationSingleJob.Bookings.CountChanged += RefreshSplitBookingDescriptionHandler;
				}
				return Res.GetString("DtbBooking|OneOfSomeNumberOfBookings", "1 of {0}", ConsolidationSingleJob.ActiveBookings.Count);
			}
		}

		public ZPropertyInfo<ZString> SplitBookingDescriptionInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(nameof(SplitBookingDescription)); }
		}

		EventHandler RefreshSplitBookingDescriptionHandler;

		[ResourceStringData("66b77a6c-499b-4861-918b-1aca892e8fb9", ShortCaption = "Cus. Cl.", Caption = "Customs Cleared")]
		public ZBool IsCustomsCleared
		{
			get
			{
				var result = false;
				var parentJob = ParentJob;
				if (parentJob != null)
				{
					var parentWithWorkflow = parentJob.ParentWithWorkflow;
					if (parentWithWorkflow != null)
					{
						result = parentWithWorkflow.GetLogs().Find(l => l.SL_SE_NKEvent == Events.CustomsClearedCode && l.SL_EventTime.IsValid && !l.IsCancelled && !l.SL_IsEstimate).Any();
					}
				}
				return result;
			}
		}

		public ITransport FirstRoutingLeg => RoutingLegsOrderHelper.FirstLeg;

		public ITransport LastRoutingLeg => RoutingLegsOrderHelper.LastLeg;

		IRoutingLegsOrderHelper RoutingLegsOrderHelper
		{
			get
			{
				if (routingLegsOrderHelper == null)
				{
					var routingParent = ConsolidationSingleJob.IsParentSupportsRouting ? ConsolidationSingleJob.ParentBO : ConsolidationSingleJob;
					routingLegsOrderHelper = ObjectFactory.Get<IRoutingLegsOrderHelper>();
					routingLegsOrderHelper.InitializeFrom(routingParent);
				}

				return routingLegsOrderHelper;
			}
		}

		IRoutingLegsOrderHelper routingLegsOrderHelper;

		public IEnumerable<ZInt> ContainerLinks
		{
			get
			{
				return containerLinks;
			}
		}
		ImmutableArray<ZInt> containerLinks = ImmutableArray<ZInt>.Empty;

		public IEnumerable<ZString> ContainerNumbers
		{
			get
			{
				return containerNumbers;
			}
		}
		ImmutableArray<ZString> containerNumbers = ImmutableArray<ZString>.Empty;

		public void SetContainerLinksAndNumbers(IEnumerable<Container> containerList)
		{
			containerLinks = containerList?.Where(c => c.Link.HasValue).Select(c => c.Link.Value).ToImmutableArray() ?? ImmutableArray<ZInt>.Empty;
			containerNumbers = containerList?.Where(c => c.ContainerNumber.HasValue && c.ContainerNumber.Value != ZString.Empty).Select(c => c.ContainerNumber.Value).ToImmutableArray() ?? ImmutableArray<ZString>.Empty;
		}

		public ZInt TotalPackages
		{
			get
			{
				var totalPacks = 0;
				var packageIDsOnBooking = new List<ZGuid>();

				foreach (var container in Containers)
				{
					foreach (var package in container.Package.Packages)
					{
						totalPacks += package.KP_PackageQty;
						packageIDsOnBooking.Add(package.PK);
					}
				}
				foreach (var package in LoosePackages)
				{
					if (!packageIDsOnBooking.Contains(package.Package.PK))
					{
						totalPacks += package.Package.KP_PackageQty;
					}
				}

				return totalPacks;
			}
		}

		public ZString TotalPackagesType
		{
			get
			{
				var packageTypesOnBooking = new List<string>();

				foreach (var container in Containers)
				{
					foreach (var package in container.Package.Packages)
					{
						packageTypesOnBooking.Add(package.KP_F3_NKPackType);
					}
				}
				foreach (var package in LoosePackages)
				{
					packageTypesOnBooking.Add(package.Package.KP_F3_NKPackType);
				}

				if (!(packageTypesOnBooking.Count > 0) || packageTypesOnBooking.Any(o => o != packageTypesOnBooking[0]))
				{
					return Constants.PkgUnit.Package;
				}
				else
				{
					return packageTypesOnBooking[0];
				}
			}
		}

		[ResourceStringData("DtbBooking|StatusDescription", Caption = "Status")]
		public ZString StatusDescription
		{
			get { return Lookups.BindToLists.Statuses.GetDescriptionFromCode(KM_Status); }
		}

		public ZPropertyInfo<ZString> StatusDescriptionInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(nameof(StatusDescription)); }
		}

		public ZString TransportBookingPartyReference
		{
			get
			{
				var externalTB = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
				return externalTB != null ? externalTB.CE_EntryNum : ZString.Empty;
			}
		}

		/// <summary>
		/// Used to default Client on the billing tab, or dispay in the Module Grid
		/// </summary>
		public ZGuid BookedByOrganisationPK
		{
			get
			{
				var consolidation = ConsolidationSingleJob;
				return consolidation != null ? consolidation.BookedByOrganisationPK : ZGuid.Empty;
			}
		}

		[List("Lookups.BindToLists.AllOrganisations")]
		[ResourceStringData("DtbBooking|LocalClient", Caption = "Client")]
		public ZGuid LocalClient
		{
			get
			{
				var result = ZGuid.Empty;

				// first try to get the address from the billing job
				var job = Job;
				if (job != null)
				{
					var localChargesAddress = job.LocalChargesAddr;
					if (localChargesAddress != null)
					{
						result = job.LocalChargesAddr.OA_OH;
					}
				}

				// fallback to the ClientReqBillToParty address on the Booking
				if (!result.IsValid)
				{
					var clientReqBillToParty = DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty);
					if (clientReqBillToParty != null && !clientReqBillToParty.E2_AddressOverride)
					{
						var org = clientReqBillToParty.Organisation;
						if (org != null)
						{
							result = org.PK;
						}
					}
				}

				return result;
			}
		}

		// rating

		public ZInt GetTotalLoosePackageQuantity()
		{
			return LoosePackages.Sum(p => p.QuantityFromInstructions);
		}

		public ZString GetTotalLoosePackageUnit()
		{
			var result = ZString.Empty;

			foreach (DtbBookingPackage_PackageView package in LoosePackages)
			{
				if (result.IsEmpty)
				{
					result = package.Package.KP_F3_NKPackType;
				}
				else if (result != package.Package.KP_F3_NKPackType)
				{
					result = Constants.PkgUnit.Package;
				}
			}

			return result;
		}

		public ZWeight GetTotalLoosePackageWeight()
		{
			return GetTotalWeight(LoosePackages);
		}

		public ZWeight GetTotalContainerisedWeight()
		{
			return GetTotalWeight(Containers);
		}

		ZWeight GetTotalWeight(IEnumerable<DtbBookingPackage_PackageView> packageViews)
		{
			var result = new ZWeight(0, DtbTransportTotalsHelper.TotalWeightUnit);

			foreach (var weight in packageViews.Select(p => p.WeightFromInstructions).Where(w => w.IsValid))
			{
				result += weight;
			}

			return result;
		}

		public ZVolume GetTotalLoosePackageVolume()
		{
			return GetTotalVolume(LoosePackages);
		}

		public ZVolume GetTotalContainerisedVolume()
		{
			return GetTotalVolume(Containers);
		}

		ZVolume GetTotalVolume(IEnumerable<DtbBookingPackage_PackageView> packageViews)
		{
			var result = new ZVolume(0, DtbTransportTotalsHelper.TotalVolumeUnit);

			foreach (var volume in packageViews.Select(p => p.VolumeFromInstructions).Where(v => v.IsValid))
			{
				result += volume;
			}

			return result;
		}

		public new DtbBookingLookups Lookups
		{
			get { return (DtbBookingLookups)base.Lookups; }
		}

		protected override Common.DtbBookingLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		DtbBookingLookups GetNewLookupsCore()
		{
			return new DtbBookingLookups(this);
		}

		public new DtbBookingValidation Validation
		{
			get { return (DtbBookingValidation)base.Validation; }
		}

		protected override Common.DtbBookingValidation GetNewValidation()
		{
#if DEBUG
			if (this.ValidationForTest != null)
			{
				return this.ValidationForTest;
			}
			return new DtbBookingValidation(this);
#else
			return new DtbBookingValidation(this);
#endif
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return KM_JobID.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, KM_JobID);
			}
		}

		ZString HumanReadableNameWithoutID
		{
			get
			{
				if (KM_IsMaster)
				{
					if (ConsolidationSingleJob.KB_JobDirection == nameof(DtbBookingDirection.PIC))
					{
						return Res.GetString("ee5937b8-4c0e-4aaf-bced-77cdb2da5869", "Pickup Master Transport Booking");
					}
					else if (ConsolidationSingleJob.KB_JobDirection == nameof(DtbBookingDirection.DLV))
					{
						return Res.GetString("1e96bc6b-e60d-479f-bb68-887901744602", "Delivery Master Transport Booking");
					}
					return Res.GetString("31d288a0-293f-41ba-af4e-29e631d65459", "Master Transport Booking");
				}
				else
				{
					return Res.GetString("acfd4294-9158-44d7-a7cb-59c0185fa29e", "Transport Booking");
				}
			}
		}

		// overrides

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return IsAutoLogged && (IsTopLevel || DtbChildEditableService.GetState(Factory) != DtbChildEditableServiceState.None); }
		}

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges
		{
			get { return IsTopLevel || DtbChildEditableService.GetState(Factory) != DtbChildEditableServiceState.None; }
		}

		public override bool ReadOnly
		{
			// view mode is tested once in DtbBookingConsolidationTest.TestReadOnlyForChildren()
			get { return base.ReadOnly || IsInactiveOrConsigned; }
			set
			{
				var originalReadOnly = ReadOnly;
				base.ReadOnly = value;

				if (originalReadOnly != ReadOnly && ReadOnlyChanged != null)
				{
					ReadOnlyChanged(this, new EventArgs());
				}
			}
		}

		public event EventHandler ReadOnlyChanged;

		public ZBool IsQuote
		{
			get { return KM_Status == TransportStatuses.Codes.Quote; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (NotificationBufferForSendingXUSToCTO.Events.HasErrors() || NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors())
			{
				KM_Status = TransportStatuses.Codes.ActionRequired;
			}
			else if (KM_Status == TransportStatuses.Codes.ActionRequired)
			{
				KM_Status = TransportStatuses.Codes.Available;
			}
		}

		// status

		public bool IsAvailable
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.Available); }
		}

		public bool IsDelivered
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered); }
		}

		public bool IsDeliveredEmptyNotReturned
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.DeliveredEmptyNotReturned); }
		}

		public bool IsHeld
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.Held); }
		}

		public bool IsPickedUp
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickedUp); }
		}

		public bool IsPickupCommenced
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickUpCommenced); }
		}

		public bool IsPickUpConfirmed
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickUpConfirmed); }
		}

		public bool IsServiceCommenced
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.ServiceCommenced); }
		}

		public bool IsActionRequired
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.ActionRequired); }
		}

		public bool IsSub
		{
			get { return KM_KM_MasterBooking != ZGuid.Empty; }
		}

		// considering status

		/// <summary>
		/// if Booking is Pickup/Origin/Export - All Delivered
		/// if Booking is Delivery/Import/Local - All Delivered
		/// </summary>
		public bool IsConsideredDelivered
		{
			get
			{
				var deliveryInstructions = Instructions.DeliveryInstructions;
				return deliveryInstructions.Any() && deliveryInstructions.All(i => i.IsCompleteDeliveryOnly);
			}
		}

		/// <summary>
		/// if Booking is Pickup/Origin/Export - All Delivered Exclude Empties
		/// if Booking is Delivery/Import/Local - All Delivered Exclude Empties
		/// </summary>
		public bool IsConsideredDelivered_EmptiesNotReturned
		{
			get
			{
				bool result = false;

				var nonEmpties = Instructions.DeliveryInstructions_ExcludeEmpties;
				var allNonEmptyInstructionsComplete = nonEmpties.Any() && nonEmpties.All(i => i.IsCompleteDeliveryOnly);
				if (allNonEmptyInstructionsComplete)
				{
					var emptiesOnly = Instructions.DeliveryInstructions_EmptiesOnly;
					var hasEmptiesButNotAllComplete = emptiesOnly.Any() && !emptiesOnly.All(i => i.IsCompleteDeliveryOnly);
					result = hasEmptiesButNotAllComplete;
				}

				return result;
			}
		}

		/// <summary>
		/// if Booking is delivery direction (Destination/Import) - All instructions of type Pickup are Complete (excluding instructions of type Multi)
		/// if Booking is not delivery direction (Origin/Export/Local/Linehaul) - All pickup instructions (Pickup or Multi) are Complete
		/// </summary>
		public bool IsConsideredPickedUp
		{
			get
			{
				bool result;

				IEnumerable<DtbBookingInstruction> pickUpInstructions;
				if (IsDeliveryDirection)
				{
					pickUpInstructions = Instructions.PickUpInstructionsExcludeMultis;
				}
				else
				{
					pickUpInstructions = Instructions.PickUpInstructions;
				}
				result = pickUpInstructions.Any() && pickUpInstructions.All(i => i.IsComplete);

				return result;
			}
		}

		public bool IsConsideredAvailable
		{
			get
			{
				var hasTransportJobs = (GetPortTransportJobs().Length + GetLandTransportJobs().Length) > 0;
				var hasNoActiveTransportJobs = (GetPortTransportJobs().Where(pt => !pt.JJ_IsCancelled).Count() + GetLandTransportJobs().Where(ltc => ltc.LTC_IsActive).Count()) == 0;
				return IsServiceCommenced && !BookingIsBeingManagedByAuthorisedCarrierBookingAgent && !HasServiceCommencedLogs && hasTransportJobs && hasNoActiveTransportJobs;
			}
		}

		public bool IsConsideredServiceCommenced
		{
			get
			{
				return IsAvailable && ((GetPortTransportJobs().Where(pt => !pt.JJ_IsCancelled).Count() + GetLandTransportJobs().Where(ltc => ltc.LTC_IsActive).Count()) == 1);
			}
		}

		public bool HasEmptyDehireInstructions
		{
			get { return Instructions.DeliveryInstructions_EmptiesOnly.Any(); }
		}

		// other

		public bool IsSendingXUSToCTO
		{
			get { return IsValidatingSendingXUSToCTO || IsActionRequired; }
		}

		public NotificationBuffer NotificationBufferForSendingXUSToCTO
		{
			get
			{
				if (notificationBufferForSendingXUSToCTO == null)
				{
					notificationBufferForSendingXUSToCTO = new NotificationBuffer();
				}

				return notificationBufferForSendingXUSToCTO;
			}
			set
			{
				notificationBufferForSendingXUSToCTO = value;
			}
		}

		NotificationBuffer notificationBufferForSendingXUSToCTO;

		public bool IsValidatingSendingXUSToCTO { get; set; }

		public bool IsOnMultiJobConsolidation
		{
			get
			{
				var consolidation = ConsolidationMultiJob;
				return consolidation != null && consolidation.IsMultiBooking;
			}
		}

		public bool HasPackages
		{
			get { return Instructions.Any(i => i.DivotsWithPackages.Typed.Any()); }
		}

		public bool HasConfirmations
		{
			get { return Instructions.Any(i => i.Confirmations.Any()); }
		}

		bool IsConsolidationReadOnly
		{
			get { return ConsolidationSingleJob != null && ConsolidationSingleJob.ReadOnly; }
		}

		bool IsConsolidationBookingsReadOnly
		{
			get { return ConsolidationSingleJob != null && ConsolidationSingleJob.IsBookingsReadOnly; }
		}

		bool IsInactiveOrConsigned
		{
			get
			{
				return ConsolidationViewModeService.GetViewMode(Factory) == ConsolidationViewMode.MultiJob
					|| !KM_IsActive
					|| ConsignmentConsol != null;
			}
		}

		public bool IsPickupDirection
		{
			get { return KM_Direction == Constants.CartageDirection.Origin || KM_Direction == Constants.CartageDirection.Export; }
		}

		public bool IsDeliveryDirection
		{
			get { return KM_Direction == Constants.CartageDirection.Destination || KM_Direction == Constants.CartageDirection.Import; }
		}

		public bool IsLocalDirection
		{
			get { return KM_Direction == Constants.CartageDirection.Local; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public bool IsAnyPackageHazardous
		{
			get { return AssignedPackages.Any(p => IsPackageOrChildrenHazardous(p)); }
		}

		bool IsPackageOrChildrenHazardous(PkgPackage package)
		{
			return package.UNDGs.Any() || package.Packages.Any(p => IsPackageOrChildrenHazardous(p));
		}

		public bool IsAnyPackageRequiresRefridgeration
		{
			get { return AssignedPackages.Any(p => IsPackageOrChildrenRequiresRefridgeration(p)); }
		}

		bool IsPackageOrChildrenRequiresRefridgeration(PkgPackage package)
		{
			return package.IsTemperatureControlled || package.Packages.Any(p => IsPackageOrChildrenRequiresRefridgeration(p));
		}

		public void UpdateIsHazardous()
		{
			KM_IsHazardous = IsAnyPackageHazardous;
		}

		public void UpdateRequiresRefrigeration()
		{
			KM_RequiresRefrigeration = IsAnyPackageRequiresRefridgeration;
		}

		// rating

		public ZBool IsContainerisedOnly
		{
			get { return IsContainerised && !IsLoose; }
		}

		public ZBool IsLooseOnly
		{
			get { return IsLoose && !IsContainerised; }
		}

		public ZBool IsContainerised
		{
			get { return Containers.Any(); }
		}

		public ZBool IsLoose
		{
			get { return LoosePackages.Any(); }
		}

		public ZBool IsFCL
		{
			get { return IsContainerised && !IsFTL; }
		}

		public ZBool IsFTL
		{
			get { return Containers.Any(c => c.Package.Container.K0_ContainerMode == Constants.ContainerModes.FTL || (c.Package.Container.ContainerType != null && c.Package.Container.ContainerType.IsRoadTruckContainer)); }
		}

		public ZBool IsFCLPort
		{
			get { return IsFCL && IsPort; }
		}

		public ZBool IsPort
		{
			get { return Instructions.Cast<DtbBookingInstruction>().Any(i => i.IsCTO || i.IsCYD); }
		}

		public ZBool IsMixedCargo
		{
			get { return (IsLoose && IsContainerised) || (IsFTL && Containers.Any(c => c.Package.Container.K0_ContainerMode != Constants.ContainerModes.FTL && c.Package.Container.K0_ContainerMode != Constants.ContainerModes.FCL)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			KM_Status = TransportStatuses.Codes.Available;
			KM_GB_Branch = GlbBranch.CurrentBranch.PK;
			var defaultTransportCompanyPk = TransportRegistry.Instance.TransportBookingDefaultTransportCompany.Value;
			if (defaultTransportCompanyPk != Guid.Empty)
			{
				Address.OrganisationPK = defaultTransportCompanyPk;
			}
			KM_JobType = TransportConsolidationJobTypes.Codes.Booking;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingFetchStrategy(this);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			UpdateReadOnlyForWhenIsActive();

			if (KM_IsMaster)
			{
				ValidateMasterAndSubInSync();
			}

			if (IsHeld)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		void AddBusinessObjectsWithRelatedNotes(List<BusinessObject> bizOs)
		{
			bizOs.Add(ConsolidationSingleJob);
		}

		void ValidateMasterAndSubInSync()
		{
			foreach (var sub in SubBookings)
			{
				sub.Validation.ValidateForSync();
			}
		}

		public override void OnSaving()
		{
			PopulateUniqueIDIfNeeded();
			UpdateMasterBookingVersionAndConsolidationParentMasterFieldsIfAttachedOrNew();
			MasterBookingHelper.UpdateMasterBookingVersion();
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				DeactivateJobHeader();
			}

			if (KM_IsMaster)
			{
				ValidateMasterAndSubInSync();
			}

			AddStatusEventIfRequired();
			AddAttachEventIfRequired();
			AddBookingRequestedEventIfRequired();
			UpdateCO2eParentStatusIfRequired();
			base.OnSaving();
		}

		internal void PopulateUniqueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted && KM_JobID.IsEmpty)
			{
				KM_JobID = GenerateID();
			}
		}

		void UpdateMasterBookingVersionAndConsolidationParentMasterFieldsIfAttachedOrNew()
		{
			if (IsInDatabase)
			{
				if (KM_KM_MasterBooking != ZGuid.Empty && (ZGuid)KM_KM_MasterBookingInfo.OriginalValue == ZGuid.Empty)
				{
					KM_MasterBookingVersion = MasterBooking.KM_MasterBookingVersion == (short)1 ? short.MaxValue : MasterBooking.KM_MasterBookingVersion - 1;
					if (ConsolidationSingleJob != null)
					{
						ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = MasterBooking.ConsolidationSingleJob.PK;
						ConsolidationSingleJob.KB_MasterBookingVersion = MasterBooking.ConsolidationSingleJob.KB_MasterBookingVersion == (short)1 ? short.MaxValue : MasterBooking.ConsolidationSingleJob.KB_MasterBookingVersion - 1;
					}
					DeleteInstructionsAndDocAddressesForSubBooking();

					UpdateAuditColumnsForMasterBusinessObjects();
				}
				else if (KM_KM_MasterBooking == ZGuid.Empty && (ZGuid)KM_KM_MasterBookingInfo.OriginalValue != ZGuid.Empty)
				{
					KM_MasterBookingVersion = (short)0;
					DocAddresses.RemoveAndDeleteAll();
					var previousMasterBooking = Factory.Load<DtbBooking>((ZGuid)KM_KM_MasterBookingInfo.OriginalValue);
					foreach (var docAddress in previousMasterBooking.DocAddresses)
					{
						DocAddresses.Add(docAddress.Clone());
					}
					if (ConsolidationSingleJob != null && ConsolidationSingleJob.Bookings.All(booking => !booking.IsSub))
					{
						var masterConsolidation = ConsolidationSingleJob.MasterBookingConsolidation;
						ConsolidationSingleJob.KB_KB_MasterBookingConsolidation = ZGuid.Empty;
						ConsolidationSingleJob.KB_MasterBookingVersion = (short)0;
						ConsolidationSingleJob.DocAddresses.RemoveAndDeleteAll();
						foreach (var docAddress in masterConsolidation.DocAddresses)
						{
							ConsolidationSingleJob.DocAddresses.Add(docAddress.Clone());
						}
					}
				}
			}

			if (!IsInDatabase && !IsDeleted)
			{
				if (KM_KM_MasterBooking != ZGuid.Empty)
				{
					if (KM_MasterBookingVersion == ZShort.Zero)
					{
						KM_MasterBookingVersion = (short)1;
					}
				}
				else
				{
					var initialMasterBookingVersion = (short)(KM_IsMaster ? 1 : 0);
					KM_MasterBookingVersion = initialMasterBookingVersion;
					if (ConsolidationSingleJob != null)
					{
						ConsolidationSingleJob.KB_IsMaster = KM_IsMaster;
						ConsolidationSingleJob.KB_MasterBookingVersion = initialMasterBookingVersion;
						if (ConsolidationSingleJob.Bookings.Any(b => b.KM_IsMaster != KM_IsMaster))
						{
							var thisIsMaster = KM_IsMaster ? (NoResString)"true" : (NoResString)"false";
							var otherIsMaster = KM_IsMaster ? (NoResString)"false" : (NoResString)"true";
							ErrorReporter.ReportOnce("DtbBooking.OnSaving", $@"All bookings under a booking consolidation should have the same value of KM_IsMaster, you are trying to save booking '{KM_JobID}' with master flag value '{thisIsMaster}' where consolidation parent has other booking(s) with master flag value '{otherIsMaster}'");
						}
					}
				}
			}
		}

		void DeleteInstructionsAndDocAddressesForSubBooking()
		{
			DeleteAllInstructions();

			var docAddresses = new JobDocAddressDependentCollection(this);
			docAddresses.Load();
			docAddresses.RemoveAndDeleteAll();
		}

		public void DeleteAllInstructions()
		{
			Instructions.DeleteAllInstructions();
		}

		void UpdateAuditColumnsForMasterBusinessObjects()
		{
			var masterConsolidation = MasterBooking.ConsolidationSingleJob;
			masterConsolidation.KB_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			masterConsolidation.KB_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
			MasterBooking.KM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			MasterBooking.KM_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
			MasterBooking.Instructions.ForEach(instruction =>
			{
				instruction.KN_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				instruction.KN_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
				instruction.Confirmations.ForEach(confirmation =>
				{
					confirmation.KK_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					confirmation.KK_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
				});
			});
		}

		ZString GenerateID()
		{
			NumberGenerator.Generate();
			NumberGenerator.EnforceMaxLengths();
			UniqueIndexHandler = new UniqueIndexHandler(NumberGenerator.PrimaryTarget);

			return NumberGenerator.PrimaryTarget.Value;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				OnSaveFailed();
			}
		}

		void OnSaveFailed()
		{
			OnSaveFailedCore();

			if (!IsInDatabase)
			{
				KM_JobID = ZString.Empty; // do this last
			}
		}

		StmALog GetLatestLog(Event eventType)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, this.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " desc";

			return Factory.LoadTop1<StmALog>(query);
		}

		NumberGenerator NumberGenerator
		{
			get { return numberGenerator ?? (numberGenerator = GetNewNumberGenerator()); }
		}

		NumberGenerator numberGenerator;

		NumberGenerator GetNewNumberGenerator()
		{
			var generator = new NumberGenerator
			{
				Factory = Factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = NumberFountainForUniqueID,
				FountainGetter = Env.NumberFountains.GetDtbTransportGeneratorFountain,
				PrimaryTarget = new BookingNumberGeneratorTarget(this)
			};

			generator.ValueProviders.AddRange(new StandardValueSource().Concat(new DomesticValueSource(this)));
			return generator;
		}

		void AddStatusEventIfRequired()
		{
			var originalStatus = IsInDatabase ? KM_StatusInfo.OriginalValue.ToString() : TransportStatuses.Codes.Available;
			if (originalStatus != KM_Status)
			{
				StatusEventToDeleteIfSaveFailed = Logs.CreateRecreateOrUpdateEventLog(Events.StatusUpdated, EstimateActual.Actual, ZDateTimeOffset.Now, "",
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalStatus),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, KM_Status));
			}
		}

		StmALog StatusEventToDeleteIfSaveFailed;

		void AddAttachEventIfRequired()
		{
			if (ConsolidationSingleJob != null && (!IsInDatabase || (ZGuid)KM_KB_BookingInfo.OriginalValue != KM_KB_Booking))
			{
				AddBookingAttachedToParentEvent();
			}

			if (MasterBooking != null && ((ZGuid)KM_KM_MasterBookingInfo.OriginalValue == ZGuid.Empty))
			{
				AddBookingAttachedToMasterEvent();
			}
			else if (MasterBooking == null && ((ZGuid)KM_KM_MasterBookingInfo.OriginalValue != ZGuid.Empty))
			{
				AddBookingDetachedToMasterEvent();
			}
		}

		public void AddBookingAttachedToParentEvent()
		{
			var parent = ConsolidationSingleJob != null ? ConsolidationSingleJob.Parent : null;
			if (parent != null)
			{
				var eventType = parent.JobDescription;
				var parentID = parent.JobNumber;
				Logs.AddNew(Events.Attached, parentID, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventType));
			}
		}

		void AddBookingAttachedToMasterEvent()
		{
			var eventType = MasterBooking.HumanReadableNameWithoutID;
			var parentID = MasterBooking.KM_JobID;
			Logs.AddNew(Events.Attached, parentID, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventType));
		}

		void AddBookingDetachedToMasterEvent()
		{
			var masterBooking = Factory.Load<DtbBooking>((ZGuid)KM_KM_MasterBookingInfo.OriginalValue);
			var eventType = masterBooking.HumanReadableNameWithoutID;
			var parentID = masterBooking.KM_JobID;
			Logs.AddNew(Events.Detached, parentID, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, eventType));
		}

		void AddBookingRequestedEventIfRequired()
		{
			if (RequiredToAddBookingRequestedEvent())
			{
				Logs.CreateOrRecreateEventLog(
					Events.BookingRequested,
					EstimateActual.Actual,
					KM_BookingOfTransportRequestedDate.IsEmpty ? ZDateTimeOffset.Now : KM_BookingOfTransportRequestedDate.ToOffset(),
					KM_JobID,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, GetBookingRequestedEventTypeParameter()));
			}
		}

		internal string GetBookingRequestedEventTypeParameter()
		{
			string result;

			if (IsPickupDirection)
			{
				result = Constants.EventReferenceParameterTypes.PickupTransport;
			}
			else
			{
				result = IsDeliveryDirection ? Constants.EventReferenceParameterTypes.DeliveryTransport : Constants.EventReferenceParameterTypes.Transport;
			}

			return result;
		}

		internal bool RequiredToAddBookingRequestedEvent() => KM_BookingOfTransportRequestedDate.IsValid && !Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode);

		internal bool RequiredToAddBookingRequestedEventForNonShipmentParent() => !(ConsolidationSingleJob?.ParentBO is IForwardingShipment) && RequiredToAddBookingRequestedEvent();

		void UpdateCO2eParentStatusIfRequired()
		{
			if (!FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.Value)
			{
				return;
			}

			if (GetParentJobWithoutUsingLoader() is ICO2ePrePostCarriage parent && IsCancelledHasChanged)
			{
				parent.OnTransportBookingActiveStatusChanged(this);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (HasChanges)
			{
				CheckForStatusChange();
				UpdateParentJobIfRequired();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CheckForStatusChange()
		{
			var sqlText = string.Format(CultureInfo.CurrentCulture, @"SELECT {0}
												FROM {1}
												WITH (UPDLOCK, ROWLOCK)
												WHERE {2} = @PKValue",
							Schema.KM_Status, TableName, PKSchemaColumn.Name);
			var command = ((IDbConnected)Factory).Connection.Command(sqlText);
			command.AddParameterBasedOnDbColumn("@PKValue", PK.ToGuid(), PKSchemaColumn);
			var status = (string)command.ExecuteScalar();

			if (status == TransportStatuses.Codes.ServiceCommenced
				&& (ZString)KM_StatusInfo.OriginalValue != TransportStatuses.Codes.ServiceCommenced
				&& KM_Status != TransportStatuses.Codes.ServiceCommenced)
			{
				throw new ZCannotSaveException(ResString.GetMultilingualString("DtbBooking|CheckForStatusChange|CommencedMessage",
					@"Consignments have already been created for this Booking, no changes can be saved."),
					Res.GetString("DtbBooking|CheckForStatusChange|CommencedTitle", "{0} has Commenced", HumanReadableName));
			}
		}

		void UpdateParentJobIfRequired()
		{
			var parentJob = GetParentJobWithoutUsingLoader();
			if (parentJob != null && ShouldUpdateParentJobOnSaving(parentJob))
			{
				parentJob.TransportBookingCreatedOrUpdated(new[] { this });
			}
		}

		bool ShouldUpdateParentJobOnSaving(IDtbBookingParent parentJob)
		{
			return Instructions.Any(x => x.KN_StatusInfo.HasChanges) && parentJob.TablePrefix == HVLVBookingHeaderSchema.Constants.Prefix;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			// tested by WorkFlowTests
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);

			if (MarkedAsNeedingStatusCheck)
			{
				FireStatusEventsIfNeeded();
				MarkedAsNeedingStatusCheck = false;
			}
		}

		void OnSaveFailedCore()
		{
			if (StatusEventToDeleteIfSaveFailed != null && !StatusEventToDeleteIfSaveFailed.IsInDatabase)
			{
				StatusEventToDeleteIfSaveFailed.Delete();
				StatusEventToDeleteIfSaveFailed = null;
			}
		}

		public void MarkAsNeedingStatusCheck()
		{
			MarkedAsNeedingStatusCheck = true;

			var consolidationSingleJob = ConsolidationSingleJob;
			if (consolidationSingleJob != null)
			{
				consolidationSingleJob.MarkAsNeedingStatusCheck();
			}
		}

		bool MarkedAsNeedingStatusCheck;

		void FireStatusEventsIfNeeded()
		{
			FirePickupEventIfNeeded();
			FireDeliveredEmptyNotReturnedEventIfNeeded();
			FireEmptyYardGateInEventIfNeeded();
			FireDeliveredEventIfNeeded();
		}

		/// <summary>
		/// Fire Picked Up Event if Picked Up.
		/// </summary>
		void FirePickupEventIfNeeded()
		{
			if (IsPickedUp || IsDeliveredEmptyNotReturned || IsDelivered)
			{
				var latestPickedUpDate = LatestPickedUpConfirmationDate;
				if (latestPickedUpDate.IsValid)
				{
					AddLogIfNotExistsOrNotMatching(Events.PickupCartageCompleteFinalised, latestPickedUpDate.ToOffset());
				}
			}
		}

		/// <summary>
		/// Fire Delivered Event if Delivered excluding Empty Dehires.
		/// </summary>
		void FireDeliveredEmptyNotReturnedEventIfNeeded()
		{
			if (IsDeliveredEmptyNotReturned || IsDelivered)
			{
				var latestConfirmationWithDateAndSignedBy = LatestDeliveryNonDehireWithDateAndSignedByConfirmation;
				var latestDateOnBooking = (latestConfirmationWithDateAndSignedBy != null) ? latestConfirmationWithDateAndSignedBy.KK_Actual : LatestDeliveryNonDehireConfirmationDate;
				if (latestDateOnBooking.IsValid)
				{
					var latestSignedByOnBooking = (latestConfirmationWithDateAndSignedBy != null) ? latestConfirmationWithDateAndSignedBy.KK_ReceivedBy : ZString.Empty;
					AddLogIfNotExistsOrNotMatching(Events.DeliveryCartageCompleteFinalised, latestDateOnBooking.ToOffset(), latestSignedByOnBooking);
				}
			}
		}

		/// <summary>
		/// Fire Dehire Gate In Event if Booking has Empties and booking is Delivered.
		/// </summary>
		void FireEmptyYardGateInEventIfNeeded()
		{
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

					AddLogIfNotExistsOrNotMatching(AutoEvents.GateIn, latestEmptyReturnedDate.ToOffset(), string.Empty, parameters);
				}
			}
		}

		/// <summary>
		/// Fire Complete Event if everything is Delivered.
		/// </summary>
		void FireDeliveredEventIfNeeded()
		{
			if (IsDelivered)
			{
				var latestCompletionDate = LatestDeliveryConfirmationDate;
				if (latestCompletionDate.IsValid)
				{
					AddLogIfNotExistsOrNotMatching(Events.CartageCompleteFinalised, latestCompletionDate.ToOffset());
				}
			}
		}

		void AddLogIfNotExistsOrNotMatching(Event eventType, ZDateTimeOffset date, string referenceFreeText = "", params KeyValuePair<string, string>[] parameters)
		{
			var existingLog = GetLatestLog(eventType);
			var reference = StmALog.GenerateEventReference(referenceFreeText, parameters);

			if (existingLog == null || existingLog.SL_EventTimeOffset != date || existingLog.SL_Reference != reference)
			{
				Logs.AddNew(eventType, reference, date);
			}
		}

		public void SetInstructionViewAndSelectedPackage(TransportBookingInstructionView bookingInstructionView, DtbBookingPackage_PackageView selectedPackageFromPackageView)
		{
			this.instructionView = bookingInstructionView;
			this.selectedPackage_PackageView = selectedPackageFromPackageView;
		}

		public TransportBookingInstructionView InstructionView
		{
			get { return instructionView; }
		}

		public DtbBookingPackage_PackageView SelectedPackage_PackageView
		{
			get { return InstructionView == TransportBookingInstructionView.Package ? selectedPackage_PackageView : null; }
		}

		TransportBookingInstructionView instructionView = TransportBookingInstructionView.Instruction;
		DtbBookingPackage_PackageView selectedPackage_PackageView;

		bool HasABookingConfirmedEvent
		{
			get
			{
				var stmALog = this.GetLogs().MostRecentLogByEventTime(Events.BookingConfirmed);
				var sender = stmALog?.RelatedEDIMessage?.Sender;

				return !string.IsNullOrEmpty(sender)
					&& DtbAgentBooking.IsAuthorisedCarrierBookingAgent((ZString)sender)
					&& CarrierBookingAgentIsAuthorised;
			}
		}

		/// <summary>
		/// Only manage from Booking Form
		/// </summary>
		public void UpdateReadOnly_SingleBookingForm()
		{
			if (DtbChildEditableService.GetState(Factory) != DtbChildEditableServiceState.Transport)
			{
				throw new NotSupportedException("Should only be called from the Booking Form.");
			}

			var makeReadOnly = IsInactiveOrConsigned || IsConsolidationReadOnly || BookingIsBeingManagedByAuthorisedCarrierBookingAgent || IsSub || HasABookingConfirmedEvent;
			if (makeReadOnly)
			{
				var shouldBePartiallyLockedDown =  ShouldBePartiallyLockedDown;

				SetReadOnlyIncludingChildren(true);

				if (shouldBePartiallyLockedDown)
				{
					UpdateReadOnlyForPartialLockdown();
				}
				else
				{
					AdditionalReferencesForBinding.SetReadOnlyIncludingChildren(true);
				}
			}
			else
			{
				OnConsolidationIsOverrideChanged();
			}
		}

		/// <summary>
		/// Only manage from Multi-Booking Form
		/// </summary>
		public void UpdateReadOnly_MultiForm()
		{
			if (DtbChildEditableService.GetState(Factory) != DtbChildEditableServiceState.Consolidation)
			{
				throw new NotSupportedException("Should only be called from the Multi Booking Form.");
			}

			if (BookingIsBeingManagedByAuthorisedCarrierBookingAgent || IsSub || HasABookingConfirmedEvent)
			{
				SetReadOnlyIncludingChildren(true);

				if (ShouldBePartiallyLockedDown)
				{
					UpdateReadOnlyForPartialLockdown();
				}
			}
		}

		bool shouldBePartiallyLockedDown_OnSingleBookingForm;

		bool ShouldBePartiallyLockedDown
		{
			get
			{
				if (DtbChildEditableService.GetState(Factory) == DtbChildEditableServiceState.Transport)
				{
					if (!shouldBePartiallyLockedDown_OnSingleBookingForm)
					{
						shouldBePartiallyLockedDown_OnSingleBookingForm = (BookingIsBeingManagedByAuthorisedCarrierBookingAgent || IsSub) && !IsInactiveOrConsigned && !IsConsolidationReadOnly && !HasABookingConfirmedEvent;
					}

					return shouldBePartiallyLockedDown_OnSingleBookingForm;
				}

				if (DtbChildEditableService.GetState(Factory) == DtbChildEditableServiceState.Consolidation)
				{
					return (BookingIsBeingManagedByAuthorisedCarrierBookingAgent || IsSub) && !HasABookingConfirmedEvent;
				}

				return false;
			}
		}

		void UpdateReadOnlyForPartialLockdown()
		{
			WorkflowItems.SetReadOnlyIncludingChildren(false);
			WorkflowItems.MilestonesIncludingRelatedSortable.SetReadOnlyIncludingChildren(false);
			WorkflowItems.Tasks.TasksViewFilter?.SetReadOnlyIncludingChildren(false);
			WorkflowItems.TriggersIncludingRelated.SetReadOnlyIncludingChildren(false);
			WorkflowItems.ExceptionsIncludingRelated.SetReadOnlyIncludingChildren(false);

			Job?.SetReadOnlyIncludingChildren(false);
			CustomBusinessObject.SetReadOnlyIncludingChildren(false);
		}

		protected override void UpdateChildReadOnlyWhenRegistering(IBusiness child)
		{
			var childShouldBeReadOnlyWhenBookingIsPartiallyLockedDown =
				child is not TransportBookingAdditionalReferenceCollection &&
				child is not ICusEntryNumAdditionalReferenceCollection;

			if (child is not StmNoteCollection && (!ShouldBePartiallyLockedDown || (childShouldBeReadOnlyWhenBookingIsPartiallyLockedDown && ShouldBePartiallyLockedDown)))
			{
				base.UpdateChildReadOnlyWhenRegistering(child);
			}
		}

		/// <summary>
		/// Triggered by Consolidation.OnConsolidationIsOverrideChanged > Collection
		/// and Multi Booking Form Load
		/// When Consolidation Override is set false: make booking and its instructions, addresses and packs readonly. Leave Job Header editable.
		/// When Consolidation Override is set true: make booking and its instructions, addresses and packs editable. Job Header to stay editable.
		/// </summary>
		public void OnConsolidationIsOverrideChanged()
		{
			var readOnly = IsInactiveOrConsigned || IsConsolidationBookingsReadOnly || IsSub || HasABookingConfirmedEvent;

			ReadOnly = readOnly;
			Instructions.SetReadOnlyIncludingChildren(readOnly);
			DocAddresses.SetReadOnlyIncludingChildren(readOnly);
			Packages_PackageView.SetReadOnlyIncludingChildren(readOnly);
			CustomBusinessObject.SetReadOnlyIncludingChildren(readOnly);
		}

		public override void Delete()
		{
			if (KM_IsMaster && HasSubBookings())
			{
				return;
			}

			if (AllowBookingDeleteWithoutWarning() || AllowBookingDeleteWithWarning())
			{
				// tested by SaveAndDeleteBusinessObject()
				DeleteAllInstructions();
				DeactivateJobHeader();

				// tested by WorkFlowTests
				WorkflowItems.RemoveAndDeleteAll();

				// tested by SaveAndDeleteBusinessObject()
				DocAddresses.RemoveAndDeleteAll();
				ViewRelatedActivityPivot.DeleteAllPivots(this);

				base.Delete();
			}
		}

		bool AllowBookingDeleteWithoutWarning()
		{
			return !(HasPackages || HasConfirmations);
		}

		bool AllowBookingDeleteWithWarning()
		{
			var cancelDelete = new CancelEventArgs(false);
			if (CancelBookingDelete != null)
			{
				CancelBookingDelete(this, cancelDelete);
			}

			return !cancelDelete.Cancel;
		}

		public event EventHandler<CancelEventArgs> CancelBookingDelete;

		void DeactivateJobHeader()
		{
			var job = Job;
			if (job != null && job.CanDeactivate && job.JH_ParentID == this.PK)
			{
				job.MarkAsInactive();
			}
		}

		public override bool CanDelete
		{
			get { return !IsInDatabase; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("31e312a3-7e40-40c9-893a-b9a569b9d2d4", "Saved Bookings cannot be deleted, however you may Deactivate the Booking instead."); }
		}

		bool HasSubBookings()
		{
			var query = new ZQuery(DtbBookingSchema.KM_KM_MasterBooking, this.PK);
			var subs = new ActiveBusinessObjectCollection<DtbBooking>(Factory, query);

			return subs.Count > 0;
		}

		public void Activate()
		{
			KM_IsActive = true;
		}

		public void Deactivate(INotifications notify = null)
		{
			var messageCaption = Res.GetString("430ababe-f994-4f18-9e37-1f84b25fd282", "Warning");
			var messageText = Res.GetString("d3bf8e9e-0f8b-48b6-a6ff-22bc95413cff",
@"Deactivating this Booking will un-assign it's packages.
If you reactivate this Booking you will need to reassign the packages.

Do you wish to deactivate the Booking?");

			var args = new QueryUserYesNoEventArgs(messageCaption, messageText, false);
			if (notify != null)
			{
				notify.AddWarning(messageText);
				notify.QueryUser(args);
			}

			if (args.Response || notify == null)
			{
				DeletePackageDivotsFromInstructions();
				KM_IsActive = false;
			}
		}

		void DeletePackageDivotsFromInstructions()
		{
			foreach (var instruction in Instructions)
			{
				instruction.PackageDivots.DeleteAll();
			}
			KM_IsHazardous = false;
			KM_RequiresRefrigeration = false;
		}

		void UpdateReadOnlyForWhenIsActive()
		{
			if (!IsDeleted && IsInstructionsInstatiated)
			{
				Instructions?.SetReadOnlyIncludingChildren(!KM_IsActive);
			}
		}

		INumberFountainProxy NumberFountainForUniqueID
		{
			get { return Env.NumberFountains.DtbBookingID; }
		}

		[ResourceStringData("DtbBooking|AdditionalReferenceNumbersSingleLine", Caption = "References")]
		public ZString AdditionalReferenceNumbersSingleLine
		{
			get
			{
				return string.Join(", ", AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Select(c => FormatCusEntryNumber(c)));
			}
		}

		string FormatCusEntryNumber(ICusEntryNumber cusEntryNum)
		{
			var typeSeparator = cusEntryNum.CE_EntryNum.IsEmpty && cusEntryNum.CE_RN_NKCountryCode.IsEmpty
				? string.Empty
				: ": ";

			var numberSeparator = cusEntryNum.CE_RN_NKCountryCode.IsEmpty
				? string.Empty
				: "/";

			return Invariant($"{cusEntryNum.CE_EntryType}{typeSeparator}{cusEntryNum.CE_EntryNum}{numberSeparator}{cusEntryNum.CE_RN_NKCountryCode}");
		}

		public IDisposable SuspendSettingPackages()
		{
			return new DisposableAction(() => suspendSettingPackages++, () => suspendSettingPackages--);
		}

		public ZBool IsSettingPackagesSuspended
		{
			get { return suspendSettingPackages > 0; }
		}

		int suspendSettingPackages;

		public IDisposable SuspendSettingServiceLevel()
		{
			return new DisposableAction(() => suspendSettingServiceLevel++, () => suspendSettingServiceLevel--);
		}

		ZBool IsSettingServiceLevelSuspended
		{
			get { return suspendSettingServiceLevel > 0; }
		}

		int suspendSettingServiceLevel;

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DtbBookingFountainUniqueIndexFailureHandler(this)); }
		}

		class DtbBookingFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DtbBookingFountainUniqueIndexFailureHandler(DtbBooking booking)
				: base(DtbBookingSchema.Constants.Indexes.NR_UC__KM_JobID, booking)
			{
				this.booking = booking;
			}

			readonly DtbBooking booking;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get
				{
					return booking.UniqueIndexHandler != null
						? booking.UniqueIndexHandler.NumberFountain
						: booking.NumberFountainForUniqueID;
				}
			}

			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				return booking.UniqueIndexHandler != null
					? booking.UniqueIndexHandler.FindMaxValueInDatabase(connection, DtbBookingSchema.KM_JobID)
					: base.CommandToFindMaxValueInDatabase(connection);
			}
		}

		UniqueIndexHandler UniqueIndexHandler;

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		RefTransitTime GetServiceLevelTransitHours(ZGuid pickupZone, ZGuid deliveryZone)
		{
			RefTransitTime result = null;

			if (!pickupZone.IsEmpty && !deliveryZone.IsEmpty && !KM_RS_NKServiceLevel.IsEmpty)
			{
				var query = new ZQuery(RefTransitTimeSchema.RTT_RS_NKServiceLevel, KM_RS_NKServiceLevel);
				query.AddToFilter(RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, pickupZone);
				query.AddToFilter(RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, deliveryZone);

				result = Factory.LoadTop1<RefTransitTime>(query);
			}

			return result;
		}

		// interface members

		CodeDescriptionPairList AdditionalReferenceNumberTypes
		{
			get
			{
				return AdditionalReferenceHelper.GetAdditionalReferenceNumberTypeList();
			}
		}

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add
			{
				var parent = ParentCreditControlledDocumentDelivery;
				if (parent != null)
				{
					parent.GetDocumentLogin += value;
				}
			}
			remove
			{
				var parent = ParentCreditControlledDocumentDelivery;
				if (parent != null)
				{
					parent.GetDocumentLogin -= value;
				}
			}
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			ParentCreditControlledDocumentDelivery?.RaiseOnGetDocumentLogin(e);
		}

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback
		{
			get => ParentCreditControlledDocumentDelivery?.DocumentLoginMessageBoxCallback ?? LocalDocumentLoginMessageBoxCallback;
			set
			{
				var parent = ParentCreditControlledDocumentDelivery;
				if (parent != null)
				{
					parent.DocumentLoginMessageBoxCallback = value;
				}
			}
		}

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
			=> ParentCreditControlledDocumentDelivery?.IsDPSFreightMovementRestricted ?? false;

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted
			=> ParentCreditControlledDocumentDelivery?.IsAviationSecurityFreightMovementRestricted ?? false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return ParentCreditControlledDocumentDelivery?.GetScreeningParties() ?? Array.Empty<ScreeningParty>();
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
			=> ParentCreditControlledDocumentDelivery?.OrganisationsForCreditChecks ?? Array.Empty<OrgHeader>();

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
			=> ParentCreditControlledDocumentDelivery?.DescriptionOfOrganisationBeingCheckedForCredit ?? string.Empty;

		string[] IRelatedJobNumber.JobNumber
			=> ParentCreditControlledDocumentDelivery?.JobNumber ?? Array.Empty<string>();

		ICreditControlledDocumentDelivery ParentCreditControlledDocumentDelivery
			=> ParentJob?.ParentWithWorkflow as ICreditControlledDocumentDelivery;

		CustomMessageBoxCallback LocalDocumentLoginMessageBoxCallback { get; }

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		[ChildEditable]
		CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
					customBusinessObject.SetReadOnlyIncludingChildren(ReadOnly);

					RegisterEditableChildObject(customBusinessObject);

					var childEditableServiceState = DtbChildEditableService.GetState(Factory);
					if ((childEditableServiceState == DtbChildEditableServiceState.Transport && ShouldBePartiallyLockedDown)
						|| (childEditableServiceState == DtbChildEditableServiceState.Consolidation && ShouldBePartiallyLockedDown))
					{
						customBusinessObject.SetReadOnlyIncludingChildren(false);
					}
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = GetDocManagerInfo()); }
		}
		DocManagerInfo docManagerInfo;

		DocManagerInfo GetDocManagerInfo()
		{
			return new DtbBookingDocManagerInfo(this);
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return GetDocumentSupporter(); }
		}

		DocumentSupporter GetDocumentSupporter()
		{
			return documentSupporter ?? (documentSupporter = new DtbBookingDocumentSupporter(this));
		}
		DocumentSupporter documentSupporter;

		[ChildEditable]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					if (IsSub)
					{
						docAddresses = MasterBooking.DocAddresses;
					}
					else
					{
						docAddresses = new JobDocAddressDependentCollection(this);
						docAddresses.Load();
					}
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		public JobDocAddress BillingPartyAddress
		{
			get
			{
				if (billingPartyAddress == null || billingPartyAddress.IsDeleted)
				{
					var requirement = (((IDocAddresses)this).GetDocAddressRequirement(DocAddressType.ClientRequestedBillingParty));
					billingPartyAddress = DocAddresses.FindOrCreateWithRequirement(requirement);
				}
				return billingPartyAddress;
			}
		}
		JobDocAddress billingPartyAddress;

		[List("Lookups.BindToLists.AllOrganisations")]
		[ReadOnly(false)]
		public ZGuid BillingPartyOrLocalClientPK
		{
			get
			{
				if (Job != null)
				{
					return Job.JH_OA_LocalChargesAddr;
				}
				else
				{
					return BillingPartyAddress.E2_OA_Address;
				}
			}
			set
			{
				var job = Job;
				if (job != null)
				{
					if (IsOKToChangeBillingParty())
					{
						job.JH_OA_LocalChargesAddr = value;
					}
				}
				else
				{
					BillingPartyAddress.E2_OA_Address = value;
				}
				BillingPartyOrLocalClientPKInfo.RefreshBinding();
			}
		}

		// this should point to the Org Header OH_PK and not the OrgAddress OA_PK
		ZGuid CurrentBillingPartyOrLocalClientOrgPK
		{
			get
			{
				var job = GetJobHeaderForWorkflow();
				if (job?.LocalChargesAddr != null)
				{
					return job.LocalChargesAddr.OA_OH;
				}

				if (billingPartyAddress?.Address != null)
				{
					return billingPartyAddress.Address.OA_OH;
				}
				return ZGuid.Empty;
			}
		}

		[List("Lookups.BindToLists.AllOrganisations")]
		[ReadOnly(false)]
		public OrgAddress BillingPartyOrLocalClientAddress
		{
			get
			{
				if (Job != null)
				{
					return Job.LocalChargesAddr;
				}
				else
				{
					return BillingPartyAddress.Address;
				}
			}
		}

		public ZPropertyInfo BillingPartyOrLocalClientPKInfo
		{
			get
			{
				if (Job == null)
				{
					return GetWrappedZPropertyInfo(nameof(BillingPartyOrLocalClientPK), x => BillingPartyAddress.E2_OA_AddressInfo);
				}
				else
				{
					return GetWrappedZPropertyInfo(nameof(BillingPartyOrLocalClientPK), x => Job.JH_OA_LocalChargesAddrInfo);
				}
			}
		}

		public ZAddress BillingPartyOrLocalClientPK_ZAddress
		{
			get
			{
				var billingPartyOrLocalClientPK_ZAddress = new ZAddress(BillingPartyOrLocalClientPKInfo);
				billingPartyOrLocalClientPK_ZAddress.IsOrgVisible = true;
				billingPartyOrLocalClientPK_ZAddress.DefaultAddressType = AddressType.ARM;
				return billingPartyOrLocalClientPK_ZAddress;
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			OnDocAddressChanged(docAddress);

			if (docAddress.DocAddressType == DocAddressType.ClientRequestedBillingParty)
			{
				BillingPartyOrLocalClientPKInfo.RefreshBinding();
				UpdateConsigneeATL();
			}
		}

		void OnDocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			OnOrgAddressBeforeChange(docAddress);
		}

		void OnOrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideCheckpoint(docAddress);
		}

		SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.TransportJobMISCDetails;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result;

			switch (addressType)
			{
				case DocAddressType.ClientRequestedBillingParty:
					result = new JobDocAddressRequirement(addressType, AddressType.ARM, ContactType.LocalTransport);
					result.CanOverride = false;
					break;

				case DocAddressType.ShippingLineAddress:
					result = new JobDocAddressRequirement(addressType);
					result.CanOverride = false;
					break;

				default:
					result = GetDocAddressRequirement(addressType);
					break;
			}

			return result;
		}

		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.TransportCompanyDocumentaryAddress:
					return new JobDocAddressRequirement(addressType, AddressType.OFC, ContactType.LocalTransport, true);
				case DocAddressType.CarrierBookingAgent:
					return new JobDocAddressRequirement(addressType);
				default:
					return null;
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return addressType == DocAddressType.ClientRequestedBillingParty
				? new DebtorCollection(Factory)
				: GetOrgHeaderList(addressType);
		}

		OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.TransportCompanyDocumentaryAddress:
					return Lookups.LocalTransportOrganisations;
				case DocAddressType.CarrierBookingAgent:
					return Lookups.BindToLists.AllOrganisations;
				default:
					return null;
			}
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new DtbTransportDocAddressValidation(addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				var result = new List<DocAddressType>
				{
					DocAddressType.NotifyParty,
					DocAddressType.FinalConsigneeAddress,
					DocAddressType.OriginatingConsignorAddress,
					DocAddressType.TransportCompanyDocumentaryAddress,
					DocAddressType.CarrierBookingAgent
				};

				if ((((IDtbBookingParent)ConsolidationSingleJob?.ParentBO)?.JobType is ZString jobType &&
					(jobType == JobInvoicingConsumerTypes.Shipment.Code || jobType == JobInvoicingConsumerTypes.Consol.Code))
					|| (ConsolidationSingleJob != null && ConsolidationSingleJob.ParentBO == null))
				{
					result.Add(DocAddressType.ShippingLineAddress);
				}

				if (!KM_IsMaster)
				{
					result.Add(DocAddressType.ClientRequestedBillingParty);
				}

				return result.ToArray();
			}
		}

		public override string CanCancel()
		{
			var result = base.CanCancel();

			if (string.IsNullOrEmpty(result))
			{
				if (KM_IsMaster && SubBookings.Count > 0)
				{
					result = Res.GetString("d360e0ff-f084-455c-824e-1f49594e004d", "Cannot Deactivate this Master Booking as it has one or more attached Sub Bookings.");
				}
				else if (ConsignmentConsol != null)
				{
					result = Res.GetString("c628a8f6-6def-41ea-bd3e-9e562edb1462", "Cannot Deactivate this Booking as it is attached to one or more Consignments.");
				}
				else if (GetLandTransportJobs().Any(x => x.LTC_IsActive == true) || GetPortTransportJobs().Any(x => x.JJ_IsCancelled == false))
				{
					result = Res.GetString("E64E41B8-D391-4535-898E-75A79FB67399", "You cannot deactivate Booking {0} since it has a related active Transport Job", KM_Description);
				}
				else
				{
					result = JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
				}
			}

			return result;
		}

		public override bool IsCancelled
		{
			get { return base.IsCancelled; }
			set
			{
				base.IsCancelled = value;

				if (IsCancelled)
				{
					DeletePackageDivotsFromInstructions();
					Cancelled?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler Cancelled;

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

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => GetNewProcessTaskCollection());
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		ProcessTaskCollection GetNewProcessTaskCollection()
		{
			return new DtbBookingProcessTaskCollection(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			if (!CurrentBillingPartyOrLocalClientOrgPK.IsEmpty)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, CurrentBillingPartyOrLocalClientOrgPK, ZGuid.Empty);
			}
			else
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ZGuid.Empty);
			}

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, KM_TransportMode, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, ConsolidationSingleJob?.KB_JobDirection ?? ZString.Empty, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, KM_KT_NKBookingTemplate, ZString.Empty);

			return result;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingWorkflowDescriptorCode; }
		}

		string IJobNumberForWorkflow.JobNumber
		{
			get { return KM_JobID; }
		}

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get { return invoicingPlugIn ?? (invoicingPlugIn = GetNewInvoicingPlugIn()); }
		}

		IJobInvoicingPlugIn invoicingPlugIn;

		public void PurgeInvoicingPlugInCache()
		{
			invoicingPlugIn = null;
		}

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return InvoicingPlugIn.AllowInvoiceDeletion; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			InvoicingPlugIn.OnJobCreating(job);

			if (job != null)
			{
				CartageHelper.AttachCartageJobsToParentJob(job, PK);
				ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader createdJob)
		{
			if (ConsolidationSingleJob != null && ConsolidationSingleJob.KB_ParentID != ZGuid.Empty &&
				ConsolidationSingleJob.KB_ParentTableCode != DtbAgentBookingSchema.Constants.Prefix && createdJob.JH_ParentID == this.PK)
			{
				ErrorReporter.ReportOnce("JobHeader has just been created on this Booking record while it is attached to a parent. When attached to a parent the booking should use the parent's JobHeader.");
			}

			createdJob.LocalChargesAddrChanged -= (e, s) => BillingPartyOrLocalClientPKInfo.RefreshBinding();

			if (JobCreated != null)
			{
				JobCreated(this, null);
			}
			createdJob.LocalChargesAddrChanged += (e, s) => BillingPartyOrLocalClientPKInfo.RefreshBinding();
			BillingPartyOrLocalClientPKInfo.RefreshBinding();
		}

		public event EventHandler JobCreated;

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
			InvoicingPlugIn.OnJobDeleting(job);
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			InvoicingPlugIn.SetJobNumberFieldOnSaving();
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get { return InvoicingPlugIn.PK; }
		}

		string IJobHeaderParentCore.TableName
		{
			get { return InvoicingPlugIn.TableName; }
		}

		bool IJobHeaderParentCore.IsInDatabase
		{
			get { return InvoicingPlugIn.IsInDatabase; }
		}

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		string IJobNumber.JobNumber
		{
			get { return InvoicingPlugIn.JobNumber; }
		}

		IJobInvoicingPlugIn GetNewInvoicingPlugIn()
		{
			var parentJob = GetParentJobWithoutUsingLoader();
			return (parentJob != null && parentJob.InvoicingJob != null)
				? parentJob.InvoicingJob
				: new DtbBookingInvoicingPlugIn(this); // maybe a forced parent from an import from an external company?
		}

		IJobInvoicingSupporter GetNewInvoicingSupporter()
		{
			return GetParentJobWithoutUsingLoader() != null ? new DtbBookingParentInvoicingSupporter(this) : InvoicingPlugIn.InvoicingSupporter;
		}

		IDtbBookingParent GetParentJobWithoutUsingLoader()
		{
			IDtbBookingParent result = null;

			var consolidation = ConsolidationSingleJob;
			if (consolidation != null)
			{
				if (!consolidation.KB_ParentID.IsEmpty && !consolidation.KB_ParentTableCode.IsEmpty)
				{
					result = consolidation.Factory.Load(consolidation.KB_ParentTableCode, consolidation.KB_ParentID) as IDtbBookingParent;
				}
			}

			return result;
		}

		ICartageHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = ObjectFactory.Get<ICartageHelper>()); }
		}
		ICartageHelper cartageHelper;

		IConsignmentJobHelper ConsignmentJobHelper
		{
			get { return consignmentJobHelper ?? (consignmentJobHelper = ObjectFactory.Get<IConsignmentJobHelper>()); }
		}
		IConsignmentJobHelper consignmentJobHelper;

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new DtbBookingRatingAdaptersProvider(this); }
		}

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.TransportBooking; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { KM_Description, KM_TransportReference, StatusDescription }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		bool ISupportDataImporting.IsImportingData
		{
			get;
			set;
		}

		IDtbBookingInstruction[] IDtbBooking.Instructions
		{
			get { return Instructions.ToArray(); }
		}

		public ZGuid JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		IPkgPackageJob IDtbBooking.PackageJob
		{
			get { return PackageJob; }
		}

		IEnumerable<IBusiness> IDtbBooking.RelatedJobs
		{
			get { return RelatedJobs; }
		}

		IJobDocAddress IDtbBooking.Address
		{
			get { return Address; }
		}

		ZString IRelatedJob.JobDescription
		{
			get { return KM_Description.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} - {1}", HumanReadableNameWithoutID, KM_Description); }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return KM_JobID; }
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
			get { return ControllerIDs.DtbBooking; }
		}

		[ChildEditable]
		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
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

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			return AdditionalReferenceNumberTypes;
		}

		public JobDocAddress GetAddress(DocAddressType docAddressType)
		{
			JobDocAddress result;

			if (docAddressType == DocAddressType.BookingPartyDocumentaryAddress)
			{
				result = ConsolidationSingleJob.BookedByAddress;
			}
			else
			{
				var requirement = (((IDocAddresses)this).GetDocAddressRequirement(docAddressType));
				JobDocAddress address = DocAddresses.FindOrCreateWithRequirement(requirement);

				if (address != null)
				{
					address.MakePersistentEvenIfEmpty();
					address.ReadOnly = ReadOnly;
				}

				result = address;
			}

			return result;
		}

		public MasterFiles.Business.Directions JobDirection
		{
			get
			{
				if (BookingTemplate == null)
				{
					return MasterFiles.Business.Directions.Unknown;
				}
				else if (BookingTemplate.KT_Code[0] == 'I')
				{
					return MasterFiles.Business.Directions.Import;
				}
				else if (BookingTemplate.KT_Code[0] == 'E')
				{
					return MasterFiles.Business.Directions.Export;
				}
				else
				{
					return MasterFiles.Business.Directions.Unknown;
				}
			}
		}

		public ZString? ParentTransportMode
		{
			get
			{
				return ParentJob?.TransportMode;
			}
		}

		ZString IJobCostingPlugIn.JK_UniqueConsignRef => KM_JobID;

		RefUNLOCO IJobCostingPlugIn.LoadPort => null;

		RefUNLOCO IJobCostingPlugIn.DischargePort => null;

		public JobProfitLossCollection ProfitLossContainer => profitLossContainer ?? (profitLossContainer = new JobProfitLossCollection(Factory));
		JobProfitLossCollection profitLossContainer;

		decimal IJobCostingPlugIn.ConsolExchangeRate => 0m;

		RefCurrency IJobCostingPlugIn.ConsolCurrency => null;

		bool IJobCostingPlugIn.IsMasterCollect => false;

		OrgHeader IJobCostingPlugIn.ReceivingAgent => null;

		OrgHeader IJobCostingPlugIn.ReceivingAgentAPInvoicingParty => null;

		OrgHeader IJobCostingPlugIn.ReceivingAgentARInvoicingParty => null;

		OrgHeader IJobCostingPlugIn.SendingAgent => null;

		OrgHeader IJobCostingPlugIn.SendingAgentAPInvoicingParty => null;

		OrgHeader IJobCostingPlugIn.SendingAgentARInvoicingParty => null;

		ZString IJobCostingPlugIn.TransportMode => ZString.Empty;

		ZString IJobCostingPlugIn.ContainerMode => ZString.Empty;

		ZString IJobCostingPlugIn.Direction => ZString.Empty;

		ZString IJobCostingPlugIn.ConsolType => ZString.Empty;

		ZString IJobCostingPlugIn.Module => ApportionmentMethodModules.TransportBooking;

		CodeDescriptionPairList IJobCostingPlugIn.PrepaidCollectList => prepaidCollectList ?? (prepaidCollectList = new CodeDescriptionPairList());
		CodeDescriptionPairList prepaidCollectList;

		IGenericJobCostSupporter IGenericJobCostPlugIn.CostSupporter
		{
			get
			{
				if (costSupporter == null)
				{
					costSupporter = KM_IsMaster ? new MasterBookingJobCostSupporter(this) : new DtbBookingJobCostSupporter(this);
				}
				return costSupporter;
			}
		}
		IGenericJobCostSupporter costSupporter;

		public void UpdateConfirmationsEstimateTime()
		{
			foreach (var confirmation in PickupConfirmations)
			{
				confirmation.UpdatePickupEstimatedTime();
			}
		}

		public void UpdateDeliveryConfirmationEstimateTime()
		{
			var pickupConfirmationWithLatestEstimated = PickupConfirmations.Any(c => c.KK_Estimated.IsEmpty) ? null : PickupConfirmations.OrderByDescending(c => c.KK_Estimated).FirstOrDefault();
			var latestPickupEstimated = pickupConfirmationWithLatestEstimated != null ? pickupConfirmationWithLatestEstimated.KK_Estimated : ZDateTime.Empty;
			var latestPickupZonePK = pickupConfirmationWithLatestEstimated != null ? pickupConfirmationWithLatestEstimated.Instruction.KN_TZ_DomesticZone : ZGuid.Empty;

			foreach (var deliveryConfirmation in DeliveryConfirmations)
			{
				var refTransitTime = latestPickupEstimated.IsValid ? GetServiceLevelTransitHours(latestPickupZonePK, deliveryConfirmation.Instruction.KN_TZ_DomesticZone) : null;
				if (refTransitTime != null)
				{
					var etd = latestPickupEstimated.AddHours(refTransitTime.RTT_TransitHours);
					deliveryConfirmation.KK_Estimated = TransportWorkingDays.GetWorkingDate(Factory, etd, Job != null && Job.Department != null ? Job.Department.PK : GlbDepartment.CurrentDepartment.PK);
				}
			}
		}

		public void SetIsEmptyContainerOnAllRelatedConfirmations()
		{
			Instructions.SelectMany(i => i.Confirmations).ForEach(c => c.SetIsEmptyContainer());
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

		decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK) => 0m;

		void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference) => Logs.AddNew(@event, reference);

		ZString IJobCostingPlugIn.GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob) => ZString.Empty;

		bool IDtbMasterBookingEntity.IsMaster => KM_IsMaster;

		short IDtbMasterBookingEntity.GetMasterBookingVersion() => KM_MasterBookingVersion;

		void IDtbMasterBookingEntity.SetMasterBookingVersion(short newMasterBookingVersion)
		{
			KM_MasterBookingVersion = newMasterBookingVersion;
		}

		IEnumerable<ZPropertyInfo> IDtbMasterBookingEntity.ReplicationFieldInfos
		{
			get
			{
				if (replicationFieldInfos == null)
				{
					replicationFieldInfos = DtbMasterBookingReplication.GetPropertyInfosFromListOfColumns(this, DtbMasterBookingReplication.DtbBookingReplicatedColumns);
				}

				return replicationFieldInfos;
			}
		}
		IEnumerable<ZPropertyInfo> replicationFieldInfos;

		DtbMasterBookingHelper MasterBookingHelper { get; }

		[ChildEditable]
		public DtbBookingCollection SubBookings
		{
			get
			{
				if (subBookings == null)
				{
					subBookings = GetNewBookingsCollection();
					RegisterEditableChildObject(subBookings);
				}

				return subBookings;
			}
		}

		IEnumerable<PkgPackage> SubBookingPackages => SubBookings.SelectMany(sb => sb.PackageJob.Packages);

		DtbBookingCollection GetNewBookingsCollection()
		{
			return new DtbBookingCollection(this);
		}

		DtbBookingCollection subBookings;

		public bool HaveJobCO2e => this.JobCO2eExists();

		public ZString TotalCO2eForBinding => this.GetTotalCO2eForBinding();

		public ZPropertyInfo TotalCO2eForBindingInfo => GetZPropertyInfo(nameof(TotalCO2eForBinding));

		[DecimalPlaces(3)]
		public ZDecimal TotalCO2eForSorting => this.GetTotalCO2e();

		public ZPropertyInfo TotalCO2eForSortingInfo => GetZPropertyInfo(nameof(TotalCO2eForSorting));

		List<string> ICO2eCalculationSupporter.ValidateInputs()
		{
			var reasons = new List<string>();
			if (Instructions.Count == 0)
			{
				reasons.Add(Res.GetString("7846e163-ed09-4204-b817-ff1bcc5b011c", "Transport Booking > Details > Instructions is empty."));
				return reasons;
			}

			var result = new CO2eInstructionMatcher(this).FindAllMatches();
			if (!result.IsValid)
			{
				reasons.Add(Res.GetString("3ffcd376-19f4-48a3-942a-024a688ca02c", "Transport Booking > Details > Instructions do not have a matching load/unload package."));
				return reasons;
			}
			result.Actions.ForEach(action =>
			{
				if (action is CO2eUnLoadAction unLoadAction)
				{
					CheckCO2eAction(unLoadAction, reasons);
				}
			});

			return reasons.Distinct().ToList();
		}

		void CheckCO2eAction(CO2eUnLoadAction unLoadAction, List<string> reasons)
		{
			if (unLoadAction.Weight == 0)
			{
				reasons.Add(Res.GetString("ab1058f8-bbcd-46b0-9252-3163377c041d", "Transport Booking > Packing > Package Detail > '{0}' > Weight is empty.", unLoadAction.UnLoadBy.PackageDescriptionWithIDAndQty));
			}

			CheckInstructionAddress(unLoadAction.LoadBy.Instruction, reasons);
			CheckInstructionAddress(unLoadAction.UnLoadBy.Instruction, reasons);
		}

		void CheckInstructionAddress(DtbBookingInstruction instruction, List<string> reasons)
		{
			if (instruction.Address.IsEmpty)
			{
				reasons.Add(Res.GetString("f055986f-08ea-4f44-a768-fd1a4cace49e", "Transport Booking > Details > Instruction '{0}' > Address is empty.", instruction.KN_Sequence));
			}
		}

		void ICO2eCalculationSupporter.OnRequested()
		{
			this.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			(this as ICO2eCalculationSupporter).RecordLog(CO2eEventType.Requested, string.Empty);
		}

		void ICO2eCalculationSupporter.OnRejected(string reason)
		{
			this.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			(this as ICO2eCalculationSupporter).RecordLog(CO2eEventType.Rejected, reason);
		}

		void ICO2eCalculationSupporter.OnCalculated(bool succeeded)
		{
			(this as ICO2eCalculationSupporter).RefreshCO2e();
			if (GetParentJobWithoutUsingLoader() is ICO2ePrePostCarriage parent)
			{
				parent.OnTransportBookingCalculated(this);
			}
		}

		AdditionalCalculationSupporter[] ICO2eCalculationSupporter.AdditionalCalculationSupporters => Array.Empty<AdditionalCalculationSupporter>();

		void ICO2eCalculationSupporter.OnAdditionalSupporterCalculated(ICO2eCalculationSupporter additionalSupporter) { }

		bool ICO2eCalculationSupporter.SaveEmissionsLogToNoteOnCalculated => false;

		void ICO2eProvider.RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue) => this.LogGHGEvent(type, extra, previousCO2eValue);

		bool ICO2eProvider.RequireTEU => false;

		public ZString CO2eStatus => this.GetCO2eStatus();

		ZGuid ICO2eParent.JobCO2eParentID => PK;

		ZString ICO2eParent.JobCO2eParentTableCode => DtbBookingSchema.Constants.Prefix;

		public IJobCO2eCollection JobCO2eCollection
		{
			get
			{
				if (jobCO2eCollection == null)
				{
					jobCO2eCollection = new JobCO2eCollection(this);
					jobCO2eCollection.JobCO2e_StatusChanged += CO2eStatusChanged;
				}
				return jobCO2eCollection;
			}
		}
		IJobCO2eCollection jobCO2eCollection;

		void CO2eStatusChanged(object sender, EventArgs e)
		{
			RefreshCO2eBinding();
			if (!FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.Value)
			{
				return;
			}

			if (GetParentJobWithoutUsingLoader() is ICO2ePrePostCarriage parent)
			{
				parent.OnTransportBookingCO2eStatusChanged(this);
			}
		}

		void RefreshCO2eBinding()
		{
			if (HaveJobCO2e)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalCO2eForBinding();
					Validation.ValidateTotalCO2eForSorting();
				}
				TotalCO2eForBindingInfo.RefreshBinding();
				TotalCO2eForSortingInfo.RefreshBinding();
			}
		}

		void ICO2eParent.RefreshCO2e() => RefreshCO2eBinding();

		ISupportWebAddressValidation[] IAddressesValidation.AddressesToValidate
		{
			get
			{
				var actions = new CO2eInstructionMatcher(this).FindAllMatches().Actions;
				return actions.Select<ICO2eMatchAction, ISupportWebAddressValidation>(action => action.Address == null ? null : (action.Address.E2_AddressOverride ? action.Address : action.Address.Address))
					.Where(address => address != null)
					.GroupBy(x => x.EntityPK)
					.Select(x => x.FirstOrDefault())
					.ToArray();
			}
		}
	}

	public enum TransportBookingInstructionView
	{
		TransportBookings,
		Standard,
		Instruction,
		Package,
		CustomFields,
		AdditionalReferences
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.Business
{
	public sealed partial class DtbBooking
	{
		public StmNoteContexts GetNoteContextsForRelatedNotesForTest()
		{
			return NoteContextsForRelatedNotes;
		}

		public CustomBusinessObject CustomBusinessObject_ForTest { get { return CustomBusinessObject; } }

		[ChildEditableTestExclude]
		public DtbBookingPackageCollection_PackageView Packages_PackageView_ForTest
		{
			get
			{
				if (packages_PackageView == null)
				{
					InitialisePackages_PackageView();
				}

				return packages_PackageView;
			}
		}

		public DtbBookingValidation ValidationForTest { get; set; }
	}
}

#endif
