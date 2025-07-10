using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.UniversalDataBuss.Integration;
using CustomsChargeType = Enterprise.Customs.ASYCUDA.Business.Constants.CustomsChargeType;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	public partial class AsycudaWriterTest : ASYCUDA.Business.UniversalDataTransfer.Testing.AsycudaWriterTestHelper
	{
		public void TestExportCustomsPort()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				PrepareCusCodeDataForTesting();
				var source = ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("SGVLI", "MGI", Factory.BOFactory);
				source.AMA_CustomsLoadPort = "AUSDY";
				source.AMA_CustomsDischargePort = "SGVLI";
				Factory.SaveForTesting();
				var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
				AssertEquals("AUSDY", headerData.CustomsLoadPort.Code);
				AssertEquals("desc1", headerData.CustomsLoadPort.Description);
				AssertEquals("SGVLI", headerData.CustomsDischargePort.Code);
				AssertEquals("desc2", headerData.CustomsDischargePort.Description);
			}
		}

		public void TestExportSGBill()
		{
			PrepareCusCodeDataForTesting();
			ASYCUDA.Business.Testing.AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "AAA", "SG", Factory.BOFactory);
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "recipient.user@forwarder.com";
			Factory.SaveForTesting();
			var source = ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("SGVLI", "MGI", Factory.BOFactory);
			var bill = source.Bills[0];
			bill.ABL_ManifestUQ = "DJC";
			bill.SG_PartyID = "PartyID";
			bill.SG_PayeeIndicator = SGPayeeIndicatorList.Codes.Q;
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			var pack = source.Bills[0].Packs.AddNew();
			pack.APA_PackUQ = "DJC";
			pack.ContainerPK = source.Containers[0].PK;
			Factory.SaveForTesting();
			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			var billData = headerData.SubShipmentCollection[0];
			AssertEquals("ABL_GoodsDescription", "Books", billData.GoodsDescription);
			AssertEquals("ABL_ManifestQty", 5, billData.OuterPacks);
			AssertEquals("ABL_RL_NKFinalDestination", "SGVLI", billData.PortOfDestination.Code);
			AssertEquals("ABL_RL_NKOrigin", "USATL", billData.PortOfOrigin.Code);
			var payeeIndicatorAddInfo = billData.AddInfoCollection.First(x => x.Key.GetValueOrDefault() == AddInfoConstants.BillCountry.PayeeIndicator);
			var partyStatusAddInfo = billData.AddInfoCollection.First(x => x.Key.GetValueOrDefault() == AddInfoConstants.BillCountry.PartyStatus);
			var partyIDAddInfo = billData.AddInfoCollection.First(x => x.Key.GetValueOrDefault() == AddInfoConstants.BillCountry.PartyIndicator);
			AssertEquals("SG_PayeeIndicator", SGPayeeIndicatorList.Codes.Q, payeeIndicatorAddInfo.Value);
			AssertEquals("SG_PartyStatus", SGPartyStatusList.Codes.A, partyStatusAddInfo.Value);
			AssertEquals("SG_PartyID", "PartyID", partyIDAddInfo.Value);
		}

		public void TestRemarks()
		{
			/*
			 *	create a manifest without any value in ABL_Remarks - ensure no Remarks element is generated in UXML
			 *	Update manifest remarks to have a value - ensure remarks value is still output in generated UXML
			 */
			PrepareCusCodeDataForTesting();
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "recipient.user@forwarder.com";
			Factory.SaveForTesting();
			var source = ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("SGVLI", "MGI", Factory.BOFactory);
			var bill = source.Bills[0];
			bill.ABL_ManifestUQ = "DJC";
			bill.ABL_Remarks = "";
			bill.SG_PartyID = "PartyID";
			bill.SG_PayeeIndicator = SGPayeeIndicatorList.Codes.Q;
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			var pack = source.Bills[0].Packs.AddNew();
			pack.APA_PackUQ = "DJC";
			pack.ContainerPK = source.Containers[0].PK;
			Factory.SaveForTesting();
			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			var billData = headerData.SubShipmentCollection[0];
			AssertNull("NoteCollection should not be generated when no remarks are entered.", billData.NoteCollection);
			bill.ABL_Remarks = "This is to be included only when a value is entered here";
			Factory.SaveForTesting();
			headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			billData = headerData.SubShipmentCollection[0];
			AssertNotNull("NoteCollection should now be generated as remarks have been entered.", billData.NoteCollection);
			var note = billData.NoteCollection[0];
			AssertEquals("Remarks NoteText", "This is to be included only when a value is entered here", note.NoteText);
		}

		public void TestCommercialCharges()
		{
			PrepareCusCodeDataForTesting();
			ASYCUDA.Business.Testing.AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "AAA", "SG", Factory.BOFactory);
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_EmailAddress = "recipient.user@forwarder.com";
			Factory.SaveForTesting();
			var source = ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting<AsycudaManifestHeader>("SGVLI", "MGI", Factory.BOFactory);
			var bill = source.Bills[0];
			bill.ABL_ManifestUQ = "DJC";
			bill.ABL_FreightValue = 110m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Australia;
			bill.ABL_InsuranceValue = 220m;
			bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Bahamas;
			bill.ABL_TransportValue = 330m;
			bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.Cambodia;
			bill.ABL_CustomsValue = 440m;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Denmark;
			bill.DiscountValue = 550m;
			bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.EastTimor;
			bill.OtherChargesValue = 660m;
			bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.FalklandIslands;
			bill.SG_PartyID = "PartyID";
			bill.SG_PayeeIndicator = SGPayeeIndicatorList.Codes.Q;
			bill.SG_PartyStatus = SGPartyStatusList.Codes.A;
			bill.TaxAmount = 10m;
			bill.DutyAmount = 30m;
			var pack1 = source.Bills[0].Packs.AddNew();
			pack1.APA_PackUQ = "DJC";
			pack1.ContainerPK = source.Containers[0].PK;
			bill.ApportionmentDirty = false;
			Factory.SaveForTesting();
			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			var billData = headerData.SubShipmentCollection[0];
			var dutyAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeType.CustomsDutyCode);
			var gSTAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeType.GSTCode);
			var exWorksAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeTypeList.Codes.ExWorks);
			var transportAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeTypeList.Codes.OverseasFreight);
			var insuranceAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeTypeList.Codes.OverseasInsurance);
			var customsAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeType.CustomsChargeCode);
			var otherChargesAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeTypeList.Codes.OtherCharges);
			var discountAmount = billData.CommercialInfo.CommercialChargeCollection.First(x => x.ChargeType.Code.GetValueOrDefault() == CustomsChargeTypeList.Codes.Discount);
			AssertEquals("Singapore dollar", "SGD", dutyAmount.Currency.Code);
			AssertEquals(30m, dutyAmount.Amount);
			AssertEquals("Singapore dollar", "SGD", gSTAmount.Currency.Code);
			AssertEquals(10m, gSTAmount.Amount);
			AssertEquals("ExWorks", Core.Constants.CurrencyCodes.Australia, exWorksAmount.Currency.Code);
			AssertEquals(110m, exWorksAmount.Amount);
			AssertEquals("OverseaInsurance", Core.Constants.CurrencyCodes.Bahamas, insuranceAmount.Currency.Code);
			AssertEquals(220m, insuranceAmount.Amount);
			AssertEquals("OverseaFreight", Core.Constants.CurrencyCodes.Cambodia, transportAmount.Currency.Code);
			AssertEquals(330m, transportAmount.Amount);
			AssertEquals("CustomsChargeCode", Core.Constants.CurrencyCodes.Denmark, customsAmount.Currency.Code);
			AssertEquals(440m, customsAmount.Amount);
			AssertEquals("Discount", Core.Constants.CurrencyCodes.EastTimor, discountAmount.Currency.Code);
			AssertEquals(550m, discountAmount.Amount);
			AssertEquals("OtherCharges", Core.Constants.CurrencyCodes.FalklandIslands, otherChargesAmount.Currency.Code);
			AssertEquals(660m, otherChargesAmount.Amount);
		}
	}
}
