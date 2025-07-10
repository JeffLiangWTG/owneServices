using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class VNEUserControl : ZUserControl
	{
		public VNEUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(VNEGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, System.EventArgs e)
		{
			var vehicle = this.CurrentDataItem as Vehicle;
			if (vehicle != null)
			{
				var parentForm = FindForm();
				var vneForm = new EPAEditForm(vehicle);
				if (fromProduct)
				{
					vneForm.ChangeVisibilityOfControlsForProduct();
				}
				ZFormModaliser.Show(vneForm, parentForm);
			}
		}

		public void SetPropertyForProduct()
		{
			fromProduct = true;
			vneDetailsGroupBox.Visible = false;
			this.VNEDetailsSplitContainer.Panel2Collapsed = true;
			this.VNEDetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(VNEDetailsSplitContainer.Size.Height);
			VNEGrid.RemoveFromAvailableColumns(USVehicleAddInfoSchema.Constants.US_VNEElectronicImage);
			VNEGrid.RemoveFromAvailableColumns(USVehicleAddInfoSchema.Constants.US_LineNo);
			VNEGrid.RemoveFromAvailableColumns(USVehicleAddInfoSchema.Constants.US_ContactEmail);
			VNEGrid.RemoveFromAvailableColumns(USVehicleAddInfoSchema.Constants.US_ContactName);
			VNEGrid.RemoveFromAvailableColumns(USVehicleAddInfoSchema.Constants.US_ContactPhoneNo);
			VNEGrid.RemoveFromAvailableColumns(Vehicle.Schema.US_TrackingStatusDesc);
			VNEGrid.RemoveFromAvailableColumns("Status");
			VNEGrid.RemoveFromAvailableColumns("StatusDesc");
			VNEGrid.RemoveFromAvailableColumns("StatusDate");
		}
		bool fromProduct;
	}
}
