using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobChargeCollection))]
	public sealed class JobChargeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobChargeCollection(Factory);
		}

		public void TestLoadThrowsNotSupportedException()
		{
			AssertExceptionThrown<NotSupportedException>("We should not be using this collection to load JobCharges",
				@"Loading of the JobChargeCollection is not supported.
We experienced poor memory performance when the JobChargeCollection was used for loading the same JobCharge entity many times in different collections. Those collections were accumulated and not disposed,
Use Factory.Load<JobCharge>(...) and List<JobCharge> in non-GUI bound cases. See this alternative for the old BusinessObjectCollection https://stackoverflow.com/c/wisetechglobal/questions/901",
				() => Collection.Load());
		}

		public static void AssertNoJobChargesContainedInJobChargeCollection(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			var jobCharges = factory.Load<JobCharge>(new ZQuery() { FetchOnlyFromLocalCache = true });

			Assert("No JobCharge should be contained in a JobChargeCollection",
				!jobCharges.Cast<IBusinessObjectInternals>().Any(x => x.ParentCollections.OfType<JobChargeCollection>().Any()));
		}
	}
}
