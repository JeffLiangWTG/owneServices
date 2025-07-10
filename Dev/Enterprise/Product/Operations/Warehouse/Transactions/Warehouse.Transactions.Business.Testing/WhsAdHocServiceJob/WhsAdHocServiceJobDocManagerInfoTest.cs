using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdHocServiceJobDocManagerInfo))]
	class WhsAdHocServiceJobDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestGetRelatedObjects

		public void TestGetRelatedObjects()
		{
			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(Data.Whs1, Data.Org1, ZDateTime.Today);
			var whsAdHocServiceJobDocManagerInfo = new WhsAdHocServiceJobDocManagerInfo(adHocServiceJob);

			AssertNotNull(whsAdHocServiceJobDocManagerInfo);

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { adHocServiceJob.Client, adHocServiceJob.JobHeader }, whsAdHocServiceJobDocManagerInfo.RelatedObjects);

			adHocServiceJob.Delete();
		}

		protected override void CleanUp(BusinessObject[] relatedObjects) => relatedObjects.ForEach(x => x.Delete());

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory)); }
		}
		TestDataSimpleEnvironment data;

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsAdHocServiceJob>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return ServiceJob;
		}

		WhsAdHocServiceJob ServiceJob
		{
			get { return serviceJob ?? (serviceJob = Helper.CreateWhsAdHocServiceJob(Data.Whs1, Data.Org1, ZDateTime.Today)); }
		}
		WhsAdHocServiceJob serviceJob;

		#endregion
	}
}
