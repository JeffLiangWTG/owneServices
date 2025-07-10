using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public interface IZZRefCusCodeListWrapperCollection
	{
		BusinessObject[] LoadCusCodeList(ZQuery query);
	}
}
