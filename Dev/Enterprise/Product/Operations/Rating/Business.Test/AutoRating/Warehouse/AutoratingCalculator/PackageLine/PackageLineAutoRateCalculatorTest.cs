using System;
using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class PackageLineAutoRateCalculatorTest : BaseAutoRateCalculatorTest
	{
		protected override SimpleArInfo[] TestUnitCalculatorCore(out string category, out string mode, out string origin, out string destination, out string chargeCode, out string unit, out decimal perUnit, out Action<RateLine> setRateLine)
		{
			category = RatingConstants.RateCategory.WHS;
			mode = RateMode.ALL;
			origin = "AU";
			destination = "US";

			chargeCode = "WHSOUT";
			unit = Weight.Kilograms;
			perUnit = 1m;

			setRateLine = SetRateLine;

			Criteria = GetWarehouseCriteria(LocalClient);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 70m, weight: 80m, packageCount: 90m, packageType: PkgUnit.Box, docketReference: "ABC");
			});

			return new[]
			{
				new SimpleArInfo
				{
					Amount = 130m,
					InvoiceLineDesc = "Warehouse Out Charge ABC",
					CalculationSingleLineDescription = @"WHSOUT: 50 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 60x BSK)
WHSOUT: 80 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 90x BOX)"
				}
			};
		}

		protected override SimpleArInfo[] TestFlatCalculatorCore(out string category, out string mode, out string origin, out string destination, out string chargeCode, out decimal flatCalculatorBaseRate, out Action<RateLine> setRateLine)
		{
			category = RatingConstants.RateCategory.WHS;
			mode = RateMode.ALL;
			origin = "AU";
			destination = "US";

			chargeCode = "WHSOUT";
			flatCalculatorBaseRate = 10m;

			setRateLine = SetRateLine;

			Criteria = GetWarehouseCriteria(LocalClient);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 70m, weight: 80m, packageCount: 90m, packageType: PkgUnit.Box, docketReference: "ABC");
			});

			return new[]
			{
				new SimpleArInfo
				{
					Amount = 20m,
					InvoiceLineDesc = "Warehouse Out Charge ABC",
					CalculationSingleLineDescription = @"WHSOUT: Base Rate AUD 10.00 (for warehouse package/s: 60x BSK)
WHSOUT: Base Rate AUD 10.00 (for warehouse package/s: 90x BOX)"
				}
			};
		}

		void SetRateLine(RateLine rateLine)
		{
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
		}

		#region Implementation

		protected TestRatingCriteria Criteria { get; set; }

		protected abstract OrgHeader LocalClient { get; }

		protected override void AssertAutorate(CostSell costSell, IEnumerable<SimpleArInfo> expected, string assertionMessage = default)
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var autoRater = new FreightAutoRater(new RatingContext());
				var results = autoRater.AutoRate(Criteria, costSell);
				AssertRatingResults(assertionMessage, expected, results);
			}
		}

		protected TestRatingCriteria GetWarehouseCriteria(OrgHeader localClient)
		{
			var criteria = new TestRatingCriteria();

			criteria.LocalClient = localClient;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseOutwards;
			criteria.RateTypeToUse = RateType.Warehouse;
			criteria.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.WHSOutwards);
			criteria.Creditors = Creditors.New(OrgWithSource.New(localClient, new List<string>() { "Provider" }));

			return criteria;
		}

		protected override void SetUp()
		{
			base.SetUp();

			InsertChargeCode(Factory, "WHSOUT", "Warehouse Out Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
			InsertChargeCode(Factory, "WHSIN", "Warehouse In Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			Factory.Save();
		}

		#endregion
	}
}
