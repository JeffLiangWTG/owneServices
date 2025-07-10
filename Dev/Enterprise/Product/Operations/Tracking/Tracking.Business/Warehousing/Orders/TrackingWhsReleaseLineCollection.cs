using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReleaseLineCollection : NonPersistentBusinessObjectCollection<TrackingWhsReleaseLine>
	{
		public TrackingWhsReleaseLineCollection(WhsReleaseLineCollection releaseLines, PartAttributeManager attributeManager, int decimalPlaces = 2)
			: this(releaseLines.Factory)
		{
			AddRange(releaseLines, attributeManager, decimalPlaces);
		}

		public TrackingWhsReleaseLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region AddRange

		void AddRange(WhsReleaseLineCollection releaseLines, PartAttributeManager attributeManager, ZInt decimalPlaces)
		{
			foreach (WhsReleaseLine releaseLine in releaseLines)
			{
				Add(new TrackingWhsReleaseLine(releaseLine, attributeManager, decimalPlaces));
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TrackingWhsReleaseLine();
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
