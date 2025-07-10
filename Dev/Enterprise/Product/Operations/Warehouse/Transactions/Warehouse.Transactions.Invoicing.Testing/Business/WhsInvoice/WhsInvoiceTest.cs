using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WhsInvoiceTest : WhsTestCaseWithFactory
	{
		#region StoragePeriod
		public void TestGetStorageDates()
		{
			var year = ZDateTime.Now.Year;
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Factory.New<OrgHeader>().PK;

			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 15);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 20);
			AssertStorageDates(invoice, new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 20));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 15),
				new ZDateTime(year, 1, 16), new ZDateTime(year, 1, 16),
				new ZDateTime(year, 1, 17), new ZDateTime(year, 1, 17),
				new ZDateTime(year, 1, 18), new ZDateTime(year, 1, 18),
				new ZDateTime(year, 1, 19), new ZDateTime(year, 1, 19),
				new ZDateTime(year, 1, 20), new ZDateTime(year, 1, 20));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;
			AssertStorageDates(invoice, new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 20));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			AssertStorageDates(invoice, new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 20));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			AssertStorageDates(invoice, new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 20));

			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 28);
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 7),
				new ZDateTime(year, 1, 8), new ZDateTime(year, 1, 14),
				new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 21),
				new ZDateTime(year, 1, 22), new ZDateTime(year, 1, 28));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 14),
				new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 28));

			invoice.ET_StorageToDate = new ZDateTime(year, 1, 29);
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 7),
				new ZDateTime(year, 1, 8), new ZDateTime(year, 1, 14),
				new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 21),
				new ZDateTime(year, 1, 22), new ZDateTime(year, 1, 28),
				new ZDateTime(year, 1, 29), new ZDateTime(year, 1, 29));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 14),
				new ZDateTime(year, 1, 15), new ZDateTime(year, 1, 28),
				new ZDateTime(year, 1, 29), new ZDateTime(year, 1, 29));
			invoice.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			AssertStorageDates(invoice, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 29));

			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);
			AssertStorageDates(invoice, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 1);
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31),
				new ZDateTime(year, 2, 1), new ZDateTime(year, 2, 1));

			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 15);
			invoice.ET_StorageToDate = new ZDateTime(year, 4, 14);
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 15), new ZDateTime(year, 2, 14),
				new ZDateTime(year, 2, 15), new ZDateTime(year, 3, 14),
				new ZDateTime(year, 3, 15), new ZDateTime(year, 4, 14));

			invoice.ET_StorageToDate = new ZDateTime(year, 4, 16);
			AssertStorageDates(invoice,
				new ZDateTime(year, 1, 15), new ZDateTime(year, 2, 14),
				new ZDateTime(year, 2, 15), new ZDateTime(year, 3, 14),
				new ZDateTime(year, 3, 15), new ZDateTime(year, 4, 14),
				new ZDateTime(year, 4, 15), new ZDateTime(year, 4, 16));

			invoice.ET_StorageToDate = ZDateTime.Invalid;
			AssertStorageDates(invoice, ZDateTime.Empty, ZDateTime.Empty);
			invoice.ET_StorageToDate = ZDateTime.Empty;
			AssertStorageDates(invoice, ZDateTime.Empty, ZDateTime.Empty);

			invoice.ET_StorageToDate = new ZDateTime(year, 4, 14);
			invoice.ET_StorageFromDate = ZDateTime.Invalid;
			AssertStorageDates(invoice, ZDateTime.Empty, ZDateTime.Empty);
			invoice.ET_StorageFromDate = ZDateTime.Empty;
			AssertStorageDates(invoice, ZDateTime.Empty, ZDateTime.Empty);
		}

		void AssertStorageDates(WhsInvoice invoice, params ZDateTime[] periodDates)
		{
			WhsInvoice.StorageDates[] storageDates = invoice.GetStorageDates();
			AssertEquals("StorageDates.Length", periodDates.Length / 2, storageDates.Length);
			for (int i = 0; i < storageDates.Length; i++)
			{
				AssertEquals(String.Format("StorageDates[{0}].From", i), periodDates[2 * i], storageDates[i].From);
				AssertEquals(String.Format("StorageDates[{0}].To", i), periodDates[2 * i + 1], storageDates[i].To);
			}
		}

		#endregion

		#region TestAdditionalJobsToShowChargesFor_IsCachedDuringAutoRating

		public void TestAdditionalJobsToShowChargesFor_IsCachedDuringAutoRating()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(invoice))
			{
				AssertEquals(receive1.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Single().PK);

				invoice.ET_OH_Client = client2.PK;
				invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
				invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);
				AssertEquals("Should still contain Original Receive since Additional Jobs is cached during AutoRating.",
					receive1.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Single().PK);
			}

			AssertEquals("Additional Jobs should now reflect reality.",
				receive2.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Single().PK);
		}

		#endregion

		#region TestAdditionalJobsToShowChargesFor_BusinessObjectsInMemory

		public void TestAdditionalJobsToShowChargesFor_Performance_BusinessObjectsInMemory()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var invoiceFromOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);

			AssertEquals(2, ((IJobInvoicingPlugInAdditionalJobs)invoiceFromOtherFactory).AdditionalJobsToShowChargesFor.Length);
			var allLoadedBusinessObjects = ((IBusinessObjectFactoryInternals)otherFactory).AllBusinessObjects;
			AssertEquals("Should not load any docket bizos into memory", 0, allLoadedBusinessObjects.OfType<WhsDocket>().Count());
		}

		#endregion

		#region TestAdditionalDockets_IsCachedDuringAutoRating

		public void TestAdditionalDockets_IsCachedDuringAutoRating()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var invoiceRatingsProvider = new WhsInvoiceRatingAdaptersProvider(invoice);

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(invoice))
			{
				AssertContainsExactElementsInAnyOrder(new[] { receive1 }, invoiceRatingsProvider.GetAdditionalJobs());

				invoice.ET_OH_Client = client2.PK;
				invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
				invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);
				AssertContainsExactElementsInAnyOrder("Should still contain Original Receive since Additional Jobs is cached during AutoRating.",
					new[] { receive1 }, invoiceRatingsProvider.GetAdditionalJobs());
			}

			AssertContainsExactElementsInAnyOrder("Additional Jobs should now reflect reality.",
				new[] { receive2 }, invoiceRatingsProvider.GetAdditionalJobs());
		}

		#endregion

		#region TestAdditionalDockets_BorderFinalisedDates

		public void TestAdditionalDockets_BorderFinalisedDates_NegativeOffsets()
		{
			var branch = Helper.CreateGlbBranch("BR1");
			var pasadenaHomePort = "USPSD"; // In April 2024, Pasadena Texas has a -07:00 Offset
			branch.GB_RL_NKHomePort = pasadenaHomePort;

			var whs = Helper.CreateWarehouse("WH1", "A", 2, 2);
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var org = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec1", new ZDateTimeOffset(2024, 03, 31, 00, 00, 00, TimeSpan.FromHours(-7)), part, 1m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec2", new ZDateTimeOffset(2024, 04, 01, 00, 00, 00, TimeSpan.FromHours(-7)), part, 3m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec3", new ZDateTimeOffset(2024, 04, 15, 00, 00, 00, TimeSpan.FromHours(-7)), part, 5m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec4", new ZDateTimeOffset(2024, 04, 30, 00, 00, 00, TimeSpan.FromHours(-7)), part, 7m);
			var receive5 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec5", new ZDateTimeOffset(2024, 05, 01, 00, 00, 00, TimeSpan.FromHours(-7)), part, 9m);
			Factory.Save();

			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(receive3);
			AssertIsFinalisedPrecondition(receive4);
			AssertIsFinalisedPrecondition(receive5);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 03, 31, 00, 00, 00, TimeSpan.FromHours(-7)), receive1.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 04, 01, 00, 00, 00, TimeSpan.FromHours(-7)), receive2.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 04, 15, 00, 00, 00, TimeSpan.FromHours(-7)), receive3.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 04, 30, 00, 00, 00, TimeSpan.FromHours(-7)), receive4.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 05, 01, 00, 00, 00, TimeSpan.FromHours(-7)), receive5.WD_FinalisedDate);

			Helper.CreateChargeCode("TST", "Test Receives", ChargeCodeGroupList.Codes.WHSInwards, null);

			var invoice = Helper.CreateInvoiceWithJobHeader(org, whs, new ZDateTime(2024, 04, 01), new ZDateTime(2024, 04, 30));
			Factory.Save();

			invoice.AutoRateJobHeader(null);
			AssertContainsExactElementsInAnyOrder(new[] { receive2, receive3, receive4 }, invoice.GetAdditionalDockets());
		}

		public void TestAdditionalDockets_BorderFinalisedDates_PositiveOffsets()
		{
			var branch = Helper.CreateGlbBranch("BR1");
			var sydneyHomePort = "AUSYD"; // In April 2024, Sydney Australia has a +10:00 Offset
			branch.GB_RL_NKHomePort = sydneyHomePort;

			var whs = Helper.CreateWarehouse("WH1", "A", 2, 2);
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var org = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(org, "P1");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec1", new ZDateTimeOffset(2024, 03, 31, 00, 00, 00, TimeSpan.FromHours(+10)), part, 1m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec2", new ZDateTimeOffset(2024, 04, 01, 00, 00, 00, TimeSpan.FromHours(+10)), part, 3m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec3", new ZDateTimeOffset(2024, 04, 15, 00, 00, 00, TimeSpan.FromHours(+10)), part, 5m);
			var receive4 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec4", new ZDateTimeOffset(2024, 04, 30, 00, 00, 00, TimeSpan.FromHours(+10)), part, 7m);
			var receive5 = Helper.CreateWhsReceiveWithInventory(org, whs, "Rec5", new ZDateTimeOffset(2024, 05, 01, 00, 00, 00, TimeSpan.FromHours(+10)), part, 9m);
			Factory.Save();

			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(receive3);
			AssertIsFinalisedPrecondition(receive4);
			AssertIsFinalisedPrecondition(receive5);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 03, 31, 00, 00, 00, TimeSpan.FromHours(+10)), receive1.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 04, 01, 00, 00, 00, TimeSpan.FromHours(+10)), receive2.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 04, 15, 00, 00, 00, TimeSpan.FromHours(+10)), receive3.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 04, 30, 00, 00, 00, TimeSpan.FromHours(+10)), receive4.WD_FinalisedDate);
			AssertEquals("Precondition", new ZDateTimeOffset(2024, 05, 01, 00, 00, 00, TimeSpan.FromHours(+10)), receive5.WD_FinalisedDate);

			Helper.CreateChargeCode("TST", "Test Receives", ChargeCodeGroupList.Codes.WHSInwards, null);

			var invoice = Helper.CreateInvoiceWithJobHeader(org, whs, new ZDateTime(2024, 04, 01), new ZDateTime(2024, 04, 30));
			Factory.Save();

			invoice.AutoRateJobHeader(null);
			AssertContainsExactElementsInAnyOrder(new[] { receive2, receive3, receive4 }, invoice.GetAdditionalDockets());
		}

		#endregion

		#region TestCustomReadOnly_IsCachedDuringAutoRating

		public void TestCustomReadOnly_IsCachedDuringAutoRating()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var invoiceRatingsProvider = new WhsInvoiceRatingAdaptersProvider(invoice);

			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(invoice))
			{
				AssertEquals("No posted charges, should not be Read Only.", false, invoice.CustomReadOnly);

				invoice.IncludeInInvoicing = false;
				AssertEquals("Even though IncludeInInvoicing is false, should not be Read Only as value is Cached.", false, invoice.CustomReadOnly);
			}

			AssertEquals("IncludeInInvoicing is false, should be Read Only.", true, invoice.CustomReadOnly);
		}

		#endregion

		#region TestET_OH_Client_OnlyInvalidatesCollectionsOnce

		public void TestET_OH_Client_OnlyInvalidatesCollectionsOnce()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var client2 = Helper.CreateClient("CLIENT2");
			client2.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			// create dummy invoice so defaulting will set the dates ahead of this invoice
			Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 3, 1), new ZDateTime(2019, 3, 31));
			Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, new ZDateTime(year, 3, 1), new ZDateTime(2019, 3, 31));
			Factory.Save();

			var invoice = Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31), isPosted: false);

			// pretend we're autorating to cache AdditionalDockets to prevent reset from firing there.
			var interactor = new TestInteractor();
			using (_Rating.Start(interactor, isEqualization: false))
			using (interactor.StartRatingSession())
			using (_Rating.StartSubSession(invoice))
			{
				_ = ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor;

				int resetCount = 0;
				((IBindingList)invoice.JobHeader.Charges).ListChanged += (sender, e) => _ = e.ListChangedType == ListChangedType.Reset ? resetCount++ : 0;

				invoice.JobHeader.JH_OA_LocalChargesAddr = client2.MainAddress.PK; // simulate the user changing the Job Header's Local Client
				AssertEquals("Client should be changed.", client2.PK, invoice.ET_OH_Client);
				AssertEquals("Client should be changed on Job Header.", client2.MainAddress.PK, invoice.JobHeader.JH_OA_LocalChargesAddr);
				AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);
				AssertEquals("Storage Dates should have been changed.", new ZDateTime(year, 4, 1), invoice.ET_StorageFromDate);
				AssertEquals("Storage Dates should have been changed.", new ZDateTime(year, 4, 30), invoice.ET_StorageToDate);
				AssertEquals("Collections should be invalidated only once.", 1, resetCount);

				invoice.JobHeader.Dispose();
			}
		}

		#endregion

		#region TestET_OH_Client_ChangeLocalClientAddress_WithSameClient

		public void TestET_OH_Client_ChangeLocalClientAddress_WithSameClient()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			var address2 = data.Org1.Addresses.AddNew();
			address2.OA_Address1 = "1 RANDOM ST";
			address2.OA_Code = "1 RANDOM ST";
			address2.OA_City = "RANDOM";
			address2.OA_PostCode = "1111";
			address2.OA_RN_NKCountryCode = "AU";
			address2.OA_State = "NSW";
			var client2 = Helper.CreateClient("CLIENT2");
			client2.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			// create dummy invoice so defaulting will set the dates ahead of this invoice
			Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();

			var invoice = Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31), isPosted: true);
			invoice.RegisterEditableChildObject(invoice.JobHeader); // simulate what the accounting plugin does

			int resetCount = 0;
			((IBindingList)invoice.JobHeader.Charges).ListChanged += (sender, e) => _ = e.ListChangedType == ListChangedType.Reset ? resetCount++ : 0;

			invoice.JobHeader.JH_OA_LocalChargesAddr = address2.PK; // simulate the user changing the Job Header's Local Client
			AssertEquals("Client is the same.", data.Org1.PK, invoice.ET_OH_Client);
			AssertEquals("Address is allowed to change if the Org is the same.", address2.PK, invoice.JobHeader.JH_OA_LocalChargesAddr);
			AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should not be invalidated.", 0, resetCount);

			invoice.JobHeader.JH_OA_LocalChargesAddr = client2.MainAddress.PK; // simulate the user changing the Job Header's Local Client
			invoice.JobHeader.Validation.ValidateJH_OA_LocalChargesAddr(); // simulate Validation running after user has tabbed off
			AssertEquals("Invalid Client should be reverted.", data.Org1.PK, invoice.ET_OH_Client);
			AssertEquals("Invalid Client should be reverted on Job Header, but should retain correct address.", address2.PK, invoice.JobHeader.JH_OA_LocalChargesAddr);
			AssertHasError(invoice.JobHeader.JH_OA_LocalChargesAddrInfo, "You cannot Change the Client if there is at least one Posted Charge.");
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should not be invalidated.", 0, resetCount);
		}

		#endregion

		#region TestET_OH_Client_RevertsLocalClientAddressIfOnePostedChargeExists

		public void TestET_OH_Client_RevertsLocalClientAddressIfOnePostedChargeExists()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var client2 = Helper.CreateClient("CLIENT2");
			client2.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			// create dummy invoice so defaulting will set the dates ahead of this invoice
			Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();

			var invoice = Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31), isPosted: true);
			invoice.RegisterEditableChildObject(invoice.JobHeader); // simulate what the accounting plugin does

			int resetCount = 0;
			((IBindingList)invoice.JobHeader.Charges).ListChanged += (sender, e) => _ = e.ListChangedType == ListChangedType.Reset ? resetCount++ : 0;

			invoice.JobHeader.JH_OA_LocalChargesAddr = client2.MainAddress.PK; // simulate the user changing the Job Header's Local Client
			invoice.JobHeader.Validation.ValidateJH_OA_LocalChargesAddr(); // simulate Validation running after user has tabbed off
			AssertEquals("Invalid Client should be reverted.", data.Org1.PK, invoice.ET_OH_Client);
			AssertEquals("Invalid Client should be reverted on Job Header.", data.Org1.MainAddress.PK, invoice.JobHeader.JH_OA_LocalChargesAddr);
			AssertHasError(invoice.JobHeader.JH_OA_LocalChargesAddrInfo, "You cannot Change the Client if there is at least one Posted Charge.");
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should not be invalidated.", 0, resetCount);

			invoice.ET_OH_Client = data.Org1.PK;
			// Error is not reset in this case because the 'Invalid Client' that was set was cleared, so we don't re-validate.
			AssertHasError(invoice.JobHeader.JH_OA_LocalChargesAddrInfo, "You cannot Change the Client if there is at least one Posted Charge.");

			invoice.JobHeader.Validation.ValidateJH_OA_LocalChargesAddr(); // simulate Validation running after user has tabbed off
			// invalid Client should no longer be persisted
			AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);

			invoice.JobHeader.JH_OA_LocalChargesAddr = client2.MainAddress.PK; // simulate the user changing the Job Header's Local Client
			AssertEquals("Invalid Client should be reverted.", data.Org1.PK, invoice.ET_OH_Client);
			AssertHasError(invoice.JobHeader.JH_OA_LocalChargesAddrInfo, "You cannot Change the Client if there is at least one Posted Charge.");
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should not be invalidated.", 0, resetCount);

			// RunPreSaveValidation should clear Invalid Client set
			invoice.RunPreSaveValidation();
			AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);
		}

		public void TestET_OH_Client_RevertsLocalClientAddressIfOnePostedChargeExists_DirectToClientSetter()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var client2 = Helper.CreateClient("CLIENT2");
			client2.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			// create dummy invoice so defaulting will set the dates ahead of this invoice
			Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();

			var invoice = Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31), isPosted: true);
			invoice.RegisterEditableChildObject(invoice.JobHeader); // simulate what the accounting plugin does

			int resetCount = 0;
			((IBindingList)invoice.JobHeader.Charges).ListChanged += (sender, e) => _ = e.ListChangedType == ListChangedType.Reset ? resetCount++ : 0;

			invoice.JobHeader.JH_OA_LocalChargesAddr = client2.MainAddress.PK; // simulate the user changing the Job Header's Local Client
			AssertEquals("Invalid Client should be reverted.", data.Org1.PK, invoice.ET_OH_Client);
			AssertHasError(invoice.JobHeader.JH_OA_LocalChargesAddrInfo, "You cannot Change the Client if there is at least one Posted Charge.");
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should not be invalidated.", 0, resetCount);

			invoice.ET_OH_Client = data.Org1.PK;
			AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);

			invoice.ET_OH_Client = client2.PK; // directly set Client
			AssertEquals("Invalid Client should be reverted.", data.Org1.PK, invoice.ET_OH_Client);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should not have been changed.", new ZDateTime(year, 1, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should not be invalidated.", 0, resetCount);

			// Validation was not yet run, since it is suspended during the ET_OH_Client setter
			AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);

			// Validating Local Client should now add the Error
			invoice.JobHeader.Validation.ValidateJH_OA_LocalChargesAddr();
			AssertHasError(invoice.JobHeader.JH_OA_LocalChargesAddrInfo, "You cannot Change the Client if there is at least one Posted Charge.");

			// RunPreSaveValidation should clear Invalid Client set
			invoice.RunPreSaveValidation();
			AssertNoErrors(invoice.JobHeader.JH_OA_LocalChargesAddrInfo);
		}

		#endregion

		#region TestET_StorageFromDate_OnlyInvalidatesCollectionsOnce

		public void TestET_StorageFromDate_OnlyInvalidatesCollectionsOnce()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var client2 = Helper.CreateClient("CLIENT2");
			client2.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Factory.Save();

			var invoice = Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31), isPosted: false);

			int resetCount = 0;
			((IBindingList)invoice.JobHeader.Charges).ListChanged += (sender, e) => _ = e.ListChangedType == ListChangedType.Reset ? resetCount++ : 0;

			invoice.ET_StorageFromDate = new ZDateTime(year, 3, 1);
			AssertEquals("Storage Date From should be updated.", new ZDateTime(year, 3, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Date To should be defaulted to new period.", new ZDateTime(year, 3, 31), invoice.ET_StorageToDate);
			AssertEquals("Collections should be invalidated once only.", 1, resetCount);

			invoice.JobHeader.Dispose();
		}

		#endregion

		#region TestET_WW_OnlyInvalidatesCollectionsOnce

		public void TestET_WW_OnlyInvalidatesCollectionsOnce()
		{
			var year = ZDateTime.Today.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var client2 = Helper.CreateClient("CLIENT2");
			client2.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var warehouse2 = Helper.CreateWarehouse("WH2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 1), data.Part1, 10m);

			// create dummy invoice so defaulting will set the dates ahead of this invoice
			Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Helper.CreateWhsInvoice(warehouse2.PK, data.Org1.PK, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();

			var invoice = Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31), isPosted: false);

			int resetCount = 0;
			((IBindingList)invoice.JobHeader.Charges).ListChanged += (sender, e) => _ = e.ListChangedType == ListChangedType.Reset ? resetCount++ : 0;

			invoice.ET_WW = warehouse2.PK;
			AssertEquals("Warehouse should be updated.", warehouse2.PK, invoice.ET_WW);
			AssertEquals("Storage Dates should have been changed.", new ZDateTime(year, 4, 1), invoice.ET_StorageFromDate);
			AssertEquals("Storage Dates should have been changed.", new ZDateTime(year, 4, 30), invoice.ET_StorageToDate);
			AssertEquals("Collections should be invalidated once only.", 1, resetCount);

			invoice.JobHeader.Dispose();
		}

		#endregion

		#region TestET_OffBandProcessingStatusDefault

		public void TestET_OffBandProcessingStatusDefault()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			AssertEquals("Should set value by default.", "NIQ", invoice.ET_OffBandProcessingStatus);
		}

		#endregion

		#region TestIAutoRatingCreateJobHeaderPromptOverride

		public void TestIAutoRatingCreateJobHeaderPromptOverride()
		{
			AssertEquals(true, ((IAutoRatingSuppressJobCreationPrompt)Factory.New<WhsInvoice>()).SuppressJobCreationPrompt);
		}

		#endregion

		#region TestIValidationSuspenderForAutoRating

		public void TestIValidationSuspenderForAutoRating()
		{
			var invoice = Factory.New<WhsInvoice>();
			var order = Factory.New<WhsOrder>();
			IValidationSuspenderForAutoRating ratingValidationSuspender = invoice;
			AssertEquals(false, ratingValidationSuspender.ShouldRunPreSaveValidationBeforeAutorating(order));
			AssertEquals(false, ratingValidationSuspender.ShouldRunPreSaveValidationBeforeAutorating(null));
			AssertEquals(true, ratingValidationSuspender.ShouldRunPreSaveValidationBeforeAutorating(invoice));

			var job = new Job.Loader(invoice).TryLoadOrCreate();
			var invoiceLocalChargesAddressValidationHitCount = 0;
			var invoiceStatusValidationHitCount = 0;
			job.JH_OA_LocalChargesAddrInfo.AdditionalValidation += () => invoiceLocalChargesAddressValidationHitCount++;
			job.JH_StatusInfo.AdditionalValidation += () => invoiceStatusValidationHitCount++;

			ratingValidationSuspender.OnValidationResumed();
			AssertEquals("Invoice Job validation should run OnValidationResumed().", 2, invoiceLocalChargesAddressValidationHitCount);
			AssertEquals("Invoice Job validation should run OnValidationResumed().", 1, invoiceStatusValidationHitCount);
		}

		#endregion

		#region TestET_OffBandProcessingStatusInfo

		public void TestET_OffBandProcessingStatusInfo()
		{
			AssertEquals("Concurrency Policy should be strict for ET_OffBandProcessingStatus.", ConcurrencyPolicy.Strict, Factory.New<WhsInvoice>().ET_OffBandProcessingStatusInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestHasCharges

		public void TestHasCharges()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertEquals("Precondition", false, invoice.HasCharges);

			using (var jobHeader = new Job.Loader(invoice).TryCreateWithMutex())
			{
				Helper.CreateJobCharge(jobHeader);
				AssertEquals(true, invoice.HasCharges);
			}
		}

		#endregion

		#region TestTryCreateJobHeader

		public void TestTryCreateJobHeader()
		{
			var year = ZDateTime.Today.Year;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			Factory.Save();

			AssertNull("Precondition", Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, invoice.PK)).SingleOrDefault());
			invoice.TryCreateJobHeader();
			AssertNotNull("Should not be null", Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, invoice.PK)).SingleOrDefault());
		}

		#endregion

		#region TestIsInvoiceBillingCheckSuspended

		public void TestIsInvoiceBillingCheckSuspended()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			AssertEquals(false, invoice.IsInvoiceBillingCheckSuspended);

			using (invoice.InvoiceBillingCheckLockSuspender())
			{
				AssertEquals(true, invoice.IsInvoiceBillingCheckSuspended);
			}

			AssertEquals("Once the process ends/finished, it should set to false.", false, invoice.IsInvoiceBillingCheckSuspended);
		}

		#endregion

		#region TestBillingAutomationStatus

		public void TestBillingAutomationStatus()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));

			AssertEquals("", invoice.BillingAutomationStatus);
			foreach (var status in BillingAutomationStatusCodes.AllStatuses)
			{
				AssertNoExceptionThrown(() => invoice.BillingAutomationStatus = status);
				AssertEquals(status, invoice.BillingAutomationStatus);
			}

			AssertExceptionThrown<InvalidOperationException>("Should only select from valid list.", "'ABC' is not valid BillingAutomationStatus code.", () => invoice.BillingAutomationStatus = "ABC");
			AssertExceptionThrown<InvalidOperationException>("Space is not valid value.", "'' is not valid BillingAutomationStatus code.", () => invoice.BillingAutomationStatus = "");
		}

		#endregion

		#region TestUniversalDataContext

		public void TestUniversalDataContext()
		{
			var attribute = typeof(WhsInvoice).GetAttribute<UniversalDataContextAttribute>();

			AssertNotNull(attribute);
			AssertEquals(DataContextType.WarehousePeriodicInvoice, attribute.DataContextType);
		}

		#endregion

		#region Implementation

		new WhsTestHelperFunctionsInvoice Helper => (WhsTestHelperFunctionsInvoice)base.Helper;

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsInvoice(Factory);
		}

		#endregion
	}
}
