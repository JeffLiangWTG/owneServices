using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPickFaceAwaitingReplenishmentView))]
	class WhsPickFaceAwaitingReplenishmentViewTestCase : WhsEnvBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete WhsPickFaceAwaitingReplenishmentView.", false, GetNewBusinessObject().CanDelete);
		}

		protected override bool IsDeleteSupported() => false;
	}
}
