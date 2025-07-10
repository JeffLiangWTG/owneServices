using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPickFaceCommittedStockView))]
	class WhsPickFaceCommittedStockViewTestCase : WhsEnvBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete WhsPickFaceCommittedStockView.", false, GetNewBusinessObject().CanDelete);
		}

		protected override bool IsDeleteSupported() => false;
	}
}
