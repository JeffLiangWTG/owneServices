using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.RefDbRepo.Common.TypeProvider;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace CargoWise.RefDbRepo.Common.Web.Auth
{
	public abstract class AuthorizationHelper : IAuthorizationHelper
	{
		public bool IsAuthorized<T>(T data, string user)
		{
			return CheckAuthorizedWithGenericType<T>(user)(data);
		}

		public bool IsAuthorized<T>(string user)
		{
			var usrAuths = GetAuthorizationData(user, typeof(T));
			return CheckAuthorizationOnType(user, usrAuths);
		}

		public bool IsAuthorizedTypeNotMatch(IDictionary<string, object> dict, string user, Type entityType, string[] keyList)
		{
			return CheckAuthorized(user, entityType, keyList)(dict);
		}

		public IEnumerable<T> InitAuthorizations<T>(IEnumerable<T> data, string user)
		{
			var editableProperty = typeof(T).GetProperty(typeof(T).GetTablePrefix() + "_IsEditable");
			if (editableProperty != null)
			{
				var checkAuthorized = CheckAuthorizedWithGenericType<T>(user);
				foreach (var dt in data)
				{
					editableProperty.SetValue(dt, checkAuthorized(dt));
					yield return dt;
				}
			}
			else
			{
				foreach (var dt in data)
				{
					yield return dt;
				}
			}
		}

		public IEnumerable<T> InitAuthorizationsForSelectExpandWrapper<T>(IEnumerable<T> data, string user) where T : ISelectExpandWrapper
		{
			var entityType = data.GetType().GetGenericArguments().FirstOrDefault().GetGenericArguments().FirstOrDefault();
			var editableProperty = entityType.GetProperty(entityType.GetTablePrefix() + "_IsEditable");

			if (editableProperty != null)
			{
				if (data != null && data.Any())
				{
					var tempWrapper = data.First() as ISelectExpandWrapper;
					var keyList = tempWrapper.ToDictionary().Keys.ToArray();
					var methodCall = CheckAuthorized(user, entityType, keyList);
					var selectExpandWrapperType = typeof(T);
					var dictField = selectExpandWrapperType.GetField("_containerDict", BindingFlags.NonPublic | BindingFlags.Instance);
					while (dictField == null)
					{
						selectExpandWrapperType = selectExpandWrapperType.BaseType;
						if (selectExpandWrapperType == null)
						{
							break;
						}
						dictField = selectExpandWrapperType.GetField("_containerDict", BindingFlags.NonPublic | BindingFlags.Instance);
					}
					if (dictField == null)
					{
						foreach (var dt in data)
						{
							yield return dt;
						}
					}
					foreach (var dt in data)
					{
						var wrapper = dt as ISelectExpandWrapper;
						var dict = wrapper.ToDictionary();
						if (dict.ContainsKey(editableProperty.Name))
						{
							dict[editableProperty.Name] = methodCall(dict);
							dictField.SetValue(dt, dict);
						}
						yield return dt;
					}
				}
			}
			else
			{
				foreach (var dt in data)
				{
					yield return dt;
				}
			}
		}

		public IEnumerable<T> FilterAuthorizedData<T>(IEnumerable<T> data, string user)
		{
			var checkAuthorized = CheckAuthorizedWithGenericType<T>(user);
			foreach (var dt in data)
			{
				var authorized = checkAuthorized(dt);
				if (authorized)
				{
					yield return dt;
				}
			}
		}

		Func<T, bool> CheckAuthorizedWithGenericType<T>(string user)
		{
			var usrAuths = GetAuthorizationData(user, typeof(T));
			return CheckAuthorizationOnType(user, usrAuths) ? x => true : CheckAuthorizedWithGenericType<T>(usrAuths);
		}

		static Func<T, bool> CheckAuthorizedWithGenericType<T>(IEnumerable<RefUserAuthorization> usrAuths)
		{
			Argument.Argument.NotNull(usrAuths, nameof(usrAuths));

			var param = Expression.Parameter(typeof(T));
			Expression result = Expression.Constant(false);
			foreach (var usrAuthGroup in usrAuths.GroupBy(x => x.UA_DataSetName))
			{
				Expression groupResult = null;
				foreach (var usrAuth in usrAuthGroup)
				{
					var propertyInfo = typeof(T).GetProperty(usrAuth.UA_ColumnName);
					var propertyFilterExp = ExpressionHelper.GetPropertyFilterExpression<T>(param, propertyInfo, usrAuth.UA_ColumnValue, Operations.Equals);
					groupResult = groupResult != null ? Expression.And(groupResult, propertyFilterExp) : propertyFilterExp;
				}
				if (groupResult != null)
				{
					result = Expression.Or(result, groupResult);
				}
			}
			return Expression.Lambda<Func<T, bool>>(result, param).Compile();
		}

		Func<IDictionary<string, object>, bool> CheckAuthorized(string user, Type entityType, string[] keyList)
		{
			var usrAuths = GetAuthorizationData(user, entityType);
			return CheckAuthorizationOnType(user, usrAuths) ? x => true : CheckAuthorized(usrAuths, keyList);
		}

		static bool CheckAuthorizationOnType(string user, IEnumerable<RefUserAuthorization> usrAuths)
		{
			Argument.Argument.NotNull(usrAuths, nameof(usrAuths));
			return usrAuths.Any(x => "*".Equals(x.UA_TableName, StringComparison.OrdinalIgnoreCase))
				|| usrAuths.Any(x => string.IsNullOrEmpty(x.UA_ColumnName));
		}

		static Func<IDictionary<string, object>, bool> CheckAuthorized(IEnumerable<RefUserAuthorization> usrAuths, string[] keyList)
		{
			Argument.Argument.NotNull(usrAuths, nameof(usrAuths));

			Expression result = Expression.Constant(false);
			var dictType = typeof(IDictionary<string, object>);
			var param = Expression.Parameter(typeof(IDictionary<string, object>));
			foreach (var usrAuthGroup in usrAuths.GroupBy(x => x.UA_DataSetName))
			{
				Expression groupResult = null;
				foreach (var usrAuth in usrAuthGroup)
				{
					if (keyList.Contains(usrAuth.UA_ColumnName))
					{
						PropertyInfo indexerProp = dictType.GetProperty("Item");
						var dictKeyConstant = Expression.Constant(usrAuth.UA_ColumnName);
						var indexExpression = Expression.MakeIndex(param, indexerProp, new[] { dictKeyConstant });
						var dictAccess = Expression.Convert(indexExpression, typeof(string));

						var propertyFilterExp = Expression.Equal(dictAccess, Expression.Constant(usrAuth.UA_ColumnValue));
						groupResult = groupResult != null ? Expression.And(groupResult, propertyFilterExp) : propertyFilterExp;
					}
					else
					{
						groupResult = null;
						break;
					}
				}
				if (groupResult != null)
				{
					result = Expression.Or(result, groupResult);
				}
			}
			return Expression.Lambda<Func<IDictionary<string, object>, bool>>(result, param).Compile();
		}

		protected abstract IEnumerable<RefUserAuthorization> GetAuthorizationData(string user, Type type);
	}
}
