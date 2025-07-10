using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ProductInformation))]
	sealed class ProductInformationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBaseProperties()
		{
			AssertType<ProductInformationDeliveryContactCollection>("DeliveryContacts", ProductInfo.DeliveryContacts);
			AssertEquals("SupplierList", "SUP1 - Test Supplier 1\r\nSUP2 - Test Supplier 2", ProductInfo.SupplierList.ElementsAsString);
		}

		public void TestConstructor()
		{
			var productInformation = new ProductInformation(Factory, Declaration, Array.Empty<BaseJobComInvoiceLine>());
			AssertEquals("Please select at least one invoice line.", productInformation.ConstructionErrorMessage);

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "Test Supplier 1";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TESTJZ001";
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Calc_Invoice = "TESTJZ001";

			var invoiceLines = new BaseJobComInvoiceLine[] { invoiceLine1 };
			productInformation = new ProductInformation(Factory, Declaration, invoiceLines);
			AssertEquals(@"Cannot find the supplier for below Invoice Line(s):
TESTJZ001 - 1
", productInformation.ConstructionErrorMessage);

			invoice1.JZ_OH_Supplier = supplier1.PK;
			productInformation = new ProductInformation(Factory, Declaration, invoiceLines);
			AssertNullOrEmpty(productInformation.ConstructionErrorMessage);
		}

		public void TestNewProductInformationDeliveryContext()
		{
			var receivers = new string[] { "rec1", "rec2" };

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = ZString.Empty;
			staff.GS_EmailAddress = "testuser@email.address";

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var context = ProductInfo.NewProductInformationDeliveryContextForTest("SUP1", "Test Supplier 1", receivers);
				CombineAssertions(() =>
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					AssertEquals("Sender", "testuser@email.address", context.sender);
					AssertEquals("Receivers", receivers, context.receivers);
					AssertEquals("Integration -> Type", "cwnext-declaration", context.integration.type);
					AssertEquals("Integration -> E-Hub ID", registrationKey.EnterpriseCode + GlbCompany.CurrentCompany.GC_Code + registrationKey.ServerCode, context.integration.ehub_id);
					AssertEquals("Integration -> Declaration Number", "TestJE001", context.integration.declaration_number);
					AssertEquals("Context -> Supplier", "Test Supplier 1", context.context.supplier);
					AssertEquals("Context -> Consignee", "Test Importer", context.context.consignee);
					AssertEquals("Context -> MAWB", "M0001", context.context.masterbill);
					AssertEquals("Context -> HAWB", "H0001", context.context.housebill);
					AssertEquals("Context -> Voyage/Flight", "QF001", context.context.voyage_flight);
					AssertEquals("Context -> Owner Ref", "OwnerRef", context.context.owner_ref);
					AssertEquals("Context -> Port of Loading", "TPORT", context.context.port_of_loading);
				});

				AssertEquals("Requested Classifications Length", 1, context.requested_classifications.Length);
				CombineAssertions(() =>
				{
					var requestedClassification = context.requested_classifications[0];
					AssertEquals("Requested Classification -> Import/Export", "e", requestedClassification.imp_exp);
					AssertEquals("Requested Classification -> Country", "er", requestedClassification.country);
				});

				AssertEquals("context should has 1 line for supplier 'SUP1'", 1, context.lines.Length);
				CombineAssertions(() =>
				{
					var line = context.lines[0];
					AssertEquals("line1: Integration -> Invoice Number", "TESTJZ001", line.integration.invoice_number);
					AssertEquals("line1: Integration -> Invoice Line Number", "1", line.integration.invoice_line);
					AssertEquals("line1: Context -> Product Code", "Prod01", line.context.part_no);
					AssertEquals("line1: Goods Description ", "Invoice Line 1 Description", line.desc);
				});

				ProductInfo.SendFrom = "overridden@email.address";
				context = ProductInfo.NewProductInformationDeliveryContextForTest("SUP1", "Test Supplier 1", receivers);
				AssertEquals("Overridden Sender", "overridden@email.address", context.sender);
			}
		}

		public void TestGetDeliveryContext()
		{
			var productInfoNotCon = new ProductInformation(Factory, Declaration, Array.Empty<BaseJobComInvoiceLine>());
			AssertNotNullOrEmpty(productInfoNotCon.ConstructionErrorMessage);
			(var errorMessage, var contexts) = productInfoNotCon.GetDeliveryContext();
			Assert("Error Message for not successfully created ProductInformation", errorMessage.StartsWith("Some error(s) block to deliver messages:"));

			ProductInfo.SendFrom = "123";
			(errorMessage, contexts) = ProductInfo.GetDeliveryContext();
			AssertEquals("Error Message when has error", @"Some input has error:
Send From: Email Address is not valid .", errorMessage);

			ProductInfo.SendFrom = "test@email.address";
			var contact1 = ProductInfo.DeliveryContacts.AddNew();
			(errorMessage, contexts) = ProductInfo.GetDeliveryContext();
			AssertEquals("Error Message when delivery contact missing info", @"Some input has error:
Supplier: Please enter a Supplier.
Delivery Method: Please enter a value.
Delivery Method Description: Please enter a value.", errorMessage);

			contact1.Supplier = "SUP1";
			contact1.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact1.DeliveryAddress = "sup1@email.address";

			(errorMessage, contexts) = ProductInfo.GetDeliveryContext();
			AssertEquals("Error Message when has supplier not input receivers",
				@"Each supplier must have at least one email address entered. Below supplier(s) have no email address:
SUP2
", errorMessage);

			var contact2 = ProductInfo.DeliveryContacts.AddNew();
			contact2.Supplier = "SUP2";
			contact2.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			contact2.DeliveryAddress = "sup2@email.address";
			(errorMessage, contexts) = ProductInfo.GetDeliveryContext();
			AssertNullOrEmpty(errorMessage);
			AssertEquals("should has 2 contexts", 2, contexts.Length);

			var context1 = contexts[0];
			var context2 = contexts[1];
			AssertEquals("Receivers", "sup1@email.address", string.Join(string.Empty, context1.receivers));
			AssertEquals("Receivers", "sup2@email.address", string.Join(string.Empty, context2.receivers));
		}

		public void TestValidatesJobType()
		{
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var productInfo = new ProductInformation(Factory, Declaration, InvoiceLines);
			AssertNull(productInfo.ConstructionErrorMessage);
			Declaration.JE_MessageType = "INV";
			productInfo = new ProductInformation(Factory, Declaration, InvoiceLines);
			AssertEquals("Job Type is not Import or Export. Cannot Create.", productInfo.ConstructionErrorMessage);
		}

		public void TestValidatesConsignee()
		{
			var productInfo = new ProductInformation(Factory, Declaration, InvoiceLines);
			AssertNull(productInfo.ConstructionErrorMessage);
			Declaration.JE_OH_Importer = Guid.Empty;
			productInfo = new ProductInformation(Factory, Declaration, InvoiceLines);
			AssertEquals("Declaration has no Consignee/Importer. Cannot Create.", productInfo.ConstructionErrorMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProductInformation(Factory, Declaration, InvoiceLines);
		}

		BaseJobDeclaration declaration;
		BaseJobDeclaration Declaration => declaration ??= CreateTestDeclaration(Factory);

		BaseJobComInvoiceLine[] InvoiceLines => Declaration.InvoiceLines.Cast<BaseJobComInvoiceLine>().ToArray();

		ProductInformationForTest productInfo;
		ProductInformationForTest ProductInfo => productInfo ??= new ProductInformationForTest(Factory, Declaration, InvoiceLines);

		public static BaseJobDeclaration CreateTestDeclaration(BusinessObjectFactory factory)
		{
			var supplier1 = factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "Test Supplier 1";
			var supplier2 = factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_Code = "SUP2";
			supplier2.OH_FullName = "Test Supplier 2";

			var consignee = factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "IMP1";
			consignee.OH_FullName = "Test Importer";

			var declaration = factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "TestJE001";
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MasterBill = "M0001";
			declaration.JE_HouseBill = "H0001";
			declaration.JE_VoyageFlightNo = "QF001";
			declaration.JE_OwnerRef = "OwnerRef";
			declaration.JE_RL_NKPortOfLoading = "TPORT";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TESTJZ001";
			invoice1.JZ_OH_Supplier = supplier1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TESTJZ002";
			invoice2.JZ_OH_Supplier = supplier2.PK;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Calc_Invoice = "TESTJZ001";
			invoiceLine1.JI_PartNo = "Prod01";
			invoiceLine1.JI_Description = "Invoice Line 1 Description";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Calc_Invoice = "TESTJZ002";
			invoiceLine2.JI_PartNo = "Prod02";
			invoiceLine2.JI_Description = "Invoice Line 2 Description";

			return declaration;
		}

		class ProductInformationForTest : ProductInformation
		{
			public ProductInformationForTest(BusinessObjectFactory factory, BaseJobDeclaration declaration, BaseJobComInvoiceLine[] invoiceLines)
				: base(factory, declaration, invoiceLines)
			{
			}

			public ProductInformationDeliveryContext NewProductInformationDeliveryContextForTest(string supplierCode, string supplierName, string[] receivers) => NewProductInformationDeliveryContext(supplierCode, supplierName, receivers);
		}
	}
}
