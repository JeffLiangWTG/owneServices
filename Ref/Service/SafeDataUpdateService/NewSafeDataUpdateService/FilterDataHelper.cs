using CargoWise.RefDbRepo.Common.TypeProvider;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class FilterDataHelper
	{
		public static IQueryable<TChild> FilterWithParentData<TChild, TParent>(IQueryable<TChild> childrenData, IQueryable<TParent> parentData)
		{
			var pkProperty = typeof(TParent).GetPKPropertyInfo();
			var fkProperty = typeof(TChild).GetFKPropertyInfo(typeof(TParent));

			var pkSelector = ExpressionHelper.GetPropertyExpression<TParent, Guid>(pkProperty);
			if (fkProperty.PropertyType == typeof(Guid?))
			{
				var nullableFkSelector = ExpressionHelper.GetPropertyExpression<TChild, Guid?>(fkProperty);
				var nullablePkSelector = Expression.Lambda<Func<TParent, Guid?>>(
					Expression.Convert(pkSelector.Body, typeof(Guid?)),
					pkSelector.Parameters);
				return childrenData.Join(parentData, nullableFkSelector, nullablePkSelector, (c, p) => c);
			}
			var fkSelector = ExpressionHelper.GetPropertyExpression<TChild, Guid>(fkProperty);
			return childrenData.Join(parentData, fkSelector, pkSelector, (c, p) => c);
		}
	}
}
