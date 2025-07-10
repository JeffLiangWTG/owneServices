using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	internal class WhsBondedWarehouseTransactionTest : WhsTestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals("Date", ZDateTime.Now.Date, header.Date);
			AssertEquals("Client", true, header.IsWarehousedByExternalAgent);
			AssertEquals("Line", "E10000", header.Lines[0].EntryKey);
			AssertEquals("TransportCompany", testClient, header.TransportCompany);
			AssertEquals("Warehouse", testClient.MainAddress, header.Warehouse);
			AssertEquals("AdditionalReferences", references, header.AdditionalReferences);
			AssertEquals("IsWarehousedByExternalAgent", true, header.IsWarehousedByExternalAgent);
			AssertEquals("JobDeclaration", true, header.IsWarehousedByExternalAgent);
			AssertEquals("Reference", "DeclarationReference1", header.Reference);
			AssertEquals("DeclarationPK", declarationPK, header.ExternalPK);
			AssertEquals("Problems", true, header.Problems.HasErrors);
			AssertEquals("Problems", 1, header.Problems.ErrorList.Count);
		}

		public void TestCopy()
		{
			WhsBondedWarehouseTransaction copiedLine = WhsBondedWarehouseTransaction.Copy(header);
			AssertEquals("Date", ZDateTime.Now.Date, copiedLine.Date);
			AssertEquals("Client", testClient, copiedLine.Client);
			AssertEquals("IsWarehousedByExternalAgent", true, copiedLine.IsWarehousedByExternalAgent);
			AssertEquals("Line", "E10000", copiedLine.Lines[0].EntryKey);
			AssertEquals("TransportCompany", testClient, copiedLine.TransportCompany);
			AssertEquals("AdditionalReferences", references, copiedLine.AdditionalReferences);
		}

		#region IWhsBondedWarehouseTransaction Members

		public void TestIWhsBondedWarehouseTransaction_AdditionalReferences()
		{
			AssertEquals(1, ((IWhsBondedWarehouseTransaction)header).AdditionalReferences.Count());
			header.AdditionalReferences = new AdditionalReference[]
			{
				new AdditionalReference("REFERENCE1", "1"), new AdditionalReference("REFERENCE2", "2")
			};
			AssertEquals(2, ((IWhsBondedWarehouseTransaction)header).AdditionalReferences.Count());
			AssertEquals("REFERENCE1", ((IWhsBondedWarehouseTransaction)header).AdditionalReferences.ElementAt(0).Type);
			AssertEquals("2", ((IWhsBondedWarehouseTransaction)header).AdditionalReferences.ElementAt(1).Value);
		}

		public void TestIWhsBondedWarehouseTransaction_IsWarehousedByExternalAgent()
		{
			AssertEquals(true, ((IWhsBondedWarehouseTransaction)header).IsWarehousedByExternalAgent);
			header.IsWarehousedByExternalAgent = false;
			AssertEquals(false, ((IWhsBondedWarehouseTransaction)header).IsWarehousedByExternalAgent);
		}

		public void TestIWhsBondedWarehouseTransaction_Lines()
		{
			AssertEquals(1, ((IWhsBondedWarehouseTransaction)header).Lines.Count);
			AssertCollectionContains(line, ((IWhsBondedWarehouseTransaction)header).Lines);
		}

		public void TestIWhsBondedWarehouseTransaction_Warehouse()
		{
			AssertEquals(testClient.MainAddress, ((IWhsBondedWarehouseTransaction)header).Warehouse);
			OrgAddress newAddress = Factory.New<OrgAddress>();
			header.Warehouse = newAddress;
			AssertEquals(newAddress, ((IWhsBondedWarehouseTransaction)header).Warehouse);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testClient = OrgHeader.New(Factory);
			testClient.Addresses.AddNewMainAddress();
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			testWhs = helper.CreateWarehouse("Test");
			references = new AdditionalReference[] { new AdditionalReference("MASTERBILL", "123456") };
			SetUpLine();
			SetUpHeader();
		}

		WhsBondedWarehouseTransaction header;
		WhsBondedWarehouseTransactionLine line;
		OrgHeader testClient;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetUp")]
		WhsWarehouse testWhs;
		AdditionalReference[] references;
		ZGuid declarationPK;

		void SetUpHeader()
		{
			header = new WhsBondedWarehouseTransaction();
			declarationPK = ZGuid.NewZGuid();
			header.ExternalPK = declarationPK;
			header.Reference = "DeclarationReference1";
			header.AdditionalReferences = references;
			header.Date = ZDateTime.Now.Date;
			header.Client = testClient;
			header.Warehouse = testClient.MainAddress;
			header.IsWarehousedByExternalAgent = true;
			header.TransportCompany = header.Client;
			header.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line);
			header.Problems.ErrorList.Add("1");
		}

		void SetUpLine()
		{
			line = new WhsBondedWarehouseTransactionLine();
			line.AddInfo = "AddInfo";
			line.BondedWarehouseQuantity = 10m;
			line.BondedWarehouseQuantityUnit = "L";
			line.PartAttrib1 = "Att1";
			line.PartAttrib2 = "Att2";
			line.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");
			line.CustomsQuantity = 10m;
			line.CustomsQuantityUnit = "L";
			line.EntryDate = ZDateTime.Today.Date;
			line.EntryKey = "E10000";
			line.EntryLineNumber = 1;
			line.Product = OrgSupplierPart.New(Factory);
			line.Quantity = 10m;
			line.QuantityUnit = "L";
			line.ValueForDuty = 10m;
			line.TILV = new Money(10m, GlbCompany.CurrentCompany.LocalCurrency);
			line.Warehouse = testClient.MainAddress;
		}

		#endregion
	}
}
