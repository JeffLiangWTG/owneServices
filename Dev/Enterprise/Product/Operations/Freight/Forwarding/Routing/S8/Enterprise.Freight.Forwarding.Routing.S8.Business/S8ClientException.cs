namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	using System;

	[Serializable]
	public class S8ClientException : Exception
	{
		public S8ClientException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public S8ClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
