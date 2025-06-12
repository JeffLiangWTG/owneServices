using System;
using System.Runtime.Serialization;

namespace OcmPoc.Mapping.Interface
{
	public class MappingFailedException : Exception
	{
		public MappingFailedException()
		{
		}

		public MappingFailedException(string message) 
			: base(message)
		{
		}

		public MappingFailedException(string message, Exception innerException) 
			: base(message, innerException)
		{
		}

		protected MappingFailedException(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}
	}
}
