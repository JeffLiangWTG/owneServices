using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class AUChargeDataProviderWrapperTest : TestCaseWithFactory
	{
		public void TestCustomsValue()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = buyer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;

			var order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = "AUD";
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LinePrice = 10m;

			IDeclarationChargeProvider wrapper = new AUChargeDataProviderWrapper(order, delegate
			{ }, delegate
			{ });
			AssertEquals(10m, wrapper.N10CustomsValue);
			AssertEquals(ZDecimal.Zero, wrapper.N20CustomsValue);
			AssertEquals(ZDecimal.Zero, wrapper.N30CustomsValue);
			Assert(wrapper.IsExemptedFromCustomsAndQuarantineFees);
		}

		public void TestDutyFeeCalculationEndToEnd()
		{
			IAUDutyCalculationManager temp = ObjectFactory.Get<IAUDutyCalculationManager>();
			temp.CreateTaxOrFee();

			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Canadian Wine";
			OrgPartUnit partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "BOT";
			partUnit.OF_ParentPackType = "BOX";
			partUnit.OF_QuantityInParent = 12;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "L";
			partUnit.OF_ParentPackType = "BOT";
			partUnit.OF_QuantityInParent = 120m;

			partUnit = product.PartUnits.AddNew();
			partUnit.OF_PackType = "LA";
			partUnit.OF_ParentPackType = "BOT";
			partUnit.OF_QuantityInParent = 1.20m;

			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = buyer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			BusinessObject classification = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IClassification>());
			classification[CusClassificationSchema.CC_RN_NKCountryCode] = Core.Constants.CountryCodes.Australia;
			classification[CusClassificationSchema.CC_TariffNum] = "2203.00.39 26";
			classification[CusClassificationSchema.CC_LookupCode] = "Wine";
			classification[CusClassificationSchema.CC_Description] = "Wine";
			classification[CusClassificationSchema.CC_ClassificationType] = "IMP";

			BusinessObject pivot = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusClassPartPivot>());
			pivot[CusClassPartPivotSchema.CI_RN_NKCountry] = Core.Constants.CountryCodes.Australia;
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_AddInfo] = "GSTE=FOOD";
			Factory.Save();

			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_Milestone_E_ARV = new ZDateTime(2009, 1, 1);
			order.JD_RX_NKOrderCurrency = "AUD";
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Partno = "Canadian Wine";
			orderLine.JO_Quantity = 10;
			orderLine.JO_F3_NKPackType = "BOX";
			orderLine.JO_LinePrice = 10000m;

			LCDutyFeeCalculationManager manager = new LCDutyFeeCalculationManager(order);
			manager.Calculate();

			DutyResult dutyResult = manager.GetDutyResult(orderLine.PK);
			AssertEquals(9020.16m, dutyResult.Amount.Amount);
			AssertEquals(15m, manager.GetFeeResult(order.PK, Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISProcessingCharge));
		}
	}
}
