using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.eTail.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;

namespace Enterprise.eTail.DataTransfer.Testing
{
	abstract class BaseH7DeclarationConverterTest<TConverter> : BaseAsycudaManifestConverterTest<TConverter>
		where TConverter : BaseH7DeclarationConverter
	{
		public void TestHVLVConsignmentEORIMappingToAsycudaBillRegNo_EndToEnd()
		{
			var shipment = SetupTestShipment(Core.Constants.TransportModes.Air);
			var consignment1 = shipment.HVLVConsignments.FirstOrDefault(x => x.HVC_WaybillNumber == "HVC00001") as HVLVConsignment;

			var reference1 = consignment1.CustomsReferenceNumbers.AddNew();
			reference1.CE_EntryType = CustomsAdditionalReferenceNumbersCodes.ExporterEORINumber;
			reference1.CE_EntryNum = "1234";

			var reference2 = consignment1.CustomsReferenceNumbers.AddNew();
			reference2.CE_EntryType = CustomsAdditionalReferenceNumbersCodes.ImporterEORINumber;
			reference2.CE_EntryNum = "9876";

			Factory.Save();

			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				var trackLogs = new List<(string Caption, string Progress, int Percentage)>();
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment, trackLogs);

				var success = converter.TryConvert(out var errorMsg);
				Assert("Precondition: Convert successfully", success);

				var converterFactory = converter.CustomsRelatedBusinessCollection.Single().Factory;

				var billType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaBillSchema.Constants.Prefix);
				var bills = converterFactory.Load(billType, new ZQuery());

				var billWithEORINumbers = bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == "HVC00001");
				var billWithOutEORINumbers = bills.Cast<AsycudaBill>().FirstOrDefault(x => x.ABL_BillNumber == "HVC00002");
				AssertNotNull(billWithEORINumbers);

				CombineAssertions(() =>
				{
					AssertEquals("bill.ABL_ShipperRegNo", "1234", billWithEORINumbers.ABL_ShipperRegNo);
					AssertEquals("bill.ABL_ShipperRegNoType", "EOR", billWithEORINumbers.ABL_ShipperRegNoType);
					AssertEquals("bill.ABL_ConsigneeRegNo", "9876", billWithEORINumbers.ABL_ConsigneeRegNo);
					AssertEquals("bill.ABL_ConsigneeRegNoType", "EOR", billWithEORINumbers.ABL_ConsigneeRegNoType);

					AssertEquals("bill.ABL_ShipperRegNo", string.Empty, billWithOutEORINumbers.ABL_ShipperRegNo);
					AssertEquals("bill.ABL_ShipperRegNoType", string.Empty, billWithOutEORINumbers.ABL_ShipperRegNoType);
					AssertEquals("bill.ABL_ConsigneeRegNo", string.Empty, billWithOutEORINumbers.ABL_ConsigneeRegNo);
					AssertEquals("bill.ABL_ConsigneeRegNoType", string.Empty, billWithOutEORINumbers.ABL_ConsigneeRegNoType);
				});
			}
		}

		public void TestHVLVItemLineMappingToAsycudaPackAndAsycudaPackedItem_EndToEnd()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			var shipment = SetupTestShipment(Core.Constants.TransportModes.Air);
			var consignment1 = shipment.HVLVConsignments.First();
			consignment1.HVC_RX_NKGoodsValueCurrency = "CAD";
			consignment1.HVC_HVH_BookingHeader = bookingHeader.PK;
			var item1 = (HVLVItem)consignment1.Items[0];
			item1.HVI_GoodsDescription = "Item1";
			var itemLine1 = item1.Lines.AddNew();

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "PROD001";

			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation1.OU_OH = ((HVLVConsignment)consignment1).BookingHeader.BillToParty.Header.PK;

			itemLine1.HVS_ProductCode = "PROD001";
			AssertEquals("Precondition: Product Code", "PROD001", itemLine1.HVS_ProductCode);

			itemLine1.HVS_Quantity = 5;
			itemLine1.HVS_GoodsDescription = "Item1 Line1";
			itemLine1.HVS_GrossWeight = 2;
			itemLine1.HVS_WeightUnit = "OZ";
			itemLine1.HVS_CustomsValue = 10;
			itemLine1.HVS_DestinationTariff = "1234.56.78";
			itemLine1.HVS_RN_NKOriginCountryCode = "CA";

			var itemLine2 = item1.Lines.AddNew();
			itemLine2.HVS_GoodsDescription = "Item1 Line2";
			itemLine2.HVS_Quantity = 1;

			var itemLine3 = ((HVLVItem)consignment1.Items.AddNew()).Lines.AddNew();
			itemLine3.HVS_GoodsDescription = "Item2 Line1";
			itemLine3.HVS_Quantity = 1;

			Factory.Save();

			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				var success = converter.TryConvert(out var errorMsg);
				Assert("Precondition: Convert successfully", success);

				var converterFactory = converter.CustomsRelatedBusinessCollection.Single().Factory;

				var billType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaBillSchema.Constants.Prefix);
				var bills = converterFactory.Load(billType, new ZQuery());
				AssertEquals("2 bills should be created", 2, bills.Cast<AsycudaBill>().Count(bill => !bill.IsChildMasterBill));

				var packType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaPackSchema.Constants.Prefix);
				var packs = converterFactory.Load(packType, new ZQuery());
				AssertEquals("3 packs should be created", 3, packs.Length);

				var packedItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(AsycudaPackedItemSchema.Constants.Prefix);
				var packedItems = converterFactory.Load(packedItemType, new ZQuery());
				AssertEquals("3 packedItems should be created", 3, packedItems.Length);

				var packWithDetails = packs.Cast<AsycudaPack>().FirstOrDefault(x => x.APA_GoodsDescription == "Item1");
				AssertNotNull(packWithDetails);

				CombineAssertions(() =>
				{
					AssertEquals("packBO.APA_PackQty", 1, packWithDetails.APA_PackQty);
					AssertEquals("packBO.APA_PackUQ", ZString.Empty, packWithDetails.APA_PackUQ);
					AssertEquals("packBO.APA_GoodsDescription", "Item1", packWithDetails.APA_GoodsDescription);
					AssertEquals("packBO.APA_Weight", (ZDecimal)0.057, packWithDetails.APA_Weight);
					AssertEquals("packBO.APA_WeightUQ", "KG", packWithDetails.APA_WeightUQ);
					AssertEquals("packBO.LinePrice", (ZDecimal)10.00, packWithDetails.LinePrice);
					AssertEquals("packBO.LinePriceCurrency", "CAD", packWithDetails.LinePriceCurrency);
					AssertEquals("packBO.APA_LineNo", (ZShort)1, packWithDetails.APA_LineNo);

					var packedItem = packWithDetails.PackedItem;
					AssertNull(packedItem);
				});

				var billWithDetails = bills.Cast<AsycudaBill>().FirstOrDefault(bill => bill.ABL_BillNumber == "HVC00001");
				AssertNotNull(billWithDetails);

				CombineAssertions(() =>
				{
					AssertEquals("billBo.PackedItem", 3, billWithDetails.PackedItems.Count);

					var packedItem1 = billWithDetails.PackedItems.FirstOrDefault(item => item.API_GoodsDescription == "Item1 Line1");
					AssertNotNull(packedItem1);
					AssertEquals("packedItemBO.API_RN_NKGoodsOrigin", "CA", packedItem1.API_RN_NKGoodsOrigin);

					var packedItem2 = billWithDetails.PackedItems.FirstOrDefault(item => item.API_GoodsDescription == "Item1 Line2");
					AssertNotNull(packedItem2);

					var packedItem3 = billWithDetails.PackedItems.FirstOrDefault(item => item.API_GoodsDescription == "Item2 Line1");
					AssertNotNull(packedItem3);
				});
			}
		}

		protected override string[] SupportedTransportModes => new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea };

		protected override RecipientRoleType GetExpectedPickupOrDeliveryRole(Directions shipmentDirection) => RecipientRoleType.DCA;

		protected override bool ShouldEntryHeaderContainDestinationCountryInsteadOfCurrentLoginCountry => true;

		protected override bool ShouldExportAdditionalReferenceCollection => true;

		protected override string GetBillHumanReadableName(string billNumber) => $"Manifest Bill {billNumber}";
	}
}
