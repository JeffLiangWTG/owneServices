using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListAttributeName))]
	public class RefCusCodeListAttributeNameTesting : EnterpriseBusinessObjectTestCase
	{
		public void TestPropertiesTranslation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("FR", "French");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			Factory.Save();
			var refCusCodeListAttributeName = GetNewBusinessObject() as RefCusCodeListAttributeName;
			AssertEquals("System", refCusCodeListAttributeName.TranslatedName);
			AssertEquals("Description", refCusCodeListAttributeName.ZXE_Description);
			AssertEquals("System", refCusCodeListAttributeName.ZXE_ColumnCaption);
			var refCusCodeListAttributeNameLanguage = Factory.New<RefCusCodeListAttributeNameLanguage>();
			refCusCodeListAttributeNameLanguage.ZXH_ZXE_CodeListAttributeName = refCusCodeListAttributeName.PK;
			refCusCodeListAttributeNameLanguage.ZXH_ZX6_NKLanguage = "FR";
			refCusCodeListAttributeNameLanguage.ZXH_Description = "Décrire";
			refCusCodeListAttributeNameLanguage.ZXH_ColumnCaption = "Colonne";
			Factory.Save();
			AssertEquals("ZXE_Name should not be translated.", "System", new BusinessObjectFactory().Load<RefCusCodeListAttributeName>(refCusCodeListAttributeName.PK).TranslatedName);
			AssertEquals("ZXE_Description should be translated.", "Décrire", new BusinessObjectFactory().Load<RefCusCodeListAttributeName>(refCusCodeListAttributeName.PK).ZXE_Description);
			AssertEquals("ZXE_ColumnCaption should be translated.", "Colonne", new BusinessObjectFactory().Load<RefCusCodeListAttributeName>(refCusCodeListAttributeName.PK).ZXE_ColumnCaption);
			refCusCodeListAttributeNameLanguage.ZXH_Name = "Système";
			Factory.Save();
			AssertEquals("ZXE_Name should be translated.", "Système", new BusinessObjectFactory().Load<RefCusCodeListAttributeName>(refCusCodeListAttributeName.PK).TranslatedName);
		}

		#region Overrides of BusinessObjectBaseTestCase
		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			return helper.CreateNewOrGetExistingRefCusCodeListAttributeName("System", "Description", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			helper = new UniversalReferenceTestDataHelper(Factory);
			codeType = helper.CreateNewOrGetExistingCusCodeType("ABC", "abc", Core.Constants.CountryCodes.Australia);
			dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Australia, "Australia");
			Factory.Save();
			base.SetUp();
		}

		RefCusCodeType codeType;
		RefDataGrouping dataGrouping;
		UniversalReferenceTestDataHelper helper;
		#endregion
	}
}
