#region Test
#if DEBUG
namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	using System.Linq;
	using CargoWise.Common.Testing;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.Freight.Agency.DataTransfer.Universal.Testing;
	using Enterprise.Freight.DataTransfer.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	class AgencyShipmentContainerUniversalTestHelper
	{
		public static AgencyShipmentContainer CreateContainerForAgencyShipment(BusinessObjectFactory factory,
			ZString containerNumber,
			ZString loadPort,
			ZString dischargePort,
			ZString vesselName,
			ZString voyageNumber,
			ZString lloydsNumber,
			ZString houseBill,
			ZString uniqueConsignRef)
		{
			return CreateContainerForAgencyShipment(factory, containerNumber, "", loadPort, dischargePort, vesselName, voyageNumber, lloydsNumber, houseBill, uniqueConsignRef);
		}

		public static AgencyShipmentContainer CreateContainerForAgencyShipment(BusinessObjectFactory factory,
			ZString containerNumber,
			ZString containerReleaseNumber,
			ZString loadPort,
			ZString dischargePort,
			ZString vesselName,
			ZString voyageNumber,
			ZString lloydsNumber,
			ZString houseBill,
			ZString uniqueConsignRef)
		{
			return CreateContainerForAgencyShipment(factory, containerNumber, containerReleaseNumber, loadPort, dischargePort, vesselName, voyageNumber, lloydsNumber, houseBill, "", uniqueConsignRef);
		}

		public static AgencyShipmentContainer CreateContainerForAgencyShipment(BusinessObjectFactory factory,
			ZString containerNumber,
			ZString containerReleaseNumber,
			ZString loadPort,
			ZString dischargePort,
			ZString vesselName,
			ZString voyageNumber,
			ZString lloydsNumber,
			ZString houseBill,
			ZString bookingReference,
			ZString uniqueConsignRef)
		{
			var vessel = RefVessel.LookupVesselByName(vesselName, factory).FirstOrDefault();

			if (vessel == null)
			{
				vessel = factory.New<RefVessel>();
				vessel.RV_Name = vesselName;
				vessel.RV_LloydsNumber = lloydsNumber;
			}

			var sailing = UniversalTestHelper.CreateSailingWithVoyage(factory, loadPort, dischargePort, vesselName, voyageNumber);
			sailing.Vessel.RV_LloydsNumber = vessel.RV_LloydsNumber;

			var agencyShipment = factory.New<AgencyShipment>();
			agencyShipment.JS_JX = sailing.PK;
			agencyShipment.JS_UniqueConsignRef = uniqueConsignRef;
			agencyShipment.JS_HouseBill = houseBill;
			agencyShipment.JS_CFSReference = bookingReference;

			var stock = factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, containerNumber));

			if (stock == null)
			{
				stock = factory.New<RefContainerStock>();
				stock.R6_ContainerNum = containerNumber;
				stock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}

			AgencyShipmentContainer agencyShipmentContainer = agencyShipment.ShippingContainers.AddNew();
			agencyShipmentContainer.JC_ContainerNum = stock.R6_ContainerNum;
			agencyShipmentContainer.JC_RC = stock.R6_RC;
			agencyShipmentContainer.JC_ReleaseNum = containerReleaseNumber;

			return agencyShipmentContainer;
		}

		public static AgencyShipmentContainer CreateContainerForAgencyShipment(BusinessObjectFactory factory, AgencyShipment shipment, ZString containerNumber, string refContainerCode)
		{
			return CreateContainerForAgencyShipment(factory, shipment, containerNumber, refContainerCode, "");
		}

		public static AgencyShipmentContainer CreateContainerForAgencyShipment(BusinessObjectFactory factory, AgencyShipment shipment, ZString containerNumber, string refContainerCode, string containerReleaseNumber)
		{
			var stock = factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, containerNumber));

			if (stock == null)
			{
				stock = factory.New<RefContainerStock>();
				stock.R6_ContainerNum = containerNumber;
				stock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, refContainerCode).PK;
			}

			AgencyShipmentContainer agencyShipmentContainer = shipment.ShippingContainers.AddNew();
			agencyShipmentContainer.JC_ContainerNum = stock.R6_ContainerNum;
			agencyShipmentContainer.JC_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, refContainerCode).PK;
			agencyShipmentContainer.JC_ReleaseNum = containerReleaseNumber;

			return agencyShipmentContainer;
		}

		public static AgencyShipmentContainer CreateTopLevelPackForAgencyShipment(BusinessObjectFactory factory,
			ZString goodsItemID,
			ZString loadPort,
			ZString dischargePort,
			ZString vesselName,
			ZString voyageNumber,
			ZString lloydsNumber,
			ZString houseBill,
			ZString uniqueConsignRef,
			ZString packingMode)
		{
			return CreateTopLevelPackForAgencyShipment(factory, goodsItemID, loadPort, dischargePort, vesselName, voyageNumber, lloydsNumber, houseBill, "", uniqueConsignRef, packingMode);
		}

		public static AgencyShipmentContainer CreateTopLevelPackForAgencyShipment(BusinessObjectFactory factory,
			ZString goodsItemID,
			ZString loadPort,
			ZString dischargePort,
			ZString vesselName,
			ZString voyageNumber,
			ZString lloydsNumber,
			ZString houseBill,
			ZString bookingReference,
			ZString uniqueConsignRef,
			ZString packingMode)
		{
			var vessel = RefVessel.LookupVesselByName(vesselName, factory).FirstOrDefault();

			if (vessel == null)
			{
				vessel = factory.New<RefVessel>();
				vessel.RV_Name = vesselName;
				vessel.RV_LloydsNumber = lloydsNumber;
			}

			var sailing = UniversalTestHelper.CreateSailingWithVoyage(factory, loadPort, dischargePort, vesselName, voyageNumber);
			sailing.Vessel.RV_LloydsNumber = vessel.RV_LloydsNumber;

			var agencyShipment = factory.New<AgencyShipment>();
			agencyShipment.JS_PackingMode = packingMode;
			agencyShipment.JS_JX = sailing.PK;
			agencyShipment.JS_UniqueConsignRef = uniqueConsignRef;
			agencyShipment.JS_HouseBill = houseBill;
			agencyShipment.JS_CFSReference = bookingReference;

			var agencyShipmentContainer = agencyShipment.ShippingContainers.AddNew();
			agencyShipmentContainer.JC_ContainerNum = goodsItemID;

			return agencyShipmentContainer;
		}

		public static AgencyShipmentContainer CreateTopLevelPackForAgencyShipment(AgencyShipment shipment, ZString goodsItemID)
		{
			AgencyShipmentContainer agencyShipmentContainer = shipment.ShippingContainers.AddNew();
			agencyShipmentContainer.JC_ContainerNum = goodsItemID;
			return agencyShipmentContainer;
		}

		public static AgencyShipmentContainerReferences CreateReferences(ZString containerNumber, ZString containerISOCode, ZString containerReleaseNumber, ZString oceanBill, SailingReference sailingReference)
		{
			var result = AgencyUniversalTestHelper.CreateReferences(oceanBill, sailingReference, o => new AgencyShipmentContainerReferences(o, containerNumber));
			result.ContainerISOCode = containerISOCode;
			result.ContainerNumber = containerNumber;
			result.ContainerReleaseNumber = containerReleaseNumber;

			return result;
		}
	}
}

#endif
#endregion
