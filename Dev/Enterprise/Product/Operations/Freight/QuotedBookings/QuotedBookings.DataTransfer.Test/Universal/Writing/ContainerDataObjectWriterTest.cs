using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	public class ContainerDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestContainerDataExporting()
		{
			var containerBO = Factory.New<ForwardingContainer>();
			var arrivalAddress = Factory.NewWithValidTestData<OrgAddress>();
			var departureAddress = Factory.NewWithValidTestData<OrgAddress>();
			containerBO.JC_OA_ArrivalContainerYardAddress = arrivalAddress.PK;
			containerBO.JC_OA_DepartureContainerYardAddress = departureAddress.PK;
			Factory.Save();
			var containerData = new ContainerDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CLI, containerBO))).GetDataObject(containerBO);
			AssertNotNull("containerData", containerData);
			#region Check Columns of Exporting
			CombineAssertions(delegate
			{
				AssertEquals("DeliveryMode", containerBO.JC_DeliveryMode, containerData.DeliveryMode);
				AssertEquals("HumidityPercent", containerBO.JC_HumidityPercent, containerData.HumidityPercent);
				AssertEquals("IsShipperOwned", containerBO.JC_IsShipperOwned, containerData.IsShipperOwned);
				var arrivalAddressDataObject = containerData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerYardEmptyReturnAddress));
				AssertNotNull("Arrival Container Yard Address Data Object", arrivalAddressDataObject);
				AssertEquals("Should match the content of arrival container address", arrivalAddress.Address1, arrivalAddressDataObject.Address1);
				var departureAddressDataObject = containerData.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ContainerYardEmptyPickupAddress));
				AssertNotNull("Departure Container Yard Address Data Object", departureAddressDataObject);
				AssertEquals("Should match the content of departure container address", departureAddress.Address1, departureAddressDataObject.Address1);
			}

			);
			#endregion
		}
	}
}
