using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class ContainerAdviceToBookingBuilderTest : TestCaseWithFactory
	{
		#region Test Build

		public void TestBuild()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			CombineAssertions(() =>
			{
				AssertEquals("ConsolNumber", "C00001000", data.ConsolNumber);
				AssertEquals("CarrierBookingReference", "B0001100", data.CarrierBookingReference);
				AssertEquals("ContainerMode", "FCL", data.ContainerMode.Code);
				AssertEquals("ShipmentType", "AGT", data.ShipmentType.Code);
				Assert("RequiresTemperatureControl", data.RequiresTemperatureControl);
				AssertEquals("TemperatureMinimum.Value", 15m, data.TemperatureMinimum.Value);
				AssertEquals("TemperatureMinimum.Unit.Code", "C", data.TemperatureMinimum.Unit.Code);
				AssertEquals("TemperatureMaximum.Value", 25m, data.TemperatureMaximum.Value);
				AssertEquals("TemperatureMaximum.Unit.Code", "C", data.TemperatureMaximum.Unit.Code);
				AssertEquals("PortOfOrigin", "FRPRA", data.PortOfOrigin.Code);
				AssertEquals("PortOfTranshipment", "SGSIN", data.PortOfTranshipment.Code);
				AssertEquals("PortOfDestination", "AUSYD", data.PortOfDestination.Code);
				AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
				AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);
				AssertEquals("ATP", "AOEUI1234", data.ATPReference);
				AssertEquals("VoyageNumber", "111", data.VoyageNumber);

				AssertEquals("Containers.Count", 1, data.Containers.Count);
			});

			AssertAddressData(consol.ShippingLineAddress, data.Carrier);
			AssertAddressData(consol.CarrierBookingAgentDocumentaryAddress, data.CarrierBookingAgent);
			AssertAddressData(consol.DepartureCTOAddress, data.CTO);
			AssertAddressData(consol.DeparturePackCFSTransportAddress, data.Transporter);
			AssertAddressData((OrgAddress)consol.SendingForwarderWithContact.OrgAddress, data.SendingForwarder);
			AssertAddressData((OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, data.ReceivingForwarder);
			AssertAddressData((OrgAddress)consol.SendingForwarderWithContact.OrgAddress, data.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, data.Forwarder);

			var container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Number", "AAAA0000007", container.Number);
				AssertEquals("ContainerCount", 1, container.ContainerCount);
				AssertEquals("DeliveryMode", "CY/CY", container.DeliveryMode);
				AssertEquals("Mode", "FCL", container.Mode.Code);
				Assert("IsEmpty", container.IsEmpty);
				Assert("Fumigated", container.Fumigated);
				Assert("OversizeContainer", container.OversizeContainer);
				Assert("HazardousCargo", container.HazardousCargo);
				Assert("MarinePollutant", container.MarinePollutant);
				AssertEquals("HandlingNotes", "container handling note", container.HandlingNotes);
				AssertEquals("TransportMode", "RTE", container.TransportMode.Code);
				AssertEquals("ECTReference", "ExportReference", container.ECTReference);

				AssertEquals("FumigationService.ServiceNote", "fumigation note", container.FumigationService.ServiceNote);
				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
			});
			AssertionHelper.AssertAddressData(consol.Containers.Cast<ForwardingContainer>().First().Services.Cast<JobService>().First().Contractor, container.FumigationService.Contractor);

			var packingLine = container.PackingLines.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("ContainerNumber", "AAAA0000007", packingLine.ContainerNumber);
				AssertEquals("ContainerNumber", 2, packingLine.Quantity);
				AssertEquals("PackageType.Code", "PLT", packingLine.PackageType.Code);
				Assert("RequiresTemperatureControl", packingLine.RequiresTemperatureControl);
				AssertEquals("TemperatureMinimum.Value", 10m, packingLine.TemperatureMinimum.Value);
				AssertEquals("TemperatureMinimum.Unit.Code", "C", packingLine.TemperatureMinimum.Unit.Code);
				AssertEquals("TemperatureMaximum.Value", 30m, packingLine.TemperatureMaximum.Value);
				AssertEquals("TemperatureMaximum.Unit.Code", "C", packingLine.TemperatureMaximum.Unit.Code);

				AssertEquals("PackingLines.Count", 1, packingLine.DangerousGoods.Count);
			});

			var undg = packingLine.DangerousGoods.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Code", "6666E", undg.Code);
				AssertEquals("MarinePollutant.Code", "N", undg.MarinePollutant.Code);
				Assert("PackedInLimitedQuantity", undg.PackedInLimitedQuantity);
				AssertEquals("State", "L", undg.State);
			});

			data.OperationalPort.Code = string.Empty;
			container.TransportMode.Code = string.Empty;
			data.ValidateAllIncludingChildren();

			AssertHasMessageError("OperationalPort Error", ((Unloco)data.OperationalPort).CodeInfo, "Operational Port is required, UNLOCO is missing from Consol > Departure > CTO Address or from the Load Port of the first SEA leg.");
			AssertHasMessageError("TransportMode Error", container.TransportMode.CodeInfo, "Transport Mode is required.");

			container.TransportMode.Code = "ABCDE";
			data.ValidateAllIncludingChildren();

			AssertHasMessageError("TransportMode Error", container.TransportMode.CodeInfo, "Transport Mode must not exceed three (3) characters.");
		}

		public void TestPCSAndOperationalPortWithFallback()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
			AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);

			consol.DepartureCTOAddress.Delete();
			data = builder.Build();
			AssertEquals("PCS", ZString.Empty, data.PCS);
			AssertHasMessageError("PCS Error", data.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", "FRPRA", data.OperationalPort.Code);
		}

		public void TestPopulateForwarder()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());

			var data = builder.Build();

			AssertAddressData(branchProxy.MainAddress, data.Forwarder);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			data = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, data.Forwarder);
		}

		public void TestPopulateAPPlusCodes()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreateAddresses(consol, true);

			AssertPopulateAPPlusCodesCI5(consol, consol.ShippingLineAddress, "CarrierCI5");
			AssertPopulateAPPlusCodesSON(consol, consol.ShippingLineAddress, "CarrierSON");

			AssertPopulateAPPlusCodesCI5(consol, consol.CarrierBookingAgentDocumentaryAddress.Address, "CarrierBookingAgentCI5");
			AssertPopulateAPPlusCodesSON(consol, consol.CarrierBookingAgentDocumentaryAddress.Address, "CarrierBookingAgentSON");

			AssertPopulateAPPlusCodesCI5(consol, consol.DepartureCTOAddress, "CTOCI5");
			AssertPopulateAPPlusCodesSON(consol, consol.DepartureCTOAddress, "CTOSON");

			AssertPopulateAPPlusCodesCI5(consol, consol.DeparturePackCFSTransportAddress, "TransporterCI5");
			AssertPopulateAPPlusCodesSON(consol, consol.DeparturePackCFSTransportAddress, "TransporterSON");

			AssertPopulateAPPlusCodesCI5(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingPartyCI5");
			AssertPopulateAPPlusCodesSOA(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingPartySOA");
			AssertPopulateAPPlusCodesSON(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingPartySON");
			AssertPopulateAPPlusCodesSOW(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingPartySOW");
			AssertPopulateAPPlusCodesUseRegistry("SendingPartySOA", "SendingPartyCI5", false);

			AssertPopulateAPPlusCodesCI5(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "ForwarderCI5");
			AssertPopulateAPPlusCodesSOA(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "ForwarderSOA");
			AssertPopulateAPPlusCodesSON(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "ForwarderSON");
			AssertPopulateAPPlusCodesSOW(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "ForwarderSOW");
			AssertPopulateAPPlusCodesUseRegistry("ForwarderSON", "ForwarderCI5", true);

			AssertPopulateAPPlusCodesCI5(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingForwarderCI5");
			AssertPopulateAPPlusCodesSON(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingForwarderSON");

			AssertPopulateAPPlusCodesCI5(consol, (OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, "ReceivingForwarderCI5");
			AssertPopulateAPPlusCodesSON(consol, (OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, "ReceivingForwarderSON");

			consol.JK_OA_DeparturePackCFSTransportAddress = ZGuid.Empty;

			var pickupCartageCoAddr = consol.TopLevelShipments.Cast<CommonShipment>().FirstOrDefault().DocsAndCartage.PickupCartageCoAddr;
			AssertPopulateAPPlusCodesCI5(consol, pickupCartageCoAddr, "TransporterCI5");
			AssertPopulateAPPlusCodesSON(consol, pickupCartageCoAddr, "TransporterSON");
		}

		void AssertPopulateAPPlusCodesCI5(ForwardingConsol consol, OrgAddress address, string ci5PropertyName)
			=> AssertPopulateAPPlusCodes(consol, address, ci5PropertyName, OrgCusCode.FranceCodeTypes.CI5);

		void AssertPopulateAPPlusCodesSOA(ForwardingConsol consol, OrgAddress address, string soaPropertyName)
			=> AssertPopulateAPPlusCodes(consol, address, soaPropertyName, OrgCusCode.FranceCodeTypes.SOA);

		void AssertPopulateAPPlusCodesSON(ForwardingConsol consol, OrgAddress address, string sonPropertyName)
			=> AssertPopulateAPPlusCodes(consol, address, sonPropertyName, OrgCusCode.FranceCodeTypes.SON);

		void AssertPopulateAPPlusCodesSOW(ForwardingConsol consol, OrgAddress address, string sowPropertyName)
			=> AssertPopulateAPPlusCodes(consol, address, sowPropertyName, OrgCusCode.FranceCodeTypes.SOW);

		void AssertPopulateAPPlusCodes(ForwardingConsol consol, OrgAddress address, string propertyName, string franceCodeType)
		{
			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var code1 = address.Header.CustomsCodes.AddNew();
			code1.OK_CodeType = franceCodeType;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			code1.OK_CustomsRegNo = "S001";

			var data = builder.Build();

			AssertEquals($"{propertyName} should be from org", "S001", ((RegistrationNumber)data[propertyName]).Value);

			var code2 = address.CustomsCodes.AddNew();
			code2.OK_CodeType = franceCodeType;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			code2.OK_CustomsRegNo = "S002";

			data = builder.Build();

			AssertEquals($"{propertyName} should be from address", "S002", ((RegistrationNumber)data[propertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			data = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)data[propertyName]).Value);
		}

		void AssertPopulateAPPlusCodesUseRegistry(string sPropertyName, string ci5PropertyName, bool isForwarderCode)
		{
			var consol = Factory.New<ForwardingConsol>();
			CreateAddresses(consol, true);

			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();
			consol.JK_OA_DepartureCTOAddress = operationalAddress.PK;

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);
			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.Code = "FRFR2";
			}

			operationalAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = unloco.Code;
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				var data = builder.Build();
				AssertEquals($"{ci5PropertyName} should be from registry", isForwarderCode ? "FORWARDER" : "AGENT", ((RegistrationNumber)data[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;
			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				var data = builder.Build();
				AssertEquals($"{sPropertyName} should be from registry", isForwarderCode ? "FORWARDER" : "AGENT", ((RegistrationNumber)data[sPropertyName]).Value);
			}
		}

		public void TestPopulateDeliveryDetails()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreateAddresses(consol);

			var ctoAddress = consol.DepartureCTOAddress;
			var psnCode = ctoAddress.Header.CustomsCodes.AddNew();
			psnCode.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			psnCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			psnCode.OK_CustomsRegNo = "Area\\Location";

			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertEquals("PortArea", "Area", data.PortArea);
			AssertEquals("PortLocation", "Location", data.PortLocation);
		}

		public void TestPopulateAMQReference()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();
			var container = data.Containers.FirstOrDefault();

			CreateEvent(consol, AutoEvents.MessageSent);

			AssertEquals("MAA event hasn't been received", string.Empty, container.AMQReference);

			CreateEvent(consol, AutoEvents.MessageAccepted, "X00001");
			data = builder.Build();
			container = data.Containers.FirstOrDefault();

			AssertEquals("Not correct container", string.Empty, container.AMQReference);

			CreateEvent(consol, AutoEvents.MessageAccepted, "X00001", container.Number);
			data = builder.Build();
			container = data.Containers.FirstOrDefault();

			AssertEquals("Populated from RFN parameter of MAA event", "X00001", container.AMQReference);
		}

		void CreateEvent(ForwardingConsol shipment, Event @event, string reference = "", string containerNumber = "")
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.PortsContainerAdviceToBookingAMQ));

			if (!string.IsNullOrEmpty(containerNumber))
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber, containerNumber));
			}

			if (!string.IsNullOrEmpty(reference))
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, reference));
			}

			shipment.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		#endregion

		#region Test Validations

		public void TestAddressValidations()
		{
			var consol = Factory.New<ForwardingConsol>();

			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertPartyNameAndAddressValidation(data.SendingParty, "Sending Party");
			AssertPartyNameAndAddressValidation(data.Carrier, "Carrier");
			AssertPartyNameAndAddressValidation(data.Forwarder, "Forwarder");
		}

		public void TestMissingProviderIDValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOA, "Sending Party", "Agent", "Consol > Sending Agent");
			AssertRequireAPPlusIDValidation(data.FormattedForwarderProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Forwarder", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);

			consol = CreateConsol(FrenchPortsConstants.PCS.MGI);
			builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOA, "Sending Party", "Agent", "Consol > Sending Agent");
			AssertRequireAPPlusIDValidation(data.FormattedForwarderProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Forwarder", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);

			consol = CreateConsol(FrenchPortsConstants.PCS.Soget);
			builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOA, "Sending Party", "Agent", "Consol > Sending Agent");
			AssertRequireAPPlusIDValidation(data.FormattedForwarderProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Forwarder", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);
		}

		void AssertPartyNameAndAddressValidation(Address address, string partyName)
		{
			address.CompanyName = ZString.Empty;
			address.Country.Code = ZString.Empty;
			address.AddressLine1 = ZString.Empty;

			address.ValidateAllIncludingChildren();
			AssertHasMessageError(address.CompanyNameInfo, $"{partyName} name and address is required.");

			address.CompanyName = "Nier";
			address.Country.Code = "CN";
			address.AddressLine1 = "Nier Address1";

			address.ValidateAllIncludingChildren();
			AssertNoMessageError(address.CompanyNameInfo, $"{partyName} name and address is required.");
		}

		public void TestTransporterRequireAPPlusIDValidation()
		{
			var consol = Factory.New<ForwardingConsol>();

			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertHasMessageError("Port Area is required", data.PortAreaInfo, "Port Area is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')");
			AssertHasMessageError("Port Location is required", data.PortLocationInfo, "Port Location is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')");

			data.PortArea = "Area";
			data.PortLocation = "Location";
			data.ValidateAllIncludingChildren();

			AssertNoMessageError(data.PortAreaInfo, "Port Area is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\')");
			AssertNoMessageError(data.PortLocationInfo, "Port Location is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\')");

			data.TransporterSON.Value = "S001";

			data.ValidateAllIncludingChildren();
			AssertNoMessageErrors(data.TransporterSON.ValueInfo);

			data.TransporterCI5.Value = "C001";

			data.ValidateAllIncludingChildren();
			AssertNoMessageErrors(data.TransporterCI5.ValueInfo);
		}

		public void TestContainerValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			var containerBO = consol.Containers.AddNew();

			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters { Data = new[] { containerBO } });
			var data = builder.Build();

			var container = data.Containers.FirstOrDefault();

			AssertNotNull(container);
			AssertHasMessageError(container.NumberInfo, "Container number is required.");

			container.Number = "C000569";
			AssertNoMessageError(container.NumberInfo, "Container number is required.");
		}

		public void TestAsciiCharactersValidation()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerAdviceToBookingBuilder(consol, parameters);
			var data = builder.Build();

			CombineAssertions(() =>
			{
				AssertAsciiCharactersValidation("BookingConfirmationCBK", data.BookingConfirmationCBKInfo);
				AssertAsciiCharactersValidation("OTC", data.OTCInfo);
				AssertAsciiCharactersValidation("ATP", data.ATPReferenceInfo);
				AssertAsciiCharactersValidation("OTCReference", data.OTCReferenceInfo);

				AssertAsciiCharactersValidation("Containers.HandlingNotes", data.Containers.First().HandlingNotesInfo);
				AssertAsciiCharactersValidation("Containers.PackingLines.GoodsDescription", data.Containers.First().PackingLines.First().GoodsDescriptionInfo);
				AssertAsciiCharactersValidation("Containers.FumigationService.ServiceNote", ((AdditionalService)data.Containers.First().FumigationService).ServiceNoteInfo);
			});
		}

		public void TestBookingConfirmationCBKValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_BookingReference = "";
			var messageError = "Carrier Booking Ref or CBK reference is required before sending this message.";
			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertHasMessageError(data.CarrierBookingReferenceInfo, messageError);
			AssertHasMessageError(data.BookingConfirmationCBKInfo, messageError);

			data.BookingConfirmationCBK = "CCC";
			data.ValidateAllIncludingChildren();

			AssertNoMessageError(data.CarrierBookingReferenceInfo, messageError);
			AssertNoMessageError(data.BookingConfirmationCBKInfo, messageError);
		}

		public void TestAdditionalValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var messageError = "OTC Reference or ATP reference or Voyage Number is required before sending this message.";
			var builder = new ContainerAdviceToBookingBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertHasMessageError(data.OTCReferenceInfo, messageError);
			AssertHasMessageError(data.ATPReferenceInfo, messageError);
			AssertHasMessageError(data.VoyageNumberInfo, messageError);
			AssertHasMessageError("PCS message error", data.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");

			data.OTCReference = "CCC";
			data.PCS = FrenchPortsConstants.PCS.MGI;
			data.ValidateAllIncludingChildren();
			AssertNoMessageError(data.OTCReferenceInfo, messageError);
			AssertNoMessageError(data.ATPReferenceInfo, messageError);
			AssertNoMessageError(data.VoyageNumberInfo, messageError);
			AssertNoMessageErrors("PCS message error", data.PCSInfo);
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol(string pcs = FrenchPortsConstants.PCS.MGI)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "B0001100";

			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			CreateTransports(consol);
			CreatePackingLinesAndContainers(consol);
			CreateAddresses(consol, pcs: pcs);

			return consol;
		}

		void CreateTransports(ForwardingConsol consol)
		{
			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "FRPRA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Random Vesel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "111";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "FRPRA";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2018, 12, 1);
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_DepartReference = "AOEUI1234";
			sailing.Destination.JB_ArrivalReference = "QWERTY123";
			transport.JW_JX = sailing.PK;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";
		}

		void CreateAddresses(ForwardingConsol consol, bool createPickupCartageCoAddr = false, string pcs = FrenchPortsConstants.PCS.MGI)
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "MAERSK";
			sendingForwarder.OH_RL_NKClosestPort = "DKAAL";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Aalborg";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivinggForwarder = Factory.New<OrgHeader>();
			receivinggForwarder.OH_FullName = "MAERSK";
			receivinggForwarder.OH_RL_NKClosestPort = "DKAAL";
			receivinggForwarder.MainAddress.Address1 = "Unit 15";
			receivinggForwarder.MainAddress.Address2 = "5 Lost Lane";
			receivinggForwarder.MainAddress.City = "Aalborg";
			receivinggForwarder.MainAddress.Postcode = "2022";
			receivinggForwarder.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ReceivingForwarderAddress = receivinggForwarder.MainAddress.PK;

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

			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);
			unloco.RefLocoMaps.DeleteAll();
			var mapFRSoget = unloco.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = pcs;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Sending Stuff";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Conficious Ave";
			departureCTOAddress.MainAddress.Postcode = "10000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			departureCTOAddress.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var departurePackCFSTransportAddress = Factory.New<OrgHeader>();
			departurePackCFSTransportAddress.OH_FullName = "I'm Receiving Stuff";
			departurePackCFSTransportAddress.OH_RL_NKClosestPort = "AUSYD";
			departurePackCFSTransportAddress.MainAddress.Address1 = "Unit 399";
			departurePackCFSTransportAddress.MainAddress.Address2 = "50 What Lane";
			departurePackCFSTransportAddress.MainAddress.City = "Sydney";
			departurePackCFSTransportAddress.MainAddress.Postcode = "5023";
			departurePackCFSTransportAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_DeparturePackCFSTransportAddress = departurePackCFSTransportAddress.MainAddress.PK;

			if (createPickupCartageCoAddr)
			{
				var pickupCartageCoAddr = Factory.New<OrgHeader>();
				pickupCartageCoAddr.OH_FullName = "Shipment Pickup Address";
				pickupCartageCoAddr.OH_RL_NKClosestPort = "AUSYD";
				pickupCartageCoAddr.MainAddress.Address1 = "Unit 12";
				pickupCartageCoAddr.MainAddress.Address2 = "60 What Street";
				pickupCartageCoAddr.MainAddress.City = "Sydney";
				pickupCartageCoAddr.MainAddress.Postcode = "2032";
				pickupCartageCoAddr.MainAddress.OA_RN_NKCountryCode = "AU";

				var shipment = consol.Shipments.AddNew();
				shipment.DocsAndCartage.PickupCartageCoPK = pickupCartageCoAddr.PK;
			}
		}

		void CreatePackingLinesAndContainers(ForwardingConsol consol)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_IsEmptyContainer = true;
			container.JC_ContainerCount = 1;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_OverhangBack = 1.1m;
			container.JC_OverhangRight = 2.1m;
			container.JC_ExportDepotCustomsReference = "ExportReference";

			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			var handlingNote = container.Notes.AddNew();
			handlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			handlingNote.ST_NoteText = "container handling note";

			var fumigationService = container.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceNote = "fumigation note";

			var contractor = Factory.New<OrgHeader>();
			contractor.OH_FullName = "CSHIPING";
			contractor.OH_RL_NKClosestPort = "FRPAR";
			contractor.MainAddress.Address1 = "Unit 18";
			contractor.MainAddress.Address2 = "5 Lost Lane";
			contractor.MainAddress.City = "PARIS";
			contractor.MainAddress.Postcode = "2000";
			contractor.MainAddress.OA_RN_NKCountryCode = "FR";
			fumigationService.ES_OH_Contractor = contractor.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_PackageCount = 2;
			packingLine.JL_F3_NKPackType = "PLT";
			packingLine.JL_ActualWeight = 200;
			packingLine.JL_ActualWeightUQ = "KG";
			packingLine.JL_ActualVolume = 300;
			packingLine.JL_ActualVolumeUQ = "M3";
			packingLine.JL_HarmonisedCode = "ABCDE";
			packingLine.JL_ExportRefNumber = "REF001";
			packingLine.JL_DetailedDescription = "pack1";
			packingLine.JL_ContainerPackingOrder = 1;

			packingLine.JL_RequiresTemperatureControl = true;
			packingLine.JL_RequiredTemperatureUnit = "C";
			packingLine.JL_RequiresTemperatureControl = true;
			packingLine.JL_RequiredTemperatureMinimum = 10;
			packingLine.JL_RequiredTemperatureMaximum = 30;

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "6666", "E", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "6666";
				subs.DG_Variant = "E";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			var undg = packingLine.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.Substance.DG_State = "L";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";

			container.PackLines.Add(packingLine);
		}

		#endregion
	}
}
