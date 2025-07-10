using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace CargoWise.RefDbRepo.T4Runner.Generator
{
	public abstract class BasePropertyInfoMapper
	{
		PropertyInfo _propertyInfo;

		protected BasePropertyInfoMapper(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}

		public PropertyInfo PropertyInfo() => _propertyInfo;

		public string Name()
		{
			switch (_propertyInfo.Name)
			{
				case "RefUNLOCOs":
					return "RefUNLOCOes";
				case "RefCusProfileQuestionPathwayXQP_XQ2_QuestionParentNavigations":
					return "RefCusProfileQuestionPathways";
				case "RefCusProfileQuestionPathwayXQP_XQ2_QuestionChildNavigations":
					return "RefCusProfileQuestionPathways1";
				case "RefCusApplicabilityZZT_ZZA_TradeGroupNavigations":
					return "RefCusApplicabilities";
				case "RefCusApplicabilityZZT_ZZA_SecondTradeGroupNavigations":
					return "RefCusApplicabilities1";
				case "RefMessagingBussCarrierInfos":
					return "RefMessagingBussCarrierInfoes";
				case "RefCusTariffUOMZZ8_ZZA_TradeGroupNavigations":
					return "RefCusTariffUOMs";
				case "RefCusTariffUOMZZ8_ZZA_SecondTradeGroupNavigations":
					return "RefCusTariffUOMs1";
				default:
					return _propertyInfo.Name;
			}
		}

		public string PropertyName() => _propertyInfo.Name;

		public Type PropertyType() => _propertyInfo.PropertyType;

		public string PropertyTypeFullName()
		{
			if (_propertyInfo.PropertyType.IsGenericType &&
				_propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type underlyingType = Nullable.GetUnderlyingType(_propertyInfo.PropertyType);
				return $"Nullable<{underlyingType?.FullName}>";
			}
			return _propertyInfo.PropertyType.FullName;
		}

		public bool HasMaximumLength()
		{
			return _propertyInfo.GetCustomAttribute<StringLengthAttribute>() != null;
		}

		public int MaximumLength()
		{
			return _propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength ?? 0;
		}

		public string GetNavigationPropertyTypeName()
		{
			return PropertyType().GetGenericArguments().FirstOrDefault().Name;
		}

		public bool IsGuid()
		{
			return PropertyType() == typeof(Guid) || PropertyType() == typeof(Guid?);
		}

		public bool IsGuidOfOtherSystem()
		{
			return PropertyName() == "ZGF_QuickStartPK";
		}

		public bool IsString()
		{
			return PropertyTypeFullName() == "System.String";
		}

		public bool IsNotMapped()
		{
			return _propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null;
		}

		public bool IsSimpleType()
		{
			return IsSimpleType(PropertyType());
		}

		public bool IsNavigationProperty()
		{
			return IsNavigationProperty(PropertyType());
		}

		public static bool IsSimpleType(Type type)
		{
			return type.IsPrimitive ||
				   type == typeof(string) ||
				   type == typeof(decimal) ||
				   type == typeof(DateTime) ||
				   type == typeof(Guid) ||
				   type == typeof(byte[]) ||
				   type.FullName == "NetTopologySuite.Geometries.Geometry" ||
				   type.IsEnum ||
				   (Nullable.GetUnderlyingType(type) != null &&
					IsSimpleType(Nullable.GetUnderlyingType(type)));
		}

		public static bool IsComplexType(Type type)
		{
			return type.IsClass &&
				   type != typeof(string) &&
				   !type.IsValueType &&
				   !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>));
		}

		public static bool IsNavigationProperty(Type type)
		{
			return type.IsGenericType &&
				   type.GetGenericTypeDefinition() == typeof(ICollection<>) &&
				   IsComplexType(type.GetGenericArguments().FirstOrDefault());
		}

		public bool IsSystemProperty()
		{
			return SystemPropertySuffixes.Any(y => PropertyName().EndsWith(y, StringComparison.OrdinalIgnoreCase));
		}

		public static string[] SystemPropertySuffixes
		{
			get
			{
				return new[] { "DataSetCode", "SysStartTime", "SysEndTime" };
			}
		}
	}
}
