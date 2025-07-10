using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Rating.Business
{
	public class ShipmentProfitShares : AutoShipmentProfitShares
	{
		public ShipmentProfitShares(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CommonShipment Shipment
		{
			get { return Factory.Load<CommonShipment>(PSS_JS); }
		}

		[RelatedBusinessObject("Shipment")]
		public override ZGuid PSS_JS
		{
			get { return base.PSS_JS; }
			set { base.PSS_JS = value; }
		}

		public ConsolidationProfitShare ParentConsol
		{
			get { return Factory.Load<ConsolidationProfitShare>(PSS_CPS); }
		}

		[RelatedBusinessObject("ParentConsol")]
		public override ZGuid PSS_CPS
		{
			get { return base.PSS_CPS; }
			set { base.PSS_CPS = value; }
		}
	}
}
