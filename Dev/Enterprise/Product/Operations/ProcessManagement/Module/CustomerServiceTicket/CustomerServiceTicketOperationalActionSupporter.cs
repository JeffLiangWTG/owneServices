using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.ProcessManagement.Module
{
	public class CustomerServiceTicketOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WorkRequest; }
		}

		public override Type RootType
		{
			get { return typeof(WorkRequest); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.CustomerServiceTicket;
	}
}
