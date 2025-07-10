using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)).GetType() == typeof(CusEntryLineFee));
		}

		public void TestCusTariff()
		{
			var tariffType = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "AAA");
			Factory.Save();
			var tariff = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "99988", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeType = tariffType.ZZI_TariffType;
			AssertEquals(tariff, fee.CusTariff);
		}

		public void TestCusTariffType()
		{
			var tariffType = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "AAA");
			Factory.Save();
			var fee = Factory.New<CusEntryLineFee>();
			fee.CF_ChargeType = tariffType.ZZI_TariffType;
			AssertEquals(tariffType, fee.CusTariffType);
		}

		public void TestIsDutyPayable()
		{
			var tariffTypes = new CodeDescriptionPairList();
			tariffTypes.AddRange(RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica));
			var testFee = Factory.NewWithValidTestData<CusEntryLineFee>();
			foreach (var code in new[] { "12A", "12B", "13A", "13B", "13C", "13D", "15A", "15B", "1P1", "13A", "2P1", "2P2", "2P3" })
			{
				testFee.CF_ChargeType = code;
				AssertEquals(code, true, testFee.IsPayableDuty);
				tariffTypes.RemoveCode(code);
			}

			foreach (ICodeDescription pair in tariffTypes)
			{
				testFee.CF_ChargeType = pair.Code;
				AssertEquals(pair.Code, false, testFee.IsPayableDuty);
			}

			testFee.CF_ChargeType = "VAT";
			Assert(!testFee.IsPayableDuty);
		}

		public void TestIsRebate()
		{
			var testFee = Factory.NewWithValidTestData<CusEntryLineFee>();
			foreach (var code in new string[] { "3P1", "3P2", "4P1", "4P2", "4P3", "4P4", "4P5", "4P6" })
			{
				testFee.CF_ChargeType = code;
				AssertEquals(code, true, testFee.IsRebate);
			}

			foreach (var code in new string[] { "12A", "12B", "13A", "13B", "13C", "13D", "15A", "15B", "1P1", "13A", "2P1", "2P2", "2P3" })
			{
				testFee.CF_ChargeType = code;
				AssertEquals(code, false, testFee.IsRebate);
			}
		}

		public void TestReadOnlyFields()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			_ = zaTestHelper.CreateCustomsStatusCusCodeEntry("6");

			var dec = Factory.New<JobDeclaration>();
			var header = dec.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();
			var fee = line.Fees.AddOrUpdate("VAT", 200m);
			Factory.Save();

			AssertEquals("CF_IsLandedCostOnly is ReadOnly", true, fee.CF_IsLandedCostOnlyInfo.ReadOnly);
			AssertEquals("CF_ChargeAmount is ReadOnly", true, fee.CF_ChargeAmountInfo.ReadOnly);
			AssertEquals("CF_RateOverrideReasonCode is ReadOnly", true, fee.CF_RateOverrideReasonCodeInfo.ReadOnly);
			AssertEquals("CF_ChargeType is ReadOnly", true, fee.CF_ChargeTypeInfo.ReadOnly);

			var testMessage = CreateZAMessage(Messaging.Business.EDIMessage.Direction.Receive, SARSEDIMessage.MessageTypes.CUSRES, CUSRESMessageProcessorTest.GetTestMessageResNo("6"), "202");
			AssertEquals("6", testMessage.EntryStatus);
			header.Messages.Add(testMessage);
			Factory.Save();

			AssertEquals("CF_IsLandedCostOnly is ReadOnly", false, fee.CF_IsLandedCostOnlyInfo.ReadOnly);
			AssertEquals("CF_ChargeAmount is ReadOnly", false, fee.CF_ChargeAmountInfo.ReadOnly);
			AssertEquals("CF_RateOverrideReasonCode is ReadOnly", false, fee.CF_RateOverrideReasonCodeInfo.ReadOnly);
			AssertEquals("CF_ChargeType is ReadOnly", false, fee.CF_ChargeTypeInfo.ReadOnly);
		}

		CUSRESEDIMessage CreateZAMessage(string receiveTransmit, string messageType, string messageText, string messageNum)
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (CUSRESEDIMessage)testInterchange.ContainedMessages.AddNew(typeof(CUSRESEDIMessage));
			testMessage.EM_ReceiveTransmit = receiveTransmit;
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = messageType;
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = messageText.Replace("\r\n", "");
			testMessage.EM_MessageNum = messageNum;
			testMessage.EM_EI = testInterchange.PK;
			return testMessage;
		}

		protected override void SetUp()
		{
			universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
	}
}
