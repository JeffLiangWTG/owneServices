using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UNDGDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWrite()
		{
			var contactBO = Factory.New<OrgContact>();
			contactBO.OC_ContactName = "ABCDE AABBCCDDEE";
			contactBO.OC_Phone = "+61 1234 5678";

			var undgSubstanceBO = Factory.New<UNDGSubstance>();
			undgSubstanceBO.DG_UNNO = "9999";
			undgSubstanceBO.DG_Class = "9.9Z";
			undgSubstanceBO.DG_FlashPoint = "27.0 c.c";
			undgSubstanceBO.DG_MP = "C";
			undgSubstanceBO.DG_PG = "III";
			undgSubstanceBO.DG_PSN = "NUCLEAR BOMB";
			undgSubstanceBO.DG_SubLabel1 = "TEST";
			undgSubstanceBO.DG_SubLabel2 = "AAA";
			undgSubstanceBO.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstanceBO.DG_State = UNDGSubstanceLookups.StateTypes.Code.Liquid;
			undgSubstanceBO.DG_EMS = "F-I,S-S";

			var today = ZDate.Today;
			var undgDataItemBO = Factory.New<UNDGDataItem>();
			undgDataItemBO.DI_DG = undgSubstanceBO.PK;
			undgDataItemBO.DI_OC_DGContact = contactBO.PK;
			undgDataItemBO.DI_TechnicalName = "TERRAFORM";
			undgDataItemBO.DI_DGFlashPoint = 11.0m;
			undgDataItemBO.DI_MPMarinePollutant = "Y";
			undgDataItemBO.DI_DGVolume = 1m;
			undgDataItemBO.DI_UnitOfVolume = "M3";
			undgDataItemBO.DI_DGWeight = 564m;
			undgDataItemBO.DI_UnitOfWeight = "KG";
			undgDataItemBO.DI_IsLimitedQuantity = true;
			undgDataItemBO.DI_PackageCount = 5;
			undgDataItemBO.DI_F3_NKPackType = "BAG";
			undgDataItemBO.DI_PackingInstructionSection = "II";
			undgDataItemBO.DI_HasOverpack = true;
			undgDataItemBO.DI_OverpackID = "111";
			undgDataItemBO.DI_HazardousWasteCode = "222";
			undgDataItemBO.DI_SpecialPermitIssueDate = today;
			undgDataItemBO.DI_SpecialPermitNumber = "333";
			undgDataItemBO.DI_IsSalvagePackaging = true;
			undgDataItemBO.DI_IsResidueLastContained = true;
			undgDataItemBO.DI_RadionuclideElement = "U";
			undgDataItemBO.DI_RadionuclideElementSuffix = "230";
			undgDataItemBO.DI_RadioactiveMaximumActivity = 6.9m;
			undgDataItemBO.DI_RadioactiveMaximumActivityUnit = "TBQ";
			undgDataItemBO.DI_RadioactiveLabelCategory = "WH1";
			undgDataItemBO.DI_RadioactiveTransportIndex = 4.2m;
			undgDataItemBO.DI_MaterialFormDescription = "New radioactive isotope, what do you think?";
			undgDataItemBO.DI_IsFissileExcepted = true;
			undgDataItemBO.DI_IsExclusiveUse = true;
			undgDataItemBO.DI_IsHighwayRouteControlledQuantity = true;

			var writer = new UNDGDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, undgDataItemBO)));
			var undgData = writer.GetDataObject(undgDataItemBO);

			CombineAssertions(delegate
			{
				AssertEquals("undgData.EmergencyScheduleFire.Code", EmergencyScheduleFireCodes.Codes.F_I, undgData.EmergencyScheduleFire.Code);
				AssertEquals("undgData.EmergencyScheduleFire.Description", EmergencyScheduleFireCodes.Descriptions.F_I, undgData.EmergencyScheduleFire.Description);
				AssertEquals("undgData.EmergencyScheduleSpillage.Code", EmergencyScheduleSpillageCodes.Codes.S_S, undgData.EmergencyScheduleSpillage.Code);
				AssertEquals("undgData.EmergencyScheduleSpillage.Description", EmergencyScheduleSpillageCodes.Descriptions.S_S, undgData.EmergencyScheduleSpillage.Description);
				AssertEquals("undgData.Contact.FullName", "ABCDE AABBCCDDEE", undgData.Contact.FullName);
				AssertEquals("undgData.Contact.Phone", "+61 1234 5678", undgData.Contact.Phone);
				AssertEquals("undgData.FlashPoint", "11.0", undgData.FlashPoint);
				AssertEquals("undgData.IMOClass", "9.9Z", undgData.IMOClass);
				AssertEquals("undgData.MarinePollutant", "Y", undgData.MarinePollutant.Code);
				AssertEquals("undgData.MarinePollutant", "Marine Pollutant", undgData.MarinePollutant.Description);
				AssertEquals("undgData.PackedInLimitedQuantity", true, undgData.PackedInLimitedQuantity);
				AssertEquals("undgData.PackingGroup", "III", undgData.PackingGroup);
				AssertEquals("undgData.ProperShippingName", "NUCLEAR BOMB", undgData.ProperShippingName);
				AssertEquals("undgData.TechicalName", "TERRAFORM", undgData.TechicalName);
				AssertEquals("undgData.UNDGCode", "9999", undgData.UNDGCode);
				AssertEquals("undgData.Standard", "IAT", undgData.Standard);
				AssertEquals("undgData.Volume", 1m, undgData.Volume);
				AssertEquals("undgData.VolumeUQ", "M3", undgData.VolumeUQ.Code);
				AssertEquals("undgData.Weight", 564m, undgData.Weight);
				AssertEquals("undgData.WeightUQ", "KG", undgData.WeightUQ.Code);
				AssertEquals("undgData.SubLabel1", "TEST", undgData.SubLabel1);
				AssertEquals("undgData.SubLabel2", "AAA", undgData.SubLabel2);
				AssertEquals("undgData.PackQty", 5, undgData.PackQty);
				AssertEquals("undgData.PackType.Code", "BAG", undgData.PackType.Code);
				AssertEquals("undgData.PackType.Description", "Bag", undgData.PackType.Description);
				AssertEquals("undgData.PackingInstructionSection", "II", undgData.PackingInstructionSection);
				AssertEquals("undgData.State", UNDGState.Liquid, undgData.State);
				AssertEquals("undgData.HasOverpack", true, undgData.HasOverpack);
				AssertEquals("undgData.OverpackID", "111", undgData.OverpackID);
				AssertEquals("undgData.WasteCode", "222", undgData.WasteCode);
				AssertEquals("undgData.SpecialPermitIssuedDate", today, undgData.SpecialPermitIssuedDate);
				AssertEquals("undgData.SpecialPermitNumber", "333", undgData.SpecialPermitNumber);
				AssertEquals("undgData.SalvagePackaging", true, undgData.SalvagePackaging);
				AssertEquals("undgData.ResidueLastContained", true, undgData.ResidueLastContained);
				AssertEquals("undgData.RadionuclideElement", "U", undgData.RadionuclideElement);
				AssertEquals("undgData.RadionuclideElementSuffix", "230", undgData.RadionuclideElementSuffix);
				AssertEquals("undgData.RadioactiveMaximumActivity", 6.9m, undgData.RadioactiveMaximumActivity);
				AssertEquals("undgData.RadioactiveMaximumActivityUnit", "TBQ", undgData.RadioactiveMaximumActivityUnit);
				AssertEquals("undgData.RadioactiveLabelCategory", "WH1", undgData.RadioactiveLabelCategory);
				AssertEquals("undgData.RadioactiveTransportIndex", 4.2m, undgData.RadioactiveTransportIndex);
				AssertEquals("undgData.Description", "New radioactive isotope, what do you think?", undgData.Description);
				AssertEquals("undgData.FissileExcepted", true, undgData.FissileExcepted);
				AssertEquals("undgData.ExclusiveUse", true, undgData.ExclusiveUse);
				AssertEquals("undgData.HighwayRouteControlledQuantity", true, undgData.HighwayRouteControlledQuantity);
			});

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				undgDataItemBO.DI_IsCombustible = false;
				undgData = writer.GetDataObject(undgDataItemBO);
				AssertEquals("undgData.IsCombustible", false, undgData.IsCombustible);

				undgDataItemBO.DI_IsCombustible = true;
				undgData = writer.GetDataObject(undgDataItemBO);
				AssertEquals("undgData.IsCombustible", true, undgData.IsCombustible);
			}
		}

		public void TestWriteEmptyDG_EMS()
		{
			var contactBO = Factory.New<OrgContact>();
			contactBO.OC_ContactName = "ABCDE AABBCCDDEE";
			contactBO.OC_Phone = "+61 1234 5678";

			var undgSubstanceBO = Factory.New<UNDGSubstance>();
			undgSubstanceBO.DG_UNNO = "9999";
			undgSubstanceBO.DG_Class = "9.9Z";
			undgSubstanceBO.DG_FlashPoint = "27.0 c.c";
			undgSubstanceBO.DG_MP = "C";
			undgSubstanceBO.DG_PG = "III";
			undgSubstanceBO.DG_PSN = "NUCLEAR BOMB";
			undgSubstanceBO.DG_SubLabel1 = "TEST";
			undgSubstanceBO.DG_SubLabel2 = "AAA";
			undgSubstanceBO.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstanceBO.DG_State = UNDGSubstanceLookups.StateTypes.Code.Liquid;
			undgSubstanceBO.DG_EMS = ZString.Empty;

			var today = ZDate.Today;
			var undgDataItemBO = Factory.New<UNDGDataItem>();
			undgDataItemBO.DI_DG = undgSubstanceBO.PK;
			undgDataItemBO.DI_OC_DGContact = contactBO.PK;
			undgDataItemBO.DI_TechnicalName = "TERRAFORM";
			undgDataItemBO.DI_DGFlashPoint = 11.0m;
			undgDataItemBO.DI_MPMarinePollutant = "Y";
			undgDataItemBO.DI_DGVolume = 1m;
			undgDataItemBO.DI_UnitOfVolume = "M3";
			undgDataItemBO.DI_DGWeight = 564m;
			undgDataItemBO.DI_UnitOfWeight = "KG";
			undgDataItemBO.DI_IsLimitedQuantity = true;
			undgDataItemBO.DI_PackageCount = 5;
			undgDataItemBO.DI_F3_NKPackType = "BAG";
			undgDataItemBO.DI_PackingInstructionSection = "II";
			undgDataItemBO.DI_HasOverpack = true;
			undgDataItemBO.DI_OverpackID = "111";
			undgDataItemBO.DI_HazardousWasteCode = "222";
			undgDataItemBO.DI_SpecialPermitIssueDate = today;
			undgDataItemBO.DI_SpecialPermitNumber = "333";
			undgDataItemBO.DI_IsSalvagePackaging = true;
			undgDataItemBO.DI_IsResidueLastContained = true;

			var writer = new UNDGDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, undgDataItemBO)));
			var undgData = writer.GetDataObject(undgDataItemBO);

			CombineAssertions(delegate
			{
				AssertNull(undgData.EmergencyScheduleFire);
				AssertNull(undgData.EmergencyScheduleSpillage);
			});
		}

		public void TestWriteClassOnlyUNDG()
		{
			var undgDataItemBO = Factory.New<UNDGDataItem>();
			undgDataItemBO.DI_IMOClass = "1.1D";

			var undgData = new UNDGDataObjectWriter(new DataWritingManager(new ActionInfo(null, undgDataItemBO))).GetDataObject(undgDataItemBO);
			AssertEquals("1.1D", undgData.IMOClass);
		}
	}
}
