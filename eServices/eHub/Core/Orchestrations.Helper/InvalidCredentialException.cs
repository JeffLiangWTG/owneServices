using System;
using System.Runtime.Serialization;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
	[Serializable]
	public class InvalidCredentialException : Exception
	{
		public InvalidCredentialException() { }
		public InvalidCredentialException(string message) : base(message) { }
		public InvalidCredentialException(string message, Exception innerException) : base(message, innerException) { }
		protected InvalidCredentialException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
