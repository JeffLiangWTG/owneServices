using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.XLANGs.BaseTypes;
using System.Collections.Concurrent;

namespace CargoWise.eHub.Shared.BizTalk.PipelineHelpers
{
	public static class PipelineContextExtensions
	{
		public static string ReadPropertyString<T>(this IBaseMessageContext context) where T : PropertyBase
		{
			var instance = instanceCache.GetOrAdd(typeof(T), t => Activator.CreateInstance<T>());
			var value = context.Read(instance.Name.Name, instance.Name.Namespace);
			return value == null ? null : value.ToString();
		}

		public static ContextPropertyType GetPropertyType<T>(this IBaseMessageContext context) where T : PropertyBase
		{
			var instance = instanceCache.GetOrAdd(typeof(T), t => Activator.CreateInstance<T>());
			return context.GetPropertyType(instance.Name.Name, instance.Name.Namespace);
		}

		public static void WriteProperty<T>(this IBaseMessageContext context, object value) where T : PropertyBase
		{
			var instance = instanceCache.GetOrAdd(typeof(T), t => Activator.CreateInstance<T>());
			context.Write(instance.Name.Name, instance.Name.Namespace, value);
		}

		public static void PromoteProperty<T>(this IBaseMessageContext context, object value) where T : PropertyBase
		{
			var instance = instanceCache.GetOrAdd(typeof(T), t => Activator.CreateInstance<T>());
			context.Promote(instance.Name.Name, instance.Name.Namespace, value);
		}

		public static void WriteProperty<T>(this IBasePropertyBag properties, object value) where T : PropertyBase
		{
			var instance = instanceCache.GetOrAdd(typeof(T), t => Activator.CreateInstance<T>());
			properties.Write(instance.Name.Name, instance.Name.Namespace, value);
		}

		public static string ReadPropertyString<T>(this IBasePropertyBag properties) where T : PropertyBase
		{
			var instance = instanceCache.GetOrAdd(typeof(T), t => Activator.CreateInstance<T>());
			return (string)properties.Read(instance.Name.Name, instance.Name.Namespace);
		}

		static ConcurrentDictionary<Type, PropertyBase> instanceCache = new ConcurrentDictionary<Type,PropertyBase>();
	}
}
