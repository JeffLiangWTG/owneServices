using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVOuterPackageTopLevelDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestImportHVLVOuterPackage()
		{
			var outerPackage = HVLVOuterPackageDataContextManagerTest.GetOuterPackageWithTestData(Factory);
			Factory.SaveForTesting();

			var importedOuterPackage = new HVLVOuterPackageTopLevelDataObjectReader(OuterPackageDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals(outerPackage.HVO_PackageReference, importedOuterPackage.HVO_PackageReference);
				AssertEquals(outerPackage.HVO_Status, importedOuterPackage.HVO_Status);
				AssertEquals(outerPackage.HVO_PackageBarcode, importedOuterPackage.HVO_PackageBarcode);
				AssertEquals(outerPackage.HVO_ContainerNumber, importedOuterPackage.HVO_ContainerNumber);
				AssertEquals(outerPackage.HVO_F3_NKPackageType, importedOuterPackage.HVO_F3_NKPackageType);
				AssertEquals(outerPackage.HVO_RH_NKCommodityCode, importedOuterPackage.HVO_RH_NKCommodityCode);
				AssertEquals(outerPackage.HVO_PL_NKLastMileCarrierServiceLevel, importedOuterPackage.HVO_PL_NKLastMileCarrierServiceLevel);
				AssertEquals(outerPackage.HVO_Volume, importedOuterPackage.HVO_Volume);
				AssertEquals(outerPackage.HVO_Weight, importedOuterPackage.HVO_Weight);
				AssertEquals(outerPackage.HVO_Length, importedOuterPackage.HVO_Length);
				AssertEquals(outerPackage.HVO_Height, importedOuterPackage.HVO_Height);
				AssertEquals(outerPackage.HVO_Width, importedOuterPackage.HVO_Width);
				AssertEquals(outerPackage.HVO_WeightUQ, importedOuterPackage.HVO_WeightUQ);
				AssertEquals(outerPackage.HVO_VolumeUQ, importedOuterPackage.HVO_VolumeUQ);
				AssertEquals(outerPackage.HVO_UnitOfDimension, importedOuterPackage.HVO_UnitOfDimension);
				AssertEquals(outerPackage.HVO_OH_LastMileCarrier, importedOuterPackage.HVO_OH_LastMileCarrier);
				AssertEquals(outerPackage.HVO_OA_DestinationDepot, importedOuterPackage.HVO_OA_DestinationDepot);
			});
		}

		public void TestPopulateOuterPackageLinkForItems()
		{
			#region Setup

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item = bookingHeader.Consignments.AddNew().Items.AddNew();
			item.HVI_ItemId = "RTX 4070";
			item.HVI_ManifestedWeight = 100;

			AssertEquals("Precondition: item is not linked to outer pacakge", Guid.Empty, item.HVI_HVO_OuterPackage);

			Factory.SaveForTesting();

			#endregion

			#region Universal Shipment

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CarrierServiceLevel = new ServiceLevel() { Code = "STD" };

			var outerPackagePackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "AMD0001",
				Barcode = "NVIDIA0001",
				ContainerNumber = "GPUCONTAINER0001",
			};

			var itemPackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "RTX 4070",
				Weight = 1,
			};

			outerPackagePackingLine.SetPackingLineCollection(() => new DataObjectList<PackingLine> { itemPackingLine }.ToList());
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { outerPackagePackingLine });

			#endregion

			var outerPackage = new HVLVOuterPackageTopLevelDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#region Assertions

			CombineAssertions(() =>
			{
				var newFactory = new BusinessObjectFactory();
				item = newFactory.Load<HVLVItem>(item.PK);

				AssertEquals("The item should be linked to outer package", outerPackage.PK, item.HVI_HVO_OuterPackage);
				AssertEquals("The item should not be updated, only the link should be", (ZDecimal)100, item.HVI_ManifestedWeight);
			});

			#endregion
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var logger = new DummyLogger();
			var outerPackageXML = HVLVOuterPackageDataContextManagerTest.GetValidPopulatedUniversalShipmentXML();
			OuterPackageDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(outerPackageXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(OuterPackageDataObject, stream, logger);
			}
		}

		UniversalShipment OuterPackageDataObject;

		#endregion
	}
}
