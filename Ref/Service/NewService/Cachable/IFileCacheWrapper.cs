namespace CargoWise.RefDbRepo.NewService
{
	public interface IFileCacheWrapper
	{
		object Get(string key, string regionName = null);
		void Add(string key, object value, string regionName = null);
		object Remove(string key, string regionName = null);
		bool Contains(string key, string regionName = null);
		string GetCachePath(string key);
		void ShrinkCacheSize();
	}
}
