using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ShippingIdentificationData))]
	sealed class ShippingIdentificationDataTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ShippingIdentificationData>
	{
		[ExpectNoExceptions]
		public void TestBizoCaptionsAndDescriptions()
		{
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(shippingIdentificationData.TW_ExpirationDateInfo, "Expiration Date", "The expiry date of the commodity.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(shippingIdentificationData.TW_ManufacturedDateInfo, "Manufactured Date", "The manufacture date of the commodity applying for inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(shippingIdentificationData.TW_ManufacturedLotNoInfo, "Lot Number", "The manufacturing lot number of the commodity applying for inspection.");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(shippingIdentificationData.TW_ProductLotNoAmountInfo, "Volume (LTR)", "The volume of the goods with a manufacturing lot number.");
			});
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			NUnit.Framework.Assert.That(shippingIdentificationData.Parent, NUnit.Framework.Is.SameAs(invoiceLine), "Parent");
		}

		[ExpectNoExceptions]
		public void TestGetNewValidation()
		{
			NUnit.Framework.Assert.That(shippingIdentificationData.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(ShippingIdentificationDataValidation)));
		}

		[ExpectNoExceptions]
		public void TestCommonRelatedColumnsReadOnly()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = jobDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var line = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.AssignCMHeaderToInvoices(controllingMessageHeader);
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX101", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX201_01", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX301", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_AX;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX301_AX", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX301_DN", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ProductLotNoAmountInfo ReadOnly");
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX401", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX601", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});

			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
			shippingIdentificationData = line.ShippingIdentificationDataCollection.AddNew();
			CombineAssertions("Linked NX603", () =>
			{
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedLotNoInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedLotNoInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ExpirationDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ExpirationDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ManufacturedDateInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), "TW_ManufacturedDateInfo ReadOnly");
				NUnit.Framework.Assert.That(shippingIdentificationData.TW_ProductLotNoAmountInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), "TW_ProductLotNoAmountInfo ReadOnly");
			});
		}

		#region Implementation
		protected override IEnumerable<ShippingIdentificationData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			yield return declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().ShippingIdentificationDataCollection.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			shippingIdentificationData = invoiceLine.ShippingIdentificationDataCollection.AddNew();
		}

		ShippingIdentificationData shippingIdentificationData;
		JobComInvoiceLine invoiceLine;
		#endregion
	}
}
