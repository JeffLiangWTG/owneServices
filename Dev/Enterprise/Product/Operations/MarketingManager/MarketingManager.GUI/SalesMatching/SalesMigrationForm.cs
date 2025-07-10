using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesMigrationForm : ZChildForm
	{
		public SalesMigrationForm()
		{
			InitializeComponent();
		}

		public SalesMigrationForm(SalesMatching matching)
			: base(matching)
		{
			salesGrid.Init(matching);
			matchingGrid.Init(matching);
		}

		public enum MigrationResult
		{
			None,
			Overwrite,
			Append,
			UseMatchingOnly
		}

		public MigrationResult UserResult { get; private set; }
		public SalesMatchingData SelectedSalesMatchingData { get; private set; }

		SalesMatching Matching
		{
			get { return DataSource as SalesMatching; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var grid = matchingGrid.SalesGrid;
			grid.MouseDoubleClick += MatchedSalesGrid_MouseDoubleClick;

			if (Matching != null && Matching.MatchedSalesCollection.Count > 0)
			{
				grid.Select(0);
			}
		}

		void MatchedSalesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			SelectAndClose();
		}

		void OverwriteButton_Click(object sender, EventArgs e)
		{
			UserResult = MigrationResult.Overwrite;
			SelectAndClose();
		}

		void AppendButton_Click(object sender, EventArgs e)
		{
			UserResult = MigrationResult.Append;
			Close();
		}

		void UseMatchingButton_Click(object sender, EventArgs e)
		{
			UserResult = MigrationResult.UseMatchingOnly;
			SelectAndClose();
		}

		void SelectAndClose()
		{
			var grid = matchingGrid.SalesGrid;
			if (grid.SelectedRowCount == 1)
			{
				var selectedSalesData = grid.SelectedElements[0] as SalesMatchingData;
				if (selectedSalesData != null)
				{
					SelectedSalesMatchingData = selectedSalesData;
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("3937db04-3422-447b-b3f7-8790a4c92215", "Please select one row"), Res.GetString("32e7f651-aec8-46d9-ba3f-03aa246a1ddd", "Cannot Select More Than One Row"));
			}
		}
	}
}
