using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public class WorkflowMacroEvaluationException : Exception
	{
		public WorkflowMacroEvaluationException(MultilingualString message, Exception innerException) : base(message, innerException)
		{
			MultilingualMessage = message;
		}

		public MultilingualString MultilingualMessage { get; }

#if NETFRAMEWORK
		protected WorkflowMacroEvaluationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
