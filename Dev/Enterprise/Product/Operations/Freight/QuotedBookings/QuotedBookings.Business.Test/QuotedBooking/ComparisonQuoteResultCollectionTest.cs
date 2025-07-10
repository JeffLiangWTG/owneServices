using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ComparisonQuoteResultCollection))]
	public class ComparisonQuoteResultCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComparisonQuoteResultCollection>
	{
		#region Test Cases
		#region Properties
		public void TestSelected()
		{
			AssertNull(TestCollection.Selected);
			TestCollection[3].Selected = ZBool.True;
			AssertEquals(TestCollection[3], TestCollection.Selected);
			TestCollection[2].Selected = ZBool.True;
			AssertEquals(TestCollection[2], TestCollection.Selected);
			TestCollection[2].Selected = ZBool.False;
			TestCollection[3].Selected = ZBool.False;
			TestCollection[4].Selected = ZBool.True;
			AssertEquals(TestCollection[4], TestCollection.Selected);
		}

		public void TestServiceLevelSpecific()
		{
			AssertEquals(3, TestCollection.ServiceLevelSpecific("S2").Count);
			AssertQuoteResult("T1", "S2", TestCollection);
			AssertQuoteResult("T2", "S2", TestCollection);
			AssertQuoteResult("T4", "S2", TestCollection);
		}

		protected void AssertQuoteResult(ZString transportMode, ZString serviceLevel, ZString carrierName, ComparisonQuoteResult quoteResult)
		{
			AssertEquals(transportMode, quoteResult.Mode);
			AssertEquals(serviceLevel, quoteResult.ServiceLevel);
			AssertEquals(carrierName, (quoteResult.Carrier == null ? ZString.Empty : quoteResult.Carrier.OH_FullName));
		}

		protected void AssertQuoteResult(ZString transportMode, ZString serviceLevel, ComparisonQuoteResultCollection collection)
		{
			bool result = false;
			foreach (ComparisonQuoteResult item in collection)
			{
				if (item.Mode == transportMode && item.ServiceLevel == serviceLevel)
				{
					result = true;
					break;
				}
			}

			Assert(result);
		}

		public void TestTransportModeSpecific()
		{
			AssertEquals(3, TestCollection.ModeSpecific("T2").Count);
			AssertQuoteResult("T2", "S2", TestCollection);
			AssertQuoteResult("T2", "S3", TestCollection);
			AssertQuoteResult("T2", "S4", TestCollection);
		}

		public void TestTransportModes()
		{
			AssertEquals(4, TestCollection.Modes.Count);
			AssertEquals(true, TestCollection.Modes.Contains(""));
			AssertEquals(true, TestCollection.Modes.Contains("T1"));
			AssertEquals(true, TestCollection.Modes.Contains("T2"));
			AssertEquals(true, TestCollection.Modes.Contains("T4"));
		}

		public void TestServiceLevels()
		{
			AssertEquals(4, TestCollection.ServiceLevels.Count);
			AssertEquals(true, TestCollection.ServiceLevels.Contains(""));
			AssertEquals(true, TestCollection.ServiceLevels.Contains("S2"));
			AssertEquals(true, TestCollection.ServiceLevels.Contains("S3"));
			AssertEquals(true, TestCollection.ServiceLevels.Contains("S4"));
		}

		#endregion
		#region Methods
		public void TestCleanZeroChargesAndResults()
		{
			TestCollection[0].Charges.Add(new ComparisonQuoteCharge("T1", 0m, "USD", 0m));
			TestCollection[1].Charges.Add(new ComparisonQuoteCharge("T2", 1m, "USD", 2m));
			TestCollection[1].Charges.Add(new ComparisonQuoteCharge("T3", 0m, "USD", 0m));
			TestCollection[1].Charges.Add(new ComparisonQuoteCharge("T4", 2m, "USD", 3m));
			TestCollection[2].Charges.Add(new ComparisonQuoteCharge("T5", -3m, "AUD", -4m));
			TestCollection[2].Charges.Add(new ComparisonQuoteCharge("T6", 0m, "AUD", 0m));
			TestCollection[2].Charges.Add(new ComparisonQuoteCharge("T7", 0m, "AUD", 0m));
			AssertEquals(6, TestCollection.Count);
			AssertEquals(1, TestCollection[0].Charges.Count);
			AssertEquals(3, TestCollection[1].Charges.Count);
			AssertEquals(3, TestCollection[2].Charges.Count);
			TestCollection.CleanZeroChargesAndResults();
			AssertEquals(2, TestCollection.Count);
			AssertEquals(2, TestCollection[0].Charges.Count);
			AssertEquals(1, TestCollection[1].Charges.Count);
			AssertEquals("T2", TestCollection[0].Charges[0].Description);
			AssertEquals("T4", TestCollection[0].Charges[1].Description);
			AssertEquals("T5", TestCollection[1].Charges[0].Description);
		}

		public void TestGetNextUnprocessedItem()
		{
			AssertEquals(TestCollection[0], TestCollection.GetNextUnprocessedItem());
			TestCollection[0].IsProcessed = ZBool.True;
			TestCollection[1].IsProcessed = ZBool.True;
			AssertEquals(TestCollection[2], TestCollection.GetNextUnprocessedItem());
			TestCollection[3].IsProcessed = ZBool.True;
			AssertEquals(TestCollection[2], TestCollection.GetNextUnprocessedItem());
			TestCollection[2].IsProcessed = ZBool.True;
			AssertEquals(TestCollection[4], TestCollection.GetNextUnprocessedItem());
			TestCollection[4].IsProcessed = ZBool.True;
			TestCollection[5].IsProcessed = ZBool.True;
			AssertNull(TestCollection.GetNextUnprocessedItem());
		}

		public void TestClearProcessedFlag()
		{
			TestCollection[1].IsProcessed = ZBool.True;
			TestCollection[2].IsProcessed = ZBool.True;
			TestCollection[3].IsProcessed = ZBool.True;
			TestCollection.ClearProcessedFlag();
			foreach (ComparisonQuoteResult testResult in TestCollection)
			{
				AssertEquals(ZBool.False, testResult.IsProcessed);
			}
		}

		public void TestContains()
		{
			foreach (ComparisonQuoteResult testResult in TestCollection)
			{
				AssertEquals(ZBool.True, TestCollection.Contains(testResult));
			}

			AssertEquals(ZBool.False, TestCollection.Contains(new ComparisonQuoteResult(Factory)));
			AssertEquals(ZBool.False, TestCollection.Contains(new ComparisonQuoteResult(Factory)));
		}

		public void TestAddComparisonQuoteResult()
		{
			ComparisonQuoteResult newResult = new ComparisonQuoteResult(Factory);
			AssertEquals(ZBool.False, TestCollection.Contains(newResult));
			TestCollection.Add(newResult);
			AssertEquals(ZBool.True, TestCollection.Contains(newResult));
		}

		#endregion
		#endregion
		#region Implementation
		protected override ComparisonQuoteResultCollection GetCollectionToTest()
		{
			return new ComparisonQuoteResultCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComparisonQuoteResult(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new ComparisonQuoteResultCollection(Factory);
			TestCollection.Add(new ComparisonQuoteResult(Factory, "T1", "S2", GetNewCarrier().PK, ZBool.True));
			TestCollection.Add(new ComparisonQuoteResult(Factory, "T2", "S2", GetNewCarrier().PK, ZBool.True));
			TestCollection.Add(new ComparisonQuoteResult(Factory, "", "", GetNewCarrier().PK, ZBool.True));
			TestCollection.Add(new ComparisonQuoteResult(Factory, "T2", "S3", GetNewCarrier().PK, ZBool.True));
			TestCollection.Add(new ComparisonQuoteResult(Factory, "T4", "S2", GetNewCarrier().PK, ZBool.True));
			TestCollection.Add(new ComparisonQuoteResult(Factory, "T2", "S4", GetNewCarrier().PK, ZBool.True));
		}

		protected OrgHeader GetNewCarrier()
		{
			return Factory.New<OrgHeader>();
		}

		protected ComparisonQuoteResultCollection TestCollection;
		#endregion
	}
}
