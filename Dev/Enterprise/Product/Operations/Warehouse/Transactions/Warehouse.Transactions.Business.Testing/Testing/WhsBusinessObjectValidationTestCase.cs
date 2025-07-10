using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsBusinessObjectValidationTestCase : Environment.Business.Testing.WhsBusinessObjectValidationTestCase
	{
		#region IsFinalised Assertions

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);

		public static void AssertIsFinalisedPrecondition(WhsDocketLine docketLine) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docketLine);

		public static void AssertIsFinalisedPrecondition(WhsPick pick) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

		public static void AssertIsFinalisedPrecondition(WhsVASOrder vasOrder) => WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder);

		#endregion

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctions(Factory);
		}

		protected new WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = (WhsTestHelperFunctions)GetNewTestHelperFunctions()); }
		}
		WhsTestHelperFunctions helper;

		protected TypeValidationLimits ValidationLimits
		{
			get { return validationLimits ?? (validationLimits = new TypeValidationLimits()); }
		}
		TypeValidationLimits validationLimits;
	}
}
