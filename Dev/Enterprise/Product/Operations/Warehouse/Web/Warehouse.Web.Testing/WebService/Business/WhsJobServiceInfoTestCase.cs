using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsJobServiceInfo))]
	public class WhsJobServiceInfoTestCase : DataObjectInfoTestCase<WhsJobServiceInfo>
	{
		#region Test Cases

		public void TestConstructor()
		{
			var service = Factory.New<WhsJobService>();
			service.ES_ServiceCode = "FUM";
			service.ES_ServiceNote = "ABC";
			service.ES_ServiceCount = 1;

			var jobServiceInfo = new WhsJobServiceInfo(service);
			AssertEquals("FUM", jobServiceInfo.Code);
			AssertEquals("ABC", jobServiceInfo.ServiceNote);
			AssertEquals(1m, jobServiceInfo.Count);
		}

		public void TestCode()
		{
			var jobServiceInfo = new WhsJobServiceInfo();
			AssertEquals("Precondition.", "", jobServiceInfo.Code);

			jobServiceInfo.Code = "FUM";
			AssertEquals("FUM", jobServiceInfo.Code);
		}

		public void TestCount()
		{
			var jobServiceInfo = new WhsJobServiceInfo();
			AssertEquals("Precondition.", 0m, jobServiceInfo.Count);

			jobServiceInfo.Count = 5;
			AssertEquals(5m, jobServiceInfo.Count);
		}

		public void TestServiceNote()
		{
			var jobServiceInfo = new WhsJobServiceInfo();
			AssertEquals("Precondition.", "", jobServiceInfo.ServiceNote);
		}

		#endregion

		#region Implementation

		protected new WhsJobServiceInfo Parent
		{
			get { return (WhsJobServiceInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsJobServiceInfo();
		}

		#endregion
	}
}
