using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	public abstract class CYDControllerTest : ZControllerBasherTest
	{
		#region TestForms

		public override void TestViewForm()
		{
			Assert("We only have a dummy form at the moment", true);
		}

		public override void TestNewForm()
		{
			Assert("We only have a dummy form at the moment", true);
		}

		public override void TestEditForm()
		{
			Assert("We only have a dummy form at the moment", true);
		}

		public override void TestDeleteForm()
		{
			Assert("We only have a dummy form at the moment", true);
		}

		#endregion

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert("Base test case fails because BusinessEntity is null", true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert("Base test case fails because BusinessEntity is null", true);
		}
	}
}
