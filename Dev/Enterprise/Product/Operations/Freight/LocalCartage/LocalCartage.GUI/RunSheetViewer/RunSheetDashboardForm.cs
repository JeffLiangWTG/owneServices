using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class RunSheetDashboardForm : ZChildForm
	{
		public RunSheetDashboardForm(RunSheetDashboardCollection dashboards)
			: base(dashboards)
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Dashboards != null)
			{
				Dashboards.CountChanged -= Dashboards_CountChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (Dashboards != null)
			{
				Dashboards.CountChanged += Dashboards_CountChanged;
			}

			OnDashboardChanged();
		}

		void Dashboards_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			OnDashboardChanged();
		}

		void OnDashboardChanged()
		{
			if (previousDashBoard != null)
			{
				previousDashBoard.FilterChanged -= Dashboard_FilterChanged;
				previousDashBoard = null;
			}

			if (Dashboard != null)
			{
				previousDashBoard = Dashboard;
				Dashboard.FilterChanged += Dashboard_FilterChanged;
			}
		}

		RunSheetDashboard previousDashBoard;

		void Dashboard_FilterChanged(object sender, EventArgs e)
		{
			Text = FormCaption;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("91080fe2-118e-463c-9e4e-4d91ee4d3716", "Run Sheet Dashboard");

				if (!Dashboard.DateRangeFilter.IsEmpty)
				{
					caption += string.Format(CultureInfo.CurrentCulture, " - {0}", Dashboard.DateRangeFilterMultilingualString);
				}

				if (!Dashboard.BranchName.IsEmpty)
				{
					caption += string.Format(CultureInfo.CurrentCulture, " - {0}", Dashboard.BranchName);
				}

				return caption;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		RunSheetDashboardCollection Dashboards
		{
			get { return (RunSheetDashboardCollection)DataSource; }
		}

		RunSheetDashboard Dashboard
		{
			get { return Dashboards != null && Dashboards.Any() ? Dashboards[0] : null; }
		}
	}
}
