using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class TradePartyValueProviderTest : TestCaseWithFactory
	{
		public void TestDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "ABC Man";
			var manAddress = manufacturer.Addresses.AddNew();
			manAddress.CustomsCodes.AddNew("MID", "AUJEP789324");
			manAddress.OA_Address1 = "MN Address1";
			manAddress.OA_City = "MN City";

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_FullName = "Buy All Co.";
			ultimateConsignee.MainAddress.OA_Address1 = "UC Address1";
			ultimateConsignee.MainAddress.OA_City = "UC City";
			ultimateConsignee.CustomsCodes.AddNew("EIN", "34-342892300");

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.OH_FullName = "Sell All Co.";
			foreignExporter.MainAddress.OA_Address1 = "FE Address1";
			foreignExporter.MainAddress.OA_City = "FE City";
			foreignExporter.MainAddress.CustomsCodes.AddNew("MID", "NZYU789432");

			invoiceLine.JI_OA_ManufacturerAddress = manAddress.PK;
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine.JI_OA_ExporterAddress = foreignExporter.MainAddress.PK;

			var tradeParties = new TradePartyValueProvider(invoiceLine).TradeParties;
			AssertEquals(3, tradeParties.Count());

			var tradeParty1 = tradeParties.ElementAt(0);
			AssertEquals("AUJEP789324", tradeParty1.ID);
			AssertEquals("ABC Man", tradeParty1.Name);
			AssertEquals(DISTradePartyType.Manufacturer, tradeParty1.Type);
			AssertEquals("MN ADDRESS1 MN CITY", tradeParty1.Address);

			var tradeParty2 = tradeParties.ElementAt(2);
			AssertEquals("34-342892300", tradeParty2.ID);
			AssertEquals("Buy All Co.", tradeParty2.Name);
			AssertEquals(DISTradePartyType.Consignee, tradeParty2.Type);
			AssertEquals("UC ADDRESS1 UC CITY", tradeParty2.Address);

			var tradeParty3 = tradeParties.ElementAt(1);
			AssertEquals("NZYU789432", tradeParty3.ID);
			AssertEquals("Sell All Co.", tradeParty3.Name);
			AssertEquals(DISTradePartyType.Exporter, tradeParty3.Type);
			AssertEquals("FE ADDRESS1 FE CITY", tradeParty3.Address);
		}
	}
}
