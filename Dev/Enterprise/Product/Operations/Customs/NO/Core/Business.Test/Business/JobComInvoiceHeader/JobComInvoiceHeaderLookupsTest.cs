using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestJZ_IncoTerm_List_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var codeList = header.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertSame("Cached", header.Lookups.JZ_IncoTerm_List, codeList);
				AssertType<NOIncotermCodeList>("Type", codeList);
			});
		}

		public void TestJZ_IncoTerm_List_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var codeList = header.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertSame("Cached", header.Lookups.JZ_IncoTerm_List, codeList);
				AssertType<NOIncotermCodeList>("Type", codeList);
			});
		}

		public void TestValuationCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			string codeType = "TRNAT";
			helper.CreateCusCodeType(codeType, "TRNAT CODE");
			var insertedRow = helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var lookupRow = header.Lookups.ValuationCodeList[0] as CodeDescriptionPair;

			CombineAssertions(() =>
			{
				AssertNotNull(lookupRow);
				AssertEquals(1, header.Lookups.ValuationCodeList.Count);
				AssertEquals(insertedRow.ZZD_Code, lookupRow.Code);
				AssertEquals(insertedRow.ZZD_Description, lookupRow.Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			header = Factory.New<JobComInvoiceHeader>();
			declaration.Invoices.Add(header);
		}
		JobComInvoiceHeader header;
		JobDeclaration declaration;
	}
}
