using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsTestCaseWithFactory : WhsTestCaseWithFactoryEnv
	{
		#region IsFinalised Assertions

		public static void AssertIsFinalisedPrecondition(WhsDocket docket) => AssertIsFinalisedPrecondition(docket, d => d.IsFinalised);

		public static void AssertIsFinalisedPrecondition(WhsDocketLine docketLine) => AssertIsFinalisedPrecondition(docketLine, dl => dl.IsFinalised);

		public static void AssertIsFinalisedPrecondition(WhsPick pick) => AssertIsFinalisedPrecondition(pick, p => p.IsFinalised);

		public static void AssertIsFinalisedPrecondition(WhsPickLine pickLine) => AssertIsFinalisedPrecondition(pickLine, pl => pl.IsFinalised);

		public static void AssertIsFinalisedPrecondition(WhsVASOrder vasOrder) => AssertIsFinalisedPrecondition(vasOrder, vo => vo.IsFinalised);

		public static void AssertIsFinalised(WhsPutawayJob putawayJob) => AssertIsFinalisedPrecondition(putawayJob, pj => pj.IsFinalised, false);

		static void AssertIsFinalisedPrecondition<T>(T bizo, Func<T, bool> isFinalised, bool isPrecondition = true)
			where T : BusinessObject
		{
			if (!isFinalised(bizo))
			{
				var errors = bizo.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString();
				var prefix = isPrecondition ? "Precondition - " : string.Empty;
				Fail($"{prefix}Ensure {bizo.HumanReadableName} is Finalised.\r\nNotifications:\r\n{errors}");
			}
		}

		#endregion

		#region Properties

		protected new WhsTestHelperFunctions Helper => (WhsTestHelperFunctions)base.Helper;

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctions(Factory);
		}

		#endregion
	}
}
