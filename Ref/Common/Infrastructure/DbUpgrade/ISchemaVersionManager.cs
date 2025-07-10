using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public interface ISchemaVersionManager
	{
		int GetVersion(IDbTransaction transaction);
		void UpdateVersion(int version, IDbTransaction transaction);
	}
}
