using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class ContainerEventRequestListTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			OneStopContainerEventRequestList requestList = OneStopContainerEventRequestList.New();
			AssertEquals(typeof(OneStopContainerEventRequestList), requestList.GetType());
		}

		public void TestFindOrCreateRequest()
		{
			OneStopContainerEventRequest createdRequest = RequestList.FindOrCreateRequest(Container);
			AssertEquals("Creating a new container event request", Container, createdRequest.Container);

			OneStopContainerEventRequest foundRequest = RequestList.FindOrCreateRequest(Container);
			AssertEquals("Finding an existing container event request", Container, foundRequest.Container);

			AssertEquals("Finding an existing request should return the same request", true, foundRequest == createdRequest);
		}

		public void TestRemove()
		{
			OneStopContainerEventRequest request = RequestList.FindOrCreateRequest(Container);
			OneStopContainerEventRequest request2 = RequestList.FindOrCreateRequest(Container2);
			AssertEquals("Contains item 1", true, RequestList.Contains(request));
			AssertEquals("Contains item 2", true, RequestList.Contains(request2));

			RequestList.Remove(request);
			AssertEquals("Removed item 1", false, RequestList.Contains(request));
			AssertEquals("Contains item 2", true, RequestList.Contains(request2));

			RequestList.Remove(request2);
			AssertEquals("Removed item 1", false, RequestList.Contains(request));
			AssertEquals("Removed item 2", false, RequestList.Contains(request2));
		}

		public void TestRemoveUnwantedRequests()
		{
			Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now;
			Container.JC_ContainerNum = "";
			Container2.JC_ContainerNum = "ContainerNum";
			Container3.JC_ContainerNum = "";

			OneStopContainerEventRequest unwantedRequest1 = RequestList.FindOrCreateRequest(Container);
			OneStopContainerEventRequest requiredRequest = RequestList.FindOrCreateRequest(Container2);
			OneStopContainerEventRequest unwantedRequest2 = RequestList.FindOrCreateRequest(Container3);

			RequestList.RemoveUnwantedRequests();
			AssertEquals("Only 1 of the requests is required", 1, RequestList.Count);
			AssertEquals("Unwanted request removed", false, RequestList.Contains(unwantedRequest1));
			AssertEquals("Required request remains", true, RequestList.Contains(requiredRequest));
			AssertEquals("Unwanted request removed", false, RequestList.Contains(unwantedRequest2));
		}

		public void TestCreateMessages_Consol()
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "ABN";

			Consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2005, 1, 1);
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			Container.JC_ContainerNum = "Container1";
			Container2.JC_ContainerNum = "Container2";

			StringWriter writer = new StringWriter();
			RequestList.FindOrCreateRequest(Container);
			RequestList.FindOrCreateRequest(Container2);
			RequestList.CreateMessages();

			AssertCreatedEDIMessageBodies(
				new[]
					{
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','CONTAINER1','" + Container.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','CONTAINER2','" + Container2.PK.ToString().Replace("-", "") + "','NZ'\r\n"
					}, Factory);
		}

		public void TestCreateMessages_Declaration()
		{
			FreightDataRegistry.HasOneStopAU = true;
			FreightDataRegistry.HasOneStopNZ = true;

			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "ABN";

			SetDeclarationOrigin("AUSYD");
			SetDeclarationDestination("NZAKL");
			CommonContainer container1 = NewCusContainer();
			CommonContainer container2 = NewCusContainer();
			SetContainerNumber(container1, "container1");
			SetContainerNumber(container2, "container2");

			StringWriter writer = new StringWriter();
			RequestList.FindOrCreateRequest(container1);
			RequestList.FindOrCreateRequest(container2);
			RequestList.CreateMessages();

			ZString container1PK = container1.PK.ToString().Replace("-", "");
			ZString container2PK = container2.PK.ToString().Replace("-", "");

			AssertCreatedEDIMessageBodies(
				new[]
					{
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','CONTAINER1','" + container1PK + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','CONTAINER1','" + container1PK + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','CONTAINER1','" + container1PK + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','CONTAINER1','" + container1PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','CONTAINER1','" + container1PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','CONTAINER1','" + container1PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','CONTAINER1','" + container1PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','CONTAINER1','" + container1PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','CONTAINER1','" + container1PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEIN','ANY','N','','','CONTAINER2','" + container2PK + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','LOAD','ANY','N','','','CONTAINER2','" + container2PK + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','EXPORT PREADVICE','ANY','N','','','CONTAINER2','" + container2PK + "','AU'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','GATEOUT','ANY','N','','','CONTAINER2','" + container2PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DISCHARGE','ANY','N','','','CONTAINER2','" + container2PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPORT PREADVICE','ANY','N','','','CONTAINER2','" + container2PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','DEHIRE','ANY','N','','','CONTAINER2','" + container2PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','STORAGESTART','ANY','N','','','CONTAINER2','" + container2PK + "','NZ'\r\n",
						"'41065894724','EDalerts','eda1ert5','Default@edi.com.au','CONTAINER','IMPAVAILABLE','ANY','N','','','CONTAINER2','" + container2PK + "','NZ'\r\n"
					}, Factory);
		}

		public static void AssertCreatedEDIMessageBodies(string[] expectedBodies, BusinessObjectFactory factory)
		{
			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(factory);
			collection.Load();

			var collectionEnumerable = collection.Cast<EDIMessage>();
			AssertContainsExactElementsInAnyOrder(expectedBodies.Select(x => x.Replace("'", "\"")), collectionEnumerable.Select(x => ((string)x.EM_MessageText)));

			foreach (var message in collectionEnumerable)
			{
				AssertEquals(ApplicationCodeList.Codes.ComTrac, message.EM_ApplicationCode);
				AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			}
		}

		#region Implementation

		readonly OneStopContainerEventRequestList RequestList = OneStopContainerEventRequestList.New();

		#region Consol

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
				}
				return consol;
			}
		}
		CommonConsol consol;

		CommonContainer Container
		{
			get
			{
				if (container == null)
				{
					container = Consol.Containers.AddNew();
					container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				}
				return container;
			}
		}
		CommonContainer container;

		CommonContainer Container2
		{
			get
			{
				if (container2 == null)
				{
					container2 = Consol.Containers.AddNew();
					container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				}
				return container2;
			}
		}
		CommonContainer container2;

		CommonContainer Container3
		{
			get
			{
				if (container3 == null)
				{
					container3 = Consol.Containers.AddNew();
					container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
				}
				return container3;
			}
		}
		CommonContainer container3;

		#endregion

		#region Declaration

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		void SetDeclarationOrigin(ZString origin)
		{
			Declaration[JobDeclarationSchema.JE_RL_NKOrigin] = origin;
		}

		void SetDeclarationDestination(ZString destination)
		{
			Declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = destination;
		}

		CommonContainer NewCusContainer()
		{
			BusinessObjectCollection containers = (BusinessObjectCollection)Declaration["CusContainers"];
			BusinessObject customsContainer = containers.AddNew();
			customsContainer[CusContainerSchema.CO_FCL_LCL_AIR] = Core.Constants.ContainerModes.FCL;
			CommonContainer container = (CommonContainer)customsContainer["JobContainer"];
			return container;
		}

		void SetContainerNumber(CommonContainer container, ZString containerNum)
		{
			container.JC_ContainerNum = containerNum;
			ZQuery query = new ZQuery(CusContainerSchema.CO_JC, container.PK);
			BusinessObject customsContainer = (BusinessObject)container.Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
			customsContainer[CusContainerSchema.CO_ContainerNumber] = containerNum;
		}

		#endregion

		#endregion
	}
}
