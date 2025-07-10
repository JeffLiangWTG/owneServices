using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGoodsCatalog))]
	public class BaseCusGoodsCatalogTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GoodsCatalog;

		public void TestIsAutoLogged()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Logs", 0, GoodsCatalog.Logs.GetAllLogs().Count);
				Factory.Save();
				AssertEquals("Save Log", 1, GoodsCatalog.Logs.GetAllLogs().Count);
			});
		}

		public void TestConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "TEST CONSIGNEE";

			var cusGoodsCatalog = Factory.New<BaseCusGoodsCatalog>();
			AssertEquals("Consignee should be emty", ZGuid.Empty, cusGoodsCatalog.CGC_OH_Owner);

			cusGoodsCatalog.CGC_OH_Owner = consignee.PK;
			AssertEquals("Consignee name should be", "TEST CONSIGNEE", cusGoodsCatalog.Owner.OH_FullName);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GoodsCatalog.CGC_GC_Company);
			AssertEquals("CGC_CustomsStatus", DefaultCustomsStatus, Factory.NewWithValidTestData<BaseCusGoodsCatalog>().CGC_CustomsStatus);
		}

		public void TestITemplateCopyable()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "TEST CONSIGNEE";

			GoodsCatalog.CGC_CatalogCode = "TEST";
			GoodsCatalog.CGC_Description = "Test Description";
			GoodsCatalog.CGC_OH_Owner = consignee.PK;
			GoodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			GoodsCatalog.CGC_Tariff = "80020000";
			GoodsCatalog.CGC_AuthorityIdentifier = "123";
			GoodsCatalog.CGC_AuthorityVersion = "1";
			GoodsCatalog.CGC_AuthorityStatus = "2";
			GoodsCatalog.CGC_MessageStatus = "ACC";
			GoodsCatalog.CGC_CustomsStatus = "ACC";

			ITemplateCopyable template = GoodsCatalog;

			var copiedGoodsCatalog = (BaseCusGoodsCatalog)template.TemplateCopy();
			CombineAssertions("Copied Goods Catalog", () =>
			{
				AssertEquals("CGC_CatalogCode", "", copiedGoodsCatalog.CGC_CatalogCode);
				AssertEquals("CGC_Description", "Test Description", copiedGoodsCatalog.CGC_Description);
				AssertEquals("CGC_OH_Owner", consignee.PK, copiedGoodsCatalog.CGC_OH_Owner);
				AssertEquals("CGC_Type", GoodsCatalogTypeList.Codes.Import, copiedGoodsCatalog.CGC_Type);
				AssertEquals("CGC_Tariff", "80020000", copiedGoodsCatalog.CGC_Tariff);
				AssertEquals("CGC_AuthorityIdentifier", ZString.Empty, copiedGoodsCatalog.CGC_AuthorityIdentifier);
				AssertEquals("CGC_AuthorityVersion", ZString.Empty, copiedGoodsCatalog.CGC_AuthorityVersion);
				AssertEquals("CGC_AuthorityStatus", ZString.Empty, copiedGoodsCatalog.CGC_AuthorityStatus);
				AssertEquals("CGC_MessageStatus", ZString.Empty, copiedGoodsCatalog.CGC_MessageStatus);
				AssertEquals("CGC_CustomsStatus", DefaultCustomsStatus, copiedGoodsCatalog.CGC_CustomsStatus);
			});
		}

		protected virtual ZString DefaultCustomsStatus => ZString.Empty;

		public void TestSupportsClone()
		{
			Assert("GoodsCatalog SupportsClone should be TRUE", GoodsCatalog.SupportsClone());
		}

		public void TestCGC_MessageStatusDescription()
		{
			AssertEquals("Not Sent", Factory.New<BaseCusGoodsCatalog>().CGC_MessageStatusDescription);
		}

		public virtual void TestFormatTariffForSaving()
		{
			GoodsCatalog.CGC_Tariff = "awd/-123456780";
			AssertEquals("CGC_Tariff", "awd/-123456780", GoodsCatalog.CGC_Tariff);
		}

		public void TestCountryCodeAndCustomsCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("CountryCode", Core.Constants.CountryCodes.PuertoRico, GoodsCatalog.CountryCode);
				AssertEquals("CustomsCountryCode", Core.Constants.CountryCodes.UnitedStates, GoodsCatalog.CustomsCountryCode);
			}
		}

		public void TestUniversalTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(GoodsCatalog.CustomsCountryCode, GoodsCatalog.UniversalTariffType);

			Factory.Save();
			var tariff = helper.CreateTariff(GoodsCatalog.CustomsCountryCode, tariffType.PK, "08091998", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			GoodsCatalog.CGC_Tariff = "08091998";
			AssertEquals(tariff, GoodsCatalog.UniversalTariff);

			GoodsCatalog.CGC_Tariff = "123";
			AssertEquals(null, GoodsCatalog.UniversalTariff);
		}

		public void TestDelectCusGoodsCatalogProductionInfosWhenDeletingBusinessObject()
		{
			var cusGoodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();

			var manufecturer = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			manufecturer.CGI_Reference = Core.Constants.CountryCodes.Brazil;
			manufecturer.CGI_CGC_Catalog = cusGoodsCatalog.PK;

			var manufecturer2 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			manufecturer2.CGI_Reference = Core.Constants.CountryCodes.Estonia;
			manufecturer2.CGI_CGC_Catalog = cusGoodsCatalog.PK;

			var manufecturer3 = Factory.New<BaseCusGoodsCatalogProductionInfo>();
			manufecturer3.CGI_Reference = Core.Constants.CountryCodes.Ukraine;
			manufecturer3.CGI_CGC_Catalog = cusGoodsCatalog.PK;

			cusGoodsCatalog.Delete();
			CombineAssertions(() =>
			{
				Assert("CusGoodsCatalogProductionInfo 1", manufecturer.IsDeleted);
				Assert("CusGoodsCatalogProductionInfo 2", manufecturer2.IsDeleted);
				Assert("CusGoodsCatalogProductionInfo 3", manufecturer3.IsDeleted);
			});
		}

		public void TestDocManagerInfo()
		{
			AssertEquals(Enterprise.Core.Constants.DocManagerCodes.CusGoodsCatalog, GoodsCatalog.DocManagerInfo.DocManagerCode);
		}

		public virtual void TestGetProductionInfoType()
		{
			AssertEquals(typeof(BaseCusGoodsCatalogProductionInfo), GoodsCatalog.GetProductionInfoType("LPN"));
			AssertEquals(typeof(BaseCusGoodsCatalogProductionInfo), GoodsCatalog.GetProductionInfoType("FOR"));
		}

		BaseCusGoodsCatalog GoodsCatalog => goodsCatalog ??= Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
		BaseCusGoodsCatalog goodsCatalog;
	}
}
