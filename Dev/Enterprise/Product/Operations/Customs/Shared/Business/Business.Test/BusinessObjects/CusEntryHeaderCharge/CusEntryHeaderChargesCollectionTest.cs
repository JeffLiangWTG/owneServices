using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>))]
	[TestsSubclassesOf(typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>))]
	public class CusEntryHeaderChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var headerCharges = entryHeader.Charges;
			return (CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)headerCharges;
		}
	}

	class CusEntryHeaderChargesCollectionBaseOnlyTest : TestCaseWithFactory
	{
		public void TestTotalAmount()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 1m);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.DutiableMail, 2m);
			AssertEquals(3m, entry.Charges.TotalAmount);
			entry.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.Honey, 4m);
			AssertEquals(7m, entry.Charges.TotalAmount);
		}

		public void TestGetTotalAmountByChargeTypes()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Charges.AddNew("AAA", 10m);
			entry.Charges.AddNew("BBB", 20m);
			entry.Charges.AddNew("CCC", 40m);
			entry.Charges.AddNew("DDD", 80m);
			AssertEquals(30m, entry.Charges.GetTotalAmount(new[] { "AAA", "BBB" }));
			AssertEquals(40m, entry.Charges.GetTotalAmount(new[] { "CCC" }));
			AssertEquals(0m, entry.Charges.GetTotalAmount(Array.Empty<string>()));
		}

		public void TestHasOverrideFeeOfGivenCode()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var charges = entryHeader.Charges;

			Assert(!charges.HasOverrideFeeOfGivenCode("TST", "XXX"));

			charges.AddNew("AAA");
			Assert(!charges.HasOverrideFeeOfGivenCode("TST", "XXX"));

			var charge = charges.AddNew("TST");
			Assert(!charges.HasOverrideFeeOfGivenCode("TST", "XXX"));

			charge.C1_RateOverrideReasonCode = "XXX";
			Assert(charges.HasOverrideFeeOfGivenCode("TST", "XXX"));
		}

		public void TestCusEntryHeaderChargesCollectionOnlyIncludeCW1Charges()
		{
			var entry = Factory.New<CusEntryHeader>();
			var charge1 = Factory.New<CusEntryHeaderCharges>();
			charge1.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			charge1.C1_CH = entry.PK;
			var charge2 = Factory.New<CusEntryHeaderCharges>();
			charge2.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			charge2.C1_CH = entry.PK;
			var charge3 = Factory.New<CusEntryHeaderCharges>();
			charge3.C1_IsLandedCostOnly = true;
			charge3.C1_CH = entry.PK;

			CombineAssertions(() =>
			{
				var charges = new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(entry);
				charges.Load();
				AssertEquals("Count", 2, charges.Count);
				AssertContainsExactElementsInAnyOrder("Only include C1_Source = CW1 or NULL", new[] { charge2.PK, charge3.PK }, charges.Cast<CusEntryHeaderCharges>().Select(x => x.PK));
			});
		}

		public void TestGetAmount()
		{
			var entry = Factory.New<CusEntryHeader>();
			var charge1 = entry.Charges.AddNew("TS1");
			charge1.C1_ChargeAmount = 1m;
			charge1.C1_IsLandedCostOnly = true;
			var charge2 = entry.Charges.AddNew("TS2", 11m);
			var charge3 = entry.Charges.AddNew("TS3", 111m);
			charge3.C1_IsLandedCostOnly = true;

			AssertEquals("GetAmount", ZDecimal.Zero, entry.Charges.GetAmount("TS1"));
			AssertEquals("GetTotalAmount W/O Landed Cost", ZDecimal.Zero, entry.Charges.GetTotalAmount("TS1", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 1m, entry.Charges.GetTotalAmount("TS1", includeLandedCostOnly: true));

			AssertEquals("GetAmount", 11m, entry.Charges.GetAmount("TS2"));
			AssertEquals("GetTotalAmount W/O Landed Cost", 11m, entry.Charges.GetTotalAmount("TS2", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 11m, entry.Charges.GetTotalAmount("TS2", includeLandedCostOnly: true));

			AssertEquals("GetAmount", ZDecimal.Zero, entry.Charges.GetAmount("TS3"));
			AssertEquals("GetTotalAmount W/O Landed Cost", ZDecimal.Zero, entry.Charges.GetTotalAmount("TS3", includeLandedCostOnly: false));
			AssertEquals("GetTotalAmount With Landed Cost", 111m, entry.Charges.GetTotalAmount("TS3", includeLandedCostOnly: true));
			AssertEquals("GetAmount with parameter", 111m, entry.Charges.GetAmountIncludingLCOnly("TS3"));
		}

		public void TestGetCharges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge1 = entry.Charges.AddNew("TS1");
			charge1.C1_IsLandedCostOnly = true;
			var charge2 = entry.Charges.AddNew("TS3");
			var charge3 = entry.Charges.AddNew("TS5");
			charge3.C1_IsLandedCostOnly = true;
			var charge4 = entry.Charges.AddNew("TS2");
			charge4.C1_IsLandedCostOnly = true;
			var charge5 = entry.Charges.AddNew("TS4");
			AssertEquals("TS1,TS2,TS3,TS4,TS5", new ZStringBuilder(entry.Charges.GetCharges().Select(x => x.C1_ChargeType).OrderBy(x => x)).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("TS1,TS2,TS5", new ZStringBuilder(entry.Charges.GetCharges(true).Select(x => x.C1_ChargeType).OrderBy(x => x)).ToStringWithDelimiterBetweenAppends(","));
			AssertEquals("TS3,TS4", new ZStringBuilder(entry.Charges.GetCharges(false).Select(x => x.C1_ChargeType).OrderBy(x => x)).ToStringWithDelimiterBetweenAppends(","));
		}

		public void TestSetAmount()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Charges["AAA"].C1_ChargeAmount = 10m;
			entry.Charges["BBB"].C1_ChargeAmount = 20m;

			entry.Charges.SetAmount("AAA", 0m);
			entry.Charges.SetAmount("BBB", 30m);
			entry.Charges.SetAmount("CCC", 40m);

			AssertEquals(0m, entry.Charges.GetAmount("AAA"));
			AssertEquals(30m, entry.Charges.GetAmount("BBB"));
			AssertEquals(40m, entry.Charges.GetAmount("CCC"));
		}

		public void TestCopyValuesFrom()
		{
			var charge1 = HeaderCharges.AddNew("AAA");
			charge1.C1_ChargeAmount = 10m;

			var charge2 = HeaderCharges.AddNew("BBB");
			charge2.C1_ChargeAmount = 20m;

			var testDec = BaseJobDeclaration.New(Factory);
			var entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			var sourceCollection = new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(entryHeader2);

			var charge3 = sourceCollection.AddNew("AAA");
			charge3.C1_ChargeAmount = 30m;

			var charge4 = sourceCollection.AddNew("CCC");
			charge4.C1_ChargeAmount = 40m;

			HeaderCharges.CopyChargesValuesFrom(sourceCollection);
			AssertEquals("Charge with AAA is 30m", 30m, HeaderCharges.GetAmount("AAA"));
			AssertEquals("Charge with CCC is 40m", 40m, HeaderCharges.GetAmount("CCC"));
			AssertEquals("Charge with BBB is cleared", 0m, HeaderCharges.GetAmount("BBB"));
		}

		public void TestIndexerWithChargeType()
		{
			var charge1 = HeaderCharges.AddNew("AAA");
			AssertEquals("Precondition: HeaderCharges.Count", 1, HeaderCharges.Count);
			AssertEquals("HeaderCharges['AAA']", charge1, HeaderCharges["AAA"]);

			var charge2 = HeaderCharges["BBB"];
			AssertNotNull("HeaderCharges['BBB'] even though it was not specifically added", charge2);
			AssertEquals("HeaderCharges.Count", 2, HeaderCharges.Count);

			var charge3 = HeaderCharges["BBB"];
			AssertEquals("Indexer called again gets same object", charge2, charge3);
			AssertEquals("HeaderCharges.Count", 2, HeaderCharges.Count);
		}

		public void TestAddNewWithChargeType()
		{
			var charge1 = HeaderCharges.AddNew("AAA");
			AssertEquals("HeaderCharges.Count", 1, HeaderCharges.Count);
			AssertEquals("Charge1.C1_ChargeType", "AAA", charge1.C1_ChargeType);

			var charge2 = HeaderCharges.AddNew("AAA");
			AssertEquals("AddNew called again gets same object if Charge Type exists", charge1, charge2);
			AssertEquals("HeaderCharges.Count", 1, HeaderCharges.Count);

			var charge3 = HeaderCharges.AddNew("BBB");
			AssertEquals("HeaderCharges.Count", 2, HeaderCharges.Count);
			AssertEquals("Charge3.C1_ChargeType", "BBB", charge3.C1_ChargeType);
		}

		public void TestGetCharge()
		{
			var charge1 = HeaderCharges.AddNew();
			charge1.C1_ChargeType = "AAA";
			charge1.C1_ChargeAmount = 1000m;

			var charge2 = HeaderCharges.AddNew();
			charge2.C1_ChargeType = "BBB";
			charge2.C1_ChargeAmount = 2000m;

			AssertEquals("AAA type", charge1.PK, HeaderCharges.GetChargeWithThisCode("AAA").PK);
		}

		public void TestGetChargeWithDuplicateChargeType()
		{
			var charge1 = HeaderCharges.AddNew();
			charge1.C1_ChargeType = "AAA";
			charge1.C1_ChargeAmount = 1000m;

			var charge2 = HeaderCharges.AddNew();
			charge2.C1_ChargeType = "BBB";
			charge2.C1_ChargeAmount = 2000m;

			AssertEquals("AAA type", 1000m, HeaderCharges.GetAmount("AAA"));
		}

		public void TestAddNew()
		{
			var newCharge = HeaderCharges.AddNew("AAA", 1000m);

			AssertEquals("1 element added", 1, HeaderCharges.Count);
			AssertEquals("Type is set", "AAA", newCharge.C1_ChargeType);
			AssertEquals("Amount is set", 1000m, newCharge.C1_ChargeAmount);
		}

		ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> HeaderCharges
		{
			get
			{
				if (fHeaderCharges == null)
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					var entry = declaration.CustomsEntryHeaders.AddNew();
					fHeaderCharges = entry.Charges;
				}
				return fHeaderCharges;
			}
		}
		ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> fHeaderCharges;
	}
}
