using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ConsolValueObjectDataAdapterForTest<CommonConsol>))]
	sealed class ConsolValueObjectDataAdapterTest : BaseConsolValueObjectDataAdapterTest<CommonConsol>
	{
		public void TestShipmentsLimit()
		{
			const string expectedError = @"Error: The number of Shipments on a Consol is limited for performance and database management reasons to 2 Shipments. Above 1 Shipments you will receive this message for every additional Shipment added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			var consolValue = new Xsd.Consol();
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL1"));
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL2"));
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL3"));

			var adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			var notifications = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notifications);

			using (FreightDataRegistry.Instance.ShipmentsPerConsolLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.ShipmentsPerConsolLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				adapter.ImportFromValueObject(Factory.New<CommonConsol>(), consolValue, context);

				AssertCollectionContains(expectedError, notifications.GetEventsByType(ErrorType.DataErrorPreventSave).Select(x => x.Message));
			}
		}

		public void TestImportOnlySpecifiedElements()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgHeader org3 = Factory.New<OrgHeader>();
			OrgHeader org4 = Factory.New<OrgHeader>();
			OrgHeader org5 = Factory.New<OrgHeader>();
			OrgHeader org6 = Factory.New<OrgHeader>();
			OrgHeader org7 = Factory.New<OrgHeader>();
			OrgHeader org8 = Factory.New<OrgHeader>();
			OrgHeader org9 = Factory.New<OrgHeader>();
			OrgHeader org10 = Factory.New<OrgHeader>();
			OrgHeader org11 = Factory.New<OrgHeader>();
			OrgHeader org12 = Factory.New<OrgHeader>();

			CommonConsol consol = NewBusinessObject();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INBOM";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;
			consol.JK_OA_CreditorAddress = org3.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = org4.MainAddress.PK;
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			consol.JK_BookingReference = "111222";
			consol.JK_OA_PackDepotAddress = org7.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = org8.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = org9.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = org10.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = org11.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = org12.MainAddress.PK;

			Xsd.Consol consolValue = new Xsd.Consol();
			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureCTOAddressImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.AllowDepartureContainerYardAddressImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(consol, consolValue, context);
			AssertEquals(Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.AgentType.Direct, consol.JK_AgentType);
			AssertEquals(org1, consol.SendingForwarder);
			AssertEquals(org2, consol.ReceivingForwarder);
			AssertEquals(org3, consol.Creditor);
			AssertEquals(org4, consol.ShippingLine);
			AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
			AssertEquals("INBOM", consol.JK_RL_NKDischargePort);
			AssertEquals(Core.Constants.PaymentType.Collect, consol.JK_PrepaidCollect);
			AssertEquals("111222", consol.JK_BookingReference);
			AssertEquals(org7.MainAddress.PK, consol.JK_OA_PackDepotAddress);
			AssertEquals(org8.MainAddress.PK, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(org9.MainAddress.PK, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals(org10.MainAddress.PK, consol.JK_OA_UnpackDepotAddress);
			AssertEquals(org11.MainAddress.PK, consol.JK_OA_ArrivalCTOAddress);
			AssertEquals(org12.MainAddress.PK, consol.JK_OA_ContainerYardEmptyReturnAddress);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			adapter.ImportFromValueObject(consol, consolValue, context);
			AssertEquals(Core.Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.LCL, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.AgentType.Agent, consol.JK_AgentType);
			AssertNull(consol.SendingForwarder);
			AssertNull(consol.ReceivingForwarder);
			AssertNull(consol.Creditor);
			AssertNull(consol.ShippingLine);
			AssertEquals("", consol.JK_RL_NKLoadPort);
			AssertEquals("", consol.JK_RL_NKDischargePort);
			AssertEquals(Core.Constants.PaymentType.Prepaid, consol.JK_PrepaidCollect);
			AssertEquals("", consol.JK_BookingReference);
			AssertEquals(ZGuid.Empty, consol.JK_OA_PackDepotAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_DepartureCTOAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ContainerYardEmptyPickupAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_UnpackDepotAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ArrivalCTOAddress);
			AssertEquals(ZGuid.Empty, consol.JK_OA_ContainerYardEmptyReturnAddress);
		}

		public void TestImportOnlySpecifiedPlannedLegs()
		{
			CommonConsol consol = NewBusinessObject();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INBOM";
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(2);
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(3);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUMEL";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "INBOM";
			consol.Transports.MostInterestingTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_ETD = ZDateTime.Now;
			transport2.JW_ETA = ZDateTime.Now.AddDays(1);
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_TransportType = Constants.TransportPlanningType.PreCarriage;

			Xsd.Consol consolValue = new Xsd.Consol();
			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			adapter.ImportFromValueObject(consol, consolValue, context);
			AssertEquals(1, consol.Transports.Count);

			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(2);
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Now.AddDays(3);
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUMEL";
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "INBOM";
			consol.Transports.MostInterestingTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2 = consol.Transports.AddNew();
			transport2.JW_ETD = ZDateTime.Now;
			transport2.JW_ETA = ZDateTime.Now.AddDays(1);
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			Factory.Save();

			consolValue = new Xsd.Consol();
			adapter.ImportFromValueObject(consol, consolValue, context);
			AssertEquals(2, consol.Transports.Count);
			AssertEquals(false, consol.Transports[0].HasChanges);
			AssertEquals(false, consol.Transports[1].HasChanges);

			SystemDataRegistry.Instance.XmlImportSpecifiedElementsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			consolValue = new Xsd.Consol();
			adapter.ImportFromValueObject(consol, consolValue, context);
			AssertEquals(1, consol.Transports.Count);
			AssertEquals(true, consol.Transports[0].HasChanges);
		}

		#region Reference Numbers

		public void TestImportReferenceNumbers()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
			entryTypeList.Add("BBB", (NoResString)"BBB Test Entry Type").IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			Xsd.Consol xmlConsol = new Xsd.Consol();
			Xsd.ReferenceNumber xsdNumber1 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber1.Type = "AAA";
			xsdNumber1.Number = "11111";
			xsdNumber1.Country.Value = "AW";

			Xsd.ReferenceNumber xsdNumber2 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber2.Type = "AAA";
			xsdNumber2.Number = "22222";
			xsdNumber2.Country.Value = "AW";

			Xsd.ReferenceNumber xsdNumber3 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber3.Type = "AAA";
			xsdNumber3.Number = "33333";
			xsdNumber3.Country.Value = "ZW";

			Xsd.ReferenceNumber xsdNumber4 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber4.Type = "BBB";
			xsdNumber4.Number = "11111";
			xsdNumber4.Country.Value = ZString.Empty;

			Xsd.ReferenceNumber xsdNumber5 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber5.Type = "BBB";
			xsdNumber5.Number = "22222";
			xsdNumber5.Country.Value = "BR";

			Xsd.ReferenceNumber xsdNumber6 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber6.Type = "BBB";
			xsdNumber6.Number = "33333";
			xsdNumber6.Country.Value = "BR";

			CommonConsol consol = Factory.New<CommonConsol>();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			adapter.ImportFromValueObject(consol, xmlConsol, context);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|22222|AW", "AAA|33333|ZW", "BBB|11111|", "BBB|22222|BR", "BBB|33333|BR" },
				consol.Numbers.Cast<CusEntryNumber>().Select((c) => String.Concat(c.CE_EntryType, "|", c.CE_EntryNum, "|", c.CE_RN_NKCountryCode)).ToArray());
		}

		public void TestImportReferenceNumbers_OverrideDuplicates()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("AAA", (NoResString)"AAA Test Entry Type").IsUnique = true;
			entryTypeList.Add("BBB", (NoResString)"BBB Test Entry Type").IsUnique = false;

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			Xsd.Consol xmlConsol = new Xsd.Consol();
			Xsd.ReferenceNumber xsdNumber1 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber1.Type = "AAA";
			xsdNumber1.Number = "11111";
			xsdNumber1.Country.Value = "AW";

			Xsd.ReferenceNumber xsdNumber2 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber2.Type = "BBB";
			xsdNumber2.Number = "11111";
			xsdNumber2.Country.Value = "BO";

			Xsd.ReferenceNumber xsdNumber3 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber3.Type = "BBB";
			xsdNumber3.Number = "22222";
			xsdNumber3.Country.Value = "BR";

			Xsd.ReferenceNumber xsdNumber4 = xmlConsol.ConsolDetail.ReferenceNumbers.AddNew();
			xsdNumber4.Type = "BBB";
			xsdNumber4.Number = "33333";
			xsdNumber4.Country.Value = "BR";

			CommonConsol consol = Factory.New<CommonConsol>();

			CusEntryNumber number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = "AAA";
			number1.CE_EntryNum = "00000";
			number1.CE_RN_NKCountryCode = ZString.Empty;

			CusEntryNumber number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = "BBB";
			number2.CE_EntryNum = "00000";
			number2.CE_RN_NKCountryCode = ZString.Empty;

			CusEntryNumber number3 = consol.Numbers.AddNew();
			number3.CE_EntryType = "BBB";
			number3.CE_EntryNum = "11111";
			number3.CE_RN_NKCountryCode = "BO";

			CusEntryNumber number4 = consol.Numbers.AddNew();
			number4.CE_EntryType = "BBB";
			number4.CE_EntryNum = "33333";
			number4.CE_RN_NKCountryCode = "BR";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			adapter.ImportFromValueObject(consol, xmlConsol, context);

			AssertContainsExactElementsInAnyOrder(new[] { "AAA|11111|AW", "AAA|00000|", "BBB|00000|", "BBB|11111|BO", "BBB|22222|BR", "BBB|33333|BR" },
				consol.Numbers.Cast<CusEntryNumber>().Select((c) => String.Concat(c.CE_EntryType, "|", c.CE_EntryNum, "|", c.CE_RN_NKCountryCode)).ToArray());
		}

		public void TestExportReferenceNumbers()
		{
			CustomsReferenceNumberTypeCollection entryTypeList = new CustomsReferenceNumberTypeCollection();
			entryTypeList.Add("TST", (NoResString)"Test Entry Type");
			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entryTypeList);

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			CusEntryNumber number = consol.Numbers.AddNew();
			number.CE_EntryType = "TST";
			number.CE_EntryNum = "123456";
			number.CE_RN_NKCountryCode = "ZW";

			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();

			Xsd.Consol xsdShipment = adapter.ExportToValueObject(consol, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(1, xsdShipment.ConsolDetail.ReferenceNumbers.Count);
			AssertEquals("TST", xsdShipment.ConsolDetail.ReferenceNumbers[0].Type);
			AssertEquals("123456", xsdShipment.ConsolDetail.ReferenceNumbers[0].Number);
			AssertEquals("ZW", xsdShipment.ConsolDetail.ReferenceNumbers[0].Country.Value);
		}

		#endregion

		#region Customs Values

		public void TestImportCustomValues()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Xsd.Consol consolValue = new Xsd.Consol();

			consolValue.ConsolDetail.CustomValues.Add(new Xsd.CustomValue() { Name = "Date1", Type = "DateTime", Value = "2009-12-09T00:00:00" });
			consolValue.ConsolDetail.CustomValues.Add(new Xsd.CustomValue() { Name = "Str1", Type = "String", Value = "test 1" });

			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals(new ZDateTime(2009, 12, 9), consol.GetUserDefinedValue<ZDateTime>("Date1"));
			AssertEquals("test 1", consol.GetUserDefinedValue<ZString>("Str1"));
		}

		public void TestExportCustomValues()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Xsd.Consol consolValue = new Xsd.Consol();

			consol.SetUserDefinedValue("Date1", new ZDateTime(2009, 12, 9));
			consol.SetUserDefinedValue("Str1", new ZString("test 1"));

			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			adapter.ExportToValueObject(consol, consolValue, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, consolValue.ConsolDetail.CustomValues.Count);
			AssertEquals("Date1", consolValue.ConsolDetail.CustomValues[0].Name);
			AssertEquals("DateTime", consolValue.ConsolDetail.CustomValues[0].Type);
			AssertEquals("2009-12-09T00:00:00", consolValue.ConsolDetail.CustomValues[0].Value);
			AssertEquals("Str1", consolValue.ConsolDetail.CustomValues[1].Name);
			AssertEquals("String", consolValue.ConsolDetail.CustomValues[1].Type);
			AssertEquals("test 1", consolValue.ConsolDetail.CustomValues[1].Value);
		}

		#endregion

		#region Import Declarations

		public void TestImportGenerateDeclarationsFromImportedShipments()
		{
			Xsd.Consol consolValue = new Xsd.Consol();
			consolValue.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;
			consolValue.ConsolDetail.PortOfLoading.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUBNE");
			consolValue.ConsolDetail.PortOfDischarge.Port = Xsd.UNLOCO.FromPortCode(Factory, "SGSIN");

			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL1"));
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL1"));
			consolValue.Shipments.Add(CreateXsdShipment(Xsd.TransportMode.SEA, "EDICUS", "EDICUS", "AUBNE", "SGSIN", "HBL3"));

			var adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			var consol = NewBusinessObject();
			adapter.ImportFromValueObject(consol, consolValue, context);

			AssertEquals(2, consol.Shipments.Count);
			AssertEquals(2, adapter.ShipmentPKsToGenerateDeclarations.Count);
			AssertEquals(consol.Shipments[0].PK, adapter.ShipmentPKsToGenerateDeclarations[0]);
			AssertEquals(consol.Shipments[1].PK, adapter.ShipmentPKsToGenerateDeclarations[1]);
		}

		class ConsolValueObjectDataAdapterForTest<TBusinessObject> : ConsolValueObjectDataAdapter<TBusinessObject, CommonShipment, Xsd.Consol>
			where TBusinessObject : CommonConsol
		{
			public ConsolValueObjectDataAdapterForTest()
			{
			}

			protected override void GenerateDeclarationForShipment(Xsd.Shipment shipmentValue, CommonShipment shipment, IValueObjectImportContext context)
			{
				base.GenerateDeclarationForShipment(shipmentValue, shipment, context);
				ShipmentPKsToGenerateDeclarations.Add(shipment.PK);
			}
			public List<ZGuid> ShipmentPKsToGenerateDeclarations = new List<ZGuid>();
		}

		#endregion
	}
}
