using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eManifest.Business;
using Enterprise.eManifest.Business.Rating;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Testing.Rating
{
	public class HVLVLineRatingAdapterTest : TestCaseWithFactory
	{
		public void TestJobDatesProvider()
		{
			var shipment = Factory.New<CommonShipment>();
			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;
			var autoRating = new HVLVLineRatingAdapter(supplierBookingLine);

			AssertType<CommonShipmentJobDatesProvider>(autoRating.JobDatesProvider);
		}

		public void TestHVLVLineRatingAdapterRaiseExceptionWhenBookingLineIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new HVLVLineRatingAdapter(null));
		}

		public void TestChargeCodeGroups()
		{
			var bookingLine = Factory.New<SupplierBookingLine>();
			var shipment = Factory.New<CommonShipment>();
			bookingLine.DL_JS_ApprovedShipment = shipment.PK;
			CommonConsol consol = shipment.Consols.AddNew();
			var autoRating = new HVLVLineRatingAdapter(bookingLine);

			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.CostChargesFilter);

			consol.TopLevelShipments.AddNew();
			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.SellChargesFilter);

			var cost = (BusinessObject)Factory.New<IJobConsolCost>();
			cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				cost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				cost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			cost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestConsumerType()
		{
			var adapter = new HVLVLineRatingAdapter(Factory.New<SupplierBookingLine>());
			AssertEquals(JobInvoicingConsumerTypes.eManifest, adapter.ConsumerType);
		}

		public void TestMergeCharges()
		{
			var adapter = new HVLVLineRatingAdapter(Factory.New<SupplierBookingLine>());
			AssertEquals(MergeChargeOptions.HLSMerge, adapter.MergeCharges);
		}

		public void TestRateTypeToUse()
		{
			var adapter = new HVLVLineRatingAdapter(Factory.New<SupplierBookingLine>());
			AssertEquals(RateType.Forwarding, adapter.RateTypeToUse);
		}

		public void TestStatusInformation()
		{
			var adapter = new HVLVLineRatingAdapter(Factory.New<SupplierBookingLine>());
			var statusInformation = adapter.StatusInformation;
			AssertEquals(true, statusInformation.CanExecute);
		}

		public void TestDestinationFromShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKDestination = "DEFRA";

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Shipments.Add(shipment);

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals("DEFRA", adapter.Destination.Code);
		}

		public void TestDestinationFromConsol()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Shipments.Add(shipment);

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals("USLAX", adapter.Destination.Code);
		}

		public void TestOriginFromShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "DEFRA";

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.Shipments.Add(shipment);

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals("DEFRA", adapter.Origin.Code);
		}

		public void TestOriginFromConsol()
		{
			var shipment = Factory.New<CommonShipment>();
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.Shipments.Add(shipment);

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals("USLAX", adapter.Origin.Code);
		}

		public void TestCarrier()
		{
			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertNull(adapter.Carrier);
		}

		public void TestConsignee()
		{
			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "Code";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = organization.PK;

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			Assert(shipment.ConsigneeDocumentaryAddress.HasRealOrganisation);
			AssertNotNull(adapter.Consignee);
			AssertEquals("Code", adapter.Consignee.OH_Code);
		}

		public void TestConsigneeDeliveryAddress()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "address1";

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = orgAddress.PK;

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertNotNull(adapter.DeliveryAddress);
			AssertEquals("address1", adapter.DeliveryAddress.E2_Address1);
		}

		public void TestConsigneeCartageEquipment()
		{
			var shipment = Factory.New<CommonShipment>();
			var docAndCartageAddress = shipment.DocsAndCartage;
			docAndCartageAddress.JP_FCLDeliveryEquipmentNeeded = "aaa";

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertNotNull(adapter.DeliveryAddress);
			AssertEquals("aaa", adapter.DeliveryCartageEquipment);
		}

		public void TestConsignor()
		{
			var organization1 = Factory.New<OrgHeader>();
			organization1.OH_Code = "Code1";

			var organization2 = Factory.New<OrgHeader>();
			organization2.OH_Code = "Code2";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = organization2.PK;

			var supplierBookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			supplierBookingHeader.DH_OA_Consignor = organization1.MainAddress.PK;

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;
			supplierBookingLine.DL_DH_BookingHeader = supplierBookingHeader.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertNotNull(adapter.Consignor);
			AssertEquals("Code1", adapter.Consignor.OH_Code);

			supplierBookingHeader.DH_OA_Consignor = ZGuid.Empty;
			AssertNotNull(adapter.Consignor);
			AssertEquals("Code2", adapter.Consignor.OH_Code);
		}

		public void TestConsignorPickupAddressFromShipment()
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "address1";

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = orgAddress.PK;

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertNotNull(adapter.DeliveryAddress);
			AssertEquals("address1", adapter.DeliveryAddress.E2_Address1);
		}

		public void TestConsignorCartageEquipment()
		{
			var shipment = Factory.New<CommonShipment>();
			var docAndCartageAddress = shipment.DocsAndCartage;
			docAndCartageAddress.JP_FCLPickupEquipmentNeeded = "aaa";

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertNotNull(adapter.PickupCartageEquipment);
			AssertEquals("aaa", adapter.PickupCartageEquipment);
		}

		public void TestFreightMode()
		{
			var shipment = Factory.New<CommonShipment>();
			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;
			var autoRating = new HVLVLineRatingAdapter(supplierBookingLine);
			FreightMode containerMask = FreightMode.NonContainerised | FreightMode.Containerised;

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Sea FCL is reported as an FCL job", FreightMode.FCL, autoRating.FreightMode);

			shipment.JS_PackingMode = Constants.ContainerModes.Liquid;
			AssertEquals("Liquid should show as NonContainerised", FreightMode.NonContainerised, autoRating.FreightMode & containerMask);

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			AssertEquals("First Air then Sea, Loose, is reported as an AIR Loose job", FreightMode.LSE, autoRating.FreightMode);

			shipment.JS_PackingMode = Constants.ContainerModes.ULD;
			AssertEquals("First Air then Sea, ULD, is reported as an AIR ULD job", FreightMode.ULD, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.SeaAir;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("First Sea then Air LCL is reported as an LCL job", FreightMode.LCL, autoRating.FreightMode);

			shipment.JS_PackingMode = Constants.ContainerModes.LTL;
			AssertEquals("First Sea then Air LTL is reported as an LCL job", FreightMode.LCL, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_PackingMode = Constants.ContainerModes.FTL;
			AssertEquals("Road FTL is reported as an FTL job", FreightMode.FTL, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("Rail LCL is reported as an LRA job", FreightMode.LRA, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Rail FCL is reported as an FRA job", FreightMode.FRA, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals("Road LCL is reported as an LRO job", FreightMode.LRO, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Road FCL is reported as an FRO job", FreightMode.FRO, autoRating.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Courier;
			AssertEquals("Courier is reported as an OBC job", FreightMode.OBC, autoRating.FreightMode);
		}

		public void TestHousebillReleaseType()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ReleaseType = "rel";
			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals("rel", adapter.HousebillReleaseType);
		}

		public void TestPaymentTerm()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_INCO = Constants.IncoTerms.ExWorks;

			var bookingLine = Factory.New<SupplierBookingLine>();
			bookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(bookingLine);

			AssertPaymentTerm(adapter, PaymentTermType.Incoterm, CostSell.Revenue, Constants.IncoTerms.ExWorks);

			shipment.IsDomesticFreight = true;
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;

			AssertPaymentTerm(adapter, PaymentTermType.DomesticPaymentTerm, CostSell.Revenue, Constants.DomesticPaymentTerms.Prepaid);
		}

		void AssertPaymentTerm(HVLVLineRatingAdapter adapter, PaymentTermType paymentTermType, CostSell costOrSell, string value)
		{
			var info = adapter.PaymentTerm.GetPaymentTermInfo(costOrSell);

			AssertNotNull(info);
			AssertEquals(paymentTermType, info.InfoType);
			AssertEquals(value, info.Value);
		}

		public void TestMeasures()
		{
			var shipment = Factory.New<CommonShipment>();

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_Cubic = 11;
			supplierBookingLine.DL_CubicUQ = Constants.Volume.CubicMetres;
			supplierBookingLine.DL_GrossWeight = 22;
			supplierBookingLine.DL_GrossWeightUQ = Constants.Weight.Kilograms;
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;
			supplierBookingLine.DL_PiecesManifested = 10;
			supplierBookingLine.DL_F3_NKPackType = Constants.PkgUnit.Bag;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);

			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals(4, rateableMeasures.MeasureTypeCount);

			AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals(22m, rateableMeasures.GetActual(MeasureType.Weight));

			AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
			AssertEquals(11m, rateableMeasures.GetActual(MeasureType.Volume));

			var containerMeasure = (IRateableContainer)rateableMeasures.GetPartList(MeasureType.Unit).Single();
			AssertEquals(nameof(IRateableContainer.PackageType), Constants.PkgUnit.Bag, containerMeasure.PackageType);
			AssertEquals(nameof(IRateableContainer.ContainerWeightInKG), 22m, containerMeasure.ContainerWeightInKG);
			AssertEquals(nameof(IRateableContainer.ContainerVolumeInM3), 11m, containerMeasure.ContainerVolumeInM3);
			AssertEquals(nameof(IRateableContainer.ContainerCount), 10, containerMeasure.ContainerCount);

			AssertEquals(true, rateableMeasures.PackageCountHasContainerType);
			AssertEquals(10m, rateableMeasures.GetActual(MeasureType.Package));
		}

		public void TestServiceLevel()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RS_NKServiceLevel = "ABC";

			var bookingLine = Factory.New<SupplierBookingLine>();
			bookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(bookingLine);

			AssertEquals("ABC", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));

			bookingLine.DL_RS_NKServiceLevel = "LLL";

			AssertEquals("LLL", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
		}

		public void TestWharfCTOAddress()
		{
			var shipment = Factory.New<CommonShipment>();
			var originConsol = shipment.Consols.AddNew();
			var destinationConsol = shipment.Consols.AddNew();

			var bookingLine = Factory.New<SupplierBookingLine>();
			bookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var autoRating = new HVLVLineRatingAdapter(bookingLine);

			originConsol.JK_OA_DepartureCTOAddress = Factory.New<OrgAddress>().PK;
			originConsol.JK_OA_PackDepotAddress = Factory.New<OrgAddress>().PK;
			destinationConsol.JK_OA_ArrivalCTOAddress = Factory.New<OrgAddress>().PK;
			destinationConsol.JK_OA_UnpackDepotAddress = Factory.New<OrgAddress>().PK;

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			originConsol.JK_RL_NKLoadPort = "USLAX";
			originConsol.JK_RL_NKDischargePort = "NZAKL";
			destinationConsol.JK_RL_NKLoadPort = "NZAKL";
			destinationConsol.JK_RL_NKDischargePort = "AUSYD";

			GlbCompany.CurrentCompany.SetCountry("AU");
			Assert(autoRating.IsImport());

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			originConsol.JK_TransportMode = Constants.TransportModes.Sea;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(destinationConsol.JK_OA_ArrivalCTOAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			originConsol.JK_TransportMode = Constants.TransportModes.Rail;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(destinationConsol.JK_OA_ArrivalCTOAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			originConsol.JK_TransportMode = Constants.TransportModes.Air;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(destinationConsol.JK_OA_ArrivalCTOAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			originConsol.JK_TransportMode = Constants.TransportModes.Road;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(destinationConsol.JK_OA_UnpackDepotAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_OA_ImportReleaseDepot = Factory.New<OrgAddress>().PK;
			AssertEquals(shipment.JS_OA_ImportReleaseDepot, autoRating.WharfCTOAddress.PK);

			GlbCompany.CurrentCompany.SetCountry("US");
			Assert(!autoRating.IsImport());

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			originConsol.JK_TransportMode = Constants.TransportModes.Sea;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(originConsol.JK_OA_DepartureCTOAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			originConsol.JK_TransportMode = Constants.TransportModes.Rail;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(originConsol.JK_OA_DepartureCTOAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			originConsol.JK_TransportMode = Constants.TransportModes.Air;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(originConsol.JK_OA_DepartureCTOAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			originConsol.JK_TransportMode = Constants.TransportModes.Road;
			destinationConsol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(originConsol.JK_OA_PackDepotAddress, autoRating.WharfCTOAddress.PK);

			shipment.JS_OA_ExportReceivingDepot = Factory.New<OrgAddress>().PK;
			AssertEquals(shipment.JS_OA_ExportReceivingDepot, autoRating.WharfCTOAddress.PK);
		}

		public void TestJobDirection()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals(Directions.Export, adapter.JobDirection);
		}

		public void TestViaGetterLoadViaIsSpecifiedReturnLoadVia()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "UAIEV";

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapterToTest = new HVLVLineRatingAdapter(supplierBookingLine);

			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		public void TestAdapterTypeAndID()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";

			var supplierBookingLine = Factory.New<SupplierBookingLine>();
			supplierBookingLine.DL_JS_ApprovedShipment = shipment.PK;

			var adapter = new HVLVLineRatingAdapter(supplierBookingLine);
			AssertEquals(AdapterType.HVLVShipment, adapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, adapter.OperationalJobCode);
		}
	}
}
