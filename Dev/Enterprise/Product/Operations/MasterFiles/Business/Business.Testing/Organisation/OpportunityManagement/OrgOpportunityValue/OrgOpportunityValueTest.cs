using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityValue))]
	public class OrgOpportunityValueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetPV_DiscountPercentTooLarge()
		{
			var valueItem = Factory.NewWithValidTestData<OrgOpportunityValue>();

			valueItem.PV_Value = 1000m;
			valueItem.PV_DiscountPercent = decimal.MaxValue;

			Assert("Discount percent should have errors as it is greater than 100", valueItem.PV_DiscountPercentInfo.HasErrors());
			AssertEquals("Discount field should be 0", valueItem.PV_Discount, ZDecimal.Zero);

			valueItem.PV_DiscountPercent = 90;
			Assert("Discount percent should not have errors", !valueItem.PV_DiscountPercentInfo.HasErrors());
			AssertEquals("Discount field should be set to 900 now", valueItem.PV_Discount, new ZDecimal(900));
		}

		public void TestDiscountReadonlyAndClearsValues()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue valueItem1 = opp.ValueItems.AddNew();
			valueItem1.PV_Value = 1000m;

			AssertEquals(true, valueItem1.PV_DiscountInfo.ReadOnly);
			AssertEquals(false, valueItem1.PV_DiscountPercentInfo.ReadOnly);

			valueItem1.PV_DiscountPercent = 30m;

			valueItem1.PV_DiscountBasis = OrgOpportunityValueLookups.DiscountBasis.Flat;
			AssertEquals(false, valueItem1.PV_DiscountInfo.ReadOnly);
			AssertEquals(true, valueItem1.PV_DiscountPercentInfo.ReadOnly);
			AssertEquals(300m, valueItem1.PV_Discount);
			AssertEquals(0m, valueItem1.PV_DiscountPercent);

			valueItem1.PV_Discount = 500m;
			valueItem1.PV_DiscountBasis = OrgOpportunityValueLookups.DiscountBasis.Percent;
			AssertEquals(true, valueItem1.PV_DiscountInfo.ReadOnly);
			AssertEquals(false, valueItem1.PV_DiscountPercentInfo.ReadOnly);
			AssertEquals(0m, valueItem1.PV_Discount);
		}

		public void TestValueDiscount()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue valueItem1 = opp.ValueItems.AddNew();

			valueItem1.PV_Value = 500m;
			AssertEquals(500m, valueItem1.ValueAfterDiscount);

			valueItem1.PV_DiscountPercent = 50m;
			AssertEquals(250m, valueItem1.PV_Discount);
			AssertEquals(250m, valueItem1.ValueAfterDiscount);

			valueItem1.PV_DiscountPercent = 10m;
			AssertEquals(50m, valueItem1.PV_Discount);
			AssertEquals(450m, valueItem1.ValueAfterDiscount);

			valueItem1.PV_Value = 1000m;
			AssertEquals("Discount re-calculated", 100m, valueItem1.PV_Discount);

			valueItem1.PV_Value = 500m;
			AssertEquals("Discount re-calculated", 50m, valueItem1.PV_Discount);

			valueItem1.PV_DiscountPercent = 0m;
			AssertEquals(0m, valueItem1.PV_Discount);
			AssertEquals(500m, valueItem1.ValueAfterDiscount);

			valueItem1.PV_DiscountBasis = OrgOpportunityValueLookups.DiscountBasis.Flat;

			valueItem1.PV_Discount = 30m;
			AssertEquals(30m, valueItem1.PV_Discount);
			AssertEquals(470m, valueItem1.ValueAfterDiscount);

			valueItem1.PV_Discount = 470m;
			AssertEquals(470m, valueItem1.PV_Discount);
			AssertEquals(30m, valueItem1.ValueAfterDiscount);

			valueItem1.PV_Value = 1000m;
			AssertEquals("Discount NOT re-calculated", 470m, valueItem1.PV_Discount);

			valueItem1.PV_Discount = 0m;
			AssertEquals(0m, valueItem1.PV_Discount);
			AssertEquals(1000m, valueItem1.ValueAfterDiscount);
		}

		public virtual void TestValue()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue valueItem1 = opp.ValueItems.AddNew();
			OrgOpportunityValue valueItem2 = opp.ValueItems.AddNew();
			OrgOpportunityValue valueItem3 = opp.ValueItems.AddNew();

			valueItem1.PV_Value = 500m;
			AssertEquals(500m * 12, opp.P8_EstimatedValue);

			valueItem2.PV_Value = 300m;
			AssertEquals(800m * 12, opp.P8_EstimatedValue);

			valueItem1.PV_Value = 1m;
			AssertEquals(301m * 12, opp.P8_EstimatedValue);

			opp.P8_EstimatedValue = 600m;
			AssertEquals(600m, opp.P8_EstimatedValue);

			valueItem3.PV_Value = 200m;
			AssertEquals(501m * 12, opp.P8_EstimatedValue);

			valueItem1.PV_Value = 50m;
			AssertEquals(550m * 12, opp.P8_EstimatedValue);

			opp.P8_EstimatedValue = 550m;
			valueItem1.PV_Value = 100m;
			AssertEquals(600m * 12, opp.P8_EstimatedValue);

			valueItem1.PV_DiscountPercent = 30m;
			AssertEquals(570m * 12, opp.P8_EstimatedValue);

			opp.ValueItems.RemoveAndDelete(valueItem1);
			AssertEquals(500m * 12, opp.P8_EstimatedValue);
		}

		public void TestLogging()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue valueItem = opp.ValueItems.AddNew();
			valueItem.PV_RevenueType = valueItem.Lookups.ValueTypes[0].Code;
			Factory.Save();

			AssertEquals("Autolog event reference description", "Value " + valueItem.Lookups.ValueTypes[0].Description, valueItem.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestRevenueDescription()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			OrgOpportunityValue valueItem = opp.ValueItems.AddNew();

			valueItem.PV_RevenueType = valueItem.Lookups.ValueTypes[0].Code;
			AssertEquals(valueItem.Lookups.ValueTypes[0].Description, valueItem.RevenueTypeDescription);

			valueItem.PV_RevenueType = "";
			AssertEquals("", valueItem.RevenueTypeDescription);

			valueItem.PV_RevenueType = valueItem.Lookups.ValueTypes[1].Code;
			AssertEquals(valueItem.Lookups.ValueTypes[1].Description, valueItem.RevenueTypeDescription);

			valueItem.PV_RevenueType = "XXX";
			AssertEquals("", valueItem.RevenueTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			return opp.ValueItems.AddNew();
		}
	}
}
