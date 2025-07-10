using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ComparisonQuoteResult))]
	public class ComparisonQuoteResultTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Cases
		public void TestConstructors()
		{
			TestComparisonQuoteResult = new ComparisonQuoteResult(Factory);
			AssertComparisonQuoteResult(ZString.Empty, ZString.Empty, ZGuid.Empty, ZBool.True, TestComparisonQuoteResult);
			ZGuid testGuid = ZGuid.NewZGuid();
			TestComparisonQuoteResult = new ComparisonQuoteResult(Factory, "TMD", "TSV", testGuid, ZBool.False);
			AssertComparisonQuoteResult("TMD", "TSV", testGuid, ZBool.False, TestComparisonQuoteResult);
		}

		protected void AssertComparisonQuoteResult(ZString transportMode, ZString serviceLevel, ZGuid carrierPK, ZBool showLocalCurrency, ComparisonQuoteResult quoteResult)
		{
			AssertEquals(transportMode, quoteResult.Mode);
			AssertEquals(serviceLevel, quoteResult.ServiceLevel);
			AssertEquals(carrierPK, quoteResult.CarrierPK);
			AssertEquals(showLocalCurrency, quoteResult.ShowLocalCurrency);
		}

		public void TestCarrier()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			TestComparisonQuoteResult.CarrierPK = carrier.PK;
			AssertEquals(carrier, TestComparisonQuoteResult.Carrier);
		}

		public void TestTransportMode()
		{
			TestComparisonQuoteResult.Mode = "T1";
			AssertEquals("T1", TestComparisonQuoteResult.Mode);
			TestComparisonQuoteResult.Mode = "T2";
			AssertEquals("T2", TestComparisonQuoteResult.Mode);
		}

		public void TestServiceLevel()
		{
			TestComparisonQuoteResult.ServiceLevel = "S1";
			AssertEquals("S1", TestComparisonQuoteResult.ServiceLevel);
			TestComparisonQuoteResult.ServiceLevel = "S2";
			AssertEquals("S2", TestComparisonQuoteResult.ServiceLevel);
		}

		public void TestCarrierPK()
		{
			ZGuid testGuid = ZGuid.NewZGuid();
			TestComparisonQuoteResult.CarrierPK = testGuid;
			AssertEquals(testGuid, TestComparisonQuoteResult.CarrierPK);
			TestComparisonQuoteResult.CarrierPK = ZGuid.NewZGuid();
			AssertNotEquals(testGuid, TestComparisonQuoteResult.CarrierPK);
		}

		public void TestBool()
		{
			TestComparisonQuoteResult.Selected = ZBool.True;
			AssertEquals(ZBool.True, TestComparisonQuoteResult.Selected);
			TestComparisonQuoteResult.Selected = ZBool.False;
			AssertEquals(ZBool.False, TestComparisonQuoteResult.Selected);
		}

		public void TestBoolInfo()
		{
			AssertEquals("Selected", TestComparisonQuoteResult.SelectedInfo.Name);
		}

		public void TestIsProcessed()
		{
			TestComparisonQuoteResult.IsProcessed = ZBool.True;
			AssertEquals(ZBool.True, TestComparisonQuoteResult.IsProcessed);
			TestComparisonQuoteResult.IsProcessed = ZBool.False;
			AssertEquals(ZBool.False, TestComparisonQuoteResult.IsProcessed);
		}

		public void TestCharges()
		{
			AssertNotNull(TestComparisonQuoteResult.Charges);
			AssertEquals(0, TestComparisonQuoteResult.Charges.Count);
			ComparisonQuoteCharge charge = new ComparisonQuoteCharge();
			TestComparisonQuoteResult.Charges.Add(charge);
			AssertEquals(1, TestComparisonQuoteResult.Charges.Count);
			AssertEquals(charge, TestComparisonQuoteResult.Charges[0]);
		}

		public void TestShowLocalCurrency()
		{
			TestComparisonQuoteResult.ShowLocalCurrency = ZBool.True;
			AssertEquals(ZBool.True, TestComparisonQuoteResult.ShowLocalCurrency);
			TestComparisonQuoteResult.ShowLocalCurrency = ZBool.False;
			AssertEquals(ZBool.False, TestComparisonQuoteResult.ShowLocalCurrency);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			TestComparisonQuoteResult = new ComparisonQuoteResult(Factory);
		}

		protected ComparisonQuoteResult TestComparisonQuoteResult;
		#endregion
	}
}
