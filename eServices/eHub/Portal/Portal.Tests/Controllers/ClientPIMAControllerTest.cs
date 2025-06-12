using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common.Logging;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Portal.Tests.Controllers
{
    [TestClass]
    public class ClientPIMAControllerTest : BaseControllerTest<ClientPIMAController>
    {
        [TestMethod]
        public void TestIndex()
        {
            var client = context.eHubClients.Where(c => c.CC_ID == "TEST0001").FirstOrDefault();
            var airConnections = context.eHubAirConnections.Where(ac => ac.AC_CC_Client == client.CC_PK);
            var airConnectionsPerBranch = context.eHubAirConnectionPerBranches.Where(ac => ac.AB_CC_Client == client.CC_PK);

            ActionResult result = controller.Index(client.CC_PK);
            Assert.IsNotNull(result);

            ViewResult view = result as ViewResult;
            ClientPIMAView clientPimaView = view.Model as ClientPIMAView;
            Assert.IsNotNull(clientPimaView);
            Assert.AreEqual(airConnections.Count() + airConnectionsPerBranch.Count(), clientPimaView.ServiceProviderPIMAView.Count + clientPimaView.ServiceProviderPIMAViewTest.Count);
        }

        [TestMethod]
        public void TestEditGet()
        {
            var client = context.eHubClients.Where(c => c.CC_ID == "TEST0001").FirstOrDefault();
            var serviceProviders = context.eHubClients.Where(c => c.CC_IsAirServiceProvider == true);
            var airConnectionsPerBranch = context.eHubAirConnectionPerBranches.Where(ac => ac.AB_CC_Client == client.CC_PK);

            ActionResult result = controller.Edit(client.CC_PK);
            Assert.IsNotNull(result);

            ViewResult view = result as ViewResult;
            ClientPIMAView clientPimaView = view.Model as ClientPIMAView;
            Assert.IsNotNull(clientPimaView);
            Assert.AreEqual(serviceProviders.Count() + airConnectionsPerBranch.Count(), clientPimaView.ServiceProviderPIMAView.Count + clientPimaView.ServiceProviderPIMAViewTest.Count);
        }

        [TestMethod]
        public void TestAjaxUpdate()
        {
            var logger = new TestLogger();
            const string UPDATED = "UPDATED";
            var client = context.eHubClients.Where(c => c.CC_ID == "TEST0001").FirstOrDefault();

            ClientPIMAView clientPimaView = (controller.Edit(client.CC_PK) as ViewResult).Model as ClientPIMAView;
            Assert.IsNotNull(clientPimaView);

            Dictionary<string, List<ServiceProviderPIMAView>> ajaxData = new Dictionary<string, List<ServiceProviderPIMAView>>();

            foreach (ServiceProviderPIMAView serviceProviderPimaView in clientPimaView.ServiceProviderPIMAView)
            {
                if (serviceProviderPimaView.IsDefault && string.IsNullOrEmpty(serviceProviderPimaView.PIMA) && 
                    string.IsNullOrEmpty(serviceProviderPimaView.Password)) continue;

                if (!ajaxData.ContainsKey(serviceProviderPimaView.ServiceProviderId.ToString()))
                {
                    ajaxData.Add(serviceProviderPimaView.ServiceProviderId.ToString(), new List<ServiceProviderPIMAView>());
                    
                }

                var spPimaView = new ServiceProviderPIMAView
                {
                    ServiceProviderId = serviceProviderPimaView.ServiceProviderId,
                    ServiceProviderName = serviceProviderPimaView.ServiceProviderName,
                    IATA = serviceProviderPimaView.IATA,
                    PIMA = serviceProviderPimaView.PIMA + UPDATED,
                    Password = serviceProviderPimaView.Password + UPDATED,
                    IsDefault = serviceProviderPimaView.IsDefault
                };

                if (!spPimaView.IsDefault)
                {
                    spPimaView.IATA += UPDATED;
                }

                ajaxData[serviceProviderPimaView.ServiceProviderId.ToString()].Add(spPimaView);
            }

            JsonResult result = UpdateTest(client.CC_PK, ajaxData, logger) as JsonResult;
            Assert.IsNotNull(result);
            Dictionary<string, object> data = result.Data as Dictionary<string, object>;
            Assert.IsTrue((bool)data["success"]);
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubAirConnectionPerBranch: AB_CC_Client=00000000-aaaa-1111-0000-000000000000, AB_CC_AirServiceProvider=00000000-aaaa-5555-0000-000000000000, AB_IssuingCarrierAgentIATACode=TestIATA1, AB_PIMA=TestPIMAPerBranch1, AB_PASSWORD=TestPasswordPerBranch1"));

            var airConnections = context.eHubAirConnections.Where(ac => ac.AC_CC_Client == client.CC_PK);
            var airConnectionsPerBranch = context.eHubAirConnectionPerBranches.Where(ac => ac.AB_CC_Client == client.CC_PK);

            foreach (eHubAirConnection ac in airConnections)
            {
                Assert.IsTrue(ac.AC_PASSWORD.Contains(UPDATED));
                Assert.IsTrue(ac.AC_PIMA.Contains(UPDATED));
            }

            foreach (eHubAirConnectionPerBranch ac in airConnectionsPerBranch)
            {
                Assert.IsTrue(ac.AB_PASSWORD.Contains(UPDATED));
                Assert.IsTrue(ac.AB_PIMA.Contains(UPDATED));
                Assert.IsTrue(ac.AB_IssuingCarrierAgentIATACode.Contains(UPDATED));
            }
        }

        void addDefaultProvider (Dictionary<string, List<ServiceProviderPIMAView>> ajaxData, List<ServiceProviderPIMAView> serviceProviderList, List<string> providerIds)
        {
            foreach (ServiceProviderPIMAView serviceProviderPimaView in serviceProviderList)
            {
                if (!ajaxData.ContainsKey(serviceProviderPimaView.ServiceProviderId.ToString()))
                {
                    ajaxData.Add(serviceProviderPimaView.ServiceProviderId.ToString(), new List<ServiceProviderPIMAView>());

                }

                var spPimaView = new ServiceProviderPIMAView
                {
                    ServiceProviderId = serviceProviderPimaView.ServiceProviderId,
                    ServiceProviderName = serviceProviderPimaView.ServiceProviderName,
                    IATA = serviceProviderPimaView.IATA,
                    PIMA = serviceProviderPimaView.PIMA,
                    Password = serviceProviderPimaView.Password,
                    IsDefault = serviceProviderPimaView.IsDefault
                };

                ajaxData[serviceProviderPimaView.ServiceProviderId.ToString()].Add(spPimaView);

                if (serviceProviderPimaView.IsDefault && string.IsNullOrEmpty(serviceProviderPimaView.PIMA) &&
                    string.IsNullOrEmpty(serviceProviderPimaView.Password))
                {
                    providerIds.Add(spPimaView.ServiceProviderId.ToString());
                    spPimaView.IsDefault = true;
                    spPimaView.IATA = "Default";
                    spPimaView.PIMA = "Default";
                    spPimaView.Password = "Password1";
                }
            }
        }

        [TestMethod]
        public void TestAjaxCreateDefault()
        {
            var logger = new TestLogger();
            var client = context.eHubClients.Where(c => c.CC_ID == "TEST0001").FirstOrDefault();

            ClientPIMAView clientPimaView = (controller.Edit(client.CC_PK) as ViewResult).Model as ClientPIMAView;
            Assert.IsNotNull(clientPimaView);

            var airConnections = context.eHubAirConnections.Where(ac => ac.AC_CC_Client == client.CC_PK);

            int initialCount = airConnections.Count();

            Dictionary<string, List<ServiceProviderPIMAView>> ajaxData = new Dictionary<string, List<ServiceProviderPIMAView>>();

            List<string> providerIds = new List<string>();

            addDefaultProvider(ajaxData, clientPimaView.ServiceProviderPIMAView, providerIds);
            addDefaultProvider(ajaxData, clientPimaView.ServiceProviderPIMAViewTest, providerIds);
            
            JsonResult result = UpdateTest(client.CC_PK, ajaxData, logger) as JsonResult;
            Assert.IsNotNull(result);
            Dictionary<string, object> data = result.Data as Dictionary<string, object>;
            Assert.IsTrue((bool)data["success"]);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubAirConnection: AB_CC_Client=00000000-aaaa-1111-0000-000000000000, AB_CC_AirServiceProvider=00000000-aaaa-3333-0000-000000000000, AC_PIMA=Default, AB_PASSWORD=Password1"));

            Assert.AreEqual(initialCount + 1, airConnections.Count());

            foreach (string providerId in providerIds)
            {
                Assert.IsTrue(airConnections.Where(ac => ac.AC_CC_AirServiceProvider == new Guid(providerId)).Count() > 0);
            }
        }

        void addProviderDetailsToAjaxData(Dictionary<string, List<ServiceProviderPIMAView>> ajaxData, List<ServiceProviderPIMAView> providerList)
        {
            foreach (ServiceProviderPIMAView serviceProviderPimaView in providerList)
            {
                if (serviceProviderPimaView.IsDefault && string.IsNullOrEmpty(serviceProviderPimaView.PIMA) &&
                    string.IsNullOrEmpty(serviceProviderPimaView.Password)) continue;

                if (!ajaxData.ContainsKey(serviceProviderPimaView.ServiceProviderId.ToString()))
                {
                    ajaxData.Add(serviceProviderPimaView.ServiceProviderId.ToString(), new List<ServiceProviderPIMAView>());
                }

                var spPimaView = new ServiceProviderPIMAView
                {
                    ServiceProviderId = serviceProviderPimaView.ServiceProviderId,
                    ServiceProviderName = serviceProviderPimaView.ServiceProviderName,
                    IATA = serviceProviderPimaView.IATA,
                    PIMA = serviceProviderPimaView.PIMA,
                    Password = serviceProviderPimaView.Password,
                    IsDefault = serviceProviderPimaView.IsDefault
                };

                ajaxData[serviceProviderPimaView.ServiceProviderId.ToString()].Add(spPimaView);
            }
        }

        [TestMethod]
        public void TestAjaxCreateNonDefault()
        {
            var logger = new TestLogger();
            const string CREATED = "CREATED";
            var client = context.eHubClients.Where(c => c.CC_ID == "TEST0001").FirstOrDefault();

            ClientPIMAView clientPimaView = (controller.Edit(client.CC_PK) as ViewResult).Model as ClientPIMAView;
            Assert.IsNotNull(clientPimaView);

            var airConnections = context.eHubAirConnectionPerBranches.Where(ac => ac.AB_CC_Client == client.CC_PK);

            int initialCount = airConnections.Count();

            Dictionary<string, List<ServiceProviderPIMAView>> ajaxData = new Dictionary<string, List<ServiceProviderPIMAView>>();

            addProviderDetailsToAjaxData(ajaxData, clientPimaView.ServiceProviderPIMAView);
            addProviderDetailsToAjaxData(ajaxData, clientPimaView.ServiceProviderPIMAViewTest);

            var first = ajaxData.First();
            first.Value.Add(new ServiceProviderPIMAView
            {
                ServiceProviderId = first.Value.First().ServiceProviderId,
                ServiceProviderName = first.Value.First().ServiceProviderName,
                IATA = CREATED,
                PIMA = CREATED,
                Password = CREATED
            });

            JsonResult result = UpdateTest(client.CC_PK, ajaxData, logger) as JsonResult;
            Assert.IsNotNull(result);
            Dictionary<string, object> data = result.Data as Dictionary<string, object>;
            Assert.IsTrue((bool)data["success"]);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubAirConnectionPerBranch: AB_CC_Client=00000000-aaaa-1111-0000-000000000000, AB_CC_AirServiceProvider=00000000-aaaa-2222-0000-000000000000, AB_IssuingCarrierAgentIATACode=TestIATA1, AB_PIMA=TestPIMAPerBranch1, AB_PASSWORD=TestPasswordPerBranch1"));

            Assert.AreEqual(initialCount + 1, airConnections.Count());
            Assert.IsTrue(airConnections.Where(ac => ac.AB_CC_AirServiceProvider == first.Value.First().ServiceProviderId &&
                ac.AB_IssuingCarrierAgentIATACode.Equals(CREATED) && ac.AB_PIMA.Equals(CREATED) && ac.AB_PASSWORD.Equals(CREATED)).Count() > 0);
        }

        protected ActionResult UpdateTest(Guid id, Dictionary<string, List<ServiceProviderPIMAView>> data, ILog logger)
        {
            controller.logger = logger;
            return controller.Update(id, data);
        }
    }
}
