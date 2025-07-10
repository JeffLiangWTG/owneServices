using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business.Testing
{
	public static class FactoryExtensions
	{
		public static void SetCachedValue<T>(this BusinessObjectFactory self, object cacheKey, T value)
		{
			self.ClearCachedValue<T>(cacheKey);
			self.GetCachedValue(cacheKey, () => value);
		}
	}
}
