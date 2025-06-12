using System;
using System.Linq;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace ParDepComponent
{
	static class QueueItemExtensions
    {
		public static string GetPartitionKey(this QueueItem item, string[] partitionHeaders)
		{
			return string.Join("::", partitionHeaders.Select(h => item.GetValue(h)));
		}


		static string GetValue(this QueueItem item, string name)
		{
			return item.GetPropertyValue(name) ??
				   item.GetMetadataValue(name);
		}

		static string GetPropertyValue(this QueueItem item, string name)
		{
			return item.GetType()
					   .GetProperty(name)
					   ?.GetValue(item)
					   ?.ToString();
		}

		static string GetMetadataValue(this QueueItem item, string name)
		{
			if (item.Metadata.TryGetValue(name, out object value))
			{
				if (value is byte[] bytes)
				{
					value = BitConverter.ToString(bytes);
				}
			}

			return value?.ToString();
		}
	}
}
