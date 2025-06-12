using System;
using System.ServiceModel;
using CargoWise.eServices.eHub.Common.EntityModel;
using CargoWise.eServices.eHub.Common.TestCase.eHubReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Common.TestCase
{
	public abstract class TestCaseWithService
	{
		[TestInitialize]
		public void SetUp()
		{
			eHubClient.Open();
			Factory = new EntityFactory(ConnectionString);
		}

		[TestCleanup]
		public void TearDown()
		{
			if (eHubClient.State != CommunicationState.Closed) eHubClient.Close();
			Factory.Dispose();
		}

		public WcfService_CargoWise_eServices_eHub_RoutingClient eHubClient
		{
			get
			{
				return client ?? (client = new WcfService_CargoWise_eServices_eHub_RoutingClient(
					new WSHttpBinding() { MaxReceivedMessageSize = 1048576, SendTimeout = new TimeSpan(0, 0, 60) }, new EndpointAddress(ServiceURL)));
			}
		}
		WcfService_CargoWise_eServices_eHub_RoutingClient client;

		public virtual string ServiceURL
		{
			get { return "http://syd-wpat-1.corporate.cargowise.com/CargoWise.eHub/WcfService_CargoWise_eServices_eHub_Routing.svc?wsdl"; }
		}

		protected EntityFactory Factory { get; set; }
		protected abstract string ConnectionString { get; }
	}
}
