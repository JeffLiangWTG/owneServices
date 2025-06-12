using System;
using System.Diagnostics.CodeAnalysis;

namespace eServices.Configuration.Framework
{
	[Serializable]
	[ExcludeFromCodeCoverage]
	public class ConfigurationException : Exception
	{
		public ConfigurationException() { }

		public ConfigurationException(string message) : base(message) { }

		public ConfigurationException(string message, Exception inner) : base(message, inner) { }

		public ConfigurationException(Exception inner) : base("Exception thrown by transport protocol", inner) { }

#if NET48
		protected ConfigurationException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
