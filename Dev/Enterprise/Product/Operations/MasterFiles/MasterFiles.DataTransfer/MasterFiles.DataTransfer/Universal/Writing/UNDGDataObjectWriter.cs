using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class UNDGDataObjectWriter : DataObjectWriter<UNDGDataItem, UNDG>
	{
		public UNDGDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override UNDG PopulateDataObject(UNDGDataItem undgBO)
		{
			var undgData = new UNDG();

			if (undgBO.DGContact != null)
			{
				undgData.Contact = new OrganizationContactDataObjectWriter(writeManager).GetDataObject(undgBO.DGContact);
			}

			undgData.FlashPoint = undgBO.DI_DGFlashPoint.ToString();

			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				undgData.IsCombustible = undgBO.DI_IsCombustible;
			}

			var substance = undgBO.Substance;
			if (substance != null)
			{
				undgData.EmergencyScheduleFire = GetEmergencyScheduleFire(substance.DG_EMS);
				undgData.EmergencyScheduleSpillage = GetEmergencyScheduleSpillage(substance.DG_EMS);
				undgData.IMOClass = substance.DG_Class;
				undgData.PackingGroup = substance.DG_PG;
				undgData.ProperShippingName = substance.DG_PSN;
				undgData.UNDGCode = substance.DG_Code;
				undgData.SubLabel1 = substance.DG_SubLabel1;
				undgData.SubLabel2 = substance.DG_SubLabel2;
				undgData.Standard = substance.DG_Standard;
				undgData.State = new UNDGStateConverter().ToEnumValue(substance.DG_State);
			}
			else
			{
				undgData.IMOClass = undgBO.DI_IMOClass;
			}

			undgData.MarinePollutant = ListHelper.GetWithDescription<UNDGMarinePollutant>(undgBO.DI_MPMarinePollutant, undgBO.Lookups.MarinePollutantList);

			undgData.PackedInLimitedQuantity = undgBO.DI_IsLimitedQuantity;
			undgData.TechicalName = undgBO.DI_TechnicalName;
			undgData.Volume = undgBO.DI_DGVolume;
			undgData.VolumeUQ = ListHelper.GetWithDescription<UnitOfVolume>(undgBO.DI_UnitOfVolume, undgBO.Lookups.VolumeUnits);
			undgData.Weight = undgBO.DI_DGWeight;
			undgData.WeightUQ = ListHelper.GetWithDescription<UnitOfWeight>(undgBO.DI_UnitOfWeight, undgBO.Lookups.WeightUnits);
			undgData.PackQty = undgBO.DI_PackageCount;
			undgData.PackType = ListHelper.GetWithDescription<PackageType>(undgBO.DI_F3_NKPackType, undgBO.Lookups.PackTypes);
			undgData.PackingInstructionSection = undgBO.DI_PackingInstructionSection;
			undgData.OverpackID = undgBO.DI_OverpackID;
			undgData.HasOverpack = undgBO.DI_HasOverpack;
			undgData.WasteCode = undgBO.DI_HazardousWasteCode;
			undgData.SpecialPermitIssuedDate = undgBO.DI_SpecialPermitIssueDate;
			undgData.SpecialPermitNumber = undgBO.DI_SpecialPermitNumber;
			undgData.SalvagePackaging = undgBO.DI_IsSalvagePackaging;
			undgData.ResidueLastContained = undgBO.DI_IsResidueLastContained;
			undgData.RadionuclideElement = undgBO.DI_RadionuclideElement;
			undgData.RadionuclideElementSuffix = undgBO.DI_RadionuclideElementSuffix;
			undgData.RadioactiveMaximumActivity = undgBO.DI_RadioactiveMaximumActivity;
			undgData.RadioactiveMaximumActivityUnit = undgBO.DI_RadioactiveMaximumActivityUnit;
			undgData.RadioactiveLabelCategory = undgBO.DI_RadioactiveLabelCategory;
			undgData.RadioactiveTransportIndex = undgBO.DI_RadioactiveTransportIndex;
			undgData.Description = undgBO.DI_MaterialFormDescription;
			undgData.FissileExcepted = undgBO.DI_IsFissileExcepted;
			undgData.ExclusiveUse = undgBO.DI_IsExclusiveUse;
			undgData.HighwayRouteControlledQuantity = undgBO.DI_IsHighwayRouteControlledQuantity;
			return undgData;
		}

		CodeDescriptionPair GetEmergencyScheduleFire(ZString emsCodes)
		{
			var emsCode = emsCodes.Split(',').FirstOrDefault(x => x.StartsWith("F-") || x.StartsWith("f-") || x.Equals("*"));

			return string.IsNullOrEmpty(emsCode) ? null : ListHelper.GetWithDescription<CodeDescriptionPair>(emsCode, new EmergencyScheduleFireCodes());
		}

		CodeDescriptionPair GetEmergencyScheduleSpillage(ZString emsCodes)
		{
			var emsCode = emsCodes.Split(',').FirstOrDefault(x => x.StartsWith("S-") || x.StartsWith("s-") || x.Equals("*"));

			return string.IsNullOrEmpty(emsCode) ? null : ListHelper.GetWithDescription<CodeDescriptionPair>(emsCode, new EmergencyScheduleSpillageCodes());
		}
	}
}
