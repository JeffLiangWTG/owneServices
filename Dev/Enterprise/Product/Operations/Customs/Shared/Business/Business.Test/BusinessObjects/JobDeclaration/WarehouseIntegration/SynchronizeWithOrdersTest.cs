using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SynchronizeWithOrdersTest : TestCaseWithFactory
	{
		public void TestSynchronizeMessage()
		{
			TestJobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var messageText = SynchronizeHelper.Synchronize();
			AssertEquals("Cannot synchronize as there is no Bonded Warehouse Client entered on this job.", messageText);

			var orgheader = Factory.NewWithValidTestData<OrgHeader>();
			TestJobDeclaration.JE_OH_Importer = orgheader.PK;
			messageText = SynchronizeHelper.Synchronize();
			AssertEquals("Cannot synchronize as there is no Bonded Warehouse Client entered on this job.", messageText);

			orgheader.OH_IsWarehouseClient = true;
			messageText = SynchronizeHelper.Synchronize();
			AssertEquals("Cannot synchronize as there is no Bonded Warehouse entered on this job.", messageText);

			TestJobDeclaration.WarehouseDocAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			messageText = SynchronizeHelper.Synchronize();
			AssertEquals("No orders found to synchronize.", messageText);
		}

		SynchronizeWithOrders SynchronizeHelper
		{
			get { return new SynchronizeTestHelper(TestJobDeclaration); }
		}

		BaseJobDeclaration TestJobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				}
				return jobDeclaration;
			}
		}
		BaseJobDeclaration jobDeclaration;
	}
}
