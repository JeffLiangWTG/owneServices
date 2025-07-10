using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLCTest : TestCaseWithFactory
	{
		public void TestApportionment()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LinePrice = 10000m;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LinePrice = 40000m;

			new ApportionManager(order, new ApportionStrategy()).ApportionAll();

			AssertEquals(20m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals(80m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestApportionmentBasedOnWeight()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_ActualWeight = 10000m;
			line1.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_ActualWeight = 40000m;
			line2.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;

			new ApportionManager(order, new ApportionStrategy()).ApportionAll();

			AssertEquals(20m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals(80m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestApportionmentBasedOnVolume()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Volume;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_ActualVolume = 10000m;
			line1.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_ActualVolume = 40000m;
			line2.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			new ApportionManager(order, new ApportionStrategy()).ApportionAll();

			AssertEquals(20m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals(80m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestApportionmentBasedOnQuantity()
		{
			var order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Quantity;

			var line1 = order.OrderLines.AddNew();
			line1.JO_Quantity = 10000m;
			line1.JO_F3_NKPackType = Constants.PkgUnit.Bottle;

			var line2 = order.OrderLines.AddNew();
			line2.JO_Quantity = 40000m;
			line2.JO_F3_NKPackType = Constants.PkgUnit.Container;

			new ApportionManager(order, new ApportionStrategy()).ApportionAll();

			AssertEquals(20m, line1.ApportionedCharges[0].J7_Amount);
			AssertEquals(80m, line2.ApportionedCharges[0].J7_Amount);
		}

		public void TestApportionmentWhenOrderLineHasOverridingCharge()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LinePrice = 10000m;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LinePrice = 40000m;

			JobComInvCharge overridingOFT = line1.Charges.AddNew();
			overridingOFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			overridingOFT.J7_Amount = 80m;
			overridingOFT.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			new ApportionManager(order, new ApportionStrategy()).ApportionAll();

			AssertEquals(80m, line1.Charges.GetCharge(charge.ApportionChargeKey).Amount);
			AssertEquals(20m, line2.ApportionedCharges.GetCharge(charge.ApportionChargeKey).Amount);
		}

		public void TestApportionmentWhenOrderLineHasItsOwnCharge()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LinePrice = 10000m;

			JobComInvCharge oFT1 = line1.Charges.AddNew();
			oFT1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT1.J7_Amount = 10m;
			oFT1.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LinePrice = 40000m;

			JobComInvCharge oFT2 = line2.Charges.AddNew();
			oFT2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT2.J7_Amount = 80m;
			oFT2.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			new ApportionManager(order, new ApportionStrategy()).ApportionAll();

			AssertEquals(90m, order.ApportionedCharges.GetCharge(oFT1.ApportionChargeKey).Amount);
		}

		public void TestEndToEndTestForLC()
		{
			IAUDutyCalculationManager temp = ObjectFactory.Get<IAUDutyCalculationManager>();
			temp.CreateTaxOrFee();

			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Female Shoes";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BusinessObject classification = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IClassification>());
			classification[CusClassificationSchema.CC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
			classification[CusClassificationSchema.CC_TariffNum] = "6405.10.00 58";
			classification[CusClassificationSchema.CC_LookupCode] = "Female Shoes";
			classification[CusClassificationSchema.CC_Description] = "Female Shoes";
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Australia;
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			Factory.Save();

			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			order.JD_RL_NKPortOfDischarge = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.JD_RL_NKPortOfLoading = "NZAKL";
			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.Australia;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			order.JD_RN_NKCountryOfSupply = Core.Constants.CountryCodes.NewZealand;

			OrderContainer container = order.PlannedContainers.AddNew();
			container.J1_ContainerCount = 2;
			container.J1_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_IsIncludedInITOT = true;
			charge.J7_Amount = 110m;
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Partno = "Female Shoes";
			orderLine.JO_Quantity = 10m;
			orderLine.JO_F3_NKPackType = "BOX";
			orderLine.JO_ItemPrice = 1000m;
			orderLine.JO_LinePrice = 10000m;
			orderLine.JO_ActualWeight = 200m;
			orderLine.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;
			orderLine.JO_ActualVolume = 300m;
			orderLine.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			((ILandedCostHeader)order).DoStuffBeforeRunningLCDistribution();

			IUltimateDistributee line = orderLine;

			AssertEquals(300m, line.Actual);

			AssertEquals(200m, line.ActualWeightInKG);

			AssertEquals(300m, line.ActualVolumeInM3);

			AssertEquals(Core.Constants.CountryCodes.NewZealand, line.CountryOfOriginCode);

			AssertEquals(10000m, line.CostInLocalCurrency);

			AssertEquals(9890m, line.CustomsValue);

			AssertEquals(10m, line.DutyPercent);

			AssertEquals("64051000 58", line.TariffNumber);

			AssertEquals(product.PK, line.FKToProduct);

			AssertEquals(Core.Constants.CurrencyCodes.Australia, line.InvoiceCurrencyCode);

			AssertEquals(10m, line.ItemCount);

			AssertEquals("BOX", line.InvoiceUQ);

			DutyTaxEntryFee dutyFee = line.LineDutyTaxEntryFeeItems;
			AssertEquals(989m, dutyFee["TDT"]);

			AssertEquals(1000m, line.UnitPriceInInvoiceCurrency);
		}

		public void TestVolumeCalculationBasedOnDimensions()
		{
			var order = Factory.New<Order>();
			var orderline = order.OrderLines.AddNew();

			orderline.JO_OuterPacks = 1;
			orderline.JO_OuterPackHeight = 100;
			orderline.JO_OuterPackLength = 200;
			orderline.JO_OuterPackWidth = 300;
			orderline.JO_OuterPackUnitOfDimension = Core.Constants.Length.Centimetres;
			orderline.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			AssertEquals(6m, orderline.JO_ActualVolume);

			orderline.JO_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			AssertEquals(6000m, orderline.JO_ActualVolume);

			orderline.JO_ActualVolume = 60;
			AssertEquals(60m, orderline.JO_ActualVolume);
		}
	}
}
