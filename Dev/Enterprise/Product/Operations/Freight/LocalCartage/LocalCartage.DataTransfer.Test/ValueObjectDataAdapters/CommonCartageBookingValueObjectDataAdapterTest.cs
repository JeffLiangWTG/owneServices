using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BookingCodes = Enterprise.Freight.Common.Business.CommonFreightConstants.LocalCartageBookingStatus.Codes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	[TestedType(typeof(CommonCartageBookingValueObjectDataAdapter))]
	sealed class CommonCartageBookingValueObjectDataAdapterTest : CommonCartageValueObjectDataAdapterTest
	{
		protected override ValueObjectDataAdapter<CommonCartage, Xsd.CartageJob> GetNewBizObjXmlDataAdapter()
		{
			return new CommonCartageBookingValueObjectDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var populatedCartage = GetSampleJobCartage(Constants.TransportModes.Air);
			CartageParentPK = populatedCartage.JJ_ParentID;
			var populatedJobCartagePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.LocalCartage.DataTransfer.Testing.TestFiles.PopulatedJobCartage.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedCartage, populatedJobCartagePath, ValidationKind.Xsd, "Populated Port Transport Job");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyCartage = Factory.New<CommonCartage>();
			var emptyCartageJobPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Freight.LocalCartage.DataTransfer.Testing.TestFiles.EmptyCartageJob.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyCartage, emptyCartageJobPath, ValidationKind.None, "Empty Port Transport Job");
		}

		ZGuid CartageParentPK
		{
			get => cartageParentPK;

			set => cartageParentPK = value;
		}

		ZGuid cartageParentPK;
		protected override void OnBeforeImportFromValueObjectForExportImportExportTest(BusinessObject bizObjOriginallyExportedFrom, BusinessObject bizObjToImportTo)
		{
			CommonCartage cartage = bizObjToImportTo as CommonCartage;
			CommonCartage originalCartage = bizObjOriginallyExportedFrom as CommonCartage;
			if (cartage != null && !CartageParentPK.IsEmpty)
			{
				cartage.JJ_ParentID = CartageParentPK;
				cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				cartage.JJ_E3_NKJobType = "";
				cartage.JJ_E3_NKJobType = originalCartage.JJ_E3_NKJobType;
				cartage.BookedMovesCollection.DeleteAll();
				cartage.JJ_ConsignmentID = "T00001234";
			}

			base.OnBeforeImportFromValueObjectForExportImportExportTest(bizObjOriginallyExportedFrom, bizObjToImportTo);
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				List<string> result = new List<string>(base.XmlNodesToExcludeFromCoverageTest);
				result.AddRange(new string[] { "TransportReference", "InvoiceNumber", "ServiceLevel", "InsuranceAmount", "TransportBillTo", "TransportBillToAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "TransportBillToAddress/Language", "TransportBillToAddress/RegistrationNumber", "BillToAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "BillToAddress/Language", "BillToAddress/RegistrationNumber", "CarrierAddress/AddressReference/Organisation/OrganisationDetails/Addresses/Language", "CarrierAddress/Language", "CarrierAddress/RegistrationNumber", "FreightCharges/ChargeCode", "FreightCharges/Description", "FreightCharges/RateChargeUnits", "FreightCharges/TotalAmount/CurrencyCode", "CartageLegs/LegType", "CartageLegs/MostDangerousGoodsStandard", "CartageLegs/MostDangerousGoodsUniqueRecordId" });
				return result.ToArray();
			}
		}

		public void TestCartageContainerLegsGetCorrectDatesForDeliveryLocalTransport()
		{
			var year = ZDateTime.Now.Year;
			var shipment = GetNewShipment("SEA", false);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(year, 1, 3);
			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			containerYard.OH_FullName = "CONTAINER PARK";
			containerYard.OH_IsContainerYard = true;
			var consol = shipment.Consols[0];
			consol.JK_OA_ContainerYardEmptyReturnAddress = containerYard.MainAddress.PK;
			var refContainer = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var container1 = shipment.Consols[0].Containers[0];
			container1.JC_ContainerNum = "CRXU1111115";
			container1.JC_ContainerMode = "FCL";
			container1.JC_RC = refContainer;
			container1.JC_DeliveryMode = "CY/CY";
			container1.JC_OA_ArrivalContainerYardAddress = containerYard.MainAddress.PK;
			container1.JC_DepartureSlotDateTime = ZDateTime.Empty;
			container1.JC_ArrivalSlotDateTime = ZDateTime.Empty;
			container1.JC_EmptyRequired = ZDateTime.Empty;
			container1.JC_PackDate = ZDateTime.Empty;
			container1.JC_LCLStorageCommences = ZDateTime.Empty;
			container1.JC_LCLUnpack = ZDateTime.Empty;
			container1.JC_LCLAvailable = ZDateTime.Empty;
			container1.JC_FCLAvailable = ZDateTime.Empty;
			container1.JC_EmptyReturnedBy = ZDateTime.Empty;
			container1.JC_ArrivalEstimatedDelivery = ZDateTime.Empty;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "TRIU1111110";
			container2.JC_ContainerMode = "FCL";
			container2.JC_RC = refContainer;
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_DepartureEstimatedPickup = new ZDateTime(year - 1, 12, 20); // should not appear on this Import Direction Cartage.
			container2.JC_ArrivalEstimatedDelivery = new ZDateTime(year, 1, 2);
			container2.JC_OA_ArrivalContainerYardAddress = containerYard.MainAddress.PK;
			shipment.JS_OuterPacks = 30;
			shipment.OuterPackLines[0].JL_JC = container1.PK;
			shipment.OuterPackLines.AddNew().JL_JC = container2.PK;
			Factory.Save();
			var cartage = new CommonCartageStatusValueObjectDataAdapter().ExportToValueObject(GetInternalCartageInNewFactory(shipment), new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("cartage.JobType", "ISCC", cartage.JobType);
			var cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartage, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Container count", 2, cartageJob.Containers.Count());
			AssertEquals("CRXU1111115", cartageJob.Containers.ElementAt(0).JC_ContainerNum);
			AssertEquals("TRIU1111110", cartageJob.Containers.ElementAt(1).JC_ContainerNum);
			AssertEquals("cartageJob.JJ_EstimatedPickup", new ZDateTime(year, 1, 3), cartageJob.JJ_EstimatedPickup);
			CombineAssertions(delegate
			{
				var container1Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[0];
				var container1Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[1];
				var container2Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[0];
				var container2Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[1];
				AssertEquals("container1Leg1.JU_PlannedPickupTime", new ZDateTime(year, 1, 3), container1Leg1.JU_PlannedPickupTime);
				AssertEquals("New CTO", container1Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg2.JU_PlannedPickupTime", ZDateTime.Empty, container1Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container1Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg1.JU_PlannedPickupTime", new ZDateTime(year, 1, 2), container2Leg1.JU_PlannedPickupTime);
				AssertEquals("New CTO", container2Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg2.JU_PlannedPickupTime", ZDateTime.Empty, container2Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container2Leg2.PickupFromDocAddress.E2_CompanyName);
			});
			cartageJob.Delete();
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Empty;
			container1.JC_ArrivalEstimatedDelivery = new ZDateTime(year, 1, 1);
			container1.JC_OverrideFCLAvailableStorage = true;
			container1.JC_FCLAvailable = new ZDateTime(year, 1, 4);
			container1.JC_ArrivalCTOStorageStartDate = new ZDateTime(year, 1, 5);
			container1.JC_EmptyReturnedBy = new ZDateTime(year, 1, 6);
			container2.JC_OverrideFCLAvailableStorage = true;
			container2.JC_FCLAvailable = new ZDateTime(year, 1, 7);
			container2.JC_ArrivalCTOStorageStartDate = new ZDateTime(year, 1, 8);
			container2.JC_EmptyReturnedBy = new ZDateTime(year, 1, 9);
			Factory.Save();
			cartage = new CommonCartageStatusValueObjectDataAdapter().ExportToValueObject(GetInternalCartageInNewFactory(shipment), new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("cartage.JobType", "ISCC", cartage.JobType);
			cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartage, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Container count", 2, cartageJob.Containers.Count());
			AssertEquals("CRXU1111115", cartageJob.Containers.ElementAt(0).JC_ContainerNum);
			AssertEquals("TRIU1111110", cartageJob.Containers.ElementAt(1).JC_ContainerNum);
			AssertEquals("cartageJob.JJ_EstimatedPickup", new ZDateTime(year, 1, 1), cartageJob.JJ_EstimatedPickup);
			CombineAssertions(delegate
			{
				var container1Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[0];
				var container1Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[1];
				var container2Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[0];
				var container2Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[1];
				AssertEquals("container1Leg1.JU_PlannedPickupTime", new ZDateTime(year, 1, 1), container1Leg1.JU_PlannedPickupTime);
				AssertEquals("New CTO", container1Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg1.JU_EstimatedDeliveryTime", ZDateTime.Empty, container1Leg1.JU_EstimatedDeliveryTime);
				AssertEquals("container1Leg2.JU_PlannedPickupTime", ZDateTime.Empty, container1Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container1Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg2.JU_EstimatedDeliveryTime", new ZDateTime(year, 1, 6), container1Leg2.JU_EstimatedDeliveryTime);
				AssertEquals("container2Leg1.JU_PlannedPickupTime", new ZDateTime(year, 1, 2), container2Leg1.JU_PlannedPickupTime);
				AssertEquals("New CTO", container2Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg1.JU_EstimatedDeliveryTime", ZDateTime.Empty, container2Leg1.JU_EstimatedDeliveryTime);
				AssertEquals("container2Leg2.JU_PlannedPickupTime", ZDateTime.Empty, container2Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container2Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg2.JU_EstimatedDeliveryTime", new ZDateTime(year, 1, 9), container2Leg2.JU_EstimatedDeliveryTime);
			});
			cartageJob.Delete();
			shipment.OuterPackLines.RemoveAndDelete(shipment.OuterPackLines[1]);
			consol.Containers.RemoveAndDelete(container2);
			container1.JC_ArrivalEstimatedDelivery = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(year, 1, 3);
			Factory.Save();
			cartage = new CommonCartageStatusValueObjectDataAdapter().ExportToValueObject(GetInternalCartageInNewFactory(shipment), new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("cartage.JobType", "ISCC", cartage.JobType);
			cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartage, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Container count", 1, cartageJob.Containers.Count());
			AssertEquals("CRXU1111115", cartageJob.Containers.ElementAt(0).JC_ContainerNum);
			AssertEquals("cartageJob.JJ_EstimatedPickup", new ZDateTime(year, 1, 3), cartageJob.JJ_EstimatedPickup);
			AssertEquals("cartageJob.JJ_EstimatedDelivery", ZDateTime.Empty, cartageJob.JJ_EstimatedDelivery);
			CombineAssertions(delegate
			{
				var container1Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[0];
				var container1Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[1];
				AssertEquals("container1Leg1.JU_PlannedPickupTime", new ZDateTime(year, 1, 3), container1Leg1.JU_PlannedPickupTime);
				AssertEquals("New CTO", container1Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg1.JU_EstimatedDeliveryTime", ZDateTime.Empty, container1Leg1.JU_EstimatedDeliveryTime);
				AssertEquals("container1Leg2.JU_PlannedPickupTime", ZDateTime.Empty, container1Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container1Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg2.JU_EstimatedDeliveryTime", new ZDateTime(year, 1, 6), container1Leg2.JU_EstimatedDeliveryTime);
			});
		}

		public void TestCartageContainerLegsGetCorrectDatesForPickupLocalTransport()
		{
			var year = ZDateTime.Now.Year;
			var shipment = GetNewShipment("SEA", true);
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(year, 1, 3);
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Empty;
			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			containerYard.OH_FullName = "CONTAINER PARK";
			containerYard.OH_IsContainerYard = true;
			var consol = shipment.Consols[0];
			consol.JK_OA_ContainerYardEmptyPickupAddress = containerYard.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			var refContainer = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			var container1 = shipment.Consols[0].Containers[0];
			container1.JC_ContainerNum = "CRXU1111115";
			container1.JC_ContainerMode = "FCL";
			container1.JC_RC = refContainer;
			container1.JC_DeliveryMode = "CY/CY";
			container1.JC_OA_ArrivalContainerYardAddress = containerYard.MainAddress.PK;
			container1.JC_DepartureSlotDateTime = ZDateTime.Empty;
			container1.JC_ArrivalSlotDateTime = ZDateTime.Empty;
			container1.JC_EmptyRequired = ZDateTime.Empty;
			container1.JC_PackDate = ZDateTime.Empty;
			container1.JC_LCLStorageCommences = ZDateTime.Empty;
			container1.JC_LCLUnpack = ZDateTime.Empty;
			container1.JC_LCLAvailable = ZDateTime.Empty;
			container1.JC_FCLAvailable = ZDateTime.Empty;
			container1.JC_EmptyReturnedBy = ZDateTime.Empty;
			container1.JC_DepartureEstimatedPickup = ZDateTime.Empty;
			container1.JC_ArrivalEstimatedDelivery = ZDateTime.Empty;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "TRIU1111110";
			container2.JC_ContainerMode = "FCL";
			container2.JC_RC = refContainer;
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_DepartureEstimatedPickup = new ZDateTime(year, 1, 2);
			container2.JC_ArrivalEstimatedDelivery = new ZDateTime(year, 2, 2);
			container2.JC_OA_ArrivalContainerYardAddress = containerYard.MainAddress.PK;
			shipment.JS_OuterPacks = 30;
			shipment.OuterPackLines[0].JL_JC = container1.PK;
			shipment.OuterPackLines.AddNew().JL_JC = container2.PK;
			Factory.Save();
			var cartage = new CommonCartageStatusValueObjectDataAdapter().ExportToValueObject(GetInternalCartageInNewFactory(shipment), new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("cartage.JobType", "ESCS", cartage.JobType);
			var cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartage, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Container count", 2, cartageJob.Containers.Count());
			AssertEquals("CRXU1111115", cartageJob.Containers.ElementAt(0).JC_ContainerNum);
			AssertEquals("TRIU1111110", cartageJob.Containers.ElementAt(1).JC_ContainerNum);
			AssertEquals("cartageJob.JJ_EstimatedPickup", new ZDateTime(year, 1, 3), cartageJob.JJ_EstimatedPickup);
			CombineAssertions(delegate
			{
				var container1Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[0];
				var container1Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[1];
				var container2Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[0];
				var container2Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[1];
				AssertEquals("container1Leg1.JU_PlannedPickupTime", ZDateTime.Empty, container1Leg1.JU_PlannedPickupTime);
				AssertEquals("CONTAINER PARK", container1Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg2.JU_PlannedPickupTime", new ZDateTime(year, 1, 3), container1Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container1Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg1.JU_PlannedPickupTime", ZDateTime.Empty, container2Leg1.JU_PlannedPickupTime);
				AssertEquals("CONTAINER PARK", container2Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg2.JU_PlannedPickupTime", new ZDateTime(year, 1, 2), container2Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container2Leg2.PickupFromDocAddress.E2_CompanyName);
			});
			cartageJob.Delete();
			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Empty;
			container1.JC_DepartureEstimatedPickup = new ZDateTime(year, 1, 1);
			container1.JC_EmptyRequired = new ZDateTime(year, 1, 6);
			container2.JC_EmptyRequired = new ZDateTime(year, 1, 9);
			Factory.Save();
			cartage = new CommonCartageStatusValueObjectDataAdapter().ExportToValueObject(GetInternalCartageInNewFactory(shipment), new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("cartage.JobType", "ESCS", cartage.JobType);
			cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartage, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Container count", 2, cartageJob.Containers.Count());
			AssertEquals("CRXU1111115", cartageJob.Containers.ElementAt(0).JC_ContainerNum);
			AssertEquals("TRIU1111110", cartageJob.Containers.ElementAt(1).JC_ContainerNum);
			AssertEquals("cartageJob.JJ_EstimatedPickup", new ZDateTime(year, 1, 1), cartageJob.JJ_EstimatedPickup);
			CombineAssertions(delegate
			{
				var container1Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[0];
				var container1Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[1];
				var container2Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[0];
				var container2Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(1))[0].CartageLegs[1];
				AssertEquals("container1Leg1.JU_PlannedPickupTime", ZDateTime.Empty, container1Leg1.JU_PlannedPickupTime);
				AssertEquals("CONTAINER PARK", container1Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg1.JU_EstimatedDeliveryTime", new ZDateTime(year, 1, 6), container1Leg1.JU_EstimatedDeliveryTime);
				AssertEquals("container1Leg2.JU_PlannedPickupTime", new ZDateTime(year, 1, 1), container1Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container1Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg2.JU_EstimatedDeliveryTime", ZDateTime.Empty, container1Leg2.JU_EstimatedDeliveryTime);
				AssertEquals("container2Leg1.JU_PlannedPickupTime", ZDateTime.Empty, container2Leg1.JU_PlannedPickupTime);
				AssertEquals("CONTAINER PARK", container2Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg1.JU_EstimatedDeliveryTime", new ZDateTime(year, 1, 9), container2Leg1.JU_EstimatedDeliveryTime);
				AssertEquals("container2Leg2.JU_PlannedPickupTime", new ZDateTime(year, 1, 2), container2Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container2Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container2Leg2.JU_EstimatedDeliveryTime", ZDateTime.Empty, container2Leg2.JU_EstimatedDeliveryTime);
			});
			cartageJob.Delete();
			shipment.OuterPackLines.RemoveAndDelete(shipment.OuterPackLines[1]);
			consol.Containers.RemoveAndDelete(container2);
			container1.JC_DepartureEstimatedPickup = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(year, 1, 3);
			Factory.Save();
			cartage = new CommonCartageStatusValueObjectDataAdapter().ExportToValueObject(GetInternalCartageInNewFactory(shipment), new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("cartage.JobType", "ESCS", cartage.JobType);
			cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartage, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("Container count", 1, cartageJob.Containers.Count());
			AssertEquals("CRXU1111115", cartageJob.Containers.ElementAt(0).JC_ContainerNum);
			AssertEquals("cartageJob.JJ_EstimatedPickup", new ZDateTime(year, 1, 3), cartageJob.JJ_EstimatedPickup);
			CombineAssertions(delegate
			{
				var container1Leg1 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[0];
				var container1Leg2 = cartageJob.GetBookedMoves(cartageJob.Containers.ElementAt(0))[0].CartageLegs[1];
				AssertEquals("container1Leg1.JU_PlannedPickupTime", ZDateTime.Empty, container1Leg1.JU_PlannedPickupTime);
				AssertEquals("CONTAINER PARK", container1Leg1.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg1.JU_EstimatedDeliveryTime", new ZDateTime(year, 1, 6), container1Leg1.JU_EstimatedDeliveryTime);
				AssertEquals("container1Leg2.JU_PlannedPickupTime", new ZDateTime(year, 1, 3), container1Leg2.JU_PlannedPickupTime);
				AssertEquals("Consignor for test", container1Leg2.PickupFromDocAddress.E2_CompanyName);
				AssertEquals("container1Leg2.JU_EstimatedDeliveryTime", ZDateTime.Empty, container1Leg2.JU_EstimatedDeliveryTime);
			});
		}

		public void TestImportVessel_MatchByLloydsNumber()
		{
			var year = ZDateTime.Now.Year;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";
			var cartageValue = new Xsd.CartageJob();
			cartageValue.BillTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(new NotificationBuffer()));
			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.VoyageNo = "325623562";
			sailing.LloydsNo = vessel.RV_LloydsNumber;
			sailing.VesselName = "BLAH";
			sailing.ETD = new ZDateTime(year, 06, 07);
			sailing.ETA = new ZDateTime(year, 06, 08);
			cartageValue.SailingInfo.Item = sailing;
			cartageValue.SailingInfo.PortOfLoading = "NLAMS";
			cartageValue.SailingInfo.PortOfDischarge = "AUBNE";
			var cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartageValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(vessel.RV_Name, cartageJob.SailingStandalone.Vessel.RV_Name);
		}

		public void TestImportVessel_MatchByVesselNameWhenLloydsNumberInvalidOrEmpty()
		{
			var year = ZDateTime.Now.Year;
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var cartageValue = new Xsd.CartageJob();
			cartageValue.BillTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(new NotificationBuffer()));
			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.VoyageNo = "325623562";
			sailing.LloydsNo = existingVessel.RV_LloydsNumber;
			sailing.VesselName = existingVessel.RV_Name;
			sailing.LloydsNo = "9999999";
			sailing.ETD = new ZDateTime(year, 06, 07);
			sailing.ETA = new ZDateTime(year, 06, 08);
			cartageValue.SailingInfo.Item = sailing;
			cartageValue.SailingInfo.PortOfLoading = "NLAMS";
			cartageValue.SailingInfo.PortOfDischarge = "AUBNE";
			var cartageJob = new CommonCartageBookingValueObjectDataAdapter().CreateOrUpdateFromValueObject(cartageValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals(existingVessel.RV_Name, cartageJob.SailingStandalone.Vessel.RV_Name);
		}

		public void TestImportContainer()
		{
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			Xsd.CartageJob cartageValue = GetXsdCartage();
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			AssertContainerInfo(cartage, cartage.Containers.ElementAt(0));
		}

		void AssertContainerInfo(CommonCartage cartage, CommonContainer container)
		{
			AssertEquals("ASK", cartage.JJ_DropMode);
			AssertEquals("JC_ContainerNum", "CONTAINER1", container.JC_ContainerNum);
			AssertEquals("JC_AdditionalSealNum", "123", container.JC_SealNum);
			AssertEquals("JC_ContainerMode", Core.Constants.ContainerModes.LCL, container.JC_ContainerMode);
			AssertEquals("JC_LCLAvailable", CurrentDate, container.JC_LCLAvailable);
			AssertEquals("JC_FCLAvailable", CurrentDate, container.JC_FCLAvailable);
			AssertEquals("JC_AirVentFlow", 6m, container.JC_AirVentFlow);
			AssertEquals("JC_AirVentFlowRateUnit", "A", container.JC_AirVentFlowRateUnit);
			AssertEquals("JC_HumidityPercent", (ZByte)7, container.JC_HumidityPercent);
			AssertEquals("JC_SetPointTemp", 8m, container.JC_SetPointTemp);
			AssertEquals("JC_SetPointTempUnit", "C", container.JC_SetPointTempUnit);
			AssertEquals("JC_AdditionalSealNum", "456", container.JC_AdditionalSealNum);
			AssertEquals("JC_GrossWeight", 5500.0m, container.JC_GrossWeight);
			AssertEquals("JC_GrossWeightUQ", Core.Constants.Weight.Kilograms, container.JC_GrossWeightUQ);
			AssertEquals("JC_TareWeight", 2280.0m, container.JC_TareWeight);
			AssertEquals("JC_TempRecorderSerialNo:", "1000", container.JC_TempRecorderSerialNo);
			AssertEquals("JC_ReleaseNum:", "Release", container.JC_ReleaseNum);
			AssertEquals("JC_EmptyRequired", CurrentDate.AddDays(3), container.JC_EmptyRequired);
			AssertEquals("JC_PackDate", CurrentDate.AddDays(1), container.JC_PackDate);
			AssertEquals("JC_DepartureSlotDateTime", CurrentDate.AddDays(-1), container.JC_DepartureSlotDateTime);
			AssertEquals("JC_DepartureSlotReference", "DEPSLOT REF", container.JC_DepartureSlotReference);
			AssertEquals("JC_ArrivalSlotDateTime", CurrentDate, container.JC_ArrivalSlotDateTime);
			AssertEquals("JC_ArrivalSlotReference:", "ARRSLOT REF", container.JC_ArrivalSlotReference);
			AssertEquals("JC_LCLStorageCommences", CurrentDate.AddDays(1), container.JC_LCLStorageCommences);
			AssertEquals("JC_ArrivalCTOStorageStartDate", CurrentDate.AddDays(1), container.JC_ArrivalCTOStorageStartDate);
			AssertEquals("JC_LCLUnpack", CurrentDate.AddDays(1), container.JC_LCLUnpack);
			AssertEquals("JC_EmptyReturnedBy", CurrentDate.AddDays(3), container.JC_EmptyReturnedBy);
		}

		Xsd.DocAddress GetNewDocAddress(OrgHeader org, INotifications notify)
		{
			Xsd.DocAddress result = new Xsd.DocAddress();
			result.AddressReference.AddressSequenceRef = 1;
			result.AddressReference.Organisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(org, new ValueObjectExportContext(notify));
			return result;
		}

		public void TestUnmatchedOrgNotes()
		{
			var unmatchedOrgRegistryItem = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			unmatchedOrgRegistryItem.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrgRegistryItem);
			var cartageValue = new Xsd.CartageJob();
			cartageValue.JobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartageValue.GoodsDescription = "goods description";
			cartageValue.ClientJobReference = "S00001000";
			var notify = new NotificationBuffer();
			var billTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notify));
			cartageValue.BillTo = billTo;
			cartageValue.DropMode = "ASK";
			var xsdLeg1 = cartageValue.CartageLegs.AddNew();
			xsdLeg1.Pickup.DocAddress = GetNewXsdDocAddress("CTO", "NameCTO", "123 CTO st", "CTO City", Xsd.DocAddressAddressType.LCT); // CTO
			xsdLeg1.Delivery.DocAddress = GetNewXsdDocAddress("CNE", "NameConsignee", "123 CNE st", "CNE City", Xsd.DocAddressAddressType.LCI); // Consignee
			var xsdContainer1 = new Xsd.CartageLegContainer();
			xsdContainer1.ContainerNumber = "Container1";
			xsdContainer1.ContainerType.ISOCode = "22G0";
			xsdContainer1.Seal = "123";
			xsdLeg1.Item = xsdContainer1;
			var xsdLeg2 = cartageValue.CartageLegs.AddNew();
			xsdLeg2.Pickup.DocAddress = GetNewXsdDocAddress("CNR", "NameConsignor", "123 CNR st", "CNR City", Xsd.DocAddressAddressType.LCE); // Consignor
			xsdLeg2.Delivery.DocAddress = GetNewXsdDocAddress("CFS", "NameCFS", "123 CFS st", "CFS City", Xsd.DocAddressAddressType.LCF); // CFS
			var xsdContainer2 = new Xsd.CartageLegContainer();
			xsdContainer2.ContainerNumber = "Container2";
			xsdContainer2.ContainerType.ISOCode = "22G0";
			xsdContainer2.Seal = "456";
			xsdLeg2.Item = xsdContainer2;
			var xsdLeg3 = cartageValue.CartageLegs.AddNew();
			xsdLeg3.Pickup.DocAddress = GetNewXsdDocAddress("CNR", "NameConsignor", "123 CNR st", "CNR City", Xsd.DocAddressAddressType.LCE); // Consignor
			xsdLeg3.Delivery.DocAddress = GetNewXsdDocAddress("CYD", "NameContainerYard", "123 CYD st", "CYD City", Xsd.DocAddressAddressType.LCY); // Container Yard
			var xsdContainer3 = new Xsd.CartageLegContainer();
			xsdContainer3.ContainerNumber = "Container3";
			xsdContainer3.ContainerType.ISOCode = "22G0";
			xsdContainer3.Seal = "789";
			xsdLeg3.Item = xsdContainer3;
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			var adapter = new CommonCartageBookingValueObjectDataAdapter();
			var cartage = Factory.New<CommonCartage>();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			var ctoAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO);
			var cneAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			var cydAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageYard);
			var cnrAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			var cfsAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
			AssertEquals(unmatchedOrgRegistryItem.Organisation, ctoAddress.OrganisationPK);
			AssertEquals(unmatchedOrgRegistryItem.Organisation, cneAddress.OrganisationPK);
			AssertEquals(unmatchedOrgRegistryItem.Organisation, cydAddress.OrganisationPK);
			AssertEquals(unmatchedOrgRegistryItem.Organisation, cnrAddress.OrganisationPK);
			AssertEquals(unmatchedOrgRegistryItem.Organisation, cfsAddress.OrganisationPK);
			var unmatchedOrgNotes = cartage.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
			AssertEquals("unmatchedOrgNotes.Length", 1, unmatchedOrgNotes.Length);
			var unmatchedOrgNote = unmatchedOrgNotes[0];
			AssertMultilineASCIIEquals("Unmatched Orgs Note Content", @" 
Organisation Type: CTO
Owner Code: CTO
EDI Code: 
Organisation Name: NameCTO
Address Line 1: 123 CTO st
Address Line 2: 
City: CTO City
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: Consignee
Owner Code: CNE
EDI Code: 
Organisation Name: NameConsignee
Address Line 1: 123 CNE st
Address Line 2: 
City: CNE City
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: Consignor
Owner Code: CNR
EDI Code: 
Organisation Name: NameConsignor
Address Line 1: 123 CNR st
Address Line 2: 
City: CNR City
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: CFS
Owner Code: CFS
EDI Code: 
Organisation Name: NameCFS
Address Line 1: 123 CFS st
Address Line 2: 
City: CFS City
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: CYD
Owner Code: CYD
EDI Code: 
Organisation Name: NameContainerYard
Address Line 1: 123 CYD st
Address Line 2: 
City: CYD City
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 ".TrimStart(), unmatchedOrgNote.ST_NoteText);
		}

		public void TestImportCartageAddresses_Empty()
		{
			Xsd.CartageJob cartageValue = new Xsd.CartageJob();
			cartageValue.JobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartageValue.GoodsDescription = "goods description";
			cartageValue.ClientJobReference = "S00001000";
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.Organisation billTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notify));
			cartageValue.BillTo = billTo;
			cartageValue.DropMode = "ASK";
			Xsd.CartageLeg xsdLeg1 = cartageValue.CartageLegs.AddNew();
			xsdLeg1.Pickup.DocAddress = GetNewDocAddress(CTO, notify);
			xsdLeg1.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LCT;
			xsdLeg1.Delivery.DocAddress = GetNewDocAddress(Consignee_GB, notify);
			xsdLeg1.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LCI;
			Xsd.CartageLegContainer xsdContainer = new Xsd.CartageLegContainer();
			xsdContainer.ContainerNumber = "Container1";
			xsdContainer.ContainerType.ISOCode = "22G0";
			xsdContainer.Seal = "123";
			xsdLeg1.Item = xsdContainer;
			Xsd.CartageLeg xsdLeg2 = cartageValue.CartageLegs.AddNew();
			xsdLeg2.Pickup.DocAddress = xsdLeg1.Delivery.DocAddress;
			xsdLeg2.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LCI;
			//XsdLeg2.Delivery.DocAddress = NOTHING!
			xsdLeg2.Item = xsdContainer;
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			JobDocAddress ctoAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO);
			JobDocAddress cneAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			JobDocAddress cydAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageYard);
			Assert(!ctoAddress.IsEmpty);
			Assert(!cneAddress.IsEmpty);
			AssertNull("No Address is specified in the XML, so there is no way to know what the address should be.", cydAddress);
		}

		public void TestImportCartageAddresses()
		{
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			Xsd.CartageJob cartageValue = GetXsdCartage();
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			JobDocAddress ctoAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO);
			JobDocAddress cneAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			JobDocAddress cydAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageYard);
			Assert(!ctoAddress.IsEmpty);
			Assert(!cneAddress.IsEmpty);
			Assert(!cydAddress.IsEmpty);
		}

		public void TestImportCartageAddresses_IncludingBooking()
		{
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			Xsd.CartageJob cartageValue = GetXsdCartage();
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			JobDocAddress ctoAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCTO);
			JobDocAddress cneAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			JobDocAddress cydAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageYard);
			Assert(!ctoAddress.IsEmpty);
			Assert(!cneAddress.IsEmpty);
			Assert(!cydAddress.IsEmpty);
			var move = cartage.BookedMovesCollection[0];
			Assert(!move.PickupFromDocAddress.E2_OA_Address.IsEmpty);
			Assert(!move.WaitPointDocAddress.E2_OA_Address.IsEmpty);
			Assert(move.EW_E2DeliveryAddressID.IsEmpty);
		}

		public void TestImportLooseCartage()
		{
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			Xsd.CartageJob cartageValue = GetLooseXsdCartage();
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			AssertEquals(4, cartage.LooseBookedMoves.Count);
			AssertEquals(1, cartage.LooseBookedMoves[0].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[1].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[2].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[3].CartageLegs.Count);
			int packCount = 0;
			packCount += cartage.LooseBookedMoves[0].EW_BookedPackCount;
			packCount += cartage.LooseBookedMoves[1].EW_BookedPackCount;
			packCount += cartage.LooseBookedMoves[2].EW_BookedPackCount;
			packCount += cartage.LooseBookedMoves[3].EW_BookedPackCount;
			AssertEquals(5 + 10 + 15 + 20, packCount);
		}

		public void TestImportLooseCartage_IncludeBooking()
		{
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			Xsd.CartageJob cartageValue = GetLooseXsdCartage();
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = "ABC";
			cartageValue.MessageDescription = ZString.Empty;
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			AssertEquals(4, cartage.LooseBookedMoves.Count);
			AssertEquals(1, cartage.LooseBookedMoves[0].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[1].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[2].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[3].CartageLegs.Count);
			int packCount = 0;
			packCount += cartage.LooseBookedMoves[0].EW_BookedPackCount;
			packCount += cartage.LooseBookedMoves[1].EW_BookedPackCount;
			packCount += cartage.LooseBookedMoves[2].EW_BookedPackCount;
			packCount += cartage.LooseBookedMoves[3].EW_BookedPackCount;
			AssertEquals(5 + 10 + 15 + 20, packCount);
			var move = cartage.LooseBookedMoves[0];
			Assert(!move.PickupFromDocAddress.E2_OA_Address.IsEmpty);
			Assert(!move.WaitPointDocAddress.E2_OA_Address.IsEmpty);
			Assert(move.EW_E2DeliveryAddressID.IsEmpty);
			move = cartage.LooseBookedMoves[1];
			Assert(!move.PickupFromDocAddress.E2_OA_Address.IsEmpty);
			Assert(!move.WaitPointDocAddress.E2_OA_Address.IsEmpty);
			Assert(move.EW_E2DeliveryAddressID.IsEmpty);
			move = cartage.LooseBookedMoves[2];
			Assert(!move.PickupFromDocAddress.E2_OA_Address.IsEmpty);
			Assert(!move.WaitPointDocAddress.E2_OA_Address.IsEmpty);
			Assert(move.EW_E2DeliveryAddressID.IsEmpty);
			move = cartage.LooseBookedMoves[3];
			Assert(!move.PickupFromDocAddress.E2_OA_Address.IsEmpty);
			Assert(!move.WaitPointDocAddress.E2_OA_Address.IsEmpty);
			Assert(move.EW_E2DeliveryAddressID.IsEmpty);
		}

		[TestDate(2011, 10, 11)]
		public override void TestImportCommentsAndBookingStatus()
		{
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			Xsd.CartageJob cartageValue = GetXsdCartage();
			cartageValue.Action = ExpectedBookingAction;
			cartageValue.ActionType = BookingCodes.PreBookingAdvice;
			cartageValue.MessageDescription = "hello";
			var buffer = new NotificationBuffer();
			CommonCartage cartage = Factory.New<CommonCartage>();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			adapter.ImportFromValueObject(cartage, cartageValue, context);
			StmALog statusUpdateLog = cartage.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
			AssertNotNull("STU event", statusUpdateLog);
			AssertEquals("STU event reference", "PBA-Pre Booking Advice (Port Transport) received from TESORGBNE - TEST ORGPROXY, Reference S00001000.", statusUpdateLog.SL_Reference);
			AssertNotNull("DIM event", cartage.Logs.MostRecentLogByEventTime(Events.DataImport, FreightConstants.CargoWiseOnePortTransportXMLFile));
			StmNote[] notes = cartage.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("there should be a note", 1, notes.Length);
			AssertEquals("Note contents", true, notes[0].ST_NoteDataAsText.Contains(@"@11-Oct-11 00:00 - PBA-Pre Booking Advice (Port Transport) received from TESORGBNE - TEST ORGPROXY, Reference S00001000.
Comments: hello"));
			AssertMultilineEquals("", @"
Importing Port Transport from Job S00001000.
Successfully matched organization with code 'TESORGBNE', Mapping Organization: TESORGBNE, Matching by Foreign code: TESORGBNE, Using: Similarity Matcher, Found match: True
Failed to matched organization with code/name 'NEWCTO1' / 'New CTO', Mapping Organization: TESORGBNE, Matching by Foreign code: NEWCTO, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (NEWCTO1) created
Failed to matched organization with code/name 'CONFORLON1' / 'Consignee for test', Mapping Organization: TESORGBNE, Matching by Foreign code: CONFORLON, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (CONFORLON1) created
Failed to matched organization with code/name 'LOCFORSYD1' / 'Local Forwarder', Mapping Organization: TESORGBNE, Matching by Foreign code: LOCFORSYD, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (LOCFORSYD1) created
@11-Oct-11 00:00 - PBA-Pre Booking Advice (Port Transport) received from TESORGBNE - TEST ORGPROXY, Reference S00001000.
Comments: hello
Port Transport  created
", buffer.AsString, '\n');
		}

		[TestDate(2011, 12, 30)]
		public void TestImportButAlreadyAccepted()
		{
			CommonCartage savedCartage = Factory.New<CommonCartage>();
			savedCartage.JJ_OrderReferenceNumber = "CLIENTCARTAGE";
			savedCartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			savedCartage.FirstDocAddress.E2_OA_Address = CTO.MainAddress.PK;
			savedCartage.SecondDocAddress.E2_OA_Address = Consignee_GB.MainAddress.PK;
			savedCartage.ThirdDocAddress.E2_OA_Address = LocalCFS.MainAddress.PK;
			savedCartage.Logs.AddNew(Events.StatusUpdated, BookingCodes.BookingAccepted + "-", ZDateTimeOffset.Now);
			JobHeader job = new JobHeader.Loader(savedCartage).TryLoadOrCreateWithMutex();
			new JobHeader.Loader(savedCartage).TryLoadOrCreateWithMutex();
			Factory.Save();
			savedCartage.LocalClientAddressPK = OrgProxy.MainAddress.PK;
			Factory.Save();
			job.Dispose();
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			xsdCartage.ClientJobReference = "CLIENTCARTAGE";
			xsdCartage.MessageDescription = "modify";
			xsdCartage.ActionType = BookingCodes.PreBookingAdvice;
			NotificationBuffer notifications = new NotificationBuffer();
			xsdCartage.BillTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notifications));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			CommonCartage foundCartage = adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			AssertNotNull(foundCartage);
			AssertEquals("Found Port Transport", savedCartage.PK, foundCartage.PK);
			AssertMultilineEquals("", @"Successfully matched organization with code 'TESORGBNE', Mapping Organization: TESORGBNE, Matching by Foreign code: TESORGBNE, Using: Similarity Matcher, Found match: True
Error: ERROR MESSAGE: (REJECTED. The Port Transport Job T00001000 has already been confirmed and cannot be updated.)
", notifications.AsString, '\n');
			StmNote[] notes = foundCartage.Notes.FindByDescription(FreightConstants.LocalCartageNote);
			AssertEquals("there should be a note", 1, notes.Length);
			AssertMultilineEquals("Note contents", @"@30-Dec-11 00:00 - PBA-Pre Booking Advice (Port Transport) received from TESORGBNE - TEST ORGPROXY, Reference CLIENTCARTAGE.
Comments: modify
Error: ERROR MESSAGE: (REJECTED. The Port Transport Job T00001000 has already been confirmed and cannot be updated.)", notes[0].ST_NoteDataAsText, '\n');
		}

		public void TestImportModes_ExportSeaContainerised()
		{
			var year = ZDateTime.Now.Year;
			var notify = new NotificationBuffer();
			var xsdCartage = new Xsd.CartageJob();
			xsdCartage.ClientJobReference = "S00001000";
			var billTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notify));
			xsdCartage.BillTo = billTo;
			var xsdLeg1 = xsdCartage.CartageLegs.AddNew();
			xsdLeg1.Pickup.DocAddress = GetNewDocAddress(CTO, notify);
			xsdLeg1.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LCE; // Exporter
			xsdLeg1.Delivery.DocAddress = GetNewDocAddress(Consignee_GB, notify);
			xsdLeg1.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LCT; // CTO
			var xsdContainer = new Xsd.CartageLegContainer();
			xsdContainer.ContainerNumber = "Container1";
			xsdContainer.ContainerType.ISOCode = "22G0";
			xsdContainer.Seal = "123";
			xsdLeg1.Item = xsdContainer;
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var sailing = new Xsd.SailingWithVesselVoyage();
			sailing.VoyageNo = "325623562";
			sailing.LloydsNo = existingVessel.RV_LloydsNumber;
			sailing.VesselName = "BLAH";
			sailing.ETD = new ZDateTime(year, 06, 07);
			sailing.ETA = new ZDateTime(year, 06, 08);
			xsdCartage.SailingInfo.Item = sailing;
			xsdCartage.SailingInfo.PortOfLoading = "NLAMS";
			xsdCartage.SailingInfo.PortOfDischarge = "AUBNE";
			xsdCartage.Action = ExpectedBookingAction;
			xsdCartage.ActionType = "ABC";
			xsdCartage.MessageDescription = ZString.Empty;
			var adapter = new CommonCartageBookingValueObjectDataAdapter();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, xsdCartage, context);
			AssertEquals(Constants.TransportModes.Sea, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			AssertEquals(Constants.CartageDirection.Export, cartage.JJ_Direction);
		}

		public void TestImportModes_ImportAirLoose()
		{
			var year = ZDateTime.Now.Year;
			var notify = new NotificationBuffer();
			var xsdCartage = new Xsd.CartageJob();
			xsdCartage.ClientJobReference = "S00001000";
			var billTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notify));
			xsdCartage.BillTo = billTo;
			var xsdLeg1 = xsdCartage.CartageLegs.AddNew();
			xsdLeg1.Pickup.DocAddress = GetNewDocAddress(CTO, notify);
			xsdLeg1.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LCF; // CFS
			xsdLeg1.Delivery.DocAddress = GetNewDocAddress(Consignee_GB, notify);
			xsdLeg1.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LCI; // Importer
			var xsdPackage1 = new Xsd.Package();
			xsdPackage1.NumberOfPacks = 5;
			xsdPackage1.PackType = "PKG";
			var xsdLoose = new Xsd.CartageLegPackageRecords();
			xsdLoose.Packs.Add(xsdPackage1);
			xsdLeg1.Item = xsdLoose;
			var existingVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var schedule = new Xsd.FlightWithFlightNumber();
			schedule.FlightNoJourneyNoTruckRegNo = "325623562";
			schedule.ETD = new ZDateTime(year, 06, 07);
			schedule.ETA = new ZDateTime(year, 06, 08);
			xsdCartage.SailingInfo.Item = schedule;
			xsdCartage.SailingInfo.PortOfLoading = "NLAMS";
			xsdCartage.SailingInfo.PortOfDischarge = "AUBNE";
			xsdCartage.Action = ExpectedBookingAction;
			xsdCartage.ActionType = "ABC";
			xsdCartage.MessageDescription = ZString.Empty;
			var adapter = new CommonCartageBookingValueObjectDataAdapter();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(cartage, xsdCartage, context);
			AssertEquals(Constants.TransportModes.Air, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
			AssertEquals(Constants.CartageDirection.Import, cartage.JJ_Direction);
		}

		Xsd.CartageJob GetXsdCartage()
		{
			Xsd.CartageJob result = new Xsd.CartageJob();
			result.JobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			result.GoodsDescription = "goods description";
			result.ClientJobReference = "S00001000";
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.Organisation billTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notify));
			result.BillTo = billTo;
			result.SailingInfo.PortOfDischarge = "AUSYD";
			result.SailingInfo.PortOfLoading = "GBLON";
			result.DropMode = "ASK";
			Xsd.SailingWithVesselVoyage xsdSailing = new Xsd.SailingWithVesselVoyage();
			xsdSailing.VesselName = "New Vessel";
			xsdSailing.VoyageNo = "123";
			xsdSailing.ETA = CurrentDate.AddDays(2);
			xsdSailing.ETD = CurrentDate;
			Xsd.CartageLeg xsdLeg1 = result.CartageLegs.AddNew();
			xsdLeg1.DangerousGoods.AddNew().UNDGCode = Substance.DG_Code;
			xsdLeg1.Pickup.DocAddress = GetNewDocAddress(CTO, notify);
			xsdLeg1.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LCT;
			xsdLeg1.Delivery.DocAddress = GetNewDocAddress(Consignee_GB, notify);
			xsdLeg1.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LCI;
			Xsd.CartageLegContainer xsdContainer = new Xsd.CartageLegContainer();
			xsdContainer.ContainerNumber = "Container1";
			xsdContainer.ContainerType.ISOCode = "22G0";
			xsdContainer.LCLAvailable = CurrentDate;
			xsdContainer.FCLAvailable = CurrentDate;
			xsdContainer.Seal = "123";
			xsdContainer.AirVentFlow = 6m;
			xsdContainer.AirVentFlowSpecified = true;
			xsdContainer.AirVentFlowRateUnit = "A";
			xsdContainer.HumidityPercent = 7;
			xsdContainer.HumidityPercentSpecified = true;
			xsdContainer.SetPointTemperature = 8m;
			xsdContainer.SetPointTemperatureSpecified = true;
			xsdContainer.SetPointTemperatureUnit = "C";
			xsdContainer.ContainerAdditionalInfo.AdditionalSealNo = "456";
			xsdContainer.ContainerAdditionalInfo.GrossWeight.Value = 5500.0m;
			xsdContainer.ContainerAdditionalInfo.GrossWeight.DimensionType = Core.Constants.Weight.Kilograms;
			xsdContainer.ContainerAdditionalInfo.TareWeight.Value = 2280.0m;
			xsdContainer.ContainerAdditionalInfo.DropMode = "ABC";
			xsdContainer.ContainerAdditionalInfo.TempRecorderNo = "1000";
			xsdContainer.ContainerAdditionalInfo.ReleaseNo = "Release";
			xsdContainer.ContainerAdditionalInfo.EmptyRequiredDate = CurrentDate.AddDays(3);
			xsdContainer.ContainerAdditionalInfo.PackDate = CurrentDate.AddDays(1);
			xsdContainer.ContainerAdditionalInfo.DepartureSlotDate = CurrentDate.AddDays(-1);
			xsdContainer.ContainerAdditionalInfo.DepartureSlotRef = "DEPSLOT REF";
			xsdContainer.ContainerAdditionalInfo.ArrivalSlotDate = CurrentDate;
			xsdContainer.ContainerAdditionalInfo.ArrivalSlotRef = "ARRSLOT REF";
			xsdContainer.ContainerAdditionalInfo.FCLStorageDate = CurrentDate.AddDays(1);
			xsdContainer.ContainerAdditionalInfo.LCLStorageDate = CurrentDate.AddDays(1);
			xsdContainer.ContainerAdditionalInfo.UnpackDate = CurrentDate.AddDays(1);
			xsdContainer.ContainerAdditionalInfo.EmptyReturnedByDate = CurrentDate.AddDays(3);
			xsdLeg1.Item = xsdContainer;
			Xsd.CartageLeg xsdLeg2 = result.CartageLegs.AddNew();
			xsdLeg2.Pickup.DocAddress = xsdLeg1.Delivery.DocAddress;
			xsdLeg2.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LC2;
			xsdLeg2.Delivery.DocAddress = GetNewDocAddress(LocalCFS, notify);
			xsdLeg2.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LC3;
			xsdLeg2.Item = xsdContainer;
			Xsd.NotesNote xsdNote = result.Notes.AddNew();
			xsdNote.NoteType = Xsd.NotesNoteNoteType.DeliveryInstructionsNote;
			xsdNote.NoteData = "TestData";
			return result;
		}

		Xsd.CartageJob GetLooseXsdCartage()
		{
			Xsd.CartageJob result = new Xsd.CartageJob();
			result.JobType = Constants.CartageJobType.NEW_LCLExport;
			result.GoodsDescription = "goods description";
			result.ClientJobReference = "S00001000";
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.Organisation billTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notify));
			result.BillTo = billTo;
			result.SailingInfo.PortOfDischarge = "AUSYD";
			result.SailingInfo.PortOfLoading = "GBLON";
			result.DropMode = "ASK";
			Xsd.SailingWithVesselVoyage xsdSailing = new Xsd.SailingWithVesselVoyage();
			xsdSailing.VesselName = "New Vessel";
			xsdSailing.VoyageNo = "123";
			xsdSailing.ETA = CurrentDate.AddDays(2);
			xsdSailing.ETD = CurrentDate;
			Xsd.CartageLeg xsdLeg1 = result.CartageLegs.AddNew();
			xsdLeg1.MostDangerousGoodsCode = Substance.DG_Code;
			xsdLeg1.Pickup.DocAddress = GetNewDocAddress(CTO, notify);
			xsdLeg1.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LCT;
			xsdLeg1.Delivery.DocAddress = GetNewDocAddress(Consignee_GB, notify);
			xsdLeg1.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LCI;
			Xsd.Package xsdPackage1 = new Xsd.Package();
			xsdPackage1.NumberOfPacks = 5;
			xsdPackage1.PackType = "PKG";
			Xsd.Package xsdPackage2 = new Xsd.Package();
			xsdPackage2.NumberOfPacks = 10;
			xsdPackage2.PackType = "PKG";
			Xsd.CartageLegPackageRecords xsdLoose = new Xsd.CartageLegPackageRecords();
			xsdLoose.Packs.Add(xsdPackage1);
			xsdLoose.Packs.Add(xsdPackage2);
			xsdLeg1.Item = xsdLoose;
			Xsd.CartageLeg xsdLeg2 = result.CartageLegs.AddNew();
			xsdLeg2.Pickup.DocAddress = xsdLeg1.Delivery.DocAddress;
			xsdLeg2.Pickup.DocAddress.AddressType = Xsd.DocAddressAddressType.LC1;
			xsdLeg2.Delivery.DocAddress = GetNewDocAddress(LocalCFS, notify);
			xsdLeg2.Delivery.DocAddress.AddressType = Xsd.DocAddressAddressType.LC2;
			Xsd.Package xsdPackage3 = new Xsd.Package();
			xsdPackage3.NumberOfPacks = 15;
			xsdPackage3.PackType = "PKG";
			Xsd.Package xsdPackage4 = new Xsd.Package();
			xsdPackage4.NumberOfPacks = 20;
			xsdPackage4.PackType = "PKG";
			Xsd.CartageLegPackageRecords xsdLoose2 = new Xsd.CartageLegPackageRecords();
			xsdLoose2.Packs.Add(xsdPackage3);
			xsdLoose2.Packs.Add(xsdPackage4);
			xsdLeg2.Item = xsdLoose2;
			Xsd.NotesNote xsdNote = result.Notes.AddNew();
			xsdNote.NoteType = Xsd.NotesNoteNoteType.DeliveryInstructionsNote;
			xsdNote.NoteData = "TestData";
			return result;
		}

		public void TestFindBusinessObject()
		{
			CommonCartage savedCartage = Factory.New<CommonCartage>();
			savedCartage.JJ_OrderReferenceNumber = "CLIENTCARTAGE";
			savedCartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			savedCartage.FirstDocAddress.E2_OA_Address = CTO.MainAddress.PK;
			savedCartage.SecondDocAddress.E2_OA_Address = Consignee_GB.MainAddress.PK;
			savedCartage.ThirdDocAddress.E2_OA_Address = LocalCFS.MainAddress.PK;
			JobHeader job = new JobHeader.Loader(savedCartage).TryLoadOrCreateWithMutex();
			new JobHeader.Loader(savedCartage).TryLoadOrCreateWithMutex();
			Factory.Save();
			savedCartage.LocalClientAddressPK = OrgProxy.MainAddress.PK;
			Factory.Save();
			job.Dispose();
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			xsdCartage.ClientJobReference = "CLIENTCARTAGE";
			NotificationBuffer notifications = new NotificationBuffer();
			xsdCartage.BillTo = new OrganisationValueObjectDataAdapter().ExportToValueObject(OrgProxy, new ValueObjectExportContext(notifications));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notifications);
			CommonCartageValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			CommonCartage foundCartage = adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			AssertNotNull(foundCartage);
			AssertEquals("Found Port Transport", savedCartage.PK, foundCartage.PK);
			xsdCartage.ClientJobReference = "DIF";
			foundCartage = adapter.CreateOrUpdateFromValueObject(xsdCartage, context);
			AssertNotEquals("Didn't Find Port Transport", savedCartage.PK, foundCartage.PK);
			Assert("Is a new Port Transport", !foundCartage.IsInDatabase);
		}

		protected override ZString ExpectedBookingAction
		{
			get
			{
				return FreightConstants.CartageJobExport;
			}
		}

		protected override ZString ExpectedBookingStatus
		{
			get
			{
				return CommonFreightConstants.LocalCartageBookingStatus.Codes.BookingModificationRequest;
			}
		}

		protected override ZString ExpectedCartageType
		{
			get
			{
				return Constants.CartageJobType.NEW_FCLExportPack;
			}
		}

		protected override ZString ExpectedReference
		{
			get
			{
				return "S00001000";
			}
		}

		protected override void AssertXsdLegs(Xsd.CartageJob xsdCartage)
		{
			AssertEquals("2 Xsd Legs should have been created", 2, xsdCartage.CartageLegs.Count);
			Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs[0];
			AssertEquals("Pickup Org", "Local Forwarder", xsdLeg.Pickup.DocAddress.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("Delivery Org", "Local Forwarder", xsdLeg.Delivery.DocAddress.AddressReference.Organisation.OrganisationDetails.Name);
			AssertEquals("Departure Demurrage", (ZDateTime)CurrentDate.ToTimeSpan(), xsdLeg.CartageLegDates.PickupDemurrage);
			AssertEquals("Arrival Demurrage", (ZDateTime)CurrentDate.ToTimeSpan(), xsdLeg.CartageLegDates.DeliveryDemurrage);
			int noOfLegsWithDGGoods = 0;
			Xsd.CartageLeg legWithDGGoods = null;
			foreach (Xsd.CartageLeg xsdCurrentLeg in xsdCartage.CartageLegs)
			{
				// legacy
				if (xsdCurrentLeg.DangerousGoods.Count > 0 || xsdCurrentLeg.MostDangerousGoodsCodeSpecified)
				{
					legWithDGGoods = xsdCurrentLeg;
					noOfLegsWithDGGoods++;
				}
			}

			AssertNotNull(legWithDGGoods);
			AssertEquals(1, noOfLegsWithDGGoods);
			AssertEquals("DangerousGoodsCode", "SUBSb", legWithDGGoods.DangerousGoods[0].UNDGCode);
			Xsd.CartageLegContainer xsdContainer = xsdLeg.Item as Xsd.CartageLegContainer;
			AssertNotNull("CommonContainer Leg", xsdContainer);
			AssertEquals("ContainerNumber", "CONTAINER1", xsdContainer.ContainerNumber);
			AssertEquals("ISOCode", "22G0", xsdContainer.ContainerType.ISOCode);
			AssertEquals("LCLAvailable", CurrentDate, xsdContainer.LCLAvailable);
			AssertEquals("Seal", "123456", xsdContainer.Seal);
			AssertEquals("AdditionalSealNo", "789", xsdContainer.ContainerAdditionalInfo.AdditionalSealNo);
			AssertEquals("DepartureSlotDate", CurrentDate, xsdContainer.ContainerAdditionalInfo.DepartureSlotDate);
			AssertEquals("DepartureSlotRef", "DepSlotRef", xsdContainer.ContainerAdditionalInfo.DepartureSlotRef);
			AssertEquals("ArrivalSlotDate", CurrentDate.AddDays(1), xsdContainer.ContainerAdditionalInfo.ArrivalSlotDate);
			AssertEquals("ArrivalSlotRef", "ArrSlotRef", xsdContainer.ContainerAdditionalInfo.ArrivalSlotRef);
			AssertEquals("ReleaseNo", "ReleaseNo", xsdContainer.ContainerAdditionalInfo.ReleaseNo);
			AssertEquals("EmptyRequiredDate", CurrentDate, xsdContainer.ContainerAdditionalInfo.EmptyRequiredDate);
			AssertEquals("PackDate", CurrentDate, xsdContainer.ContainerAdditionalInfo.PackDate);
			AssertEquals("LCLStorageDate", CurrentDate, xsdContainer.ContainerAdditionalInfo.LCLStorageDate);
			AssertEquals("LCLAvailable", CurrentDate, xsdContainer.LCLAvailable);
			AssertEquals("TempRecorderNo", "ABC", xsdContainer.ContainerAdditionalInfo.TempRecorderNo);
			AssertEquals("UnpackDate", ZDateTime.Empty, xsdContainer.ContainerAdditionalInfo.UnpackDate);
			AssertEquals("EmptyReturnedByDate", CurrentDate, xsdContainer.ContainerAdditionalInfo.EmptyReturnedByDate);
		}

		protected override void AssertXsdSailing(Xsd.CartageJob xsdCartage)
		{
			Xsd.SailingWithVesselVoyage xsdSailing = xsdCartage.SailingInfo.Item as Xsd.SailingWithVesselVoyage;
			AssertNotNull("SailingWithVesselVoyage should not be null", xsdSailing);
			AssertEquals("ETA", CurrentDate.AddDays(2), xsdSailing.ETA);
			AssertEquals("ETD", CurrentDate, xsdSailing.ETD);
			AssertEquals("Vessel Name", "New Vessel", xsdSailing.VesselName);
			AssertEquals("AvailableDate", CurrentDate.AddDays(1), xsdSailing.FCLDates.AvailableDate);
			AssertEquals("StorageDate", CurrentDate.AddDays(1), xsdSailing.FCLDates.StorageDate);
			AssertEquals("Voyage No", "123", xsdSailing.VoyageNo);
		}

		public override void TestTargetType()
		{
			CommonCartageBookingValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			AssertEquals("TargetType", Xsd.InterchangeInfoTargetType.LocalCartageBooking, adapter.TargetType);
		}

		Xsd.DocAddress GetNewXsdDocAddress(ZString organisationCode, ZString organisationName, ZString address1, ZString city, Xsd.DocAddressAddressType addressType)
		{
			var result = new Xsd.DocAddress();
			result.AddressReference.AddressSequenceRef = 1;
			result.AddressReference.Organisation = GetNewXsdOrganisation(organisationCode, organisationName, address1, city);
			result.AddressReference.IsSpecified = true;
			result.AddressLine1 = address1;
			result.AddressType = addressType;
			result.CityOrSuburb = city;
			result.IsSpecified = true;
			return result;
		}

		Xsd.Organisation GetNewXsdOrganisation(ZString code, ZString name, ZString address1, ZString city)
		{
			var result = new Xsd.Organisation();
			result.IsSpecified = true;
			result.OwnerCode = code;
			result.OrganisationDetails = new Xsd.OrganisationDetail();
			result.OrganisationDetails.Name = name;
			result.OrganisationDetails.IsSpecified = true;
			var xsdOrgAddress = result.OrganisationDetails.Addresses.AddNew();
			xsdOrgAddress.CompanyName = name;
			xsdOrgAddress.AddressLine1 = address1;
			xsdOrgAddress.CityOrSuburb = city;
			xsdOrgAddress.IsSpecified = true;
			return result;
		}

		protected override CommonCartage GetJobCartage()
		{
			CommonCartage result = GetSampleJobCartage(Constants.TransportModes.Sea);
			CommonCartageLeg leg = result.GetBookedMoves(result.Containers.ElementAt(0))[0].CartageLegs[0];
			leg.JU_CartageDeliveryDemurrage = CurrentDate;
			leg.JU_CartagePickupDemurrage = CurrentDate;
			result.JJ_OrderReferenceNumber = "DontUse";
			result.JJ_Direction = "IMP";
			return result;
		}
	}
}
