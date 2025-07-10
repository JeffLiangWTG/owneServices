using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingUNDGDataItemIMOValidationTest : TestCaseWithFactory
	{
		public void TestDGTechnicalNameValidationIfSP274OrSP318()
		{
			const string errorMessage =
				"Technical Name is required for the Ocean Booking for substances of the IMO Standard with Special Provision 274 and/or 318, and substances of the CFR Standard with Special Provision 441.";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgDataItem.LinkDefault(subs);
			undgDataItem.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertNoMessageError(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			var provision = Factory.New<UNDGCommonData>();
			provision.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			provision.DC_Index = "274";
			undgDataItem.Substance.SpecialProvisions.Add(provision);
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertHasMessageError(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			provision.DC_Index = "318";
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertHasMessageError(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.DI_TechnicalName = "Killer Vanila";
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertNoMessageError(undgDataItem.DI_TechnicalNameInfo, errorMessage);
		}

		public void TestDGTechnicalNameValidationIfMarinePollutant()
		{
			const string errorMessage =
				"Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant.";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undgDataItem.LinkDefault(subs);
			undgDataItem.Substance.DG_TechName = "";
			undgDataItem.Substance.DG_MP = "Y";
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertNoWarning(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.Substance.DG_TechName = "*";
			undgDataItem.Substance.DG_MP = "N";
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertNoWarning(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.Substance.DG_MP = "Y";
			undgDataItem.Validation.ValidateDI_TechnicalName();

			AssertHasWarning(undgDataItem.DI_TechnicalNameInfo, errorMessage);

			undgDataItem.DI_TechnicalName = "Killer Vanila";

			AssertNoWarning(undgDataItem.DI_TechnicalNameInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_PackageCount()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_PackageCount = 0;
			undgDataItem.Validation.ValidateDI_PackageCount();
			AssertHasMessageError(undgDataItem.DI_PackageCountInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_PackType()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_F3_NKPackType = ZString.Empty;
			undgDataItem.Validation.ValidateDI_F3_NKPackType();
			AssertHasMessageError(undgDataItem.DI_F3_NKPackTypeInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_Weight()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_DGVolume = 0;
			undgDataItem.DI_UnitOfVolume = ZString.Empty;
			undgDataItem.Validation.ValidateDI_DGWeight();
			AssertNoMessageError(undgDataItem.DI_DGWeightInfo, errorMessage);

			undgDataItem.DI_DGWeight = 0;
			undgDataItem.Validation.ValidateDI_DGWeight();
			AssertHasMessageError(undgDataItem.DI_DGWeightInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_UnitOfWeight()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_DGVolume = 0;
			undgDataItem.DI_UnitOfVolume = ZString.Empty;
			undgDataItem.Validation.ValidateDI_UnitOfWeight();
			AssertNoMessageError(undgDataItem.DI_UnitOfWeightInfo, errorMessage);

			undgDataItem.DI_UnitOfWeight = ZString.Empty;
			undgDataItem.Validation.ValidateDI_UnitOfWeight();
			AssertHasMessageError(undgDataItem.DI_UnitOfWeightInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_Volume()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_DGWeight = 0;
			undgDataItem.DI_UnitOfWeight = ZString.Empty;
			undgDataItem.Validation.ValidateDI_DGVolume();
			AssertNoMessageError(undgDataItem.DI_DGVolumeInfo, errorMessage);

			undgDataItem.DI_DGVolume = 0;
			undgDataItem.Validation.ValidateDI_DGVolume();
			AssertHasMessageError(undgDataItem.DI_DGVolumeInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_UnitOfVolume()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_DGWeight = 0;
			undgDataItem.DI_UnitOfWeight = ZString.Empty;
			undgDataItem.Validation.ValidateDI_UnitOfVolume();
			AssertNoMessageError(undgDataItem.DI_UnitOfVolumeInfo, errorMessage);

			undgDataItem.DI_UnitOfVolume = ZString.Empty;
			undgDataItem.Validation.ValidateDI_UnitOfVolume();
			AssertHasMessageError(undgDataItem.DI_UnitOfVolumeInfo, errorMessage);
		}

		public void TestCheckNetExplosiveContent_UNDGSubstance()
		{
			var (undgDataItem, errorMessage) = SetUpValidNetExplosiveContent();
			undgDataItem.DI_PackageCount = 0;
			undgDataItem.DI_F3_NKPackType = ZString.Empty;
			undgDataItem.DI_DGWeight = 0;
			undgDataItem.DI_UnitOfWeight = ZString.Empty;
			undgDataItem.DI_DGVolume = 0;
			undgDataItem.DI_UnitOfVolume = ZString.Empty;
			undgDataItem.Validation.ValidateDI_PackageCount();
			undgDataItem.Validation.ValidateDI_F3_NKPackType();
			undgDataItem.Validation.ValidateDI_DGWeight();
			undgDataItem.Validation.ValidateDI_UnitOfWeight();
			undgDataItem.Validation.ValidateDI_DGVolume();
			undgDataItem.Validation.ValidateDI_UnitOfVolume();
			AssertHasMessageError(undgDataItem.DI_PackageCountInfo, errorMessage);
			AssertHasMessageError(undgDataItem.DI_F3_NKPackTypeInfo, errorMessage);
			AssertHasMessageError(undgDataItem.DI_DGWeightInfo, errorMessage);
			AssertHasMessageError(undgDataItem.DI_UnitOfWeightInfo, errorMessage);
			AssertHasMessageError(undgDataItem.DI_DGVolumeInfo, errorMessage);
			AssertHasMessageError(undgDataItem.DI_UnitOfVolumeInfo, errorMessage);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1002";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_State = UNDGSubstanceLookups.StateTypes.Code.Liquid;
			undgDataItem.LinkDefault(subs);
			undgDataItem.Validation.ValidateDI_PackageCount();
			undgDataItem.Validation.ValidateDI_F3_NKPackType();
			undgDataItem.Validation.ValidateDI_DGWeight();
			undgDataItem.Validation.ValidateDI_UnitOfWeight();
			undgDataItem.Validation.ValidateDI_DGVolume();
			undgDataItem.Validation.ValidateDI_UnitOfVolume();
			AssertNoMessageError(undgDataItem.DI_PackageCountInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_F3_NKPackTypeInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_DGWeightInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_UnitOfWeightInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_DGVolumeInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_UnitOfVolumeInfo, errorMessage);
		}

		(UNDGDataItem, string) SetUpValidNetExplosiveContent()
		{
			const string errorMessage = "For explosives, the packs, pack type, net quantity (weight or volume) and unit of measurement of Explosive Content is required for the Ocean Booking.";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			var packLine = shipment.OuterPackLines.AddNew();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_State = UNDGSubstanceLookups.StateTypes.Code.ExplosiveArticle;

			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_PackageCount = 1;
			undgDataItem.DI_F3_NKPackType = PkgUnit.Pallet;
			undgDataItem.DI_DGWeight = 1;
			undgDataItem.DI_UnitOfWeight = Weight.Kilograms;
			undgDataItem.DI_DGVolume = 1;
			undgDataItem.DI_UnitOfVolume = Volume.CubicMetres;
			undgDataItem.LinkDefault(subs);
			undgDataItem.Validation.ValidateDI_PackageCount();
			undgDataItem.Validation.ValidateDI_F3_NKPackType();
			undgDataItem.Validation.ValidateDI_DGWeight();
			undgDataItem.Validation.ValidateDI_UnitOfWeight();
			undgDataItem.Validation.ValidateDI_DGVolume();
			undgDataItem.Validation.ValidateDI_UnitOfVolume();
			AssertNoMessageError(undgDataItem.DI_PackageCountInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_F3_NKPackTypeInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_DGWeightInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_UnitOfWeightInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_DGVolumeInfo, errorMessage);
			AssertNoMessageError(undgDataItem.DI_UnitOfVolumeInfo, errorMessage);

			return (undgDataItem, errorMessage);
		}
	}
}
