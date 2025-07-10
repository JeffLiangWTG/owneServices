using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ComInvOrderLineReconciliationDefaultNumberOfDecimalsSupporterTest : OrderLineDefaultNumberOfDecimalsSupporterTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			var order = Factory.New<ComInvOrderReconciliation>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;

			line = Factory.New<ComInvOrderLineReconciliation>();
			line.JO_JD = order.PK;
			line.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;
			line.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return line; }
		}
		ComInvOrderLineReconciliation line;
	}
}
