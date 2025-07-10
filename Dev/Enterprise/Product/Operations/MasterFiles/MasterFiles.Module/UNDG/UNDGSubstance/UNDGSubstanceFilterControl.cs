using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class UNDGSubstanceFilterControl : ZFilterStripControl
	{
		public UNDGSubstanceFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			var manager = new UNDGSubstanceMultiColumnsManager(this.FilteredGrid);
			manager.AddColumns(GroupColumnConstant.LimitedQuantities);
			manager.AddColumns(GroupColumnConstant.CargoAircraftOnly);
			manager.AddColumns(GroupColumnConstant.PassengerCargoAircraft);
		}
	}
}
