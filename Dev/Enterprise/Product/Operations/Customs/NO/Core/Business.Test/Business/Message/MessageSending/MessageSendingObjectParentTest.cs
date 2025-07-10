using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectParent))]
	sealed class MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetSendingObjectsCollectionCore()
		{
			CombineAssertions(() =>
			{
				declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
				AssertEquals(nameof(Parent.SendingObjectsCollection), 0, Parent.SendingObjectsCollection.Count);
				declaration.CustomsEntryHeaders.AddNew();
				declaration.CustomsEntryHeaders.AddNew();
				var objectParent = new MessageSendingObjectParent(declaration);
				AssertEquals(nameof(objectParent), 2, objectParent.SendingObjectsCollection.Count);
			});
		}

		public void TestMessageSendingObjectProperties()
		{
			var properties = Parent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals(8, properties.Count());

				AssertMessageObjectProperty(0, nameof(MessageSendingObject.DeclarationType), 100, true);
				AssertMessageObjectProperty(1, nameof(MessageSendingObject.Description), 100, true);
				AssertMessageObjectProperty(2, nameof(MessageSendingObject.Procedure), 100, true);
				AssertMessageObjectProperty(3, nameof(MessageSendingObject.MessageType), 100, true);
				AssertMessageObjectProperty(4, nameof(MessageSendingObject.CustomsOffice), 200, true);
				AssertMessageObjectProperty(5, nameof(MessageSendingObject.EntryStatus), 100, true);
				AssertMessageObjectProperty(6, nameof(MessageSendingObject.EntryNumber), 100, true);
				AssertMessageObjectProperty(7, nameof(MessageSendingObject.PaymentMethod), 100, true);
			});

			void AssertMessageObjectProperty(int index, string propertyName, int width, bool isMandatory)
			{
				AssertEquals($"{propertyName}.PropertyName", propertyName, properties.ElementAt(index).PropertyName);
				AssertEquals($"{propertyName}.ColumnWidth", width, properties.ElementAt(index).ColumnWidth);
				AssertEquals($"{propertyName}.IsMandatory", isMandatory, properties.ElementAt(index).IsMandatory);
			}
		}

		public void TestParentDeclaration()
		{
			AssertType<JobDeclaration>(Parent.ParentDeclaration);
		}

		public void TestWarningMessageIfRejectedByCustoms()
		{
			var errorMessage = "This declaration has been rejected by customs (IU). Cannot be resent.";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "IU";

			AssertEqualsIgnoreLineBreaks("Warnings should be set", errorMessage, Parent.AdditionalWarnings);
		}

		protected override BusinessObject GetNewBusinessObject() => new MessageSendingObjectParent(declaration);

		public void TestRequestedProcessingDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Date should be empty", ZDate.Empty, Parent.RequestProcessingDate);

				Parent.RequestProcessingDate = ZDate.BrettsBirthday;
				AssertEquals("Values we set should be kept", ZDate.BrettsBirthday, Parent.RequestProcessingDate);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
		MessageSendingObjectParent Parent => (MessageSendingObjectParent)base.CachedBusinessObject;
	}
}
