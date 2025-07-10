using System;
using CargoWise.Types;
using CargoWise.Workflow;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskDateTimeProvider : IProcessTaskDateTimeProvider
	{
		public DateTime Now => ZDateTime.Now.ToDateTime();
		public DateTimeOffset NowOffset => ZDateTimeOffset.Now.ToDateTimeOffset();
	}
}
