using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeType.Loader))]
	class RefCusCodeTypeLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusCodeType.Loader(Factory);
		}

		public void TestLoad()
		{
			Helper.DeleteAllCusCodeTypes();

			var codeType1 = Helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office", Core.Constants.CountryCodes.Eritrea, 8);
			var codeType2 = Helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office", Core.Constants.CountryCodes.Afghanistan, 6);
			Factory.Save();
			var loader = new RefCusCodeType.Loader(Factory);
			AssertEquals(codeType1, loader.Load(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
			AssertEquals(codeType2, loader.Load(Core.Constants.CountryCodes.Afghanistan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
			AssertEquals(null, loader.Load("", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
			AssertEquals(null, loader.Load(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice));
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
