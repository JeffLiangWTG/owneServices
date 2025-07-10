using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseMessageSendingObjectParentTest : TestCaseWithFactory
	{
		public void TestMessageSendingObjectCollectionIsRegisteredAsEditableChild()
		{
			var parent = new BaseMessageSendingObjectParentForTest(Factory);
			Assert(parent.IsRegisteredEditableChildObject(parent.SendingObjectsCollection));
		}

		public void TestGetBizObjValidationMessageErrors()
		{
			var parent = new BaseMessageSendingObjectParentForTest(Factory);
			var declaration = (BaseJobDeclaration)parent.TopLevelBusinessObject;
			declaration.JE_MessageType = "XXX";
			Assert("Precondition", declaration.HasMessageErrors);
			var sendObject = new BaseMessageSendingObjectForTest(Factory);
			var coll = new BaseMessageSendingObjectCollectionForTest(Factory);
			coll.Add(sendObject);
			parent.SendingObjectsCollection2 = coll;
			AssertEquals("", parent.BizObjValidationMessageErrors);

			parent.SendingObjectsCollection[0].ShouldSend = true;
			AssertNotEquals("", parent.BizObjValidationMessageErrors);
			AssertContains(declaration.JE_MessageTypeInfo.GetMessageErrors().First().Message, parent.BizObjValidationMessageErrors);

			parent.SendingObjectsCollection[0].ShouldSend = false;
			AssertEquals("Nothing is selected and no message errors are to be shown", "", parent.BizObjValidationMessageErrors);
		}

		public void TestAdditionalWarnings()
		{
			var parent = new BaseMessageSendingObjectParentForTest(Factory);
			parent.WarningToAdd = "Be warned.";
			AssertEquals("Be warned.", parent.AdditionalWarnings);
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = new BaseMessageSendingObjectParentForTest(Factory);
			AssertEquals(Enumerable.Empty<MessageSendingObjectProperty>(), parent.MessageSendingObjectProperties);
		}

		public void TestSecurityRightToSendMessageWithMessageError()
		{
			var parent = new BaseMessageSendingObjectParentForTest(Factory);
			parent.SecurityCheckpointToSendWithMessageErrorOverride = Env.Security.CustomsDISSendWithMessageErrors;
			var declaration = (BaseJobDeclaration)parent.TopLevelBusinessObject;
			declaration.JE_MessageType = "XXX";
			Assert("Precondition", declaration.HasMessageErrors);
			Env.Security.CustomsDISSendWithMessageErrors.IsAllowed = false;

			try
			{
				var notification = parent.MessageSendingValidation.CheckBusinessObjectLevelValidation();
				AssertContains($"{MessageSendingValidation.MessageErrorsExistWithNoSecurityRight} {string.Format(MessageSendingValidation.InformationForGetSecurityRight, Env.Security.CustomsDISSendWithMessageErrors.DisplayTextPathToSecurityRight)}", notification.ErrorNotificationsAsString());
			}
			finally
			{
				Env.Security.CustomsDISSendWithMessageErrors.IsAllowed = true;
			}
		}

		public void TestHasAnyObjectToSend()
		{
			var parent = new BaseMessageSendingObjectParentForTest(Factory);
			var sendObject = new BaseMessageSendingObjectForTest(Factory);
			var coll = new BaseMessageSendingObjectCollectionForTest(Factory);
			coll.Add(sendObject);
			parent.SendingObjectsCollection2 = coll;
			Assert(!parent.HasAnyObjectToSend);

			parent.SendingObjectsCollection[0].ShouldSend = true;
			Assert(parent.HasAnyObjectToSend);

			parent.SendingObjectsCollection[0].ShouldSend = false;
			Assert(!parent.HasAnyObjectToSend);
		}
	}

	public sealed class BaseMessageSendingObjectParentForTest : BaseMessageSendingObjectParent<BaseMessageSendingObjectForTest>
	{
		public BaseMessageSendingObjectParentForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NonPersistentBusinessObjectCollection<BaseMessageSendingObjectForTest> SendingObjectsCollection2;

		public SecurityCheckpoint SecurityCheckpointToSendWithMessageErrorOverride;

		public string WarningToAdd;

		public override BusinessObject TopLevelBusinessObject => declaration ?? (declaration = Factory.New<BaseJobDeclaration>());
		BaseJobDeclaration declaration;

		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => SecurityCheckpointToSendWithMessageErrorOverride ?? Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<BaseMessageSendingObjectForTest> GetSendingObjectsCollectionCore()
			=> SendingObjectsCollection2 ?? new BaseMessageSendingObjectCollectionForTest(Factory);

		protected override ZString GetAdditionalWarningsCore() => WarningToAdd ?? base.GetAdditionalWarningsCore();
	}
}
