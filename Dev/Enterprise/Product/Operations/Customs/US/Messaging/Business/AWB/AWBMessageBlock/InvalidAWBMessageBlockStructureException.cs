using System;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB
{
	[Serializable]
	public class InvalidAWBMessageBlockStructureException : Exception
	{
		public InvalidAWBMessageBlockStructureException(string message)
			: base(message)
		{ }

		public InvalidAWBMessageBlockStructureException(string message, Exception ex)
			: base(message, ex)
		{ }

#if NETFRAMEWORK
		protected InvalidAWBMessageBlockStructureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
