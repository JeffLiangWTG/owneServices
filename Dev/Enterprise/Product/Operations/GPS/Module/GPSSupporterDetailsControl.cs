using System;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GPS.Module
{
	public partial class GPSSupporterDetailsControl : ZUserControl
	{
		public GPSSupporterDetailsControl()
		{
			InitializeComponent();

			FindButton.Click += new EventHandler(FindButton_Click);
			ClearButton.Click += new EventHandler(ClearButton_Click);
		}

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			FilterFromDateEdit.DateTimeValue = ZDateTime.Now.AddMonths(-1);
			FilterToDateEdit.DateTimeValue = ZDateTime.Now;
		}

		#endregion

		#region GPSSupporter

		GPSSupporter GPSSupporter
		{
			get { return (GPSSupporter)CurrentDataItem; }
		}

		#endregion

		#region FindButton_Click

		void FindButton_Click(object sender, EventArgs e)
		{
			GPSSupporter.Activities.Load();
		}

		#endregion

		#region ClearButton_Click

		void ClearButton_Click(object sender, EventArgs e)
		{
			GPSSupporter.ActivityFilterProvider.ClearFilters();
			GPSSupporter.Activities.Load();
		}

		#endregion

	}
}
