using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ContextTypes = Enterprise.UniversalDataBuss.DataObjects.Universal.Event.ContextTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public abstract class BaseShipmentDataContextManagerTest<TDataContextManager, TCommonShipment> : ShipmentDataContextManagerTestCase<TDataContextManager, TCommonShipment>
			where TCommonShipment : CommonShipment
			where TDataContextManager : BaseShipmentDataContextManager<TCommonShipment>, new()
	{
		public void TestOnUniversalEventAdded_ChangeOfIdentifier()
		{
			var eventAdded = new UniversalEvent
			{
				EventType = AutoEvents.ChangeOfIdentifierCode,
				EventParameters = new EventParameters { New = "CCCC", Old = "AAAA", ReferenceNumber = "ZZZZ", Type = Constants.EventReferenceParameterTypes.ContainerID },
				ContextCollection = new List<Context> { new Context { Type = nameof(ContextTypes.ContainerISOCode), Value = "22G0" } }
			};

			var shipment = Factory.New<TCommonShipment>();
			var consol = shipment.Consols.AddNew();

			Func<string, string, string, CommonContainer> getNewContainer = (containerNum, releaseNum, containerType) =>
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = containerNum;
				container.JC_ReleaseNum = releaseNum;
				container.JC_RC = new RefContainer.Loader(consol.Factory).LoadFromCode(containerType).PK;
				return container;
			};

			var container1 = getNewContainer("AAAA", ZString.Empty, "20GP");
			var container2 = getNewContainer("BBBB", ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);

			Factory.SaveForTesting();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("OLD container exists, Replace OLD with NEW", "CCCC", container1.JC_ContainerNum);
			AssertContainsExactElementsInAnyOrder("Consol containers.", new[] { container1, container2 }, consol.Containers);

			shipment = Factory.NewWithValidTestData<TCommonShipment>();
			consol = shipment.Consols.AddNew();
			container1 = getNewContainer(ZString.Empty, "ZZZZ", "20GP");
			container2 = getNewContainer(ZString.Empty, ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);

			Factory.SaveForTesting();

			manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			serviceLogger = new ServiceTaskLogForTesting();
			messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("Container updated", "CCCC", container1.JC_ContainerNum);
			AssertEquals("Container count is the same", 2, consol.Containers.Count);
			AssertEquals("Container RefNum updated", "ZZZZ", container1.JC_ReleaseNum);
			AssertContainsExactElementsInAnyOrder("Consol containers.", new[] { container1, container2 }, consol.Containers);

			shipment = Factory.NewWithValidTestData<TCommonShipment>();
			consol = shipment.Consols.AddNew();
			container1 = getNewContainer("AAAA", ZString.Empty, "20GP");
			container2 = getNewContainer("CCCC", ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 5;
			packLine1.SetContainer(container1.PK);

			Factory.SaveForTesting();

			manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			serviceLogger = new ServiceTaskLogForTesting();
			messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("NEW container exists, pack from OLD container to NEW container.", container2.PK, packLine1.GetContainer(consol).PK);
			AssertContainsExactElementsInAnyOrder("Container1 deleted.", new[] { container2 }, consol.Containers);

			shipment = Factory.NewWithValidTestData<TCommonShipment>();
			consol = shipment.Consols.AddNew();
			container1 = getNewContainer("DDDD", ZString.Empty, "20GP");
			container2 = getNewContainer("EEEE", ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);

			Factory.SaveForTesting();

			manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			serviceLogger = new ServiceTaskLogForTesting();
			messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("OLD and NEW container number does not exist, Container with NEW number added.", 3, consol.Containers.Count);
			AssertContainsExactElementsInAnyOrder("Consol container numbers.", new ZString[] { "CCCC", "DDDD", "EEEE" }, consol.Containers.Cast<CommonContainer>().Select(x => x.JC_ContainerNum));

			shipment = Factory.NewWithValidTestData<TCommonShipment>();
			consol = shipment.Consols.AddNew();
			container1 = getNewContainer("DDDD", ZString.Empty, "20GP");
			container2 = getNewContainer(ZString.Empty, ZString.Empty, "20GP");
			container2.JC_ContainerCount = 5;
			var container3 = getNewContainer(ZString.Empty, ZString.Empty, "40GP");
			container3.JC_ContainerCount = 3;
			consol.Shipments.Add(shipment);

			Factory.SaveForTesting();

			manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			serviceLogger = new ServiceTaskLogForTesting();
			messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("OLD and NEW container number does not exist, new conatiner will be created", 4, consol.Containers.Count);
			AssertEquals("Container count reduced", (short)4, container2.JC_ContainerCount);
			AssertContainsExactElementsInAnyOrder("Consol container numbers.", new ZString[] { "CCCC", "DDDD", ZString.Empty, ZString.Empty }, consol.Containers.Cast<CommonContainer>().Select(x => x.JC_ContainerNum));

			eventAdded = new UniversalEvent { EventType = AutoEvents.ChangeOfIdentifierCode, EventParameters = new EventParameters { New = "CCCC", Type = Constants.EventReferenceParameterTypes.ContainerID } };

			shipment = Factory.NewWithValidTestData<TCommonShipment>();
			consol = shipment.Consols.AddNew();
			container1 = getNewContainer("CCCC", ZString.Empty, "20GP");
			container2 = getNewContainer("EEEE", ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);

			Factory.SaveForTesting();

			manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			serviceLogger = new ServiceTaskLogForTesting();
			messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("OLD is empty and NEW exits, no change", 2, consol.Containers.Count);
			AssertContainsExactElementsInAnyOrder("Consol containers.", new[] { container1, container2 }, consol.Containers);

			using (eventAdded = new UniversalEvent { EventType = AutoEvents.ChangeOfIdentifierCode, EventParameters = new EventParameters { New = "CCCC", ReferenceNumber = "TTTT", Type = Constants.EventReferenceParameterTypes.ContainerID } })
			{
				shipment = Factory.NewWithValidTestData<TCommonShipment>();
				consol = shipment.Consols.AddNew();
				container1 = getNewContainer("CCCC", "TTTT", "20GP");
				consol.Shipments.Add(shipment);

				Factory.SaveForTesting();

				manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
				serviceLogger = new ServiceTaskLogForTesting();
				messageLogger = new XmlSessionTracker(serviceLogger);
				manager.OnUniversalEventAdded(messageLogger, eventAdded);

				AssertEquals("OLD and NEW match, no change", 1, consol.Containers.Count);
				AssertContainsExactElementsInAnyOrder("Consol containers.", new[] { container1 }, consol.Containers);
			}
		}

		public void TestOnUniversalEventAdded_ChangeOfIdentifier_UpdateOldCusContainer()
		{
			var eventAdded = new UniversalEvent
			{
				EventType = AutoEvents.ChangeOfIdentifierCode,
				EventParameters = new EventParameters { New = "CCCC", Old = "AAAA", ReferenceNumber = "ZZZZ", Type = Constants.EventReferenceParameterTypes.ContainerID },
				ContextCollection = new List<Context> { new Context { Type = nameof(ContextTypes.ContainerISOCode), Value = "22G0" } }
			};

			var shipment = Factory.New<TCommonShipment>();
			var consol = shipment.Consols.AddNew();

			Func<string, string, string, CommonContainer> getNewContainer = (containerNum, releaseNum, containerType) =>
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = containerNum;
				container.JC_ReleaseNum = releaseNum;
				container.JC_RC = new RefContainer.Loader(consol.Factory).LoadFromCode(containerType).PK;
				return container;
			};

			var declaration = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var container1 = getNewContainer("AAAA", ZString.Empty, "20GP");
			var container2 = getNewContainer("CCCC", ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 5;
			packLine1.SetContainer(container1.PK);

			var cusContainer = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = container1.PK;

			Factory.SaveForTesting();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			CombineAssertions(() =>
			{
				AssertEquals("NEW container exists, update JC_ContainerNum", "AAAA", container2.JC_ContainerNum);
				AssertContainsExactElementsInAnyOrder("Container1 deleted.", new[] { container2 }, consol.Containers);
				AssertEquals("cusContainer link to new Conainer", container2.PK, cusContainer[CusContainerSchema.CO_JC.Name]);
			});
		}

		public void TestOnUniversalEventAdded_ChangeOfIdentifier_DeleteOldCusContainer()
		{
			var eventAdded = new UniversalEvent
			{
				EventType = AutoEvents.ChangeOfIdentifierCode,
				EventParameters = new EventParameters { New = "CCCC", Old = "AAAA", ReferenceNumber = "ZZZZ", Type = Constants.EventReferenceParameterTypes.ContainerID },
				ContextCollection = new List<Context> { new Context { Type = nameof(ContextTypes.ContainerISOCode), Value = "22G0" } }
			};

			var shipment = Factory.New<TCommonShipment>();
			var consol = shipment.Consols.AddNew();

			Func<string, string, string, CommonContainer> getNewContainer = (containerNum, releaseNum, containerType) =>
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = containerNum;
				container.JC_ReleaseNum = releaseNum;
				container.JC_RC = new RefContainer.Loader(consol.Factory).LoadFromCode(containerType).PK;
				return container;
			};

			var declaration = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var container1 = getNewContainer("AAAA", ZString.Empty, "20GP");
			var container2 = getNewContainer("CCCC", ZString.Empty, "20GP");
			consol.Shipments.Add(shipment);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 5;
			packLine1.SetContainer(container1.PK);

			var cusContainer = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC.Name] = container1.PK;

			var cusContainer2 = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer2[CusContainerSchema.CO_JE.Name] = declaration.PK;
			cusContainer2[CusContainerSchema.CO_JC.Name] = container2.PK;

			Factory.SaveForTesting();

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			CombineAssertions(() =>
			{
				AssertEquals("NEW container exists, update JC_ContainerNum", "AAAA", container2.JC_ContainerNum);
				AssertContainsExactElementsInAnyOrder("container1 deleted.", new[] { container2 }, consol.Containers);
				Assert("cusContainer deleted", cusContainer.IsDeleted);
				AssertEquals("cusContainer2 do not change", container2.PK, cusContainer2[CusContainerSchema.CO_JC.Name]);
			});
		}

		public void TestOnUniversalEventAdded_UpdateShipmentPacklinesImportRefNumber()
		{
			var shipment = Factory.New<TCommonShipment>();
			var consol1 = shipment.Consols.AddNew();

			consol1.JK_TransportMode = "AIR";
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C00000069";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "FRMRS";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_UniqueConsignRef = "C00000070";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "DRY0001";
			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "DRY0002";

			var container3 = consol2.Containers.AddNew();
			container3.JC_ContainerNum = "DRY0003";

			shipment.JS_UniqueConsignRef = "S0001000";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = container1.PK;
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_JC = container2.PK;
			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_JC = container3.PK;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Pre-condition: Import Reference Number is not set.", packline1.JL_ImportRefNumber);
				AssertNullOrEmpty("Pre-condition: Import Reference Number is not set.", packline2.JL_ImportRefNumber);
				AssertNullOrEmpty("Pre-condition: Import Reference Number is not set.", packline3.JL_ImportRefNumber);
			});

			var eventAdded = new UniversalEvent
			{
				EventType = AutoEvents.StatusUpdatedCode,
				EventParameters = new EventParameters { Type = Constants.EventReferenceParameterTypes.LPDNotification, EquipmentReferenceNumber = "DRY0001", CustomsReferenceNumber = "refnumber" }
			};

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, eventAdded);

			AssertEquals("Import Reference Number is set.", "refnumber", packline1.JL_ImportRefNumber);
			AssertNullOrEmpty("Import Reference Number is not set.", packline2.JL_ImportRefNumber);
			AssertNullOrEmpty("Import Reference Number is not set.", packline3.JL_ImportRefNumber);
		}

		public void TestOnUniversalEventAdded_UpdateShipmentPacklinesImportRefNumber_WithoutEventParametersDoesNotCrash()
		{
			var shipment = Factory.New<TCommonShipment>();
			var consol1 = shipment.Consols.AddNew();

			consol1.JK_TransportMode = "AIR";
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C00000069";

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "FRMRS";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_UniqueConsignRef = "C00000070";

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "DRY0001";
			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "DRY0002";

			var container3 = consol2.Containers.AddNew();
			container3.JC_ContainerNum = "DRY0003";

			shipment.JS_UniqueConsignRef = "S0001000";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_JC = container1.PK;
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_JC = container2.PK;
			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_JC = container3.PK;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Pre-condition: Import Reference Number is not set.", packline1.JL_ImportRefNumber);
				AssertNullOrEmpty("Pre-condition: Import Reference Number is not set.", packline2.JL_ImportRefNumber);
				AssertNullOrEmpty("Pre-condition: Import Reference Number is not set.", packline3.JL_ImportRefNumber);
			});

			var eventAdded = new UniversalEvent
			{
				EventType = AutoEvents.StatusUpdatedCode,
			};

			var manager = shipment.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);

			AssertNoExceptionThrown(() =>
			{
				manager.OnUniversalEventAdded(messageLogger, eventAdded);
			});

			CombineAssertions("Import Reference Number is not set because no event parameters caused the logic not to run.", () =>
			{
				AssertNullOrEmpty(packline1.JL_ImportRefNumber);
				AssertNullOrEmpty(packline2.JL_ImportRefNumber);
				AssertNullOrEmpty(packline3.JL_ImportRefNumber);
			});
		}
	}
}
