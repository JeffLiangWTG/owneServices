using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(ExportLicensingMessageGoodsShipment))]
	abstract class ExportLicensingMessageGoodsShipmentTest<T> : TestCaseWithFactory where T : IGoodsShipment
	{
		[ExpectNoExceptions]
		public void TestExitDateTime()
		{
			var (goodsShipment, _, declaration) = SetupData();
			declaration.JE_DateAtOrigin = new ZDateTime(2020, 1, 1);
			NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[ExpectNoExceptions]
		public void TestConsignmentType()
		{
			var (goodsShipment, _, _) = SetupData();
			NUnit.Framework.Assert.That(goodsShipment.Consignment, NUnit.Framework.Is.TypeOf<ExportLicensingMessageGoodsShipmentConsignment>());
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItemType()
		{
			var (goodsShipment, header, declaration) = SetupData();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			NUnit.Framework.Assert.That(goodsShipment.GovernmentAgencyGoodsItems.First(), NUnit.Framework.Is.TypeOf(ExpectedGovernmentAgencyGoodsItemsType));
		}

		protected virtual Type ExpectedGovernmentAgencyGoodsItemsType => typeof(ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem);

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public void TestItemChargeAmount()
		{
			var exportEntryHeader = CusEntryHeaderTest.GetExportCostAndInsuranceEntryHeaderTestCase(Factory);
			var exportDeclaration = exportEntryHeader.Declaration;
			var exportHeader = exportDeclaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var goodsShipment = GetLicensingMessageGoodsShipment(exportHeader);
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(474523m).Using(CustomComparers.TypeComparison), "ItemChargeAmount");
		}

		[ExpectNoExceptions]
		public void TestSeller()
		{
			var (goodsShipment, _, _) = SetupData();
			NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)), "Seller must be null - should be [null]");
		}

		protected abstract T GetLicensingMessageGoodsShipment(CusTWControllingMessageHeader header);

		protected (T exportLicensingMessageGoodsShipment, CusTWControllingMessageHeader header, JobDeclaration declaration) SetupData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return (GetLicensingMessageGoodsShipment(header), header, declaration);
		}
	}

	[TestedType(typeof(ExportLicensingMessageGoodsShipment))]
	sealed class ExportLicensingMessageGoodsShipmentBaseOnlyTest : ExportLicensingMessageGoodsShipmentTest<ExportLicensingMessageGoodsShipment>
	{
		protected override ExportLicensingMessageGoodsShipment GetLicensingMessageGoodsShipment(CusTWControllingMessageHeader header)
		{
			return new ExportLicensingMessageGoodsShipment(header);
		}
	}
}
