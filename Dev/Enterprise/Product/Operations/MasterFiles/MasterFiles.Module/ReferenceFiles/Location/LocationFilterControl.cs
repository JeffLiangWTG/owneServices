using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class LocationFilterControl : ZFilterStripControl
	{
		internal const string StateColumnKey = "State+RW_DescriptionMultilingual";

		public LocationFilterControl(IBusinessObjectCollection gridCollection, LocationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			this.LocationTypeFilter = ((ModuleTextFilter)filterBusinessObject["Location Type"]);
			LocationTypeFilter.PropertyInfo.ValueChanged += new EventHandler(LocationType_ValueChanged);
		}

		#region State Column

		void LocationType_ValueChanged(object sender, EventArgs e)
		{
			if (FilteredGrid.Columns[StateColumnKey] != null)
			{
				FilteredGrid.Columns[StateColumnKey].IsVisible = (LocationTypeFilter.Property == Core.Constants.LocationTypes.Codes.Port);
				FilteredGrid.RefreshTableStyles();
			}
		}

		readonly ModuleTextFilter LocationTypeFilter;

		#endregion
	}
}
