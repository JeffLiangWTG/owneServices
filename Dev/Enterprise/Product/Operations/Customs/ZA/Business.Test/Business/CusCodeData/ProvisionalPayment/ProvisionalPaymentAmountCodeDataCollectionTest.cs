using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ProvisionalPaymentAmountCodeDataCollection))]
	sealed class ProvisionalPaymentAmountCodeDataCollectionTest : CusCodeDataCollectionTest<ProvisionalPaymentAmountCodeData>
	{
		public void TestGetAmount()
		{
			var testEntryLine = Factory.New<CusEntryLine>();
			var testCollection = new ProvisionalPaymentAmountCodeDataCollection(testEntryLine);
			testCollection.AddNew("PPA", 11.11);
			testCollection.AddNew("PPC", 22.22);
			testCollection.AddNew("PPG", 33.33);
			testCollection.AddNew("PPT", 44.44);
			testCollection.AddNew("PPR", 55.55);
			testCollection.AddNew("PPE", 66.66);
			testCollection.AddNew("PEN", 77.77);
			testCollection.AddNew("FOR", 88.88);
			testCollection.AddNew("XXT", 99.99);
			testCollection.AddNew("", 1010.1010);
			testCollection.AddNew("PPA", 0.11);
			testCollection.AddNew("PPC", 0.22);
			testCollection.AddNew("PPG", 0.33);
			testCollection.AddNew("PPT", 0.44);
			testCollection.AddNew("PPR", 0.55);
			testCollection.AddNew("PPE", 0.66);
			testCollection.AddNew("PEN", 0.77);
			testCollection.AddNew("FOR", 0.88);
			testCollection.AddNew("XXT", 0.99);
			testCollection.AddNew("", 0.101);
			CombineAssertions(() =>
			{
				AssertEquals(11.22m, testCollection.GetAmount("PPA"));
				AssertEquals(22.44m, testCollection.GetAmount("PPC"));
				AssertEquals(33.66m, testCollection.GetAmount("PPG"));
				AssertEquals(44.88m, testCollection.GetAmount("PPT"));
				AssertEquals(56.10m, testCollection.GetAmount("PPR"));
				AssertEquals(67.32m, testCollection.GetAmount("PPE"));
				AssertEquals(78.54m, testCollection.GetAmount("PEN"));
				AssertEquals(89.76m, testCollection.GetAmount("FOR"));
				AssertEquals(100.98m, testCollection.GetAmount("XXT"));
				AssertEquals(1010.2020m, testCollection.GetAmount(""));
				AssertEquals(0m, testCollection.GetAmount("EMP"));
			});
		}

		public void TestGetAmountOfRateType()
		{
			var testEntryLine = Factory.New<CusEntryLine>();
			var testCollection = new ProvisionalPaymentAmountCodeDataCollection(testEntryLine);
			testCollection.AddNew("PPA", "11.11");
			testCollection.AddNew("PPC", "22.22");
			testCollection.AddNew("PPG", "33.33");
			testCollection.AddNew("PPT", "44.44");
			testCollection.AddNew("PPR", "55.55");
			testCollection.AddNew("PPE", "66.66");
			testCollection.AddNew("PEN", "77.77");
			testCollection.AddNew("FOR", "88.88");
			testCollection.AddNew("XXT", "99.99");
			testCollection.AddNew("", "1010.1010");
			testCollection.AddNew("PPA", "0.11");
			testCollection.AddNew("PPC", "0.22");
			testCollection.AddNew("PPG", "0.33");
			testCollection.AddNew("PPT", "0.44");
			testCollection.AddNew("PPR", "0.55");
			testCollection.AddNew("PPE", "0.66");
			testCollection.AddNew("PEN", "0.77");
			testCollection.AddNew("FOR", "0.88");
			testCollection.AddNew("XXT", "0.99");
			testCollection.AddNew("", "0.1010");
			CombineAssertions(() =>
			{
				AssertEquals(11.22m, testCollection.GetAmount("PPA"));
				AssertEquals(22.44m, testCollection.GetAmount("PPC"));
				AssertEquals(33.66m, testCollection.GetAmount("PPG"));
				AssertEquals(44.88m, testCollection.GetAmount("PPT"));
				AssertEquals(56.10m, testCollection.GetAmount("PPR"));
				AssertEquals(67.32m, testCollection.GetAmount("PPE"));
				AssertEquals(78.54m, testCollection.GetAmount("PEN"));
				AssertEquals(89.76m, testCollection.GetAmount("FOR"));
				AssertEquals(100.98m, testCollection.GetAmount("XXT"));
				AssertEquals(1010.2020m, testCollection.GetAmount(""));
				AssertEquals(168.3m, testCollection.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty).Amount);
				var penaltyDetails = testCollection.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty).Details.ToArray();
				AssertEquals(4, penaltyDetails.Length);
				AssertEquals(89.76m, penaltyDetails.Where(x => x.Code == "FOR").Sum(x => x.Value));
				AssertEquals(78.54m, penaltyDetails.Where(x => x.Code == "PEN").Sum(x => x.Value));
				AssertEquals(235.62m, testCollection.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment).Amount);
				var provisionalPaymentDetails = testCollection.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment).Details.ToArray();
				AssertEquals(12, provisionalPaymentDetails.Length);
				AssertEquals(11.22m, provisionalPaymentDetails.Where(x => x.Code == "PPA").Sum(x => x.Value));
				AssertEquals(22.44m, provisionalPaymentDetails.Where(x => x.Code == "PPC").Sum(x => x.Value));
				AssertEquals(33.66m, provisionalPaymentDetails.Where(x => x.Code == "PPG").Sum(x => x.Value));
				AssertEquals(44.88m, provisionalPaymentDetails.Where(x => x.Code == "PPT").Sum(x => x.Value));
				AssertEquals(56.10m, provisionalPaymentDetails.Where(x => x.Code == "PPR").Sum(x => x.Value));
				AssertEquals(67.32m, provisionalPaymentDetails.Where(x => x.Code == "PPE").Sum(x => x.Value));
				AssertEquals(0m, testCollection.GetAmountOfRateType("EMP").Amount);
				Assert(!testCollection.GetAmountOfRateType("EMP").Details.Any());
				AssertEquals(0m, testCollection.GetAmountOfRateType("").Amount);
				Assert(!testCollection.GetAmountOfRateType("").Details.Any());
				AssertEquals(0m, testCollection.GetAmountOfRateType("XXT").Amount);
				Assert(!testCollection.GetAmountOfRateType("XXT").Details.Any());
				AssertEquals(0m, testCollection.GetAmountOfRateType("FOR").Amount);
				Assert(!testCollection.GetAmountOfRateType("FOR").Details.Any());
			});
		}

		public void TestGetAmountOfRateType_ExcludeCaseClosedValue()
		{
			var testEntry = Factory.New<CusEntryHeader>();
			var testPayInfo1 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF1", true);
			var testPayInfo2 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPC, 0, "1", "REF2", true);
			var testPayInfo3 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPR, 0, "1", "REF3", false);
			var testPayInfo4 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.FOR, 0, "1", "REF4", true);
			var testPayInfo5 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPG, 0, "1", "REF5", true);
			var testPayInfo6 = testEntry.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPR, 0, "2", "REF6", true);
			var testLine = testEntry.MergedLines.AddNew();
			testLine.CL_LineNumber = 1;
			testLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 11.11);
			testLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPC, 21.11);
			testLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPG, 31.11);
			testLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPR, 41.11);
			testLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 51.11);
			testLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.FOR, 61.11);
			CombineAssertions(() =>
			{
				AssertEquals(41.11m, testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment).Amount);
				var provisionalPaymentDetails = testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment).Details.ToArray();
				AssertEquals(1, provisionalPaymentDetails.Length);
				AssertEquals(41.11m, provisionalPaymentDetails.Single(x => x.Code == "PPR").Value);
				AssertEquals(51.11m, testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty).Amount);
				var penaltyDetails = testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty).Details.ToArray();
				AssertEquals(1, penaltyDetails.Length);
				AssertEquals(51.11m, penaltyDetails.Single(x => x.Code == "PEN").Value);
			});
			testPayInfo1.C9_RemAdvReceived = false;
			testPayInfo2.C9_RemAdvReceived = false;
			testPayInfo3.C9_RemAdvReceived = false;
			testPayInfo4.C9_RemAdvReceived = false;
			testPayInfo5.C9_RemAdvReceived = false;
			testPayInfo6.C9_RemAdvReceived = false;
			CombineAssertions(() =>
			{
				AssertEquals(104.44m, testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment).Amount);
				var provisionalPaymentDetails = testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.ProvisionalPayment).Details.ToArray();
				AssertEquals(4, provisionalPaymentDetails.Length);
				AssertEquals(11.11m, provisionalPaymentDetails.Single(x => x.Code == "PPA").Value);
				AssertEquals(21.11m, provisionalPaymentDetails.Single(x => x.Code == "PPC").Value);
				AssertEquals(31.11m, provisionalPaymentDetails.Single(x => x.Code == "PPG").Value);
				AssertEquals(41.11m, provisionalPaymentDetails.Single(x => x.Code == "PPR").Value);
				AssertEquals(112.22m, testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty).Amount);
				var penaltyDetails = testLine.ProvisionalPayments.GetAmountOfRateType(Universal.Constants.RateTypes.Penalty).Details.ToArray();
				AssertEquals(2, penaltyDetails.Length);
				AssertEquals(61.11m, penaltyDetails.Single(x => x.Code == "FOR").Value);
				AssertEquals(51.11m, penaltyDetails.Single(x => x.Code == "PEN").Value);
			});
		}

		protected override CusCodeDataCollection<ProvisionalPaymentAmountCodeData> GetCusCodeDataCollection() => Factory.New<CusEntryLine>().ProvisionalPayments;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ProvisionalPaymentAmountCodeData>();
	}
}
