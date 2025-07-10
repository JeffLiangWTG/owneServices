using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class SupportingDocumentsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var supportingDocuments = bill.SupportingDocumentsForBill.AddNew();
			var statusList = supportingDocuments.Lookups.StatusList;
			var list = new CodeDescriptionPairList();
			list.AddPair(SupportingDocumentStatusList.Codes.EXS, SupportingDocumentStatusList.Descriptions.EXS);
			list.AddPair(SupportingDocumentStatusList.Codes.NOT, SupportingDocumentStatusList.Descriptions.NOT);
			AssertEquals(statusList, list);
		}

		public void TRSupportingDocumentsCodes()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRDOC", "TRDOC");

			helper.CreateCusCodeList("TR", "TRDOC", "0100", "Fatura", yesterday, tomorrow);
			helper.CreateCusCodeList("TR", "TRDOC", "0101", "Navlun Makbuzu", yesterday, tomorrow);

			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<SupportingDocuments>();
				var list = header.Lookups.CodeList;
				((BusinessObjectCollection)list).Load();
				AssertEquals(2, list.Count);
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "0100"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "0101"));
			}
		}
	}
}
