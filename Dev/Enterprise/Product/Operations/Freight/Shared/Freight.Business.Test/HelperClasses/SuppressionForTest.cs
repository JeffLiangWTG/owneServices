namespace Enterprise.Freight.Business.Testing
{
	public static class SuppressionForTest
	{
		public static void CacheObjectClear()
		{
			Suppression.CacheObject.Clear();
		}

		public static int CacheObjectCount()
		{
			return Suppression.CacheObject.Count;
		}
	}
}
