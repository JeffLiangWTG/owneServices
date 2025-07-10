using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class ContainerLoadPlanBuilderTest : TestCaseWithFactory
	{
		#region TestPopulateRouting

		public void TestPopulateRouting()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();

			AssertEquals("PortOfLoading", "CNNGB|Ningbo Lishe International Apt", loadPlan.PortOfLoading.ToAssertString());
			AssertEquals("PortOfDischarge", "SGSIN|Singapore", loadPlan.PortOfDischarge.ToAssertString());
			AssertEquals("OperationalPort", "CNNGB|Ningbo Lishe International Apt", loadPlan.OperationalPort.ToAssertString());
			AssertEquals("PortOfTranship", "SGSIN|Singapore", loadPlan.PortOfTranship.ToAssertString());
			AssertEquals("PlaceOfDelivery", "AUSYD|Sydney", loadPlan.PlaceOfDelivery.ToAssertString());
			AssertEquals("Vessel.Name", "Sea Dragon", loadPlan.Vessel.Name);
			AssertEquals("Vessel.LloydsIMO", "", loadPlan.Vessel.LloydsIMO);
		}

		#endregion

		#region TestPopulateHeaderSection

		public void TestPopulateHeaderSection()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();

			AssertEquals("ShipmentType", "AGT|Agent", loadPlan.ShipmentType.ToAssertString());
			AssertEquals("ContainerMode", $"{Core.Constants.ContainerModes.FCL}|{Core.Constants.ContainerModeDescriptions.FCL}", loadPlan.ContainerMode.ToAssertString());
			AssertEquals("W", loadPlan.TradeFlag);
			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, loadPlan.SendingAgent);
			AssertionHelper.AssertCurrentUserAddressData(loadPlan.CurrentUser);
		}

		#endregion

		public void TestPopulateBottomSection()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();

			AssertionHelper.AssertAddressData(consol.ShippingLineAddress, loadPlan.Carrier);
			AssertionHelper.AssertAddressData(consol.PackDepotAddress, loadPlan.DepartureCFSAddress);
		}

		public void TestValidateTransitBerthCode()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var containerLoadPlan = builder.Build();

			AssertHasMessageError(containerLoadPlan.TransitBerthCodeInfo, "Transit Berth Code is mandatory and should only contain 5 alphanumeric characters.");

			containerLoadPlan.TransitBerthCode = "张AC11";
			AssertHasMessageError(containerLoadPlan.TransitBerthCodeInfo, "Transit Berth Code is mandatory and should only contain 5 alphanumeric characters.");

			containerLoadPlan.TransitBerthCode = "AA123";
			AssertNoMessageErrors(containerLoadPlan.TransitBerthCodeInfo);

			containerLoadPlan.TransitBerthCode = "CCA111";
			AssertHasMessageError(containerLoadPlan.TransitBerthCodeInfo, "Transit Berth Code is mandatory and should only contain 5 alphanumeric characters.");
		}

		public void TestValidateCarrierCCCCode()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();

			AssertHasMessageError(loadPlan.Carrier.CompanyNameInfo, "Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC.");

			var carrier = Factory.Load<OrgHeader>(consol.ShippingLineAddress.OA_OH);

			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "1234";
			cusCode.OK_RN_NKCodeCountry = "US";
			carrier.CustomsCodes.Add(cusCode);

			loadPlan = builder.Build();
			AssertNoMessageError(loadPlan.Carrier.CompanyNameInfo, "Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC.");
		}

		#region TestPopulateContainers

		public void TestPopulateContainers()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();

			var containersInfo = loadPlan
				.Containers
				.OrderBy(c => c.Number)
				.Select(c => $"{c.Number}|{c.Seal}|{c.SecondSeal}|{c.ThirdSeal}|{c.Mode.ToAssertString()}|{c.PackDate}|{c.GrossWeight.Value} {c.GrossWeight.Unit.Code} |{c.TareWeight.Value} {c.TareWeight.Unit.Code}");

			AssertMultilineASCIIEquals("Containers",
@"AAAA0000007|S0001|S00012|S00013|FCL|Full Container Load|28-Jun-18 00:00:00|526 KG |222 KG
BBBB0000004|S0002|||FCL|Full Container Load||1300 KG |1250 KG",
				string.Join("\r\n", containersInfo));
		}

		#endregion

		#region TestValidateSetTemperature

		public void TestValidateSetTemperature()
		{
			var consol = CreateConsol();

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOType = "22P1";
			refContainer.RC_ContainerType = "RFG";
			refContainer.RC_TareWeight = 222;

			var container = consol.Containers.Cast<ForwardingContainer>().First();
			container.JC_RC = refContainer.PK;
			container.JC_SetPointTemp = 0;
			container.JC_SetPointTempUnit = "";
			container.JC_IsNonOperativeReefer = false;

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol.Containers.OfType<ForwardingContainer>().ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();
			var containerData = loadPlan.Containers.First();
			AssertEquals(containerData.IsNonOperativeReefer, false);
			AssertHasMessageError(containerData.SetTemperature.ValueInfo, "Temperature is required when container is a reefer.");

			container.JC_IsNonOperativeReefer = true;
			Factory.Save();

			loadPlan = builder.Build();
			containerData = loadPlan.Containers.First();
			AssertEquals(containerData.IsNonOperativeReefer, true);
			AssertNoMessageError(containerData.SetTemperature.ValueInfo, "Temperature is required when container is a reefer.");

			container.JC_IsNonOperativeReefer = false;
			container.JC_SetPointTemp = 10;
			Factory.Save();

			loadPlan = builder.Build();
			containerData = loadPlan.Containers.First();
			AssertEquals(containerData.IsNonOperativeReefer, false);
			AssertNoMessageError(containerData.SetTemperature.ValueInfo, "Temperature is required when container is a reefer.");
		}

		#endregion

		#region TestPopulatePacksGroup

		public void TestPopulatePacksGroup()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();
			var container = loadPlan.Containers.First();
			AssertEquals(3, container.Groups.Count);

			var info = string.Join(System.Environment.NewLine, container.Groups.Select(g => $"{g.SONumber} | {g.MarksAndNumbers} | {g.GoodsDescription} | {g.Quantity} | {g.PackageType.Code} | {g.Weight.Value} {g.Weight.Unit.Code} | {g.Volume.Value} {g.Volume.Unit.Code}"));
			AssertMultilineASCIIEquals(@"SLDNO001 | Marks 1, Marks 2 | Goods 1 | 5 | PTL | 35 KG | 17 M3
SLDNO002 | Marks 2 | Goods 2 | 18 | PKG | 83 KG | 74 M3
SLDNO003 |  | shipment goods | 7 | PKG | 86 KG | 36 M3", info);
		}

		#endregion

		#region TestPopulateShipperCompanyName

		public void TestPopulateShipperCompanyName()
		{
			var consol = Factory.New<ForwardingConsol>();
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();
			AssertEquals("I'm Sending Stuff", loadPlan.SendingAgent.CompanyName);

			var optionDesc = consol.SendingForwarder.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier);

			consol.SendingForwarder.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			loadPlan = builder.Build();
			AssertEquals($"I'm Sending Stuff {optionDesc}", loadPlan.SendingAgent.CompanyName);

			consol.SendingForwarder.MiscServ.OM_FWAsAgentName = "ABC Lines";
			loadPlan = builder.Build();
			AssertEquals($"I'm Sending Stuff {optionDesc} ABC Lines", loadPlan.SendingAgent.CompanyName);
		}

		#endregion

		#region TestPopulateDangerousGoods

		public void TestPopulateeDangerousGoods()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var loadPlan = builder.Build();
				var container = loadPlan.Containers.First();
				var group = container.Groups.First();

				var info = string.Join(System.Environment.NewLine, group.DangerousGoods.Select(g => $"{g.Code} | {g.Unno} | {g.Variant} | {g.Quantity} | {g.ProperShippingName} | {g.TechnicalName} | {g.IMOClass} {g.PackingGroup} | {g.SubLabel1} {g.SubLabel2}"));
				AssertMultilineASCIIEquals(@"0004a | 0004 | a | 10 | AMMONIUM PICRATE |  | 1.1D  |  
0030 | 0030 |  | 10 | DETONATORS, ELECTRIC |  | 1.1B  |  ", info);

				AssertMultilineASCIIEquals(@"UN0004, AMMONIUM PICRATE, class 1.1D, (1C c.c.), contact Handsome 1234567
UN0030, DETONATORS, ELECTRIC, class 1.1B, (3C c.c.), contact Handsome2 7654321", group.DangerousGoodsDescription);
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var loadPlan = builder.Build();
				var container = loadPlan.Containers.First();
				var group = container.Groups.First();

				AssertMultilineASCIIEquals(@"UN0004, AMMONIUM PICRATE, class 1.1D, (1C c.c.), contact Handsome 1234567
UN0030, DETONATORS, ELECTRIC, class 1.1B, contact Handsome2 7654321", group.DangerousGoodsDescription);
			}
		}

		public void TestDangerousGoodsValidation()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol.Containers.OfType<ForwardingContainer>().ToArray()
			};

			var builder = new ContainerLoadPlanBuilder(consol, parameters);
			var loadPlan = builder.Build();
			var container = loadPlan.Containers.First();
			var group = container.Groups.First();
			var dg = group.DangerousGoods.First();

			AssertNoMessageError(group.DangerousGoodsDescriptionInfo, "Contact Name is required for dangerous goods.");
			AssertNoMessageError(group.DangerousGoodsDescriptionInfo, "Contact Phone is required for dangerous goods.");

			dg.Contact.FullName = ZString.Empty;
			dg.Contact.Phone = ZString.Empty;

			AssertHasMessageError(group.DangerousGoodsDescriptionInfo, "Contact Name is required for dangerous goods.");
			AssertHasMessageError(group.DangerousGoodsDescriptionInfo, "Contact Phone is required for dangerous goods.");
		}

		#endregion

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var data = new ContainerLoadPlanBuilder(consol, parameters).Build();
			AssertEquals(Core.Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Core.Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "CNNGB";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "驴100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_GoodsDescription = "shipment goods";

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "I am the carrier";
			carrier.MainAddress.Address1 = "Unit 343";
			carrier.MainAddress.Address2 = "Carrier Lane";
			carrier.MainAddress.City = "Carrier city";
			carrier.MainAddress.Postcode = "3453";
			carrier.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var packDepot = Factory.NewWithValidTestData<OrgHeader>();
			packDepot.OH_FullName = "I am pack depot";
			packDepot.MainAddress.Address1 = "Unit 444";
			packDepot.MainAddress.Address2 = "depot lane";
			packDepot.MainAddress.City = "depot city";
			packDepot.MainAddress.Postcode = "4456";
			packDepot.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepot.MainAddress.PK;

			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNNGB";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Sea Dragon";
			transport.JW_VoyageFlight = "111";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Forrest Archer";
			transport2.JW_VoyageFlight = "222";

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOType = "22P1";
			refContainer.RC_TareWeight = 222;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_DeliveryMode = "CFS/CY";
			container1.JC_IsShipperOwned = true;
			container1.JC_GrossWeightUQ = "KG";
			container1.JC_TareWeight = 1000;
			container1.JC_DunnageWeight = 100;
			container1.JC_SealNum = "S0001";
			container1.JC_AdditionalSealNum = "S00012";
			container1.JC_Additional2SealNum = "S00013";
			container1.JC_PackDate = new ZDate(2018, 6, 28);
			container1.JC_RC = refContainer.PK;
			container1.JC_SetPointTempUnit = "C";
			container1.JC_IsNonOperativeReefer = false;

			var packline1 = Factory.New<ForwardingPackLine>();
			packline1.JL_JS = shipment.PK;
			packline1.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline1);
			packline1.JL_PackageCount = 3;
			packline1.JL_ActualWeight = 15;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline1.JL_ActualVolume = 2000;
			packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packline1.JL_ExportRefNumber = "SLDNO001";
			packline1.JL_Description = "Goods 1";
			packline1.JL_MarksAndNumbers = "Marks 1";
			packline1.JL_F3_NKPackType = "PTL";

			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			undg1.DI_IMOClass = "1.1D";
			undg1.DI_IsCombustible = true;
			undg1.DI_DGFlashPoint = 1;
			undg1.DI_PackageCount = 10;
			undg1.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg1.DGContact.OC_ContactName = "Handsome";
			undg1.DGContact.OC_Phone = "1234567";
			undg1.DI_IsCombustible = true;

			var undg2 = packline1.UNDGs.AddNew();
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0030", "", "IMO").First().PK;
			undg2.DI_IMOClass = "1.1C";
			undg2.DI_IsCombustible = true;
			undg2.DI_DGFlashPoint = 3;
			undg2.DI_PackageCount = 10;
			undg2.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg2.DGContact.OC_ContactName = "Handsome2";
			undg2.DGContact.OC_Phone = "7654321";
			undg2.DI_IsCombustible = false;

			var packline2 = Factory.New<ForwardingPackLine>();
			packline2.JL_JS = shipment.PK;
			packline2.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline2);
			packline2.JL_PackageCount = 2;
			packline2.JL_ActualWeight = 20000;
			packline2.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			packline2.JL_ActualVolume = 15;
			packline2.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline2.JL_ExportRefNumber = "SLDNO001";
			packline2.JL_Description = "Goods 1";
			packline2.JL_MarksAndNumbers = "Marks 2";
			packline2.JL_F3_NKPackType = "PTL";

			var packline3 = Factory.New<ForwardingPackLine>();
			packline3.JL_JS = shipment.PK;
			packline3.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline3);
			packline3.JL_PackageCount = 4;
			packline3.JL_ActualWeight = 30;
			packline3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline3.JL_ActualVolume = 20;
			packline3.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline3.JL_ExportRefNumber = "SLDNO002";
			packline3.JL_Description = "Goods 2";
			packline3.JL_MarksAndNumbers = "Marks 2";
			packline3.JL_F3_NKPackType = "PTL";

			var packline4 = Factory.New<ForwardingPackLine>();
			packline4.JL_JS = shipment.PK;
			packline4.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline4);
			packline4.JL_PackageCount = 6;
			packline4.JL_ActualWeight = 21;
			packline4.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline4.JL_ActualVolume = 30;
			packline4.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline4.JL_ExportRefNumber = "SLDNO002";
			packline4.JL_Description = "Goods 2";
			packline4.JL_MarksAndNumbers = "Marks 2";
			packline4.JL_F3_NKPackType = "PCE";

			var packline5 = Factory.New<ForwardingPackLine>();
			packline5.JL_JS = shipment.PK;
			packline5.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline5);
			packline5.JL_PackageCount = 8;
			packline5.JL_ActualWeight = 32;
			packline5.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline5.JL_ActualVolume = 24;
			packline5.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline5.JL_ExportRefNumber = "SLDNO002";
			packline5.JL_Description = "Goods 3";
			packline5.JL_MarksAndNumbers = "Marks 2";
			packline5.JL_F3_NKPackType = "PTE";

			var packline6 = Factory.New<ForwardingPackLine>();
			packline6.JL_JS = shipment.PK;
			packline6.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline6);
			packline6.JL_ActualWeight = 45;
			packline6.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline6.JL_ActualVolume = 25;
			packline6.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline6.JL_PackageCount = 3;
			packline6.JL_ExportRefNumber = "SLDNO003";
			packline6.JL_F3_NKPackType = "PKG";

			var packline7 = Factory.New<ForwardingPackLine>();
			packline7.JL_JS = shipment.PK;
			packline7.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packline7);
			packline7.JL_ActualWeight = 41;
			packline7.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline7.JL_ActualVolume = 11;
			packline7.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packline7.JL_PackageCount = 4;
			packline7.JL_ExportRefNumber = "SLDNO003";
			packline7.JL_F3_NKPackType = "PKG";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = "CFS/CY";
			container2.JC_IsShipperOwned = true;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 1250;
			container2.JC_DunnageWeight = 50;
			container2.JC_SealNum = "S0002";
			container2.JC_SetPointTempUnit = "C";
			container2.JC_IsNonOperativeReefer = false;

			return consol;
		}

		#endregion
	}
}
