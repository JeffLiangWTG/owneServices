using System;

namespace Enterprise.Services.ServiceHost
{
	public class GetNextTaskArgs
	{
		public Guid WarehousePK;
		public string TaskReference;
		public string FormFlowType;
		public string LastFormFlowType;
		public Guid[] TasksToIgnore;
	}
}
