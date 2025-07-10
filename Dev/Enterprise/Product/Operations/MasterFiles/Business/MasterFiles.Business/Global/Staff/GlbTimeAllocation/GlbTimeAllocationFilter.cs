using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbTimeAllocationFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public GlbTimeAllocationFilter(GlbTimeAllocationCollection coll)
			: base(coll.Factory)
		{
			this.Collection = coll;
		}

		public readonly GlbTimeAllocationCollection Collection;

		#region View

		public GlbTimeAllocationCollectionView AllocationsView
		{
			get
			{
				if (fAllocationsView == null)
				{
					fAllocationsView = new GlbTimeAllocationCollectionView(this);
					fAllocationsView.CollectionToFilter.Load();
				}
				return fAllocationsView;
			}
		}

		GlbTimeAllocationCollectionView fAllocationsView;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			startTime = ZDateTime.Today.AddDays(-7);
			endTime = ZDateTime.Today.AddDays(21);
		}

		#endregion

		#region Properties

		#region Start Time

		public ZDateTime StartTime
		{
			get { return startTime; }
			set
			{
				if (startTime != value)
				{
					SetNonPersistentPropertyValue(StartTimeInfo, ref startTime, value);
					AllocationsView.Rebuild();
				}
			}
		}

		ZDateTime startTime;

		public ZPropertyInfo StartTimeInfo
		{
			get { return GetZPropertyInfo(nameof(StartTime)); }
		}

		#endregion

		#region End Time

		public ZDateTime EndTime
		{
			get { return endTime; }
			set
			{
				if (endTime != value)
				{
					SetNonPersistentPropertyValue(EndTimeInfo, ref endTime, value);
					AllocationsView.Rebuild();
				}
			}
		}

		ZDateTime endTime;

		public ZPropertyInfo EndTimeInfo
		{
			get { return GetZPropertyInfo(nameof(EndTime)); }
		}

		#endregion

		#endregion
	}
}
