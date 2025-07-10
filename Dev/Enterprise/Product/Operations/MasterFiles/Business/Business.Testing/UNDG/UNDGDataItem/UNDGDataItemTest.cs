using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGDataItem))]
	sealed class UNDGDataItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDI_ParenIDAndDI_ParentTableCodeArePresentInCloneAction()
		{
			var obj = Factory.NewWithValidTestData<UNDGDataItem>();
			Assert(!obj.DI_ParentID.IsEmpty);
			AssertNotNullOrEmpty(obj.DI_ParentTableCode);

			var clonedObj = (UNDGDataItem)obj.Clone();
			AssertEquals(obj.DI_ParentID, clonedObj.DI_ParentID);
			AssertEquals(obj.DI_ParentTableCode, clonedObj.DI_ParentTableCode);
		}

		public void TestSubstancePKWhenCopyPersistentValuesFrom()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1234";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_LQMaxAmt = 250;
			subs.DG_LQMaxAmtUQ = "L";

			var item = Factory.New<UNDGDataItem>();
			item.SubstancePK = subs.PK;
			Factory.Save();
			AssertNotEquals("Precondition", Guid.Empty, item.DI_DG);

			var item2 = Factory.New<UNDGDataItem>();
			item2.CopyPersistentValuesFrom(item);
			AssertNotEquals(ZGuid.Empty, item2.DI_DG);
		}

		public void TestCloneWhenAttachedToZZUNDGSubstanceWithDG_UNNOEmpty()
		{
			var dgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			dgSubstance.DG_UNNO = "";
			var dgItem = Factory.NewWithValidTestData<UNDGDataItem>();
			dgItem.DI_DG = dgSubstance.PK;
			var pivot = Factory.Load<UNDGSubstancePivot>(new ZQuery(UNDGSubstancePivotSchema.DP_ParentId, dgItem.PK));
			pivot.DeleteAll();
			AssertNoExceptionThrown("precondition: UNDGDataItem can be attached to a substance with DG_UNNO as ''", () => Factory.Save());
			var clonedDg = (UNDGDataItem)dgItem.Clone();
			AssertNoExceptionThrown("cloned item can be saved (no default pivot with DP_UNNO as '' is created)", () => Factory.Save());
		}

		public void TestIMOLimitedQuantity_Volume()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1234";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_LQMaxAmt = 250;
			subs.DG_LQMaxAmtUQ = "L";
			Factory.Save();
			AssertValidateIsLimitedQuantity_Volume(subs);
		}

		public void TestCFRLimitedQuantity_Volume()
		{
			var subs = Factory.New<UNDGSubstanceCFR>();
			subs.CFR_UNNO = "1234";
			subs.CFR_Variant = "a";
			subs.CFR_LQMaxAmt = 250;
			subs.CFR_LQMaxAmtUQ = "L";
			Factory.Save();
			AssertValidateIsLimitedQuantity_Volume(subs);
		}

		void AssertValidateIsLimitedQuantity_Volume(IDGSubstance subs)
		{
			var item = Factory.New<UNDGDataItem>();
			AssertEquals("No substance", false, item.DI_IsLimitedQuantity);

			item.LinkDefault(subs);
			AssertEquals("No dimensions", false, item.DI_IsLimitedQuantity);

			item.DI_DGVolume = 100m;
			AssertEquals("No units specified", false, item.DI_IsLimitedQuantity);

			item.DI_UnitOfVolume = "L";
			AssertEquals("100 L < 250 L", true, item.DI_IsLimitedQuantity);

			item.DI_DGVolume = 300m;
			AssertEquals("300 L > 250 L", false, item.DI_IsLimitedQuantity);

			item.DI_DGVolume = 1;
			AssertEquals("1 L < 250 L", true, item.DI_IsLimitedQuantity);

			item.DI_UnitOfVolume = "ML";
			AssertEquals("1 ML > 250 L", false, item.DI_IsLimitedQuantity);

			item.DI_UnitOfVolume = "XX";
			AssertEquals("Invalid unit", false, item.DI_IsLimitedQuantity);
		}

		public void TestIMOLimitedQuantity_Weight()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1234a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_LQMaxAmt = 250;
			subs.DG_LQMaxAmtUQ = "G";
			Factory.Save();
			AssertValidateIsLimitedQuantity_Weight(subs);
		}

		public void TestCFRLimitedQuantity_Weight()
		{
			var subs = Factory.New<UNDGSubstanceCFR>();
			subs.CFR_UNNO = "1234";
			subs.CFR_Variant = "a";
			subs.CFR_LQMaxAmt = 250;
			subs.CFR_LQMaxAmtUQ = "G";
			Factory.Save();
			AssertValidateIsLimitedQuantity_Weight(subs);
		}

		void AssertValidateIsLimitedQuantity_Weight(IDGSubstance subs)
		{
			var item = Factory.New<UNDGDataItem>();
			AssertEquals("No substance", false, item.DI_IsLimitedQuantity);

			item.LinkDefault(subs);
			AssertEquals("No dimensions", false, item.DI_IsLimitedQuantity);

			item.DI_DGWeight = 100m;
			AssertEquals("No units specified", false, item.DI_IsLimitedQuantity);

			item.DI_UnitOfWeight = "G";
			AssertEquals("100 G < 250 G", true, item.DI_IsLimitedQuantity);

			item.DI_DGWeight = 300m;
			AssertEquals("300 ML > 250 ML", false, item.DI_IsLimitedQuantity);

			item.DI_DGWeight = 1;
			AssertEquals("1 G < 250 G", true, item.DI_IsLimitedQuantity);

			item.DI_UnitOfWeight = "KG";
			AssertEquals("1 KG > 250 G", false, item.DI_IsLimitedQuantity);

			item.DI_UnitOfWeight = "XX";
			AssertEquals("Invalid unit", false, item.DI_IsLimitedQuantity);
		}

		public void TestDI_IsLimitedQuantity()
		{
			var item = Factory.New<UNDGDataItem>();
			AssertEquals("Type is NULL", false, item.DI_IsLimitedQuantity);

			item.DI_QuantityClassification = "LIM";
			AssertEquals("Type is LIMITED", true, item.DI_IsLimitedQuantity);

			item.DI_QuantityClassification = "REG";
			AssertEquals("Type is REGULATED", false, item.DI_IsLimitedQuantity);

			item.DI_QuantityClassification = "EXC";
			AssertEquals("Type is EXCEPTED", false, item.DI_IsLimitedQuantity);
		}

		public void TestDI_TechnicalName_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "MMM";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			UNDGDataItem item = Factory.New<UNDGDataItem>();

			AssertEquals("should be readonly when it doesn't have a substance", true, item.DI_TechnicalName_ReadOnly);

			item.LinkDefault(subs);
			AssertEquals("should not be readonly when the substance doesn't have TechNameCheckBox true", false, item.DI_TechnicalName_ReadOnly);
		}

		public void TestDI_TechnicalName_Under_MaxLength()
		{
			var item = Factory.New<UNDGDataItem>();
			try
			{
				AssertNoExceptionThrown(() =>
					item.DI_TechnicalName = ZString.Replicate('A', 150));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestDI_TechnicalName_Exceeds_MaxLength()
		{
			var item = Factory.New<UNDGDataItem>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(()
					=> item.DI_TechnicalName = ZString.Replicate('A', 151));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestDI_MPMarinePollutant_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "MMM";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			UNDGDataItem item = Factory.New<UNDGDataItem>();

			AssertEquals("should be readonly when it doesn't have a substance", true, item.DI_MPMarinePollutant_ReadOnly);

			item.LinkDefault(subs);
			AssertEquals("should be readonly when the substance doesn't have a DG_MP", false, item.DI_MPMarinePollutant_ReadOnly);

			item.Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code;
			AssertEquals("should be readonly when the substance isn't C", false, item.DI_MPMarinePollutant_ReadOnly);

			item.Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			AssertEquals("should be readonly when the substance isn't Y", false, item.DI_MPMarinePollutant_ReadOnly);

			item.Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			AssertEquals("should not be readonly when the substance is C (Depends)", false, item.DI_MPMarinePollutant_ReadOnly);
		}

		public void TestApprovalCertificate_ReadOnly()
		{
			var item = Factory.New<UNDGDataItem>();
			AssertEquals(true, item.DI_ApprovalCertificateType_ReadOnly);
			AssertEquals(true, item.DI_ApprovalCertificateIDMark_ReadOnly);
		}

		public void TestDI_MPMarinePollutant()
		{
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "MMM";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			item.DI_MPMarinePollutant = "X";
			AssertEquals("pollutant should be blank when the substance is null", ZString.Empty, item.DI_MPMarinePollutant);

			item.DI_DG = subs.PK;
			item.LinkDefault(subs);
			item.Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			AssertEquals("pollutant should show item MP when the substance is DEPENDS", "", item.DI_MPMarinePollutant);

			item.DI_MPMarinePollutant = "Z";
			AssertEquals("pollutant should show item MP when the substance is DEPENDS", "Z", item.DI_MPMarinePollutant);

			item.Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code;
			AssertEquals("pollutant should show MP from substance when the substance is NOT DEPENDS", "Z", item.DI_MPMarinePollutant);

			item.Substance.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code;
			AssertEquals("pollutant should show MP from substance when the substance is NOT DEPENDS", "Z", item.DI_MPMarinePollutant);
		}

		public void TestIsEmptyItem()
		{
			var item = Factory.New<UNDGDataItem>();
			AssertEquals("Precondition", true, item.IsEmptyItem);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "ABC";
			item.LinkDefault(subs);
			AssertEquals("Substance", false, item.IsEmptyItem);
			var pivot = item.UNDGSubstancePivotCollection.FirstOrDefault(substancePivot => substancePivot.DP_IsDefault);
			item.UNDGSubstancePivotCollection.RemoveFromRelationship(pivot);

			item.DI_IMOClass = "DEF";
			AssertEquals("DI_IMOClass", false, item.IsEmptyItem);
			item.DI_IMOClass = ZString.Empty;

			item.DI_OC_DGContact = Factory.New<OrgContact>().PK;
			AssertEquals("DI_OC_DGContact", false, item.IsEmptyItem);
			item.DI_OC_DGContact = ZGuid.Empty;

			item.DI_DGFlashPoint = 99.9;
			AssertEquals("DI_DGFlashPoint", false, item.IsEmptyItem);
			item.DI_DGFlashPoint = 0;

			item.DI_UnitOfVolume = "M3";
			AssertEquals("DI_UnitOfVolume", false, item.IsEmptyItem);
			item.DI_UnitOfVolume = ZString.Empty;

			item.DI_UnitOfWeight = "KG";
			AssertEquals("DI_UnitOfWeight", false, item.IsEmptyItem);
			item.DI_UnitOfWeight = ZString.Empty;

			item.DI_DGWeight = 500;
			AssertEquals("DI_DGWeight", false, item.IsEmptyItem);
			item.DI_DGWeight = 0;

			item.DI_DGVolume = 20;
			AssertEquals("DI_DGVolume", false, item.IsEmptyItem);
			item.DI_DGVolume = 0;

			AssertEquals("All empty", true, item.IsEmptyItem);
		}

		public void TestOverpack()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_HasOverpack = true;
			Assert("ID is read/write", !item.DI_OverpackID_ReadOnly);

			item.DI_OverpackID = "ABC";
			AssertEquals("ABC", item.DI_OverpackID);

			item.DI_HasOverpack = false;
			AssertEquals("ID should be blanked", "", item.DI_OverpackID);
			Assert("ID is read-only", item.DI_OverpackID_ReadOnly);
		}

		public void TestPSAGroup()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";

			var item = Factory.New<UNDGDataItem>();
			item.LinkDefault(substance);

			var reference1 = Factory.New<UNDGCountryReference>();
			reference1.DCR_HasFlashPointLower = false;
			reference1.DCR_HasFlashPointUpper = true;
			reference1.DCR_FlashPointUpperCentigrade = 13.5m;
			reference1.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference1.DCR_Type = "PSA";
			reference1.DCR_Code = "1S";
			substance.UNDGCountryReferences.Add(reference1);

			var reference2 = Factory.New<UNDGCountryReference>();
			reference2.DCR_HasFlashPointLower = true;
			reference2.DCR_HasFlashPointUpper = true;
			reference2.DCR_FlashPointLowerCentigrade = 13.5m;
			reference2.DCR_FlashPointUpperCentigrade = 100.1m;
			reference2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference2.DCR_Type = "PSA";
			reference2.DCR_Code = "2S";
			substance.UNDGCountryReferences.Add(reference2);

			var reference3 = Factory.New<UNDGCountryReference>();
			reference3.DCR_HasFlashPointLower = true;
			reference3.DCR_HasFlashPointUpper = true;
			reference3.DCR_FlashPointLowerCentigrade = 100.1m;
			reference3.DCR_FlashPointUpperCentigrade = 200.5m;
			reference3.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference3.DCR_Type = "PSA";
			reference3.DCR_Code = "3S";
			substance.UNDGCountryReferences.Add(reference3);

			var reference4 = Factory.New<UNDGCountryReference>();
			reference4.DCR_HasFlashPointLower = true;
			reference4.DCR_HasFlashPointUpper = false;
			reference4.DCR_FlashPointLowerCentigrade = 200.5m;
			reference4.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference4.DCR_Type = "PSA";
			reference4.DCR_Code = "4S";
			substance.UNDGCountryReferences.Add(reference4);

			var reference5 = Factory.New<UNDGCountryReference>();
			reference5.DCR_HasFlashPointLower = false;
			reference5.DCR_HasFlashPointUpper = false;
			reference5.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference5.DCR_Type = "PSA";
			reference5.DCR_Code = "5S";
			substance.UNDGCountryReferences.Add(reference5);

			item.DI_DGFlashPoint = 5;
			AssertEquals("1S", item.PSAGroup);

			item.DI_DGFlashPoint = 20;
			AssertEquals("2S", item.PSAGroup);

			item.DI_DGFlashPoint = 150;
			AssertEquals("3S", item.PSAGroup);

			item.DI_DGFlashPoint = 300;
			AssertEquals("4S", item.PSAGroup);
		}

		public void TestPSAGroup_NoFlashPointLower()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";

			var item = Factory.New<UNDGDataItem>();
			item.LinkDefault(substance);

			var reference1 = Factory.New<UNDGCountryReference>();
			reference1.DCR_HasFlashPointLower = false;
			reference1.DCR_HasFlashPointUpper = true;
			reference1.DCR_FlashPointUpperCentigrade = 13.5m;
			reference1.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference1.DCR_Type = "PSA";
			reference1.DCR_Code = "1S";
			substance.UNDGCountryReferences.Add(reference1);

			var reference2 = Factory.New<UNDGCountryReference>();
			reference2.DCR_HasFlashPointLower = false;
			reference2.DCR_HasFlashPointUpper = false;
			reference2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference2.DCR_Type = "PSA";
			reference2.DCR_Code = "2S";
			substance.UNDGCountryReferences.Add(reference2);

			item.DI_DGFlashPoint = 15;
			AssertEquals("2S", item.PSAGroup);
		}

		public void TestPSAGroup_NoFlashPointUpper()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";
			substance.DG_LQMaxAmt = 250;
			substance.DG_LQMaxAmtUQ = "G";

			var item = Factory.New<UNDGDataItem>();
			item.LinkDefault(substance);

			var reference1 = Factory.New<UNDGCountryReference>();
			reference1.DCR_HasFlashPointLower = true;
			reference1.DCR_HasFlashPointUpper = false;
			reference1.DCR_FlashPointLowerCentigrade = 13.5m;
			reference1.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference1.DCR_Type = "PSA";
			reference1.DCR_Code = "1S";
			substance.UNDGCountryReferences.Add(reference1);

			var reference2 = Factory.New<UNDGCountryReference>();
			reference2.DCR_HasFlashPointLower = false;
			reference2.DCR_HasFlashPointUpper = false;
			reference2.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			reference2.DCR_Type = "PSA";
			reference2.DCR_Code = "2S";
			substance.UNDGCountryReferences.Add(reference2);

			item.DI_DGFlashPoint = 5;
			AssertEquals("2S", item.PSAGroup);
		}

		public void TestIsCombustible_SetsFlashPoint()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_DGFlashPoint = 23m;
			item.DI_IsCombustible = false;

			AssertEquals(23m, item.DI_DGFlashPoint);

			item.DI_IsCombustible = true;
			AssertEquals("Should set flash point to previous cached value when is combustible is set to true", 23m, item.DI_DGFlashPoint);
		}

		public void TestDI_DGFlashPoint_ReadOnly()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "TTT";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var item = Factory.New<UNDGDataItem>();

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item.DI_IsCombustible = false;
				AssertEquals("should be readonly when DI_IsCombustible is false", true, item.DI_DGFlashPointInfo.ReadOnly);

				item.DI_IsCombustible = true;
				AssertEquals("should not be readonly when DI_IsCombustible is true", false, item.DI_DGFlashPointInfo.ReadOnly);
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("should not be readonly by default", false, item.DI_DGFlashPointInfo.ReadOnly);
			}
		}

		public void TestIsCombustibleSetToTrue_When_DgSubstanceWithNonEmptyFlashPointIsAttached()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_DGFlashPoint = 23m;
			item.DI_IsCombustible = false;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "ABC";
			subs.DG_FlashPoint = "29 cc";
			item.DI_DG = subs.PK;

			AssertEquals(true, item.DI_IsCombustible);
		}

		public void TestLinkedDGSubstanceInfoCollection_EmptySubs()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = ZGuid.Empty;
			AssertEquals(0, item.LinkedDGSubstanceInfoCollection.Count);
		}

		public void TestLinkedDGSubstanceInfoCollection_DefaultSubs()
		{
			var item = Factory.New<UNDGDataItem>();
			var subs = Factory.NewWithValidTestData<UNDGSubstance>();
			item.DI_DG = subs.PK;
			AssertEquals(1, item.LinkedDGSubstanceInfoCollection.Count);
		}

		public void TestLinkedDGSubstanceInfoCollection_AdditionalSubs()
		{
			var item = Factory.New<UNDGDataItem>();
			var subs1 = Factory.NewWithValidTestData<UNDGSubstance>();
			var subs2 = Factory.NewWithValidTestData<UNDGSubstance>();
			subs2.DG_UNNO = subs1.DG_UNNO;

			item.DI_DG = subs1.PK;
			AssertEquals(2, item.LinkedDGSubstanceInfoCollection.Count);

			item.DI_DG = ZGuid.Empty;
			AssertEquals(0, item.LinkedDGSubstanceInfoCollection.Count);
		}

		public void TestIsSavedByFactory()
		{
			UNDGDataItem item = Factory.New<UNDGDataItem>();
			item.IsAutoAddedItem = true;
			AssertEquals(false, item.IsSavedByFactory);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1234";
			item.DI_DG = subs.PK;
			item.LinkDefault(subs);
			AssertEquals(true, item.IsSavedByFactory);
		}

		public void TestIsParentValidToSave()
		{
			var item = Factory.New<UNDGDataItem>();
			item.DI_ParentID = ZGuid.Invalid;
			item.DI_ParentTableCode = "";

			AssertEquals(false, item.IsParentValidToSave);

			item.DI_ParentID = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_Class, "1.1A")).PK;
			AssertEquals(false, item.IsParentValidToSave);

			item.DI_ParentTableCode = "DG";
			AssertEquals(true, item.IsParentValidToSave);
		}

		public void TestDelete()
		{
			var item = Factory.New<UNDGDataItem>();
			item.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, Array.Empty<PropertyDescriptor>());
			var pivot = item.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_UNNO = "UN10";
			pivot.DP_Variant = "V";
			pivot.DP_Standard = "IMO";
			pivot.DP_IsDefault = true;
			Factory.Save();

			AssertEquals(false, pivot.IsDeleted);
			AssertEquals(1, item.UNDGSubstancePivotCollection.Count);

			item.Delete();

			AssertEquals(true, pivot.IsDeleted);
			AssertEquals(0, item.UNDGSubstancePivotCollection.Count);
		}

		public void TestDangerousGoodsLogs()
		{
			string allDGLogsMessage;
			var parentItemToStoreTheLogs = Factory.NewWithValidTestData<OrgSupplierPart>();
			Factory.Save();
			AssertNull("prerequisite - DangerousGoodsChanged log not created", parentItemToStoreTheLogs.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged));

			var undgDataItem1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem1.DI_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			undgDataItem1.DI_ParentID = parentItemToStoreTheLogs.PK;
			var subs1 = Factory.NewWithValidTestData<UNDGSubstance>();
			undgDataItem1.DI_DG = subs1.PK;
			Factory.Save();
			var log1 = parentItemToStoreTheLogs.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			AssertNotNull("DangerousGoodsChanged log created", log1);

			var undgDataItem2 = Factory.NewWithValidTestData<UNDGDataItem>();
			undgDataItem2.DI_ParentTableCode = OrgSupplierPartSchema.Constants.Prefix;
			undgDataItem2.DI_ParentID = parentItemToStoreTheLogs.PK;
			undgDataItem2.DI_DG = subs1.PK;
			Factory.Save();
			var log2 = parentItemToStoreTheLogs.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			allDGLogsMessage = GetDgLogEventsMessage(parentItemToStoreTheLogs.Logs);
			AssertNotNull("DangerousGoodsChanged log created", log2);
			AssertNotEquals($"Log {log1.GetHashCode()} is equal to Log {log2.GetHashCode()}: {ReferenceEquals(log1, log2)}. {allDGLogsMessage}", log1, log2);

			undgDataItem1.DI_DG = ZGuid.Empty;
			Factory.Save();
			var log3 = parentItemToStoreTheLogs.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			allDGLogsMessage = GetDgLogEventsMessage(parentItemToStoreTheLogs.Logs);
			AssertNotNull("DangerousGoodsChanged log created", log3);
			AssertNotEquals($"Log {log2.GetHashCode()} is equal to Log {log3.GetHashCode()}: {ReferenceEquals(log2, log3)}. {allDGLogsMessage}", log2, log3);

			undgDataItem1.Delete();
			Factory.Save();
			var log4 = parentItemToStoreTheLogs.Logs.MostRecentLogByEventTime(Events.DangerousGoodsChanged);
			allDGLogsMessage = GetDgLogEventsMessage(parentItemToStoreTheLogs.Logs);
			AssertNotNull("DangerousGoodsChanged log created", log4);
			AssertNotEquals($"Log {log3.GetHashCode()} is equal to Log {log4.GetHashCode()}: {ReferenceEquals(log3, log4)}. {allDGLogsMessage}", log3, log4);
		}

		public void TestUNDGDataItem_ShouldRefuseToSaveData_WithInvalidParentTableCode()
		{
			var item = Factory.NewWithValidTestData<UNDGDataItem>();

			var aValidParentTableCode = JobPackLinesSchema.Constants.Prefix;
			item.DI_ParentTableCode = aValidParentTableCode;
			AssertNoExceptionThrown("Valid data should be allowed to be saved.", () => Factory.Save());

			var anInvalidParentTableCode = UNDGDataItemSchema.Constants.Prefix;
			item.DI_ParentTableCode = anInvalidParentTableCode;
			AssertExceptionThrown<ZSaveException>("Invalid data should not be allowed to be saved.", () => Factory.Save());
		}

		public void TestDI_NECWeight_ReadOnly()
		{
			var subsClassOne = Factory.New<UNDGSubstance>();
			subsClassOne.DG_UNNO = "TTT";
			subsClassOne.DG_Variant = "a";
			subsClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsClassOne.DG_Class = "1.1a";
			var subsNotClassOne = Factory.New<UNDGSubstance>();
			subsNotClassOne.DG_UNNO = "TTT";
			subsNotClassOne.DG_Variant = "";
			subsNotClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsNotClassOne.DG_Class = "2.1";
			var classOneItem = Factory.New<UNDGDataItem>();
			classOneItem.DI_DG = subsClassOne.PK;
			var nonClassOneItem = Factory.New<UNDGDataItem>();
			nonClassOneItem.DI_DG = subsNotClassOne.PK;

			AssertEquals("should be readonly for non-class one DG", true, nonClassOneItem.DI_NECWeightInfo.ReadOnly);
			AssertEquals("should not be readonly for class one DG", false, classOneItem.DI_NECWeightInfo.ReadOnly);
		}

		public void TestDI_NECWeightUQ_ReadOnly()
		{
			var subsClassOne = Factory.New<UNDGSubstance>();
			subsClassOne.DG_UNNO = "TTT";
			subsClassOne.DG_Variant = "a";
			subsClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsClassOne.DG_Class = "1.1a";
			var subsNotClassOne = Factory.New<UNDGSubstance>();
			subsNotClassOne.DG_UNNO = "TTT";
			subsNotClassOne.DG_Variant = "";
			subsNotClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsNotClassOne.DG_Class = "2.1";
			var classOneItem = Factory.New<UNDGDataItem>();
			classOneItem.DI_DG = subsClassOne.PK;
			var nonClassOneItem = Factory.New<UNDGDataItem>();
			nonClassOneItem.DI_DG = subsNotClassOne.PK;

			AssertEquals("should be readonly for non-class one DG", true, nonClassOneItem.DI_NECWeightUQInfo.ReadOnly);
			AssertEquals("should not be readonly for class one DG", false, classOneItem.DI_NECWeightUQInfo.ReadOnly);
		}

		public void TestIsDgClassOneSubstance()
		{
			var subsClassOne = Factory.New<UNDGSubstance>();
			subsClassOne.DG_UNNO = "TTT";
			subsClassOne.DG_Variant = "a";
			subsClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsClassOne.DG_Class = "1.1a";
			var subsNotClassOne = Factory.New<UNDGSubstance>();
			subsNotClassOne.DG_UNNO = "TTT";
			subsNotClassOne.DG_Variant = "";
			subsNotClassOne.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subsNotClassOne.DG_Class = "2.1";
			var classOneItem = Factory.New<UNDGDataItem>();
			classOneItem.DI_DG = subsClassOne.PK;
			var nonClassOneItem = Factory.New<UNDGDataItem>();
			nonClassOneItem.DI_DG = subsNotClassOne.PK;

			AssertEquals("should be true for class one DG", false, nonClassOneItem.IsClassOneDgSubstance);
			AssertEquals("should be false for class one DG", true, classOneItem.IsClassOneDgSubstance);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			UNDGDataItem item = (UNDGDataItem)base.GetNewBusinessObjectForDeleteTest(factory);
			var subs = UNDGSubstanceLoader.LoadSubstances(factory, "1234").FirstOrDefault();
			if (subs == null)
			{
				subs = factory.New<UNDGSubstance>();
				subs.DG_Code = "1234";
			}
			item.LinkDefault(subs);
			return item;
		}

		static string GetDgLogEventsMessage(Logs logs)
		{
			var dgLogs = logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.DangerousGoodsChanged);
			var logListMessage = "DG Logs: ";
			foreach (var log in dgLogs)
			{
				logListMessage += $"{{LogHash: {log.GetHashCode()} EventTime: {log.SL_EventTime.SqlFormat}, Code: {log.SL_SE_NKEvent}}}, \r\n";
			}
			return logListMessage;
		}
	}
}
