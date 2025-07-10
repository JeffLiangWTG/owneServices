using System.Collections.Generic;

namespace Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort
{
	// Group similar calculators together to be added
	sealed class DocLineAmountProperty : Dictionary<string, string>
	{
		public DocLineAmountProperty((string key, string value)[] properties)
		{
			foreach (var (key, value) in properties)
			{
				if (!string.IsNullOrEmpty(value))
				{
					this[key] = value;
				}
			}
		}

		public DocLineAmountProperty(DocLineAmountProperty property)
		{
			foreach (var keyValue in property)
			{
				this[keyValue.Key] = keyValue.Value;
			}
		}

		public override bool Equals(object obj)
		{
			var dict1 = this;
			var dict2 = ((DocLineAmountProperty)obj);

			if (dict1.Count != dict2.Count)
			{
				return false;
			}

			foreach (var keyValue in dict1)
			{
				if (!dict2.TryGetValue(keyValue.Key, out var value2))
				{
					return false;
				}

				if (!EqualityComparer<string>.Default.Equals(keyValue.Value, value2))
				{
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode()
		{
			var hash = 17; // Start with a prime number to minimize collisions

			unchecked
			{
				foreach (var keyValue in this)
				{
					hash = hash * 31 + keyValue.Key.GetHashCode();
					hash = hash * 31 + (keyValue.Value == null ? 0 : keyValue.Value.GetHashCode());
				}
			}

			return hash;
		}
	}
}
