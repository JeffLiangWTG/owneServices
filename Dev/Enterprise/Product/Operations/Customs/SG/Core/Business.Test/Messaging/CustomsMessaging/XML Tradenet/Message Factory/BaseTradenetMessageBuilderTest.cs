using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	public abstract class BaseTradenetMessageBuilderTest<T> : TestCaseWithFactory where T : ITradeNetSectionParent
	{
		public abstract void TestCreateTradeNetMessageParent();
		public abstract void TestGetMessageContent();
		protected abstract TradeNetMessageFactory<T> GetBuilder();
	}
}
