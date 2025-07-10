using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.Testing
{
	public class AsycudaUniversalReferenceTests : TestCaseWithFactory
	{
		public void TestShouldAddEntryDocsToEDocs()
		{
			Assert("ShouldAddEntryDocsToEDocs", !AsycudaUniversalReference.CustomsStatusAttributeHelper.ShouldAddEntryDocsToEDocs(Factory, "XXX", "ZA", ZDateTime.Now));
			var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
			attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IAddEntryDocsToEDocs;
			Factory.Save();
			cusCodeList.Attributes.Reload(true);
			Assert("ShouldAddEntryDocsToEDocs", AsycudaUniversalReference.CustomsStatusAttributeHelper.ShouldAddEntryDocsToEDocs(Factory, "XXX", "ZA", ZDateTime.Now));
		}

		public void TestGetCachedUntranslatableList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			var record = Factory.New<ZZRefCusCodeListCombined>();
			record.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes;
			record.ZZD_Code = "XXX";
			record.ZZD_Description = "XXX Desc";
			record.ZZD_CountryOrGrouping = "AU";
			record.ZZD_StartDate = ZDateTime.Today.AddMonths(-1);
			record.ZZD_EndDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();

			var translatableList = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, "AU",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);

			AssertType<CodeDescriptionPairList>(translatableList);
			AssertEquals(1, translatableList.Count);

			var untranslatableList = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedUntranslatableList(Factory, "AU",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes);

			AssertType<UntranslatableCodeDescriptionPairList>(untranslatableList);
			AssertEquals(1, untranslatableList.Count);
			AssertEquals(translatableList, untranslatableList);
		}

		ZZRefCusCodeListCombined cusCodeList;
		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsManifestStatus");
			cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus;
			cusCodeList.ZZD_Code = "XXX";
			cusCodeList.ZZD_Description = "XXX Desc";
			cusCodeList.ZZD_CountryOrGrouping = "ZA";
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddMonths(-1);
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddMonths(1);
			Factory.Save();
		}
	}
}
