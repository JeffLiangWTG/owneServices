using Microsoft.BizTalk.Message.Interop;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	public static class PipelineComponentsExtensions
	{
		public static string ReadPropertyString<T>(this IBaseMessageContext context) where T : PropertyBase
		{
			var value = context.Read(ContextProperty<T>.Name, ContextProperty<T>.Namespace);
			if (value == null) return null;

			return value.ToString();
		}

		public static ContextPropertyType GetPropertyType<T>(this IBaseMessageContext context) where T : PropertyBase
		{
			return context.GetPropertyType(ContextProperty<T>.Name, ContextProperty<T>.Namespace);
		}

		public static void WriteProperty<T>(this IBaseMessageContext context, object value) where T : PropertyBase
		{
			context.Write(ContextProperty<T>.Name, ContextProperty<T>.Namespace, value);
		}

		public static void PromoteProperty<T>(this IBaseMessageContext context, object value) where T : PropertyBase
		{
			context.Promote(ContextProperty<T>.Name, ContextProperty<T>.Namespace, value);
		}

		public static void WriteProperty<T>(this IBasePropertyBag properties, object value) where T : PropertyBase
		{
			properties.Write(ContextProperty<T>.Name, ContextProperty<T>.Namespace, value);
		}

		public static string ReadPropertyString<T>(this IBasePropertyBag properties) where T : PropertyBase
		{
			return (string)properties.Read(ContextProperty<T>.Name, ContextProperty<T>.Namespace);
		}
	}
}
