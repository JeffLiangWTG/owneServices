using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Test
{
	[TestedType(typeof(JobAddressAdditionalInfoCollection))]
	class AddressAdditionalInfoCollectionTest : ActiveBusinessObjectCollectionTestCase<JobAddressAdditionalInfoCollection>
	{
		protected override JobAddressAdditionalInfoCollection GetCollectionToTest()
		{
			var shipment = Factory.New<IForwardingShipment>() as CommonShipment;
			var jobAddressAdditionalInfo = Factory.New<JobAddressAdditionalInfo>();
			jobAddressAdditionalInfo.JAI_ParentID = shipment.PK;
			jobAddressAdditionalInfo.JAI_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobAddressAdditionalInfo.TransportMode = Core.Constants.TransportModes.Road;
			jobAddressAdditionalInfo.AddressType = AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress;
			shipment.Factory.Save();
			return new JobAddressAdditionalInfoCollection(shipment, shipment.Factory);
		}

		public void TestGetTransportModeDefaultValue()
		{
			var shipment = Factory.New<IForwardingShipment>() as CommonShipment;
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "1234";
			shipment.ConsignorPK = consignor.PK;

			AssertEquals(shipment.PickupByTransportMode, string.Empty);
		}

		public void TestSetTransportMode()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "1234";

			var shipment = Factory.New<IForwardingShipment>() as CommonShipment;
			shipment.ConsignorPK = consignor.PK;
			shipment.JobAddressAdditionalInfoCollection.GetOrCreate(AutoDocAddressTypes.Codes.ConsignorPickupDeliveryAddress);
			shipment.PickupByTransportMode = Core.Constants.TransportModes.Road;

			AssertEquals(shipment.JobAddressAdditionalInfoCollection.FirstOrDefault().TransportMode, Core.Constants.TransportModes.Road);

			shipment.PickupByTransportMode = Core.Constants.TransportModes.Rail;

			AssertEquals(shipment.JobAddressAdditionalInfoCollection.FirstOrDefault().TransportMode, Core.Constants.TransportModes.Rail);
		}
	}
}
