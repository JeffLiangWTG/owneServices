using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
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
using static CargoWise.EventReference.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using Constants = Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	public class FinalManifestBuilderTest : TestCaseWithFactory
	{
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

			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();

			CombineAssertions(() =>
			{
				AssertEquals("ConsolNumber", "C00001000", data.ConsolNumber);
				AssertEquals("TemperatureMinimum.Value", 15m, data.TemperatureMinimum.Value);
				AssertEquals("TemperatureMinimum.Unit.Code", "C", data.TemperatureMinimum.Unit.Code);
				AssertEquals("TemperatureMaximum.Value", 25m, data.TemperatureMaximum.Value);
				AssertEquals("TemperatureMaximum.Unit.Code", "C", data.TemperatureMaximum.Unit.Code);
				AssertEquals("VesselName", "Miranda", data.Vessel);
				AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);
				AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
				AssertEquals("Containers.Count", 1, data.Containers.Count);
			});

			AssertAddressData(consol.ShippingLineAddress, data.Carrier);
			AssertAddressData(consol.PackDepotAddress, data.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, data.CurrentUser);

			var container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Number", "AAAA0000007", container.Number);
				AssertEquals("ContainerCount", 1, container.ContainerCount);
				AssertEquals("DeliveryMode", "CY/CY", container.DeliveryMode);
			});
			AssertAddressData(FumigationContractor.MainAddress, container.FumigationService.Contractor);

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

			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();

			data.Containers.ElementAt(0).GoodsWeight.Value = ZDecimal.Zero;
			data.Containers.ElementAt(0).TareWeight.Value = ZDecimal.Zero;
			data.Containers.ElementAt(0).Type.Code = ZString.Empty;
			data.Containers.ElementAt(0).Type.ISOCode = ZString.Empty;

			((Measurement)data.GoodsDetails.ElementAt(0).DeclaredPackWeight).Value = ZDecimal.Zero;
			data.GoodsDetails.ElementAt(0).CommodityReference = ZString.Empty;
			data.GoodsDetails.ElementAt(0).DeclaredPackCount = ZInt.Zero;

			data.GoodsDetails.ElementAt(0).PackingLines.ElementAt(0).Volume.Value = ZDecimal.Zero;
			data.GoodsDetails.ElementAt(0).PackingLines.ElementAt(0).Quantity = ZInt.Zero;
			data.GoodsDetails.ElementAt(0).PackingLines.ElementAt(0).MarksAndNumbers = ZString.Empty;

			data.Carrier.CompanyName = ZString.Empty;
			data.Carrier.Country.Code = ZString.Empty;
			data.Carrier.AddressLine1 = ZString.Empty;

			data.SendingParty.CompanyName = ZString.Empty;
			data.SendingParty.Country.Code = ZString.Empty;
			data.SendingParty.AddressLine1 = ZString.Empty;

			data.PortArea = ZString.Empty;
			data.PortLocation = ZString.Empty;
			data.DeliveryArea = ZString.Empty;
			data.DeliveryLocation = ZString.Empty;

			data.ValidateAllIncludingChildren();

			AssertHasMessageError("GoodsWeight Error", data.Containers.ElementAt(0).GoodsWeight.ValueInfo, "Container goods weight is required.");
			AssertHasMessageError("TareWeight Error", data.Containers.ElementAt(0).TareWeight.ValueInfo, "Container tare weight is required.");
			AssertHasMessageError("Type Error", data.Containers.ElementAt(0).Type.ISOCodeInfo, "This container does not have a valid ISO Code. Enter a valid ISO code here or add it to the Container Reference File via Consol > Container > Container Type.");

			AssertHasMessageError("DeclaredPackWeight Error", ((Measurement)data.GoodsDetails.ElementAt(0).DeclaredPackWeight).ValueInfo, "Declared goods weight is required.");
			AssertHasMessageError("CommodityReference Error", data.GoodsDetails.ElementAt(0).CommodityReferenceInfo, "ECV Reference is required. Export Ref Number missing from Shipment > Packing > Pack Line.");
			AssertHasMessageError("DeclaredPackCount Error", data.GoodsDetails.ElementAt(0).DeclaredPackCountInfo, "Declared pack count is required.");

			AssertHasMessageError("Volume Error", data.GoodsDetails.ElementAt(0).PackingLines.ElementAt(0).Volume.ValueInfo, "Package volume is required.");
			AssertHasMessageError("Quantity Error", data.GoodsDetails.ElementAt(0).PackingLines.ElementAt(0).QuantityInfo, "Number of Packs is required.");
			AssertHasMessageError("MarksAndNumbers Error", data.GoodsDetails.ElementAt(0).PackingLines.ElementAt(0).MarksAndNumbersInfo, "Marks & Numbers are required.");

			AssertHasMessageError(data.Carrier.CompanyNameInfo, "Carrier name and address is required.");
			AssertHasMessageError(data.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");

			AssertHasMessageError("Port Area is required", data.PortAreaInfo, "Port Area is missing from Organization Consol > Departure > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertHasMessageError("Port Location is required.", data.PortLocationInfo, "Port Location is missing from Organization Consol > Departure > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");
			AssertHasMessageError("Delivery Area is required", data.DeliveryAreaInfo, "Delivery Area is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertHasMessageError("Delivery Location is required.", data.DeliveryLocationInfo, "Delivery Location is missing from Organization Consol > Departure > CTO > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");
		}

		public void TestBuild_NoGoodsDetailForIrrelevantShipment()
		{
			var consol = CreateConsol();
			var shipment1 = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault();
			shipment1.JS_UniqueConsignRef = "SH0001";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "AAAA0000008";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SH0002";
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var packingLine2 = shipment2.OuterPackLines.AddNew();
			container2.PackLines.Add(packingLine2);

			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.Where(c => c.JC_ContainerNum == "AAAA0000007")
					.ToArray()
			};

			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();

			AssertEquals("Wrapper should only have only 1 goods detail object for shipment.", 1, data.GoodsDetails.Count);
			Assert("Wrapper should have goods detail object for shipment SH0001.", data.GoodsDetails.Any(g => g.ShipmentNumber == "SH0001"));
			Assert("Wrapper should not have goods detail object for shipment SH0002.", !data.GoodsDetails.Any(g => g.ShipmentNumber == "SH0002"));
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

			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
			AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);

			consol.PackDepotAddress.Delete();
			data = builder.Build();
			AssertEquals("PCS", ZString.Empty, data.PCS);
			AssertHasMessageError("PCS Error", data.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", ZString.Empty, data.OperationalPort.Code);
			AssertHasMessageError("OperationalPort Error", ((Unloco)data.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Consol > Departure > CFS Address.");
		}

		public void TestGoodsDetails_ShoudIncludedSubShipment()
		{
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.CoLoadMaster);
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.BlindCoLoadMaster);
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.AssemblyMaster);
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.BuyersConsolLead);
		}

		void AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(string shipmentType)
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000008";

			var shipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_ShipmentType = shipmentType;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.JS_UniqueConsignRef = "s0001";
			var packingLine2 = subshipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 20;
			var subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.JS_UniqueConsignRef = "s0002";
			var packingLine3 = subshipment2.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 30;

			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .Where(c => c.JC_ContainerNum == "AAAA0000008")
				  .ToArray()
			};
			var data = new FinalManifestBuilder(consol, parameters).Build();

			AssertEquals(2, data.GoodsDetails.Count);
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == subshipment1.JS_UniqueConsignRef));
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == subshipment2.JS_UniqueConsignRef));
		}

		public void TestGoodsDetails_ShouldExcluded()
		{
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.CoLoadMaster);
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.BlindCoLoadMaster);
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.AssemblyMaster);
			AssertGoodsDetails_ShouldExcluded(Constants.ShipmentTypes.BuyersConsolLead, 1, false);
		}

		public void AssertGoodsDetails_ShouldExcluded(string shipmentType, int count = 0, bool isExcluded = true)
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000008";

			var shipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault();
			shipment.JS_UniqueConsignRef = "SH0001";

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packingLine = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(packingLine);

			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .Where(c => c.JC_ContainerNum == "AAAA0000008")
				  .ToArray()
			};
			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();

			AssertEquals(1, data.GoodsDetails.Count);
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == shipment.JS_UniqueConsignRef));

			shipment.JS_ShipmentType = shipmentType;
			data = builder.Build();
			AssertEquals(count, data.GoodsDetails.Count);
			AssertEquals(isExcluded, !data.GoodsDetails.Any(g => g.ShipmentNumber == shipment.JS_UniqueConsignRef));
		}

		public void TestPopulateAPPlusCodes()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreateConsolAddresses(consol);
			CreatePackingLinesAndContainers(consol);
			CreateFumigatedServiceOnFirstContainer(consol);

			AssertPopulateApplusCodesCI5(consol, consol.ShippingLineAddress, "CarrierCI5");
			AssertPopulateApplusCodesSON(consol, consol.ShippingLineAddress, "CarrierSON");

			AssertPopulateApplusCodesCI5(consol, consol.DeparturePackCFSTransportAddress, "TransporterCI5");
			AssertPopulateApplusCodesSON(consol, consol.DeparturePackCFSTransportAddress, "TransporterSON");

			AssertPopulateApplusCodesCI5(consol, consol.PackDepotAddress, "SendingPartyCI5");
			AssertPopulateApplusCodesSOA(consol, consol.PackDepotAddress, "SendingPartySOA");
			AssertPopulateApplusCodesSON(consol, consol.PackDepotAddress, "SendingPartySON");
			AssertPopulateApplusCodesSOW(consol, consol.PackDepotAddress, "SendingPartySOW");

			AssertPopulateApplusCodesCI5(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "CurrentUserCI5");
			AssertPopulateApplusCodesSOA(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "CurrentUserSOA");
			AssertPopulateApplusCodesSON(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "CurrentUserSON");
			AssertPopulateApplusCodesSOW(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "CurrentUserSOW");
			AssertPopulateAPPlusCodesUseRegistry("CurrentUserSON", "CurrentUserCI5");

			AssertPopulateApplusCodesCI5(consol, (OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, "ReceivingForwarderCI5");
			AssertPopulateApplusCodesSON(consol, (OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, "ReceivingForwarderSON");
		}

		void AssertPopulateApplusCodesCI5(ForwardingConsol consol, OrgAddress orgAddress, string ci5PropertyName)
			=> AssertPopulateAPPlusCodes(consol, orgAddress, ci5PropertyName, OrgCusCode.FranceCodeTypes.CI5);

		void AssertPopulateApplusCodesSOA(ForwardingConsol consol, OrgAddress orgAddress, string soaPropertyName) => AssertPopulateAPPlusCodes(consol, orgAddress, soaPropertyName, OrgCusCode.FranceCodeTypes.SOA);

		void AssertPopulateApplusCodesSON(ForwardingConsol consol, OrgAddress orgAddress, string sonPropertyName)
			=> AssertPopulateAPPlusCodes(consol, orgAddress, sonPropertyName, OrgCusCode.FranceCodeTypes.SON);

		void AssertPopulateApplusCodesSOW(ForwardingConsol consol, OrgAddress orgAddress, string sowPropertyName) => AssertPopulateAPPlusCodes(consol, orgAddress, sowPropertyName, OrgCusCode.FranceCodeTypes.SOW);

		void AssertPopulateAPPlusCodes(ForwardingConsol consol, OrgAddress address, string propertyName, string franceCodeType)
		{
			var parameters = new DummyDocDataObjectParameters();
			var builder = new FinalManifestBuilder(consol, parameters);
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

		void AssertPopulateAPPlusCodesUseRegistry(string sPropertyName, string ci5PropertyName)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();
			consol.JK_OA_PackDepotAddress = operationalAddress.PK;

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
				AssertEquals($"{ci5PropertyName} should be from registry", "FORWARDER", ((RegistrationNumber)builder.Build()[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{sPropertyName} should be from registry", "FORWARDER", ((RegistrationNumber)builder.Build()[sPropertyName]).Value);
			}
		}

		public void TestPopulateLDEReference()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreatePackingLinesAndContainers(consol);

			consol.Containers.Cast<ForwardingContainer>().ForEach(c =>
			{
				CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>("MST", FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>("EQN", c.JC_ContainerNum),
				new KeyValuePair<string, string>("RFN", "Example LDE " + c.JC_ContainerNum));
			});

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE,
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();
			CombineAssertions(() => data.Containers.ForEach(c => AssertEquals($"LDE is defaulted in container {c.Number}", "Example LDE " + c.Number, c.LDEReference)));
		}

		public void TestPopulateLDEReference_InvalidDocumentName()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreatePackingLinesAndContainers(consol);

			consol.Containers.Cast<ForwardingContainer>().ForEach(c =>
			{
				CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>("MST", "other document title"),
				new KeyValuePair<string, string>("EQN", c.JC_ContainerNum),
				new KeyValuePair<string, string>("RFN", "Example LDE"));
			});

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE,
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new FinalManifestBuilder(consol, parameters);
			var data = builder.Build();
			CombineAssertions(() => data.Containers.ForEach(c => AssertEquals($"LDE is not defaulted in container {c.Number} since the log has a different document title", "", c.LDEReference)));
		}

		public void TestPopulateLDEStatus()
		{
			FinalManifestBuilder CreateBuilderWithLDEStatus(string ldeStatus)
			{
				var consol = Factory.New<ForwardingConsol>();
				CreatePackingLinesAndContainers(consol);

				consol.Containers.Cast<ForwardingContainer>().ForEach(c =>
				{
					CreateLog(consol,
					Events.MessageAccepted,
					new ZDateTime(2020, 02, 02),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
					new KeyValuePair<string, string>("EQN", c.JC_ContainerNum),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Status, ldeStatus),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "Example LDE"));
				});

				var parameters = new DummyDocDataObjectParameters()
				{
					DocumentTitle = FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE,
					Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
				};

				return new FinalManifestBuilder(consol, parameters);
			}

			var data = CreateBuilderWithLDEStatus("VAL").Build();
			CombineAssertions("LDE is final", () => data.Containers.ForEach(c =>
			{
				Assert(c.LDEIsFinal);
				Assert(!c.LDEIsProvisional);
			}));

			data = CreateBuilderWithLDEStatus("PRO").Build();
			CombineAssertions("LDE is provisional", () => data.Containers.ForEach(c =>
			{
				Assert(!c.LDEIsFinal);
				Assert(c.LDEIsProvisional);
			}));
		}

		public void TestDefaultLDEStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreatePackingLinesAndContainers(consol);

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE,
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var data = new FinalManifestBuilder(consol, parameters).Build();
			CombineAssertions("LDE is final", () => data.Containers.ForEach(c =>
			{
				Assert(c.LDEIsFinal);
				Assert(!c.LDEIsProvisional);
			}));
		}

		public void TestCustomsStatusValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreatePackingLinesAndContainers(consol);
			var shipment = consol.Shipments.Cast<ForwardingShipment>().First();

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE,
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var time = ZDateTime.Now;
			var messageError = "Customs clearance is not received.";

			BuildAndAssertCustomsStatusValidation(null, 0, "", messageError);
			BuildAndAssertCustomsStatusValidation(Events.Held, 0, FinalManifest.CustomsStatusHeld, messageError);
			BuildAndAssertCustomsStatusValidation(Events.ClearanceCompleted, 0, FinalManifest.CustomsStatusCleared);
			BuildAndAssertCustomsStatusValidation(Events.Held, 1, FinalManifest.CustomsStatusHeld, messageError);
			BuildAndAssertCustomsStatusValidation(Events.ClearanceCompleted, 1, FinalManifest.CustomsStatusCleared);

			void BuildAndAssertCustomsStatusValidation(Event @event, int eventTimeDayOffset, string expectedCustomsStatus, string expectedMessageError = null)
			{
				if (@event != null)
				{
					time = time.AddDays(eventTimeDayOffset);
					CreateLog(shipment,
						@event,
						time,
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.CustomsReferenceNumber, "REF001"));
				}

				var data = new FinalManifestBuilder(consol, parameters).Build();
				var customStatusInfo = data.GoodsDetails.First().CustomsStatusInfo;
				AssertEquals(expectedCustomsStatus, customStatusInfo.Value);
				if (!string.IsNullOrEmpty(expectedMessageError))
				{
					AssertHasMessageError(customStatusInfo, expectedMessageError);
				}
				else
				{
					AssertNoMessageErrors(customStatusInfo);
				}
			}
		}

		public void TestMissingProviderIDValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "Sending Party", configPath: "Consol > Departure > CFS", isOrganization: true);

			consol = CreateConsol(FrenchPortsConstants.PCS.MGI);
			builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "Sending Party", configPath: "Consol > Departure > CFS", isOrganization: true);

			consol = CreateConsol(FrenchPortsConstants.PCS.Soget);
			builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedCarrierProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "Sending Party", configPath: "Consol > Departure > CFS", isOrganization: true);
		}

		void CreateLog(IStmALogProvider logProvider, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logProvider.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), "", parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		public void TestGoodsDetails_HasRCNStatus()
		{
			var consol = Factory.New<ForwardingConsol>();
			CreatePackingLinesAndContainers(consol, true, true);

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE,
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};
			var data = new FinalManifestBuilder(consol, parameters).Build();
			var goodsDetail = data.GoodsDetails.First();

			AssertEquals("ERC001", goodsDetail.ConsignmentNumber);
			AssertEquals("Cleared", goodsDetail.CustomsStatus);
			AssertEquals("REF001", goodsDetail.CommodityReference);
		}

		public void TestGoodsDetailsGroupByPANNumber()
		{
			var consol = CreateConsol(FrenchPortsConstants.PCS.MGI, true, true);
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SH0001";
			CreateShipmentPickUpCFSAddress(shipment2);
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var packingLine1 = shipment2.OuterPackLines.AddNew();
			packingLine1.JL_PackLineId = "P001";
			packingLine1.JL_PackageCount = 10;
			packingLine1.JL_ExportRefNumber = "REF002";
			CreateRCNNumber(packingLine1, "ERC002");
			CreatePANNumber(packingLine1, "REF002");

			var packingLine2 = shipment2.OuterPackLines.AddNew();
			packingLine2.JL_PackLineId = "P002";
			packingLine2.JL_PackageCount = 20;
			packingLine2.JL_ExportRefNumber = "REF002";
			CreateRCNNumber(packingLine2, "ERC002");
			CreatePANNumber(packingLine2, "REF002");

			var packingLine3 = shipment2.OuterPackLines.AddNew();
			packingLine3.JL_PackLineId = "P003";
			packingLine3.JL_PackageCount = 40;
			packingLine3.JL_ExportRefNumber = "REF003";
			CreateRCNNumber(packingLine3, "ERC003");
			CreatePANNumber(packingLine3, "REF003");

			var container = consol.Containers[0];
			container.PackLines.Add(packingLine1);
			container.PackLines.Add(packingLine2);
			container.PackLines.Add(packingLine3);

			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};
			var data = new FinalManifestBuilder(consol, parameters).Build();
			var goodsDetails = data.GoodsDetails;
			AssertEquals(3, goodsDetails.Count);
			var goodsDetail1 = goodsDetails.FirstOrDefault(g => g.CommodityReference == "REF001");
			AssertEquals(1, goodsDetail1.PackingLines.Count);

			var goodsDetail2 = goodsDetails.FirstOrDefault(g => g.CommodityReference == "REF002");
			AssertEquals(2, goodsDetail2.PackingLines.Count);
			Assert(goodsDetail2.PackingLines.Any(p => p.PackingLineID == packingLine1.JL_PackLineId));
			Assert(goodsDetail2.PackingLines.Any(p => p.PackingLineID == packingLine2.JL_PackLineId));
			AssertEquals(30, goodsDetail2.DeclaredPackCount);

			var goodsDetail3 = goodsDetails.FirstOrDefault(g => g.CommodityReference == "REF003");
			AssertEquals(1, goodsDetail3.PackingLines.Count);
			Assert(goodsDetail3.PackingLines.Any(p => p.PackingLineID == packingLine3.JL_PackLineId));
			AssertEquals(40, goodsDetail3.DeclaredPackCount);
		}

		#region Implementation

		ForwardingConsol CreateConsol(string pcs = FrenchPortsConstants.PCS.MGI, bool hasRCNNumber = false, bool hasPANNumber = false)
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
			CreatePackingLinesAndContainers(consol, hasRCNNumber, hasPANNumber);
			CreateConsolAddresses(consol, pcs);
			CreateFumigatedServiceOnFirstContainer(consol);

			return consol;
		}

		void CreateTransports(ForwardingConsol consol)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
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
			transport2.JW_RL_NKDiscPort = "FRPRA";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "FRPRA";
			transport3.JW_RL_NKDiscPort = "AUSYD";
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
			transport3.JW_JX = sailing.PK;
		}

		void CreatePackingLinesAndContainers(ForwardingConsol consol, bool hasRCNNumber = false, bool hasPANNumber = false)
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

			if (hasRCNNumber)
			{
				CreateRCNNumber(packingLine, "ERC001");
			}

			if (hasPANNumber)
			{
				CreatePANNumber(packingLine, "REF001");
			}

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

		void CreateRCNNumber(ForwardingPackLine packingLine, ZString number)
		{
			var rcnNumber = packingLine.AdditionalReferenceNumbers.AddNew();
			rcnNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			rcnNumber.CE_RN_NKCountryCode = "FR";
			rcnNumber.CE_EntryNum = number;
		}

		void CreatePANNumber(ForwardingPackLine packingLine, ZString number)
		{
			var panNumber = (CusEntryNumber)packingLine.PortReferences.AddNew();
			panNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			panNumber.CE_RN_NKCountryCode = "FR";
			panNumber.CE_EntryNum = number;
			panNumber.CE_EntryStatus = "CLR";
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

			CreateShipmentPickUpCFSAddress(shipment);
		}

		void CreateShipmentPickUpCFSAddress(ForwardingShipment shipment)
		{
			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRDKK";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
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

			var shippingLineAddress = Factory.New<OrgHeader>();
			shippingLineAddress.OH_FullName = "I'm Handling Stuff";
			shippingLineAddress.OH_RL_NKClosestPort = "AUSYD";
			shippingLineAddress.MainAddress.Address1 = "Unit 2";
			shippingLineAddress.MainAddress.Address2 = "60 What Lane";
			shippingLineAddress.MainAddress.City = "Sydney";
			shippingLineAddress.MainAddress.Postcode = "2023";
			shippingLineAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ShippingLineAddress = shippingLineAddress.MainAddress.PK;

			var departurePackCFSTransportAddress = Factory.New<OrgHeader>();
			departurePackCFSTransportAddress.OH_FullName = "I'm Receiving Stuff";
			departurePackCFSTransportAddress.OH_RL_NKClosestPort = "AUSYD";
			departurePackCFSTransportAddress.MainAddress.Address1 = "Unit 399";
			departurePackCFSTransportAddress.MainAddress.Address2 = "50 What Lane";
			departurePackCFSTransportAddress.MainAddress.City = "Sydney";
			departurePackCFSTransportAddress.MainAddress.Postcode = "5023";
			departurePackCFSTransportAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_DeparturePackCFSTransportAddress = departurePackCFSTransportAddress.MainAddress.PK;

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR"));
			unloco.RefLocoMaps.DeleteAll();
			var mapFRSoget = unloco.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = pcs;

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "I'm pack Stuff";
			packDepotAddress.MainAddress.Address1 = "Unit 400";
			packDepotAddress.MainAddress.Address2 = "66 What Lane";
			packDepotAddress.MainAddress.City = "Sydney";
			packDepotAddress.MainAddress.Postcode = "1024";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			packDepotAddress.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Handling Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 2";
			receivingForwarder.MainAddress.Address2 = "60 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
		}

		void CreateFumigatedServiceOnFirstContainer(ForwardingConsol consol)
		{
			var fumigationService = consol.Containers.Cast<ForwardingContainer>().First().Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceNote = "fumigation note";
			fumigationService.ES_OH_Contractor = FumigationContractor.PK;
		}

		OrgHeader _fumigationContractor;
		OrgHeader FumigationContractor => _fumigationContractor ?? (_fumigationContractor = GetFumigationContractor);
		OrgHeader GetFumigationContractor
		{
			get
			{
				var contractor = Factory.New<OrgHeader>();
				contractor.OH_FullName = "I'm a contractor, look at me";
				contractor.OH_RL_NKClosestPort = "AUSYD";
				contractor.MainAddress.Address1 = "Unit 420";
				contractor.MainAddress.Address2 = "60 Nowhere";
				contractor.MainAddress.City = "Sydney";
				contractor.MainAddress.Postcode = "2023";
				contractor.MainAddress.OA_RN_NKCountryCode = "AU";
				return contractor;
			}
		}
	}

	#endregion
}
