using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test.GUI
{
	[TestDate(2025, 1, 28, 8, 0, 0)]
	public class CYDPeriodicInvoicingRatingIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestAutoRating_GenericMatch()
		{
			var startDate = ZDateTime.Today.AddDays(-27);   // 2025-01-01
			var endDate = ZDateTime.Today;					// 2025-01-28, has to align with the default weekly period
			var periodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDate, endDate);
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, endDate.AddDays(-10), endDate.AddDays(-5));

			AutorateAndAssertPeriodicInvoicing(
				periodicInvoicing,
				[(ChargeCode.STORAGE, 6, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 6 Unit x Day (1 Unit(s) x 6 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (18-Jan-25 - 23-Jan-25)")]
			);
		}

		public void TestAutoRating_HasFreeDays()
		{
			var startDate = ZDateTime.Today.AddDays(-27);
			var endDate = ZDateTime.Today;
			var periodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDate, endDate);
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, endDate.AddDays(-10), endDate.AddDays(-5));

			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 1);

			AutorateAndAssertPeriodicInvoicing(
				periodicInvoicing,
				[(ChargeCode.STORAGE, 5, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 5 Unit x Day (1 Unit(s) x 5 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (18-Jan-25 - 23-Jan-25)")]
			);
		}

		public void TestAutoRating_HasFreeDays_ExcludeWeekends()
		{
			var startDate = ZDateTime.Today.AddDays(-27);
			var endDate = ZDateTime.Today;
			var periodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDate, endDate);
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, endDate.AddDays(-10), endDate.AddDays(-5));

			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 1);

			var rateLine = rateEntry.RateLines.Cast<RateLine>().First(rateLine => rateLine.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.YardStorage);
			Helper.SetExcludeHolidays(rateLine);
			Factory.Save();

			AutorateAndAssertPeriodicInvoicing(
				periodicInvoicing,
				[(ChargeCode.STORAGE, 3, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 3 Unit x Day (1 Unit(s) x 3 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (18-Jan-25 - 23-Jan-25)")]
			);
		}

		public void TestAutoRating_HasFreeDays_SaveStorageLine_HasNoCharge()
		{
			var startDate = ZDateTime.Today.AddDays(-27);
			var endDate = ZDateTime.Today;
			var periodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDate, endDate);
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, endDate.AddDays(-10), endDate.AddDays(-5));

			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 8);

			AutorateAndAssertPeriodicInvoicing(
				periodicInvoicing,
				[(ChargeCode.STORAGE, 0, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 0 Unit x Day (1 Unit(s) x 0 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (18-Jan-25 - 23-Jan-25)")]
			);

			var storageLine = Factory.Load<CYDYardStorageLines>(new ZQuery()).Single();
			AssertEquals(8, (int)storageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(2, (int)storageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(6, (int)storageLine.YSL_StorageDays);
			AssertEquals(0, (int)storageLine.YSL_ChargedDays);
			AssertEquals(0, (int)storageLine.YSL_ChargedAmount);
		}

		public void TestAutoRating_HasFreeDays_SaveStorageLine_HasCharge()
		{
			var startDate = ZDateTime.Today.AddDays(-27);
			var endDate = ZDateTime.Today;
			var periodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDate, endDate);
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, endDate.AddDays(-10), endDate.AddDays(-5));

			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 4);

			AutorateAndAssertPeriodicInvoicing(
				periodicInvoicing,
				[(ChargeCode.STORAGE, 2, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 2 Unit x Day (1 Unit(s) x 2 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (18-Jan-25 - 23-Jan-25)")]
			);

			var storageLine = Factory.Load<CYDYardStorageLines>(new ZQuery()).Single();
			AssertEquals(4, (int)storageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(0, (int)storageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(6, (int)storageLine.YSL_StorageDays);
			AssertEquals(2, (int)storageLine.YSL_ChargedDays);
			AssertEquals(2, (int)storageLine.YSL_ChargedAmount);
		}

		public void TestAutoRating_HasFreeDays_SaveStorageLine_DeleteJobStorage()
		{
			var startDate = ZDateTime.Today.AddDays(-27);
			var endDate = ZDateTime.Today;
			var periodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDate, endDate);
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, endDate.AddDays(-10), endDate.AddDays(-5));

			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 4);

			AutorateAndAssertPeriodicInvoicing(
				periodicInvoicing,
				[(ChargeCode.STORAGE, 2, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 2 Unit x Day (1 Unit(s) x 2 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (18-Jan-25 - 23-Jan-25)")]
			);

			var storageLines = Factory.Load<CYDYardStorageLines>(new ZQuery());
			AssertEquals(1, storageLines.Length);
			AssertEquals(4, (int)storageLines[0].YSL_FreeDaysOpenBalance);
			AssertEquals(0, (int)storageLines[0].YSL_FreeDaysCloseBalance);
			AssertEquals(6, (int)storageLines[0].YSL_StorageDays);
			AssertEquals(2, (int)storageLines[0].YSL_ChargedDays);
			AssertEquals(2, (int)storageLines[0].YSL_ChargedAmount);

			periodicInvoicing.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			AssertEquals(0, newFactory.Load<CYDYardStorageLines>(new ZQuery()).Length);
			AssertEquals(0, newFactory.Load<PeriodicInvoicing>(new ZQuery()).Length);
		}

		public void TestAutoRating_UseStandardCalculationMethod()
		{
			var startDateOfFirstPeriod = ZDateTime.Today.AddDays(-27);
			var endDateOfFirstPeriod = ZDateTime.Today.AddDays(-14);
			var startDateOfSecondPeriod = endDateOfFirstPeriod.AddDays(1);
			var endDateOfSecondPeriod = ZDateTime.Today;
			var firstPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfFirstPeriod, endDateOfFirstPeriod);			// 2025-01-01 to 2025-01-14
			var secondPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfSecondPeriod, endDateOfSecondPeriod);			// 2025-01-15 to 2025-01-28
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, startDateOfFirstPeriod.AddDays(10), endDateOfSecondPeriod.AddDays(-5));	// 2025-01-11 to 2025-01-23

			AssertEquals("STD", client.CompanyData.OB_ARYardStorageCalcMethod);
			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 6);

			AutorateAndAssertPeriodicInvoicing(
				firstPeriodicInvoicing,
				[(ChargeCode.STORAGE, 0, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 0 Unit x Day (1 Unit(s) x 0 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (11-Jan-25 - 14-Jan-25)")]
			);

			var firstStorageLine = Factory.Load<CYDYardStorageLines>(new ZQuery()).Single();
			AssertEquals(6, (int)firstStorageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(2, (int)firstStorageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(4, (int)firstStorageLine.YSL_StorageDays);
			AssertEquals(0, (int)firstStorageLine.YSL_ChargedDays);
			AssertEquals(0, (int)firstStorageLine.YSL_ChargedAmount);

			AutorateAndAssertPeriodicInvoicing(
				secondPeriodicInvoicing,
				[(ChargeCode.STORAGE, 7, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 7 Unit x Day (1 Unit(s) x 7 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (15-Jan-25 - 23-Jan-25)")]
			);

			var storageLines = Factory.Load<CYDYardStorageLines>(new ZQuery());
			AssertEquals(2, storageLines.Length);
			var secondStorageLine = storageLines.First(storageLine => storageLine.YSL_ET_JobStorage != firstPeriodicInvoicing.PK);
			AssertEquals(2, (int)secondStorageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(0, (int)secondStorageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(9, (int)secondStorageLine.YSL_StorageDays);
			AssertEquals(7, (int)secondStorageLine.YSL_ChargedDays);
			AssertEquals(7, (int)secondStorageLine.YSL_ChargedAmount);
		}

		public void TestAutoRating_UseStandardCalculationMethod_NotGatedOut()
		{
			var startDateOfFirstPeriod = ZDateTime.Today.AddDays(-27);
			var endDateOfFirstPeriod = ZDateTime.Today.AddDays(-14);
			var startDateOfSecondPeriod = endDateOfFirstPeriod.AddDays(1);
			var endDateOfSecondPeriod = ZDateTime.Today;
			var firstPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfFirstPeriod, endDateOfFirstPeriod);        // 2025-01-01 to 2025-01-14
			var secondPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfSecondPeriod, endDateOfSecondPeriod);     // 2025-01-15 to 2025-01-28
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, startDateOfFirstPeriod.AddDays(10), ZDateTime.Empty);               // 2025-01-11, not gated out

			AssertEquals("STD", client.CompanyData.OB_ARYardStorageCalcMethod);
			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 6);

			AutorateAndAssertPeriodicInvoicing(
				firstPeriodicInvoicing,
				[(ChargeCode.STORAGE, 0, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 0 Unit x Day (1 Unit(s) x 0 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (11-Jan-25 - 14-Jan-25)")]
			);

			var firstStorageLine = Factory.Load<CYDYardStorageLines>(new ZQuery()).Single();
			AssertEquals(6, (int)firstStorageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(2, (int)firstStorageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(4, (int)firstStorageLine.YSL_StorageDays);
			AssertEquals(0, (int)firstStorageLine.YSL_ChargedDays);
			AssertEquals(0, (int)firstStorageLine.YSL_ChargedAmount);

			AutorateAndAssertPeriodicInvoicing(
				secondPeriodicInvoicing,
				[(ChargeCode.STORAGE, 12, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 12 Unit x Day (1 Unit(s) x 12 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (15-Jan-25 - 28-Jan-25)")]
			);

			var storageLines = Factory.Load<CYDYardStorageLines>(new ZQuery());
			AssertEquals(2, storageLines.Length);
			var secondStorageLine = storageLines.First(storageLine => storageLine.YSL_ET_JobStorage != firstPeriodicInvoicing.PK);
			AssertEquals(2, (int)secondStorageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(0, (int)secondStorageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(14, (int)secondStorageLine.YSL_StorageDays);
			AssertEquals(12, (int)secondStorageLine.YSL_ChargedDays);
			AssertEquals(12, (int)secondStorageLine.YSL_ChargedAmount);
		}

		public void TestAutoRating_UseGateOutCalculationMethod()
		{
			var startDateOfFirstPeriod = ZDateTime.Today.AddDays(-27);
			var endDateOfFirstPeriod = ZDateTime.Today.AddDays(-14);
			var startDateOfSecondPeriod = endDateOfFirstPeriod.AddDays(1);
			var endDateOfSecondPeriod = ZDateTime.Today;
			var firstPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfFirstPeriod, endDateOfFirstPeriod);            // 2025-01-01 to 2025-01-14
			var secondPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfSecondPeriod, endDateOfSecondPeriod);         // 2025-01-15 to 2025-01-28
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, startDateOfFirstPeriod.AddDays(10), endDateOfSecondPeriod.AddDays(-5)); // 2025-01-11 to 2025-01-23

			Helper.SetClientCalculationMethod(client, "OUT");
			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 6);

			AutorateAndAssertPeriodicInvoicing(
				firstPeriodicInvoicing,
				[]
			);

			var storageLines = Factory.Load<CYDYardStorageLines>(new ZQuery());
			AssertEquals(0, storageLines.Length);

			AutorateAndAssertPeriodicInvoicing(
				secondPeriodicInvoicing,
				[(ChargeCode.STORAGE, 7, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 7 Unit x Day (1 Unit(s) x 7 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (11-Jan-25 - 23-Jan-25)")]
			);

			var storageLine = Factory.Load<CYDYardStorageLines>(new ZQuery()).Single();
			AssertEquals(6, (int)storageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(0, (int)storageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(13, (int)storageLine.YSL_StorageDays);
			AssertEquals(7, (int)storageLine.YSL_ChargedDays);
			AssertEquals(7, (int)storageLine.YSL_ChargedAmount);
		}

		public void TestAutoRating_UseStandardCalculationMethod_GatedOutOnEndDay()
		{
			var startDateOfFirstPeriod = ZDateTime.Today.AddDays(-27);
			var endDateOfFirstPeriod = ZDateTime.Today.AddDays(-14);
			var startDateOfSecondPeriod = endDateOfFirstPeriod.AddDays(1);
			var endDateOfSecondPeriod = ZDateTime.Today;
			var firstPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfFirstPeriod, endDateOfFirstPeriod);            // 2025-01-01 to 2025-01-14
			var secondPeriodicInvoicing = Helper.CreatePeriodicInvoicing(client, yard, startDateOfSecondPeriod, endDateOfSecondPeriod);         // 2025-01-15 to 2025-01-28
			CreateYardUnitForTesting("CNT0001", "20GP", isEmpty: false, startDateOfFirstPeriod.AddDays(10), endDateOfSecondPeriod.AddHours(1)); // 2025-01-11 to 2025-01-28

			Helper.SetClientCalculationMethod(client, "OUT");
			Helper.CreateYardStorageFreeDays(client, yard, freeDays: 6);

			AutorateAndAssertPeriodicInvoicing(
				firstPeriodicInvoicing,
				[]
			);

			var storageLines = Factory.Load<CYDYardStorageLines>(new ZQuery());
			AssertEquals(0, storageLines.Length);

			AutorateAndAssertPeriodicInvoicing(
				secondPeriodicInvoicing,
				[(ChargeCode.STORAGE, 12, "CYD - CYS Storage Charge", "CYD - CYS Storage Charge - 12 Unit x Day (1 Unit(s) x 12 Day(s)) @ AUD 1.00/Unit x Day: CNT0001 (11-Jan-25 - 28-Jan-25)")]
			);

			var storageLine = Factory.Load<CYDYardStorageLines>(new ZQuery()).Single();
			AssertEquals(6, (int)storageLine.YSL_FreeDaysOpenBalance);
			AssertEquals(0, (int)storageLine.YSL_FreeDaysCloseBalance);
			AssertEquals(18, (int)storageLine.YSL_StorageDays);
			AssertEquals(12, (int)storageLine.YSL_ChargedDays);
			AssertEquals(12, (int)storageLine.YSL_ChargedAmount);
		}

		#region Implementation

		new CYDYardTestHelper Helper
		{
			get { return helper ??= new CYDYardTestHelper(Factory); }
		}

		CYDYardTestHelper helper;

		WhsWarehouse yard;

		WhsLocation location;

		OrgHeader client;

		RateEntry rateEntry;

		OrgHeader transportor;

		protected override void SetUp()
		{
			base.SetUp();

			yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			location = row.Locations[0];
			client = SetupClientForRating("Client", new CYDRateEntryParameters { StartDate = ZDate.Today.AddDays(-100) }, out RateEntry rateEntry, out ClientRate clientRate);
			this.rateEntry = rateEntry;
			transportor = Helper.CreateClient("Transportor");

			Factory.Save();
		}

		OrgHeader SetupClientForRating(string clientCode, RateEntryParameters rateEntryParams, out RateEntry rateEntry, out ClientRate clientRate)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsDebtor = true;
			clientRate = Helper.CreateClientRate(client);
			rateEntry = Helper.CreateRateEntry(clientRate, rateEntryParams);

			return client;
		}

		void CreateYardUnitForTesting(string unitId, string containerType, bool isEmpty, ZDateTime gateInDay, ZDateTime gateOutDay)
		{
			var receiveAdvice = Helper.CreateReceiveAdvice(client, yard, "PRA-" + unitId);
			var yardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, unitId, containerType, isEmpty);

			var receiveTransportationUnit = Helper.CreateTransportationUnit("TPUIN-" + unitId, transportor, yard);
			Helper.AddDelivery(receiveTransportationUnit, yardUnit);
			Helper.GateInTransportationUnit(receiveTransportationUnit, yard.GetWarehouseBranchDateTimeOffset(gateInDay), location);
			Helper.GateOutTransportationUnit(receiveTransportationUnit, yard.GetWarehouseBranchDateTimeOffset(gateInDay));

			var releaseAdvice = Helper.CreateReleaseAdvice(client, yard, "REL-" + unitId);
			Helper.AddReleaseAdviceLine(releaseAdvice, yardUnit);

			if (gateOutDay.IsValid)
			{
				var dispatchTransportationUnit = Helper.CreateTransportationUnit("TPUOUT-" + unitId, transportor, yard);
				Helper.AddPickup(dispatchTransportationUnit, yardUnit);
				Helper.GateInTransportationUnit(dispatchTransportationUnit, yard.GetWarehouseBranchDateTimeOffset(gateOutDay), location);
				Helper.GateOutTransportationUnit(dispatchTransportationUnit, yard.GetWarehouseBranchDateTimeOffset(gateOutDay));
			}

			Factory.Save();
		}

		void AutorateAndAssertPeriodicInvoicing(PeriodicInvoicing periodicInvoicing, List<(ChargeCode, int, string, string)> expectedCharges)
		{
			var expected = expectedCharges.Select(static charge => new AssertionCharge
			{
				ChargeCode = Enum.GetName(typeof(ChargeCode), charge.Item1),
				JR_OSSellAmt = charge.Item2,
				JR_Desc = charge.Item3,
			});
			var interactor = new TestInteractor();
			AutorateAndAssert(expected, periodicInvoicing, client, autorateCosts: false, testInteractor: interactor);

			var invoiceRollupOrGroupCollection = new InvoiceRollupOrGroupCollection();
			var item = invoiceRollupOrGroupCollection.AddNew();
			item.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			item.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			item.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			item.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			item.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			item.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			item.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceRollupOrGroupCollection))
			{
				expected = expectedCharges.Select(static charge => new AssertionCharge
				{
					ChargeCode = Enum.GetName(typeof(ChargeCode), charge.Item1),
					JR_OSSellAmt = charge.Item2,
					JR_Desc = charge.Item4,
				});
				AutorateAndAssert(expected, periodicInvoicing, client, autorateCosts: false, testInteractor: interactor);
			}
		}

		#endregion
	}
}
