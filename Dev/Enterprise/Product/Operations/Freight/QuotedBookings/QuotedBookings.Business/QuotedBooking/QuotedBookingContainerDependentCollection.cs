using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingContainerDependentCollection : QuotedBookingContainerDependentCollection<ForwardingContainer>
	{
		public QuotedBookingContainerDependentCollection(ForwardingShipment quotedbooking, BusinessObjectFactory factory)
			: base(quotedbooking, factory)
		{
		}

		public QuotedBookingContainerDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var container = child as ForwardingContainer;
			if (container != null
				&& Master != null)
			{
				var parentAllocationLine = Master.JS_RCA_BookingAllocationLine;
				if (!parentAllocationLine.IsEmpty)
				{
					container.JC_RCA_AllocationLine = parentAllocationLine;
				}
			}
		}
	}

	public abstract class QuotedBookingContainerDependentCollection<T> : DependentBusinessObjectCollection<T, ForwardingShipment>
		where T : ForwardingContainer
	{
		protected QuotedBookingContainerDependentCollection(ForwardingShipment quotedbooking, BusinessObjectFactory factory)
			: base(quotedbooking, factory)
		{
		}

		protected QuotedBookingContainerDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobContainerSchema.JC_JS_FCLBookingOnlyLink; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (!CommodityCode.IsEmpty)
			{
				var container = (T)child;
				container.JC_RH_NKContainerCommodityCode = CommodityCode;
			}
		}

		internal ZString CommodityCode { get; set; }
	}
}
