using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.Matching.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalOrder = Enterprise.UniversalDataBuss.DataObjects.Universal.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public sealed class ShipmentDataObjectReaderTest : ShipmentDataObjectReadingHelperTest
	{
		public void TestCompanyTariffLevelOverride()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.CompanyTariffLevelOverride = 2;
			var shipmentBO = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

			AssertEquals("CompanyTariffLevelOverride should be imported", (ZByte)2, shipmentBO.JS_CompanyTariffLevelOverride);
		}

		public void TestHAWBHandlingInformationExtraText()
		{
			using (FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"\"{First}\"\" == \"\"1\"\").First()>"))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				var shipmentDataObject = SetupShipment();
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = TransportModes.Air };
				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				AssertNoExceptionThrown("Should not throw ExportAWBHeaderReplaceMacrosException", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestShipmentWithPackinglinesUpdatingContainerGrossWeight()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_UniqueConsignRef = "S2204146956";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentType = AgentType.Agent;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "MYKUL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_AgentType = AgentType.Agent;
			consol2.JK_RL_NKLoadPort = "MYKUL";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			var containerType = new RefContainer.Loader(Factory.BOFactory).LoadFromCode("LD-1");
			AssertNotNull("Found LD-1 container type", containerType);

			var container1 = consol1.Containers.AddNew();
			container1.JC_RC = containerType.PK;
			container1.JC_ContainerMode = ContainerModes.ULD;

			var container2 = consol2.Containers.AddNew();
			container2.JC_RC = containerType.PK;
			container2.JC_ContainerMode = ContainerModes.ULD;

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_PackageCount = 1;
			packingLine.JL_ActualWeight = 100;
			packingLine.JL_ActualWeightUQ = Weight.Kilograms;
			packingLine.JL_ActualVolume = 0.3;
			packingLine.JL_ActualVolumeUQ = Volume.CubicMetres;

			packingLine.Containers.AddRange(container1, container2);

			Factory.SaveForTesting();

			AssertEquals("shipment has one packline", shipment.OuterPackLines.Count, 1);
			AssertContainsExactElementsInAnyOrder("shipment is attached to 2 consols", shipment.Consols, new[] { consol1, consol2 });
			AssertContainsExactElementsInAnyOrder("consol 1 has one container", consol1.Containers, new[] { container1 });
			AssertContainsExactElementsInAnyOrder("consol 2 has one container", consol2.Containers, new[] { container2 });

			AssertArrayEqualsByElements("container 1 contains shipment packing line ",
				container1.PackLines.ToArray(), new[] { packingLine });

			AssertEquals("container 1 JC_GrossWeight (before UXml import) is the sum of tare weight and all packing lines weight",
				container1.JC_GrossWeight, container1.JC_TareWeight + packingLine.JL_ActualWeight);

			AssertArrayEqualsByElements("container 2 contains shipment packing line ",
				container2.PackLines.ToArray(), new[] { packingLine });

			AssertEquals("container 2 JC_GrossWeight (before UXml import) is the sum of tare weight and all packing lines weight",
				container2.JC_GrossWeight, container2.JC_TareWeight + packingLine.JL_ActualWeight);

			var container1GrossWeight = container1.JC_GrossWeight;
			var container2GrossWeight = container1.JC_GrossWeight;

			using (FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var res = CreateAndProcessUniversalShipment("UniversalShipmentWithPackingLines.xml");

				AssertMultilineASCIIEquals("import log",
@"Information|Updated Shipment S2204146956 from UniversalShipment.
Information|Successfully saved Shipment S2204146956 with 2 x ForwardingShipmentStmNote, 2 x ForwardingPackLine, 1 x CusEntryNumber.",
					string.Join("\r\n", res.Logs.Select(l => $"{l.Type}|{l.Message}")));
			}

			var factory = new BusinessObjectFactory();
			var shipmentAfterImport = factory.Load<ForwardingShipment>(shipment.PK);

			AssertEquals("shipment has 3 packinglines; 1 existing and 2 from UXml", shipmentAfterImport.OuterPackLines.Count, 3);

			var packinglineExistingBeforeImport = factory.Load<ForwardingPackLine>(packingLine.PK);

			var container1AfterImport = factory.Load<ForwardingContainer>(container1.PK);
			var container2AfterImport = factory.Load<ForwardingContainer>(container2.PK);

			AssertContainsExactElementsInAnyOrder("container 1 packinglines (after UXml import)",
				container1AfterImport.PackLines.ToArray(), new[] { packinglineExistingBeforeImport });

			AssertContainsExactElementsInAnyOrder("container 2 packinglines (after UXml import)",
				container2AfterImport.PackLines.ToArray(), new[] { packinglineExistingBeforeImport });

			AssertEquals("container 1 JC_GrossWeight was not updated during UXml import",
				container1AfterImport.JC_GrossWeight, container1GrossWeight);

			AssertEquals("container 2 JC_GrossWeight was not updated during UXml import",
				container2AfterImport.JC_GrossWeight, container2GrossWeight);
		}

		public void TestBookingConfirmationReferenceImportedToAdditionalReferences()
		{
			var existingShipment = Factory.New<ForwardingShipment>();
			existingShipment.JS_HouseBill = "HBL123";
			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "TR00001");
			shipmentDataObject.DataContext.SetDocumentaryOverride("Booking Request", MessagePurposes.Codes.Original, null, true, 1, 1);
			shipmentDataObject.WayBillNumber = "HBL123";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

			var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals("No BKG additional reference", null, shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG));

			shipmentDataObject.BookingConfirmationReference = "BCR555";
			shipmentDataObject.AdditionalReferenceCollection?.Clear();
			shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals("Booking confirmation reference filled in when NO co load booking confirmation reference is present", "BCR555", shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG)?.CE_EntryNum);

			shipmentDataObject.CoLoadBookingConfirmationReference = "COBCR555";
			shipmentDataObject.AdditionalReferenceCollection?.Clear();
			shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals("Co load booking confirmation filled in when ALSO booking confirmation is present", "COBCR555", shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG)?.CE_EntryNum);

			shipmentDataObject.BookingConfirmationReference = string.Empty;
			shipmentDataObject.AdditionalReferenceCollection?.Clear();
			shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals("Co load booking confirmation filled in when NO booking confirmation is present", "COBCR555", shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG)?.CE_EntryNum);
		}

		public void TestDefaultContainerModeForTransitDispatch()
		{
			using (GlbDepartment.CurrentDepartment.SetTempContext(TransportMode.Sea))
			{
				var shipmentData = SetupShipment(GetResourcePathFor("TransitDispatch.xml"));
				shipmentData.TransportMode = new CodeDescriptionPair { Code = "SEA" };
				var resultShipment = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(true, resultShipment.Containers.Any());
				AssertEquals(true, resultShipment.Containers.All(container => container.JC_ContainerMode == "LCL"));
			}

			using (GlbDepartment.CurrentDepartment.SetTempContext(TransportMode.Air))
			{
				var shipmentData = SetupShipment(GetResourcePathFor("TransitDispatch.xml"));
				shipmentData.TransportMode = new CodeDescriptionPair { Code = "AIR" };
				var resultShipment = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(true, resultShipment.Containers.Any());
				AssertEquals(true, resultShipment.Containers.All(container => container.JC_ContainerMode == "ULD"));
			}

			using (GlbDepartment.CurrentDepartment.SetTempContext(TransportMode.Sea))
			{
				var shipmentData = SetupShipment(GetResourcePathFor("TransitDispatch.xml"));
				shipmentData.DataContext.DataSourceCollection.First().Type = "ForwardingConsol";
				var resultShipment = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(true, resultShipment.Containers.Any());
				AssertEquals(true, resultShipment.Containers.All(container => container.JC_ContainerMode.IsEmpty));
			}

			using (GlbDepartment.CurrentDepartment.SetTempContext(TransportMode.Air))
			{
				var shipmentData = SetupShipment(GetResourcePathFor("TransitDispatch.xml"));
				shipmentData.TransportMode = new CodeDescriptionPair { Code = "AIR" };
				var resultShipment = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(true, resultShipment.Containers.Any());
				AssertEquals(true, resultShipment.Containers.All(container => container.JC_ContainerMode == "ULD"));
				resultShipment.Consols[0].JK_TransportMode = "AIR";
				resultShipment.Consols[0].Containers.RemoveAll();
				Factory.SaveForTesting();

				shipmentData = SetupShipment(GetResourcePathFor("TransitDispatch.xml"));
				shipmentData.AdditionalReferenceCollection.Where(x => (x.Type.Code ?? ZString.Empty) == "FSH" || (x.Type.Code ?? ZString.Empty) == "HSB").ForEach(r => r.ReferenceNumber = resultShipment.JS_UniqueConsignRef);
				resultShipment = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(true, resultShipment.Containers.Any());
				AssertEquals(true, resultShipment.Containers.All(container => container.JC_ContainerMode == "ULD"));
			}
		}

		public void TestShipmentStatusForUniversalXML()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			var shipmentBO = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals(ShipmentStatusList.Codes.Confirmed, shipmentBO.JS_ShipmentStatus);

			shipmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.SIRejected };
			shipmentBO = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals(ShipmentStatusList.Codes.SIRejected, shipmentBO.JS_ShipmentStatus);

			shipmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.ElectronicShippingInstruction };
			shipmentBO = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			AssertEquals(ShipmentStatusList.Codes.ElectronicShippingInstruction, shipmentBO.JS_ShipmentStatus);
		}

		public void TestChangingPortOfOriginDoesntChangeConsignorAddress()
		{
			var bmw = Factory.New<OrgHeader>();
			bmw.OH_Code = "BMWAGWMUC";
			bmw.OH_IsConsignor = true;

			var officeAddress = bmw.Addresses.AddNew(OrgAddressType.Office, true);
			officeAddress.OA_Code = "Wrong Address";
			officeAddress.OA_CompanyNameOverride = "BMW AG";
			officeAddress.Address1 = "PETUELRING 130";
			officeAddress.City = "MUNCHEN";
			officeAddress.Postcode = "80788";
			officeAddress.OA_State = "BY";
			officeAddress.SetBaseOA_RL_NKRelatedPortCode("DEMUC");

			var pickupAddress = bmw.Addresses.AddNew(OrgAddressType.Pickup, false);
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.AWB.Code);
			pickupAddress.AddressCapability.SetIsMainAddress(OrgAddressType.AWB.Code);
			pickupAddress.OA_Code = "Correct Address";
			pickupAddress.OA_CompanyNameOverride = "BMW AG Werk Leipzig";
			pickupAddress.Address1 = "Schenker Deutschland AG";
			pickupAddress.Address2 = "Handelsring 10-12";
			pickupAddress.City = "Leipzig";
			pickupAddress.Postcode = "04356";
			pickupAddress.OA_State = "SN";
			pickupAddress.SetBaseOA_RL_NKRelatedPortCode("DELEJ");

			var match = Factory.New<OrgPatternMatch>();
			match.OS_OH = bmw.PK;
			match.EncodeOrganisation(bmw, new StringWithLanguage(bmw.OH_FullName, bmw.OH_Language), officeAddress, "");

			var match2 = Factory.New<OrgPatternMatch>();
			match2.OS_OH = bmw.PK;
			match2.EncodeOrganisation(bmw, new StringWithLanguage(bmw.OH_FullName, bmw.OH_Language), pickupAddress, "");

			Factory.SaveForTesting();

			var shipmentData = SetupShipment(GetResourcePathFor("PortOfOriginDoesntChangeShipmentConsignorAddress.xml"));
			var reader = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null);
			var resultShipment = reader.ReadIntoBusinessObject();
			Assert("Should not override the address", !resultShipment.ConsignorDocumentaryAddress.E2_AddressOverride);
			Assert("Should match to pickup address", resultShipment.ConsignorDocumentaryAddress.IsMatchedAgainst(pickupAddress));
			Assert("Changing port of origin shouldn't change the consignor documentary address when importing from xml",
				resultShipment.ConsignorDocumentaryAddress.Address1 == "Schenker Deutschland AG" &&
				resultShipment.ConsignorDocumentaryAddress.Address2 == "Handelsring 10-12" &&
				resultShipment.ConsignorDocumentaryAddress.City == "Leipzig" &&
				resultShipment.ConsignorDocumentaryAddress.E2_CompanyName == "BMW AG Werk Leipzig" &&
				resultShipment.ConsignorDocumentaryAddress.Postcode == "04356");
		}

		public void TestChangingPortOfDestinationDoesntChangeConsigneeAddress()
		{
			var bmw = Factory.New<OrgHeader>();
			bmw.OH_Code = "BMWAGWMUC";
			bmw.OH_IsConsignee = true;

			var officeAddress = bmw.Addresses.AddNew(OrgAddressType.Office, true);
			officeAddress.OA_Code = "Wrong Address";
			officeAddress.OA_CompanyNameOverride = "BMW AG";
			officeAddress.Address1 = "PETUELRING 130";
			officeAddress.City = "MUNCHEN";
			officeAddress.Postcode = "80788";
			officeAddress.OA_State = "BY";
			officeAddress.SetBaseOA_RL_NKRelatedPortCode("DEMUC");

			var deliveryAddress = bmw.Addresses.AddNew(OrgAddressType.Delivery, false);
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.AWB.Code);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.AWB.Code);
			deliveryAddress.OA_Code = "Correct Address";
			deliveryAddress.OA_CompanyNameOverride = "BMW AG Werk Leipzig";
			deliveryAddress.Address1 = "Schenker Deutschland AG";
			deliveryAddress.Address2 = "Handelsring 10-12";
			deliveryAddress.City = "Leipzig";
			deliveryAddress.Postcode = "04356";
			deliveryAddress.OA_State = "SN";
			deliveryAddress.SetBaseOA_RL_NKRelatedPortCode("DELEJ");

			var match = Factory.New<OrgPatternMatch>();
			match.OS_OH = bmw.PK;
			match.EncodeOrganisation(bmw, new StringWithLanguage(bmw.OH_FullName, bmw.OH_Language), officeAddress, "");

			var match2 = Factory.New<OrgPatternMatch>();
			match2.OS_OH = bmw.PK;
			match2.EncodeOrganisation(bmw, new StringWithLanguage(bmw.OH_FullName, bmw.OH_Language), deliveryAddress, "");

			Factory.SaveForTesting();

			var shipmentData = SetupShipment(GetResourcePathFor("PortOfDestinationDoesntChangeShipmentConsigneeAddress.xml"));
			var reader = new ShipmentDataObjectReader(shipmentData, Logger, Factory, null);
			var resultShipment = reader.ReadIntoBusinessObject();
			Assert("Should not override the address", !resultShipment.ConsigneeDocumentaryAddress.E2_AddressOverride);
			Assert("Should match to delivery address", resultShipment.ConsigneeDocumentaryAddress.IsMatchedAgainst(deliveryAddress));
			Assert("Changing port of destination shouldn't change the consignee documentary address when importing from xml",
				resultShipment.ConsigneeDocumentaryAddress.Address1 == "Schenker Deutschland AG" &&
				resultShipment.ConsigneeDocumentaryAddress.Address2 == "Handelsring 10-12" &&
				resultShipment.ConsigneeDocumentaryAddress.City == "Leipzig" &&
				resultShipment.ConsigneeDocumentaryAddress.E2_CompanyName == "BMW AG Werk Leipzig" &&
				resultShipment.ConsigneeDocumentaryAddress.Postcode == "04356");
		}

		[ExpectNoExceptions]
		public void TestCheckConsolContainersGrossWeightAfterAttachToConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002016";

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quotedBooking = quotedBookingBuilder.CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipment = (ForwardingShipment)quotedBooking.ForwardingShipment;
			shipment.FillWithValidTestData();
			shipment.JS_PackingMode = "FCL";
			shipment.JS_UniqueConsignRef = "S00002016";
			shipment.JS_IsForwardRegistered = true;

			var qbPackLine = shipment.OuterPackLines.AddNew();
			qbPackLine.FillWithValidTestData();
			qbPackLine.JL_ActualWeight = 900000d;
			qbPackLine.JL_ActualWeightUQ = "KT";

			Factory.SaveForTesting();
			CreateAndProcessUniversalShipment("UniversalShipmentWithForwardingShipmentButQuotedBookingInDB.xml");
		}

		[ExpectNoExceptions]
		public void TestGetExistingBusinessObject_SpecifiedMethodIsSupported()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.CarrierDocumentsOverride = new CarrierDocumentsOverride();
			shipmentDataObject.CarrierDocumentsOverride.AWBHeader = new AWBHeader();
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

			reader.ReadIntoBusinessObject();
		}

		public void TestConsolShipmentsLimitExceeded()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.Shipments.AddNew();
			consolBO.Shipments.AddNew();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));

			using (FreightDataRegistry.Instance.ShipmentsPerConsolLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.ShipmentsPerConsolLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestShipmentOrdersLimitExceeded()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order1"));
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order2"));
			shipmentDataObject.RelatedShipmentCollection.Add(GetNewOrderDataObject("Order3"));
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

			using (FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestLinkWarehouseOrder()
		{
			var whsOrder = (BusinessObject)Factory.BOFactory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			whsOrder["WD_BOLNo"] = "BACON PANCAKES";
			Factory.SaveForTesting();

			var docketID = ((IWhsOrder)whsOrder).WD_DocketID;
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseReceive, docketID);
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

			var orderDataObject = new UniversalOrder();
			orderDataObject.OrderNumber = "SALAMI ICE-CREAM";
			shipmentDataObject.Order = orderDataObject;

			Logger.TopLevelDataObject = shipmentDataObject;
			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			var shipment1 = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("House bill should be correct.", "BACON PANCAKES", shipment1.JS_HouseBill);
			AssertEquals("Should not have attached any Warehouse Orders as there is no Warehouse Order Data Source.", 0, shipment1.AttachedWarehouseOrders.Count);

			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseOrder, "RANDOMID");
			var shipment2 = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should have updated same Shipment.", shipment1, shipment2);
			AssertEquals("Should not have attached any Warehouse Orders as the Warehouse Order Data Source has the wrong ID.", 0, shipment2.AttachedWarehouseOrders.Count);

			shipmentDataObject.DataContext.GetMatchingDataSource(DataContextType.WarehouseOrder).Key = docketID;
			Logger.OutboundSessionTracker = null;
			var shipment3 = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should have updated same Shipment.", shipment1, shipment3);
			AssertEquals("Should not have attached any Warehouse Orders as the Import is not Internal.", 0, shipment3.AttachedWarehouseOrders.Count);
			AssertEquals("Order item should be imported", "SALAMI ICE-CREAM", shipment3.DocsAndCartage.JP_OrderItemsAsString);

			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			var shipment4 = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("Should have updated same Shipment.", shipment1, shipment4);
			AssertContainsExactElementsInAnyOrder("Should have attached the Correct Warehouse Order.", new[] { whsOrder }, shipment4.AttachedWarehouseOrders);
			AssertEquals("Should not have imported warehouse order into Order Items", ZString.Empty, shipment4.DocsAndCartage.JP_OrderItemsAsString);
		}

		public void TestRegroupTWPackagesByCommonAttributes_PropertiesMerging()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackQty = 1;
				packline1.Weight = 3;
				packline1.Volume = 2;
				packline1.LoadingMeters = 4;
				packline1.UNDGCollection.First().Weight = 10;
				packline1.UNDGCollection.First().Volume = 20;
				packline1.UNDGCollection.First().PackQty = 10;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackQty = 1;
				packline2.Weight = 4;
				packline2.Volume = 1;
				packline2.LoadingMeters = 10;
				packline2.UNDGCollection.First().Weight = 11;
				packline2.UNDGCollection.First().Volume = 4;
				packline2.UNDGCollection.First().PackQty = 5;
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(1, shipment.OuterPackLines.Count);

				var mergedPackline = shipment.OuterPackLines.Cast<PackLine>().First();
				AssertPackLine(mergedPackline);
				AssertEquals(2, mergedPackline.JL_PackageCount);
				AssertEquals((decimal)7, mergedPackline.JL_ActualWeight);
				AssertEquals((decimal)3, mergedPackline.JL_ActualVolume);
				AssertEquals((decimal)14, mergedPackline.JL_LoadingMeters);
			}
		}

		public void TestReadingForTemplateShipment()
		{
			var bmw = Factory.New<OrgHeader>();
			bmw.OH_Code = "BMWAGWMUC";
			bmw.OH_IsConsignor = true;

			var officeAddress = bmw.Addresses.AddNew(OrgAddressType.Office, true);
			officeAddress.OA_Code = "123 QWERTY STREET";
			officeAddress.OA_CompanyNameOverride = "BMW AG";
			officeAddress.Address1 = "PETUELRING 130";
			officeAddress.City = "MUNCHEN";
			officeAddress.Postcode = "80788";
			officeAddress.OA_State = "BY";
			officeAddress.SetBaseOA_RL_NKRelatedPortCode("DEMUC");

			Factory.SaveForTesting();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var templateRecord = Factory.NewWithValidTestData<StmTemplateRecord>();
			templateRecord.STR_ModuleID = "JobShipment";
			templateRecord.STR_Data = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>TR00001000</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>
    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HouseBillOfLadingType>
      <Code>IAU</Code>
      <Description>IT Club Australia</Description>
    </HouseBillOfLadingType>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PortOfDestination>
      <Code>ADALV</Code>
      <Name>Andorra la Vella</Name>
    </PortOfDestination>
    <PortOfLoading>
      <Code>AUHEM</Code>
      <Name>Hemmant</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>ADALV</Code>
      <Name>Andorra la Vella</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>EBL</Code>
      <Description>Express Bill of Lading</Description>
    </ReleaseType>
    <ScreeningStatus>
      <Code>NOT</Code>
      <Description>Not Screened</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-05-14T02:05:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-05-21T02:05:00</Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
      </Date>
    </DateCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>123 QWERTY STREET</Address1>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>123 QWERTY STREET</AddressShortCode>
        <City>ANDORRA LA VELLA</City>
        <CompanyName>123412ALV</CompanyName>
        <Country>
          <Code>AD</Code>
          <Name>Andorra</Name>
        </Country>
        <OrganizationCode>BMWAGWMUC</OrganizationCode>
        <Port>
          <Code>ADALV</Code>
          <Name>Andorra la Vella</Name>
        </Port>
        <Postcode>1234</Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State>07</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <Address1>123 QWERTY STREET</Address1>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>123 QWERTY STREET</AddressShortCode>
        <City>ANDORRA LA VELLA</City>
        <CompanyName>123412ALV</CompanyName>
        <Country>
          <Code>AD</Code>
          <Name>Andorra</Name>
        </Country>
        <OrganizationCode>123412ALV</OrganizationCode>
        <Port>
          <Code>ADALV</Code>
          <Name>Andorra la Vella</Name>
        </Port>
        <Postcode>1234</Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State>07</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>123 QUERTY STREET</Address1>
        <AddressOverride>true</AddressOverride>
        <CompanyName>123412ALV</CompanyName>
        <Country>
          <Code>AD</Code>
          <Name>Andorra</Name>
        </Country>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Postcode>1234</Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State>07</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <Address1>123 QUERTY STREET</Address1>
        <AddressOverride>true</AddressOverride>
        <CompanyName>123412ALV</CompanyName>
        <Country>
          <Code>AD</Code>
          <Name>Andorra</Name>
        </Country>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Postcode>1234</Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State>07</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"" />
  </Shipment>
</UniversalShipment>";
			templateRecord.IsForTemplateSearch = true;

			((ITemplateRecordProvider)shipment).IsTemplateRecord = true;
			shipment.TemplateRecord = templateRecord;

			var consignorDocumentaryAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsignorDocumentaryAddress);
			consignorDocumentaryAddress.OrganizationCode = "BMWAGWMUC";
			consignorDocumentaryAddress.AddressShortCode = "123 QUERTY STREET";
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(Data.ConsigneeAddressCRAHOLSYDDataObject);
			shipmentDataObject.OrganizationAddressCollection.Add(consignorDocumentaryAddress);

			var newLogger = new ForwardingShipment.DummyLoggerForTemplate();

			newLogger.TopLevelDataObject = shipmentDataObject;

			var reader = (ITopLevelDataObjectReaderForTemplateRecord)new ShipmentDataObjectReader(shipmentDataObject, newLogger, Factory, null);
			reader.ReadDirectlyIntoBusinessObject(shipment);

			AssertNull(newLogger.Logs);

			AssertEquals(true, shipment.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals(false, shipment.ConsignorDocumentaryAddress.E2_AddressOverride);
		}

		#region TestTWUnmatchedMergedPackLineDoesNotGetMatched

		[TestDate(2021, 1, 1)]
		public void TestTWUnmatchedMergedPackLineDoesNotGetMatched()
		{
			var date1 = new ZDateTime(2021, 1, 1);
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				var existingPackline = existingShipment.OuterPackLines.AddNew();
				existingPackline.JL_PackLineId = "HI";
				existingPackline.JL_PackageCount = 2;
				existingPackline.JL_F3_NKPackType = "PLT";
				existingPackline.JL_ExportRefNumber = "EXP0001";
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = "HI-pack1";
				packline1.PackType = new PackageType() { Code = "PLT" };
				packline1.PackQty = 1;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;
				packline1.UNDGCollection.Clear();

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = "HI-pack2";
				packline2.PackType = new PackageType() { Code = "PLT" };
				packline2.PackQty = 1;
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;
				packline2.UNDGCollection.Clear();

				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", shipmentDataObject.PackingLineCollection);

				shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.False, date1));

				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("There should be two packlines, despite the merge of packline1 and packline2 matching the existing packline", 2, shipment.OuterPackLines.Count);

				var outerPackLines = shipment.OuterPackLines.Cast<ForwardingPackLine>();

				AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
						ZGuid.Empty,
						string.Empty,
						ZDateTime.Empty,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
						Array.Empty<ZString>());

				AssertPackLineReceiptInformations(shipment.OuterPackLines[1],
						warehouseAddressBO.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
						new ZString[] { "PKG1", "PKG2" });
			}
		}

		#endregion

		[TestDate(2021, 1, 1)]
		public void TestTWNotDeleteSurplusPackLineWithReceivingNoPackage()
		{
			var date1 = new ZDateTime(2021, 1, 1);
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				var existingPackline = existingShipment.OuterPackLines.AddNew();
				existingPackline.JL_PackLineId = "HI";
				existingPackline.JL_PackageCount = 2;
				existingPackline.JL_F3_NKPackType = "PLT";
				existingPackline.JL_ExportRefNumber = "EXP0001";
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = "HI-pack1";
				packline1.PackType = new PackageType() { Code = "PLT" };
				packline1.PackQty = 1;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = date1;
				packline1.UNDGCollection.Clear();

				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);
				shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, date1));
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", shipmentDataObject.PackingLineCollection);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("There should be two packlines", 2, shipment.OuterPackLines.Count);
				AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
						ZGuid.Empty,
						ZString.Empty,
						ZDate.Empty,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
						Array.Empty<ZString>());
				AssertPackLineReceiptInformations(shipment.OuterPackLines[1],
						warehouseAddressBO.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
						new ZString[] { "PKG1" });

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = "HI-pack2";
				packline2.PackType = new PackageType() { Code = "PLT" };
				packline2.PackQty = 1;
				packline2.OutturnQty = 1;
				packline2.UnloadDate = date1;
				packline2.UNDGCollection.Clear();
				packlines.Clear();
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", shipmentDataObject.PackingLineCollection);
				shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.True, date1));
				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("There should be three packlines", 3, shipment.OuterPackLines.Count);
				AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
						ZGuid.Empty,
						ZString.Empty,
						ZDate.Empty,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
						Array.Empty<ZString>());
				AssertPackLineReceiptInformations(shipment.OuterPackLines[1],
						warehouseAddressBO.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
						new ZString[] { "PKG1" });
				AssertPackLineReceiptInformations(shipment.OuterPackLines[2],
						warehouseAddressBO.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
						new ZString[] { "PKG2" });
			}
		}

		public void TestRegroupTWPackagesByCommonAttributes_WarehouseOrgFallback()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;
				packlines.Add(packline1);
				packlines.Add(packline2);

				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(1, shipment.OuterPackLines.Count);
			}
		}

		public void TestTWTransitDispatch_RegistryEnabledWithXMLProcessed()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_RefNumber = "REF001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";
				packlineBO2.JL_RefNumber = "REF002";

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var savedPackLineId1 = packlineBO1.JL_PackLineId;
				var savedPackLineId2 = packlineBO2.JL_PackLineId;

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = savedPackLineId1;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Now;
				packline1.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = savedPackLineId1;
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Now;
				packline2.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline3 = CreatePackline();
				packline3.ReferenceNumber = "PKG3";
				packline3.PackingLineID = savedPackLineId2;
				packline3.OutturnQty = 1;
				packline3.UnloadDate = ZDateTime.Now;
				packline3.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline4 = CreatePackline(); //surplus packages
				packline4.ReferenceNumber = "PKG4";
				packline4.PackingLineID = savedPackLineId1;
				packline4.OutturnQty = 1;
				packline4.UnloadDate = ZDateTime.Now;
				packline4.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);
				packlines.Add(packline4);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);

				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("Precondition: packages are imported.", 4, shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.PkgPackageCollection).Count());
				AssertEquals("Precondition: container should be empty.", 0, shipment.OuterPackLines.Cast<ForwardingPackLine>().First().Containers.Count);

				Factory.SaveForTesting();

				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "TR00001");

				var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002" };
				containers = new DataObjectList<Container>() { container1, container2 };
				shipmentDataObject.SetContainerCollection(() => containers);

				packline1.ContainerLink = 1;
				packline1.LoadDate = ZDateTime.Now;
				packline2.ContainerLink = 2;
				packline2.LoadDate = ZDateTime.Now;
				packline3.ContainerLink = 1;
				packline3.LoadDate = ZDateTime.Now;
				packline4.ContainerLink = null;
				packline4.LoadDate = ZDateTime.Now;

				foreach (var packline in packlines)
				{
					packline.ReferenceNumberCollection.ForEach(r => r.Type.Code = "TDU");
				}

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header1.DataContext = new DataContext();
				header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
				header1.VesselName = "CONT0001";
				header1.SetContainerCollection(() => new DataObjectList<Container>() { container1 });

				var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				loadList.DataContext = new DataContext();
				loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
				loadList.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = consol.JK_UniqueConsignRef },
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = "DUMMY" }
				});
				loadList.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline2 });
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { header1, loadList });

				Factory.SaveForTesting();

				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AsserrtPacklinePackages(shipment, savedPackLineId1, savedPackLineId2);

				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AsserrtPacklinePackages(shipment, savedPackLineId1, savedPackLineId2); // result should not change on second import
			}
		}

		public void TestTWTransitDispatch_ContainersImportedToCorrectConsols_MatchByConsolNum()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_RefNumber = "REF001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";
				packlineBO2.JL_RefNumber = "REF002";

				var consol1 = existingShipment.Consols.AddNew();
				consol1.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol1.JK_UniqueConsignRef = "CON888888";

				var consol2 = existingShipment.Consols.AddNew();
				consol2.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol2.JK_UniqueConsignRef = "CON888855";

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container1, container2 });

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = "TEST001";
				packline1.OutturnQty = 1;
				packline1.ContainerLink = 2;
				packline1.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = "TEST002";
				packline2.OutturnQty = 1;
				packline2.ContainerLink = 1;
				packline2.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline3 = CreatePackline();
				packline3.ReferenceNumber = "PKG3";
				packline3.PackingLineID = "TEST001";
				packline3.OutturnQty = 1;
				packline3.ContainerLink = 2;
				packline3.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header1.DataContext = new DataContext();
				header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
				header1.VesselName = "CONT0001";
				header1.SetContainerCollection(() => new DataObjectList<Container>() { container1 });

				var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header2.DataContext = new DataContext();
				header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000058");
				header2.VesselName = "CONT0002";
				header2.SetContainerCollection(() => new DataObjectList<Container>() { container2 });

				var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				loadList.DataContext = new DataContext();
				loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
				loadList.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = "CON888888" },
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = "DUMMY" }
				});
				loadList.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline2 });
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { header1, header2, loadList });

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var consol1PostImport = shipment.Consols.Cast<ForwardingConsol>().First(c => c.JK_UniqueConsignRef == "CON888888");
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0001"));
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0002"));
			}
		}

		public void TestTWTransitDispatch_ContainersImportedToCorrectConsols_MatchByMABNum()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_RefNumber = "REF001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";
				packlineBO2.JL_RefNumber = "REF002";

				var consol1 = existingShipment.Consols.AddNew();
				consol1.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol1.JK_UniqueConsignRef = "CON888882";
				consol1.JK_MasterBillNum = "MB00001";

				var consol2 = existingShipment.Consols.AddNew();
				consol2.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol2.JK_UniqueConsignRef = "CON888855";

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container1, container2 });

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = "TEST001";
				packline1.OutturnQty = 1;
				packline1.ContainerLink = 2;
				packline1.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = "TEST002";
				packline2.OutturnQty = 1;
				packline2.ContainerLink = 1;
				packline2.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline3 = CreatePackline();
				packline3.ReferenceNumber = "PKG3";
				packline3.PackingLineID = "TEST001";
				packline3.OutturnQty = 1;
				packline3.ContainerLink = 2;
				packline3.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header1.DataContext = new DataContext();
				header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
				header1.VesselName = "CONT0001";
				header1.SetContainerCollection(() => new DataObjectList<Container>() { container1 });

				var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header2.DataContext = new DataContext();
				header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000058");
				header2.VesselName = "CONT0002";
				header2.SetContainerCollection(() => new DataObjectList<Container>() { container2 });

				var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				loadList.DataContext = new DataContext();
				loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
				loadList.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference { Type = new EntryType { Code = "MAB" }, ReferenceNumber = "MB00001" },
					new AdditionalReference { Type = new EntryType { Code = "MAB" }, ReferenceNumber = "DUMMY" }
				});
				loadList.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline2 });
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { header1, header2, loadList });

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var consol1PostImport = shipment.Consols.Cast<ForwardingConsol>().First(c => c.JK_UniqueConsignRef == "CON888882");
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0001"));
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0002"));
			}
		}

		public void TestTWTransitReceive_UpdateContainersWithoutNewRoadContainer()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol.JK_UniqueConsignRef = "CON888888";

				var refContainerRoad1 = Factory.NewWithValidTestData<RefContainer>();
				refContainerRoad1.RC_Code = "RCONT0001";
				refContainerRoad1.RC_ShippingMode = TransportModes.Road;

				var refContainerRoad2 = Factory.NewWithValidTestData<RefContainer>();
				refContainerRoad2.RC_Code = "RCONT0002";
				refContainerRoad2.RC_ShippingMode = TransportModes.Road;

				var refContainerOther = Factory.NewWithValidTestData<RefContainer>();
				refContainerOther.RC_Code = "RCONT0003";
				refContainerOther.RC_ShippingMode = TransportModes.Other;

				var consolContainerRoad1 = consol.Containers.AddNew();
				consolContainerRoad1.JC_ContainerNum = "CONT0001";
				consolContainerRoad1.JC_RC = refContainerRoad1.PK;

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_RefNumber = "REF001";
				packlineBO1.SetContainer(consol, consolContainerRoad1);

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";
				packlineBO2.JL_RefNumber = "REF002";
				packlineBO2.SetContainer(consol, consolContainerRoad1);

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber },
						ReferenceNumber = "CON888888"
					}
				});

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var container1 = new Container() { Link = 1, ContainerType = new ContainerType() { Code = "RCONT0001", Description = "CONT0001" }, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerType = new ContainerType() { Code = "RCONT0002", Description = "CONT0002" }, ContainerNumber = "CONT0002" };
				var container3 = new Container() { Link = 3, ContainerType = new ContainerType() { Code = "RCONT0003", Description = "CONT0003" }, ContainerNumber = "CONT0003" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container1, container2, container3 });

				var packlinesDataObject = new DataObjectList<PackingLine>();
				var packlineDataObject = CreatePacklineForReceive();
				packlineDataObject.ReferenceNumber = "PKG3";
				packlineDataObject.PackingLineID = "TEST003";
				packlineDataObject.OutturnQty = 1;
				packlineDataObject.ContainerLink = 3;
				packlineDataObject.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				packlinesDataObject.Add(packlineDataObject);

				shipmentDataObject.SetPackingLineCollection(() => packlinesDataObject);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var consol1PostImport = shipment.Consols.Cast<ForwardingConsol>().First(c => c.JK_UniqueConsignRef == "CON888888");
				AssertEquals(2, consol1PostImport.Containers.Count);
				AssertEquals(consolContainerRoad1.JC_ContainerNum, consol1PostImport.Containers[0].JC_ContainerNum);
				AssertEquals("Non-truck container should be imported", "CONT0003", consol1PostImport.Containers[1].JC_ContainerNum);

				var existPacklineIds = new List<ZString> { packlineBO1.JL_PackLineId, packlineBO2.JL_PackLineId };
				var newPackline = shipment.OuterPackLines.Where(pl => !existPacklineIds.Contains(pl.JL_PackLineId)).FirstOrDefault();
				AssertEquals(1, newPackline.Containers.Count);
			}
		}

		public void TestTWTransitReceive_UpdateWithNoDiscrepanciesResetsPackLineStatusToConfirmed()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = WarehouseAddressBO.PK;

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var packlineBO = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO, contact);
				packlineBO.JL_PackLineId = "TEST001";
				packlineBO.JL_ActualWeight = 23;

				var shipmentDataObject = CreateShipmentDO(WarehouseAddressBO);

				var packline = CreatePackline();
				var packlines = new DataObjectList<PackingLine>();
				packline.ReferenceNumber = "PKG1";
				packline.PackingLineID = packlineBO.JL_PackLineId;
				packline.Weight = 24;
				packline.OutturnQty = 1;
				packline.UnloadDate = ZDateTime.Today;
				packline.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TRU" }, ReferenceNumber = "TD00000058" } });
				packlines.Add(packline);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packlineBO.JL_OriginTransitWarehouseStatus);

				packline.Weight = 23;
				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packlineBO.JL_OriginTransitWarehouseStatus);
			}
		}

		public void TestTWTransitReceive_UpdateWithDiscrepanciesResetsPackLineStatusToWithDiscrepancies()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = WarehouseAddressBO.PK;

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var packlineBO = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO, contact);
				packlineBO.JL_PackLineId = "TEST001";

				var shipmentDataObject = CreateShipmentDO(WarehouseAddressBO);

				var packline = CreatePackline();
				var packlines = new DataObjectList<PackingLine>();
				packline.ReferenceNumber = "PKG1";
				packline.PackingLineID = packlineBO.JL_PackLineId;
				packline.OutturnQty = 1;
				packline.UnloadDate = ZDateTime.Today;
				packline.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TRU" }, ReferenceNumber = "TD00000058" } });
				packlines.Add(packline);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				Factory.SaveForTesting();
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packlineBO.JL_OriginTransitWarehouseStatus);

				packline.Weight = 24;
				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				Factory.SaveForTesting();
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packlineBO.JL_OriginTransitWarehouseStatus);
			}
		}

		public void TestTWTransitReceive_UpdateWithNewPackageWtihDiscrepanciesResetsPackLineStatusToWithDiscrepancies()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = WarehouseAddressBO.PK;

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var packlineBO = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO, contact);
				packlineBO.JL_PackLineId = "TEST001";

				var shipmentDataObject = CreateShipmentDO(WarehouseAddressBO);

				var packline = CreatePackline();
				var packlines = new DataObjectList<PackingLine>();
				packline.ReferenceNumber = "PKG1";
				packline.PackingLineID = packlineBO.JL_PackLineId;
				packline.OutturnQty = 1;
				packline.UnloadDate = ZDateTime.Today;
				packline.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TRU" }, ReferenceNumber = "TD00000058" } });
				packlines.Add(packline);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				Factory.SaveForTesting();
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packlineBO.JL_OriginTransitWarehouseStatus);

				var packlineNew = CreatePackline();
				packlineNew.ReferenceNumber = "PKG2";
				packlineNew.PackingLineID = packlineBO.JL_PackLineId;
				packlineNew.OutturnQty = 1;
				packlineNew.UnloadDate = ZDateTime.Today;
				packlineNew.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TRU" }, ReferenceNumber = "TD00000058" } });
				packlines.Add(packlineNew);
				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				Factory.SaveForTesting();
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packlineBO.JL_OriginTransitWarehouseStatus);
			}
		}

		public void TestTWTransitReceive_ContainersImportedToCorrectConsols_MatchByConsolNum()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_RefNumber = "REF001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";
				packlineBO2.JL_RefNumber = "REF002";

				var consol1 = existingShipment.Consols.AddNew();
				consol1.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol1.JK_UniqueConsignRef = "CON888888";

				var consol2 = existingShipment.Consols.AddNew();
				consol2.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol2.JK_UniqueConsignRef = "CON888855";

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber },
						ReferenceNumber = "CON888888"
					}
				});

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container1, container2 });

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = "TEST001";
				packline1.OutturnQty = 1;
				packline1.ContainerLink = 2;
				packline1.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = "TEST002";
				packline2.OutturnQty = 1;
				packline2.ContainerLink = 1;
				packline2.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline3 = CreatePackline();
				packline3.ReferenceNumber = "PKG3";
				packline3.PackingLineID = "TEST001";
				packline3.OutturnQty = 1;
				packline3.ContainerLink = 2;
				packline3.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var consol1PostImport = shipment.Consols.Cast<ForwardingConsol>().First(c => c.JK_UniqueConsignRef == "CON888888");
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0001"));
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0002"));
			}
		}

		public void TestTWTransitReceive_ContainersImportedToCorrectConsols_MatchByMABNum()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_RefNumber = "REF001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";
				packlineBO2.JL_RefNumber = "REF002";

				var consol1 = existingShipment.Consols.AddNew();
				consol1.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol1.JK_UniqueConsignRef = "CON888882";
				consol1.JK_MasterBillNum = "MB00001";

				var consol2 = existingShipment.Consols.AddNew();
				consol2.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol2.JK_UniqueConsignRef = "CON888855";

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.MasterBill },
						ReferenceNumber = "MB00001"
					}
				});

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container1, container2 });

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = "TEST001";
				packline1.OutturnQty = 1;
				packline1.ContainerLink = 2;
				packline1.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = "TEST002";
				packline2.OutturnQty = 1;
				packline2.ContainerLink = 1;
				packline2.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });

				var packline3 = CreatePackline();
				packline3.ReferenceNumber = "PKG3";
				packline3.PackingLineID = "TEST001";
				packline3.OutturnQty = 1;
				packline3.ContainerLink = 2;
				packline3.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header1.DataContext = new DataContext();
				header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
				header1.VesselName = "CONT0001";
				header1.SetContainerCollection(() => new DataObjectList<Container>() { container1 });

				var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header2.DataContext = new DataContext();
				header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000058");
				header2.VesselName = "CONT0002";
				header2.SetContainerCollection(() => new DataObjectList<Container>() { container2 });

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var consol1PostImport = shipment.Consols.Cast<ForwardingConsol>().First(c => c.JK_UniqueConsignRef == "CON888882");
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0001"));
				Assert(consol1PostImport.Containers.Cast<ForwardingContainer>().Any(c => c.JC_ContainerNum == "CONT0002"));
			}
		}

		static void AsserrtPacklinePackages(ForwardingShipment shipment, string savedPackLineId1, string savedPackLineId2)
		{
			AssertEquals("Precondition: packline has changed.", 4, shipment.OuterPackLines.Cast<ForwardingPackLine>().Count());
			AssertEquals("Container should be created.", 2, shipment.Consols.Cast<ForwardingConsol>().First().Containers.Count);
			AssertEquals("Additional packages should be imported", 4, shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.PkgPackageCollection).Count());

			var pack1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().First(p => p.JL_PackLineId == savedPackLineId1);
			AssertEquals("REF001", pack1.JL_RefNumber);
			AssertEquals(1, pack1.PkgPackageCollection.Count);
			Assert(pack1.Containers.Cast<CommonContainer>().Any(c => c.JC_ContainerNum == "CONT0001"));
			AssertEquals("PKG1", pack1.PkgPackageCollection.First().KP_PackageID);
			//AssertEquals("CNF", pack1.JL_OriginTransitWarehouseStatus);

			var pack2 = shipment.OuterPackLines.Cast<ForwardingPackLine>().First(p => p.JL_PackLineId == savedPackLineId2);
			AssertEquals("REF002", pack2.JL_RefNumber);
			AssertEquals(1, pack2.PkgPackageCollection.Count);
			Assert(pack2.Containers.Cast<CommonContainer>().Any(c => c.JC_ContainerNum == "CONT0001"));
			AssertEquals("PKG3", pack2.PkgPackageCollection.First().KP_PackageID);
			//AssertEquals("CNF", pack2.JL_OriginTransitWarehouseStatus);

			var pack3 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.PkgPackageCollection.Count == 1 && p.PkgPackageCollection.Single().KP_PackageID == "PKG2");
			AssertEquals("REF001", pack3.JL_RefNumber);
			Assert(pack3.Containers.Cast<CommonContainer>().Any(c => c.JC_ContainerNum == "CONT0002"));
			//AssertEquals("CNF", pack3.JL_OriginTransitWarehouseStatus);

			var pack4 = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single(p => p.PkgPackageCollection.Count == 1 && p.PkgPackageCollection.Single().KP_PackageID == "PKG4");
			AssertEquals("REF001", pack3.JL_RefNumber);
			Assert(!pack4.Containers.Cast<CommonContainer>().Any());
			//AssertEquals("CNF", pack3.JL_OriginTransitWarehouseStatus);
		}

		public void TestTWTransitDispatch_PacklineValuesAreCopiedOntoSplitPackline()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();

				packlineBO1.JL_RefNumber = "REF001";
				// country
				// country hs code
				packlineBO1.JL_HarmonisedCode = "HAR";
				packlineBO1.JL_RN_NKOrigin = "AU";
				packlineBO1.JL_Damaged = 22;
				packlineBO1.JL_DetailedDescription = "detailed";
				packlineBO1.JL_EndItemNo = 5;
				packlineBO1.JL_ExportRefNumber = "export";
				packlineBO1.JL_ImportRefNumber = "import";
				packlineBO1.JL_ItemNo = 88;
				packlineBO1.JL_Pillaged = 38;
				packlineBO1.JL_LinePrice = 77;
				packlineBO1.JL_CustomAttrib1 = "aaa";
				packlineBO1.JL_CustomAttrib2 = "bbb";
				packlineBO1.JL_CustomDate1 = new ZDate(2011, 5, 5);
				packlineBO1.JL_CustomDate2 = new ZDate(2009, 5, 5);

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = packlineBO1.JL_PackLineId;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Now;

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = packlineBO1.JL_PackLineId;
				packline2.OutturnQty = 1;
				packline2.OutturnDamagedQty = 1;
				packline2.UnloadDate = ZDateTime.Now;

				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);

				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("Precondition: packages are imported.", 2, shipment.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.PkgPackageCollection).Count());

				Factory.SaveForTesting();

				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "TR00001");

				var container1 = new Container() { Link = 1, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerNumber = "CONT0002" };
				containers = new DataObjectList<Container>() { container1, container2 };
				shipmentDataObject.SetContainerCollection(() => containers);

				packline1.ContainerLink = 1;
				packline1.LoadDate = ZDateTime.Now;
				packline2.ContainerLink = 2;
				packline2.LoadDate = ZDateTime.Now;
				packline1.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });
				packline2.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000058" } });

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var header1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header1.DataContext = new DataContext();
				header1.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
				header1.VesselName = "CONT0001";
				header1.SetContainerCollection(() => new DataObjectList<Container>() { container1 });

				var header2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header2.DataContext = new DataContext();
				header2.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000058");
				header2.VesselName = "CONT0002";
				header2.SetContainerCollection(() => new DataObjectList<Container>() { container2 });

				var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				loadList.DataContext = new DataContext();
				loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
				loadList.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = consol.JK_UniqueConsignRef },
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = "DUMMY" }
				});
				loadList.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { packline2 });
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { header1, loadList });

				Factory.SaveForTesting();

				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("two packlines on shipment (second one is split)", 2, shipment.OuterPackLines.Count);

				var resPacklineBO2 = shipment.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(1);
				CombineAssertions("split packline has copied attributes", () =>
				{
					AssertEquals("REF001", resPacklineBO2.JL_RefNumber);
					AssertEquals("HAR", resPacklineBO2.JL_HarmonisedCode);
					AssertEquals("AU", resPacklineBO2.JL_RN_NKOrigin);
					AssertEquals(1, resPacklineBO2.JL_Damaged);
					AssertEquals("detailed", resPacklineBO2.JL_DetailedDescription);
					AssertEquals(5, (int)resPacklineBO2.JL_EndItemNo);
					AssertEquals("export", resPacklineBO2.JL_ExportRefNumber);
					AssertEquals("import", resPacklineBO2.JL_ImportRefNumber);
					AssertEquals(88, (int)resPacklineBO2.JL_ItemNo);
					AssertEquals(38, resPacklineBO2.JL_Pillaged);
					AssertEquals(77, (int)resPacklineBO2.JL_LinePrice);
					AssertEquals("aaa", resPacklineBO2.JL_CustomAttrib1);
					AssertEquals("bbb", resPacklineBO2.JL_CustomAttrib2);
					AssertEquals(new ZDate(2011, 5, 5), resPacklineBO2.JL_CustomDate1);
					AssertEquals(new ZDate(2009, 5, 5), resPacklineBO2.JL_CustomDate2);
					AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, resPacklineBO2.JL_OriginTransitWarehouseStatus);
				});

				CombineAssertions("split packline excluded clone attributes", () =>
				{
					AssertEquals("packline id is different, the value is generated from number fountain", "EDIDAT00000002", resPacklineBO2.JL_PackLineId);
				});

				Factory.SaveForTesting();
			}
		}

		public void TestRegroupTWPackagesByCommonAttributes_Matching()
		{
			ForwardingShipment RunCase(ZGuid shipmentCFSAddressPK, ZGuid consolDepartureCFSPK)
			{
				return new ShipmentDataObjectReader(SetupMatching(shipmentCFSAddressPK, consolDepartureCFSPK), Logger, Factory, null).ReadIntoBusinessObject();
			}
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = RunCase(WarehouseAddressBO.PK, NonWarehouseAddressBO.PK);
				AssertEquals("matches", 1, shipment.OuterPackLines.Count);
				shipment = RunCase(WarehouseAddressBO.PK, ZGuid.Empty);
				AssertEquals("matches", 1, shipment.OuterPackLines.Count);
				shipment = RunCase(NonWarehouseAddressBO.PK, WarehouseAddressBO.PK);
				AssertEquals("matches", 1, shipment.OuterPackLines.Count);
				shipment = RunCase(ZGuid.Empty, WarehouseAddressBO.PK);
				AssertEquals("matches", 1, shipment.OuterPackLines.Count);
				AssertExceptionThrown("throws exception", typeof(DataObjectReadFailureException), () => RunCase(ZGuid.Empty, ZGuid.Empty));
				AssertExceptionThrown("throws exception", typeof(DataObjectReadFailureException), () => RunCase(NonWarehouseAddressBO.PK, NonWarehouseAddressBO.PK));
			}
		}

		public void TestShouldNotCreateNewConsolWhenAllContainersSentFromTWHAreRoadContainers()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consol.JK_UniqueConsignRef = "CON888888";

				var refContainerRoad1 = Factory.NewWithValidTestData<RefContainer>();
				refContainerRoad1.RC_Code = "RCONT0001";
				refContainerRoad1.RC_ShippingMode = TransportModes.Road;

				var refContainerRoad2 = Factory.NewWithValidTestData<RefContainer>();
				refContainerRoad2.RC_Code = "RCONT0002";
				refContainerRoad2.RC_ShippingMode = TransportModes.Road;

				var consolContainerRoad1 = consol.Containers.AddNew();
				consolContainerRoad1.JC_ContainerNum = "CONT0001";
				consolContainerRoad1.JC_RC = refContainerRoad1.PK;

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitDispatch, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber },
						ReferenceNumber = "CON888888"
					}
				});

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var container1 = new Container() { Link = 1, ContainerType = new ContainerType() { Code = "RCONT0001", Description = "CONT0001" }, ContainerNumber = "CONT0001" };
				var container2 = new Container() { Link = 2, ContainerType = new ContainerType() { Code = "RCONT0002", Description = "CONT0002" }, ContainerNumber = "CONT0002" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container1, container2 });

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var factory2 = new BusinessObjectFactory();
				var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
				AssertEquals("No new consol will be created.", 1, shipment2.Consols.Count);
				AssertEquals("Still the existing one.", shipment2.Consols.First().PK, consol.PK);
			}
		}

		OrgAddress WarehouseAddressBO => warehouseAddressBO ?? (warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>());

		OrgAddress warehouseAddressBO;

		OrgAddress NonWarehouseAddressBO => nonWarehouseAddressBO ?? (nonWarehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>());

		OrgAddress nonWarehouseAddressBO;

		UniversalShipment SetupMatching(ZGuid shipmentCFSAddressPK, ZGuid consolDepartureCFSAddressPK)
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";
			Factory.SaveForTesting();

			var existingShipment = Factory.New<ForwardingShipment>();
			existingShipment.JS_HouseBill = "BACON PANCAKES";
			existingShipment.JS_OA_ExportReceivingDepot = shipmentCFSAddressPK;
			var consol = existingShipment.Consols.AddNew();
			consol.JK_OA_PackDepotAddress = consolDepartureCFSAddressPK;
			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

			var addresses = new List<OrganizationAddress>();
			addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = WarehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = WarehouseAddressBO.Header.OH_Code });
			shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

			var packlines = new DataObjectList<PackingLine>();
			var packline1 = CreatePackline();
			packline1.ReferenceNumber = "PKG1";
			packline1.OutturnQty = 1;
			packline1.UnloadDate = ZDateTime.Today;

			var packline2 = CreatePackline();
			packline2.ReferenceNumber = "PKG2";
			packline2.OutturnQty = 1;
			packline2.UnloadDate = ZDateTime.Today;
			packlines.Add(packline1);
			packlines.Add(packline2);

			AddEmptyTRUReferences(packlines);

			shipmentDataObject.SetPackingLineCollection(() => packlines);
			Factory.SaveForTesting();
			return shipmentDataObject;
		}

		public void TestRegroupTWPackagesByCommonAttributes_DifferentContainersInDifferentGroup()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT0001";
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ContainerLink = 1;
				packline1.ContainerPackingOrder = 2;
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;

				var packline2 = CreatePackline();
				packline2.ContainerPackingOrder = 3;
				packline2.ReferenceNumber = "PKG2";
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				packlines.Add(packline2);

				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(2, shipment.OuterPackLines.Count);

				var packline1BO = shipment.OuterPackLines.Cast<PackLine>().First();
				AssertEquals(1, packline1BO.Containers.Count); //automatically set on default
				AssertEquals(1, packline1BO.JL_ContainerPackingOrder);

				var packline2BO = shipment.OuterPackLines.Cast<PackLine>().ElementAt(1);
				AssertEquals(0, packline2BO.Containers.Count);
				AssertEquals(3, packline2BO.JL_ContainerPackingOrder);
			}
		}

		internal static void AddEmptyTRUReferences(DataObjectList<PackingLine> packlines)
		{
			// because transit receipt is filtered by packlines containing TRU references, add a TRU reference for each packline
			if (packlines == null)
			{
				return;
			}
			foreach (var packline in packlines)
			{
				packline.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TRU" }, ReferenceNumber = "TD00000054" } });
			}
		}

		internal static void AddEmptyTDUReferences(DataObjectList<PackingLine> packlines)
		{
			// because transit dispatch is filtered by packlines containing TDU references, add a TDU reference for each packline
			if (packlines == null)
			{
				return;
			}
			foreach (var packline in packlines)
			{
				packline.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TDU" }, ReferenceNumber = "TD00000054" } });
			}
		}

		public void TestRegroupTWPackagesByCommonAttributes_OuttrunProperties()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnComment = "com";
				packline1.OutturnDamagedQty = 3;
				packline1.OutturnedHeight = 44;
				packline1.OutturnedLength = 22;
				packline1.OutturnedVolume = 11;
				packline1.OutturnedWeight = 44;
				packline1.OutturnedWidth = 88;
				packline1.OutturnPillagedQty = 11;
				packline1.OutturnQty = 13;
				packline1.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals(1, shipment.OuterPackLines.Count);

				var packlineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
				AssertEquals(ZInt.Zero, packlineBO.JL_Outturn);
				AssertEquals("", packlineBO.JL_OutturnComment);
				AssertEquals(3, packlineBO.JL_Damaged);
				AssertEquals(ZDecimal.Zero, packlineBO.JL_OutturnedHeight);
				AssertEquals(ZDecimal.Zero, packlineBO.JL_OutturnedLength);
				AssertEquals(ZDecimal.Zero, packlineBO.JL_OutturnedVolume);
				AssertEquals(ZDecimal.Zero, packlineBO.JL_OutturnedWeight);
				AssertEquals(ZDecimal.Zero, packlineBO.JL_OutturnedWidth);
				AssertEquals(ZInt.Zero, packlineBO.JL_Pillaged);
				AssertEquals(ZInt.Zero, packlineBO.JL_Outturn);
			}
		}

		public void TestTWPackagesLinkedToPackLine_NewPackLineCreatedWithPackagesLinked()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;
				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;
				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("New pack line is created", 1, shipment.OuterPackLines.Count);

				var packlineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
				var job = packlineBO.PkgPackageCollection.First().PackageJob;
				AssertNotNull("Package Job is created", job);
				AssertEquals("Package Job Linked to Shipment", existingShipment.PK, job.KJ_ParentID);
				AssertEquals("Table code should be JS", JobShipmentSchema.Constants.Prefix, job.KJ_ParentTableCode);
				AssertEquals("2 packages are linked from xml", 2, packlineBO.PkgPackageCollection.Count);
				Assert("pack line has the status of surplus", shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus));
			}
		}

		public void TestTWPackagesLinkedToPackLine_OutturnQtyIsEmpty()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;
				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.OutturnQty = 0;
				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("New pack line is created", 1, shipment.OuterPackLines.Count);

				var packlineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
				var job = packlineBO.PkgPackageCollection.First().PackageJob;
				AssertNotNull("Package Job is created", job);
				AssertEquals("Package Job Linked to Shipment", existingShipment.PK, job.KJ_ParentID);
				AssertEquals("Table code should be JS", JobShipmentSchema.Constants.Prefix, job.KJ_ParentTableCode);
				AssertEquals("1 packages are linked from xml", 1, packlineBO.PkgPackageCollection.Count);
				Assert("pack line has the status of surplus", shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus));
			}
		}
		public void TestTWPackagesLinkedToPackLine_ExistingPacklineWeithIds()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				packlineBO2.JL_PackLineId = "TEST002";

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				AssertEquals("Precondition: 2 packline on shipment.", 2, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = packlineBO1.JL_PackLineId;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.PackingLineID = packlineBO2.JL_PackLineId;
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;

				var packline3 = CreatePackline();
				packline3.ReferenceNumber = "PKG3";
				packline3.PackingLineID = "TEST003";
				packline3.OutturnQty = 1;
				packline3.UnloadDate = ZDateTime.Today;

				var packline4 = CreatePackline();
				packline4.PackingLineID = "TEST4";
				packline4.OutturnQty = 1;
				packline4.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("3 packlines on shipment, packline with no package id is ignored", 3, shipment.OuterPackLines.Count);
				Assert("Each packline should have 1 package", shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 1));

				var surplusPackLines = shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus);
				AssertEquals("There should be 2 packages with Surplus status", 1, surplusPackLines.Count());
			}
		}

		public void TestTWPackagesLinkedToPackLine_ExtractPANAndERC()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				warehouseAddressBO.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.ExportReferenceNumber = string.Empty;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;
				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.ExportReferenceNumber = string.Empty;
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;
				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				shipmentDataObject.SetPortReferenceCollection(() =>
				{
					return new List<PortReference>()
					{
						new PortReference
						{
							Type = new PortReferenceType
							{
								Code = "PAN",
								Description = "Port Authority Number"
							},
							Reference = "P00001",
							Country = new Country
							{
								Code = "DE",
								Name = "German"
							},
							Status = new PortReferenceStatus
							{
								Code = "CLR"
							}
						}
					};
				});

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("New pack line is created", 1, shipment.OuterPackLines.Count);

				var packlineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
				var job = packlineBO.PkgPackageCollection.First().PackageJob;

				CombineAssertions(() =>
				{
					AssertNotNull("Package Job is created", job);
					AssertEquals("Package Job Linked to Shipment", existingShipment.PK, job.KJ_ParentID);
					AssertEquals("Table code should be JS", JobShipmentSchema.Constants.Prefix, job.KJ_ParentTableCode);
					AssertEquals("2 packages are linked from xml", 2, packlineBO.PkgPackageCollection.Count);
					Assert("pack line has the status of surplus", shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus));

					AssertEquals("PAN is extracted to AdditionalReferenceNumbers from PAN", "P00001", packlineBO.PortReferences.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == "PAN" && x.CE_RN_NKCountryCode == "DE" && x.CE_EntryStatus == "CLR").CE_EntryNum);
					AssertEquals("ERC is extracted to AdditionalReferenceNumbers from RCN", "TR00001", packlineBO.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);
					AssertEquals("PAN is not extracted to Export Ferefence Number as TW is not FR", string.Empty, packlineBO.JL_ExportRefNumber);
				});
			}
		}

		public void TestTWPackagesLinkedToPackLine_ExtractPANERCAndExportReferenceForFrance()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				warehouseAddressBO.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;
				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG2";
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;
				packlines.Add(packline1);
				packlines.Add(packline2);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				shipmentDataObject.SetPortReferenceCollection(() =>
				{
					return new List<PortReference>()
					{
						new PortReference
						{
							Type = new PortReferenceType
							{
								Code = "PAN",
								Description = "Port Authority Number"
							},
							Reference = "P00001",
							Country = new Country
							{
								Code = "FR",
								Name = "France"
							}
						}
					};
				});

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("New pack line is created", 1, shipment.OuterPackLines.Count);

				var packlineBO = shipment.OuterPackLines.Cast<ForwardingPackLine>().First();
				var job = packlineBO.PkgPackageCollection.First().PackageJob;

				CombineAssertions(() =>
				{
					AssertNotNull("Package Job is created", job);
					AssertEquals("Package Job Linked to Shipment", existingShipment.PK, job.KJ_ParentID);
					AssertEquals("Table code should be JS", JobShipmentSchema.Constants.Prefix, job.KJ_ParentTableCode);
					AssertEquals("2 packages are linked from xml", 2, packlineBO.PkgPackageCollection.Count);
					Assert("pack line has the status of surplus", shipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus));

					AssertEquals("PAN is extracted to AdditionalReferenceNumbers from PAN", "P00001", packlineBO.PortReferences.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN).CE_EntryNum);
					AssertEquals("ERC is extracted to AdditionalReferenceNumbers from RCN", "TR00001", packlineBO.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().First(x => x.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum);
					AssertEquals("PAN is extracted to Export Ferefence Number for France TW", "P00001", packlineBO.JL_ExportRefNumber);
				});
			}
		}

		public void TestTWSubsequentMatchesUpdatesStatus()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var shipmentBO = Factory.New<ForwardingShipment>();
				shipmentBO.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = shipmentBO.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped;

				var consol = shipmentBO.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();

				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.UNDGCollection.Clear();
				packline1.PackingLineID = packlineBO1.JL_PackLineId;
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);

				AssertEquals("Precondition - packline Transit Warehouse status", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped, shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(0).JL_OriginTransitWarehouseStatus);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("1 packline", 1, shipment.OuterPackLines.Count);
				AssertEquals("Status should have changed", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, shipment.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(0).JL_OriginTransitWarehouseStatus);
			}
		}

		#region TestTWMergeSplit_DifferentContainers

		public void TestTWMergeSplit_DifferentContainers()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "CONT0001";
				packlineBO1.JL_JC = container1.PK;
				packlineBO1.JL_RefNumber = "RefNumber1";

				Factory.SaveForTesting();

				AssertEquals("Precondition: 1 packline on shipment.", 1, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				LinkShipmentDOToConsol(shipmentDataObject, consol);

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var savedPackLineId = packlineBO1.JL_PackLineId;

				var packlines = new DataObjectList<PackingLine>();
				var packline1_1 = CreatePackline();
				packline1_1.ReferenceNumber = "PKG1";
				packline1_1.PackingLineID = savedPackLineId;
				packline1_1.ContainerLink = 1;
				packline1_1.OutturnQty = 3;
				packline1_1.Weight = 20;
				packline1_1.Volume = 5;
				packline1_1.LoadingMeters = 35;
				packline1_1.UnloadDate = ZDateTime.Today;

				var packline1_2 = CreatePackline();
				packline1_2.ReferenceNumber = "PKG2";
				packline1_2.PackingLineID = savedPackLineId;
				packline1_2.ContainerLink = 1;
				packline1_2.OutturnQty = 3;
				packline1_2.Weight = 30;
				packline1_2.Volume = 105;
				packline1_2.LoadingMeters = 125;
				packline1_2.UnloadDate = ZDateTime.Today;

				var packline2 = CreatePackline();
				packline2.ReferenceNumber = "PKG3";
				packline2.PackingLineID = savedPackLineId;
				packline2.ContainerLink = 2;
				packline2.OutturnQty = 2;
				packline2.Weight = 11;
				packline2.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1_1);
				packlines.Add(packline1_2);
				packlines.Add(packline2);

				shipmentDataObject.SetPackingLineCollection(() => packlines);
				AddEmptyTRUReferences(packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("2 packlines on shipment", 2, shipment.OuterPackLines.Count);
				var resPacklineBO1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(0);
				var resPacklineBO2 = shipment.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(1);

				AssertEquals("Packline1 has 2 packages", 2, resPacklineBO1.PkgPackageCollection.Count);
				AssertEquals("Packline2 has 1 package", 1, resPacklineBO2.PkgPackageCollection.Count);

				AssertEquals("Packline1 has discrepencies", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, resPacklineBO1.JL_OriginTransitWarehouseStatus);
				AssertEquals("Packline2 (new packline) is confirmed", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, resPacklineBO2.JL_OriginTransitWarehouseStatus);

				CombineAssertions("packline1 properties are synced with the new package", () =>
				{
					AssertEquals((decimal)50, resPacklineBO1.JL_ActualWeight);
					AssertEquals((decimal)110, resPacklineBO1.JL_ActualVolume);
					AssertEquals((decimal)0, resPacklineBO1.JL_LoadingMeters);
					AssertEquals(2, resPacklineBO1.JL_PackageCount);
				});

				CombineAssertions("packine2 (new packline) has correct properties", () =>
				{
					AssertEquals((decimal)11, resPacklineBO2.JL_ActualWeight);
				});

				AssertEquals("Packline1 is linked to container1", container1.PK, resPacklineBO1.ConsolOrShipmentContainer.PK);
				AssertEquals("Packline2 is linked to container2", "CONT0002", resPacklineBO2.JL_Calc_ContainerNum);

				AssertEquals("RefNumber is copied onto the split packline", "RefNumber1", resPacklineBO2.JL_RefNumber);
			}
		}

		#endregion

		[TestDate(2021, 1, 1)]
		public void TestTWReceipt_MultipleCFS()
		{
			var date1 = new ZDateTime(2021, 1, 1);

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO1 = Factory.NewWithValidTestData<OrgAddress>();
				var warehouseAddressBO2 = Factory.NewWithValidTestData<OrgAddress>();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				PopulatePackLine(packlineBO1, contact);

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO1.PK;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "CONT0001";
				packlineBO1.JL_JC = container1.PK;
				packlineBO1.JL_RefNumber = "RefNumber1";

				var consol2 = existingShipment.Consols.AddNew();
				consol2.JK_OA_PackDepotAddress = warehouseAddressBO2.PK;
				Factory.SaveForTesting();

				AssertEquals("Precondition: 1 packline on shipment.", 1, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataObject = CreateShipmentDO(warehouseAddressBO1);

					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					var savedPackLineId = packlineBO1.JL_PackLineId;

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
						CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG1", savedPackLineId, 1, date1)),
						CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG2", savedPackLineId, 2, date1)),
						CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG3", savedPackLineId, 2, date1))
					}));
					SetPacklineAggregates(shipmentDataObject.PackingLineCollection[0], 1, 2, 1);
					SetPacklineAggregates(shipmentDataObject.PackingLineCollection[1], 2, 4, 1);
					SetPacklineAggregates(shipmentDataObject.PackingLineCollection[2], 4, 1, 1);
					SetPacklineUNDG(shipmentDataObject.PackingLineCollection[0].UNDGCollection.First(), 1, 2, 4);
					SetPacklineUNDG(shipmentDataObject.PackingLineCollection[1].UNDGCollection.First(), 2, 4, 1);
					SetPacklineUNDG(shipmentDataObject.PackingLineCollection[2].UNDGCollection.First(), 4, 1, 2);
					AddEmptyTRUReferences(shipmentDataObject.PackingLineCollection);

					LinkShipmentDOToConsol(shipmentDataObject, consol);
					shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
					shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.False, date1));
					ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", shipmentDataObject.PackingLineCollection);
					var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertEquals("2 packlines on shipment", 2, shipment.OuterPackLines.Count);
					AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
						warehouseAddressBO1.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
						new ZString[] { "PKG1" });
					AssertPackLineReceiptInformations(shipment.OuterPackLines[1],
						warehouseAddressBO1.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
						new ZString[] { "PKG2", "PKG3" });

					AssertPacklineAggregates(shipment.OuterPackLines[0], 1, 2, 1);
					AssertPacklineAggregates(shipment.OuterPackLines[1], 6, 5, 2);
					AssertPacklineUNDG(shipment.OuterPackLines[0].UNDGs[0], 1, 2, 4);
					AssertPacklineUNDG(shipment.OuterPackLines[1].UNDGs[0], 6, 5, 3);
				}

				var date2 = date1.AddDays(1);

				existingShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
				existingShipment.OuterPackLines[1].JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
				existingShipment.OuterPackLines[1].JL_LastKnownTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;

				using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipmentDataObject = CreateShipmentDO(warehouseAddressBO2);
					LinkShipmentDOToConsol(shipmentDataObject, consol2);

					var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } };
					shipmentDataObject.SetContainerCollection(() => containers);

					var savedPackLineId = packlineBO1.JL_PackLineId;

					shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] {
						CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG2", savedPackLineId, 2, date2)),
					}));
					SetPacklineAggregates(shipmentDataObject.PackingLineCollection[0], 8, 16, 1);
					SetPacklineUNDG(shipmentDataObject.PackingLineCollection[0].UNDGCollection.First(), 8, 16, 32);
					AddEmptyTRUReferences(shipmentDataObject.PackingLineCollection);

					AssertEquals("pre: two packages on packline 2", 2, Factory.Load<ForwardingShipment>(existingShipment.PK).OuterPackLines[1].PkgPackageCollection.Count);

					shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
					shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU002", DateType.Unpack, ZBool.False, date2));

					shipmentDataObject.PackingLineCollection[0].SetReferenceNumberCollection(() => new List<Reference> { new Reference { ReferenceNumber = "TRU002", Type = new EntryType { Code = WorkflowDescriptors.TransitReceiveTransportationUnit } } });

					var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
					Factory.SaveForTesting();

					// note: PKG3 was split off from the packline, since it was not received from the new CFS
					AssertEquals("3 packlines on shipment - PKG2 was transferred from another CFS", 3, shipment.OuterPackLines.Count);
					AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
						warehouseAddressBO1.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
						new ZString[] { "PKG1" });
					AssertPackLineReceiptInformations(shipment.OuterPackLines[1],
						warehouseAddressBO1.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
						new ZString[] { "PKG3" });
					AssertPackLineReceiptInformations(shipment.OuterPackLines[2],
						warehouseAddressBO2.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date2,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
						new ZString[] { "PKG2" });

					AssertPacklineAggregates(shipment.OuterPackLines[0], 1, 2, 1);
					AssertPacklineAggregates(shipment.OuterPackLines[1], 4, 1, 1);
					AssertPacklineAggregates(shipment.OuterPackLines[2], 8, 16, 1);
					AssertPacklineUNDG(shipment.OuterPackLines[0].UNDGs[0], 1, 2, 4);
					AssertPacklineUNDG(shipment.OuterPackLines[1].UNDGs[0], 4, 1, 2);
					AssertPacklineUNDG(shipment.OuterPackLines[2].UNDGs[0], 8, 16, 32);

					var expectedContainer = consol2.Containers.Cast<ForwardingContainer>().First(c => c.JC_ContainerNum == "CONT0002");
					AssertEquals(1, shipment.OuterPackLines[2].Containers.Count);
					AssertEquals(expectedContainer.PK, shipment.OuterPackLines[2].GetContainer(consol2).PK);
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestTWReceipt_DuplicatePackagesUpdateLastKnownTWDateEveryTime()
		{
			var date1 = new ZDateTime(2021, 1, 1);

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var (contact, _) = CreateContactAndSubs();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = WarehouseAddressBO.PK;

				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO1, contact);
				packlineBO1.JL_PackLineId = "TEST001";

				Factory.SaveForTesting();
				AssertEquals("Precondition: 1 packline on shipment.", 1, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				var shipmentDataObject = CreateShipmentDO(WarehouseAddressBO);

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0001" }, new Container() { Link = 2, ContainerNumber = "CONT0002" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var packlines = new DataObjectList<PackingLine>();
				shipmentDataObject.SetPackingLineCollection(() => packlines);

				shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TD00000058", DateType.Unpack, ZBool.False, date1));

				var packline = CreatePackline();
				packline.ReferenceNumber = "PKG1";
				packline.PackingLineID = packlineBO1.JL_PackLineId;
				packline.OutturnQty = 1;
				packline.ContainerLink = 1;
				packline.UnloadDate = date1;
				packline.SetReferenceNumberCollection(() => new List<Reference>() { new Reference { Type = new EntryType { Code = "TRU" }, ReferenceNumber = "TD00000058" } });
				packlines.Add(packline);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals("1 packlines on shipment", 1, shipment.OuterPackLines.Count);
				AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
					WarehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					date1,
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1" });

				var date2 = date1.AddDays(1);

				packline.UnloadDate = date2;
				SetPacklineUNDG(shipmentDataObject.PackingLineCollection[0].UNDGCollection.First(), 8, 16, 32);
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TD00000058", DateType.Unpack, ZBool.False, date2));

				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("1 packlines on shipment", 1, shipment.OuterPackLines.Count);
				AssertPackLineReceiptInformations(shipment.OuterPackLines[0],
					WarehouseAddressBO.PK,
					FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
					date2,
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
					new ZString[] { "PKG1" });
			}
		}

		public void TestTWReceipt_PackingCountDiscrepancy()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var (contact, _) = CreateContactAndSubs();

				var warehouseAddressBO1 = Factory.NewWithValidTestData<OrgAddress>();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";
				PopulatePackLine(packlineBO1, contact);

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO1.PK;
				Factory.SaveForTesting();

				AssertEquals("Precondition: 1 packline on shipment.", 1, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO1);

				LinkShipmentDOToConsol(shipmentDataObject, consol);

				void ResetPacklineBO()
				{
					packlineBO1.PkgPackageCollection.DeleteAll();
					packlineBO1.JL_ActualVolume = 0;
					packlineBO1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown;
				}

				void SetNumberOfPackagesOnDO(int numPackages)
				{
					var collection = new DataObjectList<PackingLine>();
					for (var i = 0; i < numPackages; ++i)
					{
						var packline = CreatePacklineForReceive(new PacklineDOOverridesForTest("PKG" + i, packlineBO1.JL_PackLineId, 1));
						collection.Add(packline);
					}
					shipmentDataObject.SetPackingLineCollection(() => collection);
					AddEmptyTRUReferences(shipmentDataObject.PackingLineCollection);
				}

				packlineBO1.JL_PackageCount = 7;

				SetNumberOfPackagesOnDO(5);
				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("1 packlines on shipment", 1, shipment.OuterPackLines.Count);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);

				ResetPacklineBO();
				SetNumberOfPackagesOnDO(9);
				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("1 packlines on shipment", 1, shipment.OuterPackLines.Count);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);

				ResetPacklineBO();
				SetNumberOfPackagesOnDO(7);
				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				AssertEquals("1 packlines on shipment", 1, shipment.OuterPackLines.Count);
				AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
			}
		}

		(OrgContact contact, UNDGSubstance subs) CreateContactAndSubs()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			contact.OC_ContactName = "Steven";
			contact.OC_Phone = "88888";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Class = "8";
			subs.DG_UNNO = "8888";
			subs.DG_Code = "8888C";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_UniqueRecordId = "8888C";

			return (contact, subs);
		}

		static bool IsTWReceive(UniversalShipment shipmentDataObject)
		{
			return shipmentDataObject.DataContext?.DataSourceCollection?.Any(s => (s?.Type.GetValueOrDefault() ?? ZString.Empty) == nameof(DataContextType.TransitReceive)) ?? false;
		}

		static internal void LinkShipmentDOToConsol(UniversalShipment shipmentDataObject, CommonConsol consol)
		{
			if (IsTWReceive(shipmentDataObject))
			{
				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType { Code = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber },
						ReferenceNumber = consol.JK_UniqueConsignRef
					}
				});
				AddEmptyTRUReferences(shipmentDataObject.PackingLineCollection);
			}
			else
			{
				var header = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				header.DataContext = new DataContext();
				header.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TD00000054");
				header.VesselName = "CONT0001";
				header.SetContainerCollection(() => new DataObjectList<Container>() { });

				var loadList = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				loadList.DataContext = new DataContext();
				loadList.DataContext.AddDataSource(DataContextType.TransitDispatchLoadList, "DLL00000011");
				loadList.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = consol.JK_UniqueConsignRef },
					new AdditionalReference { Type = new EntryType { Code = "FCO" }, ReferenceNumber = "DUMMY" }
				});
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { header, loadList });

				AddEmptyTDUReferences(shipmentDataObject.PackingLineCollection);
			}
		}

		void AssertPackLineReceiptInformations(ForwardingPackLine packLine, ZGuid warehouseAddressPK, ZString lastKnownTransitWarehouseStatus, ZDateTime lastKnownTransitWarehouseStatusDateTime, ZString originTransitWarehouseStatus, ZString[] referenceNumbers)
		{
			AssertArrayEqualsByElements(referenceNumbers, packLine.PkgPackageCollection.Select(p => p.KP_PackageID).ToArray());
			AssertEquals("JL_OA_LastKnownTransitWarehouseAddress", warehouseAddressPK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
			AssertEquals("JL_OriginTransitWarehouseStatus", originTransitWarehouseStatus, packLine.JL_OriginTransitWarehouseStatus);
			AssertEquals("JL_LastKnownTransitWarehouseStatus", lastKnownTransitWarehouseStatus, packLine.JL_LastKnownTransitWarehouseStatus);
			AssertEquals("JL_LastKnownTransitWarehouseStatusDateTime", lastKnownTransitWarehouseStatusDateTime, packLine.JL_LastKnownTransitWarehouseStatusDateTime);
		}

		internal class PacklineDOOverridesForTest
		{
			public PacklineDOOverridesForTest(ZString? referenceNumber, ZString? packingLineID, ZInt? containerLink = null, ZDateTime? date = null)
			{
				ReferenceNumber = referenceNumber;
				PackingLineID = packingLineID;
				ContainerLink = containerLink;
				Date = date;
			}

			public void Apply(PackingLine packline)
			{
				packline.ReferenceNumber = ReferenceNumber;
				packline.PackingLineID = PackingLineID;
				packline.ContainerLink = ContainerLink;
				packline.OutturnQty = OutturnQty;
				packline.PackQty = PackQty;
			}

			ZString? ReferenceNumber { get; set; }
			ZString? PackingLineID { get; set; }
			ZInt? ContainerLink { get; set; }
			public ZInt? OutturnQty { get; set; } = 1;
			public long PackQty { get; set; } = 1;
			public ZDateTime? Date { get; set; }
		}

		public void TestTWMergeSplit_UseExistingPacklineWithDifferentContainer()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				packlineBO1.JL_PackLineId = "TEST001";

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "CONT0001";
				packlineBO1.SetContainer(container1.PK);
				AssertEquals("pre: Packline1 is linked to container1", "CONT0001", packlineBO1.JL_Calc_ContainerNum);

				Factory.SaveForTesting();

				AssertEquals("Precondition: 1 packline on shipment.", 1, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				var shipmentDataObject = CreateShipmentDO(warehouseAddressBO);
				LinkShipmentDOToConsol(shipmentDataObject, consol);

				var containers = new DataObjectList<Container>() { new Container() { Link = 1, ContainerNumber = "CONT0002" } };
				shipmentDataObject.SetContainerCollection(() => containers);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";
				packline1.PackingLineID = packlineBO1.JL_PackLineId;
				packline1.ContainerLink = 1;
				packline1.OutturnQty = 3;
				packline1.Weight = 20;
				packline1.Volume = 5;
				packline1.LoadingMeters = 35;
				packline1.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("1 packline on shipment", 1, shipment.OuterPackLines.Count);
				var resPacklineBO1 = shipment.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(0);

				AssertEquals("Packline1 has 1 package", 1, resPacklineBO1.PkgPackageCollection.Count);
				AssertEquals("Packline1 is linked to container2", "CONT0002", resPacklineBO1.JL_Calc_ContainerNum);
			}
		}

		internal static UniversalShipment CreateShipmentDO(OrgAddress warehouseAddressBO, DataContextType contextType = DataContextType.TransitReceive)
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(contextType, "TR00001");
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { });

			var addresses = new List<OrganizationAddress>();
			addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code, Port = new UNLOCO { Code = "ABCDE" } });
			shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

			return shipmentDataObject;
		}

		#region TestTWPackagesLinkedToPackLine_ExistingPackline_ReceivedWithDiscrepencies

		[TestDate(2021, 1, 1)]
		public void TestTWPackagesLinkedToPackLine_ExistingPackline_ReceivedWithDiscrepencies()
		{
			var date1 = new ZDateTime(2021, 1, 1);

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Monday";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_Class = "8";
				subs.DG_UNNO = "8888";
				subs.DG_Code = "8888C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_UniqueRecordId = "8888C";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO1, contact);

				packlineBO1.JL_PackLineId = "TEST001";

				var packlineBO2 = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO2, contact);
				packlineBO2.JL_PackLineId = "TEST002";

				var packlineBO3 = existingShipment.OuterPackLines.AddNew();
				packlineBO3.JL_DepartureTransitWarehouseExcluded = true;
				packlineBO3.JL_PackLineId = "TEST003";

				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				AssertEquals("Precondition: 2 packline on shipment.", 3, existingShipment.OuterPackLines.Count);
				Assert("Precondition: no package should exist", existingShipment.OuterPackLines.Cast<ForwardingPackLine>().All(p => p.PkgPackageCollection.Count == 0));

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.PackingLineID = packlineBO1.JL_PackLineId;
				packline1.ReferenceNumber = "PKG1";
				packline1.OutturnQty = 1;
				packline1.UnloadDate = ZDateTime.Today;

				var packline2 = CreatePackline();
				packline2.PackingLineID = packlineBO2.JL_PackLineId;
				packline2.ReferenceNumber = "PKG2";
				packline2.PackType = new PackageType() { Code = "AAA" };
				packline2.OutturnQty = 1;
				packline2.UnloadDate = ZDateTime.Today;

				var packline3 = CreatePackline();
				packline3.PackingLineID = "TEST004";
				packline3.ReferenceNumber = "PKG3";
				packline3.OutturnQty = 1;
				packline3.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				packlines.Add(packline2);
				packlines.Add(packline3);
				AddEmptyTRUReferences(packlines);

				shipmentDataObject.SetPackingLineCollection(() => packlines);
				shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
				ShipmentDataObjectReaderForDispatchTest.FillTransitTransportationUnit(WorkflowDescriptors.TransitReceiveTransportationUnit, "TRU001", shipmentDataObject.PackingLineCollection);
				shipmentDataObject.RelatedShipmentCollection.Add(CreateRelatedShipmentCollectionWithLastKnownTWUpdatedDate(DataContextType.TransitReceiveHeader, "TRU001", DateType.Unpack, ZBool.False, date1));
				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("4 packlines on shipment", 4, shipment.OuterPackLines.Count);
				AssertEquals("3 packlines should have 1 package", 3, shipment.OuterPackLines.Cast<ForwardingPackLine>().Count(p => p.PkgPackageCollection.Count == 1));

				var surplusPackLines = shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus);
				AssertEquals("There should be 1 packages with Surplus status", 1, surplusPackLines.Count());
				AssertPackLineReceiptInformations(surplusPackLines.First(),
						warehouseAddressBO.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
						new ZString[] { "PKG3" });

				var unknown = shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown);
				AssertEquals("There should be 1 packages with Unknown status", 1, unknown.Count());

				var discrepencies = shipment.OuterPackLines.Cast<ForwardingPackLine>().Where(p => p.JL_OriginTransitWarehouseStatus == FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies);
				AssertEquals("There should be 1 packages with Discrepencies status", 1, discrepencies.Count());
				AssertPackLineReceiptInformations(discrepencies.First(),
						warehouseAddressBO.PK,
						FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received,
						date1,
						FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
						new ZString[] { "PKG2" });

				packlines = new DataObjectList<PackingLine>();
				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);

				var date2 = date1.AddDays(1);
				TestDateAttribute.AddDays(1);

				shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertEquals("the surplus packline is not deleted", 4, shipment.OuterPackLines.Count);
			}
		}

		#endregion

		public void TestTW_NoContainersButDifferentContainerPackingOrder_IsMarkedAsReceived()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = Factory.New<OrgContact>();
				contact.OC_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				contact.OC_ContactName = "Steven";
				contact.OC_Phone = "88888";

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				var packlineBO1 = existingShipment.OuterPackLines.AddNew();
				PopulatePackLine(packlineBO1, contact);
				packlineBO1.JL_PackLineId = "TEST001";
				packlineBO1.JL_ContainerPackingOrder = 8;
				packlineBO1.UNDGs.DeleteAll();
				var consol = existingShipment.Consols.AddNew();
				consol.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "TR00001");
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";
				shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress() { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.PackingLineID = packlineBO1.JL_PackLineId;
				packline1.ContainerPackingOrder = 5;
				packline1.OutturnQty = 1;
				packline1.SetUNDGCollection(() => new List<UNDG>());
				packline1.ReferenceNumber = "PKG1";
				packline1.UnloadDate = ZDateTime.Today;

				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);

				AssertNotEquals("Precondition: packing order is different on TW and existing shipment", packlineBO1.JL_ContainerPackingOrder, packline1.ContainerPackingOrder);
				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertEquals("1 packlines on shipment", 1, shipment.OuterPackLines.Count);
				AssertEquals("Should be confirmed", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, shipment.OuterPackLines.Cast<ForwardingPackLine>().ElementAt(0).JL_OriginTransitWarehouseStatus);
			}
		}

		void AssertPackLine(PackLine packline)
		{
			AssertEquals("CM1", packline.JL_RH_NKCommodityCode);
			AssertEquals(1, packline.JL_ContainerPackingOrder);
			AssertEquals("HAR", packline.JL_HarmonisedCode);
			AssertEquals((short)2, packline.JL_ItemNo);
			AssertEquals("MARKS1", packline.JL_MarksAndNumbers);
			AssertEquals("US", packline.JL_RN_NKOrigin);
			AssertEquals("PLT", packline.JL_F3_NKPackType);
			AssertEquals("EXP0001", packline.JL_ExportRefNumber);
			AssertEquals("IMP0001", packline.JL_ImportRefNumber);
			AssertEquals("Goods", packline.JL_Description);
			AssertEquals((short)3, packline.JL_EndItemNo);
			AssertEquals((decimal)22, packline.JL_LinePrice);
			AssertEquals("detailed goods", packline.JL_DetailedDescription);
			AssertEquals((decimal)11, packline.JL_Length);
			AssertEquals((decimal)23, packline.JL_Width);
			AssertEquals((decimal)56, packline.JL_Height);
			AssertEquals("M", packline.JL_Calc_WidthUnit);
			AssertEquals("KG", packline.PackLineWeightUnit);
			AssertEquals("M3", packline.PackLineVolumeUnit);
			AssertEquals(true, packline.JL_RequiresTemperatureControl);
			AssertEquals((decimal)10, packline.JL_RequiredTemperatureMinimum);
			AssertEquals((decimal)20, packline.JL_RequiredTemperatureMaximum);
			AssertEquals(Core.Constants.Temperature.Centigrade, packline.JL_RequiredTemperatureUnit);
			AssertEquals(1, packline.HarmonisedCodes.Count);
			AssertEquals("Sunny", packline.JL_CustomAttrib1);
			AssertEquals(1, packline.UNDGs.Count);

			var mergedDGItem = packline.UNDGs.First();
			AssertEquals("8888C", mergedDGItem.SubstanceCode);
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, mergedDGItem.Substance.DG_Standard);
			AssertEquals((decimal)2.5, mergedDGItem.DI_DGFlashPoint);
			AssertEquals("N", mergedDGItem.DI_MPMarinePollutant);
			AssertEquals("tehnics", mergedDGItem.DI_TechnicalName);
			AssertEquals((decimal)21, mergedDGItem.DI_DGWeight);
			AssertEquals("KG", mergedDGItem.DI_UnitOfWeight);
			AssertEquals((decimal)24, mergedDGItem.DI_DGVolume);
			AssertEquals("M3", mergedDGItem.DI_UnitOfVolume);
			AssertEquals(15, mergedDGItem.DI_PackageCount);
			AssertEquals("PLT", mergedDGItem.DI_F3_NKPackType);
		}

		void SetPacklineAggregates(PackingLine packline, ZDecimal weight, ZDecimal volume, ZLong packageCount)
		{
			packline.Weight = weight;
			packline.Volume = volume;
			packline.PackQty = packageCount;
		}

		void AssertPacklineAggregates(PackLine packline, ZDecimal weight, ZDecimal volume, ZInt packageCount)
		{
			AssertEquals("weight", (decimal)weight, packline.JL_ActualWeight);
			AssertEquals("volume", (decimal)volume, packline.JL_ActualVolume);
			AssertEquals("weight", packageCount, packline.JL_PackageCount);
		}

		void SetPacklineUNDG(UNDG undg, ZDecimal weight, ZDecimal volume, ZInt packQty)
		{
			undg.Weight = weight;
			undg.Volume = volume;
			undg.PackQty = packQty;
		}

		void AssertPacklineUNDG(UNDGDataItem undg, decimal dgWeight, decimal dgVolume, ZInt packageCount)
		{
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, undg.Substance.DG_Standard);
			AssertEquals((decimal)2.5, undg.DI_DGFlashPoint);
			AssertEquals("N", undg.DI_MPMarinePollutant);
			AssertEquals("tehnics", undg.DI_TechnicalName);
			AssertEquals(dgWeight, undg.DI_DGWeight);
			AssertEquals("KG", undg.DI_UnitOfWeight);
			AssertEquals(dgVolume, undg.DI_DGVolume);
			AssertEquals("M3", undg.DI_UnitOfVolume);
			AssertEquals(packageCount, undg.DI_PackageCount);
			AssertEquals("PLT", undg.DI_F3_NKPackType);
		}

		void PopulatePackLine(ForwardingPackLine packline, OrgContact contact)
		{
			packline.JL_RH_NKCommodityCode = "CM1";
			packline.JL_ContainerPackingOrder = 1;
			packline.JL_HarmonisedCode = "HAR";
			packline.JL_MarksAndNumbers = "MARKS1";
			packline.JL_RN_NKOrigin = "US";
			packline.JL_UnitOfDimension = "M";
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_Length = 11;
			packline.JL_Width = 23;
			packline.JL_Height = 56;
			packline.JL_ActualVolumeUQ = "M3";
			packline.JL_ActualWeightUQ = "KG";
			packline.JL_Description = "Goods";
			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMinimum = 10;
			packline.JL_RequiredTemperatureMaximum = 20;
			packline.JL_RequiredTemperatureUnit = "C";
			packline.JL_PackageCount = 1;
			packline.JL_ActualVolume = 0;

			var undgItem = packline.UNDGs.AddNew();
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "8888", "C", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "8888";
				subs.DG_Variant = "C";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			undgItem.LinkDefault(subs);
			undgItem.DI_DGFlashPoint = 2.5;
			// note: pkgpackage reader reads code OR IMOClass currently - this is a workaround
			// see C:\git\wtg\CargoWise\Dev\Enterprise\Product\Operations\MasterFiles\MasterFiles.DataTransfer\MasterFiles.DataTransfer\Universal\Reading\UNDGDataObjectReader.cs
			undgItem.DI_IMOClass = "";
			undgItem.DI_MPMarinePollutant = "N";
			undgItem.DI_TechnicalName = "tehnics";
			undgItem.DI_UnitOfWeight = "KG";
			undgItem.DI_UnitOfVolume = "M3";
			undgItem.DI_F3_NKPackType = "PLT";
			undgItem.DI_IsLimitedQuantity = true;
			undgItem.DI_OC_DGContact = contact.PK;
		}

		internal static PackingLine CreatePacklineForReceive(PacklineDOOverridesForTest packlineDOOverrides = null)
		{
			var packline = CreatePackline(packlineDOOverrides);
			if (packline.OutturnQty.GetValueOrDefault() > 0)
			{
				packline.UnloadDate = packlineDOOverrides?.Date ?? ZDateTime.Today;
			}
			return packline;
		}

		internal static PackingLine CreatePacklineForDispatch(PacklineDOOverridesForTest packlineDOOverrides = null)
		{
			var packline = CreatePackline(packlineDOOverrides);
			if (packline.OutturnQty.GetValueOrDefault() > 0)
			{
				packline.LoadDate = packlineDOOverrides?.Date ?? ZDateTime.Today;
			}
			return packline;
		}

		internal static PackingLine CreatePackline(PacklineDOOverridesForTest packlineDOOverrides = null)
		{
			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.OutturnQty = 1;
			packline.Commodity = new Commodity() { Code = "CM1" };
			packline.ContainerPackingOrder = 1;
			packline.HarmonisedCode = "HAR";
			packline.ItemNo = 2;
			packline.MarksAndNos = "MARKS1";
			packline.CountryOfOrigin = new Country() { Code = "US" };
			packline.PackType = new PackageType() { Code = "PLT" };
			packline.ExportReferenceNumber = "EXP0001";
			packline.ImportReferenceNumber = "IMP0001";
			packline.GoodsDescription = "Goods";
			packline.EndItemNo = 3;
			packline.LinePrice = 22;
			packline.DetailedDescription = "detailed goods";
			packline.Length = 11;
			packline.Width = 23;
			packline.Height = 56;
			packline.LengthUnit = new UnitOfLength() { Code = "M" };
			packline.WeightUnit = new UnitOfWeight() { Code = "KG" };
			packline.VolumeUnit = new UnitOfVolume() { Code = "M3" };
			packline.RequiresTemperatureControl = true;
			packline.RequiredTemperatureMinimum = 10;
			packline.RequiredTemperatureMaximum = 20;
			packline.RequiredTemperatureUnit = new CodeDescriptionPair1Char { Code = Core.Constants.Temperature.Centigrade };
			packline.PackQty = 1;

			packline.SetClassificationCollection(() =>
			{
				return new DataObjectList<Classification>
				{
					new Classification() { Code = "HAA", Country = new Country() { Code = "AU" }, Type = new CodeDescriptionPair() { Code = FreightConstants.Classification.Codes.HarmonizedCode } }
				};
			});

			packline.SetCustomizedFieldCollection(() => new List<CustomizedField> { CustomizedField.New("Monday", new ZString("Sunny")) });

			var undgs = new List<UNDG>();
			undgs.Add(new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = "8888C",
				Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO,
				IMOClass = "",
				FlashPoint = "2.5",
				MarinePollutant = new UNDGMarinePollutant() { Code = "N" },
				PackedInLimitedQuantity = true,
				TechicalName = "tehnics",
				WeightUQ = new UnitOfWeight() { Code = "KG" },
				VolumeUQ = new UnitOfVolume() { Code = "M3" },
				PackType = new PackageType() { Code = "PLT" },
				Contact = new OrganizationContact() { FullName = "Steven", Phone = "88888" }
			});

			packline.SetUNDGCollection(() => undgs);

			if (packlineDOOverrides != null)
			{
				packlineDOOverrides.Apply(packline);
			}

			return packline;
		}

		public void TestLinkWarehouseOrder_RelationshipNotCreatedIfOrderLimitExceededOnShipment()
		{
			var shipment = Factory.BOFactory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "BACON PANCAKES";

			var order1 = shipment.AttachedOrders.AddNew();
			var order2 = shipment.AttachedOrders.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var whsOrder = (BusinessObject)Factory.BOFactory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			whsOrder["WD_BOLNo"] = "BACON PANCAKES";
			Factory.SaveForTesting();

			var docketID = ((IWhsOrder)whsOrder).WD_DocketID;
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseOrder, docketID);
			shipmentDataObject.WayBillNumber = "BACON PANCAKES";
			shipmentDataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };

			Logger.TopLevelDataObject = shipmentDataObject;
			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());

			using (FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				AssertExceptionThrown(typeof(DataObjectValidationException), @"S00001000:
The number of Orders on a Shipment is limited for performance and database management reasons to 2 Orders. Above 1 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.",
					() => new ShipmentDataObjectReader(shipmentDataObject, Logger, new UniversalObjectFactory(), null).ReadIntoBusinessObject());
			}
		}

		public void TestOrdersAreImported()
		{
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			AssertNotNull(shipment);
			AssertEquals(1, shipment.AttachedOrders.Count);
			var order = shipment.AttachedOrders[0];
			AssertEquals("ORDER ME", order.JD_OrderNumber);
			AssertEquals(new ZByte(2), order.JD_OrderNumberSplit);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		#region TestImportDocData

		public void TestImportDocData()
		{
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipmentDataObject = GetShipmentDataObject();
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			var docNote = DocumentNote.RetrieveNote(shipment);

			AssertNotNull(docNote);
			AssertEquals("A000123", docNote.GetSystemDefinedFieldValue("Letter of Credit Number"));
			AssertEquals("Aaaaaaaaaa", docNote.GetFieldValueAsString("Other Documents"));
		}

		public void TestDontImportDocData()
		{
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipmentDataObject = GetShipmentDataObject();
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			var docNote = DocumentNote.RetrieveNote(shipment);

			AssertNull(docNote);
		}

		UniversalShipment GetShipmentDataObject()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var docData = new DocumentData(DefaultDataObjectWriterStrategy.TestInstance);
			docData.SetSystemDefinedDataCollection(() => new List<SystemDefinedData>
			{
				new SystemDefinedData
				{
					Category = "COMMON",
					Name = "Letter of Credit Number",
					Value = "A000123"
				}
			});
			docData.SetUserDefinedDataCollection(() => new List<UserDefinedData>
			{
				new UserDefinedData
				{
					Name = "Other Documents",
					Value = "Aaaaaaaaaa"
				}
			});
			shipment.DocData = docData;
			return shipment;
		}

		public void TestImportDocData_NoExceptionThrown()
		{
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipmentDataObject = new UniversalShipment { DocData = new DocumentData(DefaultDataObjectWriterStrategy.TestInstance) };
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		#endregion

		public void TestWhenExistingOrdersAreAttachedAndShipmentNotInDatabase_OrderAttachingIsLogged()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var existingOrder = Factory.New<Order>();
			existingOrder.BuyerPK = buyer.PK;
			existingOrder.JD_OrderNumber = "ORDER ME";
			existingOrder.JD_OrderNumberSplit = new ZByte(2);

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.TotalWeight = 20m;
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			AssertNotNull(shipment);
			AssertEquals(1, shipment.AttachedOrders.Count);
			var order = shipment.AttachedOrders[0];
			AssertEquals(existingOrder, order);
			AssertContains("Order ORDER ME-2 has been attached to shipment.", Logger.Logs);
			AssertEquals("Order should not update Shipment when attaching if Shipment is not in database", 0m, shipment.JS_ActualWeight);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestWhenExistingOrdersAreAttachedAndShipmentInDatabase_ShipmentIsUpdatedAndOrderAttachingIsLogged()
		{
			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_HouseBill = "HOUSE";

			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var existingOrder = Factory.New<Order>();
			existingOrder.BuyerPK = buyer.PK;
			existingOrder.JD_OrderNumber = "ORDER ME";
			existingOrder.JD_OrderNumberSplit = new ZByte(2);

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.WayBillNumber = "HOUSE";
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.TotalWeight = 20m;
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			AssertNotNull(shipment);
			AssertEquals(matchingShipment, shipment);
			AssertEquals(1, shipment.AttachedOrders.Count);
			var order = shipment.AttachedOrders[0];
			AssertEquals(existingOrder, order);
			AssertContains("Order ORDER ME-2 has been attached to shipment.", Logger.Logs);
			AssertEquals("Order should update Shipment when attaching if Shipment is in database", 20m, shipment.JS_ActualWeight);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestWhenNewOrdersAreAttachedAndShipmentIsInDatabase_OrderAttachingIsLogged()
		{
			var matchingShipment = Factory.New<ForwardingShipment>();
			matchingShipment.JS_HouseBill = "HOUSE";

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.WayBillNumber = "HOUSE";

			var orderData = GetNewOrderDataObject();
			orderData.TotalWeight = 20m;
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			AssertNotNull(shipment);
			AssertEquals(matchingShipment, shipment);
			AssertEquals(1, shipment.AttachedOrders.Count);
			var order = shipment.AttachedOrders[0];
			AssertEquals("ORDER ME", order.JD_OrderNumber);
			AssertContains("Order ORDER ME-2 has been attached to shipment.", Logger.Logs);
			AssertEquals("Order should not update Shipment when Order is not in database", 0m, shipment.JS_ActualWeight);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestWhenExistingOrderIsAttachedToAnotherShipmentOrderGetsSkipped()
		{
			var buyer = Data.ConsigneeOrgCRAHOLSYD;
			var otherShipment = Factory.New<ForwardingShipment>();
			var existingOrder = otherShipment.AttachedOrders.AddNew();
			existingOrder.BuyerPK = buyer.PK;
			existingOrder.JD_OrderNumber = "ORDER ME";
			existingOrder.JD_OrderNumberSplit = new ZByte(2);

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder { OrderNumber = "ORDER ME", OrderNumberSplit = new ZByte(2) };
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();

			AssertNotNull(shipment);

			CombineAssertions(delegate
			{
				AssertEquals(0, shipment.AttachedOrders.Count);
				AssertNotContains("Order ORDER ME-2 has been attached to shipment.", Logger.Logs);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals("Cannot populate Order because:\r\nOrder 'ORDER ME-2' is attached to another Shipment. Order ignored.", Logger.GetWarnings());
			});
		}

		public void TestWhenDataContextHasNoDataTargetCollection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());

			var orderData = GetNewOrderDataObject();
			orderData.DataContext = DataContextFactory.New();

			AssertNull(orderData.DataContext.DataTargetCollection);
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);

			var helper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
			try
			{
				helper.PopulateAttachedOrders(shipment);
				helper.LinkWarehouseOrder(shipment);
			}
			catch (ArgumentNullException)
			{
				Fail("Order population should not fail from null DataTargetCollections");
			}
		}

		public void TestPartialAttributeOnAdditionalReferences()
		{
			var serviceTaskLog = CreateAndProcessUniversalShipment("UniversalShipment.xml");
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

			var shipmentBO = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, new ZString("S00001000"));
			AssertMultilineASCIIEquals("shipmentBO.AdditionalReferenceNumbers", @"
AMS - CE00001
".Trim(), string.Join("\r\n", shipmentBO.Numbers.Cast<CusEntryNumber>().Select(o => o.CE_EntryType + " - " + o.CE_EntryNum).OrderBy(o => o)));

			serviceTaskLog = CreateAndProcessUniversalShipment("UniversalShipmentWithAddRefsPartial.xml");
			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 2 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

			shipmentBO = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, new ZString("S00001000"));
			AssertMultilineASCIIEquals("shipmentBO.AdditionalReferenceNumbers", @"
AMS - CE00001
COC - CUSOFF01
UBR - UBAPPR01
".Trim(), string.Join("\r\n", shipmentBO.Numbers.Cast<CusEntryNumber>().Select(o => o.CE_EntryType + " - " + o.CE_EntryNum).OrderBy(o => o)));

			serviceTaskLog = CreateAndProcessUniversalShipment("UniversalShipmentWithAddRefsComplete.xml");
			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 2 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

			shipmentBO = new BusinessObjectFactory().LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, new ZString("S00001000"));
			AssertMultilineASCIIEquals("shipmentBO.AdditionalReferenceNumbers", @"
BKG - CARBRF01
UBR - UBAPPR01
".Trim(), string.Join("\r\n", shipmentBO.Numbers.Cast<CusEntryNumber>().Select(o => o.CE_EntryType + " - " + o.CE_EntryNum).OrderBy(o => o)));
		}

		public void TestImportShipmentWithKeyFails()
		{
			string fileName = "UniversalShipmentWithKey.xml";

			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Match couldn't be found for ForwardingShipment with Key S00001003
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentWithSameSubShipmentKey()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009902";
			Factory.SaveForTesting();

			var fileName = "UniversalShipmentWithSameSubShipmentKey.xml";

			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Updated Shipment S00009902 from UniversalShipment.
ERROR - Shipment (S00009902) cannot import itself as a sub shipment.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentWithCircularMasterSubShipmentRelationship()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			consol.Transports[0].JW_RL_NKDiscPort = "NZAKL";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009902";

			var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment.JS_UniqueConsignRef = "S00009903";
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;

			consol.Shipments.Add(shipment);
			consol.Shipments.Add(subShipment);

			Factory.SaveForTesting();

			var fileName = "UniversalShipmentWithCircularMasterSubShipmentRelationship.xml";

			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Updated Shipment S00009902 from UniversalShipment.
ERROR - Shipment (S00009902) cannot be imported as it would create a circular dependency between master and sub shipments.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentWithoutDataTargetAndWithCircularRelationship()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009904";
			shipment.JS_HouseBill = "HBL001122XX";

			Factory.SaveForTesting();

			var fileName = "UniversalShipmentWithoutDataTargetAndWithCircularRelationship.xml";

			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Warning - JobCosting element was ignored. To import JobCosting data the DataContext must contain a matching EnterpriseID and ServerID, and the Company Code must match a valid Company in this system. This can also be overridden by setting the CodesMappedToTarget element to true.
Updated Shipment S00009904 (House Bill='HBL001122XX') from UniversalShipment.
Added Shipment from UniversalShipment.
ERROR - Shipment () cannot be imported as it would create a circular dependency between master and sub shipments.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentRespectRegistryUpdateShipmentsDuringAutomaticImport()
		{
			string fileName = "UniversalShipment.xml";

			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Shipment S00001000 (House Bill='FRED235478923') wasn't updated because of registry settings 'eServices->Universal XML->Automatic Update on Import'.
Successfully saved, but nothing was reported as being updated.
".Trim(), serviceTaskLog.ToString());

			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Shipment S00001000 (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentRespectRegistryUpdateShipmentsDuringAutomaticImportOptionNotUpdateForAddNewShipment()
		{
			string fileName = "UniversalShipment.xml";

			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentRespectRegistryUpdateShipmentsDuringAutomaticImport_OptionUpdateForAddNewShipment()
		{
			string fileName = "UniversalShipment.xml";

			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var serviceTaskLog = CreateAndProcessUniversalShipment(fileName);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Shipment (House Bill='FRED235478923') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='FRED235478923') with 1 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestImportShipmentDoNotRespectRegistryUpdateConsolsRoutingInformationDuringAutomaticImport()
		{
			eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			string fileName = "UniversalShipmentWithOneLeg.xml";

			CreateAndProcessUniversalShipment(fileName);

			var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));
			AssertNotNull("Shipment wasn't created", shipment);
			AssertEquals("Transports was not created", 1, shipment.Transports.Count);

			var oldValue = shipment.Transports[0].JW_CarrierBookingReference;
			ZString newValue = "blabla";
			shipment.Transports[0].JW_CarrierBookingReference = newValue;
			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UniversalXMLUpdateConsolRoutingDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateAndProcessUniversalShipment(fileName);
			shipment = new BusinessObjectFactory().LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000"));
			AssertNotNull("Shipment wasn't created", shipment);
			AssertEquals("Transports was not created", 1, shipment.Transports.Count);
			AssertEquals("Transport Leg was updated", oldValue, shipment.Transports[0].JW_CarrierBookingReference);
		}

		public void TestExpectedExceptionThrowWhenImportShipment_EmptyDefaultBranch_WithLocalClient()
		{
			var jobBranchDefaultOrderRule = new JobBranchDefaultOrderRule()
			{
				DefaultToBlank = 1,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 0,
				DefaultToBranchOfOrganisation = 0,
				DefaultToLoginUserDefault = 0
			};

			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, jobBranchDefaultOrderRule))
			{
				var orgGenerator = new OrganisationTestHelper(Factory);

				var localClientDataObject = orgGenerator.CreateDataObject("MCLAREN", "BUTTON", "1234");
				localClientDataObject.AddressType = nameof(DocAddressType.LocalClient);
				localClientDataObject.AddressOverride = true;

				orgGenerator.CreateBusinessObject("MCLAREN", "BUTTON", "1234");

				Factory.SaveForTesting();

				shipmentDataObject.WayBillNumber = "HOUSEBILL";
				shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				shipmentDataObject.OrganizationAddressCollection.Add(localClientDataObject);

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

				AssertExceptionThrown(typeof(DataObjectReadFailureException)
				, "Unable to import shipment. Job Branch cannot be empty. Please check branch defaulting configuration."
				, delegate
				{ reader.ReadIntoBusinessObject(); });

				var shipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HOUSEBILL"));
				shipments[0].Job.Dispose();
			}
		}

		public void TestExpectedExceptionThrowWhenImportShipment_EmptyDefaultBranch_WithOverseaAgent()
		{
			var jobBranchDefaultOrderRule = new JobBranchDefaultOrderRule()
			{
				DefaultToBlank = 1,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 0,
				DefaultToBranchOfOrganisation = 0,
				DefaultToLoginUserDefault = 0
			};

			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, jobBranchDefaultOrderRule))
			{
				var orgGenerator = new OrganisationTestHelper(Factory);

				var overseaAgentDataObject = orgGenerator.CreateDataObject("MCLAREN", "BUTTON", "1234");
				overseaAgentDataObject.AddressType = nameof(DocAddressType.OverseasAgent);
				overseaAgentDataObject.AddressOverride = true;

				orgGenerator.CreateBusinessObject("MCLAREN", "BUTTON", "1234");

				Factory.SaveForTesting();

				shipmentDataObject.WayBillNumber = "HOUSEBILL";
				shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				shipmentDataObject.OrganizationAddressCollection.Add(overseaAgentDataObject);

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

				AssertExceptionThrown(typeof(DataObjectReadFailureException)
				, "Unable to import shipment. Job Branch cannot be empty. Please check branch defaulting configuration."
				, delegate
				{ reader.ReadIntoBusinessObject(); });

				var shipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HOUSEBILL"));
				shipments[0].Job.Dispose();
			}
		}

		public void TestClientComingInWhenNotEnoughInformationExistsToSaveTheJobHeaderFailsGracefully()
		{
			var clientAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalClient));
			var clientOrgAddress = new OrganisationDataObjectReader(clientAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			AssertEquals("Precondition: orgAddress.IsInDatabase", true, clientOrgAddress.IsInDatabase);

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dataSource = shipmentDataObject.DataContext = DataContextFactory.New();
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataSource.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipmentDataObject.WayBillNumber = "HOUSENOTSAVED";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House WayBill" };
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(clientAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, new UniversalObjectFactory(), null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException)
				, "Could not save Local Client Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the Local Client is saved on the Job Costing record."
				, delegate
				{ reader.ReadIntoBusinessObject(); });

			var shipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HOUSENOTSAVED"));
			AssertEquals("Shipments with a House Bill Number 'HOUSENOTSAVED'", 0, shipments.Length);
		}

		public void TestOverseasAgentComingInWhenNotEnoughInformationExistsToSaveTheJobHeaderFailsGracefully()
		{
			var overseasAgentAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.OverseasAgent));
			var overseasAgentOrgAddress = new OrganisationDataObjectReader(overseasAgentAddress, Logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();
			AssertEquals("Precondition: orgAddress.IsInDatabase", true, overseasAgentOrgAddress.IsInDatabase);

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dataSource = shipmentDataObject.DataContext = DataContextFactory.New();
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataSource.AddDataTarget(DataContextType.ForwardingShipment, null);

			shipmentDataObject.WayBillNumber = "HOUSENOTSAVED";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House WayBill" };
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(overseasAgentAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, new UniversalObjectFactory(), null);
			AssertExceptionThrown(typeof(DataObjectReadFailureException)
				, "Could not save Overseas Agent Organization - Must have an Origin, Destination and Transport Mode to be able to calculate a Department for a Job Costing record, and the Overseas Agent is saved on the Job Costing record."
				, delegate
				{ reader.ReadIntoBusinessObject(); });

			var shipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HOUSENOTSAVED"));
			AssertEquals("Shipments with a House Bill Number 'HOUSENOTSAVED'", 0, shipments.Length);
		}

		public void TestDeliveryLocalTransportAddress_OldKey()
		{
			var deliveryLocalTransportAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalCartageDeliverToAddress));
			var orgAddress = new OrganisationDataObjectReader(deliveryLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(deliveryLocalTransportAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.DocsAndCartage.DeliveryCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DocsAndCartage.DeliveryCartageCoAddr);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalCartageDeliverToAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'LocalCartageDeliverToAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestDeliveryLocalTransportAddress()
		{
			var deliveryLocalTransportAddressOld = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartageDeliverToAddress));
			var orgAddressOld = new OrganisationDataObjectReader(deliveryLocalTransportAddressOld, Logger, Factory).GetMatchedOrNewForTesting();
			var deliveryLocalTransportAddress = GetNewAddressData_INTHEMSYD(AddressTypes.DeliveryLocalCartage);
			var orgAddress = new OrganisationDataObjectReader(deliveryLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(deliveryLocalTransportAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.DocsAndCartage.DeliveryCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DocsAndCartage.DeliveryCartageCoAddr);
				AssertNotEquals(orgAddressOld.PK, shipmentBO.DocsAndCartage.DeliveryCartageCoAddr.PK);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalCartageDeliverToAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Warning - Matching 'DeliveryLocalCartage':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'DeliveryLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestDeliveryCFSAddress()
		{
			var deliveryCFSAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ArrivalCFSAddress));
			var orgAddress = new OrganisationDataObjectReader(deliveryCFSAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(deliveryCFSAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.ImportReleaseDepot);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ImportReleaseDepot);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ArrivalCFSAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'ArrivalCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestPickupLocalTransportAddress_OldKey()
		{
			var pickupLocalTransportAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.LocalCartagePickupFromAddress));
			var orgAddress = new OrganisationDataObjectReader(pickupLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupLocalTransportAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalCartagePickupFromAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'LocalCartagePickupFromAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestPickupLocalTransportAddress()
		{
			var pickupLocalTransportAddressOld = GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.LocalCartagePickupFromAddress));
			var orgAddressOld = new OrganisationDataObjectReader(pickupLocalTransportAddressOld, Logger, Factory).GetMatchedOrNewForTesting();
			var pickupLocalTransportAddress = GetNewAddressData_INTHEMSYD(AddressTypes.PickupLocalCartage);
			var orgAddress = new OrganisationDataObjectReader(pickupLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupLocalTransportAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.DocsAndCartage.PickupCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DocsAndCartage.PickupCartageCoAddr);
				AssertNotEquals(orgAddressOld.PK, shipmentBO.DocsAndCartage.PickupCartageCoAddr.PK);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalCartagePickupFromAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Warning - Matching 'PickupLocalCartage':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'PickupLocalCartage':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestCTStatus()
		{
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_CommunityTransitStatus = "C";
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPK = shipmentBOToLoad.PK;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House WayBill" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("JS_CommunityTransitStatus should have been updated", "C", shipmentBOToLoad.JS_CommunityTransitStatus);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestPickupLocalTransportAddress_ForWarehouseOrder()
		{
			var pickupLocalTransportAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.TransportCompanyDocumentaryAddress));
			var orgAddress = new OrganisationDataObjectReader(pickupLocalTransportAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupLocalTransportAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNull("Should not import Pickup Cartage Transport Company if not from a warehouse order", shipmentBO.DocsAndCartage.PickupCartageCoAddr);

			Logger.ClearLogs();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseOrder, null);
			var newReader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var newShipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(newShipmentBO);
			AssertNotNull(newShipmentBO.DocsAndCartage.PickupCartageCoAddr);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(newShipmentBO.DocsAndCartage.PickupCartageCoAddr);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'TransportCompanyDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Updated Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestDeliveryAgentAddress()
		{
			var deliveryAgentAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DeliveryAgent));
			var orgAddress = new OrganisationDataObjectReader(deliveryAgentAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(deliveryAgentAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.DeliveryAgent);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.DeliveryAgent.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'DeliveryAgent':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'DeliveryAgent':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestPickupCFSAddress()
		{
			var pickupCFSAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DepartureCFSAddress));
			var orgAddress = new OrganisationDataObjectReader(pickupCFSAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupCFSAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.ExportReceivingDepot);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ExportReceivingDepot);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'DepartureCFSAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'DepartureCFSAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestPickupCFSAddressWithConsignorPickupDeliveryAddress()
		{
			var consignorAddress = GetNewAddressData_INTHEMSYD(DocAddressType.ConsignorDocumentaryAddress);
			var consignorOrgAddress = new OrganisationDataObjectReader(consignorAddress, Logger, Factory).GetMatchedOrNewForTesting();

			var consignorPickupDeliveryAddress = GetUnmatchOrganizationAddress(DocAddressType.ConsignorPickupDeliveryAddress, "INTHEMSYD", "ThereVille", "OfBliss", "1233");

			var pickupCFSAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.DepartureCFSAddress);
			var picOrgAddress = new OrganisationDataObjectReader(pickupCFSAddress, Logger, Factory).GetMatchedOrNewForTesting();
			picOrgAddress.SettingDefaults = false;

			var consignorOrg = Factory.Load<OrgHeader>(consignorOrgAddress.OA_OH);
			var cfsOrg = Factory.Load<OrgHeader>(picOrgAddress.OA_OH);
			consignorOrg.AddRelatedParty(cfsOrg.PK, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, TransportModes.All, ZString.Empty, null);

			var defaultOrgAddress = cfsOrg.Addresses.AddNew(OrgAddressType.Pickup, true);
			defaultOrgAddress.CompanyName = "Default Company";
			defaultOrgAddress.Address1 = "Default Address1";

			Factory.SaveForTesting();

			AssertEquals(expected: defaultOrgAddress.PK, actual: cfsOrg.GetAddressWithFallback(AddressType.PIC).PK);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(consignorAddress);
			shipmentDataObject.OrganizationAddressCollection.Add(consignorPickupDeliveryAddress);
			shipmentDataObject.OrganizationAddressCollection.Add(pickupCFSAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_CRAHOLSYD(shipmentBO.ExportReceivingDepot);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'DepartureCFSAddress':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Matching 'DepartureCFSAddress':- Matched to 'CRAHOLSYD' by code, address '' with a score of 710.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestBookedShippingLineAddress()
		{
			var bookedShippingLineAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ShippingLineAddress));
			var orgAddress = new OrganisationDataObjectReader(bookedShippingLineAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(bookedShippingLineAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.BookedShippingLineAddress);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.BookedShippingLineAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ShippingLineAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'ShippingLineAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestCreditorAddress()
		{
			var creditorAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.Creditor));
			var orgAddress = new OrganisationDataObjectReader(creditorAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(creditorAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(() =>
			{
				AssertContents(shipmentBO);
				AssertEquals(orgAddress.Header.PK, shipmentBO.Creditor.PK);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'Creditor':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'Creditor':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestBookingPartyDocumentaryAddress()
		{
			var bookingPartyAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			var orgAddress = new OrganisationDataObjectReader(bookingPartyAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(bookingPartyAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull("Import succeed", shipmentBO);
			AssertNotNull("Forwarding shipment should support BookingPartyDocumentaryAddress type import", shipmentBO.BookingParty);
		}

		public void TestReadIntoBusinessObject_INCOTermIsSpecified_ReadFromDataObject()
		{
			shipmentDataObject.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "CFG" };
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("IncoTerm", "CFG", shipmentBO.JS_INCO);
		}

		public void TestReadIntoBusinessObject_ConsignorIsSpecifiedINCOTermIsNotSpecified_ImportedINCOTermShouldBeEmpty()
		{
			var consignorAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var orgAddress = new OrganisationDataObjectReader(consignorAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			shipmentDataObject.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "" };
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(consignorAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("IncoTerm", "", shipmentBO.JS_INCO);
		}

		public void TestPickupCFSAddress_ForWarehouseOrder()
		{
			var pickupCFSAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.DropOffAddress));
			var orgAddress = new OrganisationDataObjectReader(pickupCFSAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(pickupCFSAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNull("Should not import Pickup CFS if not from a warehouse order", shipmentBO.ExportReceivingDepot);

			Logger.ClearLogs();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseOrder, null);
			var newReader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var newShipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(newShipmentBO);
			AssertNotNull(newShipmentBO.ExportReceivingDepot);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(newShipmentBO);
				AssertAddressContentMatches_INTHEMSYD(newShipmentBO.ExportReceivingDepot);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'DropOffAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Updated Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestExportBrokerAddress()
		{
			var exportBrokerAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ExportBroker));
			var orgAddress = new OrganisationDataObjectReader(exportBrokerAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				exportBrokerAddress
			});

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.ExportBroker);

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ExportBroker.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ExportBroker':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'ExportBroker':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestImportBrokerAddress()
		{
			var importBrokerAddress = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ImportBroker));
			var orgAddress = new OrganisationDataObjectReader(importBrokerAddress, Logger, Factory).GetMatchedOrNewForTesting();

			Factory.SaveForTesting();

			AssertEquals("orgAddress.IsInDatabase", true, orgAddress.IsInDatabase);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				importBrokerAddress
			});

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.ImportBroker);

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertAddressContentMatches_INTHEMSYD(shipmentBO.ImportBroker.MainAddress);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'ImportBroker':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'ImportBroker':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLoadingShipmentThroughHouseBillWithoutParent()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPK = shipmentBOToLoad.PK;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House WayBill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.PK", shipmentPK, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadsLatestShipmentWhenHasEqualMatches()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);
			var dummyShipment = Factory.New<ForwardingShipment>();
			dummyShipment.JS_HouseBill = "NOTMYHOUSE";

			var dummyOrderItem1 = dummyShipment.DocsAndCartage.OrderItems.AddNew();
			dummyOrderItem1.JT_OrderReference = "Order";

			var dummyOrderItem2 = dummyShipment.DocsAndCartage.OrderItems.AddNew();
			dummyOrderItem2.JT_OrderReference = "Number";

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var orderItem1 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "Order";

			var orderItem2 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "Number";

			Factory.SaveForTesting();

			var orderNumber1 = new OrderNumber { OrderReference = "Order" };
			var orderNumber2 = new OrderNumber { OrderReference = "Number" };

			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber1);
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber2);
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";
			shipmentDataObject.WayBillNumber = "MYHOUSE";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Updated Shipment S00001001 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingShipmentFallsBackToPortOfOriginAndDestination()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_RL_NKOrigin = "NZDUD";

			var orderItem1 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "Order";

			var orderItem2 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "Number";

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var dummyShipment = Factory.New<ForwardingShipment>();
			dummyShipment.JS_HouseBill = "NOTMYHOUSE";
			dummyShipment.JS_RL_NKOrigin = "NZCHC";

			var dummyOrderItem1 = dummyShipment.DocsAndCartage.OrderItems.AddNew();
			dummyOrderItem1.JT_OrderReference = "Order";

			var dummyOrderItem2 = dummyShipment.DocsAndCartage.OrderItems.AddNew();
			dummyOrderItem2.JT_OrderReference = "Number";

			Factory.SaveForTesting();

			var orderNumber1 = new OrderNumber { OrderReference = "Order" };
			var orderNumber2 = new OrderNumber { OrderReference = "Number" };

			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber1);
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber2);
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", ZString.Empty, shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion

			Logger.ClearLogs();
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingShipmentWithMostMatches()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var orderItem1 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "Order";

			var orderItem2 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "Number";

			var orderItem3 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem3.JT_OrderReference = "Three";

			Factory.SaveForTesting();

			Thread.Sleep(200);

			var dummyShipment = Factory.New<ForwardingShipment>();
			dummyShipment.JS_HouseBill = "NOTMYHOUSE";

			var dummyOrderItem1 = dummyShipment.DocsAndCartage.OrderItems.AddNew();
			dummyOrderItem1.JT_OrderReference = "Order";

			var dummyOrderItem2 = dummyShipment.DocsAndCartage.OrderItems.AddNew();
			dummyOrderItem2.JT_OrderReference = "Number";

			Factory.SaveForTesting();

			var orderNumber1 = new OrderNumber { OrderReference = "Order" };
			var orderNumber2 = new OrderNumber { OrderReference = "Number" };
			var orderNumber3 = new OrderNumber { OrderReference = "Three" };

			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber1);
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber2);
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber3);
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";
			shipmentDataObject.WayBillNumber = "MYHOUSE";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingShipmentFallbackThroughOrderNumbers()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var orderItem1 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "Order";

			var orderItem2 = shipmentBOToLoad.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "Number";

			Factory.SaveForTesting();

			var orderNumber1 = new OrderNumber { OrderReference = "Order" };
			var orderNumber2 = new OrderNumber { OrderReference = "Number" };

			shipmentDataObject.LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.LocalProcessing.SetOrderNumberCollection(() => new DataObjectList<OrderNumber>());
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber1);
			shipmentDataObject.LocalProcessing.OrderNumberCollection.Add(orderNumber2);
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", ZString.Empty, shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion

			Logger.ClearLogs();
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingShipmentThroughAdditionalReferences()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var cusEntryNumber1 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AMS";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "COC";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber3 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber3.CE_EntryNum = "CE00003";
			cusEntryNumber3.CE_EntryType = "UBR";
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Factory.SaveForTesting();

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "AMS", Description = "AMS Number" }, ReferenceNumber = "CE00001" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "COC", Description = "Customs Office Code (Override)" }, ReferenceNumber = "CE00002" };
			var additionalReference3 = new AdditionalReference { Type = new EntryType { Code = "UBR", Description = "Under Bond Approval Reference Number" }, ReferenceNumber = "CE00003" };

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference1);
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference2);
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference3);
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.CodesMappedToTarget = true;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestHouseBillExceedsTheMaximumLength()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var shipmentPK = shipmentBOToLoad.PK;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSEMYHOUSEMYHOUSEMY";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertNotEquals("shipmentBO.PK", shipmentPK, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSEMYHOUSEMYHOUS", shipmentBO.JS_HouseBill);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Attempted to insert 23 characters into Field [JS_HouseBill] which has a maximum length of 20 characters. Field was truncated.
Information - Added Shipment (House Bill='MYHOUSEMYHOUSEMYHOUS') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestClientContractNumberIsReadOnJobCosting()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001000");
				shipmentDataObject.DataContext.CodesMappedToTarget = true;
				shipmentDataObject.AdditionalTerms = "Add Me Some Terms";
				shipmentDataObject.WayBillNumber = "MYHOUSE";
				shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

				shipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.JobCosting.Branch = new Branch();
				shipmentDataObject.JobCosting.Branch.Code = "SYD";
				shipmentDataObject.JobCosting.ClientContractNumber = "12345";

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);

				AssertNotNull(shipmentBO);

				var jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "S00001000").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 1, jobs.Length);
				AssertEquals(jobs[0].JH_ClientContractNumber, "12345");
			}
		}

		public void TestWorkflowCustomFieldsOnShipmentAreImported()
		{
			#region Setup Template

			MasterFilesTestHelper.ClearWorkflowTables();

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnBool2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool2.XC_Name = "Flag This!";
			genCustomColumnBool2.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool2);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			var genCustomColumnInt2 = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt2.XC_Name = "Integraler";
			genCustomColumnInt2.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt2);

			Factory.SaveForTesting();

			#endregion

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO").PadRight(GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, '1')));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are customary", new ZString("GOODBYE")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last Date", new ZDateTime(2011, 1, 2)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			shipmentDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = shipmentBO.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Custom Field 1 not found", customFieldsString.Contains("Deci Deca - 0.3"));
				Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - 01-Jan-11 00:00:00"));
				Assert("Custom Field 3 not found", customFieldsString.Contains("Flagger - Y"));
				Assert("Custom Field 4 not found", customFieldsString.Contains("Integer Mate - 42"));
				Assert("Custom Field 5 not found", customFieldsString.Contains(string.Format("Textual context - {0}", "HELLO".PadRight(100, '1'))));

				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Attempted to insert 101 characters into Field [Textual context] which has a maximum length of 100 characters. Field was truncated.
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
Information - Added Shipment from UniversalShipment.
", GenCustomAddOnValueSchema.XV_Data.MaxLength + 1, GenCustomAddOnValueSchema.XV_Data.MaxLength).Trim(), Logger.Logs);
			});
		}

		public void TestWorkflowCustomFieldsOnShipmentAreFirstlyImportedAndThenClearedBySecondImport()
		{
			#region Setup Template

			MasterFilesTestHelper.ClearWorkflowTables();

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "Textual context";
			genCustomColumnString.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnString);

			var genCustomColumnDate = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDate.XC_Name = "First Date";
			genCustomColumnDate.XC_Type = "DAT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDate);

			var genCustomColumnDecimal = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnDecimal.XC_Name = "Deci Deca";
			genCustomColumnDecimal.XC_Type = "DEC";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnDecimal);

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "Flagger";
			genCustomColumnBool.XC_Type = "BOO";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			var genCustomColumnInt = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnInt.XC_Name = "Integer Mate";
			genCustomColumnInt.XC_Type = "INT";

			processTaskTemplate.GenCustomColumnDefinitions.Add(genCustomColumnInt);

			Factory.SaveForTesting();

			#endregion

			#region First Import

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = shipmentBO.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();

				Assert("Custom Field 1 not found", customFieldsString.Contains(string.Format("Textual context - HELLO")));
				Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - 01-Jan-11 00:00:00"));
				Assert("Custom Field 3 not found", customFieldsString.Contains("Deci Deca - 0.3"));
				Assert("Custom Field 4 not found", customFieldsString.Contains("Flagger - Y"));
				Assert("Custom Field 5 not found", customFieldsString.Contains("Integer Mate - 42"));
			});

			#endregion

			#region Second Import

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString().Default));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime().Default));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal().Default));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", new ZBool().Default));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt().Default));

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				var customFields = shipmentBO.GetUserDefinedValues();
				AssertEquals("Custom fields should be cleared", 0, customFields.Count());
			});

			#endregion
		}

		public void TestReadIntoBusinessObject_ClientHasSpecificWorkflowTemplate_PopulateWorkflowCustomFields()
		{
			var orgGenerator = new OrganisationTestHelper(Factory);

			var localClientDataObject = orgGenerator.CreateDataObject("MCLAREN", "BUTTON", "1234");
			localClientDataObject.AddressType = nameof(DocAddressType.LocalClient);
			localClientDataObject.AddressOverride = true;

			MasterFilesTestHelper.ClearWorkflowTables();

			var localClientOrganisation = orgGenerator.CreateBusinessObject("MCLAREN", "BUTTON", "1234");
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "SHP";
			processTaskTemplate.P0_IsActive = true;
			processTaskTemplate.P0_OH_Client = localClientOrganisation.PK;

			var customColumn = Factory.New<GenCustomColumnDefinition>();
			customColumn.XC_Name = "ManUtd";
			customColumn.XC_Type = "STR";

			processTaskTemplate.GenCustomColumnDefinitions.Add(customColumn);

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(localClientDataObject);

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("ManUtd", new ZString("Rooney")));

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			var customField = shipmentBO.GetUserDefinedValues().FirstOrDefault(f => f.PropertyName == "ManUtd");

			AssertNotNull("Custom field exists", customField);
			AssertEquals("The field is populated", "Rooney", customField.Value);

			shipmentBO.Job.Dispose();
		}

		public void TestCustomFieldsOnShipmentAreImported()
		{
			var registryInstance = FreightDataRegistry.Instance;
			var emptyGuid = System.Guid.Empty;
			registryInstance.ShipmentCustomDate1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("First DAte", "")); // Should be a case insensitive match
			registryInstance.ShipmentCustomDate2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Last Date", ""));
			registryInstance.ShipmentCustomDecimalNo1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("DEci Deca", ""));
			registryInstance.ShipmentCustomDecimalNo2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("+ 1 point zero", ""));
			registryInstance.ShipmentCustomFlag1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flag this!", ""));
			registryInstance.ShipmentCustomFlag2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Flagger", ""));
			registryInstance.ShipmentCustomText1.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Textual context", ""));
			registryInstance.ShipmentCustomText2.SetValue(emptyGuid, emptyGuid, emptyGuid, new CaptionAndHint("Customs are customary", ""));

			shipmentDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO THIS MESSAGE IS TOO LONG")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are custOmary", new ZString("GOODBYE")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(2011, 1, 1)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last Date", new ZDateTime(2011, 1, 2)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(9999999999.99)));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flag this!", new ZString("I am NOT a BOOLEAN!!")));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			shipmentDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = shipmentBO.DocsAndCartage;
				AssertEquals("docsAndCartage.JP_CustomAttrib1", "HELLO THIS MESS", docsAndCartage.JP_CustomAttrib1);
				AssertEquals("docsAndCartage.JP_CustomAttrib2 - case insensitive match", "GOODBYE", docsAndCartage.JP_CustomAttrib2);
				AssertEquals("docsAndCartage.JP_CustomDate1 - case insensitive match", new ZDateTime(2011, 1, 1), docsAndCartage.JP_CustomDate1);
				AssertEquals("docsAndCartage.JP_CustomDate2", new ZDateTime(2011, 1, 2), docsAndCartage.JP_CustomDate2);
				AssertEquals("docsAndCartage.JP_CustomDecimal1 - case insensitive match", 0.3m, docsAndCartage.JP_CustomDecimal1);
				AssertEquals("docsAndCartage.JP_CustomDecimal2", 999999999m, docsAndCartage.JP_CustomDecimal2);
				AssertEquals("docsAndCartage.JP_CustomFlag2", true, docsAndCartage.JP_CustomFlag2);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Attempted to insert 30 characters into Field [JP_CustomAttrib1] which has a maximum length of 15 characters. Field was truncated.
Warning - Attempted to insert '9999999999.99' into Field [JP_CustomDecimal2] which has a maximum numeric value of '999999999'. Field was truncated to the max value.
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!!]. Value must be a valid Boolean (true or false).
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessingOnShipmentAreImported()
		{
			shipmentDataObject.LocalProcessing = LocalProcessingDataObjectReaderTest.SetupLocalProcessing(new CodeDescriptionPair() { Code = "BOB", Description = "THE BUILDER" }, new CodeDescriptionPair() { Code = "WEN", Description = "THE DESTROYER" }, new CodeDescriptionPair() { Code = "JOE", Description = "THE PEACEMAKER" }, new CodeDescriptionPair() { Code = "JAY", Description = "THE LAZY" });

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				var docsAndCartage = shipmentBO.DocsAndCartage;
				LocalProcessingDataObjectReaderTest.AssertContents(docsAndCartage, "BOB", "WEN", "JOE", "JAY");
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestLocalProcessingConditionalImportToShipmentPenalties()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertShipmentPenalties(Core.Constants.ContainerModes.BuyersConsol, Core.Constants.ShipmentTypes.BuyersConsolLead, 2, 0);
			AssertShipmentPenalties(Core.Constants.ContainerModes.BuyersConsol, Core.Constants.ShipmentTypes.StandardHouse, 0, 0);
			AssertShipmentPenalties(Core.Constants.ContainerModes.FCL, Core.Constants.ShipmentTypes.StandardHouse, 2, 1);
			AssertShipmentPenalties(Core.Constants.ContainerModes.LCL, Core.Constants.ShipmentTypes.StandardHouse, 0, 0);

			void AssertShipmentPenalties(ZString containerMode, ZString shipmentType, int deliveryResult, int pickupResult)
			{
				var shipmentDataObject = SetupShipment(GetResourcePathFor("UniversalShipmentWithLocalProcessingAndConsol.xml"));
				shipmentDataObject.SubShipmentCollection[0].ContainerMode = new ContainerMode() { Code = containerMode };
				shipmentDataObject.SubShipmentCollection[0].ShipmentType = new CodeDescriptionPair() { Code = shipmentType };
				var reader = new ConsolDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				AssertEquals(deliveryResult, consolBO.Containers[0].DeliveryPenalties.Count);
				AssertEquals(pickupResult, consolBO.Containers[0].PickupPenalties.Count);
			}
		}

		public void TestLocalProcessingOnShipmentAreImportedToShipmentPenalties()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var shipmentDataObject = SetupShipment(GetResourcePathFor("UniversalShipmentWithLocalProcessingAndConsol.xml"));
			var reader = new ConsolDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals(1, consolBO.Shipments[0].OuterPackLines.Count);

			var containerPK = consolBO.Containers[0].PK;

			CombineAssertions(delegate
			{
				var pickupDetention = consolBO.Containers[0].PickupPenalties.FirstOrDefault((Freight.Business.ContainerPenalty p) => p.CPY_ProcessType == Core.Constants.ContainerPenaltyProcessType.Pickup && p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
				AssertNotNull(pickupDetention);
				AssertEquals(containerPK, pickupDetention.CPY_JC_Container);
				AssertEquals(30m, pickupDetention.CPY_PerUnitCost);
				AssertEquals(2, (int)pickupDetention.FreeTimeAsDays);
				AssertEquals(3, (int)pickupDetention.DurationAsDays);

				var deliveryDetention = consolBO?.Containers[0].DeliveryPenalties.FirstOrDefault((Freight.Business.ContainerPenalty p) => p.CPY_ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery && p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
				AssertNotNull(deliveryDetention);
				AssertEquals(containerPK, deliveryDetention.CPY_JC_Container);
				AssertEquals(20m, deliveryDetention.CPY_PerUnitCost);
				AssertEquals(2, (int)deliveryDetention.FreeTimeAsDays);
				AssertEquals(4, (int)deliveryDetention.DurationAsDays);

				var deliveryStorage = consolBO.Containers[0].DeliveryPenalties.FirstOrDefault((Freight.Business.ContainerPenalty p) => p.CPY_ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery && p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
				AssertNotNull(deliveryStorage);
				AssertEquals(containerPK, deliveryStorage.CPY_JC_Container);
				AssertEquals(10m, deliveryStorage.CPY_PerUnitCost);
				AssertEquals(3, (int)deliveryStorage.DurationAsDays);
			});

			shipmentDataObject = SetupShipment(GetResourcePathFor("UniversalShipmentWithLocalProcessingAndConsol.xml"));
			shipmentDataObject.DataContext.DataTargetCollection.FirstOrDefault().Key = consolBO.Shipments[0].JS_UniqueConsignRef;
			var localProcessingDataObject = shipmentDataObject.SubShipmentCollection[0].LocalProcessing;
			localProcessingDataObject.FCLPickupDetentionCharge = 31m;
			localProcessingDataObject.FCLPickupDetentionFreeDays = 3;
			localProcessingDataObject.FCLPickupDetentionDays = 4;
			localProcessingDataObject.FCLDeliveryDetentionCharge = 21m;
			localProcessingDataObject.FCLDeliveryDetentionFreeDays = 3;
			localProcessingDataObject.FCLDeliveryDetentionDays = 5;
			localProcessingDataObject.LCLAirStorageCharge = 11m;
			localProcessingDataObject.LCLAirStorageDaysOrHours = 4;

			reader = new ConsolDataObjectReader(shipmentDataObject, Logger, Factory, null);
			consolBO = reader.ReadIntoBusinessObject();

			AssertNotNull(consolBO);
			AssertEquals(1, consolBO.Shipments[0].OuterPackLines.Count);

			containerPK = consolBO.Containers[0].PK;

			CombineAssertions(delegate
			{
				var pickupDetention = consolBO.Containers[0].PickupPenalties.FirstOrDefault((Freight.Business.ContainerPenalty p) => p.CPY_ProcessType == Core.Constants.ContainerPenaltyProcessType.Pickup && p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
				AssertNotNull(pickupDetention);
				AssertEquals(containerPK, pickupDetention.CPY_JC_Container);
				AssertEquals(31m, pickupDetention.CPY_PerUnitCost);
				AssertEquals(3, (int)pickupDetention.FreeTimeAsDays);
				AssertEquals(4, (int)pickupDetention.DurationAsDays);

				var deliveryDetention = consolBO.Containers[0].DeliveryPenalties.FirstOrDefault((Freight.Business.ContainerPenalty p) => p.CPY_ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery && p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
				AssertNotNull(deliveryDetention);
				AssertEquals(containerPK, deliveryDetention.CPY_JC_Container);
				AssertEquals(21m, deliveryDetention.CPY_PerUnitCost);
				AssertEquals(3, (int)deliveryDetention.FreeTimeAsDays);
				AssertEquals(5, (int)deliveryDetention.DurationAsDays);

				var deliveryStorage = consolBO.Containers[0].DeliveryPenalties.FirstOrDefault((Freight.Business.ContainerPenalty p) => p.CPY_ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery && p.CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
				AssertNotNull(deliveryStorage);
				AssertEquals(containerPK, deliveryStorage.CPY_JC_Container);
				AssertEquals(11m, deliveryStorage.CPY_PerUnitCost);
				AssertEquals(4, (int)deliveryStorage.DurationAsDays);
			});
		}

		public void TestAllocationOfContainerNotGenerateShipmentPenaltyWhileImporting()
		{
			var mockDetentionMatchResult = new Mock<IContainerPenaltyMatchResult>();
			mockDetentionMatchResult.Setup(x => x.FreeDays).Returns(3);

			var strategy = new Moq.Mock<IContainerDefaultingStrategy>();
			strategy.Setup(x => x.GetMatchedDetentionPenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(mockDetentionMatchResult.Object);

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var shipmentDataObject = SetupShipment(GetResourcePathFor("UniversalShipmentWithLocalProcessingAndConsol.xml"));
				var containerDTO = ContainerDataObjectTestHelper.SetupContainer(Factory);
				shipmentDataObject.ContainerCollection.Clear();
				containerDTO.Link = 1;
				shipmentDataObject.ContainerCollection.Add(containerDTO);
				shipmentDataObject.SubShipmentCollection[0].LocalProcessing = new LocalProcessing();
				shipmentDataObject.SubShipmentCollection[0].PackingLineCollection[0].ContainerNumber = containerDTO.ContainerNumber;
				var reader = new ConsolDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var consolBO = reader.ReadIntoBusinessObject();

				AssertNotNull(consolBO);
				var container = consolBO.Containers.Cast<CommonContainer>().FirstOrDefault(x => x.JC_ContainerNum == (containerDTO.ContainerNumber ?? ZString.Empty));
				AssertNotNull(container);
				Assert("allocation of packline should generate pickup penalties while importing", !container.PickupPenalties.Any());
				Assert("allocation of packline should generate delivery penalties while importing", !container.DeliveryPenalties.Any());
			}
		}

		public void TestAdditionalReferenceNumbers()
		{
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

			var consolBO = Factory.New<ForwardingConsol>();
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			consolBO.Shipments.Add(shipmentBOToLoad);

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			var additionalReferenceNumberDataObject = AdditionalReferenceDataObjectReaderTest.SetupAdditionalReference();

			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReferenceNumberDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.Numbers.Count", 1, shipmentBO.Numbers.Count);

				var additionalReferenceNumberBO = shipmentBO.Numbers[0];
				AdditionalReferenceDataObjectReaderTest.AssertContents(additionalReferenceNumberBO);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			var newLogger = new TestErrorLogger();
			reader = new ShipmentDataObjectReader(shipmentDataObject, newLogger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.Numbers.Count", 1, shipmentBO.Numbers.Count);

				var additionalReferenceNumberBO = shipmentBO.Numbers[0];
				AdditionalReferenceDataObjectReaderTest.AssertContents(additionalReferenceNumberBO);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestAdditionalReferenceNumbers_FSH()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipmentBOToLoad = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001359";

			consolBO.Shipments.Add(shipmentBOToLoad);

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			var additionalReferenceDataObject = new AdditionalReference();

			additionalReferenceDataObject.ContextInformation = "INFORMER";
			additionalReferenceDataObject.IssueDate = new ZDateTime(2011, 3, 3);
			additionalReferenceDataObject.ReferenceNumber = "S00001359";
			additionalReferenceDataObject.Type = new EntryType { Code = "FSH", Description = "Forwarding Shipment Number" };

			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReferenceDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.JobNumber", "S00001359", shipmentBO.JobNumber);

				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Unknown Additional References were found for 'Shipment S00001359' and not imported. These were added to the 'Unrecognized Additional Reference Types' Note.
Information - Updated Shipment S00001359 from UniversalShipment.".Trim(), Logger.Logs);
			});
		}

		public void TestAdditionalReferenceNumbers_DAKOSY_SZB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "HELLO";

				var userSZB = shipment.Numbers.AddNew();
				userSZB.CE_EntryIsSystemGenerated = false;
				userSZB.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
				userSZB.CE_EntryNum = "1024";

				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.DataContext = DataContextFactory.New();
				shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "HELLO");

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber },
						ReferenceNumber = "2048"
					},
				});
				shipmentDataObject.AdditionalReferenceCollection.Content = CollectionContent.Partial;

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();

				var systemSZB = shipmentBO.Numbers.Cast<CusEntryNumber>().First(number => number.PK != userSZB.PK);

				CombineAssertions("Importing new SZB Number", () =>
				{
					AssertEquals("Pre-requisite: shipment has been matched", shipment.PK, shipmentBO.PK);
					AssertContainsExactElementsInAnyOrder(new[] { systemSZB, userSZB }, shipmentBO.Numbers);

					AssertEquals("1024", userSZB.CE_EntryNum);
					AssertEquals(false, userSZB.CE_EntryIsSystemGenerated);

					AssertEquals(GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber, systemSZB.CE_EntryType);
					AssertEquals("2048", systemSZB.CE_EntryNum);
					AssertEquals("IMPORTANT: number added through universal transfer MUST have IsSystemGenerated=true", true, systemSZB.CE_EntryIsSystemGenerated);
				});

				Factory.SaveForTesting();

				shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference()
					{
						Type = new EntryType { Code = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber },
						ReferenceNumber = ""
					},
				});
				shipmentDataObject.AdditionalReferenceCollection.Content = CollectionContent.Partial;

				reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				shipmentBO = reader.ReadIntoBusinessObject();

				CombineAssertions("Importing SZB cancellation (empty SZB)", () =>
				{
					AssertEquals("Pre-requisite: shipment has been matched", shipment.PK, shipmentBO.PK);
					AssertContainsExactElementsInAnyOrder(new[] { systemSZB, userSZB }, shipmentBO.Numbers);

					AssertEquals("1024", userSZB.CE_EntryNum);
					AssertEquals(false, userSZB.CE_EntryIsSystemGenerated);

					AssertEquals("", systemSZB.CE_EntryNum);
					AssertEquals(true, systemSZB.CE_EntryIsSystemGenerated);
				});
			}
		}

		public void TestCusEntryNumbers()
		{
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

			var consolBO = Factory.New<ForwardingConsol>();
			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			consolBO.Shipments.Add(shipmentBOToLoad);

			shipmentDataObject.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>());

			var entryNumberDataObject = SetupEntryNumber();

			shipmentDataObject.EntryNumberCollection.Add(entryNumberDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CusEntryNumbersForAllCountries.Count", 1, shipmentBO.CusEntryNumbersForAllCountries.Count);

				var entryNumberBO = shipmentBO.CusEntryNumbersForAllCountries[0];
				AssertContents(entryNumberBO, shipmentBO);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			var newLogger = new TestErrorLogger();
			entryNumberDataObject.EntryLineReference = "SOMETHING";

			reader = new ShipmentDataObjectReader(shipmentDataObject, newLogger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CusEntryNumbersForAllCountries.Count", 1, shipmentBO.CusEntryNumbersForAllCountries.Count);

				var entryNumberBO = shipmentBO.CusEntryNumbersForAllCountries[0];
				AssertEquals("entryNumberBO.CE_EntryLineReference", "REFERENCE", entryNumberBO.CE_EntryLineReference);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), newLogger.Logs);
			});
		}

		public void TestCommercialInvoiceLinesUpdatePacklinesIfPacklineCollectionNotPresent()
		{
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.WarehouseOrder, null);

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2 } });
			shipmentDataObject.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
						{
							new CommercialInvoiceLine { InvoiceQuantity = 3 },
							new CommercialInvoiceLine { InvoiceQuantity = 4 }
						}))
				}
			};

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.OuterPackLines.Count);
				AssertEquals(2, shipmentBO.OuterPackLines[0].JL_PackageCount);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			shipmentDataObject.SetPackingLineCollection(() => null);
			Logger.ClearLogs();
			var newReader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var newShipmentBO = newReader.ReadIntoBusinessObject();

			AssertNotNull(newShipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals(2, newShipmentBO.OuterPackLines.Count);
				AssertEquals(3, newShipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(4, newShipmentBO.OuterPackLines[1].JL_PackageCount);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Updated Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestCommercialInvoiceLinesNotUpdatePacklinesIfPacklineCollectionNotPresentForCustomsDeclaration()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_UniqueConsignRef = "C00030000";

			var shipmentBO1 = consolBO.GridShipments.AddNew();
			shipmentBO1.JS_UniqueConsignRef = "S00030001";
			var declarationBO1 = Factory.BOFactory.New<IBaseJobDeclaration>();
			declarationBO1.JE_MessageType = "IMP";
			declarationBO1.JE_ApplicationCode = "CMR";
			declarationBO1.JE_DeclarationReference = "S00030001";
			declarationBO1.JE_JS = shipmentBO1.PK;

			Factory.SaveForTesting();

			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S00030001");
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "S00030001");
			shipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2 } });
			shipmentDataObject.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
						{
							new CommercialInvoiceLine { InvoiceQuantity = 3 },
							new CommercialInvoiceLine { InvoiceQuantity = 4 }
						}))
				}
			};

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.OuterPackLines.Count);
				AssertEquals(2, shipmentBO.OuterPackLines[0].JL_PackageCount);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Warning - Cannot add a package as there is no bill record to attach the packages to
Information - Updated Declaration S00030001 from UniversalShipment.
Information - Updated Shipment S00030001 from UniversalShipment.
".Trim(), Logger.Logs);
			});

			shipmentDataObject.SetPackingLineCollection(() => null);
			Logger.ClearLogs();
			var newReader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var newShipmentBO = newReader.ReadIntoBusinessObject();

			AssertNotNull(newShipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.OuterPackLines.Count);
				AssertEquals(2, shipmentBO.OuterPackLines[0].JL_PackageCount);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Updated Declaration S00030001 from UniversalShipment.
Information - Updated Shipment S00030001 from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestUpdatePacklinesIfItAndPackingLineCollectionEachHaveOneRecord()
		{
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 2, PackType = new PackageType { Code = "PKG" }, Weight = 35m, Height = 2m, Width = 5m
				}
			});
			shipmentDataObject.PackingLineCollection[0].SetUNDGCollection(() => null);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.OuterPackLines.Count);
				AssertEquals(0, shipmentBO.OuterPackLines[0].UNDGs.Count);
				AssertEquals(2, shipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(35m, shipmentBO.OuterPackLines[0].JL_ActualWeight);
				AssertEquals(2m, shipmentBO.OuterPackLines[0].JL_Height);
				AssertEquals(5m, shipmentBO.OuterPackLines[0].JL_Width);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			Logger.ClearLogs();

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = "PKG" }, Weight = 30m } });
			shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.OuterPackLines.Count);
				AssertEquals(2, shipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(30m, shipmentBO.OuterPackLines[0].JL_ActualWeight);
				AssertEquals(2m, shipmentBO.OuterPackLines[0].JL_Height);
				AssertEquals(5m, shipmentBO.OuterPackLines[0].JL_Width);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Updated Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			var undgDataItem = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem.DI_DGVolume = 5m;
			undgDataItem.DI_DGWeight = 8m;
			shipmentBO.OuterPackLines[0].UNDGs.Add(undgDataItem);

			shipmentDataObject.PackingLineCollection[0].SetUNDGCollection(() => new List<UNDG>());
			shipmentDataObject.PackingLineCollection[0].UNDGCollection.Add(UndgItem());

			Logger.ClearLogs();
			shipmentBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertNotNull(shipmentBO.OuterPackLines[0].UNDGs[0]);
				AssertEquals(1m, shipmentBO.OuterPackLines[0].UNDGs[0].DI_DGVolume);
				AssertEquals(2m, shipmentBO.OuterPackLines[0].UNDGs[0].DI_DGWeight);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no contact found in database with name 'Contact1' and phone '123456' for dangerous goods substance code '3000c' (IMO Class = 'Clas'). Make sure that name and phone is not empty. If not create contact first.
Warning - There is no substance with code '3000c' found. Please use standard dangerous goods substance code.
Information - Updated Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 1,Weight = 34m
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 3,Weight = 36m
				}
			});
			shipmentDataObject.PackingLineCollection[0].SetUNDGCollection(() => null);
			shipmentDataObject.PackingLineCollection[1].SetUNDGCollection(() => null);

			Logger.ClearLogs();
			shipmentBO = reader.ReadIntoBusinessObject();
			CombineAssertions(delegate
			{
				AssertEquals(2, shipmentBO.OuterPackLines.Count);
				AssertEquals(1, shipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(34m, shipmentBO.OuterPackLines[0].JL_ActualWeight);
				AssertEquals(0m, shipmentBO.OuterPackLines[0].JL_Height);
				AssertEquals(3, shipmentBO.OuterPackLines[1].JL_PackageCount);
				AssertEquals(36m, shipmentBO.OuterPackLines[1].JL_ActualWeight);
				AssertEquals(0m, shipmentBO.OuterPackLines[1].JL_Height);
				AssertEquals(0, shipmentBO.OuterPackLines[0].UNDGs.Count);
				AssertEquals(0, shipmentBO.OuterPackLines[1].UNDGs.Count);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Updated Shipment from UniversalShipment.

".Trim(), Logger.Logs);
			});
		}

		public void TestNotPackFreePackingLine_UpdateExsitingShipment()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_UniqueConsignRef = "C00000010";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_UniqueConsignRef = "S00030001";

			var container = consolBO.Containers.AddNew();
			var refC = Factory.BOFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = refC.PK;

			Factory.SaveForTesting();

			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S00030001");

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 1,
					Volume = 1.1
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 2,
					Volume = 2.2
				}
			});

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var importedShipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(importedShipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals(shipmentBO.PK, importedShipmentBO.PK);
				AssertEquals("S00030001", importedShipmentBO.JS_UniqueConsignRef);
				AssertEquals(1, importedShipmentBO.Consols.Count);
				AssertEquals(consolBO.PK, importedShipmentBO.Consols[0].PK);

				AssertEquals(0, container.PackLines.Count);
				AssertEquals(2, consolBO.UnAllocatedPackLines.Count);

				AssertEquals(2, importedShipmentBO.OuterPackLines.Count);
				AssertEquals(1, importedShipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(1.1m, importedShipmentBO.OuterPackLines[0].JL_ActualVolume);
				AssertEquals(Guid.Empty, importedShipmentBO.OuterPackLines[0].JL_JC);

				AssertEquals(2, importedShipmentBO.OuterPackLines[1].JL_PackageCount);
				AssertEquals(2.2m, importedShipmentBO.OuterPackLines[1].JL_ActualVolume);
				AssertEquals(Guid.Empty, importedShipmentBO.OuterPackLines[1].JL_JC);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Updated Shipment S00030001 from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestNotPackFreePackingLine_NewShipment()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_UniqueConsignRef = "C00000010";

			var container = consolBO.Containers.AddNew();
			var refC = Factory.BOFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = refC.PK;

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 1,
					Volume = 1.1
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 2,
					Volume = 2.2
				}
			});

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.Consols.Count);
				AssertEquals(consolBO.PK, shipmentBO.Consols[0].PK);

				AssertEquals(0, container.PackLines.Count);
				AssertEquals(2, consolBO.UnAllocatedPackLines.Count);

				AssertEquals(2, shipmentBO.OuterPackLines.Count);
				AssertEquals(1, shipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(1.1m, shipmentBO.OuterPackLines[0].JL_ActualVolume);
				AssertEquals(Guid.Empty, shipmentBO.OuterPackLines[0].JL_JC);

				AssertEquals(2, shipmentBO.OuterPackLines[1].JL_PackageCount);
				AssertEquals(2.2m, shipmentBO.OuterPackLines[1].JL_ActualVolume);
				AssertEquals(Guid.Empty, shipmentBO.OuterPackLines[1].JL_JC);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestGrossWeightCalculation_VerificationTypeNON_GrossWeightRecalculates()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_UniqueConsignRef = "C00000010";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_UniqueConsignRef = "S00030001";

			Factory.SaveForTesting();

			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C00000010");
			var containers = new DataObjectList<Container>()
			{
				new Container
				{
					ContainerCount = 1,
					Link = 1,
					ContainerNumber = "MRKU4001708",
					ContainerType = new ContainerType()
					{
						Code = "40HC"
					},
					GoodsWeight = 10400,
					GrossWeight = 14383,
					GrossWeightVerificationType = new CodeDescriptionPair()
					{
						Code = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified,
						Description = Core.Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified
					},
					TareWeight = 3980,
					WeightUnit = new UnitOfWeight()
					{
						Code = "KG"
					}
				},

				new Container
				{
					ContainerCount = 1,
					Link = 2,
					ContainerNumber = "PONU7532818",
					ContainerType = new ContainerType()
					{
						Code = "40HC"
					},
					GoodsWeight = 10500,
					GrossWeight = 14485,
					GrossWeightVerificationType = new CodeDescriptionPair()
					{
						Code = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified,
						Description = Core.Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified
					},
					TareWeight = 3980,
					WeightUnit = new UnitOfWeight()
					{
						Code = "KG"
					}
				}
			};

			shipmentDataObject.SetContainerCollection(() => containers);

			var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = subShipment1.DataContext = DataContextFactory.New();
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment1.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S00030001");
			subShipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 1,
					PackQty = 4,
					PackType = new PackageType() {
						Code = "PKG"
					},
					Volume = 40,
					VolumeUnit = new UnitOfVolume() {
						Code = "M3"
					},
					Weight = 10400,
					WeightUnit = new UnitOfWeight() {
						Code = "KG"
					}
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 2,
					PackQty = 4,
					PackType = new PackageType() {
						Code = "PKG"
					},
					Volume = 40,
					VolumeUnit = new UnitOfVolume
					{
						Code = "M3"
					},
					Weight = 10500,
					WeightUnit = new UnitOfWeight()
					{
						Code = "KG"
					}
				}
			});

			subShipment1.SetContainerCollection(() => containers);
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });

			var reader = new ConsolDataObjectReader(shipmentDataObject, logger, Factory);
			var result = reader.ReadIntoBusinessObject();
			AssertEquals((ZDecimal)14380, result.Containers[0].JC_GrossWeight);
			AssertEquals((ZDecimal)14480, result.Containers[1].JC_GrossWeight);
		}

		public void TestNotPackFreePackingLine_WithPackingLinesLinkedToContainer()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_TransportMode = "SEA";
			consolBO.JK_UniqueConsignRef = "C00000010";

			var container = consolBO.Containers.AddNew();
			var refC = Factory.BOFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = refC.PK;

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 1,
					Volume = 1.1
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 2,
					Volume = 2.2
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackQty = 3,
					Volume = 3.3,
					ContainerLink = 1
				}
			});

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM(Factory);
			containerDataObject.ContainerNumber = "DOGS0000000";
			containerDataObject.Link = 1;

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals(1, shipmentBO.Consols.Count);
				AssertEquals(consolBO.PK, shipmentBO.Consols[0].PK);

				AssertEquals(2, consolBO.Containers.Count);
				AssertEquals(2, consolBO.UnAllocatedPackLines.Count);
				AssertEquals(0, consolBO.Containers[0].PackLines.Count);
				AssertEquals("DOGS0000000", consolBO.Containers[1].JC_ContainerNum);

				AssertEquals(3, shipmentBO.OuterPackLines.Count);
				AssertEquals(1, shipmentBO.OuterPackLines[0].JL_PackageCount);
				AssertEquals(1.1m, shipmentBO.OuterPackLines[0].JL_ActualVolume);
				AssertEquals(Guid.Empty, shipmentBO.OuterPackLines[0].JL_JC);

				AssertEquals(2, shipmentBO.OuterPackLines[1].JL_PackageCount);
				AssertEquals(2.2m, shipmentBO.OuterPackLines[1].JL_ActualVolume);
				AssertEquals(Guid.Empty, shipmentBO.OuterPackLines[1].JL_JC);

				AssertEquals(3, shipmentBO.OuterPackLines[2].JL_PackageCount);
				AssertEquals(3.3m, shipmentBO.OuterPackLines[2].JL_ActualVolume);
				AssertEquals(consolBO.Containers[1].PK, shipmentBO.OuterPackLines[2].JL_JC);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestOrderNumberIsReadInEvenIfShipmentIsAttachedToAnOrder()
		{
			shipmentDataObject.Order = new UniversalOrder { OrderNumber = "ORDER ME WAR" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipment = reader.ReadIntoBusinessObject();
			AssertNotNull(shipment);

			var jobDocsAndCartage = shipment.DocsAndCartage;
			AssertEquals("ORDER ME WAR", jobDocsAndCartage.JP_OrderItemsAsString);

			jobDocsAndCartage.OrderItems.RemoveAndDeleteAll();
			shipment.GenericOrders.Add(Factory.New<Order>());

			var newReader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var reloadedShipment = newReader.ReadIntoBusinessObject();
			AssertEquals("ORDER ME WAR", reloadedShipment.DocsAndCartage.JP_OrderItemsAsString);
		}

		public void TestShipmentLevelMappingsFromOrder()
		{
			shipmentDataObject.BookingConfirmationReference = null;
			shipmentDataObject.Order = new UniversalOrder
			{
				ClientReference = "BOOK ME",
				OrderNumber = "ORDER ME WAR",
				TotalLineWeight = 14.2m,
				TotalLineVolume = 18.3m
			};
			shipmentDataObject.LocalProcessing = new LocalProcessing { DeliveryRequiredBy = new ZDateTime(2013, 1, 1) };
			shipmentDataObject.DataContext.AddDataSource(DataContextType.WarehouseOrder, "");

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.DocsAndCartage.OrderItems.Count", 1, shipmentBO.DocsAndCartage.OrderItems.Count);
				AssertEquals("shipmentBO.DocsAndCartage.JP_OrderItemsAsString", "ORDER ME WAR", shipmentBO.DocsAndCartage.JP_OrderItemsAsString);
				AssertEquals("shipmentBO.DocsAndCartage.JP_PickupRequiredBy", new ZDateTime(2013, 1, 1), shipmentBO.DocsAndCartage.JP_PickupRequiredBy);
				AssertContents(shipmentBO);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestBasicShipmentLevelFieldMappings()
		{
			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001000");

			shipmentDataObject.AdditionalTerms = "Add Me Some Terms";
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.RateCommodity = new Commodity() { Code = "CM1" };
			shipmentDataObject.FMCTariffID = "BBBB";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_RH_NKRateCommodity", "CM1", shipmentBO.JS_RH_NKRateCommodity);
				AssertEquals("shipmentBO.JS_FMCTariffID", "BBBB", shipmentBO.JS_FMCTariffID);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "Add Me Some Terms", shipmentBO.JS_AdditionalTerms);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				Assert("No shipmentBO.Containers", !shipmentBO.Containers.Any());

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestShipmentStatusWithNVOCCRecipientRole()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005000";
			shipment1.JS_BookingReference = "AGENTREF";
			shipment1.JS_HouseBill = "MYHOUSE";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			shipment1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var dataContext = new DataContext();
			dataContext.RecipientRoleCollection = new List<RecipientRole>();
			dataContext.RecipientRoleCollection.Add(new RecipientRole { Code = RecipientRoleType.NVO, ServiceCode = ServiceCodeType.SIN });
			shipmentDataObject.DataContext = dataContext;

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var bookingPartyOrgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.BookingPartyDocumentaryAddress),
				Address1 = "Booking Party Address 1",
				OrganizationCode = "BKGPARTY"
			};
			shipmentDataObject.OrganizationAddressCollection.Add(bookingPartyOrgAddress);

			shipmentDataObject.AdditionalTerms = "Add Me Some Terms";
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			shipmentDataObject.CoLoadBookingConfirmationReference = "S00005000";
			shipmentDataObject.AgentsReference = "AGENTREF";

			Factory.SaveForTesting();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.JS_ShipmentStatus", ShipmentStatusList.Codes.ElectronicShippingInstruction, shipmentBO.JS_ShipmentStatus);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Successfully loaded matching ForwardingShipment.
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Populating ForwardingShipment...
Information - Matching 'BookingPartyDocumentaryAddress':- Matched to 'BKGPARTY' by code, address 'Booking Party Address 1' (only address).
Information - Updated Shipment S00005000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingShipmentOnConsolThroughHouseBillNumber()
		{
			var consolBO = Factory.New<ForwardingConsol>();

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";
			consolBO.GridShipments.Add(shipmentBOToLoad);

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "THIS FIELD WILL NOT BE TOUCHED", shipmentBO.JS_AdditionalTerms);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				Assert("No shipmentBO.Containers", !shipmentBO.Containers.Any());

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingSubShipmentOnShipmentThroughHouseBillNumber()
		{
			var shipmentParentBO = Factory.New<ForwardingShipment>();

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";
			shipmentParentBO.CoLoadShipments.Add(shipmentBOToLoad);

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(shipmentParentBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "THIS FIELD WILL NOT BE TOUCHED", shipmentBO.JS_AdditionalTerms);
				AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
				Assert("No shipmentBO.Containers", !shipmentBO.Containers.Any());

				AssertMultilineASCIIEquals("Logger.Logs", string.Format(@"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment {0} (House Bill='MYHOUSE') from UniversalShipment.
", shipmentBO.JS_UniqueConsignRef).Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestNewSubShipmentsArePackedOnToParentShipmentConsol()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();

			//set up sub-shipment
			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = subShipment.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT1");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment.GoodsDescription = "Meatball Subs";

			//set up master shipment and attach sub shipment
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "ASM", Description = "Master" };
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			//set up container and pack lines
			var packingLineDataObject = SetupPackingLine(Factory);
			packingLineDataObject.Commodity.Code = "DOGS";
			packingLineDataObject.ContainerLink = 1;

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM(Factory);
			containerDataObject.ContainerNumber = "DOGS0000000";
			containerDataObject.Link = 1;

			subShipment.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject });
			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLineDataObject });
			SetupCFSAddresses(subShipment);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var parentShipmentBO = reader.ReadIntoBusinessObject();

			#region Check Shipment and Subshipments have been packed

			AssertNotNull("Expected to have properly processed the shipment BO", parentShipmentBO);
			AssertEquals("Expected to have included the coload shipment", 1, parentShipmentBO.CoLoadShipments.Count);
			AssertEquals("Expected consol to have been attached to the parent shipment", consolBO, parentShipmentBO.Consols[0]);

			var subShipmentBO = parentShipmentBO.CoLoadShipments.FirstOrDefault() as ForwardingShipment;
			AssertEquals("Subshipment should be attached to the same consol as the parent shipment", consolBO, subShipmentBO.Consols[0]);
			var subShipmentContainer = subShipmentBO.Consols[0].Containers.Cast<ForwardingContainer>().FirstOrDefault(c => c.JC_ContainerNum == "DOGS0000000");
			var subShipmentPackLine = subShipmentBO.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(c => c.JL_RH_NKCommodityCode == "DOGS");

			AssertNotNull(subShipmentContainer);
			AssertNotNull(subShipmentPackLine);
			AssertEquals("Packline should link to container", subShipmentContainer.PK, subShipmentPackLine.GetContainer(subShipmentBO.Consols[0]).PK);

			#endregion
		}

		public void TestBookingWasConvertedAndAttachedToConsolWhenShipmentNotExsit()
		{
			var bookingBO = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var consolBO = Factory.New<ForwardingConsol>();

			var shipmentBOToLoad = bookingBO.ForwardingShipment as ForwardingShipment;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Shipments.Count", 1, consolBO.Shipments.Count);
				AssertEquals("consolBO.Shipments[0].PK", shipmentBOToLoad.PK, consolBO.Shipments[0].PK);
				AssertEquals("shipmentBO.JS_IsBooking", true, shipmentBO.JS_IsBooking);
				AssertEquals("shipmentBO.JS_IsForwardRegistered", true, shipmentBO.JS_IsForwardRegistered);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestLoadingShipmentFromConsolidatedBooking()
		{
			var bookingBO = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var booking = bookingBO.ForwardingShipment as ForwardingShipment;
			booking.JS_HouseBill = "MYHOUSE";
			booking.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";
			booking.JS_IsForwardRegistered = true;

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Shipments[0].PK", booking.PK, shipmentBO.PK);
				AssertEquals("shipmentBO.JS_IsBooking", true, shipmentBO.JS_IsBooking);
				AssertEquals("shipmentBO.JS_IsForwardRegistered", true, shipmentBO.JS_IsForwardRegistered);
				AssertEquals("shipmentBO.JS_AdditionalTerms", "I SWEAR I'VE CHANGED", shipmentBO.JS_AdditionalTerms);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestBookingWasConvertedAndAttachedToConsol_ContainerMode()
		{
			var bookingBO = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = bookingBO.ForwardingShipment as ForwardingShipment;

			shipmentBOToLoad.JS_UniqueConsignRef = "S00001000";
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";
			shipmentBOToLoad.JS_AdditionalTerms = "THIS FIELD WILL NOT BE TOUCHED";

			Factory.SaveForTesting();

			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_ConsolMode = "BCN";

			shipmentDataObject.ContainerMode = new ContainerMode { Code = "BCN" };
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.BuyersConsolLead, Description = "Buyer's Consol Lead" };
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals("consolBO.Shipments.Count", 1, consolBO.Shipments.Count);
				AssertEquals("consolBO.Shipments[0].PK", shipmentBOToLoad.PK, consolBO.Shipments[0].PK);
				AssertEquals("consolBO.JK_ConsolMode", "BCN", consolBO.JK_ConsolMode);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestBookingWasConvertedAndAttachedToConsol_ProcessTasks()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var bookingWorkflowProvider = (IWorkflowProvider)quotedBooking;
			var shipmentBOToLoad = quotedBooking.ForwardingShipment as ForwardingShipment;

			shipmentBOToLoad.JS_UniqueConsignRef = "S00001000";
			shipmentBOToLoad.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipmentBOToLoad.JS_HouseBill = "MYHOUSE";

			var bookingMilestone = bookingWorkflowProvider.WorkflowItems.Milestones.AddNew();
			var bookingTrigger = bookingWorkflowProvider.WorkflowItems.Triggers.AddNew();
			var bookingOpenTask = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			var bookingWorkingTask = bookingWorkflowProvider.WorkflowItems.Tasks.AddNew();
			var bookingException = bookingWorkflowProvider.WorkflowItems.Exceptions.AddNew();

			bookingOpenTask.P9_Status = "OPN";
			bookingWorkingTask.P9_Status = "WRK";

			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_ConsolMode = "BCN";

			shipmentDataObject.ContainerMode = new ContainerMode { Code = "BCN" };
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.BuyersConsolLead, Description = "Buyer's Consol Lead" };
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AdditionalTerms = "I SWEAR I'VE CHANGED";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			reader.ReadIntoBusinessObject();

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, bookingMilestone.TriggerConditions.TriggerCondition);
			AssertEquals("false", bookingMilestone.TriggerConditions.TriggerConditionValue);

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, bookingTrigger.TriggerConditions.TriggerCondition);
			AssertEquals("false", bookingTrigger.TriggerConditions.TriggerConditionValue);

			AssertEquals("CAN", bookingOpenTask.P9_Status);
			AssertEquals("CLS", bookingWorkingTask.P9_Status);
			AssertEquals("RSL", bookingException.P9_Status);
		}

		public void TestBookingAttachedToConsol_BookedShippingLineIsPreserved()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = quotedBooking.ForwardingShipment as ForwardingShipment;
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001000";
			shipmentBOToLoad.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
			AssertEquals("Precondition: JS_IsForwardRegistered", false, shipmentBOToLoad.JS_IsForwardRegistered);
			AssertEquals("Precondition: JS_IsBooking", true, shipmentBOToLoad.JS_IsBooking);

			var consolBO = Factory.New<ForwardingConsol>();
			var dataSource = DataContextFactory.New();
			dataSource.AddDataTarget(DataContextType.ForwardingShipment, "S00001000");
			shipmentDataObject.DataContext = dataSource;
			shipmentDataObject.GoodsDescription = "FAKE MONEY";
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			reader.ReadIntoBusinessObject();

			AssertEquals("Goods Description", "FAKE MONEY", shipmentBOToLoad.JS_GoodsDescription);
			AssertEquals("BookedShippingLine should be preserved after attaching to Consol", carrier.MainAddress.PK, shipmentBOToLoad.JS_OA_BookedShippingLineAddress);
		}

		public void TestBookedShipmentAttachedToConsol_BookedShippingLineIsRemoved()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			carrier.MainAddress.OA_Address1 = "Address 1";

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var shipmentBOToLoad = quotedBooking.ForwardingShipment as ForwardingShipment;
			shipmentBOToLoad.JS_UniqueConsignRef = "S00001000";
			shipmentBOToLoad.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;

			var buildConsolHelper = new BuildConsolHelper();
			buildConsolHelper.TurnBookingIntoShipment(shipmentBOToLoad, null, shipmentBOToLoad.PK);
			AssertEquals("Precondition: JS_IsForwardRegistered", true, shipmentBOToLoad.JS_IsForwardRegistered);
			AssertEquals("Precondition: JS_IsBooking", true, shipmentBOToLoad.JS_IsBooking);

			var consolBO = Factory.New<ForwardingConsol>();
			var dataSource = DataContextFactory.New();
			dataSource.AddDataTarget(DataContextType.ForwardingShipment, "S00001000");
			shipmentDataObject.DataContext = dataSource;
			shipmentDataObject.GoodsDescription = "FAKE MONEY";
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			reader.ReadIntoBusinessObject();

			AssertEquals("Goods Description", "FAKE MONEY", shipmentBOToLoad.JS_GoodsDescription);
			AssertEquals("BookedShippingLine Remains", carrier.MainAddress.PK, shipmentBOToLoad.JS_OA_BookedShippingLineAddress);
		}

		public void TestWithTransportLegs()
		{
			var transportLegDataObject = TransportLegDataObjectReaderTest.SetupTransportLeg();
			transportLegDataObject.VesselName = "BUNGA DELIMA";

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipmentDataObject.TransportLegCollection.Add(transportLegDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business Object

			AssertEquals("shipmentBO.Transports.Count", 1, shipmentBO.Transports.Count);

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				TransportLegDataObjectReaderTest.AssertContents(shipmentBO.Transports[0]);
				AssertEquals("transportBO.JW_Vessel", "BUNGA DELIMA", shipmentBO.Transports[0].JW_Vessel);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestShipmentAdditionalAddressInfo()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_UniqueConsignRef = "S00001000";
			shipmentBO.ConsignorPickupAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipmentBO.ConsigneeDeliveryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipmentBO.JS_OA_ExportReceivingDepot = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipmentBO.JS_OA_ImportReleaseDepot = Factory.NewWithValidTestData<OrgAddress>().PK;

			Factory.SaveForTesting();

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S00001000");

			shipmentDataObject.SetAdditionalAddressInfoCollection(() => new List<AdditionalAddressInfo>
			{
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress),
					TransportMode = new CodeDescriptionPair { Code = "ROA", Description = "Road Freight" }
				},
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.DepartureCFSAddress),
					TransportMode = new CodeDescriptionPair { Code = "RAI", Description = "Rail Freight" }
				},
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.ArrivalCFSAddress),
					TransportMode = new CodeDescriptionPair { Code = "IWT", Description = "Inland Waterways" }
				},
				new AdditionalAddressInfo
				{
					AddressType = nameof(DocAddressType.ConsigneePickupDeliveryAddress),
					TransportMode = new CodeDescriptionPair { Code = "RAI", Description = "Rail Freight" }
				}
			});

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBOAfterImport = reader.ReadIntoBusinessObject();

			AssertEquals("ROA", shipmentBOAfterImport.PickupByTransportMode);
			AssertEquals("RAI", shipmentBOAfterImport.CFSDepartureByTransportMode);
			AssertEquals("IWT", shipmentBOAfterImport.CFSArrivalByTransportMode);
			AssertEquals("RAI", shipmentBOAfterImport.DeliveryByTransportMode);
		}

		public void TestWithDates_NotProcessedWhenIsEstimateExisted()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				shipmentDataObject.SetDateCollection(() => new List<Date>());
				shipmentDataObject.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.True, new ZDateTime(2010, 1, 1)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.Received, ZBool.True, new ZDateTime(2010, 1, 2)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.False, new ZDateTime(2010, 1, 3)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, new ZDateTime(2010, 1, 4)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.ShippedOnBoard, ZBool.True, new ZDateTime(2010, 1, 5)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.BillIssued, ZBool.True, new ZDateTime(2010, 1, 6)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.PickupReceiptRequested, ZBool.True, new ZDateTime(2016, 7, 20)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryReceiptRequested, ZBool.True, new ZDateTime(2016, 7, 23)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.PickupDispatchRequested, ZBool.True, new ZDateTime(2016, 7, 24)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryDispatchRequested, ZBool.True, new ZDateTime(2016, 7, 25)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryDueDate, ZBool.True, new ZDateTime(2016, 4, 25)));
				shipmentDataObject.DateCollection.Add(Date.New(DateType.RevisedDeliveryDueDate, ZBool.True, new ZDateTime(2016, 4, 27)));

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull(shipmentBO);

				#region Check Contents of shipment Business Object

				var log = shipmentBO.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DeliveryDateUpdated);
				var originalLog = log.FirstOrDefault(x => x.Parameters[Params.Type] == "Original");
				var revisedLog = log.FirstOrDefault(x => x.Parameters[Params.Type] == "Revised");

				CombineAssertions(() =>
				{
					AssertNotNull(originalLog);
					AssertNotNull(originalLog.Parameters[Params.New]);
					AssertEquals("Changed by Data Import", "Changed by Data Import", originalLog.Parameters[Params.Reason]);
				});

				CombineAssertions(() =>
				{
					AssertNotNull(revisedLog);
					AssertNotNull(revisedLog.Parameters[Params.New]);
					AssertEquals("Changed by Data Import", "Changed by Data Import", revisedLog.Parameters[Params.Reason]);
				});

				CombineAssertions(delegate
				{
					AssertContents(shipmentBO);
					AssertEquals("shipmentBO.JS_A_BKD", ZDateTime.Empty, shipmentBO.JS_A_BKD);
					AssertEquals("shipmentBO.JS_A_RCV", ZDateTime.Empty, shipmentBO.JS_A_RCV);
					AssertEquals("shipmentBO.JS_E_DEP", ZDateTime.Empty, shipmentBO.JS_E_DEP);
					AssertEquals("shipmentBO.JS_E_ARV", ZDateTime.Empty, shipmentBO.JS_E_ARV);
					AssertEquals("shipmentBO.JS_ShippedOnBoardDate", new ZDateTime(2010, 1, 5), shipmentBO.JS_ShippedOnBoardDate);
					AssertEquals("shipmentBO.JS_HouseBillIssueDate", new ZDateTime(2010, 1, 6), shipmentBO.JS_HouseBillIssueDate);
					AssertEquals("shipmentBO.JS_ExportReceivingDepotReceiptRequested", new ZDateTime(2016, 7, 20), shipmentBO.JS_ExportReceivingDepotReceiptRequested);
					AssertEquals("shipmentBO.JS_ImportReleaseDepotReceiptRequested", new ZDateTime(2016, 7, 23), shipmentBO.JS_ImportReleaseDepotReceiptRequested);
					AssertEquals("shipmentBO.JS_ExportReceivingDepotDispatchRequested", new ZDateTime(2016, 7, 24), shipmentBO.JS_ExportReceivingDepotDispatchRequested);
					AssertEquals("shipmentBO.JS_ImportReleaseDepotDispatchRequested", new ZDateTime(2016, 7, 25), shipmentBO.JS_ImportReleaseDepotDispatchRequested);
					AssertEquals("shipmentBO.JS_DeliveryDueDate", new ZDateTime(2016, 4, 25), shipmentBO.JS_DeliveryDueDate);
					AssertEquals("shipmentBO.JS_RevisedDeliveryDueDate", new ZDateTimeOffset(new ZDateTime(2016, 4, 27), DateTimeKind.Local, TimeSpan.Zero), shipmentBO.JS_RevisedDeliveryDueDate);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
				});

				#endregion
			}
		}

		public void TestWithDates_ProcessedWhenIsEstimateExisted()
		{
			shipmentDataObject.SetDateCollection(() => new List<Date>());

			shipmentDataObject.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2010, 1, 1)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.Received, ZBool.False, new ZDateTime(2010, 1, 2)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, new ZDateTime(2010, 1, 3)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, new ZDateTime(2010, 1, 4)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.ShippedOnBoard, ZBool.False, new ZDateTime(2010, 1, 5)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.BillIssued, ZBool.False, new ZDateTime(2010, 1, 6)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.PickupReceiptRequested, ZBool.True, new ZDateTime(2016, 7, 20)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryReceiptRequested, ZBool.True, new ZDateTime(2016, 7, 23)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.PickupDispatchRequested, ZBool.True, new ZDateTime(2016, 7, 24)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryDispatchRequested, ZBool.True, new ZDateTime(2016, 7, 25)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.DeliveryDueDate, ZBool.True, new ZDateTime(2016, 7, 25)));
			shipmentDataObject.DateCollection.Add(Date.New(DateType.RevisedDeliveryDueDate, ZBool.True, new ZDateTime(2016, 7, 26)));

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business Object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_A_BKD", new ZDateTime(2010, 1, 1), shipmentBO.JS_A_BKD);
				AssertEquals("shipmentBO.JS_A_RCV", new ZDateTime(2010, 1, 2), shipmentBO.JS_A_RCV);
				AssertEquals("shipmentBO.JS_E_DEP", new ZDateTime(2010, 1, 3), shipmentBO.JS_E_DEP);
				AssertEquals("shipmentBO.JS_E_ARV", new ZDateTime(2010, 1, 4), shipmentBO.JS_E_ARV);
				AssertEquals("shipmentBO.JS_ShippedOnBoardDate", new ZDateTime(2010, 1, 5), shipmentBO.JS_ShippedOnBoardDate);
				AssertEquals("shipmentBO.JS_HouseBillIssueDate", new ZDateTime(2010, 1, 6), shipmentBO.JS_HouseBillIssueDate);
				AssertEquals("shipmentBO.JS_ExportReceivingDepotReceiptRequested", new ZDateTime(2016, 7, 20), shipmentBO.JS_ExportReceivingDepotReceiptRequested);
				AssertEquals("shipmentBO.JS_ImportReleaseDepotReceiptRequested", new ZDateTime(2016, 7, 23), shipmentBO.JS_ImportReleaseDepotReceiptRequested);
				AssertEquals("shipmentBO.JS_ExportReceivingDepotDispatchRequested", new ZDateTime(2016, 7, 24), shipmentBO.JS_ExportReceivingDepotDispatchRequested);
				AssertEquals("shipmentBO.JS_ImportReleaseDepotDispatchRequested", new ZDateTime(2016, 7, 25), shipmentBO.JS_ImportReleaseDepotDispatchRequested);
				AssertEquals("shipmentBO.JS_DeliveryDueDate", new ZDateTime(2016, 7, 25), shipmentBO.JS_DeliveryDueDate);
				AssertEquals("shipmentBO.JS_RevisedDeliveryDueDate", new ZDateTime(2016, 7, 26), shipmentBO.JS_RevisedDeliveryDueDate.ToZDateTime());
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestWithDates_ProcessedWhenIsEstimateAbsent()
		{
			shipmentDataObject.SetDateCollection(() => new List<Date>());

			var dateForBookingConfirmed = new Date()
			{
				Type = DateType.BookingConfirmed,
				Value = new ZDateTime(2010, 1, 1),
			};
			shipmentDataObject.DateCollection.Add(dateForBookingConfirmed);

			var dateForReceived = new Date()
			{
				Type = DateType.Received,
				Value = new ZDateTime(2010, 1, 2),
			};
			shipmentDataObject.DateCollection.Add(dateForReceived);

			var dateForDeparture = new Date()
			{
				Type = DateType.Departure,
				Value = new ZDateTime(2010, 1, 3),
			};
			shipmentDataObject.DateCollection.Add(dateForDeparture);

			var dateForArrival = new Date()
			{
				Type = DateType.Arrival,
				Value = new ZDateTime(2010, 1, 4),
			};
			shipmentDataObject.DateCollection.Add(dateForArrival);

			var dateForShippedOnBoard = new Date()
			{
				Type = DateType.ShippedOnBoard,
				Value = new ZDateTime(2010, 1, 5),
			};
			shipmentDataObject.DateCollection.Add(dateForShippedOnBoard);

			var dateForBillIssued = new Date()
			{
				Type = DateType.BillIssued,
				Value = new ZDateTime(2010, 1, 6),
			};
			shipmentDataObject.DateCollection.Add(dateForBillIssued);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business Object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertEquals("shipmentBO.JS_A_BKD", new ZDateTime(2010, 1, 1), shipmentBO.JS_A_BKD);
				AssertEquals("shipmentBO.JS_A_RCV", new ZDateTime(2010, 1, 2), shipmentBO.JS_A_RCV);
				AssertEquals("shipmentBO.JS_E_DEP", new ZDateTime(2010, 1, 3), shipmentBO.JS_E_DEP);
				AssertEquals("shipmentBO.JS_E_ARV", new ZDateTime(2010, 1, 4), shipmentBO.JS_E_ARV);
				AssertEquals("shipmentBO.JS_ShippedOnBoardDate", new ZDateTime(2010, 1, 5), shipmentBO.JS_ShippedOnBoardDate);
				AssertEquals("shipmentBO.JS_HouseBillIssueDate", new ZDateTime(2010, 1, 6), shipmentBO.JS_HouseBillIssueDate);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestWithNotes()
		{
			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = false;

			shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
			shipmentDataObject.NoteCollection.Add(noteDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			StmNote[] note = shipmentBO.Notes.FindByDescription("DOG FLOGGER!!");
			AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertNoteContents(note[0]);
				AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipmentStmNote found, creating new ForwardingShipmentStmNote.
Information - Populating ForwardingShipmentStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses()
		{
			var consigneeWithDifferentDocType = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsigneeAddress));
			var matchingOrgAddressDataObjectCRD = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var newOrgAddressDataObjectNPP = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.NotifyParty));
			var unknownOrgAddressDataObjectSTP = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ShipToParty));
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(consigneeWithDifferentDocType);
			shipmentDataObject.OrganizationAddressCollection.Add(matchingOrgAddressDataObjectCRD);
			shipmentDataObject.OrganizationAddressCollection.Add(newOrgAddressDataObjectNPP);
			shipmentDataObject.OrganizationAddressCollection.Add(unknownOrgAddressDataObjectSTP);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			var jobDocAddressBOCNE = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			var jobDocAddressBOCRD = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
			var jobDocAddressBONPP = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);
			var jobDocAddressBOSTP = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ShipToParty);

			AssertEquals("shipmentBO.DocAddresses.Count", 5, shipmentBO.DocAddresses.Count);

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBOCNE);
				AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBOCRD);
				AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBONPP);
				AssertNull(jobDocAddressBOSTP);
				AssertNull("Organisation Collection without the localclient address does not add it in on the shipment business object", shipmentBO.ShipmentJobHeader);
				AssertEquals("jobDocAddressBOCRD.E2_AddressType", "CED", jobDocAddressBOCNE.E2_AddressType);
				AssertEquals("jobDocAddressBOCRD.E2_AddressType", "CRD", jobDocAddressBOCRD.E2_AddressType);
				AssertEquals("jobDocAddressBONPP.E2_AddressType", "NPP", jobDocAddressBONPP.E2_AddressType);
				AssertEquals("jobDocAddressBOCRD.E2_AddressOverride", true, jobDocAddressBOCNE.E2_AddressOverride);
				AssertEquals("jobDocAddressBOCRD.E2_AddressOverride", true, jobDocAddressBOCRD.E2_AddressOverride);
				AssertEquals("jobDocAddressBONPP.E2_AddressOverride", true, jobDocAddressBONPP.E2_AddressOverride);
				AssertEquals("jobDocAddressBOCRD.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBOCNE.E2_OA_Address);
				AssertEquals("jobDocAddressBOCRD.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBOCRD.E2_OA_Address);
				AssertEquals("jobDocAddressBONPP.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBONPP.E2_OA_Address);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'NotifyParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ShipToParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Unknown Address Type [ShipToParty] found. Job Document Address not imported.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestWithOrgAddresses_Remove()
		{
			#region Create test DataObject and BusinessObject

			var shipmentDataObject = SetupShipment(GetResourcePathFor("UniversalShipmentWithAllOrgAddresses.xml"));
			foreach (var orgAddress in shipmentDataObject.OrganizationAddressCollection)
			{
				new OrganisationDataObjectReader(orgAddress, logger, Factory).GetMatchedOrNewForTesting();
			}

			Factory.SaveForTesting();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertNotNull(shipmentDataObject);
			AssertNotNull(shipmentBO);
			AssertNotNull(shipmentBO.ConsignorDocumentaryAddress);
			AssertNotEquals(string.Empty, shipmentBO.ConsignorDocumentaryAddress.E2_Address1);
			AssertNotNull(shipmentBO.ConsigneeDocumentaryAddress);
			AssertNotEquals(string.Empty, shipmentBO.ConsigneeDocumentaryAddress.E2_Address1);
			AssertNotEquals(Guid.Empty, shipmentBO.JobHeader.JH_OA_LocalChargesAddr);
			AssertNotEquals(Guid.Empty, shipmentBO.JobHeader.JH_OA_AgentCollectAddr);
			AssertNotNull(shipmentBO.ExportBroker);
			AssertNotNull(shipmentBO.ExportReceivingDepot);
			AssertNotNull(shipmentBO.DeliveryAgent);
			AssertNotNull(shipmentBO.DocsAndCartage.PickupCartageCoAddr);
			AssertNotNull(shipmentBO.ImportBroker);
			AssertNotNull(shipmentBO.ImportReleaseDepot);
			AssertNotNull(shipmentBO.DocsAndCartage.DeliveryCartageCoAddr);

			#endregion

			foreach (var orgAddress in shipmentDataObject.OrganizationAddressCollection)
			{
				SetEmptyOrgAddress(orgAddress);
			}

			logger.ClearLogs();
			reader = new ShipmentDataObjectReader(shipmentDataObject, logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertNotNull(shipmentBO.ConsignorDocumentaryAddress);
			AssertEquals("Consignor has been reset", string.Empty, shipmentBO.ConsignorDocumentaryAddress.E2_Address1);
			AssertNotNull(shipmentBO.ConsigneeDocumentaryAddress);
			AssertEquals("Consignee has been reset", string.Empty, shipmentBO.ConsigneeDocumentaryAddress.E2_Address1);
			AssertEquals(Guid.Empty, shipmentBO.JobHeader.JH_OA_LocalChargesAddr);
			AssertEquals(Guid.Empty, shipmentBO.JobHeader.JH_OA_AgentCollectAddr);
			AssertNull(shipmentBO.ExportBroker);
			AssertNull(shipmentBO.ExportReceivingDepot);
			AssertNull(shipmentBO.DeliveryAgent);
			AssertNull(shipmentBO.DocsAndCartage.PickupCartageCoAddr);
			AssertNull(shipmentBO.ImportBroker);
			AssertNull(shipmentBO.ImportReleaseDepot);
			AssertNull(shipmentBO.DocsAndCartage.DeliveryCartageCoAddr);

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Set 'ConsignorDocumentaryAddress' to Empty.
Information - Set 'ConsigneeDocumentaryAddress' to Empty.
Information - Set 'LocalClient' to Empty.
Information - Set 'OverseasAgent' to Empty.
Information - Set 'ExportBroker' to Empty.
Information - Set 'ImportBroker' to Empty.
Information - Set 'DepartureCFSAddress' to Empty.
Information - Set 'DeliveryAgent' to Empty.
Information - Set 'PickupLocalCartage' to Empty.
Information - Set 'ArrivalCFSAddress' to Empty.
Information - Set 'DeliveryLocalCartage' to Empty.
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001000 (House Bill='FRED235478923') from UniversalShipment.
".Trim(), logger.Logs);
		}

		void SetEmptyOrgAddress(IDataObject orgAddress)
		{
			SetEmptyValues(orgAddress, "AddressType", "AddressOverride");
		}

		void SetEmptyValues(IDataObject dataObject, params string[] exceptionPropertyNames)
		{
			foreach (var property in dataObject.GetType().GetProperties())
			{
				if (!exceptionPropertyNames.Contains(property.Name))
				{
					property.SetValue(dataObject, null);
				}
			}
		}

		public void TestPackLineToContainerLink_PackLinesWithAndWithoutContainers_PackLinesWithContainerLinkShouldLinkToContainers()
		{
			var packingLineDataObject1 = SetupPackingLine(Factory);
			var packingLineDataObject2 = SetupPackingLine(Factory);

			packingLineDataObject1.Commodity.Code = "HAM";
			packingLineDataObject2.Commodity.Code = "BUT";

			var factory = new UniversalObjectFactory();

			var containerDataObject1 = ContainerDataObjectTestHelper.SetupContainerWithVGM(factory);
			var containerDataObject2 = ContainerDataObjectTestHelper.SetupContainerWithVGM(factory);

			containerDataObject1.ContainerNumber = "HAM";
			containerDataObject2.ContainerNumber = "BUT";

			packingLineDataObject1.ContainerLink = 1;
			containerDataObject1.Link = 1;

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject1, containerDataObject2 });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLineDataObject1, packingLineDataObject2 });

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			var hamContainer = shipmentBO.Consols[0].Containers.Cast<ForwardingContainer>().FirstOrDefault(c => c.JC_ContainerNum == "HAM");
			var butContainer = shipmentBO.Consols[0].Containers.Cast<ForwardingContainer>().FirstOrDefault(c => c.JC_ContainerNum == "BUT");
			var hamPackLine = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(c => c.JL_RH_NKCommodityCode == "HAM");
			var butPackLine = shipmentBO.OuterPackLines.Cast<ForwardingPackLine>().FirstOrDefault(c => c.JL_RH_NKCommodityCode == "BUT");

			AssertNotNull(shipmentBO);
			AssertEquals("shipmentBO.Containers.Count", 2, shipmentBO.Consols[0].Containers.Count);
			AssertEquals("shipmentBO.OuterPackLines.Count", 2, shipmentBO.OuterPackLines.Count);

			AssertEquals("First packline should link to container", hamContainer.PK, hamPackLine.GetContainer(shipmentBO.Consols[0]).PK);
			AssertNull("Second packline should not link to container", butPackLine.GetContainer(shipmentBO.Consols[0]));
		}

		[ExpectNoExceptions]
		public void TestPackLineIsAllocatedToContainerWithNoJobConShipLinkWithConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "XXX123";

			var containerDataObject1 = ContainerDataObjectTestHelper.SetupContainerWithVGM();

			var packingLineDataObject1 = SetupPackingLine(Factory);
			packingLineDataObject1.Commodity.Code = "BUN";

			packingLineDataObject1.ContainerLink = 1;
			containerDataObject1.Link = 1;

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject1 });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLineDataObject1 });
			SetupCFSAddresses(shipmentDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consol));
			reader.ReadIntoBusinessObject();
		}

		[ExpectNoExceptions]
		public void TestPackLineIsAllocatedToContainerWithNoJobConShipLinkWithoutConsol()
		{
			var packingLineDataObject1 = SetupPackingLine(Factory);
			packingLineDataObject1.Commodity.Code = "HAM";

			var containerDataObject1 = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			containerDataObject1.ContainerNumber = "CDFG4564563";

			packingLineDataObject1.ContainerLink = 1;
			containerDataObject1.Link = 1;

			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDataObject1 });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLineDataObject1 });
			SetupCFSAddresses(shipmentDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			reader.ReadIntoBusinessObject();
		}

		public void TestTransportLegsAreAddedOnTheCorrectParent()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Add(containerDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			var transportBO = Factory.New<Transport>();
			shipmentBO.Consols[0].Transports.Add(transportBO);
			transportBO.JW_RL_NKLoadPort = "NZAKL";
			transportBO.JW_RL_NKDiscPort = "AUMEL";

			Factory.SaveForTesting();

			shipmentDataObject.SetContainerCollection(() => null);

			var transportLegDataObject = TransportLegDataObjectReaderTest.SetupTransportLeg();
			transportLegDataObject.VesselName = "BUNGA DELIMA";

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipmentDataObject.TransportLegCollection.Add(transportLegDataObject);

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(shipmentBO.Consols[0]));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			AssertEquals("shipmentBO.Consols[0].Transports.Count", 2, shipmentBO.Consols[0].Transports.Count);

			CombineAssertions(delegate
			{
				TransportLegDataObjectReaderTest.AssertContents(shipmentBO.Consols[0].Transports[1]);
				AssertEquals("transportBO.JW_Vessel", "BUNGA DELIMA", shipmentBO.Consols[0].Transports[1].JW_Vessel);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestTransportsAreAddedOrLoadedOnTheCorrectConsols()
		{
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.PortOfLoading = new UNLOCO() { Code = "SGSIN", Name = "Singapore" };
			shipmentDataObject.PortOfDischarge = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };

			var consolBO1 = Factory.New<ForwardingConsol>();
			consolBO1.JK_RL_NKLoadPort = "AUSYD";
			consolBO1.JK_RL_NKDischargePort = "NZCHC";

			var consolBO2 = Factory.New<ForwardingConsol>();
			consolBO2.JK_RL_NKLoadPort = "NZAKL";

			var consolBO3 = Factory.New<ForwardingConsol>();
			consolBO3.JK_RL_NKLoadPort = "NZAKL";
			consolBO3.JK_RL_NKDischargePort = "AUMEL";

			var consolBO4 = Factory.New<ForwardingConsol>();
			consolBO4.JK_RL_NKDischargePort = "AUMEL";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			shipmentBO.Consols.Add(consolBO1);
			shipmentBO.Consols.Add(consolBO2);
			shipmentBO.Consols.Add(consolBO3);
			shipmentBO.Consols.Add(consolBO4);

			Factory.SaveForTesting();

			var transportLegDataObject = TransportLegDataObjectReaderTest.SetupTransportLeg();
			transportLegDataObject.VesselName = "THIS FIELD WILL NOT BE TOUCHED";

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipmentDataObject.TransportLegCollection.Add(transportLegDataObject);

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO3));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO3.Transports.Count", 1, consolBO3.Transports.Count);
			AssertEquals("consolBO3.Transports[0].JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", consolBO3.Transports[0].JW_Vessel);

			shipmentBO.Consols.RemoveAndDelete(consolBO3);

			Factory.SaveForTesting();

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO4));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO4.Transports.Count", 2, consolBO4.Transports.Count);
			AssertEquals("consolBO4.Transports[1].JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", consolBO4.Transports[1].JW_Vessel);

			shipmentBO.Consols.RemoveAndDelete(consolBO4);

			Factory.SaveForTesting();

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO2));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO2.Transports.Count", 2, consolBO2.Transports.Count);
			AssertEquals("consolBO2.Transports[1].JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", consolBO2.Transports[1].JW_Vessel);

			shipmentBO.Consols.RemoveAndDelete(consolBO2);

			Factory.SaveForTesting();

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO1));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO1.Transports.Count", 1, consolBO1.Transports.Count);
			AssertNotEquals("consolBO1.Transports[0].JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", consolBO1.Transports[0].JW_Vessel);
			AssertEquals("shipmentBO.Transports.Count", 1, shipmentBO.Transports.Count);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				TransportLegDataObjectReaderTest.AssertContents(shipmentBO.Transports[0]);
				AssertEquals("shipmentBO.Transports[0].JW_Vessel", "THIS FIELD WILL NOT BE TOUCHED", shipmentBO.Transports[0].JW_Vessel);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestConsolsAreMatchedCorrectly()
		{
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.PortOfDischarge = new UNLOCO() { Code = "H_H", Name = "Hallelujah" };
			shipmentDataObject.PortOfLoading = new UNLOCO() { Code = "L_L", Name = "Laugh Of Loudness" };

			var consolBO1 = Factory.New<ForwardingConsol>();
			consolBO1.JK_RL_NKLoadPort = "NO_";
			consolBO1.JK_RL_NKDischargePort = "H_E";

			var consolBO2 = Factory.New<ForwardingConsol>();
			consolBO2.JK_RL_NKLoadPort = "L_L";

			var consolBO3 = Factory.New<ForwardingConsol>();
			consolBO3.JK_RL_NKLoadPort = "L_L";
			consolBO3.JK_RL_NKDischargePort = "H_H";

			var consolBO4 = Factory.New<ForwardingConsol>();
			consolBO4.JK_RL_NKDischargePort = "H_H";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			shipmentBO.Consols.Add(consolBO1);
			shipmentBO.Consols.Add(consolBO2);
			shipmentBO.Consols.Add(consolBO3);
			shipmentBO.Consols.Add(consolBO4);

			Factory.SaveForTesting();

			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			shipmentDataObject.ContainerCollection.Add(containerDataObject);

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO3));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO3.Containers.Count", 1, consolBO3.Containers.Count);

			shipmentBO.Consols.RemoveAndDelete(consolBO3);

			Factory.SaveForTesting();

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO4));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO4.Containers.Count", 1, consolBO4.Containers.Count);

			shipmentBO.Consols.RemoveAndDelete(consolBO4);

			Factory.SaveForTesting();

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO2));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO2.Containers.Count", 1, consolBO2.Containers.Count);

			shipmentBO.Consols.RemoveAndDelete(consolBO2);

			Factory.SaveForTesting();

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO1));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("consolBO1.Containers.Count", 1, consolBO1.Containers.Count);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				ContainerDataObjectTestHelper.AssertContents(consolBO1.Containers[0]);
				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment S00001000 (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestTransportLegMismatchConsolTransport()
		{
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			shipmentDataObject.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			shipmentDataObject.PortOfDischarge = new UNLOCO() { Code = "NZAKL", Name = "Auckland" };

			var consolBO1 = Factory.New<ForwardingConsol>();
			consolBO1.JK_RL_NKLoadPort = "AUSYD";
			consolBO1.JK_RL_NKDischargePort = "NZCHC";

			var consolBO2 = Factory.New<ForwardingConsol>();
			consolBO2.JK_RL_NKLoadPort = "AUSYD";
			consolBO2.JK_RL_NKDischargePort = "USLAX";

			shipmentBO.Consols.Add(consolBO1);
			shipmentBO.Consols.Add(consolBO2);

			var transportBO = consolBO2.Transports.AddNew();
			transportBO.JW_RL_NKLoadPort = "NZAKL";
			transportBO.JW_RL_NKDiscPort = "AUMEL";

			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transportLegDataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL", Name = "Auckland" };
			transportLegDataObject.PortOfDischarge = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };

			shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>() { transportLegDataObject });

			Factory.SaveForTesting();

			var result = reader.ReadIntoBusinessObject();

			AssertEquals("Should not contain any transport as the matching parent is not shipment.", 0, result.Transports.Count);
		}

		public void TestGetBestMatchingConsolHasTheRightResult()
		{
			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			var consolBO1 = Factory.New<ForwardingConsol>();
			consolBO1.JK_RL_NKLoadPort = "AUSYD";
			consolBO1.JK_RL_NKDischargePort = "AUMEL";

			var consolBO2 = Factory.New<ForwardingConsol>();
			consolBO2.JK_RL_NKLoadPort = "NZAKL";
			consolBO2.JK_RL_NKDischargePort = "USLAX";

			shipmentBO.Consols.Add(consolBO1);
			shipmentBO.Consols.Add(consolBO2);

			var transportLegDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			transportLegDataObject.PortOfLoading = new UNLOCO() { Code = "NZAKL", Name = "Auckland" };
			transportLegDataObject.PortOfDischarge = new UNLOCO() { Code = "AUMEL", Name = "Melbourne" };

			Factory.SaveForTesting();

			var finder = new ConsolFinder<ForwardingConsol>(shipmentBO);
			var result1 = finder.GetBestMatchingParentForTransport(transportLegDataObject);

			AssertEquals("Should be matching the higher score in the finder.", result1.PK, consolBO1.PK);
		}

		public void TestExtraConsolsAreNotCreatedWhenImportingContainers()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();

				shipmentDataObject.WayBillNumber = "MYHOUSE";
				shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
				shipmentDataObject.ContainerCollection.Add(containerDataObject);

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();

				AssertNotNull(shipmentBO);

				CombineAssertions(delegate
				{
					AssertContents(shipmentBO);
					AssertEquals("shipmentBO.Consols.Count", 1, shipmentBO.Consols.Count);
					AssertEquals("shipmentBO.Consols[0].Containers.Count", 1, shipmentBO.Consols[0].Containers.Count);
					ContainerDataObjectTestHelper.AssertContents(shipmentBO.Consols[0].Containers[0]);
				});

				reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, new UniversalObjectFactory(), ChildShipmentsParent.ToChildShipmentsParent(shipmentBO.Consols[0]));
				shipmentBO = reader.ReadIntoBusinessObject();

				AssertNotNull(shipmentBO);

				#region Check Contents of shipment Business object

				AssertEquals("shipmentBO.Consols.Count", 1, shipmentBO.Consols.Count);
				AssertEquals("shipmentBO.Consols[0].Containers.Count", 1, shipmentBO.Consols[0].Containers.Count);

				CombineAssertions(delegate
				{
					AssertContents(shipmentBO);
					ContainerDataObjectTestHelper.AssertContents(shipmentBO.Consols[0].Containers[0]);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Successfully loaded matching ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - Successfully loaded matching JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
				});

				#endregion
			}
		}

		public void TestLinkOrderLineWithPackProduct()
		{
			var packedItemDataObject1 = new PackedItem
			{
				PackedQuantity = 1,
				OrderLineLink = 1,
				UnitOfQuantity = new PackageType { Code = "CNT" },
				Product = new Product { Code = "Camel Poodles" }
			};

			var packedItemDataObject2 = new PackedItem
			{
				PackedQuantity = 1,
				OrderLineLink = 2,
				UnitOfQuantity = new PackageType { Code = "CNT" },
				Product = new Product { Code = "Panda Chow Chows" }
			};

			var packedItemDataObject3 = new PackedItem
			{
				PackedQuantity = 1,
				OrderLineLink = 3,
				UnitOfQuantity = new PackageType { Code = "CNT" },
				Product = new Product { Code = "Dog Wow" }
			};

			var packingLineDataObject = SetupPackingLine(Factory);
			packingLineDataObject.PackQty = 3;

			packingLineDataObject.SetPackedItemCollection(() => new List<PackedItem>
			{
				packedItemDataObject1,
				packedItemDataObject2,
				packedItemDataObject3
			});

			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipmentDataObject.PackingLineCollection.Add(packingLineDataObject);

			var orderLineDataObject1 = new OrderLine
			{
				LineNumber = 1,
				Link = 1,
				Volume = 0.1m,
				OrderedQty = 1.1m
			};

			var orderLineDataObject2 = new OrderLine
			{
				LineNumber = 2,
				Link = 2,
				Volume = 2.1m,
				OrderedQty = 2.2m
			};

			var orderLineDataObject3 = new OrderLine
			{
				LineNumber = 3,
				Link = 3,
				Volume = 3.3m,
				OrderedQty = 3.4m
			};

			var orderData = GetNewOrderDataObject();
			orderData.Order = new UniversalOrder(DefaultDataObjectWriterStrategy.TestInstance);
			orderData.Order.OrderNumberSplit = new ZByte(2);

			orderData.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>
			{
				orderLineDataObject1,
				orderLineDataObject2,
				orderLineDataObject3
			});

			shipmentDataObject.SetRelatedShipmentCollection(() => new List<UniversalShipment>());
			shipmentDataObject.RelatedShipmentCollection.Add(orderData);
			SetupCFSAddresses(shipmentDataObject);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(1, shipmentBO.OuterPackLines.Count);
			AssertEquals(3, shipmentBO.OuterPackLines[0].Products.Count);
			var product1 = shipmentBO.OuterPackLines[0].Products[0];
			AssertEquals("Camel Poodles", product1.D2_ProductCode);
			var product2 = shipmentBO.OuterPackLines[0].Products[1];
			AssertEquals("Panda Chow Chows", product2.D2_ProductCode);
			var product3 = shipmentBO.OuterPackLines[0].Products[2];
			AssertEquals("Dog Wow", product3.D2_ProductCode);

			AssertEquals(1, shipmentBO.AttachedOrders.Count);
			AssertEquals(3, shipmentBO.AttachedOrders[0].OrderLines.Count);
			AssertEquals(0.1m, shipmentBO.AttachedOrders[0].OrderLines[0].JO_ActualVolume);
			AssertEquals(shipmentBO.AttachedOrders[0].OrderLines[0].PK, product1.D2_JO);
			AssertEquals(2.1m, shipmentBO.AttachedOrders[0].OrderLines[1].JO_ActualVolume);
			AssertEquals(shipmentBO.AttachedOrders[0].OrderLines[1].PK, product2.D2_JO);
			AssertEquals(3.3m, shipmentBO.AttachedOrders[0].OrderLines[2].JO_ActualVolume);
			AssertEquals(shipmentBO.AttachedOrders[0].OrderLines[2].PK, product3.D2_JO);
		}

		public void TestExtraPackLinesAreNotCreatedWhenImportingPackLines()
		{
			var packingLineDataObject = SetupPackingLine(Factory);

			SetupCFSAddresses(shipmentDataObject);
			shipmentDataObject.WayBillNumber = "MYHOUSE";
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			shipmentDataObject.PackingLineCollection.Add(packingLineDataObject);

			Factory.SaveForTesting();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			var consolBO = Factory.New<ForwardingConsol>();

			AssertNotNull(shipmentBO);
			AssertEquals("shipmentBO.OuterPackLines.Count", 1, shipmentBO.OuterPackLines.Count);

			consolBO.GridShipments.Add(shipmentBO);

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);
				AssertContents(shipmentBO.OuterPackLines[0]);
			});

			packingLineDataObject.UNDGCollection.Clear();
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			AssertEquals("shipmentBO.OuterPackLines.Count", 1, shipmentBO.OuterPackLines.Count);

			AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'DepartureCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - Matching 'ArrivalCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Matching 'LastKnownCFSFacility':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no contact found in database with name 'Telepuzik' and phone '' for dangerous goods substance code '3000c' (IMO Class = ''). Make sure that name and phone is not empty. If not create contact first.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no substance with code '3001' found. Please use standard dangerous goods substance code.
Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Matching 'DepartureCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - Matching 'ArrivalCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - Successfully loaded matching ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Matching 'LastKnownCFSFacility':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - Updated Shipment (House Bill='MYHOUSE') from UniversalShipment.
".Trim(), Logger.Logs);
		}
		public void TestImportUNDGDataItems_AirTransport_NoStandard()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "SUB1";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;

			shipmentDataObject.TransportMode = new CodeDescriptionPair
			{
				Code = Core.Constants.TransportModes.Air
			};

			var packLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetPackingLineCollection(() =>
			{
				return new DataObjectList<PackingLine>
				{
					packLine
				};
			});

			packLine.SetUNDGCollection(() =>
			{
				return new List<UNDG>
				{
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
					{
						UNDGCode = substance.DG_Code
					}
				};
			});

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(1, shipmentBO.OuterPackLines.Count);
			AssertEquals(1, shipmentBO.OuterPackLines[0].UNDGs.Count);
			AssertEquals("Read IAT UNDG item for air transport correctly", substance.PK, shipmentBO.OuterPackLines[0].UNDGs[0].DI_DG);
			AssertEquals(1, shipmentBO.OuterPackLines[0].UNDGs[0].UNDGSubstancePivotCollection.Count);
			AssertEquals("Pivot create correctly", "IAT", shipmentBO.OuterPackLines[0].UNDGs[0].UNDGSubstancePivotCollection[0].DP_Standard);
		}

		public void TestWithAllCollectionsFilledOnShipment()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				var localClientAddress = GetLocalClientAddress();
				var addressBO = new OrganisationDataObjectReader(localClientAddress, Logger, Factory).GetMatchedOrNewForTesting();

				Factory.SaveForTesting();

				var transportLegDataObject = TransportLegDataObjectReaderTest.SetupTransportLeg();
				transportLegDataObject.VesselName = "BUNGA DELIMA";

				shipmentDataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
				shipmentDataObject.TransportLegCollection.Add(transportLegDataObject);

				var noteDataObject = SetupNote();
				noteDataObject.IsCustomDescription = false;

				shipmentDataObject.SetNoteCollection(() => new DataObjectList<Note>());
				shipmentDataObject.NoteCollection.Add(noteDataObject);

				var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
				containerDataObject.Link = 1;

				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());
				shipmentDataObject.ContainerCollection.Add(containerDataObject);

				var packingLineDataObject = SetupPackingLine(Factory);
				packingLineDataObject.ContainerLink = 1;

				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
				shipmentDataObject.PackingLineCollection.Add(packingLineDataObject);

				var newOrgAddressDataObjectNPP = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.NotifyParty));

				shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				shipmentDataObject.OrganizationAddressCollection.Add(localClientAddress);
				shipmentDataObject.OrganizationAddressCollection.Add(newOrgAddressDataObjectNPP);
				SetupCFSAddresses(shipmentDataObject);
				Factory.SaveForTesting();

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();

				AssertNotNull(shipmentBO);

				#region Check Contents of shipment Business object

				StmNote[] note = shipmentBO.Notes.FindByDescription("DOG FLOGGER!!");

				AssertEquals("shipmentBO.Transports.Count", 1, shipmentBO.Transports.Count);
				AssertEquals("Note Collection contains 'DOG FLOGGER!!' note.", 1, note.Length);
				AssertEquals("shipmentBO.Consols[0].Containers.Count", 1, shipmentBO.Consols[0].Containers.Count);
				AssertEquals("shipmentBO.Containers.Count()", 1, shipmentBO.Containers.Count());
				AssertEquals("shipmentBO.OuterPackLines.Count", 1, shipmentBO.OuterPackLines.Count);
				AssertEquals("shipmentBO.DocAddresses.Count", 6, shipmentBO.DocAddresses.Count);
				AssertNotNull(shipmentBO.ShipmentJobHeader);
				AssertNotNull(shipmentBO.ShipmentJobHeader.LocalChargesAddr);
				AssertNotNull(shipmentBO.ShipmentJobHeader.LocalChargesAddr.Header);

				var jobDocAddressBONPP = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty);

				CombineAssertions(delegate
				{
					AssertContents(shipmentBO);
					TransportLegDataObjectReaderTest.AssertContents(shipmentBO.Transports[0]);
					AssertEquals("transportBO.JW_Vessel", "BUNGA DELIMA", shipmentBO.Transports[0].JW_Vessel);
					AssertNoteContents(note[0]);
					AssertEquals("noteBO.ST_IsCustomDescription", true, note[0].ST_IsCustomDescription);
					ContainerDataObjectTestHelper.AssertContents(shipmentBO.Containers.First());
					AssertContents(shipmentBO.OuterPackLines[0]);
					AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddressBONPP);
					AssertLocalClientAddress(shipmentBO.ShipmentJobHeader.LocalChargesAddr);
					AssertEquals("jobDocAddressBONPP.E2_AddressType", "NPP", jobDocAddressBONPP.E2_AddressType);
					AssertEquals("jobDocAddressBONPP.E2_AddressOverride", true, jobDocAddressBONPP.E2_AddressOverride);
					AssertEquals("jobDocAddressBONPP.E2_OA_Address", MiscOrgAddressPK, jobDocAddressBONPP.E2_OA_Address);
					AssertMultilineASCIIEquals("Logger.Logs", @"
Warning - Matching 'LocalClient':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipmentStmNote found, creating new ForwardingShipmentStmNote.
Information - Populating ForwardingShipmentStmNote...
Warning - Description(value: DOG FLOGGER!!) is a custom note. IsCustomDescription(value: False/empty) was ignored.
Information - No matching ForwardingContainer found, creating new ForwardingContainer.
Information - Populating ForwardingContainer...
Warning - Container Type 'ZW0W' is invalid.
Information - Matching 'ContainerYardEmptyPickupAddress':- Matched to 'XVBQP68SIYXQ' by code, address 'DEP123' (only address).
Information - Matching 'ContainerYardEmptyReturnAddress':- Matched to 'H5ZX52PAMCOI' by code, address 'ARV456' (only address).
Information - No matching JobService found, creating new JobService.
Information - Populating JobService...
Information - Matching 'FOO':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'GrossWeightVerifiedBy':- Matched to 'ZGP5LX5SQPEB' by code, address 'VGM789' (only address).
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: AUMEL
Information - Attempting to get Schedule for the Transport Leg
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - An existing Schedule could not be found. The Transport Leg will not be linked.
Warning - Matching 'Fooey':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'CreditorSYD':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - Transport Leg updated.
Information - Matching 'LocalClient':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'DepartureCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - Matching 'ArrivalCFSAddress':- Matched to 'OH1' by code, address 'AD1' by short code.
Warning - Matching 'NotifyParty':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Information - No matching ForwardingPackLine found, creating new ForwardingPackLine.
Information - Populating ForwardingPackLine...
Information - Matching 'LastKnownCFSFacility':- Matched to 'OH1' by code, address 'AD1' by short code.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no contact found in database with name 'Telepuzik' and phone '' for dangerous goods substance code '3000c' (IMO Class = ''). Make sure that name and phone is not empty. If not create contact first.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - There is no substance with code '3001' found. Please use standard dangerous goods substance code.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
				});

				Factory.SaveAtEndOfImport(Logger);
				#endregion
			}
		}

		public void TestDeclarationSourceIsNotImportedAsSubShipment()
		{
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			var decShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = decShipment.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.CustomsDeclaration, "DECSHIPMENT1");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			decShipment.GoodsDescription = "DEC GOODS";
			shipmentDataObject.SubShipmentCollection.Add(decShipment);

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataSource = subShipment.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT1");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment.GoodsDescription = "SUB GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CoLoadShipments.Count", 1, shipmentBO.CoLoadShipments.Count);
				var coloadShipment = shipmentBO.CoLoadShipments[0];
				AssertEquals("coloadShipment.JS_GoodsDescription", "SUB GOODS", coloadShipment.JS_GoodsDescription);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestImportSubShipmentForForwardingShipmentDataTarget()
		{
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataTarget = subShipment1.DataContext = DataContextFactory.New();
			dataTarget.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataTarget.AddDataTarget(DataContextType.ForwardingShipment, null);
			subShipment1.GoodsDescription = "SUB1 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment1);

			var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataTarget = subShipment2.DataContext = DataContextFactory.New();
			dataTarget.AddDataTarget(DataContextType.HVLVConsignment, null);
			dataTarget.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment2.GoodsDescription = "SUB2 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment2);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CoLoadShipments.Count", 1, shipmentBO.CoLoadShipments.Count);
				var coloadShipment = shipmentBO.CoLoadShipments[0];
				AssertEquals("coloadShipment.JS_GoodsDescription", "SUB1 GOODS", coloadShipment.JS_GoodsDescription);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestImportSubShipmentBasedOnDataSourceIfDataTargetMissing()
		{
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = subShipment1.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT1");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment1.GoodsDescription = "SUB1 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment1);

			var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataSource = subShipment2.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.HVLVConsignment, "SUBSHIPMENT2");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment2.GoodsDescription = "SUB2 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment2);

			var subShipment3 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment3.GoodsDescription = "SUB3 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment3);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CoLoadShipments.Count", 2, shipmentBO.CoLoadShipments.Count);
				AssertEquals("Shipment with matching DataSource is imported", "SUB1 GOODS", shipmentBO.CoLoadShipments[0].JS_GoodsDescription);
				AssertEquals("Shipment without DataSource is imported", "SUB3 GOODS", shipmentBO.CoLoadShipments[1].JS_GoodsDescription);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestImportSubShipmentWithMultipleDataSources()
		{
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = subShipment1.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT1");
			dataSource.AddDataSource(DataContextType.CustomsDeclaration, "DECLARATION");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment1.GoodsDescription = "SUB1 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment1);

			var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataSource = subShipment2.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT2");
			dataSource.AddDataSource(DataContextType.HVLVConsignment, "HVLVCONSIGNMENT");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment2.GoodsDescription = "SUB2 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment2);

			var subShipment3 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataSource = subShipment3.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT3");
			dataSource.AddDataSource(DataContextType.InBond, "INBOND");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment3.GoodsDescription = "SUB3 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment3);

			var subShipment4 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataSource = subShipment4.DataContext = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT4");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			subShipment4.GoodsDescription = "SUB4 GOODS";
			shipmentDataObject.SubShipmentCollection.Add(subShipment4);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentBO.CoLoadShipments.Count", 1, shipmentBO.CoLoadShipments.Count);
				AssertEquals("Only shipment with single DataSource is imported", "SUB4 GOODS", shipmentBO.CoLoadShipments[0].JS_GoodsDescription);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});
		}

		public void TestSubShipmentWouldNotBeAddedToColoadCollectionIfAlreadyExistThere()
		{
			var dataSource = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "SUBSHIPMENT1");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = dataSource;
			subShipment.WayBillNumber = "SUBSHIP";
			subShipment.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			subShipment.GoodsDescription = "SUB GOODS";

			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("shipmentBO.CoLoadShipments.Count", 1, shipmentBO.CoLoadShipments.Count);
			AssertEquals("SUBSHIP", shipmentBO.CoLoadShipments[0].JS_HouseBill);
			AssertEquals("SUB GOODS", shipmentBO.CoLoadShipments[0].JS_GoodsDescription);

			subShipment.GoodsDescription = "UPDATED SUB GOODS";

			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Subshipment was not added again", 1, shipmentBO.CoLoadShipments.Count);
			AssertEquals("SUBSHIP", shipmentBO.CoLoadShipments[0].JS_HouseBill);
			AssertEquals("Subshipment was updated", "UPDATED SUB GOODS", shipmentBO.CoLoadShipments[0].JS_GoodsDescription);
		}

		public void TestSubShipmentWouldNotBeAddedToColoadCollectionIfParentExixt()
		{
			var coloadShipment = Factory.BOFactory.NewWithValidTestData<ForwardingShipment>();
			coloadShipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.CoLoadMaster;

			var shipment = Factory.BOFactory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001000";
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_HouseBill = "SUBSHIPMENT";
			shipment.JS_JS_ColoadMasterShipment = coloadShipment.PK;

			Factory.SaveForTesting();

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataSource = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.ForwardingShipment, "S00001000");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			subShipment.DataContext = dataSource;
			subShipment.WayBillNumber = "SUBSHIPMENT";
			subShipment.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);

			AssertExceptionThrown<DataObjectReadFailureException>("Shipment (S00001000) is already linked to another job. XML rejected as an invalid link would be created between CLD and STD shipments."
				, () => reader.ReadIntoBusinessObject());
		}

		[TestDate(2011, 12, 1)]
		public void TestImportingOfChargeLines()
		{
			using (Factory.BOFactory.AddDisposableService())
			{
				shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001000");
				shipmentDataObject.DataContext.CodesMappedToTarget = true; // Required to import JobCosting

				shipmentDataObject.AdditionalTerms = "Add Me Some Terms";
				shipmentDataObject.WayBillNumber = "MYHOUSE";
				shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };

				shipmentDataObject.JobCosting = new JobCosting(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.JobCosting.Branch = new Branch();
				shipmentDataObject.JobCosting.Branch.Code = "SYD";
				shipmentDataObject.JobCosting.SetChargeLineCollection(() => new List<ChargeLine>());
				ChargeLine chargeLine1 = GetChargeLine("SYD", "FRT", "001", new ZDateTime(2011, 10, 04), null, new ZDateTime(2011, 10, 04), 100.00m, 100.00m, "AUD", null,
					"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, null, "FIN", 100.00m, 100.00m, "AUD", null);
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine1);
				ChargeLine chargeLine2 = GetChargeLine("SYD", "BAF", "001", new ZDateTime(2011, 10, 04), null, new ZDateTime(2011, 10, 04), 200.00m, 200.00m, "AUD", null,
					"AALSHI", "ABIGAS", "FIS", "Charge Description 1", 1, null, "FIN", 250.00m, 250.00m, "AUD", null);
				shipmentDataObject.JobCosting.ChargeLineCollection.Add(chargeLine2);
				chargeLine1.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine1.ImportMetaData.Instruction = InstructionType.Insert;
				chargeLine2.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance);
				chargeLine2.ImportMetaData.Instruction = InstructionType.Insert;

				JobHeader[] jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "S00001000").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 0, jobs.Length);

				JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery());
				AssertEquals("charges.Length", 0, charges.Length);

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(Logger);

				AssertNotNull(shipmentBO);

				#region Check Contents of shipment Business object

				CombineAssertions(delegate
				{
					AssertContents(shipmentBO);
					AssertEquals("shipmentBO.JS_AdditionalTerms", "Add Me Some Terms", shipmentBO.JS_AdditionalTerms);
					AssertEquals("shipmentBO.JS_HouseBill", "MYHOUSE", shipmentBO.JS_HouseBill);
					Assert("No shipmentBO.Containers", !shipmentBO.Containers.Any());

					AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Whilst importing Charge Line: Job Number= Charge Code=FRT Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=100.00 Sell OS Amount=100.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.

Warning - Whilst importing Charge Line: Job Number= Charge Code=BAF Creditor=AALSHI, Debtor=ABIGAS Cost OS Amount=200.00 Sell OS Amount=250.00
Warning - Override Comment: You have not entered an Override Comment.
Warning - Description: Charge description was changed from default. This description will appear on AR Invoice without translation.
Warning - Debtor: You have selected a Debtor that is neither your Local Client or your Overseas Agent.
Whilst invoicing any party is valid, this should be confirmed.
Warning - Revenue Override Comment: You have not entered a Revenue Override Comment.

Information - Added Shipment (House Bill='MYHOUSE') from UniversalShipment.
Information - Successfully saved Shipment S00001000 (House Bill='MYHOUSE').
".Trim(), Logger.Logs);
				});

				#endregion

				jobs = Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_JobNum, "S00001000").AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("jobs.Length", 1, jobs.Length);

				charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, jobs[0].PK));
				AssertEquals("charges.Length", 2, charges.Length);

				JobCharge fRTCharge;
				JobCharge bAFCharge;

				if (charges[0].ChargeCode.AC_Code == "FRT")
				{
					fRTCharge = charges[0];
					bAFCharge = charges[1];
				}
				else
				{
					fRTCharge = charges[1];
					bAFCharge = charges[0];
				}

				AssertCharge(fRTCharge, chargeLine1);
				AssertCharge(bAFCharge, chargeLine2);
			}
		}

		public void TestImportAdditionalReferenceWithIsAutomation()
		{
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ShipmentAutomaticImportOptions.Code.Update);

			var customsReferenceNumberTypeCollection = new CustomsReferenceNumberTypeCollection();
			var customsReferenceNumberType1 = customsReferenceNumberTypeCollection.Add("AAA", (NoResString)"AAA code");
			customsReferenceNumberType1.IsAutomation = true;
			var customsReferenceNumberType2 = customsReferenceNumberTypeCollection.Add("BBB", (NoResString)"BBB code");
			customsReferenceNumberType2.IsAutomation = false;
			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customsReferenceNumberTypeCollection);

			var shipmentBOToLoad = Factory.New<ForwardingShipment>();
			shipmentBOToLoad.JS_HouseBill = "AHOUSE";

			var cusEntryNumber1 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber1.CE_EntryNum = "CE00001";
			cusEntryNumber1.CE_EntryType = "AAA";
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var cusEntryNumber2 = shipmentBOToLoad.Numbers.AddNew();
			cusEntryNumber2.CE_EntryNum = "CE00002";
			cusEntryNumber2.CE_EntryType = "BBB";
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			Factory.SaveForTesting();

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "AAA", Description = "AAA code" }, ReferenceNumber = "CE00001" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "BBB", Description = "BBB code" }, ReferenceNumber = "CE00002" };

			shipmentDataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference1);
			shipmentDataObject.AdditionalReferenceCollection.Add(additionalReference2);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			AssertNotNull(shipmentBO);
			Factory.SaveForTesting();

			#region Check Contents of shipment Business object

			CombineAssertions(delegate
			{
				AssertContents(shipmentBO);

				// We should import both references via Automation
				var importedCusEntryNumber1 = shipmentBO.Numbers.Cast<CusEntryNumber>().First(item => item.CE_EntryType == "AAA");
				var importedCusEntryNumber2 = shipmentBO.Numbers.Cast<CusEntryNumber>().First(item => item.CE_EntryType == "BBB");

				// Only the IsAutomation controlled reference should be read-only
				AssertEquals(true, importedCusEntryNumber1.ReadOnly);
				AssertEquals(false, importedCusEntryNumber2.ReadOnly);

				AssertMultilineASCIIEquals("Logger.Logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Added Shipment from UniversalShipment.
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestReaderRejectsWhenTargetingForwardingShipmentButMatchesBooking()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var forwardingShipment = Factory.New<ForwardingShipment>();

			var expectedErrorMessage = "[*Matching ForwardingShipment is in the ForwardingBooking state, incorrect DataTarget found.*]";

			var dummyObjectReader = new DummyShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(forwardingConsol));
			AssertNotEquals(expectedErrorMessage, dummyObjectReader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(forwardingShipment));

			var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quotedBooking = quotedBookingBuilder.CreateNew(QuoteBookingType.QuickBooking, Factory.BOFactory);
			var booking = (ForwardingShipment)quotedBooking.ForwardingShipment;
			AssertNotEquals(
				"When message targets a forwarding shipment with a parent consol specified and matches with an un-converted booking, it should not reject.",
				expectedErrorMessage,
				dummyObjectReader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(booking));

			dummyObjectReader = new DummyShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			AssertEquals(
				"When message targets a forwarding shipment with no parent consol specified and matches with an un-converted booking, it should be rejected.",
				expectedErrorMessage,
				dummyObjectReader.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(booking));
		}

		#region CO2e

		public void TestReaderDoesNotPopulateCO2eWhenShipmentDoesNotMatchCO2eCalculationParameters()
		{
			var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
			forwardingShipment.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			forwardingShipment.JS_UniqueConsignRef = "S0001";
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, "S0001");
			Factory.SaveForTesting();

			forwardingShipment.JS_RL_NKOrigin = "USLAX";
			var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(CO2eStatusList.Codes.NotCurrent, forwardingShipment.GetCO2eStatus());
			AssertEquals(0m, forwardingShipment.GetCO2ePerTonneInKg());
			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Cannot populate CO2e for ForwardingShipment because CO2e Calculation input parameters have been changed
Information - Updated Shipment S0001 from UniversalShipment.", Logger.Logs);
		}

		public void TestCO2eCalculation_GHGUpdatedEventsLogged()
		{
			var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
			forwardingShipment.JS_HouseBill = "HOUSENUM1";
			forwardingShipment.JS_ActualWeight = 1m;
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.TotalWeight = 1m;
			Factory.SaveForTesting();
			var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
			var shipmentBo = reader.ReadIntoBusinessObject();
			AssertGHGEvent(shipmentBo.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipmentBo.Transports[0].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipmentBo.Transports[0].Sailing.Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipmentBo.Transports[1].Logs, "|NEW=2000|OLD=NA|TYP=Updated");
		}

		public void TestCO2eCalculation_GHGUpdatedEventsLogged_WhenPreviousCO2e()
		{
			// Arrange & Act
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory.BOFactory);
			consol.JK_MasterBillNum = "HOUSENUM1";
			consol.JK_TotalShipmentActWeightCheck = 1m;

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			dataObject.TotalWeight = 1m;

			Factory.SaveForTesting();

			var reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			var consolBO = reader.ReadIntoBusinessObject();

			// Assert
			AssertGHGEvent(consolBO.Logs, "|NEW=10000|OLD=NA|TYP=Updated");

			// Act
			dataObject.GreenhouseGasEmission.CO2e = 25000m;
			Factory.SaveForTesting();
			reader = new ConsolDataObjectReader(dataObject, logger, Factory);
			consolBO = reader.ReadIntoBusinessObject();

			// Assert
			AssertGHGEvent(consolBO.Logs, "|NEW=25000|OLD=10000|TYP=Updated", 2);
		}

		public void TestCO2eCalculation_GHGRejectedEventsLogged_WhenParameterChanged()
		{
			var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
			forwardingShipment.JS_HouseBill = "HOUSENUM1";
			forwardingShipment.JS_RL_NKOrigin = "USLAX";
			forwardingShipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			Factory.SaveForTesting();
			var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
			var shipmentBo = reader.ReadIntoBusinessObject();
			AssertGHGEvent(shipmentBo.Logs, "|RES=Input value(s) have changed|TYP=Rejected");
			AssertEquals("No GHG event created for transport", 0, shipmentBo.Transports[0].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals("No GHG event created for linked sailing", 0, shipmentBo.Transports[0].Sailing.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals("No GHG event created for transport", 0, shipmentBo.Transports[1].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
		}

		public void TestCO2eCalculation_GHGRejectedEventsLogged_WhenParameterChanged_EnablePrePostCarriage()
		{
			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
				forwardingShipment.JS_HouseBill = "HOUSENUM1";
				forwardingShipment.JS_RL_NKOrigin = "USLAX";
				forwardingShipment.SetTotalCO2e(99m);
				forwardingShipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				Factory.SaveForTesting();

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				dataObject.WayBillNumber = "HOUSENUM1";
				var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
				var shipmentBo = reader.ReadIntoBusinessObject();
				AssertGHGEvent(shipmentBo.Logs, "|RES=Input value(s) have changed|TYP=Rejected");
				AssertEquals("CO2e Status remains unchanged", CO2eStatusList.Codes.NotCurrent, shipmentBo.GetCO2eStatus());
				AssertEquals("Total CO2e remains unchanged", 99m, shipmentBo.GetTotalCO2e());
			}
		}

		public void TestCO2eCalculation_TriggerByWorkflow_ChangeParamBeforeReceivingResponse_GHGRejectedEventsLogged()
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
			var trigger = shipment.WorkflowItems.Triggers.AddNew();

			var transport1 = shipment.Transports[0];
			var transport2 = shipment.Transports[1];

			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 1;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest;
			shipment.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.SaveForTesting();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: Customizable Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
			var workflowDescriptor = new ForwardingShipmentWorkflowDescriptor();
			var processor = workflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);

			shipment.JS_HouseBill = "HOUSENUM1";
			shipment.JS_RL_NKOrigin = "USLAX";

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "DEHAM";
			transport3.JW_RL_NKDiscPort = "USLAX";
			transport3.JW_TransportMode = "SEA";
			transport3.JW_VoyageFlight = "VY2";

			var loggerprocess = new NotificationBuffer();
			using (Factory.BOFactory.AddDisposableService())
			{
				processor.Process(loggerprocess);
				Factory.SaveForTesting();
			}

			AssertMultilineASCIIEquals("loggger results from processor.Process()", string.Empty, loggerprocess.AsString);
			var newFactory = new BusinessObjectFactory();
			var messages = newFactory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			var message = messages[0];
			Assert(message.IsInDatabase);
			CombineAssertions("EDI Message sent", () =>
			{
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertContains("message.EM_MessageText", "UniversalShipment", message.EM_MessageText);
			});

			var interchange = newFactory.Load<EDIInterchange>(message.EM_EI);
			Assert(interchange.IsInDatabase);
			CombineAssertions("EDI Interchange", () =>
			{
				AssertNotNull(interchange);
				AssertEquals("EMISSION_CALCULATOR", interchange.EI_To);
			});

			AssertEquals(CO2eStatusList.Codes.Pending, shipment.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Pending, transport1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Pending, transport2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Pending, transport3.GetCO2eStatus());

			shipment.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);

			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			dataObject.WayBillNumber = "HOUSENUM1";
			dataObject.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };

			Factory.SaveForTesting();

			var reader = new ShipmentDataObjectReader(dataObject, logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(CO2eStatusList.Codes.NotCurrent, shipment.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.NotCurrent, transport3.GetCO2eStatus());

			AssertMultilineASCIIEquals(@"Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Warning - Cannot populate CO2e for ForwardingShipment because CO2e Calculation input parameters have been changed
Information - Updated Shipment S00001000 (House Bill='HOUSENUM1') from UniversalShipment."
			, logger.Logs);

			var transportGHGEvent = shipmentBO.Logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", "|RES=Input value(s) have changed|TYP=Rejected", transportGHGEvent.SL_Reference);
			AssertEquals("One GHG event created for transport", 1, shipmentBO.Transports[0].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			AssertEquals("One GHG event created for transport", 1, shipmentBO.Transports[1].Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
		}

		public void TestCO2eCalculation_GHGUpdatedEventsLogged_WhenWeightParameterChanged()
		{
			var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
			forwardingShipment.JS_HouseBill = "HOUSENUM1";
			forwardingShipment.JS_ActualWeight = 2m;
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject(forwardingShipment);
			dataObject.WayBillNumber = "HOUSENUM1";
			Factory.SaveForTesting();
			var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
			var shipmentBo = reader.ReadIntoBusinessObject();
			AssertGHGEvent(shipmentBo.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipmentBo.Transports[0].Logs, "|NEW=16000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipmentBo.Transports[0].Sailing.Logs, "|NEW=8000|OLD=NA|TYP=Updated");
			AssertGHGEvent(shipmentBo.Transports[1].Logs, "|NEW=4000|OLD=NA|TYP=Updated");
		}

		void AssertGHGEvent(Logs logs, string reference, int logCount = 1)
		{
			AssertEquals("New GHG event created", logCount, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", reference, transportGHGEvent.SL_Reference);
		}

		public void TestIsCO2eResponseApplicable_WithConvertedTransportModes()
		{
			SetupAndAssertTransportMode(TransportModes.SeaAir, TransportModes.Sea, "S0001");
			SetupAndAssertTransportMode(TransportModes.AirSea, TransportModes.Air, "S0002");
			SetupAndAssertTransportMode(TransportModes.Courier, TransportModes.Air, "S0003");
		}

		void SetupAndAssertTransportMode(string transportMode, string convertedTransportMode, string shipmentId)
		{
			TransportMode GetTransportMode(string mode)
			{
				return (TransportMode)Enum.Parse(typeof(TransportMode), mode, true);
			}

			var shipment = CO2eTestHelper.CreateForwardingShipmentWithIncompleteLegs(Factory.BOFactory);
			shipment.JS_UniqueConsignRef = shipmentId;
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObjectWithVirtualLegs();
			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingShipment, shipmentId);

			shipment.JS_TransportMode = transportMode;
			dataObject.TransportMode = new CodeDescriptionPair { Code = convertedTransportMode };
			foreach (var transportLeg in dataObject.TransportLegCollection)
			{
				transportLeg.TransportMode = GetTransportMode(convertedTransportMode);
			}

			Factory.SaveForTesting();

			var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			CombineAssertions("Import CO2e response sucessfully", () =>
			{
				AssertEquals(12500m, shipmentBO.GetTotalCO2e());
				AssertEquals(2000m, shipmentBO.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
			});
		}

		[TestDate(2025, 01, 01, 1, 0, 0)]
		public void TestCO2eCalculation_EmissionsCalculationLog()
		{
			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory.BOFactory);
				forwardingShipment.JS_UniqueConsignRef = "S00001000";
				forwardingShipment.JS_HouseBill = "HOUSENUM1";
				forwardingShipment.JS_ActualWeight = 1m;
				forwardingShipment.SetTotalCO2e(100m);

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				dataObject.WayBillNumber = "HOUSENUM1";
				dataObject.TotalWeight = 1m;
				Factory.SaveForTesting();
				var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
				var shipmentBo = reader.ReadIntoBusinessObject();
				var note = shipmentBo.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
				AssertNotNull("Emissions Calculation Log note exists", note);
				AssertMultilineASCIIEquals(@"01-Jan-25 01:00 ----- Shipment S00001000 (House Bill='HOUSENUM1') ----- Eagle Datamation International
Previous CO2e value: 100 kg
New CO2e value: 10000 kg
Calculated automatically

Job level input parameters:
Transport Mode: SEA
Container Mode: FCL
Total Weight: 1 T
Temperature Controlled: N", note.ST_NoteText);
			}
		}

		[TestDate(2025, 01, 01, 1, 0, 0)]
		public void TestCO2eCalculation_EmissionsCalculationLog_RequireTEU_TemperatureControlled()
		{
			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory.BOFactory);
				forwardingShipment.JS_UniqueConsignRef = "S00001000";
				forwardingShipment.JS_HouseBill = "HOUSENUM1";
				forwardingShipment.OuterPackLines[0].JL_RequiresTemperatureControl = true;
				forwardingShipment.SetTotalCO2e(100m);
				forwardingShipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				dataObject.WayBillNumber = "HOUSENUM1";
				Factory.SaveForTesting();
				var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
				var shipmentBo = reader.ReadIntoBusinessObject();
				var note = shipmentBo.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
				AssertNotNull("Emissions Calculation Log note exists", note);
				AssertMultilineASCIIEquals(@"01-Jan-25 01:00 ----- Shipment S00001000 (House Bill='HOUSENUM1') ----- Eagle Datamation International
Previous CO2e value: 100 kg
New CO2e value: 10000 kg
Calculated automatically

Job level input parameters:
Transport Mode: SEA
Container Mode: FCL
Total Weight: 1 T
Number of TEUs: 4.3
Tonnes per TEU: 0.232558 t
Temperature Controlled: Y", note.ST_NoteText);
			}
		}

		[TestDate(2025, 01, 01, 1, 0, 0)]
		public void TestCO2eCalculation_EmissionsCalculationLog_EmptyContainer()
		{
			using (FreightDataRegistry.Instance.AddEmissionsCalculationLogForShipment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingShipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory.BOFactory);
				forwardingShipment.JS_UniqueConsignRef = "S00002000";
				forwardingShipment.JS_HouseBill = "HB2000";
				forwardingShipment.SetTotalCO2e(100m);

				var container = forwardingShipment.Consols[0].Containers[0];
				container.JC_IsEmptyContainer = true;
				container.JC_ContainerNum = "TCLU1234567";
				container.JC_ContainerJobID = "CJOB2000";

				var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
				dataObject.WayBillNumber = "HB2000";

				var orgAddrFrom = new OrganizationAddress
				{
					AddressType = "PIC",
					City = "Sydney",
					Country = new Country { Code = "AU" },
					Port = new UNLOCO { Code = "AUSYD" },
					Postcode = "2000",
					GeoLocation = new GeoLocation { Latitude = -33.87m, Longitude = 151.21m }
				};
				var orgAddrTo = new OrganizationAddress
				{
					AddressType = "YRD",
					City = "Melbourne",
					Country = new Country { Code = "AU" },
					Port = new UNLOCO { Code = "AUMEL" },
					Postcode = "3000"
				};

				var emptyDto = new WeightData
				{
					ContainerJobID = "CJOB2000",
					TotalWeight = 2280m,
					TotalWeightUnit = new UnitOfWeight { Code = "KG" },
					TEU = new TEU { NumberOfTEU = 1.0m },

					EmptyPickup = new EmptyContainerAddress
					{
						TransportMode = new CodeDescriptionPair { Code = "ROA", Description = "Road" },
						From = orgAddrFrom,
						To = orgAddrTo,
						GreenhouseGasEmission = new GreenhouseGasEmission
						{
							CO2e = 12.3456m,
							CO2eUnit = new UnitOfWeight { Code = "KG" },
							CO2eDistanceInKm = 54.321m
						}
					},
					EmptyReturn = new EmptyContainerAddress
					{
						TransportMode = new CodeDescriptionPair { Code = "IWT", Description = "Inland Waterways" },
						From = orgAddrTo,
						To = orgAddrFrom,
						GreenhouseGasEmission = new GreenhouseGasEmission
						{
							CO2e = 234.5678m,
							CO2eUnit = new UnitOfWeight { Code = "KG" },
							CO2eDistanceInKm = 98.76m
						}
					}
				};

				var sub = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				sub.DataContext = new DataContext();
				sub.DataContext.AddDataSource(DataContextType.ForwardingConsol, forwardingShipment.Consols[0].JK_UniqueConsignRef); 
				sub.SetEmptyContainerCollection(() => new DataObjectList<WeightData> { emptyDto });
				dataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { sub });

				Factory.SaveForTesting();
				var reader = new ShipmentDataObjectReader(dataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();

				var note = shipmentBO.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
				AssertNotNull("Emissions Calculation Log note exists", note);

				var noteText = note.ST_NoteText;
				var consolNumber = forwardingShipment.Consols[0].JK_UniqueConsignRef;
				var expected = $@"01-Jan-25 01:00 ----- Shipment S00002000 (House Bill='HB2000') ----- Eagle Datamation International
Previous CO2e value: 100 kg
New CO2e value: 10234.5678 kg
Calculated automatically

Job level input parameters:
Transport Mode: SEA
Container Mode: FCL
Total Weight: 1 T
Number of TEUs: 4.3
Tonnes per TEU: 0.232558 t
Temperature Controlled: N

Empty Container Pickup/Return:
Consol number: {consolNumber}
Container number: TCLU1234567
No. of TEUs: 2
Tare Weight: 4560 kg
Pickup Transport Mode: Road
Pickup Origin: -33.87 151.21 // AUSYD // Sydney, AU // 2000, AU
Pickup Destination: AUMEL // Melbourne, AU // 3000, AU
Pickup CO2e: 12.3456 kg
Pickup Distance: 54.321 km
Return Transport Mode: InlandWaterway
Return Origin: AUMEL // Melbourne, AU // 3000, AU
Return Destination: -33.87 151.21 // AUSYD // Sydney, AU // 2000, AU
Return CO2e: 234.5678 kg
Return Distance: 98.76 km
";
				AssertMultilineASCIIEquals(expected, note.ST_NoteText);
			}
		}

		#endregion

		#region JS_RL_NKLoadPort and JS_RL_NKDischargePort

		public void TestPortsOfLoadingAndDischargeWhenConsolsAreAttached()
		{
			shipmentDataObject.PortOfLoading = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };
			shipmentDataObject.PortOfDischarge = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };

			var consolBO1 = Factory.New<ForwardingConsol>();
			consolBO1.JK_RL_NKLoadPort = "NZAKL";
			consolBO1.JK_RL_NKDischargePort = "USLAX";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO1));
			var shipmentBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertNotEquals("shipmentBO.JS_RL_NKLoadPort", "USLAX", shipmentBO.JS_RL_NKLoadPort);
			AssertNotEquals("shipmentBO.JS_RL_NKDischargePort", "AUSYD", shipmentBO.JS_RL_NKDischargePort);
		}

		public void TestPortsOfLoadingAndDischargeWhenConsolsAreNotAttached()
		{
			shipmentDataObject.PortOfLoading = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };
			shipmentDataObject.PortOfDischarge = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals("shipmentBO.JS_RL_NKLoadPort", "USLAX", shipmentBO.JS_RL_NKLoadPort);
			AssertEquals("shipmentBO.JS_RL_NKDischargePort", "AUSYD", shipmentBO.JS_RL_NKDischargePort);
		}

		#endregion

		#region Test Importing with Buyer Supplier Links

		public void TestWithBuyerSupplierLinks()
		{
			var consignor = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var consignee = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);

			shipmentDataObject.WayBillNumber = "MYHOUSE1";
			shipmentDataObject.WayBillType = new WayBillType { Code = "HWB", Description = "House Waybill" };

			var addressCNR = GetNewAddressData_INTHEMSYD(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var addressCNE = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(addressCNR);
			shipmentDataObject.OrganizationAddressCollection.Add(addressCNE);

			Factory.SaveForTesting();

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Precondition - Origin", "NZDUD", shipmentBO.Origin.Code);
			AssertEquals("Precondition - Destination", "AUBDG", shipmentBO.Destination.Code);

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = consignee.PK;
			link.OL_OH_Supplier = consignor.PK;
			OrgSupBuyLinkTrnMode linkTrnMode = link.OrgSupBuyLinkTrnModes[0];
			linkTrnMode.PF_OL = link.PK;
			linkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkTrnMode.PF_RL_NKPlaceOfReceivalPort = "NZCHC";
			linkTrnMode.PF_RL_NKPlaceOfDeliveryPort = "AUMEL";

			shipmentDataObject.WayBillNumber = "MYHOUSE2";
			shipmentDataObject.BookingConfirmationReference = "CONFIRM2";
			shipmentDataObject.InterimReceiptNumber = "MYRECEIPT2";
			shipmentDataObject.CFSReference = "MYCFSREF2";

			Factory.SaveForTesting();

			shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Origin is taken from XML", "NZDUD", shipmentBO.Origin.Code);
			AssertEquals("Destination is taken from XML", "AUBDG", shipmentBO.Destination.Code);

			shipmentDataObject.PortOfOrigin = new UNLOCO();
			shipmentDataObject.PortOfDestination = new UNLOCO();

			shipmentDataObject.WayBillNumber = "MYHOUSE3";
			shipmentDataObject.BookingConfirmationReference = "CONFIRM3";
			shipmentDataObject.InterimReceiptNumber = "MYRECEIPT3";
			shipmentDataObject.CFSReference = "MYCFSREF3";

			Factory.SaveForTesting();

			shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Origin falls back to Buyer / Supplier link", "NZCHC", shipmentBO.Origin.Code);
			AssertEquals("Destination falls back to Buyer / Supplier link", "AUMEL", shipmentBO.Destination.Code);
		}

		#endregion

		#region Invoice Lines Importing

		public void TestInvoiceLinesImporting_MatchedShipmentIsMasterAndHasSubshipments_DoNotCreatePackLines()
		{
			shipmentDataObject.DataContext.AddDataTarget(DataContextType.WarehouseOrder, null);

			var subShipment = Factory.New<ForwardingShipment>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "ASM";
			shipment.JS_HouseBill = "MCLAREN";
			shipment.CoLoadShipments.Add(subShipment);

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MCLAREN";
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "ASM", Description = "Master" };

			shipmentDataObject.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>
						{
							new CommercialInvoiceLine { InvoiceQuantity = 3 },
							new CommercialInvoiceLine { InvoiceQuantity = 4 }
						}))
				}
			};

			Logger.ClearLogs();

			var readerToTest = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var readShipment = readerToTest.ReadIntoBusinessObject();

			AssertContains("Log should contain the warning message", "Can't create pack lines from invoice lines as the shipment is Master.", Logger.Logs);
			AssertEquals("PackLines should not be created", 0, readShipment.OuterPackLines.Count);
		}

		#endregion

		#region Packing Lines Importing

		public void TestPackingLinesImporting_MatchedShipmentIsMasterAndHasSubshipments_DoNotCreatePackLines()
		{
			var subShipment = Factory.New<ForwardingShipment>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "ASM";
			shipment.JS_HouseBill = "MCLAREN";
			shipment.CoLoadShipments.Add(subShipment);

			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = "MCLAREN";
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "ASM", Description = "Master" };
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2 } });

			Logger.ClearLogs();

			var readerToTest = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var readShipment = readerToTest.ReadIntoBusinessObject();

			AssertContains("Log should contain the warning message", "Can't create pack lines as the shipment is Master.", Logger.Logs);
			AssertEquals("PackLines should not be created", 0, readShipment.OuterPackLines.Count);
		}

		public void TestImportCompletePacklineCollectionWithEmptyPacklines()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var existingShipment = Factory.New<ForwardingShipment>();
				existingShipment.JS_HouseBill = "BACON PANCAKES";
				existingShipment.JS_OA_ExportReceivingDepot = WarehouseAddressBO.PK;

				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_JobID = "RC0005";
				packageJob.KJ_ParentID = existingShipment.PK;
				packageJob.KJ_ParentTableCode = existingShipment.TablePrefix;

				var packlineBO = existingShipment.OuterPackLines.AddNew();
				packlineBO.JL_OA_LastKnownTransitWarehouseAddress = WarehouseAddressBO.PK;
				packlineBO.JL_PackLineId = "TEST001";
				var package1 = packlineBO.PkgPackageCollection.AddNew();
				package1.KP_PackageID = "PK1";
				package1.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package1.KP_KJ_ParentPackageJob = packageJob.PK;
				var package2 = packlineBO.PkgPackageCollection.AddNew();
				package2.KP_PackageID = "PK2";
				package2.KP_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
				package2.KP_KJ_ParentPackageJob = packageJob.PK;
				Factory.SaveForTesting();

				var emptyPackline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
				var shipmentDataObject = CreateShipmentDO(WarehouseAddressBO);
				shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { emptyPackline });
				shipmentDataObject.PackingLineCollection.Content = CollectionContent.Complete;
				var shipment = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null).ReadIntoBusinessObject();

				AssertEquals("Should be have packages removed", 0, packlineBO.PkgPackageCollection.Count);
				AssertEquals("Should have status set to short shipped",
					FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
					packlineBO.JL_OriginTransitWarehouseStatus);
			}
		}

		#endregion

		#region JS_LoadingMeters

		public void TestLoadingMeters_ValidValue()
		{
			shipmentDataObject.TotalLoadingMeters = 123m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Loading meters should be populated", 123m, shipmentBO.JS_LoadingMeters);
		}

		public void TestLoadingMeters_OutOfRange()
		{
			shipmentDataObject.TotalLoadingMeters = 1234567m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("Out of range values maps to largest possible number", 999999m, shipmentBO.JS_LoadingMeters);
		}

		#endregion

		#region JS_ActualVolume

		public void TestActualVolumeMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.TotalVolume = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_ActualVolume);
		}

		public void TestActualVolumeMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.TotalVolume = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_ActualVolume);
		}

		public void TestActualVolumeMapping_TheValueIsValid_ShouldRecalculateDependingProperties()
		{
			shipmentDataObject.TotalVolume = 666.999m;
			shipmentDataObject.TotalWeight = null;
			shipmentDataObject.ActualChargeable = null;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			shipmentDataObject.DocumentedVolume = null;
			shipmentDataObject.ManifestedVolume = null;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_DocumentedVolume);
			AssertEquals(666.999m, shipmentBO.JS_ManifestedVolume);
			AssertEquals(3147.885m, shipmentBO.JS_ActualChargeable);
		}

		#endregion

		public void TestRoundDownWhenRoundedValueIsBiggerThanDecimalPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			var registryEntryDimension = new DefaultNumberOfDecimals();
			registryEntryDimension.UnitOfMeasure = Core.Constants.Dimension.Metres;
			registryEntryDimension.TransportMode = Core.Constants.TransportModes.Air;
			registryEntryDimension.NumberOfDecimals = 1;
			registryEntryDimension.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryDimension);

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
			shipmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = Core.Constants.Weight.Kilograms };
			shipmentDataObject.TotalVolumeUnit = new UnitOfVolume { Code = Core.Constants.Volume.CubicMetres };
			shipmentDataObject.TotalWeight = 999999.999m;
			shipmentDataObject.TotalVolume = 999999.999m;
			shipmentDataObject.DocumentedWeight = 999999.999m;
			shipmentDataObject.DocumentedVolume = 999999.999m;
			shipmentDataObject.DocumentedChargeable = 999999.999m;
			shipmentDataObject.ManifestedWeight = 999999.999m;
			shipmentDataObject.ManifestedVolume = 999999.999m;
			shipmentDataObject.ManifestedChargeable = 999999.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_ActualWeight);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_ActualVolume);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_DocumentedWeight);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_DocumentedVolume);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_DocumentedChargeable);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_ManifestedWeight);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_ManifestedVolume);
			AssertEquals((ZDecimal)999999.9, shipmentBO.JS_ManifestedChargeable);
		}

		#region JS_DocumentedVolume

		public void TestDocumentedVolumeMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.DocumentedVolume = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_DocumentedVolume);
		}

		public void TestDocumentedVolumeMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.DocumentedVolume = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_DocumentedVolume);
		}

		#endregion

		#region JS_ActualWeight

		public void TestActualWeightMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.TotalWeight = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_ActualWeight);
		}

		public void TestActualWeightMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.TotalWeight = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_ActualWeight);
		}

		public void TestActualWeightMapping_TheValueIsValid_ShouldRecalculateDependingProperties()
		{
			shipmentDataObject.TotalWeight = 6m;
			shipmentDataObject.TotalVolume = null;
			shipmentDataObject.ActualChargeable = null;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "SEA" };
			shipmentDataObject.DocumentedWeight = null;
			shipmentDataObject.ManifestedWeight = null;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(6m, shipmentBO.JS_DocumentedWeight);
			AssertEquals(6m, shipmentBO.JS_ManifestedWeight);
			AssertEquals(6000m, shipmentBO.JS_ActualChargeable);
		}

		#endregion

		#region JS_DocumentedWeight

		public void TestDocumentedWeightMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.DocumentedWeight = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_DocumentedWeight);
		}

		public void TestDocumentedWeightMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.DocumentedWeight = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_DocumentedWeight);
		}

		#endregion

		#region JS_ActualChargeable

		public void TestActualChargeableMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.ActualChargeable = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_ActualChargeable);
		}

		public void TestActualChargeableMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.ActualChargeable = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_ActualChargeable);
		}

		public void TestActualChargeableMapping_TheValueIsValid_ShouldRecalculateDependingProperties()
		{
			shipmentDataObject.ActualChargeable = 666.999m;
			shipmentDataObject.TotalWeight = null;
			shipmentDataObject.TotalVolume = null;
			shipmentDataObject.DocumentedChargeable = null;
			shipmentDataObject.ManifestedChargeable = null;
			shipmentDataObject.DocumentedWeight = null;
			shipmentDataObject.ManifestedWeight = null;
			shipmentDataObject.DocumentedVolume = null;
			shipmentDataObject.ManifestedVolume = null;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_DocumentedChargeable);
			AssertEquals(666.999m, shipmentBO.JS_ManifestedChargeable);
		}

		public void TestActualChargeableMapping_TheCalculatedValueIsOutOfRange_MapToInvalidValue()
		{
			shipmentDataObject.TotalWeight = 666m;
			shipmentDataObject.TotalVolume = null;
			shipmentDataObject.ActualChargeable = null;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_ActualChargeable);
		}

		#endregion

		#region JS_DocumentedChargeable

		public void TestDocumentedChargeableMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.DocumentedChargeable = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_DocumentedChargeable);
		}

		public void TestDocumentedChargeableMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.DocumentedChargeable = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_DocumentedChargeable);
		}

		public void TestDocumentedChargeableMapping_TheCalculatedValueIsOutOfRange_MapToInvalidValue()
		{
			shipmentDataObject.DocumentedWeight = 666m;
			shipmentDataObject.DocumentedVolume = null;
			shipmentDataObject.DocumentedChargeable = null;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_DocumentedChargeable);
		}

		#endregion

		#region JS_ManifestedChargeable

		public void TestManifestedChargeableMapping_TheValueIsValid_MapIt()
		{
			shipmentDataObject.ManifestedChargeable = 666.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(666.999m, shipmentBO.JS_ManifestedChargeable);
		}

		public void TestManifestedChargeableMapping_TheValueIsOutOfAllowedRange_MapToLargestPossibleNumber()
		{
			shipmentDataObject.ManifestedChargeable = 1234567.999m;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_ManifestedChargeable);
		}

		public void TestManifestedChargeableMapping_TheCalculatedValueIsOutOfRange_MapToInvalidValue()
		{
			shipmentDataObject.ManifestedWeight = 666m;
			shipmentDataObject.ManifestedVolume = null;
			shipmentDataObject.ManifestedChargeable = null;
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(999999m, shipmentBO.JS_ManifestedChargeable);
		}

		#endregion

		#region Aviation Security Inspection Type

		public void TestReadIntoBusinessObject_AviationSecurityInspectionType()
		{
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "PHS" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("InspectionType", "PHS", shipmentBO.JS_InspectionTypeCode);

			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "UNK" };
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();
			AssertEquals("InspectionType", "UNK", shipmentBO.JS_InspectionTypeCode);

			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "" };
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();
			AssertEquals("InspectionType", "UNK", shipmentBO.JS_InspectionTypeCode);
		}

		#endregion

		#region Sub HVL Shipment Inspection Type

		public void TestReadIntoBusinessObject_SubHVLShipmentInspectionType()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			var shipment = consolBO.Shipments.AddNew();
			var subShipment = Factory.New<ForwardingShipment>();

			shipment.CoLoadShipments.Add(subShipment);
			shipment.JS_ShipmentType = "HVM";
			shipment.JS_HouseBill = "HOUSEBILL";
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			subShipment.JS_ShipmentType = "HVL";
			subShipment.JS_HouseBill = "HOUSEBILL1";
			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
			shipmentDataObject.ShipmentType = new CodeDescriptionPair { Code = "HVM" };
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "CMD" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("InspectionType", "CMD", shipment.JS_InspectionTypeCode);
			AssertEquals("InspectionType", "CMD", subShipment.JS_InspectionTypeCode);

			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "" };
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			shipmentBO = reader.ReadIntoBusinessObject();
			AssertEquals("InspectionType", "UNK", shipment.JS_InspectionTypeCode);
			AssertEquals("InspectionType", "UNK", subShipment.JS_InspectionTypeCode);
		}

		public void TestReadIntoBusinessObject_SubNonHVLShipmentInspectionType()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			var shipment = consolBO.Shipments.AddNew();
			var subShipment = Factory.New<ForwardingShipment>();

			shipment.CoLoadShipments.Add(subShipment);
			shipment.JS_ShipmentType = "HVM";
			shipment.JS_HouseBill = "HOUSEBILL";
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			subShipment.JS_ShipmentType = "SDT";
			subShipment.JS_HouseBill = "HOUSEBILL1";
			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
			shipmentDataObject.ShipmentType = new CodeDescriptionPair { Code = "HVM" };
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "CMD" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("InspectionType", "CMD", shipmentBO.JS_InspectionTypeCode);
			AssertEquals("InspectionType", "UNK", shipmentBO.CoLoadShipments[0].JS_InspectionTypeCode);
		}

		public void TestReadIntoBusinessObject_SubHVLShipmentInspectionType_ParentShipmentIsNotHVM()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			var shipment = consolBO.Shipments.AddNew();
			var subShipment = Factory.New<ForwardingShipment>();

			shipment.CoLoadShipments.Add(subShipment);
			shipment.JS_ShipmentType = "SDT";
			shipment.JS_HouseBill = "HOUSEBILL";
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			subShipment.JS_ShipmentType = "HVL";
			subShipment.JS_HouseBill = "HOUSEBILL1";
			Factory.SaveForTesting();

			shipmentDataObject.WayBillNumber = shipment.JS_HouseBill;
			shipmentDataObject.ShipmentType = new CodeDescriptionPair { Code = "SDT" };
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			shipmentDataObject.WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" };
			shipmentDataObject.AviationSecurityInspectionType = new CodeDescriptionPair { Code = "CMD" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("InspectionType", "CMD", shipmentBO.JS_InspectionTypeCode);
			AssertEquals("InspectionType", "UNK", shipmentBO.CoLoadShipments[0].JS_InspectionTypeCode);
		}

		#endregion

		#region Aviation Security Additional Inspection Type

		public void TestReadIntoBusinessObject_AviationSecurityAdditionalInspectionType()
		{
			shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = "AIR" };
			shipmentDataObject.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair { Code = "PHS" };

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("InspectionType", "PHS", shipmentBO.JS_AdditionalInspectionTypeCode);

			shipmentDataObject.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair { Code = "UNK" };
			reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipmentBO = reader.ReadIntoBusinessObject();
			AssertEquals("InspectionType", "UNK", shipmentBO.JS_AdditionalInspectionTypeCode);
		}

		#endregion

		#region JS_IsHighRisk

		public void TestJS_IsHighRisk()
		{
			shipmentDataObject.IsHighRisk = true;

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals(true, shipmentBO.JS_IsHighRisk);
		}

		#endregion

		#region Import InBond
		public void TestImportInBond()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var inBondData = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New()
				};
				inBondData.DataContext.AddDataSource(DataContextType.InBond, "S0232");
				inBondData.DataContext.AddDataTarget(DataContextType.InBond, null);
				inBondData.VesselName = "BOB'S BEST VESSEL";
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(new[] { inBondData }));

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();

				AssertEquals("shipmentBO.CoLoadShipments", 0, shipmentBO.CoLoadShipments.Count);
				var inBondHeader = shipmentBO.InBondHeader;
				AssertNotNull("inBondHeader", inBondHeader);
				AssertEquals(ZBool.True, inBondHeader.BH_OverrideFreightDefaults);
				AssertEquals("BOB'S BEST VESSEL", inBondHeader.BH_ImportConveyanceName);
				Factory.SaveForTesting();
				Factory.FireCleanupAfterSaving();
			}
		}
		#endregion

		#region TestControllingCustomer

		public void TestControllingCustomer()
		{
			var controllingCustomer = GetNewAddressData_WUFSHIJNB(nameof(DocAddressType.ControllingCustomer));
			var shipmentControllingParty = GetNewAddressData_INTHEMSYD(LegacyUniversalAddressTypes.LegacyShipmentControllingPartyAddressType);

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(controllingCustomer);
			shipmentDataObject.OrganizationAddressCollection.Add(shipmentControllingParty);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			var jobDocAddress = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
			AssertJobDocAddressContentMatches_WUFSHIJNB(jobDocAddress);
			AssertEquals("E2_AddressType", "SCP", jobDocAddress.E2_AddressType);
		}

		public void TestLegacyShipmentControllingParty()
		{
			var shipmentControllingParty = GetNewAddressData_INTHEMSYD(LegacyUniversalAddressTypes.LegacyShipmentControllingPartyAddressType);
			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			shipmentDataObject.OrganizationAddressCollection.Add(shipmentControllingParty);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);

			var jobDocAddress = shipmentBO.DocAddresses.FindByDocAddressType(DocAddressType.ControllingCustomer);
			AssertJobDocAddressContentMatches_INTHEMSYD(jobDocAddress);
			AssertEquals("E2_AddressType", "SCP", jobDocAddress.E2_AddressType);
		}

		#endregion

		#region HVLWork Item WI00200879 Support

		public void TestHVLV_SubShipmentDataContextIsNull()
		{
			AssertImportSubShipmentsAsHVLVConsignments(null);
		}

		public void TestHVLV_SubShipmentDataSourceIsHVLVConsignment()
		{
			var subShipmentDataContext = DataContextFactory.New();
			subShipmentDataContext.AddDataSource(DataContextType.HVLVConsignment, "SUBSHIPMENT1");
			subShipmentDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			AssertImportSubShipmentsAsHVLVConsignments(subShipmentDataContext);
		}

		public void TestHVLV_SubShipmentDataDataSourceTypeIsNull()
		{
			var subShipmentDataContext = (DataContext)DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
			subShipmentDataContext.DataSourceCollection = new List<DataSource>
			{
				new DataSource { }
			};

			AssertImportSubShipmentsAsHVLVConsignments(subShipmentDataContext);
		}

		public void TestHVLV_SubShipmentDataDataSourceTypeIsEmpty()
		{
			var subShipmentDataContext = (DataContext)DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
			subShipmentDataContext.DataSourceCollection = new List<DataSource>
			{
				new DataSource { Type = ZString.Empty }
			};

			AssertImportSubShipmentsAsHVLVConsignments(subShipmentDataContext);
		}

		public void TestHVLV_DoNotImport_IfSubShipmentDataDataSourceIsHVLVConsignment_ForSTDShipmentType()
		{
			var dataSource = DataContextFactory.New();
			dataSource.AddDataSource(DataContextType.HVLVConsignment, "SUBSHIPMENT1");
			dataSource.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataSource,
				WayBillNumber = "SUBSHIP",
				WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" },
				GoodsDescription = "SUB GOODS"
			};

			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.StandardHouse, Description = Core.Constants.ShipmentTypeDescriptions.StandardHouse };
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment });

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("shipmentBO.CoLoadShipments.Count", 0, shipmentBO.CoLoadShipments.Count);
		}

		public void TestHVLV_MatchedShipment_ExistingConsignmentHeaderIsReUsed()
		{
			var shipmentBO = AssertImportSubShipmentsAsHVLVConsignments(null);
			var consignmentHeaderBO = shipmentBO.HVLVConsignmentHeader;
			Factory.SaveForTesting();

			var reimportedShipmentBO = AssertImportSubShipmentsAsHVLVConsignments(null);
			CombineAssertions(() =>
			{
				AssertEquals("Shipment was matched", shipmentBO.PK, reimportedShipmentBO.PK);

				var consignmentHeaders = Factory.BOFactory.Load<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, reimportedShipmentBO.PK));
				AssertEquals("Consignment header was re-used", consignmentHeaderBO.PK, consignmentHeaders.Single().PK);
				AssertEquals("Consignment header was re-used", consignmentHeaderBO.PK, reimportedShipmentBO.HVLVConsignmentHeader.PK);
			});
		}

		ForwardingShipment AssertImportSubShipmentsAsHVLVConsignments(IDataContextDataObject subShipmentDataContext)
		{
			if (Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BILLTOPARTY")) == null)
			{
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				billToParty.OH_Code = "BILLTOPARTY";
				var billToPartyContact = billToParty.Contacts.AddNew();
				billToPartyContact.OC_ContactName = "LARRY";

				Factory.SaveForTesting();
			}

			var organizations = new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" },
				new OrganizationAddress { AddressType = nameof(DocAddressType.ConsignorPickupDeliveryAddress), OrganizationCode = "DISPATCHORG" },
			};

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = subShipmentDataContext,
				WayBillNumber = "SUBSHIP",
				WayBillType = new WayBillType() { Code = "HWB", Description = "House Waybill" },
				GoodsDescription = "SUB GOODS",
			};
			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 1, OrderReference = "ShipRef100" }
				});

			shipmentDataObject.IsLastMileDeliverySelfBooked = true;
			shipmentDataObject.ServiceLevel = new ServiceLevel { Code = "STD" };
			shipmentDataObject.SetOrganizationAddressCollection(() => organizations);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue, Description = Core.Constants.ShipmentTypeDescriptions.HighVolumeLowValue };
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertEquals("shipmentBO.CoLoadShipments.Count", 0, shipmentBO.CoLoadShipments.Count);

			var consignmentHeader = Factory.BOFactory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipmentBO.PK));
			AssertNotNull(consignmentHeader);
			var query = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, consignmentHeader.PK);
			var consignments = shipmentBO.Factory.Load<IHVLVConsignment>(query);
			AssertNotNull(consignments);
			AssertEquals(1, consignments.Length);

			var subQuery = new ZQuery(HVLVItemSchema.HVI_HVC_Consignment, consignments[0].PK);
			query = subQuery.AddToFilter(new ZQuery(HVLVItemSchema.HVI_JS_LoadedOnShipment, shipmentBO.PK));
			var items = shipmentBO.Factory.Load<IHVLVItem>(query);
			AssertNotNull(items);
			AssertEquals(1, items.Length);

			return shipmentBO;
		}

		#endregion

		#region Related Parties

		public void TestConsigneeRelatedDeliveryAgentDoesDefaultWhenImportingXML()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONS001";

			var consigneeDeliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			consigneeDeliveryAgent.OH_Code = "CDLV001";
			consignee.SetRelatedParty(consigneeDeliveryAgent, RelatedPartyTypeList.Codes.DeliveryAgent, RelatedPartyDirectionList.Codes.Delivery, "SEA", "LCL");

			Factory.SaveForTesting();

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			var consigneeAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			consigneeAddress.OrganizationCode = "CONS001";
			consigneeAddress.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);
			shipmentDataObject.OrganizationAddressCollection.Add(consigneeAddress);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();

			AssertNotNull(shipmentBO);
			AssertEquals("Delivery Agent should not be defaulted when importing XML", consigneeDeliveryAgent.PK, shipmentBO.JS_OH_DeliveryAgent);
		}

		public void TestDefaultPickupDeliveryCFSWhenCalculateDeliveryDueDateIsEnabled()
		{
			var defaultPickupCFSCalls = 0;
			var defaultDeliveryCFSCalls = 0;
			var existingShipment = Factory.New<ForwardingShipment>();
			existingShipment.JS_HouseBill = "HBL123";
			existingShipment.OnDefaultPickupCFSForDeliveryDueDate += delegate
			{ defaultPickupCFSCalls++; };
			existingShipment.OnDefaultDeliveryCFSForDeliveryDueDate += delegate
			{ defaultDeliveryCFSCalls++; };
			Factory.SaveForTesting();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				shipmentDataObject.WayBillNumber = "HBL123";
				shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
				shipmentDataObject.OrganizationAddressCollection.Add(GetUnmatchOrganizationAddress(DocAddressType.ConsignorDocumentaryAddress, "CORDOC", "Sydney", "NSW", "2000"));
				shipmentDataObject.OrganizationAddressCollection.Add(GetUnmatchOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, "CEEDOC", "Melbourne", "VIC", "3000"));
				shipmentDataObject.OrganizationAddressCollection.Add(GetUnmatchOrganizationAddress(DocAddressType.ConsignorPickupDeliveryAddress, "CORPIC", "Perth", "WA", "4000"));
				shipmentDataObject.OrganizationAddressCollection.Add(GetUnmatchOrganizationAddress(DocAddressType.ConsigneePickupDeliveryAddress, "CORDLV", "Tasmania", "TAS", "5000"));

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
				var shipmentBO = reader.ReadIntoBusinessObject();

				AssertEquals("Default Pickup CFS after populate ConsignorDocumentaryAddress and ConsignorPickupDeliveryAddress.", 2, defaultPickupCFSCalls);
				AssertEquals("Default Delivery CFS after populate ConsigneeDocumentaryAddress and ConsigneePickupDeliveryAddress.", 2, defaultDeliveryCFSCalls);
			}
		}

		OrganizationAddress GetUnmatchOrganizationAddress(DocAddressType addressType, string orgCode, string city, string state, string postcode)
		{
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType.ToString(),
				OrganizationCode = orgCode,
				CompanyName = orgCode + "WUFU SHIPPING LINE",
				Address1 = orgCode + "Level 2, Building G",
				Address2 = orgCode + "34 Dock Lane",
				City = city,
				State = state,
				Postcode = postcode,
				Country = new Country() { Code = "AU", Name = "Australia" },
			};
			return addressData;
		}

		#endregion

		#region Workflow Exceptions

		public void TestImportWorkflowExceptions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.SaveForTesting();

			AssertNull("No ExceptionCollection on XML", shipmentDataObject.ExceptionCollection);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			shipment = reader.ReadIntoBusinessObject();

			AssertEquals("No ExceptionCollection, should import without errors ", 0, shipment.WorkflowItems.Exceptions.Count);
			AssertMultilineASCIIEquals("logs", @"
Information - No matching ForwardingShipment found, creating new ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Added Shipment from UniversalShipment.", Logger.Logs.Trim());

			Factory.SaveForTesting();
			Logger.ClearLogs();

			shipmentDataObject.SetExceptionCollection(() => new List<WorkflowException>());
			shipment = reader.ReadIntoBusinessObject();

			AssertEquals("No Exception on ExceptionCollection, should import without errors ", 0, shipment.WorkflowItems.Exceptions.Count);
			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - Updated Shipment S00001001 from UniversalShipment.", Logger.Logs.Trim());

			Factory.SaveForTesting();
			Logger.ClearLogs();

			var now = ZDateTimeOffset.Now;

			var workflowException = new[]
			{
				new WorkflowException()
				{
					Description = "Exception 1",
					Date = now,
				},
				new WorkflowException()
				{
					Description = "Exception 2",
					Date = now,
					Actioned = true
				},
			};

			shipmentDataObject.SetExceptionCollection(() => workflowException.ToList());
			shipment = reader.ReadIntoBusinessObject();

			AssertEquals("Exceptions on ExceptionCollection, should import without errors ", 2, shipment.WorkflowItems.Exceptions.Count);

			var exception1 = shipment.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 1");
			AssertNotNull("Exception 1 is in shipment", exception1);
			AssertEquals(now, exception1.P9_ActualDateOffset);
			AssertEquals(false, exception1.IsExceptionActioned);

			var exception2 = shipment.WorkflowItems.Exceptions.Cast<ProcessTask>().FirstOrDefault(e => e.P9_Description == "Exception 2");
			AssertNotNull("Exception 2 is in shipment", exception2);
			AssertEquals(now, exception2.P9_ActualDateOffset);
			AssertEquals(true, exception2.IsExceptionActioned);

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching ForwardingShipment.
Information - Populating ForwardingShipment...
Information - No matching Exception: Exception 1 found, creating new Exception: Exception 1.
Information - Populating Exception: Exception 1...
Information - No matching Exception: Exception 2 found, creating new Exception: Exception 2.
Information - Populating Exception: Exception 2...
Information - Updated Shipment S00001001 from UniversalShipment.", Logger.Logs.Trim());
		}

		#endregion

		#region JS_NoOriginalBills and JS_NoCopyBills

		public void TestDefaultNumberOfBills_WithoutReleaseType()
		{
			AssertDefaultNumberOfBills(string.Empty, string.Empty, 5, 6);
		}

		public void TestDefaultNumberOfBills_WithReleaseType()
		{
			AssertDefaultNumberOfBills(Core.Constants.ShipmentReleaseTypes.ExpressBofL, "Express Bill of Lading", 0, 4);
		}

		void AssertDefaultNumberOfBills(string releaseTypeCode, string releaseTypeDescription, int expectedOriginals, int expectedCopies)
		{
			var releaseTypes = FreightDataRegistry.Instance.ReleaseTypes.DefaultValue;
			releaseTypes.OriginalsNumber = 5;
			releaseTypes.CopiesNumber = 6;

			var expressReleaseType = releaseTypes.Types.FindByCode(Core.Constants.ShipmentReleaseTypes.ExpressBofL);
			expressReleaseType.OriginalsNumber = 0;
			expressReleaseType.CopiesNumber = 4;

			FreightDataRegistry.Instance.ReleaseTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseTypes);

			shipmentDataObject.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001000");
			shipmentDataObject.NoOriginalBills = null;
			shipmentDataObject.NoCopyBills = null;
			shipmentDataObject.ReleaseType = new CodeDescriptionPair
			{
				Code = releaseTypeCode,
				Description = releaseTypeDescription
			};

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, null);
			var shipmentBO = reader.ReadIntoBusinessObject();
			AssertNotNull("Precondition", shipmentBO);
			AssertEquals("Originals", (ZByte)expectedOriginals, shipmentBO.JS_NoOriginalBills);
			AssertEquals("Copies", (ZByte)expectedCopies, shipmentBO.JS_NoCopyBills);
		}

		#endregion

		public void TestImportShipmentWithoutJS_HBLAWBChargesDisplay()
		{
			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				eAdaptorRegistry.Instance.UniversalXMLUpdateShipmentDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var warehouseAddressBO = Factory.NewWithValidTestData<OrgAddress>();
				Factory.SaveForTesting();

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "S00001640";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRMRS";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_HBLAWBChargesDisplay = "ALL";
				shipment.JS_HouseBill = "BACON PANCAKES";
				shipment.JS_OA_ExportReceivingDepot = warehouseAddressBO.PK;
				Factory.SaveForTesting();

				var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
				consolBO.JK_OA_PackDepotAddress = warehouseAddressBO.PK;
				consolBO.Shipments.Add(shipment);

				shipmentDataObject.DataContext.AddDataSource(DataContextType.TransitReceive, "S00001640");
				shipmentDataObject.TransportMode = new CodeDescriptionPair { Code = TransportModes.Air };
				shipmentDataObject.HBLAWBChargesDisplay = null;
				shipmentDataObject.DataContext.DataSourceCollection.First().Type = "TransitReceive";
				shipmentDataObject.WayBillNumber = "BACON PANCAKES";

				var addresses = new List<OrganizationAddress>();
				addresses.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressShortCode = warehouseAddressBO.AddressCode, AddressType = nameof(DocAddressType.LocalCartageCFS), OrganizationCode = warehouseAddressBO.Header.OH_Code });
				shipmentDataObject.SetOrganizationAddressCollection(() => addresses);

				var packlines = new DataObjectList<PackingLine>();
				var packline1 = CreatePackline();
				packline1.ReferenceNumber = "PKG1";

				packlines.Add(packline1);
				AddEmptyTRUReferences(packlines);
				shipmentDataObject.SetPackingLineCollection(() => packlines);
				Factory.SaveForTesting();

				var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
				shipment = reader.ReadIntoBusinessObject();

				var helper = new ShipmentDataObjectReadingHelper(shipmentDataObject, Logger, Factory);
				AssertEquals(true, helper.IsTWFunctionalityEnabled(shipment));

				AssertEquals("ALL", shipment.JS_HBLAWBChargesDisplay);
			}
		}

		public void TestWarehouseLocation_WhenUXMLExceedsMaxSize_ShouldTruncateWithoutErrors()
		{
			var validWarehouseLocation = "WAREHOUSE1";

			var shipment = Factory.New<ForwardingShipment>();
			Factory.SaveForTesting();

			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
			consolBO.Shipments.Add(shipment);

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.WarehouseLocation = "WAREHOUSE1 location exceeds 10chars from UXML";

			var reader = new ShipmentDataObjectReader(shipmentDataObject, Logger, Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			shipment = reader.ReadIntoBusinessObject();

			AssertEquals("Warehouse Location should only contain 10 characters.", validWarehouseLocation, shipment.JS_WarehouseLocation);
			AssertEquals("Warehouse Location should have correct length.", 10, shipment.JS_WarehouseLocation.Length);
		}

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Freight.Business.Testing.FreightTestHelper.TryRemovePackLineIdSequence();
		}

		protected override void TearDown()
		{
			base.TearDown();

			Freight.Business.Testing.FreightTestHelper.TryRemovePackLineIdSequence();
		}

		static string GetResourcePathFor(string fileName)
		{
			return $"Enterprise.Freight.Forwarding.DataTransfer.Test.Universal.Shipment.TestFiles.{fileName}";
		}

		ServiceTaskLogForTesting CreateAndProcessUniversalShipment(string fileName)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var message = GetQueuedUniversalShipmentMessage(resourceRetriever.GetString(GetResourcePathFor(fileName)));
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				return serviceTaskLog;
			}
		}

		UNDG UndgItem()
		{
			var item = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			item.Contact = new OrganizationContact();
			item.Contact.FullName = "Contact1";
			item.Contact.Phone = "123456";
			item.FlashPoint = "0.1";
			item.IMOClass = "Clas";
			item.MarinePollutant = new UNDGMarinePollutant();
			item.MarinePollutant.Code = "Y";
			item.MarinePollutant.Description = "Marine Pollute";
			item.PackedInLimitedQuantity = false;
			item.PackingGroup = "Gr1";
			item.ProperShippingName = "Name1";
			item.TechicalName = "Tech1";
			item.UNDGCode = "3000c";
			item.Volume = 1m;
			item.Weight = 2m;
			item.WeightUQ = new UnitOfWeight();
			item.WeightUQ.Code = "kg";
			item.WeightUQ.Description = "kilo";
			item.VolumeUQ = new UnitOfVolume();
			item.VolumeUQ.Code = "m3";
			item.VolumeUQ.Description = "cubic";
			return item;
		}

		internal static void AssertContents(ForwardingShipment shipmentBO)
		{
			AssertEquals("shipmentBO.JS_TransportMode", "SEA", shipmentBO.JS_TransportMode);
			AssertEquals("shipmentBO.JS_PackingMode", "LCL", shipmentBO.JS_PackingMode);
			AssertEquals("shipmentBO.JS_ShipmentType", "STD", shipmentBO.JS_ShipmentType);

			AssertEquals("shipmentBO.JS_AWBServiceLevel", "ELG", shipmentBO.JS_AWBServiceLevel);
			AssertEquals("shipmentBO.JS_BookingReference", "BOOK ME", shipmentBO.JS_BookingReference);
			AssertEquals("shipmentBO.JS_CartageWaybill", "CARTAGE BILL", shipmentBO.JS_CartageWaybill);
			AssertEquals("shipmentBO.JS_CFSReference", "CFS Book Ref", shipmentBO.JS_CFSReference);
			AssertEquals("shipmentBO.JS_DocumentedVolume", 3.45m, shipmentBO.JS_DocumentedVolume);
			AssertEquals("shipmentBO.JS_DocumentedWeight", 4.56m, shipmentBO.JS_DocumentedWeight);
			AssertEquals("shipmentBO.JS_UnitFreightRate", 67.89m, shipmentBO.JS_UnitFreightRate);
			AssertEquals("shipmentBO.JS_RX_NKFrtRateCurrency", Core.Constants.CurrencyCodes.CzechRepublic, shipmentBO.JS_RX_NKFrtRateCurrency);
			AssertEquals("shipmentBO.JS_GoodsDescription", "RAT HATS", shipmentBO.JS_GoodsDescription);
			AssertEquals("shipmentBO.JS_GoodsValue", 5.67m, shipmentBO.JS_GoodsValue);
			AssertEquals("shipmentBO.JS_RX_NKGoodsValueCurr", Core.Constants.CurrencyCodes.Ghana, shipmentBO.JS_RX_NKGoodsValueCurr);
			AssertEquals("shipmentBO.JS_HBLAWBChargesDisplay", "FOO", shipmentBO.JS_HBLAWBChargesDisplay);
			AssertEquals("shipmentBO.JS_HBLContainerPackModeOverride", "FAR", shipmentBO.JS_HBLContainerPackModeOverride);
			AssertEquals("shipmentBO.JS_InsuranceValue", 6.78m, shipmentBO.JS_InsuranceValue);
			AssertEquals("shipmentBO.JS_RX_NKInsuranceCurrency", Core.Constants.CurrencyCodes.Kenya, shipmentBO.JS_RX_NKInsuranceCurrency);
			AssertEquals("shipmentBO.JS_InterimReceipt", "IR Text", shipmentBO.JS_InterimReceipt);

			AssertEquals("shipmentBO.JS_IsBooking", false, shipmentBO.JS_IsBooking);
			AssertEquals("shipmentBO.JS_IsCFSRegistered", false, shipmentBO.JS_IsCFSRegistered);
			AssertEquals("shipmentBO.JS_IsForwardRegistered", true, shipmentBO.JS_IsForwardRegistered);
			AssertEquals("shipmentBO.JS_IsNeutralMaster", false, shipmentBO.JS_IsNeutralMaster);
			AssertEquals("shipmentBO.JS_IsShipping", false, shipmentBO.JS_IsShipping);
			AssertEquals("shipmentBO.JS_IsSplitShipment", false, shipmentBO.JS_IsSplitShipment);

			AssertEquals("shipmentBO.JS_ManifestedVolume", 8.90m, shipmentBO.JS_ManifestedVolume);
			AssertEquals("shipmentBO.JS_ManifestedWeight", 9.01m, shipmentBO.JS_ManifestedWeight);
			AssertEquals("shipmentBO.JS_OuterPacks", 44, shipmentBO.JS_OuterPacks);
			AssertEquals("shipmentBO.JS_F3_NKPackType", "VF", shipmentBO.JS_F3_NKPackType);

			AssertEquals("shipmentBO.JS_PackingOrder", 1, shipmentBO.JS_PackingOrder);
			AssertEquals("shipmentBO.JS_ReleaseType", "CAD", shipmentBO.JS_ReleaseType);
			AssertEquals("shipmentBO.JS_RS_NKServiceLevel", "PFT", shipmentBO.JS_RS_NKServiceLevel);
			AssertEquals("shipmentBO.JS_INCO", "CIF", shipmentBO.JS_INCO);
			AssertEquals("shipmentBO.JS_ShippedOnBoard", "LDN", shipmentBO.JS_ShippedOnBoard);

			AssertEquals("shipmentBO.JS_ShipperCODAmount", 12.34m, shipmentBO.JS_ShipperCODAmount);
			AssertEquals("shipmentBO.JS_ShipperCODPayMethod", "COC", shipmentBO.JS_ShipperCODPayMethod);
			AssertEquals("shipmentBO.JS_ShipmentStatus", "BLA", shipmentBO.JS_ShipmentStatus);

			AssertEquals("shipmentBO.JS_TotalPackageCount", 45, shipmentBO.JS_TotalPackageCount);
			AssertEquals("shipmentBO.JS_F3_NKTotalCountPackType", "KEG", shipmentBO.JS_F3_NKTotalCountPackType);

			AssertEquals("shipmentBO.JS_ActualVolume", 23.45m, shipmentBO.JS_ActualVolume);
			AssertEquals("shipmentBO.JS_UnitOfVolume", "CF", shipmentBO.JS_UnitOfVolume);
			AssertEquals("shipmentBO.JS_ActualWeight", 34.56m, shipmentBO.JS_ActualWeight);
			AssertEquals("shipmentBO.JS_UnitOfWeight", "KT", shipmentBO.JS_UnitOfWeight);

			AssertEquals("shipmentBO.JS_TranshipToOtherCFS", true, shipmentBO.JS_TranshipToOtherCFS);

			AssertEquals("shipmentBO.JS_RL_NKOrigin", "NZDUD", shipmentBO.JS_RL_NKOrigin);
			AssertEquals("shipmentBO.JS_RL_NKDestination", "AUBDG", shipmentBO.JS_RL_NKDestination);

			AssertEquals("shipmentBO.JS_WarehouseLocation", "HOME", shipmentBO.JS_WarehouseLocation);

			AssertEquals("shipmentBO.JS_ActualChargeable", 123.45m, shipmentBO.JS_ActualChargeable);
			AssertEquals("shipmentBO.JS_DocumentedChargeable", 2.34m, shipmentBO.JS_DocumentedChargeable);
			AssertEquals("shipmentBO.JS_ManifestedChargeable", 7.89m, shipmentBO.JS_ManifestedChargeable);

			AssertEquals("shipmentBO.JS_NoCopyBills", new ZByte(3), shipmentBO.JS_NoCopyBills);
			AssertEquals("shipmentBO.JS_NoOriginalBills", new ZByte(4), shipmentBO.JS_NoOriginalBills);
		}

		ChargeLine GetChargeLine(ZString? branchCode, ZString? chargeCode, ZString? costAPInvoiceNumber, ZDateTime? costDueDate, ZString? costGSTVATID, ZDateTime? costInvoiceDate,
			ZDecimal? costLocalAmount, ZDecimal? costOSAmount, ZString? costOSCurrency, ZDecimal? costOSGSTVATAmount, ZString? creditor, ZString? debtor, ZString? departmentCode,
			ZString? description, ZShort? displaySequence, ZString? sellGSTVATID, ZString? sellInvoiceType, ZDecimal? sellLocalAmount, ZDecimal? sellOSAmount,
			ZString? sellOSCurrency, ZDecimal? sellOSGSTVATAmount)
		{
			ChargeLine chargeLine = new ChargeLine(DefaultDataObjectWriterStrategy.TestInstance);

			if (branchCode.HasValue)
			{
				chargeLine.Branch = new Branch();
				chargeLine.Branch.Code = branchCode;
				chargeLine.Branch.Name = "EDIHQ";
			}

			if (chargeCode.HasValue)
			{
				chargeLine.ChargeCode = new ChargeCode();
				chargeLine.ChargeCode.Code = chargeCode;
				chargeLine.ChargeCode.Description = "International Freight";
			}

			chargeLine.CostAPInvoiceNumber = costAPInvoiceNumber;
			chargeLine.CostDueDate = costDueDate;

			if (costGSTVATID.HasValue)
			{
				chargeLine.CostGSTVATID = new TaxID();
				chargeLine.CostGSTVATID.TaxCode = costGSTVATID;
				chargeLine.CostGSTVATID.Description = "Cost Tax Description";
			}

			chargeLine.CostInvoiceDate = costInvoiceDate;
			chargeLine.CostLocalAmount = costLocalAmount;
			chargeLine.CostOSAmount = costOSAmount;

			if (costOSCurrency.HasValue)
			{
				chargeLine.CostOSCurrency = new Currency();
				chargeLine.CostOSCurrency.Code = costOSCurrency;
				chargeLine.CostOSCurrency.Description = "Cost Currency Description";
			}

			chargeLine.CostOSGSTVATAmount = costOSGSTVATAmount;

			if (creditor.HasValue)
			{
				chargeLine.Creditor = new OrganizationReference();
				chargeLine.Creditor.Key = creditor;
				chargeLine.Creditor.Type = nameof(DataContextType.Organization);
			}

			if (debtor.HasValue)
			{
				chargeLine.Debtor = new OrganizationReference();
				chargeLine.Debtor.Key = debtor;
				chargeLine.Debtor.Type = nameof(DataContextType.Organization);
			}

			if (departmentCode.HasValue)
			{
				chargeLine.Department = new Department();
				chargeLine.Department.Code = departmentCode;
				chargeLine.Department.Name = "Test Department";
			}

			chargeLine.Description = description;
			chargeLine.DisplaySequence = displaySequence;

			if (sellGSTVATID.HasValue)
			{
				chargeLine.SellGSTVATID = new TaxID();
				chargeLine.SellGSTVATID.TaxCode = sellGSTVATID;
				chargeLine.SellGSTVATID.Description = "Sell Tax Description";
			}

			chargeLine.SellInvoiceType = sellInvoiceType;
			chargeLine.SellLocalAmount = sellLocalAmount;
			chargeLine.SellOSAmount = sellOSAmount;

			if (sellOSCurrency.HasValue)
			{
				chargeLine.SellOSCurrency = new Currency();
				chargeLine.SellOSCurrency.Code = sellOSCurrency;
				chargeLine.SellOSCurrency.Description = "Sell Currency Description";
			}

			chargeLine.SellOSGSTVATAmount = sellOSGSTVATAmount;

			return chargeLine;
		}

		void AssertCharge(JobCharge charge, ChargeLine chargeLine)
		{
			if (chargeLine.Branch != null && chargeLine.Branch.Code.HasValue)
			{
				AssertNotNull("charge.Branch should not be null", charge.Branch);
				AssertEquals("Branch should be equal", chargeLine.Branch.Code, charge.Branch.GB_Code);
			}

			if (chargeLine.ChargeCode != null && chargeLine.ChargeCode.Code.HasValue)
			{
				AssertNotNull("charge.ChargeCode should not be null", charge.ChargeCode);
				AssertEquals("ChargeCode should be equal", chargeLine.ChargeCode.Code, charge.ChargeCode.AC_Code);
			}

			if (chargeLine.CostAPInvoiceNumber.HasValue)
			{
				AssertEquals("APInvoiceNum should be equal", chargeLine.CostAPInvoiceNumber, charge.JR_APInvoiceNum);
			}

			if (chargeLine.CostDueDate.HasValue)
			{
				AssertEquals("CostDueDate should be equal", chargeLine.CostDueDate, charge.JR_PaymentDate);
			}

			if (chargeLine.CostGSTVATID != null && chargeLine.CostGSTVATID.TaxCode.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertNotNull("CostGSTRate should not be null", charge.CostGSTRate);
				AssertEquals("CostGSTRate should be equal", chargeLine.CostGSTVATID.TaxCode, charge.CostGSTRate.AT_Code);
			}

			if (chargeLine.CostInvoiceDate.HasValue)
			{
				AssertEquals("CostInvoiceDate should be equal", chargeLine.CostInvoiceDate, charge.JR_APInvoiceDate);
			}

			if (chargeLine.CostLocalAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostLocalAmount, charge.JR_LocalCostAmt);
			}

			if (chargeLine.CostOSAmount.HasValue)
			{
				AssertEquals("Cost Local Amount should be equal", chargeLine.CostOSAmount, charge.JR_OSCostAmt);
			}

			if (chargeLine.CostOSCurrency != null && chargeLine.CostOSCurrency.Code.HasValue)
			{
				AssertEquals("Cost Currency should be equal", chargeLine.CostOSCurrency.Code, charge.JR_RX_NKCostCurrency);
			}

			if (chargeLine.CostOSGSTVATAmount.HasValue && !charge.JR_AT_CostGSTRate.IsEmpty)
			{
				AssertEquals("Cost OS GST VAT Amount should be equal", chargeLine.CostOSGSTVATAmount, charge.JR_OSCostGSTAmt_Calc);
			}

			if (chargeLine.Creditor != null && chargeLine.Creditor.Key.HasValue)
			{
				AssertNotNull("CostAccount should not be null", charge.CostAccount);
				AssertEquals("Creditor should be equal", chargeLine.Creditor.Key, charge.CostAccount.OH_Code);
			}

			if (chargeLine.Debtor != null && chargeLine.Debtor.Key.HasValue)
			{
				AssertNotNull("SellAccount should not be null", charge.SellAccount);
				AssertEquals("Debtor should be equal", chargeLine.Debtor.Key, charge.SellAccount.OH_Code);
			}

			if (chargeLine.Department != null && chargeLine.Department.Code.HasValue)
			{
				AssertNotNull("Department should not be null", charge.Department);
				AssertEquals("Department should be equal", chargeLine.Department.Code, charge.Department.GE_Code);
			}

			if (chargeLine.Description.HasValue)
			{
				AssertEquals("Description should be equal", chargeLine.Description, charge.JR_Desc);
			}

			if (chargeLine.DisplaySequence.HasValue)
			{
				AssertEquals("DisplaySequence should be equal", chargeLine.DisplaySequence, charge.JR_DisplaySequence);
			}

			if (chargeLine.SellGSTVATID != null && chargeLine.SellGSTVATID.TaxCode.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertNotNull("SellGSTRate should not be null", charge.SellGSTRate);
				AssertEquals("SellGSTRate should be equal", chargeLine.SellGSTVATID.TaxCode, charge.SellGSTRate.AT_Code);
			}

			if (chargeLine.SellInvoiceType.HasValue)
			{
				AssertEquals("InvoiceType should be equal", chargeLine.SellInvoiceType, charge.JR_InvoiceType);
			}

			if (chargeLine.SellLocalAmount.HasValue)
			{
				AssertEquals("Sell Local Amount should be equal", chargeLine.SellLocalAmount, charge.JR_LocalSellAmt);
			}

			if (chargeLine.SellOSAmount.HasValue)
			{
				AssertEquals("Sell OS Amount should be equal", chargeLine.SellOSAmount, charge.JR_OSSellAmt);
			}

			if (chargeLine.SellOSCurrency != null && chargeLine.SellOSCurrency.Code.HasValue)
			{
				AssertEquals("Sell OS Currency should be equal", chargeLine.SellOSCurrency.Code, charge.JR_RX_NKSellCurrency);
			}

			if (chargeLine.SellOSGSTVATAmount.HasValue && !charge.JR_AT_SellGSTRate.IsEmpty)
			{
				AssertEquals("SellOSGSTVAT Amount should be equal", chargeLine.SellOSGSTVATAmount, charge.JR_OSSellGSTAmt_Calc);
			}
		}

		class DummyShipmentDataObjectReader : ShipmentDataObjectReader
		{
			public DummyShipmentDataObjectReader(UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ChildShipmentsParent parent)
				: base(shipmentDataObject, logger, factory, parent)
			{
			}

			public new ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(ForwardingShipment targetBO)
				=> base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		#endregion
	}
}
