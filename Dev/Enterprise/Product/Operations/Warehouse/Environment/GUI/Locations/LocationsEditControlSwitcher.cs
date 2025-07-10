using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.GUI
{
	public partial class LocationsEditControlSwitcher : ZUserControl
	{
		public LocationsEditControlSwitcher()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SetupCountryDependentControls();
			}
		}

		#region Related Objects

		public WhsRow Row
		{
			get { return (WhsRow)CurrentDataItem; }
		}

		#endregion

		#region Implementation

		LocationsEditBaseControl GetNewLocationEditControl()
		{
			switch (Row.CountryCode)
			{
				case Enterprise.Core.Constants.CountryCodes.UnitedStates:
					return new US.LocationsEditUSControl();

				default:
					return new LocationsEditBaseControl();
			}
		}

		void SetupCountryDependentControls()
		{
			this.Controls.Remove(this.rowLocationsEditControl);

			this.rowLocationsEditControl = GetNewLocationEditControl();
			this.rowLocationsEditControl.BindTo = "Locations";
			this.rowLocationsEditControl.DataSourceAssemblyName = "Enterprise.Warehouse.Environment.Business";
			this.rowLocationsEditControl.DataSourceTypeName = "Enterprise.Warehouse.Environment.Business.WhsRow";
			this.rowLocationsEditControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rowLocationsEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.rowLocationsEditControl.Name = "rowLocationsEditControl";
			this.rowLocationsEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 513);
			this.rowLocationsEditControl.TabIndex = 0;
			this.rowLocationsEditControl.SetDataBinding(Row, "");

			this.Controls.Add(this.rowLocationsEditControl);
		}

		#endregion

#if DEBUG

		public LocationsEditBaseControl RowLocationEditControl
		{
			get { return this.rowLocationsEditControl; }
		}

#endif
	}
}
