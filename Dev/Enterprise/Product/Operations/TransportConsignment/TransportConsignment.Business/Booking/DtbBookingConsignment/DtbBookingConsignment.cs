using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	[UniversalDataContext(DataContextType.TransportConsignment)]
	[UniversalCopyWithExtendedEntities]
	[UniversalCopyAssociateElement("Addresses", "DocAddresses")]
	[UniversalCopyElementsOrder("PackageJob", "Instructions")]
	[UserDefinedValues]
	public class DtbBookingConsignment : DtbTransport,
		IDtbBookingConsignment,
		ILocalClientJobHandler,
		IPackingParent,
		IPackingParentWithAutoPackageBreakdown,
		IPackingParentSupportsImportingBookedDimensions,
		IPackingParentOrphanScan,
		IProcessHandlingInfoProvider,
		IRelatableActivity,
		IRelatedJob,
		IHaveServicesWithContext,
		IConsignmentService,
		IDistanceCalculationConsumer,
		IPalletTransactionConfirmationProvider,
		ICustomFieldProvider
	{
		public const string TemplateCode = "LC2C";

		public DtbBookingConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoDtbBooking.Schema
		{
			public const string PostcodeDistance = "PostcodeDistance";
			public const string PackageSummary = "PackageSummary";
			public const string SpecialInstructionsExist = "SpecialInstructionsExist";
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			KM_DistanceUnit = DistanceCalculationRegistry.Instance.DefaultDistanceUnit.Value;
			KM_JobType = TransportConsolidationJobTypes.Codes.Consignment;
		}

		#endregion

		#region Related Entities

		#region Confirmations

		public DtbConsignmentConfirmationCollection AllConfirmations
		{
			get { return allConfirmations ?? (allConfirmations = new DtbConsignmentConfirmationCollection(Factory, new DtbConsignmentConfirmationRelationship(this))); }
		}

		DtbConsignmentConfirmationCollection allConfirmations;

		IEnumerable<DtbConsignmentConfirmation> PickupConfirmations
		{
			get
			{
				return Instructions.Where(i => i.IsPickUp).SelectMany(i => i.Confirmations).Where(c => c.IsPickUp);
			}
		}

		IEnumerable<DtbConsignmentConfirmation> DeliveryConfirmations
		{
			get
			{
				return Instructions.Where(i => i.IsDelivery).SelectMany(i => i.Confirmations).Where(c => c.IsDelivery);
			}
		}

		#endregion

		#region BookedByAddress

		// Used only for the data binding
		[ChildEditable]
		[ChildEditableTestExclude]
		public JobDocAddress BookedByAddress
		{
			get
			{
				if (bookedByAddress == null || bookedByAddress.IsDeleted)
				{
					bookedByAddress = ConsolidationSingleJob.BookedByAddress;
					RegisterEditableChildObject(bookedByAddress);
				}
				return bookedByAddress;
			}
		}
		JobDocAddress bookedByAddress;

		#endregion

		#region BookingTemplate

		protected override Type BookingTemplateType
		{
			get { return typeof(DtbConsignmentTmpl); }
		}

		#endregion

		#region ConsolidationSingleJob

		[ChildEditable]
		[ChildEditableTestExclude]
		public new DtbConsignmentConsolidation ConsolidationSingleJob
		{
			get { return (DtbConsignmentConsolidation)base.ConsolidationSingleJob; }
		}

		protected override Type ConsolidationType
		{
			get { return typeof(DtbConsignmentConsolidation); }
		}

		#endregion

		#region DeliveryInstruction

		/// <summary>
		/// Assumes the Consignment will only ever have one Delivery instruction.
		/// </summary>
		public DtbConsignmentInstruction DeliveryInstruction
		{
			get { return Instructions.SingleOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery); }
		}

		#endregion

		#region DepotInstruction

		/// <summary>
		/// Assumes the Consignment will only ever have maximun two Depot instructions.         
		/// </summary>
		public IEnumerable<DtbConsignmentInstruction> DepotInstructions
		{
			get { return Instructions.Where(i => i.IsOwnDepot); }
		}

		public DtbConsignmentInstruction PickupDepotInstruction
		{
			get
			{
				return Instructions.OrderBy(i => i.KN_Sequence).FirstOrDefault(i => i.IsDepot && i.IsMulti);
			}
		}

		public DtbConsignmentInstruction DeliveryDepotInstruction
		{
			get
			{
				return Instructions.OrderBy(i => i.KN_Sequence).LastOrDefault(i => i.IsDepot && i.IsMulti);
			}
		}

		public void UpdateDepotInstructions()
		{
			if (PickupInstruction != null)
			{
				CreateOrUpdateDepotInstruction(PickupInstruction);
			}

			if (DeliveryInstruction != null)
			{
				CreateOrUpdateDepotInstruction(DeliveryInstruction);
			}
		}

		/// <summary>
		/// Assumes the Consignment will only ever have one Pickup and Depot Instruction.
		/// Has an Address, and No Depots OR Only Depot doesn't match new address THEN Insert New Depot Instruction
		/// Both Depots are different THEN update related instruction address
		/// No Address or Both Depots are the same THEN delete related instruction (merge)
		/// </summary>
		/// <returns></returns>
		public void CreateOrUpdateDepotInstruction(DtbConsignmentInstruction instruction)
		{
			if (instruction.IsMulti)
			{
				throw new ArgumentException("Do not support Multi instructions.");
			}

			var otherInstruction = instruction.IsPickUp ? DeliveryInstruction : PickupInstruction;
			var pickupDepotInstruction = PickupDepotInstruction;
			var deliveryDepotInstruction = DeliveryDepotInstruction;
			var relatedDepotInstruction = instruction.IsPickUp ? pickupDepotInstruction : deliveryDepotInstruction;
			var otherDepotInstruction = instruction.IsPickUp ? deliveryDepotInstruction : pickupDepotInstruction;
			var calculatedDepotAddress = instruction.FindDepotAddress();
			var calculatedDepotAddressPK = calculatedDepotAddress != null ? calculatedDepotAddress.PK : ZGuid.Empty;

			if (!instruction.Address.E2_OA_Address.IsEmpty
				&& (relatedDepotInstruction == null || (relatedDepotInstruction == otherDepotInstruction && relatedDepotInstruction.Address.E2_OA_Address != calculatedDepotAddressPK)))
			{
				if (otherInstruction != null && (otherInstruction.IsDepot || otherInstruction.Address.E2_OA_Address.IsEmpty) && relatedDepotInstruction == otherDepotInstruction && relatedDepotInstruction != null)
				{
					relatedDepotInstruction.Address.E2_OA_Address = calculatedDepotAddressPK;
				}
				else
				{
					CreateDepotInstructionCore(instruction, calculatedDepotAddress);
				}
			}
			else if (relatedDepotInstruction != otherDepotInstruction)
			{
				if (!instruction.Address.E2_OA_Address.IsEmpty && otherDepotInstruction.Address.E2_OA_Address != calculatedDepotAddressPK)
				{
					relatedDepotInstruction.Address.E2_OA_Address = calculatedDepotAddressPK;
				}
				else
				{
					relatedDepotInstruction.Delete();
				}
			}
			else if (instruction.Address.E2_OA_Address.IsEmpty && relatedDepotInstruction == otherDepotInstruction) //Remove related depot instruction if instruction address is empty
			{
				if (relatedDepotInstruction != null && otherInstruction != null && otherInstruction.Address.E2_OA_Address.IsEmpty)      //Only remove depot instruction which created by the instruction
				{
					relatedDepotInstruction.Delete();
				}
			}

			if (allConfirmations != null)
			{
				allConfirmations.RefreshBinding();
			}
		}

		#region CreateDepotInstructionCore

		DtbConsignmentInstruction CreateDepotInstructionCore(DtbConsignmentInstruction instruction, OrgAddress newDepotAddress)
		{
			var depotSequence = instruction.KN_Sequence + (instruction.IsPickUp ? 1 : 0); // insert after pickup or before delivery (= same as delivery + bump delivery)
			var depotInstruction = AddNewInstructionAndSequence(depotSequence);
			depotInstruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			depotInstruction.OrganisationType = OrganisationTypesList.Codes.CFS;

			if (newDepotAddress != null)
			{
				depotInstruction.Address.E2_OA_Address = newDepotAddress.PK;
			}

			return depotInstruction;
		}

		#endregion

		#region AddNewInstructionAndSequence

		/// <summary>
		/// Adds a New Run Sheet Instruction to the RunSheetInstruction Collection.
		/// Pass in a Sequence to insert in that position, otherwise it will be added to the end.
		/// </summary>
		DtbConsignmentInstruction AddNewInstructionAndSequence(int sequence = 0)
		{
			var newInstruction = Instructions.AddNew();

			if (sequence != 0)
			{
				foreach (var instruction in Instructions)
				{
					if (instruction.KN_Sequence >= sequence)
					{
						instruction.KN_Sequence++;
					}
				}

				newInstruction.KN_Sequence = sequence;
			}

			return newInstruction;
		}

		#endregion

		#endregion

		#region InstructionPkgDivots

		[ChildEditable]
		[UniversalCopyCollectionEntity(DtbBookingInstructionSchema.Constants.TableName, DtbBookingInstructionSchema.Constants.KN_KM_BookingMovement)]
		public new DtbConsignmentInstructionCollection Instructions
		{
			get { return (DtbConsignmentInstructionCollection)base.Instructions; }
		}

		protected override IDtbTransportInstructionCollection GetNewInstructionsCollection()
		{
			return new DtbConsignmentInstructionCollection(this);
		}

		#endregion

		#region Outers

		[ChildEditable]
		public PkgPackageOutersCollection Outers
		{
			get
			{
				if (outers == null)
				{
					outers = new PkgPackageOutersCollection(this);
					RegisterEditableChildObject(outers);
				}

				return outers;
			}
		}

		PkgPackageOutersCollection outers;

		#endregion

		#region PackageDivots

		public new DtbConsignmentInstructionPkgDivotCollection PackageDivots
		{
			get { return (DtbConsignmentInstructionPkgDivotCollection)base.PackageDivots; }
		}

		protected override IDtbTransportInstructionPkgDivotCollection GetNewInstructionPkgDivotCollection()
		{
			return new DtbConsignmentInstructionPkgDivotCollection(this);
		}

		#endregion

		#region PackageJob

		[UniversalCopyRelatedEntity]
		public override PkgPackageJob PackageJob
		{
			get { return packageJob ?? (packageJob = GetPackageJob()); }
		}

		PkgPackageJob GetPackageJob()
		{
			var packageJobToReturn = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(this);
			packageJobToReturn.OuterPackageAdded += PackageJob_OuterPackageAdded;
			packageJobToReturn.OuterParentPackageChanged += PackageJob_OuterParentPackageChanged;
			packageJobToReturn.PackageDataChanged += PackageJob_PackageDataChanged;
			packageJobToReturn.Packages.CountChanged += PackageJob_PackageCountChanged;
			HookPackagesForAllExistingDivots();

			return packageJobToReturn;
		}

		#region HookPackagesForAllExistingDivots

		void HookPackagesForAllExistingDivots()
		{
			// new divots are auto-hooked in DtbConsignmentInstructionPkgDivot when packs are assigned, but existing divots
			// also need hooking (Note -- The hooking is for synchronisation b/w the Outers Grid and Packing Tree).
			foreach (var divot in PackageDivots)
			{
				divot.HookPackage();
			}
		}

		#endregion

		#region PackageJob_OuterPackageAdded

		void PackageJob_OuterPackageAdded(object sender, PackageEventArgs e)
		{
			if (!AssignAllPackagesSemaphore.IsSuspended)
			{
				// If PackageJob has Package then object is committed.
				if (Outers.Contains(e.Package))
				{
					AssignPackageToAllInstructions(e.Package);
				}
				else
				{
					EventHandler<HasChangesChangedEventArgs> hasChangesEventHandler = null;
					EventHandler countChangedHandler = null;

					// Has Changes is set to true when committed or when being deleted on a Grid. Unfortunately this
					// is not enough because has changes is not set when nodes are added through the tree.
					hasChangesEventHandler = delegate
					{
						AssignPackageIfNotDeleted(e.Package);
						e.Package.HasChangesChanged -= hasChangesEventHandler;
						Outers.CountChanged -= countChangedHandler;
					};

					e.Package.HasChangesChanged += hasChangesEventHandler;

					// Use Count Changed as a fallback if HasChanges does not pick up the change.
					countChangedHandler = delegate
					{
						var addedPackage = AssignPackageIfRowStateIsAdded(e.Package);
						Outers.CountChanged -= countChangedHandler;

						if (addedPackage)
						{
							e.Package.HasChangesChanged -= hasChangesEventHandler;
						}
					};

					Outers.CountChanged += countChangedHandler;
				}
			}
		}

		void AssignPackageIfNotDeleted(PkgPackage package)
		{
			if (!package.IsDeleted)
			{
				// Is has changes was set and the Package is not deleted, it had been committed.
				AssignPackageToAllInstructions(package);
			}
		}

		bool AssignPackageIfRowStateIsAdded(PkgPackage package)
		{
			var result = false;
			// if row is not committed do not do anything, HasChangesChanged will handle the rest.
			// Otherwise the add is being done through the tree and thus assign the package and unhook.
			if (((INeedRow)package).Row.RowState == DataRowState.Added)
			{
				AssignPackageToAllInstructions(package);
				result = true;
			}

			return result;
		}

		void AssignPackageToAllInstructions(PkgPackage package)
		{
			foreach (var instruction in Instructions)
			{
				instruction.DivotsWithPackages.AddPackage(package);
			}
		}

		Semaphore AssignAllPackagesSemaphore => assignAllPackagesSemaphore ?? (assignAllPackagesSemaphore = new Semaphore());
		Semaphore assignAllPackagesSemaphore;

		#endregion

		#region PackageJob_OuterParentPackageChanged

		void PackageJob_OuterParentPackageChanged(object sender, PackageEventArgs e)
		{
			UnassignPackageFromAllInstructions(e.Package);
		}

		void UnassignPackageFromAllInstructions(PkgPackage package)
		{
			foreach (var instruction in Instructions)
			{
				instruction.DivotsWithPackages.RemovePackage(package);
			}
		}

		#endregion

		#region PackageJobSummary

		void PackageJob_PackageDataChanged(object sender, PackageDataChangeEventArgs e)
		{
			if (SummaryPackageDataTypes.Contains(e.DataChangeType))
			{
				PackageSummaryInfo.RefreshBinding();
			}
		}

		void PackageJob_PackageCountChanged(object sender, EventArgs e)
		{
			PackageSummaryInfo.RefreshBinding();
		}

		readonly PackageDataChangeType[] SummaryPackageDataTypes = { PackageDataChangeType.PackageContent, PackageDataChangeType.Weight, PackageDataChangeType.Volume };

		#endregion

		PkgPackageJob packageJob;

		#endregion

		#region Packages_PackageView

		protected override PackageCollection_PackageView GetNewPackages_PackageView()
		{
			return new DtbConsignmentPackageCollection_PackageView(this);
		}

		#endregion

		#region PickupInstruction

		/// <summary>
		/// Assumes the Consignment will only ever have one Pickup instruction.
		/// </summary>
		public DtbConsignmentInstruction PickupInstruction
		{
			get
			{
				var pickups = Instructions.Where(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp).ToArray();

				if (pickups.Length != 1)
				{
					ErrorReporter.ReportOnce(Invariant($"Consignment contains {pickups.Length} Pickup Instructions."));
				}

				return pickups.FirstOrDefault();
			}
		}

		#endregion

		#region Pallet Transactions

		[ChildEditable]
		public PkgPalletTransactionConfirmationDependentCollection PalletTransactions
		{
			get
			{
				if (palletTransactions == null)
				{
					palletTransactions = new PkgPalletTransactionConfirmationDependentCollection(this);
					RegisterEditableChildObject(palletTransactions);
				}

				return palletTransactions;
			}
		}

		PkgPalletTransactionConfirmationDependentCollection palletTransactions;

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KM_JobID

		[ResourceStringData("DtbBookingConsignment|KM_JobID", ShortCaption = "Cons. ID", Caption = "Consignment ID")]
		public override ZString KM_JobID
		{
			get { return base.KM_JobID; }
			set { base.KM_JobID = value; }
		}

		#endregion

		#region KM_TransportReference

		[ResourceStringData("DtbBookingConsignment|KM_TransportReference", Caption = "Connote No.", ShortCaption = "Connote")]
		public override ZString KM_TransportReference
		{
			get { return base.KM_TransportReference; }
			set { base.KM_TransportReference = value; }
		}

		#endregion

		#region KM_DistanceUnit

		[List("Lookups.DistanceUnits")]
		public override ZString KM_DistanceUnit
		{
			get { return base.KM_DistanceUnit; }
			set
			{
				base.KM_DistanceUnit = value;
				PostcodeDistanceInfo.RefreshBinding();
			}
		}

		#endregion

		// calculated

		#region CompletedDate

		[ResourceStringData("DtbBookingConsignment|CompletedDate", ShortCaption = "Completed", Caption = "Completed Date")]
		public ZDateTime CompletedDate
		{
			get
			{
				var completeLog = GetLatestLog(Events.CartageCompleteFinalised);
				return (completeLog != null) ? completeLog.SL_EventTime : ZDateTime.Empty;
			}
		}

		#endregion

		#region BookingID

		[ResourceStringData("DtbBookingConsignment|BookingID", Caption = "Booking ID", ShortCaption = "Booking")]
		public ZString BookingID
		{
			get
			{
				var booking = ConsolidationSingleJob.Parent;
				return (booking != null) ? booking.KM_JobID : ZString.Empty;
			}
		}
		#endregion

		#region KM_Distance

		[ResourceStringData("DtbBookingConsignment|KM_Distance", ShortCaption = "Driving", Caption = "Driving Distance")]
		public override ZDecimal KM_Distance
		{
			get { return base.KM_Distance; }
			set { base.KM_Distance = value; }
		}

		#endregion

		#region PostcodeDistance

		[ResourceStringData("DtbBookingConsignment|PostcodeDistance", ShortCaption = "P/C Distance", Caption = "Postcode Distance", FullDescription = "The Calculated Distance between the Pickup Address Postcode and Delivery Address Postcode.")]
		public ZDecimal PostcodeDistance
		{
			get
			{
				var result = new ZDecimal(RefLatLongPostcode.CalculateDistance(PickupInstruction.Address, DeliveryInstruction.Address));
				if (KM_DistanceUnit != Constants.Length.Kilometres && Constants.Length.ContainsCode(KM_DistanceUnit))
				{
					result = Constants.Length.Convert(result, Constants.Length.Kilometres, KM_DistanceUnit);
				}

				return result;
			}
		}

		public ZPropertyInfo PostcodeDistanceInfo
		{
			get { return GetZPropertyInfo(Schema.PostcodeDistance); }
		}

		#endregion

		#region Package summary

		public ZString PackageSummary
		{
			get
			{
				var packageSummary = (IPackageSummary)PackageJob;
				var captionAndValueFormat = "{0}: {1}";

				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(packageSummary.Contents);
				builder.Append(ZString.Format(captionAndValueFormat, packageSummary.VolumeCaption, packageSummary.Volume));
				builder.Append(ZString.Format(captionAndValueFormat, packageSummary.WeightCaption, packageSummary.Weight));

				return ZString.Format("({0})", builder.ToStringWithDelimiterBetweenAppends(", "));
			}
		}

		public ZPropertyInfo PackageSummaryInfo
		{
			get { return GetZPropertyInfo(Schema.PackageSummary); }
		}

		#endregion

		#endregion

		#region Flags

		#region HasDelivery

		/// <summary>
		/// This property is only used in the Consignment Module Grid, it will only check whether a real address
		/// is set or if the Address has been overriden, to match the logic of the Module Filter BizO.
		/// </summary>
		[ResourceStringData("DtbBookingConsignment|HasDelivery", Caption = "Has Delivery", ShortCaption = "Has Del.")]
		public ZBool HasDelivery
		{
			get
			{
				var deliveryAddress = DeliveryInstruction.Address;
				return deliveryAddress.E2_AddressOverride || deliveryAddress.E2_OA_Address.IsValid;
			}
		}

		#endregion

		#region IsDeliveryAllocated

		public bool IsDeliveryAllocated
		{
			get { return KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.DeliveryAllocated); }
		}

		#endregion

		#region SpecialInstructionsExist

		[ResourceStringData("DtbBookingConsignment|SpecialInstructionsExist", Caption = "Special Instructions", MediumCaption = "Instructions", ShortCaption = "Sp. Instr.")]
		public ZBool SpecialInstructionsExist
		{
			get { return Instructions.Any(a => a.Confirmations.Any(b => b.Instruction.SpecialInstructionExists)); }
		}

		public ZPropertyInfo SpecialInstructionsExistInfo
		{
			get { return GetZPropertyInfo(Schema.SpecialInstructionsExist); }
		}

		#endregion

		#endregion

		#region Lookups

		public new DtbBookingConsignmentLookups_OLD Lookups
		{
			get { return (DtbBookingConsignmentLookups_OLD)base.Lookups; }
		}

		protected override DtbTransportLookups GetNewLookupsCore()
		{
			return new DtbBookingConsignmentLookups_OLD(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return KM_JobID.IsEmpty
					? Res.GetString("325e3d88-b815-4219-80b7-6a918e2e1210", "Transport Consignment")
					: Res.GetString("d1c5bf4b-17d9-41bd-adf9-d139095b24f6", "Transport Consignment {0}", KM_JobID);
			}
		}

		#endregion

		#region Validation

		public new DtbBookingConsignmentValidation_OLD Validation
		{
			get { return (DtbBookingConsignmentValidation_OLD)base.Validation; }
		}

		protected override DtbTransportValidation GetNewValidationCore()
		{
			return new DtbBookingConsignmentValidation_OLD(this);
		}

		#endregion

		#region UpdateStatus

		protected override ZString GetExpectedStatus()
		{
			ZString result;

			if (Instructions.Deliveries.Any() && Instructions.Deliveries.All(i => i.IsDelivered))
			{
				result = TransportStatuses.Codes.Delivered;
			}
			else if (Instructions.Deliveries.Any() && Instructions.Deliveries.All(i => i.IsDelivered || i.IsAllocated))
			{
				result = TransportStatuses.Codes.DeliveryAllocated;
			}
			else if (Instructions.PickUps.Any() && Instructions.PickUps.All(i => i.IsPickedUp))
			{
				result = TransportStatuses.Codes.PickedUp;
			}
			else if (Instructions.PickUps.Any() && Instructions.PickUps.All(i => i.IsPickedUp || i.IsAllocated))
			{
				result = TransportStatuses.Codes.PickUpAllocated;
			}
			else if (!KM_IsActive)
			{
				result = TransportStatuses.Codes.Deactivated;
			}
			else
			{
				result = TransportStatuses.Codes.Available;
			}

			return result;
		}

		#endregion

		#region SplitConsignment

		public DtbBookingConsignment SplitConsignment(PkgPackage[] packages, INotifications notifications)
		{
			DtbBookingConsignment result = null;

			if (packages.Any(p => p.KP_KJ_ParentPackageJob != PackageJob.PK))
			{
				throw new InvalidOperationException("Should not Split packages that are not on this Consignment.");
			}
			else if (HasChanges)
			{
				notifications.AddError(Res.GetString("30cb4162-9534-415a-8f52-a51004a957cf", "Save this Consignment before Splitting."));
			}
			else if (IsDeliveryAllocated || IsDelivered)
			{
				notifications.AddError(Res.GetString("737fb939-3afd-4649-b806-4ba6525ff6b0", "Cannot Split Consignments where its Delivery is Allocated or Completed."));
			}
			else if (packages.Length >= Outers.Count)
			{
				notifications.AddError(Res.GetString("7bbbb254-97e3-4d59-8b0c-93cf5e7952a9", "Cannot Split all Packages."));
			}
			else
			{
				var otherFactory = new BusinessObjectFactory();
				var consignmentInOtherFactory = otherFactory.Load<DtbBookingConsignment>(PK);
				var packagesInOtherFactory = otherFactory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, packages.Select(p => p.PK)));
				result = consignmentInOtherFactory.CreateSplitConsignment(packagesInOtherFactory);
			}

			return result;
		}

		#region CreateSplitConsignment

		DtbBookingConsignment CreateSplitConsignment(PkgPackage[] packages)
		{
			var clonedConsignment = (DtbBookingConsignment)Clone();
			clonedConsignment.ReplaceDeliveryInstructionWithEmpty();
			CopyLocalClient(clonedConsignment);
			SplitPackages(packages, clonedConsignment);

			SetIsHazardousAndRequiresRefrigerationBasedOnPacks();
			clonedConsignment.SetIsHazardousAndRequiresRefrigerationBasedOnPacks();

			AddLogForConsignmentSplit(clonedConsignment);

			return clonedConsignment;
		}

		void ReplaceDeliveryInstructionWithEmpty()
		{
			DeliveryInstruction.Delete();
			Instructions.AddNew(InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE);
		}

		#region CopyLocalClient

		void CopyLocalClient(DtbBookingConsignment clonedConsignment)
		{
			var job = Job;
			if (job != null)
			{
				var localClient = job.LocalChargesAddr;
				if (localClient != null)
				{
					clonedConsignment.BillingPartyAddress.E2_OA_Address = localClient.PK;
				}
			}
		}

		#endregion

		#region SplitPackages

		void SplitPackages(PkgPackage[] packages, DtbBookingConsignment clonedConsignment)
		{
			foreach (var package in packages)
			{
				Outers.RemoveFromRelationship(package);
				UnassignPackageFromAllInstructions(package);
				clonedConsignment.Outers.Add(package);
			}
		}

		#endregion

		#region SetIsHazardousAndRequiresRefrigerationBasedOnPacks

		void SetIsHazardousAndRequiresRefrigerationBasedOnPacks()
		{
			KM_IsHazardous = AssignedPackages.Any(p => p.UNDGs.Any());
			KM_RequiresRefrigeration = AssignedPackages.Any(p => p.IsTemperatureControlled);
		}

		#endregion

		#region AddLogForConsignmentSplit

		void AddLogForConsignmentSplit(DtbBookingConsignment clonedConsignment)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			clonedConsignment.Logs.AddNew(Events.EditedARecord, Invariant($"Split from Consignment {KM_JobID}"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		#endregion

		#endregion

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[]
			{
				DtbBookingSchema.Constants.KM_JobID,
				DtbBookingSchema.Constants.KM_IsHazardous,
				DtbBookingSchema.Constants.KM_RequiresRefrigeration,
				DtbBookingSchema.Constants.KM_KT_NKBookingTemplate,
				DtbBookingSchema.Constants.KM_TransportReference
			});

			var clone = (DtbBookingConsignment)base.CloneInternal(args);
			CloneInstructionsAndTemplate(clone);

			return clone;
		}

		void CloneInstructionsAndTemplate(DtbBookingConsignment clone)
		{
			using (new SemaphoreManager(clone.AddingInstructionsFromTemplateSemaphore))
			{
				clone.KM_KT_NKBookingTemplate = KM_KT_NKBookingTemplate;
			}

			foreach (var instruction in Instructions)
			{
				clone.Instructions.Add((DtbConsignmentInstruction)instruction.Clone());
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Delete

		protected override void DeleteCore()
		{
			base.DeleteCore();
			DeletePackageJob(); // tested by PackingParentTestCase
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
		}

		void DeletePackageJob()
		{
			var packageJobToDelete = PkgPackageJob.LoadPackageJob(this);
			if (packageJobToDelete != null)
			{
				packageJobToDelete.Delete();
			}
		}

		#endregion

		#region Save

		protected override void BeforeOnSaving()
		{
			base.BeforeOnSaving();

			if (KM_TransportReference.IsEmpty)
			{
				KM_TransportReference = KM_JobID;
			}
		}

		protected override void OnSaveFailedCore()
		{
			base.OnSaveFailedCore();

			if (!IsInDatabase && KM_TransportReference.EqualsIgnoringCase(KM_JobID))
			{
				KM_TransportReference = "";
			}
		}

		#region TransportNumberGenerator

		protected override NumberGeneratorTarget TransportNumberGeneratorTargetCore()
		{
			return new BookingConsignmentNumberGeneratorTarget(this);
		}

		#endregion

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingConsignmentFetchStrategy(this);
		}

		#endregion

		#region Unique Index Failure Handler

		protected override INumberFountainProxy NumberFountainForUniqueID
		{
			get { return Env.NumberFountains.DtbConsignmentID; }
		}

		#endregion

		// interfaces

		#region IAdditionalReferenceNumberTypeProvider Members

		protected override CodeDescriptionPairList AdditionalReferenceNumberTypes
		{
			get
			{
				var referenceTypes = new CodeDescriptionPairList();
				foreach (var regType in LandTransportRegistry.Instance.LandTransportConsignmentAdditionalReferenceNumbers.Value)
				{
					referenceTypes.Add(regType);
				}

				return referenceTypes;
			}
		}

		#endregion

		#region IConsignmentService Members

		void IConsignmentService.LogServicesCommenced()
		{
			var eventParams = new List<KeyValuePair<string, string>>();
			eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Transport));

			var dlvInstruction = DeliveryInstruction;
			if (dlvInstruction != null)
			{
				eventParams.AddRange(dlvInstruction.GetInstructionEventParameters());
			}

			Logs.CreateRecreateOrUpdateEventLog(AutoEvents.ServiceCommenced, EstimateActual.Actual, ZDateTimeOffset.Now, KM_JobID, eventParams.ToArray());
		}

		#endregion

		#region IDistanceCalculationConsumer Members

		SecurityCheckpoint IDistanceCalculationConsumer.Checkpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLandTransport; }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.DestinationAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(DeliveryInstruction.Address); }
		}

		ZDecimal IDistanceCalculationConsumer.Distance
		{
			get { return KM_Distance; }
			set { KM_Distance = value; }
		}

		DistanceCalculationConfiguration IDistanceCalculationConsumer.DistanceCalculationConfig
		{
			get { return DistanceCalculationHelper.GetConfigurationFromJobDocAddress(ConsolidationSingleJob.BookedByAddress, null); }
		}

		ZString IDistanceCalculationConsumer.DistanceUnit
		{
			get { return KM_DistanceUnit; }
			set { KM_DistanceUnit = value; }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.OriginAddress
		{
			get { return DistanceCalculationHelper.GetAddressFromAddress(PickupInstruction.Address); }
		}

		public void SetCalculatedDistance(INotifications notifications)
		{
			new FreightDistanceCalculator(this, notifications).SetCalculatedDistance();
		}

		#endregion

		#region IDocAddresses Members

		#region GetCanOverrideCheckpoint

		protected override SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		#endregion

		#region AdditionalSupportedAddressTypes

		protected override DocAddressType[] AdditionalSupportedAddressTypes
		{
			get
			{
				return new[]
				{
					DocAddressType.NotifyParty,
					DocAddressType.FinalConsigneeAddress,
					DocAddressType.OriginatingConsignorAddress
				};
			}
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo GetDocManagerInfo()
		{
			return new DtbBookingConsignmentDocManagerInfo(this);
		}

		#endregion

		#region IDocumentSupportable Members

		protected override DocumentSupporter GetDocumentSupporter()
		{
			return documentSupporter ?? (documentSupporter = new DtbBookingConsignmentDocumentSupporter(this));
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IHaveServicesWithContext Members

		ZString IHaveServices.ContainerMode
		{
			get { return ""; }
		}

		IHaveServices[] IHaveServices.DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		BusinessObject IHaveServices.ServiceParent
		{
			get { return this; }
		}

		[ChildEditable()]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new BookingConsignmentJobServiceDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
				}

				return services;
			}
		}
		BookingConsignmentJobServiceDependentCollection services;

		ZString IHaveServices.TableCode
		{
			get { return TablePrefix; }
		}

		ZString IHaveServices.TransportMode
		{
			get { return ""; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return false; }
		}

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Factory.Load<GlbBranch>(KM_GB_Branch);

		#region ServiceCurrentContextList

		CodeDescriptionPairList IHaveServicesWithContext.ServiceCurrentContextList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(this.PK, ServiceContextTypes.Code.Consignment, ServiceContextTypes.Description.Consignment);
				foreach (var instruction in this.Instructions)
				{
					if (instruction.IsPickUp && !instruction.IsDepot)
					{
						result.AddPair(instruction.PK, ServiceContextTypes.Code.PickupInstruction, ServiceContextTypes.Description.PickupInstruction);
					}
					else if (instruction.IsDelivery && !instruction.IsDepot)
					{
						result.AddPair(instruction.PK, ServiceContextTypes.Code.DeliveryInstruction, ServiceContextTypes.Description.DeliveryInstruction);
					}
				}

				return result;
			}
		}

		static class ServiceContextTypes
		{
			public static class Code
			{
				public const string Consignment = "CSN";
				public const string PickupInstruction = "PIC";
				public const string DeliveryInstruction = "DLV";
			}

			public static class Description
			{
				public static MultilingualString Consignment { get { return ResString.GetMultilingualString("10BD97E5-3CB9-4E82-A3F8-5BD65AE4AEB3", "Consignment"); } }
				public static MultilingualString PickupInstruction { get { return ResString.GetMultilingualString("7A104475-A6F0-457E-B61D-B87A297DF100", "Pick up instruction"); } }
				public static MultilingualString DeliveryInstruction { get { return ResString.GetMultilingualString("362299D1-107C-40DF-8441-B3A339139AA2", "Delivery instruction"); } }
			}
		}

		#endregion

		#region GetContextTableCode

		ZString IHaveServicesWithContext.GetContextTableCode(ZGuid contextID)
		{
			return (contextID.IsEmpty || contextID == this.PK) ? "" : DtbBookingInstructionSchema.Constants.Prefix;
		}

		#endregion

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override IJobInvoicingPlugIn GetNewInvoicingPlugIn()
		{
			return new DtbBookingConsignmentInvoicingPlugIn(this);
		}

		protected override void OnJobCreatingCore(JobHeader job)
		{
			base.OnJobCreatingCore(job);

			if (job != null && !job.IsDeleting)
			{
				if (ConsolidationSingleJob != null && ConsolidationSingleJob.Parent != null)
				{
					var parentJob = ((IDtbTransport)ConsolidationSingleJob.Parent).Job;

					if (parentJob != null)
					{
						job.JH_JH_ParentJob = parentJob.PK;
					}
				}
			}
		}

		#endregion

		#region ILocalClientJobHandler Memebers

		public void CreateJobHeaderWithMutex()
		{
			if (jobHeader == null || jobHeader.IsDeleted)
			{
				jobHeader = new JobHeader.Loader(this).TryLoadOrCreateWithMutex();
				if (jobHeader != null)
				{
					RegisterEditableChildObject(jobHeader);
				}
			}
		}

		public JobHeader JobHeader
		{
			get
			{
				if (jobHeader == null || jobHeader.IsDeleted)
				{
					jobHeader = new JobHeader.Loader(this).Load(true, false);
					if (jobHeader != null)
					{
						RegisterEditableChildObject(jobHeader);
					}
				}
				return jobHeader;
			}
		}

		JobHeader jobHeader;

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return false; }
		}

		#endregion

		#region IPackingParent Members

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			return new PackageActionStrategy(package);
		}

		ControllerID IPackingParent.ControllerID
		{
			get { return ControllerIDs.DtbBookingConsignment; }
		}

		DocumentOptions IPackingParent.DocumentOptions
		{
			get { return DocumentOptions.None; }
		}

		ZString IPackingParent.JobNo
		{
			get { return KM_JobID; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return KM_TransportReference; }
		}

		ZString IPackingParent.JobDescription
		{
			get { return Res.GetString("fa1762fd-fb4f-4e13-bd1a-981dd463afb9", "Consignment"); }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			return "";
		}

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob pkgJob)
		{
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
			UnassignPackageFromAllInstructions(package);
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		bool IPackingParent.IsParentJobFinalised
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		bool IPackingParent.IsPackingJobReadOnly
		{
			get { return false; }
		}

		bool IPackingParent.IsAutoPrintAllowed
		{
			get { return false; }
		}

		bool IPackingParent.IsScanEventsVisible
		{
			get { return true; }
		}

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package)
		{
			return "";
		}

		OrgHeader IPackingParent.CarrierBookingAgent => null;

		ZString IPackingParent.TransportReference { get => ZString.Empty; set { } }

		OrgHeader IPackingParent.GetCarrier(PkgPackage package)
		{
			return null;
		}

		bool IPackingParent.IsLoosePackageIDsSupported
		{
			get { return true; }
		}

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
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

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region IPackingParentOrphanScan

		OrphanScanJobType IPackingParentOrphanScan.JobType
		{
			get { return OrphanScanJobType.CSN; }
		}

		#endregion

		#region IPackingParentWithAutoPackageBreakdown

		IDisposable IPackingParentWithAutoPackageBreakdown.SuspendWhileAddingAutoCreatedPackages()
		{
			return new SemaphoreManager(AssignAllPackagesSemaphore);
		}

		void IPackingParentWithAutoPackageBreakdown.RunAfterAllAutoCreatedPackagesAreAdded(IEnumerable<PkgPackage> packages)
		{
			if (!AssignAllPackagesSemaphore.IsSuspended)
			{
				foreach (var instruction in Instructions)
				{
					instruction.DivotsWithPackages.AddPackages(packages);
				}
			}
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new DtbBookingConsignmentProcessHandlingInfo(this); }
		}

		#endregion

		#region IRatingSupporter Members

		protected override RatingAdaptersProvider GetRatingAdaptersProvider()
		{
			return new DtbBookingConsignmentRatingAdaptersProvider(this);
		}

		#endregion

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.TransportConsignments; }
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

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		#region Summary

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { LocalClientCode, KM_TransportReference, ResString.GetMultilingualString("5e2ea453-37b7-4759-845c-0b4f4f094f1c", "{0}:{1} > {2}:{3}", ConfirmationTypes.Codes.PickUp, PickupLocation, ConfirmationTypes.Codes.Delivery, DeliverLocation) }.Where(x => !x.IsEmpty)); }
		}

		ZString LocalClientCode
		{
			get
			{
				var localClientPK = LocalClient;
				var localClient = !localClientPK.IsEmpty ? Factory.Load<OrgHeader>(localClientPK) : null;
				return localClient != null ? localClient.OH_Code : ZString.Empty;
			}
		}

		ZString PickupLocation
		{
			get
			{
				var pickupInstruction = Instructions.SingleOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp);
				if (pickupInstruction == null)
				{
					return NoLocationString;
				}

				var pickupAddress = pickupInstruction.Address;
				if (!pickupAddress.E2_AddressOverride && !pickupAddress.HasRealOrganisation)
				{
					return NoLocationString;
				}

				var pickupLocation = " " + pickupAddress.E2_City;
				if (!pickupAddress.E2_AddressOverride)
				{
					var org = pickupAddress.Organisation;
					if (org != null)
					{
						pickupLocation += ZString.Format(" ({0})", org.OH_Code);
					}
				}

				return pickupLocation;
			}
		}

		ZString DeliverLocation
		{
			get
			{
				var deliveryInstruction = Instructions.SingleOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery);
				if (deliveryInstruction == null)
				{
					return NoLocationString;
				}

				var deliveryAddress = deliveryInstruction.Address;
				if (!deliveryAddress.E2_AddressOverride && !deliveryAddress.HasRealOrganisation)
				{
					return NoLocationString;
				}

				var deliveryLocation = " " + deliveryAddress.E2_City;
				if (!deliveryAddress.E2_AddressOverride)
				{
					var org = deliveryAddress.Organisation;
					if (org != null)
					{
						deliveryLocation += ZString.Format(" ({0})", org.OH_Code);
					}
				}

				return deliveryLocation;
			}
		}

		ZString NoLocationString
		{
			get { return ResString.GetMultilingualString("189CD1BD-8C70-406D-A91E-9702E24CA800", "N/A"); }
		}

		#endregion

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

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber
		{
			get { return KM_JobID; }
		}

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return KM_Status; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.DtbBookingConsignment; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region IWorkflowProvider Members

		protected override ProcessTaskCollection GetNewProcessTaskCollection()
		{
			return new DtbBookingConsignmentProcessTaskCollection(this);
		}

		#endregion

		#region IWorkflowProviderCore Members

		protected override ZString WorkflowTypeCore
		{
			get { return WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode; }
		}

		#endregion

		#region IPalletTransactionConfirmationProvider Members

		IEnumerable<DtbConsignmentConfirmation> IPalletTransactionConfirmationProvider.Confirmations
		{
			get { return AllConfirmations.Where(x => x.Instruction.OrganisationType == OrganisationTypesList.Codes.CNE || x.Instruction.OrganisationType == OrganisationTypesList.Codes.CNR); }
		}

		#endregion

		#region ICustomFieldProvider Members

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
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
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;

		#endregion

		#region IDtbTransport Members

		public ZGuid JobHeaderPK
		{
			get { return Job != null ? Job.PK : ZGuid.Empty; }
		}

		#endregion

		#region UpdateConfirmationEstimateTime

		internal void UpdateConfirmationsEstimateTime()
		{
			foreach (var confirmation in PickupConfirmations)
			{
				confirmation.UpdatePickupEstimatedTime();
			}
		}

		internal void UpdateDeliveryConfirmationEstimateTime()
		{
			var pickupConfirmations = PickupConfirmations;
			var pickupConfirmationWithLatestEstimated = pickupConfirmations.Any(c => c.KK_Estimated.IsEmpty) ? null : pickupConfirmations.OrderByDescending(c => c.KK_Estimated).FirstOrDefault();
			var latestPickupEstimated = pickupConfirmationWithLatestEstimated != null ? pickupConfirmationWithLatestEstimated.KK_Estimated : ZDateTime.Empty;
			var latestPickupZonePK = pickupConfirmationWithLatestEstimated != null ? pickupConfirmationWithLatestEstimated.Instruction.KN_TZ_DomesticZone : ZGuid.Empty;

			foreach (var deliveryConfirmation in DeliveryConfirmations)
			{
				var refTransitTime = latestPickupEstimated.IsValid ? GetServiceLevelTransitHours(latestPickupZonePK, deliveryConfirmation.Instruction.KN_TZ_DomesticZone) : null;
				if (refTransitTime != null)
				{
					var etd = latestPickupEstimated.AddHours(refTransitTime.RTT_TransitHours);
					deliveryConfirmation.KK_Estimated = TransportWorkingDays.GetWorkingDate(Factory, etd, GetDepartmentPK());
				}
				else
				{
					deliveryConfirmation.KK_Estimated = ZDateTime.Empty;
				}
			}
		}

		ZGuid GetDepartmentPK()
		{
			var result = GlbDepartment.CurrentDepartment.PK;

			var job = Job;
			if (job != null)
			{
				var department = job.Department;
				if (department != null)
				{
					result = department.PK;
				}
			}

			return result;
		}

		#endregion

		//

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			this.KM_KT_NKBookingTemplate = TemplateCode;
			this.KM_KB_Booking = Factory.New<DtbConsignmentConsolidation>().PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion

	}
}
