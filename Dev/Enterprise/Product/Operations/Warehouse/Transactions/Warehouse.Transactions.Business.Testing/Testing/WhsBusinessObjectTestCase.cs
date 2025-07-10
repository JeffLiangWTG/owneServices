using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsBusinessObjectTestCase : WhsEnvBusinessObjectTestCase
	{
		#region Properties

		protected new WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#region IsFinalised Assertions

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);

		public static void AssertIsFinalisedPrecondition(WhsDocketLine docketLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine);

		public static void AssertIsFinalisedPrecondition(WhsPick pick) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

		public static void AssertIsFinalisedPrecondition(WhsPickLine pickLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pickLine);

		public static void AssertIsFinalisedPrecondition(WhsVASOrder vasOrder) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder);

		public static void AssertIsFinalised(WhsPutawayJob putawayJob) => WhsTestCaseWithFactory.AssertIsFinalised(putawayJob);

		#endregion
	}
}
