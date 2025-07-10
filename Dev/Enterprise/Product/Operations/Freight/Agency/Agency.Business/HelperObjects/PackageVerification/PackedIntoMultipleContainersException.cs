using System;

namespace Enterprise.Freight.Agency.Business
{
	[Serializable]
	public class PackedIntoMultipleContainersException : Exception
	{
		public PackedIntoMultipleContainersException() { }

#if NETFRAMEWORK
		protected PackedIntoMultipleContainersException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
