using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondNumberOrHolderCodes()
		{
			var irs = Factory.New<OrgCusCode>();
			irs.OK_CodeType = CodeTypeList.Codes.IRS;
			irs.OK_CustomsRegNo = "111";
			var cbp = Factory.New<OrgCusCode>();
			cbp.OK_CodeType = CodeTypeList.Codes.CBPAssignedNumber;
			cbp.OK_CustomsRegNo = "222";
			var socialSecurity = Factory.New<OrgCusCode>();
			socialSecurity.OK_CodeType = CodeTypeList.Codes.SocialSecurity;
			socialSecurity.OK_CustomsRegNo = "333";
			var encryptedConsigneeNumber = Factory.New<OrgCusCode>();
			encryptedConsigneeNumber.OK_CodeType = CodeTypeList.Codes.EncryptedConsigneeNumber;
			encryptedConsigneeNumber.OK_CustomsRegNo = "444";
			var list = Header.Lookups.BondNumberOrHolderCodes;
			Assert("irs code", list.Contains(irs));
			Assert("cbp code", list.Contains(cbp));
			Assert("social security code", list.Contains(socialSecurity));
			Assert("encrypted consignee number", !list.Contains(encryptedConsigneeNumber));
			AssertEquals(Header.Lookups.BondNumberOrHolderTypeRestriction, ((IActiveBusinessObjectCollection)Header.Lookups.BondNumberOrHolderCodes).GetAllNotificationsWhenAdditionalFilterNotMet(encryptedConsigneeNumber));
		}

		public void TestStaff()
		{
			AssertNotNull(Lookups.Staff);
		}

		public void TestBranches()
		{
			AssertEquals("Branches", typeof(GlbBranchNotCurrentCompanyRelatedCollection), Lookups.Branches.GetType());
		}

		public void TestPackingingUnitList()
		{
			var list = ShippingOrPackingingUnitList.GetWithPieceType(Factory);
			AssertEquals("PackingingUnitList", list, Header.Lookups.PackingingUnitList);
		}

		public void TestTransportModes()
		{
			AssertEquals(true, Header.Lookups.TransportModes.ContainsCode(TransportModeCodes.Codes.OceanVesselContainerized));
			AssertEquals(true, Header.Lookups.TransportModes.ContainsCode(TransportModeCodes.Codes.OceanVesselNonContainerized));
		}

		public void TestImporterCodeTypes()
		{
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			ImporterCodeTypeList list = Header.Lookups.ImporterCodeTypes;
			AssertEquals(4, list.Count);
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.CBPAssignedNumber));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.IRS));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.Passport));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.SocialSecurity));
			AssertEquals(false, list.ContainsCode(ImporterCodeTypeList.Codes.SCAC));
			Header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			list = Header.Lookups.ImporterCodeTypes;
			AssertEquals(5, list.Count);
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.CBPAssignedNumber));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.IRS));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.Passport));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.SocialSecurity));
			AssertEquals(true, list.ContainsCode(ImporterCodeTypeList.Codes.SCAC));
		}

		public void TestBondActivityCodeList()
		{
			ISFBondActivityCodeList list = Lookups.BondActivityCodeList;
			AssertEquals(5, list.Count);
			AssertEquals(true, list.ContainsCode(ISFBondActivityCodeList.Codes.ImporterOrBroker));
			AssertEquals(true, list.ContainsCode(ISFBondActivityCodeList.Codes.CustodianOfBondedMerchandise));
			AssertEquals(true, list.ContainsCode(ISFBondActivityCodeList.Codes.InternationalCarrier));
			AssertEquals(true, list.ContainsCode(ISFBondActivityCodeList.Codes.ForeignTradeZoneOperator));
			AssertEquals(true, list.ContainsCode(ISFBondActivityCodeList.Codes.ISFBond16));
			AssertEquals(false, list.ContainsCode(ISFBondActivityCodeList.Codes.ISFBond99));
		}

		public void TestShipmentTypes()
		{
			ShipmentTypeList list = Header.Lookups.ShipmentTypes;
			AssertEquals(11, list.Count);
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.StandardOrRegularFilings));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.ToOrderShipments));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.MilitaryAndGovernment));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.DiplomaticShipment));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.Carnet));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.USReturnGoods));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.FTZShipments));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.InternationalMailShipments));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.OuterContinentalShelfShipments));
			AssertEquals(true, list.ContainsCode(ShipmentTypeList.Codes.Informal));
		}

		public void TestEntryTypes()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_SystemCreateTimeUtc = new ZDateTime(2016, 2, 28);
			var list = header.Lookups.EntryTypes;
			AssertEquals(6, list.Count);
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF10));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF10ToISF5));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF5));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF5ToISF10));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.LateISF10));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.LateISF5));
			Factory.ClearCachedValue<SubmissionTypeList>("EntryTypeListWithout5and6");
			header.BF_SystemCreateTimeUtc = new ZDateTime(2016, 3, 1);
			list = header.Lookups.EntryTypes;
			AssertEquals(4, list.Count);
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF10));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF10ToISF5));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF5));
			AssertEquals(true, list.ContainsCode(SubmissionTypeList.Codes.ISF5ToISF10));
			AssertEquals(false, list.ContainsCode(SubmissionTypeList.Codes.LateISF10));
			AssertEquals(false, list.ContainsCode(SubmissionTypeList.Codes.LateISF5));
		}

		public void TestActionReasonCodeList()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ActionReasonCodeList list = header.Lookups.ActionReasonCodeList;
			AssertEquals(4, list.Count);
			AssertEquals(true, list.ContainsCode(ActionReasonCodeList.Codes.CompliantTransaction));
			AssertEquals(true, list.ContainsCode(ActionReasonCodeList.Codes.FlexibleRange));
			AssertEquals(true, list.ContainsCode(ActionReasonCodeList.Codes.FlexibleTiming));
			AssertEquals(true, list.ContainsCode(ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming));
		}

		public void TestConsigneeList()
		{
			AssertEquals(CodeTypeList.Descriptions.IRS, Lookups.ConsigneeCodeTypes.GetDescriptionFromCode(CodeTypeList.Codes.IRS));
			AssertEquals(CodeTypeList.Descriptions.SocialSecurity, Lookups.ConsigneeCodeTypes.GetDescriptionFromCode(CodeTypeList.Codes.SocialSecurity));
			AssertEquals(CodeTypeList.Descriptions.CBPAssignedNumber, Lookups.ConsigneeCodeTypes.GetDescriptionFromCode(CodeTypeList.Codes.CBPAssignedNumber));
			AssertEquals(CodeTypeList.Descriptions.EncryptedConsigneeNumber, Lookups.ConsigneeCodeTypes.GetDescriptionFromCode(CodeTypeList.Codes.EncryptedConsigneeNumber));
			AssertEquals(CodeTypeList.Descriptions.Passport, Lookups.ConsigneeCodeTypes.GetDescriptionFromCode(CodeTypeList.Codes.Passport));
		}

		[StressTest]
		public void TestUSCarrierList()
		{
			List<USCarrierCombined> validCarriers = new List<USCarrierCombined>();
			int count = 0;
			TransportModeCodes transportList = new TransportModeCodes();
			foreach (CodeDescriptionPair pair in transportList)
			{
				USCarrierCombined carrier = Factory.New<USCarrierCombined>();
				carrier.UI_Code = count.ToString().PadLeft(4, 'Z');
				carrier.UI_ModeOfTransportation = pair.Code;
				validCarriers.Add(carrier);
				count++;
			}

			AssertEquals("PreCondition", false, transportList.ContainsCode("33"));
			USCarrierCombined invalidCarrier = Factory.New<USCarrierCombined>();
			invalidCarrier.UI_Code = "1ZZZ";
			invalidCarrier.UI_ModeOfTransportation = "33";
			USCarrierCombined applyToTransportTypeCarrier = Factory.New<USCarrierCombined>();
			applyToTransportTypeCarrier.UI_Code = "2ZZZ";
			applyToTransportTypeCarrier.UI_ModeOfTransportation = "00";
			Factory.Save();
			USCarrierCombinedCollection uSCarrierList = Lookups.USCarrierList;
			validCarriers.ForEach((USCarrierCombined carrier) =>
			{
				AssertCollectionContains(carrier, uSCarrierList);
			});
			AssertCollectionNotContains(invalidCarrier, uSCarrierList);
			AssertCollectionContains(applyToTransportTypeCarrier, uSCarrierList);
		}

		CusISFHeaderLookups Lookups => Header.Lookups;

		CusISFHeader header;
		CusISFHeader Header => header ?? (header = Factory.New<CusISFHeader>());
	}
}
