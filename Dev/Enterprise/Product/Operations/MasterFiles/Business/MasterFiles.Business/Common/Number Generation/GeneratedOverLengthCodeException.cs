using System;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business
{
	[ExceptionVisibility(ExceptionVisibility.User), Serializable]
	public class GeneratedOverLengthCodeException : Exception
	{
		public GeneratedOverLengthCodeException() { }

		public GeneratedOverLengthCodeException(string message)
			: base(message) { }

		public GeneratedOverLengthCodeException(string message, Exception innerException)
			: base(message, innerException) { }

#if NETFRAMEWORK
		public GeneratedOverLengthCodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
