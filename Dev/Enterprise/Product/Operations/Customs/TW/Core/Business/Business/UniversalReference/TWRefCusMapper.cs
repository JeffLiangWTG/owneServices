using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.Business
{
	public static class TWRefCusMapper
	{
		public static ZString MapCW1UnitPriceUQToCustomsCode(BusinessObjectFactory factory, ZString code)
		{
			return code.IsEmpty ? ZString.Empty : MapCW1CodeToCustomsCode(factory, code, RefCusMapTypeList.Codes.TWI2C, ZDateTime.Today);
		}

		static ZString MapCW1CodeToCustomsCode(BusinessObjectFactory factory, ZString code, ZString type, ZDateTime dateTime)
		{
			return code.IsEmpty ? ZString.Empty : ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Taiwan, type, code, dateTime);
		}
	}
}
