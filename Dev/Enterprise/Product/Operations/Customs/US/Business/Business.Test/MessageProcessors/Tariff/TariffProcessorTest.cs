using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class TariffProcessorTest : ABIProcessorTest<UnsolicitedTariffProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			var querySturgeon = new ZQuery(USCTariffSchema.UE_Tariff, "0399999999");
			var tariffSturgeon = Factory.LoadTop1<USCTariff>(querySturgeon);
			if (tariffSturgeon == null)
			{
				tariffSturgeon = Factory.New<USCTariff>();
				tariffSturgeon.UE_Tariff = "0399999999";
				tariffSturgeon.UE_DateFrom = new ZDate(2006, 01, 01);
				tariffSturgeon.UE_DateTo = new ZDate(2099, 12, 31);
			}
			AssertNotNull(tariffSturgeon);
			AssertEquals(new ZDate(2006, 01, 01), tariffSturgeon.UE_DateFrom);

			Factory.Save();

			var processor = new UnsolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
"V10299999999R0701059999991KG       1LAMB,BONE IN, FROZEN, LOIN    000000700000",
"V20299999999000000000000000000000000000015400000000000000000000000000000",
"V30299999999                                        D E J A+AUCACLILJOMAMXSG",
"V10399999999R0101061231061KG       7STURGEON ROE, FROZEN          000000000000",
"V20399999999000015000000000000000000000000000000000030000000000000000000",
"V30399999999                                        A E J AUCACLILJOMAMXSG",
"V50399999999SG000000000000000009300000000000000000");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var queryLamb = new ZQuery(USCTariffSchema.UE_Tariff, "0299999999");
			var tariffLamb = newFactory.LoadTop1<USCTariff>(queryLamb);
			AssertNotNull(tariffLamb);
			AssertEquals(0, tariffLamb.DutyRates.Count);
			AssertEquals("D E J A+AUCACLILJOMAMXSG", tariffLamb.UE_SPICode);

			var tariffsSturgeon = newFactory.Load<USCTariff>(querySturgeon);
			AssertEquals(1, tariffsSturgeon.Length);

			tariffSturgeon = tariffsSturgeon[0];
			tariffSturgeon.Reload();
			AssertNotNull(tariffSturgeon);
			AssertEquals(1, tariffSturgeon.DutyRates.Count);
			AssertEquals("SG", tariffSturgeon.DutyRates[0].UD_ISOCountryCode);
			AssertEquals(0.093m, tariffSturgeon.DutyRates[0].UD_AdValoremSpecialRate);
			AssertEquals(new ZDate(2006, 01, 01), tariffSturgeon.UE_DateFrom);
			AssertEquals(new ZDate(2006, 12, 31), tariffSturgeon.UE_DateTo);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 0; }
		}

		public void TestProcessEditRemovesUnusedDutyRates()
		{
			var processor1 = new UnsolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor1, ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
"V10399999999R0101061231061KG       7STURGEON ROE, FROZEN          000000000000",
"V20399999999000015000000000000000000000000000000000030000000000000000000",
"V30399999999                                        A E J AUCACLILJOMAMXSG",
"V50399999999SG000000000000000009300000000000000000",
"V60399999999MY000000000000000009300000000000000000");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor1.Message = responseMessage;
			processor1.Process();
			Factory.Save();

			var querySturgeon = new ZQuery(USCTariffSchema.UE_Tariff, "0399999999");
			var tariffSturgeon = Factory.LoadTop1<USCTariff>(querySturgeon);
			AssertNotNull(tariffSturgeon);
			AssertEquals(2, tariffSturgeon.DutyRates.Count);

			var processor2 = new UnsolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor2, ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
"V10399999999R0101061231061KG       7STURGEON ROE, FROZEN          000000000000",
"V20399999999000015000000000000000000000000000000000030000000000000000000",
"V30399999999                                        A E J AUCACLILJOMAMXSG",
"V50399999999MY000000000000000009300000000000000000");

			var factory2 = new BusinessObjectFactory();
			responseMessage = factory2.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor2.Message = responseMessage;
			processor2.Process();
			factory2.Save();

			var tariffSturgeon2 = factory2.LoadTop1<USCTariff>(querySturgeon);

			AssertEquals(1, tariffSturgeon2.DutyRates.Count);
			AssertEquals("MY", tariffSturgeon2.DutyRates[0].UD_ISOCountryCode);
		}
	}
}
