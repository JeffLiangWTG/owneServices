using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RecordsNavigator : ZUserControl
	{
		public RecordsNavigator()
		{
			InitializeComponent();
			CurrentNumberTextBox.LostFocus += CurrentNumberTextBox_LostFocus;
			Enabled = false;
		}

		void CurrentNumberTextBox_LostFocus(object sender, EventArgs e)
		{
			if (int.TryParse(CurrentNumberTextBox.Text.Trim(), out int newIndex))
			{
				if (newIndex > 0 && newIndex <= Collection.Count)
				{
					ListManager.Position = newIndex - 1;
				}
				else
				{
					UpdateLayouts();
				}
			}
			else
			{
				UpdateLayouts();
			}
		}

		public void BindToCurrencyManager(CurrencyManager listManager)
		{
			if (listManager != null && listManager.List != null)
			{
				if (listManager.List is IBindingList)
				{
					if (ListManager != listManager)
					{
						if (ListManager != null)
						{
							ListManager.PositionChanged -= ListManager_PositionChanged;
						}
						if (Collection != null)
						{
							Collection.ListChanged -= Collection_ListChanged;
						}
						ListManager = listManager;
						ListManager.PositionChanged += ListManager_PositionChanged;
						Collection = listManager.List as IBindingList;
						Collection.ListChanged += Collection_ListChanged;
					}
				}
				else
				{
					throw new ArgumentException("The List must implement interface \"IBindingList\"");
				}
				UpdateLayouts();
			}
			else
			{
				throw new ArgumentNullException(nameof(listManager));
			}
			if (listManager.List.Count <= 0)
			{
				ClearLayout();
			}
			IsBoundToCurrencyManager = true;
		}

		void Collection_ListChanged(object sender, ListChangedEventArgs e)
		{
			UpdateLayouts();
		}

		void ClearLayout()
		{
			TotalNumberTextBox.Text = string.Empty;
			CurrentNumberTextBox.Text = string.Empty;
			Enabled = false;
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			UpdateLayouts();
		}

		void UpdateLayouts()
		{
			TotalNumberTextBox.Text = (Collection.Count).ToString(CultureInfo.CurrentCulture);
			CurrentNumberTextBox.Text = (ListManager.Position + 1).ToString(CultureInfo.CurrentCulture);
			MoveToFirstButton.Enabled = MoveLeftButton.Enabled = ListManager.Position > 0;
			MoveToLastButton.Enabled = MoveRightButton.Enabled = ListManager.Position < Collection.Count - 1;
			Enabled = Collection.Count > 0;
		}

		CurrencyManager ListManager;
		IBindingList Collection;

		public bool IsBoundToCurrencyManager
		{
			get;
			private set;
		}

		void MoveRightButton_Click(object sender, EventArgs e)
		{
			if (ListManager.Position < Collection.Count - 1)
			{
				ListManager.Position++;
			}
			UpdateLayouts();
		}

		void MoveLeftButton_Click(object sender, EventArgs e)
		{
			if (ListManager.Position > 0)
			{
				ListManager.Position--;
			}
			UpdateLayouts();
		}

		void MoveToFirstButton_Click(object sender, EventArgs e)
		{
			ListManager.Position = 0;
			UpdateLayouts();
		}

		void MoveToLastButton_Click(object sender, EventArgs e)
		{
			ListManager.Position = Collection.Count - 1;
			UpdateLayouts();
		}
	}
}
