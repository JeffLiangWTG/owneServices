using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	internal class WhsBondedExwEntryKeyUpdaterTest : WhsTestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullArgument()
		{
			EntryKeyUpdater = new WhsBondedExwEntryKeyUpdater(null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestUpdateThrowsExceptionIfTransactionIsNull()
		{
			EntryKeyUpdater.Update(ZGuid.NewZGuid(), null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestUpdateThrowsExceptionIfDeclarationPKIsEmpty()
		{
			EntryKeyUpdater.Update(ZGuid.Empty, new WhsBondedWarehouseTransaction());
		}

		public void TestUpdate()
		{
			// Create Nature 20
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory);
			Factory.Save(); // needed for DBOnly queries in WhsBondedCalculator
			// Create Nature 30
			WhsBondedWarehouseTransaction tran = WhsBondedWarehouseTransaction.CopyExceptLines(data.IReceive);
			WhsBondedWarehouseTransactionLine line1 = WhsBondedWarehouseTransactionLine.Copy(data.IReceiveLine1);
			WhsBondedWarehouseTransactionLine line2 = WhsBondedWarehouseTransactionLine.Copy(data.IReceiveLine2);
			tran.Lines.Add(line1);
			tran.Lines.Add(line2);
			ZGuid externalPK = tran.ExternalPK = ZGuid.NewZGuid();
			WhsBondedOutwardsProcessor processor = new WhsBondedOutwardsProcessor();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				processor.Process(Factory, tran, false);
			}

			// Assert Nature 30 was created correctly
			WhsOrder[] orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_ExWhsJobGuid, externalPK));
			AssertEquals(1, orders.Length);
			WhsOrder order = orders[0];
			AssertEquals(2, order.Lines.Count);
			WhsOrderLine orderLine1 = order.Lines[0];
			WhsOrderLine orderLine2 = order.Lines[1];
			AssertEquals(WhsBondedWarehouseAttribute.WaitingForCustomsPaymentMessage,
				orderLine1.CustomsData.WB_EntryKey);
			Assert(orderLine1.CustomsData.WB_EntryLineNo.IsEmpty);
			AssertEquals(ZDateTime.Today, orderLine1.CustomsData.WB_EntryDate);
			AssertEquals(WhsBondedWarehouseAttribute.WaitingForCustomsPaymentMessage,
				orderLine2.CustomsData.WB_EntryKey);
			Assert(orderLine2.CustomsData.WB_EntryLineNo.IsEmpty);
			AssertEquals(ZDateTime.Today, orderLine2.CustomsData.WB_EntryDate);
			// Execute EntryKeyUpdater with new Reference, Keys and Dates
			tran.Reference = "NewRef";
			line1.EntryKey = "NewKey1";
			line1.EntryLineNumber = 10;
			line1.EntryDate = ZDateTime.Today.AddDays(1);
			// Clear Unique Key on Line2 to test Old Data Matching
			line2.EntryKey = "NewKey2";
			line2.EntryLineNumber = 11;
			line2.EntryDate = ZDateTime.Today.AddDays(2);
			line2.UniqueKey = ZGuid.Empty;
			// Update and Assert results
			EntryKeyUpdater.Update(externalPK, tran);
			AssertEquals("NEWREF", orderLine1.CustomsData.WB_DeclarationReference);
			AssertEquals("NEWKEY1", orderLine1.CustomsData.WB_EntryKey);
			AssertEquals((short)10, orderLine1.CustomsData.WB_EntryLineNo);
			AssertEquals(ZDateTime.Today.AddDays(1), orderLine1.CustomsData.WB_EntryDate);
			AssertEquals("NEWREF", orderLine2.CustomsData.WB_DeclarationReference);
			AssertEquals("NEWKEY2", orderLine2.CustomsData.WB_EntryKey);
			AssertEquals((short)11, orderLine2.CustomsData.WB_EntryLineNo);
			AssertEquals(ZDateTime.Today.AddDays(2), orderLine2.CustomsData.WB_EntryDate);
		}

		public void TestMatchFoundWithOutUniqueKey()
		{
			WhsOrderLine orderLine;
			WhsBondedWarehouseTransaction transaction;
			SetUpDataForMatchFound(out orderLine, out transaction);
			WhsBondedWarehouseTransactionLine tranLine = (WhsBondedWarehouseTransactionLine)transaction.Lines[0];
			Assert("incomplete test", true);
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Warehouse = Helper.CreateWarehouse("WHS2").WarehouseAddress;
			//Assert("NoMatch on Warehouse", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Warehouse = OrderLine.Docket.Warehouse.WarehouseAddress;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Product = OrgSupplierPart.New(Factory);
			//Assert("NoMatch on Product", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Product = OrderLine.SupplierPart;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.PartAttrib1 = "NewBond1";
			//Assert("NoMatch on BondID1", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.PartAttrib1 = OrderLine.WE_PartAttrib1;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.PartAttrib2 = "NewBond2";
			//Assert("NoMatch on BondID2", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.PartAttrib2 = OrderLine.WE_PartAttrib2;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.PartAttrib3 = "NewBond3";
			//Assert("NoMatch on BondID3", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.PartAttrib3 = OrderLine.WE_PartAttrib3;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Quantity = 200m;
			//Assert("NoMatch on Quantity", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Quantity = OrderLine.WE_TransactionQuantity;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.CustomsQuantity = 200m;
			//Assert("NoMatch on CustomsQuantity", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.CustomsQuantity = OrderLine.CustomsData.WB_CustomsQty;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.CustomsQuantity = 200.2m;
			//OrderLine.CustomsData.WB_CustomsQty = 200.212454m;
			//Assert("CustomsQuantity Rounding To 1 dp", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.ValueForDuty = 1000m;
			//Assert("NoMatch on ValueForDuty", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.ValueForDuty = OrderLine.CustomsData.WB_ValueForDuty;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.ValueForDuty = 1000m;
			//OrderLine.CustomsData.WB_ValueForDuty = 1000.0158m;
			//Assert("ValueForDuty Rounding To 1 dp", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.TILV = 689.0214m;
			//Assert("NoMatch on TILV", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.TILV = OrderLine.CustomsData.WB_TILV;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.TILV = 689.0214m;
			//OrderLine.CustomsData.WB_TILV = 689.0114m;
			//Assert("TILV Rounding To 1 dp", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
		}

		public void TestMatchFoundWithUniqueKey()
		{
			WhsOrderLine orderLine;
			WhsBondedWarehouseTransaction transaction;
			SetUpDataForMatchFound(out orderLine, out transaction);
			WhsBondedWarehouseTransactionLine tranLine = (WhsBondedWarehouseTransactionLine)transaction.Lines[0];
			tranLine.UniqueKey = orderLine.PK;
			Assert("incomplete test", true);
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.Warehouse = Helper.CreateWarehouse("WHS2").WarehouseAddress;
			//Assert("Mismatch in warehouse does not affect it", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.UniqueKey = ZGuid.NewZGuid();
			//Assert("MisMatch", !EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
			//TranLine.UniqueKey = OrderLine.PK;
			//Assert("Match", EntryKeyUpdater.MatchFound(Transaction, TranLine, OrderLine));
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			EntryKeyUpdater = new WhsBondedExwEntryKeyUpdater(Factory);
		}

		void SetUpDataForMatchFound(out WhsOrderLine orderLine, out WhsBondedWarehouseTransaction tran)
		{
			var client = Helper.CreateClient();
			var order = Factory.New<WhsOrder>();
			orderLine = order.Lines.AddNew();
			orderLine.WE_BondedEntryKey = "SOMEKEY";
			WhsBondedWarehouseAttribute bondedAttribure = Factory.New<WhsBondedWarehouseAttribute>();
			bondedAttribure.WB_ParentTableCode = WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode;
			bondedAttribure.WB_ParentID = orderLine.PK;
			tran = new WhsBondedWarehouseTransaction();
			tran.Client = client;
			order.WD_OH_Client = client.PK;
			WhsBondedWarehouseTransactionLine tranLine = new WhsBondedWarehouseTransactionLine();
			tran.Lines.Add(tranLine);
			//Tran.Lines = new BondedWarehouseTransactionLineCollection(TranLine);
			WhsWarehouse whs1 = Helper.CreateWarehouse("WHS1");
			order.WD_WW_Whs = whs1.PK;
			tranLine.Warehouse = whs1.WarehouseAddress;
			tranLine.Product = OrgSupplierPart.New(Factory);
			orderLine.WE_OP = tranLine.Product.PK;
			orderLine.WE_PartAttrib1 = tranLine.PartAttrib1 = "BOND1";
			orderLine.WE_PartAttrib2 = tranLine.PartAttrib2 = "BOND2";
			orderLine.WE_TransactionQuantity = tranLine.Quantity = 10m;
			orderLine.WE_F3_NKPackType = tranLine.QuantityUnit = "KG";
			bondedAttribure.WB_CustomsUnitOfQty = tranLine.CustomsQuantityUnit = "KG";
			bondedAttribure.WB_CustomsQty = tranLine.CustomsQuantity = 250m;
			bondedAttribure.WB_BondedWhsUnitOfQty = tranLine.BondedWarehouseQuantityUnit = "M";
			bondedAttribure.WB_BondedWhsQty = tranLine.BondedWarehouseQuantity = 250m;
			bondedAttribure.WB_ValueForDuty = tranLine.ValueForDuty = 200m;
			tranLine.TILV = new Money(300m, GlbCompany.CurrentCompany.LocalCurrency);
			bondedAttribure.WB_TILV = 300m;
			bondedAttribure.WB_RX_NKTILVCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bondedAttribure.WB_AddInfo = tranLine.AddInfo = "AddInfo";
			bondedAttribure.WB_DeclarationReference = tran.Reference = "DecRef1";
			tranLine.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");
			bondedAttribure.WB_RN_NKCountryOfOrigin = tranLine.CountryOfOrigin.RN_Code;
		}

		WhsBondedExwEntryKeyUpdater EntryKeyUpdater;

		#endregion
	}
}
