using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public static class RefDbVersionControlHelper
	{
		public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> source, IReadOnlyReferenceDataRepository refDbRepo)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(refDbRepo, nameof(refDbRepo));

			return source.Join(refDbRepo.Get<RefDbVersionControl>().Where(x => !x.RVC_Deleted && x.RVC_IsPublished), PKExpression<T>(), x => x.RVC_ParentPK, (x, y) => x);
		}

		static Expression<Func<T, Guid>> PKExpression<T>()
		{
			var pkProperty = typeof(T).GetProperties().Select(x => x.Name).FirstOrDefault(x => x.EndsWith("_PK", StringComparison.InvariantCultureIgnoreCase));
			var param = Expression.Parameter(typeof(T));
			var propertyExpression = Expression.Property(param, pkProperty);
			return Expression.Lambda<Func<T, Guid>>(propertyExpression, param);
		}
	}
}
