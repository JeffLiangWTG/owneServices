using System;
using System.Collections.Generic;
using CargoWise.Workflow;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	class StmALogWrapper : IStmALogWrapper
	{
		public StmALogWrapper(StmALog log)
		{
			this.log = log ?? throw new ArgumentNullException(nameof(log));
		}

		public string SL_Reference
		{
			get => log.SL_Reference;
			set => log.SL_Reference = value;
		}

		public IDictionary<string, string> Parameters => log.Parameters;

		public DateTimeOffset LocalEventTime => Extensions.GetEventTimeLocal(log).ToDateTimeOffset();

		readonly StmALog log;
	}
}
