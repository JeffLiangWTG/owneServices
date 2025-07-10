using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BondedWarehouseTransactionTestWithInvoiceLines : TestCaseWithFactory
	{
		public void TestIsWarehousedByExternalAgent()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsWarehousedByExternalAgent", false, bondedWarehouseTransaction.IsWarehousedByExternalAgent);
			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			AssertEquals("IsWarehousedByExternalAgent", true, bondedWarehouseTransaction.IsWarehousedByExternalAgent);
		}

		public void TestLinesIMP()
		{
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Today;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("No lines with products and invoice qty and uq found", 0, bondedWarehouseTransaction.Lines.Count);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "~~~";
			declaration.JE_OH_Importer = buyer.PK;
			part.RelatedOrganisations.AddOrganisationIfNotExist(buyer.PK, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			BaseJobComInvoiceLine invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 10;

			invoiceLine.JI_PartNo = "~~~";
			invoiceLine.JI_InvoiceUQ = "";
			invoiceLine.JI_InvoiceQuantity = 0;
			AssertEquals("No lines with products and invoice qty and uq and pack to bond found", 0, bondedWarehouseTransaction.Lines.Count);

			invoiceLine.JI_InvoiceQuantity = 1;
			AssertEquals("No lines with products and invoice qty and uq and pack to bond found", 0, bondedWarehouseTransaction.Lines.Count);

			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("No lines with products and invoice qty and uq and pack to bond found", 0, bondedWarehouseTransaction.Lines.Count);

			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			bondedWarehouseTransaction = declaration.GetNewBondedWarehouseTransactionForTesting();
			bondedWarehouseTransaction.SetInvoiceLineMode();
			AssertEquals("Found correct line", 1, bondedWarehouseTransaction.Lines.Count);
			AssertEquals("Found correct entry line", 1m, bondedWarehouseTransaction.Lines[0].Quantity);
			AssertEquals("Found correct entry line", "KG", bondedWarehouseTransaction.Lines[0].QuantityUnit);
		}

		public void TestLinesEXW()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(1).ToDateTime());
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				invoiceLine.JI_InvoiceQuantity = 9024;
				BaseJobComInvoiceLine invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
				invoiceLine2.JI_InvoiceQuantity = 4;
				invoiceLine2.SetUseBondedWarehouseAutomationForTesting(false);
				AssertEquals("No lines with products and invoice qty and uq found", 0, bondedWarehouseTransaction.Lines.Count);

				invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
				bondedWarehouseTransaction = declaration.GetNewBondedWarehouseTransactionForTesting();
				bondedWarehouseTransaction.SetInvoiceLineMode();
				AssertEquals("Found correct line", 1, bondedWarehouseTransaction.Lines.Count);
				AssertEquals("Found correct entry line", 9024m, bondedWarehouseTransaction.Lines[0].Quantity);

				declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(2);
				bondedWarehouseTransaction = declaration.GetNewBondedWarehouseTransactionForTesting();
				bondedWarehouseTransaction.SetInvoiceLineMode();
				AssertEquals("All lines", 2, bondedWarehouseTransaction.Lines.Count);
				AssertEquals(9024m, bondedWarehouseTransaction.Lines[0].Quantity);
				AssertEquals(4m, bondedWarehouseTransaction.Lines[1].Quantity);
			}
		}

		public void TestAdditionalReferencesContainMasterBill()
		{
			CheckAdditionalReferencesAdded(declaration.JE_MasterBillInfo, "MAB");
		}

		public void TestAdditionalReferencesContainVoyageFlight()
		{
			CheckAdditionalReferencesAdded(declaration.JE_VoyageFlightNoInfo, "VFN");
		}

		public void TestAdditionalReferencesContainHouseBill()
		{
			CheckAdditionalReferencesAdded(declaration.JE_HouseBillInfo, "HSB");
		}

		void CheckAdditionalReferencesAdded(ZPropertyInfo property, string correctType)
		{
			AssertEquals("Additional references", 0, bondedWarehouseTransaction.AdditionalReferences.Count());
			property.Value = new ZString("12345");
			AssertEquals("Additional references", 1, bondedWarehouseTransaction.AdditionalReferences.Count());
			AssertEquals("Correct type", correctType, bondedWarehouseTransaction.AdditionalReferences.ElementAt(0).Type);
			AssertEquals("Correct value", "12345", bondedWarehouseTransaction.AdditionalReferences.ElementAt(0).Value);
		}

		public void TestTransportCompany()
		{
			var transport = Factory.NewWithValidTestData<OrgHeader>();
			transport.Addresses.AddNewMainAddress();
			declaration.DeliveryOrPickupCartageCoPK = transport.PK;
			AssertEquals(bondedWarehouseTransaction.TransportCompany, transport);
		}

		public void TestDate()
		{
			declaration.JE_DateOfFirstArrival = new ZDateTime(2006, 1, 1);
			AssertEquals(bondedWarehouseTransaction.Date, new ZDateTime(2006, 1, 1));
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2005, 1, 1));
			AssertEquals(bondedWarehouseTransaction.Date, new ZDateTime(2005, 1, 1));
		}

		public void TestWarehouse()
		{
			declaration.WarehouseDocAddress.E2_OA_Address = OrgHeader.New(Factory).MainAddress.PK;
			AssertEquals(bondedWarehouseTransaction.Warehouse, declaration.WarehouseAddress);
		}

		public void TestClient()
		{
			declaration.JE_OH_Importer = OrgHeader.New(Factory).PK;
			AssertEquals(bondedWarehouseTransaction.Client, declaration.Importer);
		}

		public void TestProblems()
		{
			AssertNotNull(bondedWarehouseTransaction.Problems);
		}

		public void TestExternalPK()
		{
			AssertEquals(declaration.PK, bondedWarehouseTransaction.ExternalPK);
		}

		public void TestReference()
		{
			declaration.JE_DeclarationReference = "DeclarationReference1";
			AssertEquals(declaration.JE_DeclarationReference, bondedWarehouseTransaction.Reference);
		}

		internal CusEntryHeader entry;
		internal BondedWarehouseTransaction bondedWarehouseTransaction;
		internal BaseJobDeclaration declaration;
		internal BaseJobComInvoiceLine invoiceLine;
		internal CusEntryLine entryLine;

		protected virtual BaseJobDeclaration GetNewJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupObjects();
		}

		protected virtual void SetupObjects()
		{
			declaration = GetNewJobDeclaration();
			invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			bondedWarehouseTransaction = declaration.GetNewBondedWarehouseTransactionForTesting();
			bondedWarehouseTransaction.SetInvoiceLineMode();
		}
	}
}
