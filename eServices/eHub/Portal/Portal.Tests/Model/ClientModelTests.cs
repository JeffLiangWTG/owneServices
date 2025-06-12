using CargoWise.eHub.Portal.Models.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Model
{
	[TestClass]
	public class ClientModelTests
	{
		#region Setup 

		IeHubTransactionsContext context;

		[TestInitialize()]
		public void Startup()
		{
			context = new Fakes.TestContext();
		}

		[TestCleanup()]
		public void Cleanup()
		{
			context = null;
		}

		#endregion

		[TestMethod]
		public void ModelValidationClientFieldsRequiredTest()
		{
			var set = new eHubClient() { };
			context.eHubClients.AddObject(set);
			var controller = ModelValidationHelper.InitilizeStubController(context);
			controller.ValidateModel<eHubClient>(set);
			Assert.IsFalse(controller.ModelState.IsValid);

			Assert.AreEqual(1, controller.ModelState["CC_ID"].Errors.Count);
			Assert.AreEqual("ID is required", controller.ModelState["CC_ID"].Errors[0].ErrorMessage);

			Assert.AreEqual(1, controller.ModelState["CC_FriendlyName"].Errors.Count);
			Assert.AreEqual("Name is required", controller.ModelState["CC_FriendlyName"].Errors[0].ErrorMessage);

			Assert.AreEqual(true, set.CC_PermitInboxSender);
			Assert.AreEqual(true, set.CC_PermitInboxRecipient);
		}

		[TestMethod]
		public void ModelValidationClientFieldsLengthTest()
		{
			const int CC_ID_LIMIT = 36;
			const int CC_FRIENDLY_NAME_LIMIT = 128;
			const int CC_AS2_CODE_LIMIT = 50;

			var set = new eHubClient() { };

			set.CC_ID = StringGenerator.GenerateString(CC_ID_LIMIT + 1);
			set.CC_FriendlyName = StringGenerator.GenerateString(CC_FRIENDLY_NAME_LIMIT + 1);
			set.CC_AS2_Code = StringGenerator.GenerateString(CC_AS2_CODE_LIMIT + 1);
			set.CC_EmailAddress = "test";
			set.CC_Password = "test";

			context.eHubClients.AddObject(set);
			var controller = ModelValidationHelper.InitilizeStubController(context);
			controller.ValidateModel<eHubClient>(set);
			Assert.IsFalse(controller.ModelState.IsValid);

			Assert.AreEqual(1, controller.ModelState["CC_ID"].Errors.Count);
			Assert.AreEqual("ID may not be longer than 36 characters", controller.ModelState["CC_ID"].Errors[0].ErrorMessage);

			Assert.AreEqual(1, controller.ModelState["CC_FriendlyName"].Errors.Count);
			Assert.AreEqual("Name may not be longer than 128 characters", controller.ModelState["CC_FriendlyName"].Errors[0].ErrorMessage);

			Assert.AreEqual(1, controller.ModelState["CC_AS2_Code"].Errors.Count);
			Assert.AreEqual("AS2 Code may not be longer than 50 characters", controller.ModelState["CC_AS2_Code"].Errors[0].ErrorMessage);

			set.CC_ID = StringGenerator.GenerateString(CC_ID_LIMIT);
			set.CC_FriendlyName = StringGenerator.GenerateString(CC_FRIENDLY_NAME_LIMIT);
			set.CC_AS2_Code = StringGenerator.GenerateString(CC_AS2_CODE_LIMIT);
			context.eHubClients.AddObject(set);
			controller = ModelValidationHelper.InitilizeStubController(context);
			controller.ValidateModel<eHubClient>(set);
			Assert.IsTrue(controller.ModelState.IsValid);
		}
	}
}
