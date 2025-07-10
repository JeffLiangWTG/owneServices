using System;

namespace CargoWise.RefDbRepo.NewService
{
	public interface ICacheWrapper
	{
		object Get(string key);
		object Add(string key, object value);
		object Add(string key, object value, DateTimeOffset absoluteExpiration);
		object Remove(string key);
		bool Contains(string key);
	}
}
