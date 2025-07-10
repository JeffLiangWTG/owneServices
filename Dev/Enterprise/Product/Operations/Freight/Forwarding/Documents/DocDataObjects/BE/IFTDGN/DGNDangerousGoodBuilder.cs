using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class DGNDangerousGoodBuilder
	{
		public DGNDangerousGood Build(UNDGDataItem itemBO)
		{
			if (itemBO == null)
			{
				return null;
			}

			var context = new CommonContext(itemBO.Factory);
			var undg = new DGNDangerousGood(itemBO.PK);

			var unitOfWeight = Constants.Weight.Kilograms;
			var unitOfVolume = Constants.Volume.CubicMetres;

			undg.Code = itemBO.Substance?.DG_Code ?? ZString.Empty;
			undg.Unno = itemBO.Substance?.DG_UNNO ?? ZString.Empty;
			undg.RegulationStandard = itemBO.Substance?.DG_Standard ?? ZString.Empty;
			undg.Variant = itemBO.Substance?.DG_Variant ?? ZString.Empty;

			undg.TransportMode = new CodeDescription(context.TransportModes)
			{
				Code = itemBO.Subs?.DG_Mode ?? ZString.Empty
			};

			undg.Quantity = itemBO.DI_PackageCount;
			undg.PackedInLimitedQuantity = itemBO.DI_IsLimitedQuantity;
			undg.ProperShippingName = itemBO.Substance?.DG_PSN ?? ZString.Empty;

			undg.TechnicalName = itemBO.DI_TechnicalName;
			if (undg.TechnicalName.IsEmpty)
			{
				undg.TechnicalName = itemBO.ProperShippingName;
			}

			undg.IMOClass = itemBO.Substance?.DG_Class ?? itemBO.DI_IMOClass;
			undg.PackingGroup = itemBO.Substance?.DG_PG ?? ZString.Empty;
			undg.SubLabel1 = itemBO.Substance?.DG_SubLabel1 ?? ZString.Empty;
			undg.SubLabel2 = itemBO.Substance?.DG_SubLabel2 ?? ZString.Empty;
			undg.State = itemBO.Substance?.DG_State ?? ZString.Empty;
			undg.Contact = new ContactBuilder().Build(itemBO.DGContact) ?? new Contact();
			undg.Standard = itemBO.Substance?.DG_Standard ?? ZString.Empty;

			if (itemBO.Substance?.StandardSubstance is UNDGSubstanceCFR cfrSubstance)
			{
				undg.SecondaryClass = cfrSubstance.CFR_SecondaryClass;
				undg.TertiaryClass = cfrSubstance.CFR_TertiaryClass;
			}

			var weight = Constants.Weight.Convert(itemBO.DI_DGWeight, itemBO.DI_UnitOfWeight, unitOfWeight);

			undg.Weight = new Measurement
			{
				Value = weight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			var volume = Constants.Volume.Convert(itemBO.DI_DGVolume, itemBO.DI_UnitOfVolume, unitOfVolume);

			undg.Volume = new Measurement
			{
				Value = volume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};

			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				if (itemBO.DI_IsCombustible)
				{
					undg.FlashPoint = new Measurement
					{
						Value = itemBO.DI_DGFlashPoint,
						Unit = new CodeDescription(context.TemperatureUnits)
						{
							Code = Constants.Temperature.Centigrade
						}
					};
				}
				else
				{
					undg.FlashPoint = null;
				}
			}
			else
			{
				undg.FlashPoint = new Measurement
				{
					Value = itemBO.DI_DGFlashPoint,
					Unit = new CodeDescription(context.TemperatureUnits)
					{
						Code = Constants.Temperature.Centigrade
					}
				};
			}

			undg.PackageType = new CodeDescription(itemBO.Lookups.PackTypes)
			{
				Code = itemBO.DI_F3_NKPackType
			};

			undg.MarinePollutant = new CodeDescription(itemBO.Lookups.MarinePollutantList)
			{
				Code = itemBO.DI_MPMarinePollutant.IsEmpty
					? itemBO.Substance?.DG_MP ?? ZString.Empty
					: itemBO.DI_MPMarinePollutant
			};

			string dg_emsCodes = itemBO.Substance?.DG_EMS ?? ZString.Empty;
			string[] dg_emsCodeList = dg_emsCodes.Split(',');

			foreach (var dg_emsCode in dg_emsCodeList)
			{
				if (dg_emsCode.StartsWith("F-") || dg_emsCode.StartsWith("f-") || dg_emsCode.Equals("*"))
				{
					undg.EmergencyScheduleFire = new CodeDescription(new EmergencyScheduleFireCodes())
					{
						Code = dg_emsCode
					};
				}

				if (dg_emsCode.StartsWith("S-") || dg_emsCode.StartsWith("s-") || dg_emsCode.Equals("*"))
				{
					undg.EmergencyScheduleSpillage = new CodeDescription(new EmergencyScheduleSpillageCodes())
					{
						Code = dg_emsCode
					};
				}
			}

			undg.NetExplosiveWeight = new Measurement
			{
				Unit = new CodeDescription(new ExplosiveWeightUnitCodes())
			};

			undg.Radioactivity = new Measurement
			{
				Unit = new CodeDescription(new RadioactivityUnitCodes())
			};

			return undg;
		}
	}
}
