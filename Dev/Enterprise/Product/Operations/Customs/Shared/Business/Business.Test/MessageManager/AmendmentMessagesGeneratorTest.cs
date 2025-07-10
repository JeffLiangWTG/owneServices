using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AmendmentMessagesGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateAmendmentMessageSet()
		{
			EDIMessage[] result = Generator.GenerateAmendmentMessageSet();
			AssertEquals(1, result.Length);
			AssertIsQueuedAmendment(result[0]);
			SavedDummy.Z0_VarCharMax = "123";
			result = Generator.GenerateAmendmentMessageSet();
			AssertEquals(2, result.Length);
			AssertIsQueuedWithdrawal(result[0]);
			AssertIsPendingOriginal(result[1]);
		}

		public void TestGeneratePendingOriginalMessage()
		{
			EDIMessage result = Generator.GeneratePendingOriginalMessage();
			AssertIsPendingOriginal(result);
		}

		public void TestGenerateQueuedAmendmentMessage()
		{
			EDIMessage result = Generator.GenerateQueuedAmendmentMessage();
			AssertIsQueuedAmendment(result);
		}

		public void TestGenerateQueuedWithdrawalMessage()
		{
			EDIMessage result = Generator.GenerateQueuedWithdrawalMessage();
			AssertIsQueuedWithdrawal(result);
		}

		public void TestUniqueIdentifierBeingChanged()
		{
			AssertEquals("UniqueIdentifierBeingChanged", false, Generator.UniqueIdentifierBeingChanged);
			SavedDummy.Z0_VarCharMax = "123";
			AssertEquals("UniqueIdentifierBeingChanged", true, Generator.UniqueIdentifierBeingChanged);
			SavedDummy.Z0_VarCharMax = (ZString)SavedDummy.Z0_VarCharMaxInfo.OriginalValue;
			AssertEquals("UniqueIdentifierBeingChanged", false, Generator.UniqueIdentifierBeingChanged);
		}

		#region Implementation

		void AssertIsPendingOriginal(EDIMessage message)
		{
			AssertEquals("EM_Status", EDIMessage.Status.Pending, message.EM_Status);
			AssertEquals("EM_MessageText", "OriginalText", message.EM_MessageText);
		}

		void AssertIsQueuedAmendment(EDIMessage message)
		{
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_MessageText", "AmendmentText", message.EM_MessageText);
		}

		void AssertIsQueuedWithdrawal(EDIMessage message)
		{
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_MessageText", "WithdrawalText", message.EM_MessageText);
		}

		TestHelperAmendmentMessagesGenerator generator;
		TestHelperAmendmentMessagesGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new TestHelperAmendmentMessagesGenerator(SavedDummy);
				}
				return generator;
			}
		}

		#region TestHelper

		class TestHelperAmendmentMessagesGenerator : AmendmentMessagesGenerator
		{
			public TestHelperAmendmentMessagesGenerator(DummyBusinessObject dummy) : base(dummy)
			{
			}

			protected override EDIMessage GenerateAmendmentMessageCore()
			{
				EDIMessage result = businessObject.Factory.New<EDIMessage>();
				result.EM_MessageText = "AmendmentText";
				return result;
			}

			protected override EDIMessage GenerateOriginalMessageCore()
			{
				EDIMessage result = businessObject.Factory.New<EDIMessage>();
				result.EM_MessageText = "OriginalText";
				return result;
			}

			protected override EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory)
			{
				EDIMessage result = businessObject.Factory.New<EDIMessage>();
				result.EM_MessageText = "WithdrawalText";
				return result;
			}

			protected override ZPropertyInfo[] UniqueIdentifierInfos
			{
				get
				{
					return new ZPropertyInfo[] { ((DummyBusinessObject)businessObject).Z0_VarCharMaxInfo };
				}
			}
		}

		#endregion

		DummyBusinessObject savedDummy;
		DummyBusinessObject SavedDummy
		{
			get
			{
				if (savedDummy == null)
				{
					savedDummy = Factory.New<DummyBusinessObject>();
					Factory.Save();
				}
				return savedDummy;
			}
		}

		#endregion
	}
}
