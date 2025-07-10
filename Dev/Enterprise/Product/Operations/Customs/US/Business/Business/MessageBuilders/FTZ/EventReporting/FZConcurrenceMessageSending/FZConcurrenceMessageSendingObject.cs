
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZConcurrenceMessageSendingObject : AutoFZConcurrenceMessageSendingObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public FZConcurrenceMessageSendingObject(IFTZConcurrence concurrence)
			: base(concurrence.Factory)
		{
			using (SuspendSettingHasChanges())
			{
				MB_ConcurrenceQty = concurrence.FTZConcurrenceQty;
			}
			this.concurrence = concurrence;
		}
		readonly IFTZConcurrence concurrence;

		[List(nameof(Lookups) + "." + nameof(FZConcurrenceMessageSendingObjectLookups.UQ_List))]
		public override ZString MB_ManifestUQ
		{
			get { return base.MB_ManifestUQ; }
			set { base.MB_ManifestUQ = value; }
		}

		[List(nameof(Lookups) + "." + nameof(FZConcurrenceMessageSendingObjectLookups.UQ_List))]
		public override ZString MB_ConcurrenceUQ
		{
			get { return base.MB_ConcurrenceUQ; }
			set { base.MB_ConcurrenceUQ = value; }
		}

		public void UpdateFTZConcurrenceQtyIfNeeded()
		{
			var newQty = MB_ConcurrenceQty;
			if (newQty != concurrence.FTZConcurrenceQty)
			{
				concurrence.FTZConcurrenceQty = newQty;
			}
		}

		#region Lookups

		public FZConcurrenceMessageSendingObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected FZConcurrenceMessageSendingObjectLookups GetNewLookups()
		{
			return new FZConcurrenceMessageSendingObjectLookups(this);
		}

		FZConcurrenceMessageSendingObjectLookups fLookups;

		#endregion
	}
}
