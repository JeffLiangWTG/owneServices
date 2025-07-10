using CargoWise.Types;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class TestCFSDocsAndCartageValidation : JobDocsAndCartageValidationTest
	{
		public void TestJP_LCLStorageCommences()
		{
			var now = ZDateTime.Now;

			docsAndCartage.JP_LCLAvailable = now;
			docsAndCartage.JP_LCLStorageCommences = ZDateTime.Empty;
			docsAndCartage.RunPreSaveValidation();
			AssertEquals("JP_StorageCommences should have no error", false, docsAndCartage.JP_LCLStorageCommencesInfo.HasNotifications());

			docsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			docsAndCartage.RunPreSaveValidation();
			AssertEquals("JP_StorageCommences should have no error", false, docsAndCartage.JP_LCLStorageCommencesInfo.HasNotifications());

			docsAndCartage.JP_LCLAvailable = now;
			docsAndCartage.JP_LCLStorageCommences = now.AddDays(-1);
			docsAndCartage.RunPreSaveValidation();
			AssertEquals("JP_StorageCommences must greater then availability date when availability set", true, docsAndCartage.JP_LCLStorageCommencesInfo.HasNotifications());

			docsAndCartage.JP_LCLAvailable = now;
			docsAndCartage.JP_LCLStorageCommences = now.AddDays(2);
			docsAndCartage.RunPreSaveValidation();
			AssertEquals("JP_StorageCommences should have no error", false, docsAndCartage.JP_LCLStorageCommencesInfo.HasNotifications());
		}

		#region Implementation

		CFSShipment shipment;
		CFSDocsAndCartage docsAndCartage;

		protected override void SetUp()
		{
			shipment = Factory.New<CFSShipment>();
			docsAndCartage = shipment.DocsAndCartage;
		}

		#endregion
	}
}
