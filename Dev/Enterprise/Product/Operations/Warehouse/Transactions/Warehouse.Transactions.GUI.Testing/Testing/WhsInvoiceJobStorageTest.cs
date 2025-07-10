using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.Invoicing.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class WhsInvoiceJobStorageTest : TestCaseWithFactory
	{
		[TestDate(2012, 12, 31)]
		[GuiTest]
		public void TestAutoRateJobHeader_MonthlySplitPeriodBilling_DbHits()
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
			using (RowFactory.SetCachedTables())
			{
				for (var index = 0; index < 10; index++)
				{
					var finalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2012, 1, index + 1));
					var receivePart1 = CreateWhsReceive(data.Whs1, data.Org1, finalisedDate, true, new PartUnit(data.Part1, 10));
					var receivePart2 = CreateWhsReceive(data.Whs1, data.Org1, finalisedDate.AddDays(10), true, new PartUnit(data.Part2, 10));
					var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDateTime(2012, 1, index + 1), "", true);
					var order = CreateFinalisedWhsOrder(data.Whs1, data.Org1, finalisedDate.AddDays(1), new PartUnit(data.Part1, 5));

					AddJobHeader(receivePart1);
					AddJobHeader(receivePart2);
					AddJobHeader(order);

					receives.Add(receivePart1);
					receives.Add(receivePart2);
					orders.Add(order);
					adhocJobs.Add(adhocServiceJob);
				}

				var adjustments = new List<WhsAdjustment>();

				for (var index = 0; index < 10; index++)
				{
					var finalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2012, 1, index + 1));

					var adjustmentInternal = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Ref1", Notify);
					adjustmentInternal.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
					var lineInAdjustmentInternal = Helper.CreateWhsAdjustmentLine(adjustmentInternal, data.Part1, 3, data.Whs1.DefaultLocation);
					var lineOutAdjustmentInternal = Helper.CreateWhsAdjustmentLine(adjustmentInternal, data.Part1, -3, data.Whs1.DefaultLocation);
					adjustmentInternal.FinaliseDocket();
					adjustmentInternal.WD_FinalisedDate = finalisedDate.AddDays(1);
					WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentInternal);

					var adjustmentPublic = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "Ref2", Notify);
					adjustmentPublic.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
					var lineInAdjustmentPublic = Helper.CreateWhsAdjustmentLine(adjustmentPublic, data.Part2, 5, data.Whs1.DefaultLocation);
					var lineOutAdjustmentPublic = Helper.CreateWhsAdjustmentLine(adjustmentPublic, data.Part2, -5, data.Whs1.DefaultLocation);
					adjustmentPublic.FinaliseDocket();
					adjustmentPublic.WD_FinalisedDate = finalisedDate.AddDays(2);
					WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentPublic);

					adjustments.Add(adjustmentPublic);
				}

				var invoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2012, 1, 1), new ZDateTime(2012, 1, 31));
				AssertContainsExactElementsInAnyOrder(adjustments, invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Adjustment));
				AssertContainsExactElementsInAnyOrder(orders, invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Order));
				AssertContainsExactElementsInAnyOrder(receives, invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));
				AssertContainsExactElementsInAnyOrder(adhocJobs, invoice.AdHocServiceJobs());

				Factory.Save();

				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var expectedHits = new Dictionary<string, int>
				{
					{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 2 },
					{ AccChargeBranchOverrideSchema.Constants.TableName, 3 },
					{ AccChargeCodeSchema.Constants.TableName, 3 },
					{ AccChargeTaxOverrideSchema.Constants.TableName, 3 },
					{ AccChargeTypeOverrideSchema.Constants.TableName, 6 },
					{ AccExchangeRateConfigurationViewSchema.Constants.TableName, 1 },
					{ ExchangeRateCurrencyConfiguration.Schema.TableName, 1 },
					{ AccTaxRateSchema.Constants.TableName, 1 },
					{ GlbBranchSchema.Constants.TableName, 2 },
					{ GlbCompanySchema.Constants.TableName, 1 },
					{ GlbDepartmentSchema.Constants.TableName, 2 },
					{ GlbDeptChargesSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ JobCartageSchema.Constants.TableName, 1 },
					{ JobChargeSchema.Constants.TableName, 3 },
					{ JobChargeTargetSchema.Constants.TableName, 0 },
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ JobExRateSchema.Constants.TableName, 1 },
					{ JobHeaderSchema.Constants.TableName, 20 },
					{ JobServiceSchema.Constants.TableName, 1 },
					{ JobStorageSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 2 },
					{ OrgHeaderSchema.Constants.TableName, 5 },
					{ OrgInvoiceRollupOrGroupSchema.Constants.TableName, 2 },
					{ OrgInvoiceTypeSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 2 },
					{ OrgRateTariffLevelSchema.Constants.TableName, 1 },
					{ OrgRelatedPartySchema.Constants.TableName, 3 },
					{ OrgStaffAssignmentsSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 2 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 3 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ RatingHeaderSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefCurrencySchema.Constants.TableName, 1 },
					{ "RefDatabase_RefAccTaxRate", 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 2 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ ViewGenericJobSchema.Constants.TableName, 3 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 0 },
					{ WhsWarehouseSchema.Constants.TableName, 2 },
					{ WhsAdHocServiceJobSchema.Constants.TableName, 1 },
					{ WhsVASOrderSchema.Constants.TableName, 1 },
				};

				var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);

				using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory))
				using (TestConnection.TrackExecutedCommands())
				{
					invoiceInOtherFactory.AutoRateJobHeader(new TestInteractor());
					AssertEquals("Should have only hit the DB once for the Docket Rating Adapters Query, even though we have multiple Dockets.", 1,
						TestConnection.ExecutedCommands.Count(c => c.Contains("@OrderPKs") && c.Contains("@NonOrderPKs")));
				}

				var pks = ((IJobInvoicingPlugInAdditionalJobs)invoiceInOtherFactory).AdditionalJobsToShowChargesFor.Select(j => j.PK);
				var query = new ZQuery();
				query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
				query.AddToFilter(WhsDocketSchema.PK, pks);
				query.FetchOnlyFromLocalCache = true;

				var receivesInMemory = otherFactory.Load<WhsReceive>(query);
				AssertEquals("All receives should have successfully created Charges from Autorating.", 20, receivesInMemory.Count(r => new Job.Loader(r).Load().Charges.Count == 2));
				AssertEquals("Periodic invoice should have successfully created matching Charges from Autorating.", 41, invoiceInOtherFactory.JobHeader.Charges.Count);

				expectedHits.Add(AccChargeRevRecOverrideSchema.Constants.TableName, 1);
				expectedHits.Add(JobChargeRevRecognitionSchema.Constants.TableName, 1);
				expectedHits[GlbBranchSchema.Constants.TableName] += 1;
				expectedHits[OrgHeaderSchema.Constants.TableName] += 1;
				expectedHits[WhsDocketSchema.Constants.TableName] += 1;
				expectedHits["RefDatabase_RefAccTaxRate"] += 1;
				using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory))
				using (RowFactory.RemoveCachedTablesTemporarily(RefPacksSchema.Constants.TableName))
				{
					invoiceInOtherFactory.RunPreSaveValidation();
				}

				var expectedHitsAfterSave = new Dictionary<string, int>(expectedHits)
				{
					{ AccChargeGLPostingOverrideSchema.Constants.TableName, 8 },
					{ AccTransactionLinesSchema.Constants.TableName, 2 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 2 },
				};

				expectedHitsAfterSave[ProcessTasksSchema.Constants.TableName] += 3;
				expectedHitsAfterSave[ProcessTaskTemplateSchema.Constants.TableName] += 2;
				expectedHitsAfterSave[StmNoteSchema.Constants.TableName] += 2;

				using (AssertDbHitsWithUsefulQueryInformation(expectedHitsAfterSave, otherFactory))
				using (RowFactory.RemoveCachedTablesTemporarily(RefPacksSchema.Constants.TableName))
				{
					otherFactory.Save();
				}

				var expectedHitsAfterFormShown = new Dictionary<string, int>(expectedHitsAfterSave)
				{
					{ OrgAddressAdditionalInfoSchema.Constants.TableName, 1 },
					{ WorkItemSchema.Constants.TableName, 0 },
					{ AccTaxConfigurationSchema.Constants.TableName, 1 },
				};

				expectedHitsAfterFormShown[JobHeaderSchema.Constants.TableName] += 3;
				expectedHitsAfterFormShown[OrgCompanyDataSchema.Constants.TableName] += 1;
				expectedHitsAfterFormShown[StmNoteSchema.Constants.TableName] += 1;
				expectedHitsAfterFormShown[GlbStaffSchema.Constants.TableName] += 2;

#if WINZOR
				expectedHitsAfterFormShown.Add(AccClientInvoiceOrderSchema.Constants.TableName, 1);
#endif

				using (var form = new InvoicingForm(invoiceInOtherFactory))
				{
					using (AssertDbHitsWithUsefulQueryInformation(expectedHitsAfterFormShown, otherFactory))
					using (RowFactory.RemoveCachedTablesTemporarily(RefPacksSchema.Constants.TableName))
					{
						form.Show();
					}

					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					var postMenuItem = plugIn.TopLevelMenu.MenuItems.FindByText(Constants.MenuNameConstants.PostAllChargesAndCosts);

					var expectedHitsForPostingFactory = new Dictionary<string, int>
					{
						{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 2 },
						{ AccChargeCodeSchema.Constants.TableName, 1 },
						{ AccChargeGLPostingOverrideSchema.Constants.TableName, 9 },
						{ AccChargeRevRecOverrideSchema.Constants.TableName, 3 },
						{ AccChargeTaxOverrideSchema.Constants.TableName, 3 },
						{ AccChargeTypeOverrideSchema.Constants.TableName, 4 },
						{ AccGLHeaderSchema.Constants.TableName, 1 },
						{ AccPeriodManagementSchema.Constants.TableName, 1 },
						{ AccTaxRateSchema.Constants.TableName, 1 },
						{ AccTransactionHeaderSchema.Constants.TableName, 2 },
						{ AccTransactionLinesSchema.Constants.TableName, 2 },
						{ GlbCompanySchema.Constants.TableName, 0 },
						{ GlbDepartmentSchema.Constants.TableName, 1 },
						{ JobCartageSchema.Constants.TableName, 1 },
						{ JobChargeSchema.Constants.TableName, 3 },
						{ JobChargeRevRecognitionSchema.Constants.TableName, 1 },
						{ JobChargeTargetSchema.Constants.TableName, 1 },
						{ JobHeaderSchema.Constants.TableName, 7 },
						{ JobStorageSchema.Constants.TableName, 1 },
						{ OrgAddressSchema.Constants.TableName, 3 },
						{ OrgAddressCapabilitySchema.Constants.TableName, 2 },
						{ OrgARTermsSchema.Constants.TableName, 1 },
						{ OrgCommissionAgreementSchema.Constants.TableName, 2 },
						{ OrgCompanyDataSchema.Constants.TableName, 3 },
						{ OrgCusCodeSchema.Constants.TableName, 1 },
						{ OrgHeaderSchema.Constants.TableName, 4 },
						{ OrgInvoiceTypeSchema.Constants.TableName, 1 },
						{ OrgMiscServSchema.Constants.TableName, 1 },
						{ OrgRelatedPartySchema.Constants.TableName, 1 },
						{ OrgStaffAssignmentsSchema.Constants.TableName, 1 },
						{ ProcessTasksSchema.Constants.TableName, 1 },
						{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
						{ "RefDatabase_RefAccTaxRate", 2 },
						{ "RefDatabase_RefDataGrouping", 1 },
						{ RefUNLOCOSchema.Constants.TableName, 1 },
						{ ViewGenericJobSchema.Constants.TableName, 3 },
						{ WhsDocketSchema.Constants.TableName, 1 },
						{ WhsWarehouseSchema.Constants.TableName, 1 },
						{ WhsAdHocServiceJobSchema.Constants.TableName, 1 },
						{ AccGLHeaderSubAccountSchema.Constants.TableName, 3 },
						{ WhsVASOrderSchema.Constants.TableName, 1 },
						{ AccTaxConfigurationSchema.Constants.TableName, 1 },
						{ AccSurchargeApplicationSchema.Constants.TableName, 1 },
					};

					InvoicingBase.UseSmallBatchSizeInTest.Value = false; // the default behaviour is to batch reload 5 JobHeaders in tests, this will bloat the dbhits for the test so we disable this here.
					using (AssertDbHitsForAllFactories("Make sure Posting Factory has proper fetch hints.", expectedHitsForPostingFactory, useOnlyNewFactories: true, tablesToCollectQueriesFor: expectedHitsForPostingFactory.Keys.ToArray()))
					using (AssertDbHitsWithUsefulQueryInformation(expectedHitsAfterFormShown, otherFactory))
					{
						postMenuItem.PerformClick();
					}

					invoiceInOtherFactory.JobHeader.Charges.Reload();
					AssertEquals("All charges should have been successfully posted.", true, invoiceInOtherFactory.JobHeader.Charges.Cast<Charge>().All(c => c.JR_IsPosted));
				}
			}
		}

		WhsTestHelperFunctionsInvoice Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory)); }
		}
		WhsTestHelperFunctionsInvoice helper;

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
		int fNextExternalReference;

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
		TestNotificationBuffer notify;

		#endregion
	}
}
