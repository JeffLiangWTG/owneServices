using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	public static class TWRefCusMapperHelperForTest
	{
		public static void SetRefCusMapper(BusinessObjectFactory factory)
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(factory);
			universalReferenceTestDataHelper.CreateCusMapType(RefCusMapTypeList.Codes.TWI2C, "OUT", "Taiwan Invoice to Customs Invoice Unit Mapping", true);
			universalReferenceTestDataHelper.CreateCusMap(RefCusMapTypeList.Codes.TWI2C, "YDS", "YRD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Taiwan);
			universalReferenceTestDataHelper.CreateCusMap(RefCusMapTypeList.Codes.TWI2C, "PCS", "PCE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Taiwan);
			universalReferenceTestDataHelper.CreateCusMap(RefCusMapTypeList.Codes.TWI2C, "DOZ", "DZN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Taiwan);
			factory.Save();
		}
	}
}
