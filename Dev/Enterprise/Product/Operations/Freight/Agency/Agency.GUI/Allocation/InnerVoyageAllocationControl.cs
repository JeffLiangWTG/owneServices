using System;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class InnerVoyageAllocationControl : ZUserControl
	{
		public InnerVoyageAllocationControl()
		{
			InitializeComponent();

			this.allocationMethodNotSetLabel.Text = Res.GetString("InnerVoyageAllocationControl|AllocationMethodNotSet",
@"Allocations cannot be specified for this Sailing Schedule because the Allocation Method is not set.
To set it, change Allocation Method on ""Allocation Method"" tab from ""Not Set"" to the method of your choice.");

			this.allocationMethodIgnore.Text = Res.GetString("InnerVoyageAllocationControl|AllocationMethodIgnore", "Allocations cannot be specified for this Sailing Schedule because Allocation Method is set to \"Ignore\".");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Principal != null)
			{
				Principal.VoyageCountry.J0_AllocationMethodInfo.ValueChanged -= new EventHandler(J0_AllocationMethodInfo_ValueChanged);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (Principal != null)
			{
				Principal.VoyageCountry.J0_AllocationMethodInfo.ValueChanged += new EventHandler(J0_AllocationMethodInfo_ValueChanged);
			}

			AllocationMethodChanged();
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (Principal != null)
					{
						Principal.VoyageCountry.J0_AllocationMethodInfo.ValueChanged -= new EventHandler(J0_AllocationMethodInfo_ValueChanged);
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			Principal.RefreshUsageData();
		}

		void AllocationMethodChanged()
		{
			allocationMethodNotSetLabel.Visible = false;
			allocationMethodIgnore.Visible = false;
			countryGroupBox.Visible = false;
			originGroupBox.Visible = false;
			sailingsGroupBox.Visible = false;
			sailingUsageSplitPanel.Panel2Collapsed = false;

			if (Principal != null)
			{
				switch (Principal.VoyageCountry.J0_AllocationMethod)
				{
					case AllocationMethodList.Codes.NotSet:
						allocationMethodNotSetLabel.Visible = true;
						break;
					case AllocationMethodList.Codes.Ignore:
						allocationMethodIgnore.Visible = true;
						break;
					case AllocationMethodList.Codes.Country:
						countryGroupBox.Visible = true;
						break;
					case AllocationMethodList.Codes.Origin:
						originGroupBox.Visible = true;
						break;
					case AllocationMethodList.Codes.Sailing:
						sailingsGroupBox.Visible = true;
						sailingUsageSplitPanel.Panel2Collapsed = true;
						break;
				}
			}
			else
			{
				allocationMethodNotSetLabel.Visible = true;
			}
		}

		void J0_AllocationMethodInfo_ValueChanged(object sender, EventArgs e)
		{
			AllocationMethodChanged();
		}

		AgencyPrincipal Principal
		{
			get { return (AgencyPrincipal)CurrentDataItem; }
		}
	}
}


