using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderSummaryLineCollection : NonPersistentBusinessObjectCollection<TrackingWhsOrderSummaryLine>, IObsoleteValidation
	{
		#region Constructors

		public TrackingWhsOrderSummaryLineCollection(TrackingWhsOrderLineCollection lines, BusinessObjectFactory factory)
			: this(factory)
		{
			AddRange(lines);
		}

		public TrackingWhsOrderSummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TrackingWhsOrderSummaryLine(Factory);
		}

		public override void Add(BusinessObject businessObject)
		{
			// To avoid CA1800 code analysis warning.
			var trackingLine = businessObject as TrackingWhsOrderLine;

			if (trackingLine != null)
			{
				businessObject = new TrackingWhsOrderSummaryLine(trackingLine);
			}

			TrackingWhsOrderSummaryLine lineToAdd = businessObject as TrackingWhsOrderSummaryLine;
			if (lineToAdd != null)
			{
				foreach (TrackingWhsOrderSummaryLine line in this)
				{
					if (line.ProductPK == lineToAdd.ProductPK && line.PacksUQ == lineToAdd.PacksUQ && line.UQ == lineToAdd.UQ)
					{
						line.PacksQuantity += lineToAdd.PacksQuantity;
						line.OrderedQuantity += lineToAdd.OrderedQuantity;
						line.ReservedQuantity += lineToAdd.ReservedQuantity;
						return;
					}
				}
				base.Add(lineToAdd);
			}
		}

		#endregion
	}
}
