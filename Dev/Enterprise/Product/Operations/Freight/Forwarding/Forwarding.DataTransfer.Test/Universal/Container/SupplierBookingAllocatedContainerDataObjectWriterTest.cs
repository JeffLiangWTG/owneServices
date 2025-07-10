using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	internal class SupplierBookingAllocatedContainerDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestPopulateDataObject()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "JK001";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "JC001";
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerCount = 1;
			var writer = new SupplierBookingAllocatedContainerDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, container)));

			var dataObject = writer.GetDataObject(container);
			var containerObject = dataObject.ContainerCollection.Single();
			AssertEquals("JK001", dataObject.GetMatchingDataSource(DataContextType.ForwardingConsol).Key);
			AssertEquals("JC001", containerObject.ContainerNumber);
			AssertEquals(Core.Constants.DeliveryModes.Codes.CY_CY, containerObject.DeliveryMode);
			AssertEquals(Core.Constants.ContainerModes.LCL, containerObject.FCL_LCL_AIR.Code);
			AssertEquals("20GP", containerObject.ContainerType.Code);
			AssertEquals(1, containerObject.ContainerCount);
		}
	}
}
