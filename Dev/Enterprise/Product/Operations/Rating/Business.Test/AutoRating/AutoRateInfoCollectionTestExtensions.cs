using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business.Testing
{
	public static class AutoRateInfoCollectionTestExtensions
	{
		public static AutoRateInfo AddNew(this AutoRateInfoCollection collection, CalculationResult result, AutoRatingCalculatorParameters parameters)
		{
			var info = new AutoRateInfo(result, parameters, collection.Factory);
			collection.Add(info);
			return info;
		}

		public static void AddIfNotExist(this AutoRateInfoCollection collection, AutoRateInfo autoRateInfo)
		{
			var newInfo = new AutoRateInfo(collection.Factory);
			if (!collection.Select(x => x.ChargeCode == autoRateInfo.ChargeCode && x.Currency == autoRateInfo.Currency && x.Amount == autoRateInfo.Amount).Any())
			{
				collection.Add(newInfo);
			}
		}

		public static AutoRateInfo AddNew(this AutoRateInfoCollection collection, AccChargeCode chargeCode, ZString currency, ZDecimal amount, string operationalJobCode = "S00001000", string container = null, string commodity = null)
		{
			var newInfo = new AutoRateInfo(collection.Factory);
			newInfo.ChargeCode = chargeCode;
			newInfo.Currency = currency;
			newInfo.AddFlatPaymentBasis(amount, operationalJobCode, currency);

			if (!string.IsNullOrEmpty(container))
			{
				newInfo.Attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, container);
			}

			if (!string.IsNullOrEmpty(commodity))
			{
				newInfo.Attributes.Add(JobChargeAttribTypeList.Codes.Commodity, commodity);
			}

			collection.Add(newInfo);

			return newInfo;
		}

		public static List<ZGuid> GetUniqueChargeCodePKs(this AutoRateInfoCollection collection)
		{
			return collection.Cast<AutoRateInfo>().Select(x => x.ChargeCode).Where(x => x != null).Select(x => x.PK).Distinct().ToList();
		}
	}

	public class AutoRateInfoCollectionTest : TestCaseWithFactory
	{
		protected AutoRateInfoCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new AutoRateInfoCollection(Factory);
				}
				return collection;
			}
		}

		AutoRateInfoCollection collection;

		public void TestSortByInvoiceLine()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var entry1 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			var line1 = entry1.RateLines[0];
			line1.TL_RateCalculator = FlatCalculator.Code;
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)300m;
			line1.TL_RX_NKCurrency = "AUD";

			var line2 = entry1.AddRateLine("BAF", FlatCalculator.Code);
			line2.TL_RX_NKCurrency = "USD";
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)300m;

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var result1 = CalculationResult.CreateForTest(line1, 100m, 100m, 100m, criteria);
			var info1 = new AutoRateInfo(result1, parameters, Factory);
			info1.InvoiceLineDescription = "Freight Charge AAA";

			var result2 = CalculationResult.CreateForTest(line1, 100m, 100m, 100m, criteria);
			var info2 = new AutoRateInfo(result2, parameters, Factory);
			info2.InvoiceLineDescription = "Freight Charge BBB";

			var result3 = CalculationResult.CreateForTest(line1, 100m, 100m, 100m, criteria);
			var info3 = new AutoRateInfo(result3, parameters, Factory);
			info3.InvoiceLineDescription = "Freight Charge BBB";

			var result4 = CalculationResult.CreateForTest(line2, 100m, 100m, 100m, criteria);
			var info4 = new AutoRateInfo(result4, parameters, Factory);
			info4.InvoiceLineDescription = "Freight Charge BBB";

			var result5 = CalculationResult.CreateForTest(line2, 150m, 100m, 100m, criteria);
			var info5 = new AutoRateInfo(result5, parameters, Factory);
			info5.InvoiceLineDescription = "Freight Charge BBB";

			Collection.Add(info5);
			Collection.Add(info4);
			Collection.Add(info3);
			Collection.Add(info2);
			Collection.Add(info1);

			Collection.SumUpSameCharges();

			AssertEquals("Client Rate | Freight Charge AAA | AUD | 100m", info1, Collection[0]);
			AssertEquals("Client Rate | Freight Charge BBB | USD | 150m", info5, Collection[1]);
		}

		public void TestGetUniqueChargeCodePKs()
		{
			var info1 = Collection.AddNew(null, "AUD", 10m);

			var info2 = Collection.AddNew(ChargeCode1, "AUD", 20m);

			AssertEquals(1, Collection.GetUniqueChargeCodePKs().Count);
			AssertEquals(true, Collection.GetUniqueChargeCodePKs().Contains(ChargeCode1.PK));

			var info3 = Collection.AddNew(ChargeCode1, "AUD", 30m);
			AssertEquals(1, Collection.GetUniqueChargeCodePKs().Count);
			AssertEquals(true, Collection.GetUniqueChargeCodePKs().Contains(ChargeCode1.PK));

			var info4 = Collection.AddNew(ChargeCode2, "AUD", 40m);
			AssertEquals(2, Collection.GetUniqueChargeCodePKs().Count);
			AssertEquals(true, Collection.GetUniqueChargeCodePKs().Contains(ChargeCode1.PK));
			AssertEquals(true, Collection.GetUniqueChargeCodePKs().Contains(ChargeCode2.PK));
		}

		public void TestSumUpDifferentCharges_OneInfo_ImmediateReturn()
		{
			var collection = new AutoRateInfoCollection(Factory);
			AddNewInfo(collection, Factory.New<AccChargeCode>(), "AUD", 101m, "Calc Desc 1", "Invoice Line Desc 1", "KG");

			collection.SumUpSameCharges();

			AssertLessThanOrEqualTo(0, collection.findExistingInfoIndexHitCount);
		}

		public void TestSumUpDifferentCharges_ZeroInfo_ImmediateReturn()
		{
			var collection = new AutoRateInfoCollection(Factory);

			collection.SumUpSameCharges();

			AssertLessThanOrEqualTo(0, collection.findExistingInfoIndexHitCount);
		}

		public void TestSumUpDifferentChargesNoMoreThanON()
		{
			var collection = new AutoRateInfoCollection(Factory);
			var initialNumberOfCharges = 10;
			var numberOfCharges = initialNumberOfCharges;
			while (numberOfCharges-- > 0)
			{
				AddNewInfo(collection, Factory.New<AccChargeCode>(), "AUD", 101m, "Calc Desc 1", "Invoice Line Desc 1", "KG");
			}

			collection.SumUpSameCharges();
			var hitCount = collection.findExistingInfoIndexHitCount;

			AssertLessThanOrEqualTo(hitCount, initialNumberOfCharges);
		}

		public void TestSumUpSameChargesNoMoreThanON()
		{
			var initialNumberOfCharges = 10;
			var numberOfCharges = initialNumberOfCharges;
			var chargeCode = Factory.New<AccChargeCode>();
			var collection = new AutoRateInfoCollection(Factory);
			while (numberOfCharges-- > 0)
			{
				AddNewInfo(collection, chargeCode, "AUD", 101m, "Calc Desc 1", "Invoice Line Desc 1", "KG");
			}

			collection.SumUpSameCharges();
			var hitCount = collection.findExistingInfoIndexHitCount;

			AssertLessThanOrEqualTo(hitCount, initialNumberOfCharges);
		}

		public void TestSumUpSameChargesDifferentAttributesNoMoreThanON()
		{
			var initialNumberOfCharges = 10;
			var numberOfCharges = initialNumberOfCharges;
			var chargeCode = Factory.New<AccChargeCode>();
			var collection = new AutoRateInfoCollection(Factory);
			while (numberOfCharges-- > 0)
			{
				var autoRateInfo = AddNewInfo(collection, chargeCode, "AUD", 101m, "Calc Desc 1", "Invoice Line Desc 1", "KG");
				autoRateInfo.Attributes.Add(JobChargeAttribTypeList.Codes.LocationDesc, $"a different attribute: {numberOfCharges}");
			}

			collection.SumUpSameCharges();
			var hitCount = collection.findExistingInfoIndexHitCount;

			AssertLessThanOrEqualTo(hitCount, initialNumberOfCharges);
		}

		public void TestSumUpSimilarChargesNoMoreThanONpow2()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var collection = new AutoRateInfoCollection(Factory);
			var hashCode = 12346;
			var initialNumberOfCharges = 10;
			var numberOfCharges = initialNumberOfCharges;
			while (numberOfCharges-- > 0)
			{
				var rateMock = new Moq.Mock<AutoRateInfo>(Moq.MockBehavior.Loose, new[] { Factory });
				rateMock.CallBase = true;
				rateMock.Setup(x => x.HashCodeForMerging()).Returns(hashCode);
				rateMock.Object.ChargeCode = Factory.New<AccChargeCode>();
				rateMock.Object.Currency = "USD";
				rateMock.Object.AddFlatPaymentBasis(321m, "S00001000", "USD");
				rateMock.Object.InvoiceLineDescription = "InvoiceLineDescription";
				collection.Add(rateMock.Object);
			}

			collection.SumUpSameCharges();
			var hitCount = collection.findExistingInfoIndexHitCount;

			AssertLessThanOrEqualTo(hitCount, initialNumberOfCharges * initialNumberOfCharges * 10);
		}

		public void TestSumUpDifferentAutoRateInfoSameHash()
		{
			var collection = new AutoRateInfoCollection(Factory);
			var chargeCode1 = Factory.New<AccChargeCode>();
			var chargeCode2 = Factory.New<AccChargeCode>();

			var firstInfo = collection.AddNew(chargeCode1, "AUD", 101m);
			firstInfo.InvoiceLineDescription = "InvoiceLineDescription1";
			firstInfo.ChargeUnit = "KG";

			var secondInfoMock = new Moq.Mock<AutoRateInfo>(Moq.MockBehavior.Loose, new[] { Factory });
			secondInfoMock.CallBase = true;
			secondInfoMock.Setup(x => x.HashCodeForMerging()).Returns(firstInfo.HashCodeForMerging());
			secondInfoMock.Object.ChargeCode = chargeCode2;
			secondInfoMock.Object.Currency = "USD";
			secondInfoMock.Object.AddFlatPaymentBasis(321m, "S00001020", "USD");
			secondInfoMock.Object.InvoiceLineDescription = "InvoiceLineDescription2";
			collection.Add(secondInfoMock.Object);

			var thirdInfoMock = new Moq.Mock<AutoRateInfo>(Moq.MockBehavior.Loose, new[] { Factory });
			thirdInfoMock.CallBase = true;
			thirdInfoMock.Setup(x => x.HashCodeForMerging()).Returns(firstInfo.HashCodeForMerging());
			thirdInfoMock.Object.ChargeCode = firstInfo.ChargeCode;
			thirdInfoMock.Object.Currency = firstInfo.Currency;
			thirdInfoMock.Object.AddFlatPaymentBasis(123m, "S00001000", firstInfo.Currency);
			thirdInfoMock.Object.InvoiceLineDescription = "InvoiceLineDescription3";
			collection.Add(thirdInfoMock.Object);

			collection.SumUpSameCharges();

			AssertEquals(firstInfo, collection[0]);
			AssertEquals(secondInfoMock.Object, collection[1]);
			AssertEquals(2, collection.Count);
		}

		public void TestSumUpSameCharges()
		{
			var dummyChargeCode1 = Factory.New<AccChargeCode>();
			var dummyChargeCode2 = Factory.New<AccChargeCode>();
			var dummyChargeCode3 = Factory.New<AccChargeCode>();

			collection = new AutoRateInfoCollection(Factory);
			AddNewInfo(collection, dummyChargeCode1, "AUD", 101, "Calc Desc 1", "Invoice Line Desc 1", "KG");
			AddNewInfo(collection, dummyChargeCode2, "AUD", 102, "Calc Desc 2", "Invoice Line Desc 2", "KG");
			AddNewInfo(collection, dummyChargeCode2, "USD", 103, "Calc Desc 3", "Invoice Line Desc 2", "KG");
			AddNewInfo(collection, dummyChargeCode3, "AUD", 104, "Calc Desc 4", "Invoice Line Desc 3", "HB");
			AddNewInfo(collection, dummyChargeCode3, "AUD", 105, "Calc Desc 5", "Invoice Line Desc 3", "");

			CombineAssertions(() =>
			{
				collection.SumUpSameCharges();
				AssertEquals("Result Count", 4, collection.Count);
				AssertEquals("Result[0] Amount", 101M, collection[0].Amount);
				AssertEquals("Result[1] Amount", 102M, collection[1].Amount);
				AssertEquals("Result[2] Amount", 103M, collection[2].Amount);
				AssertEquals("Result[3] Amount", 209M, collection[3].Amount);
			});
		}

		public void TestSumUpSameCharges_Attributes()
		{
			var dummyChargeCode1 = Factory.New<AccChargeCode>();
			collection = new AutoRateInfoCollection(Factory);

			var info1 = collection.AddNew(dummyChargeCode1, "AUD", 101);
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1");
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item2");
			var info2 = collection.AddNew(dummyChargeCode1, "AUD", 102);
			info2.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1");

			collection.SumUpSameCharges();
			AssertEquals("Different Attributes - but both have Cartage Zone - so CAN be merged", 1, collection.Count);

			var expectedAttributes1 = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1"),
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item2")
			};
			AssertContainsExactElementsInAnyOrder("Expected attributes should match after merging.", expectedAttributes1, collection[0].Attributes.Attributes);
			collection.RemoveAndDeleteAll();

			info1 = collection.AddNew(dummyChargeCode1, "AUD", 0);
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1");
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item2");
			info2 = collection.AddNew(dummyChargeCode1, "AUD", 102);
			info2.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1");

			collection.SumUpSameCharges();
			AssertEquals("Different Attributes - but both have Cartage Zone - so CAN be merged", 1, collection.Count);

			var expectedAttributes2 = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1")
			};
			AssertContainsExactElementsInAnyOrder("Expected attributes should match after merging.", expectedAttributes2, collection[0].Attributes.Attributes);
			collection.RemoveAndDeleteAll();

			info1 = collection.AddNew(dummyChargeCode1, "AUD", 0);
			info2 = collection.AddNew(dummyChargeCode1, "AUD", 102);
			info2.Attributes.Add(JobChargeAttribTypeList.Codes.CartageZoneDescription, "Item1");

			collection.SumUpSameCharges();
			AssertEquals("Different Attributes - but both have Cartage Zone - so CAN be merged", 1, collection.Count);

			AssertContainsExactElementsInAnyOrder("Expected attributes should be retained after merging.", expectedAttributes2, collection[0].Attributes.Attributes);
			collection.RemoveAndDeleteAll();

			info1 = collection.AddNew(dummyChargeCode1, "AUD", 0);
			info2 = collection.AddNew(dummyChargeCode1, "AUD", 102);
			info2.Attributes.Add(JobChargeAttribTypeList.Codes.ItemsToRateUnit, "Item1");

			collection.SumUpSameCharges();
			AssertEquals("Different Attributes - but one is Items To Rate Unit - so CAN be merged", 1, collection.Count);

			var expectedAttributes3 = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.ItemsToRateUnit, "Item1")
			};
			AssertContainsExactElementsInAnyOrder("Expected attributes should be retained after merging.", expectedAttributes3, collection[0].Attributes.Attributes);
		}

		public void TestSumUpSameCharges_ByServiceID()
		{
			var frt = Factory.New<AccChargeCode>();
			frt.AC_Code = "FRT";

			var baf = Factory.New<AccChargeCode>();
			baf.AC_Code = "BAF";

			var infos = new AutoRateInfoCollection(Factory);
			var info1 = infos.AddNew(frt, "AUD", 10);
			info1.Attributes.Add(JobChargeAttribTypeList.Codes.ServiceID, "111");
			var info2 = infos.AddNew(frt, "AUD", 50);
			info2.Attributes.Add(JobChargeAttribTypeList.Codes.ServiceID, "222");
			var info3 = infos.AddNew(frt, "AUD", 100);
			info3.Attributes.Add(JobChargeAttribTypeList.Codes.ServiceID, "111");
			var info4 = infos.AddNew(baf, "AUD", 200);
			info4.Attributes.Add(JobChargeAttribTypeList.Codes.ServiceID, "111");

			infos.SumUpSameCharges();

			var actual = infos.Select(i => new
			{
				ChargeCode = (string)i.ChargeCode.AC_Code,
				Amount = i.Amount,
				ServiceID = i.Attributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.ServiceID)
			})
			.Select(i => $"{i.ChargeCode}|{i.Amount}|{i.ServiceID}")
			.ToArray();

			var expectedResult = new[]
			{
				"FRT|110|111",
				"FRT|50|222",
				"BAF|200|111",
			};

			AssertContainsExactElementsInAnyOrder(
				"The summed up charges should match the expected collection",
				expectedResult,
				actual
			);
		}

		public void TestSumUpSameCharges_OverridenDescription()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_Code = "BLAH";
			dummyChargeCode.AC_Desc = "Default Charge Code";
			dummyChargeCode.AC_ChargeGroup = "WHS";
			dummyChargeCode.AC_RateCalculator = MinimumCalculator.Code;

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var clientEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			var clientRateLine1 = clientEntry.AddRateLine(dummyChargeCode.AC_Code, MinimumCalculator.Code);
			var clientRateLine2 = clientEntry.AddRateLine(dummyChargeCode.AC_Code, MinimumCalculator.Code);
			clientRateLine2.TL_RateDesc = "Overriden Description";

			var criteria = new TestRatingCriteria();
			var autoRatingParams = new AutoRatingCalculatorParametersForTesting(criteria);

			var clientRateResult = CalculationResult.CreateForTest(clientRateLine1, 100m, 100m, 100m, criteria);
			var clientRateInfo = new AutoRateInfo(clientRateResult, autoRatingParams, Factory);

			var costingResult = CalculationResult.CreateForTest(clientRateLine2, 100m, 100m, 100m, criteria);
			var costingInfo = new AutoRateInfo(costingResult, autoRatingParams, Factory);

			Collection.Add(clientRateInfo);
			Collection.SumUpSameCharges();

			AssertEquals("Should include client rate info into collection", clientRateInfo, Collection[0]);
			AssertEquals("Expected to be default charge code only", "Default Charge Code", Collection[0].InvoiceLineDescription);

			Collection.Add(costingInfo);
			Collection.SumUpSameCharges();

			AssertEquals("Should include client rate info into collection", clientRateInfo, Collection[0]);
			AssertEquals("Should not combine overriden charge code with default description", "Overriden Description", Collection[0].InvoiceLineDescription);
		}

		public void TestSumUpSameCharges_CalculationLogs()
		{
			var chargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var clientEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			var clientRateLine = clientEntry.AddRateLine(chargeCode.AC_Code, MinimumCalculator.Code);

			var criteria = new TestRatingCriteria();
			var autoRatingParams = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(clientRateLine, 10, decimal.MinValue, decimal.MaxValue, criteria);

			AutoRateInfo CreateRateInfoWithCalculationLogs(string[] calculatorCodes)
			{
				var result = new AutoRateInfo(calculationResult, autoRatingParams, Factory);

				foreach (var calculatorCode in calculatorCodes)
				{
					var log = new CalculationLog();
					log.CalculatorCode = calculatorCode;

					result.CalculationLogs.Logs.Add(log);
				}

				return result;
			}

			var rateInfo1 = CreateRateInfoWithCalculationLogs(new[] { "AAA" });
			var rateInfo2 = CreateRateInfoWithCalculationLogs(new[] { "BB1", "BB2" });
			var rateInfo3 = CreateRateInfoWithCalculationLogs(new[] { "CCC" });
			var rateInfo4 = CreateRateInfoWithCalculationLogs(new[] { "DDD" });
			rateInfo3.ChargeCode = Factory.New<AccChargeCode>();

			var infoCollection = new AutoRateInfoCollection(Factory);
			infoCollection.Add(rateInfo1);
			infoCollection.Add(rateInfo2);
			infoCollection.Add(rateInfo3);

			AssertEquals("Precondition", 1, rateInfo1.CalculationLogs.Logs.Count);

			infoCollection.SumUpSameCharges();
			AssertEquals("Calculation logs added", 3, rateInfo1.CalculationLogs.Logs.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "BB1", "BB2" }, rateInfo1.CalculationLogs.Logs.Select(x => x.CalculatorCode));
		}

		public void TestAddAndCheckAutoRateInfoCollection()
		{
			var newInfo = new AutoRateInfo(Collection.Factory);
			ChargeCode1.AC_GC = ZGuid.NewZGuid();
			newInfo.ChargeCode = ChargeCode1;
			var autoRateInfoCollection = new List<AutoRateInfo>() { newInfo };
			Collection.CheckAndAddRange(autoRateInfoCollection);

			AssertEquals("AutoRating should not be mapping other Company's Charge Codes which is causing Issue 00850403. Please report to the Rating Team.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation

		AccChargeCode ChargeCode1;
		AccChargeCode ChargeCode2;

		RefCurrency Currency1;
		RefCurrency Currency2;

		protected override void SetUp()
		{
			base.SetUp();

			ChargeCode1 = Factory.New<AccChargeCode>();
			ChargeCode1.AC_ChargeType = Constants.ChargeType.Margin;

			ChargeCode2 = Factory.New<AccChargeCode>();
			ChargeCode2.AC_ChargeType = Constants.ChargeType.Disbursement;

			Currency1 = Factory.New<RefCurrency>();
			Currency1.RX_Code = "AUD";

			Currency2 = Factory.New<RefCurrency>();
			Currency2.RX_Code = "USD";
		}

		AutoRateInfo AddNewInfo(AutoRateInfoCollection collection, AccChargeCode chargeCode, ZString currency, ZDecimal amount, ZString calculationDescription, ZString invoiceLineDesc, string chargeUnit = "")
		{
			var newInfo = collection.AddNew(chargeCode, currency, amount);
			if (!string.IsNullOrEmpty(calculationDescription))
			{
				newInfo.CalculationDescription = calculationDescription;
			}

			newInfo.InvoiceLineDescription = invoiceLineDesc;
			newInfo.ChargeUnit = chargeUnit;

			return newInfo;
		}

		#endregion
	}
}