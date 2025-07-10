using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class ExportUnionDeclarationProviderTest : TestCaseWithFactory
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "21340300IM123456";
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_CL = cusEntryLine.PK;

#if NETFRAMEWORK
			var expectedMessage = "Value cannot be null.\r\nParameter name: jobDeclaration";
#else
			var expectedMessage = "Value cannot be null. (Parameter 'jobDeclaration')";
#endif
			AssertExceptionThrown<ArgumentNullException>("Null ExportUnionCompaniesProvider", expectedMessage,
			() => new ExportUnionDeclarationProvider(null, null, null));
			AssertNoExceptionThrown("All ok", () => new ExportUnionDeclarationProvider(jobDeclaration, invoiceLine, null));
		});

		public void TestExportUnionDeclarationMembers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				headerJobDeclaration.ZG_TradeType = TradeTypeList.Codes.ETD;
				var entryInstruction = headerJobDeclaration.CusEntryInstruction;
				entryInstruction.ZG_ExportUnionSecretaryCode = "1";
				entryInstruction.ZG_ExportUnionCode = "2";
				entryInstruction.ZG_ExportUnionCountryCode = "052";
				entryInstruction.ZG_InlandTransportType = "10";
				headerJobDeclaration.CusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
				headerJobDeclaration.CusEntryHeader.EntryInstruction.CEI_DateForDuty = new ZDateTime(2020, 7, 21);

				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				var exportUnionDeclaration = declaration.ExportUnionDeclaration;
				CombineAssertions("Export Union Declaration Members | Case 1", () =>
				{
					AssertEquals("SoftwareHouseCode", "SKDWL20AO1", exportUnionDeclaration.SoftwareHouseCode);
					AssertEquals("DeclarationDate", ZDateTime.Now.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear), exportUnionDeclaration.DeclarationDate);
					AssertEquals("UnionSecretaryCode", "1", exportUnionDeclaration.UnionSecretaryCode);
					AssertEquals("UnionCode", "2", exportUnionDeclaration.UnionCode);
					AssertEquals("CustomsRegistrationCode", ZString.Empty, exportUnionDeclaration.CustomsRegistrationCode);
					AssertEquals("CustomsRegistrationDate", ZString.Empty, exportUnionDeclaration.CustomsRegistrationDate);
					AssertEquals("OrderKindCode", "3", exportUnionDeclaration.OrderKindCode);
					AssertEquals("ECommerce", "1", exportUnionDeclaration.ECommerce);
					AssertEquals("DestinationTransitCountryCode", "052", exportUnionDeclaration.DestinationTransitCountryCode);
					AssertEquals("Container", "1", exportUnionDeclaration.Container);
					AssertEquals("DeliveryLocation", "IST", exportUnionDeclaration.DeliveryLocation);
					AssertEquals("DeclarationType1", "EX", exportUnionDeclaration.DeclarationType1);
					AssertEquals("DeclarationType2", "1", exportUnionDeclaration.DeclarationType2);
					AssertEquals("OriginCountryCode", "003", exportUnionDeclaration.OriginCountryCode);
					AssertEquals("CurrencyRate", 20m, exportUnionDeclaration.CurrencyRate);
					AssertEquals("AgreementType", "11", exportUnionDeclaration.AgreementType);
					AssertEquals("TotalDomesticExpenditureCurrency", "TRY", exportUnionDeclaration.TotalDomesticExpenditureCurrency);
					AssertEquals("InternalTransportType", "10", exportUnionDeclaration.InternalTransportType);
					AssertEquals("PaymentType", "XX", exportUnionDeclaration.PaymentType);
					AssertEquals("PaymentPassword", ZString.Empty, exportUnionDeclaration.PaymentPassword);
					AssertEquals("Companies", 5, exportUnionDeclaration.Companies.Count);
				});

				headerJobDeclaration = helper.GetProviderHeader();
				headerJobDeclaration.ZG_TradeType = TradeTypeList.Codes.ET;
				headerJobDeclaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
				entryInstruction = headerJobDeclaration.CusEntryInstruction;

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				exportUnionDeclaration = declaration.ExportUnionDeclaration;
				CombineAssertions("Export Union Declaration Members | Case 2", () =>
				{
					AssertEquals("ECommerce", "2", exportUnionDeclaration.ECommerce);
					AssertEquals("Container", "0", exportUnionDeclaration.Container);
				});

				headerJobDeclaration.ZG_TradeType = ZString.Empty;
				headerJobDeclaration.JE_ContainerMode = ZString.Empty;

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
				exportUnionDeclaration = declaration.ExportUnionDeclaration;
				CombineAssertions("Export Union Declaration Members | Case 3", () =>
				{
					AssertEquals("ECommerce", "2", exportUnionDeclaration.ECommerce);
					AssertEquals("Container", "0", exportUnionDeclaration.Container);
				});
			}
		}
	}
}
