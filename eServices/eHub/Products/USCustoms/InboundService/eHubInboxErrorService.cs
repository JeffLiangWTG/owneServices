using System;
using System.Data.SqlClient;
using CargoWise.eServices.USCustoms.Common;
using ServiceBroker.Interface;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public class eHubInboxErrorService : Service
	{
		public eHubInboxErrorService(SqlConnection connection, SqlTransaction transaction, bool isProd)
			: base(isProd ? ProdServiceName : TestServiceName, connection, transaction)
		{
			this.isProd = isProd;
		}

		static readonly string ProdServiceName = ServiceBrokerConstants.eHubInboxErrorServiceConstants.ServiceName;
		static readonly string TestServiceName = ServiceBrokerConstants.eHubInboxErrorServiceTestConstants.ServiceName;

		public override Guid ServiceHandle
		{
			get { return isProd ? serviceHandle : serviceHandleTest; }
		}

		static readonly Guid serviceHandle = new Guid("734975F1-D611-47BE-A720-C87B76983985");
		static readonly Guid serviceHandleTest = new Guid("F0706472-8D16-4607-8BDD-9E3C38CF88F7");
		private readonly bool isProd;
	}
}
