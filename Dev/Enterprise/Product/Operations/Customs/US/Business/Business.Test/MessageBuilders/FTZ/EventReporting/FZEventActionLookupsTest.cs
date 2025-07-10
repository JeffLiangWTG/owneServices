using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZEventActionLookupsTest : TestCaseWithFactory
	{
		public void TestActionCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var actionList = new FZEventAction(declaration, FZEventType.Concur).Lookups.ActionCodeList;
			AssertEquals("A, B, C", actionList.CodesAsString);
			AssertSame(new FZEventAction(declaration, FZEventType.Concur).Lookups.ActionCodeList, actionList);

			actionList = new FZEventAction(declaration, FZEventType.Delivery).Lookups.ActionCodeList;
			AssertEquals(false, object.ReferenceEquals(new FZEventAction(declaration, FZEventType.Concur).Lookups.ActionCodeList, actionList));
			AssertEquals("G, H", actionList.CodesAsString);
			AssertSame(new FZEventAction(Factory.New<JobDeclaration>(), FZEventType.Delivery).Lookups.ActionCodeList, actionList);
		}

		public void TestReasonCodeList()
		{
			var ftzEventAction = new FZEventAction(Factory.New<JobDeclaration>(), FZEventType.Concur);
			AssertSame(Factory.GetCachedValue<FTZUnconcurrenceReasonCodeList>(), ftzEventAction.Lookups.ReasonCodeList);
		}
	}
}
