using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public abstract class QuickAddressHelperStrategy
	{
		protected QuickAddressHelperStrategy(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; private set; }

		public abstract CartageBindToLists.AddressSelectionElement[] GetAddressElements();
		public abstract ZPropertyInfo Info { get; }
		public abstract IDocAddresses Parent { get; }
		public abstract DocAddressType DefaultAddressType { get; }
	}

	public class PickupLegQuickAddressHelperStrategy : QuickAddressHelperStrategy
	{
		public PickupLegQuickAddressHelperStrategy(CommonCartageLeg leg)
			: base(leg.Factory)
		{
			Leg = leg;
		}
		readonly CommonCartageLeg Leg;

		public override CartageBindToLists.AddressSelectionElement[] GetAddressElements() { return Leg.Lookups.GetCartageAddressElements(); }
		public override ZPropertyInfo Info { get { return Leg.JU_E2PickupAddressIDInfo; } }
		public override IDocAddresses Parent { get { return Leg.Cartage; } }
		public override DocAddressType DefaultAddressType { get { return Leg.PickupDocAddressType; } }
	}

	public class WaitPointLegQuickAddressHelperStrategy : QuickAddressHelperStrategy
	{
		public WaitPointLegQuickAddressHelperStrategy(CommonCartageLeg leg)
			: base(leg.Factory)
		{
			Leg = leg;
		}
		readonly CommonCartageLeg Leg;

		public override CartageBindToLists.AddressSelectionElement[] GetAddressElements() { return Leg.Lookups.GetCartageAddressElements(); }
		public override ZPropertyInfo Info { get { return Leg.JU_E2WaitPointAddressIDInfo; } }
		public override IDocAddresses Parent { get { return Leg.Cartage; } }
		public override DocAddressType DefaultAddressType { get { return Leg.WaitPointDocAddressType; } }
	}

	public class DeliveryLegQuickAddressHelperStrategy : QuickAddressHelperStrategy
	{
		public DeliveryLegQuickAddressHelperStrategy(CommonCartageLeg leg)
			: base(leg.Factory)
		{
			Leg = leg;
		}
		readonly CommonCartageLeg Leg;

		public override CartageBindToLists.AddressSelectionElement[] GetAddressElements() { return Leg.Lookups.GetCartageAddressElements(); }
		public override ZPropertyInfo Info { get { return Leg.JU_E2DeliveryAddressIDInfo; } }
		public override IDocAddresses Parent { get { return Leg.Cartage; } }
		public override DocAddressType DefaultAddressType { get { return Leg.DeliverToDocAddressType; } }
	}

	public class FirstBookingQuickAddressHelperStrategy : QuickAddressHelperStrategy
	{
		public FirstBookingQuickAddressHelperStrategy(CommonBookedCtgMove booking)
			: base(booking.Factory)
		{
			Booking = booking;
		}
		readonly CommonBookedCtgMove Booking;

		public override CartageBindToLists.AddressSelectionElement[] GetAddressElements() { return Booking.Lookups.GetCartageAddressElements(); }
		public override ZPropertyInfo Info { get { return Booking.EW_E2PickupAddressIDInfo; } }
		public override IDocAddresses Parent { get { return Booking.Cartage; } }
		public override DocAddressType DefaultAddressType { get { return Booking.PickupDocAddressType; } }
	}

	public class SecondBookingQuickAddressHelperStrategy : QuickAddressHelperStrategy
	{
		public SecondBookingQuickAddressHelperStrategy(CommonBookedCtgMove booking)
			: base(booking.Factory)
		{
			Booking = booking;
		}
		readonly CommonBookedCtgMove Booking;

		public override CartageBindToLists.AddressSelectionElement[] GetAddressElements() { return Booking.Lookups.GetCartageAddressElements(); }
		public override ZPropertyInfo Info { get { return Booking.EW_E2WaitPointAddressIDInfo; } }
		public override IDocAddresses Parent { get { return Booking.Cartage; } }
		public override DocAddressType DefaultAddressType { get { return Booking.WaitPointDocAddressType; } }
	}
}
