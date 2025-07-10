using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class BusinessObjectCollectionValidationCache<T> where T : BusinessObject
	{
		public BusinessObjectCollectionValidationCache(BusinessObjectCollection bizos, params ValidationRule<T>[] rules)
		{
			this.bizos = bizos;
			this.bizosByPK = bizos.ToDictionary(b => b.PK, v => v as T);
			this.rules = rules.ToDictionary(s => s.Name);
			this.cache = rules.ToDictionary(s => s, s => new Dictionary<ZGuid, List<AddValidationNotification<T>>>());

			foreach (var rule in rules)
			{
				var fieldBindings = rule.GetFieldBindings();
				if (fieldBindings.Any())
				{
					bizos.SubscribeToChildrenChanges(() =>
					{
						Refresh(rule);
					}, fieldBindings);
				}
			}
		}

		readonly BusinessObjectCollection bizos;
		Dictionary<ZGuid, T> bizosByPK;
		readonly Dictionary<string, ValidationRule<T>> rules;
		readonly Dictionary<ValidationRule<T>, Dictionary<ZGuid, List<AddValidationNotification<T>>>> cache;

		bool isValidatingAll;

		#region Public API

		public void Refresh(ValidationRule<T> rule = null)
		{
			bizosByPK = bizos.ToDictionary(b => b.PK, v => v as T);

			if (rule == null)
			{
				foreach (var r in rules.Values)
				{
					cache[r].Clear();
				}
			}
			else
			{
				cache[rule].Clear();
			}
		}

		public void Validate(T bizo, ZPropertyInfo info)
		{
			if (bizosByPK.ContainsKey(bizo.PK))
			{
				RunValidationRules(bizo, info);
			}
		}

		public void RunValidationRules(T bizo, ZPropertyInfo info)
		{
			var pk = bizo.PK;

			foreach (var rule in cache.Where(p => p.Key.PropertyName.Equals(info.Name)).ToArray())
			{
				var key = rule.Key;
				if (!isValidatingAll && key.IsCachedForValidateAllOnly)
				{
					key.CallValidation(bizo, info);
				}
				else if (!TryValidate(bizo, info, pk, rule))
				{
					var itemsChanged = key.ApplyRuleToCache(bizosByPK, rule.Value);

					if (!TryValidate(bizo, info, pk, rule))
					{
						ErrorReporter.ReportOnce("BusinessObjectCollectionValidationCache.InvalidValidationRule", string.Format(CultureInfo.InvariantCulture, "Rule {0} didn't cover bizo. ({1})", key.Name, bizo));
					}

					if (!isValidatingAll && itemsChanged.Remove(bizo)) // Only call validation on related items if validation on current item changed.
					{
						foreach (var item in itemsChanged)
						{
							key.CallValidation(item, info);
						}
					}
				}
			}
		}

		public IDisposable SetCacheForValidateAll()
		{
			isValidatingAll = true;

			return new DisposableAction(() =>
			{
				isValidatingAll = false;
				foreach (var rule in cache.Keys.Where(r => r.IsCachedForValidateAllOnly))
				{
					cache[rule].Clear();
				}
			});
		}

		static bool TryValidate(T bizo, ZPropertyInfo info, ZGuid pk, KeyValuePair<ValidationRule<T>, Dictionary<ZGuid, List<AddValidationNotification<T>>>> rule)
		{
			if (rule.Value.TryGetValue(pk, out List<AddValidationNotification<T>> action))
			{
				foreach (var item in action)
				{
					item(bizo, info);
				}
				return true;
			}
			return false;
		}

		public void Invalidate(string name) => Invalidate(rules[name]);

		#endregion

		void Invalidate(ValidationRule<T> rule) => cache[rule].Clear();
	}

	public delegate void AddValidationNotification<T>(T item, ZPropertyInfo info);

	public delegate void CallValidation<T>(T item, ZPropertyInfo info);

	[SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures", Justification = "I wanted to though...")]
	public delegate IEnumerable<IGrouping<AddValidationNotification<T>, T>> GroupFunction<T>(IEnumerable<T> t);

	public class ValidationRule<T> where T : BusinessObject
	{
		public ValidationRule(string name, string validationPropertyName, GroupFunction<T> groupFunction, CallValidation<T> callValidation, bool isCachedForValidateAllOnly, params string[] fieldBindings)
		{
			Argument.NotNull(name, nameof(name));
			Argument.NotNull(validationPropertyName, nameof(validationPropertyName));
			Argument.NotNull(isCachedForValidateAllOnly, nameof(isCachedForValidateAllOnly));
			Argument.NotNull(groupFunction, nameof(groupFunction));
			Argument.NotNull(callValidation, nameof(callValidation));
			Argument.NotNull(fieldBindings, nameof(fieldBindings));

			Name = name;
			PropertyName = validationPropertyName;
			IsCachedForValidateAllOnly = isCachedForValidateAllOnly;
			this.groupFunction = groupFunction;
			this.fieldBindings = fieldBindings;
			CallValidation = callValidation;
		}

		public string Name { get; }
		public string PropertyName { get; }
		public bool IsCachedForValidateAllOnly { get; }
		public string[] GetFieldBindings() => fieldBindings;
		public CallValidation<T> CallValidation { get; }

		readonly GroupFunction<T> groupFunction;
		readonly string[] fieldBindings;

		internal HashSet<T> ApplyRuleToCache(IDictionary<ZGuid, T> collection, Dictionary<ZGuid, List<AddValidationNotification<T>>> cache)
		{
			var itemsChanged = new HashSet<T>(cache.Keys.Select(k => collection.GetValueSafe(k)).WhereNotNull());
			cache.Clear();
			foreach (var item in collection.Values)
			{
				cache[item.PK] = new List<AddValidationNotification<T>>();
			}

			foreach (var group in groupFunction(collection.Values))
			{
				var key = group.Key;
				foreach (var item in group)
				{
					if (cache.TryGetValue(item.PK, out List<AddValidationNotification<T>> validationNotifications))
					{
						validationNotifications.Add(key);
						itemsChanged.Add(item);
					}
				}
			}
			return itemsChanged;
		}

		#region Equality 

		public override int GetHashCode() => Name.GetHashCode();

		public override bool Equals(object obj)
		{
			return obj is ValidationRule<T> rule && Name == rule.Name;
		}

		#endregion
	}
}
