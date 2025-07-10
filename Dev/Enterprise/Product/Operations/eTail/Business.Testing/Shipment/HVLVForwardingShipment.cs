using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVForwardingShipment : ForwardingShipment
	{
		public HVLVForwardingShipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
		}
	}
}
