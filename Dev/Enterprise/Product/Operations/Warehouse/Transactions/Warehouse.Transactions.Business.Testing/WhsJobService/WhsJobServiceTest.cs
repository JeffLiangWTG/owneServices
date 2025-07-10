using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsJobService))]
	public class WhsJobServiceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestJobServiceValidation()
		{
			AssertType<WhsJobServiceValidation>(((WhsJobService)BusinessObject).Validation);
		}

		public void TestShowDocumentsInDynamicMenu()
		{
			var adhocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var serviceForAdHocServiceJob = Factory.New<WhsJobService>();
			serviceForAdHocServiceJob.Parent = adhocServiceJob;
			AssertEquals("Document Menu should be hidden for Ad Hoc Service Jobs.", false, serviceForAdHocServiceJob.DocumentSupporter.ShowDocumentsInDynamicMenu);

			// Create a parent of Type Docket (Table Code: WD)
			var order = Factory.New<WhsOrder>();
			var serviceForOrder = Factory.New<WhsJobService>();
			serviceForOrder.Parent = order;
			AssertEquals("Document Menu should be shown for Dockets.", true, serviceForOrder.DocumentSupporter.ShowDocumentsInDynamicMenu);
		}
	}
}
