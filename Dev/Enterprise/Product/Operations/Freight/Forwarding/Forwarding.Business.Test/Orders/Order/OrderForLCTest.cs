using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderForLCTest : TestCaseWithFactory
	{
		public void TestUltimateDistributees()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "O84589";
			order.JD_OrderNumberSplit = 1;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LineStatus = Core.Constants.OrderStatus.Confirmed;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;

			OrderLine line3 = order.OrderLines.AddNew();
			line3.JO_LineStatus = Core.Constants.OrderStatus.Incomplete;

			AssertEquals(order.JD_OrderNumberAndSplit, ((ILandedCostDistributeTo)order).UniqueCode);

			List<IUltimateDistributee> ditributees = new List<IUltimateDistributee>(((ILandedCostDistributeTo)order).UltimateDistributees);
			AssertEquals("Cancelled lines should have been excluded", 2, ditributees.Count);
			Assert(!ditributees.Contains(line2));

			ditributees = new List<IUltimateDistributee>(((ILandedCostHeader)order).UltimateDistributees);
			AssertEquals("Cancelled lines should have been excluded", 2, ditributees.Count);
			Assert(!ditributees.Contains(line2));
		}

		public void TestILandedCostChargeHolder()
		{
			Order order = Factory.New<Order>();
			order.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			JobComInvCharge charge = order.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.J7_IsIncludedInITOT = true;

			Assert("If it is part of Line Price, it should not default", !new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)order).ChargesToImportForLandedCosting).Contains(charge));

			charge.J7_IsIncludedInITOT = false;
			Assert("Valid now", new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)order).ChargesToImportForLandedCosting).Contains(charge));

			charge.J7_Amount = 0m;
			Assert("No cost", !new List<IDefaultLandedCostInput>(((ILandedCostChargeHolder)order).ChargesToImportForLandedCosting).Contains(charge));
		}

		public void TestILandedCostExchangeRateHolder()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = "AUD";
			order.JD_EstimatedExchangeRate = 0.52857m;

			ILandedCostExchangeRateHolder holder = order;
			AssertEquals("AUD", holder.CurrencyCode);
			AssertEquals(0.52857m, holder.LandedCostExchangeRateDefault);

			holder.LandedCostExchangeRate = 0.89652m;
			AssertEquals(0.89652m, holder.LandedCostExchangeRateDefault);
		}

		public void TestILandedCostHeaderGetDefaultExchangeRates()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = "AUD";
			order.JD_EstimatedExchangeRate = 0.52857m;

			JobComInvCharge charge1 = order.Charges.AddNew();
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			charge1.J7_ExchangeRate = 1.2345m;

			JobComInvCharge charge2 = order.Charges.AddNew();
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;
			charge2.J7_ExchangeRate = 0.9999m;

			Dictionary<ZString, ZDecimal> defaultRates = ((ILandedCostHeader)order).GetDefaultExchangeRates();
			AssertEquals(2, defaultRates.Count);
			AssertEquals(1.2345m, defaultRates[Core.Constants.CurrencyCodes.KoreaRepublicOf]);
			AssertEquals(0.9999m, defaultRates[Core.Constants.CurrencyCodes.Japan]);
		}

		public void TestILandedCostHeaderDistributeTo()
		{
			Order order = Factory.New<Order>();

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LineStatus = Core.Constants.OrderStatus.Confirmed;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;

			OrderLine line3 = order.OrderLines.AddNew();
			line3.JO_LineStatus = Core.Constants.OrderStatus.Incomplete;

			List<ILandedCostDistributeTo> result = new List<ILandedCostDistributeTo>(((ILandedCostHeader)order).CandidatesToDistributeCostTo);
			Assert(result.Contains(line1));
			Assert(!result.Contains(line2));
			Assert(result.Contains(line3));
		}

		public void TestHasMultipleInvoices()
		{
			Order order = Factory.New<Order>();

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LineStatus = Core.Constants.OrderStatus.Confirmed;
			line1.JO_CommercialInvoiceNo = "1";

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;
			line2.JO_CommercialInvoiceNo = "2";

			OrderLine line3 = order.OrderLines.AddNew();
			line3.JO_LineStatus = Core.Constants.OrderStatus.Incomplete;
			line3.JO_CommercialInvoiceNo = "1";

			Assert("Cancelled line does not count", !((ILandedCostHeader)order).HasMultiInvoices);

			line3.JO_CommercialInvoiceNo = "2";
			Assert(((ILandedCostHeader)order).HasMultiInvoices);

			order.JD_OrderNumber = "ABC";
			AssertEquals("ABC", ((ILandedCostHeader)order).JobNumber);
		}

		public void TestIsLCSupported()
		{
			Order order = Factory.New<Order>();
			order.JD_RL_NKPortOfLoading = "NZAKL";
			order.JD_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("PreCondition", true, order.IsImport());

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";

			order.BuyerPK = importer.PK;
			Assert(((ILandedCostHeader)order).IsLCSupported);
			Assert(order.JD_ChargesVisible);

			order.JD_RL_NKPortOfDischarge = "USLAX";
			AssertEquals("PreCondition", false, order.IsImport());
			Assert(!((ILandedCostHeader)order).IsLCSupported);
			Assert(!order.JD_ChargesVisible);
		}

		public void TestOnLCSupportedChanged()
		{
			int count = 0;

			Order order = Factory.New<Order>();
			((ILandedCostHeader)order).OnLCSupportedChanged += new EventHandler(delegate
			{ count++; });

			order.JD_RL_NKPortOfLoading = "NZAKL";
			AssertEquals(1, count);

			order.JD_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals(2, count);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;
			AssertEquals(3, count);

			order.JD_JS = ZGuid.Empty;
			AssertEquals(4, count);
		}

		public void TestMarkApportionmentDirty()
		{
			Order order = Factory.New<Order>();
			Assert(!order.ApportionmentDirty);

			order.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			Assert(order.ApportionmentDirty);

			order.ApportionChargesIfNecessary();
			Assert(!order.ApportionmentDirty);

			order.JD_EstimatedExchangeRate = 2m;
			Assert(order.ApportionmentDirty);
		}

		public void TestApportionOnSavingIfDirty()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();

			Order order = Factory.New<Order>();
			order.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.Australia;
			order.BuyerPK = buyer.PK;

			JobComInvCharge charge1 = order.Charges.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge1.J7_Amount = 100m;
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			JobComInvCharge charge2 = order.Charges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge2.J7_Amount = 20m;
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;

			Factory.Save();

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_LinePrice = 1000m;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LinePrice = 4000m;

			AssertEquals("PreCondition", true, order.ApportionmentDirty);
			Factory.Save();

			AssertEquals("Lines have apportioned charges", 2, line1.ApportionedCharges.Count);
			AssertEquals("Lines have apportioned charges", 2, line2.ApportionedCharges.Count);
		}

		public void TestCurrencyConversion()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsReciprocal = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			AssertNotEquals("PreCondition", company.GC_IsReciprocal, GlbCompany.CurrentCompany.GC_IsReciprocal);

			Factory.Save();

			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.Japan;
			order.JD_EstimatedExchangeRate = 0.987m;

			BusinessObject lcHeader = Factory.New(ObjectFactory.GetType<Enterprise.Integration.LandedCosting.ILandedCostHeader>());
			lcHeader[LandedCostHeaderSchema.LT_ParentID] = order.PK;
			lcHeader[LandedCostHeaderSchema.LT_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;
			lcHeader[LandedCostHeaderSchema.LT_GC] = company.PK;

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LinePrice = 1000m;
			AssertEquals(987m, orderLine.LinePriceMoney.Amount);
			AssertEquals(company.LocalCurrency, orderLine.LinePriceMoney.Currency);
		}

		public void TestDefaultEstimatedDutyPercent()
		{
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierBuyerLink link = buyer.SupplierLinks.AddNew();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_DefaultDutyRate = 2m;

			Order order = Factory.New<Order>();
			ILandedCostHeader lcHost = order;
			AssertEquals(0m, lcHost.DefaultEstimatedDutyPercent);

			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;

			AssertEquals(2m, lcHost.DefaultEstimatedDutyPercent);
		}

		[TestDate(2013, 12, 31)]
		public void TestEndToEndForLC()
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
			order.JD_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			order.JD_RL_NKPortOfDischarge = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.JD_RL_NKPortOfLoading = "NZAKL";
			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.Australia;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.LCL;

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

			OrderLine orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_Partno = "Female Shoes";
			orderLine2.JO_Quantity = 20m;
			orderLine2.JO_F3_NKPackType = "BOX";
			orderLine2.JO_ItemPrice = 2000m;
			orderLine2.JO_LinePrice = 20000m;

			ILandedCostHeader lcHost = order;

			AssertEquals(3, lcHost.CandidatesToDistributeCostTo.Count());
			AssertEquals(true, lcHost.CandidatesToDistributeCostTo.Contains(order));

			AssertEquals(buyer, lcHost.Consignee);

			AssertEquals(LandedCostType.Estimated, lcHost.LandedCostType);

			Assert(lcHost.UltimateDistributees.Contains(orderLine));
			Assert(lcHost.UltimateDistributees.Contains(orderLine2));

			lcHost.DoStuffBeforeRunningLCDistribution();
			DutyTaxEntryFee total = lcHost.TotalDutyTaxEntryFeeItems;
			AssertEquals(2988.99m, total["TDT"]);
			AssertEquals(81m, total["ENT"]);

			Assert(lcHost.IsLCSupported);
		}

		public void TestEntryFeesOnDiplomatBuyer()
		{
			IAUDutyCalculationManager temp = ObjectFactory.Get<IAUDutyCalculationManager>();
			temp.CreateTaxOrFee();

			OrgHeader nonDiplomatBuyer = Factory.NewWithValidTestData<OrgHeader>();

			OrgHeader diplomatBuyer = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = diplomatBuyer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;

			Order order = Factory.New<Order>();
			order.BuyerPK = nonDiplomatBuyer.PK;
			order.JD_RL_NKPortOfDischarge = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.Australia;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LinePrice = 50000m;

			ILandedCostHeader lcHost = order;
			lcHost.DoStuffBeforeRunningLCDistribution();
			DutyTaxEntryFee total = lcHost.TotalDutyTaxEntryFeeItems;
			AssertNotEquals("Entry fees should not be zero for non-diplomat buyer", 0m, total["ENT"]);

			order.BuyerPK = diplomatBuyer.PK;

			lcHost.DoStuffBeforeRunningLCDistribution();
			total = lcHost.TotalDutyTaxEntryFeeItems;
			AssertEquals("Entry fees should be zero for diplomat buyer", 0m, total["ENT"]);
		}
	}
}
