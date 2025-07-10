using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentOrgFetcherTest : BaseFreightTest
	{
		public void TestOrgFetcherSystemNotBroken()
		{
			OrgHeader aIRCartage1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader lCLCartage1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader fCLCartage1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader aIRCartage2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader lCLCartage2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader fCLCartage2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.SetRelatedParty(aIRCartage1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(lCLCartage1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignor.SetRelatedParty(fCLCartage1, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.SetRelatedParty(aIRCartage2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			consignee.SetRelatedParty(lCLCartage2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignee.SetRelatedParty(fCLCartage2, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			CommonShipment shipment = CommonShipment.New(Factory);
			AssertNotNull("Lazy Load - cartage will not be set until the docs and cartage is loaded (dodgy)", shipment.DocsAndCartage);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			AssertEquals("Export Sea FCL", fCLCartage1.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Import Sea FCL", fCLCartage2.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Export Sea LCL", lCLCartage1.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Import Sea LCL", lCLCartage2.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals("Export Sea BCN", lCLCartage1.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Import Sea BCN", fCLCartage2.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);

			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertEquals("Expected Air", aIRCartage1.PK, shipment.DocsAndCartage.PickupCartageCoPK);
			AssertEquals("Expected Air", aIRCartage2.PK, shipment.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestSettingConsignorSetsOriginUsingRegistryDefault()
		{
			try
			{
				OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress address1 = consignor1.Addresses.AddNew();
				OrgAddress address2 = consignor1.Addresses.AddNew();
				OrgAddress address3 = consignor1.Addresses.AddNew();

				address1.FillWithValidTestData();
				address2.FillWithValidTestData();
				address3.FillWithValidTestData();

				address1.OA_RL_NKRelatedPortCode = "USNYC";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address3.OA_RL_NKRelatedPortCode = "";
				consignor1.OH_RL_NKClosestPort = "HKHKG";

				OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				consignor2.OH_RL_NKClosestPort = "NZAKL";

				Factory.Save();

				FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);

				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.ConsignorPK = consignor1.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Origin to be same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Origin to be same as selected Address.", address2.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				shipment.ConsignorPK = consignor2.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor2.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				CommonShipment shipment2 = Factory.New<CommonShipment>();
				shipment2.ConsignorPK = consignor1.PK;
				AssertEquals("Expecting origin to be empty as defaulting will occur from consol.", true, shipment2.JS_RL_NKOrigin.IsEmpty);
			}
			finally
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad).ClearCache();
			}
		}

		public void TestSettingConsignorSetsOriginUsingRegistryDefault_Booking()
		{
			try
			{
				var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				var address1 = consignor1.Addresses.AddNew();
				var address2 = consignor1.Addresses.AddNew();
				var address3 = consignor1.Addresses.AddNew();

				address1.FillWithValidTestData();
				address2.FillWithValidTestData();
				address3.FillWithValidTestData();

				address1.OA_RL_NKRelatedPortCode = "USNYC";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address3.OA_RL_NKRelatedPortCode = "";
				consignor1.OH_RL_NKClosestPort = "HKHKG";

				var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				consignor2.OH_RL_NKClosestPort = "NZAKL";

				Factory.Save();

				FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_IsForwardRegistered = false;
				shipment.JS_IsBooking = true;

				shipment.ConsignorPK = consignor1.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Origin to be same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Origin to be same as selected Address.", address2.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				shipment.ConsignorPK = consignor2.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor2.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				var shipment2 = Factory.New<CommonShipment>();
				shipment2.JS_IsForwardRegistered = false;
				shipment2.JS_IsBooking = true;

				shipment2.ConsignorPK = consignor1.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment2.JS_RL_NKOrigin);

				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Origin to be same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment2.JS_RL_NKOrigin);

				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Origin to be same as selected Address.", address2.OA_RL_NKRelatedPortCode, shipment2.JS_RL_NKOrigin);

				shipment2.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment2.JS_RL_NKOrigin);

				shipment2.ConsignorPK = consignor2.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor2.OH_RL_NKClosestPort, shipment2.JS_RL_NKOrigin);
			}
			finally
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad).ClearCache();
			}
		}

		public void TestSettingConsigneeSetsDestinationUsingRegistryDefault()
		{
			try
			{
				OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress address1 = consignee1.Addresses.AddNew();
				OrgAddress address2 = consignee1.Addresses.AddNew();
				OrgAddress address3 = consignee1.Addresses.AddNew();

				address1.FillWithValidTestData();
				address2.FillWithValidTestData();
				address3.FillWithValidTestData();

				address1.OA_RL_NKRelatedPortCode = "USNYC";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address3.OA_RL_NKRelatedPortCode = "";
				consignee1.OH_RL_NKClosestPort = "HKHKG";

				OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
				consignee2.OH_RL_NKClosestPort = "NZAKL";

				Factory.Save();

				FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);

				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.ConsigneePK = consignee1.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Destination to be same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Destination to be same as selected Address.", address2.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				shipment.ConsigneePK = consignee2.PK;
				AssertEquals("Expecting Destination to be same as new Consignee Port.", consignee2.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				CommonShipment shipment2 = Factory.New<CommonShipment>();
				shipment2.ConsigneePK = consignee1.PK;
				AssertEquals("Expecting Origin to remain blank because default in registry is false.", true, shipment2.JS_RL_NKDestination.IsEmpty);
			}
			finally
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge).ClearCache();
			}
		}

		public void TestSettingConsigneeSetsDestinationUsingRegistryDefault_Booking()
		{
			try
			{
				var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
				var address1 = consignee1.Addresses.AddNew();
				var address2 = consignee1.Addresses.AddNew();
				var address3 = consignee1.Addresses.AddNew();

				address1.FillWithValidTestData();
				address2.FillWithValidTestData();
				address3.FillWithValidTestData();

				address1.OA_RL_NKRelatedPortCode = "USNYC";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address3.OA_RL_NKRelatedPortCode = "";
				consignee1.OH_RL_NKClosestPort = "HKHKG";

				var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
				consignee2.OH_RL_NKClosestPort = "NZAKL";

				Factory.Save();

				FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_IsForwardRegistered = false;
				shipment.JS_IsBooking = true;

				shipment.ConsigneePK = consignee1.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Destination to be same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Destination to be same as selected Address.", address2.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				shipment.ConsigneePK = consignee2.PK;
				AssertEquals("Expecting Destination to be same as new Consignee Port.", consignee2.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				var shipment2 = Factory.New<CommonShipment>();
				shipment2.JS_IsForwardRegistered = false;
				shipment2.JS_IsBooking = true;

				shipment2.ConsigneePK = consignee1.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment2.JS_RL_NKDestination);

				shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Destination to be same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment2.JS_RL_NKDestination);

				shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Destination to be same as selected Address.", address2.OA_RL_NKRelatedPortCode, shipment2.JS_RL_NKDestination);

				shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment2.JS_RL_NKDestination);

				shipment2.ConsigneePK = consignee2.PK;
				AssertEquals("Expecting Destination to be same as new Consignee Port.", consignee2.OH_RL_NKClosestPort, shipment2.JS_RL_NKDestination);
			}
			finally
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge).ClearCache();
			}
		}

		public void TestSettingConsignorSetsOriginAfterSaveUsingRegistryDefault()
		{
			try
			{
				OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress address1 = consignor1.Addresses.AddNew();
				OrgAddress address2 = consignor1.Addresses.AddNew();
				OrgAddress address3 = consignor1.Addresses.AddNew();

				address1.FillWithValidTestData();
				address2.FillWithValidTestData();
				address3.FillWithValidTestData();

				address1.OA_RL_NKRelatedPortCode = "USNYC";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address3.OA_RL_NKRelatedPortCode = "";
				consignor1.OH_RL_NKClosestPort = "HKHKG";

				OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				consignor2.OH_RL_NKClosestPort = "NZAKL";

				Factory.Save();

				FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);

				CommonShipment shipment = Factory.New<CommonShipment>();

				Factory.Save();

				shipment.ConsignorPK = consignor1.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				shipment.JS_RL_NKOrigin = "";

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Origin to become same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKOrigin);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Origin remain the same as before.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKOrigin);

				shipment.JS_RL_NKOrigin = "";

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Origin to be same as Consignor port.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				shipment.ConsignorPK = consignor2.PK;
				AssertEquals("Expecting Origin remain the same as before.", consignor1.OH_RL_NKClosestPort, shipment.JS_RL_NKOrigin);

				FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				CommonShipment shipment2 = Factory.New<CommonShipment>();
				shipment2.ConsignorPK = consignor1.PK;
				AssertEquals("Expecting origin to be empty as defaulting will occur from consol.", true, shipment2.JS_RL_NKOrigin.IsEmpty);
			}
			finally
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad).ClearCache();
			}
		}

		public void TestSettingConsigneeSetsDestinationAfterSaveUsingRegistryDefault()
		{
			try
			{
				OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
				OrgAddress address1 = consignee1.Addresses.AddNew();
				OrgAddress address2 = consignee1.Addresses.AddNew();
				OrgAddress address3 = consignee1.Addresses.AddNew();

				address1.FillWithValidTestData();
				address2.FillWithValidTestData();
				address3.FillWithValidTestData();

				address1.OA_RL_NKRelatedPortCode = "USNYC";
				address2.OA_RL_NKRelatedPortCode = "USLAX";
				address3.OA_RL_NKRelatedPortCode = "";
				consignee1.OH_RL_NKClosestPort = "HKHKG";

				OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
				consignee2.OH_RL_NKClosestPort = "NZAKL";

				Factory.Save();

				FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, false);

				CommonShipment shipment = Factory.New<CommonShipment>();

				Factory.Save();

				shipment.ConsigneePK = consignee1.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				shipment.JS_RL_NKDestination = "";

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address1.PK;
				AssertEquals("Expecting Destination to become same as selected Address.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKDestination);

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address2.PK;
				AssertEquals("Expecting Destination remain the same as before.", address1.OA_RL_NKRelatedPortCode, shipment.JS_RL_NKDestination);

				shipment.JS_RL_NKDestination = "";

				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = address3.PK;
				AssertEquals("Expecting Destination to be same as Consignee port.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				shipment.ConsigneePK = consignee2.PK;
				AssertEquals("Expecting Destination remain the same as before.", consignee1.OH_RL_NKClosestPort, shipment.JS_RL_NKDestination);

				FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, true);
				CommonShipment shipment2 = Factory.New<CommonShipment>();
				shipment2.ConsigneePK = consignee1.PK;
				AssertEquals("Expecting Destination to be empty as defaulting will occur from consol.", true, shipment2.JS_RL_NKDestination.IsEmpty);
			}
			finally
			{
				((IRegistryItemInternals)FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge).ClearCache();
			}
		}

		#region GetParametersForEvent

		public void TestGetParametersForEvent_EventIsCargoAvailable_ReturnFacilityParam()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Facility", Facilities.Code.Terminal, shipment.GetParametersForEvent(Events.CargoAvailable)[EventReferenceParameters.Codes.Facility]);

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("Facility", Facilities.Code.Depot, shipment.GetParametersForEvent(Events.CargoAvailable)[EventReferenceParameters.Codes.Facility]);
		}

		public void TestGetParametersForEvent_EventIsCargoAvailable_ReturnLocationParam()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			AssertEquals("Location", "AUSYD", shipment.GetParametersForEvent(Events.CargoAvailable)[EventReferenceParameters.Codes.Location]);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = "UAIEV";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKDischargePort = "AUSYD";
			consol2.Transports.AddNew("UAIEV", "USLAX");
			consol2.Transports.AddNew("USLAX", "USNYC");

			AssertEquals("Location", "USNYC", shipment.GetParametersForEvent(Events.CargoAvailable)[EventReferenceParameters.Codes.Location]);

			shipment.DocsAndCartage.JP_LCLDatesOverrideConsol = true;
			AssertEquals("Location", "AUSYD", shipment.GetParametersForEvent(Events.CargoAvailable)[EventReferenceParameters.Codes.Location]);
		}

		public void TestGetParametersForEvent_GetsFLOFromTransportLeg()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			consol.Transports.AddNew("USNYC", "UAIEV");

			var shipment = consol.Shipments.AddNew();

			AssertEquals("Location", "AUSYD", shipment.GetParametersForEvent(Events.FreightLoaded)[EventReferenceParameters.Codes.Location]);

			consol.Transports.RemoveAll();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "UAIEV";

			AssertEquals("Location", "AUBNE", shipment.GetParametersForEvent(Events.FreightLoaded)[EventReferenceParameters.Codes.Location]);
		}

		public void TestInterimReceiptProducedCodeEvent_IncludeLocationAndFacility()
		{
			var shipment = Factory.New<CommonShipment>();
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			org.MainAddress.AddAddressType(OrgAddressType.Office);
			shipment.JS_OA_ExportReceivingDepot_ZAddress.OrgPK = org.PK;
			shipment.JS_A_RCV = ZDateTime.Today;

			var eventLog = shipment.Logs.MostRecentLogByEventTime(Events.InterimReceiptProduced);
			AssertEquals("Facility and Location should be Included", "|FAC=CFS|LOC=USLAX", eventLog.SL_Reference);
		}

		#endregion

		#region Events

		public void TestCargoAvailableEventHasBeenAdded_UpdateJP_LCLAvailable()
		{
			AssertLCLDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "",
				eventFacitity: Facilities.Code.Depot,
				eventDate: 2.DaysAgo(),
				expectOverride: false,
				expectedDate: 1.DaysAgo());

			AssertLCLDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: "McLaren",
				eventDate: 2.DaysAgo(),
				expectOverride: false,
				expectedDate: 1.DaysAgo());

			AssertLCLDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: Facilities.Code.Depot,
				eventDate: 2.DaysAgo(),
				expectOverride: true,
				expectedDate: 2.DaysAgo());

			AssertLCLDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: Facilities.Code.Depot,
				eventDate: 1.DaysAgo(),
				expectOverride: false,
				expectedDate: 1.DaysAgo());
		}

		void AssertLCLDateUpdated(ZDateTime inheritedDate, string eventLocation, string eventFacitity, ZDateTime eventDate, bool expectOverride, ZDateTime expectedDate)
		{
			var sailling = Factory.NewWithValidTestData<JobSailing>();
			sailling.JX_DepotAvailabilityDate = inheritedDate;
			sailling.Destination.JB_RL_NKPortOfDischarge = "UAIEV";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.Transports[0].JW_IsLinked = true;
			consol.Transports[0].JW_JX = sailling.PK;
			consol.Transports[0].JW_RL_NKDiscPort = "UAIEV";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_RL_NKDestination = "UAIEV";
			shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_LCLDatesOverrideConsol = false;
			shipment.Logs.RemoveAndDeleteAll();

			shipment.Logs.CreateOrRecreateEventLog(
				Events.CargoAvailable,
				EstimateActual.Actual,
				eventDate.ToOffset(),
				"MCLAREN",
				EventReferenceParameters.Codes.Location.AsKeyFor(eventLocation),
				EventReferenceParameters.Codes.Facility.AsKeyFor(eventFacitity));

			AssertEquals("DocsAndCartage.JP_LCLAvailable", expectedDate, shipment.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("DocsAndCartage.JP_LCLDatesOverrideConsol", expectOverride, shipment.DocsAndCartage.JP_LCLDatesOverrideConsol);
		}

		#endregion
	}
}
