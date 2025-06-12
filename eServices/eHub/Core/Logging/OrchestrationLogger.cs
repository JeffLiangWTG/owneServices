using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Common.Logging;

namespace CargoWise.eHub.Core.Logging
{
	[Serializable]
	public class OrchestrationLogger : ISerializable
	{
		public ILog Log { get; private set; }
		internal string OrchestrationName { get; private set; }

		public OrchestrationLogger() : this(new StackFrame(1, false).GetMethod().DeclaringType.FullName)
		{
		}

		internal OrchestrationLogger(string orchestrationName)
		{
			this.OrchestrationName = orchestrationName;
			this.Log = LoggerHelpers.GetLogger(this.OrchestrationName, null, LogType.Orchestration);
		}

		public OrchestrationLogger(SerializationInfo info, StreamingContext context)
		{
			this.OrchestrationName = (string)info.GetValue("OrchestrationName", typeof(string));
			this.Log = LoggerHelpers.GetLogger(this.OrchestrationName, null, LogType.Orchestration);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("OrchestrationName", this.OrchestrationName, typeof(string));
		}
	}
}
