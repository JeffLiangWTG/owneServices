using System;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class PropertyMapping<T>
		where T : RefDataRepoModelEntityType, new()
	{
		public PropertyMapping(Expression<Func<T, object>> propertyExpression, int startPosition, object defaultValue = null) : this(propertyExpression, startPosition, -1, defaultValue)
		{
		}

		public PropertyMapping(Expression<Func<T, object>> propertyExpression, int startPosition, int length, object defaultValue = null)
		{
			PropertyInfo = GetPropertyInfo(propertyExpression.Body);
			StartPosition = startPosition;
			Length = length;
			DefaultValue = defaultValue;
		}

		public PropertyInfo PropertyInfo { get; }

		public int StartPosition { get; }

		public int Length { get; }

		public object DefaultValue { get; }

		static PropertyInfo GetPropertyInfo(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.MemberAccess:
					return (PropertyInfo)((MemberExpression)expression).Member;
				case ExpressionType.Convert:
					return GetPropertyInfo(((UnaryExpression)expression).Operand);
				default:
					throw new NotSupportedException(expression.NodeType.ToString());
			}
		}
	}
}
