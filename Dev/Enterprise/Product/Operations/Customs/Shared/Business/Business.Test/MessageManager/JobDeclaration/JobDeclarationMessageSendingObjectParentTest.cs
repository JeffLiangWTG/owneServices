using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>))]
	sealed class JobDeclarationMessageSendingObjectParentBaseOnlyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBizObjValidationMessageErrors()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var testItem = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			AssertEquals("", testItem.BizObjValidationMessageErrors);

			testItem.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First().ShouldSend = true;
			Assert(testItem.BizObjValidationMessageErrors.StartsWith("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:\r\n"));
		}

		public void TestAdditionalWarnings()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var testItem = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			AssertEquals("Additional Warnings", testItem.AdditionalWarnings);
		}

		public void TestGetSendingObjectsCollectionCore()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_JE = declaration.PK;
			entry1.CH_CEI_Instruction = instruction.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_JE = declaration.PK;
			entry2.CH_CEI_Instruction = instruction.PK;

			var testItem = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			AssertEquals(2, testItem.GetSendingObjectsCollectionCoreTest().Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var testItem = new JobDeclarationMessageSendingObjectParentForTest(declaration).MessageSendingObjectProperties.ToList();

			AssertEquals(3, testItem.Count);
			Assert("MessageSendingObjectProperties should contain \"MessageType\"", testItem.Exists(item => item.PropertyName == "MessageType"));
			Assert("MessageSendingObjectProperties should contain \"DeclarationType\"", testItem.Exists(item => item.PropertyName == "DeclarationType"));
			Assert("MessageSendingObjectProperties should contain \"EntryStatus\"", testItem.Exists(item => item.PropertyName == "EntryStatus"));
		}

		protected override BusinessObject GetNewBusinessObject() => new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(Factory.NewWithValidTestData<BaseJobDeclaration>());

		sealed class JobDeclarationMessageSendingObjectParentForTest : JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>
		{
			public JobDeclarationMessageSendingObjectParentForTest(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			public NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> GetSendingObjectsCollectionCoreTest()
			{
				return GetSendingObjectsCollectionCore();
			}

			protected override ZString GetAdditionalWarningsCore() => "Additional Warnings";
		}
	}
}
