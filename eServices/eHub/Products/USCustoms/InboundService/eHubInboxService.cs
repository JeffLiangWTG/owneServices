using System;
using System.Data.SqlClient;
using CargoWise.eServices.USCustoms.Common;
using ServiceBroker.Interface;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public class eHubInboxService : Service
	{
		public eHubInboxService(SqlConnection connection, SqlTransaction transaction, bool isProd)
			: base(isProd ? ProdServiceName : TestServiceName, connection, transaction)
		{
			this.isProd = isProd;
		}

		static readonly string ProdServiceName = ServiceBrokerConstants.eHubInboxServiceConstants.ServiceName;
		static readonly string TestServiceName = ServiceBrokerConstants.eHubInboxServiceTestConstants.ServiceName;

		public override Guid ServiceHandle
		{
			get { return isProd ? serviceHandle : serviceHandleTest; }
		}

		static readonly Guid serviceHandle = new Guid("E9C75239-EDF4-4119-BF85-DE0A904C46BC");
		static readonly Guid serviceHandleTest = new Guid("51868DD4-5D98-44B2-8646-E312CF2868E4");
		private readonly bool isProd;
	}
}
