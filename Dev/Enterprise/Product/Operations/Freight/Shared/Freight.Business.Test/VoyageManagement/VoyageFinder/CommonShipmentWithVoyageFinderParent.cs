using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	public sealed class CommonShipmentWithVoyageFinderParent : CommonShipment, IVoyageFinderParent
	{
		public CommonShipmentWithVoyageFinderParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IVoyageFinderParent Members

		ZString IVoyageFinderParent.TransportMode
		{
			get { return JS_TransportMode; }
		}

		ZString IVoyageFinderParent.LoadPort
		{
			get { return JS_RL_NKOrigin; }
		}

		ZString IVoyageFinderParent.DischargePort
		{
			get { return JS_RL_NKDestination; }
		}

		ZGuid IVoyageFinderParent.CarrierPK
		{
			get { return BookedShippingLinePK; }
		}

		#endregion
	}
}
