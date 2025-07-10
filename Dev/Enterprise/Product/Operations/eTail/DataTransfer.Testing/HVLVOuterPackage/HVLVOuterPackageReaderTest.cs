using System.Linq;
using CargoWise.Definitions.Ecommerce;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVOuterPackageReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void Test_PopulateItems_RejectsUpdatingOuterPackageWithStatusCON()
		{
			var existingLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			existingLoadList.HVL_UniqueReference = "LOADLIST1";

			var existingPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			existingPackage.HVO_PackageReference = "OuterPackageRef";
			existingPackage.HVO_ContainerNumber = "1234567";
			existingPackage.HVO_Status = HVLVOuterPackageStatus.Codes.Consolidated;

			Factory.SaveForTesting();

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.MainAddress.Address1 = "2 Destination Depot Street";
			destinationDepot.OH_Code = "DESORG";

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_WithOuterPackage);
			loadListDataObject.DataContext.DataTargetCollection.First().Key = "LOADLIST1";
			var logger = new TestErrorLogger();

			new HVLVOriginLoadListDataObjectReader(loadListDataObject, logger, Factory).ReadIntoBusinessObject();

			var expectErrorMessage = @"Cannot populate HVLVOuterPackage because:
Outer Package OuterPackageRef cannot be updated via XUS as it has already been consolidated.";

			AssertMultilineASCIIEquals("Service Task Logs", expectErrorMessage, logger.GetErrors());
			AssertEquals("Outer Package should not be updated", "1234567", existingPackage.HVO_ContainerNumber);
		}
	}
}
