using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI.Testing
{
	public class ShipmentUserControlTestForm : ZForm
	{
		public HVLVShipmentUserControl ShipmentUserControl { get; }
		public ZGrid ConsignmentsGrid => ((ZModuleButtonGrid)Controls.Find("consignmentsGrid", true).SingleOrDefault())?.InnerGrid;
		public ZGrid ItemsGrid => (ZGrid)Controls.Find("itemsGrid", true).SingleOrDefault();
		public ZGrid ItemLinesGrid => (ZGrid)Controls.Find("itemLinesGrid", true).SingleOrDefault();
		public ShipmentUserControlTestForm(HVLVConsignmentHeader dataSource)
			: base(dataSource)
		{
			ShipmentUserControl = new HVLVShipmentUserControl();
			Controls.Add(ShipmentUserControl);
		}

		public ShipmentUserControlTestForm()
			: this(null)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				ShipmentUserControl.Dispose();
			}
		}
	}
}
