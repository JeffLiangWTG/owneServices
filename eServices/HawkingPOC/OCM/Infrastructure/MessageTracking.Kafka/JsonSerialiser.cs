using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka
{
	public class JsonSerialiser<T> : IDisposable, ISerializer<T>, IDeserializer<T>
    {
		readonly StringSerializer serialiser;
		readonly StringDeserializer deserialiser;

		public JsonSerialiser()
		{
			serialiser = new StringSerializer(Encoding.UTF8);
			deserialiser = new StringDeserializer(Encoding.UTF8);
		}

		public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
		{
			return config;
		}

		public byte[] Serialize(string topic, T data)
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
		}

		public T Deserialize(string topic, byte[] data)
		{
			return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
		}

		#region IDisposable Support
		private bool disposedValue = false; 

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					serialiser?.Dispose();
					deserialiser?.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
