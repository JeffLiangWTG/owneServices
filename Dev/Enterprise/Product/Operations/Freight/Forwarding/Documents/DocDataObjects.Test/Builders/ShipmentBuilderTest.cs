using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.Builders.Testing
{
	sealed class ShipmentBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateOrigin()
		{
			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipmentBO.JS_RL_NKOrigin = "AUSYD";

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals("AUSYD", shipmentDO.Origin.Code);
		}

		public void TestPopulateDestination()
		{
			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipmentBO.JS_RL_NKDestination = "AUSYD";

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals("AUSYD", shipmentDO.Destination.Code);
		}

		public void TestPopulateConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TS1";
			consignee.OH_FullName = "Test1 Name";

			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipmentBO.ConsigneePK = consignee.PK;

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals("Test1 Name", shipmentDO.Consignee.CompanyName);
		}

		public void TestPopulateConsignor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TS1";
			consignor.OH_FullName = "Test1 Name";

			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipmentBO.ConsignorPK = consignor.PK;

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals("Test1 Name", shipmentDO.Consignor.CompanyName);
		}

		public void TestPopulateNotifyParty()
		{
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_Code = "TS1";
			notifyParty.OH_FullName = "Test1 Name";

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_Code = "TS2";
			notifyParty2.OH_FullName = "Test2 Name";

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_Code = "TS3";
			notifyParty3.OH_FullName = "Test3 Name";

			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipmentBO.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			shipmentBO.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
			shipmentBO.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals("Test1 Name", shipmentDO.NotifyParty.CompanyName);
			AssertEquals("Test2 Name", shipmentDO.NotifyParty2.CompanyName);
			AssertEquals("Test3 Name", shipmentDO.NotifyParty3.CompanyName);
		}

		public void TestPopulateGoodsValue()
		{
			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipmentBO.JS_GoodsValue = 1001;
			shipmentBO.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.EuropeanUnion;

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals(shipmentBO.JS_GoodsValue, shipmentDO.GoodsValue.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, shipmentDO.GoodsValue.Currency.Code);
		}

		public void TestPopulateITNNumber()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(string.Empty, shipmentDO.ITNNumber);

			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryType = "ITN";
			cusEntryNumber1.CE_EntryNum = "C001";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001", shipmentDO.ITNNumber);

			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryType = "ITN";
			cusEntryNumber2.CE_EntryNum = "C002";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001, C002", shipmentDO.ITNNumber);
		}

		public void TestPopulateCTKNumber()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AOAZZ";
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(string.Empty, shipmentDO.ITNNumber);

			var ctkNumber1 = shipment.Numbers.AddNew();
			ctkNumber1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			ctkNumber1.CE_EntryNum = "C001";
			ctkNumber1.CE_RN_NKCountryCode = "AO";

			var ctkNumber2 = shipment.Numbers.AddNew();
			ctkNumber2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			ctkNumber2.CE_EntryNum = "C002";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001", shipmentDO.CTKNumber);

			ctkNumber2.CE_RN_NKCountryCode = "AO";
			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001, C002", shipmentDO.CTKNumber);

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null, (x) => null);
			AssertEquals("C001, C002", shipmentDO.CTKNumber);
		}

		public void TestPopulateTransports()
		{
			var context = new CommonContext(Factory);

			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_UniqueConsignRef = "C00001001";
			consolBO.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolBO.JK_AgentType = Core.Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var consolTransportBO = consolBO.Transports.OfType<Freight.Business.Transport>().Single();
			consolTransportBO.JW_LegOrder = 1;
			consolTransportBO.JW_TransportMode = Core.Constants.TransportModes.Sea;
			consolTransportBO.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			consolTransportBO.JW_RL_NKLoadPort = "AUSYD";
			consolTransportBO.JW_RL_NKDiscPort = "SGSIN";
			consolTransportBO.JW_ETD = new ZDateTime(2024, 9, 11, 10, 4, 0);
			consolTransportBO.JW_ETA = new ZDateTime(2024, 9, 18, 10, 4, 0);
			consolTransportBO.JW_Vessel = "Dragon";
			consolTransportBO.JW_VoyageFlight = "111";

			var consolTransportBO2 = consolBO.Transports.AddNew();
			consolTransportBO2.JW_LegOrder = 2;
			consolTransportBO2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			consolTransportBO2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			consolTransportBO2.JW_RL_NKLoadPort = "SGSIN";
			consolTransportBO2.JW_RL_NKDiscPort = "BEANR";
			consolTransportBO2.JW_ETD = new ZDateTime(2024, 9, 21, 10, 7, 0);
			consolTransportBO2.JW_ETA = new ZDateTime(2024, 9, 28, 10, 7, 0);
			consolTransportBO2.JW_Vessel = "Steven";
			consolTransportBO2.JW_VoyageFlight = "222";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipmentTransportBO = shipmentBO.Transports.AddNew();
			shipmentTransportBO.JW_LegOrder = 0;
			shipmentTransportBO.JW_TransportMode = Core.Constants.TransportModes.Road;
			shipmentTransportBO.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			shipmentTransportBO.JW_RL_NKLoadPort = "AUMEL";
			shipmentTransportBO.JW_RL_NKDiscPort = "AUSYD";
			shipmentTransportBO.JW_ETD = new ZDateTime(2024, 9, 7, 15, 3, 0);
			shipmentTransportBO.JW_ETA = new ZDateTime(2024, 9, 8, 15, 3, 0);
			shipmentTransportBO.JW_Vessel = "TRUCK 123";
			shipmentTransportBO.JW_VoyageFlight = "123";
			shipmentTransportBO.IsDomestic = true;

			var shipmentTransportBO2 = shipmentBO.Transports.AddNew();
			shipmentTransportBO2.JW_LegOrder = 0;
			shipmentTransportBO2.JW_TransportMode = Core.Constants.TransportModes.Road;
			shipmentTransportBO2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			shipmentTransportBO2.JW_RL_NKLoadPort = "BEANR";
			shipmentTransportBO2.JW_RL_NKDiscPort = "NLAMS";
			shipmentTransportBO2.JW_ETD = new ZDateTime(2024, 10, 1, 15, 4, 0);
			shipmentTransportBO2.JW_ETA = new ZDateTime(2024, 10, 2, 15, 4, 0);
			shipmentTransportBO2.JW_Vessel = "TRUCK 456";
			shipmentTransportBO2.JW_VoyageFlight = "456";
			shipmentTransportBO2.IsDomestic = false;

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals(4, shipmentDO.Transports.Count);

			AssertTransport(shipmentDO.Transports.ToList()[0], 1, "ROA", "PRE", "AUMEL", "AUSYD", "TRUCK 123", "123");
			AssertTransport(shipmentDO.Transports.ToList()[1], 2, "SEA", "MAI", "AUSYD", "SGSIN", "Dragon", "111");
			AssertTransport(shipmentDO.Transports.ToList()[2], 3, "SEA", "OTH", "SGSIN", "BEANR", "Steven", "222");
			AssertTransport(shipmentDO.Transports.ToList()[3], 4, "ROA", "ONF", "BEANR", "NLAMS", "TRUCK 456", "456");

			void AssertTransport(ITransport transport, ZInt legOrder, ZString transportMode, ZString planningType, ZString portOfLoading, ZString portOfDischarge, ZString vessel, ZString voyageFlightNumber)
			{
				AssertEquals(legOrder, transport.LegOrder);
				AssertEquals(transportMode, transport.Mode.Code);
				AssertEquals(planningType, transport.Type.Code);
				AssertEquals(portOfLoading, transport.PortOfLoading.Code);
				AssertEquals(portOfDischarge, transport.PortOfDischarge.Code);
				AssertEquals(vessel, transport.Vessel.Name);
				AssertEquals(voyageFlightNumber, transport.VoyageFlightNumber);
			}
		}

		public void TestPopulateShipmentType()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals("STD", shipmentDO.ShipmentType.Code);
		}

		public void TestPopulateDUENumber()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "SHP000001";

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
			shipment.CustomsEntryNumber = "xxx_111";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("Not DUE, so empty string", ZString.Empty, shipmentDO.DUENumber);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Brazil.DUE;

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("DUE, so should be populated", "xxx_111", shipmentDO.DUENumber);

			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			cusEntryNumber1.CE_EntryNum = "xxx_222";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("DUE, so should be populated", "xxx_111, xxx_222", shipmentDO.DUENumber);
		}

		public void TestPopulateUCRNumber()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(string.Empty, shipmentDO.UCRNumber);

			var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber1.CE_EntryType = "UCR";
			cusEntryNumber1.CE_EntryNum = "C001";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001", shipmentDO.UCRNumber);

			var cusEntryNumber2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber2.CE_EntryType = "UCR";
			cusEntryNumber2.CE_EntryNum = "C002";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C001, C002", shipmentDO.UCRNumber);
		}

		public void TestPopulateCTNNumber()
		{
			var context = new CommonContext(Factory);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";
			shipment.Consols.Add(consol);

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals(string.Empty, shipmentDO.CTNNumber);

			var cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = "CTN";
			cusEntryNumber.CE_EntryNum = "C002";

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);
			AssertEquals("C002", shipmentDO.CTNNumber);
		}

		public void TestPopulateExportStatement()
		{
			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "SHP000001";

			var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			var countrySetting = countrySettingCollection[Core.Constants.CountryCodes.Guam];

			using (FreightDataRegistry.Instance.ExportStatementSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection))
			{
				AssertExportStatement("USCHI", "AUSYD", true);

				shipment.DocsAndCartage.JP_ExportStatement = "LOW";

				AssertExportStatement("USCHI", "AUSYD", false);
				AssertExportStatement("USCHI", "US2AA", true);
				AssertExportStatement("USCHI", "GUAGA", false);

				AssertExportStatement("GUAGA", "AUSYD", false);
				AssertExportStatement("GUAGA", "GUDED", true);
				AssertExportStatement("GUAGA", "USCHI", false);

				AssertExportStatement("AUSYD", "USCHI", true);
				AssertExportStatement("AUSYD", "GUAGA", true);
				AssertExportStatement("AUSYD", "AUEML", true);
			}

			void AssertExportStatement(ZString originPort, ZString destinationPort, ZBool isEmpty)
			{
				shipment.JS_RL_NKOrigin = originPort;
				shipment.JS_RL_NKDestination = destinationPort;

				var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

				if (isEmpty)
				{
					AssertNullOrEmpty(shipmentDO.ExportStatement);
				}
				else
				{
					AssertEquals("NOEEI §30.37(a)", shipmentDO.ExportStatement);
				}
			}
		}

		public void TestPopulateExportStatementForUSOverseasTerritories()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			var countrySetting = countrySettingCollection[Core.Constants.CountryCodes.PuertoRico];
			var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
			statementSetting.Visibility = "UDF";
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

			var context = new CommonContext(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "SHP000001";

			var shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(ZString.Empty, shipmentDO.ExportStatement);

			shipment.JS_RL_NKOrigin = "PRSJU";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

			shipmentDO = new ShipmentBuilder(context).Build(shipment, null);

			AssertEquals(statementSetting.Statement, shipmentDO.ExportStatement);
		}

		public void TestPopulatePackCount()
		{
			var context = new CommonContext(Factory);
			var shipmentBO = Factory.New<ForwardingShipment>();

			var shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals(0, shipmentDO.PackCount);
			AssertEquals("PLT", shipmentDO.PackType.Code);

			shipmentBO.JS_OuterPacks = 10;
			shipmentBO.JS_F3_NKPackType = Core.Constants.PkgUnit.Bottle;

			shipmentDO = new ShipmentBuilder(context).Build(shipmentBO, (ForwardingShipment shipment) => Enumerable.Empty<PackingLine>());

			AssertEquals(10, shipmentDO.PackCount);
			AssertEquals("BOT", shipmentDO.PackType.Code);
		}
	}
}
