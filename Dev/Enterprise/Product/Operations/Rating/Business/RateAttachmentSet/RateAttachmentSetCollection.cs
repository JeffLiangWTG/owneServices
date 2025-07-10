using System.Collections;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class RateAttachmentSetCollection : BusinessObjectCollection<RateAttachmentSet>
	{
		public RateAttachmentSetCollection(BusinessObjectFactory factory)
			: this(null, factory) { }
		public RateAttachmentSetCollection(Quote master, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Quote = master;
		}

		#region Load

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			base.Load(alternativeAdditionalFilter);
			Sort(new RateAttachmentComparer());

			if (Quote != null)
			{
				for (var i = Count - 1; i >= 0; i--)
				{
					if (this[i].TS_GC != Quote.TH_GC && !this[i].TS_GC.IsEmpty)
					{
						Remove(this[i].PK);
					}
				}

				for (var i = Count - 1; i >= 0; i--)
				{
					if (this[i].IsImage && !this[i].HasImage)
					{
						Remove(this[i]);
					}
				}
			}
		}

		#endregion

		#region Implementation

		readonly Quote Quote;

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return new RateAttachmentComparer();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public bool RateAttachmentSetAllowSort;

		protected override bool AllowSort
		{
			get { return RateAttachmentSetAllowSort; }
		}

		#endregion

		#region Testing Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		public bool Contains(ZString attachmentName)
		{
			foreach (RateAttachmentSet set in this)
			{
				if (set.TS_AttachmentName == attachmentName)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}

