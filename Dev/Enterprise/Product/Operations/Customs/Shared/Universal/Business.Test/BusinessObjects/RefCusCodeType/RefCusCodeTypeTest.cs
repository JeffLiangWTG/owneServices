using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeType))]
	class RefCusCodeTypeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<RefCusCodeType>();
		}

		public void TestGetMaxLength()
		{
			Helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office", Core.Constants.CountryCodes.Eritrea, 8);
			Helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office", Core.Constants.CountryCodes.Afghanistan, 6);
			Factory.Save();
			AssertEquals("Get ZZK_Maxlength for AF", 6, RefCusCodeType.GetMaxLength(Factory, Core.Constants.CountryCodes.Afghanistan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, 5, null));
			AssertEquals("Get ZZK_Maxlength for AF", 6, RefCusCodeType.GetMaxLength(Factory, Core.Constants.CountryCodes.Afghanistan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, 5, ""));
			AssertEquals("Get the length passed in", 5, RefCusCodeType.GetMaxLength(Factory, Core.Constants.CountryCodes.Bahamas, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, 5, null));
			AssertEquals("Get default ZZK_Maxlength", 8, RefCusCodeType.GetMaxLength(Factory, Core.Constants.CountryCodes.Bahamas, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, 5, Core.Constants.CountryCodes.Eritrea));
		}

		public void TestZZK_Description()
		{
			Helper.CreateOrGetLanguage("FR", "French");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			Factory.Save();
			var refCusCodeType = GetNewBusinessObject() as RefCusCodeType;
			refCusCodeType.ZZK_CodeType = "ABC";
			refCusCodeType.ZZK_Description = "Description";
			refCusCodeType.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			var refCusCodeTypeLanguage = Factory.New<RefCusCodeTypeLanguage>();
			refCusCodeTypeLanguage.ZXI_ZZK_CodeType = refCusCodeType.PK;
			refCusCodeTypeLanguage.ZXI_ZX6_NKLanguage = "FR";
			refCusCodeTypeLanguage.ZXI_Description = "Décrire";
			Factory.Save();
			AssertEquals("ZZK_Description should be translated.", "Décrire", new BusinessObjectFactory().Load<RefCusCodeType>(refCusCodeType.PK).ZZK_Description);
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
