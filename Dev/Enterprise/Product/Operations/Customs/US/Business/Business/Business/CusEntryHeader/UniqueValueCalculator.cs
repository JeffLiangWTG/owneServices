using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	static class UniqueValueCalculator
	{
		public static TResult GetUniqueValue<T, TResult>(IEnumerable<T> elements, Func<T, TResult> getValueFromElement, TResult valueIfMulti) where TResult : IZType
		{
			TResult result = default(TResult);

			if (HasMultiValues(elements, getValueFromElement))
			{
				result = valueIfMulti;
			}
			else
			{
				var firstElement = elements.FirstOrDefault();

				if (firstElement != null)
				{
					result = getValueFromElement(firstElement);
				}
			}

			return result;
		}

		public static bool HasMultiValues<T, TValue>(IEnumerable<T> elements, Func<T, TValue> getValueFromElement) where TValue : IZType
		{
			bool result = false;

			bool hasResultBeenSet = false;
			TValue value = default(TValue);

			foreach (T element in elements)
			{
				var valueFromElement = getValueFromElement(element);
				if (!valueFromElement.IsEmpty)
				{
					if (!hasResultBeenSet)
					{
						hasResultBeenSet = true;
						value = valueFromElement;
					}
					else
					{
						if (!value.Equals(valueFromElement))
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}
	}
}
