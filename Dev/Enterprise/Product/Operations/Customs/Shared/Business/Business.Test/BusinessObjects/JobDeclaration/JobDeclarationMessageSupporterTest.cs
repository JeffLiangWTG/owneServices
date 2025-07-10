using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class JobDeclarationMessageSupporterTest<T> : TestCaseWithFactory
		where T : BaseJobDeclaration
	{
		public abstract void TestIJobDeclarationMessageSupporterMembers();

		public void TestGetMessageSupporterByCountry()
		{
			var supporter = (Integration.Customs.IJobDeclarationAutoSendingMessageSupporter)Factory.New<BaseJobDeclaration>();
			AssertNotNull(supporter);
			AssertEquals(typeof(T), supporter.GetType());
		}
	}
}
