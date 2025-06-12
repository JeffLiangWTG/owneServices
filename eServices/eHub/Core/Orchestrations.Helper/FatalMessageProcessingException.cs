using System;
using System.Runtime.Serialization;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	[Serializable]
	public class FatalMessageProcessingException : Exception
	{
		public FatalMessageProcessingException() { }
		public FatalMessageProcessingException(string message) : base(message) { }
		public FatalMessageProcessingException(string message, Exception innerException) : base(message, innerException) { }
		protected FatalMessageProcessingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
