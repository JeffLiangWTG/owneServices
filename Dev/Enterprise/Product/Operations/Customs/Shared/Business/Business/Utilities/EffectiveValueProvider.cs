using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IEffectiveValueSupporter
	{
		EffectiveValueProvider EffectiveValueProvider { get; }
	}

	public class EffectiveValueProvider
	{
		public T GetValueToReturn<T>(T baseValue, BusinessObject parentObject, string parentFieldName, string childFieldName) where T : IZType
		{
			T result = baseValue;
			if (result.IsEmpty)
			{
				var effectiveValue = GetEffectiveValue(childFieldName) ?? GetParentValue(parentObject, parentFieldName, (T)result.Default);
				result = (T)effectiveValue;
			}
			return result;
		}

		public T GetValueToSet<T>(T valuePassed, BusinessObject parentObject, string parentFieldName) where T : IZType
		{
			T result = valuePassed;

			if (!valuePassed.IsDefault && GetParentValue(parentObject, parentFieldName, (T)result.Default).Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		public static void ClearChildValuesIfSame<T>(IEnumerable<T> collection, IZType parentValue, string childFieldName)
			where T : BusinessObject, IEffectiveValueSupporter
		{
			foreach (var child in collection)
			{
				var childValue = (IZType)child[childFieldName];

				if (childValue.Equals(parentValue))
				{
					using (child.EffectiveValueProvider.SuspendValue(childFieldName, parentValue))
					{
						child[childFieldName] = childValue.Default;
					}
					child.ZPropertyInfoHash[childFieldName]?.RefreshBinding();
				}
			}
		}

		IDisposable SuspendValue(string fieldName, IZType parentValue)
		{
			return new DisposableAction(() =>
			{
				if (!EffectiveValueSuspenders.ContainsKey(fieldName))
				{
					EffectiveValueSuspenders.Add(fieldName, parentValue);
				}
			}, () =>
			{
				if (EffectiveValueSuspenders.ContainsKey(fieldName))
				{
					EffectiveValueSuspenders.Remove(fieldName);
				}
			});
		}

		IZType GetEffectiveValue(string fieldName) => EffectiveValueSuspenders.TryGetValue(fieldName, out var value) ? value : null;

		T GetParentValue<T>(BusinessObject parentObject, string parentFieldName, T defaultValue) where T : IZType => parentObject == null ? defaultValue : (T)parentObject[parentFieldName];

		Dictionary<string, IZType> EffectiveValueSuspenders => effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, IZType>());
		Dictionary<string, IZType> effectiveValueSuspenders;
	}
}
