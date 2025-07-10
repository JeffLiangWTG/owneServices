using System;

namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	[Serializable]
	public class NonUniqueAllocationCodeException : Exception
	{
		public NonUniqueAllocationCodeException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected NonUniqueAllocationCodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
