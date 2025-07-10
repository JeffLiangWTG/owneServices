using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Schedule;

namespace Enterprise.Freight.Agency.Business
{
	partial class AgencyShipment : ITransportParent
	{
		protected override TransportCollection GetNewTransportCollection()
		{
			return new AgencyShipmentTransportCollection(this);
		}

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new AgencyShipmentTransportSupporter<AgencyShipment>(this); }
		}

		ZString ITransportParentCommon.TypeCode
		{
			get { return Constants.TransportParentTypes.AgencyShipment; }
		}

		#endregion
	}
}
