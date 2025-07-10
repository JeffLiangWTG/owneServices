using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Lists;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.ModuleTextFilter.ComparisonConstants;

namespace Enterprise.TransportBookings.Business
{
	/// <summary>
	/// This collection does NOT behave like a normal ActiveBusinessObjectCollection,
	/// unless using the constructor with a Consolidation passed in, only bookings
	/// which have been saved will appear in the collection.
	/// </summary>
	[ModuleID("DtbBooking")]
	public class DtbBookingCollection : ActiveBusinessObjectCollection<DtbBooking>,
		IDtbBookingCollection
	{
		public DtbBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AllowNewCore = true;
		}

		public DtbBookingCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public DtbBookingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public DtbBookingCollection(DtbBookingConsolidation consolidation)
			: base(consolidation.Factory, consolidation, null, consolidation.IsMultiBooking ? DtbBookingSchema.KM_KB_BookingConsolidationMultiJob : DtbBookingSchema.KM_KB_Booking)
		{
			AllowNewCore = !consolidation.KB_IsMaster;
			Consolidation = consolidation;
		}

		public DtbBookingCollection(DtbBooking masterBooking)
			: base(masterBooking.Factory, masterBooking, null, DtbBookingSchema.KM_KM_MasterBooking)
		{
			AllowNewCore = false;
		}

		readonly DtbBookingConsolidation Consolidation;

		IDtbBooking IDtbBookingCollection.this[int index] => this[index];

		protected override bool AllowNew
		{
			get { return ConsolidationViewModeService.GetViewMode(Factory) != ConsolidationViewMode.MultiJob && AllowNewCore && !IsReadOnly; }
		}

		bool IsReadOnly
		{
			get { return Consolidation != null && Consolidation.IsBookingsReadOnly; }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		public bool AllowNewCore;

		public bool IsAnyBookingHeld
		{
			get { return this.Any(b => b.IsHeld); }
		}

		public bool IsDelivered
		{
			get { return Count > 0 && this.All(b => b.IsDelivered); }
		}

		public bool IsDeliveredEmptyNotReturned
		{
			get { return Count > 0 && this.All(b => b.IsDeliveredEmptyNotReturned || b.IsDelivered); }
		}

		public bool IsPickedUp
		{
			get { return Count > 0 && this.All(b => b.IsPickedUp || b.IsDeliveredEmptyNotReturned || b.IsDelivered); }
		}

		public bool IsServiceCommenced
		{
			get { return Count > 0 && this.All(b => b.IsServiceCommenced); }
		}

		public bool IsActionRequired
		{
			get { return Count > 0 && this.All(b => b.IsActionRequired); }
		}

		public void OnConsolidationIsOverrideChanged()
		{
			foreach (var booking in this)
			{
				booking.OnConsolidationIsOverrideChanged();
			}

			if (this.Count > 0)
			{
				((IBusinessObjectCollectionInternals)this).FireListResetEvent();
			}
		}

		string GetParentType()
		{
			return TransportConsolidationJobTypes.Codes.Booking;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery(DtbBookingSchema.KM_JobType, GetParentType());
			filter.AddToFilter(base.CreateRelationshipFilter());

			return filter;
		}

		public IEnumerable<DtbBooking> Typed
		{
			get { return this; }
		}
	}

	/// <summary>
	/// Use this collection to Attach Bookings to a Consolidation.
	/// </summary>
	public class DtbBookingCollectionForFindBox : DtbBookingCollection
	{
		public DtbBookingCollectionForFindBox(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public DtbBookingCollectionForFindBox(BusinessObjectFactory factory, OrgHeader transportCo)
			: base(factory)
		{
			AllowNewCore = false;
			TransportCo = transportCo;
			SetupFilterBusinessObjectDefaults();
		}

		void SetupFilterBusinessObjectDefaults()
		{
			if (TransportCo != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.TransportCompany, "Property", TransportCo.PK, true));
				AdditionalFilter = new TransportBookingsQueryHelper().BookingTransportCompanyQuery(TransportCo.PK);
			}
			else
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.TransportCompany, "Property", ZGuid.Empty, true));
			}

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingStatus, "Property", (ZString)TransportStatuses.Codes.Available, true));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingConsolidated, "Property", (ZString)BookingConsolidatedStatuses.Codes.Unconsolidated, true));
		}

		readonly OrgHeader TransportCo;

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			if (TransportCo != null)
			{
				var booking = (DtbBooking)selectedBusinessObject;
				if (booking.Address.OrganisationPK != TransportCo.PK)
				{
					errors.Add(Res.GetString("2269ed18-1f4a-49c3-a836-fdcbc53396e9",
						"This Booking does not have the same Transport Company as the Consolidation ({0}).", TransportCo.OH_Code));
				}
			}
		}
	}

	/// <summary>
	/// Use this collection to Attach Bookings to a Master Booking.
	/// </summary>
	[ZFilterModuleAlwaysCheckExtraNotification]
	public class DtbBookingCollectionForSubBookingsFindBox : ActiveBusinessObjectCollection<DtbBooking>,
		IDtbBookingCollection, IFilterModuleExtraNotificationProvider
	{
		public DtbBookingCollectionForSubBookingsFindBox(BusinessObjectFactory factory, OrgAddress transportCo, bool isExtraValidationSuspended) : base(factory ?? new BusinessObjectFactory())
		{
			TransportCo = transportCo;
			CarrierBookingAgent = null;
			MasterBooking = null;
			MasterBookingTemplate = null;
			MasterBookingDirection = null;
			MasterBookingConsolidationJobDirection = null;
			MasterBookingTransportMode = null;
			IsExtraValidationSuspended = isExtraValidationSuspended;
			SetupAdditionalFilter();
			SetupFilterBusinessObjectDefaults();
		}

		public DtbBookingCollectionForSubBookingsFindBox(DtbBooking masterBooking, bool isExtraValidationSuspended) : base(masterBooking.Factory ?? new BusinessObjectFactory())
		{
			TransportCo = masterBooking.Address.Address;
			CarrierBookingAgent = masterBooking.CarrierBookingAgentDocAddress.Address;
			MasterBooking = masterBooking;
			MasterBookingTemplate = masterBooking.KM_KT_NKBookingTemplate;
			MasterBookingDirection = masterBooking.KM_Direction;
			MasterBookingConsolidationJobDirection = masterBooking.ConsolidationSingleJob.KB_JobDirection;
			MasterBookingTransportMode = masterBooking.KM_TransportMode;
			IsExtraValidationSuspended = isExtraValidationSuspended;
			SetupAdditionalFilter();
			SetupFilterBusinessObjectDefaults();
		}

		readonly OrgAddress TransportCo;
		readonly OrgAddress CarrierBookingAgent;
		readonly DtbBooking MasterBooking;
		readonly string MasterBookingTemplate;
		readonly string MasterBookingDirection;
		readonly string MasterBookingConsolidationJobDirection;
		readonly string MasterBookingTransportMode;
		readonly bool IsExtraValidationSuspended;

		void SetupAdditionalFilter()
		{
			var filter = new ZQuery(DtbBookingSchema.KM_JobType, GetParentType());
			filter.AddToFilter(DtbBookingSchema.KM_IsMaster, SQLComparisonOperator.Equal, false);
			filter.AddToFilter(DtbBookingSchema.KM_KB_BookingConsolidationMultiJob, SQLComparisonOperator.IsBlank, null);
			filter.AddToFilter(DtbBookingSchema.KM_KM_MasterBooking, SQLComparisonOperator.IsBlank, null);
			filter.AddToFilter(DtbBookingSchema.KM_MasterBookingVersion, SQLComparisonOperator.Equal, (short)0);
			filter.AddToFilter(DtbBookingSchema.KM_Status, SQLComparisonOperator.Equal, TransportStatuses.Codes.Available);
			filter.AddToFilter(DtbBookingSchema.KM_IsAgentBooking, SQLComparisonOperator.Equal, false);

			if (TransportCo != null)
			{
				filter.AddToFilter(TransportBookingsQueryHelper.OrgAddressMatchQueryForNotEmpty(DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, TransportCo.PK));
			}
			else
			{
				filter.AddToFilter(TransportBookingsQueryHelper.OrgAddressMatchQueryForEmpty(DocAddressTypes.Codes.TransportCompanyDocumentaryAddress));
			}

			if (!IsExtraValidationSuspended && MasterBooking != null)
			{
				if (CarrierBookingAgent != null)
				{
					filter.AddToFilter(TransportBookingsQueryHelper.OrgAddressMatchQueryForNotEmpty(DocAddressTypes.Codes.CarrierBookingAgent, MasterBooking.CarrierBookingAgentDocAddress.E2_OA_Address));
				}
				else
				{
					filter.AddToFilter(TransportBookingsQueryHelper.OrgAddressMatchQueryForEmpty(DocAddressTypes.Codes.CarrierBookingAgent));
				}

				filter.AddToFilter(TransportBookingsQueryHelper.BookingTemplateMatchesMasterBookingTemplate(MasterBooking.KM_KT_NKBookingTemplate));
				filter.AddToFilter(TransportBookingsQueryHelper.BookingDirectionMatchesMasterBookingDirection(MasterBooking.KM_Direction));
				filter.AddToFilter(TransportBookingsQueryHelper.BookingConsolidationJobDirectionMatchesMasterBookingConsolidationJobDirection(MasterBooking.ConsolidationSingleJob.KB_JobDirection));
				filter.AddToFilter(TransportBookingsQueryHelper.BookingTransportModeMatchesMasterBookingTransportMode(MasterBooking.KM_TransportMode));
			}

			AdditionalFilter = filter;
		}

		void SetupFilterBusinessObjectDefaults()
		{
			if (TransportRegistry.Instance.MasterBookingsEnabled.Value)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingIsMaster, "Property", (ZString)IsMasterBookingStatuses.Codes.NotMasterBooking, true));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingIsSub, "Property", (ZString)IsSubBookingStatuses.Codes.NotSubBooking, true));
			}
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingStatus, "Property", (ZString)TransportStatuses.Codes.Available, true));
			if (TransportCo != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.TransportCompany, "Property", TransportCo.Header.PK, true));
			}
			else
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.TransportCompany, "ComparisonOperator", (ZString)IsBlank, true));
			}

			if (!IsExtraValidationSuspended && MasterBooking != null)
			{
				if (CarrierBookingAgent != null)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.CarrierBookingAgent, "Property", CarrierBookingAgent.Header.PK, true));
				}
				else
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.CarrierBookingAgent, "ComparisonOperator", (ZString)IsBlank, true));
				}

				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingConsolidationTemplate, "Property", MasterBooking.KM_KT_NKBookingTemplate, true));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingConsolidationJobDirection, "Property", MasterBooking.ConsolidationSingleJob.KB_JobDirection, true));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterNameConstants.BookingTransportMode, "Property", MasterBooking.KM_TransportMode, true));
			}
		}

		string GetParentType()
		{
			return TransportConsolidationJobTypes.Codes.Booking;
		}

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			Notification result = null;
			var openedBookingFormCounter = ObjectFactory.Get<IOpenedBookingFormCounter>();
			var booking = businessObject as DtbBooking;
			if (openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob) > 0)
			{
				result = new Notification(CargoWise.ComponentModel.NotificationType.Error, (NoResString)"Cannot be attached to the Master Booking as the job is currently open.");
			}
			return result;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery(DtbBookingSchema.KM_JobType, GetParentType());
			filter.AddToFilter(base.CreateRelationshipFilter());

			return filter;
		}

		protected override bool AllowNew => false;

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var booking = (DtbBooking)selectedBusinessObject;
			if (booking.KM_IsMaster)
			{
				errors.Add(Res.GetString("3257FD21-324A-4CF2-A654-0C2FA71C3F4C", "Booking is a Master Booking."));
			}
			if (!booking.KM_KM_MasterBooking.IsEmpty)
			{
				errors.Add(Res.GetString("49824367-90F9-4B86-952E-FAF2F576D80A", "This Booking is already attached to a Master Booking."));
			}
			if (booking.KM_MasterBookingVersion > 0)
			{
				errors.Add(Res.GetString("252A4885-014D-42DC-90B0-E1BCD908ABEF", "This Booking has a Master Version greater than zero."));
			}
			if (!booking.KM_KB_BookingConsolidationMultiJob.IsEmpty)
			{
				errors.Add(Res.GetString("8FC064C4-FD4F-4262-9CF9-909AB8E658D6", "This Booking is part of a Consolidated Booking."));
			}
			if (!booking.IsAvailable)
			{
				errors.Add(Res.GetString("285427B0-A410-4FFE-9576-98A7F6C73012", "This Booking does not have a status of Available."));
			}
			if (booking.KM_IsAgentBooking)
			{
				errors.Add(Res.GetString("5C028201-0D7C-445A-9C23-8C820BD2FB93", "This Booking is an Agent Booking."));
			}
			if (booking.KM_KT_NKBookingTemplate != MasterBookingTemplate)
			{
				errors.Add(Res.GetString("D91EEF0F-754B-4861-A190-9793F06315B4", "This Booking has a different template to the Master Booking."));
			}
			if (booking.KM_Direction != MasterBookingDirection)
			{
				errors.Add(Res.GetString("78033BCF-0B61-4C3C-BDE2-37E3D3C74938", "This Booking has a different booking direction to the Master Booking."));
			}
			if (booking.ConsolidationSingleJob.KB_JobDirection != MasterBookingConsolidationJobDirection)
			{
				errors.Add(Res.GetString("ce146cb1-0b20-4a07-9358-f707fabc263f", "This Booking's consolidation has a different job direction from that of the Master Booking's consolidation."));
			}
			if (booking.KM_TransportMode != MasterBookingTransportMode)
			{
				errors.Add(Res.GetString("daf16d29-1faf-4b15-8a66-85285807efee", "This Booking has a different booking transport mode to the Master Booking."));
			}

			if (TransportCo != null && booking.Address.E2_OA_Address != TransportCo.PK || TransportCo == null && booking.Address.E2_OA_Address != ZGuid.Empty)
			{
				errors.Add(Res.GetString("043C2CA2-714D-44B6-AAFD-CE2C68AF5EEB", "This Booking has a different Transport Company from the Master Booking ({0}).", MasterBooking.Address.Address?.OA_Code));
			}
			if (CarrierBookingAgent != null && booking.CarrierBookingAgentDocAddress.E2_OA_Address != CarrierBookingAgent.PK || CarrierBookingAgent == null && booking.CarrierBookingAgentDocAddress.E2_OA_Address != ZGuid.Empty)
			{
				errors.Add(Res.GetString("9749F1B8-01D1-430C-B2B0-6693D8D90DCE", "This Booking has a different Carrier Booking Agent from the Master Booking ({0}).", MasterBooking.CarrierBookingAgentDocAddress.Address?.OA_Code));
			}
		}

		IDtbBooking IDtbBookingCollection.this[int index] => this[index];
	}
}
