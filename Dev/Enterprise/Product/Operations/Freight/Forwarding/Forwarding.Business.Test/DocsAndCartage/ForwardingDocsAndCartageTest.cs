using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingDocsAndCartageTest : BaseFreightTest
	{
		#region TestUpdateOrderStatusWhenJP_DeliveryCartageCompletedSet

		public void TestUpdateOrderStatusWhenJP_DeliveryCartageCompletedSet()
		{
			OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition - AutomaticallySetOrderStatus should be false", true, OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.Value);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Order order1 = shipment.AttachedOrders.AddNew();
			Order order2 = shipment.AttachedOrders.AddNew();
			AssertEquals("PreCondition of Order1: Should be Incomplete", Core.Constants.OrderStatus.Incomplete, order1.JD_OrderStatus);
			AssertEquals("PreCondition of Order2: Should be Incomplete", Core.Constants.OrderStatus.Incomplete, order1.JD_OrderStatus);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Now;

			AssertEquals("Order1: Should now be Delivered", Core.Constants.OrderStatus.Delivered, order1.JD_OrderStatus);
			AssertEquals("Order2: Should now be Delivered", Core.Constants.OrderStatus.Delivered, order1.JD_OrderStatus);
		}

		#endregion

		#region TestUsingCorrectValidationObject

		public void TestUsingCorrectValidationObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
			AssertEquals("Should be using the ForwardingDocsAndCartageValidation", typeof(ForwardingDocsAndCartageValidation), docsAndCartage.Validation.GetType());
		}

		#endregion

		#region TestConsignorDocumentaryAddressPopulatesAWB

		public void TestConsignorDocumentaryAddressPopulatesAWB()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingDocsAndCartage docsAndCartage = shipment.DocsAndCartage;

			OrgHeader consignor = OrgHeader.New(Factory);
			shipment.ConsignorPK = consignor.PK;

			OrgAddress shipperAddress = shipment.Consignor.MainAddress;
			shipperAddress.OA_Address1 = "ShipperAddress";
			shipment.PopulateAWB();
			AssertEquals("Should be ShipperAddress", "ShipperAddress", shipment.AWBHeader.EH_ShipperAddress);

			OrgAddress shipperDocumentAddress = shipment.Consignor.Addresses.AddNew();
			shipperDocumentAddress.OA_Address1 = "ShipperDocumentAddress";
			shipperDocumentAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);

			AssertEquals("Shipper postal address added, Still should be ShipperAddress", "ShipperAddress", shipment.AWBHeader.EH_ShipperAddress);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperDocumentAddress.PK;
			AssertEquals("ShipperDocumentaryAddress set, should be ShipperDocumentAddress", "ShipperDocumentAddress", shipment.AWBHeader.EH_ShipperAddress);
		}

		#endregion

		#region TestConsigneeDocumentaryAddressPopulatesAWB

		public void TestConsigneeDocumentaryAddressPopulatesAWB()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingDocsAndCartage docsAndCartage = shipment.DocsAndCartage;

			OrgHeader consignee = OrgHeader.New(Factory);
			shipment.ConsigneePK = consignee.PK;

			OrgAddress consigneeAddress = shipment.Consignee.MainAddress;
			consigneeAddress.OA_Address1 = "ConsigneeAddress";
			shipment.PopulateAWB();
			AssertEquals("Should be ConsigneeAddress", "ConsigneeAddress", shipment.AWBHeader.EH_ConsigneeAddress);

			OrgAddress consigneeDocumentAddress = shipment.Consignee.Addresses.AddNew();
			consigneeDocumentAddress.OA_Address1 = "ConsigneeDocumentAddress";
			consigneeDocumentAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);

			AssertEquals("Consignee postal address added, Still should be ConsigneeAddress", "ConsigneeAddress", shipment.AWBHeader.EH_ConsigneeAddress);

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentAddress.PK;
			AssertEquals("ConsigneeDocumentaryAddress set, should be ConsigneeDocumentAddress", "ConsigneeDocumentAddress", shipment.AWBHeader.EH_ConsigneeAddress);
		}

		#endregion

		#region Delivery / Pickup Events

		public void TestDeliveryDateLog()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			StmALog log = StmALogEntryLocator.Instance.GetLastPostEventOfType(shipment, AutoEvents.Delivered);
			AssertNull(log);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today.AddDays(2);
			log = StmALogEntryLocator.Instance.GetLastPostEventOfType(shipment, AutoEvents.Delivered);

			AssertNull(log);
		}

		public void TestPickupDateLog()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			StmALog log = StmALogEntryLocator.Instance.GetLastPostEventOfType(shipment, AutoEvents.PickedUp);
			AssertNull(log);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today.AddDays(-3);
			log = StmALogEntryLocator.Instance.GetLastPostEventOfType(shipment, AutoEvents.PickedUp);

			AssertNull(log);
		}

		#endregion

		#region AWB Print Option

		public void TestAWBPrintOption()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ActualVolume = 200m;
			ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_Height = 10m;
			packLine.JL_Width = 5m;
			packLine.JL_Length = 3m;
			packLine.JL_PackageCount = 4;
			ForwardingDocsAndCartage docsAndCartage = shipment.DocsAndCartage;

			AssertEquals("DIMS 300x500x1000 CM x 4", ((ShipmentExportAWBHeader)shipment.AWBHeader).VolumeAndDimensionForFollowOnPage);

			docsAndCartage.JP_PrintOptionForPackagesOnAWB = "BTH";
			AssertEquals("DIMS 300x500x1000 CM x 4\nVOL 200.000 M3", ((ShipmentExportAWBHeader)shipment.AWBHeader).VolumeAndDimensionForFollowOnPage);
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource

		public void TestParentWorkflowProviders()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertNotNull("Precondition", shipment.DocsAndCartage);

			IWorkflowTriggerFieldChangeSource workflowTriggerChangeSource = shipment.DocsAndCartage;
			AssertCollectionContains(shipment, workflowTriggerChangeSource.ParentWorkflowProviders);
		}

		#endregion

		#region ReadOnlySecurity

		public void TestReadOnlySecurityDueToPhase()
		{
			var shipmentMock = Factory.NewMoq<ForwardingShipment>();
			ForwardingDocsAndCartage docsAndCartage = shipmentMock.Object.DocsAndCartage;

			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_ArrivalCartageRefInfo.Name)).Returns(true);
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_EstimatedDeliveryInfo.Name)).Returns(false);

			AssertEquals(true, docsAndCartage.JP_ArrivalCartageRefInfo.ReadOnly);
			AssertEquals(false, docsAndCartage.JP_EstimatedDeliveryInfo.ReadOnly);
			shipmentMock.VerifyAll();

			shipmentMock.Reset();
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_ArrivalCartageRefInfo.Name)).Returns(false);
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_EstimatedDeliveryInfo.Name)).Returns(true);

			AssertEquals(false, docsAndCartage.JP_ArrivalCartageRefInfo.ReadOnly);
			AssertEquals(true, docsAndCartage.JP_EstimatedDeliveryInfo.ReadOnly);

			shipmentMock.VerifyAll();

			shipmentMock.Reset();
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_EstimatedDeliveryInfo.Name)).Returns(false);
			Env.Security.MaintainShipmentEstimatedDeliveryDateOverride.IsAllowed = true;

			AssertEquals(false, docsAndCartage.JP_EstimatedDeliveryInfo.ReadOnly);

			shipmentMock.VerifyAll();

			shipmentMock.Reset();
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_EstimatedDeliveryInfo.Name)).Returns(true);
			Env.Security.MaintainShipmentEstimatedDeliveryDateOverride.IsAllowed = false;

			AssertEquals(true, docsAndCartage.JP_EstimatedDeliveryInfo.ReadOnly);

			shipmentMock.VerifyAll();

			shipmentMock.Reset();
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_EstimatedDeliveryInfo.Name)).Returns(true);
			Env.Security.MaintainShipmentEstimatedDeliveryDateOverride.IsAllowed = true;

			AssertEquals(true, docsAndCartage.JP_EstimatedDeliveryInfo.ReadOnly);

			shipmentMock.VerifyAll();

			shipmentMock.Reset();
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, (ZString)docsAndCartage.JP_EstimatedDeliveryInfo.Name)).Returns(false);
			Env.Security.MaintainShipmentEstimatedDeliveryDateOverride.IsAllowed = false;

			AssertEquals(true, docsAndCartage.JP_EstimatedDeliveryInfo.ReadOnly);

			shipmentMock.VerifyAll();
		}

		public void TestRegisterEditableChildObjectWithChildName()
		{
			var shipmentMock = Factory.NewMoq<ForwardingShipment>();
			ForwardingDocsAndCartage docsAndCartage = shipmentMock.Object.DocsAndCartage;

			DummyBusinessObject child1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject child2 = Factory.New<DummyBusinessObject>();

			AssertEquals("Precondition", false, child1.ReadOnly);
			AssertEquals("Precondition", false, child2.ReadOnly);

			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, new ZString("Cookies"))).Returns(true);
			shipmentMock.Setup(m => m.IsChildPropertyReadOnlyDueToPhase(docsAndCartage.PK, new ZString("IceCream"))).Returns(false);

			// Need to use reflection, inheritance didn't worked because of weird DocsAndCartage <-> Shipment relationship
			MethodInfo registerChildMethodInfo = typeof(ForwardingDocsAndCartage).GetMethod("RegisterEditableChildObject", BindingFlags.NonPublic | BindingFlags.Instance);
			registerChildMethodInfo.Invoke(docsAndCartage, new object[] { child1, new ZString("Cookies") });
			registerChildMethodInfo.Invoke(docsAndCartage, new object[] { child2, new ZString("IceCream") });

			AssertEquals("Child registered", true, docsAndCartage.IsRegisteredEditableChildObject(child1));
			AssertEquals("Child registered", true, docsAndCartage.IsRegisteredEditableChildObject(child2));

			AssertEquals(true, child1.ReadOnly);
			AssertEquals(false, child2.ReadOnly);

			shipmentMock.VerifyAll();
		}

		#endregion

		#region PhaseDependantMandatoryValidation

		public void TestPhaseDependantMandatoryValidation_PropertyChange()
		{
			var phaseSecurity = GetPhaseSecurityForTesting();

			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var docsAndCartage = shipment.DocsAndCartage;

				docsAndCartage.JP_ExportStatement = "DEF";
				docsAndCartage.JP_CustomDate1 = ZDate.Today;
				docsAndCartage.JP_CustomDecimal1 = 100;

				shipment.JS_Phase = "AAA";

				AssertNoErrors(docsAndCartage.JP_ExportStatementInfo);
				AssertNoErrors(docsAndCartage.JP_CustomDate1Info);
				AssertNoErrors(docsAndCartage.JP_CustomDecimal1Info);

				docsAndCartage.JP_ExportStatement = "";
				docsAndCartage.JP_CustomDate1 = ZDate.Empty;
				docsAndCartage.JP_CustomDecimal1 = 0;

				AssertHasError(docsAndCartage.JP_ExportStatementInfo, "Export Statement has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Shipment -> Phases for details.");
				AssertHasError(docsAndCartage.JP_CustomDate1Info, "Custom Date 1 has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Shipment -> Phases for details.");
				AssertNoErrors("JP_CustomDecimal1 is not required", docsAndCartage.JP_CustomDecimal1Info);
			}
		}

		public void TestPhaseDependantMandatoryValidation_PhaseChange()
		{
			var phaseSecurity = GetPhaseSecurityForTesting();

			using (ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, phaseSecurity))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var docsAndCartage = shipment.DocsAndCartage;

				docsAndCartage.JP_ExportStatement = "DEF";
				docsAndCartage.JP_CustomDate1 = ZDate.Today;
				docsAndCartage.JP_CustomDecimal1 = 100;

				AssertEquals("Precondition", "ALL", shipment.JS_Phase);

				docsAndCartage.JP_ExportStatement = "";
				docsAndCartage.JP_CustomDate1 = ZDate.Empty;
				docsAndCartage.JP_CustomDecimal1 = 0;

				AssertNoErrors(docsAndCartage.JP_ExportStatementInfo);
				AssertNoErrors(docsAndCartage.JP_CustomDate1Info);
				AssertNoErrors(docsAndCartage.JP_CustomDecimal1Info);

				shipment.JS_Phase = "AAA";
				shipment.DocsAndCartage.RunPreSaveValidation();

				AssertHasError(docsAndCartage.JP_ExportStatementInfo, "Export Statement has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Shipment -> Phases for details.");
				AssertHasError(docsAndCartage.JP_CustomDate1Info, "Custom Date 1 has been made mandatory in the selected Phase. Please refer to Registry -> Freight -> Shipment -> Phases for details.");
				AssertNoErrors("JP_CustomDecimal1 is not required", docsAndCartage.JP_CustomDecimal1Info);

				shipment.JS_Phase = "ALL";
				shipment.DocsAndCartage.RunPreSaveValidation();

				AssertNoErrors(docsAndCartage.JP_ExportStatementInfo);
				AssertNoErrors(docsAndCartage.JP_CustomDate1Info);
				AssertNoErrors(docsAndCartage.JP_CustomDecimal1Info);
			}
		}

		PhaseSecurity GetPhaseSecurityForTesting()
		{
			var locationsList = PhaseConstants.GetShipmentLocationsList();

			var security = new PhaseSecurity(locationsList);
			security.IsEnabled = true;

			var phase = security.Phases.AddNew();
			phase.Code = "AAA";
			phase.Description = (NoResString)"AAA Phase Description";

			var rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			Action<PhaseRule, string> addDependant = (phaseRule, dependantName) =>
			{
				var dependant = phaseRule.Dependants.AddNew();
				dependant.DependantType = PhaseConstants.DependantType.Property;
				dependant.Name = dependantName;
				dependant.Description = dependantName + "Description";
				dependant.IsMandatory = true;
			};

			addDependant(rule, "DocsAndCartage.JP_ExportStatement");
			addDependant(rule, "DocsAndCartage.JP_CustomDate1");

			security.Phases.Add(phase);

			return security;
		}

		#endregion

		public void TestGetDISHost()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var docsAndCartage = shipment.DocsAndCartage;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobDeclaration>());
			usDeclaration[JobDeclarationSchema.JE_JS.Name] = shipment.PK;

			var host = ((MasterFiles.Business.DIS.IDISHostProvider)docsAndCartage).DISHost;
			AssertEquals("DIS declaration", usDeclaration, host);
		}
	}
}
