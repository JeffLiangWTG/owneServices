using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobOrderHeaderLookups : AutoJobOrderHeaderLookups
	{
		public JobOrderHeaderLookups(AutoJobOrderHeader parent)
			: base(parent)
		{
		}

		public JobShipmentPreplanningCollection ShipmentPrePlannings
		{
			get { return new OrderPreAdviseCollection(Factory, Parent); }
		}

		#region Buyers

		public virtual OrgHeaderCollection Buyers
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		#endregion

		#region Suppliers

		public virtual OrgHeaderCollection Suppliers
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		#endregion

		#region Implementation

		internal class OrderPreAdviseCollection : JobShipmentPreplanningCollection
		{
			public OrderPreAdviseCollection(BusinessObjectFactory factory, Order parent)
				: base(factory)
			{
				this.parent = parent;
			}

			protected override ZQuery CreateAdditionalFilter()
			{
				return parent.Buyer == null ? new ZQuery() : new ZQuery(JobShipmentPreplanningSchema.EF_OA_BuyerAddress, parent.Buyer.Addresses.Select(x => x.PK));
			}

			protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

				JobShipmentPreplanning preAdvice = (JobShipmentPreplanning)selectedBusinessObject;

				if (!parent.JD_OA_BuyerAddress.IsEmpty && preAdvice.BuyerPK != parent.BuyerPK)
				{
					errors.Add(Res.GetString("3074df4e-1321-42e8-bfa3-3f46fbfca89a", "You can only choose Pre Advices that are for the same buyer as the Order."));
				}
			}

			readonly Order parent;
		}

		protected new Order Parent
		{
			get { return (Order)base.Parent; }
		}

		#endregion
	}
}
