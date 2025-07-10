using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public abstract partial class BaseShipmentDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestLoadingMeters_ValidValue()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_LoadingMeters = 2.5;

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData should not be null", shipmentData);
			AssertNotNull("shipmentData.TotalLoadingMeters should not be null", shipmentData.TotalLoadingMeters);
			AssertEquals("TotalLoadingMeters is populated", (ZDecimal)2.5, shipmentData.TotalLoadingMeters);
		}

		public void TestLoadingMeters_ZeroValue()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_LoadingMeters = 0;

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			AssertNotNull("shipmentData should not be null", shipmentData);
			AssertNull("TotalLoadingMeters should be null", shipmentData.TotalLoadingMeters);
		}

		public void TestPickupLocalTransportAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.DocsAndCartage.JP_OA_PickupCartageCoAddr = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;

			#endregion

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB(AddressTypes.PickupLocalCartage,
				shipmentData.OrganizationAddressCollection.Single(),
				AddressTypes.PickupLocalCartage);

			#endregion
		}

		public void TestDeliveryAgentAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_OH_DeliveryAgent = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			#endregion

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB("DeliveryAgent",
				shipmentData.OrganizationAddressCollection.Single(),
				"DeliveryAgent");

			#endregion
		}

		public void TestConsignorDocumentaryAddressContactIsPopulated()
		{
			var factory = new BusinessObjectFactory();
			var shipmentBO = factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "Test Name";

			var address = orgHeader.MainAddress;
			address.Address1 = "Place";
			address.Address2 = "123 Test Street";

			var orgDocument = orgContact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = ContactType.Consignor.Code;
			orgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Print;

			shipmentBO.ConsignorDocumentaryAddress.E2_OA_Address = address.PK;

			AssertEquals("prerequisite; contact has been defaulted from document group", orgContact, shipmentBO.ConsignorDocumentaryAddress.Contact);

			factory.Save();
			#endregion

			shipmentBO = Factory.Load<ForwardingShipment>(shipmentBO.PK);

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			var addresses = shipmentData.OrganizationAddressCollection?.Select(a => $"{a.AddressType}|{a.Contact}");

			AssertContainsExactElementsInAnyOrder("OrganizationAddressCollection", new[] { "ConsignorDocumentaryAddress|Test Name" }, addresses);
		}

		public void TestAllJobDocAdressesAreExported()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			var expectedAddresses = new List<ZString>();

			foreach (System.Reflection.PropertyInfo propertyInfo in typeof(ForwardingShipment).GetProperties())
			{
				if (typeof(JobDocAddress).IsAssignableFrom(propertyInfo.PropertyType) && !propertyInfo.Name.StartsWith("Get"))
				{
					var address = propertyInfo.GetValue(shipmentBO, null) as JobDocAddress;
					if (address == null)
					{
						continue;
					}

					address.E2_AddressOverride = true;
					var addressType = address.DocAddressType.ToString();
					expectedAddresses.Add(addressType);

					if (SchemaVersionManager.Current == UniversalXmlSchema.Version_2011_11 && addressType == "ControllingCustomer")
					{
						expectedAddresses.Add(LegacyUniversalAddressTypes.LegacyShipmentControllingPartyAddressType);
					}
				}
			}

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);
			var addresses = shipmentData.OrganizationAddressCollection.Select(a => a.AddressType);
			AssertContainsExactElementsInAnyOrder("All job doc addresses should be exported", expectedAddresses, addresses);
		}

		public void TestPopulateWarehouseAddress()
		{
			var org = Factory.New<OrgHeader>();
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.DocAddresses.CreateWithAddressType(DocAddressType.Warehouse).E2_OA_Address = org.MainAddress.PK;

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);
			var addresses = shipmentData.OrganizationAddressCollection.Select(a => a.AddressType);
			AssertEquals("The only address shipment data should have is the warehouse address.", 1, addresses.Count());
			AssertCollectionContains("Exported addresses should contain warehouse if shipment has one.", nameof(DocAddressType.Warehouse), addresses);
		}

		public void TestWarehouseClientJobDocAdresseIsExported()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.DocAddresses.CreateWithAddressType(DocAddressType.WarehouseClient).E2_OA_Address = org.MainAddress.PK;

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);
			var addresses = shipmentData.OrganizationAddressCollection.Select(a => a.AddressType);
			AssertEquals("Address should have a count of 1.", 1, addresses.Count());
			AssertCollectionContains("Exported addresses should contain Warehouse Client if shipment has one.", nameof(DocAddressType.WarehouseClient), addresses);
		}

		public void TestWarehouseClient_IsNotExportedByDefault()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);
			AssertEquals("Exported addresses should not contain Warehouse Client by default.", null, shipmentData.OrganizationAddressCollection);
		}

		public void TestPickupCFSAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_OA_ExportReceivingDepot = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress.PK;

			#endregion

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			#region Check Contents of shipmentData object

			AssertNotNull("shipmentData", shipmentData);
			AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
			AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

			AssertOrganizationBO_WUFSHIJNB("DepartureCFSAddress",
				shipmentData.OrganizationAddressCollection.Single(),
				"DepartureCFSAddress");

			#endregion
		}

		public void TestExportBrokerAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_OH_ExportBroker = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ExportBroker", shipmentData.OrganizationAddressCollection.Single(), "ExportBroker");
		}

		public void TestImportBrokerAddress()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_OH_ImportBroker = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			AssertEquals(1, shipmentData.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ImportBroker", shipmentData.OrganizationAddressCollection.Single(), "ImportBroker");
		}

		public void TestControllingCustomer_2011_11()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.Address1 = "123 Test Street";
			shipmentBO.DocAddresses.AddNew(orgAddress, DocAddressType.ControllingCustomer);

			#endregion

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

				#region Check Contents of shipmentData object

				AssertNotNull("shipmentData", shipmentData);
				AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
				AssertEquals("shipmentData.OrganizationAddressCollection.Count", 2, shipmentData.OrganizationAddressCollection.Count);

				var controllingCustomerAddress = shipmentData.OrganizationAddressCollection.FirstOrDefault("ControllingCustomer");
				AssertEquals("123 Test Street", controllingCustomerAddress.Address1);

				var shipmentControllingPartyAddress = shipmentData.OrganizationAddressCollection.FirstOrDefault("ShipmentControllingParty");
				AssertEquals("Controlling Customer is also exported as ShipmentControllingParty", "123 Test Street", shipmentControllingPartyAddress.Address1);

				#endregion
			}
		}

		public void TestControllingCustomer_2012_11()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			#region Setup shipmentBO

			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.Address1 = "123 Test Street";
			shipmentBO.DocAddresses.AddNew(orgAddress, DocAddressType.ControllingCustomer);

			#endregion

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

				#region Check Contents of shipmentData object

				AssertNotNull("shipmentData", shipmentData);
				AssertNotNull("shipmentData.OrganizationAddressCollection", shipmentData.OrganizationAddressCollection);
				AssertEquals("shipmentData.OrganizationAddressCollection.Count", 1, shipmentData.OrganizationAddressCollection.Count);

				var controllingCustomerAddress = shipmentData.OrganizationAddressCollection.FirstOrDefault("ControllingCustomer");
				AssertEquals("ControllingCustomer is the only exported address", "123 Test Street", controllingCustomerAddress.Address1);

				#endregion
			}
		}

		public void TestCusEntryNumbersAreExported()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var cusEntryNumber = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);

			shipmentBO.CusEntryNumbers.Add(cusEntryNumber);

			var writer = GetNewShipmentDataObjectWriter(shipmentBO);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("Precondition: shipmentData.EntryNumberCollection", shipmentData.EntryNumberCollection);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentData.EntryNumberCollection.Count", 1, shipmentData.EntryNumberCollection.Count);

				var entryNumberDataObject = shipmentData.EntryNumberCollection[0];
				EntryNumberDataObjectWriterTest.AssertContents(entryNumberDataObject);
			});
		}

		public void TestIsCancelledIsExported()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();

			var writer = GetNewShipmentDataObjectWriter(shipmentBO);
			var shipmentData = writer.GetDataObject(shipmentBO);
			Assert("IsCancelled in shipment should be false.", !shipmentData.IsCancelled.Value);

			shipmentBO.JS_IsCancelled = true;
			writer = GetNewShipmentDataObjectWriter(shipmentBO);
			shipmentData = writer.GetDataObject(shipmentBO);
			Assert("IsCancelled in shipment should be true.", shipmentData.IsCancelled.Value);
		}

		public void TestAdditionalReferenceNumbersAreExported()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var additionalReferenceBO = AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory.BOFactory);

			shipmentBO.Numbers.Add(additionalReferenceBO);

			var writer = GetNewShipmentDataObjectWriter(shipmentBO);
			var shipmentData = writer.GetDataObject(shipmentBO);
			AssertNotNull("Precondition: shipmentData", shipmentData);
			AssertNotNull("Precondition: shipmentData.AdditionalReferenceCollection", shipmentData.AdditionalReferenceCollection);

			CombineAssertions(delegate
			{
				AssertEquals("shipmentData.AdditionalReferenceCollection.Count", 1, shipmentData.AdditionalReferenceCollection.Count);

				var additionalReferenceDataObject = shipmentData.AdditionalReferenceCollection[0];
				AdditionalReferenceDataObjectWriterTest.AssertContents(additionalReferenceDataObject);
			});
		}

		public void TestWithPackLines()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = TransportModes.Sea; // Sea Freight
			shipmentBO.JS_PackingMode = ContainerModes.LCL;
			shipmentBO.JS_ShipmentType = ShipmentTypes.StandardHouse; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();

			var commodity1 = GetCommodity(Factory.BOFactory);

			var container1 = consolBO.Containers.AddNew();
			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipmentBO.PK;

			var package1 = AddPackage(packLine1, packageJob, "PLT", "PKG1", 13, Volume.CubicInches, 14, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);
			AddInnerPackage(package1, packageJob, "CTN", "INNERPKG1", 1, Volume.Litre, 2, Weight.Kilotonnes, Length.Inches, 3, 4, 5, 6);
			AddInnerPackage(package1, packageJob, "BOX", "INNERPKG2", 7, Volume.CubicMetres, 8, Weight.Grams, Length.Feet, 9, 10, 11, 12);

			AssertEquals("Precondition: packLine1.JL_OutturnedVolume", 37.539m, packLine1.JL_OutturnedVolume);
			AssertEquals("Precondition: packLine1.JL_ActualVolume", 87.943m, packLine1.JL_ActualVolume);

			AssertEquals("Precondition: container1.GoodsWeightForBinding", 5158.817m, container1.GoodsWeightForBinding);
			AssertEquals("Precondition: container1.WeightUnitForBinding", "LB", container1.WeightUnitForBinding);

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 1, shipmentData.PackingLineCollection.Count);
				CombineAssertions(() =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection[0];
					PackingLineDataObjectWriterTest.AssertPackLine(packingLineDataObject);
					AssertEquals("packingLineDataObject.ContainerNumber", "OOCL0000027", packingLineDataObject.ContainerNumber);
					AssertEquals("packingLineDataObject.Commodity.Code", "FGFG", packingLineDataObject.Commodity.Code);
					AssertEquals("packingLineDataObject.Commodity.Description", "Fudge Guts Fingers Gone", packingLineDataObject.Commodity.Description);
					AssertNull("packingLineDataObject.PackingLineCollection", packingLineDataObject.PackingLineCollection);
				});
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 1, shipmentData.PackingLineCollection.Count);
				CombineAssertions(() =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection[0];
					AssertPackingLine(packingLineDataObject, "PLT", "PKG1", 4481583766.326, Volume.CubicInches, 252, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);
					AssertEquals("packingLineDataObject.ContainerNumber", "OOCL0000027", packingLineDataObject.ContainerNumber);
					AssertNotNull("packingLineDataObject.PackingLineCollection", packingLineDataObject.PackingLineCollection);
					AssertEquals("packingLineDataObject.PackingLineCollection.Count", 2, packingLineDataObject.PackingLineCollection.Count);
					var innerPackingLineDataObject1 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => l.ReferenceNumber.GetValueOrDefault() == "INNERPKG1");
					AssertPackingLine(innerPackingLineDataObject1, "CTN", "INNERPKG1", 5.898, Volume.Litre, 12, Weight.Kilotonnes, Length.Inches, 3, 4, 5, 6);
					var innerPackingLineDataObject2 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => l.ReferenceNumber.GetValueOrDefault() == "INNERPKG2");
					AssertPackingLine(innerPackingLineDataObject2, "BOX", "INNERPKG2", 336.408, Volume.CubicMetres, 96, Weight.Grams, Length.Feet, 9, 10, 11, 12);
					AssertContainsExactElementsInAnyOrder("PackingLine.Link", new ZInt[] { 1, 2, 3 }, new[] { packingLineDataObject.Link, innerPackingLineDataObject1.Link, innerPackingLineDataObject2.Link });
				});
			}
		}

		public void TestWithPackLines_DuplicatePackage()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentBO.JS_TransportMode = TransportModes.Air; // Sea Freight
			shipmentBO.JS_PackingMode = ContainerModes.Loose;
			shipmentBO.JS_ShipmentType = ShipmentTypes.StandardHouse; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();

			var commodity1 = GetCommodity(Factory.BOFactory);

			var container1 = consolBO.Containers.AddNew();
			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			packLine1.SetContainer(container1.PK);

			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);
			packLine2.SetContainer(container1.PK);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipmentBO.PK;

			var package1 = AddPackage(packLine1, packageJob, "PLT", "PKG1", 13, Volume.CubicInches, 14, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);

			var jobPackLinePackage2 = Factory.New<JobPackLinePackage>();
			jobPackLinePackage2.JPP_KP_Packge = package1.PK;
			jobPackLinePackage2.JPP_JL_PackLine = packLine2.PK;

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNull("PackingLineCollection should not be populated", shipmentData.PackingLineCollection);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertContains("Cannot add duplicate package", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestWithPackLines_DepartureTransitWarehouseExcluded()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = TransportModes.Sea; // Sea Freight
			shipmentBO.JS_PackingMode = ContainerModes.LCL;
			shipmentBO.JS_ShipmentType = ShipmentTypes.StandardHouse; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();
			var commodity1 = GetCommodity(Factory.BOFactory);
			var container1 = consolBO.Containers.AddNew();
			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;
			packLine1.JL_DepartureTransitWarehouseExcluded = true;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipmentBO.PK;

			var package1 = AddPackage(packLine1, packageJob, "PLT", "PKG1", 13, Volume.CubicInches, 14, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);
			AddInnerPackage(package1, packageJob, "CTN", "INNERPKG1", 1, Volume.Litre, 2, Weight.Kilotonnes, Length.Inches, 3, 4, 5, 6);
			AddInnerPackage(package1, packageJob, "BOX", "INNERPKG2", 7, Volume.CubicMetres, 8, Weight.Grams, Length.Feet, 9, 10, 11, 12);

			AssertEquals("Precondition: packLine1.JL_OutturnedVolume", 37.539m, packLine1.JL_OutturnedVolume);
			AssertEquals("Precondition: packLine1.JL_ActualVolume", 87.943m, packLine1.JL_ActualVolume);
			AssertEquals("Precondition: container1.GoodsWeightForBinding", 5158.817m, container1.GoodsWeightForBinding);
			AssertEquals("Precondition: container1.WeightUnitForBinding", "LB", container1.WeightUnitForBinding);

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ATW, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 1, shipmentData.PackingLineCollection.Count);
				CombineAssertions(() =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection[0];
					PackingLineDataObjectWriterTest.AssertPackLine(packingLineDataObject);
					AssertEquals("packingLineDataObject.ContainerNumber", "OOCL0000027", packingLineDataObject.ContainerNumber);
					AssertEquals("packingLineDataObject.Commodity.Code", "FGFG", packingLineDataObject.Commodity.Code);
					AssertEquals("packingLineDataObject.Commodity.Description", "Fudge Guts Fingers Gone", packingLineDataObject.Commodity.Description);
					AssertNull("packingLineDataObject.PackingLineCollection", packingLineDataObject.PackingLineCollection);
				});

				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipmentBO)), true, false);
				shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 0, shipmentData.PackingLineCollection.Count);
			}

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ATW, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 1, shipmentData.PackingLineCollection.Count);
				CombineAssertions(() =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection[0];
					AssertPackingLine(packingLineDataObject, "PLT", "PKG1", 4481583766.326, Volume.CubicInches, 252, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);
				});

				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipmentBO)), true, false);
				shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 0, shipmentData.PackingLineCollection.Count);
			}
		}

		public void TestWithPackLines_WithPackagesAndInnerPackLines()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_TransportMode = TransportModes.Sea; // Sea Freight
			shipmentBO.JS_PackingMode = ContainerModes.LCL;
			shipmentBO.JS_ShipmentType = ShipmentTypes.StandardHouse; // Standard House

			var consolBO = shipmentBO.Consols.AddNew();
			var commodity1 = GetCommodity(Factory.BOFactory);
			var container1 = consolBO.Containers.AddNew();
			ContainerDataObjectWriterTest.PopulateContainer1(container1, commodity1, Factory.BOFactory);

			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			packLine1.SetContainer(container1.PK);
			packLine1.JL_RH_NKCommodityCode = commodity1.RH_Code;
			var packLine11 = packLine1.InnerPackLines.AddNew();
			packLine11.JL_F3_NKPackType = PkgUnit.Box;
			packLine11.JL_ActualVolume = 1m;
			packLine11.JL_ActualVolumeUQ = Volume.CubicInches;
			packLine11.JL_ActualWeight = 2m;
			packLine11.JL_ActualWeightUQ = Weight.Hectograms;
			packLine11.JL_UnitOfDimension = Length.Metres;
			packLine11.JL_Length = 3m;
			packLine11.JL_Width = 4m;
			packLine11.JL_Height = 5m;
			packLine11.JL_PackageCount = 6;
			var packLine12 = packLine1.InnerPackLines.AddNew();
			packLine12.JL_F3_NKPackType = PkgUnit.Bag;
			packLine12.JL_ActualVolume = 7m;
			packLine12.JL_ActualVolumeUQ = Volume.CubicInches;
			packLine12.JL_ActualWeight = 8m;
			packLine12.JL_ActualWeightUQ = Weight.Hectograms;
			packLine12.JL_UnitOfDimension = Length.Metres;
			packLine12.JL_Length = 9m;
			packLine12.JL_Width = 10m;
			packLine12.JL_Height = 11m;
			packLine12.JL_PackageCount = 14;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = shipmentBO.PK;

			var package1 = AddPackage(packLine1, packageJob, PkgUnit.Box, "PKG1", 1, Volume.CubicInches, 2, Weight.Hectograms, Length.Metres, 3, 4, 5, 6);
			var package2 = AddPackage(packLine1, packageJob, PkgUnit.Bag, "PKG2", 7, Volume.CubicInches, 8, Weight.Hectograms, Length.Metres, 9, 10, 11, 12);
			var package3 = AddPackage(packLine1, packageJob, PkgUnit.Pallet, "PKG3", 13, Volume.CubicInches, 14, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);
			AssertEquals("Precondition: packLine1.JL_OutturnedVolume", 37.539m, packLine1.JL_OutturnedVolume);
			AssertEquals("Precondition: packLine1.JL_ActualVolume", 87.943m, packLine1.JL_ActualVolume);
			AssertEquals("Precondition: container1.GoodsWeightForBinding", 5158.817m, container1.GoodsWeightForBinding);
			AssertEquals("Precondition: container1.WeightUnitForBinding", "LB", container1.WeightUnitForBinding);

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull("Precondition: shipmentData", shipmentData);
				AssertNotNull("shipmentData.PackingLineCollection", shipmentData.PackingLineCollection);
				AssertEquals("shipmentData.PackingLineCollection.Count", 3, shipmentData.PackingLineCollection.Count);
				CombineAssertions("PackingLine PKG1", () =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection.FirstOrDefault(l => l.ReferenceNumber.GetValueOrDefault() == "PKG1");
					AssertPackingLine(packingLineDataObject, PkgUnit.Box, "PKG1", 21968547.876, Volume.CubicInches, 12, Weight.Hectograms, Length.Metres, 3, 4, 5, 6);
					AssertNotNull("packingLineDataObject.PackingLineCollection", packingLineDataObject.PackingLineCollection);
					AssertEquals("packingLineDataObject.PackingLineCollection.Count", 2, packingLineDataObject.PackingLineCollection.Count);
					var innerPackingLineDataObject1 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => (l.PackType?.Code).GetValueOrDefault() == PkgUnit.Box);
					AssertPackingLine(innerPackingLineDataObject1, PkgUnit.Box, ZString.Empty, 21968547.874, Volume.CubicInches, 2, Weight.Hectograms, Length.Metres, 3, 4, 5, 2);
					var innerPackingLineDataObject2 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => (l.PackType?.Code).GetValueOrDefault() == PkgUnit.Bag);
					AssertPackingLine(innerPackingLineDataObject2, PkgUnit.Bag, ZString.Empty, 845789093.153, Volume.CubicInches, 8, Weight.Hectograms, Length.Metres, 9, 10, 11, 4);
				});
				CombineAssertions("PackingLine PKG2", () =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection.FirstOrDefault(l => l.ReferenceNumber.GetValueOrDefault() == "PKG2");
					AssertPackingLine(packingLineDataObject, PkgUnit.Bag, "PKG2", 724962079.848, Volume.CubicInches, 96, Weight.Hectograms, Length.Metres, 9, 10, 11, 12);
					AssertNotNull("packingLineDataObject.PackingLineCollection", packingLineDataObject.PackingLineCollection);
					AssertEquals("packingLineDataObject.PackingLineCollection.Count", 2, packingLineDataObject.PackingLineCollection.Count);
					var innerPackingLineDataObject1 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => (l.PackType?.Code).GetValueOrDefault() == PkgUnit.Box);
					AssertPackingLine(innerPackingLineDataObject1, PkgUnit.Box, ZString.Empty, 21968547.874, Volume.CubicInches, 2, Weight.Hectograms, Length.Metres, 3, 4, 5, 2);
					var innerPackingLineDataObject2 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => (l.PackType?.Code).GetValueOrDefault() == PkgUnit.Bag);
					AssertPackingLine(innerPackingLineDataObject2, PkgUnit.Bag, ZString.Empty, 845789093.153, Volume.CubicInches, 8, Weight.Hectograms, Length.Metres, 9, 10, 11, 4);
				});
				CombineAssertions("PackingLine PKG3", () =>
				{
					var packingLineDataObject = shipmentData.PackingLineCollection.FirstOrDefault(l => l.ReferenceNumber.GetValueOrDefault() == "PKG3");
					AssertPackingLine(packingLineDataObject, PkgUnit.Pallet, "PKG3", 4481583766.326, Volume.CubicInches, 252, Weight.Hectograms, Length.Metres, 15, 16, 17, 18);
					AssertNotNull("packingLineDataObject.PackingLineCollection", packingLineDataObject.PackingLineCollection);
					AssertEquals("packingLineDataObject.PackingLineCollection.Count", 2, packingLineDataObject.PackingLineCollection.Count);
					var innerPackingLineDataObject1 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => (l.PackType?.Code).GetValueOrDefault() == PkgUnit.Box);
					AssertPackingLine(innerPackingLineDataObject1, PkgUnit.Box, ZString.Empty, 21968547.874, Volume.CubicInches, 2, Weight.Hectograms, Length.Metres, 3, 4, 5, 2);
					var innerPackingLineDataObject2 = packingLineDataObject.PackingLineCollection.FirstOrDefault(l => (l.PackType?.Code).GetValueOrDefault() == PkgUnit.Bag);
					AssertPackingLine(innerPackingLineDataObject2, PkgUnit.Bag, ZString.Empty, 845789093.153, Volume.CubicInches, 8, Weight.Hectograms, Length.Metres, 9, 10, 11, 6);
				});
			}
		}

		public void TestBusinessContext()
		{
			var shipmentBO = Factory.New<ForwardingShipment>();

			shipmentBO.JS_TransportMode = "SEA";
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD";

			var shipmentData = GetNewShipmentDataObjectWriter(shipmentBO).GetDataObject(shipmentBO);

			AssertEquals(true, shipmentBO.Factory.HasContext(BusinessContext.NonAccountingCode));
		}

		#region Implementation

		protected abstract BaseShipmentDataObjectWriter GetNewShipmentDataObjectWriter(BusinessObject topLevelBO);

		internal static RefCommodityCode GetCommodity(BusinessObjectFactory factory, string code = "FGFG", string description = "Fudge Guts Fingers Gone")
		{
			var commodity1 = factory.New<RefCommodityCode>();
			commodity1.RH_Code = code;
			commodity1.RH_Description = description;
			commodity1.RH_IsShipping = true;
			commodity1.RH_IsForwarding = true;
			commodity1.RH_IsLandTransport = false;
			return commodity1;
		}

		protected virtual string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}

		static void AssertPackingLine(
			PackingLine packingLine, ZString packType, ZString referenceNumber, ZDecimal volume, ZString volumeUnit, ZDecimal weight,
			ZString weightUnit, ZString lengthUnit, ZDecimal length, ZDecimal width, ZDecimal height, ZLong packQty)
		{
			AssertNotNull($"referenceNumber: {referenceNumber}", packingLine);
			AssertEquals("packingLine.PackType", packType, packingLine.PackType?.Code);
			AssertEquals("packingLine.ReferenceNumber", referenceNumber, packingLine.ReferenceNumber);
			AssertEquals("packingLine.Volume", volume, packingLine.Volume);
			AssertEquals("packingLine.VolumeUnit", volumeUnit, packingLine.VolumeUnit?.Code);
			AssertEquals("packingLine.Weight", weight, packingLine.Weight);
			AssertEquals("packingLine.WeightUnit", weightUnit, packingLine.WeightUnit?.Code);
			AssertEquals("packingLine.LengthUnit", lengthUnit, packingLine.LengthUnit?.Code);
			AssertEquals("packingLine.Length", length, packingLine.Length);
			AssertEquals("packingLine.Width", width, packingLine.Width);
			AssertEquals("packingLine.Height", height, packingLine.Height);
			AssertEquals("packingLine.PackQty", packQty, packingLine.PackQty);
		}

		static void PopulatePackage(
			PkgPackage package, ZString packType, ZString packageID, ZDecimal volume, ZString volumeUQ, ZDecimal weight,
			ZString weightUQ, ZString dimensionUQ, ZDecimal length, ZDecimal width, ZDecimal height, ZInt packageQty)
		{
			package.KP_F3_NKPackType = packType;
			package.KP_PackageID = packageID;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_DimensionUQ = dimensionUQ;
			package.KP_Length = length;
			package.KP_Width = width;
			package.KP_Height = height;
			package.KP_PackageQty = packageQty;
		}

		static PkgPackage AddPackage(
			ForwardingPackLine packLine, PkgPackageJob packageJob, ZString packType, ZString packageID, ZDecimal volume, ZString volumeUQ,
			ZDecimal weight, ZString weightUQ, ZString dimensionUQ, ZDecimal length, ZDecimal width, ZDecimal height, ZInt packageQty)
		{
			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			PopulatePackage(package, packType, packageID, volume, volumeUQ, weight, weightUQ, dimensionUQ, length, width, height, packageQty);
			return package;
		}

		PkgPackage AddInnerPackage(
			PkgPackage package, PkgPackageJob packageJob, ZString packType, ZString packageID, ZDecimal volume, ZString volumeUQ,
			ZDecimal weight, ZString weightUQ, ZString dimensionUQ, ZDecimal length, ZDecimal width, ZDecimal height, ZInt packageQty)
		{
			var innerPackage = Factory.New<PkgPackage>();
			innerPackage.KP_KP_TopHandlingUnitPackage = package.PK;
			innerPackage.KP_KJ_ParentPackageJob = packageJob.PK;
			PopulatePackage(innerPackage, packType, packageID, volume, volumeUQ, weight, weightUQ, dimensionUQ, length, width, height, packageQty);

			var divot = package.PackageHandlingUnitHandlingUnitDivots.FirstOrDefault(d => d.KPD_KP_Package == innerPackage.PK);
			if (divot == null)
			{
				divot = package.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = innerPackage.PK;
			}
			divot.KPD_PackedTime = ZDateTimeOffset.Now;
			divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			return innerPackage;
		}

		#endregion
	}
}
