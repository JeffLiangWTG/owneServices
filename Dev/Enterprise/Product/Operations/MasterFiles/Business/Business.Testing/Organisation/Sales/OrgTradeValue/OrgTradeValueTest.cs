using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradeValue))]
	sealed class OrgTradeValueTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			tradePeriod.PAS_OH_Client = org.PK;
			var tradeValue = tradePeriod.TradeValues.AddNew();
			tradeValue.PAV_GC = Env.CurrentCompanyPK;

			return tradeValue;
		}

		#endregion
	}
}
