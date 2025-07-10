using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing
{
	sealed class CusSCAHouseDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestSkipFindingExisingHousebillIfCreatingNewFromHVLV()
		{
			var oceanHousebillDataObject = new Shipment();
			var hvlvShipmentDataObject = new Shipment()
			{
				WayBillNumber = "HBL9988"
			};
			var oceanBill = Factory.New<TestCusSCAOceanBill>();
			var oceanHousebill = Factory.New<CusSCAHouseForTest>();
			oceanHousebill.CA_CB = oceanBill.PK;
			oceanHousebill.CA_HouseBill = "HBL9988";
			var reader = new CusSCAHouseDataObjectReaderForTest(oceanBill,
				new HVLVShipmentDataObjectWrapper(hvlvShipmentDataObject, oceanHousebillDataObject, Factory),
				hvlvShipmentDataObject, new DummyLogger(), Factory) as ITopLevelDataObjectReader;
			Assert("precondition", !oceanBill.IsInDatabase);
			AssertNull("Should not find anything", reader.GetExistingBusinessObject());
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRulesIsSealed()
		{
			var type = typeof(CusSCAHouseDataObjectReader<,>);
			var method = type.GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert(@"GetExistingBusinessObjectUsingModuleSpecificBusinessRules should be sealed.
If it is needed to be overridden, please make sure Finding existing housebill is skipped when XUS is from HVLV Shipment and masterbill is new.",
method.IsFinal);
		}

		sealed class CusSCAHouseDataObjectReaderForTest : CusSCAHouseDataObjectReader<CusSCAHouseForTest, CusSCAPivotForTest>
		{
			public CusSCAHouseDataObjectReaderForTest(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(oceanBill, hvlvConsolidatorShipmentWrapper, dataObject, logger, factory) { }

			protected override CusSCAPivotDataObjectReader<CusSCAPivotForTest> GetNewCusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, PackingLine packageDataObject)
			{
				throw new NotImplementedException();
			}

			protected override void PopulateCountrySpecificDetails(IColumnIndexer houseBill)
			{
				throw new NotImplementedException();
			}
		}
	}
}
