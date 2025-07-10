using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerStockManagerContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJC_RC()
		{
			const string message = "The ISO code recorded against the container 'FAKU4100011' (22G0) does not match the ISO code for this container type (42G0).";
			ZGuid rC_20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			ZGuid rC_40GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "FAKU4100011";
			stock.R6_RC = rC_20GP_PK;
			Factory.Save();
			Container.JC_ContainerNum = "FAKU4100011";
			Container.JC_RC = rC_40GP_PK;
			AssertHasWarning(Container.JC_RCInfo, message);
			Container.JC_RC = rC_20GP_PK;
			AssertNoWarnings(Container.JC_RCInfo);
		}

		public void TestJC_IsShipperOwned()
		{
			const string msgShipperNotTicked = "This container is recorded as being shipper owned but the shipper owned flag has not been ticked.";
			const string msgShipperTicked = "This container is recorded as being non-shipper owned but the shipper owned flag has been ticked.";
			ZGuid rC_20GP_PK = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			RefContainerStock stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "FAKU4100011";
			stock1.R6_RC = rC_20GP_PK;
			stock1.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			Factory.Save();
			Container.JC_ContainerNum = "FAKU4100011";
			Container.JC_IsShipperOwned = false;
			AssertHasWarning(Container.JC_IsShipperOwnedInfo, msgShipperNotTicked);
			stock1.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Container.JC_IsShipperOwned = true;
			AssertHasWarning("The container is shipper owned: ", Container.JC_IsShipperOwnedInfo, msgShipperTicked);
		}

		#region Implementation
		BillOfLading Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<BillOfLading>());
			}
		}

		BillOfLading shipment;
		BillOfLadingContainer Container
		{
			get
			{
				return container ?? (container = Shipment.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer container;
		#endregion
	}
}
