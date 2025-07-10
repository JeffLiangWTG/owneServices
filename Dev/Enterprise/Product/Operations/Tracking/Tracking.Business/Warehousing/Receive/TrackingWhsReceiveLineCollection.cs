using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReceiveLineCollection : NonPersistentBusinessObjectCollection<TrackingWhsReceiveLine>
	{
		public TrackingWhsReceiveLineCollection(TrackingWhsReceive receive)
			: base(receive.WhsReceive.Factory)
		{
			whsReceive = receive.WhsReceive;
		}

		protected WhsReceive whsReceive;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return TrackingHelper.Get(whsReceive.Lines.AddNew());
		}

		public override void Load()
		{
			RemoveAll();

			foreach (WhsReceiveLine line in whsReceive.Lines)
			{
				Add(TrackingHelper.Get(line));
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			var line = ((TrackingWhsReceiveLine)businessObject).WhsReceiveLine;
			if (line != null && !whsReceive.Lines.Contains(line))
			{
				whsReceive.Lines.Add(line);
			}
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			base.Remove(elementToRemove);

			var line = ((TrackingWhsReceiveLine)elementToRemove).WhsReceiveLine;
			if (line != null && whsReceive.Lines.Contains(line))
			{
				whsReceive.Lines.RemoveFromRelationship(line);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);

			var line = ((TrackingWhsReceiveLine)elementToDelete).WhsReceiveLine;
			if (line != null)
			{
				if (whsReceive.Lines.Contains(line))
				{
					whsReceive.Lines.Delete(line);
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

			var line = ((TrackingWhsReceiveLine)bizO).WhsReceiveLine;
			if (line != null && whsReceive.Lines.Contains(line))
			{
				whsReceive.Lines.RemoveFromRelationship(line);
			}
		}
	}
}
