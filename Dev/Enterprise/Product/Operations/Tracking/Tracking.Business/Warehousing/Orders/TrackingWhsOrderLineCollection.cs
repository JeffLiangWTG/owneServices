using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderLineCollection : NonPersistentBusinessObjectCollection<TrackingWhsOrderLine>
	{
		public TrackingWhsOrderLineCollection(TrackingWhsOrder order) : this(order, order.WhsOrder.Factory) { }

		public TrackingWhsOrderLineCollection(TrackingWhsOrder order, BusinessObjectFactory factory) : base(factory)
		{
			whsOrder = order.WhsOrder;
		}

		protected WhsOrder whsOrder;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return TrackingHelper.Get(whsOrder.Lines.AddNew());
		}

		public override void Load(ZQuery filter)
		{
			foreach (WhsOrderLine declaration in whsOrder.Lines)
			{
				Add(TrackingHelper.Get(declaration));
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			var line = ((TrackingWhsOrderLine)businessObject).WhsOrderLine;
			if (line != null && !whsOrder.Lines.Contains(line))
			{
				whsOrder.Lines.Add(line);
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);

			var line = ((TrackingWhsOrderLine)elementToRemove).WhsOrderLine;
			if (line != null && whsOrder.Lines.Contains(line))
			{
				whsOrder.Lines.RemoveFromRelationship(line);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);

			var line = ((TrackingWhsOrderLine)elementToDelete).WhsOrderLine;
			if (line != null)
			{
				if (whsOrder.Lines.Contains(line))
				{
					whsOrder.Lines.Delete(line);
				}
				else if (!line.IsDeleted)
				{
					line.Delete();
				}
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			var line = ((TrackingWhsOrderLine)bizO).WhsOrderLine;
			if (line != null && whsOrder.Lines.Contains(line))
			{
				whsOrder.Lines.RemoveFromRelationship(line);
			}
		}
	}
}
