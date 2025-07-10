using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CusStatementController))]
	sealed class CusStatementControllerTest : Customs.Module.Testing.StatementControllerTest
	{
		public override void TestDeleteForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestEditForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestNewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestViewForm()
		{
			Assert("Not Implemented", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Not Implemented", true);
		}
	}
}
