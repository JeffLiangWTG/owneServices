using NUnit.Framework.Interfaces;

namespace CargoWise.Blazor.Testing.Common
{
	public static class NunitExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1166:Do not extend NUnit", Justification = "Baseline")]
		public static bool TryGetPropertyFromParents<T>(this ITest test, string propertyKey, out T value, out ITest parentWithValue)
		{
			var currentParent = test.Parent;
			while (currentParent != null)
			{
				value = (T)currentParent.Properties.Get(propertyKey);
				if (value != null)
				{
					parentWithValue = currentParent;
					return true;
				}
				currentParent = currentParent.Parent;
			}
			value = default;
			parentWithValue = default;
			return false;
		}
	}
}
