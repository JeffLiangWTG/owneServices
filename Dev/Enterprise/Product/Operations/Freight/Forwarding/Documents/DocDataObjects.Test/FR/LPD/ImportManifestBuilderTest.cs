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
	sealed class ImportManifestBuilderTest : TestCaseWithFactory
	{
		#region Test Build

		public void TestBuild()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .ToArray()
			};

			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();

			CombineAssertions(() =>
			{
				AssertEquals("ConsolNumber", "C00001000", data.ConsolNumber);
				Assert("RequiresTemperatureControl", data.RequiresTemperatureControl);
				AssertEquals("TemperatureMinimum.Value", 15m, data.TemperatureMinimum.Value);
				AssertEquals("TemperatureMinimum.Unit.Code", "C", data.TemperatureMinimum.Unit.Code);
				AssertEquals("TemperatureMaximum.Value", 25m, data.TemperatureMaximum.Value);
				AssertEquals("TemperatureMaximum.Unit.Code", "C", data.TemperatureMaximum.Unit.Code);
				AssertEquals("PortOfOrigin", "AUSYD", data.PortOfOrigin.Code);
				AssertEquals("PortOfTranshipment", "NZAKL", data.PortOfTranshipment.Code);
				AssertEquals("PortOfDestination", "FRPRA", data.PortOfDestination.Code);
				AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);
				AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
				AssertEquals("ETA", new ZDateTime(2019, 1, 1), data.ETA);
				AssertEquals("VesselName", "Miranda", data.VesselName);
				AssertEquals("OTCReference", string.Empty, data.OTCReference);
				AssertEquals("OTCReference", "333", data.VoyageNumber);
				AssertEquals("ATPReference", "QWERTY123", data.ATPReference);
				AssertEquals("ContainerMode", "LCL", data.ContainerMode.Code);
				AssertEquals("ShipmentType", "AGT", data.ShipmentType.Code);
				AssertEquals("Containers.Count", 1, data.Containers.Count);
				AssertEquals("Containers.Count", 1, data.GoodsDetails.Count);
			});

			AssertAddressData((OrgAddress)consol.SendingForwarderWithContact.OrgAddress, data.SendingForwarder);
			AssertAddressData((OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, data.ReceivingForwarder);
			AssertAddressData(consol.ArrivalUnpackCFSTransportAddress, data.Transporter);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, data.SendingParty);
			AssertAddressData(consol.ShippingLine, data.Carrier);

			var container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Number", "AAAA0000007", container.Number);
				AssertEquals("ContainerCount", 1, container.ContainerCount);
				AssertEquals("DeliveryMode", "CY/CY", container.DeliveryMode);
				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
			});

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
				AssertEquals("DG", "5 Bag of UN6666, DG SHIPPER NAME, (WHATEVER), (sub1), PG GRO, (15.0C c.c.), N, LTD QTY", undg.ToString());
			});

			var goodsDetail = data.GoodsDetails.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentNumber", "SH0001000", goodsDetail.ShipmentNumber);
				AssertEquals("HouseBillNumber", "H0000056", goodsDetail.HouseBillNumber);
				AssertEquals("PortOfOrigin", "AUSYD", goodsDetail.PortOfOrigin.Code);
				Assert("RequiresTemperatureControl", goodsDetail.RequiresTemperatureControl);
				Assert("HazardousCargo", goodsDetail.HazardousCargo);
				AssertEquals("MarksAndNumbers", "Marks", goodsDetail.MarksAndNumbers);
				AssertEquals("GoodsDescription", "Goods description", goodsDetail.GoodsDescription);
				AssertEquals("TaxAmount.Currency.Code", "AUD", goodsDetail.TaxAmount.Currency.Code);
				AssertEquals("GoodsHandlingNotes", "shipment handling note", goodsDetail.GoodsHandlingNotes);
				AssertEquals("PackingLines.Count", 1, goodsDetail.PackingLines.Count);
			});

			var shipment = (ForwardingShipment)consol.Shipments.FirstOrDefault();
			AssertAddressData(shipment.ConsigneeDocumentaryAddress, goodsDetail.Consignee);
			AssertAddressData(shipment.ConsignorDocumentaryAddress, goodsDetail.Shipper);
			AssertAddressData(shipment.NotifyPartyDocumentaryAddress, goodsDetail.NotifyParty);
			AssertAddressData(shipment.NotifyParty2DocumentaryAddress, goodsDetail.NotifyParty2);
		}

		public void TestValidations()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();

			data.BillOfLadingNumber = ZString.Empty;

			data.ValidateAllIncludingChildren();

			AssertHasMessageError("BOL Error", data.BillOfLadingNumberInfo, "Bill of Lading (BOL) Number is required.");
		}

		public void TestPopulateSendingParty()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "AU";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var parameters = new DummyDocDataObjectParameters();

			var builder = new ImportManifestBuilder(consol, parameters);

			var data = builder.Build();

			AssertAddressData(branchProxy.MainAddress, data.SendingParty);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			data = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, data.SendingParty);
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

			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
			AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);

			consol.UnpackDepotAddress.Delete();
			data = builder.Build();
			AssertEquals("PCS", ZString.Empty, data.PCS);
			AssertHasMessageError("PCS Error", data.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", "", data.OperationalPort.Code);
			AssertHasMessageError("OperationalPort Error", ((Unloco)data.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Consol > Arrival > CFS Address.");
		}

		public void TestPopulateAPPlusCodes()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreateConsolAddresses(consol);

			AssertPopulateAPPlusCodes(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingForwarderSON", "SendingForwarderCI5");
			AssertPopulateAPPlusCodes(consol, (OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, "ReceivingForwarderSON", "ReceivingForwarderCI5");
			AssertPopulateAPPlusCodes(consol, consol.ArrivalUnpackCFSTransportAddress, "TransporterSON", "TransporterCI5");
			AssertPopulateAPPlusCodes(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartySON", "SendingPartyCI5");

			AssertPopulateAPPlusCodes(consol, consol.ShippingLineAddress, "CarrierSON", "CarrierCI5");

			AssertPopulateAPPlusCodesUseRegistry("SendingPartySON", "SendingPartyCI5");
			AssertPopulateAPPlusCodesUseRegistry("ReceivingForwarderSON", "ReceivingForwarderCI5");
		}

		public void TestPopulateGoodsDetailsAPPlusCode()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .ToArray()
			};

			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();

			var goodDetail = data.GoodsDetails.First();

			var receivingForwarderWithContactOrgHeader = consol.ReceivingForwarderWithContact.OrgHeader as OrgHeader;

			AssertAddressData(receivingForwarderWithContactOrgHeader, goodDetail.ThirdParty);
			AssertEquals(ZString.Empty, goodDetail.ThirdPartyCI5.Value);
			AssertEquals(ZString.Empty, goodDetail.ThirdPartySON.Value);

			var sonCode1 = receivingForwarderWithContactOrgHeader.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = receivingForwarderWithContactOrgHeader.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code1.OK_CustomsRegNo = "C001";

			data = builder.Build();
			goodDetail = data.GoodsDetails.First();

			AssertAddressData(receivingForwarderWithContactOrgHeader, goodDetail.ThirdParty);
			AssertEquals("C001", goodDetail.ThirdPartyCI5.Value);
			AssertEquals("S001", goodDetail.ThirdPartySON.Value);

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);

			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.RL_Code = "FRFR2";
			}

			var shipment = consol.Shipments.OfType<ForwardingShipment>().First(s => !s.IsMasterShipmentRepresentingAllChildShipments);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = unloco.Code;
			var orgAddress = orgHeader.MainAddress;

			shipment.JS_OH_DeliveryAgent = orgHeader.PK;

			var sonCode2 = orgHeader.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = orgHeader.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code2.OK_CustomsRegNo = "C002";

			data = builder.Build();
			goodDetail = data.GoodsDetails.First();

			AssertAddressData(orgHeader, goodDetail.ThirdParty);
			AssertEquals("C002", goodDetail.ThirdPartyCI5.Value);
			AssertEquals("S002", goodDetail.ThirdPartySON.Value);

			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT222";
			code.ForwarderCode = "FORWARDER222";
			code.Port = "FRFR2";
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();
			consol.JK_OA_UnpackDepotAddress = operationalAddress.PK;
			operationalAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				data = builder.Build();
				goodDetail = data.GoodsDetails.First();

				AssertEquals("FORWARDER222", goodDetail.ThirdPartyCI5.Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				data = builder.Build();
				goodDetail = data.GoodsDetails.First();

				AssertEquals("FORWARDER222", goodDetail.ThirdPartySON.Value);
			}

			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;

			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				data = builder.Build();
				goodDetail = data.GoodsDetails.First();

				AssertEquals("FORWARDER222", goodDetail.ThirdPartyCI5.Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				data = builder.Build();
				goodDetail = data.GoodsDetails.First();

				AssertEquals("FORWARDER222", goodDetail.ThirdPartySON.Value);
			}
		}

		void AssertPopulateAPPlusCodes(ForwardingConsol consol, OrgAddress address, string sonPropertyName, string ci5PropertyName)
		{
			var parameters = new DummyDocDataObjectParameters();
			var builder = new ImportManifestBuilder(consol, parameters);
			var sonCode1 = address.Header.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = address.Header.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code1.OK_CustomsRegNo = "C001";

			var data = builder.Build();

			AssertEquals($"{sonPropertyName} should be from org", "S001", ((RegistrationNumber)data[sonPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from org", "C001", ((RegistrationNumber)data[ci5PropertyName]).Value);

			var sonCode2 = address.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = address.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code2.OK_CustomsRegNo = "C002";

			data = builder.Build();

			AssertEquals($"{sonPropertyName} should be from address", "S002", ((RegistrationNumber)data[sonPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from address", "C002", ((RegistrationNumber)data[ci5PropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			data = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)data[sonPropertyName]).Value);
			AssertEquals(string.Empty, ((RegistrationNumber)data[ci5PropertyName]).Value);
		}

		void AssertPopulateAPPlusCodesUseRegistry(string sPropertyName, string ci5PropertyName)
		{
			var consol = Factory.New<ForwardingConsol>();
			CreateConsolAddresses(consol);

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();
			consol.JK_OA_UnpackDepotAddress = operationalAddress.PK;

			var parameters = new DummyDocDataObjectParameters();

			var builder = new ImportManifestBuilder(consol, parameters);

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);

			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.RL_Code = "FRFR2";
			}

			operationalAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = "FRFR2";
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{ci5PropertyName} should be from registry", "FORWARDER", ((RegistrationNumber)builder.Build()[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{sPropertyName} should be from registry", "FORWARDER", ((RegistrationNumber)builder.Build()[sPropertyName]).Value);
			}
		}

		public void TestPopulateLPDReference()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray()
			};
			CreateEvent(consol, AutoEvents.MessageSent);

			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();
			var container = data.Containers.FirstOrDefault();

			AssertEquals("MAA event hasn't been received", string.Empty, container.LPDReference);

			CreateEvent(consol, AutoEvents.MessageAccepted, "X00001");
			data = builder.Build();
			container = data.Containers.FirstOrDefault();

			AssertEquals("Not correct container", string.Empty, container.LPDReference);

			CreateEvent(consol, AutoEvents.MessageAccepted, "X00001", container.Number);
			data = builder.Build();
			container = data.Containers.FirstOrDefault();
			AssertEquals("Populated from RFN parameter of MAA event", "X00001", container.LPDReference);
		}

		void CreateEvent(ForwardingConsol shipment, Event @event, string reference = "", string containerNumber = "")
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.ProvisionalUnpackingListLPD));

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

		public void TestLPDStatusValidation()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .ToArray()
			};

			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();
			var container = data.Containers.FirstOrDefault();
			var lpdError = "LPD Status is required.";

			AssertEquals("LPDStatus", "VAL", container.LPDStatus);
			Assert(!container.IsLPDStatusProvisional);
			Assert(container.IsLPDStatusFinalized);
			Assert(!container.IsLPDStatusFinalizedNoPacksWeight);
			Assert(!container.IsLPDStatusFinalizedNoPacksOnly);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusProvisionalInfo);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusFinalizedInfo);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusFinalizedNoPacksWeightInfo);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusFinalizedNoPacksOnlyInfo);

			container.IsLPDStatusProvisional = true;

			AssertEquals("LPDStatus", "PRO", container.LPDStatus);
			Assert(container.IsLPDStatusProvisional);
			Assert(!container.IsLPDStatusFinalized);
			Assert(!container.IsLPDStatusFinalizedNoPacksWeight);
			Assert(!container.IsLPDStatusFinalizedNoPacksOnly);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusProvisionalInfo);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusFinalizedInfo);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusFinalizedNoPacksWeightInfo);
			AssertNoMessageErrors(lpdError, container.IsLPDStatusFinalizedNoPacksOnlyInfo);
		}

		public void TestPortLocationAndAreaValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "I'm Receiving Stuff";
			unpackDepotAddress.OH_RL_NKClosestPort = "AUSYD";
			unpackDepotAddress.MainAddress.Address1 = "Unit 399";
			unpackDepotAddress.MainAddress.Address2 = "50 What Lane";
			unpackDepotAddress.MainAddress.City = "Sydney";
			unpackDepotAddress.MainAddress.Postcode = "5023";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;
			var parameters = new DummyDocDataObjectParameters();
			var builder = new ImportManifestBuilder(consol, parameters);

			var data = builder.Build();

			AssertEquals("PortLocation", string.Empty, data.PortLocation);
			AssertEquals("PortArea", string.Empty, data.PortArea);
			AssertHasMessageError("Port Area is required", data.PortAreaInfo, "Port Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertHasMessageError("Port Location is required", data.PortLocationInfo, "Port Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");

			var cfsAddress = consol.UnpackDepotAddress;
			var psnCode = cfsAddress.Header.CustomsCodes.AddNew();
			psnCode.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			psnCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			psnCode.OK_CustomsRegNo = "Area\\Location";
			data = builder.Build();

			AssertEquals("PortLocation", "Location", data.PortLocation);
			AssertEquals("PortArea", "Area", data.PortArea);
			AssertNoMessageError("Port Area is required", data.PortAreaInfo, "Port Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertNoMessageError("Port Location is required", data.PortLocationInfo, "Port Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");
		}

		public void TestGoodsDetailsValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();

			var parameters = new DummyDocDataObjectParameters();
			var builder = new ImportManifestBuilder(consol, parameters);
			var data = builder.Build();
			var goodsDetail = data.GoodsDetails.FirstOrDefault();

			goodsDetail.PortOfOrigin.Code = "";
			goodsDetail.ShipmentNumber = "";
			goodsDetail.HouseBillNumber = "";

			AssertHasMessageError(goodsDetail.PortOfOrigin.CodeInfo, "Origin Port is required.");
			AssertHasMessageError(goodsDetail.ShipmentNumberInfo, "Shipment Number is required.");
			AssertHasMessageError(goodsDetail.HouseBillNumberInfo, "House Bill Number is required.");
			AssertHasMessageError(goodsDetail.GoodsDescriptionInfo, "Goods Description is required.");
			AssertHasMessageError(goodsDetail.MarksAndNumbersInfo, "Marks & Numbers are required.");

			goodsDetail.PortOfOrigin.Code = "AU";
			goodsDetail.ShipmentNumber = "1001";
			goodsDetail.HouseBillNumber = "1002";
			goodsDetail.GoodsDescription = "12";
			goodsDetail.MarksAndNumbers = "123";
			goodsDetail.ValidateAllIncludingChildren();

			AssertNoMessageError(goodsDetail.PortOfOrigin.CodeInfo, "Origin Port is required.");
			AssertNoMessageError(goodsDetail.ShipmentNumberInfo, "Shipment Number is required.");
			AssertNoMessageError(goodsDetail.HouseBillNumberInfo, "House Bill Number is required.");
			AssertNoMessageError(goodsDetail.GoodsDescriptionInfo, "Goods Description is required.");
			AssertNoMessageError(goodsDetail.MarksAndNumbersInfo, "Marks & Numbers are required.");
		}

		public void TestGoodsDetails_ShouldExcluded()
		{
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.CoLoadMaster);
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.BlindCoLoadMaster);
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.AssemblyMaster);
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.BuyersConsolLead, 3, false);
		}

		void AssertGoodsDetails_ShouldExcluded(string shipmentType, int count = 2, bool isExcluded = true)
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000008";

			var shipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_ShipmentType = shipmentType;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.JS_UniqueConsignRef = "s0001";

			var subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.JS_UniqueConsignRef = "s0002";

			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .Where(c => c.JC_ContainerNum == "AAAA0000008")
				  .ToArray()
			};
			var data = new ImportManifestBuilder(consol, parameters).Build();

			AssertEquals(count, data.GoodsDetails.Count);
			AssertEquals(isExcluded, !data.GoodsDetails.Any(g => g.ShipmentNumber == shipment.JS_UniqueConsignRef));
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == subshipment1.JS_UniqueConsignRef));
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == subshipment2.JS_UniqueConsignRef));
		}

		public void TestValidateAddresses()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters();
			var builder = new ImportManifestBuilder(consol, parameters);

			var data = builder.Build();

			data.ReceivingForwarder.CompanyName = ZString.Empty;
			data.ReceivingForwarder.Country.Code = ZString.Empty;
			data.ReceivingForwarder.AddressLine1 = ZString.Empty;

			data.SendingParty.CompanyName = ZString.Empty;
			data.SendingParty.Country.Code = ZString.Empty;
			data.SendingParty.AddressLine1 = ZString.Empty;

			AssertHasMessageError(data.ReceivingForwarder.CompanyNameInfo, "Receiving Agent name and address is required.");
			AssertHasMessageError(data.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");

			data.ReceivingForwarder.CompanyName = "Nier";
			data.ReceivingForwarder.Country.Code = "CN";
			data.ReceivingForwarder.AddressLine1 = "Nier Address1";

			data.SendingParty.CompanyName = "Tom";
			data.SendingParty.Country.Code = "CN";
			data.SendingParty.AddressLine1 = "Tom Address1";

			AssertNoMessageError(data.ReceivingForwarder.CompanyNameInfo, "Receiving Agent name and address is required.");
			AssertNoMessageError(data.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");

			consol.JK_RL_NKDischargePort = "RECAF";
			data = builder.Build();

			foreach (var goodsDetail in data.GoodsDetails)
			{
				goodsDetail.Shipper.CompanyName = ZString.Empty;
				goodsDetail.Shipper.City = ZString.Empty;
				goodsDetail.Shipper.Country.Code = ZString.Empty;
				goodsDetail.Shipper.AddressLine1 = ZString.Empty;
				goodsDetail.Shipper.AddressLine2 = ZString.Empty;

				AssertHasMessageError(goodsDetail.Shipper.CompanyNameInfo, "Shipper company name is required as per SIMAR requirement.");
				AssertHasMessageError(goodsDetail.Shipper.AddressLine1Info, "Shipper address is required as per SIMAR requirement.");
				AssertHasMessageError(goodsDetail.Shipper.CityInfo, "Shipper city is required as per SIMAR requirement.");
				AssertHasMessageError(goodsDetail.Shipper.Country.NameInfo, "Shipper country is required as per SIMAR requirement.");

				goodsDetail.Shipper.CompanyName = "Shippy";
				goodsDetail.Shipper.City = "ShipVille";
				goodsDetail.Shipper.Country.Code = "RE";
				goodsDetail.Shipper.AddressLine1 = "ShippyLane";

				AssertNoMessageError(goodsDetail.Shipper.CompanyNameInfo, "Shipper company name is required as per SIMAR requirement.");
				AssertNoMessageError(goodsDetail.Shipper.AddressLine1Info, "Shipper address is required as per SIMAR requirement.");
				AssertNoMessageError(goodsDetail.Shipper.CityInfo, "Shipper city is required as per SIMAR requirement.");
				AssertNoMessageError(goodsDetail.Shipper.Country.NameInfo, "Shipper country is required as per SIMAR requirement.");

				goodsDetail.Consignee.CompanyName = ZString.Empty;
				goodsDetail.Consignee.City = ZString.Empty;
				goodsDetail.Consignee.Country.Code = ZString.Empty;
				goodsDetail.Consignee.AddressLine1 = ZString.Empty;
				goodsDetail.Consignee.AddressLine2 = ZString.Empty;

				AssertHasMessageError(goodsDetail.Consignee.CompanyNameInfo, "Consignee company name is required as per SIMAR requirement.");
				AssertHasMessageError(goodsDetail.Consignee.AddressLine1Info, "Consignee address is required as per SIMAR requirement.");
				AssertHasMessageError(goodsDetail.Consignee.CityInfo, "Consignee city is required as per SIMAR requirement.");
				AssertHasMessageError(goodsDetail.Consignee.Country.NameInfo, "Consignee country is required as per SIMAR requirement.");

				goodsDetail.Consignee.CompanyName = "Conny";
				goodsDetail.Consignee.City = "ConnyVille";
				goodsDetail.Consignee.Country.Code = "FR";
				goodsDetail.Consignee.AddressLine1 = "ConnyLane";

				AssertNoMessageError(goodsDetail.Consignee.CompanyNameInfo, "Consignee company name is required as per SIMAR requirement.");
				AssertNoMessageError(goodsDetail.Consignee.AddressLine1Info, "Consignee address is required as per SIMAR requirement.");
				AssertNoMessageError(goodsDetail.Consignee.CityInfo, "Consignee city is required as per SIMAR requirement.");
				AssertNoMessageError(goodsDetail.Consignee.Country.NameInfo, "Consignee country is required as per SIMAR requirement.");
			}
		}

		public void TestMissingProviderIDValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new ImportManifestBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Sending Party", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(data.FormattedReceivingForwarderProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Receiving Agent", "Forwarder", "Consol > Receiving Agent");
			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", string.Empty, "Consol > Carrier", true);

			foreach (var goodsDetail in data.GoodsDetails)
			{
				AssertRequireAPPlusIDValidation(goodsDetail.ThirdPartyProviderID.ValueInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Third Party", "Forwarder", "Consol > Receiving Agent", extraConfigPath: "Shipment > Delivery > Delivery Agent");
			}

			consol = CreateConsol(FrenchPortsConstants.PCS.MGI);
			builder = new ImportManifestBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Sending Party", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(data.FormattedReceivingForwarderProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Receiving Agent", "Forwarder", "Consol > Receiving Agent");
			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", string.Empty, "Consol > Carrier", true);

			foreach (var goodsDetail in data.GoodsDetails)
			{
				AssertRequireAPPlusIDValidation(goodsDetail.ThirdPartyProviderID.ValueInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Third Party", "Forwarder", "Consol > Receiving Agent", extraConfigPath: "Shipment > Delivery > Delivery Agent");
			}

			consol = CreateConsol(FrenchPortsConstants.PCS.Soget);
			builder = new ImportManifestBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Sending Party", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(data.FormattedReceivingForwarderProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Receiving Agent", "Forwarder", "Consol > Receiving Agent");
			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", string.Empty, "Consol > Carrier", true);

			foreach (var goodsDetail in data.GoodsDetails)
			{
				AssertRequireAPPlusIDValidation(goodsDetail.ThirdPartyProviderID.ValueInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Third Party", "Forwarder", "Consol > Receiving Agent", extraConfigPath: "Shipment > Delivery > Delivery Agent");
			}
		}

		public void TestValidateReferences()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new ImportManifestBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			var allReferencesAreEmptyMessageError = "Voyage Number, OTC Reference or ATP reference is required before sending this message.";

			CombineAssertions(() =>
			{
				AssertEquals("OTCReference", string.Empty, data.OTCReference);
				AssertEquals("ATPReference", string.Empty, data.ATPReference);
				AssertEquals("VoyageNumber", string.Empty, data.VoyageNumber);

				AssertHasMessageError("OTC is mandatory", data.OTCReferenceInfo, allReferencesAreEmptyMessageError);
				AssertHasMessageError("Voyage Number or ATP is required", data.ATPReferenceInfo, allReferencesAreEmptyMessageError);
				AssertHasMessageError("Voyage Number or ATP is required", data.VoyageNumberInfo, allReferencesAreEmptyMessageError);
			});

			data.OTCReference = "OTC";

			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.OTCReferenceInfo, allReferencesAreEmptyMessageError);
			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.ATPReferenceInfo, allReferencesAreEmptyMessageError);
			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.VoyageNumberInfo, allReferencesAreEmptyMessageError);

			data.OTCReference = string.Empty;
			data.ATPReference = "ATP";

			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.ATPReferenceInfo, allReferencesAreEmptyMessageError);
			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.VoyageNumberInfo, allReferencesAreEmptyMessageError);

			data.ATPReference = string.Empty;
			data.VoyageNumber = "V0001";

			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.ATPReferenceInfo, allReferencesAreEmptyMessageError);
			AssertNoMessageError("One of Voyage Number/ATP/OTC is required", data.VoyageNumberInfo, allReferencesAreEmptyMessageError);
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol(string pcs = FrenchPortsConstants.PCS.MGI)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "FRPRA";
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
			CreateConsolAddresses(consol, pcs);

			return consol;
		}

		void CreateTransports(ForwardingConsol consol)
		{
			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

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
			transport3.JW_RL_NKDiscPort = "FRPRA";
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";
			transport3.JW_ETA = new ZDateTime(2019, 1, 1);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Test Vessel Name 2";
			vessel.RV_LloydsNumber = "Lllloyd";
			vessel.RV_RadioCallSign = "Radio123";
			transport2.JW_Vessel = vessel.RV_Code;
			transport2.JW_VoyageFlight = "CC456";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Random Vesel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel2.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2019, 1, 1);
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_DepartReference = "AOEUI1234";
			sailing.Destination.JB_ArrivalReference = "QWERTY123";
			transport3.JW_JX = sailing.PK;
		}

		void CreateConsolAddresses(ForwardingConsol consol, string pcs = FrenchPortsConstants.PCS.MGI)
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

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Handling Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 2";
			receivingForwarder.MainAddress.Address2 = "60 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var arrivalUnpackCFSTransportAddress = Factory.New<OrgHeader>();
			arrivalUnpackCFSTransportAddress.OH_FullName = "I'm Receiving Stuff";
			arrivalUnpackCFSTransportAddress.OH_RL_NKClosestPort = "AUSYD";
			arrivalUnpackCFSTransportAddress.MainAddress.Address1 = "Unit 399";
			arrivalUnpackCFSTransportAddress.MainAddress.Address2 = "50 What Lane";
			arrivalUnpackCFSTransportAddress.MainAddress.City = "Sydney";
			arrivalUnpackCFSTransportAddress.MainAddress.Postcode = "5023";
			arrivalUnpackCFSTransportAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalUnpackCFSTransportAddress.MainAddress.PK;

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR"));
			unloco.RefLocoMaps.DeleteAll();
			var mapFRSoget = unloco.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = pcs;

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "I'm Receiving Stuff";
			unpackDepotAddress.MainAddress.Address1 = "Unit 400";
			unpackDepotAddress.MainAddress.Address2 = "50 What Lane";
			unpackDepotAddress.MainAddress.City = "Sydney";
			unpackDepotAddress.MainAddress.Postcode = "5023";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			unpackDepotAddress.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "I'm pack Stuff";
			packDepotAddress.OH_RL_NKClosestPort = "FRPAR";
			packDepotAddress.MainAddress.Address1 = "Unit 400";
			packDepotAddress.MainAddress.Address2 = "66 What Lane";
			packDepotAddress.MainAddress.City = "Sydney";
			packDepotAddress.MainAddress.Postcode = "1024";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			packDepotAddress.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
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

			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			var containerHandlingNote = container.Notes.AddNew();
			containerHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			containerHandlingNote.ST_NoteText = "container handling note";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "H0000056";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPRA";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_MarksAndNumbers = "Marks";
			shipment.JS_GoodsDescription = "Goods description";
			var shipmentHandlingNote = shipment.Notes.AddNew();
			shipmentHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentHandlingNote.ST_NoteText = "shipment handling note";

			CreateShipmentAddress(shipment);

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

		void CreateShipmentAddress(ForwardingShipment shipment)
		{
			var consingee = Factory.New<OrgHeader>();
			consingee.OH_FullName = "I'm Sending Stuff";
			consingee.OH_RL_NKClosestPort = "CNNJI";
			consingee.MainAddress.Address1 = "Unit 200";
			consingee.MainAddress.Address2 = "55 Why Lane";
			consingee.MainAddress.City = "Conficious Ave";
			consingee.MainAddress.Postcode = "10000";
			consingee.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consingee.MainAddress.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "I'm Receiving Stuff";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 399";
			shipper.MainAddress.Address2 = "50 What Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "5023";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "MAERSK";
			notifyParty.OH_RL_NKClosestPort = "DKAAL";
			notifyParty.MainAddress.Address1 = "Unit 13";
			notifyParty.MainAddress.Address2 = "4 Lost Lane";
			notifyParty.MainAddress.City = "Aalborg";
			notifyParty.MainAddress.Postcode = "2000";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "DK";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Handling Stuff";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
		}

		#endregion
	}
}
