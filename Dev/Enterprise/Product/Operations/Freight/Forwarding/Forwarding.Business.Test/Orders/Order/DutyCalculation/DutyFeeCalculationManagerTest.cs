using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class DutyFeeCalculationManagerTest : TestCaseWithFactory
	{
		[TestDate(2014, 6, 25)]
		public void TestEndToEndForAU()
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

			order.ApportionChargesIfNecessary();

			LCDutyFeeCalculationManager manager = new LCDutyFeeCalculationManager(order);
			manager.Calculate();

			DutyTaxEntryFee dutyFee = manager.GetDutyTaxEntryFeeForLine(orderLine);

			AssertEquals("Duty amount", 989m, dutyFee["TDT"]);
			AssertEquals("Entry Fee", 81m, dutyFee["ENT"]);
		}
	}
}
