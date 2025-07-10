using CargoWise.EntityFramework;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubZonePivotCollection : DependentBusinessObjectCollection<PortHubZonePivot, PortHubSelection>
	{
		public PortHubZonePivotCollection(PortHubSelection parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			IsManagedForDataRefresh = true;
		}

		protected override string FkColumnName
		{
			get { return PortHubZonePivot.Schema.TX_TY_Hub; }
		}
	}
}
