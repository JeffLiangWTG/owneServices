using Enterprise.Packing.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocumentSupporterTestCase : Environment.Business.Testing.WhsDocumentSupporterTestCase
	{
		#region IsFinalised Assertions

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);

		public static void AssertIsFinalisedPrecondition(WhsDocketLine docketLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine);

		public static void AssertIsFinalisedPrecondition(WhsPick pick) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

		public static void AssertIsFinalisedPrecondition(WhsVASOrder vasOrder) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder);

		#endregion

		#region Properties

		WhsTestHelperFunctions helper;
		protected new WhsTestHelperFunctions Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctions(Factory);
				}
				return helper;
			}
		}

		#endregion

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion
	}
}
