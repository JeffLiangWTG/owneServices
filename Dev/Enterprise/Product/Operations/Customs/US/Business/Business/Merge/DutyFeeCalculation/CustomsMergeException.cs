using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	[Serializable]
	public class CustomsMergeException : ZException
	{
		public CustomsMergeException(string message) : base(message) { }

#if NETFRAMEWORK
		protected CustomsMergeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
