using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class SailingIFindBoxForTest : SailingIFindBox
	{
		public SailingIFindBoxForTest(Transport transport)
			: base(transport, null)
		{
		}

		public SailingIFindBoxForTest(CommonShipment shipment)
			: base(shipment, null)
		{
		}

		public SailingIFindBoxForTest(CommonShipment shipment, ZForm parentForm)
			: base(shipment, parentForm)
		{
		}

		public SailingIFindBoxForTest(ISailingParentFindBox shipment, ZForm parentForm)
			: base(shipment, parentForm)
		{
		}

		public new JobSailingCollection Sailings
		{
			get { return base.Sailings; }
		}

		public new EmbeddedModulePopup Popup
		{
			set { base.Popup = value; }
		}
	}
}
