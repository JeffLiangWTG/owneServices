using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortHubSelectionCollectionWrapper))]
	public class PortHubSelectionCollectionWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDuplicateSelectionValidation()
		{
			const string duplicateError = "There exists another Selection matching these Conditions and at least one matching Zone.";

			var collectionWrapper = new PortHubSelectionCollectionWrapper(Factory);
			var portHub1 = collectionWrapper.Collection.AddNew();
			var portHub2 = collectionWrapper.Collection.AddNew();
			var zone1 = Factory.New<RateTransportZone>();
			var zone2 = Factory.New<RateTransportZone>();
			var dispatchDepotAddress1 = Factory.New<OrgAddress>();
			var dispatchDepotAddress2 = Factory.New<OrgAddress>();
			var carrierBookingAgent1 = Factory.New<OrgHeader>();
			var carrierBookingAgent2 = Factory.New<OrgHeader>();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "CSM";
			serviceLevel.PL_CarrierServiceLevelDescription = "Custom";
			var portHubZonePivot1 = Factory.New<PortHubZonePivot>();
			portHubZonePivot1.TX_PL_NKCarrierServiceLevel = serviceLevel.PL_Code;
			var portHubZonePivot2 = Factory.New<PortHubZonePivot>();
			portHubZonePivot2.TX_PL_NKCarrierServiceLevel = serviceLevel.PL_Code;

			Action<PortHubSelection, PortHubZonePivot> setPortHubToDefault = (portHub, portHubZonePivot) =>
				{
					portHub.TY_Direction = "DLV";
					portHub.TY_UndgClass = "1";
					portHub.TY_RS_NKServiceLevel = "DEF";
					portHub.TY_F3_NKPackType = "BAG";
					portHub.TY_PackMode = "";
					portHub.TY_OA_DispatchDepotAddress = dispatchDepotAddress1.PK;
					portHub.TY_OH_CarrierBookingAgent = carrierBookingAgent1.PK;
					portHubZonePivot.TX_TY_Hub = portHub.PK;
					portHubZonePivot.TX_TZ_Zone = zone1.PK;
				};

			setPortHubToDefault(portHub1, portHubZonePivot1);
			setPortHubToDefault(portHub2, portHubZonePivot2);
			collectionWrapper.RunPreSaveValidation();
			AssertHasRowError(portHub1, duplicateError);
			AssertHasRowError(portHub2, duplicateError);

			portHub2.TY_Direction = "PIC";
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors(portHub1);
			AssertNoRowErrors(portHub2);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHub2.TY_UndgClass = "2";
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors(portHub1);
			AssertNoRowErrors(portHub2);

			portHub2.TY_UndgClass = "ALL";
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowError(portHub1, duplicateError);
			AssertNoRowError(portHub2, duplicateError);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHub2.TY_RS_NKServiceLevel = "D2D";
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors(portHub1);
			AssertNoRowErrors(portHub2);

			portHub2.TY_RS_NKServiceLevel = ZString.Empty;
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowError(portHub1, duplicateError);
			AssertNoRowError(portHub2, duplicateError);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHub2.TY_F3_NKPackType = "PLT";
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors(portHub1);
			AssertNoRowErrors(portHub2);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHub2.TY_OA_DispatchDepotAddress = dispatchDepotAddress2.PK;
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors(portHub1);
			AssertNoRowErrors(portHub2);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHubZonePivot2.TX_TY_Hub = portHub2.PK;
			collectionWrapper.RunPreSaveValidation();
			AssertHasRowError(portHub1, duplicateError);
			AssertHasRowError(portHub2, duplicateError);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHub2.TY_PackMode = "LTL";
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors("Different packmodes should prevent duplicate detection", portHub1);
			AssertNoRowErrors("Different packmodes should prevent duplicate detection", portHub2);

			setPortHubToDefault(portHub2, portHubZonePivot2);
			portHubZonePivot2.TX_TZ_Zone = zone1.PK;
			portHubZonePivot1.TX_TZ_Zone = ZGuid.Empty;
			collectionWrapper.RunPreSaveValidation();
			AssertNoRowErrors(portHub1);
			AssertNoRowErrors(portHub2);
		}
	}
}
