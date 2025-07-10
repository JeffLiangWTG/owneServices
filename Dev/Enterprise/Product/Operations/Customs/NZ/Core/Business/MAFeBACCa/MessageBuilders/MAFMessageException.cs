using System;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	[Serializable]
	internal class MAFMessageException : Exception
	{
		internal MAFMessageException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MAFMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
