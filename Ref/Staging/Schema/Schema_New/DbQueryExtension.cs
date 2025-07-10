using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public static class DbQueryExtension
	{
		public static IQueryable<T> NewInclude<T>(this IQueryable<T> query, string path) where T : class
		{
			Argument.NotNull(query, nameof(query));
			if (typeof(IQueryable<T>).IsAssignableFrom(query.GetType()))
			{
				return query.Include(path);
			}
			return query;
		}
	}
}
