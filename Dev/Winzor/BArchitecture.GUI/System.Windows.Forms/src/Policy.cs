using System.ComponentModel;

namespace System.Windows.Forms
{
	public partial class DataGrid
	{
		/// <summary>
		///  Holds policy information for what the grid can and cannot do.
		/// </summary>
		private class Policy
		{
			private bool allowAdd = true;
			private bool allowEdit = true;
			private bool allowRemove = true;

			public Policy()
			{
			}

			public bool AllowAdd
			{
				get
				{
					return allowAdd;
				}
				set
				{
					if (allowAdd != value)
					{
						allowAdd = value;
					}
				}
			}

			public bool AllowEdit
			{
				get
				{
					return allowEdit;
				}
				set
				{
					if (allowEdit != value)
					{
						allowEdit = value;
					}
				}
			}

			public bool AllowRemove
			{
				get
				{
					return allowRemove;
				}
				set
				{
					if (allowRemove != value)
					{
						allowRemove = value;
					}
				}
			}

			// returns true if the UI needs to be updated (here because addnew has changed)
			public bool UpdatePolicy(CurrencyManager listManager, bool gridReadOnly)
			{
				bool change = false;
				// only IBindingList can have an AddNewRow
				IBindingList bl = listManager == null ? null : listManager.List as IBindingList;
				if (listManager == null)
				{
					if (!allowAdd)
					{
						change = true;
					}

					allowAdd = allowEdit = allowRemove = true;
				}
				else
				{
					if (AllowAdd != listManager.AllowAdd && !gridReadOnly)
					{
						change = true;
					}

					AllowAdd = listManager.AllowAdd && !gridReadOnly && bl != null && bl.SupportsChangeNotification;
					AllowEdit = listManager.AllowEdit && !gridReadOnly;
					AllowRemove = listManager.AllowRemove && !gridReadOnly && bl != null && bl.SupportsChangeNotification;     //
				}
				return change;
			}
		}
	}
}
