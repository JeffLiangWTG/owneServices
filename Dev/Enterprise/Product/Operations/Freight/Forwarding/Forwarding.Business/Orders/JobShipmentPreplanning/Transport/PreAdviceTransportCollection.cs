using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PreAdviceTransportCollection : TransportCollection
	{
		public PreAdviceTransportCollection(JobShipmentPreplanning parent)
			: base(parent)
		{
		}

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			Transport transport = (Transport)child;
			base.SetDefaultsForNewChild(transport);

			int nextLeg = 1;
			foreach (Transport existingTransport in this)
			{
				if (existingTransport.JW_LegOrder >= nextLeg)
				{
					nextLeg = existingTransport.JW_LegOrder + 1;
				}
			}

			transport.JW_LegOrder = (ZByte)(short)nextLeg;
		}

		#endregion
	}
}
