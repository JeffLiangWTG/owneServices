using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReSequencer))]
	sealed class ReSequencerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var sequence = new SequenceNumberForTesting();
			var reSequencer = new ReSequencer(new[] { sequence }, Factory);
			AssertEquals((ZShort)1, reSequencer.SequenceStep);
		}

		public void TestReSequence()
		{
			var sequence3 = new SequenceNumberForTesting() { SequenceNumber = 1 };
			var sequence2 = new SequenceNumberForTesting() { SequenceNumber = 3 };
			var sequence1 = new SequenceNumberForTesting() { SequenceNumber = 2 };
			var reSequencer = new ReSequencer(new[] { sequence1, sequence2, sequence3 }, Factory);
			reSequencer.ReSequence();
			AssertEquals("link3.SequenceNumber", (ZShort)1, sequence3.SequenceNumber);
			AssertEquals("link1.SequenceNumber", (ZShort)2, sequence1.SequenceNumber);
			AssertEquals("link2.SequenceNumber", (ZShort)3, sequence2.SequenceNumber);

			reSequencer.StartSequence = 31;
			reSequencer.SequenceStep = 10;
			reSequencer.ReSequence();
			AssertEquals("link3.SequenceNumber", (ZShort)31, sequence3.SequenceNumber);
			AssertEquals("link1.SequenceNumber", (ZShort)41, sequence1.SequenceNumber);
			AssertEquals("link2.SequenceNumber", (ZShort)51, sequence2.SequenceNumber);

			reSequencer.StartSequence = 0;
			reSequencer.SequenceStep = 3;
			reSequencer.ReSequence();
			AssertEquals("link3.SequenceNumber", (ZShort)31, sequence3.SequenceNumber);
			AssertEquals("link1.SequenceNumber", (ZShort)41, sequence1.SequenceNumber);
			AssertEquals("link2.SequenceNumber", (ZShort)51, sequence2.SequenceNumber);

			reSequencer.StartSequence = 7;
			reSequencer.SequenceStep = 0;
			reSequencer.ReSequence();
			AssertEquals("link3.SequenceNumber", (ZShort)31, sequence3.SequenceNumber);
			AssertEquals("link1.SequenceNumber", (ZShort)41, sequence1.SequenceNumber);
			AssertEquals("link2.SequenceNumber", (ZShort)51, sequence2.SequenceNumber);

			reSequencer.SequenceStep = 7;
			reSequencer.ReSequence();
			AssertEquals("link3.SequenceNumber", (ZShort)7, sequence3.SequenceNumber);
			AssertEquals("link1.SequenceNumber", (ZShort)14, sequence1.SequenceNumber);
			AssertEquals("link2.SequenceNumber", (ZShort)21, sequence2.SequenceNumber);

			reSequencer.StartSequence = short.MaxValue - 1;
			reSequencer.ReSequence();
			AssertEquals("link3.SequenceNumber", (ZShort)short.MaxValue - 1, sequence3.SequenceNumber);
			AssertEquals("link1.SequenceNumber", (ZShort)short.MaxValue, sequence1.SequenceNumber);
			AssertEquals("link2.SequenceNumber", (ZShort)short.MaxValue, sequence2.SequenceNumber);
		}

		public void TestValidation()
		{
			var sequence = new SequenceNumberForTesting();
			var reSequencer = new ReSequencer(new[] { sequence }, Factory);
			reSequencer.RunPreSaveValidation();
			var startSequenceMessageError = "Please enter a 'Start Sequence' greater than or equal to 1.";
			AssertHasError(reSequencer.StartSequenceInfo, startSequenceMessageError);
			var sequenceStepMessageError = "Please enter a 'Sequence Step' greater than or equal to 1.";
			AssertNoError(reSequencer.SequenceStepInfo, sequenceStepMessageError);
			reSequencer.StartSequence = 0;
			AssertHasError(reSequencer.StartSequenceInfo, startSequenceMessageError);

			reSequencer.SequenceStep = 1;
			AssertNoError(reSequencer.SequenceStepInfo, sequenceStepMessageError);
			reSequencer.SequenceStep = 0;
			AssertHasError(reSequencer.SequenceStepInfo, sequenceStepMessageError);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReSequencer(new[] { new SequenceNumberForTesting() }, Factory);
		}
	}
}
