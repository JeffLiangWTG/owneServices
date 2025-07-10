using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Module
{
	public class CarrierBookingAgentChangeActionMethodApplicator : DtbBookingOperationalActionMethodApplicator
	{
		public CarrierBookingAgentChangeActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
			CarrierBookingAgents = new OrgHeaderCollection(factory);
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] bookings)
		{
			base.ApplyCore(log, bookings);
			if (SomeTargetsAreLockedDown)
			{
				SomeTargetsAreLockedDown = false;
				return;
			}

			var orgHeader = !CarrierBookingAgentPK.IsEmpty ? Factory.Load<OrgHeader>(CarrierBookingAgentPK) : null;
			var addressPK = orgHeader?.MainAddress.PK ?? ZGuid.Empty;

			if (orgHeader != null || CarrierBookingAgentPK.IsEmpty)
			{
				log.SetSectionProgressMax(bookings.Length);

				foreach (DtbBooking booking in bookings)
				{
					AssignCarrierBookingAgent(booking, addressPK, log);
					log.BumpSectionProgress();
				}
			}
			else if (!CarrierBookingAgentPK.IsEmpty)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("7a65cbe4-296f-41d2-ba88-0ad36254b543", "Failed to load Carrier Booking Agent organization."));
			}
		}

		void AssignCarrierBookingAgent(DtbBooking booking, ZGuid addressPK, IOperationalActionSectionLog log)
		{
			var bookingLink = GetBookingIdLink(booking);

			if (booking.CarrierBookingAgentDocAddress.ReadOnly)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0} - {1}", bookingLink, Res.GetString("4a7e9157-ee68-42d3-8db8-0b59c9edc4ab", "The action '{0}' failed as the booking is read only.", Name));
			}
			else
			{
				if (booking.CarrierBookingAgentDocAddress.E2_OA_Address != addressPK)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					booking.Logs.AddNew(AutoEvents.EditedARecord, "Assigned a Carrier Booking Agent via Operational Action.", ZDateTimeOffset.Now, false);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					booking.CarrierBookingAgentDocAddress.E2_OA_Address = addressPK;
				}

				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} - {1}", bookingLink, Res.GetString("8a6e9157-ee68-42d3-8db8-0b59c9ecc4da", "The action '{0}' was successfully processed.", Name));
			}
		}

		[ResourceStringData("CarrierBookingAgentChangeActionMethodApplicator|CarrierBookingAgentPK", Caption = "Carrier Booking Agent")]
		public ZGuid CarrierBookingAgentPK
		{
			get { return carrierBookingAgentPK; }
			set { SetNonPersistentPropertyValue(CarrierBookingAgentPKInfo, ref carrierBookingAgentPK, value); }
		}

		public ZPropertyInfo CarrierBookingAgentPKInfo => GetZPropertyInfo(nameof(CarrierBookingAgentPK));

		public OrgHeaderCollection CarrierBookingAgents { get; }

		ZGuid carrierBookingAgentPK;
	}
}
