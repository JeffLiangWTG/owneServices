using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;

namespace Enterprise.TransportCommon.Business
{
	[CodeProperty(DtbBookingSchema.Constants.KM_JobID)]
	[DescriptionProperty(DtbBookingSchema.Constants.KM_Description)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.DtbTransport)]
	public abstract partial class DtbTransport : AutoDtbBooking,
		IAdditionalReferenceNumberTypeProvider,
		IDocAddresses,
		IDtbTransport,
		IEDocsProvider,
		IJobInvoicingPlugIn,
		IRatingSupporter,
		IWorkflowProvider,
		IUniversalXMLNoteParent,
		IAddress,
		ITransportAdditionalReferenceNumbers
	{
		protected DtbTransport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConfirmLoadingCorrectType(factory, row);
		}

		#region Confirm Loading Correct Type

		void ConfirmLoadingCorrectType(BusinessObjectFactory factory, DataRow row)
		{
			var loadAs = TypeDecider.GetTypeForLoad(row, factory);
			if (!loadAs.IsAssignableFrom(this.GetType()))
			{
				throw new NotSupportedException(string.Format(Culture.Invariant, "Job type did not match the type of the class. The class type was {0}, but should have been {1}", GetType().ToString(), loadAs.ToString()));
			}
		}

		#endregion

		#region TypeDecider

		public static readonly DtbTransportTypeDecider TypeDecider = new DtbTransportTypeDecider();

		#endregion

		#region Related Entities

		#region BookingTemplate

		public DtbTransportTmpl BookingTemplate
		{
			get { return (DtbTransportTmpl)Factory.LoadFromNaturalKey(BookingTemplateType, DtbBookingTmplSchema.KT_Code, KM_KT_NKBookingTemplate); }
		}

		protected abstract Type BookingTemplateType { get; }

		#endregion

		#region BusinessObjectsWithRelatedEvents

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

				var dtbTransportInstructions = Instructions.Cast<DtbTransportInstruction>();
				relatedObjects.AddRange(dtbTransportInstructions);
				relatedObjects.AddRange(dtbTransportInstructions.SelectMany(i => i.Confirmations.Cast<DtbTransportConfirmation>()));

				return relatedObjects.ToArray();
			}
		}

		#endregion

		#region ConsolidationSingleJob

		[ChildEditable]
		[ChildEditableTestExclude]
		public DtbTransportConsolidation ConsolidationSingleJob
		{
			get
			{
				if (consolidationSingleJob == null)
				{
					consolidationSingleJob = (DtbTransportConsolidation)Factory.Load(ConsolidationType, KM_KB_Booking);
					if (consolidationSingleJob != null && DtbChildEditableService.GetState(Factory) == DtbChildEditableServiceState.Transport)
					{
						RegisterEditableChildObject(consolidationSingleJob);
					}
				}

				return consolidationSingleJob;
			}
		}

		DtbTransportConsolidation consolidationSingleJob;

		protected abstract Type ConsolidationType { get; }

		#endregion

		#region Instructions

		[ChildEditable]
		public IDtbTransportInstructionCollection Instructions
		{
			get
			{
				if (instructions == null)
				{
					instructions = GetNewInstructionsCollection();
					RegisterEditableChildObject(instructions);
					instructions.CountChanged += Instructions_CountChanged;
				}

				return instructions;
			}
		}

		protected abstract IDtbTransportInstructionCollection GetNewInstructionsCollection();

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

		protected virtual bool UpdateStatusOnInstructionsCountChanged
		{
			get { return !IsDeleted; }
		}

		IDtbTransportInstructionCollection instructions;

		protected bool IsInstructionsInstatiated
		{
			get
			{
				return instructions != null;
			}
		}

		#endregion

		#region Job

		[ChildEditableTestExclude]
		[ChildEditable]
		public virtual JobHeader Job
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
				foreach (DtbTransportInstruction i in instructions)
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

		#endregion

		#region PackageJob

		public abstract PkgPackageJob PackageJob { get; }

		#endregion

		#region Packages

		public PackageCollectionForTransport AssignedPackages
		{
			get { return packages ?? (packages = new PackageCollectionForTransport(this)); }
		}

		PackageCollectionForTransport packages;

		#endregion

		#region Packages_PackageView

		[ChildEditable]
		public PackageCollection_PackageView Packages_PackageView
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

		protected abstract PackageCollection_PackageView GetNewPackages_PackageView();

		PackageCollection_PackageView packages_PackageView;

		#endregion

		#region PackageDivots

		public IDtbTransportInstructionPkgDivotCollection PackageDivots
		{
			get { return packageDivots ?? (packageDivots = GetNewInstructionPkgDivotCollection()); }
		}

		protected abstract IDtbTransportInstructionPkgDivotCollection GetNewInstructionPkgDivotCollection();

		IDtbTransportInstructionPkgDivotCollection packageDivots;

		#endregion

		#region Containers

		public IEnumerable<Package_PackageView> Containers
		{
			get { return Packages_PackageView.Cast<Package_PackageView>().Where(p => p.Package.IsContainer); }
		}

		#endregion

		#region LoosePackages

		public IEnumerable<Package_PackageView> LoosePackages
		{
			get { return Packages_PackageView.Cast<Package_PackageView>().Where(p => !p.Package.IsContainer); }
		}

		#endregion

		#region Notes

		#region BusinessObjectsWithRelatedNotes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);
				foreach (DtbTransportInstruction instruction in Instructions)
				{
					var address = instruction.Address;
					if (!address.E2_AddressOverride)
					{
						result.Add(address.Organisation);
					}
				}
				AddBusinessObjectsWithRelatedNotes(result);
				return result.ToArray();
			}
		}

		protected virtual void AddBusinessObjectsWithRelatedNotes(List<BusinessObject> bizOs)
		{
		}

		#endregion

		#region NoteContextsForRelatedNotes

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

		#region AddNoteContextsForRelatedNotes_Directions

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

		#endregion

		#region AddNoteContextsForRelatedNotes_FreightModes

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

		#endregion

		#endregion

		#endregion

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			KM_Status = TransportStatuses.Codes.Available;
			KM_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

		#endregion

		#region Properties

		// persistent

		#region KM_IsActive

		public override ZBool KM_IsActive
		{
			get { return base.KM_IsActive; }
			set
			{
				base.KM_IsActive = value;
				UpdateStatus();
			}
		}

		#endregion

		#region KM_KB_Booking

		[RelatedBusinessObject("ConsolidationSingleJob")]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZGuid KM_KB_Booking
		{
			get { return base.KM_KB_Booking; }
			set { base.KM_KB_Booking = value; }
		}

		#endregion

		#region KM_KT_NKBookingTemplate

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
			using (new DisposableAction(
				() => KM_KT_NKBookingTemplateCache = newValue,
				() => KM_KT_NKBookingTemplateCache = null))
			{
				DefaultFromTemplate();
			}
		}

		ZString? KM_KT_NKBookingTemplateCache;

		#region DefaultFromTemplate

		void DefaultFromTemplate()
		{
			var transportTemplate = BookingTemplate;
			if (transportTemplate != null)
			{
				var existingPackages = GetPreExistingAssignedPackages();

				KM_Description = transportTemplate.KT_Description;
				KM_Direction = transportTemplate.KT_Direction; // required to default confirmations
				KM_RatingFreightMode = transportTemplate.KT_RatingFreightMode;

				if (!AddingInstructionsFromTemplateSemaphore.IsSuspended)
				{
					Array.ForEach(Instructions.ToArray(), i => i.Delete()); // we sort instructions when deleting, using DeleteAll() throws an exception when deleting the last element
					transportTemplate.Instructions.ApplySort(DtbBookingInstructionTmplSchema.Constants.K2_Sequence, ListSortDirection.Ascending);
					Array.ForEach(transportTemplate.Instructions.ToArray(), t => AddInstructionFromTemplate(t, existingPackages));
				}

				var packageJobHasPackages = (PackageJob != null) && PackageJob.Packages.Any();
				if (!AssignedPackages.Any() && packageJobHasPackages)
				{
					var message = Res.GetString("77eef24f-b382-4a03-96b7-e408450bb261", @"Could not find Containers/Outer Packages based on Instruction Template Package Type. See Packages Tab.");
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

		void AddInstructionFromTemplate(DtbTransportInstructionTmpl instructionTemplate, Collection<PkgPackage> existingPackages)
		{
			var instruction = (DtbTransportInstruction)Instructions.AddNew();
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

		#endregion

		#endregion

		#region KM_Status

		[ReadOnly(true)]
		public override ZString KM_Status
		{
			get { return base.KM_Status; }
			set { base.KM_Status = value; }
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

		protected abstract ZString GetExpectedStatus();

		#endregion

		#region ParentContainerLinksAndAddresses

		public List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)> addressDetails, bool shouldFallbackToShipmentContainerYard)> ParentContainerLinksAndAddresses { get; set; }

		#endregion

		#region PackageContainerLinks

		public IDictionary<ZInt, PkgPackage> PackageContainerLinks { get; set; }

		#endregion

		// calculated

		#region StatusDescription

		[ResourceStringData("DtbTransport|StatusDescription", Caption = "Status")]
		public ZString StatusDescription
		{
			get { return Lookups.BindToLists.Statuses.GetDescriptionFromCode(KM_Status); }
		}

		public ZPropertyInfo<ZString> StatusDescriptionInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(nameof(StatusDescription)); }
		}

		#endregion

		#region TransportBookingPartyReference

		public ZString TransportBookingPartyReference
		{
			get
			{
				var externalTB = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
				return externalTB != null ? externalTB.CE_EntryNum : ZString.Empty;
			}
		}

		#endregion

		#region BookedByOrganisationPK

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

		#endregion

		#region LocalClient

		[List("Lookups.BindToLists.AllOrganisations")]
		[ResourceStringData("DtbTransport|LocalClient", Caption = "Client")]
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

		#endregion

		// rating

		#region GetTotalLoosePackageQuantity

		public ZInt GetTotalLoosePackageQuantity()
		{
			return LoosePackages.Sum(p => p.QuantityFromInstructions);
		}

		#endregion

		#region GetTotalLoosePackageUnit

		public ZString GetTotalLoosePackageUnit()
		{
			var result = ZString.Empty;

			foreach (Package_PackageView package in LoosePackages)
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

		#endregion

		#region GetTotalLoosePackageWeight / GetTotalContainerisedWeight

		public ZWeight GetTotalLoosePackageWeight()
		{
			return GetTotalWeight(LoosePackages);
		}

		public ZWeight GetTotalContainerisedWeight()
		{
			return GetTotalWeight(Containers);
		}

		protected ZWeight GetTotalWeight(IEnumerable<Package_PackageView> packageViews)
		{
			var result = new ZWeight(0, DtbTransportTotalsHelper.TotalWeightUnit);

			foreach (var weight in packageViews.Select(p => p.WeightFromInstructions).Where(w => w.IsValid))
			{
				result += weight;
			}

			return result;
		}

		#endregion

		#region GetTotalLoosePackageVolume / GetTotalContainerisedVolume

		public ZVolume GetTotalLoosePackageVolume()
		{
			return GetTotalVolume(LoosePackages);
		}

		public ZVolume GetTotalContainerisedVolume()
		{
			return GetTotalVolume(Containers);
		}

		protected ZVolume GetTotalVolume(IEnumerable<Package_PackageView> packageViews)
		{
			var result = new ZVolume(0, DtbTransportTotalsHelper.TotalVolumeUnit);

			foreach (var volume in packageViews.Select(p => p.VolumeFromInstructions).Where(v => v.IsValid))
			{
				result += volume;
			}

			return result;
		}

		#endregion

		#endregion

		#region Flags

		#region IsDelivered

		public bool IsDelivered
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered); }
		}

		#endregion

		#region IsPickupDirection

		public bool IsPickupDirection
		{
			get { return KM_Direction == Constants.CartageDirection.Origin || KM_Direction == Constants.CartageDirection.Export; }
		}

		#endregion

		#region IsDeliveryDirection

		public bool IsDeliveryDirection
		{
			get { return KM_Direction == Constants.CartageDirection.Destination || KM_Direction == Constants.CartageDirection.Import; }
		}

		#endregion

		#region IsLocalDirection

		public bool IsLocalDirection
		{
			get { return KM_Direction == Constants.CartageDirection.Local; }
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IsAnyPackageHazardous

		public bool IsAnyPackageHazardous
		{
			get { return AssignedPackages.Any(p => IsPackageOrChildrenHazardous(p)); }
		}

		bool IsPackageOrChildrenHazardous(PkgPackage package)
		{
			return package.UNDGs.Any() || package.Packages.Any(p => IsPackageOrChildrenHazardous(p));
		}

		#endregion

		#region IsAnyPackageRequiresRefridgeration

		public bool IsAnyPackageRequiresRefridgeration
		{
			get { return AssignedPackages.Any(p => IsPackageOrChildrenRequiresRefridgeration(p)); }
		}

		bool IsPackageOrChildrenRequiresRefridgeration(PkgPackage package)
		{
			return package.IsTemperatureControlled || package.Packages.Any(p => IsPackageOrChildrenRequiresRefridgeration(p));
		}

		#endregion

		// rating

		#region IsContainerisedOnly

		public ZBool IsContainerisedOnly
		{
			get { return IsContainerised && !IsLoose; }
		}

		#endregion

		#region IsLooseOnly

		public ZBool IsLooseOnly
		{
			get { return IsLoose && !IsContainerised; }
		}

		#endregion

		#region IsContainerised

		public ZBool IsContainerised
		{
			get { return Containers.Any(); }
		}

		#endregion

		#region IsLoose

		public ZBool IsLoose
		{
			get { return LoosePackages.Any(); }
		}

		#endregion

		#region IsFCL

		public ZBool IsFCL
		{
			get { return IsContainerised && !IsFTL; }
		}

		#endregion

		#region IsFTL

		public ZBool IsFTL
		{
			get { return Containers.Any(c => c.Package.Container.K0_ContainerMode == Constants.ContainerModes.FTL || (c.Package.Container.ContainerType != null && c.Package.Container.ContainerType.IsRoadTruckContainer)); }
		}

		#endregion

		#region IsFCLPort

		public ZBool IsFCLPort
		{
			get { return IsFCL && IsPort; }
		}

		#endregion

		#region IsPort

		public ZBool IsPort
		{
			get { return Instructions.Cast<DtbTransportInstruction>().Any(i => i.IsCTO || i.IsCYD); }
		}

		#endregion

		#endregion

		#region Lookups

		public new DtbTransportLookups Lookups
		{
			get { return (DtbTransportLookups)base.Lookups; }
		}

		protected sealed override DtbBookingLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportValidation Validation
		{
			get { return (DtbTransportValidation)base.Validation; }
		}

		protected sealed override DtbBookingValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportValidation GetNewValidationCore();

		#endregion

		#region Save

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			// tested by WorkFlowTests
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public sealed override void OnSaving()
		{
			PopulateUnqiueIDIfNeeded();
			BeforeOnSaving();
			base.OnSaving();
		}

		protected virtual void BeforeOnSaving()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				DeactivateJobHeader();
			}
		}

		internal void PopulateUnqiueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted && KM_JobID.IsEmpty)
			{
				KM_JobID = GenerateID();
			}
		}

		ZString GenerateID()
		{
			NumberGenerator.Generate();
			NumberGenerator.EnforceMaxLengths();
			UniqueIndexHandler = new UniqueIndexHandler(NumberGenerator.PrimaryTarget);

			return NumberGenerator.PrimaryTarget.Value;
		}

		#region OnSaved

		public sealed override void OnSaved(bool saveSucceeded)
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

		protected virtual void OnSaveFailedCore()
		{
		}

		protected StmALog GetLatestLog(Event eventType)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, this.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " desc";

			return Factory.LoadTop1<StmALog>(query);
		}

		#endregion

		#region NumberGenerator

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
				PrimaryTarget = TransportNumberGeneratorTargetCore()
			};

			generator.ValueProviders.AddRange(new StandardValueSource().Concat(new DomesticValueSource(this)));
			return generator;
		}

		protected abstract NumberGeneratorTarget TransportNumberGeneratorTargetCore();

		#endregion

		#endregion

		#region Delete

		public sealed override void Delete()
		{
			if (AllowBookingDeleteWithoutWarning() || AllowBookingDeleteWithWarning())
			{
				// tested by SaveAndDeleteBusinessObject()
				Array.ForEach(Instructions.ToArray(), i => i.Delete()); // we sort instructions when deleting, using DeleteAll() throws an exception when deleting the last element
				DeactivateJobHeader();

				// tested by WorkFlowTests
				WorkflowItems.RemoveAndDeleteAll();

				DeleteCore();

				base.Delete();
			}
		}

		#region AllowBookingDeleteWithoutWarning

		bool AllowBookingDeleteWithoutWarning()
		{
			return AllowBookingDeleteWithoutWarningCore();
		}

		protected virtual bool AllowBookingDeleteWithoutWarningCore()
		{
			return true;
		}

		#endregion

		#region AllowBookingDeleteWithWarning

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

		#endregion

		#region DeleteJobHeader

		void DeactivateJobHeader()
		{
			var job = Job;
			if (job != null && job.CanDeactivate && job.JH_ParentID == this.PK)
			{
				job.MarkAsInactive();
			}
		}

		#endregion

		#region DeleteCore

		protected virtual void DeleteCore()
		{
		}

		#endregion

		#endregion

		#region SuspendSettingPackages

		public IDisposable SuspendSettingPackages()
		{
			return new DisposableAction(() => suspendSettingPackages++, () => suspendSettingPackages--);
		}

		public ZBool IsSettingPackagesSuspended
		{
			get { return suspendSettingPackages > 0; }
		}

		int suspendSettingPackages;

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DtbTransportFountainUniqueIndexFailureHandler(this)); }
		}

		class DtbTransportFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DtbTransportFountainUniqueIndexFailureHandler(DtbTransport transport)
				: base(DtbBookingSchema.Constants.Indexes.NR_UC__KM_JobID, transport)
			{
				Transport = transport;
			}

			readonly DtbTransport Transport;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get
				{
					return Transport.UniqueIndexHandler != null
						? Transport.UniqueIndexHandler.NumberFountain
						: Transport.NumberFountainForUniqueID;
				}
			}

			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				return Transport.UniqueIndexHandler != null
					? Transport.UniqueIndexHandler.FindMaxValueInDatabase(connection, DtbBookingSchema.KM_JobID)
					: base.CommandToFindMaxValueInDatabase(connection);
			}
		}

		protected abstract INumberFountainProxy NumberFountainForUniqueID { get; }
		UniqueIndexHandler UniqueIndexHandler;

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		// Interfaces

		#region CusEntryNum

		#region AdditionalReferenceNumbers

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

		#endregion

		#region IAdditionalReferenceNumberTypeProvider Members

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			return AdditionalReferenceNumberTypes;
		}

		protected abstract CodeDescriptionPairList AdditionalReferenceNumberTypes { get; }

		#endregion

		#endregion

		#region IAddress members

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

		#endregion

		#region IDocAddresses Members

		#region DocAddresses

		[ChildEditable]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region BillingParty

		#region TransportCommon.Business

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

		#endregion

		#region BillingPartyOrLocalClientPK

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
		protected ZGuid CurrentBillingPartyOrLocalClientOrgPK
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

		protected virtual bool IsOKToChangeBillingParty()
		{
			return true;
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

		#endregion

		#region BillingPartyOrLocalClientPK_ZAddress

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

		#endregion

		#endregion

		#region Events

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

		protected virtual void OnDocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			OnOrgAddressBeforeChange(docAddress);
		}

		protected virtual void OnOrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		#endregion

		#region CanDeleteAddress

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		#endregion

		#region GetCanOverrideCheckpoint

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideCheckpoint(docAddress);
		}

		protected abstract SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress);

		#endregion

		#region GetDocAddressRequirement

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result;

			switch (addressType)
			{
				case DocAddressType.ClientRequestedBillingParty:
					result = new JobDocAddressRequirement(addressType, AddressType.ARM, ContactType.LocalTransport);
					result.CanOverride = false;
					break;
				default:
					result = GetDocAddressRequirement(addressType);
					break;
			}

			return result;
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return addressType == DocAddressType.ClientRequestedBillingParty
				? new DebtorCollection(Factory)
				: GetOrgHeaderList(addressType);
		}

		protected virtual OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region PiggyBackedDocAddressValidation

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new DtbTransportDocAddressValidation(addressToValidate);
		}

		#endregion

		#region SupportedAddressTypes

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new[] { DocAddressType.ClientRequestedBillingParty }.Concat(AdditionalSupportedAddressTypes).ToArray(); }
		}

		protected virtual DocAddressType[] AdditionalSupportedAddressTypes
		{
			get { return Array.Empty<DocAddressType>(); }
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = GetDocManagerInfo()); }
		}
		DocManagerInfo docManagerInfo;

		protected abstract DocManagerInfo GetDocManagerInfo();

		#endregion

		#region IJobInvoicingPlugIn Members

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get { return invoicingPlugIn ?? (invoicingPlugIn = GetNewInvoicingPlugIn()); }
		}

		IJobInvoicingPlugIn invoicingPlugIn;

		protected abstract IJobInvoicingPlugIn GetNewInvoicingPlugIn();

		public void PurgeInvoicingPlugInCache()
		{
			invoicingPlugIn = null;
		}

		#endregion

		#region IJobInvoicingPlugIn_InvoicingSupporter

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = GetNewInvoicingSupporter()); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		protected virtual IJobInvoicingSupporter GetNewInvoicingSupporter()
		{
			return InvoicingPlugIn.InvoicingSupporter;
		}

		#endregion

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return InvoicingPlugIn.AllowInvoiceDeletion; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			InvoicingPlugIn.OnJobCreating(job);

			OnJobCreatingCore(job);
		}

		protected virtual void OnJobCreatingCore(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
			job.LocalChargesAddrChanged -= (e, s) => BillingPartyOrLocalClientPKInfo.RefreshBinding();

			if (JobCreated != null)
			{
				JobCreated(this, null);
			}
			job.LocalChargesAddrChanged += (e, s) => BillingPartyOrLocalClientPKInfo.RefreshBinding();
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

		#endregion

		#region IJobHeaderParentCore Members

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

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = GetDocumentSupporter()); }
		}
		DocumentSupporter documentSupporter;

		protected abstract DocumentSupporter GetDocumentSupporter();

		#endregion

		#region IEDocsProvider members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return InvoicingPlugIn.JobNumber; }
		}

		#endregion

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return GetRatingAdaptersProvider(); }
		}

		protected abstract RatingAdaptersProvider GetRatingAdaptersProvider();

		#endregion

		#region IWorkflowProvider Members

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

		protected abstract ProcessTaskCollection GetNewProcessTaskCollection();

		ProcessTaskCollection workflowItems;

		#endregion

		#region IWorkflowProviderCore Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return GetTemplateSelectionCriteriaCore();
		}

		protected virtual IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowTypeCore; }
		}

		protected abstract ZString WorkflowTypeCore { get; }

		#endregion

		#region GetServiceLevelTransitHours

		protected RefTransitTime GetServiceLevelTransitHours(ZGuid pickupZone, ZGuid deliveryZone)
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

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.TransportCommon.Business
{
	public abstract partial class DtbTransport
	{
		public StmNoteContexts GetNoteContextsForRelatedNotesForTest()
		{
			return NoteContextsForRelatedNotes;
		}

		[ChildEditableTestExclude]
		public PackageCollection_PackageView Packages_PackageView_ForTest
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
	}
}



#endif
#endregion
