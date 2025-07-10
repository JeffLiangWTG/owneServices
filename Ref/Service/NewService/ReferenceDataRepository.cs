using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.NewService
{
	public class ReferenceDataRepository : IReferenceDataRepository, IReadOnlyReferenceDataRepository
	{
		public ReferenceDataRepository(string nameOrConnectionString)
		{
			this.nameOrConnectionString = nameOrConnectionString;
			CreateDbContext();
		}

		void CreateDbContext()
		{
			var dbContextOptionsBuilder = new DbContextOptionsBuilder<SafeDbContext>()
				.UseSqlServer(nameOrConnectionString, x => x.UseNetTopologySuite());
			entities = new SafeDbContext(dbContextOptionsBuilder.Options);
			if (int.TryParse(ApplicationConfig.CommandTimeout, out var cmdTimeout))
			{
				entities.Database.SetCommandTimeout(cmdTimeout);
			}
		}

#if DEBUG
		void CreateLogDbContext()
		{
			var dbContextOptionsBuilder = new DbContextOptionsBuilder<SafeDbContext>()
				.UseSqlServer(nameOrConnectionString, x => x.UseNetTopologySuite());
			dbContextOptionsBuilder.LogTo(sql => recentExecutedSqlQuery.AppendLine(sql), LogLevel.Information);
			entities = new SafeDbContext(dbContextOptionsBuilder.Options);
			if (int.TryParse(ApplicationConfig.CommandTimeout, out var cmdTimeout))
			{
				entities.Database.SetCommandTimeout(cmdTimeout);
			}
		}

		protected void EnableStatistics()
		{
			CreateLogDbContext();
		}

		public string GetRecentExecutedSqlQuery()
		{
			return recentExecutedSqlQuery.ToString();
		}

		readonly StringBuilder recentExecutedSqlQuery = new StringBuilder();
#endif

		SafeDbContext entities;
		readonly string nameOrConnectionString;

		public bool IsDbProvider
		{
			get { return true; }
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return entities.Set<T>()?.AsNoTracking();
		}

		public async Task<int> SaveChangesAsync()
		{
			return await entities.SaveChangesAsync();
		}

		public void Add<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			Entry(data).State = EntityState.Added;
		}

		public void Update<T>(T data) where T : class
		{
			Argument.NotNull(data, nameof(data));
			Entry(data).State = EntityState.Modified;
		}

		EntityEntry<T> Entry<T>(T data) where T : class
		{
			return entities.Entry(data);
		}

		public void Dispose()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			entities.Dispose();
		}
	}
}
