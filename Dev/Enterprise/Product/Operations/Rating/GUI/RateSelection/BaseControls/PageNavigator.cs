using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using Enterprise.Rating.GUI.RateSelection.HelperClasses;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.BaseControls
{
	public partial class PageNavigator : ZUserControl
	{
		public PageNavigator()
		{
			InitializeComponent();

			lblPageNumber.Text = Res.GetString("1F2F0044-F6A4-4042-90C1-F04C08D8DFB5", "Page Number");

#if DEBUG
			TypeDescriptor.AddAttributes(lblPageNumber, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		PagedList data;

		public event EventHandler<PageChangedEventArgs> PageChanged;

		protected virtual void OnPageChanged()
		{
			if (data != null)
			{
				btnPreviousPage.Enabled = !data.IsFirstPage;
				btnNextPage.Enabled = !data.IsLastPage;
				lblPageNumber.Text = Res.GetString("BC94CB7E-7A84-4AEA-9F90-1EF370C99217", "Page {0} of {1}", data.CurrentPage, data.TotalPageCount);

				if (data.TotalPageCount > 0)
				{
					cbPageNumberIsChangedFromUI = false;
					cbPageNumber.SelectedIndex = data.CurrentPage - 1;
					cbPageNumberIsChangedFromUI = true;
				}

				PageChanged?.Invoke(this, new PageChangedEventArgs(data.GetCurrentPage()));
			}
		}

		public int MaxPageSize
		{
			get
			{
				return maxPageSize;
			}
			set
			{
				if (value != maxPageSize)
				{
					if (value % 5 > 0)
					{
						throw new Exception("MaxPageSize should be divisible by 5.");
					}

					maxPageSize = value;
					SetPageSizeOptions(maxPageSize);
				}
			}
		}
		int maxPageSize = 40;

		//PageNavigator is bindable, it can be binded to any IEnumerable data in its parent control. 
		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem is IEnumerable enumerableData)
			{
				SetData(enumerableData);
			}
		}

		public void SetData(IEnumerable allData)
		{
			maxPageSize = maxPageSize > 0 ? maxPageSize : 40;
			data = new PagedList(allData, maxPageSize);
			SetPageSizeOptions(maxPageSize);
			SetPageNumberOptions();
			OnPageChanged();
		}

		void btnFirstPage_Click(object sender, EventArgs e)
		{
			if (data != null)
			{
				data.GotoPage(1);
				OnPageChanged();
			}
		}

		void btnLastPage_Click(object sender, EventArgs e)
		{
			if (data != null)
			{
				data.GotoPage(data.TotalPageCount);
				OnPageChanged();
			}
		}

		void btnPreviousPage_Click(object sender, EventArgs e)
		{
			if (data != null)
			{
				data.PreviousPage();
				OnPageChanged();
			}
		}

		void btnNextPage_Click(object sender, EventArgs e)
		{
			if (data != null)
			{
				data.NextPage();
				OnPageChanged();
			}
		}

		void SetPageSizeOptions(int maxSize)
		{
			cbPageSizeIsChangedFromUI = false;

			cbPageSize.Items.Clear();
			foreach (var item in Enumerable.Range(1, maxSize / 5).Select(i => i * 5))
			{
				cbPageSize.Items.Add(item);
			}

			cbPageSize.SelectedIndex = cbPageSize.Items.Count - 1;
			cbPageSizeIsChangedFromUI = true;
		}

		void SetPageNumberOptions()
		{
			if (data != null)
			{
				cbPageNumberIsChangedFromUI = false;

				cbPageNumber.Items.Clear();
				for (var i = 1; i <= data.TotalPageCount; i++)
				{
					cbPageNumber.Items.Add(i);
				}

				cbPageNumberIsChangedFromUI = true;
			}
		}

		void cbPageSize_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cbPageSizeIsChangedFromUI && !string.IsNullOrEmpty(cbPageSize.Text) && data != null)
			{
				data.ChangePageSize(int.Parse(cbPageSize.Text));
				SetPageNumberOptions();
				OnPageChanged();
			}
		}
		bool cbPageSizeIsChangedFromUI = true;

		void cbPageNumber_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cbPageNumberIsChangedFromUI && !string.IsNullOrEmpty(cbPageNumber.Text) && data != null)
			{
				data.GotoPage(int.Parse(cbPageNumber.Text));
				OnPageChanged();
			}
		}
		bool cbPageNumberIsChangedFromUI = true;
	}

	public class PageChangedEventArgs : EventArgs
	{
		public PageChangedEventArgs(IEnumerable pageData)
		{
			PageData = pageData;
		}
		public IEnumerable PageData { get; }
	}
}
