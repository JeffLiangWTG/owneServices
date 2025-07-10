using System;
using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class WarehouseTest : RatingTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			InsertChargeCode(Factory, "WHSOUT", "Warehouse Out Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
			InsertChargeCode(Factory, "WHSIN", "Warehouse In Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
		}

		protected abstract JobInvoicingConsumerType CriteriaConsumerType { get; }

		protected abstract string CriteriaChargeCodeGroup { get; }

		protected TestRatingCriteria GetWarehouseCriteria(OrgHeader localClient)
		{
			var criteria = new TestRatingCriteria();

			criteria.LocalClient = localClient;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			criteria.RateTypeToUse = RateType.Warehouse;
			criteria.ChargeCodeGroups.Add(CriteriaChargeCodeGroup);
			criteria.Creditors = Creditors.New(OrgWithSource.New(localClient, new List<string>() { "Provider" }));

			return criteria;
		}

		protected void AssertAutorate(string assertionMessage, TestRatingCriteria criteria, CostSell costSell, IEnumerable<SimpleArInfo> expected)
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var autoRater = new FreightAutoRater(new RatingContext());
				var results = autoRater.AutoRate(criteria, costSell);
				AssertRatingResults(assertionMessage, expected, results);
			}
		}
	}
}
