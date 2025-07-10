using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class RollBookingApplicator : AutoRollBookingApplicator
	{
		public RollBookingApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("53772c3d-4844-41eb-82b8-ca02056e1e22", "Roll Bookings"), factory)
		{
			dummyBooking = factory.New<AgencyBooking>();
			transports = new AgencyShipmentTransportCollection(dummyBooking);
		}
		readonly AgencyShipment dummyBooking;

		#region ApplyCore

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (Transports.Count > 0 && targets.Length > 0)
			{
				var bookings = targets.OfType<AgencyBooking>().ToArray();

				Transports.Sort(MovementLegComparer.PortsAndDatesBased(Transports));
				var transports = Transports.Cast<Transport>().ToArray();

				transports.ForEach(t => t.Validation.ValidateAll());
				if (transports.Any(t => t.HasErrors))
				{
					Globals.Message.ShowError(Res.GetString("755bfdd1-5961-4d9a-8427-398041277bf9", "Please fix the transport errors first and run this again.")); // Show Message in Applicator
					return;
				}

				var mainLegs = transports
					.Where(t => t.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
				if (mainLegs.Count() > 1)
				{
					log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("f6385189-2116-4587-aa6b-2a454a6c6c4a", "Can't have more than one MAI Transport Type."));
					return;
				}

				RollBookings(log, bookings, transports);
			}
		}

		void RollBookings(IOperationalActionSectionLog log, AgencyBooking[] bookings, Transport[] transports)
		{
			var helper = ObjectFactory.Get<IUpdateExchangeRates>();
			var sourceProvider = GetSourceProvider();

			var cachedUsage = new AllocationUsage();

			var transportNewSailingDict = CreateNewSailings(bookings.First().Factory, transports);
			var mainTransport = transports.FirstOrDefault(t => t.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel);
			var mainSailing = transportNewSailingDict.TryGetValue(mainTransport?.PK ?? ZGuid.Empty, out var newSailing) ? newSailing : mainTransport?.Sailing;

			foreach (var booking in bookings)
			{
				ValidateRoutingLeg(log, transports, mainSailing, booking);

				CopyTransportsToBooking(transports, booking, transportNewSailingDict);
				booking.JS_JX = mainSailing?.PK ?? ZGuid.Empty;

				if (!ValidateAllocation(log, booking, cachedUsage))
				{
					return;
				}

				helper.UpdateExchangeRates(booking, sourceProvider, log);
			}
		}

		IDictionary<ZGuid, JobSailing> CreateNewSailings(BusinessObjectFactory factory, Transport[] transports)
		{
			var transportNewSailingDict = new Dictionary<ZGuid, JobSailing>();
			foreach (var (transport, sailing) in transports.Select(t => (t, t.Sailing)))
			{
				if (sailing?.Voyage != null && !sailing.IsInDatabase)
				{
					var cloneArgs = new BusinessObjectCloneArgs(factory, Enumerable.Empty<string>(), null, true);
					var newVoyage = (JobVoyage)sailing.Voyage.Clone(cloneArgs);
					var newSailing = newVoyage.Sailings.Cast<JobSailing>().FirstOrDefault();
					transportNewSailingDict.Add(transport.PK, newSailing);
				}
			}
			return transportNewSailingDict;
		}

		void CopyTransportsToBooking(Transport[] transports, AgencyBooking booking, IDictionary<ZGuid, JobSailing> transportNewSailingDict)
		{
			booking.Transports.RemoveAndDeleteAll();

			foreach (var transport in transports)
			{
				var cloneArgs = new BusinessObjectCloneArgs(booking.Factory, Enumerable.Empty<string>(), null, true);
				var newTransport = transport.Clone(cloneArgs) as Transport;
				if (transportNewSailingDict.TryGetValue(transport.PK, out var newSailing))
				{
					newTransport.JW_JX = newSailing.PK;
				}
				booking.Transports.Add(newTransport);
			}
		}

		Converter<BusinessObject, IExchangeRateSource> GetSourceProvider()
		{
			return delegate(BusinessObject target)
			{
				var provider = target as IJobInvoicingExRateSourceProvider;
				return provider == null ? null : provider.GetExRateSource(ExRateSourceType.Voyage);
			};
		}

		#endregion

		#region Validation

		bool ValidateAllocation(IOperationalActionSectionLog log, AgencyBooking booking, AllocationUsage cachedUsage)
		{
			var allocationMethod = booking.Sailing?.Origin?.VoyageCountry?.J0_AllocationMethod ?? ZString.Empty;
			if (allocationMethod == AllocationMethodList.Codes.NotSet
				|| allocationMethod == AllocationMethodList.Codes.Ignore)
			{
				return true;
			}

			if (booking.ShouldEnforceAllocations)
			{
				var usageAfterChange = AllocationUsage.LoadFromShipment(booking);

				var allocationSet = booking.LoadAllocationUsageSet();
				allocationSet.Used.Add(cachedUsage);

				var canFit = allocationSet?.CanFit(usageAfterChange) ?? false;
				if (!canFit)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("947886ad-005f-4467-9c8e-b23901fc1ec0", "Agency Booking {0} has the following errors:", booking.JS_UniqueConsignRef));
					log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("b41ffb16-ac92-4b16-8a00-80c628372791", "Allocations on the new schedule exceed the size of the bookings."));
					return false;
				}

				cachedUsage.Add(usageAfterChange);
			}

			return true;
		}

		void ValidateRoutingLeg(IOperationalActionSectionLog log, Transport[] transports, JobSailing routingLeg, AgencyBooking booking)
		{
			var messages = new List<string>();

			booking.Transports.Sort(MovementLegComparer.PortsAndDatesBased(booking.Transports));

			var firstLegETD = transports.First().JW_ETD;
			if (firstLegETD.IsValid
				&& booking.JS_E_DEP.IsValid
				&& firstLegETD < booking.JS_E_DEP)
			{
				messages.Add(Res.GetString("4928e0e4-2e8f-4396-a5e5-6b75fb7a5ebe", "The ETD of the new voyage is earlier than Origin ETD of the shipment."));
			}

			if (routingLeg != null && booking.BookedShippingLinePK != (routingLeg.Voyage?.JV_OH_Line ?? ZGuid.Empty))
			{
				messages.Add(Res.GetString("309c3e6c-4863-4bce-b519-5a718eb0cc81", "The Carrier is not the same."));
			}

			var lastBookingLeg = booking.Transports.Cast<Transport>().LastOrDefault();
			if (lastBookingLeg != null
				&& lastBookingLeg.JW_RL_NKDiscPort != transports.Last().JW_RL_NKDiscPort)
			{
				messages.Add(Res.GetString("ceefbbd0-71fc-42cd-9fe9-9edccba9c97a", "The Discharge Port on the booking does not match the Discharge Port of the last routing leg."));
			}

			var lastLegETA = transports.Last().JW_ETA;
			if (lastLegETA.IsValid
				&& booking.JS_E_ARV.IsValid
				&& lastLegETA >= booking.JS_E_ARV)
			{
				messages.Add(Res.GetString("abe9e129-b1a3-42be-9356-2ff5b6c1459d", "The Discharge ETA of the new voyage must be prior to the Destination ETA of the Shipment."));
			}

			if (messages.Any())
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					Res.GetString("40daf80b-55dd-4d98-984a-544396da1177", "Agency Booking {0} has the following warnings:", booking.JS_UniqueConsignRef));

				foreach (var message in messages)
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, message);
				}
			}
		}

		#endregion

		#region Transports

		public AgencyShipmentTransportCollection Transports => transports;
		readonly AgencyShipmentTransportCollection transports;

		#endregion
	}
}
