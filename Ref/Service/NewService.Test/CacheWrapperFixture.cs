using System;
using System.Runtime.Caching;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class CacheWrapperFixture
	{
		[Test]
		public void Add_AbsoluteExpiration()
		{
			using (var mem = new MemoryCache(Guid.NewGuid().ToString()))
			{
				var wrapper = new CacheWrapper(mem, new CacheItemPolicy());
				wrapper.Add("Key", "Value", DateTimeOffset.UtcNow.AddMilliseconds(1000));
				Assert.NotNull(wrapper.Get("Key"));
				Thread.Sleep(2000);
				Assert.Null(wrapper.Get("Key"));
			}
		}
	}
}
