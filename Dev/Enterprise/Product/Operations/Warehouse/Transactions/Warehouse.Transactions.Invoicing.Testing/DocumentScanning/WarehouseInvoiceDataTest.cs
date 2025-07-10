using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WarehouseInvoiceDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(WhsInvoice), AssemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertNotNull(AssemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsInvoiceCollection), AssemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.WhsInvoicing, AssemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, AssemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, AssemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region TestGetQuery

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetQuery()
		{
			var otherGlbCompany = Factory.NewWithValidTestData<GlbCompany>();

			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			var whs = Helper.CreateWarehouse("WHS");
			var oldDate = ZDateTime.Today.AddDays(-5);
			var emptyDate = ZDateTime.Empty;
			var fromDate = ZDateTime.Today.AddDays(-20);

			var invoice1 = CreateWhsInvoice(client1, whs, emptyDate, fromDate);

			var invoice2 = CreateWhsInvoice(client1, whs, oldDate.AddDays(1), fromDate.AddDays(-10)); // Closed Periodic Invoices
			var invoice3 = CreateWhsInvoice(client1, whs, oldDate.AddDays(2), fromDate.AddDays(-20));
			var invoice4 = CreateWhsInvoice(client1, whs, oldDate.AddDays(3), fromDate.AddDays(-30));
			var invoice5 = CreateWhsInvoice(client1, whs, oldDate.AddDays(4), fromDate.AddDays(-40));

			var invoice6 = CreateWhsInvoice(client1, whs, emptyDate, fromDate.AddDays(-50)); // Invoice with 2 JobHeaders for different GlbCompanies, one closed one not. Should only be included only when search non closed jobs.
			var secondJobHeader = Helper.CreateAccountingDataWithNoCharge(invoice6);
			secondJobHeader.JH_GC = otherGlbCompany.PK;
			secondJobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			secondJobHeader.JH_A_JCL = oldDate.AddDays(5);

			var invoice7 = CreateWhsInvoice(client2, whs, emptyDate, fromDate); // Invoice with other client

			Factory.Save();

			// Is Consignee - should have no impact on Invoice Jobs
			var query1 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, client1.PK));
			AssertCorrectInvoiceListLoaded(query1, new WhsInvoice[] { invoice1, invoice2, invoice3, invoice4, invoice5, invoice6 });

			var query2 = AssemblyData.GetQuery(new AssemblyDataParams(false, true, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, client1.PK));
			AssertCorrectInvoiceListLoaded(query2, new WhsInvoice[] { invoice1, invoice2, invoice3, invoice4, invoice5, invoice6 });

			// Is Client/Consignor - should decide all Invoice Jobs for selected Organisation
			var query3 = AssemblyData.GetQuery(new AssemblyDataParams(true, false, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, client1.PK));
			AssertCorrectInvoiceListLoaded(query3, Array.Empty<WhsInvoice>());

			var query4 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, client1.PK));
			AssertCorrectInvoiceListLoaded(query4, new WhsInvoice[] { invoice1, invoice2, invoice3, invoice4, invoice5, invoice6 });

			// Finalised Dates
			var query5 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate, emptyDate, oldDate.AddDays(2), emptyDate, client1.PK)); // Job Closed From
			AssertCorrectInvoiceListLoaded(query5, new WhsInvoice[] { invoice3, invoice4, invoice5 });

			var query6 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, oldDate.AddDays(3), client1.PK)); // Job Closed To
			AssertCorrectInvoiceListLoaded(query6, new WhsInvoice[] { invoice2, invoice3, invoice4 });

			var query7 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate, emptyDate, oldDate.AddDays(2), oldDate.AddDays(3), client1.PK)); // Job Closed Range
			AssertCorrectInvoiceListLoaded(query7, new WhsInvoice[] { invoice3, invoice4 });

			// Different Client
			var query8 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, emptyDate, client2.PK));
			AssertCorrectInvoiceListLoaded(query8, new WhsInvoice[] { invoice7 });
		}

		WhsInvoice CreateWhsInvoice(OrgHeader client, WhsWarehouse whs, ZDateTime closedDate, ZDateTime fromDate)
		{
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = whs.PK;
			invoice.ET_StorageFromDate = fromDate;
			invoice.ET_StorageToDate = fromDate.AddDays(7);
			invoice.ET_BillingDate = ZDateTime.Today.AddDays(-6);

			Helper.CreateAccountingDataWithNoCharge(invoice);

			if (!closedDate.IsEmpty)
			{
				invoice.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
				invoice.JobHeader.JH_A_JCL = closedDate;
			}
			return invoice;
		}

		void AssertCorrectInvoiceListLoaded(ZQuery query, WhsInvoice[] expectedInvoiceList)
		{
			var invoiceList = new WhsInvoiceCollection(Factory);
			invoiceList.Load(query);

			AssertEquals("Incorrect List of Invoices was loaded", expectedInvoiceList.Length, invoiceList.Count);
			foreach (var invoice in expectedInvoiceList)
			{
				AssertEquals("Expected Invoice has been not loaded: " + invoice.ET_StorageJobNumber, true, invoiceList.Contains(invoice.PK));
			}
		}

		#endregion

		#region Implementation

		WarehouseInvoiceAssemblyData AssemblyData => assemblyData ?? (assemblyData = new WarehouseInvoiceAssemblyData());
		WarehouseInvoiceAssemblyData assemblyData;

		#endregion
	}
}
