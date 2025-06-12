using CargoWise.eHub.Portal.Models.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Model
{
	[TestClass]
	public class TransformationSetModelTests
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
		public void ModelValidationTransformationSetFieldsTest()
		{
			var set = new eHubTransformationSet() { };
			set.TS_XPathPredicate = StringGenerator.GenerateString(1025);
			context.eHubTransformationSets.AddObject(set);
			var controller = ModelValidationHelper.InitilizeStubController(context);
			controller.ValidateModel<eHubTransformationSet>(set);
			Assert.IsFalse(controller.ModelState.IsValid);

			Assert.AreEqual(1, controller.ModelState["TS_Name"].Errors.Count);
			Assert.AreEqual("Name is required", controller.ModelState["TS_Name"].Errors[0].ErrorMessage);

			Assert.AreEqual(1, controller.ModelState["TS_XPathPredicate"].Errors.Count);
			Assert.AreEqual("XPathPredicate may not be longer than 1024 characters", controller.ModelState["TS_XPathPredicate"].Errors[0].ErrorMessage);
		}
	}
}
