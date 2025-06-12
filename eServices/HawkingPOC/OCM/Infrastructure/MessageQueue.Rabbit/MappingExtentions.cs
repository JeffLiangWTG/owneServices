using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	static class MappingExtentions
	{
		static Dictionary<Type, Func<object, bool>> isDefaultPredicate =
			new Dictionary<Type, Func<object, bool>>
			{
				[typeof(sbyte)] = value => (sbyte)value == default(sbyte),
				[typeof(short)] = value => (short)value == default(short),
				[typeof(int)] = value => (int)value == default(int),
				[typeof(long)] = value => (long)value == default(long),
				[typeof(float)] = value => (float)value == default(float),
				[typeof(double)] = value => (double)value == default(double),
				[typeof(decimal)] = value => (decimal)value == default(decimal),
				[typeof(Guid)] = value => (Guid)value == default(Guid),
				[typeof(string)] = value => (string)value == default(string)
			};

		static HashSet<string> ignoredProperties = new HashSet<string>
		{
			nameof(QueueItem.Metadata), nameof(QueueItem.Delivery)
		};

		public static IBasicProperties MapFrom<TQueueItem>(this IBasicProperties properties, TQueueItem item)
			where TQueueItem : QueueItem
		{
			Preconditions.CheckNotNull(properties, nameof(properties));
			Preconditions.CheckNotNull(item, nameof(item));

			properties.Headers = properties.Headers ?? new Dictionary<string, object>();

			item.Metadata.CopyTo(properties.Headers);

			foreach (var property in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (ignoredProperties.Contains(property.Name)) { continue; }

				var value = property.GetValue(item);

				if (!HasDefaultValue(property, value))
				{
					properties.Headers.SetValue(property.Name, value);
				}
			}

			return properties;
		}

		private static void CopyTo(this IDictionary<string, object> source, IDictionary<string, object> destination)
		{
			foreach (var key in source.Keys)
			{
				destination.SetValue(key, source[key]);
			}
		}

		private static void SetValue(this IDictionary<string, object> dictionary, string key, object value)
		{
			switch (value)
			{
				case string s:
					dictionary[key] = Encoding.UTF8.GetBytes(s);
					break;
				case Guid g:
					dictionary[key] = g.ToByteArray();
					break;
				default:
					dictionary[key] = value;
					break;
			}
		}

		public static TQueueItem MapTo<TQueueItem>(this IBasicProperties properties)
			where TQueueItem : QueueItem, new()
		{
			Preconditions.CheckNotNull(properties, nameof(properties));

			var item = new TQueueItem();

			if (properties.Headers == null) { return item; }

			foreach (var key in properties.Headers.Keys)
			{
				var property = typeof(TQueueItem).GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
				if (property == null)
				{
					item.Metadata[key] = properties.Headers[key];
				}
				else if (!property.PropertyType.IsSupported())
				{
					throw new InvalidOperationException($"the {typeof(TQueueItem).Name}.{property.Name} property " +
														$"of type {property.PropertyType} is not supported.");
				}
				else
				{
					item.SetProperty(property, properties.Headers[key]);
				}
			}

			return item;
		}

		public static TQueueItem MapTo<TQueueItem>(this BasicDeliverEventArgs eventArgs)
			where TQueueItem : QueueItem, new()
		{
			Preconditions.CheckNotNull(eventArgs, nameof(eventArgs));

			var item = eventArgs.BasicProperties.MapTo<TQueueItem>();

			item.Delivery = new QueueItemDelivery(eventArgs.DeliveryTag, eventArgs.Exchange, eventArgs.RoutingKey, eventArgs.Redelivered);

			return item;
		}

		public static TQueueItem MapTo<TQueueItem>(this BasicGetResult result)
			where TQueueItem : QueueItem, new()
		{
			Preconditions.CheckNotNull(result, nameof(result));

			var item = result.BasicProperties.MapTo<TQueueItem>();

			item.Delivery = new QueueItemDelivery(result.DeliveryTag, result.Exchange, result.RoutingKey, result.Redelivered);

			return item;
		}

		private static bool IsSupported(this Type propertyType)
		{
			return isDefaultPredicate.ContainsKey(propertyType);
		}

		private static void SetProperty<TQueueItem>(this TQueueItem item, PropertyInfo property, object value)
			where TQueueItem : QueueItem
		{
			var propertyType = property.PropertyType;

			if (propertyType == typeof(Guid) && TryGetGuid(value, out Guid valueAsGuid))
			{
				value = valueAsGuid;
			}

			if (propertyType == typeof(string) && TryGetString(value, out string valueAsString))
			{
				value = valueAsString;
			}

			var valueType = value.GetType();
			if (valueType != propertyType)
			{
				throw new InvalidOperationException($"the value of type '{valueType}' in the '{property.Name}' header is not compatible " +
													$"with the matched property of type {propertyType}.");
			}

			property.SetValue(item, value);
		}

		private static bool TryGetGuid(object value, out Guid guid)
		{
			if (!(value is byte[] bytes)) { return false; }
			if (bytes.Length != Guid.Empty.ToByteArray().Length) { return false; }

			guid = new Guid(bytes);
			return true;
		}

		private static bool TryGetString(object value, out string result)
		{
			result = null;

			if (!(value is byte[] bytes)) { return false; }

			result = Encoding.UTF8.GetString(bytes);
			return true;
		}

		private static bool HasDefaultValue(PropertyInfo property, object value)
		{
			Type propertyType = property.PropertyType;

			try
			{
				return isDefaultPredicate[propertyType](value);
			}
			catch (KeyNotFoundException)
			{ 
				string fullPropertyName = $"{property.DeclaringType.Name}.{property.Name}";
				throw new InvalidOperationException(
					$"the {fullPropertyName} property of type {propertyType} is not supported.");
			}
		}
	}
}
