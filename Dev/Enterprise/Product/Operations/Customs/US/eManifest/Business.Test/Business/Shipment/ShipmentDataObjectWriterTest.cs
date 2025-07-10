using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using static Enterprise.Customs.US.DataTransfer.Universal.Constants;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalXmlCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var tripBO = Factory.New<Trip>();
				var shipmentBO = tripBO.Shipments.AddNew();
				var writer = ShipmentDataObjectWriter.New(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipmentBO),
					writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(UniversalXmlCustoms.CommercialInvoiceHeader.CommercialInvoiceLineCollection))), shipmentBO, tripBO);
				var shipment = writer.GetDataObject(shipmentBO);
				var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
				AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

				writer = ShipmentDataObjectWriter.New(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipmentBO)), shipmentBO, tripBO);
				shipment = writer.GetDataObject(shipmentBO);
				commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
				AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
			}
		}

		public void TestPopulateDataObject()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MFG";
			var shipTo = Factory.NewWithValidTestData<OrgHeader>();
			shipTo.OH_Code = "SST";
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_RN_NKCountryCode = "US";
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var tripBO = Factory.New<Trip>();
				tripBO.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
				tripBO.BH_GB = GlbBranch.CurrentBranch.PK;
				tripBO.BH_GB = branch.PK;
				tripBO.BH_VoyageNumber = "Voyage1";
				tripBO.BH_PortUnladingDCode = "2704";
				tripBO.BH_RL_NKPortUnlading = "USBUF";
				tripBO.BH_ETA = ZDateTime.BrettsBirthday;
				tripBO.BH_CarrierSCAC = "OKSC";
				var importer = Factory.NewWithValidTestData<OrgAddress>();
				importer.Header.OH_Code = "IMPORTER1";
				tripBO.BH_OA_Importer = importer.PK;
				tripBO.BH_TransitDirection = "I";
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "CARRIER1";
				var scacCode = carrier.CustomsCodes.AddNew();
				scacCode.OK_CodeType = "CCC";
				scacCode.OK_CustomsRegNo = "OKSC";
				tripBO.BH_OH_Carrier = carrier.PK;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = "Big Boss";
				contact.OC_Phone = "+3 (126) 4846516";
				var shipment1 = tripBO.Shipments.AddNew();
				shipment1.B0_MasterBillNumber = "MasterB";
				shipment1.B0_Firms = "FIRM";
				shipment1.B0_ReferenceID = "Shipment1";
				shipment1.B0_ManifestQty = 3;
				shipment1.B0_ManifestUQ = "PK";
				shipment1.B0_Weight = 30;
				shipment1.B0_WeightUQ = "KG";
				shipment1.B0_DescriptionOfCargo = "DESC";
				var ship1Commodity = shipment1.Commodities.AddNew();
				ship1Commodity.BY_InvoiceQuantity = 22m;
				ship1Commodity.BY_ManifestUnitCode = "BKT";
				ship1Commodity.BY_PieceCount = 11;
				ship1Commodity.BY_GrossWeight = 12m;
				ship1Commodity.BY_GrossWeightUnit = "KG";
				ship1Commodity.BY_Description = "ship1comm1";
				var tariff = ship1Commodity.HarmonizedNumbers.AddNew();
				tariff.CY_TariffFormatted = "3920.99.1000";
				ship1Commodity.BY_HazardousGoodsIdentifier = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
				var undg = ship1Commodity.UNDGs.AddNew();
				undg.DI_DG = ship1Commodity.BY_HazardousGoodsIdentifier;
				var manufacturerParty = shipment1.Parties.AddNew();
				manufacturerParty.E2_AddressType = PartyTypes.Codes.ManufacturerOfGoods;
				manufacturerParty.OrganisationPK = manufacturer.PK;
				var shipToParty = shipment1.Parties.AddNew();
				shipToParty.E2_AddressType = PartyTypes.Codes.ShipTo;
				shipToParty.OrganisationPK = shipTo.PK;
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var writer = ShipmentDataObjectWriter.New(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, shipment1)), shipment1, tripBO);
				var dataObject = writer.GetDataObject(shipment1);
				dataObject.DataContext = dataContext;
				CombineAssertions(() =>
				{
					AssertEquals("Shipment type", Core.Constants.FreightShipmentDirection.Code.Import, dataObject.MessageType.Code);
					AssertEquals("Transport", Core.Constants.TransportModes.Truck, dataObject.TransportMode.Code);
					AssertEquals("TripVoyageNumber", tripBO.BH_VoyageNumber, dataObject.WayBillNumber);
					AssertEquals("VoyageFightNumber", tripBO.BH_VoyageNumber, dataObject.AddInfoCollection.Find(x => x.Key.Value == "MasterWayBillNumber").Value);
					AssertEquals("NCT", ContainerModeList.Codes.NonContainerized, dataObject.ContainerMode.Code);
					AssertEquals("Non-Containerized", ContainerModeList.Descriptions.NonContainerized, dataObject.ContainerMode.Description);
					AssertNotNull(dataObject.AdditionalBillCollection.Find(x => x.BillNumber?.ToString() == tripBO.BH_VoyageNumber));
					var importerOrg = dataObject.OrganizationAddressCollection.Find(x => x.OrganizationCode.ToString() == tripBO.Importer.Header.OH_Code);
					AssertEquals("Client -> Importer", nameof(DocAddressType.ConsigneeDocumentaryAddress), importerOrg.AddressType);
					var shippingParty = dataObject.OrganizationAddressCollection.Find(x => x.OrganizationCode.ToString() == tripBO.Carrier.OH_Code);
					AssertEquals("Carrier -> Transport Carrier Code", AddressTypes.ShippingLine, shippingParty.AddressType);
					AssertEquals("Carrier Code-> Carrier SCAC", tripBO.BH_CarrierSCAC, dataObject.AddInfoCollection.Find(x => x.Key.Value == Constants.AddInfoKeys.Declaration.NKCarrierSCAC).Value);
					AssertEquals("Carrier Code-> Issuer SCAC", tripBO.BH_CarrierSCAC, dataObject.AddInfoCollection.Find(x => x.Key.Value == Constants.AddInfoKeys.Declaration.MasterWayBillIssuerSCAC).Value);
					var dischargeProt = dataObject.AddInfoCollection.Find(x => x.Key.Value == Constants.AddInfoKeys.Declaration.SchDEntry);
					AssertEquals("Discharge Port Code", tripBO.BH_PortUnladingDCode, dischargeProt.Value);
					var arrivalPort = dataObject.AddInfoCollection.Find(x => x.Key.Value == Constants.AddInfoKeys.Declaration.SchDArrival);
					AssertEquals("Arrival Port", tripBO.BH_PortUnladingDCode, arrivalPort.Value);
					var entryDate = dataObject.AddInfoCollection.Find(x => x.Key.Value == Constants.AddInfoKeys.Declaration.EntryDate);
					AssertEquals("Estimated Date of Arrival - Entry Date", tripBO.BH_ETA.SqlFormat, entryDate.Value);
					var departureDate = dataObject.DateCollection.Find(x => x.Type.ToString() == nameof(UniversalXml.DateType.Departure));
					AssertEquals("Departure Date", tripBO.BH_ETA, departureDate.Value);
					var dischargeDate = dataObject.DateCollection.Find(x => x.Type.ToString() == nameof(UniversalXml.DateType.DischargeDate));
					AssertEquals("Discharge Date", tripBO.BH_ETA, dischargeDate.Value);
					AssertEquals("Shipment Quantity UQ - Totoal Number of Package and UQ", shipment1.B0_ManifestQty, dataObject.OuterPacks.Value);
					AssertEquals("Shipment Quantity UQ - Package UQ", shipment1.B0_ManifestUQ, dataObject.OuterPacksPackageType.Code);
					AssertEquals("Shipment Weight UQ - Total Weight and UQ", shipment1.B0_Weight, dataObject.TotalWeight.Value);
					AssertEquals("Shipment Weight UQ - UQ", shipment1.B0_WeightUQ, dataObject.TotalWeightUnit.Code);
					var commInvoiceLineColl = dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[1];
					AssertCommercialInvoiceLine(commInvoiceLineColl, tariff.CY_TariffFormatted, ship1Commodity);
					var address = dataObject.CommercialInfo.CommercialInvoiceCollection[0].OrganizationAddressCollection;
					AssertNotNull(address.Any(x => x.AddressType.ToString() == nameof(DocAddressType.Manufacturer)));
					AssertNotNull(address.Any(x => x.AddressType.ToString() == AddressType.ShipToParty));
				});
			}
		}

		void AssertCommercialInvoiceLine(UniversalXmlCustoms.CommercialInvoiceLine commInvoiceLineColl, ZString tariff, Commodity ship1Commodity)
		{
			AssertEquals("One Tariff on InvoiceLine", tariff, commInvoiceLineColl.HarmonisedCode);
			var sub = Factory.Load<UNDGSubstance>(ship1Commodity.BY_HazardousGoodsIdentifier);
			AssertEquals("Hazardous Goods Code", sub?.DG_Code, commInvoiceLineColl.HazardousMaterial.Code);
			var undg = commInvoiceLineColl.HazardousMaterial.UNDGCollection;
			AssertEquals("Hazardous Goods Code", ship1Commodity.UNDGs[0].UNDGSubstance.DG_Code, undg[0].UNDGCode);
			AssertEquals("Packages1", ship1Commodity.BY_PieceCount, (int)commInvoiceLineColl.InvoiceQuantity);
			AssertEquals("Gross weight1", ship1Commodity.BY_GrossWeight, commInvoiceLineColl.Weight);
			AssertEquals("Gross weightUnit", ship1Commodity.BY_GrossWeightUnit, commInvoiceLineColl.WeightUnit.Code);
		}
	}
}
