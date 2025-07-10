using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	sealed class XmlValueObjectSerializerIFSTestCase : XmlValueObjectSerializerTestCase
	{
		public void TestImport()
		{
			var data = new TestDataForIFS(Factory);
			data.CreateIfsOrders();
			Factory.Save();

			var dataAdapter = new WhsOrderCartageValueObjectDataAdapterIFS();
			var serializer = new XmlValueObjectSerializerIFS();
			var orders = new WhsOrderCollection(Factory);

			using (var stream = new MemoryStream(IFSFileForImport))
			{
				serializer.ImportXmlData(stream, dataAdapter, orders, null, new NotificationBuffer());
			}

			AssertEquals("TEST11212", data.Order1.WD_TransportReference);
			AssertEquals(data.TransportCo.PK, data.Order1.TransportCoPK);
			AssertEquals(data.Service.PL_Code, data.Order1.WD_PL_NKCarrierServiceLevel);

			AssertEquals("TEST11212", data.Order2.WD_TransportReference);
			AssertEquals(data.TransportCo.PK, data.Order2.TransportCoPK);
			AssertEquals(data.Service.PL_Code, data.Order2.WD_PL_NKCarrierServiceLevel);

			AssertEquals("TEST11212", data.Order3.WD_TransportReference);
			AssertEquals(data.TransportCo.PK, data.Order3.TransportCoPK);
			AssertEquals(data.Service.PL_Code, data.Order3.WD_PL_NKCarrierServiceLevel);

			AssertEquals("TEST11212", data.Order4.WD_TransportReference);
			AssertEquals(data.TransportCo.PK, data.Order4.TransportCoPK);
			AssertEquals(data.Service.PL_Code, data.Order4.WD_PL_NKCarrierServiceLevel);

			AssertEquals("TEST11212", data.Order5.WD_TransportReference);
			AssertEquals(data.TransportCo.PK, data.Order5.TransportCoPK);
			AssertEquals(data.Service.PL_Code, data.Order5.WD_PL_NKCarrierServiceLevel);

			AssertEquals(5, orders.Count);
			ReleaseMutexesOnJobs();
		}

		public void TestImportOfCharges()
		{
			var data = new TestDataForIFS(Factory);
			data.CreateIfsOrders();
			Factory.Save();

			var dataAdapter = new WhsOrderCartageValueObjectDataAdapterIFS();
			var serializer = new XmlValueObjectSerializerIFS();
			var orders = new WhsOrderCollection(Factory);

			using (var stream = new MemoryStream(IFSFileForImport))
			{
				serializer.ImportXmlData(stream, dataAdapter, orders, null, new NotificationBuffer());
			}

			// Assert that the transport Co's charges are added to the Orders
			var job1 = (Job)data.Order1.JobHeader;
			AssertEquals(0.83m, job1.Charges[0].JR_OSCostAmt);
			AssertEquals(3.00m, job1.Charges[0].JR_OSSellAmt);

			var job2 = (Job)data.Order2.JobHeader;
			AssertEquals(0.83m, job2.Charges[0].JR_OSCostAmt);
			AssertEquals(3.00m, job1.Charges[0].JR_OSSellAmt);

			var job3 = (Job)data.Order3.JobHeader;
			AssertEquals(0.83m, job3.Charges[0].JR_OSCostAmt);
			AssertEquals(3.00m, job3.Charges[0].JR_OSSellAmt);

			var job4 = (Job)data.Order4.JobHeader;
			AssertEquals(0.83m, job4.Charges[0].JR_OSCostAmt);
			AssertEquals(3.00m, job4.Charges[0].JR_OSSellAmt);

			var job5 = (Job)data.Order5.JobHeader;
			AssertEquals("$4.17 across 5 Orders is 0.83c per Order with 0.2c remainder -- the extra 0.2c should have been apportioned to the last Order.", 0.85m, job5.Charges[0].JR_OSCostAmt);
			AssertEquals(3.00m, job5.Charges[0].JR_OSSellAmt);

			AssertEquals(5, orders.Count);
			ReleaseMutexesOnJobs();
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		void ReleaseMutexesOnJobs()
		{
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			var jobs = Factory.Load<Job>(query);
			foreach (var job in jobs)
			{
				job.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		byte[] IFSFileForImport => resourceRetriever.Value.GetBytes("Enterprise.Warehouse.Transactions.DataTransfer.Testing.TestFiles.IFSFileForImport.xml");
	}
}
