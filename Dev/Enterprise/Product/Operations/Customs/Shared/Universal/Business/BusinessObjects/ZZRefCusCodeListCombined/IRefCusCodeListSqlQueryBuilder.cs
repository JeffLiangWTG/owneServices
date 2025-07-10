using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public interface IRefCusCodeListSqlQueryBuilder
	{
		ZJoinQuery Build(ZQuery query);
	}
}
