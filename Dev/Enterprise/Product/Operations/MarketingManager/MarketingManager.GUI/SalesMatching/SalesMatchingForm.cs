using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesMatchingForm : ZChildForm
	{
		public SalesMatchingForm()
		{
			InitializeComponent();
		}

		public SalesMatchingForm(SalesMatching matching, bool hasModeAndType = true)
			: base(matching)
		{
			MatchedSalesGrid.Init(matching);
			HasModeAndType = hasModeAndType;
		}

		SalesMatching Matching
		{
			get { return DataSource as SalesMatching; }
		}

		protected readonly bool HasModeAndType;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MatchedSalesGrid.SalesGrid.MouseDoubleClick += MatchedSalesGrid_MouseDoubleClick;

			if (!HasModeAndType)
			{
				NewModeTypeButton.SetReadOnly(true);
			}

			if (Matching != null && Matching.MatchedSalesCollection.Count > 0)
			{
				MatchedSalesGrid.SalesGrid.Select(0);
			}
		}

		void MatchedSalesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			SelectAndClose(newModeAndType: false);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Enter)
			{
				SelectAndClose(newModeAndType: false);
			}
			else if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}

		void UseExistingButton_Click(object sender, EventArgs e)
		{
			SelectAndClose(newModeAndType: false);
		}

		void NewBuyerSupplierButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void NewModeTypeButton_Click(object sender, EventArgs e)
		{
			SelectAndClose(newModeAndType: true);
		}

		void SelectAndClose(bool newModeAndType)
		{
			if (MatchedSalesGrid.SalesGrid.SelectedRowCount == 1)
			{
				var selectedSalesData = MatchedSalesGrid.SalesGrid.SelectedElements[0] as SalesMatchingData;
				if (selectedSalesData != null)
				{
					Matching.ReplaceWithClonedExisting(selectedSalesData, skipCloningDetail: newModeAndType);
					SelectedSalesMatchingData = selectedSalesData;
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("3937db04-3422-447b-b3f7-8790a4c92215", "Please select one row"), Res.GetString("32e7f651-aec8-46d9-ba3f-03aa246a1ddd", "Cannot Select More Than One Row"));
			}
		}

		public SalesMatchingData SelectedSalesMatchingData { get; private set; }
	}
}
