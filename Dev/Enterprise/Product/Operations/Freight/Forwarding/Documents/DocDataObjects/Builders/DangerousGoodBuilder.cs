using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class DangerousGoodBuilder
	{
		public DangerousGood Build(UNDGDataItem itemBO, IContext context)
		{
			if (itemBO == null)
			{
				return null;
			}

			context = context ?? new CommonContext(itemBO.Factory);

			var undg = new DangerousGood(itemBO.PK)
			{
				Standard = itemBO.Substance?.DG_Standard ?? ZString.Empty
			};

			undg.Code = itemBO.Substance?.DG_Code ?? ZString.Empty;
			undg.Unno = itemBO.Substance?.DG_UNNO ?? ZString.Empty;
			undg.Variant = itemBO.Substance?.DG_Variant ?? ZString.Empty;

			undg.TransportMode = new CodeDescription(context.TransportModes)
			{
				Code = itemBO.Subs?.DG_Mode ?? ZString.Empty
			};

			undg.Contact = new ContactBuilder().Build(itemBO.DGContact) ?? new Contact();

			undg.Quantity = itemBO.DI_PackageCount;
			undg.PackedInLimitedQuantity = itemBO.DI_IsLimitedQuantity;
			undg.ProperShippingName = itemBO.Substance?.DG_PSN ?? ZString.Empty;
			undg.TechnicalName = itemBO.DI_TechnicalName;
			undg.IMOClass = itemBO.Substance?.DG_Class ?? itemBO.DI_IMOClass;
			undg.PackingGroup = itemBO.Substance?.DG_PG ?? ZString.Empty;
			undg.SubLabel1 = itemBO.Substance?.DG_SubLabel1 ?? ZString.Empty;
			undg.SubLabel2 = itemBO.Substance?.DG_SubLabel2 ?? ZString.Empty;
			undg.State = itemBO.Substance?.DG_State ?? ZString.Empty;
			undg.MaterialFormDescription = itemBO.DI_MaterialFormDescription;
			undg.RadionuclideElementSuffix = itemBO.DI_RadionuclideElementSuffix;
			undg.SpecialPermitNumber = itemBO.DI_SpecialPermitNumber;
			undg.HazardousWasteCode = itemBO.DI_HazardousWasteCode;
			undg.PSAGroup = itemBO.IsPSAGroupApplicable ? itemBO.PSAGroup : ZString.Empty;
			undg.IsFissileExcepted = itemBO.DI_IsFissileExcepted;
			undg.IsExclusiveUse = itemBO.DI_IsExclusiveUse;
			undg.IsHighwayRouteControlledQuantity = itemBO.DI_IsHighwayRouteControlledQuantity;
			undg.IsResidueLastContained = itemBO.DI_IsResidueLastContained;
			undg.IsSalvagePackaging = itemBO.DI_IsSalvagePackaging;
			undg.RadioactiveTransportIndex = itemBO.DI_RadioactiveTransportIndex;

			var parentPackLine = itemBO.Factory.Load<PackLine>(itemBO.DI_ParentID);
			undg.RequiresTemperatureControl = parentPackLine?.JL_RequiresTemperatureControl ?? false;
			undg.RequiredTemperatureMaximum = new Measurement
			{
				Value = parentPackLine?.JL_RequiredTemperatureMaximum ?? 0,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = parentPackLine?.JL_RequiredTemperatureUnit ?? string.Empty
				}
			};

			undg.Weight = new Measurement
			{
				Value = itemBO.DI_DGWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = itemBO.DI_UnitOfWeight
				}
			};

			undg.Volume = new Measurement
			{
				Value = itemBO.DI_DGVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = itemBO.DI_UnitOfVolume
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

			undg.RadioactiveMaximumActivity = new Measurement
			{
				Value = itemBO.DI_RadioactiveMaximumActivity,
				Unit = new CodeDescription(context.RadioactiveUnits)
				{
					Code = itemBO.DI_RadioactiveMaximumActivityUnit
				}
			};

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

			undg.RadioactiveLabelCategory = new CodeDescription(new RadioactiveLabelCategoryList())
			{
				Code = itemBO.DI_RadioactiveLabelCategory
			};

			undg.RadionuclideElement = new CodeDescription(new RadionuclideElementList())
			{
				Code = itemBO.DI_RadionuclideElement
			};

			var emsCodes = (itemBO.Substance?.DG_EMS ?? ZString.Empty).Split(',');
			foreach (var emsCode in emsCodes)
			{
				if (emsCode.StartsWith("F-") || emsCode.StartsWith("f-") || emsCode.Equals("*"))
				{
					undg.EmergencyScheduleFire = new CodeDescription(new EmergencyScheduleFireCodes())
					{
						Code = emsCode
					};
				}

				if (emsCode.StartsWith("S-") || emsCode.StartsWith("s-") || emsCode.Equals("*"))
				{
					undg.EmergencyScheduleSpillage = new CodeDescription(new EmergencyScheduleSpillageCodes())
					{
						Code = emsCode
					};
				}
			}

			if (itemBO.Substance?.Lookups.ExceptedQuantityList is CodeDescriptionPairList exceptedQuantityList)
			{
				undg.ExceptedQuantityCode = new CodeDescription(exceptedQuantityList)
				{
					Code = itemBO.Substance.DG_ExceptedQuantityCode
				};
			}

			if (itemBO.Substance?.StandardSubstance is UNDGSubstanceCFR cfrSubstance)
			{
				undg.Prefix = cfrSubstance.CFR_Prefix;
				undg.SecondaryClass = cfrSubstance.CFR_SecondaryClass;
				undg.TertiaryClass = cfrSubstance.CFR_TertiaryClass;
				undg.PoisonInhalationHazard = cfrSubstance.CFR_PoisonInhalationHazard;

				undg.ReportableQuantity = new Measurement
				{
					Value = cfrSubstance.CFR_ReportableQuantity,
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = cfrSubstance.CFR_ReportableQuantityUnit
					}
				};
			}

			var kgNetExplosiveWeight = Constants.Weight.Convert(itemBO.DI_NECWeight, itemBO.DI_NECWeightUQ, Core.Constants.Weight.Kilograms);

			if (!itemBO.DI_NECWeight.IsDefault && itemBO.DI_NECWeight != 0)
			{
				undg.NetExplosiveWeight = new Measurement
				{
					Value = Utilities.Round(kgNetExplosiveWeight, 3),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = Core.Constants.Weight.Kilograms
					}
				};
			}

			return undg;
		}
	}
}
