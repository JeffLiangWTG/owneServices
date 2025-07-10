using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	[TestedType(typeof(WhsInvoice))]
	class WhsInvoiceJobStorageTest : JobStorageTestCase
	{
		#region TestWarehouseName_Translatable

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse");
			var invoice = (WhsInvoice)GetNewBusinessObject();
			invoice.ET_WW = warehouse.PK;
			AssertEquals("WarehouseName in English.", "Test Warehouse", invoice.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("WarehouseName in Chinese.", "测试仓库", invoice.WarehouseName);
			}
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			WhsInvoice invoice = (WhsInvoice)GetNewBusinessObject();
			invoice.ET_StorageJobNumber = "I00001001";
			AssertEquals("Warehouse Periodic Invoice I00001001", invoice.HumanReadableName);
		}

		#endregion

		#region TestNoteContextsForRelatedNotes

		public void TestNoteContextsForRelatedNotes()
		{
			WhsInvoice invoice = (WhsInvoice)GetNewBusinessObject();

			Assert("Should always be 'Warehouse' module", (invoice.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert("Should always be 'Internal' direction", (invoice.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.I) != 0);
			Assert("Should always be 'Periodic billing' freight mode", (invoice.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.P) != 0);

			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("Warehouse");
			CreateTestNoteColection(client);
			CreateTestNoteColection(warehouse);

			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = warehouse.PK;
			AssertEquals("Visible Notes Count", 8, invoice.Notes.VisibleNotes.Count);
		}

		#endregion

		#region CreateTestNoteColection

		void CreateTestNoteColection(BusinessObject businessObject)
		{
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.A, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.R, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.R, StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.O);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O, StmNoteContextFreightMode.T);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.T);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.D);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.S);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I, StmNoteContextFreightMode.P);

			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.I, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.O, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A, StmNoteContextFreightMode.S);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A, StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.I, StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.A, StmNoteContextFreightMode.P);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.I, StmNoteContextFreightMode.P);
		}

		static StmNote GetStmNote(StmNote note, StmNoteContextModule module, StmNoteContextDirection direction, StmNoteContextFreightMode freightMode)
		{
			note.ST_NoteContextModule = module.ToString();
			note.ST_NoteContextDirection = direction.ToString();
			note.ST_NoteContextFreightMode = freightMode.ToString();
			return note;
		}

		#endregion

		#region TestGetReasonNotToAllowPosting

		public void TestGetReasonNotToAllowPosting()
		{
			var invoice = (WhsInvoice)GetNewBusinessObject();
			var invoiceSupporter = new WhsInvoiceInvoicingSupporter(invoice);

			invoice.ET_StorageToDate = ZDateTime.Today.AddDays(-5);

			AssertEquals(null, invoiceSupporter.GetReasonNotToAllowPosting());

			invoice.ET_StorageToDate = ZDateTime.Today.AddDays(1);
			AssertEquals(WhsInvoiceValidation.ErrorMessages.FutureDateNotAllowPosting, invoiceSupporter.GetReasonNotToAllowPosting());
		}
		#endregion

		#region TestMonthlySplitPeriodBilling

		#region TestMonthlySplitPeriodBillingForInventoriesOutsideInsideAndFuturePeriod

		[TestDate(2012, 12, 31)]
		public void TestMonthlySplitPeriodBillingForInventoriesOutsideInsideAndFuturePeriod()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			SetupDataForSplitPeriodicBilling(data, usePalletLocation: false);

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 31));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			Factory.Save();
			AssertEquals("Precondition: 1 charge should be created.", 5, invoice.JobHeader.Charges.Count);
			invoice.JobHeader.Charges.Sort("JR_Desc", ListSortDirection.Ascending);
			AssertEquals("Receive Handling R3 - P1 (P1)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Receive Handling sell Amount is incorrect for part1.", 1.5m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
			AssertEquals("Receive Handling R4 - P1 (P1)", invoice.JobHeader.Charges[1].JR_Desc);
			AssertEquals("Receive Handling sell Amount is incorrect for part1.", 1.5m, invoice.JobHeader.Charges[1].JR_LocalSellAmt);
			AssertEquals("Receive Storage R3 - P1 (P1)", invoice.JobHeader.Charges[2].JR_Desc);
			AssertEquals("Receive Storage sell Amount is incorrect for part1.", 4m, invoice.JobHeader.Charges[2].JR_LocalSellAmt);
			AssertEquals("Receive Storage R4 - P1 (P1)", invoice.JobHeader.Charges[3].JR_Desc);
			AssertEquals("Receive Storage sell Amount is incorrect for part1.", 2m, invoice.JobHeader.Charges[3].JR_LocalSellAmt);
			AssertEquals("Warehouse Storage - P1 (P1) for 31 days (01-Oct-12 - 31-Oct-12)", invoice.JobHeader.Charges[4].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 10m, invoice.JobHeader.Charges[4].JR_LocalSellAmt);
		}

		#endregion

		#region TestMonthlySplitPeriodBillingForInventoriesOutsideInsideAndFuturePeriod_WithPalletLocation

		[TestDate(2012, 12, 31)]
		public void TestMonthlySplitPeriodBillingForInventoriesOutsideInsideAndFuturePeriod_WithPalletLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			SetupDataForSplitPeriodicBilling(data, usePalletLocation: true);

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 31));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			Factory.Save();
			AssertEquals("Precondition: 1 charge should be created.", 5, invoice.JobHeader.Charges.Count);
			invoice.JobHeader.Charges.Sort("JR_Desc", ListSortDirection.Ascending);
			AssertEquals("Receive Handling R3 - P1 (P1)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Receive Handling sell Amount is incorrect for part1.", 1.5m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
			AssertEquals("Receive Handling R4 - P1 (P1)", invoice.JobHeader.Charges[1].JR_Desc);
			AssertEquals("Receive Handling sell Amount is incorrect for part1.", 1.5m, invoice.JobHeader.Charges[1].JR_LocalSellAmt);
			AssertEquals("Receive Storage R3 - P1 (P1)", invoice.JobHeader.Charges[2].JR_Desc);
			AssertEquals("Receive Storage sell Amount is incorrect for part1.", 4m, invoice.JobHeader.Charges[2].JR_LocalSellAmt);
			AssertEquals("Receive Storage R4 - P1 (P1)", invoice.JobHeader.Charges[3].JR_Desc);
			AssertEquals("Receive Storage sell Amount is incorrect for part1.", 2m, invoice.JobHeader.Charges[3].JR_LocalSellAmt);
			AssertEquals("Warehouse Storage - P1 (P1) for 31 days (01-Oct-12 - 31-Oct-12)", invoice.JobHeader.Charges[4].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 10m, invoice.JobHeader.Charges[4].JR_LocalSellAmt);
		}

		#endregion

		#region TestValidationIsDeferredForAutoRatingAndChildJobsAreNotValidated

		[TestDate(2012, 12, 31)]
		public void TestValidationIsDeferredForAutoRatingAndChildJobsAreNotValidated()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			var invoiceTo = Helper.CreateClient("RECEIVABLES");
			invoiceTo.OH_IsDebtor = true;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.SetRelatedParty(invoiceTo, RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, "");
			SetupDataForSplitPeriodicBilling(data, usePalletLocation: false);

			var receives = new List<WhsReceive>();
			var orders = new List<WhsOrder>();
			var adhocJobs = new List<WhsAdHocServiceJob>();

			var warehouseFreeStoreDept = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "WFS");
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), warehouseFreeStoreDept.PK.ToGuid()))
			{
				for (int index = 0; index < 10; index++)
				{
					var receivePart1 = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2012, 1, index + 1), true, new PartUnit(data.Part1, 10));
					var receivePart2 = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2012, 1, index + 11), true, new PartUnit(data.Part2, 10));
					var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDateTime(2012, 1, index + 1), "", true);
					var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2012, 1, index + 2), new PartUnit(data.Part1, 5));

					AddJobHeader(receivePart1);
					AddJobHeader(receivePart2);
					AddJobHeader(order);

					receives.Add(receivePart1);
					receives.Add(receivePart2);
					orders.Add(order);
					adhocJobs.Add(adhocServiceJob);
				}

				var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2012, 1, 1), new ZDateTime(2012, 1, 31));
				AssertContainsExactElementsInAnyOrder(orders, invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));
				AssertContainsExactElementsInAnyOrder(receives, invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));
				AssertContainsExactElementsInAnyOrder(adhocJobs, invoice.AdHocServiceJobs());

				Factory.Save();

				var invoiceValidationHitCount = 0;
				invoice.JobHeader.JH_OA_LocalChargesAddrInfo.AdditionalValidation += () => invoiceValidationHitCount++;

				var childJobValidationHitCount = 0;
				foreach (var job in receives.Select(r => r.JobHeader).Concat(orders.Select(o => o.JobHeader)).Concat(adhocJobs.Select(a => a.JobHeader)))
				{
					job.JH_OA_LocalChargesAddrInfo.AdditionalValidation += () => childJobValidationHitCount++;
				}

				invoice.AutoRateJobHeader(new TestInteractor());

				CombineAssertions(() =>
				{
					AssertEquals("Invoice should be validated before Autorating and when Autorating finishes.", 3, invoiceValidationHitCount);
					AssertEquals("Child Jobs should not be validated.", 0, childJobValidationHitCount);
				});
			}
		}

		#endregion

		void SetupDataForSplitPeriodicBilling(TestDataSimpleEnvironment data, bool usePalletLocation, string storagePeriod = Constants.StorageCalculationPeriods.Monthly)
		{
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = storagePeriod;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 1m);

			Factory.Save();

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WIN", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");
			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WST", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			var receiveHandlingCharge = Helper.CreateChargeCode("INWHAN", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receiveStorageCharge = Helper.CreateChargeCode("INWSTO", "Receive Storage", ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var warehouseStorageCharge = Helper.CreateChargeCode("WHSSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			var warehouseEntry = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));

			Helper.CreateRateLine(warehouseEntry, receiveHandlingCharge, "UNT", 1.5m, UnitCalculator.Code);

			var receiveStorageRateLine = warehouseEntry.AddRateLine(receiveStorageCharge.AC_Code, SplitMonthBillingCalculator.Code, "UNT");
			receiveStorageRateLine.Calculator["-15"] = (ZDecimal)4m;
			receiveStorageRateLine.Calculator["+15"] = (ZDecimal)2m;

			Helper.CreateRateLine(warehouseEntry, warehouseStorageCharge, (usePalletLocation ? "PL" : "UNT"), 5m, UnitCalculator.Code);
			var location = data.Whs1.FindLocation("A-1-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2011, 1, 1), data.Part1, 1m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2011, 1, 1), data.Part1, 1m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(2012, 10, 2), data.Part1, 1m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", new ZDateTimeOffset(2012, 10, 22), data.Part1, 1m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", new ZDateTimeOffset(2012, 11, 10), data.Part1, 1m, location, "");

			Factory.Save();
		}

		#region TestMonthlySplitPeriodBillingForLongStayingInventories

		[TestDate(2010, 1, 1)]
		public void TestMonthlySplitPeriodBillingForLongStayingInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetUpDataForMonthlySplitPeriodBillingTests(data, new ZDateTimeOffset(2009, 1, 20), new ZDateTimeOffset(2009, 3, 13));

			var invoice4 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2009, 3, 1), new ZDateTime(2009, 3, 31));
			var rateResultOrder = Helper.AutoRateJob(splitBillingOrder);

			AssertEquals(2, rateResultOrder.Count);
			AssertCorrectRateDescriptions(rateResultOrder[0], "Outward Handling REF1", "OUTHAN: 10 Unit(s) @ AUD 3.00/Unit");
			AssertCorrectRateDescriptions(rateResultOrder[1], "Outward Storage REF1 - P1 (P1)", "OUTSTO: 10 Unit(s) (Released Before 15 Day(s)) @ AUD 2.00/Unit");

			var invoice5 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2009, 4, 1), new ZDateTime(2009, 4, 30));
			var rateResult5 = Helper.CreateAutoRateInfoCollection_Obsolete(invoice5);
			AssertEquals(0, rateResult5.Count);
		}

		#endregion

		#region TestWeeklySplitPeriodBillingForInventoriesOutsideInsideAndFuturePeriod_OverOneMonth

		[TestDate(2012, 12, 31)]
		public void TestWeeklySplitPeriodBillingForInventoriesOutsideInsideAndFuturePeriod_OverOneMonth()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			SetupDataForSplitPeriodicBilling(data, usePalletLocation: false, storagePeriod: Constants.StorageCalculationPeriods.Weekly);

			var from = new ZDateTime(2012, 10, 1);
			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, from, from.AddDays(7 * 4 - 1));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			Factory.Save();
			AssertEquals("Precondition: 8 charges should be created.", 8, invoice.JobHeader.Charges.Count);
			var charges = new List<string>();
			foreach (Charge charge in invoice.JobHeader.Charges)
			{
				charges.Add(string.Format("Desc: {0}, Amount: {1}", charge.JR_Desc, charge.JR_LocalSellAmt.ToString("G29", CultureInfo.CurrentCulture)));
			}

			AssertCollectionContains("Desc: Receive Handling R3 - P1 (P1), Amount: 1.5", charges);
			AssertCollectionContains("Desc: Receive Handling R4 - P1 (P1), Amount: 1.5", charges);
			AssertCollectionContains("Desc: Receive Storage R3 - P1 (P1), Amount: 4", charges);
			AssertCollectionContains("Desc: Receive Storage R4 - P1 (P1), Amount: 2", charges);
			AssertCollectionContains("Desc: Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12), Amount: 10", charges);
			AssertCollectionContains("Desc: Warehouse Storage - P1 (P1) for 7 days (08-Oct-12 - 14-Oct-12), Amount: 15", charges);
			AssertCollectionContains("Desc: Warehouse Storage - P1 (P1) for 7 days (15-Oct-12 - 21-Oct-12), Amount: 15", charges);
			AssertCollectionContains("Desc: Warehouse Storage - P1 (P1) for 7 days (22-Oct-12 - 28-Oct-12), Amount: 15", charges);
		}

		#endregion

		#region TestMonthlySplitPeriodBillingForInventoriesThatLeaveTheSameMonthAsArrive

		[TestDate(2010, 1, 1)]
		public void TestMonthlySplitPeriodBillingForInventoriesThatLeaveTheSameMonthAsArrive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			SetUpDataForMonthlySplitPeriodBillingTests(data, new ZDateTimeOffset(2009, 1, 10), new ZDateTimeOffset(2009, 1, 23));

			var invoice1 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2008, 12, 1), new ZDateTime(2008, 12, 31));
			var rateResult1 = Helper.CreateAutoRateInfoCollection_Obsolete(invoice1);
			AssertEquals("Make sure that no rate applied before inventory arrive", 0, rateResult1.Count);

			var invoice2 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2009, 1, 1), new ZDateTime(2009, 1, 31));
			var rateResult2 = Helper.AutoRateJob(invoice2);
			AssertEquals(0, rateResult2.Count);
			var rateResultReceive = Helper.AutoRateJob(splitBillingReceive);
			var rateResultOrder = Helper.AutoRateJob(splitBillingOrder);

			AssertCorrectRateDescriptions(rateResultOrder[0], "Outward Handling REF1", "OUTHAN: 10 Unit(s) @ AUD 3.00/Unit");
			AssertCorrectRateDescriptions(rateResultOrder[1], "Outward Storage REF1 - P1 (P1)", "OUTSTO: 0 Unit(s) (Released After 15 Day(s)On Request) @ AUD 4.00/Unit");
			AssertCorrectRateDescriptions(rateResultReceive[0], "Receive Handling REF0 - P1 (P1)", "INWHAN: 10 Unit(s) @ AUD 1.00/Unit");
			AssertCorrectRateDescriptions(rateResultReceive[1], "Receive Storage REF0 - P1 (P1)", "INWSTO: 10 Unit(s) (Received Before 15 Day(s)) @ AUD 4.00/Unit");

			var invoice3 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2009, 2, 1), new ZDateTime(2009, 2, 28));
			var rateResult3 = Helper.CreateAutoRateInfoCollection_Obsolete(invoice3);
			AssertEquals("Make sure that no rate applied after stock is removed", 0, rateResult3.Count);
		}

		#endregion

		void SetUpDataForMonthlySplitPeriodBillingTests(TestDataSimpleEnvironment data, ZDateTimeOffset receiveArrivalDate, ZDateTimeOffset orderFinaliseDate)
		{
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WIN", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");
			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WOU", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");
			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WST", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			var receiveHandlingCharge = Helper.CreateChargeCode("INWHAN", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receiveStorageCharge = Helper.CreateChargeCode("INWSTO", "Receive Storage", ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var warehouseStorageCharge = Helper.CreateChargeCode("WHSSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			var outwardHandlingCharge = Helper.CreateChargeCode("OUTHAN", "Outward Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var outwardStorageCharge = Helper.CreateChargeCode("OUTSTO", "Outward Storage", ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);

			var clientRate = Factory.New<ClientRate>();
			data.Org1.OH_IsDebtor = true;
			clientRate.TH_OH = data.Org1.PK;

			var warehouseEntry = Helper.CreateRateEntry(clientRate, new ZDate(2008, 9, 1), new ZDate(2009, 9, 1));

			var receiveHandlingRateLine = warehouseEntry.AddRateLine(receiveHandlingCharge.AC_Code, UnitCalculator.Code, "UNT");
			receiveHandlingRateLine.Calculator.Decimal1 = 1m;

			var receiveStorageRateLine = warehouseEntry.AddRateLine(receiveStorageCharge.AC_Code, SplitMonthBillingCalculator.Code, "UNT");
			receiveStorageRateLine.Calculator["-15"] = (ZDecimal)4m;
			receiveStorageRateLine.Calculator["+15"] = (ZDecimal)2m;

			var warehouseStorageRateLine = warehouseEntry.AddRateLine(warehouseStorageCharge.AC_Code, UnitCalculator.Code, "UNT");
			warehouseStorageRateLine.Calculator.Decimal1 = 5m;

			var outwardHandlingRateLine = warehouseEntry.AddRateLine(outwardHandlingCharge.AC_Code, UnitCalculator.Code, "UNT");
			outwardHandlingRateLine.Calculator.Decimal1 = 3m;
			outwardHandlingRateLine.TL_IsWhsJobLevelCharge = true;

			var outwardStorageRateLine = warehouseEntry.AddRateLine(outwardStorageCharge.AC_Code, SplitMonthBillingCalculator.Code, "UNT");
			outwardStorageRateLine.Calculator["-15"] = (ZDecimal)2m;
			outwardStorageRateLine.Calculator["+15"] = (ZDecimal)4m;

			splitBillingReceive = CreateWhsReceive(data.Whs1, data.Org1, receiveArrivalDate, true, new PartUnit(data.Part1, 10));
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(splitBillingReceive);
			Factory.Save();

			splitBillingOrder = CreateFinalisedWhsOrder(data.Whs1, data.Org1, orderFinaliseDate, new PartUnit(data.Part1, 10));
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(splitBillingOrder);

			Factory.Save();
		}

		WhsReceive splitBillingReceive;
		WhsOrder splitBillingOrder;

		void AssertCorrectRateDescriptions(AutoRateInfo rateInfo, ZString expectedInvoiceLineDesc, ZString expectedSellCalcSingleLineDesc)
		{
			AssertEquals(expectedInvoiceLineDesc, rateInfo.InvoiceLineDescription);
			AssertEquals(expectedSellCalcSingleLineDesc, rateInfo.SingleLineDescription);
		}

		#region TestMonthlySplitPeriodBilling_CombinationReceiveChargeAndStorageCharge

		public void TestMonthlySplitPeriodBilling_ReceiveNOTRatedBeforePeriodicInvoicing()
		{
			MonthlySplitPeriodBilling_ReceiveRatedOrNOTRatedBeforePeriodicInvoicing(false);
		}

		public void TestMonthlySplitPeriodBilling_ReceiveRatedBeforePeriodicInvoicing()
		{
			MonthlySplitPeriodBilling_ReceiveRatedOrNOTRatedBeforePeriodicInvoicing(true);
		}

		void MonthlySplitPeriodBilling_ReceiveRatedOrNOTRatedBeforePeriodicInvoicing(bool runAutoRateOnReceive)
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WIN", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");
			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WST", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			var receiveHandlingCharge = Helper.CreateChargeCode("INWSTO", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var warehouseStorageCharge = Helper.CreateChargeCode("WHSSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			var warehouseEntry = Helper.CreateRateEntry(clientRate, new ZDate(year - 1, 1, 1), new ZDate(year, 12, 31));

			var receiveStorageRateLine = warehouseEntry.AddRateLine(receiveHandlingCharge.AC_Code, SplitMonthBillingCalculator.Code, "UNT");
			receiveStorageRateLine.Calculator["-15"] = (ZDecimal)4m;
			receiveStorageRateLine.Calculator["+15"] = (ZDecimal)2m;

			var warehouseStorageRateLine = warehouseEntry.AddRateLine(warehouseStorageCharge.AC_Code, UnitCalculator.Code, "UNT");
			warehouseStorageRateLine.Calculator.Decimal1 = 5m;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new DateTime(year - 1, 12, 11), data.Part1, 10);
			Factory.Save();

			if (runAutoRateOnReceive)
			{
				var jobHeader = Helper.CreateRatingJob(receive);

				var iReceive = (IRatingSupporter)receive;
				Helper.AutoRateJob(receive, jobHeader, iReceive.AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue).ToArray());

				var charge = jobHeader.Charges.Cast<Charge>().Single();
				AssertEquals("charge.JR_AC", receiveHandlingCharge.PK, charge.JR_AC);
				AssertEquals("charge.JR_Desc", "Receive Handling R1 - P1 (P1)", charge.JR_Desc);

				Factory.Save();
			}

			// periodic invoicing
			var invoice1 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year - 1, 12, 1), new ZDateTime(year - 1, 12, 31));
			var rateResult1 = Helper.AutoRateJob(invoice1);
			AssertEquals(0, rateResult1.Count);
			var rateResultReceive = Helper.AutoRateJob(receive);

			AssertCorrectRateDescriptions(rateResultReceive[0], "Receive Handling R1 - P1 (P1)", "INWSTO: 10 Unit(s) (Received Before 15 Day(s)) @ AUD 4.00/Unit");

			var invoice2 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var rateResult2 = Helper.AutoRateJob(invoice2);
			AssertEquals(1, rateResult2.Count);
			var yearInTwoDigit = new ZDateTime(year, 1, 1).ToString("yy");
			AssertCorrectRateDescriptions(rateResult2[0], "Warehouse Storage - P1 (P1) for 31 days (01-Jan-" + yearInTwoDigit + " - 31-Jan-" + yearInTwoDigit + ")", "WHSSTO: 10 Unit(s) @ AUD 5.00/Unit");
		}

		#endregion

		#endregion

		#region TestCompanyTariffBasedCalculator_CorrectlyConvertUNTintoPLT

		[TestDate(2010, 1, 1)]
		public void TestCompanyTariffBasedCalculator_CorrectlyConvertUNTintoPLT_UsingCompanyTariffs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 10m);
			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WST", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// setup warehouse company tariff + unit calculator
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.TH_GlobalRateDescription = "Company Tariff Level 1";

			RateEntry warehouseEntry = companyTariff.AddRateEntry("WHS", "ALL", "", "", "", "");
			warehouseEntry.RateLines.RemoveAndDeleteAll();
			warehouseEntry.TI_RateStartDate = new ZDate(2009, 9, 1);

			AccChargeCode warehouseStorageCharge = Helper.CreateChargeCode("WHSSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			RateLine warehouseStorageRateLine = warehouseEntry.AddRateLine(warehouseStorageCharge.AC_Code, UnitCalculator.Code, "PLT");
			warehouseStorageRateLine.Calculator.Decimal1 = 5m;

			// setup warehouse Receive
			WhsReceive receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2009, 9, 1), true, new PartUnit(data.Part1, 40));
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			// setup another CompanyTariff + company tariff based calculator (this is the calculator with the bug)
			var otherFactory = new BusinessObjectFactory();
			var companyTariff2 = otherFactory.New<CompanyTariff>();
			AssertEquals("Precondition - ensure that tariff was auto generated.", 1, companyTariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.WHS).Count);

			companyTariff2.Discounts.SetDiscount(RatingConstants.RateCategory.WHS, 10m);
			RateEntry companyTariffEntry2 = companyTariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.WHS)[0];
			companyTariffEntry2.RateLines.OverrideTariffLines(new[] { companyTariffEntry2.RateLines[0] });
			AssertEquals("Precondition - ensure that Company Tariff Line was overriden.", false, companyTariffEntry2.RateLines[0].IsTariffLineInherited);
			AssertEquals("Precondition - ensure that Company Tariff Based Calculator is applied.", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode, companyTariffEntry2.RateLines[0].TL_RateCalculator);
			AssertEquals("Precondition - ensure that discount was applied correctly - percent", -10m, companyTariffEntry2.RateLines[0].GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent);
			AssertEquals("Precondition - ensure that discount was applied correctly - perUnitPercent", -10m, companyTariffEntry2.RateLines[0].GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent);

			otherFactory.Save();

			// setup an invoice and rate the Receive (using the unit calculator)
			var invoice1Factory = new BusinessObjectFactory();
			WhsInvoice invoice1 = Helper.CreateInvoiceWithJobHeader(invoice1Factory, data.Org1, data.Whs1, new ZDateTime(2009, 10, 1), new ZDateTime(2009, 10, 7));
			invoice1.Client.CompanyData.RateTariffLevels.SetLevel("DEF", 1);
			AutoRateInfoCollection rateResult1 = Helper.AutoRateJob(invoice1);
			AssertEquals(1, rateResult1.Count);
			AssertCorrectRateDescriptions(rateResult1[0], "Warehouse Storage - P1 (P1) for 7 days (01-Oct-09 - 07-Oct-09)", "WHSSTO: 4 Pallet(s) @ AUD 5.00/Pallet");
			AssertEquals("Sell Amount is incorrect.", 20m, rateResult1[0].Amount);

			// setup an invoice and rate the Receive (company tariff based calculator)
			var invoice2Factory = new BusinessObjectFactory();
			WhsInvoice invoice2 = Helper.CreateInvoiceWithJobHeader(invoice2Factory, data.Org1, data.Whs1, new ZDateTime(2009, 10, 8), new ZDateTime(2009, 10, 14));
			invoice2.Client.CompanyData.RateTariffLevels.SetLevel("DEF", 2);
			AutoRateInfoCollection rateResult2 = Helper.AutoRateJob(invoice2);
			AssertEquals(1, rateResult2.Count);
			AssertCorrectRateDescriptions(rateResult2[0], "Warehouse Storage - P1 (P1) for 7 days (08-Oct-09 - 14-Oct-09)", "WHSSTO: 4 Pallet(s) @ AUD 4.50/Pallet");
			AssertEquals("Sell Amount is incorrect.", 18m, rateResult2[0].Amount);
		}

		[TestDate(2010, 1, 1)]
		public void TestCompanyTariffBasedCalculator_CorrectlyConvertUNTintoPLT_UsingClientRates()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 10m);
			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WST", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// setup warehouse company tariff + unit calculator
			var companyTariff = Factory.New<CompanyTariff>();
			companyTariff.TH_GlobalRateLevel = 1;
			companyTariff.TH_GlobalRateDescription = "Company Tariff Level 1";

			var companyTariffWarehouseEntry = companyTariff.AddRateEntry("WHS", "ALL", "", "", "", "");
			companyTariffWarehouseEntry.RateLines.RemoveAndDeleteAll();
			companyTariffWarehouseEntry.TI_RateStartDate = new ZDate(2009, 9, 1);

			var companyTariffWarehouseStorageCharge = Helper.CreateChargeCode("WHSSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			var companyTariffWarehouseStorageRateLine = companyTariffWarehouseEntry.AddRateLine(companyTariffWarehouseStorageCharge.AC_Code, UnitCalculator.Code, "PLT");
			companyTariffWarehouseStorageRateLine.Calculator.Decimal1 = 5m;

			data.Org1.CompanyData.RateTariffLevels.SetLevel("DEF", 1);
			data.Org1.OH_IsDebtor = true;

			// setup warehouse Receive
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2009, 9, 1), true, new PartUnit(data.Part1, 40));
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			// setup Client Rate + company tariff based calculator (this is the calculator with the bug)
			var otherFactory = new BusinessObjectFactory();
			var clientRate = otherFactory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			var clientRateWarehouseEntry = clientRate.AddRateEntry("WHS", "ALL", "", "", "");
			clientRateWarehouseEntry.RateLines.RemoveAndDeleteAll();
			clientRateWarehouseEntry.TI_RateStartDate = new ZDate(2009, 9, 1);

			var clientRateWarehouseStorageCharge = Helper.CreateChargeCode("WHSSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			var clientRateWarehouseStorageRateLine = clientRateWarehouseEntry.AddRateLine(
				companyTariffWarehouseStorageCharge.AC_Code,
				CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode,
				"PLT");
			clientRateWarehouseStorageRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = -10m;
			clientRateWarehouseStorageRateLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = -10m;

			otherFactory.Save();

			// setup an invoice and rate the Receive (Company Tariff Based Calculator)
			var invoice1Factory = new BusinessObjectFactory();
			var invoice1 = Helper.CreateInvoiceWithJobHeader(invoice1Factory, data.Org1, data.Whs1, new ZDateTime(2009, 10, 1), new ZDateTime(2009, 10, 7));
			var rateResult1 = Helper.AutoRateJob(invoice1);
			AssertEquals(1, rateResult1.Count);
			AssertCorrectRateDescriptions(rateResult1[0], "Warehouse Storage - P1 (P1) for 7 days (01-Oct-09 - 07-Oct-09)", "WHSSTO: 4 Pallet(s) @ AUD 4.50/Pallet");
			AssertEquals("Sell Amount is incorrect.", 18m, rateResult1[0].Amount);
		}

		#endregion

		#region TestContainerRatingWihtIsPalletizedAttribute

		public void TestContainerRatingWihtIsPalletizedAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "CTN";
			data.Part1.PartUnits.GetUnitConversion("CTN", "UNT").OF_QuantityInParent = 24m;

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, "WIN", "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			AccChargeCode receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");

			var clientRate = Factory.New<ClientRate>();
			data.Org1.OH_IsDebtor = true;
			clientRate.TH_OH = data.Org1.PK;

			// create a base rate (for containers)

			RateEntry warehouseEntry = clientRate.AddRateEntry("WHS", "ALL", "", "", "", "20GP");
			warehouseEntry.RateLines.RemoveAndDeleteAll();
			warehouseEntry.TI_RateStartDate = ZDate.Today.AddMonths(-1);
			warehouseEntry.TI_RateEndDate = ZDate.Today.AddYears(1);

			// create non-pallatized calculator

			RateLine receiveHandlingRateLine_NonPalletized = warehouseEntry.AddRateLine(receiveHandlingCharge.AC_Code, CombinedCalculator.Code, "CTN");
			receiveHandlingRateLine_NonPalletized.Calculator.AddRateLineItem("-", 1000m, 0m, 250m);
			receiveHandlingRateLine_NonPalletized.Calculator.AddRateLineItem("+", 1000m, 0m, 350m);
			receiveHandlingRateLine_NonPalletized.Calculator.AddRateLineItem("+", 2000m, 0m, 450m);

			// create pallatized calculator

			RateLine receiveHandlingRateLine_Palletized = warehouseEntry.AddRateLine(receiveHandlingCharge.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			receiveHandlingRateLine_Palletized.TL_IsOnPallets = true;
			receiveHandlingRateLine_Palletized.Calculator.Decimal1 = 1000m;

			// create receives

			WhsReceive receive_WithNoContainer = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_WithNoContainer, data.Part1, 1160m);
			receive_WithNoContainer.AllocateLocationsWithMock();
			receive_WithNoContainer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_WithNoContainer);

			const bool IS_PALLETIZED = true;

			WhsReceive receive_WithNonPalletizedContainer = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_WithNonPalletizedContainer, data.Part1, 1160m);
			Helper.CreateWhsDocketContainer(receive_WithNonPalletizedContainer, "CONTAINER1", "20GP", true, !IS_PALLETIZED);
			receive_WithNonPalletizedContainer.AllocateLocationsWithMock();
			receive_WithNonPalletizedContainer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_WithNonPalletizedContainer);

			WhsReceive receive_WithPalletizedContainer = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive_WithPalletizedContainer, data.Part1, 1160m);
			Helper.CreateWhsDocketContainer(receive_WithPalletizedContainer, "CONTAINER2", "20GP", true, IS_PALLETIZED);
			receive_WithPalletizedContainer.AllocateLocationsWithMock();
			receive_WithPalletizedContainer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_WithPalletizedContainer);

			Factory.Save();

			// test that we add 1 rate per receive, and that we rate with the correct calculator

			WhsInvoice invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, ZDateTime.Today.AddDays(-3), ZDateTime.Today.AddDays(+4));
			var rateResult = Helper.AutoRateJob(invoice);
			AssertEquals("Should be 0 charges for invoice", 0, rateResult.Count);

			var rateResult_NoContainer = Helper.AutoRateJob(receive_WithNoContainer);
			AssertEquals("Should be 0 charges for receive with non-palletized container", 0, rateResult_NoContainer.Count);

			var rateResult_NonPalletizedContainer = Helper.AutoRateJob(receive_WithNonPalletizedContainer);
			AssertEquals("Should be 1 charge for receive R2", 1, rateResult_NonPalletizedContainer.Count);

			var rateResult_WithPalletizedContainer = Helper.AutoRateJob(receive_WithPalletizedContainer);
			AssertEquals("Should be 1 charge for receive R3", 2, rateResult_WithPalletizedContainer.Count);

			AssertCorrectRateDescriptions(rateResult_WithPalletizedContainer[0], "Receive Handling R3", "WRECHAN: 1 20GP Container(s) @ AUD 1000.00/Container");
			AssertCorrectRateDescriptions(rateResult_NonPalletizedContainer[0], "Receive Handling R2 - P1 (P1)", "WRECHAN: Base Rate AUD 350.00 for 1160 CTN");
			AssertCorrectRateDescriptions(rateResult_WithPalletizedContainer[1], "Receive Handling R3 - P1 (P1)", "WRECHAN: Base Rate AUD 350.00 for 1160 CTN");
		}

		#endregion

		#region TestToDateSetOnClient

		[TestDate(2008, 1, 1)]
		public void TestToDateSetOnClient()
		{
			WhsInvoice invoice = Factory.NewWithValidTestData<WhsInvoice>();
			OrgHeader client = Factory.New<OrgHeader>();

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 9, 4);
			AssertEquals(new ZDateTime(2006, 9, 10), invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 9, 4);
			AssertEquals(new ZDateTime(2006, 9, 4), invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 9, 4);
			AssertEquals(new ZDateTime(2006, 9, 17), invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 9, 4);
			AssertEquals(new ZDateTime(2006, 10, 3), invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.BillingPeriod;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 9, 4);
			AssertEquals(ZDateTime.Today, invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 9, 1);
			AssertEquals(new ZDateTime(2006, 9, 30), invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			invoice.ET_OH_Client = ZGuid.Empty;
			invoice.ET_OH_Client = client.PK;
			invoice.ET_StorageFromDate = new DateTime(2006, 10, 1);
			AssertEquals(new ZDateTime(2006, 10, 31), invoice.ET_StorageToDate);
			invoice.Validation.ValidateET_StorageToDate();
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2006, 11, 30);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2006, 12, 31);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2007, 1, 31);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2007, 2, 28);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2007, 3, 31);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2007, 4, 30);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.ET_StorageToDate = new ZDateTime(2007, 5, 30);
			AssertHasErrors(invoice.ET_StorageToDateInfo);
		}

		#endregion

		#region TestReadOnlyAndIsFinalised

		public void TestReadOnlyAndIsFinalised()
		{
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			AssertEquals(false, invoice.ReadOnly);
			AssertEquals(false, invoice.IsFinalised);
			Factory.Save();

			using (var jobHeader = new Job.Loader(invoice).TryCreateWithMutex())
			{
				jobHeader.PlugInData = invoice;

				JobCharge charge = invoice.JobHeader.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_LocalSellAmt = 100m;
				invoice.AutoRateJobHeader(null);
				invoice.PostInvoice();
				AssertEquals(false, invoice.ReadOnly);
				AssertEquals(false, invoice.IsFinalised);

				invoice.JobHeader.Close(null, null);
				AssertEquals(true, invoice.ReadOnly);
				AssertEquals(true, invoice.IsFinalised);
				AssertEquals("Must save before posting so AH_ConsolidatedInvoiceRef can be set", true, invoice.IsInDatabase);
			}
		}

		#endregion

		#region TestReadonly_StatusNotCommitted

		public void TestReadonly_StatusNotCommitted()
		{
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			AssertEquals("Precondition", false, invoice.ReadOnly);
			AssertEquals("Precondition", false, invoice.IsFinalised);
			Factory.Save();

			using (var jobHeader = new Job.Loader(invoice).TryCreateWithMutex())
			{
				AssertNotEquals("Precondition", JobHeaderStatus.Closed.Code, jobHeader.JH_Status);
				AssertEquals("Precondition", false, invoice.ReadOnly);
				AssertEquals("Precondition", false, invoice.IsFinalised);

				Factory.Save();
				AssertEquals(false, invoice.ReadOnly);

				jobHeader.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals(false, invoice.IsFinalised);
				AssertEquals(false, invoice.ReadOnly);

				Factory.Save();
				AssertEquals(false, invoice.IsFinalised);
				AssertEquals(false, invoice.ReadOnly);

				jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
				AssertEquals("Invoice should be finalised because the Job is closed.", true, invoice.IsFinalised);
				AssertEquals("Invoice is finalised, but it is not committed, so invoice should still be editable.", false, invoice.ReadOnly);

				Factory.Save();
				AssertEquals("Invoice should be finalised because the Job is closed.", true, invoice.IsFinalised);
				AssertEquals("Invoice is finalised and IS committed, so invoice should be readonly.", true, invoice.ReadOnly);

				jobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
				AssertEquals("Status is changed back to not Closed, so should not be finalised.", false, invoice.IsFinalised);
				AssertEquals("Status is changed back to not Closed, so should be editable again.", false, invoice.ReadOnly);
			}
		}

		#endregion

		#region TestReadonly_NoJobHeader

		public void TestReadonly_NoJobHeader()
		{
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			AssertEquals("Precondition", false, invoice.ReadOnly);
			AssertEquals("Precondition", false, invoice.IsFinalised);

			invoice.ReadOnly = true;
			AssertEquals("The invoice doesn't have a job header, so can't be finalised.", false, invoice.IsFinalised);
			AssertEquals("The invoice was marked readonly, and even though there is no job header, the invoice ignores the job header and should be readonly.", true, invoice.ReadOnly);
		}

		#endregion

		#region TestHasOverlappingInvoices_DatesOutOfRange

		public void TestHasOverlappingInvoices_DatesOutOfRange()
		{
			var testInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			testInvoice.ET_StorageFromDate = new ZDateTime(2080, 1, 1);
			testInvoice.ET_StorageToDate = new ZDateTime(2080, 1, 2);

			Assert(testInvoice.ET_StorageFromDateInfo.HasErrors());
			Assert(testInvoice.ET_StorageToDateInfo.HasErrors());
			Assert(!testInvoice.HasOverlappingInvoices());

			testInvoice.ET_StorageFromDate = new ZDateTime(1899, 12, 24);
			testInvoice.ET_StorageToDate = new ZDateTime(1899, 12, 25);

			Assert(testInvoice.ET_StorageFromDateInfo.HasErrors());
			Assert(testInvoice.ET_StorageToDateInfo.HasErrors());
			Assert(!testInvoice.HasOverlappingInvoices());
		}

		#endregion

		#region TestDefaultValues

		public void TestDefaultValues()
		{
			WhsInvoice invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = ZGuid.Empty;
			AssertEquals("Storage type should be WHS (Warehouse)", WhsInvoice.StorageType, invoice.ET_StorageType);
			AssertEquals(ZDateTime.Today, invoice.ET_BillingDate);
			AssertEquals(ZDateTime.Today, invoice.ET_StorageToDate);
		}

		#endregion

		#region TestOnFactorySaving

		#endregion

		#region TestWhsAdHocServiceJobsCollection

		public void TestWhsAdHocServiceJobCollection()
		{
			var year = ZDateTime.Now.Year;

			var warehouse = Helper.CreateWarehouse("Whs1");
			var client = Helper.CreateClient("Client");

			Helper.CreateWhsAdHocServiceJob(warehouse, client, new ZDateTime(year, 2, 1, 11, 0, 0), "", true);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(warehouse, client, new ZDateTime(year, 2, 2, 11, 0, 0), "", true);
			var adhocServiceJob3 = Helper.CreateWhsAdHocServiceJob(warehouse, client, new ZDateTime(year, 2, 9, 11, 0, 0), "", true);
			var adhocServiceJob4 = Helper.CreateWhsAdHocServiceJob(warehouse, client, new ZDateTime(year, 3, 1, 11, 0, 0), "", true);
			Helper.CreateWhsAdHocServiceJob(warehouse, client, new ZDateTime(year, 2, 9, 11, 0, 0), "", false);

			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = warehouse.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 2);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 9);

			AssertContainsExactElementsInAnyOrder(new[] { adhocServiceJob2, adhocServiceJob3 }, invoice.AdHocServiceJobs());

			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { adhocServiceJob2, adhocServiceJob3, adhocServiceJob4 }, invoice.AdHocServiceJobs());

			invoice.ET_WW = Helper.CreateWarehouse("Whs2").PK;
			AssertEquals(0, invoice.AdHocServiceJobs().Length);

			invoice.ET_WW = warehouse.PK;
			invoice.ET_OH_Client = Helper.CreateClient("Client2").PK;
			AssertEquals(0, invoice.AdHocServiceJobs().Length);
		}

		#endregion

		#region TestWhsVASOrderCollection

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestWhsVASOrderCollection_TransferInOnly()
		{
			TestWhsVASOrderCollection_Core(hasTransferOut: false);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestWhsVASOrderCollection_TransferOut()
		{
			TestWhsVASOrderCollection_Core(hasTransferOut: true);
		}

		void TestWhsVASOrderCollection_Core(bool hasTransferOut)
		{
			TestDateAttribute.UseUNLOCO = true;
			var year = ZDateTime.Now.Year;
			var whs = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var client = Helper.CreateClient("Client1");
			var product = Helper.CreateProduct("P1", client);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", new DateTime(year, 1, 1, 11, 0, 0), product, 7m);
			CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 1, 7, 0, 0));
			var vasOrderFeb2 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 2, 11, 0, 0), (hasTransferOut ? new DateTime(year, 2, 2, 11, 0, 0) : (DateTime?)null));
			var vasOrderFeb8 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 9, 0, 0, 0), (hasTransferOut ? new DateTime(year, 2, 9, 0, 0, 0) : (DateTime?)null));
			var vasOrderFeb9 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 9, 11, 0, 0), (hasTransferOut ? new DateTime(year, 2, 9, 11, 0, 0) : (DateTime?)null));
			var vasOrderFebLast = CreateVASOrder(client, serviceArea, product, new DateTime(year, 3, 1, 0, 0, 0), (hasTransferOut ? new DateTime(year, 3, 1, 0, 0, 0) : (DateTime?)null));
			var vasOrderMar1 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 3, 1, 11, 0, 0), (hasTransferOut ? new DateTime(year, 3, 1, 11, 0, 0) : (DateTime?)null));
			CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 9, 11, 0, 0), (hasTransferOut ? new DateTime(year, 2, 9, 11, 0, 0) : (DateTime?)null), finalised: false);

			var invoice = Helper.CreateInvoice(Factory, client, whs, new ZDateTime(year, 2, 2), new ZDateTime(year, 2, 9));
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice, vasOrderFeb2, vasOrderFeb8);

			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice, vasOrderFeb2, vasOrderFeb8, vasOrderFeb9, vasOrderFebLast);

			invoice.ET_WW = Helper.CreateWarehouse("Whs2").PK;
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice);

			invoice.ET_WW = whs.PK;
			invoice.ET_OH_Client = Helper.CreateClient("Client2").PK;
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestWhsVASOrderCollection_ClientWithNoMainAddress()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Testing";
			client.OH_Code = "Testing";

			AssertNull(Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>());
			BusinessObjectUniversalCopyFactoryService.EnsureServiceIsSetUp(Factory); // Simulate start Universal Copy
			AssertNotNull("Precondition: if Universal Copy Service exists and client does not have main address.(In this case does not create address)", Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>());

			var invoice = Helper.CreateInvoice(Factory, client, whs, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			AssertNoExceptionThrown("Should be able to get Additional jobs with no exception.", () => _ = ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestWhsVASOrderCollection_TransferOutFinaliseDateDifferentFromTransferIn()
		{
			var year = ZDateTime.Now.Year;
			var whs = Helper.CreateWarehouse("Whs1", "A", 2, 1);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var client = Helper.CreateClient("Client1");
			var product = Helper.CreateProduct("P1", client);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", new DateTime(year, 1, 1, 11, 0, 0), product, 5m);
			var vasOrderOnlyTransferIn1 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 2, 4, 0, 0));
			var vasOrderOnlyTransferIn2 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 2, 11, 0, 0));
			var vasOrderWithTransferOut1 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 2, 11, 0, 0), new DateTime(year, 2, 3, 5, 0, 0));
			var vasOrderWithTransferOut2 = CreateVASOrder(client, serviceArea, product, new DateTime(year, 2, 2, 13, 0, 0), new DateTime(year, 2, 3, 13, 0, 0));

			var invoice = Helper.CreateInvoice(Factory, client, whs, new ZDateTime(year, 2, 2), new ZDateTime(year, 2, 3));
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice, vasOrderOnlyTransferIn1, vasOrderOnlyTransferIn2, vasOrderWithTransferOut1);

			invoice.ET_StorageToDate = new ZDateTime(year, 2, 2);
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice, vasOrderOnlyTransferIn1);

			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 3);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 3);
			AssertJobInvoicingPlugInAdditionalJobsResult(invoice, vasOrderOnlyTransferIn2, vasOrderWithTransferOut1);
		}

		static void AssertJobInvoicingPlugInAdditionalJobsResult(WhsInvoice invoice, params WhsVASOrder[] expectedVASOrders)
		{
			var additionalJobs = (IJobInvoicingPlugInAdditionalJobs)invoice;
			AssertContainsExactElementsInAnyOrder(expectedVASOrders.Select(v => v.PK), additionalJobs.AdditionalJobsToShowChargesFor.Select(j => j.PK));
		}

		WhsVASOrder CreateVASOrder(OrgHeader client, WhsArea serviceArea, OrgSupplierPart product, ZDateTimeOffset transferInfinaliseDate, ZDateTimeOffset? transferOutfinaliseDate = null, bool finalised = true)
		{
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Helper.CreateWhsVASOrderLine(vasOrder, product, 1m);
			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			initialTransfer.WD_FinalisedDate = transferInfinaliseDate;
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();
			if (finalised)
			{
				if (transferOutfinaliseDate.HasValue)
				{
					WhsTransfer returnTransfer;

					using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
					{
						returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
					}
					returnTransfer.FinaliseDocketWithoutUserConfirmation();
					returnTransfer.WD_FinalisedDate = transferOutfinaliseDate.Value;
					vasOrder.WVO_FinalizedTimeUtc = transferOutfinaliseDate.Value.ToUtcDateTime();
				}
				else
				{
					vasOrder.WVO_FinalizedTimeUtc = transferInfinaliseDate.ToUtcDateTime();
				}
			}
			AssertEquals($"VAS Order should {(finalised ? "not " : "")}be finalised.", finalised, vasOrder.IsFinalised);
			Factory.Save();
			return vasOrder;
		}

		#endregion

		#region TestWhsReceiveAndWhsOrderCollections

		public void TestWhsReceiveAndWhsOrderCollections()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();
			var docket1 = CreateWhsReceive(Whs2, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			var docket2 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 11, 0, 0), true, new PartUnit(Part1, 15), new PartUnit(Part1, 5));
			var docket2a = CreateWhsReceive(Whs1, Org2, new ZDateTimeOffset(year, 2, 5, 11, 0, 0), true, new PartUnit(Part1, 50));
			var docket3 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 11, 0, 0), true, new PartUnit(Part2, 35));
			var docket4 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 10, 11, 0, 0), true, new PartUnit(Part1, 20));

			var docket5 = CreateFinalisedWhsOrder(Whs2, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var docket6 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 3, 11, 0, 0), new PartUnit(Part1, 10));
			var docket6a = CreateFinalisedWhsOrder(Whs1, Org2, new ZDateTimeOffset(year, 2, 7, 11, 0, 0), new PartUnit(Part1, 40));
			var docket7 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 15, 0, 0), new PartUnit(Part1, 10), new PartUnit(Part2, 15));
			var docket8 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 3, 1, 11, 0, 0), new PartUnit(Part2, 20));

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 2);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 9);
			AssertContainsExactElementsInAnyOrder(new[] { docket2, docket3 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));

			AssertContainsExactElementsInAnyOrder(new[] { docket6, docket7 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { docket2, docket3, docket4 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));

			AssertContainsExactElementsInAnyOrder(new[] { docket6, docket7, docket8 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { docket2, docket3, docket4 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));
			AssertContainsExactElementsInAnyOrder(new[] { docket6, docket7, docket8 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_WW = Whs2.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { docket1 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));
			AssertContainsExactElementsInAnyOrder(new[] { docket5 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_WW = ZGuid.Empty;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertEquals(0, invoice.GetAdditionalDockets().Length);
		}

		#endregion

		#region TestWhsReceiveCollections_ExcludePickByBOMReceives

		public void TestWhsReceiveCollections_ExcludePickByBOMReceives()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			var bike = Helper.CreateProduct(Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(Org1, Whs1, "R1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(2000, 1, 1);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(Org1, Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 10m);
			var pick = Helper.CreatePickNew(order);

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			createdReceive.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 7, 0, 0, 0);

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 2);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 9);
			AssertEquals("The createdReceive should be excluded.", 0, invoice.GetAdditionalDockets().Count(d => d.WD_DocketType == DocketType.Codes.Receive));
		}

		#endregion

		#region TestGetAdditionalDocketsWithoutSetJobDefaults

		public void TestGetAdditionalDocketsWithoutSetJobDefaults()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();
			var docket1 = CreateWhsReceive(Whs2, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			var docket2 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 11, 0, 0), true, new PartUnit(Part1, 15), new PartUnit(Part1, 5));
			var docket2a = CreateWhsReceive(Whs1, Org2, new ZDateTimeOffset(year, 2, 5, 11, 0, 0), true, new PartUnit(Part1, 50));
			var docket3 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 11, 0, 0), true, new PartUnit(Part2, 35));
			var docket4 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 10, 11, 0, 0), true, new PartUnit(Part1, 20));

			var docket5 = CreateFinalisedWhsOrder(Whs2, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var docket6 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 3, 11, 0, 0), new PartUnit(Part1, 10));
			var docket6a = CreateFinalisedWhsOrder(Whs1, Org2, new ZDateTimeOffset(year, 2, 7, 11, 0, 0), new PartUnit(Part1, 40));
			var docket7 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 15, 0, 0), new PartUnit(Part1, 10), new PartUnit(Part2, 15));
			var docket8 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 3, 1, 11, 0, 0), new PartUnit(Part2, 20));

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 2);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 9);
			AssertContainsExactElementsInAnyOrder(new[] { docket2, docket3 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));

			AssertContainsExactElementsInAnyOrder(new[] { docket6, docket7 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { docket2, docket3, docket4 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));

			AssertContainsExactElementsInAnyOrder(new[] { docket6, docket7, docket8 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { docket2, docket3, docket4 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));
			AssertContainsExactElementsInAnyOrder(new[] { docket6, docket7, docket8 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_WW = Whs2.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertContainsExactElementsInAnyOrder(new[] { docket1 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));
			AssertContainsExactElementsInAnyOrder(new[] { docket5 },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));

			invoice.ET_WW = ZGuid.Empty;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			AssertEquals(0, invoice.GetAdditionalDockets().Length);

			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			Helper.CreateJobHeader(docket6);
			docket6.JobHeader.JH_ExcludeFromPeriodicRating = true;
			AssertContainsExactElementsInAnyOrder(new[] { docket7, docket8 },
				invoice.GetAdditionalDocketsWithoutSetJobDefaults().Where(d => d.WD_DocketType == DocketType.Codes.Order));
			AssertEquals("JH_ExcludeFromPeriodicRating should not be set to default(false)", true, docket6.JobHeader.JH_ExcludeFromPeriodicRating);
		}

		#endregion

		#region DefaultStorageFromDate

		public void TestDefaultStorageFromDate()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			WhsInvoice invoice = factory2.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			AssertEquals(ZDateTime.Empty, invoice.ET_StorageFromDate);
			AssertEquals(false, invoice.ET_StorageFromDateInfo.ReadOnly);

			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			CreateWhsReceive(Whs1, Org2, new ZDateTimeOffset(year, 2, 9, 11, 0, 0), true, new PartUnit(Part1, 50));
			CreateWhsReceive(Whs1, Org2, new ZDateTimeOffset(year, 2, 5, 11, 0, 0), true, new PartUnit(Part1, 50));
			Factory.Save();

			invoice.ET_OH_Client = Org2.PK;
			AssertEquals(ZDateTime.Empty, invoice.ET_StorageFromDate);
			AssertEquals(false, invoice.ET_StorageFromDateInfo.ReadOnly);

			invoice.ET_WW = Whs1.PK;
			AssertEquals(new ZDateTime(year, 2, 5), invoice.ET_StorageFromDate);
			AssertEquals(true, invoice.ET_StorageFromDateInfo.ReadOnly);

			invoice.ET_StorageToDate = new ZDateTime(year, 2, 20);
			factory2.Save();

			WhsInvoice invoice2 = Factory.New<WhsInvoice>();
			invoice2.ET_OH_Client = Org2.PK;
			invoice2.ET_WW = Whs1.PK;
			AssertEquals(new ZDateTime(year, 2, 21), invoice2.ET_StorageFromDate);
		}

		public void TestDefaultStorageFromDate_DifferentWarehouses()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			WhsInvoice invoice = factory2.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			AssertEquals(ZDateTime.Empty, invoice.ET_StorageFromDate);

			CreateWhsReceive(Whs1, Org1, Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 2, 5, 11, 0, 0)), true, new PartUnit(Part1, 50));
			Factory.Save();

			invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			AssertEquals(new ZDateTime(year, 2, 5), invoice.ET_StorageFromDate);

			invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs2.PK;
			AssertZDatesWithin5Minutes("None found, so year ago date", ZDateTime.Today.AddYears(-1), invoice.ET_StorageFromDate);
			AssertEquals(false, invoice.ET_StorageFromDateInfo.ReadOnly);
		}

		#endregion

		#region TestDoesNotAccessPropertyOfDeletedJobHeader

		public void TestDoesNotAccessPropertyOfDeletedJobHeader()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, today, today.AddDays(6));
			invoice.JobHeader.Delete();

			Job header;
			AssertNoExceptionThrown(() => header = invoice.JobHeader);
		}

		#endregion

		#region TestChangePeriodicInvoiceStatusUpdateRelatedJobsStatus

		[TestDate(2011, 1, 1)]
		public void TestChangePeriodicInvoiceStatusUpdateRelatedJobsStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2010, 1, 1, 0, 0, 0), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2010, 1, 2, 0, 0, 0), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2010, 1, 1), new ZDateTime(2010, 1, 3));
			invoice.NotificationManager.Push(Notify); // so that we can control the Yes/No GUI interactions
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			// close the invoice
			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();
			invoice.JobHeader.Close(null, null);
			Factory.Save();

			string expectedLastMessageWhenClosing =
				"\r\n" +
				"The following jobs have been successfully updated:\r\n" +
				"\r\n" +
				"   W00000001\r\n" +
				"   W00000002\r\n";

			AssertEquals(expectedLastMessageWhenClosing, ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Message);
			AssertEquals("Precondition - ensure the Job was closed.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Closing the Periodic Invoice should have also closed the Receive's Invoice.", JobHeaderStatus.Closed.Code, receiveJobHeader.JH_Status);
			AssertEquals("Closing the Periodic Invoice should have also closed the Order's Invoice.", JobHeaderStatus.Closed.Code, orderJobHeader.JH_Status);

			// re-open the invoice
			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Notify.DefaultResponse = false; // to check that the user is not asked to update related jobs when the status is not 'Closed'
			Factory.Save();
			AssertEquals("Re-opening the Periodic Invoice should have also re-opened the Receive's Invoice.", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Re-opening the Periodic Invoice should have also re-opened the Order's Invoice.", JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);

			// jobs to update should be cleared when finished
			AssertNotNull("Precondition", Notify.LastQueryUserEventArgs);
			Notify.LastQueryUserEventArgs = null;
			invoice.ET_BillingDate = ZDateTime.Now; // just need a change so that OnFactorySaving is called..
			Factory.Save();
			AssertNull("Should be no notifications, JobsToUpdate might not have been cleared thus being reprocessed.", Notify.LastQueryUserEventArgs);

			// dialog result of 'No' to the update
			invoice.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Notify.DefaultResponse = false;
			Factory.Save();

			string expectedLastMessageWhenClosingAndUpdateDeclined =
				"\r\n" +
				"You are about to update the invoice status for the following related warehouse jobs, and reverse all related WIPs and ACRs as necessary:\r\n" +
				"\r\n" +
				"   W00000001\r\n" +
				"   W00000002\r\n" +
				"\r\n" +
				"Are you sure you want to continue?";

			AssertEquals(expectedLastMessageWhenClosingAndUpdateDeclined, ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Message);
			AssertEquals("Status should not be updated - dialog answer was 'No',", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Status should not be updated - dialog answer was 'No',", JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);
			Notify.DefaultResponse = true; // cleanup

			// profit / loss validation error message
			var profitLossReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = profitLossReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profitLossReasonCodes);

			var profitLossRequiringReasonParams = new JobProfitLossRequiringReasonParameters();
			profitLossRequiringReasonParams.ProfitThreshold = 10M;
			profitLossRequiringReasonParams.LossThreshold = 10M;
			profitLossRequiringReasonParams.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profitLossRequiringReasonParams);

			((Job)receive.JobHeader).Charges.AddNew().FillWithValidTestData();
			((Job)order.JobHeader).Charges.AddNew().FillWithValidTestData();

			invoice.JobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
			Factory.Save();

			string expectedLastMessageForProfitMarginErrors =
				"The following jobs require a Reason Code because their Profit Margin falls outside the tolerated margin threshold.\r\n" +
				"\r\n" +
				"Please assign a Job Profit / Loss Reason Code to these jobs and then manually update them.\r\n" +
				"\r\n" +
				"   W00000001\r\n" +
				"   W00000002\r\n";

			AssertEquals(expectedLastMessageForProfitMarginErrors, notify.LastEvent.Message);
			AssertEquals("Status should not have been updated as there were errors.", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Status should not have been updated as there were errors.", JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);
		}

		[TestDate(2011, 1, 1)]
		public void TestChangePeriodicInvoiceStatusUpdateRelatedJobsStatusWithoutJobHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2010, 1, 1, 0, 0, 0), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2010, 1, 2, 0, 0, 0), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2010, 1, 1), new ZDateTime(2010, 1, 3));
			invoice.NotificationManager.Push(Notify); // so that we can control the Yes/No GUI interactions
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			invoice.PostInvoice();

			// After WI00160704 it should not be possible to save JobHeader with empty JH_Status, but WhsInvoice.GetInvoiceStatus() may return empty value if WhsInvoice.GetJobHeader(this.PK) is empty.
			// Testing that code that updates related jobs does not crash in case of missing JobHeader.
			AssertNull("Precondition", invoice.JobHeader);

			Factory.Save();

			// Making sure that JobHeader was not created in the process. If something in future will create JobHeader during Save(), this test may become obsolete.
			AssertNull(invoice.JobHeader);

			AssertEquals(JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals(JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);
		}

		#endregion

		#region TestChangePeriodicInvoiceStatusViaJobStatusWithPromptlessRelatedJobUpdate

		[TestDate(2022, 12, 1)]
		public void TestChangePeriodicInvoiceStatusViaJobStatusWithPromptlessRelatedJobUpdate_WithJobHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2021, 12, 1, 0, 0, 0), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2021, 12, 2, 0, 0, 0), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2021, 12, 1), new ZDateTime(2021, 12, 3));
			invoice.NotificationManager.Push(Notify);
			Notify.DefaultResponse = false;
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();

			// profit margin of receive job falls outside the tolerated margin threshold
			var profitLossReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = profitLossReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profitLossReasonCodes);

			var profitLossRequiringReasonParams = new JobProfitLossRequiringReasonParameters();
			profitLossRequiringReasonParams.ProfitThreshold = 10m;
			profitLossRequiringReasonParams.LossThreshold = 10m;
			profitLossRequiringReasonParams.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, profitLossRequiringReasonParams);

			((Job)receive.JobHeader).Charges.AddNew().FillWithValidTestData();

			invoice.JobHeader.JH_ProfitLossReasonCode = "INV";
			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.JobInvoiced.Code;
			AssertEquals("VerifyJobHeaderNotificationErrors is set to true.", true, invoice.VerifyJobHeaderNotificationErrors);
			AssertHasErrors("VerifyJobHeaderNotification should contain job header's notification errors.", invoice.VerifyJobHeaderNotificationErrorsInfo);

			invoice.JobHeader.JH_ProfitLossReasonCode = "TST";
			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.JobInvoiced.Code;
			AssertEquals("VerifyJobHeaderNotificationErrors is set to true.", true, invoice.VerifyJobHeaderNotificationErrors);
			Factory.Save();

			AssertEquals("The job is invoiced.", JobHeaderStatus.JobInvoiced.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Status should not have been updated as profit margin falls outside the tolerated margin threshold.", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Status should have been updated.", JobHeaderStatus.JobInvoiced.Code, orderJobHeader.JH_Status);
			AssertEquals("JobStatusWithPromptlessRelatedJobUpdate is reset to empty.", ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals("VerifyJobHeaderNotificationErrors is reset to false.", false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(false, invoice.JobStatusWithPromptlessRelatedJobUpdateInfo.ReadOnly);

			// change invoice status to working
			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Working.Code;
			AssertEquals("VerifyJobHeaderNotificationErrors is set to true.", true, invoice.VerifyJobHeaderNotificationErrors);
			Factory.Save();
			AssertEquals("The job is re-opened.", JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Re-opening the Periodic Invoice should have also re-opened the Receive's Invoice.", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Re-opening the Periodic Invoice should have also re-opened the Order's Invoice.", JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);
			AssertEquals("JobStatusWithPromptlessRelatedJobUpdate is reset to empty.", ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals("VerifyJobHeaderNotificationErrors is reset to false.", false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(false, invoice.JobStatusWithPromptlessRelatedJobUpdateInfo.ReadOnly);

			// close the invoice
			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
			AssertEquals("VerifyJobHeaderNotificationErrors is set to true.", true, invoice.VerifyJobHeaderNotificationErrors);
			Factory.Save();

			AssertEquals("The job is closed.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Closing the Periodic Invoice should have also closed the Receive's Invoice.", JobHeaderStatus.Closed.Code, receiveJobHeader.JH_Status);
			AssertEquals("Closing the Periodic Invoice should have also closed the Order's Invoice.", JobHeaderStatus.Closed.Code, orderJobHeader.JH_Status);
			AssertEquals("JobStatusWithPromptlessRelatedJobUpdate is reset to empty.", ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals("VerifyJobHeaderNotificationErrors is reset to false.", false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(true, invoice.JobStatusWithPromptlessRelatedJobUpdateInfo.ReadOnly);
		}

		[TestDate(2022, 12, 1)]
		public void TestChangePeriodicInvoiceStatus_FirstViaJobStatusWithPromptlessRelatedJobUpdate_ThenViaJH_Status()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2021, 12, 1, 0, 0, 0), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2021, 12, 2, 0, 0, 0), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2021, 12, 1), new ZDateTime(2021, 12, 3));
			invoice.NotificationManager.Push(Notify);
			Notify.DefaultResponse = false;
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();
			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.JobInvoiced.Code;
			AssertEquals("VerifyJobHeaderNotificationErrors is set to true.", true, invoice.VerifyJobHeaderNotificationErrors);
			Factory.Save();

			AssertEquals("The job is invoiced.", JobHeaderStatus.JobInvoiced.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Receive order is invoiced.", JobHeaderStatus.JobInvoiced.Code, receiveJobHeader.JH_Status);
			AssertEquals("Order is invoiced.", JobHeaderStatus.JobInvoiced.Code, orderJobHeader.JH_Status);
			AssertEquals("JobStatusWithPromptlessRelatedJobUpdate is reset to empty.", ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals("VerifyJobHeaderNotificationErrors is reset to false.", false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(false, invoice.JobStatusWithPromptlessRelatedJobUpdateInfo.ReadOnly);

			// close the invoice
			invoice.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			string expectedLastMessageWhenClosingAndUpdateDeclined =
				"\r\n" +
				"You are about to update the invoice status for the following related warehouse jobs, and reverse all related WIPs and ACRs as necessary:\r\n" +
				"\r\n" +
				"   W00000001\r\n" +
				"   W00000002\r\n" +
				"\r\n" +
				"Are you sure you want to continue?";

			AssertEquals(expectedLastMessageWhenClosingAndUpdateDeclined, ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Message);
			AssertEquals("The job is closed.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Status should not be updated - dialog answer was 'No'.", JobHeaderStatus.JobInvoiced.Code, receiveJobHeader.JH_Status);
			AssertEquals("Status should not be updated - dialog answer was 'No'.", JobHeaderStatus.JobInvoiced.Code, orderJobHeader.JH_Status);
			AssertEquals("JobStatusWithPromptlessRelatedJobUpdate remains empty.", ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals("VerifyJobHeaderNotificationErrors remains false.", false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(true, invoice.JobStatusWithPromptlessRelatedJobUpdateInfo.ReadOnly);
		}

		[TestDate(2022, 12, 1)]
		public void TestChangePeriodicInvoiceStatusViaJobStatusWithPromptlessRelatedJobUpdate_WithoutJobHeader()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2021, 12, 1, 0, 0, 0), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2021, 12, 2, 0, 0, 0), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2021, 12, 1), new ZDateTime(2021, 12, 3));
			invoice.NotificationManager.Push(Notify);
			Notify.DefaultResponse = false;
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			invoice.PostInvoice();

			AssertNull("Precondition", invoice.JobHeader);
			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
			AssertEquals("VerifyJobHeaderNotificationErrors remains false.", false, invoice.VerifyJobHeaderNotificationErrors);
			Factory.Save();

			AssertEquals(string.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals("VerifyJobHeaderNotificationErrors remains false.", false, invoice.VerifyJobHeaderNotificationErrors);
			AssertNull(invoice.JobHeader);
			AssertEquals(JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals(JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);
		}

		#endregion

		#region TestJobsToUpdate_OneWayUpdate

		public void TestJobsToUpdate_OneWayUpdate_InDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2018, 1, 1), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2018, 1, 2), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2018, 1, 1), new ZDateTime(2018, 1, 3));
			invoice.AutoRateJobHeader(null);
			Factory.Save();
			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			Factory.Save();

			AssertEquals("Precondition", JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);

			invoice.JobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
			Factory.Save();
			AssertEquals("Invoice should update related jobs.", JobHeaderStatus.JobInvoiced.Code, receiveJobHeader.JH_Status);
			AssertEquals("Invoice should update related jobs.", JobHeaderStatus.JobInvoiced.Code, orderJobHeader.JH_Status);

			receiveJobHeader.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
			Factory.Save();
			AssertEquals("Related jobs should not update Invoice.", JobHeaderStatus.JobInvoiced.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Receive status should not back to job status.", JobHeaderStatus.InvoiceOnHold.Code, receiveJobHeader.JH_Status);
			AssertEquals("Order status should not change.", JobHeaderStatus.JobInvoiced.Code, orderJobHeader.JH_Status);

			orderJobHeader.JH_Status = JobHeaderStatus.ScheduledForArchive.Code;
			Factory.Save();
			AssertEquals("Related jobs should not update Invoice.", JobHeaderStatus.JobInvoiced.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Receive status should not change.", JobHeaderStatus.InvoiceOnHold.Code, receiveJobHeader.JH_Status);
			AssertEquals("Order status should not back to job status.", JobHeaderStatus.ScheduledForArchive.Code, orderJobHeader.JH_Status);
		}

		public void TestJobsToUpdate_OneWayUpdate_InMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = CreateWhsReceive(data.Whs1, data.Org1, new ZDateTimeOffset(2018, 1, 1), true, new PartUnit(data.Part1, 10));
			var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, new ZDateTimeOffset(2018, 1, 2), new PartUnit(data.Part1, 5));

			var receiveJobHeader = AddJobHeader(receive);
			var orderJobHeader = AddJobHeader(order);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			orderJobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2018, 1, 1), new ZDateTime(2018, 1, 3));
			Factory.Save();
			invoice.AutoRateJobHeader(null);

			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, order }, invoice.GetAdditionalDockets());

			AssertEquals("Precondition", JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, receiveJobHeader.JH_Status);
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, orderJobHeader.JH_Status);
			Assert("Precondition", !invoice.JobHeader.IsInDatabase);

			invoice.JobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
			Factory.Save();
			AssertEquals("Invoice should update ralted jobs.", JobHeaderStatus.JobInvoiced.Code, receiveJobHeader.JH_Status);
			AssertEquals("Invoice should update ralted jobs.", JobHeaderStatus.JobInvoiced.Code, orderJobHeader.JH_Status);
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			AssertEquals(invoice, invoice.DocManagerInfo.BusinessEntity);
			AssertEquals("WIV", invoice.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestAuditSecurity

		public void TestAuditSecurity()
		{
			AssertEquals("AuditSecurity", Env.Security.WhsInvoicingAuditBilling, ((IJobInvoicingPlugIn)Factory.New<WhsInvoice>()).InvoicingSupporter.AuditSecurity);
		}

		#endregion

		#region TestReadOnlyWhenContainsChargePostedFromThisJob

		[TestDate(2012, 6, 1)]
		public void TestReadOnlyWhenContainsChargePostedFromThisJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;
			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			// Create Warehouse Charge Codes.
			var receiveHandling = Helper.CreateChargeCode("WRECHAN", "", ChargeCodeGroupList.Codes.WHSInwards, "");
			receiveHandling.AC_AG_WIPAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_DisallowDirectPosting, false)).PK;
			Helper.CreateRateLine(warehouseRate, receiveHandling, "UNT", 5m);
			Factory.Save();

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive1.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive1);

			//Posting the Receive
			var receiveJobHeader = Helper.CreateJobHeaderAndCharge(receive1);
			receiveJobHeader.Charges[0].JR_SellRatingOverride = false;

			new AutoRatingStarter(receive1, new LoggerDecorator()).ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);

			PostJob(receiveJobHeader);
			AssertEquals("Charge should be posted.", true, receiveJobHeader.Charges[0].IsRevenuePosted);

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();
			AssertInvoiceFormFieldsReadOnly("Should only be Read Only when theres a line which was posted from the invoice", invoice, false);

			Helper.CreateJobCharges(invoice.JobHeader);
			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();

			AssertInvoiceFormFieldsReadOnly("", invoice, true);
			AssertEquals(true, invoice.ET_StorageFromDateInfo.ReadOnly);
		}

		public void TestReadOnlyWhenContainsChargePostedFromThisJob_CallsRefreshBindings()
		{
			var storageToDateInfoChanged = false;
			var storageFromDateInfoChanged = false;
			var warehouseInfoChanged = false;
			var billingDateInfoChanged = false;
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_StorageToDateInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				storageToDateInfoChanged = true;
			};
			invoice.ET_StorageFromDateInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				storageFromDateInfoChanged = true;
			};
			invoice.ET_WWInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				warehouseInfoChanged = true;
			};
			invoice.ET_StorageToDateInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				billingDateInfoChanged = true;
			};
			invoice.InvoicingSupporter.PostedStateChanged();
			Assert(storageToDateInfoChanged);
			Assert(storageFromDateInfoChanged);
			Assert(warehouseInfoChanged);
			Assert(billingDateInfoChanged);
		}

		void AssertInvoiceFormFieldsReadOnly(ZString message, WhsInvoice invoice, bool readOnly)
		{
			AssertEquals(message, readOnly, invoice.ET_StorageToDateInfo.ReadOnly);
			AssertEquals(message, readOnly, invoice.ET_WWInfo.ReadOnly);
			AssertEquals(message, readOnly, invoice.ET_BillingDateInfo.ReadOnly);
		}

		void PostJob(Job jobHeader)
		{
			var postManager = new InvoicingPostManager(jobHeader);
			Factory.Save();
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
		}

		#endregion

		#region Properties

		#region Include in Invoicing

		public void TestIncludeInInvoicing()
		{
			WhsInvoice invoice = Factory.NewWithValidTestData<WhsInvoice>();

			AssertEquals(true, invoice.IncludeInInvoicing);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			invoice.IncludeInInvoicing = false;
			AssertEquals(false, invoice.IncludeInInvoicing);
			AssertNoErrors(invoice.ET_StorageToDateInfo);

			using (invoice.GetValidationSuspender())
			{
				invoice.IncludeInInvoicing = true;
				AssertEquals(true, invoice.IncludeInInvoicing);
				AssertNoErrors(invoice.ET_StorageToDateInfo);

				invoice.ET_StorageToDate = ZDateTime.Today.AddDays(1);
			}
			invoice.IncludeInInvoicing = true;
			AssertNoErrors(invoice.ET_StorageToDateInfo);
		}

		#endregion

		#region JobStatusWithPromptlessRelatedJobUpdate

		public void TestJobStatusWithPromptlessRelatedJobUpdate()
		{
			var invoice = Factory.New<WhsInvoice>();

			AssertNull(invoice.JobHeader);
			AssertEquals(ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			AssertEquals(false, invoice.VerifyJobHeaderNotificationErrors);

			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
			AssertNull(invoice.JobHeader);
			AssertEquals(false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(JobHeaderStatus.Closed.Code, invoice.JobStatusWithPromptlessRelatedJobUpdate);

			invoice.AutoRateJobHeader(null);

			AssertEquals(JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals(false, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(JobHeaderStatus.Closed.Code, invoice.JobStatusWithPromptlessRelatedJobUpdate);

			invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
			AssertEquals(JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals(true, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(JobHeaderStatus.Closed.Code, invoice.JobStatusWithPromptlessRelatedJobUpdate);

			invoice.JobStatusWithPromptlessRelatedJobUpdate = ZString.Empty;
			AssertEquals(JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals(true, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);

			invoice.JobStatusWithPromptlessRelatedJobUpdate = null;
			AssertEquals(JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals(true, invoice.VerifyJobHeaderNotificationErrors);
			AssertEquals(ZString.Empty, invoice.JobStatusWithPromptlessRelatedJobUpdate);
			DeleteJobHeadersWithoutAnyCharges(invoice);
		}

		void DeleteJobHeadersWithoutAnyCharges(WhsInvoice invoice)
		{
			if (invoice.JobHeader != null && !invoice.JobHeader.IsInDatabase && !(invoice.JobHeader.Charges.Count > 0))
			{
				invoice.JobHeader.Delete();
			}
		}

		#endregion

		#region VerifyJobHeaderNotificationErrors

		public void TestVerifyJobHeaderNotificationErrors()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertEquals(false, invoice.VerifyJobHeaderNotificationErrors);

			invoice.VerifyJobHeaderNotificationErrors = true;
			AssertEquals(true, invoice.VerifyJobHeaderNotificationErrors);

			invoice.VerifyJobHeaderNotificationErrors = false;
			AssertEquals(false, invoice.VerifyJobHeaderNotificationErrors);
		}

		#endregion

		#region CustomReadOnly

		public void TestCustomReadOnly()
		{
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			AssertEquals("Precondition: ", false, invoice.CustomReadOnly);
			invoice.CustomReadOnly = true;
			AssertEquals(true, invoice.CustomReadOnly);
		}

		#endregion

		#endregion

		#region AutoRateJobHeader

		#region TestAutoRateJobHeader_RateServices

		[TestDate(2012, 6, 1)]
		public void TestAutoRateJobHeader_RateServices()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			// Create Warehouse Charge Codes.
			var receiveHandlingFumigationCharge = Helper.CreateChargeCode("WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m); // charge by service count.

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = ZDateTime.Today;

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Only 1 charge should be created.", 1, (receive.JobHeader as Job).Charges.Count);
			AssertEquals("Service should be charged for correct count.", 30m, (receive.JobHeader as Job).Charges[0].JobChargeAttrib_ItemsToRate);
			invoice.JobHeader.Charges.RemoveAndDeleteAll(); // clean up

			rateLine.TL_WeightVolume = "UNT"; // rating by units.
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Only 2 charges should be created, one for each product.", 2, (receive.JobHeader as Job).Charges.Count);
			invoice.JobHeader.Charges.Sort("JobChargeAttrib_ItemsToRate");
			AssertEquals("Service should be charged for correct count.", 10m, (receive.JobHeader as Job).Charges[0].JobChargeAttrib_ItemsToRate);
			AssertEquals("Service should be charged for correct count.", 15m, (receive.JobHeader as Job).Charges[1].JobChargeAttrib_ItemsToRate);
		}

		#endregion

		#region TestAutoRateJobHeader_RateServices_AdHocSerivceJobs

		public void TestAutoRateJobHeader_RateServices_AdHocSerivceJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			// Create Warehouse Charge Codes.
			var adHocServiceJobFumigationCharge = Helper.CreateChargeCode("WAHFUM", "Warehouse Ad Hoc Service Job Fumigation Charge", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, Constants.FreightServiceType.Codes.Fumigation);

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			var warehouseAdHocServiceJobRate = Helper.CreateRateEntry(clientRate, new ZDate(2016, 1, 1), new ZDate(2017, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseAdHocServiceJobRate, adHocServiceJobFumigationCharge, "SV", 5m);

			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDate(2016, 2, 1), "WI0001", true);
			var fumigationService = adHocServiceJob.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 3m;
			fumigationService.ES_Completed = new ZDate(2016, 2, 1);
			AssertEquals("Precondition: Adhoc Service Job should be Finalised.", true, adHocServiceJob.IsFinalised);

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2016, 2, 1), new ZDateTime(2016, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Only 1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Service should be charged for correct count.", 3m, invoice.JobHeader.Charges[0].JobChargeAttrib_ItemsToRate);
			invoice.JobHeader.Charges.RemoveAndDeleteAll(); // clean up
		}

		#endregion

		#region TestAutoRateJobHeader_LocaitonPalletRatesAreNotInterferingWithPkgRates

		[TestDate(2012, 6, 1)]
		public void TestAutoRateJobHeader_LocaitonPalletRatesAreNotInterferingWithPkgRates()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			// Create Warehouse Charge Codes.
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			var rateLine1 = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 4m); // charge by Units.
			var rateLine2 = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "PL", 7m); // charge by Location Pallet. Only for Product2!
			rateLine2.TL_OP_ProductNumber = data.Part2.PK;

			// Create Receive for Warehouse Storage charges. Only receiving Product1!
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Only 1 charges should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Storage - P1 (P1) for 7 days (01-Feb-11 - 07-Feb-11)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Sell Amount is incorrect.", 40m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRate_IncludesPositiveAdjustments / TestAutoRate_IncludesPositiveAndNegativeInternalAdjustments

		[TestDate(2012, 6, 1)]
		public void TestAutoRate_IncludesPositiveAdjustments()
		{
			// Normal adjustments with negative qty should be ignored in autorating
			TestAutoRate_IncludesPositiveAndNegativeInternalAdjustmentsCore(AdjustmentType.Codes.Adjustment, 56m, "Cost should be 8 * 7 = 56.");
		}

		[TestDate(2012, 6, 1)]
		public void TestAutoRate_IncludesPositiveAndNegativeInternalAdjustments()
		{
			// Internal warehouse adjustments with negative qty should be counted in autorating
			TestAutoRate_IncludesPositiveAndNegativeInternalAdjustmentsCore(AdjustmentType.Codes.InternalWarehouseAdjustment, 21m, "Cost should be (8-5) * 7 = 21.");
		}

		void TestAutoRate_IncludesPositiveAndNegativeInternalAdjustmentsCore(string adjustmentTypeToCreate, decimal expectedStorageChargeAmount, string expectedStorageChargeAmountExplanation)
		{
			// Internal warehouse adjustments with negative qty should be counted in autorating

			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Daily;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;

			var warehouseStorageCharge = Helper.CreateChargeCode("XXX", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;
			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 7m, UnitCalculator.Code);

			// ADJ +8
			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "adj123", Notify);
			var adjustmentLineIn = Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 8m, data.Whs1.FindLocation("A"));
			adjustmentIn.WD_DocketSubType = adjustmentTypeToCreate;
			adjustmentIn.FinaliseDocket();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(2011, 2, 5);
			adjustmentLineIn.WE_AdjustmentArrivalDate = adjustmentIn.WD_FinalisedDate;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentIn);

			// ADJ -5
			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "adj234", Notify);
			var adjustmentLineOut = Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -5m, data.Whs1.FindLocation("A"));
			adjustmentOut.WD_DocketSubType = adjustmentTypeToCreate;
			adjustmentOut.FinaliseDocket();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(2011, 2, 5);
			adjustmentLineOut.WE_AdjustmentArrivalDate = adjustmentOut.WD_FinalisedDate;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentOut);

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 5), new ZDateTime(2011, 2, 5));
			Factory.Save();
			invoice.AutoRateJobHeader(null);

			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name); // to avoid intermidient test failures.
			AssertEquals("1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals(expectedStorageChargeAmountExplanation, expectedStorageChargeAmount, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region Storage for InternalAdjustmentsForDifferentStorageCalculations

		[TestDate(2012, 6, 1)]
		public void TestAutoRate_InternalAdjustmentsForWarehouseStorageMax()
		{
			AutoRate_InternalAdjustmentsForDifferentStorageCalculationsCore(OrgCompanyDataLookups.WarehouseStorageMax);
		}

		[TestDate(2012, 6, 1)]
		public void TestAutoRate_InternalAdjustmentsForWarehouseStorageClosingBalance()
		{
			AutoRate_InternalAdjustmentsForDifferentStorageCalculationsCore(OrgCompanyDataLookups.WarehouseStorageClosingBalance);
		}

		[TestDate(2012, 6, 1)]
		public void TestAutoRate_InternalAdjustmentsForWarehouseSplitPeriodBilling()
		{
			AutoRate_InternalAdjustmentsForDifferentStorageCalculationsCore(OrgCompanyDataLookups.WarehouseSplitPeriodBilling);
		}

		[TestDate(2012, 6, 1)]
		public void TestAutoRate_InternalAdjustmentsForWarehouseStoragePeak()
		{
			AutoRate_InternalAdjustmentsForDifferentStorageCalculationsCore(OrgCompanyDataLookups.WarehouseStoragePeak);
		}

		void AutoRate_InternalAdjustmentsForDifferentStorageCalculationsCore(string storageCalcMethod)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = storageCalcMethod;

			var warehouseStorageCharge = Helper.CreateChargeCode("XXX", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;
			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 7m, UnitCalculator.Code);

			//         |                   |
			// +12(REC)|      +5 (ADJ)     |
			//-┴-------|-------┼-----------|---
			//         |      -5 (ADJ)     |
			//  JAN    |        FEB        |

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 12);
			receive.WD_FinalisedDate = new ZDateTimeOffset(2011, 1, 1);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, data.Whs1.FindLocation("A"));
			var adjustmentLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, data.Whs1.FindLocation("A"));
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(2011, 2, 13);
			adjustmentLineOut.WE_AdjustmentArrivalDate = receive.Lines[0].WE_AdjustmentArrivalDate;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 28));
			Factory.Save();
			invoice.AutoRateJobHeader(null);

			AssertEquals("1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Adjustment for +5 and -5 should compensate each other, so amount must equals to opening balance 12 * 7 = 84", 84m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region Storage for InternalAdjustments AdjustmentArrivalDate

		[TestDate(2024, 6, 1)]
		public void TestAutoRate_InternalAdjustments_AdjustmentArrivalDate_WarehouseStorageMax()
		{
			TestAutoRate_InternalAdjustments_AdjustmentArrivalDateCore(OrgCompanyDataLookups.WarehouseStorageMax);
		}

		[TestDate(2024, 6, 1)]
		public void TestAutoRate_InternalAdjustments_AdjustmentArrivalDate_WarehouseStorageClosingBalance()
		{
			TestAutoRate_InternalAdjustments_AdjustmentArrivalDateCore(OrgCompanyDataLookups.WarehouseStorageClosingBalance);
		}

		[TestDate(2024, 6, 1)]
		public void TestAutoRate_InternalAdjustments_AdjustmentArrivalDate_WarehouseSplitPeriodBilling()
		{
			TestAutoRate_InternalAdjustments_AdjustmentArrivalDateCore(OrgCompanyDataLookups.WarehouseSplitPeriodBilling);
		}

		[TestDate(2024, 6, 1)]
		public void TestAutoRate_InternalAdjustments_AdjustmentArrivalDate_WarehouseStoragePeak()
		{
			TestAutoRate_InternalAdjustments_AdjustmentArrivalDateCore(OrgCompanyDataLookups.WarehouseStoragePeak);
		}

		void TestAutoRate_InternalAdjustments_AdjustmentArrivalDateCore(string storageCalcMethod)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = storageCalcMethod;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var warehouseStorageCharge = Helper.CreateChargeCode("XXX", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;
			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2022, 1, 1), new ZDate(2025, 1, 1));
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 7m, UnitCalculator.Code);

			//           |             |
			// +12(REC)  |  +5 (ADJ)   |
			//-┴---------|-------------|---
			// -5 (ADJ)  |             |
			//  JAN      |    FEB      |

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2023, 1, 1), data.Part1, 12);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ", Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			var adjustmentLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, data.Whs1.FindLocation("A"));
			var adjustmentLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, data.Whs1.FindLocation("A"));
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(2023, 2, 13);
			adjustmentLineIn.WE_AdjustmentArrivalDate = adjustment.WD_FinalisedDate;
			adjustmentLineOut.WE_AdjustmentArrivalDate = receive.Lines[0].WE_AdjustmentArrivalDate;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2023, 2, 1), new ZDateTime(2023, 2, 28));
			Factory.Save();
			invoice.AutoRateJobHeader(null);

			AssertEquals("1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Adjustment for +5 and -5 should compensate each other, so amount must equals to opening balance 12 * 7 = 84", 84m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRateJobHeader_FlatCalculatorReturnsDocketReference

		[TestDate(2011, 6, 1)]
		public void TestAutoRateJobHeader_FlatCalculatorReturnsDocketReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			// Create Warehouse Charge Codes.
			var warehouseReceiveHandlingCharge = Helper.CreateChargeCode("WREC", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			var warehouseOutwardCharge = Helper.CreateChargeCode("WOUT", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			var rateLine1 = Helper.CreateRateLine(warehouseRate, warehouseReceiveHandlingCharge, "", 4m, FlatCalculator.Code);
			var rateLine2 = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "", 7m, FlatCalculator.Code);
			var rateLine3 = Helper.CreateRateLine(warehouseRate, warehouseOutwardCharge, "", 12m, FlatCalculator.Code);

			// Create Receive for Warehouse Receive Handling and Storage charges.
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			// Create Order for Warehouse Outwards Handling charge
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			// ensuring jobs are finalised in suitable time for Periodic Billing.
			receive1.WD_FinalisedDate = new ZDateTimeOffset(2011, 1, 31);
			receive2.WD_FinalisedDate = new ZDateTimeOffset(2011, 2, 3);
			order.WD_FinalisedDate = new ZDateTimeOffset(2011, 2, 5);

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			Factory.Save();
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name); // to avoid intermidient test failures.
			AssertEquals("3 charges should be created.", 3, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Receive Handling R2", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Receive Handling sell Amount is incorrect.", 4m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
			AssertEquals("Receive Handling rates should have Docket reference.", "R2", invoice.JobHeader.Charges[0].JobChargeAttrib_DocketReference);

			AssertEquals("Warehouse Storage for 7 days (01-Feb-11 - 07-Feb-11)", invoice.JobHeader.Charges[1].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect.", 7m, invoice.JobHeader.Charges[1].JR_LocalSellAmt);
			AssertEquals("Warehouse Storage rates should never have Docket reference.", "", invoice.JobHeader.Charges[1].JobChargeAttrib_DocketReference);

			AssertEquals("Warehouse Outwards Handling TEST", invoice.JobHeader.Charges[2].JR_Desc);
			AssertEquals("Warehouse Outwards Handling sell Amount is incorrect.", 12m, invoice.JobHeader.Charges[2].JR_LocalSellAmt);
			AssertEquals("Warehouse Outwards Handling rates should have Docket reference.", "TEST", invoice.JobHeader.Charges[2].JobChargeAttrib_DocketReference);
		}

		#endregion

		#region TestAutoRateJobHeader_WarehouseLocationTypeCalculator

		[TestDate(2012, 6, 1)]
		public void TestAutoRateJobHeader_WarehouseLocationTypeCalculator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.OH_IsDebtor = true;

			// Create Warehouse Charge Code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Line
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2011, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = warehouseRate.AddRateLine(warehouseStorageCharge.AC_Code, WarehouseLocationTypeCalculator.Code, "");
			((WarehouseLocationTypeCalculator)rateLine.Calculator).AddRateLineItem("RNO", 0m, 3m);

			// Create data to rate.
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 1, 3), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 1, 1), new ZDateTime(2012, 1, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("At the time of rating all stock was in A-1, so it should be the only location to rate.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Storage for 7 days (01-Jan-12 - 07-Jan-12)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect.", 3m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRateJobHeader_HaveSoftLinkToOriginalJob

		public void TestAutoRateJobHeader_HaveSoftLinkToOriginalJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDateTime.Today;
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			// Create Warehouse Charge Codes.
			var receiveHandlingFumigationCharge = Helper.CreateChargeCode("WREC", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(today.Year - 1, 1, 1), new ZDate(today.Year, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "UNT", 5m);

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(today.Year - 1, 2, 3), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.CreateJobHeader(receive);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(today.Year - 1, 2, 1), new ZDateTime(today.Year - 1, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Only 1 charges should be created.", 1, (receive.JobHeader as Job).Charges.Count);
			AssertEquals("Receive should be charged for correct amount (10 x 5).", 50m, (receive.JobHeader as Job).Charges[0].JR_LocalSellAmt);
			AssertEquals("Job Charge should have soft link to original job.", receive.WD_DocketID, (receive.JobHeader as Job).Charges[0].JR_OrderReference);
		}

		#endregion

		public void TestAutoRateJobHeader_NotFinalisedReceivesWithFinalisedPutawayTransfer()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.OH_IsDebtor = true;

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			// create client rate.
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var warehouseRate = Helper.CreateRateEntry(clientRate, today.Date.AddDays(-50), today.Date.AddDays(50));
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 7m, UnitCalculator.Code);

			// create inventory
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transferLine.FinaliseDocketLine();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, today.AddDays(-10), today.AddDays(-4));
			Helper.AutoRateJob(invoice);
			Factory.Save();
			AssertEquals("No charges should be created.", 0, invoice.JobHeader.Charges.Count);
		}

		#endregion

		#region PostInvoice

		[TestDate(2011, 1, 20)]
		public void TestPostInvoice_BillingAndPostDates()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			OrgHeader client2 = Helper.CreateClient("CLIENT2");
			OrgHeader client3 = Helper.CreateClient("CLIENT3");
			client2.OH_IsDebtor = true;
			client3.OH_IsDebtor = true;

			AccChargeCode orderHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");

			var today = new ZDateTime(2011, 1, 20);
			var invoiceDate = new ZDateTime(2011, 1, 15);

			WhsInvoice invoice1 = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 7));
			WhsInvoice invoice2 = Helper.CreateInvoiceWithJobHeader(client2, data.Whs1, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 7));
			WhsInvoice invoice3 = Helper.CreateInvoiceWithJobHeader(client3, data.Whs1, new ZDateTime(2011, 1, 1), new ZDateTime(2011, 1, 7));
			invoice1.ET_BillingDate = invoiceDate;
			invoice2.ET_BillingDate = invoiceDate;
			invoice3.ET_BillingDate = invoiceDate;

			Helper.CreateJobCharges(invoice1.JobHeader);
			Helper.CreateJobCharges(invoice2.JobHeader);
			Helper.CreateJobCharges(invoice3.JobHeader);

			Factory.Save();

			AssertEquals("Precondition:", false, AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.OverridePostDate);
			AssertEquals("Precondition:", false, AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.DefaultPostDateFromInvoiceDate);

			invoice1.PostInvoice();
			Factory.Save();
			invoice1.JobHeader.PrintingFilter.RefreshInvoiceList();
			AssertEquals("Charge should be posted.", true, invoice1.JobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("1 Posted transaction should be created.", 1, invoice1.JobHeader.PrintingFilter.Transactions.Count);
			AssertEquals("Incorrect Billing Date.", invoiceDate, invoice1.JobHeader.PrintingFilter.Transactions[0].AH_InvoiceDate);
			AssertEquals("Incorrect Post Date.", today, invoice1.JobHeader.PrintingFilter.Transactions[0].AH_PostDate);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration() { OverridePostDate = true, DefaultPostDateFromInvoiceDate = false });
			invoice2.PostInvoice();
			Factory.Save();
			invoice2.JobHeader.PrintingFilter.RefreshInvoiceList();
			AssertEquals("Charge should be posted.", true, invoice2.JobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("1 Posted transaction should be created.", 1, invoice2.JobHeader.PrintingFilter.Transactions.Count);
			AssertEquals("Incorrect Invoice Date.", invoiceDate, invoice2.JobHeader.PrintingFilter.Transactions[0].AH_InvoiceDate);
			AssertEquals("Incorrect Post Date.", today, invoice2.JobHeader.PrintingFilter.Transactions[0].AH_PostDate);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new BackDateInvoicesConfiguration() { OverridePostDate = true, DefaultPostDateFromInvoiceDate = true });
			invoice3.PostInvoice();
			Factory.Save();
			invoice3.JobHeader.PrintingFilter.RefreshInvoiceList();
			AssertEquals("Charge should be posted.", true, invoice3.JobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("1 Posted transaction should be created.", 1, invoice3.JobHeader.PrintingFilter.Transactions.Count);
			AssertEquals("Incorrect Invoice Date.", invoiceDate, invoice3.JobHeader.PrintingFilter.Transactions[0].AH_InvoiceDate);
			AssertEquals("Incorrect Post Date.", invoiceDate, invoice3.JobHeader.PrintingFilter.Transactions[0].AH_PostDate);
		}

		#endregion

		#region TestPostInvoiceUpdatesRelatedJobStatuses

		[TestDate(2011, 6, 1)]
		public void TestPostInvoiceUpdatesRelatedJobStatuses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			// Create Warehouse Charge Codes.
			var warehouseReceiveHandlingCharge = Helper.CreateChargeCode("WREC", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			var rateLine1 = Helper.CreateRateLine(warehouseRate, warehouseReceiveHandlingCharge, "", 4m, FlatCalculator.Code);

			// Create Receive for Warehouse Receive Handling and Storage charges.
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);

			// ensuring jobs are finalised in suitable time for Periodic Billing.
			receive2.WD_FinalisedDate = new ZDateTimeOffset(2011, 2, 3);

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			receive2.JobHeader.JH_ExcludeFromPeriodicRating = true;
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name); // to avoid intermidient test failures.
			AssertEquals("Should be one charge", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Receive Handling R2", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Receive Handling sell Amount is incorrect.", 4m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
			AssertEquals("Receive Handling rates should have Docket reference.", "R2", invoice.JobHeader.Charges[0].JobChargeAttrib_DocketReference);

			Factory.Save();
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, receive2.JobHeader.JH_Status);

			invoice.JobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
			Factory.Save();
			AssertEquals("Job should still change to invoiced if periodic changes", JobHeaderStatus.JobInvoiced.Code, invoice.JobHeader.JH_Status);
			AssertEquals(JobHeaderStatus.JobInvoiced.Code, receive2.JobHeader.JH_Status);

			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();
			AssertEquals(JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Jobs should not change status if excluded from periodic", JobHeaderStatus.JobInvoiced.Code, receive2.JobHeader.JH_Status);
		}

		#endregion

		#region TestPostInvoiceUpdatesRelatedJobStatuses_AdHocServiceJobs

		public void TestPostInvoiceUpdatesRelatedJobStatuses_AdHocServiceJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			// Create Warehouse Charge Codes.
			var adHocServiceJobFumigationCharge = Helper.CreateChargeCode("WAHFUM", "Warehouse Ad Hoc Service Job Fumigation Charge", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, Constants.FreightServiceType.Codes.Fumigation);

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			var warehouseAdHocServiceJobRate = Helper.CreateRateEntry(clientRate, new ZDate(2016, 1, 1), new ZDate(2017, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseAdHocServiceJobRate, adHocServiceJobFumigationCharge, "SV", 5m);

			var adHocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDate(2016, 2, 1), "WI0001", true);
			var fumigationService = adHocServiceJob.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 3m;
			fumigationService.ES_Completed = new ZDate(2016, 2, 1);
			AssertEquals("Precondition: Adhoc Service Job should be Finalised.", true, adHocServiceJob.IsFinalised);

			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2016, 2, 1), new ZDateTime(2016, 2, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Only 1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Service should be charged for correct count.", 3m, invoice.JobHeader.Charges[0].JobChargeAttrib_ItemsToRate);

			Factory.Save();
			AssertEquals(JobHeaderStatus.Working.Code, adHocServiceJob.JobHeader.JH_Status);

			invoice.JobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
			Factory.Save();
			AssertEquals("Job should still change to invoiced if periodic changes", JobHeaderStatus.JobInvoiced.Code, invoice.JobHeader.JH_Status);
			AssertEquals(JobHeaderStatus.JobInvoiced.Code, adHocServiceJob.JobHeader.JH_Status);

			invoice.JobHeader.Charges.RemoveAndDeleteAll(); // clean up
		}

		#endregion

		#region Delete

		#region TestDelete

		public void TestDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			Factory.Save(); // debtor needs to be in db to be a valid debtor to set as JR_OH_SellAccount
			var today = ZDateTimeOffset.Today;

			// Create Charges.
			var warehouseReceiveHandlingCharge = Helper.CreateChargeCode("WREC", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var warehouseOutwardCharge = Helper.CreateChargeCode("WOUT", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");

			// Create Client Rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, today.AddDays(-365).Date, today.AddDays(365).Date);
			var rateLine1 = Helper.CreateRateLine(warehouseRate, warehouseReceiveHandlingCharge, "UNT", 4m, UnitCalculator.Code);
			var rateLine2 = Helper.CreateRateLine(warehouseRate, warehouseOutwardCharge, "UNT", 12m, UnitCalculator.Code);

			// Create Receive for Warehouse Receive Handling charge.
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var jobForReceive = Helper.CreateJobHeaderAndCharge(receive);
			AssertEquals("Precondition - ensure Charge was created.", 1, jobForReceive.Charges.Count);

			// Create Order for Warehouse Outwards Handling charge
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var jobForOrder = Helper.CreateJobHeaderAndCharge(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition - ensure Charge was created.", 1, jobForOrder.Charges.Count);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			// ensuring jobs are finalised in suitable time for Periodic Billing.
			receive.WD_FinalisedDate = today.AddDays(1);
			order.WD_FinalisedDate = today.AddDays(4);

			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, today.Date, today.AddDays(6).Date); // 1st to 7th (not 1st to 8th)
			Factory.Save();

			invoice.AutoRateJobHeader(null);
			AssertEquals("Precondition - should have 4 charges for invoice.", 4, invoice.JobHeader.Charges.Count);
			AssertEquals("Precondition - should have 2 charges for order", 2, (order.JobHeader as Job).Charges.Count);
			AssertEquals("Precondition - should have 2 charges for receive", 2, (receive.JobHeader as Job).Charges.Count);
			var periodicCharges = invoice.JobHeader.Charges.Where(ch => ch.PK != jobForReceive.Charges[0].PK && ch.PK != jobForOrder.Charges[0].PK).ToArray();
			var periodicCharge1 = jobForReceive.Charges[0];
			var periodicCharge2 = jobForOrder.Charges[0];

			AssertExceptionThrown<InvalidOperationException>("You cannot delete Job in Database.", () => invoice.Delete());
		}

		#endregion

		#region TestInvoiceDeleteAllRelatedAdjustment

		public void TestInvoiceDeleteAllRelatedAdjustment()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var inventoryLocation = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-1");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, inventoryLocation);
			adjustment.FinaliseDocketWithoutUserConfirmation();

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, today.AddDays(-1), today.AddDays(5));
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, adjustment }, invoice.GetAdditionalDockets());
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>("You cannot delete Job in Database.", () => invoice.Delete());
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDateTime.Today;

			var invoiceWithoutJobHeader = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-10), today.AddDays(-3));
			var invoiceWithCharges = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, today.AddDays(-2), today.AddDays(5));
			Helper.CreateJobCharges(invoiceWithCharges.JobHeader);

			Factory.Save();

			AssertEquals("System should allow to delete Periodic Invoice without Job Header.", true, invoiceWithoutJobHeader.CanDelete);
			AssertEquals("System should NOT allow to delete Periodic Invoice with saved charges.", false, invoiceWithCharges.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var today = ZDateTime.Today;

			var invoiceWithoutJobHeader = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-10), today.AddDays(-3));
			var invoiceWithCharges = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, today.AddDays(-2), today.AddDays(5));
			Helper.CreateJobCharges(invoiceWithCharges.JobHeader);

			Factory.Save();

			AssertEquals("System should allow to delete Periodic Invoice without Job Header.", "", invoiceWithoutJobHeader.ReasonForNotAbleToDelete);
			AssertEquals("System should NOT allow to delete Periodic Invoice with saved charges.", @"This record cannot be deleted.
An Invoicing Job Header (I00000002) has been created in the company EDI.", invoiceWithCharges.ReasonForNotAbleToDelete);
		}

		#endregion

		#endregion

		#region IJobInvoicingPlugIn Members

		#region TestIJobInvoicingPlugIn_DefaultDebtor

		public override void TestIJobInvoicingPlugIn_DefaultDebtor()
		{
			WhsInvoice invoice = (WhsInvoice)GetNewBusinessObject();
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			AssertNull(invoice.Client);
			AssertNull(((IJobInvoicingPlugIn)invoice).InvoicingSupporter.GetDefaultDebtor(null));

			invoice.ET_OH_Client = org1.PK;
			AssertNull(((IJobInvoicingPlugIn)invoice).InvoicingSupporter.GetDefaultDebtor(null));

			org1.SetRelatedParty(org2, RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, "");
			AssertEquals(org2, ((IJobInvoicingPlugIn)invoice).InvoicingSupporter.GetDefaultDebtor(null));
		}

		#endregion

		#region TestIJobInvoicingPlugIn_OperationsBranch_BasedOnRegistrySettings

		public void TestIJobInvoicingPlugIn_OperationsBranch_BasedOnRegistrySettings()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branchAUSYD = company.Branches.AddNew();
			branchAUSYD.GB_Code = "AUS";
			branchAUSYD.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchAUSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var client = Factory.NewWithValidTestData<OrgHeader>();
				var clientBranch = Factory.NewWithValidTestData<GlbBranch>();
				client.CompanyData.OB_GB_ControllingBranch = clientBranch.PK;

				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				var warehouseBranch = Factory.NewWithValidTestData<GlbBranch>();
				warehouse.WW_GB_RelatedCompanyBranch = warehouseBranch.PK;

				var invoice = (WhsInvoice)GetNewBusinessObject();
				invoice.ET_WW = warehouse.PK;
				invoice.ET_OH_Client = client.PK;

				Factory.Save();

				// Default to blank
				SetUpRegistry(1, 0, 0, 0);
				var jobInvPlugIn = (IJobInvoicingPlugIn)invoice;
				AssertNull("Operations Branch should be Null.", jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				// Default to Warehouse Branch
				SetUpRegistry(0, 1, 0, 0);
				AssertEquals("Operations Branch should be Warehouse's Branch.", warehouseBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				// Default to Organisation Branch
				SetUpRegistry(0, 0, 1, 0);
				AssertEquals("Operations Branch should be Client's Branch.", clientBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);

				// Default to Login User Default
				SetUpRegistry(0, 0, 0, 1);
				AssertEquals("Operations Branch should be Login user's current branch.", branchAUSYD.PK, jobInvPlugIn.InvoicingSupporter.OperationsBranch.PK);

				// Default to Warehouse Branch, but however it's inactive, so should return to next level which is Organisation Branch
				warehouseBranch.GB_IsActive = false;
				SetUpRegistry(0, 1, 2, 3);
				AssertEquals("Operations Branch should be Client's Branch if warehouse's branch is inactive.", clientBranch, jobInvPlugIn.InvoicingSupporter.OperationsBranch);
			}
		}

		void SetUpRegistry(ZShort defaultToBlank, ZShort defaultToBranchRelatedToPortOrWarehouseBranch,
			ZShort defaultToBranchOfOrganisation, ZShort defaultToLoginUserDefault)
		{
			var rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = defaultToBlank;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = defaultToBranchRelatedToPortOrWarehouseBranch;
			rule.DefaultToBranchOfOrganisation = defaultToBranchOfOrganisation;
			rule.DefaultToLoginUserDefault = defaultToLoginUserDefault;

			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		#endregion

		#endregion

		#region IJobHeaderParent Members

		protected override bool ExpectedIJobHeaderParent_AllowInvoiceDeletion
		{
			get { return false; }
		}

		#endregion

		#region TestIJobInvoicingPlugInAdditionalJobs Members

		public void TestIJobInvoicingPlugIn_AdditionalJobs()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			var order = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var iCartageJob = Helper.CreateCartageJob(order);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 8);
			AssertEquals("Precondition", 1, invoice.GetAdditionalDockets().Count(d => d.WD_DocketType == DocketType.Codes.Order));

			var iAdditionalJob = (IJobInvoicingPlugInAdditionalJobs)invoice;
			var plugins = iAdditionalJob.AdditionalJobsToShowChargesFor;
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { order.PK, iCartageJob.PK }, plugins.Select(p => p.PK));
		}

		public void TestIJobInvoicingPlugIn_AdditionalJobs_Adjustments()
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Ref0", data.Part1, 10);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RefZ", data.Part2, 10);

			var receiveStorageCharge = Helper.CreateChargeCode("RECSTO", "Receive Storage", ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var orderStorageCharge = Helper.CreateChargeCode("ORDSTO", "Order Storage", ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var storageCharge = Helper.CreateChargeCode("STO", "Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			var adjustmentInternal = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Ref1", Notify);
			adjustmentInternal.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			var lineInAdjustmentInternal = Helper.CreateWhsAdjustmentLine(adjustmentInternal, data.Part1, 3, data.Whs1.DefaultLocation);
			var lineOutAdjustmentInternal = Helper.CreateWhsAdjustmentLine(adjustmentInternal, data.Part1, -3, data.Whs1.DefaultLocation);
			adjustmentInternal.FinaliseDocket();
			adjustmentInternal.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 2);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentInternal);

			var adjustmentPublic = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Ref2", Notify);
			adjustmentPublic.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			var lineInAdjustmentPublic = Helper.CreateWhsAdjustmentLine(adjustmentPublic, data.Part2, 5, data.Whs1.DefaultLocation);
			var lineOutAdjustmentPublic = Helper.CreateWhsAdjustmentLine(adjustmentPublic, data.Part2, -5, data.Whs1.DefaultLocation);
			adjustmentPublic.FinaliseDocket();
			adjustmentPublic.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 3);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentPublic);

			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 2, 8));
			var testInteractor = new TestInteractor();
			var iAdditionalJob = (IJobInvoicingPlugInAdditionalJobs)invoice;

			AssertContainsExactElementsInAnyOrder(Array.Empty<IJobInvoicingPlugIn>(), iAdditionalJob.AdditionalJobsToShowChargesFor);
			AssertEquals("Adjustments only used in Split Month Billing.", 0,
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Adjustment).Count());
			invoice.AutoRateJobHeader(testInteractor);
			AssertEquals(0, testInteractor.YesNoWarnings.Count);

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			AssertEquals(adjustmentPublic.PK, iAdditionalJob.AdditionalJobsToShowChargesFor.Single().PK);
			AssertContainsExactElementsInAnyOrder(new[] { adjustmentPublic },
				invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Adjustment));
			invoice.AutoRateJobHeader(testInteractor);
			AssertEquals("Periodic Invoice always creates Job Headers, does not ask.", 0, testInteractor.YesNoWarnings.Count);
		}

		[TestDate(2017, 03, 03)]
		public void TestSetDefaultDebtorOnCharge()
		{
			var helper = new TestHelper(Factory);
			var winChargeCode = helper.ChargeCodes["WIN1"];
			var wouChargeCode = helper.ChargeCodes["WOU1"];
			var wstChargeCode = helper.ChargeCodes["WST1"];
			var wahChargeCode = helper.ChargeCodes["WAH1"];

			winChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			wouChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			wstChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSStorage;
			wahChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSAdHocServiceJob;

			var year = ZDateTime.Now.Year;
			SetUpData();

			Org1.OH_IsDebtor = true;

			var order = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var orderJob = new Job.Loader(order).TryCreate();

			AssertChargeDebtor(winChargeCode, orderJob, Org1.PK);
			AssertChargeDebtor(wouChargeCode, orderJob, Org1.PK);
			AssertChargeDebtor(wstChargeCode, orderJob, Org1.PK);
			AssertChargeDebtor(wahChargeCode, orderJob, Org1.PK);
		}

		void AssertChargeDebtor(AccChargeCode chargeCode, Job job, ZGuid expectedDebtorPK)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			AssertEquals("Should correctly set Debtor", expectedDebtorPK, charge.JR_OH_SellAccount);
		}

		public void TestRefreshAfterPostingDoesNotLoadCartageJob()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			Org1.OH_IsDebtor = true;
			var chargeCode = Helper.CreateChargeCode("AAA", "Test charge A", "WOU", "");
			var order = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var orderJob = new Job.Loader(order).TryCreateWithoutMutexForTestOnly();
			var charge = orderJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_SellAccount = Org1.PK;
			charge.JR_OSSellAmt = 100m;
			var iCartageJob = Helper.CreateCartageJob(order);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 8);
			AssertEquals("Precondition", 1, invoice.GetAdditionalDockets().Count(d => d.WD_DocketType == DocketType.Codes.Order));
			Factory.Save();

			var job = new Job.Loader(invoice).TryCreateWithoutMutexForTestOnly();
			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			var postedInvoices = postManager.Poster.PostedInvoices;
			Factory.Save();
			AssertEquals("Precondition: one invoice was posted", 1, postedInvoices.Count);

			var printingFilter = job.PrintingFilter;
			var printingFilterFactory = printingFilter.Transactions.Factory;
			var dBhitCountBeforeRefresh = printingFilterFactory.GetTableHitCount(JobCartageSchema.Constants.TableName);
			printingFilter.RefreshInvoiceList();
			AssertEquals("Transactions should contain 1 invoice", 1, printingFilter.FilteredTransactions.Count);

			var jobInPrintingFilterFactory = printingFilterFactory.Load<Job>(postedInvoices[0].AH_JH);
			AssertEquals("Should not initialize the job details", false, jobInPrintingFilterFactory.DefaultValuesHasBeenAssigned);

			var dBhitCountAfterRefresh = printingFilterFactory.GetTableHitCount(JobCartageSchema.Constants.TableName);
			AssertEquals("Refreshing invoice list should not load JobCartage", 0, dBhitCountAfterRefresh - dBhitCountBeforeRefresh);
		}

		public void TestIJobInvoicingPlugInAdditionalJobsFetchHint()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			var order1 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var iCartageJob1 = Helper.CreateCartageJob(order1);

			var order2 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var iCartageJob2 = Helper.CreateCartageJob(order2);

			var order3 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var iCartageJob3 = Helper.CreateCartageJob(order3);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 8);
			AssertEquals("Precondition", 3, invoice.GetAdditionalDockets().Count(d => d.WD_DocketType == DocketType.Codes.Order));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceInNewFactory = newFactory.Load<WhsInvoice>(invoice.PK);
			var iAdditionalJob = (IJobInvoicingPlugInAdditionalJobs)invoiceInNewFactory;

			int jobCartageHitCountBeforeLoadingJob = newFactory.GetTableHitCount(JobCartageSchema.Constants.TableName);
			var additionalJobs = iAdditionalJob.AdditionalJobsToShowChargesFor;
			int jobCartageHitCountAfterLoadingJob = newFactory.GetTableHitCount(JobCartageSchema.Constants.TableName);

			AssertEquals("Should be 1 hit", 1, jobCartageHitCountAfterLoadingJob - jobCartageHitCountBeforeLoadingJob);
		}

		#endregion

		#region IJobInvoicingAdditionalData Members

		public void TestGetAdditionalProperties()
		{
			var client = Helper.CreateClient("Client");
			var part1 = Helper.CreateProduct(client, "P1");
			Helper.CreateProductUnit(part1, "KG", "UNT", 2m);
			Helper.CreateProductUnit(part1, "M3", "UNT", 3m);

			var transportCo = Helper.CreateClient("TransportCo");
			var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "AAA";

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;

			var otherDocket = Factory.NewWithValidTestData<WhsOrder>();
			otherDocket.WD_OH_Client = client.PK;
			otherDocket.WD_DocketID = "Docket ID";
			otherDocket.WD_ExternalReference = "External Reference";
			otherDocket.WD_CustomerReference = "Customer Reference";
			otherDocket.TransportCoPK = transportCo.PK;
			otherDocket.WD_PL_NKCarrierServiceLevel = "AAA";

			var job = Helper.CreateRatingJob(invoice);
			var charge1 = Factory.New<JobCharge>();
			var charge2 = Factory.New<JobCharge>();
			charge1.JR_JH = job.PK;
			charge2.JR_JH = job.PK;

			var additionalData = invoice as IJobInvoicingAdditionalData;
			AssertNotNull(additionalData);

			var additionalProperties = additionalData.GetAdditionalProperties();
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketID].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.CustomerReference].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.TransportCo].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ServiceLevel].GetValue(charge1));
			AssertEquals(0m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Weight].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.WeightUQ].GetValue(charge1));
			AssertEquals(0m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Volume].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.VolumeUQ].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCode].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress1].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress2].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCity].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneePostCode].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeState].GetValue(charge1));
			AssertEquals("", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeUNLOCO].GetValue(charge1));

			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.DocketReference, "External Reference");
			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.Product, "P1");
			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.ItemsToRate, "10.0");
			AddJobChargeAttrib(charge2, JobChargeAttribTypeList.Codes.UnroundedItemsToRate, "10.0");
			additionalProperties = additionalData.GetAdditionalProperties();
			var consignee = Helper.CreateClient("CONSIGNEE");
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");
			otherDocket.ConsigneeDocAddress.OrganisationPK = consignee.PK;

			AssertEquals(otherDocket.WD_DocketID, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.DocketID].GetValue(charge2));
			AssertEquals(otherDocket.WD_CustomerReference, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.CustomerReference].GetValue(charge2));
			AssertEquals("TransportCo", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.TransportCo].GetValue(charge2));
			AssertEquals("AAA", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ServiceLevel].GetValue(charge2));
			AssertEquals(20m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Weight].GetValue(charge2));
			AssertEquals("KG", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.WeightUQ].GetValue(charge2));
			AssertEquals(30m, additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.Volume].GetValue(charge2));
			AssertEquals("M3", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.VolumeUQ].GetValue(charge2));
			AssertEquals("CONSIGIEV", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCode].GetValue(charge2));
			AssertEquals("Addr1", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress1].GetValue(charge2));
			AssertEquals("Addr2", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress2].GetValue(charge2));
			AssertEquals("City", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCity].GetValue(charge2));
			AssertEquals("12345", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneePostCode].GetValue(charge2));
			AssertEquals("State", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeState].GetValue(charge2));
			AssertEquals("UAIEV", additionalProperties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeUNLOCO].GetValue(charge2));
		}

		void SetupMainOrgAddress(OrgHeader org, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, ZString relatedPortCode)
		{
			var address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
		}

		void AddJobChargeAttrib(JobCharge charge, string name, string value)
		{
			JobChargeAttrib attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;
		}

		#endregion

		#region ISendEmailSource members

		public void TestGetEmailSubject()
		{
			WhsInvoice invoice = Factory.NewWithValidTestData<WhsInvoice>();
			AssertEquals("Periodic Invoice - " + invoice.ET_StorageJobNumber, ((ISendEmailSource)invoice).EmailSubject);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMax

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMax()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, Constants.PkgUnit.Unit, 1m);

			// create receive for warehouse receive handling and storage charges.
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2012, 09, 30), data.Part1, 5m, sourceLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2012, 09, 30), data.Part2, 10m, sourceLocation, "");

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation, destinationLocation);
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, sourceLocation, destinationLocation);
			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transferLineForPart2.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			// create a order
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2012, 10, 01), data.Part2, 2m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name);
			AssertEquals("Precondition: 2 charge should be created.", 2, invoice.JobHeader.Charges.Count);
			var chargeCodeForPart1 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)");
			var chargeCodeForPart2 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P2 (P2) for 7 days (01-Oct-12 - 07-Oct-12)");
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 5m, chargeCodeForPart1.JR_LocalSellAmt);
			AssertEquals("Since storage calc method is WarehouseStorageMaximum, we should not reduce ordered amount.", 10m, chargeCodeForPart2.JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseNonStorageMax

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseNonStorageMax()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 1m);

			// create receive for warehouse receive handling and storage charges.
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2012, 09, 30), data.Part1, 5m, sourceLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2012, 09, 30), data.Part2, 10m, sourceLocation, "");

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, sourceLocation, destinationLocation);
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, sourceLocation, destinationLocation);
			transfer.RunPreSaveValidation();
			transfer.FinaliseDocket();
			transferLineForPart1.PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(2012, 10, 3);
			transferLineForPart2.PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(2012, 10, 3);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transferLineForPart2.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			// create a order
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2012, 10, 01), data.Part2, 2m);
			order.WD_RequiredDate = new ZDateTimeOffset(2012, 10, 01);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2012, 10, 01);
			}

			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name);
			AssertEquals("Precondition: 2 charge should be created.", 2, invoice.JobHeader.Charges.Count);
			var chargeCodeForPart1 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)");
			var chargeCodeForPart2 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P2 (P2) for 7 days (01-Oct-12 - 07-Oct-12)");
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 5m, chargeCodeForPart1.JR_LocalSellAmt);
			AssertEquals("Since Storage calc method is closing balance, it should reduce the ordered amount.", 8m, chargeCodeForPart2.JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMaxForPalletLocationRating

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMaxForPalletLocationRating()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);

			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "PL", 1m);

			// create receive for warehouse receive handling and storage charges.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2012, 09, 30), data.Part1, 6m, sourceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2012, 09, 30), data.Part2, 10m, sourceLocation, "");

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1-1", "A-1-2");
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1-1", "A-1-2");
			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transferLineForPart2.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			// create an order
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2012, 10, 01), data.Part2, 2m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name);
			AssertEquals("Precondition: 2 charges should be created.", 2, invoice.JobHeader.Charges.Count);
			var chargeCodeForPart1 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)");
			var chargeCodeForPart2 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P2 (P2) for 7 days (01-Oct-12 - 07-Oct-12)");
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 3m, chargeCodeForPart1.JR_LocalSellAmt);
			AssertEquals("Since storage calc method is WarehouseStorageMaximum, we should not reduce ordered amount.", 5m, chargeCodeForPart2.JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseNonStorageMaxForPalletLocationRating

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseNonStorageMaxForPalletLocationRating()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);

			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "PL", 1m);

			// create receive for warehouse receive handling and storage charges.
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2012, 09, 30), data.Part1, 6m, sourceLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2012, 09, 30), data.Part2, 10m, sourceLocation, "");

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1-1", "A-1-2");
			var transferLineForPart2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, "A-1-1", "A-1-2");
			transfer.RunPreSaveValidation();
			transfer.FinaliseDocket();
			transferLineForPart1.PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(2012, 10, 3);
			transferLineForPart2.PickLines[0].WZ_PickedDateTime = new ZDateTimeOffset(2012, 10, 3);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transferLineForPart2.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			// create a order
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2012, 10, 01), data.Part2, 2m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2012, 10, 01);
			}

			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name);
			AssertEquals("Precondition: 2 charge should be created.", 2, invoice.JobHeader.Charges.Count);
			var chargeCodeForPart1 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)");
			var chargeCodeForPart2 = invoice.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Warehouse Storage - P2 (P2) for 7 days (01-Oct-12 - 07-Oct-12)");
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 4m, chargeCodeForPart1.JR_LocalSellAmt);
			AssertEquals("Since Storage calc method is closing balance, it should reduce the ordered amount.", 4m, chargeCodeForPart2.JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMaxForPalletLocationRatingWithTransferToMakeFullPalletWithOneReceive

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMaxForPalletLocationRatingWithTransferToMakeFullPalletWithOneReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);

			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "PL", 1m);

			// create receive for warehouse receive handling and storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", new ZDateTimeOffset(2012, 09, 30), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, sourceLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, destinationLocation);
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1-1", "A-1-2");
			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 2m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMaxForPalletLocationRatingWithTransferToMakeFullPalletWithTwoReceives

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseStorageMaxForPalletLocationRatingWithTransferToMakeFullPalletWithTwoReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);

			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "PL", 1m);

			// create receive for warehouse receive handling and storage charges.
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2012, 09, 30), data.Part1, 1m, sourceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2012, 09, 30), data.Part1, 1m, destinationLocation, "");

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1-1", "A-1-2");
			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 2m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseNonStorageMaxForPalletLocationRatingTransferToMakeFullPallet

		[TestDate(2013, 6, 1)]
		public void TestAutoRatingWarehouseStorageWithTransfers_UsingWarehouseNonStorageMaxForPalletLocationRatingTransferToMakeFullPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);

			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var sourceLocation = data.Whs1.FindLocation("A-1-1");
			var destinationLocation = data.Whs1.FindLocation("A-1-2");

			Helper.CreateOrgInvoiceRollupOrGroup(data.Org1, ChargeCodeGroupList.Codes.WHSStorage, "ALL", "ALL", "ROL", "CCG", "ALL", "DEF");

			// create Warehouse Storage charge code
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// create client rate
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// create warehouse rate entry and lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2012, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "PL", 1m);

			// create receive for warehouse receive handling and storage charges.
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2012, 09, 30), data.Part1, 1m, sourceLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(2012, 09, 30), data.Part1, 1m, destinationLocation, "");

			// transfer from one location to another
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1", Notify);
			var transferLineForPart1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1-1", "A-1-2");
			transferLineForPart1.RunPreSaveValidation();
			transferLineForPart1.PickedTime = new ZDateTimeOffset(2012, 10, 3);
			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);

			// hack to put finalised date inside periodic billing
			transferLineForPart1.WE_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(2012, 10, 3);
			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, new ZDateTime(2012, 10, 1), new ZDateTime(2012, 10, 7));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			invoice.JobHeader.Charges.Sort(JobChargeSchema.JR_LocalSellAmt.Name);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Storage - P1 (P1) for 7 days (01-Oct-12 - 07-Oct-12)", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Storage sell Amount is incorrect for part1.", 1m, invoice.JobHeader.Charges[0].JR_LocalSellAmt);
		}

		#endregion

		#region TestInvoice_NoExceptionOnSave_WithOverlappingDate_UnIncludedInvoicing

		public void TestInvoice_NoExceptionOnSave_WithOverlappingDate_UnIncludedInvoicing_DateBeforeUntick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = "BIL";
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2018, 9, 1), new ZDateTime(2018, 9, 7));
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2018, 8, 25), new ZDateTime(2018, 8, 31));
			invoice2.ET_BillingDate = new ZDateTime(2018, 8, 31);
			invoice2.ET_StorageToDate = new ZDateTime(2018, 8, 30);
			Factory.Save();

			//changes to reset non-breaking
			invoice2.ET_BillingDate = new ZDateTime(2018, 9, 1);
			invoice2.ET_StorageFromDate = new ZDateTime(2018, 8, 27);

			//breaking value after setting false
			invoice2.IncludeInInvoicing = false;
			invoice2.ET_StorageToDate = new ZDateTime(2018, 9, 5);
			AssertNoExceptionThrown("The overlapping invoice is not included so changes should not be saved. Expect no exception", () => Factory.Save());

			AssertEquals("Unincluded invoice ET_StorageToDateInfo is now readonly to prevent invalid data entry.", invoice2.ET_StorageToDateInfo.ReadOnly, true);
			AssertEquals("Changed ET_StorageFromDate of invoice2 reset back to original value.", new ZDateTime(2018, 8, 25), invoice2.ET_StorageFromDate);
			AssertEquals("Changed ET_StorageToDate of invoice2 reset back to original value.", new ZDateTime(2018, 8, 30), invoice2.ET_StorageToDate);
			AssertEquals("Changed ET_BillingDate of invoice2 reset back to original value.", new ZDateTime(2018, 8, 31), invoice2.ET_BillingDate);
		}

		public void TestInvoice_NoExceptionOnSave_WithOverlappingDate_UnIncludedInvoicing_DateAfterUntick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = "BIL";
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2018, 9, 1), new ZDateTime(2018, 9, 7));
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2018, 8, 25), new ZDateTime(2018, 8, 31));
			invoice2.ET_BillingDate = new ZDateTime(2018, 8, 31);
			invoice2.ET_StorageToDate = new ZDateTime(2018, 8, 30);
			Factory.Save();

			//changes to reset non-breaking
			invoice2.ET_BillingDate = new ZDateTime(2018, 9, 1);
			invoice2.ET_StorageFromDate = new ZDateTime(2018, 8, 27);

			//breaking value before setting false
			invoice2.ET_StorageToDate = new ZDateTime(2018, 9, 5);
			invoice2.IncludeInInvoicing = false;
			AssertNoExceptionThrown("The overlapping invoice is not included so changes should not be saved. Expect no exception", () => Factory.Save());

			AssertEquals("Unincluded invoice ET_StorageToDateInfo is now readonly to prevent invalid data entry.", invoice2.ET_StorageToDateInfo.ReadOnly, true);
			AssertEquals("Changed ET_StorageFromDate of invoice2 reset back to original value.", new ZDateTime(2018, 8, 25), invoice2.ET_StorageFromDate);
			AssertEquals("Changed ET_StorageToDate of invoice2 reset back to original value.", new ZDateTime(2018, 8, 30), invoice2.ET_StorageToDate);
			AssertEquals("Changed ET_BillingDate of invoice2 reset back to original value.", new ZDateTime(2018, 8, 31), invoice2.ET_BillingDate);
		}

		#endregion

		public void TestAutoRateJobHeader_PackageLineUnitFactor_IncludesDocketReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.Postcode = "2015";
			Helper.CreateProductUnit(data.Part1, PkgUnit.Unit, PkgUnit.Pallet, partUnitSize: 2m);
			var whsChargeCode = Helper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);

			var rate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(rate, ZDate.Today.AddDays(-10), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(whsChargeCode, CombinedCalculator.Code, QuantityUnit.KG);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem("-", 4m, 5m, QuantityUnit.KM);
			calculator["+4"] = (ZDecimal)10m;
			calculator["+10"] = (ZDecimal)15m;
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, units: 1000);
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			Factory.Save();

			var whsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var whsOrderLine = Helper.CreateWhsOrderLine(whsOrder, data.Part1, units: 50m);
			whsOrderLine.WE_F3_NKPackType = PkgUnit.Pallet;

			whsOrder.ConsigneeDocAddress.E2_AddressOverride = true;
			whsOrder.ConsigneeDocAddress.E2_City = "Sydney";
			whsOrder.ConsigneeDocAddress.E2_Postcode = "2000";
			whsOrder.ConsigneeDocAddress.E2_State = "NSW";
			whsOrder.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";

			Helper.CreatePickNew(whsOrder);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(whsOrder);
			var packingHelper = new PackingTestHelper(Factory);
			var package = packingHelper.CreatePackage(packageJob, 2, PkgUnit.Box);
			package.Pack(whsOrderLine.ReleaseLines[0], 10m);

			whsOrder.WD_FinalisedDate = ZDateTimeOffset.Today;
			Factory.Save();

			// Create Periodic Billing.
			var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, ZDateTime.Today.AddDays(-3), ZDateTime.Today.AddDays(3));
			Factory.Save();
			invoice.AutoRateJobHeader(null);
			Factory.Save();
			AssertEquals("1 charges should be created.", 1, invoice.JobHeader.Charges.Count);
			AssertEquals("Warehouse Charge O1", invoice.JobHeader.Charges[0].JR_Desc);
			AssertEquals("Warehouse Charge rates should have Docket reference.", "O1", invoice.JobHeader.Charges[0].JobChargeAttrib_DocketReference);
		}

		#region Implementation

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsInvoice>();
		}

		#endregion

		#region Org1

		OrgHeader Org1
		{
			get
			{
				if (fOrg1 == null)
				{
					fOrg1 = Factory.New<OrgHeader>();
					fOrg1.OH_Code = "TESTORG1";
					fOrg1.MainAddress.OA_Address1 = "1 HIGH ST";
					fOrg1.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.BillingPeriod;
					fOrg1.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = true;
				}
				return fOrg1;
			}
		}

		#endregion

		#region Org2

		OrgHeader Org2
		{
			get
			{
				if (fOrg2 == null)
				{
					fOrg2 = Factory.New<OrgHeader>();
					fOrg2.OH_Code = "TESTORG2";
					fOrg2.MainAddress.OA_Address1 = "2 HIGH ST";
					fOrg2.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.BillingPeriod;
				}
				return fOrg2;
			}
		}

		#endregion

		#region SetUpData

		void SetUpData()
		{
			// create environment
			Whs1 = Helper.CreateWarehouse("AAAA", "A", 2, 2);
			Whs2 = Helper.CreateWarehouse("BBBB", "B", 2, 2);

			// create product
			Part1 = Helper.CreateProduct(Org1, "AAA");
			OrgPartRelation rel = Part1.RelatedOrganisations.AddNew();
			rel.OU_OH = Org2.PK;
			rel.OU_OP = Part1.PK;
			rel.OU_Relationship = "OWN";
			OrgPartUnit unit = Part1.PartUnits.AddNew();
			unit.OF_OP = Part1.PK;
			unit.OF_PackType = "UNT";
			unit.OF_ParentPackType = "PLT";
			unit.OF_QuantityInParent = 8;
			Part1.OP_Weight = 1m;
			Part1.OP_WeightUQ = "LB";
			Part1.OP_Cubic = 1m;
			Part1.OP_CubicUQ = "L";
			Part1.OP_RH_NKCommodityCode = "GEN";

			Part2 = Helper.CreateProduct(Org1, "BBB");
			rel = Part2.RelatedOrganisations.AddNew();
			rel.OU_OH = Org2.PK;
			rel.OU_OP = Part2.PK;
			rel.OU_Relationship = "OWN";
			unit = Part2.PartUnits.AddNew();
			unit.OF_OP = Part2.PK;
			unit.OF_PackType = "UNT";
			unit.OF_ParentPackType = "PLT";
			unit.OF_QuantityInParent = 7;
			Part2.OP_Weight = 10m;
			Part2.OP_WeightUQ = "KG";
			Part2.OP_Cubic = 0.01m;
			Part2.OP_CubicUQ = "M3";
			Part2.OP_RH_NKCommodityCode = "HAZ";
		}

		#endregion

		#region PartUnit

		struct PartUnit
		{
			public PartUnit(OrgSupplierPart part, int units, params string[] attributes)
			{
				Part = part;
				Units = units;
				Attributes = attributes;
			}

			public readonly OrgSupplierPart Part;
			public readonly int Units;
			public readonly string[] Attributes;
		}

		#endregion

		#region CreateWhsReceive

		WhsReceive CreateWhsReceive(WhsWarehouse whs, OrgHeader org, ZDateTimeOffset finalisedDate, bool finalise, params PartUnit[] partUnits)
		{
			var docket = Helper.CreateWhsReceive(org, whs, NextExternalReference, new TestNotificationBuffer());
			docket.WD_BookingDate = finalisedDate.AddDays(-1);

			foreach (var partUnit in partUnits)
			{
				var line = Helper.CreateWhsReceiveInventoryLine(docket, partUnit.Part, partUnit.Units);
				if (partUnit.Attributes.Length > 0)
				{
					line.WI_PartAttrib1 = partUnit.Attributes[0];
				}

				if (partUnit.Attributes.Length > 1)
				{
					line.WI_PartAttrib2 = partUnit.Attributes[1];
				}

				if (partUnit.Attributes.Length > 2)
				{
					line.WI_PartAttrib3 = partUnit.Attributes[2];
				}
			}

			docket.AllocateLocationsWithMock();
			if (finalise)
			{
				docket.FinaliseDocket();
				docket.WD_FinalisedDate = finalisedDate;
			}

			return docket;
		}

		#endregion

		#region CreateFinalisedWhsOrder

		WhsOrder CreateFinalisedWhsOrder(WhsWarehouse whs, OrgHeader org, ZDateTimeOffset finalisedDate, params PartUnit[] partUnits)
		{
			var order = Helper.CreateWhsOrder(org, whs, NextExternalReference);
			order.WD_RequiredDate = finalisedDate;
			order.ConsigneePK = org.PK;
			order.ConsigneeAddressPK = org.MainAddress.PK;

			foreach (var partUnit in partUnits)
			{
				var line = Helper.CreateWhsOrderLine(order, partUnit.Part, partUnit.Units);
				if (partUnit.Attributes.Length > 0)
				{
					line.WE_PartAttrib1 = partUnit.Attributes[0];
				}

				if (partUnit.Attributes.Length > 1)
				{
					line.WE_PartAttrib2 = partUnit.Attributes[1];
				}

				if (partUnit.Attributes.Length > 2)
				{
					line.WE_PartAttrib3 = partUnit.Attributes[2];
				}
			}

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			order.WD_FinalisedDate = finalisedDate;

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = finalisedDate;
			}

			Factory.Save();

			return order;
		}

		#endregion

		#region NextExternalReference

		ZString NextExternalReference
		{
			get
			{
				return "REF" + (fNextExternalReference++).ToString("f0");
			}
		}

		#endregion

		#region Helper

		public WhsTestHelperFunctionsInvoice Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory)); }
		}

		#endregion

		#region AddJobHeader

		JobHeader AddJobHeader(WhsDocket docket)
		{
			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = docket.WD_DocketID;
			jobHeader.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			jobHeader.JH_ParentID = docket.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return jobHeader;
		}

		#endregion

		#region Notify

		TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}

		#endregion

		WhsTestHelperFunctionsInvoice helper;
		TestNotificationBuffer notify;
		WhsWarehouse Whs1;
		WhsWarehouse Whs2;
		OrgSupplierPart Part1;
		OrgSupplierPart Part2;
		OrgHeader fOrg1;
		OrgHeader fOrg2;
		int fNextExternalReference;

		#endregion
	}
}
