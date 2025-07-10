using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Invoicing.Business.Testing
{
	public class WhsInvoiceRatingAdapterTest : WhsTestCaseWithFactory
	{
		#region Private Class Tests

		#region TestClassUnitsTableKey

		public void TestClassUnitsTableKey()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var productAttributeMeasure = new ProductAttributesMeasure();
			var unitsTableKey = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, data.Whs1.DefaultLocation.PK, data.Part1.PK, productAttributeMeasure, "P01");

			AssertEquals(data.Whs1.PK, unitsTableKey.WarehousePK);
			AssertEquals(data.Org1.PK, unitsTableKey.ClientPK);
			AssertEquals(data.Whs1.DefaultLocation.PK, unitsTableKey.LocationPK);
			AssertEquals(data.Part1.PK, unitsTableKey.ProductPK);
			AssertEquals(productAttributeMeasure, unitsTableKey.Attributes);
			AssertEquals("P01", unitsTableKey.PalletID);
		}

		#endregion

		#region TestClassBalanceUnits

		public void TestClassBalanceUnits()
		{
			var balanceUnits = new WhsInvoiceRatingAdapter.BalanceUnits(-10m);
			AssertEquals(-10m, balanceUnits.CurrentUnits);
			AssertEquals(-10m, balanceUnits.MaximumUnits);
			AssertEquals(-10m, balanceUnits.MinimumUnits);

			balanceUnits.RemoveNegativeValues();
			AssertEquals(0m, balanceUnits.CurrentUnits);
			AssertEquals(0m, balanceUnits.MaximumUnits);
			AssertEquals(0m, balanceUnits.MinimumUnits);

			balanceUnits.CurrentUnits = 10m;
			AssertEquals(10m, balanceUnits.CurrentUnits);
			AssertEquals(10m, balanceUnits.MaximumUnits);
			AssertEquals(0m, balanceUnits.MinimumUnits);

			balanceUnits.CurrentUnits = -10m;
			AssertEquals(-10m, balanceUnits.CurrentUnits);
			AssertEquals(10m, balanceUnits.MaximumUnits);
			AssertEquals(-10m, balanceUnits.MinimumUnits);
		}

		#endregion

		#region TestClassBalanceUnitsTable

		#region TestClassBalanceUnitsTable_AddBalance

		public void TestClassBalanceUnitsTable_AddBalance_WarehouseSplitPeriodBilling()
		{
			TestClassBalanceUnitsTable_AddBalanceCore(OrgCompanyDataLookups.WarehouseSplitPeriodBilling);
		}

		public void TestClassBalanceUnitsTable_AddBalance_WarehouseStorageMax()
		{
			TestClassBalanceUnitsTable_AddBalanceCore(OrgCompanyDataLookups.WarehouseStorageMax);
		}

		public void TestClassBalanceUnitsTable_AddBalance_WarehouseStoragePeak()
		{
			TestClassBalanceUnitsTable_AddBalanceCore(OrgCompanyDataLookups.WarehouseStoragePeak);
		}

		public void TestClassBalanceUnitsTable_AddBalance_WarehouseStorageClosingBalance()
		{
			TestClassBalanceUnitsTable_AddBalanceCore(OrgCompanyDataLookups.WarehouseStorageClosingBalance);
		}

		void TestClassBalanceUnitsTable_AddBalanceCore(string calcMethod)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			var client2 = Helper.CreateClient("CL2");
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationB1 = whs2.FindLocation("B-1");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var productAttributeMeasure = new ProductAttributesMeasure();
			var productAttributeMeasure2 = new ProductAttributesMeasure("AA");

			var productKey1 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure, "P01");
			var productKey2 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part2.PK, productAttributeMeasure, "P01");
			var productKey3 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, client2.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure, "P02");
			var productKey4 = new WhsInvoiceRatingAdapter.UnitsTableKey(whs2.PK, data.Org1.PK, locationB1.PK, data.Part1.PK, productAttributeMeasure, "P02");
			var productKey5 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure2, "");
			var productKey6 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA2.PK, data.Part1.PK, productAttributeMeasure, "P01");

			var balanceUnits = new WhsInvoiceRatingAdapter.BalanceUnitsTable(calcMethod);
			balanceUnits.AddBalance(productKey1, 10m);
			balanceUnits.AddBalance(productKey1, 10m);
			balanceUnits.AddBalance(productKey1, 0m);
			balanceUnits.AddBalance(productKey1, -10m);
			balanceUnits.AddBalance(productKey2, 10m);
			balanceUnits.AddBalance(productKey3, 10m);
			balanceUnits.AddBalance(productKey4, 10m);
			balanceUnits.AddBalance(productKey5, 10m);
			balanceUnits.AddBalance(productKey6, 10m);

			var unitsByProduct = balanceUnits.GetUnitsByProduct();
			AssertNotNull(unitsByProduct);
			if (calcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
			{
				AssertEquals(4, unitsByProduct.Count);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey1).Value, 30m, 30m, 30m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
			}
			else
			{
				AssertEquals(5, unitsByProduct.Count);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey1).Value, 20m, 20m, 20m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey5).Value, 10m, 10m, 10m);
			}

			var unitsByProductLocation = balanceUnits.GetUnitsByProductLocation();
			AssertNotNull(unitsByProductLocation);

			if (calcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
			{
				AssertEquals(4, unitsByProductLocation.Count);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey1).Value, 30m, 30m, 30m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
			}
			else
			{
				AssertEquals(6, unitsByProductLocation.Count);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey5).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey6).Value, 10m, 10m, 10m);
			}

			var unitsByPalletID = balanceUnits.GetUnitsByPalletID();
			AssertNotNull(unitsByPalletID);
			AssertEquals(5, unitsByPalletID.Count);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey1).Value, 20m, 20m, 20m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey5).Value, 10m, 10m, 10m);
		}

		#endregion

		#region TestClassBalanceUnitsTable_AddTransaction

		public void TestClassBalanceUnitsTable_AddTransaction_WarehouseStorageMax()
		{
			TestClassBalanceUnitsTable_AddTransactionCore(OrgCompanyDataLookups.WarehouseStorageMax);
		}

		public void TestClassBalanceUnitsTable_AddTransaction_WarehouseStorageClosingBalance()
		{
			TestClassBalanceUnitsTable_AddTransactionCore(OrgCompanyDataLookups.WarehouseStorageClosingBalance);
		}

		public void TestClassBalanceUnitsTable_AddTransaction_WarehouseStoragePeak()
		{
			TestClassBalanceUnitsTable_AddTransactionCore(OrgCompanyDataLookups.WarehouseStoragePeak);
		}

		public void TestClassBalanceUnitsTable_AddTransaction_WarehouseSplitPeriodBilling()
		{
			TestClassBalanceUnitsTable_AddTransactionCore(OrgCompanyDataLookups.WarehouseSplitPeriodBilling);
		}

		void TestClassBalanceUnitsTable_AddTransactionCore(string calcMethod)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			var client2 = Helper.CreateClient("CL2");
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationB1 = whs2.FindLocation("B-1");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var productAttributeMeasure = new ProductAttributesMeasure();
			var productAttributeMeasure2 = new ProductAttributesMeasure("AA");

			var productKey1 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure, "P01");
			var productKey2 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part2.PK, productAttributeMeasure, "P01");
			var productKey3 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, client2.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure, "P02");
			var productKey4 = new WhsInvoiceRatingAdapter.UnitsTableKey(whs2.PK, data.Org1.PK, locationB1.PK, data.Part1.PK, productAttributeMeasure, "P02");
			var productKey5 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure2, "");
			var productKey6 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA2.PK, data.Part1.PK, productAttributeMeasure, "P01");

			var balanceUnits = new WhsInvoiceRatingAdapter.BalanceUnitsTable(calcMethod);
			balanceUnits.AddTransaction(productKey1, 10m);
			balanceUnits.AddTransaction(productKey1, 10m);
			balanceUnits.AddTransaction(productKey1, 0m);
			balanceUnits.AddTransaction(productKey1, -10m);
			balanceUnits.AddTransaction(productKey2, 10m);
			balanceUnits.AddTransaction(productKey3, 10m);
			balanceUnits.AddTransaction(productKey4, 10m);
			balanceUnits.AddTransaction(productKey5, 10m);
			balanceUnits.AddTransaction(productKey6, 10m);

			var unitsByProduct = balanceUnits.GetUnitsByProduct();
			AssertNotNull(unitsByProduct);
			if (calcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
			{
				AssertEquals(4, unitsByProduct.Count);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey1).Value, 30m, 30m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
			}
			else
			{
				AssertEquals(5, unitsByProduct.Count);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey1).Value, 20m, 20m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey5).Value, 10m, 10m, 10m);
			}

			var unitsByProductLocation = balanceUnits.GetUnitsByProductLocation();
			AssertNotNull(unitsByProductLocation);

			if (calcMethod == OrgCompanyDataLookups.WarehouseSplitPeriodBilling)
			{
				AssertEquals(4, unitsByProductLocation.Count);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey1).Value, 30m, 30m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
			}
			else
			{
				AssertEquals(6, unitsByProductLocation.Count);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey1).Value, 10m, 20m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey2).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey5).Value, 10m, 10m, 10m);
				AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey6).Value, 10m, 10m, 10m);
			}
		}

		#endregion

		#region TestClassBalanceUnitsTable_AddPalletIDTransaction

		public void TestClassBalanceUnitsTable_AddPalletIDTransaction_WarehouseStorageMax()
		{
			TestClassBalanceUnitsTable_AddPalletIDTransactionCore(OrgCompanyDataLookups.WarehouseStorageMax);
		}

		public void TestClassBalanceUnitsTable_AddPalletIDTransaction_WarehouseStorageClosingBalance()
		{
			TestClassBalanceUnitsTable_AddPalletIDTransactionCore(OrgCompanyDataLookups.WarehouseStorageClosingBalance);
		}

		public void TestClassBalanceUnitsTable_AddPalletIDTransaction_WarehouseStoragePeak()
		{
			TestClassBalanceUnitsTable_AddPalletIDTransactionCore(OrgCompanyDataLookups.WarehouseStoragePeak);
		}

		public void TestClassBalanceUnitsTable_AddPalletIDTransaction_WarehouseSplitPeriodBilling()
		{
			TestClassBalanceUnitsTable_AddPalletIDTransactionCore(OrgCompanyDataLookups.WarehouseSplitPeriodBilling);
		}

		void TestClassBalanceUnitsTable_AddPalletIDTransactionCore(string calcMethod)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			var client2 = Helper.CreateClient("CL2");
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationB1 = whs2.FindLocation("B-1");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var productAttributeMeasure = new ProductAttributesMeasure();
			var productAttributeMeasure2 = new ProductAttributesMeasure("AA");

			var productKey1 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, ZGuid.Empty, ZGuid.Empty, ProductAttributesMeasure.Empty, "P01");
			var productKey2 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, ZGuid.Empty, ZGuid.Empty, ProductAttributesMeasure.Empty, "P01");
			var productKey3 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, client2.PK, ZGuid.Empty, ZGuid.Empty, ProductAttributesMeasure.Empty, "P02");
			var productKey4 = new WhsInvoiceRatingAdapter.UnitsTableKey(whs2.PK, data.Org1.PK, ZGuid.Empty, ZGuid.Empty, ProductAttributesMeasure.Empty, "P02");
			var productKey5 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, ZGuid.Empty, ZGuid.Empty, ProductAttributesMeasure.Empty, "");

			var balanceUnits = new WhsInvoiceRatingAdapter.BalanceUnitsTable(calcMethod);
			balanceUnits.AddPalletIDTransaction(productKey1, 10m);
			balanceUnits.AddPalletIDTransaction(productKey1, 10m);
			balanceUnits.AddPalletIDTransaction(productKey1, 0m);
			balanceUnits.AddPalletIDTransaction(productKey1, -10m);
			balanceUnits.AddPalletIDTransaction(productKey2, 10m);
			balanceUnits.AddPalletIDTransaction(productKey3, 10m);
			balanceUnits.AddPalletIDTransaction(productKey4, 10m);
			balanceUnits.AddPalletIDTransaction(productKey5, 10m);

			var unitsByPalletID = balanceUnits.GetUnitsByPalletID();
			AssertNotNull(unitsByPalletID);
			AssertEquals(4, unitsByPalletID.Count);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey1).Value, 20m, 20m, 10m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey3).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey4).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPalletID.Single(p => p.Key == productKey5).Value, 10m, 10m, 10m);
		}

		#endregion

		#region TestClassBalanceUnitsTable_RemoveNegativeValues

		public void TestClassBalanceUnitsTable_RemoveNegativeValues()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var productAttributeMeasure = new ProductAttributesMeasure();

			var productKey1 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part1.PK, productAttributeMeasure, "P01");
			var productKey2 = new WhsInvoiceRatingAdapter.UnitsTableKey(data.Whs1.PK, data.Org1.PK, locationA1.PK, data.Part2.PK, productAttributeMeasure, "P02");

			var balanceUnits = new WhsInvoiceRatingAdapter.BalanceUnitsTable(OrgCompanyDataLookups.WarehouseStoragePeak);
			balanceUnits.AddBalance(productKey1, 10m);
			balanceUnits.AddBalance(productKey2, -10m);

			var unitsByProduct = balanceUnits.GetUnitsByProduct();
			AssertNotNull(unitsByProduct);
			AssertEquals(2, unitsByProduct.Count);
			AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey2).Value, -10m, -10m, -10m);

			var unitsByProductLocation = balanceUnits.GetUnitsByProductLocation();
			AssertNotNull(unitsByProductLocation);
			AssertEquals(2, unitsByProductLocation.Count);
			AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey2).Value, -10m, -10m, -10m);

			var unitsByPallet = balanceUnits.GetUnitsByPalletID();
			AssertNotNull(unitsByPallet);
			AssertEquals(2, unitsByProductLocation.Count);
			AssertBalanceUnits(unitsByPallet.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPallet.Single(p => p.Key == productKey2).Value, -10m, -10m, -10m);

			balanceUnits.RemoveNegativeValues();

			AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByProduct.Single(p => p.Key == productKey2).Value, 0m, 0m, 0m);

			AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByProductLocation.Single(p => p.Key == productKey2).Value, 0m, 0m, 0m);

			AssertBalanceUnits(unitsByPallet.Single(p => p.Key == productKey1).Value, 10m, 10m, 10m);
			AssertBalanceUnits(unitsByPallet.Single(p => p.Key == productKey2).Value, 0m, 0m, 0m);
		}

		#endregion

		void AssertBalanceUnits(WhsInvoiceRatingAdapter.BalanceUnits balanceUnits, ZDecimal expectedCurrentUnits, ZDecimal expectedMaxUnits, ZDecimal expectedMinUnits)
		{
			AssertEquals("CurrentUnits", expectedCurrentUnits, balanceUnits.CurrentUnits);
			AssertEquals("MaximumUnits", expectedMaxUnits, balanceUnits.MaximumUnits);
			AssertEquals("MinimumUnits", expectedMinUnits, balanceUnits.MinimumUnits);
		}

		#endregion

		#endregion

		#region TestDBHits_Measures

		public void TestDBHits_Measures()
		{
			var warehouse = Helper.CreateWarehouse("AAAA", "A", 2, 2);
			var year = ZDateTime.Now.Year - 1;

			for (int i = 0; i < 10; i++)
			{
				var product = CreateProductWithManyConversoins("AA" + i);
				CreateWhsReceive(warehouse, Org1, new ZDateTimeOffset(year, 2, 5, 11, 0, 0), true, new PartUnit(product, 10));
			}

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = warehouse.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 11);

			Factory.Save();

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ JobStorageSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
			};

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);
			var autoRating = GetIAutoRating(invoiceInOtherFactory);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;

			using (RowFactory.SetCachedTables())
			{
				foreach (MeasureType measureType in System.Enum.GetValues(typeof(MeasureType)))
				{
					_ = rateableMeasures.GetActual(measureType);
				}
			}

			AssertDbHits(expectedDBHitsForValidation, otherFactory);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures

		#region TestIAutoRatingFreightInfo_Measures

		public void TestIAutoRatingFreightInfo_Measures()
		{
			var year = ZDateTime.Now.Year - 1;
			SetUpData();
			Factory.Save();

			CreateWhsReceive(Whs2, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 11, 0, 0), true, new PartUnit(Part1, 15), new PartUnit(Part1, 5));
			CreateWhsReceive(Whs1, Org2, new ZDateTimeOffset(year, 2, 5, 11, 0, 0), true, new PartUnit(Part1, 50));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 11, 0, 0), true, new PartUnit(Part2, 35));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 10, 11, 0, 0), true, new PartUnit(Part1, 45));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 10, 11, 0, 0), false, new PartUnit(Part1, 20));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs2, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 3, 11, 0, 0), new PartUnit(Part1, 10));
			CreateFinalisedWhsOrder(Whs1, Org2, new ZDateTimeOffset(year, 2, 7, 11, 0, 0), new PartUnit(Part1, 40));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 15, 0, 0), new PartUnit(Part1, 10), new PartUnit(Part2, 15));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 3, 1, 11, 0, 0), new PartUnit(Part2, 20));

			var invoice = Factory.New<WhsInvoice>();
			var autoRating = GetIAutoRating(invoice);
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 2);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 9);

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(55m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(35m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
			AssertEquals(20m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.Unit, "GEN"));
			AssertEquals(35m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.Unit, "HAZ"));

			AssertEquals("KG", rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals("M3", rateableMeasures.GetUnit(MeasureType.Volume));
			AssertEquals(9.07m, Utilities.Round(rateableMeasures.UnitsByProduct_ForTest(MeasureType.Weight, Part1.PK), 2));
			AssertEquals(350m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Weight, Part2.PK));
			AssertEquals(0.02m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Volume, Part1.PK));
			AssertEquals(0.35m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Volume, Part2.PK));

			Assert(!rateableMeasures.MeasureHasLocation(MeasureType.Unit));
			Assert(rateableMeasures.MeasureHasLocation(MeasureType.LocationPallet));
			AssertEquals(7.5m, rateableMeasures.GetActual(MeasureType.LocationPallet));
			AssertEquals(1.875m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, "A-1-1")); // =15/8
			AssertEquals(0.625m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, "A-1-2")); // =5/8
			AssertEquals(5m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, "A-2-2")); // =35/7
			AssertEquals(0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, "B-1-1"));
			AssertEquals(2.50m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.LocationPallet, "GEN"));
			AssertEquals(5m, rateableMeasures.UnitsByCommodity_ForTest(MeasureType.LocationPallet, "HAZ"));

			invoice.ET_StorageToDate = new ZDateTime(year, 2, 10);
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(100m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(65m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(35m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStoragePeak;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(80m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(45m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(45m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs1.PK, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs2.PK, Part1.PK));
			AssertEquals(35m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 10);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(65m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(45m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
			Assert(rateableMeasures.ContainsProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(45m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(45m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(45m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs1.PK, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs2.PK, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
			Assert(!rateableMeasures.ContainsProduct_ForTest(MeasureType.Unit, Part2.PK));

			invoice.ET_WW = Whs1.PK;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(45m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(45m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs1.PK, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs2.PK, Part1.PK));

			invoice.ET_WW = Whs2.PK;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Unit));
			AssertEquals(0m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs1.PK, Part1.PK));
			AssertEquals(5m, rateableMeasures.UnitsByWarehouseAndProduct_ForTest(MeasureType.Unit, Whs2.PK, Part1.PK));

			invoice.ET_WW = ZGuid.Empty;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 3, 1);
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_LoadOnlyNeededData

		[TestDate(2017, 1, 1)]
		public void TestIAutoRatingFreightInfo_Measures_LoadOnlyNeededData()
		{
			var year = ZDateTime.Now.Year - 1;
			var whs = Helper.CreateWarehouse("Whs", "A", 3, 1);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var client = Helper.CreateClient("C01", "client");
			client.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			var productStoreInJan = Helper.CreateProduct(client, "ProductA");
			var productStoreInFeb = Helper.CreateProduct(client, "ProductB");
			var productStoreInMar = Helper.CreateProduct(client, "ProductC");
			Helper.CreateProductUnit(productStoreInJan, productStoreInJan.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 1m);
			Helper.CreateProductUnit(productStoreInFeb, productStoreInFeb.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 1m);
			Helper.CreateProductUnit(productStoreInMar, productStoreInMar.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 1m);
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			var location2 = whs.FindLocation("A-2");
			var location3 = whs.FindLocation("A-3");
			StoreInWarehouseForOneDay(whs, client, productStoreInJan, "1", location1, new ZDateTime(year, 1, 2));
			StoreInWarehouseForOneDay(whs, client, productStoreInFeb, "2", location2, new ZDateTime(year, 2, 2));
			StoreInWarehouseForOneDay(whs, client, productStoreInMar, "3", location3, new ZDateTime(year, 3, 2));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var invoice = newFactory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = whs.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 28);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, productStoreInFeb.PK));
			AssertContainsExactElementsInAnyOrder("Should only load product B. Does not need to load product A and C.", new ZGuid[] { productStoreInFeb.PK }, newFactory.Load<OrgSupplierPart>(new ZQuery { FetchOnlyFromLocalCache = true }).Select(p => p.PK));
			AssertContainsExactElementsInAnyOrder("Should only load location A-2. Does not to load location A-1 and A-3.", new ZGuid[] { location2.PK }, newFactory.Load<WhsLocation>(new ZQuery { FetchOnlyFromLocalCache = true }).Select(l => l.PK));
		}

		void StoreInWarehouseForOneDay(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, string refNumber, WhsLocation location, ZDateTime date)
		{
			Helper.CreateWhsReceiveWithInventory(client, whs, "R" + refNumber, date.ToOffset(), product, 1m, location, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, "O" + refNumber, date.AddDays(1).ToOffset(), product, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_UniquePallet

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "A", 2, 1);
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			var whs1FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new DateTime(year, 8, 31));
			var whs2FinalisedDate = whs2.GetWarehouseBranchDateTimeOffset(new DateTime(year, 8, 31));

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA2, "P02");
			// Empty Pallet ID, not involved in calculation
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = whs1FinalisedDate;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m, locationA2, "P02");
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = whs1FinalisedDate;

			// Different client, not included in calculation
			var receive3 = Helper.CreateWhsReceive(org2, data.Whs1, "R3");
			Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, locationA1, "P03");
			receive3.FinaliseDocketWithoutUserConfirmation();
			receive3.WD_FinalisedDate = whs1FinalisedDate;

			// Different warehouse, not included in calculation
			var receive4 = Helper.CreateWhsReceive(data.Org1, whs2, "R4");
			Helper.CreateWhsReceiveLine(receive4, data.Part1, 20m, whs2.DefaultLocation, "P04");
			receive4.FinaliseDocketWithoutUserConfirmation();
			receive4.WD_FinalisedDate = whs2FinalisedDate;

			// Out of period range, not included in calculation
			var receive5 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R5");
			Helper.CreateWhsReceiveLine(receive5, data.Part1, 10m, locationA1, "P05");
			Helper.CreateWhsReceiveLine(receive5, data.Part1, 10m, locationA2, "P06");
			receive5.FinaliseDocketWithoutUserConfirmation();
			receive5.WD_FinalisedDate = whs1FinalisedDate.AddMonths(1);

			// Out of period range but still on hand, should be included in calculation
			var receive6 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R6");
			Helper.CreateWhsReceiveLine(receive6, data.Part1, 10m, locationA1, "P07");
			receive6.FinaliseDocketWithoutUserConfirmation();
			receive6.WD_FinalisedDate = whs1FinalisedDate.AddMonths(-1);

			// Adjust in and in the period range, should be included in calculation
			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 10m, locationA1.ToLocationString(), palletID: "P08");
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = whs1FinalisedDate;

			Factory.Save();

			// Transfer is in the period range, should be included in calculation
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "P01", locationA2.ToLocationString(), "P09");

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transfer.WD_FinalisedDate = whs1FinalisedDate;
			transferLine.WE_FinalisedDate = whs1FinalisedDate;
			Factory.Save();

			// Inter-Whs transfer is in the period range, should be inculded in calculation
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR2", transferType: TransferType.Codes.InterWhsDest);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, whs2.DefaultLocation.ToLocationString(), whs2.PK, locationA1.ToLocationString());
			transferLine2.WE_TransferFromPalletId = "P04";
			transferLine2.WE_PalletID = "P10";
			transfer2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer2);

			transfer2.WD_FinalisedDate = whs1FinalisedDate;
			transferLine2.WE_FinalisedDate = whs2FinalisedDate;
			transfer2.ChildTransfers.Single().WD_FinalisedDate = whs1FinalisedDate;
			transferLine2.ChildTransferLine.WE_FinalisedDate = whs2FinalisedDate;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 1, "P07");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 6, "P01", "P02", "P07", "P08", "P09", "P10");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 6, "P01", "P02", "P07", "P08", "P09", "P10");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 6, "P01", "P02", "P07", "P08", "P09", "P10");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletIDSharedWithDifferentClients()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA2, "P02");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, locationA1, "P01");
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			// only charge for Client1
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 2, "P01", "P02");

			var invoice2 = Factory.New<WhsInvoice>();
			invoice2.ET_OH_Client = client2.PK;
			invoice2.ET_WW = data.Whs1.PK;
			invoice2.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice2.ET_StorageToDate = new ZDateTime(year, 8, 31);

			// only charge for Client2
			var companyData2 = client2.CompanyData;
			companyData2.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData2, invoice2, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData2, invoice2, OrgCompanyDataLookups.WarehouseStorageMax, 1, "P01");
			AssertUniquePalletCount(companyData2, invoice2, OrgCompanyDataLookups.WarehouseStoragePeak, 1, "P01");
			AssertUniquePalletCount(companyData2, invoice2, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P01");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_WithFreeStorage()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine11 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, locationA2, "P02");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 25));

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, locationA1, "P03");
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 28));

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, locationA1, "P04");
			receive3.FinaliseDocketWithoutUserConfirmation();
			receive3.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 30));
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 15m);
			orderLine1.ReserveStockIfAbleTo(receiveLine11.Inventory[0], 10m);
			orderLine1.ReserveStockIfAbleTo(receiveLine2.Inventory[0], 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 15m);
			orderLine1.ReserveStockIfAbleTo(receiveLine3.Inventory[0], 10m);
			orderLine1.ReserveStockIfAbleTo(receiveLine2.Inventory[0], 5m);
			var pick = Helper.CreatePickNew(order1, order2);
			order1.FinaliseDocketWithoutUserConfirmation();
			order1.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 29, 10, 00, 00));
			order2.FinaliseDocketWithoutUserConfirmation();
			order2.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 31, 10, 00, 00));
			pick.FinalisePick();
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;

			companyData.OB_WhsClientFreeStorageDays = 0;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			// P01 + P02 + P03 + P04
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 4, "P01", "P02", "P03", "P04");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 4, "P01", "P02", "P03", "P04");
			// P02
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 1;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 4, "P01", "P02", "P03", "P04");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 4, "P01", "P02", "P03", "P04");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 2;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 3;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 4;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 5;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 1, "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 1, "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 6;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 1, "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 1, "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");

			companyData.OB_WhsClientFreeStorageDays = 7;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 0);
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_WithFreeStorage_OutNotInPeriod()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, locationA2, "P02");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 25));
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locationA1.ToLocationString(), palletID: "P01");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 8, 26));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 25);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			companyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Daily;

			// P01 + P02
			companyData.OB_WhsClientFreeStorageDays = 0;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 2, "P01", "P02");

			// P01 has been adjust out, P02 is free storage, the result should be 0
			companyData.OB_WhsClientFreeStorageDays = 1;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 0);
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_WithHeldInventory()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			receiveLine1.IsInventoryEditForm = true;
			receiveLine1.HeldCodeChangeQuantity = 5m;
			receiveLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine1.ChangeInventoryHeldCode(true);

			var statusChangeLine = Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));
			AssertNotNull("Precondition: HoldCode Changed.", statusChangeLine);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, locationA1.ToLocationString(), palletID: "P01");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 2, "P01", "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasOutBeforePeriod()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 7, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 7, 20);
			pick.FinalisePick();
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 1, "P01");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 1, "P01");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 1, "P01");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P01");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasExistsBeforePeriodButOutInPeriod()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 7, 1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC2", null);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, locationA1, "P03");
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 26);
			Factory.Save();

			// Release out P01 & P03
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			pick.FinalisePick();
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 1, "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 3, "P01", "P02", "P03");

			companyData.OB_WhsChargeStorageInAdvance = true;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 1, "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasReleased()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			pick.FinalisePick();
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_WithInTransitInventory()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 8, 31);
			}
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 2, "P01", "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasAdjustedOut()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, locationA1.ToLocationString(), palletID: "P01");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasInternalWarehouseAdustment()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 7, 31);
			Factory.Save();

			var internalAdjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ1");
			internalAdjustmentIn.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(internalAdjustmentIn, data.Part1, 10m, locationA1.ToLocationString(), palletID: "P03");
			internalAdjustmentIn.FinaliseDocketWithoutUserConfirmation();
			internalAdjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			adjustmentLine.WE_AdjustmentArrivalDate = internalAdjustmentIn.WD_FinalisedDate;
			Factory.Save();

			var internalAdjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ2");
			internalAdjustmentOut.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			Helper.CreateWhsAdjustmentLine(internalAdjustmentOut, data.Part1, -10m, locationA1.ToLocationString(), palletID: "P01");
			internalAdjustmentOut.FinaliseDocketWithoutUserConfirmation();
			internalAdjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 0, 0);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 3, "P01", "P02", "P03");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 2, "P02", "P03");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasOwnershipAdjustedOut()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var ownershipAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, org2, null);
			Helper.CreateWhsAdjustmentLine(ownershipAdjustment, data.Part1, -10m, locationA1.ToLocationString(), palletID: "P01");
			ownershipAdjustment.FinaliseDocketWithoutUserConfirmation();
			ownershipAdjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasTransferedToAnotherWhs()
		{
			var year = ZDateTime.Today.Year;
			var outwardFinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var destLocationB1 = whs2.FindLocation("B-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR", transferType: TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), whs2.PK, destLocationB1.ToLocationString());
			transferLine.WE_TransferFromPalletId = "P01";
			transferLine.WE_PalletID = "P01";
			transfer.RunPreSaveValidation(); // commit transfer line
			transferLine.PickedTime = outwardFinalisedDate;
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			transfer.WD_FinalisedDate = outwardFinalisedDate;
			transferLine.WE_FinalisedDate = outwardFinalisedDate;
			transfer.ChildTransfers.Single().WD_FinalisedDate = outwardFinalisedDate;
			transferLine.ChildTransferLine.WE_FinalisedDate = outwardFinalisedDate;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasTransferedToAnotherWhs_OnlyFinalisedTransferLine()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("JGU", "B", 2, 1);
			var outwardFinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var destLocationB1 = whs2.FindLocation("B-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR", transferType: TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), whs2.PK, destLocationB1.ToLocationString());
			transferLine.WE_TransferFromPalletId = "P01";
			transferLine.WE_PalletID = "P03";
			transfer.RunPreSaveValidation(); // commit transfer line
			transferLine.PickedTime = outwardFinalisedDate;
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition: Transfer is not finalised.", false, transfer.IsFinalised);
			AssertIsFinalisedPrecondition(transferLine);

			transferLine.WE_FinalisedDate = outwardFinalisedDate;
			transferLine.ChildTransferLine.WE_FinalisedDate = outwardFinalisedDate;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_MultipleTransfer()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var finalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 0, 0);
			var finalisedDate2 = finalisedDate.AddHours(1);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "P01", locationA2.ToLocationString(), "P03");
			transfer.RunPreSaveValidation(); // commit transfer line
			transferLine.PickedTime = finalisedDate;

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transfer.WD_FinalisedDate = finalisedDate;
			transferLine.WE_FinalisedDate = finalisedDate;
			Factory.Save();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, locationA2.ToLocationString(), "P03", locationA1.ToLocationString(), "P04");
			transfer2.RunPreSaveValidation(); // commit transfer line
			transferLine2.PickedTime = finalisedDate2;

			transfer2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer2);
			transfer2.WD_FinalisedDate = finalisedDate2;
			transferLine2.WE_FinalisedDate = finalisedDate2;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 4, "P01", "P02", "P03", "P04");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 4, "P01", "P02", "P03", "P04");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 2, "P02", "P04");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasTransferedWithoutPalletID()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var outwardFinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "P01", locationA2.ToLocationString(), "");
			transfer.RunPreSaveValidation(); // commit transfer line
			transferLine.PickedTime = outwardFinalisedDate;

			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transfer.WD_FinalisedDate = outwardFinalisedDate;
			transferLine.WE_FinalisedDate = outwardFinalisedDate;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		public void TestIAutoRatingFreightInfo_Measures_UniquePallet_PalletHasTransferedWithoutPalletID_OnlyFinalisedTransferLine()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var outwardFinalisedDate = new ZDateTimeOffset(year, 8, 31, 10, 00, 00);
			Factory.Save();

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA1, "P01");
			Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, locationA2, "P02");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 8, 31);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TFR");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "P01", locationA2.ToLocationString(), "");
			transfer.RunPreSaveValidation(); // commit transfer line
			transferLine.PickedTime = outwardFinalisedDate;
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition: Transfer is not finalised.", false, transfer.IsFinalised);
			AssertIsFinalisedPrecondition(transferLine);
			transferLine.WE_FinalisedDate = outwardFinalisedDate;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 8, 25);
			invoice.ET_StorageToDate = new ZDateTime(year, 8, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 2, "P01", "P02");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "P02");
		}

		void AssertUniquePalletCount(OrgCompanyData companyData, WhsInvoice invoice, ZString calcMethod, int expectedCount, params string[] expectedPalletIDs)
		{
			companyData.OB_ARWhsStorageCalcMethod = calcMethod;

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			var palletIdList = rateableMeasures.GetPalletIds_ForTest().ToList();
			AssertEquals(expectedCount, palletIdList.Distinct().Count());
			AssertContainsExactElementsInAnyOrder(expectedPalletIDs, palletIdList);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_WithProductAttributes

		[TestDate(2007, 6, 1)]
		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes()
		{
			SetUpData();

			SetClientAttributesType(Org1, true, true, true, false, false);
			SetProductAttributesUse(Org1, Part1, true, true, true, false, false);

			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 10), true, new PartUnit(Part1, 10, "A1", "B1", "C1"));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 11), true, new PartUnit(Part1, 10, "A1", "B1", "C1"));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 12), true, new PartUnit(Part1, 30, "A1", "B1", "C2"));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 13), true, new PartUnit(Part1, 40, "A1", "B2", "C2"));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 14), true, new PartUnit(Part1, 50, "A2", "B2", "C2"));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 21), new PartUnit(Part1, 11, "A1", "B1", "C1"));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 22), new PartUnit(Part1, 22, "A1", "B1", "C2"));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 23), new PartUnit(Part1, 33, "A1", "B2", "C2"));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 24), new PartUnit(Part1, 20, "A2", "B2", "C2"));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 25), new PartUnit(Part1, 24, "A2", "B2", "C2"));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2007, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2007, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(140m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1"));

			Org1.MiscServ.OM_IMAttrib1IsKey = true;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(90m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1"));
			AssertEquals(50m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A2"));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1"));

			Org1.MiscServ.OM_IMAttrib2IsKey = true;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(50m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1"));
			AssertEquals(40m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B2"));
			AssertEquals(50m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A2", "B2"));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1", "C1"));

			Org1.MiscServ.OM_IMAttrib3IsKey = true;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1", "C1"));
			AssertEquals(30m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1", "C2"));
			AssertEquals(40m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B2", "C2"));
			AssertEquals(50m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A2", "B2", "C2"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(9m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1", "C1"));
			AssertEquals(8m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B1", "C2"));
			AssertEquals(7m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A1", "B2", "C2"));
			AssertEquals(6m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "A2", "B2", "C2"));

			Org1.MiscServ.OM_IMAttrib1IsKey = false;
			Org1.MiscServ.OM_IMAttrib2IsKey = false;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(9m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, "C1"));
			AssertEquals(21m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, "C2"));
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_WithSerialNumber()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();
			Helper.SetClientAttributeType(Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(Org1, Whs1, NextExternalReference, new TestNotificationBuffer());
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, Part1, 1);
			line1.WI_PartAttrib1 = "PA1";
			line1.WI_SerialNumber = "SN1";

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 10);

			var receive2 = Helper.CreateWhsReceive(Org1, Whs1, NextExternalReference, new TestNotificationBuffer());
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive2, Part1, 1);
			line2.WI_PartAttrib1 = "PA2";
			line2.WI_SerialNumber = "SN2";

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 11);

			var receive3 = Helper.CreateWhsReceive(Org1, Whs1, NextExternalReference, new TestNotificationBuffer());
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive3, Part1, 1);
			line3.WI_PartAttrib1 = "PA3";
			line3.WI_SerialNumber = "SN3";

			receive3.AllocateLocationsWithMock();
			receive3.FinaliseDocket();
			receive3.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 12);

			Factory.Save();

			var order = Helper.CreateWhsOrder(Org1, Whs1, NextExternalReference);
			order.WD_RequiredDate = new ZDateTimeOffset(year, 1, 25);
			order.ConsigneePK = Org1.PK;
			order.ConsigneeAddressPK = Org1.MainAddress.PK;

			var line = Helper.CreateWhsOrderLine(order, Part1, 1m);
			line.WE_PartAttrib1 = "PA2";
			line.WE_SerialNumber = "SN2";

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 25);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 25);
			}

			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(3m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));

			Org1.MiscServ.OM_IMSerialNumberIsKey = true;
			var rateableMeasures2 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, null, "SN1"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, null, "SN2"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, null, "SN3"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures3 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, null, "SN1"));
			AssertEquals(0m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, null, "SN2"));
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, null, null, null, "SN3"));
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_WorkOrder()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			var bikeFrame = Helper.CreateProduct(Org1, "FRAME");
			SetupProduct(bikeFrame, Org2);

			var bikeWheel = Helper.CreateProduct(Org1, "WHEEL");
			SetupProduct(bikeWheel, Org2);

			var bomBike = Helper.CreateProduct(Org1, "BIKE");
			SetupProduct(bomBike, Org2);

			Helper.SetClientAttributeType(Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, bikeFrame, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, bikeFrame, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(Org1, bikeFrame, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(Org1, bikeFrame, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, bikeWheel, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, bikeWheel, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(Org1, bikeWheel, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(Org1, bikeWheel, AttributeNumber.Serial, true);
			Helper.CreateProductBOM(bomBike, bikeFrame);
			Helper.CreateProductBOM(bomBike, bikeWheel, 2m, bikeWheel.OP_StockKeepingUnit);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(Org1, Whs1, NextExternalReference, new TestNotificationBuffer());
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, bikeFrame, 1m);
			line1.WI_PartAttrib1 = "PA1";
			line1.WI_PartAttrib2 = "PA2";
			line1.WI_PartAttrib3 = "PA3";
			line1.WI_SerialNumber = "SN1";
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive1, bikeWheel, 1m);
			line2.WI_PartAttrib1 = "PA2";
			line2.WI_PartAttrib2 = "PA3";
			line2.WI_PartAttrib3 = "PA4";
			line2.WI_SerialNumber = "SN2";
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive1, bikeWheel, 1m);
			line3.WI_PartAttrib1 = "PA3";
			line3.WI_PartAttrib2 = "PA4";
			line3.WI_PartAttrib3 = "PA5";
			line3.WI_SerialNumber = "SN3";

			var date = new ZDateTimeOffset(year, 1, 10);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			receive1.WD_FinalisedDate = date;
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(Org1, Whs1, bomBike, 1m);
			var workOrderPick = Helper.CreatePickNew(workOrder);

			// Pick the line
			foreach (var pickLine in workOrder.Lines[0].ChildComponentLines.SelectMany(cl => cl.PickLines).ToArray())
			{
				pickLine.WZ_PickedDateTime = date;
			}

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.WD_FinalisedDate = date;
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(workOrderPick);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			Org1.MiscServ.OM_IMAttrib1IsKey = true;
			Org1.MiscServ.OM_IMAttrib2IsKey = true;
			Org1.MiscServ.OM_IMAttrib3IsKey = true;
			Org1.MiscServ.OM_IMSerialNumberIsKey = true;
			var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, bikeFrame.PK, "PA1", "PA2", "PA3", "SN1"));
			AssertEquals(1m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, bikeWheel.PK, "PA2", "PA3", "PA4", "SN2"));
			AssertEquals(1m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, bikeWheel.PK, "PA3", "PA4", "PA5", "SN3"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures2 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(0m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, bikeFrame.PK, "PA1", "PA2", "PA3", "SN1"));
			AssertEquals(0m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, bikeWheel.PK, "PA2", "PA3", "PA4", "SN2"));
			AssertEquals(0m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, bikeWheel.PK, "PA3", "PA4", "PA5", "SN3"));
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_Adjustments()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			Helper.SetClientAttributeType(Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var date = new ZDateTimeOffset(year, 1, 10);
			var adjustment1 = Helper.CreateWhsAdjustment(Org1, Whs1, "A1");
			Helper.CreateWhsAdjustmentLine(adjustment1, Part1.PK, 1m, Whs1.DefaultLocation.WLV_LocationString, "PLT1", date, "", "ATT1", "ATT2", "ATT3", "SN1", ZDate.Empty, ZDate.Empty);
			adjustment1.FinaliseDocketWithoutUserConfirmation();
			adjustment1.WD_FinalisedDate = date;

			var adjustment2 = Helper.CreateWhsAdjustment(Org1, Whs1, "A2");
			Helper.CreateWhsAdjustmentLine(adjustment2, Part1.PK, 1m, Whs1.DefaultLocation.WLV_LocationString, "PLT1", date, "", "ATT1", "ATT2", "ATT3", "SN2", ZDate.Empty, ZDate.Empty);
			adjustment2.FinaliseDocketWithoutUserConfirmation();
			adjustment2.WD_FinalisedDate = date;

			var adjustment3 = Helper.CreateWhsAdjustment(Org1, Whs1, "A3");
			Helper.CreateWhsAdjustmentLine(adjustment3, Part1.PK, 1m, Whs1.DefaultLocation.WLV_LocationString, "PLT1", date, "", "ATT1", "ATT2", "ATT3", "SN3", ZDate.Empty, ZDate.Empty);
			adjustment3.FinaliseDocketWithoutUserConfirmation();
			adjustment3.WD_FinalisedDate = date;
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(3m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));

			Org1.MiscServ.OM_IMAttrib1IsKey = true;
			Org1.MiscServ.OM_IMAttrib2IsKey = true;
			Org1.MiscServ.OM_IMAttrib3IsKey = true;
			Org1.MiscServ.OM_IMSerialNumberIsKey = true;
			var rateableMeasures2 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ATT1", "ATT2", "ATT3", "SN1"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ATT1", "ATT2", "ATT3", "SN2"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ATT1", "ATT2", "ATT3", "SN3"));
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_Transfers()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();
			Factory.Save();

			var locationA1 = Whs1.FindLocation("A-1-1");
			var locationA2 = Whs1.FindLocation("A-1-2");

			Helper.SetClientAttributeType(Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Three, true);

			var date = new ZDateTimeOffset(year, 1, 10);
			var receive = Helper.CreateWhsReceive(Org1, Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, Part1.PK, 1m, locationA1.PK, "PLT1", ZDate.Empty, ZDate.Empty, "ABC1", "ABC2", "ABC3", "SN1", "");
			Helper.CreateWhsReceiveLine(receive, Part1.PK, 1m, locationA1.PK, "PLT2", ZDate.Empty, ZDate.Empty, "ABC2", "ABC3", "ABC4", "SN2", "");
			Helper.CreateWhsReceiveLine(receive, Part1.PK, 1m, locationA1.PK, "PLT3", ZDate.Empty, ZDate.Empty, "ABC3", "ABC4", "ABC5", "SN3", "");
			Helper.CreateWhsReceiveLine(receive, Part1.PK, 1m, locationA1.PK, "PLT4", ZDate.Empty, ZDate.Empty, "ABC4", "ABC5", "ABC6", "SN4", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = date;
			Factory.Save();

			var transferFinalised = Helper.CreateWhsTransfer(Org1, Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transferFinalised, Part1, 1m, "A-1-1", "A-1-2");
			transferLine.WE_TransferFromPalletId = "PLT1";
			transferLine.WE_PalletID = "PLT1";
			transferLine.WE_PartAttrib1 = "ABC1";
			transferLine.WE_PartAttrib2 = "ABC2";
			transferLine.WE_PartAttrib3 = "ABC3";
			transferLine.WE_SerialNumber = "SN1";
			transferFinalised.Lines[0].PickedTime = date;
			transferFinalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferFinalised);
			transferFinalised.WD_FinalisedDate = date;
			transferFinalised.Lines[0].WE_FinalisedDate = date;
			Factory.Save();

			var transferPartiallyFinalised = Helper.CreateWhsTransfer(Org1, Whs1, "TR2");
			var transferLineFinalised = Helper.CreateWhsTransferLine(transferPartiallyFinalised, Part1, 1m, "A-1-1", "A-1-2");
			transferLineFinalised.WE_TransferFromPalletId = "PLT2";
			transferLineFinalised.WE_PalletID = "PLT2";
			transferLineFinalised.WE_PartAttrib1 = "ABC2";
			transferLineFinalised.WE_PartAttrib2 = "ABC3";
			transferLineFinalised.WE_PartAttrib3 = "ABC4";
			transferLineFinalised.WE_SerialNumber = "SN2";
			transferLineFinalised.PickedTime = date;
			transferLineFinalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineFinalised);
			transferLineFinalised.WE_FinalisedDate = date;

			var transferLinePickedNotFinalised = Helper.CreateWhsTransferLine(transferPartiallyFinalised, Part1, 1m, "A-1-1", "A-1-2");
			transferLinePickedNotFinalised.WE_TransferFromPalletId = "PLT3";
			transferLinePickedNotFinalised.WE_PalletID = "PLT3";
			transferLinePickedNotFinalised.WE_PartAttrib1 = "ABC3";
			transferLinePickedNotFinalised.WE_PartAttrib2 = "ABC4";
			transferLinePickedNotFinalised.WE_PartAttrib3 = "ABC5";
			transferLinePickedNotFinalised.WE_SerialNumber = "SN3";
			transferLinePickedNotFinalised.RunPreSaveValidation(); // to commit inventory
			transferLinePickedNotFinalised.PickedTime = date;
			Factory.Save();

			var transferNotFinalised = Helper.CreateWhsTransfer(Org1, Whs1, "TR3");
			var transferLineNotFinalised = Helper.CreateWhsTransferLine(transferNotFinalised, Part1, 1m, "A-1-1", "A-1-2");
			transferLineNotFinalised.WE_TransferFromPalletId = "PLT4";
			transferLineNotFinalised.WE_PalletID = "PLT4";
			transferLineNotFinalised.WE_PartAttrib1 = "ABC4";
			transferLineNotFinalised.WE_PartAttrib2 = "ABC5";
			transferLineNotFinalised.WE_PartAttrib3 = "ABC6";
			transferLineNotFinalised.WE_SerialNumber = "SN4";
			transferNotFinalised.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(4m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));

			Org1.MiscServ.OM_IMAttrib1IsKey = true;
			Org1.MiscServ.OM_IMAttrib2IsKey = true;
			Org1.MiscServ.OM_IMAttrib3IsKey = true;
			Org1.MiscServ.OM_IMSerialNumberIsKey = true;
			var rateableMeasures2 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC2", "ABC3", "ABC4", "SN2"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC3", "ABC4", "ABC5", "SN3"));
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC4", "ABC5", "ABC6", "SN4"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures3 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC2", "ABC3", "ABC4", "SN2"));
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC3", "ABC4", "ABC5", "SN3"));
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC4", "ABC5", "ABC6", "SN4"));
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_InterWhsTransfers()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();
			Factory.Save();

			var whs1Location = Whs1.DefaultLocation;
			var whs2Location = Whs2.DefaultLocation;

			Helper.SetClientAttributeType(Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Three, true);

			var finalisedDate = Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 1, 1));
			var receive = Helper.CreateWhsReceive(Org1, Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, Part1.PK, 1m, whs1Location.PK, "PLT1", ZDate.Empty, ZDate.Empty, "ABC1", "ABC2", "ABC3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(Org1, Whs1, "T1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var line = Helper.CreateWhsTransferLine(transfer, Part1.PK, 1m, whs1Location.ToLocationString(), "PLT1", Whs2.PK, whs2Location.ToLocationString(), "PLT1", ZDateTimeOffset.Empty, ZDate.Empty, ZDate.Empty, "ABC1", "ABC2", "ABC3");
			line.WE_SerialNumber = "SN1";

			transfer.FinaliseDocket();
			transfer.WD_FinalisedDate = finalisedDate;
			AssertEquals(true, transfer.IsFinalised);
			Factory.Save();

			var invoice1 = Factory.New<WhsInvoice>();
			invoice1.ET_OH_Client = Org1.PK;
			invoice1.ET_WW = Whs1.PK;
			invoice1.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice1.ET_StorageToDate = new ZDateTime(year, 12, 31);

			var iAutoRating = GetIAutoRating(invoice1);
			var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));

			Org1.MiscServ.OM_IMAttrib1IsKey = true;
			Org1.MiscServ.OM_IMAttrib2IsKey = true;
			Org1.MiscServ.OM_IMAttrib3IsKey = true;
			Org1.MiscServ.OM_IMSerialNumberIsKey = true;
			var rateableMeasures2 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures3 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(0m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));

			var invoice2 = Factory.New<WhsInvoice>();
			invoice2.ET_OH_Client = Org1.PK;
			invoice2.ET_WW = Whs2.PK;
			invoice2.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice2.ET_StorageToDate = new ZDateTime(year, 12, 31);

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;
			var iAutoRating2 = GetIAutoRating(invoice2);
			var rateableMeasures21 = (RateableMeasureSet)iAutoRating2.RateableMeasures;
			AssertEquals(1m, rateableMeasures21.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures22 = (RateableMeasureSet)iAutoRating2.RateableMeasures;
			AssertEquals(1m, rateableMeasures22.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));
		}

		public void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_UnfinalisedInterWhsTransfer()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();
			Factory.Save();

			var whs1Location = Whs1.DefaultLocation;
			var whs2Location = Whs2.DefaultLocation;

			Helper.SetClientAttributeType(Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.Three, true);

			var finalisedDate = Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 1, 1));
			var receive = Helper.CreateWhsReceive(Org1, Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, Part1.PK, 1m, whs1Location.PK, "PLT1", ZDate.Empty, ZDate.Empty, "ABC1", "ABC2", "ABC3", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(Org1, Whs1, "T1");
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsSource;
			var sourceLine = Helper.CreateWhsTransferLine(transfer, Part1.PK, 1m, whs1Location.ToLocationString(), "PLT1", Whs2.PK, whs2Location.ToLocationString(), "PLT1", ZDateTimeOffset.Empty, ZDate.Empty, ZDate.Empty, "ABC1", "ABC2", "ABC3");
			sourceLine.WE_SerialNumber = "SN1";

			sourceLine.PickedTime = finalisedDate;
			AssertEquals("Source Transfer Line should not be Finalised.", false, sourceLine.IsFinalised);

			var destinationTransfer = transfer.ChildTransfers.Single();
			var destinationLine = destinationTransfer.Lines.Single();
			AssertEquals("Destination Transfer Line should not be Finalised.", false, destinationLine.IsFinalised);
			Factory.Save();

			var invoice1 = Factory.New<WhsInvoice>();
			invoice1.ET_OH_Client = Org1.PK;
			invoice1.ET_WW = Whs1.PK;
			invoice1.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice1.ET_StorageToDate = new ZDateTime(year, 12, 31);

			var iAutoRating = GetIAutoRating(invoice1);
			var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));

			Org1.MiscServ.OM_IMAttrib1IsKey = true;
			Org1.MiscServ.OM_IMAttrib2IsKey = true;
			Org1.MiscServ.OM_IMAttrib3IsKey = true;
			Org1.MiscServ.OM_IMSerialNumberIsKey = true;
			var rateableMeasures2 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures3 = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures3.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));

			var invoice2 = Factory.New<WhsInvoice>();
			invoice2.ET_OH_Client = Org1.PK;
			invoice2.ET_WW = Whs2.PK;
			invoice2.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice2.ET_StorageToDate = new ZDateTime(year, 12, 31);

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;
			var iAutoRating2 = GetIAutoRating(invoice2);
			var rateableMeasures21 = (RateableMeasureSet)iAutoRating2.RateableMeasures;
			AssertEquals(0m, rateableMeasures21.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			var rateableMeasures22 = (RateableMeasureSet)iAutoRating2.RateableMeasures;
			AssertEquals(0m, rateableMeasures22.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK, "ABC1", "ABC2", "ABC3", "SN1"));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_WithInvalidSmallDateTime

		public void TestIAutoRatingFreightInfo_Measures_WithInvalidSmallDateTime()
		{
			SetUpData();
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;

			var date = new ZDateTime(1812, 9, 7);
			invoice.ET_StorageFromDate = date;
			Factory.Save();
			Assert("Should NOT have sent a silent error report on set of a SmallDateTime", ErrorReporter.TotalErrorCount == 0);

			var autoRating1 = GetIAutoRating(invoice);
			var rateableMeasures1 = (RateableMeasureSet)autoRating1.RateableMeasures;
			AssertNotNull("Should not blow up when invoked with a date smaller than smalldatetime.Min.", rateableMeasures1);

			date = new ZDateTime(2112, 9, 7);
			invoice.ET_StorageFromDate = date;
			Factory.Save();
			Assert("Should NOT have sent a silent error report on set of a SmallDateTime", ErrorReporter.TotalErrorCount == 0);

			var autoRating2 = GetIAutoRating(invoice);
			var rateableMeasures2 = (RateableMeasureSet)autoRating2.RateableMeasures;
			AssertNotNull("Should not blow up when invoked with a date greater than smalldatetime.Max.", rateableMeasures2);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_Order

		public void TestIAutoRatingFreightInfo_Measures_Order()
		{
			TestIAutoRatingFreightInfo_Measures_OrderCore(usingInTransitTransfer: false, (data) =>
			{
				var year = ZDateTime.Now.Year;
				var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 2);

				// After some of the stock above is picked/finalised, receive some new stock. 
				// This will test the query in GetUnitsPerPeriod as billing is actually based on maximum over the period.
				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
				receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 6);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
				return (receive1, order, orderLine);
			});
		}

		public void TestIAutoRatingFreightInfo_Measures_Order_WithInTransitTransfers()
		{
			TestIAutoRatingFreightInfo_Measures_OrderCore(usingInTransitTransfer: true, (data) =>
			{
				var year = ZDateTime.Now.Year;
				var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 2);

				// After some of the stock above is picked/finalised, receive some new stock. 
				// This will test the query in GetUnitsPerPeriod as billing is actually based on maximum over the period.
				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
				receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 6);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
				return (receive1, order, orderLine);
			});
		}

		public void TestIAutoRatingFreightInfo_Measures_WorkOrder()
		{
			TestIAutoRatingFreightInfo_Measures_OrderCore(usingInTransitTransfer: false, (data) =>
			{
				var year = ZDateTime.Now.Year;
				var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 2);

				// After some of the stock above is picked/finalised, receive some new stock. 
				// This will test the query in GetUnitsPerPeriod as billing is actually based on maximum over the period.
				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
				receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 6);
				Factory.Save();

				Helper.CreateProductBOM(data.Part2, data.Part1, 1m, data.Part1.OP_StockKeepingUnit);
				var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
				var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 8m);
				return (receive1, workOrder, workOrderLine.ChildComponentLines.Single());
			});
		}

		public void TestIAutoRatingFreightInfo_Measures_DynamicWorkOrder()
		{
			TestIAutoRatingFreightInfo_Measures_OrderCore(usingInTransitTransfer: false, (data) =>
			{
				var year = ZDateTime.Now.Year;
				data.Whs1.WW_IsVirtualWarehouse = true;
				var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
				var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
				inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
				inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
				Factory.Save();

				var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				receive1.WD_IsInwardsProcessingJob = true;
				receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
				var recLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, inwardProcessingLocation);
				recLine1.CustomsData.WB_EntryKey = "ENT1";
				receive1.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive1);
				receive1.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 2);

				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
				receive2.WD_IsInwardsProcessingJob = true;
				receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
				var recLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, inwardProcessingLocation);
				recLine2.CustomsData.WB_EntryKey = "ENT2";
				receive2.FinaliseDocket();
				Factory.Save();
				AssertIsFinalisedPrecondition(receive2);
				receive2.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 6);
				// After some of the stock above is picked/finalised, receive some new stock. 
				// This will test the query in GetUnitsPerPeriod as billing is actually based on maximum over the period.

				var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
				dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
				dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
				dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
				dynamicWorkOrder.ConsigneeAddressPK = data.Org1.MainAddress.PK;

				var kitLine = Factory.New<WhsDynamicWorkOrderLine>();
				var componentLine = Factory.New<WhsDynamicWorkOrderLine>();

				kitLine.WE_WD = dynamicWorkOrder.PK;
				kitLine.WE_OP = data.Part2.PK;
				kitLine.WE_TransactionQuantity = 8m;
				kitLine.WE_F3_NKPackType = data.Part2.OP_StockKeepingUnit;
				kitLine.IsMainInwardProcessedItem = true;

				componentLine.WE_WD = dynamicWorkOrder.PK;
				componentLine.WE_OP = data.Part1.PK;
				componentLine.WE_TransactionQuantity = 8m;
				componentLine.WE_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
				componentLine.WE_WE_ParentDocketLine = kitLine.PK;

				return (receive1, dynamicWorkOrder, componentLine);
			});
		}

		void TestIAutoRatingFreightInfo_Measures_OrderCore(bool usingInTransitTransfer, Func<TestDataSimpleEnvironment, (WhsReceive, WhsPickableDocket, WhsPickableDocketLine)> createReceiveAndOrder)
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "PLT";
			Factory.Save();

			var (receive, order, orderLine) = createReceiveAndOrder(data);

			var pick = Helper.CreatePickNew(order);

			// Pick the line / create In-Transit Transfer
			var pickLine = pick.GetAllPickLines().Single();

			WhsTransferLine transferLine = null;
			if (usingInTransitTransfer)
			{
				transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
				Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);

				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);

			// Finalise transfer, pick/finalise the dock door stock
			if (usingInTransitTransfer)
			{
				transferLine.FinaliseDocketLine();
				transferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 4);
				Factory.Save();

				var rateableMeasures1 = (RateableMeasureSet)iAutoRating.RateableMeasures;
				AssertEquals("Should add back the In-Transit Inventory.", 20m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
				AssertEquals("Should add back the In-Transit Inventory.", 20m, rateableMeasures1.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
				AssertEquals("Should not include inventory in the dock door (it's shown in the source location).", 0m, rateableMeasures1.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString));
			}

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			order.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 5);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			// MAX
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("No longer In-Transit, but should include the finalised order.", 20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("No longer In-Transit, but should include the finalised order.", 20m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should not include the dock door as it's included in the source location.", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString));

			// Closing Balance
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("No longer In-Transit, closing balance should be 12m.", 12m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("No longer In-Transit, closing balance should be 12m.", 12m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should not show any stock in the dock door location.", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString));

			// Peak (happens to be closing balance, but this tests changes made to the query involving OriginalPickedInventory
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStoragePeak;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("No longer In-Transit, closing balance should be 2m.", 12m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("No longer In-Transit, closing balance should be 2m.", 12m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should not show any stock in the dock door location.", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_Order_InTransit

		public void TestIAutoRatingFreightInfo_Measures_Order_InTransit_WithoutTransfer()
		{
			TestIAutoRatingFreightInfo_Measures_Order_InTransitCore(usingInTransitTransfer: false);
		}

		public void TestIAutoRatingFreightInfo_Measures_Order_InTransit_WithTransfer()
		{
			TestIAutoRatingFreightInfo_Measures_Order_InTransitCore(usingInTransitTransfer: true);
		}

		void TestIAutoRatingFreightInfo_Measures_Order_InTransitCore(bool usingInTransitTransfer)
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "PLT";
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 2);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line / create In-Transit Transfer
			var pickLine = pick.GetAllPickLines().Single();

			if (usingInTransitTransfer)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
				Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 2);

				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, order.Lines[0].PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			if (usingInTransitTransfer)
			{
				AssertEquals("Should add back the In-Transit Inventory.", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
				AssertEquals("Should add back the In-Transit Inventory.", 10m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			}
			AssertEquals("Should not include inventory in the dock door (it's shown in the source location).", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString));

			// Closing balance should also add back the In-Transit Inventory
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			if (usingInTransitTransfer)
			{
				AssertEquals("Should add back the In-Transit Inventory.", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
				AssertEquals("Should add back the In-Transit Inventory.", 10m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			}
			AssertEquals("Should not include inventory in the dock door (it's shown in the source location).", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_WithFreeStorageDays

		[TestDate(2007, 6, 1)]
		public void TestIAutoRatingFreightInfo_Measures_WithFreeStorageDays()
		{
			SetUpData();

			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 10), true, new PartUnit(Part1, 2));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 1, 12), true, new PartUnit(Part2, 20));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 12, 1, 0, 0), new PartUnit(Part1, 1));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 1, 15, 1, 0, 0), new PartUnit(Part2, 10));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2007, 1, 11);
			invoice.ET_StorageToDate = new ZDateTime(2007, 1, 16);
			var autoRating = GetIAutoRating(invoice);

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			Org1.CompanyData.OB_WhsClientFreeStorageDays = 0;
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 4;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 5;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 6;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 7;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStoragePeak;
			Org1.CompanyData.OB_WhsClientFreeStorageDays = 0;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 2;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 3;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(20m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 4;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 5;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 6;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 7;
			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime_InnerTransfer

		public void TestIAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime_InnerTransfer()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Part1.OP_StockKeepingUnit = "PLT";
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 2);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 4m, locationA1, locationA2);
			transfer.RunPreSaveValidation();
			var pickLine = transferLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 29);
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transfer.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 2);
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year, 2, 2);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("Should add back the In-Transit Inventory to source location.", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should add back the In-Transit Inventory to source location.", 10m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should not include inventory in location A2.", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, locationA2.WLV_LocationString));

			// Closing balance should also add back the In-Transit Inventory
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("Should add back the In-Transit Inventory to source location.", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should add back the In-Transit Inventory to source location.", 10m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should not include inventory in location A2.", 0m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, locationA2.WLV_LocationString));

			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 28);

			iAutoRating = GetIAutoRating(invoice);
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("Should add back the In-Transit Inventory to source location.", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should **not** add back the In-Transit to source location.", 6m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should include inventory in location A2.", 4m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, locationA2.WLV_LocationString));

			// Closing balance should also add back the In-Transit Inventory
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("Should sum all inventories in all locations.", 10m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should **not** add back the In-Transit Inventory to source location.", 6m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, receive.Lines[0].LocationString));
			AssertEquals("Should have inventory in location A2 after finalising transfer.", 4m, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, locationA2.WLV_LocationString));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime_InterWhsTransfers

		public void TestIAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime_InterWhsTransfers()
		{
			var year = ZDateTime.Now.Year;
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var whs3 = Helper.CreateWarehouse("WH3", "C");
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			part.OP_StockKeepingUnit = "PLT";
			Factory.Save();
			var locationA = whs1.FindLocation("A");
			var locationB = whs2.FindLocation("B");
			var locationC = whs3.FindLocation("C");

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 100m, locationA, "");
			receive.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 10, 1));
			Factory.Save();

			var transferInterWhsSource = Helper.CreateWhsTransfer(client, whs1, "TR2", transferType: TransferType.Codes.InterWhsSource);
			var transferInterWhsSourceLine = Helper.CreateWhsTransferLine(transferInterWhsSource, part, 15m, "A", whs2.PK, "B");
			transferInterWhsSource.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 15m, transferInterWhsSourceLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsSourceLine.PickedTime = whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 10, 28));

			var transferInterWhsDest = Helper.CreateWhsTransfer(client, whs3, "TR3", transferType: TransferType.Codes.InterWhsDest);
			var transferInterWhsDestLine = Helper.CreateWhsTransferLine(transferInterWhsDest, part, 25m, "A", whs1.PK, "C");
			transferInterWhsDest.RunPreSaveValidation();
			AssertEquals("Precondition - Committed stock to transfer.", 25m, transferInterWhsDestLine.QtyCommittedIncludingMatchingLines);
			transferInterWhsDestLine.PickedTime = whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 10, 28));

			transferInterWhsSourceLine.FinaliseDocketLine();
			transferInterWhsDestLine.FinaliseDocketLine();

			var finalisedDate = whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(year, 11, 2));
			transferInterWhsSourceLine.WE_FinalisedDate = finalisedDate;
			transferInterWhsSourceLine.ChildTransferLine.WE_FinalisedDate = finalisedDate;
			transferInterWhsDestLine.WE_FinalisedDate = finalisedDate;
			transferInterWhsDestLine.ChildTransferLine.WE_FinalisedDate = finalisedDate;
			Factory.Save();
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine);
			AssertIsFinalisedPrecondition(transferInterWhsDestLine);
			Factory.Save();

			// WarehouseStorageMax 
			var message = "Should add back the In-Transit Inventory to source location.";
			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageMax, client, part
							, locationA, fromDate: new ZDateTime(year, 10, 1), toDate: new ZDateTime(year, 10, 31), expectedInventoryInLocation: 100m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageMax, client, part
							, locationB, fromDate: new ZDateTime(year, 10, 1), toDate: new ZDateTime(year, 10, 31), expectedInventoryInLocation: 0m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageMax, client, part
							, locationC, fromDate: new ZDateTime(year, 10, 1), toDate: new ZDateTime(year, 10, 31), expectedInventoryInLocation: 0m);

			// WarehouseStorageClosingBalance 
			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageClosingBalance, client, part
							, locationA, fromDate: new ZDateTime(year, 10, 1), toDate: new ZDateTime(year, 10, 31), expectedInventoryInLocation: 100m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageClosingBalance, client, part
							, locationB, fromDate: new ZDateTime(year, 10, 1), toDate: new ZDateTime(year, 10, 31), expectedInventoryInLocation: 0m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageClosingBalance, client, part
							, locationC, fromDate: new ZDateTime(year, 10, 1), toDate: new ZDateTime(year, 10, 31), expectedInventoryInLocation: 0m);

			// WarehouseStorageMax 
			message = "Should move In-Transit Inventory to desc location.";
			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime("In starting period still 100 product sould be in source location.", OrgCompanyDataLookups.WarehouseStorageMax, client, part
							, locationA, fromDate: new ZDateTime(year, 11, 1), toDate: new ZDateTime(year, 11, 30), expectedInventoryInLocation: 100m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageMax, client, part
							, locationB, fromDate: new ZDateTime(year, 11, 1), toDate: new ZDateTime(year, 11, 30), expectedInventoryInLocation: 15m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageMax, client, part
							, locationC, fromDate: new ZDateTime(year, 11, 1), toDate: new ZDateTime(year, 11, 30), expectedInventoryInLocation: 25m);

			// WarehouseStorageClosingBalance 
			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageClosingBalance, client, part
							, locationA, fromDate: new ZDateTime(year, 11, 1), toDate: new ZDateTime(year, 11, 30), expectedInventoryInLocation: 60m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageClosingBalance, client, part
							, locationB, fromDate: new ZDateTime(year, 11, 1), toDate: new ZDateTime(year, 11, 30), expectedInventoryInLocation: 15m);

			Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(message, OrgCompanyDataLookups.WarehouseStorageClosingBalance, client, part
							, locationC, fromDate: new ZDateTime(year, 11, 1), toDate: new ZDateTime(year, 11, 30), expectedInventoryInLocation: 25m);
		}

		void Assert_IAutoRatingFreightInfo_Measures_WhenFinaliseDateIsDifferentToPickTime(string message, string storageCalcMethod, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDateTime fromDate, ZDateTime toDate, decimal expectedInventoryInLocation)
		{
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = location.Warehouse.PK;
			invoice.ET_StorageFromDate = fromDate;
			invoice.ET_StorageToDate = toDate;
			var iAutoRating = GetIAutoRating(invoice);
			client.CompanyData.OB_ARWhsStorageCalcMethod = storageCalcMethod;
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(message, expectedInventoryInLocation, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, part.PK));
			AssertEquals(message, expectedInventoryInLocation, rateableMeasures.UnitsByLocation_ForTest(MeasureType.LocationPallet, location.WLV_LocationString));
			AssertEquals(message, expectedInventoryInLocation, rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, part.PK));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_StorageChargeShoudNotSplitByDocket

		public void TestIAutoRatingFreightInfo_Measures_StorageChargeShoudNotSplitByDocket()
		{
			var today = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-10), true, new PartUnit(data.Part1, 15));
			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-30).Date, today.AddDays(-1).Date);

			var autoRating = GetIAutoRating(invoice);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			foreach (MeasureType measureType in System.Enum.GetValues(typeof(MeasureType)))
			{
				if (rateableMeasures.HasMeasureType(measureType))
				{
					AssertEquals(false, rateableMeasures.MeasureHasDocketReference(measureType));
				}
			}
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_Time

		public void TestIAutoRatingFreightInfo_Measures_Time()
		{
			SetUpData();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2009, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2009, 1, 31);

			var iAutoRating = GetIAutoRating(invoice);
			var timeInfo = ((RateableMeasureSet)iAutoRating.RateableMeasures).Time;
			AssertEquals("31 days (01-Jan-09 - 31-Jan-09)", timeInfo.ToString(TimeInfo.Exclusion.PublicHolidays));

			Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			timeInfo = ((RateableMeasureSet)iAutoRating.RateableMeasures).Time;
			AssertEquals("31 days (01-Jan-09 - 31-Jan-09)", timeInfo.ToString(TimeInfo.Exclusion.PublicHolidays));

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			Org1.CompanyData.OB_WhsChargeStorageInAdvance = true;
			timeInfo = ((RateableMeasureSet)iAutoRating.RateableMeasures).Time;
			AssertEquals("If we charge in advance with Split Period Billing, then rate for future month", "28 days (01-Feb-09 - 28-Feb-09)", timeInfo.ToString(TimeInfo.Exclusion.PublicHolidays));

			Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			timeInfo = ((RateableMeasureSet)iAutoRating.RateableMeasures).Time;
			AssertEquals("If we DON'T charge in advance, than it is still current month", "31 days (01-Jan-09 - 31-Jan-09)", timeInfo.ToString(TimeInfo.Exclusion.PublicHolidays));
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_StartFebruary()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2009, 2, 1), "31 days (01-Mar-09 - 31-Mar-09)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_MidFebruary()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2009, 2, 15), "31 days (15-Mar-09 - 14-Apr-09)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_EndFebruary()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2009, 2, 28), "31 days (28-Mar-09 - 27-Apr-09)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_StartJan()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2009, 1, 1), "28 days (01-Feb-09 - 28-Feb-09)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_MidJanuary()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2009, 1, 15), "28 days (15-Feb-09 - 14-Mar-09)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_EndJanuary()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2009, 1, 31), "28 days (28-Feb-09 - 27-Mar-09)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_StartJan_LeapYear()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2008, 1, 1), "29 days (01-Feb-08 - 29-Feb-08)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_MidJanuary_LeapYear()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2008, 1, 15), "29 days (15-Feb-08 - 14-Mar-08)");
		}

		public void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvance_EndJanuary_LeapYear()
		{
			TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(new ZDateTime(2008, 1, 31), "29 days (29-Feb-08 - 28-Mar-08)");
		}

		void TestIAutoRatingFreightInfo_Measures_Time_ChargeStorageInAdvanceCore(ZDateTime startDate, string expectedOutput)
		{
			SetUpData();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = startDate;

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Org1.CompanyData.OB_WhsChargeStorageInAdvance = true;

			var iAutoRating = GetIAutoRating(invoice);
			var timeInfo = ((RateableMeasureSet)iAutoRating.RateableMeasures).Time;

			AssertEquals(expectedOutput, timeInfo.ToString(TimeInfo.Exclusion.PublicHolidays));
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_LazyLoaded

		public void TestIAutoRatingFreightInfo_Measures_LazyLoaded()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.ToOffset().AddDays(-10), data.Part1, 10m);
			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-8), today.AddDays(-1));
			Factory.Save();

			var autoRating = GetIAutoRating(invoice);
			var hits = GetPersistentPropertiesHitCount(() =>
			{
				_ = autoRating.RateableMeasures;
			});
			AssertEquals("Poking measures should NOT trigger many property hits.", true, hits < 20);
		}

		public void TestIAutoRatingFreightInfo_Measures_LazyLoaded_CachingAcrossMeasures()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today.ToOffset().AddDays(-10), data.Part1, 10m);
			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-8), today.AddDays(-1));
			Factory.Save();

			var autoRating = GetIAutoRating(invoice);
			var hits = GetPersistentPropertiesHitCount(() =>
			{
				var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
				foreach (MeasureType measureType in System.Enum.GetValues(typeof(MeasureType)))
				{
					_ = rateableMeasures.GetActual(measureType);
					_ = rateableMeasures.GetPartCount(measureType);
				}
			});
			AssertEquals("Poking measures and triggering calculations should NOT trigger many property hits.", true, hits < 750);
		}

		#endregion

		#region TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment

		public void TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment_BeforeInvoice()
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location, "P01");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year - 1, 12, 25);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 5m, location.ToLocationString(), "ABC", receive.WD_ArrivalDate);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -5m, location.ToLocationString(), "P01", receive.WD_ArrivalDate);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 14);
			AssertIsFinalisedPrecondition(adjustment);
			Assert("Precondition", adjustment.Lines.All(l => l.WE_AdjustmentArrivalDate == receive.WD_ArrivalDate));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			companyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Daily;

			companyData.OB_WhsClientFreeStorageDays = 0;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 1, "ABC");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, 1, "ABC");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, 1, "ABC");
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, 1, "ABC");
		}

		public void TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment_DuringInvoice()
		{
			TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment_DuringInvoiceCore(duringInvoice: true, new[] { "ABC" });
		}

		public void TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment_NotSet()
		{
			var noResult = Array.Empty<string>();
			// will finalised after invoice duration
			TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment_DuringInvoiceCore(duringInvoice: false, noResult);
		}

		void TestIAutoRatingFreightInfo_Measures_RenamePalletByInternalWarehouseAdjustment_DuringInvoiceCore(bool duringInvoice, string[] expectedResult)
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location, "P01");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year - 1, 12, 25);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			var arrivalDate = duringInvoice ? new ZDateTimeOffset(year, 1, 14) : ZDateTimeOffset.Empty;
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 5m, location.ToLocationString(), "ABC", arrivalDate);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -5m, location.ToLocationString(), "P01", receive.WD_ArrivalDate);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 14);
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 1, 31);

			var companyData = data.Org1.CompanyData;
			companyData.OB_WhsChargeStorageInAdvance = false;
			companyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			companyData.OB_WhsClientFreeStorageDays = 0;
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseSplitPeriodBilling, 0, Array.Empty<string>()); // WarehouseSplitPeriodBilling ignore internal adjustments.
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageMax, expectedResult.Length, expectedResult);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStoragePeak, expectedResult.Length, expectedResult);
			AssertUniquePalletCount(companyData, invoice, OrgCompanyDataLookups.WarehouseStorageClosingBalance, expectedResult.Length, expectedResult);
		}

		#endregion

		#endregion

		#region TestIAutoRatingWarehouseInfo

		public void TestIAutoRatingWarehouseInfo_WarehousePK()
		{
			var year = ZDateTime.Now.Year - 1;
			SetUpData();
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 11, 0, 0), true, new PartUnit(Part1, 15), new PartUnit(Part1, 5));
			CreateWhsReceive(Whs1, Org2, new ZDateTimeOffset(year, 2, 5, 11, 0, 0), true, new PartUnit(Part1, 50));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 11, 0, 0), true, new PartUnit(Part2, 35));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 10, 11, 0, 0), true, new PartUnit(Part1, 45));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 10, 11, 0, 0), false, new PartUnit(Part1, 20));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 3, 11, 0, 0), new PartUnit(Part1, 10));
			CreateFinalisedWhsOrder(Whs1, Org2, new ZDateTimeOffset(year, 2, 7, 11, 0, 0), new PartUnit(Part1, 40));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 9, 15, 0, 0), new PartUnit(Part1, 10), new PartUnit(Part2, 15));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 3, 1, 11, 0, 0), new PartUnit(Part2, 20));

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 2);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 9);
			var autoRating = GetIAutoRating(invoice);

			AssertEquals(Whs1.PK, ((IAutoRatingWarehouseInfo)autoRating).WarehousePK);
		}

		public void TestIAutoRatingWarehouseInfo_WarehouseFallbackConsignorForFilterOnly()
		{
			var year = ZDateTime.Now.Year - 1;
			SetUpData();
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			var autoRating = GetIAutoRating(invoice);

			AssertEquals(null, ((IAutoRatingWarehouseInfo)autoRating).WarehouseFallbackConsignorForFilterOnly);
		}

		#endregion

		#region TestIAutoRating_StatusInformation

		public void TestIAutoRating_StatusInformation()
		{
			SetUpData();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = ZDateTime.Empty;
			invoice.ET_StorageToDate = ZDateTime.Empty;

			var iInvoice = GetIAutoRating(invoice);
			AssertEquals(false, iInvoice.StatusInformation.CanExecute);
			AssertEquals("Valid From and To dates are required before this Warehouse Periodic Invoice can be Auto Rated.", iInvoice.StatusInformation.Message);

			invoice.ET_StorageFromDate = ZDateTime.Invalid;
			invoice.ET_StorageToDate = new ZDateTime(2009, 1, 31);
			AssertEquals(false, iInvoice.StatusInformation.CanExecute);
			AssertEquals("Valid From and To dates are required before this Warehouse Periodic Invoice can be Auto Rated.", iInvoice.StatusInformation.Message);

			invoice.ET_StorageFromDate = new ZDateTime(2009, 1, 31);
			invoice.ET_StorageToDate = ZDateTime.Invalid;
			AssertEquals(false, iInvoice.StatusInformation.CanExecute);
			AssertEquals("Valid From and To dates are required before this Warehouse Periodic Invoice can be Auto Rated.", iInvoice.StatusInformation.Message);

			invoice.ET_StorageFromDate = new ZDateTime(2009, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2009, 1, 31);
			AssertEquals(true, iInvoice.StatusInformation.CanExecute);
		}

		public void TestStatusInformation_WhenWeightUnitIsInvalid()
		{
			SetUpData();

			Part1.OP_PartNum = "XYZ";

			CreateWhsReceive(Whs1, Org1, Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2016, 3, 1)), true, new PartUnit(Part1, 2));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2016, 3, 5)), new PartUnit(Part1, 1));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2016, 3, 1);
			invoice.ET_StorageToDate = new ZDateTime(2016, 3, 31);

			var rating = GetIAutoRating(invoice);

			Factory.Save();

			Part1.OP_StockKeepingUnit = "0";
			Part1.OP_WeightUQ = "0";
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(string.Empty, rating.StatusInformation.Message);

			var expectedErrorStr =
@"Invalid Weight Unit in this Product Code: XYZ. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Weight).Single());

			Part1.OP_StockKeepingUnit = "0";
			Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(true, string.IsNullOrEmpty(rating.StatusInformation.Message));
			rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.Weight));

			Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			Part1.OP_WeightUQ = "0";
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(true, string.IsNullOrEmpty(rating.StatusInformation.Message));
			rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.Weight));

			Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(true, string.IsNullOrEmpty(rating.StatusInformation.Message));

			rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.Weight));
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.Weight).Any());
		}

		public void TestStatusInformation_WhenVolumeUnitIsInvalid()
		{
			SetUpData();

			Part1.OP_PartNum = "XYZ";

			CreateWhsReceive(Whs1, Org1, Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2016, 3, 1)), true, new PartUnit(Part1, 2));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2016, 3, 5)), new PartUnit(Part1, 1));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2016, 3, 1);
			invoice.ET_StorageToDate = new ZDateTime(2016, 3, 31);

			var rating = GetIAutoRating(invoice);

			Factory.Save();

			Part1.OP_StockKeepingUnit = "0";
			Part1.OP_CubicUQ = "0";
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(string.Empty, rating.StatusInformation.Message);

			var expectedErrorStr =
@"Invalid Volume Unit in this Product Code: XYZ. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Volume).Single());
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Volume));

			Part1.OP_StockKeepingUnit = "0";
			Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(true, string.IsNullOrEmpty(rating.StatusInformation.Message));
			rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.Volume));

			Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			Part1.OP_CubicUQ = "0";
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(true, string.IsNullOrEmpty(rating.StatusInformation.Message));
			rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.Volume));

			Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(true, string.IsNullOrEmpty(rating.StatusInformation.Message));

			rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.Volume));
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.Volume).Any());
		}

		public void TestStatusInformation_WhenMultipleFieldsAreInvalid()
		{
			// Arrange

			SetUpData();

			Part1.OP_PartNum = "XYZ";
			Part1.OP_StockKeepingUnit = "0";
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2016, 3, 1), true, new PartUnit(Part1, 2));
			Factory.Save();
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2016, 3, 5), new PartUnit(Part1, 1));
			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;

			invoice.ET_StorageFromDate = new ZDateTime(2016, 3, 1);
			invoice.ET_StorageToDate = new ZDateTime(2016, 3, 31);
			Part1.OP_WeightUQ = "0";
			Part1.OP_CubicUQ = "0";

			var rating = GetIAutoRating(invoice);
			Factory.Save();

			// Act

			var status = rating.StatusInformation;

			// Assert

			AssertEquals(true, rating.StatusInformation.CanExecute);
			AssertEquals(string.Empty, rating.StatusInformation.Message);

			var expectedWeightErrorStr =
@"Invalid Weight Unit in this Product Code: XYZ. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var expectedVolumeErrorStr =
@"Invalid Volume Unit in this Product Code: XYZ. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var rateableMeasures = (RateableMeasureSet)rating.RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(expectedWeightErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Weight).Single());
			AssertEquals(expectedVolumeErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.Volume).Single());
		}

		#endregion

		#region TestIAutoRating_UnitsAndPalletsJobLevel

		[TestDate(2008, 1, 1)]
		public void TestIAutoRating_UnitsAndPalletsJobLevel()
		{
			SetUpData();
			// 8 UNT per PLT in Part 1
			// 7 UNT per PLT in Part 2

			var receiveStorage = Factory.New<AccChargeCode>();
			receiveStorage.AC_Code = "TSTINWSTG#";
			receiveStorage.AC_Desc = "Receive Storage";
			receiveStorage.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSStorage;
			receiveStorage.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 6, 10), true, new PartUnit(Part1, 800), new PartUnit(Part2, 700));

			Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			var invoiceJune = Factory.New<WhsInvoice>();
			invoiceJune.ET_OH_Client = Org1.PK;
			invoiceJune.ET_WW = Whs1.PK;
			invoiceJune.ET_StorageFromDate = new ZDateTime(2007, 6, 1);
			invoiceJune.ET_StorageToDate = new ZDateTime(2007, 6, 30);

			Factory.Save();

			var autoRating = GetIAutoRating(invoiceJune);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(1500m, rateableMeasures.GetActual(MeasureType.JobUnit));
			AssertEquals(200m, rateableMeasures.GetActual(MeasureType.ChargeablePallet));
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling

		[TestDate(2008, 1, 1)]
		public void TestIAutoRatingSplitPeriodBilling()
		{
			SetUpData();
			// 8 UNT per PLT in Part 1
			// 7 UNT per PLT in Part 2

			var receiveSimple = Factory.New<AccChargeCode>();
			receiveSimple.AC_Code = "TSTINWSPL#";
			receiveSimple.AC_Desc = "Receive Simple";
			receiveSimple.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;

			var receiveStorage = Factory.New<AccChargeCode>();
			receiveStorage.AC_Code = "TSTINWSTG#";
			receiveStorage.AC_Desc = "Receive Storage";
			receiveStorage.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			receiveStorage.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;

			// Part1:
			//            |               |
			//  +100      |  +20          |
			//----┴--┬----|---┴-----------|------
			//      -50   |               |
			//            |               |
			//    JUNE    |      JULY     |   AUGUST
			//

			// Part2:
			//            |               |
			//   +60      |  +140         |
			//----┴-------|---┴-----┬-----|-----
			//            |        -80    |
			//            |               |
			//    JUNE    |      JULY     |   AUGUST
			//

			var receive1 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 6, 10), true, new PartUnit(Part1, 100), new PartUnit(Part2, 60));
			var receive2 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 7, 12), true, new PartUnit(Part2, 140));
			receive2.WD_ExternalReference = "";

			var receive3 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 7, 15), true, new PartUnit(Part1, 20));
			Factory.Save();

			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 6, 15), new PartUnit(Part1, 50));
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 7, 20), new PartUnit(Part2, 80));
			Factory.Save();

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			Factory.Save();

			AssertSMB_June();
			AssertSMB_July();
			AssertSMB_August();

			var jobHeader1 = AddJobHeader(receive1);
			var charge1a = AddJobCharge(jobHeader1, receiveStorage);
			Factory.Save();

			AssertSMB_June();
			AssertSMB_July();
			AssertSMB_August();

			AddJobChargeAttrib(charge1a, JobChargeAttribTypeList.Codes.DocketLinePK, receive1.Lines[0].PK.ToString());
			AddJobChargeAttrib(charge1a, JobChargeAttribTypeList.Codes.DocketLinePK, "CRAP"); // causes exception if you don't convert PK to string before comparing
			Factory.Save();

			AssertSMB_June();
			AssertSMB_July();
			AssertSMB_August();

			var jobHeader3 = AddJobHeader(receive3);
			var charge3a = AddJobCharge(jobHeader3, receiveSimple);
			AddJobChargeAttrib(charge3a, JobChargeAttribTypeList.Codes.DocketLinePK, receive3.Lines[0].PK.ToString());
			Factory.Save();

			AssertSMB_June();
			AssertSMB_July();
			AssertSMB_August();

			var charge3b = AddJobCharge(jobHeader3, receiveStorage);
			AddJobChargeAttrib(charge3b, JobChargeAttribTypeList.Codes.DocketLinePK, receive3.Lines[0].PK.ToString());
			Factory.Save();

			AssertSMB_June();
			AssertSMB_July();
			AssertSMB_August();
		}

		#region AssertSMB_June

		void AssertSMB_June()
		{
			var otherFactory = new BusinessObjectFactory();
			var invoiceJune = otherFactory.New<WhsInvoice>();
			invoiceJune.ET_OH_Client = Org1.PK;
			invoiceJune.ET_WW = Whs1.PK;
			invoiceJune.ET_StorageFromDate = new ZDateTime(2007, 6, 1);
			invoiceJune.ET_StorageToDate = new ZDateTime(2007, 6, 30);

			var autoRating = GetIAutoRating(invoiceJune);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
			AssertEquals(0m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part1.PK));
			AssertEquals(0m, ((ZDecimal)rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part2.PK)).Round(2));
		}

		#endregion

		#region AssertSMB_July

		void AssertSMB_July()
		{
			var otherFactory = new BusinessObjectFactory();
			var invoiceJuly = otherFactory.New<WhsInvoice>();
			invoiceJuly.ET_OH_Client = Org1.PK;
			invoiceJuly.ET_WW = Whs1.PK;
			invoiceJuly.ET_StorageFromDate = new ZDateTime(2007, 7, 1);
			invoiceJuly.ET_StorageToDate = new ZDateTime(2007, 7, 31);

			var autoRating = GetIAutoRating(invoiceJuly);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(50m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(60m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
			AssertEquals("Should be 50 / 8 = 6.25", 6.25m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part1.PK));
			AssertEquals("Should be 60 / 7 = 8.57...", 8.57m, ((ZDecimal)rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part2.PK)).Round(2));
		}

		#endregion

		#region AssertSMB_August

		void AssertSMB_August()
		{
			var otherFactory = new BusinessObjectFactory();
			var invoiceAugust = otherFactory.New<WhsInvoice>();
			invoiceAugust.ET_OH_Client = Org1.PK;
			invoiceAugust.ET_WW = Whs1.PK;
			invoiceAugust.ET_StorageFromDate = new ZDateTime(2007, 8, 1);
			invoiceAugust.ET_StorageToDate = new ZDateTime(2007, 8, 31);

			var autoRating = GetIAutoRating(invoiceAugust);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(70m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals(120m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part2.PK));
			AssertEquals("Should be 70 / 8 = 8.75", 8.75m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part1.PK));
			AssertEquals("Should be 120 / 7 = 17.14...", 17.14m, ((ZDecimal)rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part2.PK)).Round(2));
		}

		#endregion

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_UsesMinimumStock

		[TestDate(2008, 1, 1)]
		public void TestIAutoRatingSplitPeriodBilling_UsesMinimumStock()
		{
			SetUpData();
			// Part1:
			//       |             |
			//  +10  |     +6      |
			//----┴--|--┬---┴------|---
			//       | -6          |
			//       |             |
			// JUNE  |    JULY     |
			//

			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 6, 20), true, new PartUnit(Part1, 10));
			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(2007, 7, 12), true, new PartUnit(Part1, 6));
			Factory.Save();
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(2007, 7, 3), new PartUnit(Part1, 6));
			Factory.Save();

			Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var invoiceJuly = Factory.New<WhsInvoice>();
			invoiceJuly.ET_OH_Client = Org1.PK;
			invoiceJuly.ET_WW = Whs1.PK;
			invoiceJuly.ET_StorageFromDate = new ZDateTime(2007, 7, 1);
			invoiceJuly.ET_StorageToDate = new ZDateTime(2007, 7, 31);
			var autoRating = GetIAutoRating(invoiceJuly);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Minimum stock for July should be 4", 4m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, Part1.PK));
			AssertEquals("Should be 4 / 8 = 0.5", 0.5m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.LocationPallet, Part1.PK));
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_WithChargeInAdvance

		public void TestIAutoRatingSplitPeriodBilling_WithChargeInAdvance()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			// Part1:
			// 
			//       |             |
			//  +10  | +5     +4   |
			//----┴--|--┴--┬---┴---|---
			//       |    -7       |
			//       |             |
			//       |             |
			// JUNE  |    JULY     |

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, data.Whs1.DefaultLocation);
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = new ZDateTimeOffset(2007, 6, 20);

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, data.Whs1.DefaultLocation);
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 12);

			var receive3 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 4, data.Whs1.DefaultLocation);
			receive3.FinaliseDocketWithoutUserConfirmation();
			receive3.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 25);

			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			helper.CreateWhsOrderLine(order, data.Part1, 7);
			var pick = helper.CreatePickNew(true, true, order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 20);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(2007, 7, 20);
			}

			Factory.Save();

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			// Charging in advance!
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = true;

			var invoiceJuly = Factory.New<WhsInvoice>();
			invoiceJuly.ET_OH_Client = data.Org1.PK;
			invoiceJuly.ET_WW = data.Whs1.PK;
			invoiceJuly.ET_StorageFromDate = new ZDateTime(2007, 7, 1);
			invoiceJuly.ET_StorageToDate = new ZDateTime(2007, 7, 31);
			var autoRating = GetIAutoRating(invoiceJuly);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Closing stock for July should be 12", 12m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should have 1 point with grouped data", 1, rateableMeasures.GetPartCount(MeasureType.Unit));
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_IgnoresAttributes

		public void TestIAutoRatingSplitPeriodBilling_IgnoresAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			// Part1:
			// 
			// blue  |  red        |
			//  +10  | +5          |
			//----┴--|--┴--┬-------|---
			//       |    -7       |
			//       |    blue     |
			//       |             |
			// JUNE  |    JULY     |

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = new ZDateTimeOffset(2007, 6, 20);

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 12);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			helper.CreateWhsOrderLine(order, data.Part1, 7, ZDate.Empty, ZDate.Empty, "BLUE", "", "", "", "");
			var pick = helper.CreatePickNew(true, true, order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 20);
			Factory.Save();

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var invoiceJuly = Factory.New<WhsInvoice>();
			invoiceJuly.ET_OH_Client = data.Org1.PK;
			invoiceJuly.ET_WW = data.Whs1.PK;
			invoiceJuly.ET_StorageFromDate = new ZDateTime(2007, 7, 1);
			invoiceJuly.ET_StorageToDate = new ZDateTime(2007, 7, 31);
			var autoRating = GetIAutoRating(invoiceJuly);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Minimum stock for July should be 8", 8m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should have 1 point with grouped data", 1, rateableMeasures.GetPartCount(MeasureType.Unit));
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_IgnoresLocations

		public void TestIAutoRatingSplitPeriodBilling_IgnoresLocations()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			// Part1:
			// 
			// Loc1  |  Loc2       |
			//  +10  | +5          |
			//----┴--|--┴--┬-------|---
			//       |    -7       |
			//       |    Loc1     |
			//       |             |
			// JUNE  |    JULY     |

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			receive1.WD_FinalisedDate = new ZDateTimeOffset(2007, 6, 20);

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, data.Whs1.FindLocation("A-2"));
			receive2.FinaliseDocketWithoutUserConfirmation();
			receive2.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 12);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			helper.CreateWhsOrderLine(order, data.Part1, 7);
			var pick = helper.CreatePickNew(true, true, order);
			order.WD_FinalisedDate = new ZDateTimeOffset(2007, 7, 20);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var invoiceJuly = Factory.New<WhsInvoice>();
			invoiceJuly.ET_OH_Client = data.Org1.PK;
			invoiceJuly.ET_WW = data.Whs1.PK;
			invoiceJuly.ET_StorageFromDate = new ZDateTime(2007, 7, 1);
			invoiceJuly.ET_StorageToDate = new ZDateTime(2007, 7, 31);
			var autoRating = GetIAutoRating(invoiceJuly);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Minimum stock for July should be 8", 8m, rateableMeasures.UnitsByProduct_ForTest(MeasureType.Unit, data.Part1.PK));
			AssertEquals("Should have 1 point with grouped data", 1, rateableMeasures.GetPartCount(MeasureType.Unit));
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_IgnoresInternalAdjustments

		public void TestIAutoRatingSplitPeriodBilling_IgnoresInternalAdjustments()
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			SetupChargesAndRateLinesForSplitMonthBillingWithAdjustments(data, year);

			//        |                      |                      |                      |
			// +5(REC)|                      | +5 (Internal ADJ IN) |                      |
			//-┴------|-------------┬--------|-----┴----------------|----------------------|
			//        |-3 (Internal ADJ OUT) |                      |                      |
			//        |        JAN           |          FEB         |          MAR         |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year - 1, 12, 25), data.Part1, 5m);
			Factory.Save();

			// In future it will be impossible to create internal adjustments with total by product != 0. When this happens
			// change the test and leave one adjustment with same "in" and "out" amounts.
			var internalAdjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			internalAdjustmentOut.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			var adjustmentLineOut = Helper.CreateWhsAdjustmentLine(internalAdjustmentOut, data.Part1.PK, -3m, "A");
			internalAdjustmentOut.FinaliseDocket();
			internalAdjustmentOut.WD_FinalisedDate = adjustmentLineOut.WE_FinalisedDate = new ZDateTimeOffset(year, 1, 20);
			AssertIsFinalisedPrecondition(internalAdjustmentOut);

			var internalAdjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			internalAdjustmentIn.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			var adjustmentLineIn = Helper.CreateWhsAdjustmentLine(internalAdjustmentIn, data.Part1.PK, 5m, "A");
			internalAdjustmentIn.FinaliseDocket();
			internalAdjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(year, 2, 13);
			adjustmentLineIn.WE_AdjustmentArrivalDate = internalAdjustmentIn.WD_FinalisedDate;
			AssertIsFinalisedPrecondition(internalAdjustmentIn);

			Factory.Save();

			var invoiceJanuary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var invoiceFebruary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 3, 1).AddDays(-1));
			var invoiceMarch = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();

			// Internal adjustments should be completely ignored for billing purposes

			// January should be 5 STO ($3 x 5). No charges for internal adjustments.
			invoiceJanuary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceJanuary.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Jan) is minimum for stock in Jan. 5 UNT * $3 = $15.", 15m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceJanuary))).JR_LocalSellAmt);

			// February should be 2 STO ($3 x 2). No charges for internal adjustments.
			invoiceFebruary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceFebruary.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Feb) is minimum for stock in Feb. 2 UNT * $3 = $6.", 6m, invoiceFebruary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceFebruary))).JR_LocalSellAmt);

			// March should be 7 STO ($3 x 7)
			invoiceMarch.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceMarch.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Mar) is minimum for stock in Mar. 7 UNT * $3 = $21.", 21m, invoiceMarch.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceMarch))).JR_LocalSellAmt);
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_IgnoresPutawayTransfers

		public void TestIAutoRatingSplitPeriodBilling_IgnoresPutawayTransfers()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 10m);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageClosingBalance;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = new ZDateTimeOffset(year - 1, 12, 25);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Factory.Save();

			// create Putaway Transfer to represent movement from Inbound Dock Door to Putaway Location
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, data.Whs1.DefaultLocation, "PLT-1", 10m);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			// set Picked time and finalised time manually
			transferLine.PickedTime = new ZDateTimeOffset(year - 1, 12, 24);
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transferLine.WE_FinalisedDate = new ZDateTimeOffset(year - 1, 12, 24);
			Factory.Save();

			// finalise goods into warehouse
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var invoiceBeforeStockReceipted = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year - 1, 11, 21), new ZDateTime(year - 1, 12, 21)); // place invoice period before goods are receipted.
			var autoRating1 = GetIAutoRating(invoiceBeforeStockReceipted);
			var rateableMeasures1 = (RateableMeasureSet)autoRating1.RateableMeasures;
			AssertEquals("No Chargeable Units should be found since no goods were receipted in invoice Period.", 0m, rateableMeasures1.UnitsByProduct_ForTest(MeasureType.LocationPallet, data.Part1.PK));
			AssertEquals("No Chargeable Units should be found since no goods were receipted in invoice Period.", 0m, rateableMeasures1.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultLocation.ToLocationString()));
			AssertEquals("No Chargeable Units should be found since no goods were receipted in invoice Period.", 0m, rateableMeasures1.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString()));

			var invoiceWhenStockReceipted = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year - 1, 12, 22), new ZDateTime(year, 1, 22)); // place invoice period during when goods are receipted.
			var autoRating2 = GetIAutoRating(invoiceWhenStockReceipted);
			var rateableMeasures2 = (RateableMeasureSet)autoRating2.RateableMeasures;
			AssertEquals("Only the Receive Units should be found as chargeable.", 1m, rateableMeasures2.UnitsByProduct_ForTest(MeasureType.LocationPallet, data.Part1.PK));
			AssertEquals("Only the Receive Units should be found as chargeable.", 1m, rateableMeasures2.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultLocation.ToLocationString()));
			AssertEquals("No goods were receipted into the Dock Door.", 0m, rateableMeasures2.UnitsByLocation_ForTest(MeasureType.LocationPallet, data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString()));
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_WithAdjustmentsIn

		public void TestIAutoRatingSplitPeriodBilling_WithAdjustmentsIn()
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			SetupChargesAndRateLinesForSplitMonthBillingWithAdjustments(data, year);

			//        |      +5 (ADJ Arrival)|                      |                      |
			// +5(REC)|      +5 (REC)        |                      |   +5 (ADJ Finalised) |
			//-┴------|-------┴-----┬--------|----------------------|----┴-----------------|
			//        |            -7 (ORD)  |                      |                      |
			//        |        JAN           |          FEB         |          MAR         |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year - 1, 12, 25), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 10), data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(year, 1, 16), data.Part1, 7m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 16);
			}

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 5m, "A", new ZDateTimeOffset(year, 1, 10));
			adjustment.FinaliseDocket();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 3, 6);
			AssertIsFinalisedPrecondition(adjustment);

			// January should be 5 RECSTO ($5 x 5) + 2 ORDSTO ($4 x 2) + 3 STO ($3 x 3)
			var invoiceJanuary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			Factory.Save();
			invoiceJanuary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 3 charge should be created.", 3, invoiceJanuary.JobHeader.Charges.Count);
			AssertEquals("Receive storage = 5 * $5 = $25", 25m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Receive Storage R2 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Order storage = 2 * $4 = $8.", 8m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Order Storage O1 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Storage - P1 (Jan) is minimum for stock in Jan. 3 UNT * $3 = $9.", 9m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceJanuary))).JR_LocalSellAmt);
			Factory.Save();

			// February should be 7 STO ($3 x 7)
			var invoiceFebruary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 3, 1).AddDays(-1));
			Factory.Save();
			invoiceFebruary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceFebruary.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Feb) is minimum for stock in Feb. 3 UNT * $3 = $9.", 9m, invoiceFebruary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceFebruary))).JR_LocalSellAmt);
			Factory.Save();

			// March should be 3 STO ($3 x 3) + 5 RECSTO ($5 x 5)
			var invoiceMarch = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();
			invoiceMarch.AutoRateJobHeader(null);
			AssertEquals("Precondition: 2 charge should be created.", 2, invoiceMarch.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Mar) is minimum for stock in Mar. 3 UNT * $3 = $9.", 9m, invoiceMarch.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceMarch))).JR_LocalSellAmt);
			AssertEquals("Receive storage = 5 * $5 = $25", 25m, invoiceMarch.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Receive Storage AD1 - P1 (P1)").JR_LocalSellAmt);
			Factory.Save();
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_WithAdjustmentsOut

		public void TestIAutoRatingSplitPeriodBilling_WithAdjustmentsOut()
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			SetupChargesAndRateLinesForSplitMonthBillingWithAdjustments(data, year);

			// +5(REC)|      +9  (REC)       |                      |                      |
			//-┴------|-------┬-----┬--------|----------------------|----┬-----------------|
			//        |      -5 (ADJ Arrival)|                      |   -5 (ADJ Finalised) |
			//        |            -7 (ORD)  |                      |                      |
			//        |        JAN           |          FEB         |          MAR         |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year - 1, 12, 25), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(year, 1, 10), data.Part1, 9m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(year, 1, 16), data.Part1, 7m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			foreach (var pickLine in pick.GetAllPickLines())
			{
				pickLine.WZ_PickedDateTime = new ZDateTimeOffset(year, 1, 16);
			}

			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -5m, data.Whs1.DefaultLocation.ToLocationString(), new ZDateTimeOffset(year, 1, 10));
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 3, 6);
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var invoiceJanuary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var invoiceFebruary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 3, 1).AddDays(-1));
			var invoiceMarch = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 3, 1), new ZDateTime(year, 3, 31));
			Factory.Save();

			// January should be 9 RECSTO ($5 x 9) + 0 ORDSTO ($4 x 0) + 5 STO ($3 x 5)
			invoiceJanuary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 3 charge should be created.", 3, invoiceJanuary.JobHeader.Charges.Count);
			AssertEquals("Receive storage = 9 * $5 = $45", 45m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Receive Storage R2 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Order storage is zero as we do not consider adjustment arrival date and 7 (ORD) < 9 (REC).", 0m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Order Storage O1 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Storage - P1 (Jan) is minimum for stock in Jan. 5 UNT * $3 = $15.", 15m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceJanuary))).JR_LocalSellAmt);

			// February should be 7 STO ($3 x 7)
			invoiceFebruary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceFebruary.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Feb) is minimum for stock in Feb. 7 UNT * $3 = $21.", 21m, invoiceFebruary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceFebruary))).JR_LocalSellAmt);

			// March should be 5 ORD ($4 x 5) + 2 STO ($3 x 2)
			invoiceMarch.AutoRateJobHeader(null);
			AssertEquals("Precondition: 2 charges should be created.", 2, invoiceMarch.JobHeader.Charges.Count);
			AssertEquals("Adjustment out was finalised in March, so we count it as order storage. 5 UNT * $4 = $20 ", 20m, invoiceMarch.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Order Storage AD1 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Storage - P1 (Mar) is minimum for stock in Mar. 2 UNT * $3 = $6.", 6m, invoiceMarch.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceMarch))).JR_LocalSellAmt);

			Factory.Save();
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_AdjustmentInOutCompensateEachOtherForSameProduct

		public void TestIAutoRatingSplitPeriodBilling_AdjustmentInOutCompensateEachOtherForSameProduct()
		{
			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			SetupChargesAndRateLinesForSplitMonthBillingWithAdjustments(data, year);

			//        |                      |                      | 
			// +5(REC)|      +2 (ADJ P1)     |                      | 
			//-┴------|-------┼--------------|----------------------|---
			//        |      -4 (ADJ P1)     |                      | 
			//        |        JAN           |          FEB         | 

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(year - 1, 12, 25), data.Part1, 5m);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			var adjustmentLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 2m, data.Whs1.DefaultLocation.ToLocationString());
			var adjustmentLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -4m, data.Whs1.DefaultLocation.ToLocationString());
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 14);
			AssertIsFinalisedPrecondition(adjustment);

			var invoiceJanuary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var invoiceFebruary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 3, 1).AddDays(-1));
			Factory.Save();

			invoiceJanuary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 2 charge should be created.", 2, invoiceJanuary.JobHeader.Charges.Count);
			AssertEquals("Order storage = 2 * $4 = $8.", 8m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Order Storage AD1 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Storage - P1 (Jan) is minimum for stock in Jan. 3 UNT * $3 = $9.", 9m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceJanuary))).JR_LocalSellAmt);

			invoiceFebruary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceFebruary.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Feb) is minimum for stock in Feb. 3 UNT * $3 = $9.", 9m, invoiceFebruary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceFebruary))).JR_LocalSellAmt);

			adjustment.JobHeader.Dispose();
		}

		#endregion

		#region TestIAutoRatingSplitPeriodBilling_AdjustmentInOutCompensateEachOtherForSameProductDifferentAttributes

		public void TestIAutoRatingSplitPeriodBilling_AdjustmentInOutCompensateEachOtherForSameProductDifferentAttributes()
		{
			// attributes should be ignored in split month billing

			var year = ZDateTime.Now.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			SetupChargesAndRateLinesForSplitMonthBillingWithAdjustments(data, year);

			//        |                      |                      | 
			//+10(REC)|    +2 (ADJ P1 ATT-1) |                      | 
			//-┴------|-------┼--------------|----------------------|---
			//        |    -4 (ADJ P1 ATT-2) |                      | 
			//        |        JAN           |          FEB         | 

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "ATT-1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "ATT-2", "", "", "");
			receive.WD_ArrivalDate = new ZDateTimeOffset(year - 1, 12, 25);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", Helper.Notify);
			adjustment.IsUniqueExternalReferenceCreatedOnSave = false;
			var adjustmentLineIn = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 2m, data.Whs1.DefaultLocation.ToLocationString(), "ATT-1", "", "", "", ZDate.Empty, ZDate.Empty);
			var adjustmentLineOut = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -4m, data.Whs1.DefaultLocation.ToLocationString(), "ATT-2", "", "", "", ZDate.Empty, ZDate.Empty);
			adjustmentLineOut.WE_AdjustmentArrivalDate = receive.WD_ArrivalDate;
			adjustment.FinaliseDocketWithoutUserConfirmation();
			adjustment.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 14);
			AssertIsFinalisedPrecondition(adjustment);

			var invoiceJanuary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 1, 1), new ZDateTime(year, 1, 31));
			var invoiceFebruary = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 3, 1).AddDays(-1));
			Factory.Save();

			invoiceJanuary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 2 charge should be created.", 2, invoiceJanuary.JobHeader.Charges.Count);
			AssertEquals("Order storage = 2 * $4 = $8.", 8m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == "Order Storage AD1 - P1 (P1)").JR_LocalSellAmt);
			AssertEquals("Storage - P1 (Jan) is minimum for stock in Jan. 8 UNT * $3 = $24.", 24m, invoiceJanuary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceJanuary))).JR_LocalSellAmt);

			invoiceFebruary.AutoRateJobHeader(null);
			AssertEquals("Precondition: 1 charge should be created.", 1, invoiceFebruary.JobHeader.Charges.Count);
			AssertEquals("Storage - P1 (Feb) is minimum for stock in Feb. 8 UNT * $3 = $24.", 24m, invoiceFebruary.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_Desc == string.Format("Storage - P1 (P1) {0}", GetChargeDateDesc(invoiceFebruary))).JR_LocalSellAmt);

			adjustment.JobHeader.Dispose();
		}

		#endregion

		#region SplitMonthBilling helpers

		void SetupChargesAndRateLinesForSplitMonthBillingWithAdjustments(TestDataSimpleEnvironment data, int year)
		{
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var receiveStorageCharge = Helper.CreateChargeCode("RECSTO", "Receive Storage", ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeSubGroupList.Storage);
			var orderStorageCharge = Helper.CreateChargeCode("ORDSTO", "Order Storage", ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeSubGroupList.Storage);
			var storageCharge = Helper.CreateChargeCode("STO", "Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			var clientRate = Factory.New<ClientRate>();
			data.Org1.OH_IsDebtor = true;
			clientRate.TH_OH = data.Org1.PK;
			var rateEntry = Helper.CreateRateEntry(clientRate, new ZDate(year - 1, 1, 1), new ZDate(year + 1, 1, 1));
			Helper.CreateRateLine(rateEntry, receiveStorageCharge, "UNT", 5m);
			Helper.CreateRateLine(rateEntry, orderStorageCharge, "UNT", 4m);
			Helper.CreateRateLine(rateEntry, storageCharge, "UNT", 3m);
		}

		string GetChargeDateDesc(WhsInvoice invoice)
		{
			return string.Format("for {0} days ({1} - {2})", (int)(invoice.ET_StorageToDate - invoice.ET_StorageFromDate).TotalDays + 1, invoice.ET_StorageFromDate.ToShortDateString(), invoice.ET_StorageToDate.ToShortDateString());
		}

		#endregion

		#region TestAutoRating_WithFreeStorageDays

		public void TestAutoRating_WithFreeStorageDays()
		{
			SetUpData();

			Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Factory.Save();

			var finalisedDate = Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2007, 12, 24));
			var receive1 = CreateWhsReceiveForPutawayTransfer("Rec1", finalisedDate.AddDays(-14), 1m, "PLT-1");
			var receive2 = CreateWhsReceiveForPutawayTransfer("Rec2", finalisedDate.AddDays(-12), 3m, "PLT-2");
			var receive3 = CreateWhsReceiveForPutawayTransfer("Rec3", finalisedDate.AddDays(-11), 5m, "PLT-3");
			var receive4 = CreateWhsReceiveForPutawayTransfer("Rec4", finalisedDate.AddDays(-4), 7m, "PLT-4");
			Factory.Save();

			PreparePutawayTransfer(finalisedDate, 1m, "PLT-1");
			PreparePutawayTransfer(finalisedDate, 3m, "PLT-2");
			PreparePutawayTransfer(finalisedDate, 5m, "PLT-3");
			PreparePutawayTransfer(finalisedDate, 7m, "PLT-4");
			Factory.Save();

			// finalise goods into warehouse
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			receive3.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive3);
			receive4.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive4);
			Factory.Save();

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 0;
			var invoice = Helper.CreateInvoiceWithJobHeader(Org1, Whs1, new ZDateTime(2007, 12, 22), new ZDateTime(2007, 12, 22));
			var autoRating1 = GetIAutoRating(invoice);
			var rateableMeasures1 = (RateableMeasureSet)autoRating1.RateableMeasures;
			AssertEquals("All Units should be found since no free storage days provided.", 1m + 3m + 5m + 7m, rateableMeasures1.GetActual(MeasureType.Unit));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 5;
			var autoRating2 = GetIAutoRating(invoice);
			var rateableMeasures2 = (RateableMeasureSet)autoRating2.RateableMeasures;
			AssertEquals("Only some Units should be found since free storage days elapsed.", 1m + 3m + 5m, rateableMeasures2.GetActual(MeasureType.Unit));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 10;
			var autoRating3 = GetIAutoRating(invoice);
			var rateableMeasures3 = (RateableMeasureSet)autoRating3.RateableMeasures;
			AssertEquals("Only some Units should be found since free storage days elapsed.", 1m + 3m, rateableMeasures3.GetActual(MeasureType.Unit));

			Org1.CompanyData.OB_WhsClientFreeStorageDays = 15;
			var autoRating4 = GetIAutoRating(invoice);
			var rateableMeasures4 = (RateableMeasureSet)autoRating4.RateableMeasures;
			AssertEquals("No Units should be found as free storage days cover full period.", 0m, rateableMeasures4.GetActual(MeasureType.Unit));
		}

		WhsReceive CreateWhsReceiveForPutawayTransfer(ZString reference, ZDateTimeOffset arrivalDate, ZDecimal units, ZString palletID)
		{
			var receive = Helper.CreateWhsReceive(Org1, Whs1);
			receive.WD_ExternalReference = reference;
			receive.WD_ArrivalDate = arrivalDate;
			Helper.CreateWhsReceiveLine(receive, Part1, units, Whs1.DefaultOutboundDockDoorLocation, palletID);
			return receive;
		}

		WhsTransfer PreparePutawayTransfer(ZDateTimeOffset finalisedDate, ZDecimal units, ZString palletID)
		{
			// create Putaway Transfer to represent movement from Inbound Dock Door to Putaway Location
			var transfer = Helper.CreateWhsTransfer(Org1, Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, Part1, Whs1.DefaultOutboundDockDoorLocation, Whs1.DefaultLocation, palletID, units);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", units, transferLine.QtyCommittedIncludingMatchingLines);

			// set Picked time and finalised time manually
			transferLine.PickedTime = finalisedDate;
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);
			transferLine.WE_FinalisedDate = finalisedDate;

			return transfer;
		}

		#endregion

		#region TestAutoRating_TransportProviders

		public void TestAutoRating_TransportProviders()
		{
			SetUpData();
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_WW = Whs1.PK;

			var autoRating = GetIAutoRating(invoice);
			AssertEquals(1, autoRating.Creditors.AllOrgs.Count);
			AssertEquals(Whs1.WarehouseAddress.Header, autoRating.Creditors.AllOrgs[0]);
		}

		#endregion

		#region TestAutoRating_AdapterTypeAndID

		public void TestAutoRating_AdapterTypeAndID()
		{
			SetUpData();
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_WW = Whs1.PK;

			var adapter = GetIAutoRating(invoice);
			AssertEquals(AdapterType.WarehouseInvoice, adapter.AdapterType);
			AssertEquals(invoice.ET_StorageJobNumber, adapter.OperationalJobCode);
		}

		#endregion

		#region TestAutoRating_WillNotExludeReceivesWithoutWE_ClientOrdredUnits

		[TestDate(2015, 10, 16)]
		public void TestAutoRating_WillNotExludeReceivesWithoutWE_ClientOrdredUnits()
		{
			// this test is a part of the fix WI00101067 for Senator when those receives were ignored by storage charges
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 3m, data.Whs1.DefaultLocation);
			// for second line we will create ASN
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, data.Whs1.DefaultLocation);
			var asnLinePart2 = receive2.AsnLines.AddNew();
			asnLinePart2.WN_OP = data.Part2.PK;
			asnLinePart2.WN_Quantity = inventoryLine2.WI_InDocketLineUnits;
			receive2.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive2);
			AssertEquals("There is no asnLine for 1st receive line, so WE_ClientOrderedUnits should become 0.", 0m, inventoryLine1.InDocketLine.WE_ClientOrderedUnits);

			Helper.CreateChargeCode("WST", "Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;

			var invoiceOctober = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2015, 10, 01), new ZDateTime(2015, 10, 31));

			Factory.Save();

			var autoRating = GetIAutoRating(invoiceOctober);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Both receives should be counted for storage.", 5m + 3m + 1m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region TestAutoRating_InventoryStatusChangeDoNotAffectStorageCharges

		[TestDate(2015, 10, 16)]
		public void TestAutoRating_InventoryStatusChangeDoNotAffectStorageCharges()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var receiveLine = receive.Lines[0];

			receiveLine.HeldCodeToChangeTo = "DAM";
			receiveLine.HeldCodeChangeQuantity = 7m;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			Helper.CreateChargeCode("WST", "Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseStorageMax;

			var invoiceOctober = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, new ZDateTime(2015, 10, 01), new ZDateTime(2015, 10, 31));

			Factory.Save();

			var autoRating = GetIAutoRating(invoiceOctober);
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("Only original receive is counted for charges.", 10m, rateableMeasures.GetActual(MeasureType.Unit));
		}

		#endregion

		#region IAutoRatingAdditionalJobs Members

		public void TestAutoRatingAdditionalJobs()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			var receive1 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			var receive1Job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			receive1Job.JH_ParentID = receive1.PK;
			receive1Job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			receive1Job.JH_ExcludeFromPeriodicRating = true;

			CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 11, 0, 0), true, new PartUnit(Part1, 15));
			Factory.Save();
			CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));
			var order2 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 8, 11, 0, 0), new PartUnit(Part1, 10));
			var order2Job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			order2Job.JH_ParentID = order2.PK;
			order2Job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			var docket4Charge = Factory.NewWithValidTestData<JobCharge>();
			docket4Charge.JR_JH = order2Job.PK;

			var invoice = Factory.New<WhsInvoice>();
			var invoiceJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			invoiceJob.JH_ParentID = invoice.PK;
			invoiceJob.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;

			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 8);

			Factory.Save();

			AssertEquals(2, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Where(j => j.TableName == WhsDocketSchema.Constants.TableName)
				.Select(j => Factory.Load<WhsDocket>(j.PK)).Count(d => d.WD_DocketType == DocketType.Codes.Receive));
			AssertEquals(2, invoice.GetAdditionalDockets().Count(d => d.WD_DocketType == DocketType.Codes.Order));

			AssertEquals("Only single RatingAdapter should exist on Invoice", 1, invoice.GetRatingAdapters().Count);
		}

		public void TestInvoicingAdditionalJobs()
		{
			var year = ZDateTime.Now.Year;
			SetUpData();

			var docket1 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 1, 11, 0, 0), true, new PartUnit(Part1, 10));
			var docket1Job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			docket1Job.JH_ParentID = docket1.PK;
			docket1Job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			docket1Job.JH_ExcludeFromPeriodicRating = true;

			var docket2 = CreateWhsReceive(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 11, 0, 0), true, new PartUnit(Part1, 15));

			var docket3 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 2, 0, 0, 0), new PartUnit(Part1, 5));

			var docket4 = CreateFinalisedWhsOrder(Whs1, Org1, new ZDateTimeOffset(year, 2, 8, 11, 0, 0), new PartUnit(Part1, 10));
			var docket4Job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			docket4Job.JH_ParentID = docket4.PK;
			docket4Job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			Factory.Save();

			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = Org1.PK;
			invoice.ET_WW = Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, 2, 1);
			invoice.ET_StorageToDate = new ZDateTime(year, 2, 8);

			AssertEquals(2, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Where(j => j.TableName == WhsDocketSchema.Constants.TableName)
				.Select(j => Factory.Load<WhsDocket>(j.PK)).Count(d => d.WD_DocketType == DocketType.Codes.Receive));
			AssertEquals(2, invoice.GetAdditionalDockets().Count(d => d.WD_DocketType == DocketType.Codes.Order));

			AssertEquals(4, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Length);
			AssertCollectionContains("Excluded from periodic rating but still shown", docket1.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Select(j => j.PK));

			Org1.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = false;

			AssertEquals("Only one adapter should be created", 1, invoice.GetRatingAdapters().Count);
			AssertEquals(docket3.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor[0].PK);
			AssertEquals(docket4.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor[1].PK);

			Org1.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice = false;

			AssertEquals(0, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Length);

			Org1.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = true;

			AssertEquals(2, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor.Length);
			AssertEquals(docket1.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor[0].PK);
			AssertEquals(docket2.PK, ((IJobInvoicingPlugInAdditionalJobs)invoice).AdditionalJobsToShowChargesFor[1].PK);
		}

		#endregion

		#region TestJobDatesProvider

		public void TestJobDatesProvider()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertType<WhsInvoiceJobDatesProvider>(GetIAutoRating(invoice).JobDatesProvider);
		}

		#endregion

		#region TestImportBroker

		public void TestImportBroker()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertNull(GetIAutoRating(invoice).ImportBroker);
		}

		#endregion

		#region TestExportBroker

		public void TestExportBroker()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertNull(GetIAutoRating(invoice).ExportBroker);
		}

		#endregion

		#region IImportExport

		public void TestIImportExport()
		{
			var iInvoice = GetIAutoRating(Factory.New<WhsInvoice>());

			Assert(!iInvoice.IsImport());
			Assert(!iInvoice.IsExport());
			Assert(!iInvoice.IsDomestic());
			Assert(!iInvoice.IsCrossTrade());
			Assert(iInvoice.IsUnknown());
			AssertEquals(Directions.Unknown, iInvoice.JobDirection);
		}

		#endregion

		#region Implementation

		#region SetUpData

		void SetUpData()
		{
			// create environment
			Whs1 = Helper.CreateWarehouse("AAAA", "A", 2, 2);
			Whs2 = Helper.CreateWarehouse("BBBB", "B", 2, 2);

			// create product
			Part1 = Helper.CreateProduct(Org1, "AAA");
			SetupProduct(Part1, Org2);

			Part2 = Helper.CreateProduct(Org1, "BBB");
			var rel = Part2.RelatedOrganisations.AddNew();
			rel.OU_OH = Org2.PK;
			rel.OU_OP = Part2.PK;
			rel.OU_Relationship = "OWN";
			var unit = Part2.PartUnits.AddNew();
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

		void SetupProduct(OrgSupplierPart part, OrgHeader org)
		{
			var rel = part.RelatedOrganisations.AddNew();
			rel.OU_OH = org.PK;
			rel.OU_OP = part.PK;
			rel.OU_Relationship = "OWN";
			var unit = part.PartUnits.AddNew();
			unit.OF_OP = part.PK;
			unit.OF_PackType = "UNT";
			unit.OF_ParentPackType = "PLT";
			unit.OF_QuantityInParent = 8;
			part.OP_Weight = 1m;
			part.OP_WeightUQ = "LB";
			part.OP_Cubic = 1m;
			part.OP_CubicUQ = "L";
			part.OP_RH_NKCommodityCode = "GEN";
		}

		#endregion

		#region CreateProductWithManyConversoins

		OrgSupplierPart CreateProductWithManyConversoins(ZString code)
		{
			// create product
			var part = Helper.CreateProduct(Org1, code);
			var partUnits = part.PartUnits;
			var rel = part.RelatedOrganisations.AddNew();
			rel.OU_OH = Org2.PK;
			rel.OU_OP = part.PK;
			rel.OU_Relationship = "OWN";

			var partUnit1 = partUnits.AddNew();
			partUnit1.OF_OP = part.PK;
			partUnit1.OF_PackType = "CAS";
			partUnit1.OF_QuantityInParent = 1;
			partUnit1.OF_ParentPackType = "PLT";

			var partUnit2 = partUnits.AddNew();
			partUnit2.OF_PackType = "BOX";
			partUnit2.OF_QuantityInParent = 2;
			partUnit2.OF_ParentPackType = "CAS";

			var partUnit3 = partUnits.AddNew();
			partUnit3.OF_PackType = "UNT";
			partUnit3.OF_QuantityInParent = 3;
			partUnit3.OF_ParentPackType = "BOX";

			var partUnit4 = partUnits.AddNew();
			partUnit4.OF_PackType = "LB";
			partUnit4.OF_QuantityInParent = 4;
			partUnit4.OF_ParentPackType = "UNT";

			var partUnit5 = partUnits.AddNew();
			partUnit5.OF_PackType = "KG";
			partUnit5.OF_QuantityInParent = 5;
			partUnit5.OF_ParentPackType = "LB";

			part.OP_StockKeepingUnit = "KG";

			return part;
		}

		#endregion

		#region CreateWhsReceive

		WhsReceive CreateWhsReceive(WhsWarehouse whs, OrgHeader org, ZDateTimeOffset finalisedDate, bool finalise, params PartUnit[] partUnits)
		{
			var docket = Helper.CreateWhsReceive(org, whs, NextExternalReference, new TestNotificationBuffer());

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

		#region SetClientAttributesType

		void SetClientAttributesType(OrgHeader client, bool usePartAttrib1, bool usePartAttrib2, bool usePartAttrib3, bool useExpiryDate, bool usePackingDate)
		{
			if (usePartAttrib1)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			}

			if (usePartAttrib2)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			}

			if (usePartAttrib3)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			}

			if (useExpiryDate)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			}

			if (usePackingDate)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
			}
		}

		#endregion

		#region SetProductAttributesUse

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part, bool usePartAttrib1, bool usePartAttrib2, bool usePartAttrib3, bool useExpiryDate, bool usePackingDate)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, usePartAttrib1);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, usePartAttrib2);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, usePartAttrib3);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, useExpiryDate);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, usePackingDate);
		}

		#endregion

		#region AddJobHeader

		JobHeader AddJobHeader(WhsDocket docket)
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = docket.WD_DocketID;
			jobHeader.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			jobHeader.JH_ParentID = docket.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return jobHeader;
		}

		#endregion

		#region AddJobCharge

		JobCharge AddJobCharge(JobHeader jobHeader, AccChargeCode chargeCode)
		{
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_AC = chargeCode.PK;

			return jobCharge;
		}

		#endregion

		#region AddJobChargeAttrib

		JobChargeAttrib AddJobChargeAttrib(JobCharge jobCharge, ZString name, ZString value)
		{
			var jobChargeAttrib = Factory.New<JobChargeAttrib>();
			jobChargeAttrib.EC_JR = jobCharge.PK;
			jobChargeAttrib.EC_Name = name;
			jobChargeAttrib.EC_Value = value;

			return jobChargeAttrib;
		}

		#endregion

		#region NextExternalReference

		ZString NextExternalReference => "REF" + fNextExternalReference++.ToString("f0");

		#endregion

		#region Helper

		public new WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));

		#endregion

		#region GetIAutoRating

		IAutoRating GetIAutoRating(WhsInvoice invoice)
		{
			return new WhsInvoiceRatingAdapter(invoice);
		}

		#endregion

		WhsTestHelperFunctionsInvoice helper;
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
