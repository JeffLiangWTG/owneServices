using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[UniversalCopyWithExtendedEntities]
	public class DtbConsignmentInstruction : DtbTransportInstruction, IDocAddressesCaption
	{
		public DtbConsignmentInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : DtbTransportInstruction.Schema
		{
			public const string Estimated = "Estimated";
		}

		#endregion

		ZString IDocAddressesCaption.GetAddressCaption(JobDocAddress docAddress)
		{
			ZString result;
			switch (docAddress.DocAddressType)
			{
				case DocAddressType.LocalCartageExporter:
					result = Res.GetString("B8F78C66-AB24-4118-B45B-21C1CCDD38A4", "Pickup Organization");
					break;
				case DocAddressType.LocalCartageImporter:
					result = Res.GetString("52404005-BFFA-491F-B74E-4D63E5FFC67E", "Delivery Organization");
					break;
				default:
					result = "";
					break;
			}

			return result;
		}

		#region Related Entities

		#region Booking

		public new DtbBookingConsignment Booking
		{
			get { return (DtbBookingConsignment)base.Booking; }
		}

		protected override Type TransportType
		{
			get { return typeof(DtbBookingConsignment); }
		}

		#endregion

		#region Confirmations

		[ChildEditable]
		[UniversalCopyCollectionEntity(DtbBookingConfirmationSchema.Constants.TableName, DtbBookingConfirmationSchema.Constants.KK_KN_BookingInstruction)]
		public new DtbConsignmentConfirmationCollection Confirmations
		{
			get { return (DtbConsignmentConfirmationCollection)base.Confirmations; }
		}

		protected override IDtbTransportConfirmationCollection GetNewConfirmationsCollection()
		{
			return new DtbConsignmentConfirmationCollection(this);
		}

		#endregion

		#region DeliveryConfirmation

		public DtbConsignmentConfirmation DeliveryConfirmation
		{
			get { return Confirmations.Single(c => c.IsDelivery); }
		}

		#endregion

		#region PackageJob

		protected override PkgPackageJob PackageJob
		{
			get { return Booking.PackageJob; }
		}

		#endregion

		#region Packages

		protected override IDivotsWithPackagesCollection GetNewDivotsWithPackagesCollection()
		{
			return new DivotsWithPackagesCollection<DtbConsignmentInstructionPkgDivot>(this);
		}

		#endregion

		#region PackageDivots

		[ChildEditable]
		[UniversalCopyCollectionEntity(DtbBookingInstructionPkgDivotSchema.Constants.TableName, DtbBookingInstructionPkgDivotSchema.Constants.KD_KN_BookingInstruction)]
		public new DtbConsignmentInstructionPkgDivotCollection PackageDivots
		{
			get { return (DtbConsignmentInstructionPkgDivotCollection)base.PackageDivots; }
		}

		protected override IDtbTransportInstructionPkgDivotCollection GetNewInstructionPkgDivotsCollection()
		{
			return new DtbConsignmentInstructionPkgDivotCollection(this);
		}

		#endregion

		#region PickupConfirmation

		public DtbConsignmentConfirmation PickupConfirmation
		{
			get { return Confirmations.Single(c => c.IsPickUp); }
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KN_KM_BookingMovement

		public override ZGuid KN_KM_BookingMovement
		{
			get { return base.KN_KM_BookingMovement; }
			set
			{
				var previousValue = KN_KM_BookingMovement;
				base.KN_KM_BookingMovement = value;

				if (previousValue != KN_KM_BookingMovement)
				{
					AssignAllPacks();
				}
			}
		}

		void AssignAllPacks()
		{
			var booking = Booking;
			if (booking != null)
			{
				DivotsWithPackages.AddPackages(booking.PackageJob.Packages);
			}
		}

		#endregion

		#region KN_InstructionType

		[ReadOnly(true)]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString KN_InstructionType
		{
			get { return base.KN_InstructionType; }
			set
			{
				base.KN_InstructionType = value;
				AutoCreateConfirmationIfNecessary();
			}
		}

		void AutoCreateConfirmationIfNecessary()
		{
			var consignment = Booking;
			if (consignment != null)
			{
				if ((IsMulti || IsPickUp) && !Confirmations.HasPickup)
				{
					Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
				}
				if ((IsMulti || IsDelivery) && !Confirmations.HasDelivery)
				{
					Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
				}
			}
		}

		#endregion

		// calculated

		#region ReqFrom

		[ResourceStringData("DtbConsignmentInstruction|ReqFrom", Caption = "Requested From", ShortCaption = "Req. From")]
		public override ZDateTime ReqFrom
		{
			get { return base.ReqFrom; }
			set
			{
				base.ReqFrom = value;

				// tested in DtbConsignmentInstructionValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateReqFrom();
				}
			}
		}

		#endregion

		#region ReqTo

		[ResourceStringData("DtbConsignmentInstruction|ReqTo", Caption = "Requested To", ShortCaption = "Req. To")]
		public override ZDateTime ReqTo
		{
			get { return base.ReqTo; }
			set
			{
				base.ReqTo = value;

				// tested in DtbConsignmentInstructionValidation
				if (!IsValidationSuspended)
				{
					Validation.ValidateReqTo();
				}
			}
		}

		#endregion

		#region ZoneDescription

		public ZString ZoneDescription
		{
			get
			{
				var zone = Zone;
				return (zone != null) ? Res.GetString("a1a2e294-c6ed-4097-a539-1c538600a005", "(Zone {0})", zone.TZ_ZoneName) : "";
			}
		}

		#endregion

		#region Facility

		ZString FacilityCode
		{
			get
			{
				var result = "";

				if (IsDepot)
				{
					result = CargoWise.EventReference.Constants.Facilities.Code.Depot;
				}
				else if (IsPickUp)
				{
					result = CargoWise.EventReference.Constants.Facilities.Code.Consignor;
				}
				else if (IsDelivery)
				{
					result = CargoWise.EventReference.Constants.Facilities.Code.Consignee;
				}

				return result;
			}
		}

		#endregion

		// panel view

		#region Estimated

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbConsignmentInstruction|Estimated", Caption = "Estimated")]
		public ZDateTime Estimated
		{
			get
			{
				var lastConfirm = DefaultConfirmations.OrderBy(c => c.KK_Estimated).LastOrDefault();
				return lastConfirm != null ? lastConfirm.KK_Estimated : ZDateTime.Empty;
			}
			set
			{
				GetAndCreateDefaultConfirmationsIfNotExists.ForEach(c => c.KK_Estimated = value);
				EstimatedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo<ZDateTime> EstimatedInfo
		{
			get { return (ZPropertyInfo<ZDateTime>)GetZPropertyInfo(Schema.Estimated); }
		}

		#endregion

		#endregion

		#region Flags

		#region IsPickedUp

		public bool IsPickedUp
		{
			get { return IsPickUp && KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.PickedUp); }
		}

		#endregion

		#region IsDelivered

		public bool IsDelivered
		{
			get { return IsDelivery && KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.Delivered); }
		}

		#endregion

		#region IsAllocated

		public bool IsAllocated
		{
			get { return KN_Status.EqualsIgnoringCase(TransportStatuses.Codes.Allocated); }
		}

		#endregion

		#region SpecialInstructionExists

		public bool SpecialInstructionExists
		{
			get { return !KN_ServiceInstruction.IsEmpty; }
		}

		#endregion

		#region ServiceExists

		public ZBool ServiceExists
		{
			get { return this.Booking.Services.Cast<JobService>().Any(s => s.ES_CurrentContextID.IsEmpty || s.ES_CurrentContextID == PK); }
		}

		#endregion

		#region MarkRequiresSignatureEventLog

		public void MarkRequiresSignatureEventLog()
		{
			RequiresSignatureEventLog = true;
		}
		bool RequiresSignatureEventLog;

		#endregion

		#endregion

		#region Save

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			UpdateLogs();
		}

		void UpdateLogs()
		{
			if (RequiresSignatureEventLog)
			{
				var latestPickupConfirmation = GetLatestConfirmation(Confirmations.Where(c => c.IsPickUp));
				var latestDeliveryConfirmation = GetLatestConfirmation(Confirmations.Where(c => c.IsDelivery));

				foreach (var confirmation in Confirmations)
				{
					confirmation.RequiresSignatureEventLog = false;
				}

				if (latestPickupConfirmation != null && latestPickupConfirmation.HasChanges)
				{
					AddSignatureLogToBooking(Events.SignatureCaptured, latestPickupConfirmation);
				}

				if (latestDeliveryConfirmation != null && latestDeliveryConfirmation.HasChanges)
				{
					AddSignatureLogToBooking(Events.SignatureCaptured, latestDeliveryConfirmation);
				}
			}
		}

		static DtbConsignmentConfirmation GetLatestConfirmation(IEnumerable<DtbConsignmentConfirmation> collection)
		{
			if (collection.All(c => (!c.KK_ReceivedBy.IsEmpty || !c.KK_ReceivedBySignature.IsEmpty)))
			{
				return collection.OrderBy(c => c.KK_Actual).ThenBy(c => c.KK_ReceivedBy).LastOrDefault();
			}

			return null;
		}

		void AddSignatureLogToBooking(Event eventType, DtbConsignmentConfirmation confirmation)
		{
			#region SuppressResourceStringsCheckRegion

			var reference = new StringBuilder();
			if (!Address.E2_City.IsEmpty)
			{
				reference.Append(Events.SignatureCaptured.Description).Append(" | ").Append(Address.E2_City);
			}

			reference.Append(" | Signed");
			if (!confirmation.KK_ReceivedBy.IsEmpty)
			{
				reference.Append(" by ").Append(confirmation.KK_ReceivedBy);
			}
			var eventTime = !confirmation.KK_Actual.IsEmpty ? confirmation.KK_Actual.ToOffset() : ZDateTimeOffset.Now;

			#endregion

			Booking.Logs.CreateRecreateOrUpdateEventLog(eventType, EstimateActual.Actual, eventTime, reference.ToString());
		}

		internal void AddLogToBooking(Event eventType, ZDateTimeOffset eventTime, string reference = "")
		{
			Booking.Logs.CreateRecreateOrUpdateEventLog(eventType, EstimateActual.Actual, eventTime, reference, GetInstructionEventParameters().ToArray());
		}

		internal List<KeyValuePair<string, string>> GetInstructionEventParameters()
		{
			var eventParams = new List<KeyValuePair<string, string>>();

			if (!FacilityCode.IsEmpty)
			{
				eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, FacilityCode));
			}

			if (!Address.E2_City.IsEmpty)
			{
				eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, Address.E2_City));
			}

			return eventParams;
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { DtbBookingInstructionSchema.Constants.KN_KM_BookingMovement, DtbBookingInstructionSchema.Constants.KN_Status });
			var clone = (DtbConsignmentInstruction)base.CloneInternal(args);
			CloneAddress(clone);
			CloneConfirmations(clone);
			return clone;
		}

		void CloneAddress(DtbConsignmentInstruction clone)
		{
			clone.DocAddresses.RemoveAndDeleteAll();
			clone.DocAddresses.Add(Address.Clone());
		}

		void CloneConfirmations(DtbConsignmentInstruction clone)
		{
			foreach (var confirmation in Confirmations)
			{
				clone.Confirmations.Add((DtbConsignmentConfirmation)confirmation.Clone());
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region UpdateStatus

		protected override ZString GetExpectedStatus()
		{
			ZString result = ZString.Empty;

			// poke collection (collection may not be intialised yet) so as to update status with any
			// new confirmations that were added directly onto the Instruction through CountChanged firing
			var poke = Confirmations; // may not need as we should add count changed on Confirmations collection instead

			if (Confirmations.Any())
			{
				result = GetExpectedStatusBasedOnConfirmations();
			}

			return result;
		}

		ZString GetExpectedStatusBasedOnConfirmations()
		{
			ZString result = ZString.Empty;

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

			foreach (var confirmation in Confirmations)
			{
				var runSheetInstruction = confirmation.RunSheetInstruction;
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
				result = KN_InstructionType == InstructionTypes.Codes.PickUp
					? TransportStatuses.Codes.PickedUp
					: TransportStatuses.Codes.Delivered;
			}

			return result;
		}

		ZString GetExpectedMultiStatus()
		{
			ZString result = ZString.Empty;

			var pickUpConfirmation = Confirmations.SingleOrDefault(c => c.IsPickUp);
			if (pickUpConfirmation != null)
			{
				result = GetExpectedStatus(pickUpConfirmation, TransportStatuses.Codes.PickUpAllocated, TransportStatuses.Codes.PickedUp);
			}

			if (result.IsEmpty)
			{
				var deliveryConfirmation = Confirmations.SingleOrDefault(c => c.IsDelivery);
				if (deliveryConfirmation != null)
				{
					result = GetExpectedStatus(deliveryConfirmation, TransportStatuses.Codes.DeliveryAllocated, TransportStatuses.Codes.Delivered);
				}
			}

			return result;
		}

		static ZString GetExpectedStatus(DtbConsignmentConfirmation confirmation, string allocatedStatus, string completedStatus)
		{
			ZString result = ZString.Empty;

			var runSheetInstruction = confirmation.RunSheetInstruction;
			if (runSheetInstruction != null)
			{
				result = runSheetInstruction.K1_TimeIn.IsEmpty && runSheetInstruction.K1_TimeOut.IsEmpty
					? allocatedStatus
					: completedStatus;
			}

			return result;
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbConsignmentInstructionFetchStrategy(this);
		}

		#endregion

		#region Lookups

		public new DtbConsignmentInstructionLookups Lookups
		{
			get { return (DtbConsignmentInstructionLookups)base.Lookups; }
		}

		protected override DtbTransportInstructionLookups GetNewLookupsCore()
		{
			return new DtbConsignmentInstructionLookups(this);
		}

		#endregion

		#region Validation

		public new DtbConsignmentInstructionValidation Validation
		{
			get { return (DtbConsignmentInstructionValidation)base.Validation; }
		}

		protected override DtbTransportInstructionValidation GetNewValidationCore()
		{
			return new DtbConsignmentInstructionValidation(this);
		}

		#endregion

		#region GetAndCreateDefaultConfirmationsIfNotExists

		IEnumerable<DtbTransportConfirmation> GetAndCreateDefaultConfirmationsIfNotExists
		{
			get
			{
				if (!DefaultConfirmations.Any())
				{
					CreateTransportConfirmation();
				}
				return DefaultConfirmations;
			}
		}

		#endregion

		// interfaces

		#region IDocAddresses Members

		protected override SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress)
		{
			var addressType = (docAddress != null) ? docAddress.E2_AddressType : ZString.Empty;

			switch (addressType)
			{
				case DocAddressTypes.Codes.LocalCartageExporter:
					return Env.Security.DtbBookingConsignmentMISCDetailsConsignor;
				case DocAddressTypes.Codes.LocalCartageImporter:
					return Env.Security.DtbBookingConsignmentMISCDetailsConsignee;
				default:
					return Env.Security.DtbBookingConsignmentMISCDetails;
			}
		}

		protected override void OnDocAddressChangedCore(JobDocAddress docAddress)
		{
			base.OnDocAddressChangedCore(docAddress);

			SetDropMode(docAddress);
			UpdateInstructionOrgType(docAddress);

			CheckAndInsertDepotInstruction();

			// tested in DtbConsignmentInstructionValidation
			ValidateInstructionFieldsWhereAddressIsRequired();

			var booking = Booking;
			if (booking != null)
			{
				booking.UpdateConfirmationsEstimateTime();
			}
		}

		void CheckAndInsertDepotInstruction()
		{
			if (Booking != null && Booking.Instructions.Any(i => i.IsPickUp))   //at least one pickup
			{
				if (IsDepot && !IsMulti)
				{
					var otherInstruction = IsPickUp ? Booking.DeliveryInstruction : Booking.PickupInstruction;
					var relatedDepotInstruction = IsPickUp ? Booking.PickupDepotInstruction : Booking.DeliveryDepotInstruction;
					if (relatedDepotInstruction != null && (Booking.PickupDepotInstruction != Booking.DeliveryDepotInstruction || otherInstruction.IsDepot))
					{
						relatedDepotInstruction.Delete();
					}
				}
				else
				{
					if (!IsDepot && !IsMulti)
					{
						Booking.CreateOrUpdateDepotInstruction(this);
					}
				}
			}
		}

		void UpdateInstructionOrgType(JobDocAddress docAddress)
		{
			var docAddressType = ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(KN_InstructionType, docAddress.DocAddressType);
			var orgType = CartageOrgTypeAddressTypeConverter.GetOrgTypeFromCartageDocAddressType(docAddressType);
			UpdateOrganisationTypeIfRequired(orgType);
		}

		void UpdateOrganisationTypeIfRequired(string orgType)
		{
			if (OrganisationType != orgType)
			{
				OrganisationType = orgType;
			}
		}

		void SetDropMode(JobDocAddress docAddress)
		{
			if (docAddress.E2_OA_Address.IsValid)
			{
				KN_DropMode = docAddress.Address.OA_LCLEquipmentNeeded;
			}
		}

		void ValidateInstructionFieldsWhereAddressIsRequired()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateKN_DropMode();
				Validation.ValidateKN_ServiceInstruction();
				Validation.ValidateReqFrom();
				Validation.ValidateReqTo();
			}
		}

		#endregion

		#region PiggyBackedDocAddressValidationCore

		protected override ZValidation PiggyBackedDocAddressValidationCore(JobDocAddress addressToValidate)
		{
			return new DtbConsignmentInstructionDocAddressValidation(addressToValidate);
		}

		#endregion

		//

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var consignment = Factory.New<DtbBookingConsignment>();
			consignment.Instructions.Add(this);

			using (new SemaphoreManager(consignment.AddingInstructionsFromTemplateSemaphore))
			{
				consignment.FillWithValidTestData(kind, Array.Empty<PropertyDescriptor>());
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		public string FacilityCodeForTesting
		{
			get { return FacilityCode; }
		}
#endif
		#endregion
	}
}
