using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	[CodeProperty(DtbBookingConsolidationSchema.Constants.KB_JobID), DescriptionProperty(DtbBookingConsolidationSchema.Constants.KB_JobID)]
	public abstract class DtbTransportConsolidation : AutoDtbBookingConsolidation,
		IDtbTransportConsolidation,
		IJobNumber,
		IDocAddresses
	{
		protected DtbTransportConsolidation(BusinessObjectFactory factory, DataRow row)
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

		public static readonly DtbTransportConsolidationTypeDecider TypeDecider = new DtbTransportConsolidationTypeDecider();

		#endregion

		#region SetDefaultValues

		protected sealed override void SetDefaultValues()
		{
			base.SetDefaultValues();

			KB_JobType = DefaultJobType;
			SetDefaultValuesCore();
		}

		protected abstract string DefaultJobType { get; }

		protected virtual void SetDefaultValuesCore()
		{
		}

		#endregion

		#region Related Entities

		#region Bookings

		[ChildEditable]
		public IDtbTransportCollection Bookings
		{
			get
			{
				if (Transports == null)
				{
					Transports = GetNewTransportsCollection();

					if (DtbChildEditableService.GetState(Factory) != DtbChildEditableServiceState.Transport)
					{
						RegisterEditableChildObject(Transports);
					}

					Transports.CountChanged += Transports_CountChanged;
				}

				return Transports;
			}
		}

		protected abstract IDtbTransportCollection GetNewTransportsCollection();

		void Transports_CountChanged(object sender, EventArgs e)
		{
			OnTransportsCountChanged();
		}

		protected virtual void OnTransportsCountChanged()
		{
		}

		protected void ClearTransports()
		{
			Transports = null;
		}

		IDtbTransportCollection Transports;

		#endregion

		#region BookedByAddress

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

		#endregion

		#endregion

		#region Properties

		// calculated

		#region ParentJobDescription

		public ZString ParentJobDescription
		{
			get { return ParentJobDescriptionCore; }
		}

		protected abstract ZString ParentJobDescriptionCore
		{
			get;
		}

		#endregion

		#region BookedByOrganisationPK

		public ZGuid BookedByOrganisationPK
		{
			get
			{
				var bookedBy = BookedByAddress;
				return bookedBy != null && !bookedBy.E2_AddressOverride ? bookedBy.OrganisationPK : ZGuid.Empty;
			}
		}

		#endregion

		#endregion

		#region Lookups

		public new DtbTransportConsolidationLookups Lookups
		{
			get { return (DtbTransportConsolidationLookups)base.Lookups; }
		}

		protected sealed override DtbBookingConsolidationLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportConsolidationLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportConsolidationValidation Validation
		{
			get { return (DtbTransportConsolidationValidation)base.Validation; }
		}

		protected sealed override DtbBookingConsolidationValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportConsolidationValidation GetNewValidationCore();

		#endregion

		#region Save

		public sealed override void OnSaving()
		{
			PopulateUnqiueIDIfNeeded();
			BeforeOnSaving();
			base.OnSaving();
		}

		void PopulateUnqiueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted)
			{
				KB_JobID = NumberFountainForUniqueID.GetNextFormatted(Factory);
			}
		}

		protected virtual void BeforeOnSaving()
		{
		}

		#endregion

		#region Delete

		public sealed override void Delete()
		{
			// tested by SaveAndDeleteBusinessObject()
			DeleteTransports();
			DeleteCore();

			base.Delete();
		}

		protected virtual void DeleteTransports()
		{
			Bookings.DeleteAll();
		}

		protected virtual void DeleteCore()
		{
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DtbTransportConsolidationFountainUniqueIndexFailureHandler(this)); }
		}

		class DtbTransportConsolidationFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DtbTransportConsolidationFountainUniqueIndexFailureHandler(DtbTransportConsolidation consolidation)
				: base(DtbBookingConsolidationSchema.Constants.Indexes.NR_UC__KB_JobID, consolidation)
			{
				Consolidation = consolidation;
			}

			readonly DtbTransportConsolidation Consolidation;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Consolidation.NumberFountainForUniqueID; }
			}
		}

		INumberFountainProxy NumberFountainForUniqueID
		{
			get { return GetNumberFountainForUniqueID(); }
		}

		protected abstract INumberFountainProxy GetNumberFountainForUniqueID();

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return KB_JobID; }
		}

		#endregion

		#region Notifications

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

		#endregion

		#region IDocAddresses Members

		#region DocAddresses

		[ChildEditable()]
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

		#region SupportedAddressTypes

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				var result = new List<DocAddressType>() { DocAddressType.BookingPartyDocumentaryAddress };
				result.AddRange(GetSupportedAddressTypesCore());
				return result.ToArray();
			}
		}

		protected virtual IEnumerable<DocAddressType> GetSupportedAddressTypesCore()
		{
			return Array.Empty<DocAddressType>();
		}

		#endregion

		#region Events

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			DocAddressChangedCore(docAddress);
			if (docAddress.DocAddressType == DocAddressType.BookingPartyDocumentaryAddress
				&& !docAddress.E2_AddressOverride)
			{
				var clientRequestedBillToParty = FindClientRequestedBillToPartyForBookingPartyDocumentaryAddress(docAddress);
				if (clientRequestedBillToParty != null)
				{
					foreach (DtbTransport booking in Bookings)
					{
						var billingPartyAddress = booking.BillingPartyAddress;
						if (!billingPartyAddress.E2_AddressOverride && !billingPartyAddress.E2_OA_Address.IsValid)
						{
							billingPartyAddress.E2_OA_Address = clientRequestedBillToParty.MainAddress.PK;
						}
					}
				}
			}
			OnDocAddressChanged(docAddress);
		}

		protected virtual void OnDocAddressChanged(JobDocAddress docAddress)
		{
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

		protected virtual void DocAddressChangedCore(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			OrgAddressBeforeChangeCore(docAddress);
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		protected virtual void OrgAddressBeforeChangeCore(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			OrgHeaderAfterChangeCore(docAddress);
		}

		protected virtual void OrgHeaderAfterChangeCore(JobDocAddress docAddress)
		{
		}

		#endregion

		#region CanDeleteAddress

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return CanDeleteAddressCore(docAddress);
		}

		protected virtual bool CanDeleteAddressCore(JobDocAddress docAddress)
		{
			return true;
		}

		#endregion

		#region GetCanOverrideCheckpoint

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.TransportJobMISCDetails;
		}

		#endregion

		#region GetDocAddressRequirement

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
			=> GetDocAddressRequirementCore(addressType)
				?? new JobDocAddressRequirement(addressType, AddressType.OFC, ContactType.LocalTransport);

		protected virtual JobDocAddressRequirement GetDocAddressRequirementCore(DocAddressType addressType)
		{
			return null;
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
			return null;
		}

		#endregion

		#endregion
	}
}
