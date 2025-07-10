using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Tests
{
	public class JobAddressAdditionalInfoDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var jobAddressAdditionalInfoBO = Factory.New<JobAddressAdditionalInfo>();
			jobAddressAdditionalInfoBO.JAI_AddressType = "CEG";
			jobAddressAdditionalInfoBO.JAI_TransportMode = "ROA";

			var actionInfo = new ActionInfo(RecipientRoleType.ORP, jobAddressAdditionalInfoBO);
			var dataWritingManager = new DataWritingManager(actionInfo);

			var writer = new JobAddressAdditionalInfoDataObjectWriter(dataWritingManager);

			var dataObject = writer.GetDataObject(jobAddressAdditionalInfoBO);

			AssertEquals("ConsigneePickupDeliveryAddress", dataObject.AddressType);
			AssertEquals("ROA", dataObject.TransportMode.Code);
			AssertEquals("Road Freight", dataObject.TransportMode.Description);
		}
	}
}
