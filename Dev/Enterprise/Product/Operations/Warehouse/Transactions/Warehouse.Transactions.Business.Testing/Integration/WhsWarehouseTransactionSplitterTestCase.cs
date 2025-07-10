using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal abstract class WhsWarehouseTransactionSplitterTestCase : WhsTestCaseWithFactory
	{
		public void TestSplit()
		{
			SetupDataForSplitTest();
			AssertOutput(GetNewSplitter().Split(Input));
		}

		public void TestCopy()
		{
			SetupDataForCopyTest();
			AssertCopy(GetNewSplitter().Split(Input));
		}

		void AssertOutput(IWhsWarehouseTransaction[] output)
		{
			AssertOutputCounts(output);
			foreach (IWhsWarehouseTransaction tran in output)
			{
				AssertTransactionEquals(Input, tran);
			}

			AssertOutputLines(output);
		}

		protected virtual void AssertCopy(IWhsWarehouseTransaction[] output)
		{
			Assert("Precondition", output.Length == 1 && output[0].Lines.Count == 1);
			AssertTransactionEquals(Input, output[0]);
			AssertTransactionLineEquals(Input.Lines[0], output[0].Lines[0]);
		}

		#region Implementation

		protected virtual void AssertOutputCounts(IWhsWarehouseTransaction[] output)
		{
			Fail("TestSubClass to implement Assertion");
		}

		protected virtual void AssertOutputLines(IWhsWarehouseTransaction[] output)
		{
			Fail("TestSubClass to implement Assertion");
		}

		protected virtual void AssertTransactionEquals(IWhsWarehouseTransaction tran1, IWhsWarehouseTransaction tran2)
		{
			AssertEquals(tran1.Client, tran2.Client);
			AssertEquals(tran1.Date, tran2.Date);
			AssertEquals(tran1.ExternalPK, tran2.ExternalPK);
			AssertEquals(tran1.Reference, tran2.Reference);
			AssertEquals(tran1.TransportCompany, tran2.TransportCompany);
		}

		protected virtual void AssertTransactionLineEquals(IWhsWarehouseTransactionLine line1,
			IWhsWarehouseTransactionLine line2)
		{
			AssertEquals(line1.EntryKey, line2.EntryKey);
			AssertEquals(line1.EntryLineNumber, line2.EntryLineNumber);
			AssertEquals(line1.PartAttrib1, line2.PartAttrib1);
			AssertEquals(line1.PartAttrib2, line2.PartAttrib2);
			AssertEquals(line1.PartAttrib3, line2.PartAttrib3);
			AssertEquals(line1.Product, line2.Product);
			AssertEquals(line1.Quantity, line2.Quantity);
			AssertEquals(line1.QuantityUnit, line2.QuantityUnit);
			AssertEquals(line1.Warehouse, line2.Warehouse);
		}

		#endregion

		#region Setup Data

		protected virtual WhsWarehouseTransaction GetNewTestTransaction()
		{
			return new WhsWarehouseTransaction();
		}

		protected virtual WhsWarehouseTransactionLine GetNewTestTransactionLine()
		{
			return new WhsWarehouseTransactionLine();
		}

		protected virtual void SetupDataForSplitTest()
		{
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			OrgHeader client = helper.CreateClient();
			OrgAddress address1 = Factory.New<OrgAddress>();
			OrgAddress address2 = Factory.New<OrgAddress>();
			address1.OA_Code = "WHS1";
			address2.OA_Code = "WHS2";
			OrgSupplierPart part1 = helper.CreateProduct(client, "P1");
			OrgSupplierPart part2 = helper.CreateProduct(client, "P2");
			Input = GetNewTestTransaction();
			Input.Client = client;
			Input.Date = ZDateTime.Today;
			Input.ExternalPK = ZGuid.NewZGuid();
			//Input.Lines = new WarehouseTransactionLineCollection();
			Input.Reference = "INWREF";
			Input.TransportCompany = client;
			InputLine1 = GetNewTestTransactionLine();
			InputLine1.EntryKey = "E3";
			InputLine1.EntryLineNumber = 2;
			InputLine1.PartAttrib1 = "PA41";
			InputLine1.PartAttrib2 = "PA42";
			InputLine1.PartAttrib3 = "PA43";
			InputLine1.Product = part2;
			InputLine1.Quantity = 130m;
			InputLine1.QuantityUnit = "KG";
			InputLine1.Warehouse = address2;
			Input.Lines.Add(InputLine1);
			InputLine2 = GetNewTestTransactionLine();
			InputLine2.EntryKey = "E3";
			InputLine2.EntryLineNumber = 1;
			InputLine2.PartAttrib1 = "PA51";
			InputLine2.PartAttrib2 = "PA52";
			InputLine2.PartAttrib3 = "PA53";
			InputLine2.Product = part1;
			InputLine2.Quantity = 140m;
			InputLine2.QuantityUnit = "KG";
			InputLine2.Warehouse = address1;
			Input.Lines.Add(InputLine2);
			InputLine3 = GetNewTestTransactionLine();
			InputLine3.EntryKey = "E1";
			InputLine3.EntryLineNumber = 1;
			InputLine3.PartAttrib1 = "PA11";
			InputLine3.PartAttrib2 = "PA12";
			InputLine3.PartAttrib3 = "PA13";
			InputLine3.Product = part2;
			InputLine3.Quantity = 100m;
			InputLine3.QuantityUnit = "KG";
			InputLine3.Warehouse = address1;
			Input.Lines.Add(InputLine3);
			InputLine4 = GetNewTestTransactionLine();
			InputLine4.EntryKey = "E2";
			InputLine4.EntryLineNumber = 1;
			InputLine4.PartAttrib1 = "PA21";
			InputLine4.PartAttrib2 = "PA22";
			InputLine4.PartAttrib3 = "PA23";
			InputLine4.Product = part1;
			InputLine4.Quantity = 110m;
			InputLine4.QuantityUnit = "KG";
			InputLine4.Warehouse = address2;
			Input.Lines.Add(InputLine4);
			InputLine5 = GetNewTestTransactionLine();
			InputLine5.EntryKey = "E3";
			InputLine5.EntryLineNumber = 1;
			InputLine5.PartAttrib1 = "PA31";
			InputLine5.PartAttrib2 = "PA32";
			InputLine5.PartAttrib3 = "PA33";
			InputLine5.Product = part1;
			InputLine5.Quantity = 120m;
			InputLine5.QuantityUnit = "KG";
			InputLine5.Warehouse = address1;
			Input.Lines.Add(InputLine5);
		}

		protected virtual void SetupDataForCopyTest()
		{
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			OrgHeader client = helper.CreateClient();
			OrgAddress address1 = Factory.New<OrgAddress>();
			address1.OA_Code = "WHS1";
			OrgSupplierPart part1 = helper.CreateProduct(client, "P1");
			Input = GetNewTestTransaction();
			Input.Client = client;
			Input.Date = ZDateTime.Today;
			Input.ExternalPK = ZGuid.NewZGuid();
			//Input.Lines = new WarehouseTransactionLineCollection();
			Input.Reference = "INWREF";
			Input.TransportCompany = client;
			InputLine1 = GetNewTestTransactionLine();
			InputLine1.EntryKey = "E3";
			InputLine1.EntryLineNumber = 2;
			InputLine1.PartAttrib1 = "PA11";
			InputLine1.PartAttrib2 = "PA12";
			InputLine1.PartAttrib3 = "PA13";
			InputLine1.Product = part1;
			InputLine1.Quantity = 130m;
			InputLine1.QuantityUnit = "KG";
			InputLine1.Warehouse = address1;
			Input.Lines.Add(InputLine1);
		}

		#endregion

		protected abstract WhsWarehouseTransactionSplitter GetNewSplitter();
		protected WhsWarehouseTransaction Input;
		protected WhsWarehouseTransactionLine InputLine1;
		protected WhsWarehouseTransactionLine InputLine2;
		protected WhsWarehouseTransactionLine InputLine3;
		protected WhsWarehouseTransactionLine InputLine4;
		protected WhsWarehouseTransactionLine InputLine5;
	}
}
