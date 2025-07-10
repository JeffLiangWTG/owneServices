using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListProvider : Integration.Customs.Shared.Universal.IRefCusCodeListProvider
	{
		public ZString[] GetAttributeValues(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZString code, ZString attributeName)
		{
			return RefCusCodeListAttributeTypes.GetAttributeValuesFor(factory, dataGroupingCode, codeType, date, code, attributeName);
		}
	}
}
