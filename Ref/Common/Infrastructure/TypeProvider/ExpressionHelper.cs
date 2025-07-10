using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Common.TypeProvider
{
	public static class ExpressionHelper
	{
		public static Expression<Func<T, Guid>> GetPKExpression<T>()
		{
			var pkColumn = typeof(T).GetPKPropertyInfo();
			return GetPropertyExpression<T, Guid>(pkColumn);
		}

		public static Expression<Func<T, bool>> GetPropertyFiltersExpression<T>(PropertyInfo property, object[] values, Operations operation, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase, ParameterExpression paramExpression = null)
		{
			Argument.Argument.NotNull(values, nameof(values));
			Argument.Argument.NotNull(property, nameof(property));

			var param = paramExpression ?? Expression.Parameter(typeof(T));
			Expression result = null;
			foreach (var value in values)
			{
				var valueExp = GetPropertyFilterExpression<T>(param, property, value, operation, comparisonType);
				if (result != null)
				{
					result = Expression.Or(result, valueExp);
				}
				else
				{
					result = valueExp;
				}
			}
			return Expression.Lambda<Func<T, bool>>(result, param);
		}


		/// <summary>
		/// Should be used when IQueryable is of type DbSet<T>, by default it is case-insentive.
		/// </summary>
		public static Expression<Func<T, bool>> GetPropertyFiltersExpressionEqualsUsingLikeDbFunction<T>(PropertyInfo property, object value)
		{
			var param = Expression.Parameter(typeof(T));
			var propertyExp = Expression.Property(param, property);
			Expression expression;

			if (property.PropertyType == typeof(string))
			{
				var likeMethod = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), new Type[] { typeof(DbFunctions), typeof(string), typeof(string) });
				var pattern = value.ToString();

				expression = Expression.Equal(
								Expression.Call(
								null,
								likeMethod,
								Expression.Constant(EF.Functions),
								propertyExp,
								Expression.Constant(pattern)),
								Expression.Constant(true));
			}
			else
			{
				expression = Expression.Equal(propertyExp, Expression.Constant(value, property.PropertyType));
			}
			return Expression.Lambda<Func<T, bool>>(expression, param);
		}
		
		public static Expression<Func<T, bool>> GetRelatedEntityPropertyFilterExpression<T>(Type relatedEntityType, PropertyInfo relatedEntityProperty, object value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			Argument.Argument.NotNull(relatedEntityType, nameof(relatedEntityType));
			Argument.Argument.NotNull(relatedEntityProperty, nameof(relatedEntityProperty));

			var param = Expression.Parameter(typeof(T));
			var relatedProperty = typeof(T).GetProperties().FirstOrDefault(x => x.PropertyType == relatedEntityType);
			var relatedPropertyExp = Expression.Property(param, relatedProperty);
			var relatedEntityPropertyExp = Expression.Property(relatedPropertyExp, relatedEntityProperty);
			var equalExp = GetFilterExpressionEquals(relatedEntityPropertyExp, relatedEntityProperty, value, comparisonType);
			return Expression.Lambda<Func<T, bool>>(equalExp, param);
		}

		public static Expression<Func<T, bool>> GetRelatedRelatedEntityPropertyFilterExpression<T>(Type relatedEntityType, Type relatedRelatedEntityType, PropertyInfo relatedRelatedEntityProperty, object value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			Argument.Argument.NotNull(relatedRelatedEntityType, nameof(relatedRelatedEntityType));
			Argument.Argument.NotNull(relatedRelatedEntityProperty, nameof(relatedRelatedEntityProperty));

			var param = Expression.Parameter(typeof(T));
			var relatedProperty = typeof(T).GetProperties().FirstOrDefault(x => x.PropertyType == relatedEntityType);
			var relatedRelatedProperty = relatedProperty.PropertyType.GetProperties().FirstOrDefault(x => x.PropertyType == relatedRelatedEntityType);
			var relatedPropertyExp = Expression.Property(param, relatedProperty);
			var relatedEntityPropertyExp = Expression.Property(relatedPropertyExp, relatedRelatedProperty);
			var relatedRelatedEntityPropertyExp = Expression.Property(relatedEntityPropertyExp, relatedRelatedEntityProperty);
			var equalExp = GetFilterExpressionEquals(relatedRelatedEntityPropertyExp, relatedRelatedEntityProperty, value, comparisonType);

			return Expression.Lambda<Func<T, bool>>(equalExp, param);
		}

		public static BinaryExpression GetPropertyFilterExpression<T>(ParameterExpression param, PropertyInfo property, object value, Operations operation, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			Argument.Argument.NotNull(property, nameof(property));
			var propertyExp = Expression.Property(param, property);

			switch (operation)
			{
				case Operations.StartsWith:
					var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string), typeof(StringComparison) });
					return Expression.Equal(
						Expression.Call(
							propertyExp,
							startsWithMethod,
							new Expression[] { Expression.Constant(value), Expression.Constant(comparisonType) }),
						Expression.Constant(true));
				case Operations.LessThan:
					return Expression.LessThan(propertyExp, Expression.Constant(value, property.PropertyType));
				case Operations.GreaterThan:
					return Expression.GreaterThan(propertyExp, Expression.Constant(value, property.PropertyType));
				case Operations.GreaterThanOrEqual:
					return Expression.GreaterThanOrEqual(propertyExp, Expression.Constant(value, property.PropertyType));
				case Operations.LessThanOrEqual:
					return Expression.LessThanOrEqual(propertyExp, Expression.Constant(value, property.PropertyType));
				default:
					return GetFilterExpressionEquals(propertyExp, property, value, comparisonType);
			}
		}

		static BinaryExpression GetFilterExpressionEquals(Expression propertyExp, PropertyInfo property, object value, StringComparison comparisonType)
		{
			if (property.PropertyType == typeof(string))
			{
				var stringEqualsMethod = typeof(string).GetMethod(nameof(string.Equals), new Type[] { typeof(string), typeof(StringComparison) });
				return Expression.Equal(
					Expression.Call(
						propertyExp,
						stringEqualsMethod,
						new Expression[] { Expression.Constant(value), Expression.Constant(comparisonType) }),
					Expression.Constant(true));
			}
			return Expression.Equal(propertyExp, Expression.Constant(value, property.PropertyType));
		}

		public static Expression<Func<T, TProperty>> GetPropertyExpression<T, TProperty>(PropertyInfo propertyInfo)
		{
			Argument.Argument.NotNull(propertyInfo, nameof(propertyInfo));

			var param = Expression.Parameter(typeof(T));
			var propertyExp = Expression.Property(param, propertyInfo);
			return Expression.Lambda<Func<T, TProperty>>(propertyExp, param);
		}

		public static Expression<Func<T, TProperty>> GetStructPropertyExpression<T, TProperty>(PropertyInfo propertyInfo) where TProperty : struct
		{
			Argument.Argument.NotNull(propertyInfo, nameof(propertyInfo));
			var param = Expression.Parameter(typeof(T));
			return GetStructPropertyExpression<T, TProperty>(param, propertyInfo);
		}

		public static Expression<Func<T, TProperty>> GetStructPropertyExpression<T, TProperty>(ParameterExpression param, PropertyInfo propertyInfo) where TProperty : struct
		{
			Argument.Argument.NotNull(propertyInfo, nameof(propertyInfo));
			var propertyExp = Expression.Property(param, propertyInfo);
			if (propertyInfo.PropertyType == typeof(Nullable<TProperty>))
			{
				var valueProperty = typeof(Nullable<TProperty>).GetProperty(nameof(Nullable<TProperty>.Value));
				propertyExp = Expression.Property(propertyExp, valueProperty);
			}
			return Expression.Lambda<Func<T, TProperty>>(propertyExp, param);
		}

		public static Expression<Func<T, bool>> ContainsStructPropertyExpression<T, TProperty>(List<TProperty> values, PropertyInfo propertyInfo) where TProperty : struct
		{
			Argument.Argument.NotNull(values, nameof(values));
			Argument.Argument.NotNull(propertyInfo, nameof(propertyInfo));

			var param = Expression.Parameter(typeof(T));
			var valuesExp = Expression.Constant(values, typeof(List<TProperty>));
			var propertyExp = ExpressionHelper.GetStructPropertyExpression<T, TProperty>(param, propertyInfo);
			var containsMethod = typeof(List<TProperty>).GetMethod(nameof(List<TProperty>.Contains));
			var containsExp = Expression.Call(valuesExp, containsMethod, propertyExp.Body);
			return Expression.Lambda<Func<T, bool>>(containsExp, param);
		}
	}
}
