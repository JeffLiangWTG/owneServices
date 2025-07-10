using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[DependentBusinessObject(typeof(DtbConsignment), "Addresses")]
	public class DtbConsignmentAddress : AutoDtbConsignmentAddress, IDocAddresses, IConsignmentAddress, IDtbConsignmentAddress
	{
		public DtbConsignmentAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoDtbConsignmentAddress.Schema
		{
			public const string OrganisationType = "OrganisationType";
			public const int OrganisationTypeMaxLength = 3;
		}

		#endregion

		#region Delete

		public sealed override void Delete()
		{
			Actions.DeleteAll();
			DocAddresses.RemoveAndDeleteAll();

			base.Delete();
		}

		#endregion

		#region Actions

		public DtbConsignmentActionCollection Actions
		{
			get { return new DtbConsignmentActionCollection(this); }
		}

		#endregion

		#region Consignment

		public DtbConsignment Consignment
		{
			get { return Factory.Load<DtbConsignment>(LTS_LTC_Consignment); }
		}

		#endregion

		#region PickupAction

		public DtbConsignmentAction PickupAction
		{
			get { return Actions.Single(c => c.IsPickUp); }
		}

		#endregion

		#region DeliveryAction

		public DtbConsignmentAction DeliveryAction
		{
			get { return Actions.Single(c => c.IsDelivery); }
		}

		#endregion

		#region DefaultActions

		public IEnumerable<DtbConsignmentAction> DefaultActions
		{
			get
			{
				var defaultActions = Actions.Cast<DtbConsignmentAction>();
				return IsPickUp ? defaultActions.Where(c => c.IsPickUp) : defaultActions.Where(c => c.IsDelivery);
			}
		}

		#endregion

		#region ReqFrom

		public ZDateTimeOffset ReqFrom
		{
			get
			{
				var lastAction = DefaultActions.OrderBy(c => c.LTA_RequiredFrom).LastOrDefault();
				return lastAction != null ? lastAction.LTA_RequiredFrom : ZDateTimeOffset.Empty;
			}
			set
			{
				DefaultActions.ForEach(c => c.LTA_RequiredFrom = value);
			}
		}

		#endregion

		#region ReqTo

		public ZDateTimeOffset ReqTo
		{
			get
			{
				var lastAction = DefaultActions.OrderBy(c => c.LTA_RequiredTo).LastOrDefault();
				return lastAction != null ? lastAction.LTA_RequiredTo : ZDateTimeOffset.Empty;
			}
			set
			{
				DefaultActions.ForEach(c => c.LTA_RequiredTo = value);
			}
		}

		#endregion

		#region Flags

		#region IsPickUp

		public bool IsPickUp
		{
			get { return LTS_InstructionType.EqualsIgnoringCase(ConsignmentAddressTypes.Codes.PickUp); }
		}

		#endregion

		#region IsDelivery

		public bool IsDelivery
		{
			get { return LTS_InstructionType.EqualsIgnoringCase(ConsignmentAddressTypes.Codes.Delivery); }
		}

		#endregion

		#region IsDepot

		public bool IsDepot
		{
			get { return OrganisationType == OrganisationTypesList.Codes.CFS; }
		}

		#endregion

		#region IsMulti

		public bool IsMulti
		{
			get { return LTS_InstructionType.EqualsIgnoringCase(InstructionTypes.Codes.Multi); }
		}

		#endregion

		#endregion

		#region Address

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "E2_ParentID", DisableCopyMethodLink = true)]
		public JobDocAddress Address
		{
			get
			{
				if (address == null || address.IsDeleted)
				{
					var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(OrganisationType);
					var requirement = (((IDocAddresses)this).GetDocAddressRequirement(docAddressType));
					address = DocAddresses.FindOrCreateWithRequirement(requirement);
					address.MakePersistentEvenIfEmpty();
				}

				return address;
			}
		}

		JobDocAddress address;

		ZString ExistingDocAddressOrganisationType
		{
			get { return DocAddresses.Count > 0 ? CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(DocAddresses[0].DocAddressType) : ZString.Empty; }
		}

		[ResourceStringData("DtbConsignmentAddress|OrganisationType", ShortCaption = "Org. Type", Caption = "Organization Type")]
		public ZString OrganisationType
		{
			get { return organisationType.IsEmpty ? (organisationType = ExistingDocAddressOrganisationType) : organisationType; }
		}

		ZString organisationType;

		#endregion

		#region IConsignmentAddress

		AutoDtbBooking IConsignmentAddress.Booking => Booking;

		public AutoDtbBooking Booking
		{
			get
			{
				var bookingJobId = Consignment?.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportCommonAdditionalReferenceTypes.Codes.BookingJobId)?.CE_EntryNum;
				return bookingJobId.HasValue ? (AutoDtbBooking)Factory.LoadTop1<IDtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, bookingJobId)) : null;
			}
		}

		public ZString DropMode
		{
			get { return LTS_DropMode; }
		}

		public ZString Status
		{
			get { return LTS_Status; }
		}

		public ZString ServiceInstruction
		{
			get { return LTS_Notes; }
		}

		public ZString ConsignmentAddressType
		{
			get { return LTS_InstructionType; }
		}

		public RefEquipment Equipment
		{
			get { return !LTS_RQ_RequiredEquipment.IsEmpty ? Factory.Load<RefEquipment>(LTS_RQ_RequiredEquipment) : null; }
		}

		public ZBool IsOwnDepot
		{
			get { return IsMulti && IsDepot; }
		}

		public IEnumerable<PkgPackage> GetPackages
		{
			get { return Consignment.PackageJob.Packages; }
		}

		#region UpdateStatus

		public void UpdateStatus()
		{
			if (!IsDeleted)
			{
				var result = TransportStatuses.Codes.Available;

				var expectedStatus = GetExpectedStatus();
				if (!expectedStatus.IsEmpty)
				{
					result = expectedStatus;
				}

				LTS_Status = result;
			}
		}

		protected ZString GetExpectedStatus()
		{
			ZString result = ZString.Empty;

			if (Actions.Any())
			{
				result = GetExpectedStatusBasedOnAction();
			}

			return result;
		}

		ZString GetExpectedStatusBasedOnAction()
		{
			ZString result;

			if (IsMulti)
			{
				result = GetExpectedMultiStatus();
			}
			else
			{
				result = GetExpectedPickupOrDeliveryStatus();
			}

			return result;
		}

		ZString GetExpectedPickupOrDeliveryStatus()
		{
			ZString result = ZString.Empty;

			foreach (var action in Actions)
			{
				var runSheetInstruction = action.RunSheetInstruction;
				if (runSheetInstruction == null)
				{
					result = TransportStatuses.Codes.Available;
					break;
				}

				if (runSheetInstruction.K1_TimeIn.IsEmpty && runSheetInstruction.K1_TimeOut.IsEmpty)
				{
					result = TransportStatuses.Codes.Allocated;
				}
			}

			if (result.IsEmpty)
			{
				result = LTS_InstructionType == InstructionTypes.Codes.PickUp
					? TransportStatuses.Codes.PickedUp
					: TransportStatuses.Codes.Delivered;
			}

			return result;
		}

		ZString GetExpectedMultiStatus()
		{
			ZString result = ZString.Empty;

			var pickUpAction = Actions.SingleOrDefault(c => c.IsPickUp);
			if (pickUpAction != null)
			{
				result = GetExpectedStatus(pickUpAction, TransportStatuses.Codes.PickUpAllocated, TransportStatuses.Codes.PickedUp);
			}

			if (result.IsEmpty)
			{
				var deliveryAction = Actions.SingleOrDefault(c => c.IsDelivery);
				if (deliveryAction != null)
				{
					result = GetExpectedStatus(deliveryAction, TransportStatuses.Codes.DeliveryAllocated, TransportStatuses.Codes.Delivered);
				}
			}

			return result;
		}

		static ZString GetExpectedStatus(DtbConsignmentAction action, string allocatedStatus, string completedStatus)
		{
			ZString result = ZString.Empty;

			var runSheetInstruction = action.RunSheetInstruction;
			if (runSheetInstruction != null)
			{
				result = runSheetInstruction.K1_TimeIn.IsEmpty && runSheetInstruction.K1_TimeOut.IsEmpty
					? allocatedStatus
					: completedStatus;
			}

			return result;
		}

		#endregion

		#endregion

		#region IDocAddresses Members

		#region DocAddresses

		[UniversalCopyCollectionEntity(JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.Constants.E2_ParentID)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region SupportedAddressTypes

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.LocalCartageCFS,
					DocAddressType.LocalCartageCTO,
					DocAddressType.LocalCartageExporter,
					DocAddressType.LocalCartageImporter,
					DocAddressType.LocalCartageYard,
					DocAddressType.LocalCartageService,
					DocAddressType.LocalCartageMSC,
					DocAddressType.LocalCartageWarehouse,
					DocAddressType.None,
				};
			}
		}

		#endregion

		#region Events

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			SetDropMode(docAddress);
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		#endregion

		#region DocAddressChanged Functions

		void SetDropMode(JobDocAddress docAddress)
		{
			if (docAddress.E2_OA_Address.IsValid)
			{
				LTS_DropMode = docAddress.Address.OA_LCLEquipmentNeeded;
			}
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
			return Env.Security.None;
		}

		#endregion

		#region GetDocAddressRequirement

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return new JobDocAddressRequirement(addressType, OrganisationAddressType, ContactType.LocalTransport, true);
		}

		protected AddressType OrganisationAddressType
		{
			get
			{
				var result = AddressType.NoDefault;

				switch (LTS_InstructionType)
				{
					case ConsignmentAddressTypes.Codes.PickUp:
						result = AddressType.PIC;
						break;

					case ConsignmentAddressTypes.Codes.Delivery:
						result = AddressType.DLV;
						break;
				}

				return result;
			}
		}

		#endregion

		#region PiggyBackedDocAddressValidation

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		#endregion

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.LocalCartageCFS:
					return Lookups.CFSOrganisations;
				case DocAddressType.LocalCartageCTO:
					return Lookups.CTOOrganisations;
				case DocAddressType.LocalCartageYard:
					return Lookups.ContainerYardOrganisations;
				case DocAddressType.LocalCartageImporter:
					return Lookups.ConsigneeOrganisations;
				case DocAddressType.LocalCartageExporter:
					return Lookups.ConsignorOrganisations;

				default:
					return Lookups.AllOrganisations;
			}
		}

		#endregion

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			this.LTS_Sequence = 1;
			this.LTS_InstructionType = "PIC";

			if (LTS_LTC_Consignment.IsEmpty)
			{
				var helper = new Testing.TransportConsignmentTestHelper(Factory);
				var consignment = helper.CreateConsignment();
				LTS_LTC_Consignment = consignment.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
