using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Business.Testing
{
	public class ShipmentRatingSupportTest : BaseFreightTest
	{
		#region ISpotRate

		public void TestSellSpotRateInfo()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UnitFreightRate = 20m;
			shipment.JS_RX_NKFrtRateCurrency = "USD";
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);

			AssertEquals(Money.Invalid, adapter.SellSpotRateInfo.Rate);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.StandardRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.ClientRate, adapter.SellSpotRateInfo.AutoratedValueType);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;

			AssertEquals(Money.Invalid, adapter.SellSpotRateInfo.Rate);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.StandardRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.ClientRate, adapter.SellSpotRateInfo.AutoratedValueType);

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "GGG";
			jobHeader.JH_GE = department.PK;

			AssertEquals(20m, adapter.SellSpotRateInfo.Rate.Amount);
			AssertEquals("USD", adapter.SellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.SpotRate, adapter.SellSpotRateInfo.AutoratedValueType);

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Shipments.Add(shipment);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedAgentPort = Factory.New<OrgAppointedAgentPorts>();
			appointedAgentPort.O5_PortOrCountry = consol.LoadPort.Code;
			appointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPort);

			AssertEquals(25m, adapter.SellSpotRateInfo.Rate.Amount);
			AssertEquals("AUD", adapter.SellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.GatewaySell, adapter.SellSpotRateInfo.AutoratedValueType);

			department.GE_Code = "CCC";

			AssertEquals(20m, adapter.SellSpotRateInfo.Rate.Amount);
			AssertEquals("USD", adapter.SellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.SpotRate, adapter.SellSpotRateInfo.AutoratedValueType);
		}

		public void TestCostSpotRateInfo()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UnitFreightRate = 20m;
			shipment.JS_RX_NKFrtRateCurrency = "USD";
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_FreightCostRate = 30m;
			shipment.JS_RX_NKFreightCostRateCurrency = "GBP";
			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);

			AssertEquals(Money.Invalid, adapter.CostSpotRateInfo.Rate);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;

			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.StandardRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.Cost, adapter.CostSpotRateInfo.AutoratedValueType);

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "GGG";
			jobHeader.JH_GE = department.PK;

			AssertEquals(30m, adapter.CostSpotRateInfo.Rate.Amount);
			AssertEquals("GBP", adapter.CostSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.NegotiatedCost, adapter.CostSpotRateInfo.AutoratedValueType);

			department.GE_Code = "CCC";

			AssertEquals(30m, adapter.CostSpotRateInfo.Rate.Amount);
			AssertEquals("GBP", adapter.CostSpotRateInfo.Rate.Currency.Code);

			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.NegotiatedCost, adapter.CostSpotRateInfo.AutoratedValueType);

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Shipments.Add(shipment);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals(30m, adapter.CostSpotRateInfo.Rate.Amount);
			AssertEquals("GBP", adapter.CostSpotRateInfo.Rate.Currency.Code);

			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.NegotiatedCost, adapter.CostSpotRateInfo.AutoratedValueType);

			var appointedAgentPort = Factory.New<OrgAppointedAgentPorts>();
			appointedAgentPort.O5_PortOrCountry = consol.JK_RL_NKLoadPort;
			appointedAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(appointedAgentPort);

			AssertEquals(25m, adapter.CostSpotRateInfo.Rate.Amount);
			AssertEquals("AUD", adapter.CostSpotRateInfo.Rate.Currency.Code);

			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.GatewaySell, adapter.CostSpotRateInfo.AutoratedValueType);
		}

		#endregion

		public void TestDeliveryAddress()
		{
			var documentaryConsignee = Factory.New<OrgHeader>();
			documentaryConsignee.OH_IsConsignee = true;
			var deliveryConsignee = Factory.New<OrgHeader>();
			deliveryConsignee.OH_IsConsignee = true;

			var shipment = Factory.New<CommonShipment>();
			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);

			Assert(shipment.ConsigneeDocumentaryAddress.IsEmpty);
			Assert(shipment.ConsigneeDeliveryAddress.IsEmpty);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = documentaryConsignee.PK;
			AssertEquals("Adapter should take Shipment's consignee address as the delivery address", documentaryConsignee.MainAddress.PK, adapter.DeliveryAddress.E2_OA_Address);

			shipment.ConsigneeDeliveryAddress.OrganisationPK = deliveryConsignee.PK;
			AssertEquals("Shipment's Delivery Address is set, adapter should take it as the delivery address", deliveryConsignee.MainAddress.PK, adapter.DeliveryAddress.E2_OA_Address);
		}

		public void TestPickupAddress()
		{
			var documentaryConsignor = Factory.New<OrgHeader>();
			documentaryConsignor.OH_IsConsignor = true;
			var pickupConsignor = Factory.New<OrgHeader>();
			pickupConsignor.OH_IsConsignor = true;

			var shipment = Factory.New<CommonShipment>();
			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);

			Assert(shipment.ConsignorDocumentaryAddress.IsEmpty);
			Assert(shipment.ConsignorPickupAddress.IsEmpty);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = documentaryConsignor.PK;
			AssertEquals("Adapter should take Shipment's consignor address as the pickup address", documentaryConsignor.MainAddress.PK, adapter.PickupAddress.E2_OA_Address);

			shipment.ConsignorPickupAddress.OrganisationPK = pickupConsignor.PK;
			AssertEquals("Shipment's Pickup Address is set, adapter should take it as the pickup address", pickupConsignor.MainAddress.PK, adapter.PickupAddress.E2_OA_Address);
		}

		public void TestConsignee()
		{
			var miscOrg = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			var documentaryConsignee = Factory.New<OrgHeader>();
			documentaryConsignee.OH_IsConsignee = true;
			var deliveryConsignee = Factory.New<OrgHeader>();
			deliveryConsignee.OH_IsConsignee = true;

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = documentaryConsignee.PK;
			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);
			AssertEquals(documentaryConsignee.PK, adapter.Consignee.PK);

			shipment.ConsigneePK = deliveryConsignee.PK;
			AssertEquals("Delivery consignee is preferred", deliveryConsignee.PK, adapter.Consignee.PK);

			shipment.ConsigneeDeliveryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(deliveryConsignee.PK, adapter.Consignee.PK);

			shipment.ConsigneePK = miscOrg;

			AssertEquals("Misc org cannot be consignee", null, adapter.Consignee);

			shipment.ConsigneePK = ZGuid.Empty;

			AssertEquals("no fall backs", true, shipment.ConsigneePK.IsEmpty);
			AssertNull(adapter.Consignee);
		}

		public void TestConsignor()
		{
			var miscOrg = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			var documentaryConsignor = Factory.New<OrgHeader>();
			documentaryConsignor.OH_IsConsignor = true;
			var pickupConsignor = Factory.New<OrgHeader>();
			pickupConsignor.OH_IsConsignor = true;

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = documentaryConsignor.PK;
			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);
			AssertEquals(documentaryConsignor.PK, adapter.Consignor.PK);

			shipment.ConsignorPK = pickupConsignor.PK;
			AssertEquals("Pickup consignor is prefered", pickupConsignor.PK, adapter.Consignor.PK);

			shipment.ConsignorPickupAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(pickupConsignor.PK, adapter.Consignor.PK);

			shipment.ConsignorPK = miscOrg;

			AssertEquals("Misc org cannot be consignee", null, adapter.Consignor);

			shipment.ConsignorPK = ZGuid.Empty;

			AssertEquals("no fall backs", true, shipment.ConsignorPK.IsEmpty);
			AssertNull(adapter.Consignor);
		}

		public void TestImportBroker()
		{
			var broker = Factory.New<OrgHeader>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OH_ImportBroker = broker.PK;

			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);
			AssertEquals(broker.PK, adapter.ImportBroker.PK);
		}

		public void TestExportBroker()
		{
			var broker = Factory.New<OrgHeader>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OH_ExportBroker = broker.PK;

			var adapter = new ShipmentRatingAdapter<CommonShipment>(shipment);
			AssertEquals(broker.PK, adapter.ExportBroker.PK);
		}

		public void TestJobDatesProvider()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertType<CommonShipmentJobDatesProvider>(shipment.RatingAdapter.JobDatesProvider);
		}

		public virtual void TestAdapterTypeAndID()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertEquals(AdapterType.Shipment, shipment.RatingAdapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, shipment.RatingAdapter.OperationalJobCode);
			AssertEquals(shipment.JS_UniqueConsignRef, shipment.RatingAdapter.JobID);
		}

		public void TestChargeableCommodity()
		{
			var shipment = Factory.New<CommonShipment>();
			var rating = GetAdapter(shipment);

			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();
			var line3 = shipment.OuterPackLines.AddNew();
			var line4 = shipment.OuterPackLines.AddNew();

			line1.JL_RH_NKCommodityCode = "APL";
			line2.JL_RH_NKCommodityCode = "APL";
			line3.JL_RH_NKCommodityCode = "APL";
			line4.JL_RH_NKCommodityCode = "APL";

			AssertEquals("APL", ((RateableMeasureSet)rating.RateableMeasures).GetChargeableCommodity());

			line4.JL_RH_NKCommodityCode = "";

			AssertEquals("", ((RateableMeasureSet)rating.RateableMeasures).GetChargeableCommodity());

			line4.JL_RH_NKCommodityCode = "XXX";

			AssertEquals("", ((RateableMeasureSet)rating.RateableMeasures).GetChargeableCommodity());

			line1.JL_RH_NKCommodityCode = "XXX";
			line2.JL_RH_NKCommodityCode = "XXX";
			line3.JL_RH_NKCommodityCode = "XXX";

			AssertEquals("XXX", ((RateableMeasureSet)rating.RateableMeasures).GetChargeableCommodity());
		}

		public void TestWharfCTOAddress()
		{
			var shipment = Factory.New<CommonShipment>();
			var originConsol = shipment.Consols.AddNew();
			var destinationConsol = shipment.Consols.AddNew();
			var autoRating = GetAdapter(shipment);

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

		public void TestServiceLevel()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();

			consol.JK_AWBServiceLevel = "XYZ";
			shipment.JS_RS_NKServiceLevel = "ABC";

			var adapter = GetAdapter(shipment);

			AssertEquals("ABC", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("XYZ", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			consol.JK_AWBServiceLevel = "";
			AssertEquals("ABC", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("STD", adapter.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_RS_NKServiceLevel = "DEF";
			var adapter2 = GetAdapter(shipment2);

			AssertEquals("DEF", adapter2.ServiceLevel.GetServiceLevel(ServiceLevelType.Client));
			AssertEquals("DEF", adapter2.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier));
		}

		public void TestPaymentTerm()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_INCO = Constants.IncoTerms.ExWorks;

			AssertPaymentTerm(shipment, PaymentTermType.Incoterm, CostSell.Cost, Constants.IncoTerms.ExWorks);
			AssertPaymentTerm(shipment, PaymentTermType.Incoterm, CostSell.Revenue, Constants.IncoTerms.ExWorks);

			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

			AssertPaymentTerm(shipment, PaymentTermType.Incoterm, CostSell.Cost, Constants.IncoTerms.ExWorks);

			shipment.IsDomesticFreight = true;
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;

			AssertPaymentTerm(shipment, PaymentTermType.DomesticPaymentTerm, CostSell.Revenue, Constants.DomesticPaymentTerms.Prepaid);
		}

		void AssertPaymentTerm(CommonShipment shipment, PaymentTermType paymentTermType, CostSell costOrSell, string value)
		{
			var info = GetAdapter(shipment).PaymentTerm.GetPaymentTermInfo(costOrSell);
			AssertNotNull(info);
			AssertEquals(paymentTermType, info.InfoType);
			AssertEquals(value, info.Value);
		}

		public void TestFreightMode()
		{
			CommonShipment shipment = GetShipment();
			var autoRating = GetAdapter(shipment);
			FreightMode containerMask = FreightMode.NonContainerised | FreightMode.Containerised;

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals("Sea FCL is reported as an FCL job", FreightMode.FCL, autoRating.FreightMode);

			shipment.JS_PackingMode = Constants.ContainerModes.Groupage;
			AssertEquals("Sea GRP is reported as an FCL job", FreightMode.FCL, autoRating.FreightMode);

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

		public void TestFreightMode_BreakBulk_Bulk()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var autoRating = GetAdapter(shipment);
			FreightMode containerMask = FreightMode.NonContainerised | FreightMode.Containerised;

			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			AssertEquals("Break Bulk should show as NonContainerised if no containers exist", FreightMode.NonContainerised, autoRating.FreightMode & containerMask);

			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			AssertEquals("Bulk should show as NonContainerised if no containers exist", FreightMode.NonContainerised, autoRating.FreightMode & containerMask);

			PackLine outerPackLine = shipment.OuterPackLines.AddNew();
			CommonContainer container = outerPackLine.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			AssertEquals("Break Bulk should show as Containerised if containers exist", FreightMode.NonContainerised, autoRating.FreightMode & containerMask);

			shipment.JS_PackingMode = Constants.ContainerModes.Bulk;
			AssertEquals("Bulk should show as Containerised if containers exist", FreightMode.NonContainerised, autoRating.FreightMode & containerMask);
		}

		public void TestContainerCountUsedWhenTwoShipments()
		{
			CommonShipment shipment = GetShipment();
			CommonShipment shipment2 = GetShipment();

			// Test Consol with 1 CommonShipment and 3 containers
			CommonConsol consol = shipment.Consols.AddNew();    // 3 total containers
			CommonContainer cont1 = consol.Containers.AddNew();
			cont1.JC_RC = Factory.New(typeof(RefContainer)).PK;
			CommonContainer cont2 = consol.Containers.AddNew();
			cont2.JC_RC = Factory.New(typeof(RefContainer)).PK;
			CommonContainer cont3 = consol.Containers.AddNew();
			cont3.JC_RC = Factory.New(typeof(RefContainer)).PK;

			shipment2.Consols.Add(consol);

			var adapter = GetAdapter(shipment);

			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("All containers on consol are NOT deemed as part of this Shipment", 0, measures.GetContainerTypePKs().Count());
			AssertEquals("0 total containers", 0, measures.GetAllContainers().Count());
		}

		public void TestGetStatusInformationCoreHandlesInvalidWeightAndVolumeUnits()
		{
			CommonShipment shipment = GetShipment();

			shipment.JS_UnitOfVolume = "KG";
			Assert("AutoRating cannot be executed with invalid volume unit", !GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("Error message should be shown for invalid volume unit", GetAdapter(shipment).StatusInformation.Message.ToString(), "Invalid unit of volume: 'KG'.");
			shipment.JS_UnitOfVolume = "M3";
			Assert("AutoRating can be executed with valid volume unit", GetAdapter(shipment).StatusInformation.CanExecute);

			shipment.JS_UnitOfWeight = "KG";
			Assert("AutoRating can be executed with valid weight unit", GetAdapter(shipment).StatusInformation.CanExecute);
			shipment.JS_UnitOfWeight = "M3";
			Assert("AutoRating cannot be executed with invalid weight unit", !GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("Error message should be shown for invalid weight unit", GetAdapter(shipment).StatusInformation.Message.ToString(), "Invalid unit of weight: 'M3'.");
		}

		public void TestAutoRatingStatusInformation()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			AssertEquals("AutoRating can be executed", true, GetAdapter(shipment).StatusInformation.CanExecute);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			// Test Consol
			AssertEquals("AutoRating can't be executed without a Consol", false, GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("AutoRating error regarding consol", "A Consol must be attached to this Shipment before you can do FCL Autorating.", GetAdapter(shipment).StatusInformation.Message);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.ULD;

			// Test Consol
			AssertEquals("AutoRating can't be executed without a Consol", false, GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("AutoRating error regarding consol", "A Consol must be attached to this Shipment before you can do ULD Autorating.", GetAdapter(shipment).StatusInformation.Message);

			// Test Invalid JS_JS_CoLoadMasterShipment
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Invalid;
			AssertEquals("Please enter a valid value for Master / Lead. ", false, GetAdapter(shipment).StatusInformation.CanExecute);
			shipment.JS_JS_ColoadMasterShipment	= ZGuid.Empty;

			CommonConsol consol = shipment.Consols.AddNew();    // 3 total containers

			AssertEquals("Autorating can be executed as consol exists", true, GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("No error message present", ZString.Empty, GetAdapter(shipment).StatusInformation.Message);

			// Add 3 containers to consol
			CommonContainer cont1 = consol.Containers.AddNew();
			cont1.JC_RC = Factory.New<RefContainer>().PK;
			CommonContainer cont2 = consol.Containers.AddNew();
			cont2.JC_RC = Factory.New<RefContainer>().PK;
			CommonContainer cont3 = consol.Containers.AddNew();
			cont3.JC_RC = Factory.New<RefContainer>().PK;

			shipment.OnlyShipmentInConsol += TestShipment_OnlyShipmentInConsol_YES;

			AssertEquals("Precondition", 0, shipment.OuterPackLines.Count);
			AssertEquals("Autorating can be executed as all conatiners assumed on this Shipment", true, GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("No error message present", ZString.Empty, GetAdapter(shipment).StatusInformation.Message);
			AssertEquals("Event Fired", true, onlyShipmentInConsolEventFired);
			AssertEquals("Containers allocated to this Shipment.", 3, shipment.OuterPackLines.Count);
			shipment.OuterPackLines.RemoveAndDeleteAll();
			onlyShipmentInConsolEventFired = false;

			shipment.OnlyShipmentInConsol -= TestShipment_OnlyShipmentInConsol_YES;
			shipment.OnlyShipmentInConsol += TestShipment_OnlyShipmentInConsol_NO;

			AssertEquals("Autorating can't be executed as all conatiners NOT assumed on this Shipment since event was cancelled", false, GetAdapter(shipment).StatusInformation.CanExecute);
			AssertEquals("Error regarding cancelled event", "You chose to not allocate the unallocated containers to this Shipment. Please allocate them before Autorating.", GetAdapter(shipment).StatusInformation.Message);
			AssertEquals("Event Fired", true, onlyShipmentInConsolEventFired);
			onlyShipmentInConsolEventFired = false;

			// New Shipment in same consol
			CommonShipment shipment2 = GetShipment();
			shipment2.Consols.Add(consol);

			AssertEquals("Autorating can't be executed as all containers aren't allocated", false, GetAdapter(shipment).StatusInformation.CanExecute);
			var message = GetAdapter(shipment).StatusInformation.Message;
			Assert("Autorating error regarding allocation of containers", message.IndexOf("Please allocate them before Autorating.") > -1);
			AssertEquals("Event NOT Fired", false, onlyShipmentInConsolEventFired);
		}

		public void TestContainerTypeForCLMCLAConsols()
		{
			var consolCLM = Factory.NewWithValidTestData<CommonConsol>();
			consolCLM.JK_AgentType = Constants.AgentType.AWBMaster;
			consolCLM.JK_TransportMode = Constants.TransportModes.Air;

			var container = consolCLM.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			var consolCLA = Factory.NewWithValidTestData<CommonConsol>();
			consolCLA.JK_AgentType = Constants.AgentType.AWBCoload;
			consolCLA.JK_TransportMode = Constants.TransportModes.Air;

			consolCLA.JK_JK_MasterConsol = consolCLM.PK;

			var shipment = consolCLA.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			packline.SetContainer(consolCLM, container);
			var adapter = GetAdapter(shipment);

			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Container Type should be present", container.JC_RC, measures.GetContainerTypePKs().First());
		}

		public void TestAutoRatingStatus_IsMasterShipmentRepresentingAllChildShipments()
		{
			CommonShipment shipment1 = GetShipment();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			AssertEquals("Precondition", false, shipment1.IsMasterShipmentRepresentingAllChildShipments);

			shipment1.CoLoadShipments.AddNew();
			AssertEquals("IsMasterShipmentRepresentingAllChildShipments", true, shipment1.IsMasterShipmentRepresentingAllChildShipments);
			AssertEquals("AutoRating error regarding consol", "A Consol must be attached to this Shipment before you can do FCL Autorating.", GetAdapter(shipment1).StatusInformation.Message);

			CommonConsol consol1 = shipment1.Consols.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";
			container1.JC_RC = Factory.New<RefContainer>().PK;

			AssertEquals("can't be executed as all containers aren't allocated", false, GetAdapter(shipment1).StatusInformation.CanExecute);
			var message = GetAdapter(shipment1).StatusInformation.Message;
			Assert("Autorating error regarding allocation of containers", message.IndexOf("Please allocate them before Autorating.") > 0);

			CommonShipment shipment2 = GetShipment();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			AssertEquals("Precondition", false, shipment2.IsMasterShipmentRepresentingAllChildShipments);

			CommonConsol consol2 = shipment2.Consols.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB2222222";
			container2.JC_RC = Factory.New<RefContainer>().PK;

			AssertEquals("AutoRating can be executed", true, GetAdapter(shipment2).StatusInformation.CanExecute);
			AssertEquals("No warnings", ZString.Empty, GetAdapter(shipment2).StatusInformation.Message);
		}

		public void TestAutoRatingTime()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "###";

			CommonShipment shipment = GetShipment();
			var autoRating = GetAdapter(shipment);

			shipment.DocsAndCartage.JP_PickupTruckWaitTime = new ZDateTime(2006, 1, 1, 1, 40, 0);
			shipment.DocsAndCartage.JP_PickupLabourTime = new ZDateTime(2006, 1, 1, 2, 15, 0);
			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 3, 20, 0);
			shipment.DocsAndCartage.JP_DeliveryLabourTime = new ZDateTime(2006, 1, 1, 4, 45, 0);
			shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 5;

			chargeCode.AC_ChargeGroup = "FRT";
			AssertEquals("Time for not origin/destination", 0d, autoRating.JobServices.Time(chargeCode)?.Span.TotalHours ?? 0);

			chargeCode.AC_ChargeGroup = "ORG";
			AssertEquals("Time for empty subgroup", 0d, autoRating.JobServices.Time(chargeCode)?.Span.TotalHours ?? 0);
			chargeCode.AC_ChargeSubGroup = "DME";
			AssertEquals("Origin detention (hours)", 100d, autoRating.JobServices.Time(chargeCode).Span.TotalMinutes);
			chargeCode.AC_ChargeSubGroup = "LBR";
			AssertEquals("Origin labor (hours)", 135d, autoRating.JobServices.Time(chargeCode).Span.TotalMinutes);

			chargeCode.AC_ChargeGroup = "DST";
			chargeCode.AC_ChargeSubGroup = "DME";
			AssertEquals("Destination detention (hours)", 200d, autoRating.JobServices.Time(chargeCode).Span.TotalMinutes);
			chargeCode.AC_ChargeSubGroup = "LBR";
			AssertEquals("Destination labor (hours)", 285d, autoRating.JobServices.Time(chargeCode).Span.TotalMinutes);
			chargeCode.AC_ChargeSubGroup = "DTN";
			AssertEquals("Container detention from shipment (days)", 5d, autoRating.JobServices.Time(chargeCode).Span.TotalDays);

			chargeCode.AC_ChargeSubGroup = "STG";
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 6;
			AssertEquals("AIR storage (hours)", 6d, autoRating.JobServices.Time(chargeCode).Span.TotalHours);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 7;
			AssertEquals("LCL storage (days)", 7d, autoRating.JobServices.Time(chargeCode).Span.TotalDays);
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 8;
			AssertEquals("FCL storage (days)", 8d, autoRating.JobServices.Time(chargeCode).Span.TotalDays);
		}

		public void TestAutoRatingTime_SpecialService()
		{
			var chargeCodeFilter = new ZQuery(
				new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
				new ZQuery(AccChargeCodeSchema.AC_Code, "OFUMI"));

			var chargeCode = Factory.Load<AccChargeCode>(chargeCodeFilter).FirstOrDefault();

			var shipment = GetShipment();
			var autoRating = GetAdapter(shipment);

			var fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Now;
			fumigation.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			AssertEquals(2d, autoRating.JobServices.Time(chargeCode).Span.TotalHours);
		}

		public void TestAutoRatingServiceDuration()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			CommonShipment shipment = GetShipment();
			shipment.DocsAndCartage.JP_PickupTruckWaitTime = new ZDateTime(2006, 1, 1, 1, 40, 0);
			shipment.DocsAndCartage.JP_PickupLabourTime = new ZDateTime(2006, 1, 1, 2, 15, 0);
			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 3, 20, 0);
			shipment.DocsAndCartage.JP_DeliveryLabourTime = new ZDateTime(2006, 1, 1, 4, 45, 0);
			shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 8;

			var adapter = GetAdapter(shipment);
			var collection = adapter.JobServices;

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			var services = collection.FindServices(chargeCode);
			AssertEquals("Time for empty subgroup", false, services.Any());

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			services = collection.FindServices(chargeCode);
			AssertEquals("Time for empty subgroup", false, services.Any());

			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			services = collection.FindServices(chargeCode);
			AssertEquals("Origin detention service existence", true, services.Any());
			AssertEquals("Origin detention (hours)", 100d, services.FirstOrDefault().ServiceDuration.TotalMinutes);

			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Labor;
			services = collection.FindServices(chargeCode);
			AssertEquals("Origin labor service existence", true, services.Any());
			AssertEquals("Origin labor (hours)", 135d, services.FirstOrDefault().ServiceDuration.TotalMinutes);

			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.CartageDemurrageTotal;
			services = collection.FindServices(chargeCode);
			AssertEquals("Destination detention service existence", true, services.Any());
			AssertEquals("Destination detention (hours)", 200d, services.FirstOrDefault().ServiceDuration.TotalMinutes);

			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Labor;
			services = collection.FindServices(chargeCode);
			AssertEquals("Destination labor service existence", true, services.Any());
			AssertEquals("Destination labor (hours)", 285d, services.FirstOrDefault().ServiceDuration.TotalMinutes);

			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.ContainerDetention;
			services = collection.FindServices(chargeCode);
			AssertEquals("Container detention service existence", true, services.Any());
			AssertEquals("Container detention (total from containers) (days)", 8d, services.FirstOrDefault().ServiceDuration.TotalDays);

			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 6;
			services = adapter.JobServices.FindServices(chargeCode);
			AssertEquals("AIR storage service existence", true, services.Any());
			AssertEquals("AIR storage (hours)", 6d, services.FirstOrDefault().ServiceDuration.TotalHours);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 7;
			services = adapter.JobServices.FindServices(chargeCode);
			AssertEquals("LCL storage service existence", true, services.Any());
			AssertEquals("LCL storage (days)", 7d, services.FirstOrDefault().ServiceDuration.TotalDays);

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 8;
			services = adapter.JobServices.FindServices(chargeCode);
			AssertEquals("FCL storage service existence", true, services.Any());
			AssertEquals("FCL storage (days)", 8d, services.FirstOrDefault().ServiceDuration.TotalDays);
		}

		public void TestAutoRatingSpecialServiceDuration()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var shipment = GetShipment();
			var autoRating = GetAdapter(shipment);

			var fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Now;
			fumigation.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			var services = autoRating.JobServices.FindServices(chargeCode);
			AssertEquals("Service existence", true, services.Any());
			AssertEquals(2d, services.FirstOrDefault().ServiceDuration.TotalHours);
		}

		[TestDate(2006, 01, 01)]
		public void TestAutoRatingJobServices()
		{
			ZDateTime today = ZDateTime.Today;

			var rateTransportZoneHelper = new Mock<IRateTransportZoneHelper>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(rateTransportZoneHelper.Object))
			{
				rateTransportZoneHelper.Setup(m => m.IsBeyond(It.IsAny<BusinessObjectFactory>(),
					It.IsAny<IOrgHeader>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(false);

				ZString currentBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

				CommonShipment shipment = GetShipment();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_E_DEP = today.AddDays(4);

				var autoRating = GetAdapter(shipment);

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal));
				shipment.DocsAndCartage.JP_PickupTruckWaitTime = new ZDateTime(2006, 1, 1, 1, 0, 0);
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor));
				shipment.DocsAndCartage.JP_PickupLabourTime = new ZDateTime(2006, 1, 1, 2, 0, 0);
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal));
				shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 3, 0, 0);
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Labor));
				shipment.DocsAndCartage.JP_DeliveryLabourTime = new ZDateTime(2006, 1, 1, 4, 0, 0);
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Labor));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention));
				shipment.DocsAndCartage.JP_FCLDeliveryDetentionDays = 5;
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.ContainerDetention));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage));
				shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 6;
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
				fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
				fumigation.ES_Completed = today;
				var fumigationContractor = Factory.New<OrgHeader>();
				fumigation.ES_OH_Contractor = fumigationContractor.PK;
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));
				JobService quarantine = shipment.DocsAndCartage.Services.AddNew();
				quarantine.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));
				quarantine.ES_Completed = today;
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));

				fumigation.ES_OA_Location = fumigationContractor.MainAddress.PK;

				fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "";
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "ISREY";
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";

				fumigation.ES_OA_Location = ZGuid.Empty;

				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.QuarantineInspection));

				fumigation.ES_Completed = today.AddDays(6);
				quarantine.ES_Completed = today.AddDays(6);

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.QuarantineInspection));

				AssertEquals(1, autoRating.JobServices.GetContractors().Count);
				AssertEquals(fumigationContractor.PK, autoRating.JobServices.GetContractors()[0].PK);

				fumigation.ES_OA_Location = fumigationContractor.MainAddress.PK;

				fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "";
				fumigationContractor.MainAddress.OA_RN_NKCountryCode = "";
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "ISREY";
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				fumigationContractor.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, Constants.FreightServiceType.Codes.Fumigation));

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchHomePort;

				var origTransportProvider = Factory.NewWithValidTestData<OrgHeader>();
				var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = origTransportProvider.MainAddress.PK;
				shipment.ConsignorPickupAddress.E2_Postcode = "21064";
				shipment.ConsignorPickupAddress.E2_RN_NKCountryCode = "AU";

				var delTransportProvider = Factory.NewWithValidTestData<OrgHeader>();
				var auckland = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = delTransportProvider.MainAddress.PK;
				shipment.ConsigneeDeliveryAddress.E2_Postcode = "32567";
				shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "NZ";

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageBeyondPostcode));
				rateTransportZoneHelper.VerifyAll();
				rateTransportZoneHelper.Verify(m => m.IsBeyond(Factory,
					origTransportProvider, sydney, "AU", "21064", ""), Times.Exactly(1));

				rateTransportZoneHelper.Setup(m => m.IsBeyond(Factory,
					origTransportProvider, sydney, "AU", "21064", ""))
					.Returns(true);

				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageBeyondPostcode));
				rateTransportZoneHelper.VerifyAll();
				rateTransportZoneHelper.Verify(m => m.IsBeyond(Factory,
					origTransportProvider, sydney, "AU", "21064", ""), Times.Exactly(2));

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageBeyondPostcode));
				rateTransportZoneHelper.VerifyAll();
				rateTransportZoneHelper.Verify(m => m.IsBeyond(Factory, delTransportProvider, auckland, "NZ", "32567", ""), Times.Exactly(3));

				rateTransportZoneHelper.Setup(m => m.IsBeyond(Factory,
					delTransportProvider, auckland, "NZ", "32567", ""))
					.Returns(true);

				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageBeyondPostcode));
				rateTransportZoneHelper.VerifyAll();
				rateTransportZoneHelper.Verify(m => m.IsBeyond(Factory,
					delTransportProvider, auckland, "NZ", "32567", ""), Times.Exactly(4));

				GlbCompany.CurrentCompany.SetCountry("US");
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "CATOR";

				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageBeyondPostcode));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageBeyondPostcode));

				var aciConsignorOriginZone = Factory.New<RefDomesticCartageZone>();
				aciConsignorOriginZone.F1_RL_NKLoco = "USCHI";
				aciConsignorOriginZone.F1_IsBeyond = true;
				aciConsignorOriginZone.F1_CityTownPostCode = "21064";
				aciConsignorOriginZone.F1_PortCode = aciConsignorOriginZone.Loco.RL_IATA;

				var aciConsigneeOriginZone = Factory.New<RefDomesticCartageZone>();
				aciConsigneeOriginZone.F1_RL_NKLoco = "CATOR";
				aciConsigneeOriginZone.F1_IsBeyond = true;
				aciConsigneeOriginZone.F1_CityTownPostCode = "32567";
				aciConsigneeOriginZone.F1_PortCode = "YTO";

				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageBeyondPostcode));
				Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aciConsignorOriginZone.F1_IsBeyond = false;
				aciConsigneeOriginZone.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageBeyondPostcode));
				Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageBeyondPostcode));
			}
		}

		public void TestAutoRatingJobServicesByDomestic()
		{
			CombineAssertions(delegate
			{
				ZDateTime today = ZDateTime.Today;

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "AUCNS";

				IAutoRating autoRating = GetAdapter(shipment);

				var serviceTypes = new[]
						{
							Constants.FreightServiceType.Codes.Cleaning,
							Constants.FreightServiceType.Codes.CustomsHold,
							Constants.FreightServiceType.Codes.ExtraInspection,
							Constants.FreightServiceType.Codes.SteamCleaning,
							Constants.FreightServiceType.Codes.Fumigation,
						};

				var subGroups = new[]
						{
							Constants.FreightServiceType.Codes.Cleaning,
							Constants.FreightServiceType.Codes.CustomsHold,
							Constants.FreightServiceType.Codes.ExtraInspection,
							Constants.FreightServiceType.Codes.SteamCleaning,
							Constants.FreightServiceType.Codes.Fumigation,
						};

				for (int i = 0; i < serviceTypes.Length; i++)
				{
					JobService service = shipment.DocsAndCartage.Services.AddNew();
					service.ES_ServiceCode = serviceTypes[i];
					service.ES_Completed = today.AddDays(i);
				}

				for (int i = 0; i < serviceTypes.Length; i++)
				{
					AssertEquals(ZString.Format("No dates set: Origin-{0}", i), true, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, subGroups[i]));
					AssertEquals(ZString.Format("No dates set: Destination-{0}", i), false, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, subGroups[i]));
				}

				shipment.JS_E_DEP = today.AddDays(1);

				for (int i = 0; i < serviceTypes.Length; i++)
				{
					bool expectedAtOrigin = i <= 1;
					AssertEquals(ZString.Format("Shipment ETD set: Origin-{0}", i), expectedAtOrigin, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, subGroups[i]));
					AssertEquals(ZString.Format("Shipment ETD set: Destination-{0}", i), !expectedAtOrigin, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, subGroups[i]));
				}

				Transport transport = shipment.Transports.AddNew();
				transport.JW_ETD = today.AddDays(3);

				for (int i = 0; i < serviceTypes.Length; i++)
				{
					bool expectedAtOrigin = i <= 3;
					AssertEquals(ZString.Format("Load ETD set: Origin-{0}", i), expectedAtOrigin, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, subGroups[i]));
					AssertEquals(ZString.Format("Load ETD set: Destination-{0}", i), !expectedAtOrigin, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, subGroups[i]));
				}

				transport.JW_ATD = today.AddDays(2);

				for (int i = 0; i < serviceTypes.Length; i++)
				{
					bool expectedAtOrigin = i <= 2;
					AssertEquals(ZString.Format("Load ATD set: Origin-{0}", i), expectedAtOrigin, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, subGroups[i]));
					AssertEquals(ZString.Format("Load ATD set: Destination-{0}", i), !expectedAtOrigin, autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Destination, subGroups[i]));
				}
			});
		}

		public void TestAutoRatingJobServicesFromContainerLevel()
		{
			ZDateTime today = ZDateTime.Today;

			ZString currentBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = today.AddDays(5);
			var autoRating = GetAdapter(shipment);

			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer cont1 = consol.Containers.AddNew();
			cont1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			cont1.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont1.PackLines.Add(shipment.OuterPackLines.AddNew());
			CommonContainer cont2 = consol.Containers.AddNew();
			cont2.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			cont2.JC_ContainerMode = Constants.ContainerModes.FCL;
			cont2.PackLines.Add(shipment.OuterPackLines.AddNew());

			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			Assert(!autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));

			JobService fumigation = cont1.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = today;
			var fumigationContractor = Factory.New<OrgHeader>();
			fumigation.ES_OH_Contractor = fumigationContractor.PK;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			Assert(autoRating.JobServices.GetContractors().Contains(fumigationContractor));

			fumigation = cont2.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Today;

			JobService quarantine = shipment.DocsAndCartage.Services.AddNew();
			quarantine.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			quarantine.ES_Completed = ZDateTime.Today;
			Assert(autoRating.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.QuarantineInspection));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchHomePort;
		}

		public void TestAutoRatingJobServicesBuyersConsol()
		{
			ZDateTime today = ZDateTime.Today;

			ZString currentBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

			CommonShipment mainShipment = GetShipment();
			mainShipment.JS_TransportMode = Constants.TransportModes.Sea;
			mainShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			mainShipment.JS_RL_NKOrigin = "AUSYD";
			mainShipment.JS_RL_NKDestination = "USLAX";
			mainShipment.JS_E_DEP = today.AddDays(5);
			var autoRatingObj = GetAdapter(mainShipment);

			CommonShipment shipment1 = GetShipment();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_E_DEP = today.AddDays(5);

			CommonConsol consol = mainShipment.Consols.AddNew();
			shipment1.Consols.Add(consol);

			CommonContainer cont1 = consol.Containers.AddNew();
			cont1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			cont1.JC_ContainerMode = Constants.ContainerModes.FCL;
			CommonContainer cont2 = consol.Containers.AddNew();
			cont2.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			cont2.JC_ContainerMode = Constants.ContainerModes.FCL;

			mainShipment.OuterPackLines.AddNew().JL_JC = cont1.PK;
			shipment1.OuterPackLines.AddNew().JL_JC = cont2.PK;

			mainShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			mainShipment.CoLoadShipments.Add(shipment1);
			AssertEquals(shipment1.GetConsolLeadShipment().PK, mainShipment.PK);

			JobService fumigation = cont1.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = today;
			Assert(autoRatingObj.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			//AssertEquals(1, autoRatingObj.Measures[MeasureType.ContainerCount].Value.Containers.Count(x => x.HasJobServiceInfo(Constants.FreightServiceType.Codes.Fumigation)));

			var adapters = mainShipment.GetRatingAdapters();
			AssertEquals(3, adapters.Count);
			Assert(adapters[1].JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			//AssertEquals(1, adapters[1].Measures[MeasureType.ContainerCount].Value.Containers.Count(x => x.HasJobServiceInfo(Constants.FreightServiceType.Codes.Fumigation)));
			Assert(!adapters[2].JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));

			fumigation = cont2.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = today;
			Assert(autoRatingObj.JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			//AssertEquals(1, autoRatingObj.Measures[MeasureType.ContainerCount].Value.Containers.Count(x => x.HasJobServiceInfo(Constants.FreightServiceType.Codes.Fumigation)));

			adapters = mainShipment.GetRatingAdapters();
			Assert(adapters[1].JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			//AssertEquals(2, adapters[1].Measures[MeasureType.ContainerCount].Value.Containers.Count(x => x.HasJobServiceInfo(Constants.FreightServiceType.Codes.Fumigation)));
			Assert(adapters[2].JobServices.IsEnabled(ChargeCodeGroupList.Codes.Origin, Constants.FreightServiceType.Codes.Fumigation));
			//AssertEquals(1, adapters[2].Measures[MeasureType.ContainerCount].Value.Containers.Count(x => x.HasJobServiceInfo(Constants.FreightServiceType.Codes.Fumigation)));

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchHomePort;
		}

		[TestDate(2012, 01, 01)]
		public void TestBCNDepartureAndArrivalDates()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var mainShipment = consol.Shipments.AddNew();
			mainShipment.JS_TransportMode = Constants.TransportModes.Sea;
			mainShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			mainShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			Assert(mainShipment.GetFirstAdapter().StatusInformation.CanExecute);

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ETD = new ZDateTime(2012, 6, 11);
			transport1.JW_ETA = new ZDateTime(2012, 7, 7);

			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ETD = new ZDateTime(2012, 7, 11);
			transport2.JW_ETA = new ZDateTime(2012, 7, 22);

			mainShipment.JS_E_DEP = new ZDateTime(2012, 5, 12);
			mainShipment.JS_E_ARV = new ZDateTime(2012, 7, 22);

			var buyersConsolAdapter = mainShipment.GetRatingAdapters().FirstOrDefault(adapter => adapter is ConsolLeadShipmentRatingAdapter);

			AssertNotNull("Pre-condition", buyersConsolAdapter);
			AssertEquals(new ZDateTime(2012, 5, 12), buyersConsolAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2012, 7, 22), buyersConsolAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestBCNOriginAndDestination()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var mainShipment = consol.Shipments.AddNew();
			mainShipment.JS_ShipmentType = Constants.TransportModes.Sea;
			mainShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			mainShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			mainShipment.JS_RL_NKOrigin = "NZWEL";
			mainShipment.JS_RL_NKDestination = "AUMEL";

			var subShipment = consol.Shipments.AddNew();
			subShipment.JS_ShipmentType = Constants.TransportModes.Sea;
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_RL_NKOrigin = "SGSIN";
			subShipment.JS_RL_NKDestination = "UALAX";

			Assert(mainShipment.GetFirstAdapter().StatusInformation.CanExecute);

			var buyersConsolAdapter = mainShipment.GetRatingAdapters().FirstOrDefault(adapter => adapter is ConsolLeadShipmentRatingAdapter);

			AssertNotNull("Pre-condition", buyersConsolAdapter);
			AssertEquals("Origin Port from the Main BCN Shipment", mainShipment.Origin, buyersConsolAdapter.Origin);
			AssertEquals("Destination Port from the Main Shipment", mainShipment.Destination, buyersConsolAdapter.Destination);
		}

		#region Via

		public void TestViaGetter_ViasAreNotSpecified_ReturnNoVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "UAIEV";

			var adapterToTest = shipment.RatingAdapter;

			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_LoadViaIsSpecified_ReturnLoadVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "UAIEV";

			var adapterToTest = shipment.RatingAdapter;

			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_DischargeViaIsSpecified_ReturnLoadDischargeVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USNYC";

			var adapterToTest = shipment.RatingAdapter;

			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "USNYC", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ShipmentIsImportAndBothViasAreSpecified_ReturnDischargeVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var adapterToTest = shipment.RatingAdapter;

			AssertEquals("Via", "AUMEL", adapterToTest.GetVia(CostSell.Cost).Code);
			AssertEquals("Via", "AUMEL", adapterToTest.GetVia(CostSell.Revenue).Code);
		}

		public void TestViaGetter_ShipmentIsExportAndBothViasAreSpecified_ReturnLoadVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "UAIEV";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var adapterToTest = shipment.RatingAdapter;

			var via = adapterToTest.GetVia(CostSell.Cost);
			AssertEquals("Via", "USNYC", via.Code);

			via = adapterToTest.GetVia(CostSell.Revenue);
			AssertEquals("Via", "USNYC", via.Code);
		}

		public void TestViaGetter_ShipmentIsOffshoreAndBothViasAreSpecified_ReturnNoVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "UAIEV";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var adapterToTest = shipment.RatingAdapter;

			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_ShipmentIsDomesticAndBothViasAreSpecified_ReturnNoVia()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUCNS";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var adapterToTest = shipment.RatingAdapter;

			AssertNull("Via", adapterToTest.GetVia(CostSell.Cost));
			AssertNull("Via", adapterToTest.GetVia(CostSell.Revenue));
		}

		public void TestViaGetter_ShipmentIsImportAndMultiRoutesAndDifferentTransport()
		{
			var shipment = GetShipment();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consol.Transports.AddNew("SGSIN", "AUMEL");
			transport1.JW_TransportType = "MAI";
			transport1.JW_TransportMode = "SEA";

			var transport2 = consol.Transports.AddNew("AUMEL", "AUSYD");
			transport2.JW_TransportType = "OTH";
			transport2.JW_TransportMode = "RAI";

			var adapterToTest = shipment.RatingAdapter;

			var collection = new AutoratingViaPortConfigurationCollection(null, null);
			var viaConfigurations = new (string jobType, string mode, string direction, string origin, string destination, string via)[]
			{
				("SHP", "SEA", "IMP", "1L", "LD", "LRD"),
				("SHP", "SEA", "IMP", "N1L", "LD", "LRD"),
			};

			foreach (var settings in viaConfigurations.GroupBy(x => (x.jobType, x.mode)))
			{
				var configuration = collection.AddNew();
				configuration.JobType = settings.Key.jobType;
				configuration.TransportMode = settings.Key.mode;

				foreach (var item in settings)
				{
					var setting = configuration.Settings.AddNew();
					setting.Direction = item.direction;
					setting.OriginSourceOption = item.origin;
					setting.DestinationSourceOption = item.destination;
					setting.ViaSourceOption = item.via;
				}
			}

			using (RatingDataRegistry.Instance.AutoratingViaPort.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Via", "AUMEL", adapterToTest.GetVia(CostSell.Cost).Code);
				AssertEquals("Via", "AUMEL", adapterToTest.GetVia(CostSell.Revenue).Code);
			}
		}

		#endregion

		#region Matching Locations

		public void TestMatchingLocations_MultiConsol()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEFRA";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = "AIR";
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "AUMEL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "AUMEL"; // FirstLoad
			consol2.JK_RL_NKDischargePort = "SGSIN";
			consol2.Transports[0].JW_IsLinked = false;
			consol2.Transports[0].JW_TransportType = "PRE";
			consol2.Transports[0].JW_TransportMode = "ROA";
			consol2.Transports[0].JW_RL_NKLoadPort = "AUMEL";
			consol2.Transports[0].JW_RL_NKDiscPort = "AUFRE";
			var transport2 = consol2.Transports.AddNew("AUFRE", "SGSIN"); // FirstRouteSetLoad
			transport2.JW_TransportType = "OTH";
			transport2.JW_TransportMode = "SEA";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = "SEA";
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "DEBER"; // LastDischarge
			consol3.Transports[0].JW_IsLinked = false;
			consol3.Transports[0].JW_TransportType = "OTH";
			consol3.Transports[0].JW_TransportMode = "SEA";
			consol3.Transports[0].JW_RL_NKLoadPort = "SGSIN";
			consol3.Transports[0].JW_RL_NKDiscPort = "DEHAM"; // LastRouteSetDischarge
			var transport3 = consol3.Transports.AddNew("DEHAM", "DEBER");
			transport3.JW_TransportType = "ONF";
			transport3.JW_TransportMode = "ROA";

			var consol4 = shipment.Consols.AddNew();
			consol4.JK_TransportMode = "AIR";
			consol4.JK_RL_NKLoadPort = "DEBER";
			consol4.JK_RL_NKDischargePort = "DEFRA";

			var adapterToTest = shipment.RatingAdapter;

			// Enable Multi-Route Auto-Costing => No
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("FirstLoad", "AUMEL", adapterToTest.GetFirstLoad(CostSell.Cost).Code);
				AssertEquals("FirstLoad", "AUMEL", adapterToTest.GetFirstLoad(CostSell.Revenue).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Cost).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Revenue).Code);
				AssertEquals("FirstRouteSetLoad", "AUFRE", adapterToTest.GetFirstRouteSetLoad(CostSell.Cost).Code);
				AssertEquals("FirstRouteSetLoad", "AUFRE", adapterToTest.GetFirstRouteSetLoad(CostSell.Revenue).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Cost).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Revenue).Code);
			}

			// Enable Multi-Route Auto-Costing => Yes
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("FirstLoad", "AUMEL", adapterToTest.GetFirstLoad(CostSell.Cost).Code);
				AssertEquals("FirstLoad", "AUMEL", adapterToTest.GetFirstLoad(CostSell.Revenue).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Cost).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Revenue).Code);
				AssertEquals("FirstRouteSetLoad", "AUFRE", adapterToTest.GetFirstRouteSetLoad(CostSell.Cost).Code);
				AssertEquals("FirstRouteSetLoad", "AUFRE", adapterToTest.GetFirstRouteSetLoad(CostSell.Revenue).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Cost).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Revenue).Code);
			}
		}

		public void TestMatchingLocations_SingleRouteSet_MultiLeg()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEBER";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEBER";
			consol.AddContainer("20GP");
			SetTransport("ROA", "OTH", "AUBNE", "AUSYD", consol.Transports[0]);
			SetTransport("SEA", "MAI", "AUSYD", "HKHKG", consol.Transports.AddNew());
			SetTransport("RAI", "OTH", "HKHKG", "SGSIN", consol.Transports.AddNew());
			SetTransport("SEA", "ONF", "SGSIN", "DEHAM", consol.Transports.AddNew());
			SetTransport("ROA", "ONF", "DEHAM", "DEBER", consol.Transports.AddNew());

			var adapterToTest = shipment.RatingAdapter;

			// Enable Multi-Route Auto-Costing => No
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("FirstLoad", "AUBNE", adapterToTest.GetFirstLoad(CostSell.Cost).Code);
				AssertEquals("FirstLoad", "AUBNE", adapterToTest.GetFirstLoad(CostSell.Revenue).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Cost).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Revenue).Code);
				AssertEquals("FirstRouteSetLoad", "AUSYD", adapterToTest.GetFirstRouteSetLoad(CostSell.Cost).Code);
				AssertEquals("FirstRouteSetLoad", "AUSYD", adapterToTest.GetFirstRouteSetLoad(CostSell.Revenue).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Cost).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Revenue).Code);
			}

			// Enable Multi-Route Auto-Costing => Yes
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var ratingRoutes = consol.GetRatingRoutes(CostSell.Cost);
				AssertEquals(1, ratingRoutes.Count);
				AssertEquals("SEA", ratingRoutes[0].TransportMode);

				AssertEquals("FirstLoad", "AUBNE", adapterToTest.GetFirstLoad(CostSell.Cost).Code);
				AssertEquals("FirstLoad", "AUBNE", adapterToTest.GetFirstLoad(CostSell.Revenue).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Cost).Code);
				AssertEquals("LastDischarge", "DEBER", adapterToTest.GetLastDischarge(CostSell.Revenue).Code);
				AssertEquals("FirstRouteSetLoad", "AUBNE", adapterToTest.GetFirstRouteSetLoad(CostSell.Cost).Code);
				AssertEquals("FirstRouteSetLoad", "AUSYD", adapterToTest.GetFirstRouteSetLoad(CostSell.Revenue).Code);
				AssertEquals("LastRouteSetDischarge", "DEBER", adapterToTest.GetLastRouteSetDischarge(CostSell.Cost).Code);
				AssertEquals("LastRouteSetDischarge", "DEHAM", adapterToTest.GetLastRouteSetDischarge(CostSell.Revenue).Code);
			}

			void SetTransport(string mode, string type, string loadPort, string dischargePort, Transport transport)
			{
				transport.JW_IsLinked = false;
				transport.JW_TransportMode = mode;
				transport.JW_TransportType = type;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = dischargePort;
			}
		}

		#endregion

		public void TestChargeCodeGroups_LCLShipmentIsTheOnlySipmentOnFCLConsol_IncludeNonConsolLevelChargesOnly()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var autoRating = GetAdapter(shipment);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroups_FCLShipmentIsTheOnlySipmentOnLCLConsol_IncludeNonConsolLevelChargesOnly()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			var autoRating = GetAdapter(shipment);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroups_LCLShipmentIsTheOnlySipmentOnLCLConsol_IncludeAllCharges()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			var autoRating = GetAdapter(shipment);
			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroups_FCLShipmentIsTheOnlySipmentOnFCLConsol_IncludeAllCharges()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var autoRating = GetAdapter(shipment);
			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroups_ConsolHasOtherShipments_IncludeNonConsolLevelChargesOnly()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.Shipments.AddNew();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var autoRating = GetAdapter(shipment);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroups_ConsolHasCosts_IncludeNonConsolLevelChargesOnly()
		{
			var shipment = GetShipment();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var cost = (BusinessObject)Factory.New<IJobConsolCost>();
			cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			cost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			try
			{
				cost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				cost[JobConsolCostSchema.E6_ParentTableCode] = "JK";
			}
			finally
			{
				cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			var autoRating = GetAdapter(shipment);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroupsGetter_CurrentDepartmentIsGateway_SetAllCostChargesFilter()
		{
			var shipment1 = GetShipment();
			var shipment2 = GetShipment();

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.New<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUBNE";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			new JobHeader.Loader((IJobHeaderParent)consol).TryCreate();

			Assert("Pre-condition", consol.IsGatewayBillingEnabled());

			var autorating = shipment1.RatingAdapter;
			AssertEquals(ChargeCodeFilter.AutorateAll, autorating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autorating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroupsGetter_CurrentDepartmentIsNotGateway_SetNonGroupageCostChargesFilter()
		{
			var shipment1 = GetShipment();
			var shipment2 = GetShipment();

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Assert("Pre-condition", !consol.IsGatewayBillingEnabled());

			var autorating = shipment1.RatingAdapter;
			AssertEquals(ChargeCodeFilter.AutorateAll, autorating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autorating.ChargeCodeGroups.CostChargesFilter);
		}

		public void TestChargeCodeGroups_Gateway_Disabled()
		{
			GlbDepartment nonGatewayDepartment = GlbDepartment.CurrentDepartment;
			var gatewayDepartment = Factory.New<GlbDepartment>();
			gatewayDepartment.GE_Code = "GGG";

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			CommonShipment shipment = GetShipment();
			CommonShipment shipment2 = GetShipment();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(shipment2);
			var autoRating = GetAdapter(shipment);

			var gatewayJob = Factory.NewJobForTesting<JobHeader>();
			gatewayJob.Parent = (IJobHeaderParent)consol;

			Assert("Pre-condition", consol.IsGatewayBillingEnabled());

			consol.Job.JH_GE = nonGatewayDepartment.PK;
			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);

			consol.Job.JH_GE = gatewayDepartment.PK;
			AssertEquals(ChargeCodeFilter.AutorateAll, autoRating.ChargeCodeGroups.SellChargesFilter);
			AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, autoRating.ChargeCodeGroups.CostChargesFilter);
		}

		public virtual void TestRemovingOfBrokerageChargeCodeGroups()
		{
			Env.Registry.Rating.SetFreightRatedCodes("OBR,OBO,ORG,LOD,UNL,DST,INS,FRT,BRK,BON");

			CommonShipment shipment = GetShipment();
			var autoRating = GetAdapter(shipment);
			AssertEquals(10, autoRating.ChargeCodeGroups.Count);

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(6, autoRating.ChargeCodeGroups.Count);
			Assert(!autoRating.ChargeCodeGroups.Contains("BRK"));
			Assert(!autoRating.ChargeCodeGroups.Contains("BON"));
			Assert(!autoRating.ChargeCodeGroups.Contains("OBR"));
			Assert(!autoRating.ChargeCodeGroups.Contains("OBO"));
		}

		public void TestWeightVolumeLoadingMetersWithDifferentCommodity()
		{
			CommonShipment shipment = GetShipment();
			var autoRating = GetAdapter(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualWeight = 3100m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_ActualVolume = 2.8m;
			shipment.JS_OuterPacks = 11;
			shipment.JS_LoadingMeters = 10.24m;
			shipment.OuterPackLines.RemoveAndDeleteAll();
			AssertEquals(0, shipment.OuterPackLines.Count);

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(3100m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(2.8m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(3.1m, rateableMeasures.GetActual(MeasureType.Chargeable));
			AssertEquals(11m, rateableMeasures.GetActual(MeasureType.Package));
			AssertEquals(10.24m, rateableMeasures.GetActual(MeasureType.LoadingMeters));
			var commodities = rateableMeasures.GetCommodities().ToList();
			AssertEquals(1, commodities.Count);
			AssertEquals(string.Empty, commodities[0]);

			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_ActualWeightUQ = "KG";
			pack1.JL_ActualWeight = 1000m;
			pack1.JL_ActualVolumeUQ = "M3";
			pack1.JL_ActualVolume = 1.5m;
			pack1.JL_PackageCount = 6;
			pack1.JL_LoadingMeters = 1.024m;
			pack1.JL_RH_NKCommodityCode = "GEN";

			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_ActualWeightUQ = "T";
			pack2.JL_ActualWeight = 2m;
			pack2.JL_ActualVolumeUQ = "L";
			pack2.JL_ActualVolume = 1200m;
			pack2.JL_PackageCount = 4;
			pack2.JL_LoadingMeters = 2.048m;
			pack2.JL_RH_NKCommodityCode = "GEN";
			AssertEquals(2, shipment.OuterPackLines.Count);

			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(3000m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(2.7m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(3.1m, rateableMeasures.GetActual(MeasureType.Chargeable));
			AssertEquals(10m, rateableMeasures.GetActual(MeasureType.Package));
			AssertEquals(3.072m, rateableMeasures.GetActual(MeasureType.LoadingMeters));
			commodities = rateableMeasures.GetCommodities().ToList();
			AssertEquals(1, commodities.Count);
			AssertEquals("GEN", commodities[0]);

			pack2.JL_RH_NKCommodityCode = "HAZ";
			AssertEquals(2, shipment.OuterPackLines.Count);

			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(3000m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(2.7m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(3.1m, rateableMeasures.GetActual(MeasureType.Chargeable));
			AssertEquals(10m, rateableMeasures.GetActual(MeasureType.Package));
			AssertEquals(3.072m, rateableMeasures.GetActual(MeasureType.LoadingMeters));
			commodities = rateableMeasures.GetCommodities().ToList();
			AssertEquals(3, commodities.Count);
			AssertCollectionContains("GEN", commodities);
			AssertCollectionContains("HAZ", commodities);
			AssertCollectionContains(string.Empty, commodities);
		}

		public void TestWeightVolumePerContainer()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = GetShipment();
			consol.Shipments.Add(shipment);

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			PackLine pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_JC = container1.PK;
			pack1.JL_ActualVolume = 100m;

			PackLine pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_JC = container2.PK;
			pack2.JL_ActualVolume = 75m;

			var autoRating = GetAdapter(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(175m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(2, rateableMeasures.GetPacklineUniqueContainerTypePKs_ForTest().Count());
		}

		public void TestWeightVolumePerContainer_SinglePackLine()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = GetShipment();
			consol.Shipments.Add(shipment);

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			PackLine pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_JC = container1.PK;

			var autoRating = GetAdapter(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(container1.JC_RC, rateableMeasures.GetPacklineUniqueContainerTypePKs_ForTest().First());
		}

		public void TestWeightVolumeUnitsForPackLinesWhenBlank()
		{
			CommonShipment shipment = GetShipment();
			var autoRating = GetAdapter(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_ActualWeight = 1000m;
			pack1.JL_ActualWeightUQ = "";
			pack1.JL_ActualVolume = 1.5m;
			pack1.JL_ActualVolumeUQ = "M3";

			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_ActualWeight = 2m;
			pack2.JL_ActualWeightUQ = "T";
			pack2.JL_ActualVolume = 5m;
			pack2.JL_ActualVolumeUQ = "";

			AssertEquals(2, shipment.OuterPackLines.Count);

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals(3000m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals(6.5m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
		}

		public void TestDistanceMeasures()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var autoRating = GetAdapter(shipment);

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("PickupDistance is empty - no pickup confirms on shipment", 0m, rateableMeasures.PickupDistance.Amount);
			AssertEquals("DeliveryDistance is empty - no delivery confirms on shipment", 0m, rateableMeasures.DeliveryDistance.Amount);

			var consignorPickupAddress = Factory.New<OrgAddress>();
			consignorPickupAddress.OA_City = "Consignor";
			consignorPickupAddress.OA_Address1 = "Address";

			var consigneeDeliveryAddress = Factory.New<OrgAddress>();
			consigneeDeliveryAddress.OA_City = "Consignee";
			consigneeDeliveryAddress.OA_Address1 = "Address";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.PK;

			shipment.OuterPackLines.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			pickupConfirm.GetDivot(packline).J8_PackagesDelivered = 6;
			pickupConfirm.EU_Distance = 10m;
			pickupConfirm.EU_DistanceUnit = Constants.Length.Kilometres;

			CommonPickupDeliveryConfirm pickupConfirm2 = shipment.PickupConfirms.AddNew();
			pickupConfirm2.GetDivot(packline).J8_PackagesDelivered = 4;
			pickupConfirm2.EU_Distance = 8m;
			pickupConfirm2.EU_DistanceUnit = Constants.Length.Miles;

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm.GetDivot(packline).J8_PackagesDelivered = 7;
			deliveryConfirm.EU_Distance = 32m;
			deliveryConfirm.EU_DistanceUnit = Constants.Length.Kilometres;

			CommonPickupDeliveryConfirm deliveryConfirm2 = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm2.GetDivot(packline).J8_PackagesDelivered = 1;
			deliveryConfirm2.EU_Distance = 16m;
			deliveryConfirm2.EU_DistanceUnit = Constants.Length.Miles;

			CommonPickupDeliveryConfirm deliveryConfirm3 = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm3.GetDivot(packline).J8_PackagesDelivered = 1;
			deliveryConfirm3.EU_Distance = 100m;
			deliveryConfirm3.EU_DistanceUnit = "";

			AssertEquals("Shipment's pickup confirms not empty", 2, shipment.PickupConfirms.Count);
			AssertEquals("Shipment's delivery confirms not empty", 3, shipment.DeliveryConfirms.Count);

			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("PickupDistance was taken from confirm, where ConfirmAddress equals ConsignorPickupAddress", 8m, rateableMeasures.PickupDistance.Amount);
			AssertEquals("PickupDistance unit", Constants.Length.Miles, rateableMeasures.PickupDistance.Unit);
			AssertEquals("DeliveryDistance was taken from confirm, where ConfirmAddress equals ConsigneeDeliveryAddress", 32m, rateableMeasures.DeliveryDistance.Amount);
			AssertEquals("DeliveryDistance unit", Constants.Length.Kilometres, rateableMeasures.DeliveryDistance.Unit);

			shipment.ServiceLevel.RS_IsDoorToDoor = true;
			Transport transport = shipment.Transports.AddNew();
			transport.JW_Distance = 500m;
			transport.JW_DistanceUnit = Constants.Length.Kilometres;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;

			transport = shipment.Transports.AddNew();
			transport.JW_Distance = 1000m;
			transport.JW_DistanceUnit = Constants.Length.Kilometres;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("When service level is DoorToDoor, PickupDistance is taken from main transport", 1000m, rateableMeasures.PickupDistance.Amount);
			AssertEquals("PickupDistance unit", Constants.Length.Kilometres, rateableMeasures.PickupDistance.Unit);
			AssertEquals("When service level is DoorToDoor, DeliveryDistance is taken from main transport", 1000m, rateableMeasures.DeliveryDistance.Amount);
			AssertEquals("DeliveryDistance unit", Constants.Length.Kilometres, rateableMeasures.DeliveryDistance.Unit);
		}

		public void TestDistanceMeasures_FCL()
		{
			CommonShipment shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			CommonConsol consol = shipment.Consols.AddNew();
			CommonContainer container = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			PackLine line = shipment.OuterPackLines.AddNew();
			PackLine line2 = shipment.OuterPackLines.AddNew();
			line.SetContainer(consol, container);
			line2.SetContainer(consol, container2);

			var autoRating = GetAdapter(shipment);

			AssertNull("There should exist no current confirm", container.OriginGetConfirm);
			AssertNull("There should exist no current confirm", container.DestinationGetConfirm);
			AssertEquals("No confirms at all", 0, container.Confirms.Count);

			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("PickupDistance is empty - no pickup confirms on shipment", 0m, rateableMeasures.PickupDistance.Amount);
			AssertEquals("DeliveryDistance is empty - no delivery confirms on shipment", 0m, rateableMeasures.DeliveryDistance.Amount);

			CombineAssertions("No confirm should be created when the measures are retrieved", () =>
			{
				AssertNull("There should exist no current confirm", container.OriginGetConfirm);
				AssertNull("There should exist no current confirm", container.DestinationGetConfirm);
				AssertEquals("No confirms at all", 0, container.Confirms.Count);
			});

			var consignorPickupAddress = Factory.New<OrgAddress>();
			consignorPickupAddress.OA_City = "Consignor";
			consignorPickupAddress.OA_Address1 = "Address";

			var consigneeDeliveryAddress = Factory.New<OrgAddress>();
			consigneeDeliveryAddress.OA_City = "Consignee";
			consigneeDeliveryAddress.OA_Address1 = "Address";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.PK;

			CommonPickupDeliveryConfirm pickupConfirm = container.OriginConfirm;
			pickupConfirm.EU_Distance = 10m;
			pickupConfirm.EU_DistanceUnit = Constants.Length.Kilometres;

			CommonPickupDeliveryConfirm pickupConfirm2 = container2.OriginConfirm;
			pickupConfirm2.EU_Distance = 8m;
			pickupConfirm2.EU_DistanceUnit = Constants.Length.Miles;

			CommonPickupDeliveryConfirm deliveryConfirm = container.DestinationConfirm;
			deliveryConfirm.EU_Distance = 32m;
			deliveryConfirm.EU_DistanceUnit = Constants.Length.Kilometres;

			CommonPickupDeliveryConfirm deliveryConfirm2 = container2.DestinationConfirm;
			deliveryConfirm2.EU_Distance = 16m;
			deliveryConfirm2.EU_DistanceUnit = Constants.Length.Miles;

			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("PickupDistance was taken from confirm, where ConfirmAddress equals ConsignorPickupAddress", 8m, rateableMeasures.PickupDistance.Amount);
			AssertEquals("PickupDistance unit", Constants.Length.Miles, rateableMeasures.PickupDistance.Unit);
			AssertEquals("DeliveryDistance was taken from confirm, where ConfirmAddress equals ConsigneeDeliveryAddress", 32m, rateableMeasures.DeliveryDistance.Amount);
			AssertEquals("DeliveryDistance unit", Constants.Length.Kilometres, rateableMeasures.DeliveryDistance.Unit);

			shipment.ServiceLevel.RS_IsDoorToDoor = true;
			Transport transport = shipment.Transports.AddNew();
			transport.JW_Distance = 500m;
			transport.JW_DistanceUnit = Constants.Length.Kilometres;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;

			transport = shipment.Transports.AddNew();
			transport.JW_Distance = 1000m;
			transport.JW_DistanceUnit = Constants.Length.Kilometres;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;

			rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertEquals("When service level is DoorToDoor, PickupDistance is taken from main transport", 1000m, rateableMeasures.PickupDistance.Amount);
			AssertEquals("PickupDistance unit", Constants.Length.Kilometres, rateableMeasures.PickupDistance.Unit);
			AssertEquals("When service level is DoorToDoor, DeliveryDistance is taken from main transport", 1000m, rateableMeasures.DeliveryDistance.Amount);
			AssertEquals("DeliveryDistance unit", Constants.Length.Kilometres, rateableMeasures.DeliveryDistance.Unit);
		}

		public void TestPopulateAdditionalJobs_BCNShipmentWithNonBCNMaster()
		{
			var localClient = Factory.New<OrgHeader>();
			localClient.CompanyData.OB_ARBuyersConsolInvoicingStyle = "APP";

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var subShipment = Factory.New<CommonShipment>();
			subShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			new JobHeader.Loader(subShipment).TryCreate();
			subShipment.Job.LocalChargesPK = localClient.PK;

			AssertNoExceptionThrown(() => subShipment.GetRatingAdapters());
		}

		public void TestBuyersConsolLeadShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AssertEquals(null, shipment.GetConsolLeadShipment());

			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(masterShipment, shipment.GetConsolLeadShipment());

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals(shipment, shipment.GetConsolLeadShipment());
		}

		public void TestDebtorOrgs()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var org = Factory.New<OrgHeader>();

			var scp = shipment.ControllingCustomerAddress;
			scp.OrganisationPK = org.PK;
			AssertEquals(org, shipment.ControllingCustomer);

			var adapter = GetAdapter(shipment);
			AssertNotNull(adapter.DebtorOrgs);
			AssertEquals("Should be 1 debtor org", 1, adapter.DebtorOrgs.Count);
			AssertEquals("Should be Org as a controlling customer", org, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);
		}

		public void TestGetContractNumber()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Numbers.AddOrSkipContractNumber("AAA", CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);
			consol.Numbers.AddOrSkipContractNumber("BBB", CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON);

			var shipment = consol.Shipments.AddNew();
			shipment.Numbers.AddOrSkipContractNumber("CCC", CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CLC);

			var adapter = GetAdapter(shipment);
			AssertContainsExactElementsInAnyOrder(new[] { "CCC" }, adapter.ClientContractNumbers);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB" }, adapter.CarrierContractNumbers);
		}

		#region Measures

		public void TestMeasures_ContainerCount_AddCountWithConsolContainerTypeAndCommodity()
		{
			var shipment = Factory.New<CommonShipment>();

			var consol = Factory.New<CommonConsol>();
			consol.Shipments.Add(shipment);

			consol.AddContainer("20GP", "ALUM", count: 2, packLines: new[]
			{
				shipment.AddPackLine(commodity: "GEN", weight: 100),
				shipment.AddPackLine(commodity: "ALUM", weight: 200),
			});

			consol.AddContainer("40GP", "BOAT", count: 5, packLines: new[]
			{
				shipment.AddPackLine(commodity: "GEN", weight: 100),
				shipment.AddPackLine(commodity: "ALUM", weight: 200),
			});

			consol.AddContainer("20GP", "HAZ", count: 3, packLines: new[]
			{
				shipment.AddPackLine(commodity: "ALUM", weight: 100),
				shipment.AddPackLine(commodity: "SALT", weight: 200),
			});

			var adapter = GetAdapter(shipment);

			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			var containersAsString = GetContainersAsString(rateableMeasures);

			AssertEquals(3, containersAsString.Count());
			AssertEquals("2 20GP ALUM", containersAsString.ElementAt(0));
			AssertEquals("5 40GP BOAT", containersAsString.ElementAt(1));
			AssertEquals("3 20GP HAZ", containersAsString.ElementAt(2));
		}

		public void TestMeasures_ContainerCount_ConsolHasNoCommodityAndShipmentHasSingleCommodity_UseCommodityFromShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			var anotherShipment = Factory.New<CommonShipment>();

			var consol = Factory.New<CommonConsol>();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);

			consol.AddContainer("20GP", "", count: 2, packLines: new[]
			{
				shipment.AddPackLine(commodity: "HAZ", weight: 100),
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 200),
			});

			var adapter = GetAdapter(shipment);

			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("Shipment with HAZ commodity is being autorated, so, we can fallback to it if consol commodity is empty",
				"2 20GP HAZ",
				GetContainersAsString(rateableMeasures).Single());
		}

		public void TestMeasures_ContainerCount_ConsolHasNoCommodityAndShipmentHasMultipleCommodities_UseEmptyCommodity()
		{
			var shipment = Factory.New<CommonShipment>();
			var anotherShipment = Factory.New<CommonShipment>();

			var consol = Factory.New<CommonConsol>();
			consol.Shipments.Add(shipment);
			consol.Shipments.Add(anotherShipment);

			consol.AddContainer("20GP", "", count: 2, packLines: new[]
			{
				shipment.AddPackLine(commodity: "HAZ", weight: 100),
				shipment.AddPackLine(commodity: "SALT", weight: 200),
				anotherShipment.AddPackLine(commodity: "ALUM", weight: 200),
			});

			var adapter = GetAdapter(shipment);

			var rateableMeasures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("The shipment being autorated has HAZ and SALT commodity, so, we can't figure out single commodity for the container",
				"2 20GP ",
				GetContainersAsString(rateableMeasures).Single());
		}

		public void TestMeasures_ContainerCount_ShipmentInMultipleConsols_ShouldOnlyUseMostCorrectConsol_Import()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var shipment = Factory.New<CommonShipment>();
			var originConsol = shipment.Consols.AddNew();
			var destinationConsol = shipment.Consols.AddNew();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			originConsol.JK_RL_NKLoadPort = "USLAX";
			originConsol.JK_RL_NKDischargePort = "NZAKL";
			destinationConsol.JK_RL_NKLoadPort = "NZAKL";
			destinationConsol.JK_RL_NKDischargePort = "AUSYD";

			var pack1 = shipment.AddPackLine(commodity: "ALUM");
			var pack2 = shipment.AddPackLine(commodity: "ALUM");

			// Consol 1 - 2 x 20GP
			originConsol.AddContainer(containerType: "20GP", containerMode: "FCL", packLines: new[]
			{
				pack1,
			});
			originConsol.AddContainer(containerType: "20GP", containerMode: "FCL", packLines: new[]
			{
				pack2,
			});

			// Consol 2 - 1 x 40GP
			destinationConsol.AddContainer(containerType: "40GP", containerMode: "FCL", commodity: string.Empty, packLines: new[]
			{
				pack1,
				pack2,
			});

			var adapter = GetAdapter(shipment);
			Assert(adapter.IsImport());
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("1 40GP ALUM", GetContainersAsString(measures).Single());
		}

		public void TestMeasures_ContainerCount_ShipmentInMultipleConsols_ShouldOnlyUseMostCorrectConsol_Export()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var shipment = Factory.New<CommonShipment>();
			var originConsol = shipment.Consols.AddNew();
			var destinationConsol = shipment.Consols.AddNew();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			originConsol.JK_RL_NKLoadPort = "AUSYD";
			originConsol.JK_RL_NKDischargePort = "NZAKL";
			destinationConsol.JK_RL_NKLoadPort = "NZAKL";
			destinationConsol.JK_RL_NKDischargePort = "USLAX";

			var pack1 = shipment.AddPackLine(commodity: "GEN");
			var pack2 = shipment.AddPackLine(commodity: "ALUM");

			// Consol 1 - 2 x 20GP
			originConsol.AddContainer(containerType: "20GP", containerMode: "FCL", packLines: new[]
			{
				pack1,
			});
			originConsol.AddContainer(containerType: "20GP", containerMode: "FCL", packLines: new[]
			{
				pack2,
			});

			// Consol 2 - 1 x 40GP
			destinationConsol.AddContainer(containerType: "40GP", containerMode: "FCL", commodity: string.Empty, packLines: new[]
			{
				pack1,
				pack2,
			});

			var adapter = GetAdapter(shipment);
			Assert(adapter.IsExport());
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			AssertEquals("2 20GP GEN", GetContainersAsString(measures).Single());
		}

		public void TestMeasures_ContainerCount_ShipmentPackedInMultipleContainers_ShouldCountAllContainersShipmentPackedIn()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			consol.AddContainer(containerType: "20GP", commodity: "ALUM", containerMode: "FCL", number: "1111", packLines: new[]
			{
				shipment1.AddPackLine(),
				shipment2.AddPackLine()
			});
			consol.AddContainer(containerType: "20GP", commodity: "CAR", containerMode: "FCL", number: "2222", packLines: new[]
			{
				shipment1.AddPackLine(),
			});

			var measures = (RateableMeasureSet)shipment1.RatingAdapter.RateableMeasures;
			var containersAsString = GetContainersAsString(measures);

			AssertEquals(2, containersAsString.Count());
			AssertEquals("1 20GP ALUM", containersAsString.ElementAt(0));
			AssertEquals("1 20GP CAR", containersAsString.ElementAt(1));

			measures = (RateableMeasureSet)shipment2.RatingAdapter.RateableMeasures;
			AssertEquals("1 20GP ALUM", GetContainersAsString(measures).Single());
		}

		public void TestRatingAdapterLclContainersAreNotConsideredForContainerCountMeasure()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			consol.AddContainer(containerType: "20GP", commodity: "FCLC", containerMode: Constants.ContainerModes.FCL, number: "1111", packLines: new[]
			{
				shipment.AddPackLine()
			});

			consol.AddContainer(containerType: "20GP", commodity: "LCLC", containerMode: Constants.ContainerModes.LCL, number: "2222", packLines: new[]
			{
				shipment.AddPackLine(),
			});

			var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;
			AssertEquals("1 20GP FCLC", GetContainersAsString(measures).Single());
		}

		public void TestRatingAdapterGrpContainersAreNotConsideredForContainerCountMeasure()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			consol.AddContainer(containerType: "20GP", commodity: "FCLC", containerMode: Constants.ContainerModes.FCL, number: "1111", packLines: new[]
			{
				shipment.AddPackLine()
			});

			consol.AddContainer(containerType: "20GP", commodity: "GRPC", containerMode: Constants.ContainerModes.Groupage, number: "2222", packLines: new[]
			{
				shipment.AddPackLine(),
			});

			var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;
			AssertEquals("1 20GP FCLC", GetContainersAsString(measures).Single());
		}

		IEnumerable<string> GetContainersAsString(RateableMeasureSet measures)
		{
			foreach (var container in measures.GetContainerTypeAndCommodityList())
			{
				var refContainer = Factory.Load<RefContainer>(container.ContainerTypePk);
				yield return FormattableString.Invariant($"{container.ContainerCount} {refContainer.RC_Code} {container.CommodityCode}");
			}
		}

		#endregion

		#region TestLocationValidation

		public void TestLocationValidation()
		{
			var shipment = GetShipment();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			Factory.Save();

			// Assign location string which do not exists
			shipment.LocationWhsGuid = whs.PK;
			shipment.LocationString = "A-1";
			AssertHasErrors(shipment.LocationStringInfo);

			// Assign location string which does exists
			shipment.LocationString = "A";
			AssertNoErrors(shipment.LocationStringInfo);
			Factory.Save();

			row.WR_Columns = 2;
			Factory.Save();
			// Reset the locationstring
			shipment.LocationString = "A-1";
			AssertNoErrors(shipment.LocationStringInfo);
		}

		#endregion

		#region Implementation

		static RatingAdapter GetAdapter(CommonShipment shipment)
		{
			return shipment.RatingAdapter as RatingAdapter;
		}

		CommonShipment GetShipment()
		{
			return (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
		}

		bool onlyShipmentInConsolEventFired;

		void TestShipment_OnlyShipmentInConsol_YES(CommonShipment.OnlyShipmentInConsolEventArgs args)
		{
			onlyShipmentInConsolEventFired = true;
		}

		void TestShipment_OnlyShipmentInConsol_NO(CommonShipment.OnlyShipmentInConsolEventArgs args)
		{
			onlyShipmentInConsolEventFired = true;
			args.Cancel = true;
		}

		#endregion
	}
}
