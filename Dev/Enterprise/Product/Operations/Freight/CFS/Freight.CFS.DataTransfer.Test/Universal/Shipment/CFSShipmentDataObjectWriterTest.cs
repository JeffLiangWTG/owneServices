using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.CFS.DataTransfer.Universal.Testing
{
	sealed class CFSShipmentDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriting()
		{
			var shipment = Factory.New<CFSShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "HBL12345";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_ConsolReference = "CONREF";
			shipment.JS_InterimReceipt = "RCPT9999";
			shipment.JS_A_BKD = new ZDateTime(2015, 1, 5);
			shipment.JS_CartageWaybill = "BILLY";
			shipment.JS_GoodsDescription = "POKEMON";
			shipment.CustomsEntryNumber = "CUS456";
			shipment.JS_WarehouseLocation = "SYD";
			shipment.JS_OuterPacks = 5;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Package;
			shipment.JS_ActualVolume = 6.5;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 7.5;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment.JS_TranshipToOtherCFS = true;

			shipment.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2015, 1, 10);
			shipment.DocsAndCartage.JP_LCLStorageCommences = new ZDateTime(2015, 1, 15);
			shipment.DocsAndCartage.Services.AddNew().ES_ServiceCode = "ABC";
			shipment.DocsAndCartage.Services.AddNew().ES_ServiceCode = "XYZ";

			var fwdorg = CreateOrg("FWDORG");
			var contactFWD = fwdorg.Contacts.AddNew();
			contactFWD.OC_ContactName = "Angry Bill";
			shipment.JS_OH_HandledOnBehalfOfForwarder = fwdorg.PK;

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = CreateOrg("CNRORG").MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = CreateOrg("CNEORG").MainAddress.PK;
			shipment.JS_OA_CartageCoAddr = CreateOrg("CTGORG").MainAddress.PK;

			var cnrpicdlvorg = CreateOrg("CNRPICDLVORG");
			var contactCNR = cnrpicdlvorg.Contacts.AddNew();
			contactCNR.OC_ContactName = "Rakis Kotakis";
			shipment.ConsignorPickupAddress.E2_OA_Address = cnrpicdlvorg.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment.ConsignorPickupAddress.E2_Contact = "Andreus Papandreus";
			shipment.ConsignorPickupAddress.E2_CompanyName = "FC Saloniki";

			var cnepicdlvorg = CreateOrg("CNEPICDLVORG");
			cnepicdlvorg.OH_FullName = "Panathinaikos";
			var contactCNE = cnepicdlvorg.Contacts.AddNew();
			contactCNE.OC_ContactName = "Simonis Katakonics";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = cnepicdlvorg.MainAddress.PK;

			shipment.OuterPackLines[0].JL_MarksAndNumbers = "MARKSANDNO1";
			shipment.OuterPackLines.AddNew().JL_MarksAndNumbers = "MARKSANDNO2";

			var masterShipment = Factory.New<CFSShipment>();
			masterShipment.JS_GoodsDescription = "RABBLE RABBLE";
			var masterConsol = masterShipment.Consols.AddNew();
			masterConsol.JK_BookingReference = "CHICKEN SANDWICH";
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Shipment dataObject;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				dataObject =
					new CFSShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true,
						true, new ContainerLinkManager<CFSLoadListConsol>(null)).GetDataObject(shipment);
			}

			CombineAssertions(() =>
			{
				AssertEquals(Constants.ShipmentTypes.StandardHouse, dataObject.ShipmentType.Code);
				AssertEquals(Constants.TransportModes.Sea, dataObject.TransportMode.Code);
				AssertEquals("HBL12345", dataObject.WayBillNumber);
				AssertEquals("AUMEL", dataObject.PortOfOrigin.Code);
				AssertEquals("USCHI", dataObject.PortOfDestination.Code);
				AssertEquals("CONREF", dataObject.AgentsReference);
				AssertEquals("RCPT9999", dataObject.InterimReceiptNumber);
				AssertEquals(new ZDateTime(2015, 1, 5), dataObject.DateCollection.FirstOrDefault(DateType.BookingConfirmed, false).Value);
				AssertEquals("BILLY", dataObject.CartageWaybillNumber);
				AssertEquals("POKEMON", dataObject.GoodsDescription);
				AssertEquals(1, dataObject.EntryNumberCollection.Count);
				AssertEquals("CUS456", dataObject.EntryNumberCollection[0].Number);
				AssertEquals(Factory.Load<RefServiceLevel>(Env.Registry.ServiceLevel).RS_Code, dataObject.ServiceLevel.Code);
				AssertEquals("SYD", dataObject.WarehouseLocation);
				AssertEquals(5, dataObject.OuterPacks);
				AssertEquals(Constants.PkgUnit.Package, dataObject.OuterPacksPackageType.Code);
				AssertEquals(new ZDecimal(6.5), dataObject.TotalVolume);
				AssertEquals(Constants.Volume.CubicMetres, dataObject.TotalVolumeUnit.Code);
				AssertEquals(new ZDecimal(7.5), dataObject.TotalWeight);
				AssertEquals(Constants.Weight.Kilograms, dataObject.TotalWeightUnit.Code);
				AssertEquals(true, dataObject.TranshipToOtherCFS);

				AssertEquals(new ZDateTime(2015, 1, 10), dataObject.LocalProcessing.LCLAvailable);
				AssertEquals(new ZDateTime(2015, 1, 15), dataObject.LocalProcessing.LCLStorageCommences);
				AssertContainsExactElementsInAnyOrder(new[] { "ABC", "XYZ" }, dataObject.LocalProcessing.AdditionalServiceCollection.Select(x => x.ServiceCode.Code.ToString()));

				AssertOrg(dataObject, AddressTypes.Forwarder, "FWDORG", string.Empty, null);
				AssertOrg(dataObject, nameof(DocAddressType.ConsignorDocumentaryAddress), "CNRORG", string.Empty, null);
				AssertOrg(dataObject, nameof(DocAddressType.ConsigneeDocumentaryAddress), "CNEORG", string.Empty, null);
				AssertOrg(dataObject, nameof(DocAddressType.LocalCartageAddress1), "CTGORG", string.Empty, null);
				AssertOrg(dataObject, nameof(DocAddressType.ConsignorPickupDeliveryAddress), null, "FC Saloniki", "Andreus Papandreus");
				AssertOrg(dataObject, nameof(DocAddressType.ConsigneePickupDeliveryAddress), "CNEPICDLVORG", "Panathinaikos", null);
				AssertContainsExactElementsInAnyOrder(new[] { "MARKSANDNO1", "MARKSANDNO2" }, dataObject.PackingLineCollection.Select(x => x.MarksAndNos.ToString()));

				AssertEquals(1, dataObject.ParentShipmentCollection.Count);
				AssertEquals("RABBLE RABBLE", dataObject.ParentShipmentCollection[0].GoodsDescription);
				AssertEquals(1, dataObject.ParentShipmentCollection[0].ParentShipmentCollection.Count);
				AssertEquals("CHICKEN SANDWICH", dataObject.ParentShipmentCollection[0].ParentShipmentCollection[0].BookingConfirmationReference);
			});
		}

		public void TestCFSUniversalShipment_v1_TopLevelDataObject()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.Consols.AddNew();

			Shipment shipmentData;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				shipmentData =
					new CFSShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment)), true,
						true, new ContainerLinkManager<CFSLoadListConsol>(null))
						.GetDataObject(shipment);
			}

			AssertEquals("CFSLoadListConsol", shipmentData.DataContext.DataSourceCollection.First().Type);
		}

		#region Implementation

		OrgHeader CreateOrg(ZString code)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;

			return org;
		}

		void AssertOrg(Shipment dataObject, string addressType, ZString? expectedCode, ZString? expectedName, ZString? expectedContactName)
		{
			var addressDO = dataObject.OrganizationAddressCollection.FirstOrDefault(addressType);
			AssertNotNull(addressDO);
			AssertEquals(string.Concat(addressType, ".OrganizationCode"), expectedCode, addressDO.OrganizationCode);
			AssertEquals(string.Concat(addressType, ".CompanyName"), expectedName, addressDO.CompanyName);
			AssertEquals(string.Concat(addressType, ".Contact"), expectedContactName, addressDO.Contact);
		}

		#endregion
	}
}
