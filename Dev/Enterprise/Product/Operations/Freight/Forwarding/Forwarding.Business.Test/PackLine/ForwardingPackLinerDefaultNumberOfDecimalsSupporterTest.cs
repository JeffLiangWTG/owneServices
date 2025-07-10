using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingPackLinerDefaultNumberOfDecimalsSupporterTest : PackLinerDefaultNumberOfDecimalsSupporterTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
		}

		public override BusinessObject BizObj
		{
			get { return packLine; }
		}
		ForwardingPackLine packLine;
	}
}
