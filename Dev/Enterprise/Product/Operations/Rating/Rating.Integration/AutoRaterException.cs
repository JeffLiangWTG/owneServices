using System;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Integration
{
	[Serializable]
	public class AutoRaterException : ZException
	{
		public AutoRaterException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected AutoRaterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}