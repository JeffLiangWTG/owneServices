using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UNDGDataObjectReader : DataObjectReader<UNDG, UNDGDataItem>
	{
		public UNDGDataObjectReader(UNDG dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<UNDGDataItem> undgDataItemBizObjProvider = null)
			: base(dataObject, logger, factory)
		{
			this.undgDataItemBizObjProvider = undgDataItemBizObjProvider;
		}

		public UNDGDataObjectReader(UNDG dataObject, IXmlImportLogger logger, BusinessObjectFactory factory, Func<UNDGDataItem> undgDataItemBizObjProvider = null)
			: base(dataObject, logger, new UniversalObjectFactory())
		{
			this.factoryOverride = factory;
			this.undgDataItemBizObjProvider = undgDataItemBizObjProvider;
		}

		readonly BusinessObjectFactory factoryOverride;
		readonly Func<UNDGDataItem> undgDataItemBizObjProvider;

		protected override UNDGDataItem GetExistingBusinessObject()
		{
			return undgDataItemBizObjProvider != null ? undgDataItemBizObjProvider() : null;
		}

		protected override UNDGDataItem GetNewBusinessObject()
		{
			return factoryOverride != null ? factoryOverride.New<UNDGDataItem>() : base.GetNewBusinessObject();
		}

		protected override void PopulateBusinessObject(UNDGDataItem undgBO)
		{
			using (undgBO.TempSetIsImportingData())
			{
				var undgRow = GetColumnIndexer(undgBO);
				if (dataObject.Contact != null)
				{
					var contact = new OrganizationContactDataObjectReader(dataObject.Contact, logger, factory).GetMatched();
					if (contact == null)
					{
						logger.Log(LogType.Warning, Res.GetString("6CAC234E-86D3-4BE9-B0D6-555598C0A66D", "There is no contact found in database with name '{0}' and phone '{1}' for dangerous goods substance code '{2}' (IMO Class = '{3}'). Make sure that name and phone is not empty. If not create contact first.", dataObject.Contact.FullName, dataObject.Contact.Phone, dataObject.UNDGCode, dataObject.IMOClass));
					}
					else
					{
						SetValue(undgRow, UNDGDataItemSchema.DI_OC_DGContact, contact.PK);
					}
				}

				if (dataObject.FlashPoint.HasValue)
				{
					ZDecimal flashPoint;
					if (ZDecimal.TryParse(dataObject.FlashPoint.Value, out flashPoint))
					{
						SetValue(undgRow, UNDGDataItemSchema.DI_DGFlashPoint, flashPoint);
					}
				}

				if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
				{
					SetValue(undgRow, UNDGDataItemSchema.DI_IsCombustible, dataObject.IsCombustible);
				}

				var undgCode = dataObject.UNDGCode.GetValueOrDefault();

				if (undgCode.IsEmpty)
				{
					var imoClass = dataObject.IMOClass.GetValueOrDefault();

					if (imoClass.IsEmpty)
					{
						logger.Log(LogType.Warning, Res.GetString("90df5dcb-d12c-4d8e-bd05-0a79e2b4427f", "No UNDG Code or IMO Class was provided."));
					}
					else
					{
						SetValue(undgRow, UNDGDataItemSchema.DI_IMOClass, dataObject.IMOClass);
					}
				}
				else
				{
					var substances = FindUNDGSubstance(undgCode);
					if (substances == null || substances.Length == 0)
					{
						logger.Log(LogType.Warning, Res.GetString("6CAC234E-86D3-4BE9-B0D6-555598C0A66C", "There is no substance with code '{0}' found. Please use standard dangerous goods substance code.", dataObject.UNDGCode));
					}
					else
					{
						SetValue(undgRow, UNDGDataItemSchema.DI_DG, substances[0].PK);

						if (!(undgRow is UNDGDataItem) && (StandardWithFallback.IsEmpty || StandardWithFallback == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO))
						{
							SetValue(undgRow, UNDGDataItemSchema.DI_DG_NKSubs, substances[0].DG_Code);
						}

						PopulateDefaultSubstancePivot(undgBO, substances[0]);
					}
				}

				var quantityClassification = QuantityClassificationHelper.GetQuantityClassification(dataObject.PackedInLimitedQuantity);

				SetValue(undgRow, UNDGDataItemSchema.DI_MPMarinePollutant, dataObject.MarinePollutant);
				SetValue(undgRow, UNDGDataItemSchema.DI_IsLimitedQuantity, dataObject.PackedInLimitedQuantity);
				SetValue(undgRow, UNDGDataItemSchema.DI_QuantityClassification, quantityClassification);
				SetValue(undgRow, UNDGDataItemSchema.DI_TechnicalName, dataObject.TechicalName);
				SetValue(undgRow, UNDGDataItemSchema.DI_DGVolume, dataObject.Volume);
				SetValue(undgRow, UNDGDataItemSchema.DI_DGWeight, dataObject.Weight);
				SetValue(undgRow, UNDGDataItemSchema.DI_PackageCount, dataObject.PackQty);
				SetValue(undgRow, UNDGDataItemSchema.DI_F3_NKPackType, dataObject.PackType);
				SetValue(undgRow, UNDGDataItemSchema.DI_PackingInstructionSection, dataObject.PackingInstructionSection);
				SetValue(undgRow, UNDGDataItemSchema.DI_OverpackID, dataObject.OverpackID);
				SetValue(undgRow, UNDGDataItemSchema.DI_HasOverpack, dataObject.HasOverpack);
				SetValue(undgRow, UNDGDataItemSchema.DI_HazardousWasteCode, dataObject.WasteCode);
				SetValue(undgRow, UNDGDataItemSchema.DI_SpecialPermitIssueDate, dataObject.SpecialPermitIssuedDate);
				SetValue(undgRow, UNDGDataItemSchema.DI_SpecialPermitNumber, dataObject.SpecialPermitNumber);
				SetValue(undgRow, UNDGDataItemSchema.DI_IsSalvagePackaging, dataObject.SalvagePackaging);
				SetValue(undgRow, UNDGDataItemSchema.DI_IsResidueLastContained, dataObject.ResidueLastContained);
				SetValue(undgRow, UNDGDataItemSchema.DI_RadionuclideElement, dataObject.RadionuclideElement);
				SetValue(undgRow, UNDGDataItemSchema.DI_RadionuclideElementSuffix, dataObject.RadionuclideElementSuffix);
				SetValue(undgRow, UNDGDataItemSchema.DI_RadioactiveMaximumActivity, dataObject.RadioactiveMaximumActivity);
				SetValue(undgRow, UNDGDataItemSchema.DI_RadioactiveMaximumActivityUnit, dataObject.RadioactiveMaximumActivityUnit);
				SetValue(undgRow, UNDGDataItemSchema.DI_RadioactiveLabelCategory, dataObject.RadioactiveLabelCategory);
				SetValue(undgRow, UNDGDataItemSchema.DI_RadioactiveTransportIndex, dataObject.RadioactiveTransportIndex);
				SetValue(undgRow, UNDGDataItemSchema.DI_MaterialFormDescription, dataObject.Description);
				SetValue(undgRow, UNDGDataItemSchema.DI_IsFissileExcepted, dataObject.FissileExcepted);
				SetValue(undgRow, UNDGDataItemSchema.DI_IsExclusiveUse, dataObject.ExclusiveUse);
				SetValue(undgRow, UNDGDataItemSchema.DI_IsHighwayRouteControlledQuantity, dataObject.HighwayRouteControlledQuantity);
				PopulateWeightAndVolumeUnits(undgRow);
			}
		}

		void PopulateWeightAndVolumeUnits(IColumnIndexer undgRow)
		{
			if (dataObject.WeightUQ?.Code.GetValueOrDefault() is ZString weightUQ
				&& !weightUQ.IsEmpty
				&& !Constants.Weight.ContainsCode(weightUQ.ToUpper()))
			{
				throw new DataObjectReadFailureException(Res.GetString("AE16943C-23D8-4083-A7BC-BA41B510C5FE", "UNDG Weight unit '{0}' is invalid. These are valid Weight unit types: {1}. Please correct this error.", dataObject.WeightUQ.Code.GetValueOrDefault(), string.Join(", ", Constants.Weight.Codes)));
			}
			else
			{
				SetValue(undgRow, UNDGDataItemSchema.DI_UnitOfWeight, dataObject.WeightUQ);
			}

			if (dataObject.VolumeUQ?.Code.GetValueOrDefault() is ZString volumeUQ
				&& !volumeUQ.IsEmpty
				&& !Constants.Volume.ContainsCode(volumeUQ.ToUpper()))
			{
				throw new DataObjectReadFailureException(Res.GetString("48AC948E-60A3-4C77-816C-9A54F0F9B0E7", "UNDG Volume unit '{0}' is invalid. These are valid Volume unit types: {1}. Please correct this error.", dataObject.VolumeUQ.Code.GetValueOrDefault(), string.Join(", ", Constants.Volume.Codes)));
			}
			else
			{
				SetValue(undgRow, UNDGDataItemSchema.DI_UnitOfVolume, dataObject.VolumeUQ);
			}
		}

		protected virtual ZString StandardWithFallback => dataObject.Standard.GetValueOrDefault();

		UNDGSubstance[] FindUNDGSubstance(ZString undgCode)
		{
			var unno = undgCode.SubstringSafe(0, 4);
			var variant = undgCode.SubstringSafe(4);

			return UNDGSubstanceLoader.LoadSubstances(factory.BOFactory, unno, variant, StandardWithFallback).ToArray();
		}

		void PopulateDefaultSubstancePivot(UNDGDataItem undgBO, UNDGSubstance substance)
		{
			var defaultPivot = undgBO.UNDGSubstancePivotCollection.FirstOrDefault(p => p.DP_IsDefault)
				?? undgBO.UNDGSubstancePivotCollection.AddNew();

			var defaultPivotRow = GetColumnIndexer(defaultPivot);
			SetValue(defaultPivotRow, UNDGSubstancePivotSchema.DP_IsDefault, true);
			SetValue(defaultPivotRow, UNDGSubstancePivotSchema.DP_UNNO, substance.DG_UNNO);
			SetValue(defaultPivotRow, UNDGSubstancePivotSchema.DP_Variant, substance.DG_Variant);

			SetValue(defaultPivotRow, UNDGSubstancePivotSchema.DP_Standard, StandardWithFallback.IsEmpty
				? new ZString(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO)
				: StandardWithFallback);
		}
	}
}
