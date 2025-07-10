using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class EntryGrid : ZGrid
	{
		public EntryGrid()
		{
			FetchingForView += EntryGrid_FetchingForView;
		}

		void EntryGrid_FetchingForView(object sender, FetchForViewEventArgs e)
		{
			// FetchHint for RateLines for visible RateEntry rows.
			foreach (var entry in e.BusinessObjects)
			{
				entry.Factory.AddFetchHint(RateLinesSchema.TL_TI, entry.PK);
			}
		}

		public RateEntryCollection EntryCollection
		{
			get { return (RateEntryCollection)List; }
		}

		public RateEntry CurrentEntry
		{
			get
			{
				if (EntryCollection == null || CurrentRowIndex < 0 || CurrentRowIndex >= EntryCollection.Count)
				{
					return null;
				}
				else
				{
					return EntryCollection[CurrentRowIndex];
				}
			}
		}

		protected override int HandleDelete(int clickedRow)
		{
			using (EntryCollection.SuspendListChanged())
			{
				return base.HandleDelete(clickedRow);
			}
		}

		#region Drag-n-Drop

		public override bool AllowDrop
		{
			get { return !ReadOnly; }
		}

#if DEBUG
		internal void OnDragDrop_ForTest(DragEventArgs e)
		{
			OnDragDrop(e);
		}
#endif

		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);

			if (EntryCollection != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				var objectList = (ArrayList)e.Data.GetData(typeof(ArrayList));
				foreach (BusinessObject item in objectList)
				{
					if (item is RateEntry && !EntryCollection.Contains(item) && ((RateEntry)item).TI_RateCategory == EntryCollection.RateEntryType)
					{
						var newEntry = ((RateEntry)item).DeepClone(EntryCollection);

						if (newEntry != null)
						{
							newEntry.TI_CreationSource = RateEntryCreator.Sources.Manual;
							newEntry.RunPreSaveValidation();
							EntryCollection.RefreshBindingIncludingChildren();
						}
					}
				}
			}
		}

#if !WINZOR

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);

			if (EntryCollection != null && e.Data.GetDataPresent(typeof(ArrayList)))
			{
				ArrayList objectList = (ArrayList)e.Data.GetData(typeof(ArrayList));
				foreach (BusinessObject @object in objectList)
				{
					if (@object is RateEntry && !EntryCollection.Contains(@object) && ((RateEntry)@object).TI_RateCategory == EntryCollection.RateEntryType)
					{
						e.Effect = DragDropEffects.Copy;
						break;
					}
				}
			}
		}

#endif

		#endregion
	}
}
