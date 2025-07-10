using System;

namespace Enterprise.Services.ServiceHost
{
	public class CreateAdHocServiceJobArgs
	{
		public Guid JobPK;
		public Guid  ClientPK;
		public Guid WarehousePK;
		public DateTime BillingDate;
		public string CustomerReference;
	}
}
