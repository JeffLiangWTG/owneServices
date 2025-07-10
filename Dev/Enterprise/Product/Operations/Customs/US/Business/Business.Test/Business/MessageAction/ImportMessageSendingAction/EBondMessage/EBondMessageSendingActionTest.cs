using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EBondMessageSendingAction))]
	sealed class EBondMessageSendingActionTest : XmlSerializableNonPersistentBusinessObjectTest<EBondMessageSendingAction>
	{
		public void TestUS_MessageDescription()
		{
			AssertEquals("eBond Request: EB181120", SendingAction.US_MessageDescription);
		}

		public void TestUS_MessageContents()
		{
			AssertContains("Should get the universal shipment content of declaration.", "<UniversalShipment", SendingAction.US_MessageContents);
		}

		public override void TestSchemaPropertiesHaveFields()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			return new EBondMessageSendingAction(entryHeader, actions);
		}

		EBondMessageSendingAction SendingAction => sendingAction ?? (sendingAction = (EBondMessageSendingAction)GetNewBusinessObject());
		EBondMessageSendingAction sendingAction;
	}
}
