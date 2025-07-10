using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class NZReleaseOrderActionMethodApplicatorTest : ReleaseImportOrderActionMethodApplicatorTest
	{
		public void TestApply_EHubIdIsNotSet_LogError()
		{
			using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ShippingPortsMessagingEHubIDCollection()))
			{
				var target = Factory.NewWithValidTestData<BillOfLadingContainer>();

				this.ApplyApplicator(new[] { target }, "ERROR: The Import Release Order message cannot be sent as eHub ID is not set.");
			}
		}

		public void TestApply_BrunchWithoutOrgProxy_LogError()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var target = Factory.NewWithValidTestData<BillOfLadingContainer>();

					this.ApplyApplicator(new[] { target }, "ERROR: The current branch does not have an org. proxy.");
				}
			}
		}

		public void TestIsApplicable_ImportReleaseOrderPortsRegistryEnabled()
		{
			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container1 = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, dischargePort: "AUSYD");
					var container2 = GetContainerWithoutErrors("BOOKING_2", "CONTAINER_2", PrincipalNZ);

					var expectedLog =
						"WARNING: Principal is not configured for sending Import Release Order to AUSYD." + "\r\n" +
						"INFO: Processing Container 'CONTAINER_2'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: \r\n" +
						"INFO: Delivery Succeeded.";

					this.ApplyApplicator(new[] { container1, container2 }, expectedLog);
				}
			}
		}

		public void TestIsApplicable_OrgProxyHasMatchingCARCode()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Australia).PK;
			branch.Company.GC_RN_NKCountryCode = "AU";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container1 = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ);
					var container2 = GetContainerWithoutErrors("BOOKING_2", "CONTAINER_2", principalNZ);

					var expectedLog =
						"WARNING: The current branch's organisation proxy does not have a CAR code for the country/region of container's <[HL CONTAINER_1]> port of discharge entered. The container is skipped" + "\r\n" +
						"WARNING: The current branch's organisation proxy does not have a CAR code for the country/region of container's <[HL CONTAINER_2]> port of discharge entered. The container is skipped";

					this.ApplyApplicator(new[] { container1, container2 }, expectedLog);
				}
			}
		}

		public void TestApply_SkipErrorsMode_SkipContainersWithErrors()
		{
			Settings.ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Skip;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", ZGuid.Empty, true);
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container1 = GetContainerWithErrors("BOOKING_1", "CONTAINER_1");
					var container2 = GetContainerWithoutErrors("BOOKING_2", "CONTAINER_2", PrincipalNZ);

					var expectedLog =
						"WARNING: '[HL BOOKING_1]' has errors and/or message errors." + "\r\n" +
						"You will need to correct these before an Import Release Order message can be sent for it." + "\r\n" +
						"INFO: Processing Container 'CONTAINER_2'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: \r\n" +
						"INFO: Delivery Succeeded.";

					this.ApplyApplicator(new[] { container1, container2 }, expectedLog);

					AssertMessageHasBeenSent(container1, false);
					AssertMessageHasBeenSent(container2, true);
				}
			}
		}

		public void TestApply_AbortInCaseOfErrorMode_AbortOnContainerWithErrors()
		{
			Settings.ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Abort;

			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", ZGuid.Empty, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container1 = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ);
					var container2 = GetContainerWithErrors("BOOKING_2", "CONTAINER_2");
					var container3 = GetContainerWithoutErrors("BOOKING_3", "CONTAINER_3", PrincipalNZ);

					var expectedLog =
						"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: " + "\r\n" +
						"INFO: Delivery Succeeded.\r\n" +
						"ERROR: '[HL BOOKING_2]' has errors and/or message errors." + "\r\n" +
						"You will need to correct these before an Import Release Order message can be sent for it.";

					this.ApplyApplicator(new[] { container1, container2, container3 }, expectedLog);

					AssertMessageHasBeenSent(container1, true);
					AssertMessageHasBeenSent(container2, false);
					AssertMessageHasBeenSent(container3, false);
				}
			}
		}

		public void TestApply_DischargePortOfContainerIsApplicableOrNot()
		{
			using (Factory.AddDisposableService())
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZLYT", ZGuid.Empty, false);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container1 = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "AUSYD", "NZAKL");
					var container2 = GetContainerWithoutErrors("BOOKING_2", "CONTAINER_2", PrincipalNZ, "AUSYD", "NZLYT");

					var expectedLog =
						"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: " + "\r\n" +
						"INFO: Delivery Succeeded." + "\r\n" +
						"WARNING: Principal is not configured for sending Import Release Order to NZLYT." + "\r\n";

					this.ApplyApplicator(new[] { container1, container2 }, expectedLog);

					AssertMessageHasBeenSent(container1, true);
					AssertMessageHasBeenSent(container2, false);
				}
			}
		}

		public void TestAssertUXMLMessage()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true).OfType<PortMessagingPort>().First(x => x.Port == "NZAKL").SenderID = "DIJIAAUTOMAN";

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "AUSYD", "NZAKL", Constants.CountryCodes.NewZealand);

					var expectedLog =
						"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: " + "\r\n" +
						"INFO: Delivery Succeeded." + "\r\n";

					this.ApplyApplicator(new[] { container }, expectedLog);

					AssertMessageHasBeenSent(container, true);

					var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("Message created", 1, ediMessages.Length);

					var uxml = XDocument.Parse(ediMessages[0].EM_MessageText);
					var ns = uxml.Root.GetDefaultNamespace();

					var addInfoCollectionDataContext = uxml.Descendants(ns + "AddInfoCollection").First();
					var expectedAddInfoCollectionDataContext = @"<AddInfoCollection xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <AddInfo>
    <Key>SenderID</Key>
    <Value>DIJIAAUTOMAN</Value>
  </AddInfo>
  <AddInfo>
    <Key>OperationalPort_Code</Key>
    <Value>NZAKL</Value>
  </AddInfo>
  <AddInfo>
    <Key>OperationalPort_Name</Key>
    <Value>Auckland</Value>
  </AddInfo>
</AddInfoCollection>";
					AssertMultilineASCIIEquals(expectedAddInfoCollectionDataContext, addInfoCollectionDataContext.ToString());

					var lloydsImo = uxml.Descendants(ns + "Shipment").First().Element(ns + "LloydsIMO").Value;
					AssertEquals("VesselLloydsIMO", "8811924", lloydsImo);
				}
			}
		}

		public void TestApply_MessageIsAlreadySent_DoNotSendMessage()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithMessageSent("BOOKING", "CONTAINER", PrincipalNZ);
					var expectedLog = string.Format("ERROR: [HL CONTAINER] is still waiting on a response.", container.JC_ContainerNum);

					this.ApplyApplicator(new[] { container }, expectedLog);

					AssertMessageHasBeenSent(container, false);
				}
			}
		}

		public void TestApply_SendIndividualPortsIfRegistryIsEnabled_System()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", ZGuid.Empty, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "UAIEV", "NZAKL", Constants.CountryCodes.NewZealand);
					var expectedLog =
					"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
					"INFO: Universal Shipment queued for sending." + "\r\n" +
					"INFO: " + "\r\n" +
					"INFO: Delivery Succeeded.";
					ApplyApplicator(new[] { container }, expectedLog);
					AssertMessageHasBeenSent(container, true);
				}
			}
		}

		public void TestApply_SendIndividualPortsIfRegistryIsEnabled_Company()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", ZGuid.Empty, true);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(NewZeelandBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "UAIEV", "NZAKL", Constants.CountryCodes.NewZealand);
					var expectedLog =
					"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
					"INFO: Universal Shipment queued for sending." + "\r\n" +
					"INFO: " + "\r\n" +
					"INFO: Delivery Succeeded.";
					ApplyApplicator(new[] { container }, expectedLog);
					AssertMessageHasBeenSent(container, true);
				}
			}
		}

		public void TestApply_DoNotSendNZPortsIfRegistryIsNotEnabled()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, false);

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "UAIEV", "NZAKL", Constants.CountryCodes.Australia);
					ApplyApplicator(new[] { container }, "WARNING: Principal is not configured for sending Import Release Order to NZAKL.");
					AssertMessageHasBeenSent(container, false);
				}
			}
		}

		public void TestApply_DoNotSendNZPortsIfRegistryIsNotConfigured()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZeelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "UAIEV", "NZAKL", Constants.CountryCodes.Australia);
					ApplyApplicator(new[] { container }, "WARNING: Principal is not configured for sending Import Release Order to NZAKL.");
					AssertMessageHasBeenSent(container, false);
				}
			}
		}

		public void TestApply_RecipientIdIsEqualToShippingPortsMessagingEHubIDRegistryOfSpanishPort()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, SpainBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "ESVLC", ZGuid.Empty, true);

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();
				var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID.Port = "ESVLC";
				shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.xHub;
				shippingPortsMessagingEHubID.RecipientID = "ESVLC_RECIPIENT";

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalES, "UAIEV", "ESVLC", Constants.CountryCodes.Spain);
					var expectedLog =
						"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: " + "\r\n" +
						"INFO: Delivery Succeeded.";
					ApplyApplicator(new[] { container }, expectedLog);
					AssertMessageHasBeenSent(container, true);

					var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("ESVLC_RECIPIENT", ediMessages.FirstOrDefault().EM_InterchangeReceiver);
				}
			}
		}

		public void TestApply_RecipientIdIsEqualToShippingPortsMessagingEHubIDRegistryWithEmptyPort()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, SpainBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			using (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "ESVLC", ZGuid.Empty, true);

				var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

				var shippingPortsMessagingEHubID2 = shippingPortsMessagingEHubIDCollection.AddNew();
				shippingPortsMessagingEHubID2.Port = ZString.Empty;
				shippingPortsMessagingEHubID2.Module = ModuleTypes.Codes.eHub;
				shippingPortsMessagingEHubID2.RecipientID = "DEF_RECIPIENT";

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, shippingPortsMessagingEHubIDCollection))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalES, "UAIEV", "ESVLC", Constants.CountryCodes.Spain);
					var expectedLog =
						"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: " + "\r\n" +
						"INFO: Delivery Succeeded.";
					ApplyApplicator(new[] { container }, expectedLog);
					AssertMessageHasBeenSent(container, true);

					var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("DEF_RECIPIENT", ediMessages.FirstOrDefault().EM_InterchangeReceiver);
				}
			}
		}

		#region Implementation

		protected ShippingPortsMessagingEHubIDCollection GetShippingPortsMessagingEHubIDCollection()
		{
			var shippingPortsMessagingEHubIDCollection = new ShippingPortsMessagingEHubIDCollection();

			var shippingPortsMessagingEHubID = shippingPortsMessagingEHubIDCollection.AddNew();
			shippingPortsMessagingEHubID.Port = ZString.Empty;
			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;
			shippingPortsMessagingEHubID.RecipientID = "McLaren";

			return shippingPortsMessagingEHubIDCollection;
		}

		protected abstract BillOfLadingContainer GetContainerWithMessageSent(string uniqueConsignRef, string containerNumber, OrgHeader principal);

		protected abstract void AssertMessageHasBeenSent(BillOfLadingContainer container, bool expectedToBeSent, int numberOfEdimessage = 1);

		protected BillOfLadingContainer GetContainerWithoutErrors(string uniqueConsignRef, string containerNumber, OrgHeader principal, string loadPort = "USLAX", string dischargePort = "NZAKL", string countryCode = Constants.CountryCodes.NewZealand)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", countryCode);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = loadPort;
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_FK;
			voyage.JV_VoyageFlight = "EK134";
			voyage.JV_OH_Line = carrier.PK;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_Calc_ArrivalCTOAddressOrg = Factory.NewWithValidTestData<OrgHeader>().PK;
			destination.JB_E_ARV = 5.DaysAgo();

			voyage.GenerateSailings();

			var booking = Factory.NewWithValidTestData<BillOfLading>();
			booking.JS_HouseBill = uniqueConsignRef;
			booking.JS_UniqueConsignRef = uniqueConsignRef;
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			booking.JS_OH_DeliveryAgent = principal.PK;
			booking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			booking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			booking.Transports[0].JW_ETD = 10.DaysAgo();
			booking.JS_GoodsDescription = "McLaren";

			var container = booking.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_GrossWeight = 100.0;

			var line = booking.OuterPackLines.AddNew();
			line.JL_JC = container.PK;
			line.JL_DetailedDescription = "McLaren";

			Factory.Save();
			return container;
		}

		BillOfLadingContainer GetContainerWithErrors(string uniqueConsignRef, string containerNumber)
		{
			var booking = Factory.NewWithValidTestData<BillOfLading>();
			booking.JS_HouseBill = uniqueConsignRef;
			booking.JS_UniqueConsignRef = uniqueConsignRef;
			booking.JS_NKDischargePort = "NZAKL";

			var container = booking.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;

			return container;
		}

		GlbBranch NewZeelandBranch
		{
			get
			{
				return newZeelandBranch ?? (newZeelandBranch = CreateNewZeelandBranch());
			}
		}
		GlbBranch newZeelandBranch;

		GlbBranch CreateNewZeelandBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand).PK;
			branch.Company.GC_RN_NKCountryCode = "NZ";

			Factory.Save();

			return branch;
		}

		GlbBranch SpainBranch
		{
			get
			{
				return spainBranch ?? (spainBranch = CreateSpainBranch());
			}
		}

		GlbBranch spainBranch;

		GlbBranch CreateSpainBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.Spain).PK;
			branch.Company.GC_RN_NKCountryCode = "ES";

			Factory.Save();

			return branch;
		}

		PortMessagingPortCollection AddNewPortMessagingPort(PortMessagingPortCollection importReleaseOrderPorts, string port, ZGuid principalPK, bool enabled, string senderID = "Sender")
		{
			var portMessagingPort = importReleaseOrderPorts.AddNew();
			portMessagingPort.Port = port;
			portMessagingPort.PrincipalPK = principalPK;
			portMessagingPort.SenderID = $"{senderID}_{importReleaseOrderPorts.Count}";
			portMessagingPort.Enabled = enabled;

			return importReleaseOrderPorts;
		}

		protected List<KeyValuePair<string, string>> GetMessageParameters(BillOfLadingContainer container, string messageType)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(Params.MessageType.AsKeyFor(messageType));
			ZBool registryValidation = false;
			if (container.Booking.TransportsIncludingRelated.LastLeg != null)
			{
				registryValidation = AgencyRegistry.Instance.ImportReleaseOrderPorts.Value.OfType<PortMessagingPort>().FirstOrDefault(x => x.Port == container.Booking.TransportsIncludingRelated.LastLeg.JW_RL_NKDiscPort)?.Enabled ?? false;
			}

			if (registryValidation)
			{
				parameters.Add(Params.Department.AsKeyFor("Terminal"));
				parameters.Add(Params.Location.AsKeyFor(container.Booking.TransportsIncludingRelated.LastLeg.JW_RL_NKDiscPort));
			}
			else
			{
				parameters.Add(Params.Department.AsKeyFor("NZ Ports"));
			}

			return parameters;
		}

		OrgHeader PrincipalNZ
		{
			get
			{
				return principalNZ ?? (principalNZ = CreateNewPrincipal(Constants.CountryCodes.NewZealand));
			}
		}
		OrgHeader principalNZ;

		OrgHeader PrincipalES
		{
			get
			{
				return principalES ?? (principalES = CreateNewPrincipal(Constants.CountryCodes.Spain));
			}
		}
		OrgHeader principalES;

		OrgHeader CreateNewPrincipal(ZString countryCode)
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", countryCode);
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			return principal;
		}

		private protected static ZQuery GetEDIMessageFilter()
		{
			var filter = new ZQuery();
			filter.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc;

			return filter;
		}

		#endregion
	}
}
