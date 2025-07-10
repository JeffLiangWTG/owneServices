using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX601MessageSendingObjectAdditionalMessageErrorCollector))]
	sealed class NX601MessageSendingObjectAdditionalMessageErrorCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidateGoodsType()
		{
			var messageError = "Goods Type: You have not entered a Goods Type.";
			invoiceLine.JI_GoodsType = ZString.Empty;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Goods Type");
			invoiceLine.JI_GoodsType = CPT_107_601_GoodsTypeList.Codes._1;
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Goods Type");
		}

		[ExpectNoExceptions]
		public void TestValidateJI_InnerPackType()
		{
			var messageError = "Packaging Type: You have not entered a Packaging Type.";
			invoiceLine.JI_InnerPackType = ZString.Empty;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Packaging Type");
			invoiceLine.JI_InnerPackType = CPT_115_InnerPackageTypeList.Codes._1;
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Packaging Type");
		}

		[ExpectNoExceptions]
		public void TestValidateJI_InnerPackingMaterial()
		{
			var messageError = "Packaging Material: You have not entered a Packaging Material.";
			invoiceLine.JI_InnerPackingMaterial = ZString.Empty;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Packaging Material");
			invoiceLine.JI_InnerPackingMaterial = CPT_114_InnerPackingMaterialList.Codes._1;
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Packaging Material");
		}

		[ExpectNoExceptions]
		public void TestValidateJI_Compositions()
		{
			var messageError = "Specification: You have not entered a Specification.";
			invoiceLine.JI_Compositions = ZString.Empty;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Specification");
			invoiceLine.JI_Compositions = "Test Specification";
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Specification");
		}

		[ExpectNoExceptions]
		public void TestValidateStorageAndShippingConditionJobComInvLineRefsCollection()
		{
			var messageError = "Storage and Shipping Conditions: You have not entered at least one Storage and Shipping Conditions Code.";
			invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.RemoveAll();
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Storage and Shipping Conditions Code");
			var storageAndShippingCondition = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			storageAndShippingCondition.JG_ReferenceNumber = "1";
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Storage and Shipping Conditions Code");
		}

		[ExpectNoExceptions]
		public void TestValidateJI_NetWeight()
		{
			var messageError = "Net Weight: Please enter a 'Net Weight' greater than 0.";
			invoiceLine.JI_NetWeight = -1m;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Net Weight");
			invoiceLine.JI_NetWeight = 0m;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Net Weight");
			invoiceLine.JI_NetWeight = 1m;
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Net Weight");
		}

		[ExpectNoExceptions]
		public void TestValidateJI_CustomsThirdQuantity()
		{
			var messageError = "Licensing Quantity: Licensing Quantity cannot be zero.";
			invoiceLine.JI_CustomsThirdQuantity = ZDecimal.Zero;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Licensing Quantity");
			invoiceLine.JI_CustomsThirdQuantity = 1m;
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Licensing Quantity");
		}

		[ExpectNoExceptions]
		public void TestValidateJI_CustomsThirdUnitQty()
		{
			var messageError = "Licensing UQ: You have not entered a Licensing UQ.";
			invoiceLine.JI_CustomsThirdUnitQty = ZString.Empty;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Licensing UQ");
			invoiceLine.JI_CustomsThirdUnitQty = "ACR";
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Licensing UQ");
		}

		[ExpectNoExceptions]
		public void TestValidateForeignManufacturerCompanyName()
		{
			var messageError = "Foreign Manufacturer: You have not entered a Foreign Manufacturer Company Name.";
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			manufacturerDocAddress.E2_AddressOverride = true;
			manufacturerDocAddress.E2_CompanyName = ZString.Empty;
			NUnit.Framework.Assert.That(collector.Contains(messageError), NUnit.Framework.Is.True, "not entered a Foreign Manufacturer Company Name");
			manufacturerDocAddress.E2_CompanyName = "Test Company";
			NUnit.Framework.Assert.That(!collector.Contains(messageError), NUnit.Framework.Is.True, "entered a Foreign Manufacturer Company Name");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			sendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX601);
			collector = new NX601MessageSendingObjectAdditionalMessageErrorCollector(sendingObjectParent);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		LicensingMessageSendingObjectParent sendingObjectParent;
		NX601MessageSendingObjectAdditionalMessageErrorCollector collector;
	}
}
