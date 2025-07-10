using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;

namespace Enterprise.Rating.Business
{
	public class ConsolidationProfitShare : AutoConsolidationProfitShare
	{
		public ConsolidationProfitShare(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CommonConsol Consol
		{
			get { return Factory.Load<CommonConsol>(CPS_JK); }
		}

		[RelatedBusinessObject("Consol")]
		public override ZGuid CPS_JK
		{
			get { return base.CPS_JK; }
			set { base.CPS_JK = value; }
		}

		public ProfitShareRedistribution Parent
		{
			get { return Factory.Load<ProfitShareRedistribution>(CPS_PSR); }
		}

		[RelatedBusinessObject("Parent")]
		public override ZGuid CPS_PSR
		{
			get { return base.CPS_PSR; }
			set { base.CPS_PSR = value; }
		}

		public ShipmentProfitSharesCollection ShipmentProfitShares
		{
			get
			{
				if (shipmentProfitShares == null)
				{
					shipmentProfitShares = new ShipmentProfitSharesCollection(this);
					shipmentProfitShares.Load();
				}
				return shipmentProfitShares;
			}
		}
		ShipmentProfitSharesCollection shipmentProfitShares;

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPS_RX_NKCurrency = Env.CurrentCompany.LocalCurrency.Code;
		}
#endif
		#endregion
	}
}
