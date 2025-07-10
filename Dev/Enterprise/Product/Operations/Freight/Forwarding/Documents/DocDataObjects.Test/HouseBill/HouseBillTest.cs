using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(HouseBill))]
	sealed class HouseBillTest : NonPersistentBusinessObjectTestCase
	{
		#region TestGetCustomField

		public void TestGetCustomField()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;

			var existingTemplates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "SHP"));
			foreach (var existingTemplate in existingTemplates)
			{
				existingTemplate.P0_IsActive = false;
			}

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_SubType1 = TransportModes.Sea;
			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "Custom Number";
			customField1.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField2 = template.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "MY.Custom.Number";
			customField2.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var customField3 = template.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "Wow_My-So`Very-Custom+Number";
			customField3.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			var customBusinessObject = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
			customBusinessObject[((IDynamicBusinessObject)customBusinessObject).PropertyNames[0]] = 123;
			customBusinessObject[((IDynamicBusinessObject)customBusinessObject).PropertyNames[1]] = true;
			customBusinessObject[((IDynamicBusinessObject)customBusinessObject).PropertyNames[2]] = "abc567";
			Factory.Save();

			var houseBill = new HouseBillBuilder(shipment, null).Build();

			var expr = "GetCustomField(\"Custom Number\")".With<DataLibrary>().CreateExpression();
			var result = expr.Evaluate(houseBill);
			Assert("Evaluated result should be ZInt", result is ZInt);
			AssertEquals("Evaluated ZInt should be correct", 123, result);

			expr = "GetCustomField(\"MY.Custom.Number\")".With<DataLibrary>().CreateExpression();
			result = expr.Evaluate(houseBill);
			Assert("Evaluated result should be ZBool", result is ZBool);
			AssertEquals("Evaluated ZInt should be correct", true, result);

			expr = "GetCustomField(\"Wow_My-So`Very-Custom+Number\")".With<DataLibrary>().CreateExpression();
			result = expr.Evaluate(houseBill);
			Assert("Evaluated result should be ZString", result is ZString);
			AssertEquals("Evaluated ZInt should be correct", "abc567", result);

			expr = "GetCustomField(\"Wow_My-So`Very-Custom+Number\")".With<DataLibrary>().CreateExpression();
			result = expr.Evaluate(houseBill);
			Assert("Evaluated result should be ZString (2nd evaluation)", result is ZString);
			AssertEquals("Evaluated ZInt should be correct (2nd evaluation)", "abc567", result);

			expr = "GetCustomField(\"Blah Blah\")".With<DataLibrary>().CreateExpression();
			AssertNull("Evaluated result should be null when custom field does not exist", expr.Evaluate(houseBill));
		}

		public void TestGetCustomFieldWithNullCustomBusinessObject()
		{
			var houseBill = new HouseBill("ForwardingShipment", "S00012");
			var expr = "GetCustomField(\"Custom Number\")".With<DataLibrary>().CreateExpression();
			AssertNoExceptionThrown(() => expr.Evaluate(houseBill));
		}

		#endregion

		#region TestRegistryItemShowPackLineDetails

		public void TestRegistryItemShowPackLineDetails()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine1.JL_PackageCount = 10;
			container.PackLines.Add(packLine1);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine2.JL_PackageCount = 20;
			container.PackLines.Add(packLine2);

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			shipment.JS_OuterPacks = 15;

			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();
				AssertEquals("When ShowPackLineDetailsOnHouseBills is true, the TotalPackCount of houseBill should equal the sum of JL_PackageCount of OuterPackLines in shipment", 30, houseBill.TotalPackCount);
			}

			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();
				AssertEquals("When ShowPackLineDetailsOnHouseBills is false, the TotalPackCount of houseBill should equal the vaule of JS_OuterPacks  in shipment", 15, houseBill.TotalPackCount);
			}

			void AssertMarksAndNumbers(bool showPackLineDetailsOnHouseBills, ZString shipmentMarksAndNumbers, ZString packLine1MarksAndNumbers, ZString packLine2MarksAndNumbers, ZString houseBillMarksAndNumbers)
			{
				using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, showPackLineDetailsOnHouseBills))
				{
					shipment.JS_MarksAndNumbers = shipmentMarksAndNumbers;
					packLine1.JL_MarksAndNumbers = packLine1MarksAndNumbers;
					packLine2.JL_MarksAndNumbers = packLine2MarksAndNumbers;
					var houseBill = new HouseBillBuilder(shipment, null).Build();
					AssertEquals(houseBillMarksAndNumbers, houseBill.MarksAndNumbers);
				}
			}

			AssertMarksAndNumbers(true, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertMarksAndNumbers(false, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertMarksAndNumbers(true, "MARKS AND NUMBERS SHIPMENT", ZString.Empty, ZString.Empty, "MARKS AND NUMBERS SHIPMENT");
			AssertMarksAndNumbers(false, "MARKS AND NUMBERS SHIPMENT", ZString.Empty, ZString.Empty, "MARKS AND NUMBERS SHIPMENT");
			AssertMarksAndNumbers(true, "MARKS AND NUMBERS SHIPMENT", "marks and numbers 1", "marks and numbers 2", "MARKS AND NUMBERS SHIPMENT");
			AssertMarksAndNumbers(false, "MARKS AND NUMBERS SHIPMENT", "marks and numbers 1", "marks and numbers 2", "MARKS AND NUMBERS SHIPMENT");
			AssertMarksAndNumbers(true, ZString.Empty, "marks and numbers 1", "marks and numbers 2", "marks and numbers 1\r\nmarks and numbers 2");
			AssertMarksAndNumbers(false, ZString.Empty, "marks and numbers 1", "marks and numbers 2", ZString.Empty);
		}

		#endregion

		#region TestINCOTermForFCA

		public void TestINCOTermForFCA()
		{
			var shipment = Factory.New<ForwardingShipment>();
			foreach (var inco in IncoTerms.Incoterms2020)
			{
				shipment.JS_INCO = inco;

				var houseBill = new HouseBillBuilder(shipment, null).Build();

				if (inco == IncoTerms.FreeCarrierBuyer || inco == IncoTerms.FreeCarrierSeller)
				{
					AssertEquals("FC1 and FC2 should be mapped to FCA when displayed on doucment(Code)", IncoTerms.FreeCarrier, houseBill.INCO.Code);
					AssertEquals("FC1 and FC2 should be mapped to FCA when displayed on doucment(Description)", IncoTerms.Descriptions.FreeCarrier, houseBill.INCO.Description);
				}
				else
				{
					AssertEquals("Official Incoterm other than FC1 and FC2 should be mapped as it is(Code)", inco, houseBill.INCO.Code);
					AssertEquals("Official Incoterm other than FC1 and FC2 should be mapped as it is(Description)", IncoTerms.Descriptions.DefaultCodeDescriptionPairs[inco], houseBill.INCO.Description);
				}
			}
		}

		#endregion

		#region TestLoadAndDischargePortsFromRelatedTransports

		public void TestLoadandDischargePortsFromRelatedTransports()
		{
			#region Setup
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHXXXX0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ShippedOnBoard = "SHP";

			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			SetConsolsWithTransport(shipment);

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "COPY"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			#endregion

			AssertEquals("PortOfLoading.Code", "AUSYD", houseBill.PortOfLoading.Code);
			AssertEquals("PortOfDischarge.Code", "CNSHA", houseBill.PortOfDischarge.Code);
		}

		void SetConsolsWithTransport(ForwardingShipment shipment)
		{
			var consol4 = shipment.Consols.AddNew();
			consol4.JK_UniqueConsignRef = "CONSOL0004";
			consol4.JK_TransportMode = "SEA";
			consol4.JK_RL_NKLoadPort = "HKHKG";
			consol4.JK_RL_NKDischargePort = "CNSHA";

			var consolTransport4 = consol4.Transports[0];
			consolTransport4.JW_Vessel = "Vessel4";
			consolTransport4.JW_VoyageFlight = "1003";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "CONSOL0002";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "CNCAN";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			var consolTransport2 = consol2.Transports[0];
			consolTransport2.JW_Vessel = "Vessel2";
			consolTransport2.JW_VoyageFlight = "F1001";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "CONSOL0003";
			consol3.JK_TransportMode = "SEA";
			consol3.JK_RL_NKLoadPort = "NZAKL";
			consol3.JK_RL_NKDischargePort = "HKHKG";

			var consolTransport3 = consol3.Transports[0];
			consolTransport3.JW_Vessel = "Vessel3";
			consolTransport3.JW_VoyageFlight = "F1002";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CONSOL0001";
			consol1.JK_TransportMode = "SEA";
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNCAN";

			var consolTransport1 = consol1.Transports[0];
			consolTransport1.JW_Vessel = "Vessel1";
			consolTransport1.JW_VoyageFlight = "F1000";
		}

		public void TestLoadandDischargePortsOfManufactureHBL()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHXXXX0001";
			shipment.JS_ShipmentType = ShipmentTypes.ThirdPartyOwnershipHouse;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "MANUFACTURER";
			manufacturer.Addresses[0].OA_Address1 = "ManufacturerAddress1";
			manufacturer.Addresses[0].OA_Address2 = "ManufacturerAddress2";
			manufacturer.Addresses[0].OA_RL_NKRelatedPortCode = "AUMEL";
			shipment.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;

			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ShippedOnBoard = "SHP";

			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			SetConsolsWithTransport(shipment);

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "COPY",
				DataStoreName = "ManufacturerBillOfLadingXX"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("PortOfLoading.Code", "AUMEL", houseBill.PortOfLoading.Code);
			AssertEquals("PortOfDischarge.Code", "CNSHA", houseBill.PortOfDischarge.Code);
		}

		#endregion

		#region TestPopulateFromDirectShipmentAttachedToPreCarriageAGTConsol

		public void TestPopulateFromDirectShipmentAttachedToPreCarriageAGTConsol()
		{
			var today = ZDateTime.Today;

			var directShipment = Factory.New<ForwardingShipment>();
			directShipment.JS_UniqueConsignRef = "SHXXXX0001";
			directShipment.JS_HouseBill = "HOUSEBILL001";
			directShipment.JS_PackingMode = ContainerModes.FCL;
			directShipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			directShipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			directShipment.JS_RL_NKOrigin = "USLAX";
			directShipment.JS_RL_NKDestination = "AUSYD";
			directShipment.JS_ShippedOnBoard = "SHP";

			var directDepartureConsol = directShipment.Consols.AddNew();
			directDepartureConsol.JK_UniqueConsignRef = "CONSOL0001";
			directDepartureConsol.JK_TransportMode = "SEA";
			directDepartureConsol.JK_RL_NKLoadPort = "USLAX";
			directDepartureConsol.JK_RL_NKDischargePort = "SGSIN";
			directDepartureConsol.JK_AgentType = AgentType.Direct;

			var directDepartureConsolTransport = directDepartureConsol.Transports[0];
			directDepartureConsolTransport.JW_Vessel = "Vessel 1";
			directDepartureConsolTransport.JW_ETD = today.AddDays(2);
			directDepartureConsolTransport.JW_ETA = today.AddDays(3);

			var directArrivalConsol = directShipment.Consols.AddNew();
			directArrivalConsol.JK_UniqueConsignRef = "CONSOL0002";
			directArrivalConsol.JK_TransportMode = "SEA";
			directArrivalConsol.JK_RL_NKLoadPort = "SGSIN";
			directArrivalConsol.JK_RL_NKDischargePort = "AUSYD";
			directArrivalConsol.JK_AgentType = AgentType.Direct;

			var directArrivalConsolTransport = directArrivalConsol.Transports[0];
			directArrivalConsolTransport.JW_Vessel = "Vessel 2";
			directArrivalConsolTransport.JW_ETD = today.AddDays(4);
			directArrivalConsolTransport.JW_ETA = today.AddDays(5);

			var agentPreCarriageConsol = directShipment.Consols.AddNew();
			agentPreCarriageConsol.JK_UniqueConsignRef = "CONSOL0003";
			agentPreCarriageConsol.JK_TransportMode = "SEA";
			agentPreCarriageConsol.JK_RL_NKLoadPort = "USCHI";
			agentPreCarriageConsol.JK_RL_NKDischargePort = "USLAX";
			agentPreCarriageConsol.JK_AgentType = AgentType.Agent;

			var agentPreCarriageConsolTransport = agentPreCarriageConsol.Transports[0];
			agentPreCarriageConsolTransport.JW_Vessel = "Vessel 3";
			agentPreCarriageConsolTransport.JW_ETD = today;
			agentPreCarriageConsolTransport.JW_ETA = today.AddDays(1);

			var agentOnForwardingConsol = directShipment.Consols.AddNew();
			agentOnForwardingConsol.JK_UniqueConsignRef = "CONSOL0004";
			agentOnForwardingConsol.JK_TransportMode = "SEA";
			agentOnForwardingConsol.JK_RL_NKLoadPort = "AUSYD";
			agentOnForwardingConsol.JK_RL_NKDischargePort = "AUMEL";
			agentOnForwardingConsol.JK_AgentType = AgentType.Agent;

			var agentOnForwardingConsolTransport = agentOnForwardingConsol.Transports[0];
			agentOnForwardingConsolTransport.JW_Vessel = "Vessel 4";
			agentOnForwardingConsolTransport.JW_ETD = today.AddDays(6);
			agentOnForwardingConsolTransport.JW_ETA = today.AddDays(7);

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "COPY"
			};

			var houseBill = new HouseBillBuilder(directShipment, parameters).Build();

			AssertEquals("PortOfLoading.Code", "USCHI", houseBill.PortOfLoading.Code);
			AssertEquals("PortOfDischarge.Code", "AUMEL", houseBill.PortOfDischarge.Code);
			AssertEquals("Transports", 4, houseBill.Transports.Count);
			AssertEquals("Transports.Main", directDepartureConsolTransport.JW_Vessel, houseBill.Transports.Main.Vessel.Name);
			AssertEquals("Consols.Departure", directDepartureConsol.JK_UniqueConsignRef, houseBill.Consols.Departure.ConsolNumber);
			AssertEquals("Consols.Arrival", directArrivalConsol.JK_UniqueConsignRef, houseBill.Consols.Arrival.ConsolNumber);
		}

		#endregion

		#region TestPopulateAddresses

		public void TestPopulateAddresses_ColoadWith()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "COPY"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNull(shipment.DepartureConsol);
			AssertNullOrEmpty(houseBill.ColoadWith.CompanyName);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine1);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine2);
			AssertNullOrEmpty(houseBill.ColoadWith.AdditionalAddressInformation);
			AssertNullOrEmpty(houseBill.ColoadWith.City);
			AssertNullOrEmpty(houseBill.ColoadWith.State);
			AssertNullOrEmpty(houseBill.ColoadWith.Postcode);
			AssertNullOrEmpty(houseBill.ColoadWith.Country.Code);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "CNCAN";
			departureConsol.JK_BookingReference = "BookingRef";
			departureConsol.JK_CoLoadBookingReference = "CoLoadBookingRef";
			departureConsol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			departureConsol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			Assert(departureConsol.IsCoLoad);
			AssertNullOrEmpty(houseBill.ColoadWith.CompanyName);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine1);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine2);
			AssertNullOrEmpty(houseBill.ColoadWith.AdditionalAddressInformation);
			AssertNullOrEmpty(houseBill.ColoadWith.City);
			AssertNullOrEmpty(houseBill.ColoadWith.State);
			AssertNullOrEmpty(houseBill.ColoadWith.Postcode);
			AssertNullOrEmpty(houseBill.ColoadWith.Country.Code);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);

			var creditorAddress = Factory.New<OrgHeader>();
			creditorAddress.OH_FullName = "BLACKPINK";
			creditorAddress.OH_RL_NKClosestPort = "CNSHG";
			creditorAddress.MainAddress.Address1 = "Unit 124";
			creditorAddress.MainAddress.Address2 = "24 Nanjing Road";
			creditorAddress.MainAddress.City = "ShangHai";
			creditorAddress.MainAddress.Postcode = "0023";
			creditorAddress.MainAddress.StateCode = "SH";
			creditorAddress.MainAddress.OA_RN_NKCountryCode = "CN";
			creditorAddress.MainAddress.OA_Fax = "+45 65 43 21 01";
			creditorAddress.MainAddress.OA_Phone = "+45 65 43 21 01";
			creditorAddress.MainAddress.OA_Email = "test@BLACKPINK.com";

			departureConsol.JK_OA_CreditorAddress = creditorAddress.MainAddress.PK;

			var shippingLineAddress = Factory.New<OrgHeader>();
			shippingLineAddress.OH_FullName = "JIMMY";
			shippingLineAddress.OH_RL_NKClosestPort = "NZAKL";
			shippingLineAddress.MainAddress.Address1 = "Unit 399";
			shippingLineAddress.MainAddress.Address2 = "50 What Lane";
			shippingLineAddress.MainAddress.City = "Auckland";
			shippingLineAddress.MainAddress.Postcode = "5023";
			shippingLineAddress.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingLineAddress.MainAddress.OA_Fax = "+66 65 43 21 01";
			shippingLineAddress.MainAddress.OA_Phone = "+66 65 43 21 01";
			shippingLineAddress.MainAddress.OA_Email = "test@JIMMY.com";
			shippingLineAddress.OH_IsShippingProvider = true;
			shippingLineAddress.OH_IsShippingLine = false;
			shippingLineAddress.OH_IsSeaWholesaler = false;

			departureConsol.JK_OA_ShippingLineAddress = shippingLineAddress.MainAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			Assert(departureConsol.IsCoLoad);
			AssertAddressData(departureConsol.CreditorAddress, houseBill.ColoadWith);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);

			departureConsol.JK_AgentType = Core.Constants.AgentType.Agent;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			Assert(!departureConsol.IsCoLoad);
			Assert(!departureConsol.ShippingLineIsNVOCC);
			AssertNullOrEmpty(houseBill.ColoadWith.CompanyName);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine1);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine2);
			AssertNullOrEmpty(houseBill.ColoadWith.AdditionalAddressInformation);
			AssertNullOrEmpty(houseBill.ColoadWith.City);
			AssertNullOrEmpty(houseBill.ColoadWith.State);
			AssertNullOrEmpty(houseBill.ColoadWith.Postcode);
			AssertNullOrEmpty(houseBill.ColoadWith.Country.Code);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);

			shippingLineAddress.OH_IsSeaWholesaler = true;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			Assert(!departureConsol.IsCoLoad);
			Assert(departureConsol.ShippingLineIsNVOCC);
			AssertAddressData(departureConsol.ShippingLineAddress, houseBill.ColoadWith);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);

			shippingLineAddress.OH_IsShippingLine = true;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			Assert(!departureConsol.IsCoLoad);
			Assert(!departureConsol.ShippingLineIsNVOCC);
			AssertNullOrEmpty(houseBill.ColoadWith.CompanyName);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine1);
			AssertNullOrEmpty(houseBill.ColoadWith.AddressLine2);
			AssertNullOrEmpty(houseBill.ColoadWith.AdditionalAddressInformation);
			AssertNullOrEmpty(houseBill.ColoadWith.City);
			AssertNullOrEmpty(houseBill.ColoadWith.State);
			AssertNullOrEmpty(houseBill.ColoadWith.Postcode);
			AssertNullOrEmpty(houseBill.ColoadWith.Country.Code);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);

			departureConsol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			Assert(!departureConsol.IsCoLoad);
			Assert(departureConsol.ShippingLineIsNVOCC);
			AssertAddressData(departureConsol.ShippingLineAddress, houseBill.ColoadWith);
			AssertNullOrEmpty(houseBill.ColoadWith.Fax);
			AssertNullOrEmpty(houseBill.ColoadWith.Email);
			AssertNullOrEmpty(houseBill.ColoadWith.Phone);
		}

		public void TestPopulateAddresses_GoodsDelivery()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "COPY"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertNull("DeliveryAgent", shipment.DeliveryAgent);
				Assert("Consols", !shipment.Consols.Any());
				AssertNull("LastDischargeConsol", shipment.LastDischargeConsol);
				AssertNullOrEmpty("CompanyName", houseBill.GoodsDelivery.CompanyName);
				AssertNullOrEmpty("AddressLine1", houseBill.GoodsDelivery.AddressLine1);
				AssertNullOrEmpty("AddressLine2", houseBill.GoodsDelivery.AddressLine2);
				AssertNullOrEmpty("AdditionalAddressInformation", houseBill.GoodsDelivery.AdditionalAddressInformation);
				AssertNullOrEmpty("City", houseBill.GoodsDelivery.City);
				AssertNullOrEmpty("State", houseBill.GoodsDelivery.State);
				AssertNullOrEmpty("Postcode", houseBill.GoodsDelivery.Postcode);
				AssertNullOrEmpty("Country", houseBill.GoodsDelivery.Country.Code);
				AssertNullOrEmpty("Fax", houseBill.GoodsDelivery.Fax);
				AssertNullOrEmpty("Email", houseBill.GoodsDelivery.Email);
				AssertNullOrEmpty("Phone", houseBill.GoodsDelivery.Phone);
			});

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNCAN";
			consol.JK_BookingReference = "BookingRef1";
			consol.JK_CoLoadBookingReference = "CoLoadBookingRef1";
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var consolReceivingForwarder = Factory.New<OrgHeader>();
			consolReceivingForwarder.OH_FullName = "JIMMY";
			consolReceivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			consolReceivingForwarder.MainAddress.Address1 = "Unit 399";
			consolReceivingForwarder.MainAddress.Address2 = "50 What Lane";
			consolReceivingForwarder.MainAddress.City = "Auckland";
			consolReceivingForwarder.MainAddress.Postcode = "5023";
			consolReceivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			consolReceivingForwarder.MainAddress.OA_Phone = "11234";

			var consolReceivingForwarderAddress = consolReceivingForwarder.Addresses.AddNew();
			consolReceivingForwarderAddress.Address1 = "Unit 400";
			consolReceivingForwarderAddress.Address2 = "40 What Lane";
			consolReceivingForwarderAddress.City = "Auckland";
			consolReceivingForwarderAddress.Postcode = "5033";
			consolReceivingForwarderAddress.OA_RN_NKCountryCode = "NZ";
			consolReceivingForwarderAddress.OA_Fax = "223333";
			consol.JK_OA_ReceivingForwarderAddress = consolReceivingForwarderAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertNull("DeliveryAgent", shipment.DeliveryAgent);
				Assert("Consols", shipment.Consols.Any());
				AssertNull("LastDischargeConsol", shipment.LastDischargeConsol);
				AssertEquals("CompanyName", "JIMMY", houseBill.GoodsDelivery.CompanyName);
				AssertEquals("AddressLine1", "Unit 400", houseBill.GoodsDelivery.AddressLine1);
				AssertEquals("AddressLine2", "40 What Lane", houseBill.GoodsDelivery.AddressLine2);
				AssertEquals("City", "Auckland", houseBill.GoodsDelivery.City);
				AssertEquals("Postcode", "5033", houseBill.GoodsDelivery.Postcode);
				AssertEquals("Country", "NZ", houseBill.GoodsDelivery.Country.Code);
				AssertEquals("Fax", "223333", houseBill.GoodsDelivery.Fax);
				AssertEquals("Phone", "11234", houseBill.GoodsDelivery.Phone);
			});

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "CONSOL0002";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "CNCAN";
			consol2.JK_RL_NKDischargePort = "AUMEL";
			consol2.JK_BookingReference = "BookingRef2";
			consol2.JK_CoLoadBookingReference = "CoLoadBookingRef2";
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var consol2ReceivingForwarder = Factory.New<OrgHeader>();
			consol2ReceivingForwarder.OH_FullName = "KIMMY";
			consol2ReceivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			consol2ReceivingForwarder.MainAddress.Address1 = "Unit 555";
			consol2ReceivingForwarder.MainAddress.Address2 = "66 What Lane";
			consol2ReceivingForwarder.MainAddress.City = "Auckland";
			consol2ReceivingForwarder.MainAddress.Postcode = "6666";
			consol2ReceivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			consol2ReceivingForwarder.MainAddress.OA_Phone = "66666";

			var consol2ReceivingForwarderAddress = consol2ReceivingForwarder.Addresses.AddNew();
			consol2ReceivingForwarderAddress.Address1 = "Unit 559";
			consol2ReceivingForwarderAddress.Address2 = "669 What Lane";
			consol2ReceivingForwarderAddress.City = "Auckland";
			consol2ReceivingForwarderAddress.Postcode = "0123";
			consol2ReceivingForwarderAddress.OA_RN_NKCountryCode = "NZ";
			consol2ReceivingForwarderAddress.OA_Fax = "777777";
			consol2.JK_OA_ReceivingForwarderAddress = consol2ReceivingForwarderAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertNull("DeliveryAgent", shipment.DeliveryAgent);
				Assert("Consols", shipment.Consols.Any());
				AssertNull("LastDischargeConsol", shipment.LastDischargeConsol);
				AssertEquals("CompanyName", "KIMMY", houseBill.GoodsDelivery.CompanyName);
				AssertEquals("AddressLine1", "Unit 559", houseBill.GoodsDelivery.AddressLine1);
				AssertEquals("AddressLine2", "669 What Lane", houseBill.GoodsDelivery.AddressLine2);
				AssertEquals("City", "Auckland", houseBill.GoodsDelivery.City);
				AssertEquals("Postcode", "0123", houseBill.GoodsDelivery.Postcode);
				AssertEquals("Country", "NZ", houseBill.GoodsDelivery.Country.Code);
				AssertEquals("Fax", "777777", houseBill.GoodsDelivery.Fax);
				AssertEquals("Phone", "66666", houseBill.GoodsDelivery.Phone);
			});

			var lastDischargeConsol = shipment.Consols.AddNew();
			lastDischargeConsol.JK_UniqueConsignRef = "CONSOL0003";
			lastDischargeConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			lastDischargeConsol.JK_RL_NKLoadPort = "AUSYD";
			lastDischargeConsol.JK_RL_NKDischargePort = "NZAKL";
			lastDischargeConsol.JK_BookingReference = "BookingRef3";
			lastDischargeConsol.JK_CoLoadBookingReference = "CoLoadBookingRef3";
			lastDischargeConsol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			lastDischargeConsol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var lastDischargeConsolReceivingForwarder = Factory.New<OrgHeader>();
			lastDischargeConsolReceivingForwarder.OH_FullName = "JIMMY2";
			lastDischargeConsolReceivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			lastDischargeConsolReceivingForwarder.MainAddress.Address1 = "Unit 299";
			lastDischargeConsolReceivingForwarder.MainAddress.Address2 = "22 What Lane";
			lastDischargeConsolReceivingForwarder.MainAddress.City = "Auckland";
			lastDischargeConsolReceivingForwarder.MainAddress.Postcode = "5023";
			lastDischargeConsolReceivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			lastDischargeConsolReceivingForwarder.MainAddress.OA_Phone = "000-00";
			lastDischargeConsolReceivingForwarder.MainAddress.OA_Fax = "1111-00";
			lastDischargeConsol.JK_OA_ReceivingForwarderAddress = lastDischargeConsolReceivingForwarder.MainAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertNull("DeliveryAgent", shipment.DeliveryAgent);
				Assert("Consols", shipment.Consols.Any());
				AssertNotNull("LastDischargeConsol", shipment.LastDischargeConsol);
				AssertEquals("CompanyName", "JIMMY2", houseBill.GoodsDelivery.CompanyName);
				AssertEquals("AddressLine1", "Unit 299", houseBill.GoodsDelivery.AddressLine1);
				AssertEquals("AddressLine2", "22 What Lane", houseBill.GoodsDelivery.AddressLine2);
				AssertEquals("City", "Auckland", houseBill.GoodsDelivery.City);
				AssertEquals("Postcode", "5023", houseBill.GoodsDelivery.Postcode);
				AssertEquals("Country", "NZ", houseBill.GoodsDelivery.Country.Code);
				AssertEquals("Fax", "1111-00", houseBill.GoodsDelivery.Fax);
				AssertEquals("Phone", "000-00", houseBill.GoodsDelivery.Phone);
			});

			var deliveryAgent = Factory.New<OrgHeader>();
			deliveryAgent.OH_FullName = "BLACKPINK";
			deliveryAgent.OH_RL_NKClosestPort = "CNSHG";
			deliveryAgent.MainAddress.Address1 = "Unit 124";
			deliveryAgent.MainAddress.Address2 = "24 Nanjing Road";
			deliveryAgent.MainAddress.City = "ShangHai";
			deliveryAgent.MainAddress.Postcode = "0023";
			deliveryAgent.MainAddress.StateCode = "SH";
			deliveryAgent.MainAddress.OA_RN_NKCountryCode = "CN";
			deliveryAgent.MainAddress.OA_Fax = "+45 65 43 21 01";
			deliveryAgent.MainAddress.OA_Phone = "+45 65 43 21 01";
			deliveryAgent.MainAddress.OA_Email = "test@BLACKPINK.com";

			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			CombineAssertions(() =>
			{
				AssertNotNull("DeliveryAgent", shipment.DeliveryAgent);
				Assert("Consols", shipment.Consols.Any());
				AssertNotNull("LastDischargeConsol", shipment.LastDischargeConsol);
				AssertEquals("CompanyName", "BLACKPINK", houseBill.GoodsDelivery.CompanyName);
				AssertEquals("AddressLine1", "Unit 124", houseBill.GoodsDelivery.AddressLine1);
				AssertEquals("AddressLine2", "24 Nanjing Road", houseBill.GoodsDelivery.AddressLine2);
				AssertEquals("City", "ShangHai", houseBill.GoodsDelivery.City);
				AssertEquals("Postcode", "0023", houseBill.GoodsDelivery.Postcode);
				AssertEquals("Country", "CN", houseBill.GoodsDelivery.Country.Code);
				AssertEquals("Fax", "+45 65 43 21 01", houseBill.GoodsDelivery.Fax);
				AssertEquals("Phone", "+45 65 43 21 01", houseBill.GoodsDelivery.Phone);
			});
		}

		#endregion

		#region TestPopulateFromForwardingShipment

		[TestDate(2018, 1, 1)]
		public void TestPopulateFromForwardingShipment()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var houseBill = (HouseBill)GetNewBusinessObject();

				CombineAssertions(() =>
				{
					AssertEquals("IsOriginal", false, houseBill.IsOriginal);
					AssertEquals("HouseBillNumber", "HOUSEBILL001", houseBill.HouseBillNumber);
					AssertEquals("ShipmentNumber", "SH0001", houseBill.ShipmentNumber);
					AssertEquals("ShippersReference", "BKG000001", houseBill.ShippersReference);
					AssertEquals(nameof(HouseBill.CarrierBookingReference), "BookingRef",
						houseBill.CarrierBookingReference);
					AssertEquals(nameof(HouseBill.CoLoadBookingReference), "CoLoadBookingRef",
						houseBill.CoLoadBookingReference);
					AssertEquals("Clause", "Oh my gut.", houseBill.Clause);
					AssertEquals("GoodsDescription", "goods description", houseBill.GoodsDescription);
					AssertEquals("MarksAndNumbers", "marks & numbers", houseBill.MarksAndNumbers);

					AssertEquals("ShipperLoadAndCount.Code", "SLC", houseBill.ShipperLoadAndCount.Code);
					AssertEquals("ShipperLoadAndCount.Description", "Shipper Load and Count",
						houseBill.ShipperLoadAndCount.Description);

					AssertEquals("INCO.Code", "CFR", houseBill.INCO.Code);
					AssertEquals("INCO.Description", "Cost And Freight", houseBill.INCO.Description);

					AssertEquals("CustomsEntryNumber", "T7HRTXGXT", houseBill.CustomsEntryNumber.Value);
					AssertEquals("CustomsEntryNumber", CANType.ContingencyCustomsAuthorityNumber.Code,
						houseBill.CustomsEntryNumber.Type.Code);
					AssertEquals("CustomsEntryNumber", CANType.ContingencyCustomsAuthorityNumber.Description,
						houseBill.CustomsEntryNumber.Type.Description);

					AssertEquals("MoveTypeFrom", "CY", houseBill.MoveTypeFrom);
					AssertEquals("MoveTypeTo", "CY", houseBill.MoveTypeTo);

					AssertEquals("MoveTypeList", "CFS, CY, DOOR", houseBill.MoveTypeList.CodesAsString);

					AssertEquals("NumberOfCopies", 1, houseBill.NumberOfCopies);
					AssertEquals("NumberOfOriginals", 2, houseBill.NumberOfOriginals);

					AssertEquals("ReleaseType.Code", ShipmentReleaseTypes.SeaWaybill, houseBill.ReleaseType.Code);
					AssertEquals("ReleaseType.Description", "Sea Waybill", houseBill.ReleaseType.Description);

					AssertEquals("HouseBillOfLadingType.Code", "FIA", houseBill.HouseBillOfLadingType.Code);

					AssertEquals("ContainerMode.Code", Core.Constants.ContainerModes.FCL, houseBill.ContainerMode.Code);
					AssertEquals("ContainerMode.Description", "Full Container Load",
						houseBill.ContainerMode.Description);

					AssertEquals("MainTransport.Vessel.Name", "Vessel", houseBill.Transports.Main.Vessel.Name);
					AssertEquals("MainTransport.Vessel.LloydsIMO", ZString.Empty,
						houseBill.Transports.Main.Vessel.LloydsIMO);
					AssertEquals("MainTransport.VoyageFlightNumber", "F9999",
						houseBill.Transports.Main.VoyageFlightNumber);

					AssertEquals("AUSYD", houseBill.PlaceOfReceipt.Code);
					AssertEquals("NZAKL", houseBill.PlaceOfDelivery.Code);

					AssertEquals("PortOfLoading.Code", "AUSYD", houseBill.PortOfLoading.Code);
					AssertEquals("PortOfDischarge.Code", "NZAKL", houseBill.PortOfDischarge.Code);

					AssertEquals("PortOfOrigin.Code", "AUSYD", houseBill.PortOfOrigin.Code);
					AssertEquals("PortOfDestination.Code", "NZAKL", houseBill.PortOfDestination.Code);

					AssertEquals("PaymentTerms.Code", "CCX", houseBill.PaymentTerms.Code);
					AssertEquals("PaymentTerms.Description", "Freight Collect", houseBill.PaymentTerms.Description);

					AssertEquals("FreightPayableAt.Code", shipment.Destination.Code, houseBill.FreightPayableAt.Code);

					AssertEquals("ShippedOnBoard.Code", "SHP", houseBill.ShippedOnBoard.Code);
					AssertEquals("ShippedOnBoard.Description", "Shipped", houseBill.ShippedOnBoard.Description);
					AssertEquals("ShippedOnBoard.Date", ZDate.Today, houseBill.ShippedOnBoard.Date);

					AssertEquals("DepartureDate", ZDate.Today.AddDays(1), houseBill.DepartureDate);
					AssertEquals("ArrivalDate", ZDate.Today.AddDays(2), houseBill.ArrivalDate);

					AssertEquals("GoodsDetailsTextOverride", ZString.Empty, houseBill.GoodsDetailsTextOverride);
					AssertEquals("ChargesTextOverride", ZString.Empty, houseBill.ChargesTextOverride);
					AssertEquals("FollowOnTextOverride", ZString.Empty, houseBill.FollowOnTextOverride);

					AssertEquals("AsAgentDetail exists", ZString.Empty, houseBill.AsAgentDetail);
					AssertEquals("FreightNominee exists", ZString.Empty, houseBill.FreightNominee);
					AssertEquals("CarrierAgent exists", ZString.Empty, houseBill.CarrierAgent);
				});

				AssertAddressData(shipment.Consignor, houseBill.Shipper);
				AssertAddressData(shipment.Consignee, houseBill.Consignee);
				AssertAddressData(GlbBranch.CurrentBranch.OrgProxy, houseBill.ForwardingAgent);
				AssertAddressData((OrgAddress)shipment.LastDischargeConsol.ReceivingForwarderWithContact.OrgAddress, houseBill.GoodsDelivery);
				AssertAddressData(shipment.NotifyParty, houseBill.NotifyParty);
				AssertAddressData(shipment.NotifyParty2DocumentaryAddress.Organisation, houseBill.NotifyParty2);
				AssertAddressData(shipment.NotifyParty3DocumentaryAddress.Organisation, houseBill.NotifyParty3);
				AssertAddressData(shipment.NotifyParty3DocumentaryAddress.Organisation, houseBill.NotifyParty3);
				AssertAddressData((OrgAddress)shipment.DepartureConsol.SendingForwarderWithContact.OrgAddress, houseBill.SendingForwarder);
				AssertAddressData((OrgAddress)shipment.DepartureConsol.ReceivingForwarderWithContact.OrgAddress, houseBill.ReceivingForwarder);

				AssertEquals("Containers.Count", 1, houseBill.Containers.Count);
				AssertEquals("Container PackingLines.Count", 2, houseBill.Containers.Single().PackingLines.Count);
				AssertEquals("LoosePackingLines.Count", 1, houseBill.LoosePackingLines.Count);
				AssertEquals("I'm loose baby!", houseBill.LoosePackingLines.Single().GoodsDescription);
				AssertContainerData(shipment.DepartureConsol.Containers.OfType<ForwardingContainer>().Single(),
					houseBill.Containers.Single());

				AssertEquals("expected no SubHouseBills", 0, houseBill.SubHouseBills.Count);
			}
		}

		#endregion

		#region TestFallbackOnShipmentPackTypeWhenPackingLinesHaveDifferentTypes

		public void TestFallbackOnShipmentPackTypeWhenPackingLinesHaveDifferentTypes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			container.PackLines.Add(packLine1);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			container.PackLines.Add(packLine2);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_F3_NKPackType = Core.Constants.PkgUnit.Reel;
			container.PackLines.Remove(packLine3);

			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_F3_NKPackType = Core.Constants.PkgUnit.Drum;
			container.PackLines.Remove(packLine4);

			AssertEquals("prerequisite: shipment has 4 packinglines", 4, shipment.OuterPackLines.Count);
			AssertEquals("prerequisite: container has 2 packinglines", 2, container.PackLines.Count);

			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Bag;

			var houseBill = new HouseBillBuilder(shipment, null).Build();

			AssertEquals("packed goods packtype fallback to shipment pack type", Core.Constants.PkgUnit.Bag, houseBill.TotalPackType.Code);
			AssertEquals("loose goods packtype fallback to shipment pack type", Core.Constants.PkgUnit.Bag, houseBill.TotalLoosePackType.Code);
		}

		#endregion

		#region TestPopulateOverrides

		public void TestPopulateOverrides()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride.Code, "goods details override");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HouseBillChargesOverride.Code, "charges override");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HouseBillFollowOnOverride.Code, "follow on override");

			var houseBill = new HouseBillBuilder(shipment, null).Build();

			AssertEquals("GoodsDetailsTextOverride", "goods details override", houseBill.GoodsDetailsTextOverride);
			AssertEquals("HasGoodsDetailsTextOverride", ZBool.True, houseBill.HasGoodsDetailsTextOverride);

			AssertEquals("ChargesTextOverride", "charges override", houseBill.ChargesTextOverride);
			AssertEquals("HasChargesTextOverride", ZBool.True, houseBill.HasChargesTextOverride);

			AssertEquals("FollowOnTextOverride", "follow on override", houseBill.FollowOnTextOverride);
			AssertEquals("HasFollowOnTextOverride", ZBool.True, houseBill.HasFollowOnTextOverride);
		}

		#endregion

		#region TestPopulateFromForwardingShipment_Empty

		public void TestPopulateFromForwardingShipment_Empty()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(Factory.New<ForwardingShipment>(), parameters).Build();

			AssertNotNull("data object has been created for null shipment", houseBill);
			AssertEquals("HouseBill data object has been created", typeof(HouseBill), houseBill.GetType());
		}

		#endregion

		#region TestPopulationOfNotes

		public void TestPopulateofNotesFromShipment()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();

			var marksAndNumbers = shipment.Notes.AddNew();
			marksAndNumbers.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbers.ST_NoteDataAsText = "Marks & Numbers";

			var goodsDescription = shipment.Notes.AddNew();
			goodsDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescription.ST_NoteDataAsText = "Goods Description";

			var handlingInstructions = shipment.Notes.AddNew();
			handlingInstructions.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			handlingInstructions.ST_NoteDataAsText = "Handling Instructions";

			var certificateOfOrigin = shipment.Notes.AddNew();
			certificateOfOrigin.ST_Description = PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description;
			certificateOfOrigin.ST_NoteDataAsText = "Certificate of Origin";

			var countryRules = shipment.Notes.AddNew();
			countryRules.ST_Description = PredefinedNoteTypes.Instance.CountryRules.Description;
			countryRules.ST_NoteDataAsText = "Country Rules";

			var clientVisibleJob = shipment.Notes.AddNew();
			clientVisibleJob.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description;
			clientVisibleJob.ST_NoteDataAsText = "Client Visible Job";

			var importDeliveryInstructions = shipment.Notes.AddNew();
			importDeliveryInstructions.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			importDeliveryInstructions.ST_NoteDataAsText = "Import Delivery Instructions";

			var exportPickupInstructions = shipment.Notes.AddNew();
			exportPickupInstructions.ST_Description = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			exportPickupInstructions.ST_NoteDataAsText = "Export Pickup Instructions";

			var specialInstructions = shipment.Notes.AddNew();
			specialInstructions.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			specialInstructions.ST_NoteDataAsText = "Special Instructions";

			var bookingNotes = shipment.Notes.AddNew();
			bookingNotes.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Description;
			bookingNotes.ST_NoteDataAsText = "Booking Notes";

			var deliveryOrderReceiptNotes = shipment.Notes.AddNew();
			deliveryOrderReceiptNotes.ST_Description = PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description;
			deliveryOrderReceiptNotes.ST_NoteDataAsText = "Delivery Order Receipt Notes";

			var manifestGoodsDescription = shipment.Notes.AddNew();
			manifestGoodsDescription.ST_Description = PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description;
			manifestGoodsDescription.ST_NoteDataAsText = "Manifest Goods Description";

			var dangerousGoodsHandlingInfo = shipment.Notes.AddNew();
			dangerousGoodsHandlingInfo.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			dangerousGoodsHandlingInfo.ST_NoteDataAsText = "Dangerous Goods Additional Handling";

			var awbRatelineOvertypesNotes = shipment.Notes.AddNew();
			awbRatelineOvertypesNotes.ST_Description = PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description;
			awbRatelineOvertypesNotes.ST_NoteDataAsText = "AWB Rateline Overtypes Notes";

			var customsInstructionsNotes = shipment.Notes.AddNew();
			customsInstructionsNotes.ST_Description = PredefinedNoteTypes.Instance.CustomsInstructionNotes.Description;
			customsInstructionsNotes.ST_NoteDataAsText = "Customs Instructions";

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("Marks & Numbers", marksAndNumbers.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.MarksAndNumbers.Description));
			AssertEquals("Goods Description", goodsDescription.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description));
			AssertEquals("Handling Instructions", handlingInstructions.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.HandlingInstructions.Description));
			AssertEquals("Certificate of Origin", certificateOfOrigin.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description));
			AssertEquals("Country Rules", countryRules.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.CountryRules.Description));
			AssertEquals("Client Visible Job Notes", clientVisibleJob.ST_NoteDataAsText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description));
			AssertEquals("Delivery Instructions", importDeliveryInstructions.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description));
			AssertEquals("Pickup Instructions Note", exportPickupInstructions.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description));
			AssertEquals("Special Instructions", specialInstructions.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.SpecialInstructions.Description));
			AssertEquals("Booking Notes", bookingNotes.ST_NoteDataAsText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.BookingNotes.Description));
			AssertEquals("Delivery Order Receipt Notes", deliveryOrderReceiptNotes.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description));
			AssertEquals("Manifest Goods Description", manifestGoodsDescription.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description));
			AssertEquals("Dangerous Goods Additional Handling Information", dangerousGoodsHandlingInfo.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description));
			AssertEquals("AWB Rateline Overtyped Notes", awbRatelineOvertypesNotes.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes.Description));
			AssertEquals("Customs Instructions", customsInstructionsNotes.ST_NoteText, GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.CustomsInstructionNotes.Description));
		}

		ZString GetNoteTextByDescription(HouseBill houseBill, string description)
		{
			return houseBill
				.Notes
				.SingleOrDefault(note => note.Description == description)
				.Text;
		}

		public void TestMissingNotesFromShipment()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Precondition: no notes", false, shipment.Notes.HasNotes);

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Notes collection is empty", 0, houseBill.Notes.Count);
		}

		public void TestGoodsDescription_NoFullWidthCharacters()
		{
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GoodsDescription = "ＡＢＣＤＥＦＧａｂｃｄｅｆｇ，";

			var houseBill = new HouseBillBuilder(shipment, null).Build();

			AssertEquals("GoodsDescription", "ABCDEFGabcdefg,", houseBill.GoodsDescription);
		}

		#endregion

		#region TestTaxNumber

		#region General
		void PopulateRefData()
		{
			var cnpj = Factory.New<RefDocOrgCusCode>();
			cnpj.DOC_DocumentType = "HBL";
			cnpj.DOC_RN_NKCodeCountry = CountryCodes.Brazil;
			cnpj.DOC_RN_NKRegulatingCountry = CountryCodes.Brazil;
			cnpj.DOC_Notes = "Tax Id";
			cnpj.DOC_CodeType = "CJN";
			cnpj.DOC_ShortLabel = "CJN";
			cnpj.DOC_Priority = 1;

			var com = Factory.New<RefDocOrgCusCode>();
			com.DOC_DocumentType = "HBL";
			com.DOC_RN_NKCodeCountry = CountryCodes.Brazil;
			com.DOC_RN_NKRegulatingCountry = CountryCodes.Brazil;
			com.DOC_Notes = "Tax Id";
			com.DOC_CodeType = "COM";
			com.DOC_ShortLabel = "COM";
			com.DOC_Priority = 2;

			var nit = Factory.New<RefDocOrgCusCode>();
			nit.DOC_DocumentType = "HBL";
			nit.DOC_RN_NKCodeCountry = CountryCodes.Colombia;
			nit.DOC_RN_NKRegulatingCountry = CountryCodes.Colombia;
			nit.DOC_Notes = "TEST";
			nit.DOC_CodeType = "NIT";
			nit.DOC_ShortLabel = "NIT";
			nit.DOC_Priority = 1;

			var gcr = Factory.New<RefDocOrgCusCode>();
			gcr.DOC_DocumentType = "HBL";
			gcr.DOC_RN_NKCodeCountry = CountryCodes.Colombia;
			gcr.DOC_RN_NKRegulatingCountry = CountryCodes.Colombia;
			gcr.DOC_Notes = "TEST";
			gcr.DOC_CodeType = "GCR";
			gcr.DOC_ShortLabel = "GCR";
			gcr.DOC_Priority = 2;

			Factory.Save();
		}

		public void TestTaxNumber()
		{
			PopulateRefData();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "COMDE";
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "COMDE";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER";
			shipper.OH_RL_NKClosestPort = "BRASO";
			shipper.MainAddress.Address1 = "Unit 15";
			shipper.MainAddress.Address2 = "1 E Lane";
			shipper.MainAddress.City = "São Paulo";
			shipper.MainAddress.Postcode = "14780";
			shipper.MainAddress.OA_RN_NKCountryCode = "BR";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "COMDE";
			consignee.MainAddress.Address1 = "Unit 15";
			consignee.MainAddress.Address2 = "1 E Lane";
			consignee.MainAddress.City = "Medellín";
			consignee.MainAddress.Postcode = "050022";
			consignee.MainAddress.OA_RN_NKCountryCode = "CO";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFYPARTY";
			notifyParty.OH_RL_NKClosestPort = "COARQ";
			notifyParty.MainAddress.Address1 = "Unit 15";
			notifyParty.MainAddress.Address2 = "1 E Lane";
			notifyParty.MainAddress.City = "‎Arauquita‎";
			notifyParty.MainAddress.Postcode = "050022";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "CO";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var brTaxNum1 = shipper.CustomsCodes.AddNew();
			brTaxNum1.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			brTaxNum1.OK_RN_NKCodeCountry = CountryCodes.Brazil;
			brTaxNum1.OK_CustomsRegNo = "TAXBR001";

			var coTaxNum1 = consignee.CustomsCodes.AddNew();
			coTaxNum1.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			coTaxNum1.OK_RN_NKCodeCountry = CountryCodes.Colombia;
			coTaxNum1.OK_CustomsRegNo = "TAXCO002";

			var veTaxNum1 = notifyParty.CustomsCodes.AddNew();
			veTaxNum1.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			veTaxNum1.OK_RN_NKCodeCountry = CountryCodes.Colombia;
			veTaxNum1.OK_CustomsRegNo = "TAXCO003";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("TAXBR001", houseBill.ShipperTaxInfo.Number);
			AssertEquals("TAXCO002", houseBill.ConsigneeTaxInfo.Number);
			AssertEquals("TAXCO003", houseBill.NotifyPartyTaxInfo.Number);

			AssertEquals("COM", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertEquals("GCR", houseBill.ConsigneeTaxInfo.DisplayedLabel);
			AssertEquals("GCR", houseBill.NotifyPartyTaxInfo.DisplayedLabel);

			var brTaxNum2 = shipper.CustomsCodes.AddNew();
			brTaxNum2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			brTaxNum2.OK_RN_NKCodeCountry = CountryCodes.Brazil;
			brTaxNum2.OK_CustomsRegNo = "TAXBR001";

			var coTaxNum2 = consignee.CustomsCodes.AddNew();
			coTaxNum2.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			coTaxNum2.OK_RN_NKCodeCountry = CountryCodes.Colombia;
			coTaxNum2.OK_CustomsRegNo = "TAXCO002";

			var veTaxNum2 = notifyParty.CustomsCodes.AddNew();
			veTaxNum2.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			veTaxNum2.OK_RN_NKCodeCountry = CountryCodes.Colombia;
			veTaxNum2.OK_CustomsRegNo = "TAXCO003";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("TAXBR001", houseBill.ShipperTaxInfo.Number);
			AssertEquals("TAXCO002", houseBill.ConsigneeTaxInfo.Number);
			AssertEquals("TAXCO003", houseBill.NotifyPartyTaxInfo.Number);

			AssertEquals("CJN", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertEquals("NIT", houseBill.ConsigneeTaxInfo.DisplayedLabel);
			AssertEquals("NIT", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
		}

		public void TestTaxNumber_WhenImportToBangladesh_ShouldUseBIN()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "BIN", ZString.Empty, "HBL");
			CreateRefDocOrgCusCode("AIN", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "AIN", ZString.Empty, "HBL");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bangladesh Consignee";
			consignee.OH_RL_NKClosestPort = "BDDAC";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BD";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			consol.ReceivingForwarderWithContact.OrgPK = consignee.PK;

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			shipment.NotifyPartyContactPK = ZGuid.Empty;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = ZGuid.Empty;

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertHasMessageError(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.BangladeshConsigneeMissingBIN);
			AssertNoMessageError(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.BangladeshNotifyPartyMissingBIN);

			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertHasMessageError(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.BangladeshConsigneeMissingBIN);
			AssertHasMessageError(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.BangladeshNotifyPartyMissingBIN);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			Factory.Save();

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertNoMessageError(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.BangladeshConsigneeMissingBIN);
			AssertNoMessageError(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.BangladeshNotifyPartyMissingBIN);
			AssertEquals("123BIN", houseBill.ConsigneeTaxInfo.Number);
			AssertEquals("BIN", houseBill.ConsigneeTaxInfo.DisplayedLabel);
			AssertEquals("123BIN", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("BIN", houseBill.NotifyPartyTaxInfo.DisplayedLabel);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNoMessageError(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.BangladeshConsigneeMissingBIN);
			AssertNoMessageError(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.BangladeshNotifyPartyMissingBIN);
			AssertEquals("123BIN", houseBill.ConsigneeTaxInfo.Number);
			AssertEquals("BIN", houseBill.ConsigneeTaxInfo.DisplayedLabel);
			AssertEquals("123BIN", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("BIN", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
		}

		void CreateRefDocOrgCusCode(ZString code, ZString regulatingCountry, ZString codeCountry, ZByte priority, ZString shortLabel, ZString longLabel, ZString documentType)
		{
			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = code;
			orgCusCode.DOC_RN_NKRegulatingCountry = regulatingCountry;
			orgCusCode.DOC_RN_NKCodeCountry = codeCountry;
			orgCusCode.DOC_Priority = priority;
			orgCusCode.DOC_ShortLabel = shortLabel;
			orgCusCode.DOC_LongLabel = longLabel;
			orgCusCode.DOC_DocumentType = documentType;
		}
		#endregion

		#region India

		void PopulateRefDataIndia()
		{
			var indiaPAN = Factory.New<RefDocOrgCusCode>();
			indiaPAN.DOC_DocumentType = "HBL";
			indiaPAN.DOC_RN_NKCodeCountry = CountryCodes.India;
			indiaPAN.DOC_RN_NKRegulatingCountry = CountryCodes.India;
			indiaPAN.DOC_Notes = "Tax Id";
			indiaPAN.DOC_CodeType = "PAN";
			indiaPAN.DOC_ShortLabel = "PAN";
			indiaPAN.DOC_Priority = 1;

			var indiaIEC = Factory.New<RefDocOrgCusCode>();
			indiaIEC.DOC_DocumentType = "HBL";
			indiaIEC.DOC_RN_NKCodeCountry = CountryCodes.India;
			indiaIEC.DOC_RN_NKRegulatingCountry = CountryCodes.India;
			indiaIEC.DOC_Notes = "TEST";
			indiaIEC.DOC_CodeType = "IEC";
			indiaIEC.DOC_ShortLabel = "IEC";
			indiaIEC.DOC_Priority = 1;

			Factory.Save();
		}

		public void TestTaxNumberForIndia()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER";
			shipper.OH_RL_NKClosestPort = "INABH";
			shipper.MainAddress.Address1 = "Unit 15";
			shipper.MainAddress.Address2 = "1 E Lane";
			shipper.MainAddress.City = "São Paulo";
			shipper.MainAddress.Postcode = "14780";
			shipper.MainAddress.OA_RN_NKCountryCode = "IN";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "INABH";
			consignee.MainAddress.Address1 = "Unit 15";
			consignee.MainAddress.Address2 = "1 E Lane";
			consignee.MainAddress.City = "IndiaCity";
			consignee.MainAddress.Postcode = "050022";
			consignee.MainAddress.OA_RN_NKCountryCode = "IN";

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFYPARTY";
			notifyParty.OH_RL_NKClosestPort = "INABH";
			notifyParty.MainAddress.Address1 = "Unit 15";
			notifyParty.MainAddress.Address2 = "1 E Lane";
			notifyParty.MainAddress.City = "‎Arauquita‎";
			notifyParty.MainAddress.Postcode = "050022";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "IN";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var iecTaxNumShipiper = shipper.CustomsCodes.AddNew();
			iecTaxNumShipiper.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
			iecTaxNumShipiper.OK_RN_NKCodeCountry = CountryCodes.India;
			iecTaxNumShipiper.OK_CustomsRegNo = "IEC0001";

			var iecTaxNumConsignee = consignee.CustomsCodes.AddNew();
			iecTaxNumConsignee.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
			iecTaxNumConsignee.OK_RN_NKCodeCountry = CountryCodes.India;
			iecTaxNumConsignee.OK_CustomsRegNo = "IEC0001";

			var iecTaxNumNotifyParty = notifyParty.CustomsCodes.AddNew();
			iecTaxNumNotifyParty.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
			iecTaxNumNotifyParty.OK_RN_NKCodeCountry = CountryCodes.India;
			iecTaxNumNotifyParty.OK_CustomsRegNo = "PAN0001";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("IEC0001", houseBill.ShipperTaxInfo.Number);
			AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
			AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);

			AssertEquals("IEC", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertEquals("IEC", houseBill.ConsigneeTaxInfo.DisplayedLabel);
			AssertEquals("PAN", houseBill.NotifyPartyTaxInfo.DisplayedLabel);

			shipper.CustomsCodes.RemoveAll();
			var panTaxNumShipiper = shipper.CustomsCodes.AddNew();
			panTaxNumShipiper.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
			panTaxNumShipiper.OK_RN_NKCodeCountry = CountryCodes.India;
			panTaxNumShipiper.OK_CustomsRegNo = "PAN0001";

			consignee.CustomsCodes.RemoveAll();
			var panTaxNumConsignee = consignee.CustomsCodes.AddNew();
			panTaxNumConsignee.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
			panTaxNumConsignee.OK_RN_NKCodeCountry = CountryCodes.India;
			panTaxNumConsignee.OK_CustomsRegNo = "PAN0001";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("PAN0001", houseBill.ShipperTaxInfo.Number);
			AssertEquals("PAN0001", houseBill.ConsigneeTaxInfo.Number);

			AssertEquals("PAN", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertEquals("PAN", houseBill.ConsigneeTaxInfo.DisplayedLabel);
		}

		public void TestTaxNumberForIndiaWarningMsgWhenIndiaConsigneeMissingIECAndPAN()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "INDEL";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "INDEL";
			consignee.MainAddress.Address1 = "Unit 15";
			consignee.MainAddress.Address2 = "1 E Lane";
			consignee.MainAddress.City = "Medellín";
			consignee.MainAddress.Postcode = "050022";
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			consignee.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, "2934798", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			consignee.CustomsCodes.RemoveAll();
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			((Address)houseBill.Consignee).CompanyName = "TO ORDER";
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			((Address)houseBill.Consignee).CompanyName = "CONSIGNEE";
			((Address)houseBill.Consignee).AddressFormatted = "TO ORDER";
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			((Address)houseBill.Consignee).CompanyName = "CONSIGNEE";
			((Address)houseBill.Consignee).AddressFormatted = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			consignee.OH_RL_NKClosestPort = "INDEL";
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;
			consignee.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "779494", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			consignee.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, "2934798", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);
		}

		public void TestTaxNumberForIndiaWarningMsgWhenIndiaConsignorMissingIECAndPAN()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "INDEL";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.OH_RL_NKClosestPort = "INDEL";
			consignor.MainAddress.Address1 = "Unit 15";
			consignor.MainAddress.Address2 = "1 E Lane";
			consignor.MainAddress.City = "Medellín";
			consignor.MainAddress.Postcode = "050022";
			consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(houseBill.ShipperTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsignorMissingIECOrPanMessage);

			consignor.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, "2934798", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ShipperTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsignorMissingIECOrPanMessage);

			consignor.CustomsCodes.RemoveAll();
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(houseBill.ShipperTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsignorMissingIECOrPanMessage);

			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ShipperTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsignorMissingIECOrPanMessage);

			consignor.OH_RL_NKClosestPort = "INDEL";
			consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;
			consignor.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "779494", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.ShipperTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsignorMissingIECOrPanMessage);
		}

		public void TestTaxNumberForIndiaWarningMsgWhenIndiaNotifyPartyMissingIECAndPAN()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "INDEL";

			var notifyPart = Factory.New<OrgHeader>();
			notifyPart.OH_FullName = "NOFITYPARTY";
			notifyPart.OH_RL_NKClosestPort = "INDEL";
			notifyPart.MainAddress.Address1 = "Unit 16";
			notifyPart.MainAddress.Address2 = "2 E Lane";
			notifyPart.MainAddress.City = "Medellín";
			notifyPart.MainAddress.Postcode = "050023";
			notifyPart.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPart.MainAddress.PK;

			var houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			((Address)houseBill.NotifyParty).CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			((Address)houseBill.NotifyParty).CompanyName = "NOFITYPARTY";
			((Address)houseBill.NotifyParty).AddressFormatted = "SAME AS CONSIGNEE";
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			((Address)houseBill.NotifyParty).CompanyName = "NOFITYPARTY";
			((Address)houseBill.NotifyParty).AddressFormatted = "NOFITYPARTY";
			notifyPart.OH_RL_NKClosestPort = "AUSYD";
			notifyPart.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			notifyPart.OH_RL_NKClosestPort = "INDEL";
			notifyPart.MainAddress.OA_RN_NKCountryCode = CountryCodes.India;
			notifyPart.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, "779494", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			notifyPart.CustomsCodes.RemoveAndDeleteAll();
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);

			notifyPart.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, "779494", CountryCodes.India);
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, HouseBillBuilder.IndiaConsigneeOrNotifyPartyMissingIECOrPanMessage);
		}

		public void TestTaxNumberForIndiaWarningMsgWhenBothOfIndiaConsigneeAndNotifyPartyMissingEmailAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "INDEL";

			var houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertHasWarning(((Address)houseBill.Consignee).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);
			AssertHasWarning(((Address)houseBill.NotifyParty).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);

			houseBill.Consignee.Email = "blah@blahblah.blah";
			AssertNoWarning(((Address)houseBill.Consignee).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);
			AssertNoWarning(((Address)houseBill.NotifyParty).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);

			houseBill.Consignee.Email = "";
			AssertHasWarning(((Address)houseBill.Consignee).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);
			AssertHasWarning(((Address)houseBill.NotifyParty).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);

			houseBill.NotifyParty.Email = "blah@blahblah.blah";
			AssertNoWarning(((Address)houseBill.Consignee).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);
			AssertNoWarning(((Address)houseBill.NotifyParty).EmailInfo, HouseBillBuilder.IndiaBothOfConsigneeAndNotifyPartyMissingEmailMessage);
		}

		#endregion

		#region Egypt
		void PopulateRefDataEgypt()
		{
			var egyptVAT = Factory.New<RefDocOrgCusCode>();
			egyptVAT.DOC_DocumentType = "HBL";
			egyptVAT.DOC_RN_NKCodeCountry = CountryCodes.Egypt;
			egyptVAT.DOC_RN_NKRegulatingCountry = CountryCodes.Egypt;
			egyptVAT.DOC_Notes = "Tax Id";
			egyptVAT.DOC_CodeType = "VAT";
			egyptVAT.DOC_Priority = 1;

			var egyptCOM = Factory.New<RefDocOrgCusCode>();
			egyptCOM.DOC_DocumentType = "HBL";
			egyptCOM.DOC_RN_NKCodeCountry = CountryCodes.Egypt;
			egyptCOM.DOC_RN_NKRegulatingCountry = CountryCodes.Egypt;
			egyptCOM.DOC_Notes = "Tax Id";
			egyptCOM.DOC_CodeType = "COM";
			egyptCOM.DOC_ShortLabel = "CRN";
			egyptCOM.DOC_Priority = 2;

			var egyptGCR = Factory.New<RefDocOrgCusCode>();
			egyptGCR.DOC_DocumentType = "HBL";
			egyptGCR.DOC_RN_NKCodeCountry = CountryCodes.Egypt;
			egyptGCR.DOC_RN_NKRegulatingCountry = CountryCodes.Egypt;
			egyptGCR.DOC_Notes = "TEST";
			egyptGCR.DOC_CodeType = "GCR";
			egyptGCR.DOC_ShortLabel = "GCR";
			egyptGCR.DOC_Priority = 2;

			var australiaCOM = Factory.New<RefDocOrgCusCode>();
			australiaCOM.DOC_DocumentType = "HBL";
			australiaCOM.DOC_RN_NKCodeCountry = CountryCodes.Australia;
			australiaCOM.DOC_RN_NKRegulatingCountry = CountryCodes.Egypt;
			australiaCOM.DOC_Notes = "Tax Id";
			australiaCOM.DOC_CodeType = "ABN";
			australiaCOM.DOC_ShortLabel = "ABN";
			australiaCOM.DOC_Priority = 1;

			var australiaGCR = Factory.New<RefDocOrgCusCode>();
			australiaGCR.DOC_DocumentType = "HBL";
			australiaGCR.DOC_RN_NKCodeCountry = CountryCodes.Australia;
			australiaGCR.DOC_RN_NKRegulatingCountry = CountryCodes.Egypt;
			australiaGCR.DOC_Notes = "TEST";
			australiaGCR.DOC_CodeType = "GCR";
			australiaGCR.DOC_ShortLabel = "GCR";
			australiaGCR.DOC_Priority = 2;

			Factory.Save();
		}

		public void TestTaxNumberForEGImport_Consignee()
		{
			const string expectedWarning = "It is recommended to fill in the Commercial Registration Number to comply with Advance Cargo Information (ACI) for Egypt";

			PopulateRefDataEgypt();

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var consigneeEG = Factory.New<OrgHeader>();
			consigneeEG.OH_FullName = "DUMMY";
			consigneeEG.OH_RL_NKClosestPort = "EGCAI";
			consigneeEG.MainAddress.Address1 = "Unit 1";
			consigneeEG.MainAddress.Address2 = "4 What Lane";
			consigneeEG.MainAddress.City = "Auckland";
			consigneeEG.MainAddress.Postcode = "5022";
			consigneeEG.MainAddress.OA_RN_NKCountryCode = "EG";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeEG.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("Consginee Tax Number should be empty", string.Empty, houseBill.ConsigneeTaxInfo.Number);
			AssertHasWarning("Consginee Tax Number should have a warning", houseBill.ConsigneeTaxInfo.NumberInfo, expectedWarning);

			var consingeeGCR = shipment.Consignee.CustomsCodes.AddNew();
			consingeeGCR.OK_RN_NKCodeCountry = "EG";
			consingeeGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consingeeGCR.OK_CustomsRegNo = "";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consginee Tax Number should be empty", string.Empty, houseBill.ConsigneeTaxInfo.Number);
			AssertHasWarning("Consginee Tax Number should have a warning", houseBill.ConsigneeTaxInfo.NumberInfo, expectedWarning);

			shipment.Consignee.CustomsCodes.RemoveAll();
			consingeeGCR = shipment.Consignee.CustomsCodes.AddNew();
			consingeeGCR.OK_RN_NKCodeCountry = "EG";
			consingeeGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consingeeGCR.OK_CustomsRegNo = "123456";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consginee Tax Number should be filled in", "123456", houseBill.ConsigneeTaxInfo.Number);
			AssertNoWarning("Consginee Tax Number should not have a warning", houseBill.ConsigneeTaxInfo.NumberInfo, expectedWarning);

			var consingeeCOM = shipment.Consignee.CustomsCodes.AddNew();
			consingeeCOM.OK_RN_NKCodeCountry = "EG";
			consingeeCOM.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			consingeeCOM.OK_CustomsRegNo = "7890";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consginee Tax Number should be filled in", "7890", houseBill.ConsigneeTaxInfo.Number);
			AssertNoWarning("Consginee Tax Number should not have a warning", houseBill.ConsigneeTaxInfo.NumberInfo, expectedWarning);

			var consingeeVAT = shipment.Consignee.CustomsCodes.AddNew();
			consingeeVAT.OK_RN_NKCodeCountry = "EG";
			consingeeVAT.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			consingeeVAT.OK_CustomsRegNo = "AB1234";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consginee Tax Number should be filled in", "AB1234", houseBill.ConsigneeTaxInfo.Number);
			AssertNoWarning("Consginee Tax Number should not have a warning", houseBill.ConsigneeTaxInfo.NumberInfo, expectedWarning);
		}

		public void TestTaxNumberForEGImport_Consignor()
		{
			const string expectedWarning = "It is recommended to fill in the Export Registration Number to comply with Advance Cargo Information (ACI) for Egypt";

			PopulateRefDataEgypt();

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var shipperAU = Factory.New<OrgHeader>();
			shipperAU.OH_FullName = "MAERSK";
			shipperAU.OH_RL_NKClosestPort = "AUSYD";
			shipperAU.MainAddress.Address1 = "Unit 13";
			shipperAU.MainAddress.Address2 = "4 Lost Lane";
			shipperAU.MainAddress.City = "Sydney";
			shipperAU.MainAddress.Postcode = "2000";
			shipperAU.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAU.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("Consignor Tax Number should be empty", string.Empty, houseBill.ShipperTaxInfo.Number);
			AssertHasWarning("Consignor Tax Number should have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			var consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "AU";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be empty", string.Empty, houseBill.ShipperTaxInfo.Number);
			AssertHasWarning("Consignor Tax Number should have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			shipment.Consignor.CustomsCodes.RemoveAll();
			consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "AU";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "123456";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be filled in", "AU-01-123456", houseBill.ShipperTaxInfo.Number);
			AssertEquals("GCR", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertNoWarning("Consignor Tax Number should not have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			var consignorABN = shipment.Consignor.CustomsCodes.AddNew();
			consignorABN.OK_RN_NKCodeCountry = "AU";
			consignorABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			consignorABN.OK_CustomsRegNo = "123456";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be filled in", "AU-02-123456", houseBill.ShipperTaxInfo.Number);
			AssertEquals("ABN", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertNoWarning("Consignor Tax Number should not have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			var shipperEG = Factory.New<OrgHeader>();
			shipperEG.OH_FullName = "MAERSK";
			shipperEG.OH_RL_NKClosestPort = "EGCAI";
			shipperEG.MainAddress.Address1 = "Unit 13";
			shipperEG.MainAddress.Address2 = "4 Lost Lane";
			shipperEG.MainAddress.City = "Sydney";
			shipperEG.MainAddress.Postcode = "2000";
			shipperEG.MainAddress.OA_RN_NKCountryCode = "EG";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperEG.MainAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("Consignor Tax Number should be empty", string.Empty, houseBill.ShipperTaxInfo.Number);
			AssertHasWarning("Consignor Tax Number should have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "EG";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be empty", string.Empty, houseBill.ShipperTaxInfo.Number);
			AssertHasWarning("Consignor Tax Number should have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			shipment.Consignor.CustomsCodes.RemoveAll();
			consignorABN = shipment.Consignor.CustomsCodes.AddNew();
			consignorABN.OK_RN_NKCodeCountry = "EG";
			consignorABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			consignorABN.OK_CustomsRegNo = "123456";

			consignorGCR = shipment.Consignor.CustomsCodes.AddNew();
			consignorGCR.OK_RN_NKCodeCountry = "EG";
			consignorGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			consignorGCR.OK_CustomsRegNo = "123456";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be filled in", "EG-01-123456", houseBill.ShipperTaxInfo.Number);
			AssertNoWarning("Consignor Tax Number should not have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			var consignorCOM = shipment.Consignor.CustomsCodes.AddNew();
			consignorCOM.OK_RN_NKCodeCountry = "EG";
			consignorCOM.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			consignorCOM.OK_CustomsRegNo = "7890";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be filled in", "EG-02-7890", houseBill.ShipperTaxInfo.Number);
			AssertEquals("When Dest is Egypt and Short Label not empty ,the Tax type should first be Short Label ", "CRN", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertNoWarning("Consignor Tax Number should not have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);

			var consingeeVAT = shipment.Consignor.CustomsCodes.AddNew();
			consingeeVAT.OK_RN_NKCodeCountry = "EG";
			consingeeVAT.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			consingeeVAT.OK_CustomsRegNo = "AB1234";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("Consignor Tax Number should be filled in", "EG-02-AB1234", houseBill.ShipperTaxInfo.Number);
			AssertEquals("When Dest is Egypt and Short Label is empty ,the Tax type should fallback to Code Type ", "VAT", houseBill.ShipperTaxInfo.DisplayedLabel);
			AssertNoWarning("Consignor Tax Number should not have a warning", houseBill.ShipperTaxInfo.NumberInfo, expectedWarning);
		}

		public void TestTaxNumberForEGImport_NotifyParty()
		{
			PopulateRefDataEgypt();

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";
			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var notifyPartyAU = Factory.New<OrgHeader>();
			notifyPartyAU.OH_FullName = "MAERSK";
			notifyPartyAU.OH_RL_NKClosestPort = "AUSYD";
			notifyPartyAU.MainAddress.Address1 = "Unit 13";
			notifyPartyAU.MainAddress.Address2 = "4 Lost Lane";
			notifyPartyAU.MainAddress.City = "Sydney";
			notifyPartyAU.MainAddress.Postcode = "2000";
			notifyPartyAU.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPartyAU.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("NotifyParty Tax Number should be empty", string.Empty, houseBill.NotifyPartyTaxInfo.Number);

			var notifyPartyGCR = shipment.NotifyParty.CustomsCodes.AddNew();
			notifyPartyGCR.OK_RN_NKCodeCountry = "AU";
			notifyPartyGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			notifyPartyGCR.OK_CustomsRegNo = "";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be empty", string.Empty, houseBill.NotifyPartyTaxInfo.Number);

			shipment.NotifyParty.CustomsCodes.RemoveAll();
			notifyPartyGCR = shipment.NotifyParty.CustomsCodes.AddNew();
			notifyPartyGCR.OK_RN_NKCodeCountry = "AU";
			notifyPartyGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			notifyPartyGCR.OK_CustomsRegNo = "123456";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be filled in", "123456", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("GCR", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
			var notifyPartyCOM = shipment.NotifyParty.CustomsCodes.AddNew();
			notifyPartyCOM.OK_RN_NKCodeCountry = "AU";
			notifyPartyCOM.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			notifyPartyCOM.OK_CustomsRegNo = "7890";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be filled in", "7890", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("ABN", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
			var notifyPartyEG = Factory.New<OrgHeader>();
			notifyPartyEG.OH_FullName = "MAERSK";
			notifyPartyEG.OH_RL_NKClosestPort = "EGCAI";
			notifyPartyEG.MainAddress.Address1 = "Unit 13";
			notifyPartyEG.MainAddress.Address2 = "4 Lost Lane";
			notifyPartyEG.MainAddress.City = "Sydney";
			notifyPartyEG.MainAddress.Postcode = "2000";
			notifyPartyEG.MainAddress.OA_RN_NKCountryCode = "EG";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPartyEG.MainAddress.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("NotifyParty Tax Number should be empty", string.Empty, houseBill.NotifyPartyTaxInfo.Number);

			notifyPartyGCR = shipment.NotifyParty.CustomsCodes.AddNew();
			notifyPartyGCR.OK_RN_NKCodeCountry = "EG";
			notifyPartyGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			notifyPartyGCR.OK_CustomsRegNo = "";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be empty", string.Empty, houseBill.NotifyPartyTaxInfo.Number);

			shipment.NotifyParty.CustomsCodes.RemoveAll();
			notifyPartyGCR = shipment.NotifyParty.CustomsCodes.AddNew();
			notifyPartyGCR.OK_RN_NKCodeCountry = "EG";
			notifyPartyGCR.OK_CodeType = OrgCusCode.CodeTypes.CorporationCode;
			notifyPartyGCR.OK_CustomsRegNo = "123456";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be filled in", "123456", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("GCR", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
			notifyPartyCOM = shipment.NotifyParty.CustomsCodes.AddNew();
			notifyPartyCOM.OK_RN_NKCodeCountry = "EG";
			notifyPartyCOM.OK_CodeType = OrgCusCode.EgyptCodeTypes.CommercialRegistrationNumber;
			notifyPartyCOM.OK_CustomsRegNo = "7890";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be filled in", "7890", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("CRN", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
			var consingeeVAT = shipment.NotifyParty.CustomsCodes.AddNew();
			consingeeVAT.OK_RN_NKCodeCountry = "EG";
			consingeeVAT.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			consingeeVAT.OK_CustomsRegNo = "AB1234";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("NotifyParty Tax Number should be filled in", "AB1234", houseBill.NotifyPartyTaxInfo.Number);
			AssertEquals("VAT", houseBill.NotifyPartyTaxInfo.DisplayedLabel);
		}

		#endregion

		#region Indonesia

		void PopulateRefDataIndonesia()
		{
			var indonesiaPAS = Factory.New<RefDocOrgCusCode>();
			indonesiaPAS.DOC_DocumentType = "HBL";
			indonesiaPAS.DOC_RN_NKCodeCountry = CountryCodes.Indonesia;
			indonesiaPAS.DOC_RN_NKRegulatingCountry = CountryCodes.Indonesia;
			indonesiaPAS.DOC_CodeType = "PAS";
			indonesiaPAS.DOC_ShortLabel = "KTP";
			indonesiaPAS.DOC_Priority = 1;
			indonesiaPAS.DOC_Description = "Resident ID (Kartu Tanda Penduduk)";

			var indonesiaPPN = Factory.New<RefDocOrgCusCode>();
			indonesiaPPN.DOC_DocumentType = "HBL";
			indonesiaPPN.DOC_RN_NKCodeCountry = CountryCodes.Indonesia;
			indonesiaPPN.DOC_RN_NKRegulatingCountry = CountryCodes.Indonesia;
			indonesiaPPN.DOC_CodeType = "PPN";
			indonesiaPPN.DOC_ShortLabel = "NPWP";
			indonesiaPPN.DOC_Priority = 0;
			indonesiaPPN.DOC_Description = "Nomor Pokok Wajib Pajak";

			Factory.Save();
		}

		public void TestTaxNumberForIndonesia()
		{
			const string warningConsignee = "Consignee's PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017 for Indonesia, when Notify party is not in Indonesia.";
			const string warningConsignee2 = "Consignee's or Notify party's PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017 for Indonesia, when Consignee and Notify party are in Indonesia.";
			const string warningNotifyparty = "Notify party's PPN(NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with  Regulation No. 158/PMK.04/2017 for Indonesia, when Consignee is not in Indonesia.";
			const string warningNotifyparty2 = "Notify party's or Consignee's PPN(NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with  Regulation No. 158/PMK.04/2017 for Indonesia, when Notify party and Consignee are in Indonesia.";

			PopulateRefDataIndonesia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IDTPP";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IDTPP";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER";
			shipper.OH_RL_NKClosestPort = "IDTPP";
			shipper.MainAddress.Address1 = "Unit 15";
			shipper.MainAddress.Address2 = "some lane";
			shipper.MainAddress.City = "Tanjung Priok";
			shipper.MainAddress.Postcode = "19130";
			shipper.MainAddress.OA_RN_NKCountryCode = CountryCodes.Indonesia;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "IDTPP";
			consignee.MainAddress.Address1 = "Central";
			consignee.MainAddress.Address2 = "";
			consignee.MainAddress.City = "Jakarta";
			consignee.MainAddress.Postcode = "10110";
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Indonesia;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFYPARTY";
			notifyParty.OH_RL_NKClosestPort = "IDTPP";
			notifyParty.MainAddress.Address1 = "East";
			notifyParty.MainAddress.Address2 = "";
			notifyParty.MainAddress.City = "Jakarta";
			notifyParty.MainAddress.Postcode = "13110";
			notifyParty.MainAddress.OA_RN_NKCountryCode = CountryCodes.Indonesia;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNullOrEmpty(houseBill.ConsigneeTaxInfo.Number);
			AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);

			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee);
			AssertHasWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee2);

			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty);
			AssertHasWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty2);

			notifyParty.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNullOrEmpty(houseBill.ConsigneeTaxInfo.Number);
			AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);

			AssertHasWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee);
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee2);

			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty);
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty2);

			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			notifyParty.MainAddress.OA_RN_NKCountryCode = CountryCodes.Indonesia;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNullOrEmpty(houseBill.ConsigneeTaxInfo.Number);
			AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);

			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee);
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee2);

			AssertHasWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty);
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty2);

			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			notifyParty.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNullOrEmpty(houseBill.ConsigneeTaxInfo.Number);
			AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);

			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee);
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee2);

			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty);
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty2);

			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Indonesia;
			notifyParty.MainAddress.OA_RN_NKCountryCode = CountryCodes.Indonesia;

			var pasTaxNumConsignee = consignee.CustomsCodes.AddNew();
			pasTaxNumConsignee.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			pasTaxNumConsignee.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			pasTaxNumConsignee.OK_CustomsRegNo = "PAS0002";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("PAS0002", houseBill.ConsigneeTaxInfo.Number);
			AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);

			AssertEquals("KTP", houseBill.ConsigneeTaxInfo.DisplayedLabel);

			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee);
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee2);

			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty);
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty2);

			var pasTaxNumNotifyParty = notifyParty.CustomsCodes.AddNew();
			pasTaxNumNotifyParty.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			pasTaxNumNotifyParty.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			pasTaxNumNotifyParty.OK_CustomsRegNo = "PAS0003";

			var ppnTaxNumNotifyParty = notifyParty.CustomsCodes.AddNew();
			ppnTaxNumNotifyParty.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PPN;
			ppnTaxNumNotifyParty.OK_RN_NKCodeCountry = CountryCodes.Indonesia;
			ppnTaxNumNotifyParty.OK_CustomsRegNo = "PPN0003";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("PAS0002", houseBill.ConsigneeTaxInfo.Number);
			AssertEquals("PPN0003", houseBill.NotifyPartyTaxInfo.Number);

			AssertEquals("KTP", houseBill.ConsigneeTaxInfo.DisplayedLabel);
			AssertEquals("NPWP", houseBill.NotifyPartyTaxInfo.DisplayedLabel);

			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee);
			AssertNoWarning(houseBill.ConsigneeTaxInfo.NumberInfo, warningConsignee2);

			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty);
			AssertNoWarning(houseBill.NotifyPartyTaxInfo.NumberInfo, warningNotifyparty2);
		}

		#endregion

		#endregion

		#region TestDoNotThrowNREWhenConvertingNullAddressPropertyToUppercase

		[ExpectNoExceptions]
		public void TestDoNotThrowNREWhenConvertingNullAddressPropertyToUppercase()
		{
			const string macro = "Organization.CompanyName.ToUpper()";

			var obj = new
			{
				Organization = new Address(Factory)
			}.MakeDynamic();

			var expr = macro.With<StandardLibrary>().CreateExpression();

			expr.Evaluate(obj);

			var nullAddrObj = new
			{
				Organization = (IAddress)null
			}.MakeDynamic();

			expr.Evaluate(nullAddrObj);
		}

		#endregion

		#region TestPopulateConsols

		public void TestPopulateConsols()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZCHC";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CONSOL0001";
			consol1.JK_TransportMode = "ROA";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "CONSOL0002";
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "CONSOL0003";
			consol3.JK_TransportMode = "SEA";
			consol3.JK_RL_NKLoadPort = "NZAKL";
			consol3.JK_RL_NKDischargePort = "NZCHC";

			var houseBill = new HouseBillBuilder(shipment, new DummyDocDataObjectParameters()).Build();

			AssertContainsExactElementsInAnyOrder("Consols have been populated",
				new[]
				{
					"CONSOL0001",
					"CONSOL0002",
					"CONSOL0003"
				}, houseBill.Consols.Select(c => c.ConsolNumber));

			AssertEquals("Departure consol", "CONSOL0002", houseBill.Consols.Departure.ConsolNumber);
			AssertEquals("Arrival consol", "CONSOL0003", houseBill.Consols.Arrival.ConsolNumber);
		}

		#endregion

		#region TestPopulateOrderReferences

		public void TestPopulateOrderReferences()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.DocsAndCartage.JP_OrderItemsAsString = "AAA, BBB, CCC";

			var houseBill = new HouseBillBuilder(shipment, new DummyDocDataObjectParameters()).Build();

			AssertContainsExactElementsInAnyOrder("Order references have been populated",
				new[]
				{
					"AAA",
					"BBB",
					"CCC"
				}, houseBill.OrderReferences);
		}

		#endregion

		#region TestPopulateOrders

		public void TestPopulateOrders()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var order1 = shipment.AttachedOrders.AddNew();
			order1.JD_OrderNumber = "ORD0001";

			var order2 = shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "ORD0002";

			var order3 = shipment.AttachedOrders.AddNew();
			order3.JD_OrderNumber = "ORD0003";

			var houseBill = new HouseBillBuilder(shipment, new DummyDocDataObjectParameters()).Build();

			AssertContainsExactElementsInAnyOrder("Order references have been populated",
				new[]
				{
					"ORD0001",
					"ORD0002",
					"ORD0003"
				}, houseBill.Orders.Select(o => o.OrderNumber));
		}

		#endregion

		#region TestPopulatePackingLines_ShowPackingLinesRegistryItem

		public void TestPopulatePackingLines_ShowPackingLinesRegistryItem_On() => AssertPopulatePackingLines_ShowPackingLinesRegistryItem(true);

		public void TestPopulatePackingLines_ShowPackingLinesRegistryItem_Off() => AssertPopulatePackingLines_ShowPackingLinesRegistryItem(false);

		void AssertPopulatePackingLines_ShowPackingLinesRegistryItem(bool showPackingLinesOnHouseBill)
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				showPackingLinesOnHouseBill))
			{
				var houseBill = (HouseBill)GetNewBusinessObject();

				if (showPackingLinesOnHouseBill)
				{
					AssertEquals("Packing lines are hidden in the wrapper because ShowPackLineDetailsOnHouseBills registry setting is set to true",
						2, houseBill.Containers.SelectMany(c => c.PackingLines).Count());
				}
				else
				{
					AssertEquals("Packing lines are hidden in the wrapper because ShowPackLineDetailsOnHouseBills registry setting is set to false",
						0, houseBill.Containers.SelectMany(c => c.PackingLines).Count());
				}
			}
		}

		#endregion

		#region TestAsAgentOption

		public void TestAsAgentOption()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var houseBill = new HouseBillBuilder(shipment, new DummyDocDataObjectParameters()).Build();

			AssertEquals("AsAgent option exists in House Bill and is default as AS CARRIER", HouseBillLookups.AsAgentOption.AsCarrier, houseBill.AsAgentOption.Code);
		}

		#endregion

		#region TestPopulateLoosePackingLines

		public void TestPopulateLoosePackingLines_ShowPackLineDetailsOnHouseBillsSetToTrue()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var houseBill = (HouseBill)GetNewBusinessObject();

				AssertEquals(nameof(houseBill.LoosePackingLines), 1, houseBill.LoosePackingLines.Count);
			}
		}

		public void TestPopulateLoosePackingLines_ShowPackLineDetailsOnHouseBillsSetToFalse()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				false))
			{
				var houseBill = (HouseBill)GetNewBusinessObject();

				AssertEquals(nameof(houseBill.LoosePackingLines), 1, houseBill.LoosePackingLines.Count);
			}
		}

		#endregion

		#region TestSubShipments

		public void TestAssemblyMasterShipmentWithContainerInSubShipment()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				shipment.JS_HBLAWBChargesDisplay = "ALL";
				shipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

				var subShipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				subShipment.JS_RL_NKOrigin = "AUSYD";
				subShipment.JS_RL_NKDestination = "USCHI";
				subShipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				subShipment.JS_HBLAWBChargesDisplay = "ALL";
				subShipment.JS_JS_ColoadMasterShipment = shipment.PK;

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUSYD";
				departureConsol.JK_RL_NKDischargePort = "USCHI";

				var refContainer = Factory.NewWithValidTestData<RefContainer>();
				refContainer.RC_ISOType = "22P1";
				refContainer.RC_TareWeight = 222;

				var packline1 = Factory.New<ForwardingPackLine>();
				packline1.JL_JS = subShipment.PK;
				packline1.JL_FreightMode = FreightConstants.OuterPackType;
				packline1.JL_PackageCount = 3;
				packline1.JL_ActualWeight = 15;
				packline1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
				packline1.JL_ActualVolume = 2000;
				packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
				packline1.JL_ExportRefNumber = "SLDNO001";
				packline1.JL_Description = "Goods 1";
				packline1.JL_MarksAndNumbers = "Marks 1";
				packline1.JL_F3_NKPackType = "PTL";

				var container1 = departureConsol.Containers.AddNew();
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
				container1.PackLines.Add(packline1);

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertEquals(3, houseBill.TotalPackCount);
			}
		}

		public void TestAssemblyMasterShipmentWithContainerAndMultipleSubShipments()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_INCO = IncoTerms.FreeOnBoard;
				shipment.JS_HBLAWBChargesDisplay = "ALL";
				shipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

				var subShipment1 = CreateSubShipment(shipment, "SUB1");
				var subShipment2 = CreateSubShipment(shipment, "SUB2");
				var subShipment3 = CreateSubShipment(shipment, "SUB3");
				var subShipment4 = CreateSubShipment(shipment, "SUB4");

				var departureConsol = shipment.Consols.AddNew();
				departureConsol.JK_RL_NKLoadPort = "AUSYD";
				departureConsol.JK_RL_NKDischargePort = "USCHI";

				var refContainer = Factory.NewWithValidTestData<RefContainer>();
				refContainer.RC_ISOType = "22P1";
				refContainer.RC_TareWeight = 222;

				var packline1 = CreatePackLine(subShipment1, 5);
				var packline2 = CreatePackLine(subShipment2, 1);
				var packline3 = CreatePackLine(subShipment3, 1);
				var packline4 = CreatePackLine(subShipment4, 2);

				var container1 = departureConsol.Containers.AddNew();
				container1.JC_ContainerNum = "AAAA0000007";
				container1.JC_ContainerMode = ContainerModes.FCL;
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

				container1.PackLines.Add(packline1);
				container1.PackLines.Add(packline2);
				container1.PackLines.Add(packline3);
				container1.PackLines.Add(packline4);

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertEquals(4, houseBill.SubHouseBills.Count);

				var expectedPackCounts = new ZInt[] { 5, 1, 1, 2 };
				var containers = houseBill.SubHouseBills.SelectMany(a => a.Containers).OfType<Container>();

				AssertContainsExactElementsInAnyOrder(
					"Each SubHouseBill should have it's own PackCount based on container packlines",
					expectedPackCounts,
					containers.Select(c => c.PackCount)
				);

				AssertEquals(
					"Each SubHouseBill container should have a unique ID",
					4,
					containers.Select(c => c.Identifier).Distinct().Count()
				);

				AssertEquals(
					"When wrapping the containers in dynamic data, it should still have unique identifiers",
					4,
					containers.Select(c => c.MakeDocDataDynamic().Value.As<Container>().Identifier).Distinct().Count()
				);
			}
		}

		public void TestAssemblyMasterShipmentWithLoosePackLinesInSubShipment()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				shipment.JS_HBLAWBChargesDisplay = "ALL";
				shipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

				var subShipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				subShipment.JS_RL_NKOrigin = "AUSYD";
				subShipment.JS_RL_NKDestination = "USCHI";
				subShipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
				subShipment.JS_HBLAWBChargesDisplay = "ALL";
				subShipment.JS_JS_ColoadMasterShipment = shipment.PK;

				var loosePackLine = subShipment.OuterPackLines.AddNew();
				loosePackLine.JL_JS = subShipment.PK;
				loosePackLine.JL_ActualWeight = 3000;
				loosePackLine.JL_ActualWeightUQ = Weight.Kilograms;
				loosePackLine.JL_ActualVolume = 1.3;
				loosePackLine.JL_ActualVolumeUQ = Volume.CubicMetres;
				loosePackLine.JL_Description = "I'm loose baby!";
				loosePackLine.JL_FreightMode = FreightConstants.OuterPackType;
				loosePackLine.JL_PackageCount = 2;

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertEquals(2, houseBill.TotalLoosePackCount);
			}
		}

		public void TestSubHouseBillsForCoLoad()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;

			var subShipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;

			var loosePackLine = subShipment.OuterPackLines.AddNew();
			loosePackLine.JL_JS = subShipment.PK;
			loosePackLine.JL_Description = "I'm loose baby!";
			loosePackLine.JL_FreightMode = FreightConstants.OuterPackType;
			loosePackLine.JL_PackageCount = 2;

			var loosePackLine2 = subShipment.OuterPackLines.AddNew();
			loosePackLine2.JL_JS = subShipment.PK;
			loosePackLine2.JL_Description = "Another loose baby!";
			loosePackLine2.JL_FreightMode = FreightConstants.OuterPackType;
			loosePackLine2.JL_PackageCount = 2;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals(1, houseBill.SubHouseBills.Count);
			AssertEquals("I'm loose baby!", houseBill.SubHouseBills.Single().LoosePackingLines.First().GoodsDescription);
			AssertEquals("Another loose baby!", houseBill.SubHouseBills.First().LoosePackingLines.Last().GoodsDescription);

			var subShipment2 = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipment2.JS_RL_NKOrigin = "AUSYD";
			subShipment2.JS_RL_NKDestination = "USCHI";
			subShipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals(2, houseBill.SubHouseBills.Count);
		}

		public void TestSubHouseBillsForBuyersConsolLead()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;

			var subShipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;

			var loosePackLine = subShipment.OuterPackLines.AddNew();
			loosePackLine.JL_JS = subShipment.PK;
			loosePackLine.JL_Description = "I'm loose baby!";
			loosePackLine.JL_FreightMode = FreightConstants.OuterPackType;
			loosePackLine.JL_PackageCount = 2;

			var loosePackLine2 = subShipment.OuterPackLines.AddNew();
			loosePackLine2.JL_JS = subShipment.PK;
			loosePackLine2.JL_Description = "Another loose baby!";
			loosePackLine2.JL_FreightMode = FreightConstants.OuterPackType;
			loosePackLine2.JL_PackageCount = 2;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNull(houseBill.SubHouseBills);
		}

		#endregion

		#region TestCharge

		public void TestCharge()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_HBLAWBChargesDisplay = "ALL";

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_FullName = "Ziggy Z";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.MainAddress.Address1 = "Unit 13";
			localClient.MainAddress.Address2 = "4 Lost Lane";
			localClient.MainAddress.City = "Sydney";
			localClient.MainAddress.Postcode = "2000";
			localClient.MainAddress.OA_RN_NKCountryCode = "AU";

			var agentCollect = Factory.New<OrgHeader>();
			agentCollect.OH_FullName = "Airmarine Inc.";
			agentCollect.OH_RL_NKClosestPort = "USCHI";
			agentCollect.MainAddress.Address1 = "5638 S Central Ave";
			agentCollect.MainAddress.City = "Chicago";
			agentCollect.MainAddress.Postcode = "60638";
			agentCollect.MainAddress.OA_RN_NKCountryCode = "US";

			var loader = new JobHeader.Loader(shipment);
			var header = loader.TryLoadOrCreate();

			header.LocalChargesPK = localClient.PK;
			header.AgentCollectPK = agentCollect.PK;

			var exRates = (BusinessObjectCollection)header["ExchangeRates"];

			var usdRate = exRates.AddNew();
			usdRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "USD";
			usdRate[JobExRateSchema.Constants.JF_BaseRate] = 1.2;

			var auRate = exRates.AddNew();
			auRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "AUD";
			auRate[JobExRateSchema.Constants.JF_BaseRate] = 1.1;

			CreateLineCharge(shipment.JobHeader, shipment.JobHeader.LocalChargesPK, 1000m, "FRT", "AUD");

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var charge = houseBill.Charges.FirstOrDefault();

			AssertEquals("FRT", charge.ChargeCode.Code);
		}

		#endregion

		#region TestShipmentTypesForThirdPartyOwnershipHouse

		public void TestShipmentTypesForThirdPartyOwnershipHouse()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHXXXX0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShipmentType = ShipmentTypes.ThirdPartyOwnershipHouse;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.Addresses[0].OA_Address1 = "ConsignorAddress1";
			consignor.Addresses[0].OA_Address2 = "ConsignorAddress2";
			consignor.Addresses[0].OA_RL_NKRelatedPortCode = "DEHAM";
			shipment.ConsignorPK = consignor.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.Addresses[0].OA_Address1 = "ConsigneeAddress1";
			consignee.Addresses[0].OA_Address2 = "ConsigneeAddress2";
			consignee.Addresses[0].OA_RL_NKRelatedPortCode = "NZAKL";
			shipment.ConsigneePK = consignee.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL",
				DataStoreName = "ManufacturerBillOfLading"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertAddressData(shipment.ConsignorDocumentaryAddress, houseBill.Consignee);
			AssertEquals("ThirdParty of PortOfLoading.Code", string.Empty, houseBill.PortOfLoading.Code);
			AssertEquals("ThirdParty of FreightPayableAt.Code", shipment.Destination.Code, houseBill.FreightPayableAt.Code);

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER";
			shipper.Addresses[0].OA_Address1 = "ManufacturerAddress1";
			shipper.Addresses[0].OA_Address2 = "ManufacturerAddress2";
			shipper.Addresses[0].OA_RL_NKRelatedPortCode = "NZAKL";
			shipment.ManufacturerDocAddress.OrganisationPK = shipper.PK;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertAddressData(shipment.ManufacturerDocAddress, houseBill.Shipper);
			AssertEquals("ThirdParty of PortOfLoading.Code", "NZAKL", houseBill.PortOfLoading.Code);
		}

		#endregion

		#region TestShippersLoadAndCount

		public void TestShippersLoadAndCount()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_UniqueConsignRef = "C20201209";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "BEANR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "B20201025";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "H0000056";
			shipment.JS_RL_NKOrigin = "BEANR";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_MarksAndNumbers = "Marks";
			shipment.JS_GoodsDescription = "Goods description";
			shipment.CustomsEntryNumberType = "MRN";
			shipment.CustomsEntryNumber = "MRN111";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 2;
			packingLine1.JL_F3_NKPackType = "PLT";
			packingLine1.JL_ActualWeight = 200;
			packingLine1.JL_ActualWeightUQ = "KG";
			packingLine1.JL_ActualVolume = 300;
			packingLine1.JL_ActualVolumeUQ = "M3";
			packingLine1.JL_DetailedDescription = "pack1";
			packingLine1.JL_ContainerPackingOrder = 1;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TBNU1111111";

			packingLine1.SetContainer(consol, container);
			packingLine1.JL_ExportRefNumber = "MRN_CONT_111";

			container.JC_DeliveryMode = "CY/CY";
			var houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("1 SLC filled in - FCL - CY/CY", ShipperLoadAndCountTypes.Codes.ShipperLoadAndCount, houseBill.ShipperLoadAndCount.Code);

			container.JC_DeliveryMode = "DR/DR";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("2 SLC filled in - FCL - DR/DR", ShipperLoadAndCountTypes.Codes.ShipperLoadAndCount, houseBill.ShipperLoadAndCount.Code);

			container.JC_DeliveryMode = "CFS/CFS";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("3 SLC not filled in - FCL - CFS/CFS", ZString.Empty, houseBill.ShipperLoadAndCount.Code);

			container.JC_ContainerMode = ContainerModes.LCL;
			container.JC_DeliveryMode = "CY/CY";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("4 SLC not filled in - LCL - CY/CY", ZString.Empty, houseBill.ShipperLoadAndCount.Code);

			container.JC_DeliveryMode = "DR/DR";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("5 SLC not filled in - LCL - DR/DR", ZString.Empty, houseBill.ShipperLoadAndCount.Code);

			container.JC_DeliveryMode = "CFS/CFS";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("6 SLC not filled in - LCL - CFS/CFS", ZString.Empty, houseBill.ShipperLoadAndCount.Code);

			container.JC_ContainerMode = ContainerModes.FCL;
			shipment.JS_HBLContainerPackModeOverride = "CY/CY";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("7 SLC filled in - FCL - CY/CY", ShipperLoadAndCountTypes.Codes.ShipperLoadAndCount, houseBill.ShipperLoadAndCount.Code);

			shipment.JS_HBLContainerPackModeOverride = "DOOR/DOOR";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("8 SLC filled in - FCL - DOOR/DOOR", ShipperLoadAndCountTypes.Codes.ShipperLoadAndCount, houseBill.ShipperLoadAndCount.Code);

			shipment.JS_HBLContainerPackModeOverride = "CFS/CFS";
			houseBill = new HouseBillBuilder(shipment, null).Build();
			AssertEquals("9 SLC not filled in - FCL - CFS/CFS", ZString.Empty, houseBill.ShipperLoadAndCount.Code);
		}
		#endregion

		#region TestACIDNumber

		public void TestACIDNumber()
		{
			var expectedACIWarning = "It is recommended to capture the House ACID to comply with the Advance Cargo Information (ACI) for Egypt.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("ACI Number empty", string.Empty, houseBill.ACIDNO);
			AssertHasWarning("ACI Number not filled in warning", houseBill.ACIDNOInfo, expectedACIWarning);

			var aciNumber = shipment.Numbers.AddNew();
			aciNumber.CE_RN_NKCountryCode = CountryCodes.Egypt;
			aciNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			aciNumber.CE_EntryNum = "1111111111111111111";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("ACI Number 1", "ACID No: 1111111111111111111", houseBill.ACIDNO);
			AssertNoWarning("ACI Number 1 filled in no warning", houseBill.ACIDNOInfo, expectedACIWarning);

			aciNumber = shipment.Numbers.AddNew();
			aciNumber.CE_RN_NKCountryCode = CountryCodes.Egypt;
			aciNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			aciNumber.CE_EntryNum = "2222222222222222222";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("ACI Number 2", "ACID No: 1111111111111111111, 2222222222222222222", houseBill.ACIDNO);
			AssertNoWarning("ACI Number 2 filled in no warning", houseBill.ACIDNOInfo, expectedACIWarning);

			shipment.JS_RL_NKDestination = "BEANR";
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals("ACI Number 3", string.Empty, houseBill.ACIDNO);
			AssertNoWarning("ACI Number 3 filled in no warning", houseBill.ACIDNOInfo, expectedACIWarning);
		}

		#endregion

		#region TestExportStatement

		public void TestExportStatement()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BEANR";
			shipment.DocsAndCartage.JP_ExportStatement = "NMD";

			var countrySettings = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			var countrySetting = countrySettings.AddNew();
			countrySetting.CountryCode = CountryCodes.Australia;

			var statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "NMD";
			statementSetting.Statement = "NOT MANDATORY";
			statementSetting.Visibility = "UDF";
			statementSetting.UseOnHouseBillOfLading = true;

			using (FreightDataRegistry.Instance.ExportStatementSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettings))
			{
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertEquals("ExportStatement UDF", "NOT MANDATORY", houseBill.ExportStatement);
			}

			statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "AES";
			statementSetting.Statement = "STATEMENT FOR TESTING \r\n NEW LINE STATEMENT";
			statementSetting.Visibility = "MAN";
			statementSetting.UseOnHouseBillOfLading = true;

			statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "TST";
			statementSetting.Statement = "STATEMENT FOR TESTING \r\n NEW LINE STATEMENT";
			statementSetting.Visibility = "MAN";
			statementSetting.UseOnHouseBillOfLading = true;

			using (FreightDataRegistry.Instance.ExportStatementSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettings))
			{
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertEquals("ExportStatement MAN", "NOT MANDATORY\r\nSTATEMENT FOR TESTING \r\n NEW LINE STATEMENT\r\nSTATEMENT FOR TESTING \r\n NEW LINE STATEMENT", houseBill.ExportStatement);
			}
		}
		#endregion

		#region TestDescriptionNormalisation

		public void TestDescriptionNormalisation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_GoodsDescription = "test string \uDDDD that is invalid";

			var parameters = new DummyDocDataObjectParameters { DocumentTitle = "SCOOBY DOO" };
			new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals(
				"Should report the invalid string",
				ErrorReporter.LastMessageReported,
				$"goodsDescription contains characters that are not valid unicode and so cannot be normalised: {shipment.JS_GoodsDescription}");

			ErrorReporter.Clear();
		}

		#endregion

		#region TestConsignorShipperTerminology

		public void TestConsignorShipperTerminology()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (FreightDataRegistry.Instance.ConsignorShipperTerminology.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();
				AssertEquals("Default value for ConsignorShipperTerminology", FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue, houseBill.ConsignorShipperTerminology);
			}

			using (FreightDataRegistry.Instance.ConsignorShipperTerminology.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TestTest"))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();
				AssertEquals("Value as set in ConsignorShipperTerminology registry setting", "TestTest", houseBill.ConsignorShipperTerminology);
			}
		}

		#endregion

		#region TestContainersAndPackingLinesSequence

		public void TestContainersAndPackingLinesSequence()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBillOfLadingType = FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.DefaultValue.GetCodeDescriptionPairList()[0].Code;

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNCAN";
			consol.JK_BookingReference = "BookingRef";
			consol.JK_CoLoadBookingReference = "CoLoadBookingRef";
			shipment.OuterPackLines.RemoveAll();
			consol.Containers.RemoveAll();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_ContainerMode = ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "AAAA0000008";
			container2.JC_ContainerMode = ContainerModes.FCL;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualWeight = 1000;
			packline1.JL_ContainerPackingOrder = 2;
			packline1.JL_ActualWeightUQ = Weight.Kilograms;
			packline1.JL_ActualVolume = 1.3;
			packline1.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline1.JL_PackLineId = "Packline1";
			container1.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 2000;
			packline2.JL_ContainerPackingOrder = 1;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;
			packline2.JL_ActualVolume = 1.3;
			packline2.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline2.JL_PackLineId = "Packline2";
			container1.PackLines.Add(packline2);

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_ActualWeight = 3000;
			packline3.JL_ContainerPackingOrder = 3;
			packline3.JL_ActualWeightUQ = Weight.Kilograms;
			packline3.JL_ActualVolume = 1.3;
			packline3.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline3.JL_PackLineId = "Packline3";
			container2.PackLines.Add(packline3);

			var packline30 = shipment.OuterPackLines.AddNew();
			packline30.JL_ActualWeight = 3000;
			packline30.JL_ContainerPackingOrder = 3;
			packline30.JL_ActualWeightUQ = Weight.Kilograms;
			packline30.JL_ActualVolume = 1.3;
			packline30.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline30.UNDGs.AddNew();
			packline30.JL_PackLineId = "Packline30";
			container2.PackLines.Add(packline30);

			var packline32 = shipment.OuterPackLines.AddNew();
			packline32.JL_ActualWeight = 3000;
			packline32.JL_ContainerPackingOrder = 3;
			packline32.JL_ActualWeightUQ = Weight.Kilograms;
			packline32.JL_ActualVolume = 1.3;
			packline32.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline32.UNDGs.AddNew();
			packline32.JL_PackLineId = "Packline32";
			container2.PackLines.Add(packline32);

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_ActualWeight = 3000;
			packline4.JL_ContainerPackingOrder = 4;
			packline4.JL_ActualWeightUQ = Weight.Kilograms;
			packline4.JL_ActualVolume = 1.3;
			packline4.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline4.UNDGs.AddNew();
			packline4.JL_PackLineId = "Packline4";
			container2.PackLines.Add(packline4);

			var loosePackLine0 = shipment.OuterPackLines.AddNew();
			loosePackLine0.JL_ActualWeight = 3000;
			loosePackLine0.JL_ContainerPackingOrder = 3;
			loosePackLine0.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine0.JL_ActualVolume = 1.3;
			loosePackLine0.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine0.JL_Description = "I'm loose baby1!";
			loosePackLine0.JL_PackLineId = "LoosePackline0";
			loosePackLine0.Containers.RemoveAll();

			var loosePackLine1 = shipment.OuterPackLines.AddNew();
			loosePackLine1.JL_ActualWeight = 3000;
			loosePackLine1.JL_ContainerPackingOrder = 5;
			loosePackLine1.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine1.JL_ActualVolume = 1.3;
			loosePackLine1.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine1.JL_Description = "I'm loose baby1!";
			loosePackLine1.JL_PackLineId = "LoosePackline1";
			loosePackLine1.UNDGs.AddNew();
			loosePackLine1.Containers.RemoveAll();

			var loosePackLine11 = shipment.OuterPackLines.AddNew();
			loosePackLine11.JL_ActualWeight = 3000;
			loosePackLine11.JL_ContainerPackingOrder = 5;
			loosePackLine11.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine11.JL_ActualVolume = 1.3;
			loosePackLine11.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine11.JL_Description = "I'm loose baby1!";
			loosePackLine11.JL_PackLineId = "LoosePackline11";
			loosePackLine11.Containers.RemoveAll();

			var loosePackLine2 = shipment.OuterPackLines.AddNew();
			loosePackLine2.JL_ActualWeight = 3000;
			loosePackLine2.JL_ContainerPackingOrder = 6;
			loosePackLine2.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine2.JL_ActualVolume = 1.3;
			loosePackLine2.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine2.JL_Description = "I'm loose baby2!";
			loosePackLine2.JL_PackLineId = "LoosePackline2";
			loosePackLine2.UNDGs.AddNew();
			loosePackLine2.Containers.RemoveAll();

			Factory.Save();

			loosePackLine0.JL_ContainerPackingOrder = 3;
			loosePackLine1.JL_ContainerPackingOrder = 5;
			loosePackLine11.JL_ContainerPackingOrder = 5;
			loosePackLine2.JL_ContainerPackingOrder = 6;

			Factory.Save();

			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ContainerAndPackingOrder))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000007, AAAA0000008", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline2, Packline1, Packline3, Packline30, Packline32, Packline4", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("1, 2, 3, 3, 3, 4", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline0, LoosePackline1, LoosePackline11, LoosePackline2", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("3, 5, 5, 6", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ContainerAndPackingOrder))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000007, AAAA0000008", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertNullOrEmpty(string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertNullOrEmpty(string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline0, LoosePackline1, LoosePackline11, LoosePackline2", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("3, 5, 5, 6", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline30, Packline32, Packline4, Packline3, Packline2, Packline1", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("3, 3, 4, 3, 1, 2", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertNullOrEmpty(string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertNullOrEmpty(string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			shipment.JS_HouseBillOfLadingType = string.Empty;
			Factory.Save();

			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline30, Packline32, Packline4, Packline3, Packline2, Packline1", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("3, 3, 4, 3, 1, 2", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			shipment.JS_HouseBillOfLadingType = FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.DefaultValue.GetCodeDescriptionPairList()[0].Code;
			Factory.Save();

			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline30, Packline32, Packline4, Packline3, Packline2, Packline1", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("3, 3, 4, 3, 1, 2", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			var additionalHouseBillOfLadingTypeCollection = new AdditionalHouseBillOfLadingTypeCollection();
			var additionalHouseBillOfLadingType = new AdditionalHouseBillOfLadingType
			{
				Code = "ABC",
				Description = (NoResString)"ABC AdditionalHouseBillOfLadingType"
			};
			additionalHouseBillOfLadingTypeCollection.Add(additionalHouseBillOfLadingType);

			var houseBillOfLadingTypesForSeaCodeList = new SystemDefinableCodeDescriptionBoolCollection
			{
				"DEF"
			};

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, additionalHouseBillOfLadingTypeCollection))
			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, houseBillOfLadingTypesForSeaCodeList))
			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline30, Packline32, Packline4, Packline3, Packline2, Packline1", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("3, 3, 4, 3, 1, 2", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			shipment.JS_HouseBillOfLadingType = "ABC";
			Factory.Save();

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, additionalHouseBillOfLadingTypeCollection))
			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, houseBillOfLadingTypesForSeaCodeList))
			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline30, Packline32, Packline4, Packline3, Packline2, Packline1", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("3, 3, 4, 3, 1, 2", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			shipment.JS_HouseBillOfLadingType = "DEF";
			Factory.Save();

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, additionalHouseBillOfLadingTypeCollection))
			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, houseBillOfLadingTypesForSeaCodeList))
			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ShowDGCargoFirst))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000008, AAAA0000007", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline30, Packline32, Packline4, Packline3, Packline2, Packline1", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("3, 3, 4, 3, 1, 2", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline1, LoosePackline2, LoosePackline0, LoosePackline11", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("5, 6, 3, 5", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, additionalHouseBillOfLadingTypeCollection))
			using (FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, houseBillOfLadingTypesForSeaCodeList))
			using (FreightDataRegistry.Instance.HBLPackLinesDisplayOrder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, HBLPackLinesDisplayOrders.ContainerAndPackingOrder))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var houseBill = new HouseBillBuilder(shipment, null).Build();

				CombineAssertions(() =>
				{
					AssertEquals("AAAA0000007, AAAA0000008", string.Join(", ", houseBill.Containers.Select(container => container.Number)));
					AssertEquals("Packline2, Packline1, Packline3, Packline30, Packline32, Packline4", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingLineID))));
					AssertEquals("1, 2, 3, 3, 3, 4", string.Join(", ", houseBill.Containers.SelectMany(container => container.PackingLines.Select(packingLine => packingLine.PackingOrder))));
					AssertEquals("LoosePackline0, LoosePackline1, LoosePackline11, LoosePackline2", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingLineID)));
					AssertEquals("3, 5, 5, 6", string.Join(", ", houseBill.LoosePackingLines.Select(packingLine => packingLine.PackingOrder)));
				});
			}
		}

		#endregion

		#region TestTariffLineItemReference

		public void TestTariffLineItemReference()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("TLI", string.Empty, houseBill.TariffLineItemReference);

			shipment.JS_RH_NKRateCommodity = "0027";
			shipment.JS_FMCTariffID = "8886";
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "0027";

			Factory.Save();

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("TLI", "0027.8886", houseBill.TariffLineItemReference);

			var map = commodity.RefCommodityCodeMaps.AddNew();
			map.LC_RH_NKCommodityCode = "0027";
			map.LC_LocalCode = "9900270003";
			map.LC_RN_NKCountry = "";
			map.LC_LocalCodeProvider = "RAT";

			Factory.Save();

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("TLI", "9900270003.8886", houseBill.TariffLineItemReference);
		}

		#endregion

		#region TestAddFIATAHBLValidation

		public void TestAddFIATAHBLValidation_DateOfIssueInfo()
		{
			var errorMessage = @"Date of Issue is mandatory as per FIATA requirement.
Please verify in Shipment > Basic Registration > Issue Date.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertEquals(ZDateTime.Empty, houseBill.DateOfIssue);
			AssertHasMessageError(houseBill.DateOfIssueInfo, errorMessage);

			shipment.JS_HouseBillIssueDate = new ZDateTime(2023, 11, 01);

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals(new ZDateTime(2023, 11, 01), houseBill.DateOfIssue);
			AssertNoMessageError(houseBill.DateOfIssueInfo, errorMessage);

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals(ZDateTime.Empty, houseBill.DateOfIssue);
			AssertNoMessageError(houseBill.DateOfIssueInfo, errorMessage);
		}

		public void TestAddFIATAHBLValidation_PlaceOfIssue()
		{
			var portCodeRequiredErrorMessage = "Port Code is required, if Port Name is entered.";
			var portNameMandatoryErrorMessage = @"Place of Issue is mandatory as per FIATA requirement.
Please enter this field or verify in current login Branch > Branch Details > Home Port.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNotNullOrEmpty(houseBill.PlaceOfIssue.Code);
			AssertNotNullOrEmpty(houseBill.PlaceOfIssue.Name);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).CodeInfo, portCodeRequiredErrorMessage);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).NameInfo, portNameMandatoryErrorMessage);

			houseBill.PlaceOfIssue.Code = ZString.Empty;
			houseBill.PlaceOfIssue.Name = "Shanghai";

			AssertNullOrEmpty(houseBill.PlaceOfIssue.Code);
			AssertEquals("Shanghai", houseBill.PlaceOfIssue.Name);
			AssertHasMessageError(((Unloco)houseBill.PlaceOfIssue).CodeInfo, portCodeRequiredErrorMessage);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).NameInfo, portNameMandatoryErrorMessage);

			houseBill.PlaceOfIssue.Code = "CNSHG";
			houseBill.PlaceOfIssue.Name = ZString.Empty;

			AssertEquals("CNSHG", houseBill.PlaceOfIssue.Code);
			AssertNullOrEmpty(houseBill.PlaceOfIssue.Name);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).CodeInfo, portCodeRequiredErrorMessage);
			AssertHasMessageError(((Unloco)houseBill.PlaceOfIssue).NameInfo, portNameMandatoryErrorMessage);

			houseBill.PlaceOfIssue.Code = ZString.Empty;
			houseBill.PlaceOfIssue.Name = ZString.Empty;

			AssertNullOrEmpty(houseBill.PlaceOfIssue.Code);
			AssertNullOrEmpty(houseBill.PlaceOfIssue.Name);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).CodeInfo, portCodeRequiredErrorMessage);
			AssertHasMessageError(((Unloco)houseBill.PlaceOfIssue).NameInfo, portNameMandatoryErrorMessage);

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			houseBill.PlaceOfIssue.Code = ZString.Empty;
			houseBill.PlaceOfIssue.Name = ZString.Empty;

			AssertNullOrEmpty(houseBill.PlaceOfIssue.Code);
			AssertNullOrEmpty(houseBill.PlaceOfIssue.Name);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).CodeInfo, portCodeRequiredErrorMessage);
			AssertNoMessageError(((Unloco)houseBill.PlaceOfIssue).NameInfo, portNameMandatoryErrorMessage);
		}

		public void TestAddFIATAHBLValidation_StandardAddressValidation_Country()
		{
			var mandatoryMessage = "Country is mandatory as per FIATA requirement.";
			var codeIsRequiredMessage = "Country code is required, if Country is entered.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			AssertCountryMandatoryValidation("Shipper");
			AssertCountryMandatoryValidation("Consignee");
			AssertCountryMandatoryValidation("NotifyParty");
			AssertCountryMandatoryValidation("GoodsDelivery");

			AssertCodeRequiredValidation("Shipper");
			AssertCodeRequiredValidation("Consignee");
			AssertCodeRequiredValidation("NotifyParty");
			AssertCodeRequiredValidation("GoodsDelivery");

			void AssertCountryMandatoryValidation(string addressPropertyName)
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var address = (Address)houseBill[addressPropertyName];

				address.CompanyName = ZString.Empty;
				address.Country.Code = ZString.Empty;
				address.Country.Name = ZString.Empty;
				address.AddressLine1 = ZString.Empty;
				address.AddressLine2 = ZString.Empty;
				address.City = ZString.Empty;
				address.State = ZString.Empty;
				address.Postcode = ZString.Empty;

				AssertNullOrEmpty(address.CompanyName);
				AssertNullOrEmpty(address.Country.Code);
				AssertNullOrEmpty(address.Country.Name);
				AssertNullOrEmpty(address.AddressLine1);
				AssertNullOrEmpty(address.AddressLine2);
				AssertNullOrEmpty(address.City);
				AssertNullOrEmpty(address.State);
				AssertNullOrEmpty(address.Postcode);
				AssertNoMessageError(address.AddressFormattedInfo, mandatoryMessage);

				address.CompanyName = "Company Name";
				address.Country.Code = ZString.Empty;
				address.Country.Name = ZString.Empty;
				address.AddressLine1 = ZString.Empty;
				address.AddressLine2 = ZString.Empty;
				address.City = ZString.Empty;
				address.State = ZString.Empty;
				address.Postcode = ZString.Empty;

				AssertEquals("Company Name", address.CompanyName);
				AssertNullOrEmpty(address.Country.Code);
				AssertNullOrEmpty(address.Country.Name);
				AssertNullOrEmpty(address.AddressLine1);
				AssertNullOrEmpty(address.AddressLine2);
				AssertNullOrEmpty(address.City);
				AssertNullOrEmpty(address.State);
				AssertNullOrEmpty(address.Postcode);
				AssertNoMessageError(address.AddressFormattedInfo, mandatoryMessage);

				address.CompanyName = "Company Name";
				address.Country.Code = ZString.Empty;
				address.Country.Name = ZString.Empty;
				address.AddressLine1 = ZString.Empty;
				address.AddressLine2 = ZString.Empty;
				address.City = "Nanjing";
				address.State = ZString.Empty;
				address.Postcode = ZString.Empty;

				AssertEquals("Company Name", address.CompanyName);
				AssertNullOrEmpty(address.Country.Code);
				AssertNullOrEmpty(address.Country.Name);
				AssertNullOrEmpty(address.AddressLine1);
				AssertNullOrEmpty(address.AddressLine2);
				AssertEquals("Nanjing", address.City);
				AssertNullOrEmpty(address.State);
				AssertNullOrEmpty(address.Postcode);
				AssertHasMessageError(address.AddressFormattedInfo, mandatoryMessage);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				address = (Address)houseBill[addressPropertyName];

				address.CompanyName = ZString.Empty;
				address.Country.Code = ZString.Empty;
				address.Country.Name = ZString.Empty;
				address.AddressLine1 = ZString.Empty;
				address.AddressLine2 = ZString.Empty;
				address.City = "Nanjing";
				address.State = ZString.Empty;
				address.Postcode = ZString.Empty;

				AssertNullOrEmpty(address.CompanyName);
				AssertNullOrEmpty(address.Country.Code);
				AssertNullOrEmpty(address.Country.Name);
				AssertNullOrEmpty(address.AddressLine1);
				AssertNullOrEmpty(address.AddressLine2);
				AssertEquals("Nanjing", address.City);
				AssertNullOrEmpty(address.State);
				AssertNullOrEmpty(address.Postcode);
				AssertNoMessageError(address.AddressFormattedInfo, mandatoryMessage);
			}

			void AssertCodeRequiredValidation(string addressPropertyName)
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var address = (Address)houseBill[addressPropertyName];

				address.Country.Code = ZString.Empty;
				address.Country.Name = ZString.Empty;

				AssertNullOrEmpty(address.Country.Code);
				AssertNullOrEmpty(address.Country.Name);
				AssertNoMessageError(address.AddressFormattedInfo, codeIsRequiredMessage);

				address.Country.Code = "DE";
				address.Country.Name = ZString.Empty;

				AssertEquals("DE", address.Country.Code);
				AssertNullOrEmpty(address.Country.Name);
				AssertNoMessageError(address.AddressFormattedInfo, codeIsRequiredMessage);

				address.Country.Code = ZString.Empty;
				address.Country.Name = "China";

				AssertNullOrEmpty(address.Country.Code);
				AssertEquals("China", address.Country.Name);
				AssertHasMessageError(address.AddressFormattedInfo, codeIsRequiredMessage);

				address.Country.Code = "KR";
				address.Country.Name = "Korean";

				AssertEquals("KR", address.Country.Code);
				AssertEquals("Korean", address.Country.Name);
				AssertNoMessageError(address.AddressFormattedInfo, codeIsRequiredMessage);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				address = (Address)houseBill[addressPropertyName];

				address.Country.Code = ZString.Empty;
				address.Country.Name = "China";

				AssertNullOrEmpty(address.Country.Code);
				AssertEquals("China", address.Country.Name);
				AssertNoMessageError(address.AddressFormattedInfo, codeIsRequiredMessage);
			}
		}

		public void TestAddFIATAHBLValidation_StandardAddressValidation_Email()
		{
			var errorMessage = "Please enter a valid email. Email must contain at least 6 characters, at least one dot '.' after '@' with at least one character in between and at least 2 characters after the dot. Email can only contain alphanumeric characters and '_', '-', '@', '.'.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			AssertEmailValidation("Shipper");
			AssertEmailValidation("Consignee");
			AssertEmailValidation("NotifyParty");
			AssertEmailValidation("GoodsDelivery");

			void AssertEmailValidation(string addressPropertyName)
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var address = (Address)houseBill[addressPropertyName];

				address.Email = ZString.Empty;

				AssertNullOrEmpty(address.Email);
				AssertNoMessageError(address.EmailInfo, errorMessage);

				address.Email = "test@test.com";
				AssertEquals("test@test.com", address.Email);
				AssertNoMessageError(address.EmailInfo, errorMessage);

				address.Email = "test";
				AssertEquals("test", address.Email);
				AssertHasMessageError(address.EmailInfo, errorMessage);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				address = (Address)houseBill[addressPropertyName];

				address.Email = "test";
				AssertEquals("test", address.Email);
				AssertNoMessageError(address.EmailInfo, errorMessage);
			}
		}

		public void TestAddFIATAHBLValidation_ShipperAddressValidation()
		{
			var errorMessage = "Shipper party name and address information is required.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var shipper = (Address)houseBill.Shipper;

			shipper.CompanyName = ZString.Empty;
			AssertHasMessageError(shipper.AddressFormattedInfo, errorMessage);

			shipper.CompanyName = "Test";
			shipper.AddressLine1 = "Test";
			shipper.Country.Code = "DE";
			AssertNoMessageError(shipper.AddressFormattedInfo, errorMessage);

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			shipper = (Address)houseBill.Shipper;

			shipper.CompanyName = ZString.Empty;
			AssertNoMessageError(shipper.AddressFormattedInfo, errorMessage);
		}

		#endregion

		#region TestToOrderSupportAndSameAsConsigneeSupport

		public void TestConsigneeAddressSameAsConsigneeValidation_FIATA()
		{
			var errorMessage = "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);

				notifyParty.CompanyName = "Test";
				AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);

				notifyParty.CompanyName = "SAME AS CONSIGNEE";
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				consignee = (Address)houseBill.Consignee;
				notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);

				notifyParty.CompanyName = "Test";
				AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);

				notifyParty.CompanyName = "SAME AS CONSIGNEE";
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);
			}
		}

		public void TestConsigneeAddressSameAsConsigneeValidation_EnableBoleroEHBLIntegration()
		{
			var errorMessage = "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);

				notifyParty.CompanyName = "Test";
				AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);

				notifyParty.CompanyName = "SAME AS CONSIGNEE";
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);
			}
		}

		public void TestConsigneeAddressSameAsConsigneeValidation_EditingElectronicBOL()
		{
			var errorMessage = "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			shipment.IsEditingElectronicBOL = true;
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var consignee = (Address)houseBill.Consignee;
			var notifyParty = (Address)houseBill.NotifyParty;

			consignee.CompanyName = ZString.Empty;
			notifyParty.CompanyName = ZString.Empty;
			AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);

			notifyParty.CompanyName = "Test";
			AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);

			notifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);
		}

		public void TestNotifyPartyAddressToOrderValidation_FIATA()
		{
			var errorMessage = "Notify Party name and address information is required, when Consignee is empty or TO ORDER.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "Test";
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "TO ORDER";
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				consignee = (Address)houseBill.Consignee;
				notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "Test";
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "TO ORDER";
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);
			}
		}

		public void TestNotifyPartyAddressToOrderValidation_EnableBoleroEHBLIntegration()
		{
			var errorMessage = "Notify Party name and address information is required, when Consignee is empty or TO ORDER.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "Test";
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "TO ORDER";
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				consignee = (Address)houseBill.Consignee;
				notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = ZString.Empty;
				notifyParty.CompanyName = ZString.Empty;
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "Test";
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				consignee.CompanyName = "TO ORDER";
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);
			}
		}

		public void TestNotifyPartyAddressToOrderValidation_EditingElectronicBOL()
		{
			var errorMessage = "Notify Party name and address information is required, when Consignee is empty or TO ORDER.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			shipment.IsEditingElectronicBOL = true;
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var consignee = (Address)houseBill.Consignee;
			var notifyParty = (Address)houseBill.NotifyParty;

			consignee.CompanyName = ZString.Empty;
			notifyParty.CompanyName = ZString.Empty;
			AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

			consignee.CompanyName = "Test";
			AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);

			consignee.CompanyName = "TO ORDER";
			AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);
		}

		public void TestConsigneeToOrderSupport()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.OH_RL_NKClosestPort = "INABH";
			consignee.MainAddress.Address1 = "Unit 15";
			consignee.MainAddress.Address2 = "1 E Lane";
			consignee.MainAddress.City = "IndiaCity";
			consignee.MainAddress.Postcode = "050022";
			consignee.MainAddress.OA_RN_NKCountryCode = "IN";
			consignee.MainAddress.State = "ST";
			consignee.MainAddress.OA_Email = "test@test.com";
			consignee.MainAddress.OA_Fax = "12345";
			consignee.MainAddress.OA_Phone = "22222";

			var iecTaxNumConsignee = consignee.CustomsCodes.AddNew();
			iecTaxNumConsignee.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
			iecTaxNumConsignee.OK_RN_NKCodeCountry = CountryCodes.India;
			iecTaxNumConsignee.OK_CustomsRegNo = "IEC0001";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_Contact = "Contact";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("CONSIGNEE", houseBill.Consignee.CompanyName);
				AssertEquals("Contact", houseBill.Consignee.Contact);
				AssertEquals("Unit 15", houseBill.Consignee.AddressLine1);
				AssertEquals("1 E Lane", houseBill.Consignee.AddressLine2);
				AssertEquals("IndiaCity", houseBill.Consignee.City);
				AssertEquals("ST", houseBill.Consignee.State);
				AssertEquals("050022", houseBill.Consignee.Postcode);
				AssertEquals("22222", houseBill.Consignee.Phone);
				AssertEquals("12345", houseBill.Consignee.Fax);
				AssertEquals("test@test.com", houseBill.Consignee.Email);

				houseBill.Consignee.CompanyName = "TO ORDER";

				AssertNullOrEmpty(houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("TO ORDER", houseBill.Consignee.CompanyName);
				AssertNullOrEmpty(houseBill.Consignee.Contact);
				AssertNullOrEmpty(houseBill.Consignee.AddressLine1);
				AssertNullOrEmpty(houseBill.Consignee.AddressLine2);
				AssertNullOrEmpty(houseBill.Consignee.City);
				AssertNullOrEmpty(houseBill.Consignee.State);
				AssertNullOrEmpty(houseBill.Consignee.Postcode);
				AssertNullOrEmpty(houseBill.Consignee.Phone);
				AssertNullOrEmpty(houseBill.Consignee.Fax);
				AssertNullOrEmpty(houseBill.Consignee.Email);

				houseBill.Consignee.CompanyName = "1";

				AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("1", houseBill.Consignee.CompanyName);
				AssertEquals("Contact", houseBill.Consignee.Contact);
				AssertEquals("Unit 15", houseBill.Consignee.AddressLine1);
				AssertEquals("1 E Lane", houseBill.Consignee.AddressLine2);
				AssertEquals("IndiaCity", houseBill.Consignee.City);
				AssertEquals("ST", houseBill.Consignee.State);
				AssertEquals("050022", houseBill.Consignee.Postcode);
				AssertEquals("22222", houseBill.Consignee.Phone);
				AssertEquals("12345", houseBill.Consignee.Fax);
				AssertEquals("test@test.com", houseBill.Consignee.Email);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;

				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("CONSIGNEE", houseBill.Consignee.CompanyName);
				AssertEquals("Contact", houseBill.Consignee.Contact);
				AssertEquals("Unit 15", houseBill.Consignee.AddressLine1);
				AssertEquals("1 E Lane", houseBill.Consignee.AddressLine2);
				AssertEquals("IndiaCity", houseBill.Consignee.City);
				AssertEquals("ST", houseBill.Consignee.State);
				AssertEquals("050022", houseBill.Consignee.Postcode);
				AssertEquals("22222", houseBill.Consignee.Phone);
				AssertEquals("12345", houseBill.Consignee.Fax);
				AssertEquals("test@test.com", houseBill.Consignee.Email);

				houseBill.Consignee.CompanyName = "TO ORDER";

				AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("TO ORDER", houseBill.Consignee.CompanyName);
				AssertEquals("Contact", houseBill.Consignee.Contact);
				AssertEquals("Unit 15", houseBill.Consignee.AddressLine1);
				AssertEquals("1 E Lane", houseBill.Consignee.AddressLine2);
				AssertEquals("IndiaCity", houseBill.Consignee.City);
				AssertEquals("ST", houseBill.Consignee.State);
				AssertEquals("050022", houseBill.Consignee.Postcode);
				AssertEquals("22222", houseBill.Consignee.Phone);
				AssertEquals("12345", houseBill.Consignee.Fax);
				AssertEquals("test@test.com", houseBill.Consignee.Email);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;

				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("CONSIGNEE", houseBill.Consignee.CompanyName);
				AssertEquals("Contact", houseBill.Consignee.Contact);
				AssertEquals("Unit 15", houseBill.Consignee.AddressLine1);
				AssertEquals("1 E Lane", houseBill.Consignee.AddressLine2);
				AssertEquals("IndiaCity", houseBill.Consignee.City);
				AssertEquals("ST", houseBill.Consignee.State);
				AssertEquals("050022", houseBill.Consignee.Postcode);
				AssertEquals("22222", houseBill.Consignee.Phone);
				AssertEquals("12345", houseBill.Consignee.Fax);
				AssertEquals("test@test.com", houseBill.Consignee.Email);

				houseBill.Consignee.CompanyName = "TO ORDER";

				AssertNullOrEmpty(houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("TO ORDER", houseBill.Consignee.CompanyName);
				AssertNullOrEmpty(houseBill.Consignee.Contact);
				AssertNullOrEmpty(houseBill.Consignee.AddressLine1);
				AssertNullOrEmpty(houseBill.Consignee.AddressLine2);
				AssertNullOrEmpty(houseBill.Consignee.City);
				AssertNullOrEmpty(houseBill.Consignee.State);
				AssertNullOrEmpty(houseBill.Consignee.Postcode);
				AssertNullOrEmpty(houseBill.Consignee.Phone);
				AssertNullOrEmpty(houseBill.Consignee.Fax);
				AssertNullOrEmpty(houseBill.Consignee.Email);

				houseBill.Consignee.CompanyName = "1";

				AssertEquals("IEC0001", houseBill.ConsigneeTaxInfo.Number);
				AssertEquals("1", houseBill.Consignee.CompanyName);
				AssertEquals("Contact", houseBill.Consignee.Contact);
				AssertEquals("Unit 15", houseBill.Consignee.AddressLine1);
				AssertEquals("1 E Lane", houseBill.Consignee.AddressLine2);
				AssertEquals("IndiaCity", houseBill.Consignee.City);
				AssertEquals("ST", houseBill.Consignee.State);
				AssertEquals("050022", houseBill.Consignee.Postcode);
				AssertEquals("22222", houseBill.Consignee.Phone);
				AssertEquals("12345", houseBill.Consignee.Fax);
				AssertEquals("test@test.com", houseBill.Consignee.Email);
			}
		}

		public void TestNotifyPartySameAsConsigneeSupport()
		{
			PopulateRefDataIndia();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";
			shipment.JS_RL_NKOrigin = "BRASO";
			shipment.JS_RL_NKDestination = "INABH";

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFYPARTY";
			notifyParty.OH_RL_NKClosestPort = "INABH";
			notifyParty.MainAddress.Address1 = "Unit 15";
			notifyParty.MainAddress.Address2 = "1 E Lane";
			notifyParty.MainAddress.City = "Arauquita";
			notifyParty.MainAddress.Postcode = "050022";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "IN";
			notifyParty.MainAddress.State = "ST";
			notifyParty.MainAddress.OA_Email = "test@test.com";
			notifyParty.MainAddress.OA_Fax = "12345";
			notifyParty.MainAddress.OA_Phone = "22222";

			var iecTaxNumNotifyParty = notifyParty.CustomsCodes.AddNew();
			iecTaxNumNotifyParty.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
			iecTaxNumNotifyParty.OK_RN_NKCodeCountry = CountryCodes.India;
			iecTaxNumNotifyParty.OK_CustomsRegNo = "PAN0001";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_Contact = "Contact";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = false,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("NOTIFYPARTY", houseBill.NotifyParty.CompanyName);
				AssertEquals("Contact", houseBill.NotifyParty.Contact);
				AssertEquals("Unit 15", houseBill.NotifyParty.AddressLine1);
				AssertEquals("1 E Lane", houseBill.NotifyParty.AddressLine2);
				AssertEquals("Arauquita", houseBill.NotifyParty.City);
				AssertEquals("ST", houseBill.NotifyParty.State);
				AssertEquals("050022", houseBill.NotifyParty.Postcode);
				AssertEquals("22222", houseBill.NotifyParty.Phone);
				AssertEquals("12345", houseBill.NotifyParty.Fax);
				AssertEquals("test@test.com", houseBill.NotifyParty.Email);

				houseBill.NotifyParty.CompanyName = "Same As Consignee";

				AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("Same As Consignee", houseBill.NotifyParty.CompanyName);
				AssertNullOrEmpty(houseBill.NotifyParty.Contact);
				AssertNullOrEmpty(houseBill.NotifyParty.AddressLine1);
				AssertNullOrEmpty(houseBill.NotifyParty.AddressLine2);
				AssertNullOrEmpty(houseBill.NotifyParty.City);
				AssertNullOrEmpty(houseBill.NotifyParty.State);
				AssertNullOrEmpty(houseBill.NotifyParty.Postcode);
				AssertNullOrEmpty(houseBill.NotifyParty.Phone);
				AssertNullOrEmpty(houseBill.NotifyParty.Fax);
				AssertNullOrEmpty(houseBill.NotifyParty.Email);

				houseBill.NotifyParty.CompanyName = "1";

				AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("1", houseBill.NotifyParty.CompanyName);
				AssertEquals("Contact", houseBill.NotifyParty.Contact);
				AssertEquals("Unit 15", houseBill.NotifyParty.AddressLine1);
				AssertEquals("1 E Lane", houseBill.NotifyParty.AddressLine2);
				AssertEquals("Arauquita", houseBill.NotifyParty.City);
				AssertEquals("ST", houseBill.NotifyParty.State);
				AssertEquals("050022", houseBill.NotifyParty.Postcode);
				AssertEquals("22222", houseBill.NotifyParty.Phone);
				AssertEquals("12345", houseBill.NotifyParty.Fax);
				AssertEquals("test@test.com", houseBill.NotifyParty.Email);

				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();
				AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("NOTIFYPARTY", houseBill.NotifyParty.CompanyName);
				AssertEquals("Contact", houseBill.NotifyParty.Contact);
				AssertEquals("Unit 15", houseBill.NotifyParty.AddressLine1);
				AssertEquals("1 E Lane", houseBill.NotifyParty.AddressLine2);
				AssertEquals("Arauquita", houseBill.NotifyParty.City);
				AssertEquals("ST", houseBill.NotifyParty.State);
				AssertEquals("050022", houseBill.NotifyParty.Postcode);
				AssertEquals("22222", houseBill.NotifyParty.Phone);
				AssertEquals("12345", houseBill.NotifyParty.Fax);
				AssertEquals("test@test.com", houseBill.NotifyParty.Email);

				houseBill.NotifyParty.CompanyName = "Same As Consignee";
				AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("Same As Consignee", houseBill.NotifyParty.CompanyName);
				AssertEquals("Contact", houseBill.NotifyParty.Contact);
				AssertEquals("Unit 15", houseBill.NotifyParty.AddressLine1);
				AssertEquals("1 E Lane", houseBill.NotifyParty.AddressLine2);
				AssertEquals("Arauquita", houseBill.NotifyParty.City);
				AssertEquals("ST", houseBill.NotifyParty.State);
				AssertEquals("050022", houseBill.NotifyParty.Postcode);
				AssertEquals("22222", houseBill.NotifyParty.Phone);
				AssertEquals("12345", houseBill.NotifyParty.Fax);
				AssertEquals("test@test.com", houseBill.NotifyParty.Email);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("NOTIFYPARTY", houseBill.NotifyParty.CompanyName);
				AssertEquals("Contact", houseBill.NotifyParty.Contact);
				AssertEquals("Unit 15", houseBill.NotifyParty.AddressLine1);
				AssertEquals("1 E Lane", houseBill.NotifyParty.AddressLine2);
				AssertEquals("Arauquita", houseBill.NotifyParty.City);
				AssertEquals("ST", houseBill.NotifyParty.State);
				AssertEquals("050022", houseBill.NotifyParty.Postcode);
				AssertEquals("22222", houseBill.NotifyParty.Phone);
				AssertEquals("12345", houseBill.NotifyParty.Fax);
				AssertEquals("test@test.com", houseBill.NotifyParty.Email);

				houseBill.NotifyParty.CompanyName = "Same As Consignee";

				AssertNullOrEmpty(houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("Same As Consignee", houseBill.NotifyParty.CompanyName);
				AssertNullOrEmpty(houseBill.NotifyParty.Contact);
				AssertNullOrEmpty(houseBill.NotifyParty.AddressLine1);
				AssertNullOrEmpty(houseBill.NotifyParty.AddressLine2);
				AssertNullOrEmpty(houseBill.NotifyParty.City);
				AssertNullOrEmpty(houseBill.NotifyParty.State);
				AssertNullOrEmpty(houseBill.NotifyParty.Postcode);
				AssertNullOrEmpty(houseBill.NotifyParty.Phone);
				AssertNullOrEmpty(houseBill.NotifyParty.Fax);
				AssertNullOrEmpty(houseBill.NotifyParty.Email);

				houseBill.NotifyParty.CompanyName = "1";

				AssertEquals("PAN0001", houseBill.NotifyPartyTaxInfo.Number);
				AssertEquals("1", houseBill.NotifyParty.CompanyName);
				AssertEquals("Contact", houseBill.NotifyParty.Contact);
				AssertEquals("Unit 15", houseBill.NotifyParty.AddressLine1);
				AssertEquals("1 E Lane", houseBill.NotifyParty.AddressLine2);
				AssertEquals("Arauquita", houseBill.NotifyParty.City);
				AssertEquals("ST", houseBill.NotifyParty.State);
				AssertEquals("050022", houseBill.NotifyParty.Postcode);
				AssertEquals("22222", houseBill.NotifyParty.Phone);
				AssertEquals("12345", houseBill.NotifyParty.Fax);
				AssertEquals("test@test.com", houseBill.NotifyParty.Email);
			}
		}

		public void TestAddAddressFormattedValidationDependencies_SimulateReset()
		{
			var addressInformationErrorMessage = "Shipper party name and address information is required.";
			var countryMandatoryMessage = "Country is mandatory as per FIATA requirement.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var shipper = (Address)houseBill.Shipper;

			shipper.CompanyName = ZString.Empty;
			AssertHasMessageError(shipper.AddressFormattedInfo, addressInformationErrorMessage);

			shipper.CompanyName = "Test";
			shipper.AddressLine1 = "Test";
			shipper.Country.Code = "DE";
			AssertEquals("TEST\r\nTEST\r\nGERMANY", shipper.AddressFormatted);
			AssertNoMessageError(shipper.AddressFormattedInfo, addressInformationErrorMessage);
			AssertNoMessageError(shipper.AddressFormattedInfo, countryMandatoryMessage);

			shipper.Country.Code = ZString.Empty;
			AssertEquals("TEST\r\nTEST", shipper.AddressFormatted);
			AssertHasMessageError(shipper.AddressFormattedInfo, countryMandatoryMessage);

			using (shipper.AddressFormattedInfo.SuspendOnValueChanged())
			{
				shipper.AddressFormatted = "TEST\r\nTEST\r\nGERMANY";
			}
			AssertNullOrEmpty(shipper.Country.Code);
			using (shipper.Country.CodeInfo.SuspendOnValueChanged())
			{
				shipper.Country.Code = "DE";
			}
			using (shipper.Country.NameInfo.SuspendOnValueChanged())
			{
				shipper.Country.Name = "GERMANY";
			}
			AssertEquals("TEST\r\nTEST\r\nGERMANY", shipper.AddressFormatted);
			AssertNoMessageError("Simulate the `Reset` operation in the form.", shipper.AddressFormattedInfo, countryMandatoryMessage);

			shipper.AddressLine1 = ZString.Empty;
			AssertEquals("TEST\r\nGERMANY", shipper.AddressFormatted);
			AssertHasMessageError(shipper.AddressFormattedInfo, addressInformationErrorMessage);

			using (shipper.AddressFormattedInfo.SuspendOnValueChanged())
			{
				shipper.AddressFormatted = "TEST\r\nTEST\r\nGERMANY";
			}
			AssertNullOrEmpty(shipper.AddressLine1);
			using (shipper.AddressLine1Info.SuspendOnValueChanged())
			{
				shipper.AddressLine1 = "Test";
			}
			AssertEquals("TEST\r\nTEST\r\nGERMANY", shipper.AddressFormatted);
			AssertNoMessageError("Simulate the `Reset` operation in the form.", shipper.AddressFormattedInfo, addressInformationErrorMessage);
		}

		public void TestAddAddressFormattedValidationDependencies_SimulateReopen()
		{
			var errorMessage = "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				notifyParty.CompanyName = "SAME AS CONSIGNEE";
				consignee.CompanyName = "ABC Company";
				consignee.AddressLine1 = "AddressLine1";
				consignee.Country.Code = ZString.Empty;
				AssertHasMessageError(consignee.AddressFormattedInfo, errorMessage);

				consignee.Country.Code = Core.Constants.CountryCodes.Cambodia;
				AssertNoMessageError(consignee.AddressFormattedInfo, errorMessage);

				using (notifyParty.CompanyNameInfo.SuspendOnValueChanged())
				{
					notifyParty.CompanyName = "SAME AS CONSIGNEE";
				}
				using (consignee.CompanyNameInfo.SuspendOnValueChanged())
				{
					consignee.CompanyName = "ABC Company";
				}
				using (consignee.AddressLine1Info.SuspendOnValueChanged())
				{
					consignee.AddressLine1 = "AddressLine1";
				}
				using (consignee.Country.CodeInfo.SuspendOnValueChanged())
				{
					consignee.Country.Code = ZString.Empty;
				}
				using (consignee.Country.NameInfo.SuspendOnValueChanged())
				{
					consignee.Country.Name = ZString.Empty;
				}
				AssertHasMessageError("Simulate the Reopen form.", consignee.AddressFormattedInfo, errorMessage);
			}

			errorMessage = "Notify Party name and address information is required, when Consignee is empty or TO ORDER.";

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test/",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test/",
				GalileoTestAudience = Guid.NewGuid().ToString(),
			}))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var consignee = (Address)houseBill.Consignee;
				var notifyParty = (Address)houseBill.NotifyParty;

				consignee.CompanyName = "TO ORDER";
				notifyParty.CompanyName = "ABC Company";
				notifyParty.AddressLine1 = "AddressLine1";
				notifyParty.Country.Code = ZString.Empty;
				AssertHasMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				notifyParty.Country.Code = Core.Constants.CountryCodes.Cambodia;
				AssertNoMessageError(notifyParty.AddressFormattedInfo, errorMessage);

				using (consignee.CompanyNameInfo.SuspendOnValueChanged())
				{
					consignee.CompanyName = "TO ORDER";
				}
				using (notifyParty.CompanyNameInfo.SuspendOnValueChanged())
				{
					notifyParty.CompanyName = "ABC Company";
				}
				using (notifyParty.AddressLine1Info.SuspendOnValueChanged())
				{
					notifyParty.AddressLine1 = "AddressLine1";
				}
				using (notifyParty.Country.CodeInfo.SuspendOnValueChanged())
				{
					notifyParty.Country.Code = ZString.Empty;
				}
				using (notifyParty.Country.NameInfo.SuspendOnValueChanged())
				{
					notifyParty.Country.Name = ZString.Empty;
				}
				AssertHasMessageError("Simulate the Reopen form.", notifyParty.AddressFormattedInfo, errorMessage);
			}
		}

		#endregion

		#region TestCTKNumber

		public void TestPopulateCTKNumber()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GNACC";

			var ctkNumber = shipment.Numbers.AddNew();
			ctkNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			ctkNumber.CE_EntryNum = "0001";
			ctkNumber.CE_RN_NKCountryCode = "GN";

			Factory.Save();

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertEquals("0001", houseBill.CTKNumber);
		}

		public void TestValidateCTKNumber()
		{
			var warningMessage = "The CTK – Cargo Tracking Note number is required for shipments destined to Guinea.\r\nEnter the CTK in the Shipment > Additional Details > Reference Numbers.";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GNACC";

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertHasWarning(houseBill.CTKNumberInfo, warningMessage);

			var ctkNumber = shipment.Numbers.AddNew();
			ctkNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			ctkNumber.CE_EntryNum = "0001";
			ctkNumber.CE_RN_NKCountryCode = "GN";

			Factory.Save();

			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			AssertNoWarning(houseBill.CTKNumberInfo, warningMessage);
		}

		#endregion

		#region TestUnitedStatesCustomEntryNumber
		public void TestUnitedStatesCustomEntryNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.CustomsEntryNumber = "T7HRTXGXT";
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.UnitedStates.ITN;

				SetConsolsWithTransport(shipment);

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertEquals("Custom entry number with 'ITN' type in United States should show 'AES' instead in HBL", CusEntryNumberTypes.UnitedStates.AES, houseBill.CustomsEntryNumber.Type.Code);
			}
		}

		#endregion

		#region ChargeFromGlobalJobCosting

		public void TestChargeFromGlobalJobCosting()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");
				var cnOrgProxy = CreateOrg("CNPROORG", "CNProxy", "CNProxy Address", "CN", "CNSHA");
				var deOrgProxy = CreateOrg("DEPROORG", "DEProxy", "DEProxy Address", "DE", "DEHAM");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);
				var cnCompany = CreateCompany("C#3", "CN Test", "B#3", "Brach 3", "CNSHA", "CNY", cnOrgProxy.PK);
				var deCompany = CreateCompany("C#4", "DE Test", "B#4", "Brach 4", "DEHAM", "EUR", deOrgProxy.PK);

				var deCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany));
				FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(frCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new[] { deCountry.PK.ToGuid(), new Guid() });

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				CreateAccChargeCode("DDOC", nzCompany.PK);
				CreateAccChargeCode("521", nzCompany.PK);
				CreateAccChargeCode("CAF", nzCompany.PK);

				Factory.Save();

				ZGuid shipmentPK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();

					var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
					var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
					var nzOrgProxy2 = factory2.Load<OrgHeader>(nzOrgProxy.PK);
					consignor2.CompanyData.OB_IsDebtor = true;
					consignee2.CompanyData.OB_IsDebtor = true;
					nzOrgProxy2.CompanyData.OB_IsDebtor = true;

					var shipment1 = factory2.New<ForwardingShipment>();
					shipmentPK = shipment1.PK;

					shipment1.JS_TransportMode = TransportModes.Sea;
					shipment1.JS_RL_NKOrigin = "FRPAR";
					shipment1.JS_RL_NKDestination = "NZAKL";
					shipment1.JS_INCO = IncoTerms.FreeOnBoard;
					shipment1.JS_HBLAWBChargesDisplay = "ALL";
					shipment1.JS_UniqueConsignRef = "S00005000";

					shipment1.ConsignorPK = consignor2.PK;
					shipment1.ConsigneePK = consignee2.PK;

					var loader = new JobHeader.Loader(shipment1);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = consignor2.PK;
					header.AgentCollectPK = nzOrgProxy2.PK;

					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 15m, "FRT", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", factory2);

					factory2.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory3 = new BusinessObjectFactory();

					var consignor3 = factory3.Load<OrgHeader>(consignor.PK);
					var consignee3 = factory3.Load<OrgHeader>(consignee.PK);
					var frOrgProxy3 = factory3.Load<OrgHeader>(frOrgProxy.PK);
					consignor3.CompanyData.OB_IsDebtor = true;
					consignee3.CompanyData.OB_IsDebtor = true;
					frOrgProxy3.CompanyData.OB_IsDebtor = true;

					var shipment2 = factory3.Load<ForwardingShipment>(shipmentPK);

					var loader2 = new JobHeader.Loader(shipment2);
					var header2 = loader2.TryLoadOrCreateWithMutex();
					header2.LocalChargesPK = consignee3.PK;
					header2.AgentCollectPK = frOrgProxy3.PK;

					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 40m, "DDOC", "NZD", factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.AgentCollectPK, 30m, "CAF", "AUD", factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 20m, "521", "NZD", factory3);

					factory3.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory4 = new BusinessObjectFactory();
					var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment3, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);

					var frtChargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "FRT");
					AssertEquals("International Freight", frtChargeLine.Description);
					AssertEquals(true, frtChargeLine.IsPrepaid);
					AssertEquals(15m, frtChargeLine.Sell.Amount);
					AssertEquals("AUD", frtChargeLine.Sell.Currency.Code);

					var ddochargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "DDOC");
					AssertEquals("DDOC Desc", ddochargeLine.Description);
					AssertEquals(false, ddochargeLine.IsPrepaid);
					AssertEquals(40m, ddochargeLine.Sell.Amount);
					AssertEquals("NZD", ddochargeLine.Sell.Currency.Code);

					var chargeLine521 = houseBill.Charges.First(c => c.ChargeCode.Code == "521");
					AssertEquals("521 Desc", chargeLine521.Description);
					AssertEquals(false, chargeLine521.IsPrepaid);
					AssertEquals(20m, chargeLine521.Sell.Amount);
					AssertEquals("NZD", chargeLine521.Sell.Currency.Code);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory5 = new BusinessObjectFactory();
					var shipment4 = factory5.Load<ForwardingShipment>(shipmentPK);

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment4, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);

					var frtChargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "FRT");
					AssertEquals("International Freight", frtChargeLine.Description);
					AssertEquals(true, frtChargeLine.IsPrepaid);
					AssertEquals(15m, frtChargeLine.Sell.Amount);
					AssertEquals("AUD", frtChargeLine.Sell.Currency.Code);

					var ddochargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "DDOC");
					AssertEquals("DDOC Desc", ddochargeLine.Description);
					AssertEquals(false, ddochargeLine.IsPrepaid);
					AssertEquals(40m, ddochargeLine.Sell.Amount);
					AssertEquals("NZD", ddochargeLine.Sell.Currency.Code);

					var chargeLine521 = houseBill.Charges.First(c => c.ChargeCode.Code == "521");
					AssertEquals("521 Desc", chargeLine521.Description);
					AssertEquals(false, chargeLine521.IsPrepaid);
					AssertEquals(20m, chargeLine521.Sell.Amount);
					AssertEquals("NZD", chargeLine521.Sell.Currency.Code);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory6 = new BusinessObjectFactory();
					var shipment5 = factory6.Load<ForwardingShipment>(shipmentPK);

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment5, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);

					var frtChargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "FRT");
					AssertEquals("International Freight", frtChargeLine.Description);
					AssertEquals(true, frtChargeLine.IsPrepaid);
					AssertEquals(15m, frtChargeLine.Sell.Amount);
					AssertEquals("AUD", frtChargeLine.Sell.Currency.Code);

					var ddochargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "DDOC");
					AssertEquals("DDOC Desc", ddochargeLine.Description);
					AssertEquals(false, ddochargeLine.IsPrepaid);
					AssertEquals(40m, ddochargeLine.Sell.Amount);
					AssertEquals("NZD", ddochargeLine.Sell.Currency.Code);

					var chargeLine521 = houseBill.Charges.First(c => c.ChargeCode.Code == "521");
					AssertEquals("521 Desc", chargeLine521.Description);
					AssertEquals(false, chargeLine521.IsPrepaid);
					AssertEquals(20m, chargeLine521.Sell.Amount);
					AssertEquals("NZD", chargeLine521.Sell.Currency.Code);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory7 = new BusinessObjectFactory();
					var shipment6 = factory7.Load<ForwardingShipment>(shipmentPK);
					shipment6.JS_RL_NKOrigin = "DEHAM";
					factory7.Save();

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment6, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);

					var frtChargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "FRT");
					AssertEquals("International Freight", frtChargeLine.Description);
					AssertEquals(true, frtChargeLine.IsPrepaid);
					AssertEquals(15m, frtChargeLine.Sell.Amount);
					AssertEquals("AUD", frtChargeLine.Sell.Currency.Code);

					var ddochargeLine = houseBill.Charges.First(c => c.ChargeCode.Code == "DDOC");
					AssertEquals("DDOC Desc", ddochargeLine.Description);
					AssertEquals(false, ddochargeLine.IsPrepaid);
					AssertEquals(40m, ddochargeLine.Sell.Amount);
					AssertEquals("NZD", ddochargeLine.Sell.Currency.Code);

					var chargeLine521 = houseBill.Charges.First(c => c.ChargeCode.Code == "521");
					AssertEquals("521 Desc", chargeLine521.Description);
					AssertEquals(false, chargeLine521.IsPrepaid);
					AssertEquals(20m, chargeLine521.Sell.Amount);
					AssertEquals("NZD", chargeLine521.Sell.Currency.Code);
				}
			}
		}

		public void TestChargeFromGlobalJobCosting_WithOverseasAgentBranchProxy()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");
				var cnOrgProxy = CreateOrg("CNPROORG", "CNProxy", "CNProxy Address", "CN", "CNSHA");
				var nzBranchOrgProxy = CreateOrg("NZBRORG", "NZBRProxy", "NZBRProxy Address", "NZ", "NZAKL");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK, nzBranchOrgProxy.PK);
				var cnCompany = CreateCompany("C#3", "CN Test", "B#3", "Brach 3", "CNSHA", "CNY", cnOrgProxy.PK);

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				CreateAccChargeCode("DDOC", nzCompany.PK);
				CreateAccChargeCode("521", nzCompany.PK);
				CreateAccChargeCode("CAF", nzCompany.PK);

				Factory.Save();

				ZGuid shipmentPK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();

					var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
					var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
					var nzBranchOrgProxy2 = factory2.Load<OrgHeader>(nzBranchOrgProxy.PK);
					consignor2.CompanyData.OB_IsDebtor = true;
					consignee2.CompanyData.OB_IsDebtor = true;
					nzBranchOrgProxy2.CompanyData.OB_IsDebtor = true;

					var shipment1 = factory2.New<ForwardingShipment>();
					shipmentPK = shipment1.PK;

					shipment1.JS_TransportMode = TransportModes.Sea;
					shipment1.JS_RL_NKOrigin = "FRPAR";
					shipment1.JS_RL_NKDestination = "NZAKL";
					shipment1.JS_INCO = IncoTerms.FreeOnBoard;
					shipment1.JS_HBLAWBChargesDisplay = "ALL";
					shipment1.JS_UniqueConsignRef = "S00005000";

					shipment1.ConsignorPK = consignor.PK;
					shipment1.ConsigneePK = consignee.PK;

					var loader = new JobHeader.Loader(shipment1);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = consignor.PK;
					header.AgentCollectPK = nzBranchOrgProxy.PK;

					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 15m, "FRT", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", factory2);

					factory2.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory3 = new BusinessObjectFactory();

					var consignor3 = factory3.Load<OrgHeader>(consignor.PK);
					var consignee3 = factory3.Load<OrgHeader>(consignee.PK);
					var frOrgProxy3 = factory3.Load<OrgHeader>(frOrgProxy.PK);
					consignor3.CompanyData.OB_IsDebtor = true;
					consignee3.CompanyData.OB_IsDebtor = true;
					frOrgProxy3.CompanyData.OB_IsDebtor = true;

					var shipment2 = factory3.Load<ForwardingShipment>(shipmentPK);

					var loader2 = new JobHeader.Loader(shipment2);
					var header2 = loader2.TryLoadOrCreateWithMutex();
					header2.LocalChargesPK = consignee.PK;
					header2.AgentCollectPK = frOrgProxy.PK;

					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 40m, "DDOC", "NZD", factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.AgentCollectPK, 30m, "CAF", "AUD", factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 20m, "521", "NZD", factory3);

					factory3.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory4 = new BusinessObjectFactory();
					var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment3, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory5 = new BusinessObjectFactory();
					var shipment4 = factory5.Load<ForwardingShipment>(shipmentPK);

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment4, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory6 = new BusinessObjectFactory();
					var shipment5 = factory6.Load<ForwardingShipment>(shipmentPK);

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment5, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "DDOC", "521" }, chargeCodes);
				}
			}
		}

		public void TestChargeFromGlobalJobCosting_FallbackToOverseasAgentAsCollectCharges()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Sea, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");
				var cnOrgProxy = CreateOrg("CNPROORG", "CNProxy", "CNProxy Address", "CN", "CNSHA");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);
				var cnCompany = CreateCompany("C#3", "CN Test", "B#3", "Brach 3", "CNSHA", "CNY", cnOrgProxy.PK);

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				var thirdParty = CreateOrg("THIRDPARTY", "ThirdParty", "ThirdParty Address", "FR", "FRPAR");
				thirdParty.OH_IsDebtor = true;

				CreateAccChargeCode("FRT", frCompany.PK);
				CreateAccChargeCode("BAF", frCompany.PK);
				CreateAccChargeCode("CAF", frCompany.PK);
				CreateAccChargeCode("DTHC", frCompany.PK);

				Factory.Save();

				ZGuid shipmentPK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();

					var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
					var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
					var thirdParty2 = factory2.Load<OrgHeader>(thirdParty.PK);
					var nzOrgProxy2 = factory2.Load<OrgHeader>(nzOrgProxy.PK);
					consignor2.CompanyData.OB_IsDebtor = true;
					consignee2.CompanyData.OB_IsDebtor = true;
					thirdParty2.CompanyData.OB_IsDebtor = true;
					nzOrgProxy2.CompanyData.OB_IsDebtor = true;

					var shipment1 = factory2.New<ForwardingShipment>();
					shipmentPK = shipment1.PK;

					shipment1.JS_TransportMode = TransportModes.Sea;
					shipment1.JS_RL_NKOrigin = "FRPAR";
					shipment1.JS_RL_NKDestination = "NZAKL";
					shipment1.JS_INCO = IncoTerms.FreeOnBoard;
					shipment1.JS_HBLAWBChargesDisplay = "ALL";
					shipment1.JS_UniqueConsignRef = "S00005000";

					shipment1.ConsignorPK = consignor.PK;
					shipment1.ConsigneePK = consignee.PK;

					var loader = new JobHeader.Loader(shipment1);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = consignor.PK;
					header.AgentCollectPK = nzOrgProxy.PK;

					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 20m, "FRT", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 10m, "BAF", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, thirdParty2.PK, 40m, "CAF", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 30m, "DTHC", "NZD", factory2);

					factory2.Save();

					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment1, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "BAF", "DTHC", "CAF" }, chargeCodes);

					var prepaidChargeCodes = houseBill.Charges.Where(c => c.IsPrepaid).Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "BAF" }, prepaidChargeCodes);

					var collectChargeCodes = houseBill.Charges.Where(c => !c.IsPrepaid).Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "DTHC", "CAF" }, collectChargeCodes);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory3 = new BusinessObjectFactory();
					var shipment2 = factory3.Load<ForwardingShipment>(shipmentPK);
					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment2, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "BAF", "DTHC", "CAF" }, chargeCodes);

					var prepaidChargeCodes = houseBill.Charges.Where(c => c.IsPrepaid).Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "BAF" }, prepaidChargeCodes);

					var collectChargeCodes = houseBill.Charges.Where(c => !c.IsPrepaid).Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "DTHC", "CAF" }, collectChargeCodes);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory4 = new BusinessObjectFactory();
					var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);
					var parameters = new DummyDocDataObjectParameters
					{
						DocumentTitle = "ORIGINAL"
					};

					var houseBill = new HouseBillBuilder(shipment3, parameters).Build();
					var chargeCodes = houseBill.Charges.Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "BAF", "DTHC", "CAF" }, chargeCodes);

					var prepaidChargeCodes = houseBill.Charges.Where(c => c.IsPrepaid).Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "FRT", "BAF" }, prepaidChargeCodes);

					var collectChargeCodes = houseBill.Charges.Where(c => !c.IsPrepaid).Select(c => c.ChargeCode.Code);
					AssertContainsExactElementsInAnyOrder(new[] { "DTHC", "CAF" }, collectChargeCodes);
				}
			}
		}

		AccChargeCode CreateAccChargeCode(string code, ZGuid companyPK)
		{
			var accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = code;
			accChargeCode.AC_Desc = code + " Desc";
			accChargeCode.AC_ChargeGroup = code.Substring(0, AccChargeCodeSchema.AC_ChargeGroup.MaxLength);
			accChargeCode.AC_GC = companyPK;

			accChargeCode.SetGLAccountDataForTesting(accChargeCode.GLAccountForTesting);

			return accChargeCode;
		}

		GlbCompany CreateCompany(string companyCode, string companyName, string branchCode, string branchName, string homePort, string currency, ZGuid orgProxyPK, ZGuid orgBranchProxyPK = default)
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = companyCode;
			newCompany.GC_Name = companyName;
			newCompany.GC_RN_NKCountryCode = homePort.Substring(0, 2);
			newCompany.GC_OH_OrgProxy = orgProxyPK;
			newCompany.GC_RX_NKLocalCurrency = currency;

			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = branchCode;
			newBranch.GB_BranchName = branchName;
			newBranch.GB_RL_NKHomePort = homePort;
			newBranch.GB_OH_OrgProxy = orgBranchProxyPK.IsEmpty ? orgProxyPK : orgBranchProxyPK;

			return newCompany;
		}

		OrgHeader CreateOrg(string code, string fullName, string address1, string countryCode, string closestPort)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.Address1 = address1;
			org.MainAddress.OA_RN_NKCountryCode = countryCode;

			return org;
		}

		#endregion

		#region TestIsElectronicBOL

		public void TestIsElectronicBOL()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GNACC";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			shipment.IsEditingElectronicBOL = true;

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			Assert(!houseBill.IsElectronicBOL);

			houseBill = new HouseBillBuilder(shipment, parameters).Build(isDraft: true);
			Assert(!houseBill.IsElectronicBOL);

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			Assert(houseBill.IsElectronicBOL);

			shipment.IsEditingElectronicBOL = false;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			Assert(!houseBill.IsElectronicBOL);

			shipment.IsEditingElectronicBOL = false;
			houseBill = new HouseBillBuilder(shipment, parameters).Build(isDraft: true);
			Assert(!houseBill.IsElectronicBOL);

			shipment.IsEditingElectronicBOL = true;
			houseBill = new HouseBillBuilder(shipment, parameters).Build(isDraft: true);
			Assert(houseBill.IsElectronicBOL);
		}

		#endregion

		#region TestAddVesselNameAndVoyageValidation

		public void TestAddVesselNameAndVoyageValidation_MissingMainTransport()
		{
			var errorMessage = "Main Sea transport leg is missing in the Routing tab. Please enter it in Consol or Shipment.";

			var transportModes = new List<string> { TransportModes.Sea, TransportModes.SeaAir, TransportModes.AirSea };

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			HouseBill houseBill;

			foreach (var transportMode in transportModes)
			{
				shipment.JS_TransportMode = transportMode;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertHasMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);
			}

			shipment.JS_TransportMode = TransportModes.Air;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNoMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";
			shipment.JS_TransportMode = transportModes.First();
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNoMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);
		}

		public void TestAddVesselNameAndVoyageValidation_MissingMainTransport_EditingElectronicBOL()
		{
			var errorMessage = "Main Sea transport leg is missing in the Routing tab. Please enter it in Consol or Shipment.";

			var transportModes = new List<string> { TransportModes.Sea, TransportModes.SeaAir, TransportModes.AirSea };

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.DataHawkBill;
			shipment.IsEditingElectronicBOL = true;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			HouseBill houseBill;

			foreach (var transportMode in transportModes)
			{
				shipment.JS_TransportMode = transportMode;
				houseBill = new HouseBillBuilder(shipment, parameters).Build();

				AssertHasMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);
			}

			shipment.JS_TransportMode = TransportModes.Air;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNoMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";
			shipment.JS_TransportMode = transportModes.First();
			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNoMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);
		}

		public void TestAddVesselNameAndVoyageValidation_Vessel()
		{
			var errorMessage = "Both Vessel Name and Voyage Number are required.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			shipment.JS_TransportMode = TransportModes.Sea;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "iddqd";
			vessel.RV_Name = "vessel1";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";
			transport.JW_Vessel = "";

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var mainTransport = (Transport)houseBill.Transports.Main;

			AssertNotNull(mainTransport);
			AssertNotNull(mainTransport.Vessel);
			AssertHasMessageError(mainTransport.Vessel.NameInfo, errorMessage);

			shipment.JS_TransportMode = TransportModes.Air;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			mainTransport = (Transport)houseBill.Transports.Main;

			AssertNoMessageError(mainTransport.Vessel.NameInfo, errorMessage);

			transport.JW_Vessel = "vessel1";
			shipment.JS_TransportMode = TransportModes.Sea;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			mainTransport = (Transport)houseBill.Transports.Main;

			AssertNoMessageError(mainTransport.Vessel.NameInfo, errorMessage);
		}

		public void TestAddVesselNameAndVoyageValidation_VoyageNumber()
		{
			var errorMessage = "Both Vessel Name and Voyage Number are required.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			shipment.JS_TransportMode = TransportModes.Sea;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var mainTransport = (Transport)houseBill.Transports.Main;

			AssertNotNull(mainTransport);
			AssertHasMessageError(mainTransport.VoyageFlightNumberInfo, errorMessage);

			shipment.JS_TransportMode = TransportModes.Air;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			mainTransport = (Transport)houseBill.Transports.Main;

			AssertNoMessageError(mainTransport.VoyageFlightNumberInfo, errorMessage);

			transport.JW_VoyageFlight = "voyage1";
			shipment.JS_TransportMode = TransportModes.Sea;
			houseBill = new HouseBillBuilder(shipment, parameters).Build();
			mainTransport = (Transport)houseBill.Transports.Main;

			AssertNoMessageError(mainTransport.VoyageFlightNumberInfo, errorMessage);
		}

		#endregion

		#region TestAddPackingLinesValidation

		public void TestAddPackingLinesValidation()
		{
			var goodsDescriptionErrorMessage = "Goods Description is required.";
			var packageCountErrorMessage = "Package Count is required and cannot be zero.";

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine1.JL_PackageCount = 0;
			container.PackLines.Add(packLine1);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine2.JL_PackageCount = 0;
			container.PackLines.Add(packLine2);

			var loosePackLine = shipment.OuterPackLines.AddNew();
			loosePackLine.JL_ActualWeight = 3000;
			loosePackLine.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine.JL_ActualVolume = 1.3;
			loosePackLine.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine.Containers.RemoveAll();

			var parameters = new DummyDocDataObjectParameters { DocumentTitle = "ORIGINAL" };

			AssertHasMessageErrorWithShowPackLineDetails();
			AssertHasMessageErrorWithShowPackLineDetails(true);

			shipment.IsEditingElectronicBOL = true;

			AssertHasMessageErrorWithShowPackLineDetails();
			AssertHasMessageErrorWithShowPackLineDetails(true);

			packLine1.JL_PackageCount = 1;
			packLine1.JL_Description = "pack line 1";

			packLine2.JL_PackageCount = 2;
			packLine2.JL_Description = "pack line 2";

			loosePackLine.JL_PackageCount = 3;
			loosePackLine.JL_Description = "I'm loose baby!";

			AssertNoMessageErrorWithShowPackLineDetails();
			AssertNoMessageErrorWithShowPackLineDetails(true);

			void AssertHasMessageErrorWithShowPackLineDetails(bool showPackLineDetails = false)
			{
				using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, showPackLineDetails))
				{
					var houseBill = new HouseBillBuilder(shipment, parameters).Build();

					foreach (var itemContainer in houseBill.Containers.OfType<Container>())
					{
						AssertHasMessageError(itemContainer.PackCountInfo, packageCountErrorMessage);
						AssertNotNull(itemContainer.PackingLines);

						foreach (var packingLine in itemContainer.PackingLines.OfType<PackingLine>())
						{
							AssertHasMessageError(packingLine.ShortGoodsDescriptionInfo, goodsDescriptionErrorMessage);
						}
					}

					foreach (var packingLine in houseBill.LoosePackingLines.OfType<PackingLine>())
					{
						AssertHasMessageError(packingLine.QuantityInfo, packageCountErrorMessage);
						AssertHasMessageError(packingLine.ShortGoodsDescriptionInfo, goodsDescriptionErrorMessage);
					}
				}
			}

			void AssertNoMessageErrorWithShowPackLineDetails(bool showPackLineDetails = false)
			{
				using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, showPackLineDetails))
				{
					var houseBill = new HouseBillBuilder(shipment, parameters).Build();

					foreach (var itemContainer in houseBill.Containers.OfType<Container>())
					{
						AssertNoMessageError(itemContainer.PackCountInfo, packageCountErrorMessage);
						AssertNotNull(itemContainer.PackingLines);

						foreach (var packingLine in itemContainer.PackingLines.OfType<PackingLine>())
						{
							AssertNoMessageError("ContainerPackingLines", packingLine.ShortGoodsDescriptionInfo, goodsDescriptionErrorMessage);
						}
					}

					foreach (var packingLine in houseBill.LoosePackingLines.OfType<PackingLine>())
					{
						AssertNoMessageError("LoosePackingLines", packingLine.QuantityInfo, packageCountErrorMessage);
						AssertNoMessageError("LoosePackingLines", packingLine.ShortGoodsDescriptionInfo, goodsDescriptionErrorMessage);
					}
				}
			}
		}

		#endregion

		#region TestAddShipperAddressValidation

		public void TestAddShipperAddressValidation_EnableBoleroEHBLIntegration()
		{
			var errorMessage = "Shipper party name and address information is required.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var boleroEBLConfiguration = new BoleroEBLConfiguration
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			{
				shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
				var houseBill = new HouseBillBuilder(shipment, parameters).Build();
				var shipper = (Address)houseBill.Shipper;
				shipper.CompanyName = ZString.Empty;

				AssertHasMessageError(shipper.AddressFormattedInfo, errorMessage);
			}
		}

		public void TestAddShipperAddressValidation_EditingElectronicBOL()
		{
			var errorMessage = "Shipper party name and address information is required.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			shipment.IsEditingElectronicBOL = true;
			var houseBill = new HouseBillBuilder(shipment, parameters).Build();
			var shipper = (Address)houseBill.Shipper;
			shipper.CompanyName = ZString.Empty;

			AssertHasMessageError(shipper.AddressFormattedInfo, errorMessage);
		}

		#endregion

		#region Implementation

		ForwardingShipment shipment;

		protected override BusinessObject GetNewBusinessObject()
		{
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";

			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var mainTransport = shipment.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var otherTransport = shipment.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "NZAKL";
			otherTransport.JW_RL_NKDiscPort = "NZALR";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = "SEA";
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "CNCAN";
			departureConsol.JK_BookingReference = "BookingRef";
			departureConsol.JK_CoLoadBookingReference = "CoLoadBookingRef";

			var consolTransport = departureConsol.Transports[0];
			consolTransport.JW_Vessel = "Vessel";
			consolTransport.JW_VoyageFlight = "F9999";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_ContainerMode = ContainerModes.FCL;

			var packline1 = shipment.OuterPackLines.Single();
			container.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 2000;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;
			packline2.JL_ActualVolume = 1.3;
			packline2.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline2.JL_JS = shipment.PK;
			container.PackLines.Add(packline2);

			var loosePackLine = shipment.OuterPackLines.AddNew();
			loosePackLine.JL_ActualWeight = 3000;
			loosePackLine.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine.JL_ActualVolume = 1.3;
			loosePackLine.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine.JL_Description = "I'm loose baby!";

			loosePackLine.Containers.RemoveAll();

			Assert("prerequisite: loose packline should be loose", !loosePackLine.Containers.Any());

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_TransportMode = "SEA";
			arrivalConsol.JK_RL_NKLoadPort = "CNCAN";
			arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			sendingForwarderContact.OC_Email = "test1@test.com";
			sendingForwarderContact.OC_Phone = "11111";
			sendingForwarderContact.OC_ContactName = "ContactName1";

			departureConsol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "JIMMY";
			receivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Auckland";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			receivingForwarderContact.OC_Email = "test2@test.com";
			receivingForwarderContact.OC_Phone = "22222";
			receivingForwarderContact.OC_ContactName = "ContactName2";

			departureConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			arrivalConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "MANY";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "TOO MUCH";
			notifyParty3.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3.MainAddress.Address1 = "Unit 686";
			notifyParty3.MainAddress.Address2 = "99 How Lane";
			notifyParty3.MainAddress.City = "Auckland";
			notifyParty3.MainAddress.Postcode = "5038";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			shipment.JS_NoCopyBills = 1;
			shipment.JS_NoOriginalBills = 2;

			var incoTermDefinitions = new IncoTermChargeCodesCollection();
			var def = incoTermDefinitions.AddNew();
			def.Origin = PaymentParty.Consignee;
			def.Loading = PaymentParty.Consignee;
			def.Freight = PaymentParty.Consignee;
			def.Insurance = PaymentParty.Consignee;
			def.Unloading = PaymentParty.Consignee;
			def.Destination = PaymentParty.Consignee;
			def.Brokerage = PaymentParty.Consignee;
			def.CustomsDuty = PaymentParty.Consignee;
			def.OriginBrokerage = PaymentParty.Consignee;
			def.IncoTerm = IncoTerms.CostAndFreight;

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			using (FreightDataRegistry.Instance.BOLClause.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Oh my gut."))
			{
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "COPY"
				};

				return new HouseBillBuilder(shipment, parameters).Build();
			}
		}

		void CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode, BusinessObjectFactory otherFactory = default)
		{
			var factory = otherFactory ?? Factory;

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;
		}

		ForwardingShipment CreateSubShipment(ForwardingShipment shipment, string uniqueConsignmentRef)
		{
			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_RL_NKOrigin = "AUSYD";
			subShipment.JS_RL_NKDestination = "USCHI";
			subShipment.JS_INCO = IncoTerms.FreeOnBoard;
			subShipment.JS_HBLAWBChargesDisplay = "ALL";
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			subShipment.JS_UniqueConsignRef = uniqueConsignmentRef;

			return subShipment;
		}

		ForwardingPackLine CreatePackLine(ForwardingShipment shipment, int packageCount)
		{
			var packline = Factory.New<ForwardingPackLine>();
			packline.JL_JS = shipment.PK;
			packline.JL_FreightMode = FreightConstants.OuterPackType;
			packline.JL_PackageCount = packageCount;

			return packline;
		}

		static PrintChargesBilledToLocalClientAtDestAsCollect CreatePrintChargesBilledToLocalClientAtDestAsCollect(PrintChargesBilledToLocalClientAtDestAsCollectCollection collection, ZString transportMode, ZString exportCountry, ZString importCountry)
		{
			var setting = collection.AddNew();

			setting.TransportMode = transportMode;
			setting.ExportCountry = exportCountry;
			setting.ImportCountry = importCountry;

			return setting;
		}

		#endregion
	}
}
