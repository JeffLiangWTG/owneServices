using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class DemandeDeTracingBuilderTest : TestCaseWithFactory
	{
		public void TestBuildImport()
		{
			var consol = CreateConsol();
			var containers = CreateContainers();

			var builder = new DemandeDeTracingBuilder(new ForwardingConsolTRCDetailsProvider(consol), containers, DemandeDeTracingDirection.Import);
			var demandeDeTracing = builder.Build();
			AssertNotNull(demandeDeTracing);

			AssertEquals("ConsolNumber", "C00001000", demandeDeTracing.ConsolNumber);
			AssertEquals("OperationalPort", "FRPAR", demandeDeTracing.OperationalPort.Code);
			AssertEquals("BookingConfirmationReference", "BookingReference", demandeDeTracing.BookingConfirmationReference);
			AssertEquals("ContainerMode", "FCL", demandeDeTracing.ContainerMode.Code);
			AssertEquals("PortOfDestination", "FRPAR", demandeDeTracing.PortOfDestination.Code);
			AssertEquals("PortOfOrigin", "AUSYD", demandeDeTracing.PortOfOrigin.Code);
			AssertEquals("ShipmentType", "AGT", demandeDeTracing.ShipmentType.Code);
			AssertEquals("WaybillNumber", "BOL_Reference", demandeDeTracing.WaybillNumber);
			AssertEquals("ETA", new ZDateTime(2022, 04, 14, 00, 00, 00), demandeDeTracing.ETA);
			Assert("ETD", demandeDeTracing.ETD.IsEmpty);

			AssertEquals("ContainersCount", 3, demandeDeTracing.Containers.Count);
			var demandeDeTracingContainers = demandeDeTracing.Containers;
			AssertEquals("ContainerNumber1", "123",
				demandeDeTracingContainers.First(c => c.ContainerNumber == "123").ContainerNumber);
			AssertEquals("ContainerNumber2", "456",
				demandeDeTracingContainers.First(c => c.ContainerNumber == "456").ContainerNumber);
			AssertEquals("ContainerNumber3", "789",
				demandeDeTracingContainers.First(c => c.ContainerNumber == "789").ContainerNumber);
			AssertEquals("IsNonOperativeReefer1", ZBool.False,
				demandeDeTracingContainers.First(c => c.ContainerNumber == "123").IsNonOperativeReefer);
			AssertEquals("IsNonOperativeReefer2", ZBool.False,
				demandeDeTracingContainers.First(c => c.ContainerNumber == "456").IsNonOperativeReefer);
			AssertEquals("IsNonOperativeReefer3", ZBool.False,
				demandeDeTracingContainers.First(c => c.ContainerNumber == "789").IsNonOperativeReefer);

			AssertAddressData(consol.ReceivingForwarderAddress, demandeDeTracing.ReceivingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, demandeDeTracing.SendingParty);
		}

		public void TestBuildExport()
		{
			var consol = CreateConsol(isImport: false);
			var containers = CreateContainers();

			var builder = new DemandeDeTracingBuilder(new ForwardingConsolTRCDetailsProvider(consol), containers, DemandeDeTracingDirection.Export);
			var demandeDeTracing = builder.Build();
			AssertNotNull(demandeDeTracing);

			AssertEquals("ConsolNumber", "C00001000", demandeDeTracing.ConsolNumber);
			AssertEquals("OperationalPort", "FRPAR", demandeDeTracing.OperationalPort.Code);
			AssertEquals("BookingConfirmationReference", "BookingReference", demandeDeTracing.BookingConfirmationReference);
			AssertEquals("ContainerMode", "FCL", demandeDeTracing.ContainerMode.Code);
			AssertEquals("PortOfDestination", "AUSYD", demandeDeTracing.PortOfDestination.Code);
			AssertEquals("PortOfOrigin", "FRPAR", demandeDeTracing.PortOfOrigin.Code);
			AssertEquals("ShipmentType", "AGT", demandeDeTracing.ShipmentType.Code);
			AssertEquals("WaybillNumber", "BOL_Reference", demandeDeTracing.WaybillNumber);
			AssertEquals("ETD", new ZDateTime(2022, 04, 24, 00, 00, 00), demandeDeTracing.ETD);
			Assert("ETA", demandeDeTracing.ETA.IsEmpty);

			AssertEquals("ContainersCount", 3, demandeDeTracing.Containers.Count);
			var demandeDeTracingContainers = demandeDeTracing.Containers;
			AssertEquals("ContainerNumber1", "123",
				demandeDeTracingContainers.First(c => c.ContainerNumber == "123").ContainerNumber);
			AssertEquals("ContainerNumber2", "456",
				demandeDeTracingContainers.First(c => c.ContainerNumber == "456").ContainerNumber);
			AssertEquals("ContainerNumber3", "789",
				demandeDeTracingContainers.First(c => c.ContainerNumber == "789").ContainerNumber);
			AssertEquals("IsNonOperativeReefer1", ZBool.False,
				demandeDeTracingContainers.First(c => c.ContainerNumber == "123").IsNonOperativeReefer);
			AssertEquals("IsNonOperativeReefer2", ZBool.False,
				demandeDeTracingContainers.First(c => c.ContainerNumber == "456").IsNonOperativeReefer);
			AssertEquals("IsNonOperativeReefer3", ZBool.False,
				demandeDeTracingContainers.First(c => c.ContainerNumber == "789").IsNonOperativeReefer);

			AssertAddressData(consol.ShippingLineAddress, demandeDeTracing.Carrier);
			AssertAddressData(consol.SendingForwarderAddress, demandeDeTracing.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, demandeDeTracing.SendingParty);
		}

		public void TestPopulateCurrentUser()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = CreateConsol();

			var builder = new DemandeDeTracingBuilder(new ForwardingConsolTRCDetailsProvider(consol), Array.Empty<ForwardingContainer>(), DemandeDeTracingDirection.Import);

			var demandeDeTracing = builder.Build();

			if (branchProxy.MainAddress.OA_RN_NKCountryCode.IsEmpty)
			{
				branchProxy.MainAddress.OA_RN_NKCountryCode = GlbBranch.CurrentBranch.BaseCountry?.Code ?? ZString.Empty;
			}
			AssertAddressData(branchProxy.MainAddress, demandeDeTracing.SendingParty);
			AssertPopulateAPPlusCodesUseRegistry("SendingPartyCI5", "SendingPartySON", DemandeDeTracingDirection.Import);
			AssertPopulateAPPlusCodesUseRegistry("SendingPartyCI5", "SendingPartySON", DemandeDeTracingDirection.Export);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			demandeDeTracing = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, demandeDeTracing.SendingParty);
			AssertPopulateAPPlusCodesUseRegistry("SendingPartyCI5", "SendingPartySON", DemandeDeTracingDirection.Import);
			AssertPopulateAPPlusCodesUseRegistry("SendingPartyCI5", "SendingPartySON", DemandeDeTracingDirection.Export);
		}

		void AssertPopulateAPPlusCodesUseRegistry(string ci5PropertyName, string sPropertyName, DemandeDeTracingDirection direction)
		{
			ForwardingConsol consol = CreateConsol();

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);

			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.RL_Code = "FRFR2";
			}

			if (direction == DemandeDeTracingDirection.Import)
			{
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport.JW_RL_NKDiscPort = unloco.Code;
			}
			else
			{
				var transport = consol.Transports.OfType<Transport>().FirstOrDefault() ?? consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = unloco.Code;
			}

			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = unloco.Code;
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			var builder = new DemandeDeTracingBuilder(new ForwardingConsolTRCDetailsProvider(consol), Array.Empty<ForwardingContainer>(), direction);

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{ci5PropertyName} should be from registry", "FORWARDER", ((DocumentVisualizer.DocDataObjects.RegistrationNumber)builder.Build()[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{sPropertyName} should be from registry", "FORWARDER", ((DocumentVisualizer.DocDataObjects.RegistrationNumber)builder.Build()[sPropertyName]).Value);
			}
		}

		public void TestPopulateAddressesImport()
		{
			var consol = CreateConsol();
			var containers = CreateContainers();

			var builder = new DemandeDeTracingBuilder(new ForwardingConsolTRCDetailsProvider(consol), containers, DemandeDeTracingDirection.Import);
			var demandeDeTracing = builder.Build();

			AssertNotNull(demandeDeTracing);

			AssertAddressData(consol.ReceivingForwarderAddress, demandeDeTracing.ReceivingForwarder);
		}

		public void TestPopulateAddressesExport()
		{
			var consol = CreateConsol(isImport: false);
			var containers = CreateContainers();

			var builder = new DemandeDeTracingBuilder(new ForwardingConsolTRCDetailsProvider(consol), containers, DemandeDeTracingDirection.Export);
			var demandeDeTracing = builder.Build();
			AssertNotNull(demandeDeTracing);

			AssertAddressData(consol.ShippingLineAddress, demandeDeTracing.Carrier);
			AssertAddressData(consol.SendingForwarderAddress, demandeDeTracing.SendingForwarder);
		}

		#region Implement

		ForwardingConsol CreateConsol(bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPAR";

				var transportLeg1 = consol.Transports[0];
				transportLeg1.JW_LegOrder = 1;
				transportLeg1.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg1.JW_RL_NKLoadPort = "AUSYD";
				transportLeg1.JW_RL_NKDiscPort = "BEANR";

				var transportLeg2 = consol.Transports.AddNew();
				transportLeg2.JW_LegOrder = 2;
				transportLeg2.JW_TransportMode = Constants.TransportModes.Road;
				transportLeg2.JW_RL_NKLoadPort = "BEANR";
				transportLeg2.JW_RL_NKDiscPort = "BEZEE";

				var transportLeg3 = consol.Transports.AddNew();
				transportLeg3.JW_LegOrder = 3;
				transportLeg3.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg3.JW_RL_NKLoadPort = "BEZEE";
				transportLeg3.JW_RL_NKDiscPort = "NLRTM";

				var transportLe4 = consol.Transports.AddNew();
				transportLe4.JW_LegOrder = 4;
				transportLe4.JW_TransportMode = Constants.TransportModes.Road;
				transportLe4.JW_RL_NKLoadPort = "NLRTM";
				transportLe4.JW_RL_NKDiscPort = "NLAMS";

				var transportLeg5 = consol.Transports.AddNew();
				transportLeg5.JW_LegOrder = 5;
				transportLeg5.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg5.JW_RL_NKLoadPort = "NLAMS";
				transportLeg5.JW_RL_NKDiscPort = "FRPAR";
				transportLeg5.JW_ETA = new DateTime(2022, 04, 14);
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transportLeg1 = consol.Transports[0];
				transportLeg1.JW_LegOrder = 1;
				transportLeg1.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg1.JW_RL_NKLoadPort = "FRPAR";
				transportLeg1.JW_RL_NKDiscPort = "NLAMS";
				transportLeg1.JW_ETD = new DateTime(2022, 04, 24);

				var transportLeg2 = consol.Transports.AddNew();
				transportLeg2.JW_LegOrder = 2;
				transportLeg2.JW_TransportMode = Constants.TransportModes.Road;
				transportLeg2.JW_RL_NKLoadPort = "NLAMS";
				transportLeg2.JW_RL_NKDiscPort = "NLRTM";

				var transportLeg3 = consol.Transports.AddNew();
				transportLeg3.JW_LegOrder = 3;
				transportLeg3.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg3.JW_RL_NKLoadPort = "NLRTM";
				transportLeg3.JW_RL_NKDiscPort = "BEZEE";

				var transportLe4 = consol.Transports.AddNew();
				transportLe4.JW_LegOrder = 4;
				transportLe4.JW_TransportMode = Constants.TransportModes.Road;
				transportLe4.JW_RL_NKLoadPort = "BEZEE";
				transportLe4.JW_RL_NKDiscPort = "BEANR";

				var transportLeg5 = consol.Transports.AddNew();
				transportLeg5.JW_LegOrder = 5;
				transportLeg5.JW_TransportMode = Constants.TransportModes.Sea;
				transportLeg5.JW_RL_NKLoadPort = "BEANR";
				transportLeg5.JW_RL_NKDiscPort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			return consol;
		}

		IReadOnlyCollection<ForwardingContainer> CreateContainers()
		{
			var containers = new List<ForwardingContainer>();

			var container1 = Factory.New<ForwardingContainer>();
			container1.JC_ContainerNum = "123";
			container1.JC_IsNonOperativeReefer = false;
			containers.Add(container1);

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_ContainerNum = "456";
			container2.JC_IsNonOperativeReefer = false;
			containers.Add(container2);

			var container3 = Factory.New<ForwardingContainer>();
			container3.JC_ContainerNum = "789";
			container3.JC_IsNonOperativeReefer = false;
			containers.Add(container3);

			return containers;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}

		#endregion
	}
}
