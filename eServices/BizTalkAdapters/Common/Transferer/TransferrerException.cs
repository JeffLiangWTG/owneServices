using System;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	[Serializable]
	[ExcludeFromCodeCoverageAttribute]
	public class TransferrerException : Exception
	{
		public TransferrerException() { }

		public TransferrerException(string message) : base(message) { }

		public TransferrerException(string message, Exception inner) : base(message, inner) { }

		public TransferrerException(Exception inner) : base("Exception thrown by transport protocol", inner) { }

		protected TransferrerException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
