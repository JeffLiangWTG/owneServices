using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ContainerAutomationDeclarationSubscriptionUpdater))]
	class ContainerAutomationDeclarationSubscriptionUpdaterTest : ContainerAutomationSubscriptionUpdaterTest<ContainerAutomationDeclarationSubscriptionUpdater>
	{
		public void TestProcessLogs_UserDepartmentAndBranchAreSerialized_Declaration()
		{
			ProcessLogs_UserDepartmentAndBranchAreSerialized(() => CreateDeclaration().Declaration);
		}

		public void TestProcessLogs_DontAddCoLoadAttributesWithStringEmptyValue()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var (declaration, containers) = CreateDeclaration();

				Factory.Save();

				var logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should create new SBR event", 1, logs.Count);

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "CoLoadBookingReference").Count());
				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "CoLoadBillNumber").Count());
				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "CoLoadWithName").Count());
				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "CoLoadWithCode").Count());
				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "CoLoadWithC1CCode").Count());
				AssertGreaterThan(xmlEvent.AdditionalContextCollection.Count, 0);
			}
		}

		public void TestProcessLogs_DontAddConsolTypeAttribute()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var (declaration, containers) = CreateDeclaration();

				Factory.Save();

				var logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should create new SBR event", 1, logs.Count);

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertEquals(0, xmlEvent.ContextCollection.Where(c => c.Type == "ConsolType").Count());
			}
		}

		public void TestProcessLogs_AddAcceptsOnlyProvidedContainersFlag_True()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var (declaration, containers) = CreateDeclaration();

				Factory.Save();

				var logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should create new SBR event", 1, logs.Count);

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertEquals(1, xmlEvent.ContextCollection.Where(c => c.Type == "AcceptsOnlyProvidedContainers").Count());
				AssertEquals(
					"AcceptsOnlyProvidedContainers set to 'True' for standalone delcaration",
					"True",
					xmlEvent.ContextCollection.Single(c => c.Type == "AcceptsOnlyProvidedContainers").Value.Value);

				AssertGreaterThan(xmlEvent.AdditionalContextCollection.Count, 0);
				AssertEquals(containers.Count, xmlEvent.AdditionalContextCollection.Count);
				AssertArrayEqualsByElements(
					"Container numbers are correct",
					containers.Select(container => container.JC_ContainerNum).ToArray(),
					xmlEvent.AdditionalContextCollection
						.Select(additionalContext => additionalContext
							.ContextCollection
							.Single(context => context.Type == "ContainerNumber")
							.Value.Value)
						.ToArray());
			}
		}

		public void TestProcessLogs_TransportLegsAreSerialized()
		{
			// Arrange
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			{
				var (declaration, containers) = CreateDeclaration();

				Factory.Save();

				var logs = (declaration as EnterpriseBusinessObject).Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should create new SBR event", 1, logs.Count);

				// Act
				RunLogWalkerCycleForTest();

				// Assert
				var xmlEvent = GetLastMessageAsUniversalEvent();

				AssertEquals("EventType", AutoEvents.SubscriptionRequested.Code, xmlEvent.EventType);
				var transportLegs = xmlEvent.ContextCollection.Where(x => x.Type == ContainerAutomationEventContextType.TransportLeg).ToArray();
				AssertEquals(1, transportLegs.Length);
				AssertFirstLeg(transportLegs);
			}
		}

		protected override EnterpriseBusinessObject GetBusinessObjectInstance() => CreateDeclaration().Declaration as EnterpriseBusinessObject;

		(IBaseJobDeclaration Declaration, List<ForwardingContainer> Containers) CreateDeclaration()
		{
			var declaration = (IBaseJobDeclaration)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IBaseJobDeclaration)));
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Constants.CountryCodes.UnitedStates);

			var transports = ((ITransportParent)declaration).Transports;
			AddOrUpdateSeaTransport(transports);
			transports[0].JW_LegOrder = 1;

			declaration.JE_MasterBill = "12345678";
			declaration["JE_OH_ShippingLine"] = shippingLine.PK;

			var container1 = Factory.NewWithValidTestData<ForwardingContainer>();
			container1.JC_ContainerNum = "AAAA0000007";

			var cusContainers = (BusinessObjectCollection)declaration["CusContainers"];

			var cusContainer1 = cusContainers.AddNew();
			cusContainer1[CusContainerSchema.CO_JC] = container1.PK;
			cusContainer1[CusContainerSchema.CO_ContainerNumber] = container1.JC_ContainerNum;

			return (declaration, new List<ForwardingContainer> { container1 });
		}
	}
}
