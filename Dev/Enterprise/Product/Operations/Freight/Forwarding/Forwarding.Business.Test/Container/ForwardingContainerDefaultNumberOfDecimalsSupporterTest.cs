using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerDefaultNumberOfDecimalsSupporterTest : CommonContainerDefaultNumberOfDecimalsSupporterTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return container; }
		}
		ForwardingContainer container;
	}
}
