using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT.Testing
{
	class CusCarSealTest : TestCaseWithFactory
	{
		public void TestSealingParty_CountrySpecificMapping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, MapDirectionList.Codes.BTH, "Description", false);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, "CA", Core.Constants.CountryCodes.SouthAfrica);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Terminal, "TO", Core.Constants.CountryCodes.SouthAfrica);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Customs, "CU", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();
			ICusCarSeal zaSealTOR = new CusCarSeal(Factory, ZString.Empty, Core.Constants.ContainerSealParties.Codes.Terminal, ZString.Empty);
			AssertEquals("Returns the mapped ZA customs value", "TO", zaSealTOR.SealingParty);
			ICusCarSeal zaSealCAR = new CusCarSeal(Factory, ZString.Empty, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, ZString.Empty);
			AssertEquals("Returns the mapped ZA customs value", "CA", zaSealCAR.SealingParty);
			ICusCarSeal zaSealCUS = new CusCarSeal(Factory, ZString.Empty, Core.Constants.ContainerSealParties.Codes.Customs, ZString.Empty);
			AssertEquals("We should not default to the base mappings, as the country has it's own set of mappings, we only default to base if the country has no mappings.", ZString.Empty, zaSealCUS.SealingParty);
		}

		public void TestSealingParty_GenericMappping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, MapDirectionList.Codes.BTH, "Description", false);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, "CR", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();
			ICusCarSeal zaSealCAR = new CusCarSeal(Factory, ZString.Empty, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, ZString.Empty);
			AssertEquals("Returns the generic mapped customs value", "CR", zaSealCAR.SealingParty);
		}

		public void TestSealType_CountrySpecificMapping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.MechanicalSeal, "1", Core.Constants.CountryCodes.SouthAfrica);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.ElectronicSeal, "2", Core.Constants.CountryCodes.SouthAfrica);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.MSELT, "A", "3", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();
			ICusCarSeal zaSealM = new CusCarSeal(Factory, ZString.Empty, ZString.Empty, SealTypeList.Codes.MechanicalSeal);
			AssertEquals("Returns the mapped ZA customs value", "1", zaSealM.SealType);
			ICusCarSeal zaSealE = new CusCarSeal(Factory, ZString.Empty, ZString.Empty, SealTypeList.Codes.ElectronicSeal);
			AssertEquals("Returns the mapped ZA customs value", "2", zaSealE.SealType);
			ICusCarSeal zaSealA = new CusCarSeal(Factory, ZString.Empty, ZString.Empty, "A");
			AssertEquals("We should not default to the base mappings, as the country has it's own set of mappings, we only default to base if the country has no mappings.", ZString.Empty, zaSealA.SealType);
		}

		public void TestSealType_GenericMapping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			CreateRefCusMap(helper, RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.MechanicalSeal, "0", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();
			ICusCarSeal zaSealM = new CusCarSeal(Factory, ZString.Empty, ZString.Empty, SealTypeList.Codes.MechanicalSeal);
			AssertEquals("Returns the generic mapped customs value", "0", zaSealM.SealType);
		}

		RefCusMap CreateRefCusMap(UniversalReferenceTestDataHelper helper, string mapType, string cw1Value, string customsValue, string countryCode) => helper.CreateCusMap(mapType, cw1Value, customsValue, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, countryCode);
	}
}
