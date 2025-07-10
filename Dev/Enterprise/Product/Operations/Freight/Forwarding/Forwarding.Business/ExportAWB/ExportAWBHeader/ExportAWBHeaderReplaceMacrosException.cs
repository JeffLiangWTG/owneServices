using System;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[Serializable]
	public class ExportAWBHeaderReplaceMacrosException : Exception
	{
		public ExportAWBHeaderReplaceMacrosException(string message, Exception ex) : base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected ExportAWBHeaderReplaceMacrosException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
