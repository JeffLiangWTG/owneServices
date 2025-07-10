using System;

namespace Enterprise.ProcessManagement.Business
{
	[Serializable]
	public class JiraRequestException : Exception
	{
		public JiraRequestException(JiraResult result)
		{
			ResultThatCausedException = result;
		}

#if NETFRAMEWORK
		public JiraRequestException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public JiraResult ResultThatCausedException { get; }
	}
}
