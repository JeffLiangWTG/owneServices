using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsOutwardsProcessorTestCase : WhsTestCaseWithFactory
	{
		#region Implementation

		protected virtual void AssertTransactionLine(IWhsWarehouseTransactionLine result,
			IWhsWarehouseTransactionLine source, IOrgAddress warehouse)
		{
			AssertTransactionLine(result, source, warehouse, source.Quantity);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransactionLine result,
			IWhsWarehouseTransactionLine source, IOrgAddress warehouse, decimal quantity)
		{
			AssertEquals(source.Product, result.Product);
			AssertEquals(quantity, result.Quantity);
			AssertEquals(source.EntryKey, result.EntryKey);
			AssertEquals(source.EntryLineNumber, result.EntryLineNumber);
			AssertEquals(source.PartAttrib1, result.PartAttrib1);
			AssertEquals(source.PartAttrib2, result.PartAttrib2);
			AssertEquals(source.PartAttrib3, result.PartAttrib3);
			AssertEquals(warehouse, result.Warehouse);
			AssertNotNull(result.Warehouse);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransaction result, IOrgAddress warehouse,
			IOrgSupplierPart part, ZDecimal quantity, ZString bondedEntryKey, ZString partAttrib1, ZString partAttrib2,
			ZString partAttrib3)
		{
			EntryLineCodeParser parser = new EntryLineCodeParser(bondedEntryKey);
			string entryKey = parser.EntryNumber;
			short entryLineNo = parser.LineNumber;
			foreach (IWhsWarehouseTransactionLine line in result.Lines)
			{
				if ((quantity == line.Quantity) && (warehouse == line.Warehouse) &&
					(part == null || part == line.Product) && (string.IsNullOrEmpty(entryKey) || entryKey == line.EntryKey) &&
					(entryLineNo == 0 || entryLineNo == line.EntryLineNumber) &&
					(partAttrib1.IsEmpty || partAttrib1 == line.PartAttrib1) &&
					(partAttrib2.IsEmpty || partAttrib2 == line.PartAttrib2) &&
					(partAttrib3.IsEmpty || partAttrib3 == line.PartAttrib3))
				{
					return;
				}
			}

			Assert("Could not find matching line", false);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransaction result,
			IWhsWarehouseTransactionLine source, IOrgAddress warehouse)
		{
			AssertTransactionLine(result, warehouse, source.Product, source.Quantity,
				BondedEntryKey(source.EntryKey, source.EntryLineNumber), source.PartAttrib1, source.PartAttrib2,
				source.PartAttrib3);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransaction result,
			IWhsWarehouseTransactionLine source, IWhsWarehouseTransactionLine attributesToMatchFrom,
			IOrgAddress warehouse)
		{
			AssertTransactionLine(result, warehouse, source.Product, source.Quantity,
				BondedEntryKey(source.EntryKey, source.EntryLineNumber), attributesToMatchFrom.PartAttrib1,
				attributesToMatchFrom.PartAttrib2, attributesToMatchFrom.PartAttrib3);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransaction result,
			IWhsWarehouseTransactionLine source, IOrgAddress warehouse, ZDecimal quantity)
		{
			AssertTransactionLine(result, warehouse, source.Product, quantity,
				BondedEntryKey(source.EntryKey, source.EntryLineNumber), source.PartAttrib1, source.PartAttrib2,
				source.PartAttrib3);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransaction result,
			IWhsWarehouseTransactionLine source, IWhsWarehouseTransactionLine attributesToMatchFrom,
			IOrgAddress warehouse, ZDecimal quantity)
		{
			AssertTransactionLine(result, warehouse, source.Product, quantity,
				BondedEntryKey(source.EntryKey, source.EntryLineNumber), attributesToMatchFrom.PartAttrib1,
				attributesToMatchFrom.PartAttrib2, attributesToMatchFrom.PartAttrib3);
		}

		protected virtual void AssertTransactionLine(IWhsWarehouseTransaction result, IOrgAddress warehouse,
			IOrgSupplierPart part, ZDecimal quantity)
		{
			AssertTransactionLine(result, warehouse, part, quantity, "", "", "", "");
		}

		protected string BondedEntryKey(string key, short lineNo)
		{
			return WhsBondedWarehouseAttribute.BuildKey(key, lineNo);
		}

		#endregion

		#region Test Data

		#region Base

		public abstract class TestDataBase
		{
			public TestDataBase(BusinessObjectFactory factory)
			{
				this.Factory = factory;
				SetupData();
				ProcessInwardMovement();
				AlterArrivalDates();
			}

			protected virtual void SetupData()
			{
				Helper = new WhsTestHelperFunctions(Factory);
				Org = Helper.CreateClient();
				Org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
				Org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
				Org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
				Address1 = Factory.NewWithValidTestData<OrgAddress>();
				Address1.OA_Address1 = "1 BOND ST";
				Address1.OA_OH = Org.PK;
				Address2 = Factory.NewWithValidTestData<OrgAddress>();
				Address2.OA_Address1 = "2 BOND ST";
				Address2.OA_OH = Org.PK;
				Address3 = Factory.NewWithValidTestData<OrgAddress>();
				Address3.OA_Address1 = "3 BOND ST";
				Address3.OA_OH = Org.PK;
				Whs1 = Helper.CreateWarehouse("1");
				Whs2 = Helper.CreateWarehouse("2");
				Whs3 = Helper.CreateWarehouse("3");
				Helper.EnableWarehouseForBond(Whs1, true);
				Helper.EnableWarehouseForBond(Whs2, true);
				Helper.EnableWarehouseForBond(Whs3, true);
				Whs1.WW_OA_WarehouseAddress = Address1.PK;
				Whs2.WW_OA_WarehouseAddress = Address2.PK;
				Whs3.WW_OA_WarehouseAddress = Address3.PK;
				Part1 = Helper.CreateProduct(Org, "P1");
				Part2 = Helper.CreateProduct(Org, "P2");
				Part3 = Helper.CreateProduct(Org, "P3");
				Part4 = Helper.CreateProduct(Org, "P4");
				Part5 = Helper.CreateProduct(Org, "P5");
				Part6 = Helper.CreateProduct(Org, "P6");
				PartNS = Helper.CreateProduct(Org, "NoStock");
				Helper.SetProductAttributeUse(Org, Part3, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(Org, Part4, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(Org, Part5, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(Org, Part5, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(Org, Part6, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(Org, Part6, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(Org, Part6, AttributeNumber.Three, true);
				Factory.Save();
			}

			protected abstract void ProcessInwardMovement();

			protected virtual void AlterArrivalDates()
			{
			}

			protected WhsWarehouseTransactionLine CreateReceiveLine(OrgAddress whs, OrgSupplierPart part, ZDecimal qty,
				ZString qU, ZString bondedEntryKey, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
			{
				WhsWarehouseTransactionLine line = new WhsWarehouseTransactionLine();
				EntryLineCodeParser parser = new EntryLineCodeParser(bondedEntryKey);
				line.Warehouse = whs;
				line.Product = part;
				line.Quantity = qty;
				line.QuantityUnit = qU;
				line.EntryKey = parser.EntryNumber;
				line.EntryLineNumber = parser.LineNumber;
				line.PartAttrib1 = partAttrib1;
				line.PartAttrib2 = partAttrib2;
				line.PartAttrib3 = partAttrib3;
				return line;
			}

			public OrgHeader Org;
			public OrgAddress Address1;
			public OrgAddress Address2;
			public OrgAddress Address3;
			public WhsWarehouse Whs1;
			public WhsWarehouse Whs2;
			public WhsWarehouse Whs3;
			public OrgSupplierPart Part1;
			public OrgSupplierPart Part2;
			public OrgSupplierPart Part3;
			public OrgSupplierPart Part4;
			public OrgSupplierPart Part5;
			public OrgSupplierPart Part6;
			public OrgSupplierPart PartNS;
			public WhsTestHelperFunctions Helper;
			protected BusinessObjectFactory Factory;
		}

		#endregion

		#region Products (Plain)

		public class TestDataForSingleWarehouse : TestDataBase
		{
			public TestDataForSingleWarehouse(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void SetupData()
			{
				base.SetupData();
				IReceive1 = new WhsWarehouseTransaction();
				IReceive1.Date = ZDateTime.Now;
				IReceive1.Client = Org;
				IReceive1.Reference = "TestDataForSingleWarehouse";
				//IInwards1.Lines = new WarehouseTransactionLineCollection();
				IReceiveLine111 = CreateReceiveLine(Address1, Part1, 10, "NO", "E111-1", "", "", "");
				IReceiveLine112 = CreateReceiveLine(Address1, Part1, 20, "NO", "E111-1", "", "", "");
				IReceiveLine113 = CreateReceiveLine(Address1, Part1, 30, "NO", "E111-2", "", "", "");
				IReceiveLine121 = CreateReceiveLine(Address1, Part2, 10, "NO", "E111-3", "", "", "");
				IReceiveLine122 = CreateReceiveLine(Address1, Part2, 20, "NO", "E111-4", "", "", "");
				IReceiveLine123 = CreateReceiveLine(Address1, Part2, 30, "NO", "E111-5", "", "", "");
				IReceive1.Lines.Add(IReceiveLine111);
				IReceive1.Lines.Add(IReceiveLine112);
				IReceive1.Lines.Add(IReceiveLine113);
				IReceive1.Lines.Add(IReceiveLine121);
				IReceive1.Lines.Add(IReceiveLine122);
				IReceive1.Lines.Add(IReceiveLine123);
			}

			protected override void ProcessInwardMovement()
			{
				WhsReceiveProcessor processor = new WhsReceiveProcessor();
				processor.Process(Factory, IReceive1);
			}

			public WhsWarehouseTransaction IReceive1;
			public WhsWarehouseTransactionLine IReceiveLine111;
			public WhsWarehouseTransactionLine IReceiveLine112;
			public WhsWarehouseTransactionLine IReceiveLine113;
			public WhsWarehouseTransactionLine IReceiveLine121;
			public WhsWarehouseTransactionLine IReceiveLine122;
			public WhsWarehouseTransactionLine IReceiveLine123;
		}

		public class TestDataForMultiWarehouse : TestDataForSingleWarehouse
		{
			public TestDataForMultiWarehouse(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void SetupData()
			{
				base.SetupData();
				IReceive2 = new WhsWarehouseTransaction();
				IReceive2.Date = ZDateTime.Today.AddDays(-1);
				IReceive2.Client = Org;
				IReceive2.Reference = "TestDataForMultiWarehouse2";
				IReceiveLine211 = CreateReceiveLine(Address2, Part1, 10, "NO", "E211-1", "", "", "");
				IReceiveLine212 = CreateReceiveLine(Address2, Part1, 20, "NO", "E211-2", "", "", "");
				IReceiveLine213 = CreateReceiveLine(Address2, Part1, 30, "NO", "E211-3", "", "", "");
				IReceiveLine221 = CreateReceiveLine(Address2, Part2, 10, "NO", "E211-4", "", "", "");
				IReceiveLine222 =
					CreateReceiveLine(Address2, Part2, 20, "NO", "E211-4", "", "",
						""); // just to make duplicate (*not* set to E211-5)
				IReceiveLine223 = CreateReceiveLine(Address2, Part2, 30, "NO", "E211-6", "", "", "");
				IReceive2.Lines.Add(IReceiveLine211);
				IReceive2.Lines.Add(IReceiveLine212);
				IReceive2.Lines.Add(IReceiveLine213);
				IReceive2.Lines.Add(IReceiveLine221);
				IReceive2.Lines.Add(IReceiveLine222);
				IReceive2.Lines.Add(IReceiveLine223);
				IReceive3 = new WhsWarehouseTransaction();
				IReceive3.Date = ZDateTime.Today.AddDays(-2);
				IReceive3.Client = Org;
				IReceive3.Reference = "TestDataForMultiWarehouse3";
				IReceiveLine311 = CreateReceiveLine(Address3, Part1, 10, "NO", "E311-1", "", "", "");
				IReceiveLine312 = CreateReceiveLine(Address3, Part1, 20, "NO", "E311-2", "", "", "");
				IReceiveLine313 = CreateReceiveLine(Address3, Part1, 30, "NO", "E311-3", "", "", "");
				IReceiveLine321 = CreateReceiveLine(Address3, Part2, 10, "NO", "E311-4", "", "", "");
				IReceiveLine322 = CreateReceiveLine(Address3, Part2, 20, "NO", "E311-4", "", "", "");
				IReceiveLine323 = CreateReceiveLine(Address3, Part2, 30, "NO", "E311-5", "", "", "");
				IReceiveLine323b = CreateReceiveLine(Address3, Part2, 30, "NO", "E311-5", "", "", "");
				IReceive3.Lines.Add(IReceiveLine311);
				IReceive3.Lines.Add(IReceiveLine312);
				IReceive3.Lines.Add(IReceiveLine313);
				IReceive3.Lines.Add(IReceiveLine321);
				IReceive3.Lines.Add(IReceiveLine322);
				IReceive3.Lines.Add(IReceiveLine323);
				IReceive3.Lines.Add(IReceiveLine323b);
			}

			protected override void ProcessInwardMovement()
			{
				WhsReceiveProcessor processor = new WhsReceiveProcessor();
				processor.Process(Factory, IReceive1);
				processor.Process(Factory, IReceive2);
				processor.Process(Factory, IReceive3);
			}

			public WhsWarehouseTransaction IReceive2;
			public WhsWarehouseTransaction IReceive3;
			public WhsWarehouseTransactionLine IReceiveLine211;
			public WhsWarehouseTransactionLine IReceiveLine212;
			public WhsWarehouseTransactionLine IReceiveLine213;
			public WhsWarehouseTransactionLine IReceiveLine221;
			public WhsWarehouseTransactionLine IReceiveLine222;
			public WhsWarehouseTransactionLine IReceiveLine223;
			public WhsWarehouseTransactionLine IReceiveLine311;
			public WhsWarehouseTransactionLine IReceiveLine312;
			public WhsWarehouseTransactionLine IReceiveLine313;
			public WhsWarehouseTransactionLine IReceiveLine321;
			public WhsWarehouseTransactionLine IReceiveLine322;
			public WhsWarehouseTransactionLine IReceiveLine323;
			public WhsWarehouseTransactionLine IReceiveLine323b;
		}

		#endregion

		#region Products (With Attributes)

		public class TestDataForSingleWarehouseAttributes : TestDataBase
		{
			public TestDataForSingleWarehouseAttributes(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void SetupData()
			{
				base.SetupData();
				IReceive1 = new WhsWarehouseTransaction();
				IReceive1.Date = ZDateTime.Now;
				IReceive1.Client = Org;
				IReceive1.Reference = "TestDataForSingleWarehouse1";
				//IInwards1.Lines = new WarehouseTransactionLineCollection();
				IReceiveLine111 = CreateReceiveLine(Address1, Part5, 10, "NO", "E111-1", "PA111", "PA121", "");
				IReceiveLine112 = CreateReceiveLine(Address1, Part5, 20, "NO", "E111-2", "PA112", "PA122", "");
				IReceiveLine113 = CreateReceiveLine(Address1, Part5, 30, "NO", "E111-3", "PA112", "PA123", "");
				IReceiveLine121 = CreateReceiveLine(Address1, Part6, 10, "NO", "E111-4", "PA114", "PA124", "PA134");
				IReceiveLine122 = CreateReceiveLine(Address1, Part6, 20, "NO", "E111-5", "PA115", "PA125", "PA135");
				IReceiveLine123 = CreateReceiveLine(Address1, Part6, 30, "NO", "E111-6", "PA116", "PA125", "PA136");
				IReceiveLine131 = CreateReceiveLine(Address1, Part6, 10, "NO", "E111-7", "PA117", "PA127", "PA137");
				IReceiveLine132 = CreateReceiveLine(Address1, Part6, 20, "NO", "E111-8", "PA118", "PA128", "PA138");
				IReceiveLine133 = CreateReceiveLine(Address1, Part6, 30, "NO", "E111-9", "PA119", "PA129", "PA138");
				IReceive1.Lines.Add(IReceiveLine111);
				IReceive1.Lines.Add(IReceiveLine112);
				IReceive1.Lines.Add(IReceiveLine113);
				IReceive1.Lines.Add(IReceiveLine121);
				IReceive1.Lines.Add(IReceiveLine122);
				IReceive1.Lines.Add(IReceiveLine123);
				IReceive1.Lines.Add(IReceiveLine131);
				IReceive1.Lines.Add(IReceiveLine132);
				IReceive1.Lines.Add(IReceiveLine133);
				Factory.Save();
			}

			protected override void ProcessInwardMovement()
			{
				WhsReceiveProcessor processor = new WhsReceiveProcessor();
				processor.Process(Factory, IReceive1);
			}

			public WhsWarehouseTransaction IReceive1;
			public WhsWarehouseTransactionLine IReceiveLine111;
			public WhsWarehouseTransactionLine IReceiveLine112;
			public WhsWarehouseTransactionLine IReceiveLine113;
			public WhsWarehouseTransactionLine IReceiveLine121;
			public WhsWarehouseTransactionLine IReceiveLine122;
			public WhsWarehouseTransactionLine IReceiveLine123;
			public WhsWarehouseTransactionLine IReceiveLine131;
			public WhsWarehouseTransactionLine IReceiveLine132;
			public WhsWarehouseTransactionLine IReceiveLine133;
		}

		public class TestDataForMultiWarehouseAttributes : TestDataForSingleWarehouseAttributes
		{
			public TestDataForMultiWarehouseAttributes(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void SetupData()
			{
				base.SetupData();
				IReceive2 = new WhsWarehouseTransaction();
				IReceive2.Date = ZDateTime.Today.AddDays(-1);
				IReceive2.Client = Org;
				IReceive2.Reference = "TestDataForMultiWarehouse2";
				//IInwards2.Lines = new WarehouseTransactionLineCollection();
				IReceiveLine211 = CreateReceiveLine(Address2, Part5, 10, "NO", "E211-1", "PA211", "PA221", "");
				IReceiveLine212 = CreateReceiveLine(Address2, Part5, 20, "NO", "E211-2", "PA212", "PA222", "");
				IReceiveLine213 = CreateReceiveLine(Address2, Part5, 30, "NO", "E211-3", "PA213", "PA223", "");
				IReceiveLine221 = CreateReceiveLine(Address2, Part6, 10, "NO", "E211-4", "PA214", "PA224", "PA234");
				IReceiveLine222 = CreateReceiveLine(Address2, Part6, 20, "NO", "E211-4", "PA215", "PA225", "PA235");
				IReceiveLine223 = CreateReceiveLine(Address2, Part6, 30, "NO", "E211-5", "PA216", "PA226", "PA236");
				IReceive2.Lines.Add(IReceiveLine211);
				IReceive2.Lines.Add(IReceiveLine212);
				IReceive2.Lines.Add(IReceiveLine213);
				IReceive2.Lines.Add(IReceiveLine221);
				IReceive2.Lines.Add(IReceiveLine222);
				IReceive2.Lines.Add(IReceiveLine223);
				IReceive3 = new WhsWarehouseTransaction();
				IReceive3.Date = ZDateTime.Today.AddDays(-2);
				IReceive3.Client = Org;
				IReceive3.Reference = "TestDataForMultiWarehouse3";
				//IInwards3.Lines = new WarehouseTransactionLineCollection();
				IReceiveLine311 = CreateReceiveLine(Address3, Part5, 10, "NO", "E311-1", "PA311", "PA321", "");
				IReceiveLine312 = CreateReceiveLine(Address3, Part5, 20, "NO", "E311-2", "PA312", "PA322", "");
				IReceiveLine313 = CreateReceiveLine(Address3, Part5, 30, "NO", "E311-3", "PA313", "PA323", "");
				IReceiveLine321 = CreateReceiveLine(Address3, Part6, 10, "NO", "E311-4", "PA314", "PA324", "PA334");
				IReceiveLine322 = CreateReceiveLine(Address3, Part6, 20, "NO", "E311-4", "PA315", "PA325", "PA335");
				IReceiveLine323 = CreateReceiveLine(Address3, Part6, 30, "NO", "E311-5", "PA316", "PA326", "PA336");
				IReceiveLine324 = CreateReceiveLine(Address3, Part6, 30, "NO", "E311-5", "PA316", "PA326", "PA336");
				IReceiveLine323b =
					CreateReceiveLine(Address3, Part6, 30, "NO", "E311-5", "PA216", "PA226",
						"PA236"); // duplicate from Line223
				IReceive3.Lines.Add(IReceiveLine311);
				IReceive3.Lines.Add(IReceiveLine312);
				IReceive3.Lines.Add(IReceiveLine313);
				IReceive3.Lines.Add(IReceiveLine321);
				IReceive3.Lines.Add(IReceiveLine322);
				IReceive3.Lines.Add(IReceiveLine323);
				IReceive3.Lines.Add(IReceiveLine324);
				IReceive3.Lines.Add(IReceiveLine323b);
				Factory.Save();
			}

			protected override void ProcessInwardMovement()
			{
				WhsReceiveProcessor processor = new WhsReceiveProcessor();
				processor.Process(Factory, IReceive1);
				processor.Process(Factory, IReceive2);
				processor.Process(Factory, IReceive3);
			}

			public WhsWarehouseTransaction IReceive2;
			public WhsWarehouseTransaction IReceive3;
			public WhsWarehouseTransactionLine IReceiveLine211;
			public WhsWarehouseTransactionLine IReceiveLine212;
			public WhsWarehouseTransactionLine IReceiveLine213;
			public WhsWarehouseTransactionLine IReceiveLine221;
			public WhsWarehouseTransactionLine IReceiveLine222;
			public WhsWarehouseTransactionLine IReceiveLine223;
			public WhsWarehouseTransactionLine IReceiveLine311;
			public WhsWarehouseTransactionLine IReceiveLine312;
			public WhsWarehouseTransactionLine IReceiveLine313;
			public WhsWarehouseTransactionLine IReceiveLine321;
			public WhsWarehouseTransactionLine IReceiveLine322;
			public WhsWarehouseTransactionLine IReceiveLine323;
			public WhsWarehouseTransactionLine IReceiveLine324;
			public WhsWarehouseTransactionLine IReceiveLine323b;
		}

		#endregion

		#endregion

		#region Receive Processor

		public class WhsReceiveProcessor
		{
			#region Initialise

			void Initialise(BusinessObjectFactory factory, IWhsWarehouseTransaction input)
			{
				if (input == null)
				{
					throw new ArgumentNullException("Input cannot be null");
				}

				if (factory == null)
				{
					factory = new BusinessObjectFactory("Factory cannot be null");
				}

				this.Factory = factory;
				this.Input = input;
			}

			#endregion

			#region Process

			public IWhsWarehouseTransaction Process(BusinessObjectFactory factory, IWhsWarehouseTransaction input)
			{
				Initialise(factory, input);
				ProcessCore();
				return input;
			}

			void ProcessCore()
			{
				Receive = Factory.New<WhsReceive>();
				Receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				Receive.WD_OH_Client = Input.Client.PK;
				Receive.WD_WW_Whs = FindWarehouse(Input.Lines[0].Warehouse).PK;
				Receive.WD_ExternalReference = Input.Reference;
				Receive.WD_ArrivalDate = Input.Date.ToOffset();
				Receive.NotificationManager.Push(new TestNotificationBuffer());
				foreach (IWhsWarehouseTransactionLine line in Input.Lines)
				{
					WhsInventoryView inventory = Receive.Lines.AddNew().Inventory[0];
					inventory.WI_OP = line.Product.PK;
					inventory.WI_InDocketLineUnits = line.Quantity;
					var entryKey = WhsBondedWarehouseAttribute.BuildKey(line.EntryKey, line.EntryLineNumber);
					inventory.WI_BondedEntryKey = entryKey;
					inventory.WI_PartAttrib1 = line.PartAttrib1;
					inventory.WI_PartAttrib2 = line.PartAttrib2;
					inventory.WI_PartAttrib3 = line.PartAttrib3;
					inventory.CustomsData.WB_EntryLineNo = line.EntryLineNumber;
					inventory.CustomsData.WB_EntryKey = line.EntryKey;
					inventory.CustomsData.WB_BondedWhsQty = 1m;
				}

				Receive.AllocateLocationsWithMock();
				Receive.FinaliseDocket();
				if (!Receive.IsFinalised)
				{
					throw new CouldNotProcessReceiveException();
				}
			}

			// find warehouse from dbo.orgaddress
			WhsWarehouse FindWarehouse(IOrgAddress address)
			{
				ZQuery filter = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, address.PK);
				WhsWarehouse result = Factory.LoadTop1<WhsWarehouse>(filter);
				return result;
			}

			WhsReceive Receive;
			IWhsWarehouseTransaction Input;
			BusinessObjectFactory Factory;

			#endregion
		}

		#endregion
	}
}
