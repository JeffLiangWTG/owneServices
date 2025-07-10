using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingHouse))]
	sealed class PackingHouseTest : Customs.Business.Testing.CusCodeDataTest<PackingHouse>
	{
		[ExpectNoExceptions]
		public void TestCY_Code()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(packingHouse.CY_CodeInfo, "Code", "The code of packing house for plant products.");
		}

		[ExpectNoExceptions]
		public void TestCusCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "Taiwan Packing House");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			packingHouse.CY_Code = "XXXX0123";
			NUnit.Framework.Assert.That(packingHouse.CusCode, NUnit.Framework.Is.EqualTo(ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "XXXX0123", Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, ZDateTime.Now)));
		}
		[ExpectNoExceptions]
		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "Taiwan Packing House");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, "XXXX0123", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			packingHouse.CY_Code = "XXXX0123";
			var code = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "XXXX0123", Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanPackingHouse, ZDateTime.Now);
			NUnit.Framework.Assert.That(packingHouse.Description, NUnit.Framework.Is.EqualTo("XXXXXX").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(packingHouse.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.PackingHouses).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(packingHouse.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return packingHouse;
		}

		protected override IEnumerable<PackingHouse> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().PackingHouseCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return invoiceLine.PackingHouseCollection.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			packingHouse = invoiceLine.PackingHouseCollection.AddNew();
		}

		PackingHouse packingHouse;
		JobComInvoiceLine invoiceLine;
	}
}
