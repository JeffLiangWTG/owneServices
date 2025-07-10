using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS50Test : TestCaseWithFactory
	{
		public void TestParseForABI()
		{
			AssertEquals(0m, new ZString("00000").ParseForABI(5, 2));
			AssertEquals(0.01m, new ZString("00001").ParseForABI(5, 2));
			AssertEquals(2.01m, new ZString("00201").ParseForABI(5, 2));
			AssertEquals("00201", new ZDecimal(2.01m).ToStringForABI("", 5, 2));
			AssertEquals("00001", new ZDecimal(0.01m).ToStringForABI("", 5, 2));
			AssertEquals("00000", new ZDecimal(0.00m).ToStringForABI("XX", 5, 2));
			AssertEquals("**********", new ZDecimal(1234567890.00m).ToStringForABI("", 10, 2));
			AssertEquals("space when uq is empty and value is zero", "", new ZDecimal(0.00m).ToStringForABI("", 5, 2));
		}

		public void TestUpdateInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var notifications = new NotificationBuffer();
			var aens50 = new AENS50()
			{
				HTSNumber = "123456",
				Quantity1 = "10000",
				UnitOfMeasureCode1 = "KG",
				ValueOfGoodsAmount = 1000m
			};
			((IACEBIRDSecondaryLineRecord)aens50).Update(invoiceLine, false, notifications);
			AssertEquals("KG", invoiceLine.JI_CustomsUnitQty);
			AssertEquals(100m, invoiceLine.JI_CustomsQuantity);
			AssertEquals(1000m, invoiceLine.JI_LinePrice);
		}

		public void TestENS50ChargesZeroFills()
		{
			var aens50 = new AENS50()
			{
				HTSNumber = "123456",
				Quantity1 = "000000010000",
				Quantity2 = "",
				Quantity3 = "",
				UnitOfMeasureCode1 = "KG",
				ValueOfGoodsAmount = 1000m
			};

			AssertEquals("50123456     0000000000 0000001000 000000010000KG                               ", aens50.Serialise());
		}
	}
}
