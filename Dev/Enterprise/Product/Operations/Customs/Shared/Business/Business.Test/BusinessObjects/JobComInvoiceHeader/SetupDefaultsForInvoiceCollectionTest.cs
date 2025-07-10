using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SetupDefaultsForInvoiceCollectionTest : TestCaseWithFactory
	{
		public void TestSetDefaultValuesForNewChildFromJobDeclarationSupplier()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_RX_NKEXDefCurrency = Core.Constants.CurrencyCodes.UnitedKingdom;
			supplier.MiscServ.OM_EXDefaultIncoTerm = "XYZ";

			var testJobDeclaration = BaseJobDeclaration.New(Factory);
			testJobDeclaration.JE_OH_Supplier = supplier.PK;
			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Failed to add new record", 1, testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count);
			AssertEquals("Default Supplier", supplier.PK, testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);

			AssertEquals("Default Export IncoTerm", supplier.MiscServ.OM_EXDefaultIncoTerm, testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_IncoTerm);
			Assert("FOBCurrency Should not be filled in", !testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_FOBCurrency.IsEmpty);
			Assert("CIFCurrency should not be filled in", !testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_Calc_CIFCurrency.IsEmpty);
		}

		public void TestDefaultValuesForNewChild()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = "FOB";

			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;
			testDec.JE_OH_Supplier = Supplier.PK;

			var testItem1 = testDec.Invoices.AddNew();
			AssertEquals("Weight", "KG", testItem1.JZ_WeightUQ);
			AssertEquals("Supplier", testDec.JE_OH_Supplier, testItem1.JZ_OH_Supplier);
			AssertEquals("Incoterm", "FOB", testItem1.JZ_IncoTerm);
		}

		public void TestDefaultValuesForNewChild_SupportAdditionalInvoices()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = "FOB";

			var testDec = Factory.New<JobDeclarationSupportAdditionalInvoices>();
			testDec.JE_JS = shipment.PK;
			testDec.JE_OH_Supplier = Supplier.PK;

			var invoice = Factory.New<JobComInvoiceHeaderSupportAdditionalDeclarations>();
			invoice.AttachToAdditionalDeclaration(testDec);

			var testItem1 = testDec.Invoices.AddNew();
			AssertEquals("Weight", "KG", testItem1.JZ_WeightUQ);
			AssertEquals("Supplier", testDec.JE_OH_Supplier, testItem1.JZ_OH_Supplier);
			AssertEquals("Incoterm", "FOB", testItem1.JZ_IncoTerm);
		}

		public void TestSetUpDataWhenCollectionNotEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_INCO = "FOB";

			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;
			testDec.JE_OH_Supplier = Supplier.PK;

			var testItem1 = testDec.Invoices.AddNew();
			AssertEquals("Weight", "KG", testItem1.JZ_WeightUQ);
			AssertEquals("Supplier", testDec.JE_OH_Supplier, testItem1.JZ_OH_Supplier);
			AssertEquals("Incoterm", "FOB", testItem1.JZ_IncoTerm);

			var supplier2 = Factory.New<OrgHeader>();
			var supplierAddress = supplier2.Addresses.AddNew();

			testItem1.JZ_OH_Supplier = supplier2.PK;
			testItem1.JZ_OA_SupplierAddress = supplierAddress.PK;
			testItem1.JZ_RX_NKInvoice_Currency = "USD";
			testItem1.JZ_IncoTerm = "CIF";

			var testItem2 = testDec.Invoices.AddNew();
			AssertEquals("Group Header PK", testItem1.JZ_JZ_GroupInvoiceFK, testItem2.JZ_JZ_GroupInvoiceFK);
			AssertEquals("Supplier Address", supplierAddress.PK, testItem2.JZ_OA_SupplierAddress);
			AssertEquals("Supplier", supplier2.PK, testItem2.JZ_OH_Supplier);
			AssertEquals("Currency", "USD", testItem2.JZ_RX_NKInvoice_Currency);
			AssertEquals("Incoterm", "CIF", testItem2.JZ_IncoTerm);
		}

		public void TestSettingDefaultValuesDoesntMarkApportionmentDirty()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;
			testDec.JE_ShipmentIncoTerm = "FOB";

			AssertEquals("Apportionment is not dirty", false, testDec.ApportionmentDirty);
			var invoice = testDec.Invoices.AddNew();
			AssertEquals("PreCondition:Incoterm has been set", testDec.JE_ShipmentIncoTerm, invoice.JZ_IncoTerm);
			AssertEquals("Apportionment is not dirty", false, testDec.ApportionmentDirty);
		}

		public void TestDefaultCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var uSD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = uSD.RX_Code;
				Factory.Save();

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var invoice = declaration.Invoices.AddNew();
				AssertEquals("", invoice.JZ_RX_NKInvoice_Currency);

				invoice.Delete();
				CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				invoice = declaration.Invoices.AddNew();
				AssertEquals(uSD.RX_Code, invoice.JZ_RX_NKInvoice_Currency);
			}
		}

		OrgHeader Supplier
		{
			get { return supplier ?? (supplier = Factory.LoadTop1<OrgHeader>(new ZQuery())); }
		}
		OrgHeader supplier;
	}
}
