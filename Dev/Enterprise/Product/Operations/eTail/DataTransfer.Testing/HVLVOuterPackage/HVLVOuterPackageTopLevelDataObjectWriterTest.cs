using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVOuterPackageTopLevelDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestExportHVLVOuterPackage()
		{
			var manager = OuterPackageBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, OuterPackageBO)));
			var universalShipment = writer.GetDataObject(OuterPackageBO);

			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(universalShipment, stream);

					using (var reader = new StreamReader(stream))
					{
						var result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Expected Universal Shipment Message", HVLVOuterPackageDataContextManagerTest.GetValidPopulatedUniversalShipmentXML(), result);
					}
				}
			}
		}

		public void TestWriteToDataObject()
		{
			var writer = new HVLVOuterPackageTopLevelDataObjectWriter(new DataWritingManager(new ActionInfo(null, OuterPackageBO)));
			var dataObject = writer.GetDataObject(OuterPackageBO);

			#region Assertions

			CombineAssertions(() =>
			{
				var packingLineCollection = dataObject.PackingLineCollection;
				AssertEquals(1, packingLineCollection.Count);

				#region HVLV Outer Package

				var outerPackageDataObject = dataObject.PackingLineCollection.Single(x => x.Barcode.HasValue && x.Barcode.Value == "ECOM1234");
				AssertEquals("OPN", outerPackageDataObject.Status);
				AssertEquals("WTGC1234", outerPackageDataObject.ContainerNumber);
				AssertEquals("56L", outerPackageDataObject.PackType.Code);
				AssertEquals(1.1m, outerPackageDataObject.Volume);
				AssertEquals(22.2m, outerPackageDataObject.Weight);
				AssertEquals(3.3m, outerPackageDataObject.Length);
				AssertEquals(4.4m, outerPackageDataObject.Height);
				AssertEquals(5.5m, outerPackageDataObject.Width);
				AssertEquals(Weight.Kilograms, outerPackageDataObject.WeightUnit.Code);
				AssertEquals(Volume.CubicMetres, outerPackageDataObject.VolumeUnit.Code);
				AssertEquals(Length.Metres, outerPackageDataObject.LengthUnit.Code);
				AssertEquals("LTT001", outerPackageDataObject.ReferenceNumber);
				AssertEquals("KFC", outerPackageDataObject.Commodity.Code);
				AssertEquals("D2D", dataObject.CarrierServiceLevel.Code);
				AssertEquals(1L, outerPackageDataObject.PackQty);

				#endregion

				#region OrgAddresses

				var customsDepot = outerPackageDataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.CustomsDepotAddress));
				AssertEquals("123 NVIDIA Street", customsDepot.Address1);

				var arrivalCFS = outerPackageDataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
				AssertEquals("123 NVIDIA Street", arrivalCFS.Address1);

				var deliveryLocalCartage = outerPackageDataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == AddressTypes.DeliveryLocalCartage);
				AssertEquals("789 AMD Street", deliveryLocalCartage.Address1);

				var pickupLocalCartage = outerPackageDataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == AddressTypes.PickupLocalCartage);
				AssertEquals("789 AMD Street", pickupLocalCartage.Address1);

				#endregion

				#region HVLV Items

				var outerPackageDataObjectPackingLineCollection = outerPackageDataObject.PackingLineCollection;
				AssertEquals(2, outerPackageDataObjectPackingLineCollection.Count);

				var item1DataObject = outerPackageDataObjectPackingLineCollection.Single(x => x.OrderReference.HasValue && x.OrderReference.Value == "NVIDIA001");
				AssertEquals("1234567890", item1DataObject.Barcode);
				AssertEquals("BOX", item1DataObject.PackType.Code);
				AssertEquals((ZDecimal)1.23, item1DataObject.ManifestedWeight);
				AssertEquals((ZDecimal)10, item1DataObject.Weight);
				AssertEquals(false, item1DataObject.RequiresFumigationCertificate);
				AssertEquals(false, item1DataObject.IsPersonalEffects);
				AssertEquals(false, item1DataObject.IsTimber);
				AssertEquals(false, item1DataObject.IsPerishable);
				AssertEquals(0, item1DataObject.OutturnQty);
				AssertEquals(0, item1DataObject.OutturnDamagedQty);
				AssertEquals(0, item1DataObject.OutturnPillagedQty);
				AssertEquals("HVI00001", item1DataObject.ReferenceNumber);

				AssertEquals(3m, item1DataObject.Height);
				AssertEquals(4m, item1DataObject.Length);
				AssertEquals(5m, item1DataObject.Width);
				AssertEquals((ZDecimal)0.5, item1DataObject.ManifestedVolume);
				AssertEquals((ZDecimal)0.5, item1DataObject.Volume);
				AssertEquals(Length.Metres, item1DataObject.LengthUnit.Code);
				AssertEquals(Volume.CubicMetres, item1DataObject.VolumeUnit.Code);

				var item2DataObject = outerPackageDataObjectPackingLineCollection.Single(x => x.OrderReference.HasValue && x.OrderReference.Value == "AMD001");
				AssertEquals("BOX", item2DataObject.PackType.Code);
				AssertEquals((ZDecimal)10, item2DataObject.ManifestedWeight);
				AssertEquals((ZDecimal)10, item2DataObject.Weight);
				AssertEquals((ZDecimal)0.5, item2DataObject.ManifestedVolume);
				AssertEquals((ZDecimal)0.5, item2DataObject.Volume);
				AssertEquals(false, item2DataObject.RequiresFumigationCertificate);
				AssertEquals(false, item2DataObject.IsPersonalEffects);
				AssertEquals(false, item2DataObject.IsTimber);
				AssertEquals(false, item2DataObject.IsPerishable);
				AssertEquals(1, item2DataObject.OutturnQty);
				AssertEquals(1, item2DataObject.OutturnDamagedQty);
				AssertEquals(0, item2DataObject.OutturnPillagedQty);
				AssertEquals("HVI00002", item2DataObject.ReferenceNumber);

				#endregion

				#region HVLV Item Lines

				AssertEquals(1, item1DataObject.PackedItemCollection.Count);
				var itemLine1DataObject = item1DataObject.PackedItemCollection.Single(x => x.Description.Value == "RTX 3090 Ti");
				AssertEquals(54.321M, itemLine1DataObject.GoodsValue);
				AssertEquals(1.23M, itemLine1DataObject.GrossWeight);
				AssertEquals("KG", itemLine1DataObject.GrossWeightUnit.Code);
				AssertEquals(3.21M, itemLine1DataObject.NetWeight);
				AssertEquals("KG", itemLine1DataObject.NetWeightUnit.Code);
				AssertEquals(1M, itemLine1DataObject.PackedQuantity);
				AssertEquals("RTX3090", itemLine1DataObject.Product.Code);
				AssertEquals(12.345M, itemLine1DataObject.CIFValue);
				AssertEquals("www.nvidia.com/en-au/geforce/graphics-cards/30-series/rtx-3090-3090ti/", itemLine1DataObject.ItemSpecificationUrl);

				var itemLine1CustomsInfoDataObject = dataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single(x => x.Link == itemLine1DataObject.CommercialInvoiceLineLink);
				AssertEquals("654321", itemLine1CustomsInfoDataObject.HarmonisedCode);
				AssertEquals(12.345M, itemLine1CustomsInfoDataObject.CustomsValue);
				AssertEquals(3.21M, itemLine1CustomsInfoDataObject.NetWeight);
				AssertEquals(1.23M, itemLine1CustomsInfoDataObject.Weight);
				AssertEquals("KG", itemLine1CustomsInfoDataObject.WeightUnit.Code);
				AssertEquals(1M, itemLine1CustomsInfoDataObject.CustomsQuantity);

				var itemLine1CustomsSupportInfoDataObject = itemLine1CustomsInfoDataObject.CustomsSupportingInformationCollection.Single();
				AssertEquals("123456", itemLine1CustomsSupportInfoDataObject.Tariff);
				AssertEquals("AU", itemLine1CustomsSupportInfoDataObject.Country.Code);

				#endregion
			});

			#endregion
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			OuterPackageBO = HVLVOuterPackageDataContextManagerTest.GetOuterPackageWithTestData(Factory);
			Factory.SaveForTesting();
		}

		HVLVOuterPackage OuterPackageBO;

		#endregion
	}
}
