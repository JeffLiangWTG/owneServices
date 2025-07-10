using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CurrencyProcessorTest : ABIProcessorTest<CurrencyProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.CurrencyUpdate;
			var percent1 = new CURPercent1();
			percent1.ExchangeRate1 = 1.23m;
			percent1.ISOCountryCode = "AU";
			percent1.ISOCurrencyCode = "AUD";
			percent1.Indicator1 = "D";
			percent1.ExchangeRateDate1 = ZDate.Today;
			generator.AddMessageBlock(percent1);

			ProcessMessage(generator);

			var currencey = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"));
			AssertEquals(1.23m, currencey.GetCustomsRate(percent1.ExchangeRateDate1));
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 0; }
		}
	}
}
