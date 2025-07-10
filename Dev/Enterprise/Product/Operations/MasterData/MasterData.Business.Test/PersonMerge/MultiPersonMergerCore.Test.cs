using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	class MultiPersonMergerCoreTest : TestCaseWithFactory
	{
		public void TestMerge()
		{
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "John Mark";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Peter Chen";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Henry George";
			dissolvedPerson2.PER_HomePhone = "+61 3 1415 9265";

			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			Factory.Save();

			var multiPersonMerger = new MultiPersonMergerTest.MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			CombineAssertions(() =>
			{
				AssertNull(Factory.Load<GlbPerson>(dissolvedPerson1.PK));
				AssertNull(Factory.Load<GlbPerson>(dissolvedPerson2.PK));
				AssertEquals("+61 3 1415 9265", retainedPerson.PER_HomePhone);
			});
		}

		internal class MultiPersonMergerCoreForTest : MultiPersonMergerCore
		{
			readonly IEnumerator<Exception> enumeratorToThrowException;
			readonly IEnumerator<IPersonMergeTransactionSaver> enumeratorTransactionSaver;

			public MultiPersonMergerCoreForTest(IEnumerator<IPersonMergeTransactionSaver> enumeratorTransactionSaver, IEnumerator<Exception> enumeratorToThrowException)
				: base(new PersonMergeTransactionSaver())
			{
				this.enumeratorToThrowException = enumeratorToThrowException;
				this.enumeratorTransactionSaver = enumeratorTransactionSaver;
			}

			public override MultiPersonMergerResult Merge(GlbPerson retained, GlbPerson dissolved)
			{
				enumeratorTransactionSaver.MoveNext();      //move next element from tuple collection
				TransactionSaver = enumeratorTransactionSaver.Current;

				enumeratorToThrowException.MoveNext();
				if (enumeratorToThrowException.Current != null)
				{
					throw enumeratorToThrowException.Current;
				}

				return base.Merge(retained, dissolved);
			}
		}
	}
}
