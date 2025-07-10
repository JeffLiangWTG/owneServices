using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CustomsOfficesCodeListProvider : Integration.Customs.ZA.ICustomsOfficesProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription item in ZARefCusCodeListTypes.GetCustomsOfficeList(Factory))
			{
				result.AddPair(item.Code, ZString.Format("{0} - {1}", item.Code, item.Description));
			}
			return result;
		}
	}
}
