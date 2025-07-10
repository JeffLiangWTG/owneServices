using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>))]
	sealed class ConfirmedCusEntryHeaderChargesBizoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var headerCharges = entryHeader.ConfirmedCharges;
			return (ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>)headerCharges;
		}
	}

	sealed class ConfirmedCusEntryHeaderChargesCollectionTest : TestCaseWithFactory
	{
		public void TestConfirmedCusEntryHeaderChargesCollectionOnlyIncludeCUSCharges()
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
				var confirmedCharges = new ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>(entry);
				confirmedCharges.Load();
				AssertEquals("Count", 1, confirmedCharges.Count);
				AssertContainsExactElementsInAnyOrder("Only include C1_Source = CUS", new[] { charge1.PK }, confirmedCharges.Select(x => x.PK));
			});
		}

		public void TestIndexerWithChargeType()
		{
			CombineAssertions(() =>
			{
				var charge1 = HeaderCharges.AddOrUpdate("AAA");
				AssertEquals("Precondition: HeaderCharges.Count", 1, HeaderCharges.Count);
				AssertEquals("HeaderCharges['AAA']", charge1, HeaderCharges["AAA"]);

				var charge2 = HeaderCharges["BBB"];
				AssertNotNull("HeaderCharges['BBB'] even though it was not specifically added", charge2);
				AssertEquals("HeaderCharges.Count: charge2 added", 2, HeaderCharges.Count);

				var charge3 = HeaderCharges["BBB"];
				AssertEquals("Indexer called again gets same object", charge2, charge3);
				AssertEquals("HeaderCharges.Count: no charge added", 2, HeaderCharges.Count);

				charge2.C1_IsLandedCostOnly = true;
				var charge4 = HeaderCharges["BBB"];
				AssertNotNull("HeaderCharges['BBB'] added as the existed BBB charge is LandedCostOnly", charge4);
				AssertEquals("HeaderCharges.Count: charge4 added", 3, HeaderCharges.Count);
			});
		}

		public void TestAddOrUpdateWithChargeType()
		{
			CombineAssertions(() =>
			{
				var charge1 = HeaderCharges.AddOrUpdate("AAA");
				AssertEquals("HeaderCharges.Count", 1, HeaderCharges.Count);
				AssertEquals("Charge1.C1_ChargeType", "AAA", charge1.C1_ChargeType);

				var charge2 = HeaderCharges.AddOrUpdate("AAA");
				AssertEquals("HeaderCharges.Count: update charge1", 1, HeaderCharges.Count);
				AssertEquals("Gets same object if Charge Type exists", charge1, charge2);

				charge2.C1_IsLandedCostOnly = true;
				var charge3 = HeaderCharges.AddOrUpdate("AAA");
				AssertEquals("HeaderCharges.Count: add charge3", 2, HeaderCharges.Count);
				AssertEquals("Charge3 added as the existed AAA charge is LandedCostOnly", "AAA", charge3.C1_ChargeType);

				var charge4 = HeaderCharges.AddOrUpdate("BBB");
				AssertEquals("HeaderCharges.Count: add charge4", 3, HeaderCharges.Count);
				AssertEquals("Charge4 added as no existed BBB charge", "BBB", charge4.C1_ChargeType);
			});
		}

		IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> HeaderCharges
		{
			get
			{
				if (fHeaderCharges == null)
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					var entry = declaration.CustomsEntryHeaders.AddNew();
					fHeaderCharges = entry.ConfirmedCharges;
				}
				return fHeaderCharges;
			}
		}
		IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> fHeaderCharges;
	}
}
