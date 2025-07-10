using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public class RateAttributeSet
	{
		public IEnumerable<RateAttribute> Attributes
			=> comparableTypes.Values.SelectMany(x => x)
				.Concat(otherTypes.Values.SelectMany(x => x));

		public IEnumerable<RateAttribute> ComparableAttributes
			=> comparableTypes.Values.SelectMany(x => x);

		public void Add(string code, string value, decimal? amount = null)
		{
			var map = JobChargeAttrib.IsComparableType(code) ? comparableTypes : otherTypes;
			if (!map.TryGetValue(code, out var values))
			{
				values = new HashSet<RateAttribute>(RateAttributeComparer.CodeValue);
				map.Add(code, values);
			}

			var item = new RateAttribute(code, value, amount ?? 0);
			if (!values.Contains(item))
			{
				values.Add(item);
			}
		}

		public HashSet<RateAttribute> Get(string code)
		{
			var map = JobChargeAttrib.IsComparableType(code) ? comparableTypes : otherTypes;
			if (map.TryGetValue(code, out var values))
			{
				return values;
			}

			return new HashSet<RateAttribute>();
		}

		public T GetSingleValue<T>(string code)
		{
			var attribute = Get(code).FirstOrDefault();
			if (attribute == null)
			{
				return default;
			}

			return typeof(T) switch
			{
				{ } t when t == typeof(string) => (T)Convert.ChangeType(attribute.Value, t),
				{ } t when t == typeof(decimal) && decimal.TryParse(attribute.Value, out var d) => (T)Convert.ChangeType(d, t),
				{ } t when t == typeof(Guid) && Guid.TryParse(attribute.Value, out var g) => (T)Convert.ChangeType(g, t),
				_ => default
			};
		}

		/// <summary>
		///		Adds <paramref name="attribute"/> from <paramref name="sourceSet"/> to the current attribute set if
		///		it doesn't exist or replaces it if it does exist. 
		/// </summary>
		public void AddOrReplace(RateAttributeSet sourceSet, string attribute)
		{
			var sourceAttributes = sourceSet.Get(attribute);
			if (sourceAttributes.Any())
			{
				var map = JobChargeAttrib.IsComparableType(attribute) ? comparableTypes : otherTypes;
				map[attribute] = sourceAttributes;
			}
		}

		/// <summary>
		///		Adds <paramref name="attribute"/> from <paramref name="sourceSet"/> to the current attribute set if
		///		it doesn't exist. 
		/// </summary>
		public void AddIfNotFound(RateAttributeSet sourceSet, string attribute)
		{
			var map = JobChargeAttrib.IsComparableType(attribute) ? comparableTypes : otherTypes;
			if (map.ContainsKey(attribute))
			{
				return;
			}

			AddOrReplace(sourceSet, attribute);
		}

		public void Clear()
		{
			comparableTypes.Clear();
			otherTypes.Clear();
		}

		readonly Dictionary<string, HashSet<RateAttribute>> comparableTypes = new Dictionary<string, HashSet<RateAttribute>>();
		readonly Dictionary<string, HashSet<RateAttribute>> otherTypes = new Dictionary<string, HashSet<RateAttribute>>();

		#region Types

		public class RateAttributeSetMergeComparer : IEqualityComparer<RateAttributeSet>
		{
			public RateAttributeSetMergeComparer(HashSet<string> excludedAttributes)
			{
				this.excludedAttributes = excludedAttributes ?? new HashSet<string>();
			}

			readonly HashSet<string> excludedAttributes;

			public bool Equals(RateAttributeSet x, RateAttributeSet y)
			{
				if (x.comparableTypes.Count != y.comparableTypes.Count)
				{
					return false;
				}

				foreach (var pair in x.comparableTypes)
				{
					if (excludedAttributes.Contains(pair.Key))
					{
						continue;
					}

					if (!y.comparableTypes.TryGetValue(pair.Key, out var otherValues) || !comparer.Equals(pair.Value, otherValues))
					{
						return false;
					}
				}

				return true;
			}

			public int GetHashCode(RateAttributeSet obj)
			{
				var result = 666;

				foreach (var pair in obj.comparableTypes)
				{
					if (excludedAttributes.Contains(pair.Key))
					{
						continue;
					}

					result ^= comparer.GetHashCode(pair.Value);
				}

				return result;
			}

			[ThreadSafe]
			static readonly IEqualityComparer<HashSet<RateAttribute>> comparer = HashSet<RateAttribute>.CreateSetComparer();
		}

		#endregion
	}

	public class RateAttribute
	{
		public RateAttribute(string code, string value)
		{
			Argument.NotNull(code, nameof(code));

			Code = code;
			Value = value;
		}

		public RateAttribute(string code, string value, decimal amount)
			: this(code, value)
		{
			Amount = amount;
		}

		public string Code { get; }
		public string Value { get; }
		public decimal Amount { get; }

		public override int GetHashCode()
		{
			return Value != null ? Value.GetHashCode() : Code.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var other = obj as RateAttribute;
			if (other == null)
			{
				return false;
			}

			return RateAttributeComparer.CodeValueAmount.Equals(this, other);
		}
	}

	static class RateAttributeComparer
	{
		[ThreadSafe]
		public static readonly IEqualityComparer<RateAttribute> CodeValue = new CodeValueComparer();
		[ThreadSafe]
		public static readonly IEqualityComparer<RateAttribute> CodeValueAmount = new CodeValueAmountComparer();

		[Immutable]
		class CodeValueComparer : IEqualityComparer<RateAttribute>
		{
			public bool Equals(RateAttribute x, RateAttribute y)
			{
				return x.Code == y.Code && x.Value == y.Value;
			}

			public int GetHashCode(RateAttribute obj)
			{
				return obj.Value != null ? obj.Value.GetHashCode() : obj.Code.GetHashCode();
			}
		}

		[Immutable]
		class CodeValueAmountComparer : IEqualityComparer<RateAttribute>
		{
			public bool Equals(RateAttribute x, RateAttribute y)
			{
				return x.Code == y.Code && x.Value == y.Value && x.Amount == y.Amount;
			}

			public int GetHashCode(RateAttribute obj)
			{
				return obj.Value != null ? obj.Value.GetHashCode() : obj.Code.GetHashCode();
			}
		}
	}
}
