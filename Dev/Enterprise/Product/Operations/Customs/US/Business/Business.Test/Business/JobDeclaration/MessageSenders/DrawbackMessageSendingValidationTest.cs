using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business.Testing
{
	class DrawbackMessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestUSDrawbackSendWithMessageErrorsCheckpoint()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Env.Security.USDrawbackSendWithMessageErrors.IsAllowed = false;
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var validation = new DrawbackMessageSendingValidation(drawback, null);
			var notifications = validation.CheckBusinessObjectLevelValidation();
			Assert(notifications.Any(x => x.Message.Contains(Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight)));
			Env.Security.USDrawbackSendWithMessageErrors.IsAllowed = true;
			notifications = validation.CheckBusinessObjectLevelValidation();
			Assert(!notifications.Any(x => x.Message.Contains(Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight)));
		}
	}
}
