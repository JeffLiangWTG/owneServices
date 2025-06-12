using System.Data.Entity;

namespace CargoWise.eHub.DataModel.Common
{
	public interface IDbContextInjectable
	{
		void SetDbContext(DbContext context);
	}
}
