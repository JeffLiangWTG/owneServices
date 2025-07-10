using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class AUDutyDataWrapperTest : TestCaseWithFactory
	{
		public void TestEndToEndTest()
		{
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
			pivot[CusClassPartPivotSchema.CI_OP] = product.PK;
			pivot[CusClassPartPivotSchema.CI_ChildType] = "HTI";
			pivot[CusClassPartPivotSchema.CI_CC] = classification.PK;
			pivot[CusClassPartPivotSchema.CI_AddInfo] = "GSTE=FOOD*PST=CA";
			Factory.Save();

			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_Milestone_E_ARV = new ZDateTime(2009, 1, 1);
			order.JD_RL_NKGoodsDeliveredTo = "AUSYD";

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Partno = "Canadian Wine";
			orderLine.JO_Quantity = 10;
			orderLine.JO_F3_NKPackType = "BOX";

			AssertEquals("PreCondition", product, orderLine.Product);

			ICMRDutyData dutyData = new AUDutyDataWrapper(orderLine, false, Factory, delegate
			{ }, delegate
			{ });
			Assert(dutyData.IsSubjectToDutyAndTax);
			Assert(dutyData.IsNotLowValueShipment);

			DutyDataFromInvoiceLine lineDutyData = dutyData.RandomLineDutyData;
			AssertEquals("22030039", lineDutyData.FirstTariffNumber);
			AssertEquals("26", lineDutyData.StatCode);
			AssertEquals(new ZDateTime(2009, 1, 1), lineDutyData.EffectiveDutyDate);
			AssertEquals("LA", lineDutyData.FirstUQ);
			AssertEquals("L", lineDutyData.SecondUQ);
			AssertEquals("CA", lineDutyData.Preference);
			AssertEquals(true, lineDutyData.IsGSTExempt);

			AssertEquals(144m, dutyData.FirstQty);
			AssertEquals(14400m, dutyData.SecondQty);

			ICustomsDetails customsDetails = (ICustomsDetails)dutyData;
			AssertEquals(144m, customsDetails.CustomsQuantity);
			AssertEquals("LA", customsDetails.CustomsUQ);
			AssertEquals("22030039 26", customsDetails.TariffNumber);
		}

		public void TestIUnitConverterDataProviderType()
		{
			var wrapper = new AUDutyDataWrapper(null, false, Factory, delegate
			{ }, delegate
			{ });
			AssertEquals("Pack Conversion Type should be All Areas", RPTypeList.Codes.AllAreas, ((IUnitConverterDataProvider)wrapper).Type);
		}
	}
}
