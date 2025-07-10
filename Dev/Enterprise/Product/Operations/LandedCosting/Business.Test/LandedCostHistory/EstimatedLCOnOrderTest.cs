using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class EstimatedLCOnOrderTest : TestCaseWithFactory
	{
		public void TestCalculateDutyAmount()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();

			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Female Shoes";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BusinessObject classification = Factory.New(ObjectFactory.GetType<Integration.Customs.AU.IClassification>());
			classification[CusClassificationSchema.CC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
			classification[CusClassificationSchema.CC_TariffNum] = "6405.10.00 58";// 10%
			classification[CusClassificationSchema.CC_LookupCode] = "Female Shoes";
			classification[CusClassificationSchema.CC_Description] = "Female Shoes";
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Integration.Customs.AU.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Australia;
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			Factory.Save();

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			lCHeader.LT_LandedCostType = LandedCostType.Estimated;
			lCHeader.LT_DefaultEstimatedDutyRate = 15m;
			lCHeader.LT_GC = company.PK;

			helper.DummyHeader.TotalDutyTaxEntryFeeItemsExposed = new DutyTaxEntryFee();
			helper.DummyHeader.TotalDutyTaxEntryFeeItemsExposed["TDT"] = 2000m;

			helper.Ultimate1.CustomsValueExposed = 10000m;
			helper.Ultimate2.CustomsValueExposed = 20000m;
			helper.Ultimate2.FKToProductExposed = product.PK;
			helper.Ultimate2.DutyPercentExposed = 10m;
			helper.Ultimate2.LineDutyTaxEntryFeeItemsExposed = new DutyTaxEntryFee();
			helper.Ultimate2.LineDutyTaxEntryFeeItemsExposed["TDT"] = 2000m;

			LCDistributionManager manager = new LCDistributionManager(lCHeader);
			manager.RunLandedCosting();

			AssertEquals(2, lCHeader.Histories.Count);

			LandedCostHistory history1 = lCHeader.Histories.Get(helper.Ultimate1);
			AssertNotNull(history1);

			AssertEquals("Duty percent is defaulted from header", 15m, history1.LH_DutyPercent);
			AssertEquals(1500m, history1.GetLineValue("TDT"));

			LandedCostHistory history2 = lCHeader.Histories.Get(helper.Ultimate2);
			AssertNotNull(history2);

			AssertEquals("Duty percent is defaulted from product", 10m, history2.LH_DutyPercent);
			AssertEquals(2000m, history2.GetLineValue("TDT"));
		}

		[TestDate(2013, 12, 31)]
		public void TestEndToEndTestOnOrder()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				IAUDutyCalculationManager temp = ObjectFactory.Get<IAUDutyCalculationManager>();
				temp.CreateTaxOrFee();

				OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();

				OrgSupplierPart product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "Female Shoes";

				OrgPartRelation relation = product.RelatedOrganisations.AddNew();
				relation.OU_OH = buyer.PK;
				relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

				BusinessObject classification = Factory.New(ObjectFactory.GetType<Integration.Customs.AU.IClassification>());
				classification[CusClassificationSchema.CC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
				classification[CusClassificationSchema.CC_TariffNum] = "6405.10.00 58";
				classification[CusClassificationSchema.CC_LookupCode] = "Female Shoes";
				classification[CusClassificationSchema.CC_Description] = "Female Shoes";
				classification[CusClassificationSchema.CC_ClassificationType] = "IMP";

				BusinessObject pivot = Factory.New(ObjectFactory.GetType<Integration.Customs.AU.ICusClassPartPivot>());
				pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Australia;
				pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
				pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
				pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
				Factory.Save();

				BusinessObject order = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IOrder>());
				order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = buyer.MainAddress.PK;
				order[JobOrderHeaderSchema.JD_IncoTerm] = Core.Constants.IncoTerms.CostAndFreight;
				order[JobOrderHeaderSchema.JD_RL_NKPortOfDischarge] = "AUSYD";
				order[JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo] = "AUSYD";
				order[JobOrderHeaderSchema.JD_RL_NKPortOfLoading] = "NZAKL";
				order[JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt] = "NZAKL";
				order[JobOrderHeaderSchema.JD_RX_NKOrderCurrency] = Core.Constants.CurrencyCodes.Australia;
				order[JobOrderHeaderSchema.JD_TransportMode] = Core.Constants.TransportModes.Sea;
				order[JobOrderHeaderSchema.JD_ContainerMode] = Core.Constants.ContainerModes.LCL;
				order[JobOrderHeaderSchema.JD_RN_NKCountryOfSupply] = Core.Constants.CountryCodes.NewZealand;

				BusinessObject container = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderContainer>());
				container[JobOrderContainerSchema.J1_ContainerCount] = new ZShort(2);
				container[JobOrderContainerSchema.J1_RC] = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
				container[JobOrderContainerSchema.J1_ParentID] = order.PK;
				container[JobOrderContainerSchema.J1_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;

				BusinessObject charge = Factory.New(ObjectFactory.GetType<Integration.Freight.IJobComInvHeaderCharge>());
				charge[JobComInvHeaderChargeSchema.J7_DistributeBy] = "VAL";
				charge[JobComInvHeaderChargeSchema.J7_ChargeType] = "OFT";
				charge[JobComInvHeaderChargeSchema.J7_IsIncludedInITOT] = true;
				charge[JobComInvHeaderChargeSchema.J7_Amount] = 120m;
				charge[JobComInvHeaderChargeSchema.J7_RX_NKCurrency] = Core.Constants.CurrencyCodes.Australia;
				charge[JobComInvHeaderChargeSchema.J7_ParentID] = order.PK;
				charge[JobComInvHeaderChargeSchema.J7_ParentTableCode] = JobOrderHeaderSchema.Constants.Prefix;

				BusinessObject orderLine = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
				orderLine[JobOrderLineSchema.JO_JD] = order.PK;
				orderLine[JobOrderLineSchema.JO_LineNo] = new ZInt(1);
				orderLine[JobOrderLineSchema.JO_Partno] = "Female Shoes";
				orderLine[JobOrderLineSchema.JO_Quantity] = 10m;
				orderLine[JobOrderLineSchema.JO_F3_NKPackType] = "BOX";
				orderLine[JobOrderLineSchema.JO_ItemPrice] = 1000m;
				orderLine[JobOrderLineSchema.JO_LinePrice] = 10000m;
				orderLine[JobOrderLineSchema.JO_ActualWeight] = 200m;
				orderLine[JobOrderLineSchema.JO_UnitOfWeight] = Core.Constants.Weight.Kilograms;
				orderLine[JobOrderLineSchema.JO_ActualVolume] = 300m;
				orderLine[JobOrderLineSchema.JO_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

				BusinessObject orderLine2 = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
				orderLine2[JobOrderLineSchema.JO_JD] = order.PK;
				orderLine2[JobOrderLineSchema.JO_LineNo] = new ZInt(2);
				orderLine2[JobOrderLineSchema.JO_Partno] = "Female Shoes";
				orderLine2[JobOrderLineSchema.JO_Quantity] = 20m;
				orderLine2[JobOrderLineSchema.JO_F3_NKPackType] = "BOX";
				orderLine2[JobOrderLineSchema.JO_ItemPrice] = 2000m;
				orderLine2[JobOrderLineSchema.JO_LinePrice] = 20000m;
				orderLine2[JobOrderLineSchema.JO_ActualWeight] = 300m;
				orderLine2[JobOrderLineSchema.JO_UnitOfWeight] = Core.Constants.Weight.Kilograms;
				orderLine2[JobOrderLineSchema.JO_ActualVolume] = 700m;
				orderLine2[JobOrderLineSchema.JO_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

				Factory.Save();

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				BusinessObject orderLoaded = factory2.Load(ObjectFactory.GetType<Integration.Forwarding.IOrder>(), order.PK);

				LandedCostHeader lcHeader = LandedCostHeader.New((ILandedCostHeader)orderLoaded);

				LandCostInput cost = lcHeader.CostInputs.AddNew();
				cost.LI_CostAmount = 150m;
				cost.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Australia;
				cost.LI_LandedCostGroup = 1;
				cost.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;
				cost.LI_ParentID = order.PK;
				cost.LI_ParentTableCode = JobOrderHeaderSchema.Constants.Prefix;

				new LCDistributionManager(lcHeader).RunLandedCosting();

				AssertEquals(2, lcHeader.Histories.Count);

				LandedCostHistory line1 = lcHeader.Histories.FirstOrDefault(x => x.TotalCost == 11073m);
				LandedCostHistory line2 = lcHeader.Histories.FirstOrDefault(x => x.TotalCost == 22146m);

				AssertNotNull(line1);
				AssertNotNull(line2);

				AssertEquals(50m, line1.LH_LandedCostGroup1);
				AssertEquals("Total duty", 996m, line1.GetLineValue("TDT"));
				AssertEquals("Duty percent", 10m, line1.LH_DutyPercent);
				AssertEquals("Entry Fees", 27m, line1.GetLineValue("ENT").Round(2));

				AssertEquals(100m, line2.LH_LandedCostGroup1);
				AssertEquals("Total duty", 1992m, line2.GetLineValue("TDT"));
				AssertEquals("Duty percent", 10m, line2.LH_DutyPercent);
				AssertEquals("Entry Fees", 54m, line2.GetLineValue("ENT").Round(2));
			}
		}

		public void TestEndToEndTestOnOrderWhereNoAutomaticDutyCalcIsDone()
		{
			//no automatic duty calculation is done
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Female Shoes";

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			BusinessObject classification = Factory.New(ObjectFactory.GetType<Integration.Customs.NZ.ICusClassification>());
			classification[CusClassificationSchema.CC_RN_NKCountryCode] = Core.Constants.CountryCodes.NewZealand;
			classification[CusClassificationSchema.CC_TariffNum] = "6405.10.21.01K";
			classification[CusClassificationSchema.CC_LookupCode] = "Female Shoes";
			classification[CusClassificationSchema.CC_Description] = "Female Shoes";
			classification[CusClassificationSchema.CC_ClassificationType] = "BTH";

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Integration.Customs.NZ.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.NewZealand;
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			Factory.Save();

			BusinessObject order = Factory.New(ObjectFactory.GetType<Integration.Forwarding.IOrder>());
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = buyer.MainAddress.PK;
			order[JobOrderHeaderSchema.JD_IncoTerm] = Core.Constants.IncoTerms.CostAndFreight;
			order[JobOrderHeaderSchema.JD_RL_NKPortOfDischarge] = "NZAKL";
			order[JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo] = "NZAKL";
			order[JobOrderHeaderSchema.JD_RL_NKPortOfLoading] = "AUSYD";
			order[JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt] = "AUSYD";
			order[JobOrderHeaderSchema.JD_RX_NKOrderCurrency] = Core.Constants.CurrencyCodes.NewZealand;
			order[JobOrderHeaderSchema.JD_TransportMode] = Core.Constants.TransportModes.Sea;
			order[JobOrderHeaderSchema.JD_ContainerMode] = Core.Constants.ContainerModes.LCL;
			order[JobOrderHeaderSchema.JD_RN_NKCountryOfSupply] = Core.Constants.CountryCodes.Australia;

			BusinessObject orderLine = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
			orderLine[JobOrderLineSchema.JO_JD] = order.PK;
			orderLine[JobOrderLineSchema.JO_LineNo] = new ZInt(1);
			orderLine[JobOrderLineSchema.JO_Partno] = "Female Shoes";
			orderLine[JobOrderLineSchema.JO_Quantity] = 10m;
			orderLine[JobOrderLineSchema.JO_F3_NKPackType] = "BOX";
			orderLine[JobOrderLineSchema.JO_ItemPrice] = 1000m;
			orderLine[JobOrderLineSchema.JO_LinePrice] = 10000m;
			orderLine[JobOrderLineSchema.JO_ActualWeight] = 200m;
			orderLine[JobOrderLineSchema.JO_UnitOfWeight] = Core.Constants.Weight.Kilograms;
			orderLine[JobOrderLineSchema.JO_ActualVolume] = 300m;
			orderLine[JobOrderLineSchema.JO_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

			BusinessObject orderLine2 = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IOrderLine>());
			orderLine2[JobOrderLineSchema.JO_JD] = order.PK;
			orderLine2[JobOrderLineSchema.JO_LineNo] = new ZInt(2);
			orderLine2[JobOrderLineSchema.JO_Partno] = "Female Shoes";
			orderLine2[JobOrderLineSchema.JO_Quantity] = 20m;
			orderLine2[JobOrderLineSchema.JO_F3_NKPackType] = "BOX";
			orderLine2[JobOrderLineSchema.JO_ItemPrice] = 2000m;
			orderLine2[JobOrderLineSchema.JO_LinePrice] = 20000m;
			orderLine2[JobOrderLineSchema.JO_ActualWeight] = 300m;
			orderLine2[JobOrderLineSchema.JO_UnitOfWeight] = Core.Constants.Weight.Kilograms;
			orderLine2[JobOrderLineSchema.JO_ActualVolume] = 700m;
			orderLine2[JobOrderLineSchema.JO_UnitOfVolume] = Core.Constants.Volume.CubicMetres;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject orderLoaded = factory2.Load(ObjectFactory.GetType<Integration.Forwarding.IOrder>(), order.PK);

			LandedCostHeader lcHeader = LandedCostHeader.New((ILandedCostHeader)orderLoaded);

			LandCostInput cost = lcHeader.CostInputs.AddNew();
			cost.LI_CostAmount = 150m;
			cost.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.NewZealand;
			cost.LI_LandedCostGroup = 1;
			cost.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;
			cost.LI_ParentID = order.PK;
			cost.LI_ParentTableCode = JobOrderHeaderSchema.Constants.Prefix;

			new LCDistributionManager(lcHeader).RunLandedCosting();

			AssertEquals(2, lcHeader.Histories.Count);

			LandedCostHistory line1 = lcHeader.Histories.FirstOrDefault(x => x.TotalCost == 10050m);
			LandedCostHistory line2 = lcHeader.Histories.FirstOrDefault(x => x.TotalCost == 20100m);

			AssertNotNull(line1);
			AssertNotNull(line2);

			AssertEquals(50m, line1.LH_LandedCostGroup1);
			AssertEquals("Total duty", 0m, line1.GetLineValue("TDT"));
			AssertEquals("users should be able to enter % themselves if automatic duty calc is not supported", false, line1.LH_DutyPercentInfo.ReadOnly);
			AssertEquals("Duty percent", 0m, line1.LH_DutyPercent);
			AssertEquals("Entry Fees", 0m, line1.GetLineValue("ENT").Round(2));

			AssertEquals(100m, line2.LH_LandedCostGroup1);
			AssertEquals("Total duty", 0m, line2.GetLineValue("TDT"));
			AssertEquals("users should be able to enter % themselves if automatic duty calc is not supported", false, line2.LH_DutyPercentInfo.ReadOnly);
			AssertEquals("Duty percent", 0m, line2.LH_DutyPercent);
			AssertEquals("Entry Fees", 0m, line2.GetLineValue("ENT").Round(2));

			line1.LH_DutyPercent = 10m;
			line2.LH_DutyPercent = 15m;
			new LCDistributionManager(lcHeader).RunLandedCosting();

			AssertEquals("PreCondition", 10m, line1.LH_DutyPercent);
			AssertEquals("PreCondition", 15m, line2.LH_DutyPercent);
			AssertEquals("Total duty should be calculated from %", 1000m, line1.GetLineValue("TDT"));
			AssertEquals(11050m, line1.TotalCost);

			AssertEquals("Total duty should be calculated from %", 3000m, line2.GetLineValue("TDT"));
			AssertEquals(23100m, line2.TotalCost);
		}
	}
}
