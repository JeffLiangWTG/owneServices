using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	partial class DataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReader_CreatesPivot()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNotNull(Sub7);

			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "12.4",
				MarinePollutant = new UNDGMarinePollutant() { Code = "M", Description = "MARY" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME",
				Volume = 15m,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 20.5m,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Pounds, Description = "Pounds" },
				PackQty = 5,
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet, Description = "Pallet" },
				UNDGCode = "UN15b",
				Standard = "IMO"
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory);
			var data = reader.ReadIntoBusinessObject();

			AssertEquals("Default pivot has been created", 1, data.UNDGSubstancePivotCollection.Count);
			var pivot = data.UNDGSubstancePivotCollection.First();
			AssertEquals("Standard is set", "IMO", pivot.DP_Standard);
			AssertEquals("Pivot is default", true, pivot.DP_IsDefault);
			AssertEquals("UNNO is set", "UN15", pivot.DP_UNNO);
			AssertEquals("Variant is set", "b", pivot.DP_Variant);
		}

		public void TestReader_CreatesPivot_ADR()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var substance = Factory.New<UNDGSubstanceADR>();
			substance.ADR_UNNO = "7898";
			substance.ADR_Variant = "a";

			Factory.SaveForTesting();

			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "12.4",
				MarinePollutant = new UNDGMarinePollutant() { Code = "M", Description = "MARY" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME",
				Volume = 15m,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 20.5m,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Pounds, Description = "Pounds" },
				PackQty = 5,
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet, Description = "Pallet" },
				UNDGCode = "7898a",
				Standard = "ADR"
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory);
			var data = reader.ReadIntoBusinessObject();

			AssertEquals("Default pivot has been created", 1, data.UNDGSubstancePivotCollection.Count);

			var pivot = data.UNDGSubstancePivotCollection.First();
			AssertEquals("Mode is set", "ADR", pivot.DP_Standard);
		}

		public void TestReader_CreatesPivot_RID()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var substance = Factory.New<UNDGSubstanceRID>();
			substance.RID_UNNO = "7898";
			substance.RID_Variant = "a";

			Factory.SaveForTesting();

			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "12.4",
				MarinePollutant = new UNDGMarinePollutant() { Code = "M", Description = "MARY" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME",
				Volume = 15m,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 20.5m,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Pounds, Description = "Pounds" },
				PackQty = 5,
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet, Description = "Pallet" },
				UNDGCode = "7898a",
				Standard = "RID"
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory);
			var data = reader.ReadIntoBusinessObject();

			AssertEquals("Default pivot has been created", 1, data.UNDGSubstancePivotCollection.Count);

			var pivot = data.UNDGSubstancePivotCollection.First();
			AssertEquals("Standard is set", "RID", pivot.DP_Standard);
		}

		public void TestUNDGDataObjectReader()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNotNull(ContactBobPhonePizzaHut);
			AssertNotNull(ContactWendyPhonePizzaHut);
			AssertNotNull(ContactBobPhoneDominos);
			AssertNotNull(Sub1);
			AssertNotNull(Sub2);
			AssertNotNull(Sub3);
			AssertNotNull(Sub4);
			AssertNotNull(Sub5);
			AssertNotNull(Sub6);

			var today = ZDate.Today;
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				FlashPoint = "12.4",
				MarinePollutant = new UNDGMarinePollutant() { Code = "M", Description = "MARY" },
				PackedInLimitedQuantity = ZBool.True,
				TechicalName = "TECHNAME",
				Volume = 15m,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicFeet, Description = "Cubit Feet" },
				Weight = 20.5m,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Pounds, Description = "Pounds" },
				PackQty = 5,
				PackType = new PackageType() { Code = Core.Constants.PkgUnit.Pallet, Description = "Pallet" },
				PackingInstructionSection = "II",
				HasOverpack = true,
				OverpackID = "111",
				WasteCode = "222",
				SpecialPermitIssuedDate = today,
				SpecialPermitNumber = "333",
				SalvagePackaging = true,
				ResidueLastContained = true,
				RadionuclideElement = "U",
				RadionuclideElementSuffix = "230",
				RadioactiveMaximumActivity = 6.9m,
				RadioactiveMaximumActivityUnit = "TBQ",
				RadioactiveLabelCategory = "WH1",
				RadioactiveTransportIndex = 4.2m,
				Description = "New radioactive isotope, what do you think?",
				FissileExcepted = true,
				ExclusiveUse = true,
				HighwayRouteControlledQuantity = true
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory);
			var data = reader.ReadIntoBusinessObject();
			var dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, false, data.DI_IsCombustible);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 12.4m, data.DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_MPMarinePollutant, "M", dataRow.GetValue(UNDGDataItemSchema.DI_MPMarinePollutant));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsLimitedQuantity, ZBool.True, dataRow.GetValue(UNDGDataItemSchema.DI_IsLimitedQuantity));
			AssertEquals(UNDGDataItemSchema.Constants.DI_QuantityClassification, "LIM", dataRow.GetValue(UNDGDataItemSchema.DI_QuantityClassification));
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME", dataRow.GetValue(UNDGDataItemSchema.DI_TechnicalName));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 15m, dataRow.GetValue(UNDGDataItemSchema.DI_DGVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfVolume, Core.Constants.Volume.CubicFeet, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 20.5m, dataRow.GetValue(UNDGDataItemSchema.DI_DGWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfWeight, Core.Constants.Weight.Pounds, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_OC_DGContact, ZGuid.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_OC_DGContact));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DG, ZGuid.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_DG));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 5, dataRow.GetValue(UNDGDataItemSchema.DI_PackageCount));
			AssertEquals(UNDGDataItemSchema.Constants.DI_F3_NKPackType, "PLT", dataRow.GetValue(UNDGDataItemSchema.DI_F3_NKPackType));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackingInstructionSection, "II", dataRow.GetValue(UNDGDataItemSchema.DI_PackingInstructionSection));
			AssertEquals(UNDGDataItemSchema.Constants.DI_HasOverpack, true, dataRow.GetValue(UNDGDataItemSchema.DI_HasOverpack));
			AssertEquals(UNDGDataItemSchema.Constants.DI_OverpackID, "111", dataRow.GetValue(UNDGDataItemSchema.DI_OverpackID));
			AssertEquals(UNDGDataItemSchema.Constants.DI_HazardousWasteCode, "222", dataRow.GetValue(UNDGDataItemSchema.DI_HazardousWasteCode));
			AssertEquals(UNDGDataItemSchema.Constants.DI_SpecialPermitIssueDate, today, dataRow.GetValue(UNDGDataItemSchema.DI_SpecialPermitIssueDate));
			AssertEquals(UNDGDataItemSchema.Constants.DI_SpecialPermitNumber, "333", dataRow.GetValue(UNDGDataItemSchema.DI_SpecialPermitNumber));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsSalvagePackaging, true, dataRow.GetValue(UNDGDataItemSchema.DI_IsSalvagePackaging));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsResidueLastContained, true, dataRow.GetValue(UNDGDataItemSchema.DI_IsResidueLastContained));
			AssertEquals(UNDGDataItemSchema.Constants.DI_RadionuclideElement, "U", dataRow.GetValue(UNDGDataItemSchema.DI_RadionuclideElement));
			AssertEquals(UNDGDataItemSchema.Constants.DI_RadionuclideElementSuffix, "230", dataRow.GetValue(UNDGDataItemSchema.DI_RadionuclideElementSuffix));
			AssertEquals(UNDGDataItemSchema.Constants.DI_RadioactiveMaximumActivity, 6.9m, dataRow.GetValue(UNDGDataItemSchema.DI_RadioactiveMaximumActivity));
			AssertEquals(UNDGDataItemSchema.Constants.DI_RadioactiveMaximumActivityUnit, "TBQ", dataRow.GetValue(UNDGDataItemSchema.DI_RadioactiveMaximumActivityUnit));
			AssertEquals(UNDGDataItemSchema.Constants.DI_RadioactiveLabelCategory, "WH1", dataRow.GetValue(UNDGDataItemSchema.DI_RadioactiveLabelCategory));
			AssertEquals(UNDGDataItemSchema.Constants.DI_RadioactiveTransportIndex, 4.2m, dataRow.GetValue(UNDGDataItemSchema.DI_RadioactiveTransportIndex));
			AssertEquals(UNDGDataItemSchema.Constants.DI_MaterialFormDescription, "New radioactive isotope, what do you think?", dataRow.GetValue(UNDGDataItemSchema.DI_MaterialFormDescription));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsFissileExcepted, true, dataRow.GetValue(UNDGDataItemSchema.DI_IsFissileExcepted));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsExclusiveUse, true, dataRow.GetValue(UNDGDataItemSchema.DI_IsExclusiveUse));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsHighwayRouteControlledQuantity, true, dataRow.GetValue(UNDGDataItemSchema.DI_IsHighwayRouteControlledQuantity));

			undg.FlashPoint = null;
			undg.PackQty = null;
			undg.PackType = new PackageType() { Code = Core.Constants.PkgUnit.Bag, Description = "BAG" };
			data = reader.ReadIntoBusinessObject();
			dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, false, data.DI_IsCombustible);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 0m, data.DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_MPMarinePollutant, "M", dataRow.GetValue(UNDGDataItemSchema.DI_MPMarinePollutant));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsLimitedQuantity, ZBool.True, dataRow.GetValue(UNDGDataItemSchema.DI_IsLimitedQuantity));
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME", dataRow.GetValue(UNDGDataItemSchema.DI_TechnicalName));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 15m, dataRow.GetValue(UNDGDataItemSchema.DI_DGVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfVolume, Core.Constants.Volume.CubicFeet, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 20.5m, dataRow.GetValue(UNDGDataItemSchema.DI_DGWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfWeight, Core.Constants.Weight.Pounds, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_OC_DGContact, ZGuid.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_OC_DGContact));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DG, ZGuid.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_DG));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 0, dataRow.GetValue(UNDGDataItemSchema.DI_PackageCount));
			AssertEquals(UNDGDataItemSchema.Constants.DI_F3_NKPackType, "BAG", dataRow.GetValue(UNDGDataItemSchema.DI_F3_NKPackType));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackingInstructionSection, "II", dataRow.GetValue(UNDGDataItemSchema.DI_PackingInstructionSection));

			undg.Contact = new OrganizationContact() { FullName = BobName, Phone = DominosNumber };
			data = reader.ReadIntoBusinessObject();
			dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, false, data.DI_IsCombustible);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 0m, data.DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_MPMarinePollutant, "M", dataRow.GetValue(UNDGDataItemSchema.DI_MPMarinePollutant));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsLimitedQuantity, ZBool.True, dataRow.GetValue(UNDGDataItemSchema.DI_IsLimitedQuantity));
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME", dataRow.GetValue(UNDGDataItemSchema.DI_TechnicalName));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 15m, dataRow.GetValue(UNDGDataItemSchema.DI_DGVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfVolume, Core.Constants.Volume.CubicFeet, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 20.5m, dataRow.GetValue(UNDGDataItemSchema.DI_DGWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfWeight, Core.Constants.Weight.Pounds, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_OC_DGContact, ContactBobPhoneDominos.PK, dataRow.GetValue(UNDGDataItemSchema.DI_OC_DGContact));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DG, ZGuid.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_DG));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 0, dataRow.GetValue(UNDGDataItemSchema.DI_PackageCount));
			AssertEquals(UNDGDataItemSchema.Constants.DI_F3_NKPackType, "BAG", dataRow.GetValue(UNDGDataItemSchema.DI_F3_NKPackType));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackingInstructionSection, "II", dataRow.GetValue(UNDGDataItemSchema.DI_PackingInstructionSection));

			undg.UNDGCode = "UN121";
			undg.PackingGroup = "PG";
			undg.ProperShippingName = "PSN";
			undg.IMOClass = "C";
			data = reader.ReadIntoBusinessObject();
			dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, false, data.DI_IsCombustible);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 0m, data.DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_MPMarinePollutant, "M", dataRow.GetValue(UNDGDataItemSchema.DI_MPMarinePollutant));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsLimitedQuantity, ZBool.True, dataRow.GetValue(UNDGDataItemSchema.DI_IsLimitedQuantity));
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME", dataRow.GetValue(UNDGDataItemSchema.DI_TechnicalName));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 15m, dataRow.GetValue(UNDGDataItemSchema.DI_DGVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfVolume, Core.Constants.Volume.CubicFeet, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 20.5m, dataRow.GetValue(UNDGDataItemSchema.DI_DGWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfWeight, Core.Constants.Weight.Pounds, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_OC_DGContact, ContactBobPhoneDominos.PK, dataRow.GetValue(UNDGDataItemSchema.DI_OC_DGContact));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DG, Sub1.PK, dataRow.GetValue(UNDGDataItemSchema.DI_DG));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 0, dataRow.GetValue(UNDGDataItemSchema.DI_PackageCount));
			AssertEquals(UNDGDataItemSchema.Constants.DI_F3_NKPackType, "BAG", dataRow.GetValue(UNDGDataItemSchema.DI_F3_NKPackType));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackingInstructionSection, "II", dataRow.GetValue(UNDGDataItemSchema.DI_PackingInstructionSection));

			undg.UNDGCode = null;
			undg.FlashPoint = "0.0";
			data = reader.ReadIntoBusinessObject();
			dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, false, data.DI_IsCombustible);
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 0m, data.DI_DGFlashPoint);
			AssertEquals(UNDGDataItemSchema.Constants.DI_MPMarinePollutant, "M", dataRow.GetValue(UNDGDataItemSchema.DI_MPMarinePollutant));
			AssertEquals(UNDGDataItemSchema.Constants.DI_IsLimitedQuantity, ZBool.True, dataRow.GetValue(UNDGDataItemSchema.DI_IsLimitedQuantity));
			AssertEquals(UNDGDataItemSchema.Constants.DI_TechnicalName, "TECHNAME", dataRow.GetValue(UNDGDataItemSchema.DI_TechnicalName));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGVolume, 15m, dataRow.GetValue(UNDGDataItemSchema.DI_DGVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfVolume, Core.Constants.Volume.CubicFeet, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfVolume));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DGWeight, 20.5m, dataRow.GetValue(UNDGDataItemSchema.DI_DGWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfWeight, Core.Constants.Weight.Pounds, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_OC_DGContact, ContactBobPhoneDominos.PK, dataRow.GetValue(UNDGDataItemSchema.DI_OC_DGContact));
			AssertEquals(UNDGDataItemSchema.Constants.DI_DG, ZGuid.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_DG));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackageCount, 0, dataRow.GetValue(UNDGDataItemSchema.DI_PackageCount));
			AssertEquals(UNDGDataItemSchema.Constants.DI_F3_NKPackType, "BAG", dataRow.GetValue(UNDGDataItemSchema.DI_F3_NKPackType));
			AssertEquals(UNDGDataItemSchema.Constants.DI_PackingInstructionSection, "II", dataRow.GetValue(UNDGDataItemSchema.DI_PackingInstructionSection));

			undg.UNDGCode = "UN166";
			undg.Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			data = reader.ReadIntoBusinessObject();
			dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_DG, Sub6.PK, dataRow.GetValue(UNDGDataItemSchema.DI_DG));

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				undg.FlashPoint = "0.0";
				undg.IsCombustible = true;
				data = reader.ReadIntoBusinessObject();
				AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, true, data.DI_IsCombustible);
				AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 0m, data.DI_DGFlashPoint);

				undg.IsCombustible = false;
				data = reader.ReadIntoBusinessObject();
				AssertEquals(UNDGDataItemSchema.Constants.DI_IsCombustible, false, data.DI_IsCombustible);
				AssertEquals(UNDGDataItemSchema.Constants.DI_DGFlashPoint, 0m, data.DI_DGFlashPoint);
			}

			undg.WeightUQ.Code = ZString.Empty;
			undg.VolumeUQ.Code = ZString.Empty;
			reader.ReadIntoBusinessObject();
			data = reader.ReadIntoBusinessObject();
			dataRow = (IColumnIndexer)((IBusinessObjectInternals)data).Row;
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfWeight, ZString.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfWeight));
			AssertEquals(UNDGDataItemSchema.Constants.DI_UnitOfVolume, ZString.Empty, dataRow.GetValue(UNDGDataItemSchema.DI_UnitOfVolume));

			undg.WeightUQ.Code = "0";
			AssertExceptionThrown<DataObjectReadFailureException>(
				@$"
Error - UNDG Weight unit '0' is invalid. These are valid Weight unit types: {string.Join(", ", Constants.Weight.Codes)}. Please correct this error.
The whole process will be terminated".Trim(),
				() => reader.ReadIntoBusinessObject());

			undg.WeightUQ.Code = Core.Constants.Weight.Pounds;
			undg.VolumeUQ.Code = "0";
			AssertExceptionThrown<DataObjectReadFailureException>(
				@$"
Error - UNDG Volume unit '0' is invalid. These are valid Volume unit types: {string.Join(", ", Constants.Volume.Codes)}. Please correct this error.
The whole process will be terminated".Trim(),
				() => reader.ReadIntoBusinessObject());
		}

		public void TestReadIntoExistingUNDG()
		{
			var undgBO = Factory.New<UNDGDataItem>();
			undgBO.DI_DGWeight = 30m;
			undgBO.DI_DGVolume = 40.5m;

			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TechicalName = "TECHNAME",
				Volume = 15m,
				Weight = 20.5m,
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory, () => undgBO);
			var data = reader.ReadIntoBusinessObject();

			AssertEquals(undgBO.PK, data.PK);
			AssertEquals("TECHNAME", data.DI_TechnicalName);
			AssertEquals(15m, data.DI_DGVolume);
			AssertEquals(20.5m, data.DI_DGWeight);
		}

		public void TestPopulate_DI_DG()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertPopulate_DI_DG();
			}

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertPopulate_DI_DG();
			}

			void AssertPopulate_DI_DG()
			{
				AssertNotNull(Sub6);
				AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, Sub6.DG_Standard);

				var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
				{
					UNDGCode = Sub6.DG_Code,
					Standard = Sub6.DG_Standard
				};

				var reader = new UNDGDataObjectReader(undg, logger, Factory);
				var data = reader.ReadIntoBusinessObject();
				AssertEquals(Sub6.PK, data.DI_DG);

				AssertNotNull(Sub5);
				AssertNotEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA, Sub5.DG_Standard);

				undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
				{
					UNDGCode = Sub5.DG_Code,
					Standard = Sub5.DG_Standard
				};

				reader = new UNDGDataObjectReader(undg, logger, Factory);
				data = reader.ReadIntoBusinessObject();
				AssertEquals(Sub5.PK, data.DI_DG);

				undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
				{
					UNDGCode = Sub5.DG_Code
				};

				reader = new UNDGDataObjectReader(undg, logger, Factory);
				data = reader.ReadIntoBusinessObject();
				AssertEquals(Sub5.PK, data.DI_DG);
			}
		}

		UNDGSubstance Sub1
		{
			get
			{
				if (sub1 == null)
				{
					sub1 = Factory.New<UNDGSubstance>();
					sub1.DG_Code = "CD1";
					sub1.DG_UNNO = "UN12";
					sub1.DG_Variant = "1";
					sub1.DG_PG = "PG";
					sub1.DG_PSN = "PSN";
					sub1.DG_Class = "C";
				}
				return sub1;
			}
		}
		UNDGSubstance sub1;

		UNDGSubstance Sub2
		{
			get
			{
				if (sub2 == null)
				{
					sub2 = Factory.New<UNDGSubstance>();
					sub2.DG_Code = "CD2";
					sub2.DG_UNNO = "UN12";
					sub2.DG_Variant = "2";
					sub2.DG_PG = "PG2";
					sub2.DG_PSN = "PSN";
					sub2.DG_Class = "C";
				}
				return sub2;
			}
		}
		UNDGSubstance sub2;

		UNDGSubstance Sub3
		{
			get
			{
				if (sub3 == null)
				{
					sub3 = Factory.New<UNDGSubstance>();
					sub3.DG_Code = "CD3";
					sub3.DG_UNNO = "UN12";
					sub3.DG_Variant = "3";
					sub3.DG_PG = "PG";
					sub3.DG_PSN = "PSN3";
					sub3.DG_Class = "C";
				}
				return sub3;
			}
		}
		UNDGSubstance sub3;

		UNDGSubstance Sub4
		{
			get
			{
				if (sub4 == null)
				{
					sub4 = Factory.New<UNDGSubstance>();
					sub4.DG_Code = "CD4";
					sub4.DG_UNNO = "UN12";
					sub4.DG_Variant = "4";
					sub4.DG_PG = "PG";
					sub4.DG_PSN = "PSN";
					sub4.DG_Class = "C4";
				}
				return sub4;
			}
		}
		UNDGSubstance sub4;

		UNDGSubstance Sub5
		{
			get
			{
				if (sub5 == null)
				{
					sub5 = Factory.New<UNDGSubstance>();
					sub5.DG_Code = "CD5";
					sub5.DG_UNNO = "UN15";
					sub5.DG_Variant = "5";
					sub5.DG_PG = "PG";
					sub5.DG_PSN = "PSN";
					sub5.DG_Class = "C";
				}
				return sub5;
			}
		}
		UNDGSubstance sub5;

		UNDGSubstance Sub6
		{
			get
			{
				if (sub6 == null)
				{
					sub6 = Factory.New<UNDGSubstance>();
					sub6.DG_Code = "CD6";
					sub6.DG_UNNO = "UN16";
					sub6.DG_Variant = "6";
					sub6.DG_PG = "PG";
					sub6.DG_PSN = "PSN";
					sub6.DG_Class = "C";
					sub6.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
					sub6.DG_UniqueRecordId = "1010";
				}
				return sub6;
			}
		}
		UNDGSubstance sub6;

		UNDGSubstance Sub7
		{
			get
			{
				if (sub7 == null)
				{
					sub7 = Factory.New<UNDGSubstance>();
					sub7.DG_Code = "UN15b";
					sub7.DG_UNNO = "UN15";
					sub7.DG_Variant = "b";
					sub7.DG_PG = "PG";
					sub7.DG_PSN = "PSN";
					sub7.DG_Class = "C";
					sub7.DG_SubLabel1 = "ZOMBIE VIRUS";
					sub7.DG_SubLabel2 = "HMZ-2026";
				}
				return sub7;
			}
		}
		UNDGSubstance sub7;

		public void TestReadClassOnlyUNDG()
		{
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			undg.IMOClass = "1.1D";

			var bizO = new UNDGDataObjectReader(undg, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("1.1D", bizO.DI_IMOClass);
		}

		public void TestUNDGCodeAndIMOClassMissing()
		{
			new UNDGDataObjectReader(new UNDG(DefaultDataObjectWriterStrategy.TestInstance), logger, Factory).ReadIntoBusinessObject();
			AssertMultilineASCIIEquals("Should log warning", @"Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Warning - No UNDG Code or IMO Class was provided.", logger.Logs);
		}

		public void TestDI_IsLimitedQuantityIsImported_OnExistingUDNGDataItem()
		{
			var undgDataItemBO = Factory.New<UNDGDataItem>();
			undgDataItemBO.DI_IsLimitedQuantity = false;
			undgDataItemBO.DI_DGWeight = 30m;
			undgDataItemBO.DI_DGVolume = 40.5m;

			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackedInLimitedQuantity = ZBool.True,
				Volume = 15,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres, Description = "Cubic Metres" },
				Weight = 20.5,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms, Description = "Kilograms" },
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory, () => undgDataItemBO);
			var data = reader.ReadIntoBusinessObject();

			AssertEquals(undgDataItemBO.PK, data.PK);
			AssertEquals(ZBool.True, data.DI_IsLimitedQuantity);
			AssertEquals(new ZDecimal(15), data.DI_DGVolume);
			AssertEquals(new ZDecimal(20.5), data.DI_DGWeight);
		}

		public void TestDI_IsLimitedQuantityIsImported_OnNewUDNGDataItem()
		{
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackedInLimitedQuantity = ZBool.True,
				Volume = 15,
				VolumeUQ = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres, Description = "Cubic Metres" },
				Weight = 20.5,
				WeightUQ = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms, Description = "Kilograms" },
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory);
			var undgDataItemBO = reader.ReadIntoBusinessObject();

			AssertEquals(ZBool.True, undgDataItemBO.DI_IsLimitedQuantity);
			AssertEquals(new ZDecimal(15), undgDataItemBO.DI_DGVolume);
			AssertEquals(new ZDecimal(20.5), undgDataItemBO.DI_DGWeight);
		}

		public void TestDI_DGFlashPointIsImported_OnExistingUDNGDataItem()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_FlashPoint = "-28.0";

			var undgDataItemBO = Factory.New<UNDGDataItem>();

			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = "123a",
				FlashPoint = "-2.0"
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory, () => undgDataItemBO);
			var data = reader.ReadIntoBusinessObject();

			AssertEquals(undgDataItemBO.PK, data.PK);
			AssertEquals(new ZDecimal(-2.0), undgDataItemBO.DI_DGFlashPoint);
		}

		public void TestDI_DGFlashPointIsImported_OnNewUDNGDataItem()
		{
			var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = "123a",
				FlashPoint = "-2.0"
			};

			var reader = new UNDGDataObjectReader(undg, logger, Factory);
			var undgDataItemBO = reader.ReadIntoBusinessObject();

			AssertEquals(new ZDecimal(-2.0), undgDataItemBO.DI_DGFlashPoint);
		}
	}
}
