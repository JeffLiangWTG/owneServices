using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class AgencyShipmentContainerDefaultingStrategy : IContainerDefaultingStrategy
	{
		public AgencyShipmentContainerDefaultingStrategy(AgencyShipmentContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			this.container = container;
		}

		readonly AgencyShipmentContainer container;

		public OrgAddress CalculateReleaseContainerYard()
		{
			if (container.Booking is not AgencyShipment shipment
				|| shipment.Principal is not OrgHeader carrier
				|| container.Container is not RefContainer refContainer
				|| refContainer.RC_StorageClass.IsEmpty)
			{
				return null;
			}

			return carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress(shipment.JS_RL_NKOrigin, refContainer);
		}

		public OrgAddress CalculateReturnContanierYard()
		{
			if (container.Booking is not AgencyShipment shipment
				|| shipment.Principal is not OrgHeader carrier
				|| container.Container is not RefContainer refContainer
				|| refContainer.RC_StorageClass.IsEmpty)
			{
				return null;
			}

			return carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress(shipment.JS_RL_NKDestination, refContainer);
		}

		public (ZDateTime returnedBy, ZDateTime availableDate, ZString availableDateName) CalculateRequiredBy()
		{
			AgencyShipment shipment = container.Booking;

			if (shipment == null)
			{
				return (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);
			}

			var filter = new ContainerPenaltyMatchFilter
			{
				Carrier = shipment.Principal,
				Client = shipment.Consignee,
				OriginPort = shipment.JS_RL_NKOrigin,
				DetentionPort = shipment.JS_RL_NKDestination,
				ContainerClass = container?.Container?.RC_StorageClass ?? ZString.Empty,
				Direction = ContainerDetentionDirection.Import,
				ProcessType = Core.Constants.ContainerPenaltyProcessType.Import,
				Container = container,
				Company = GetLoggedCompany(shipment)
			};
			var detention = PenaltyMatcherFactory.MatchDetention(filter);

			var (availableDate, availableDateName) = CalculateAvailableDateForDetention(detention);

			var returnByDate = ZDateTime.Empty;
			if (availableDate.IsValid && detention != null)
			{
				returnByDate = availableDate.AddDays(Math.Max(detention.FreeDays - 1, 0));
			}

			return (returnByDate, availableDate, availableDateName);
		}

		static GlbCompany GetLoggedCompany(AgencyShipment shipment)
		{
			if (GlbStaff.CurrentUser.GS_Code != User.ServiceUserCode)
			{
				return GlbCompany.CurrentCompany;
			}

			var log = shipment.Sailing?.Voyage?.Logs?.MostRecentLogByPostedTime(AutoEvents.CargoAvailable);
			if (log == null)
			{
				return GlbCompany.CurrentCompany;
			}

			var factory = shipment.Factory;
			if (!log.IsCancelled)
			{
				var staff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, log.SL_GS_NKUser));
				return (staff.HomeBranch ?? staff.LastLogonBranch)?.Company ?? GlbCompany.CurrentCompany;
			}

			return GlbCompany.CurrentCompany;
		}

		(ZDateTime availableDate, ZString availableDateName) CalculateAvailableDateForDetention(IContainerPenaltyMatchResult detention)
		{
			var freeDayType = detention == null ? Core.Constants.ContainerDetentionFreeDayType.CTOAvailable : detention.FreeDayType.ToString();
			AgencyShipment shipment = container.Booking;
			Transport transport = shipment == null ? null : shipment.TransportsIncludingRelated.ArrivalTransport;

			return freeDayType switch
			{
				Core.Constants.ContainerDetentionFreeDayType.CTOAvailable when (transport == null || !transport.JW_TerminalAvailabilityDate.IsValid)
					=> (ZDateTime.Empty, Res.GetString("bd7f1f89-c3a1-45e7-8da3-d9a4521d0f3c", "CTO Available")),
				Core.Constants.ContainerDetentionFreeDayType.CTOAvailable
					=> (transport.JW_TerminalAvailabilityDate, Res.GetString("bd7f1f89-c3a1-45e7-8da3-d9a4521d0f3c", "CTO Available")),
				Core.Constants.ContainerDetentionFreeDayType.CTOGateOut
					=> (container.JC_FCLWharfGateOut, Res.GetString("6e9acdd2-f184-4b6c-985a-989342c9029f", "CTO Gate Out")),
				Core.Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload when container.JC_FCLUnloadFromVessel.IsEmpty
					=> (ZDateTime.Empty, Res.GetString("b03a5ba0-9c37-4e8a-bfde-0d982b508681", "Day after FCL Unload")),
				Core.Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload
					=> (container.JC_FCLUnloadFromVessel.AddDays(1), Res.GetString("b03a5ba0-9c37-4e8a-bfde-0d982b508681", "Day after FCL Unload")),
				Core.Constants.ContainerDetentionFreeDayType.FCLUnload
					=> (container.JC_FCLUnloadFromVessel, Res.GetString("06b3f394-2a54-42e9-b953-713c17f49ab3", "FCL Unload")),
				Core.Constants.ContainerDetentionFreeDayType.VesselArrival when transport == null
					=> (ZDateTime.Empty, Res.GetString("57fabef3-e3a0-4d90-b639-94bd4a45544a", "Vessel Arrival")),
				Core.Constants.ContainerDetentionFreeDayType.VesselArrival
					=> (transport.JW_ATA, Res.GetString("57fabef3-e3a0-4d90-b639-94bd4a45544a", "Vessel Arrival")),
				_	=> (ZDateTime.Empty, ZString.Empty)
			};
		}

		public (ZDateTime storageStart, ZDateTime availableDate, ZString availableDateName) CalculateStorageStart(ZString direction)
		{
			return (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);
		}

		public ZDateTime CalculateStorageStartAvailableDate(ZString direction, ZString creditorType)
		{
			return ZDateTime.Empty;
		}

		public ContainerPenaltyDate CalculateAvailableDateForStorage(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment = null)
		{
			return ContainerPenaltyDate.Empty;
		}

		public ContainerPenaltyDate CalculateAvailableDateForDetention(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			return ContainerPenaltyDate.Empty;
		}

		public ContainerPenaltyDate CalculateAvailableDateForMDD(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			return ContainerPenaltyDate.Empty;
		}

		public IEnumerable<IContainerPenaltyMatchResult> CalculateMatchedPenalties(ZString processType, CommonShipment shipment = null)
		{
			return Enumerable.Empty<IContainerPenaltyMatchResult>();
		}

		public IContainerPenaltyMatchResult GetMatchedStoragePenalty(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment = null)
		{
			return null;
		}

		public IContainerPenaltyMatchResult GetMatchedDetentionPenalty(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			return null;
		}

		public IContainerPenaltyMatchResult GetMatchedMDDPenalty(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null)
		{
			return null;
		}

		IContainerPenaltyMatcherFactory PenaltyMatcherFactory => penaltyMatcherFactory ??= new ContainerPenaltyMatcherFactory();
		IContainerPenaltyMatcherFactory penaltyMatcherFactory;
	}
}
