using System;
using System.Runtime.Serialization;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	[Serializable]
	public class UnhandledApplicationException : Exception
	{
		public UnhandledApplicationException()
		{
		}

		public UnhandledApplicationException(string message) : base(message)
		{
		}

		public UnhandledApplicationException(string message, Exception innerException) : base(message, innerException)
		{
		}

#pragma warning disable SYSLIB0051 // Type or member is obsolete
		protected UnhandledApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
		{
		}
	}
}
