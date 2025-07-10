using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.Business.TransitConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	public static class TWHJobValidationHelper
	{
		#region UNDG

		public static string ValidateDGPackageNotExceedCore(IEnumerable<WhsItemPackageState> pkgStates, WhsWarehouse warehouse)
		{
			var errorMessages = new List<string>();

			if (warehouse.WW_IsDangerousGoodsManagementEnabled)
			{
				var factory = new BusinessObjectFactory();

				var dgDataItems = GetUNDGDataItems(pkgStates, factory);

				if (dgDataItems.Any())
				{
					var dgSubstancePKs = dgDataItems.Select(di => di.DI_DG);

					var undgSubstances = GetUNDGSubstances(dgSubstancePKs, factory);

					var (references, undgCountryReferencePivots) = GetUNDGCountryReferencesAndPivots(undgSubstances, factory);

					var undgTotalsViews = GetUNDGTotalsView(dgDataItems, undgSubstances, references, warehouse, factory);

					foreach (var undgTotalsView in undgTotalsViews.OrderByDescending(u => u.WDT_Source).ThenByDescending(u => u.WDT_Code))
					{
						var caption = string.Empty;
						var code = string.Empty;

						var currentWeight = undgTotalsView.WDT_TotalWeight;
						var currentVolume = undgTotalsView.WDT_TotalVolume;
						var currentWeightPercentage = 0m;
						var currentVolumePercentage = 0m;

						if (undgTotalsView.WDT_Source == DGSource.UNDGSubstance)
						{
							code = undgSubstances.FirstOrDefault(s => s.PK == undgTotalsView.WDT_DG_Substance).DG_Code;
							caption = dgCaption;

							foreach (var dgDataItem in dgDataItems.Where(d => d.DI_DG == undgTotalsView.WDT_DG_Substance))
							{
								currentWeight += ConvertWeightUnit(dgDataItem.DI_DGWeight, dgDataItem.DI_UnitOfWeight, undgTotalsView.WDT_TotalWeightLimitUQ);
								currentVolume += ConvertVolumeUnit(dgDataItem.DI_DGVolume, dgDataItem.DI_UnitOfVolume, undgTotalsView.WDT_TotalVolumeLimitUQ);
							}
						}
						else if (undgTotalsView.WDT_Source == DGSource.CountryReference)
						{
							var matchedReference = references.FirstOrDefault(r => r.PK == undgTotalsView.WDT_DCR_UNDGCountryReference);

							if (matchedReference != null)
							{
								code = matchedReference.DCR_Code;
								caption = matchedReference.DCR_Type;

								var matchedReferencePivots = undgCountryReferencePivots.Where(rp => rp.DCP_DCR == matchedReference.PK);

								CalculateCountryReferenceRelatedDataItemWeightAndVolume(matchedReferencePivots, undgTotalsView, dgDataItems, undgSubstances, ref currentWeight, ref currentVolume);
							}
						}
						else if (undgTotalsView.WDT_Source == DGSource.UNDGClass)
						{
							code = undgTotalsView.WDT_Code;
							caption = dgClassCaption;

							foreach (var dgDataItem in dgDataItems.Where(d => d.DI_IMOClass.StartsWith(undgTotalsView.WDT_Code)))
							{
								currentWeight += ConvertWeightUnit(dgDataItem.DI_DGWeight, dgDataItem.DI_UnitOfWeight, undgTotalsView.WDT_TotalWeightLimitUQ);
								currentVolume += ConvertVolumeUnit(dgDataItem.DI_DGVolume, dgDataItem.DI_UnitOfVolume, undgTotalsView.WDT_TotalVolumeLimitUQ);
							}
						}

						currentWeightPercentage = undgTotalsView.WDT_TotalWeightLimit > 0 ? currentWeight / undgTotalsView.WDT_TotalWeightLimit * 100 : 0;
						currentVolumePercentage = undgTotalsView.WDT_TotalVolumeLimit > 0 ? currentVolume / undgTotalsView.WDT_TotalVolumeLimit * 100 : 0;

						if (undgTotalsView.WDT_TotalWeightLimit == 0 && undgTotalsView.WDT_TotalVolumeLimit == 0)
						{
							errorMessages.Add(Res.GetString("42f6b288-ba14-499a-ae88-9a4cf72db2b7", "{0} {1} is not allowed to be stored in the whs.", caption, code));
						}
						else
						{
							if (undgTotalsView.WDT_TotalWeightLimit > 0 && currentWeight > undgTotalsView.WDT_TotalWeightLimit)
							{
								errorMessages.Add(Res.GetString("4be8dcec-18c4-4b10-831d-d072200d089f", "{0} {1} would put the whs. at {2}% weight capacity.", caption, code, Utilities.Round(currentWeightPercentage, 0)));
							}
							if (undgTotalsView.WDT_TotalVolumeLimit > 0 && currentVolume > undgTotalsView.WDT_TotalVolumeLimit)
							{
								errorMessages.Add(Res.GetString("8479ee63-bd34-48d3-acb0-e52236cd71f6", "{0} {1} would put the whs. at {2}% volume capacity.", caption, code, Utilities.Round(currentVolumePercentage, 0)));
							}
						}
					}
				}
			}

			return errorMessages.Count > 0 ? string.Join("\r\n", errorMessages.ToArray()) : null;
		}

		static UNDGDataItem[] GetUNDGDataItems(IEnumerable<WhsItemPackageState> pkgStates, IFactory factory)
		{
			if (pkgStates == null || !pkgStates.Any())
			{
				return Array.Empty<UNDGDataItem>();
			}

			var pkgPKs = pkgStates.Select(ps => ps.WPS_KP_Package);
			var undgItemsQuery = new ZDBOnlyQuery(typeof(UNDGDataItem));
			undgItemsQuery.AddToFilter(UNDGDataItemSchema.DI_ParentTableCode, PkgPackageSchema.Constants.Prefix);
			undgItemsQuery.AddToFilter(UNDGDataItemSchema.DI_ParentID, pkgPKs);
			return factory
				.Load<UNDGDataItem>(undgItemsQuery)
				.ToArray();
		}

		static UNDGSubstance[] GetUNDGSubstances(IEnumerable<ZGuid> dgSubstancePKs, IFactory factory)
		{
			var undgSubstancesQuery = new ZDBOnlyQuery(typeof(UNDGSubstance));
			undgSubstancesQuery.AddToFilter(UNDGSubstanceSchema.PK, dgSubstancePKs);

			return factory.Load<UNDGSubstance>(undgSubstancesQuery).ToArray();
		}

		static (UNDGCountryReference[], UNDGCountryReferencePivot[]) GetUNDGCountryReferencesAndPivots(UNDGSubstance[] undgSubstances, IFactory factory)
		{
			var undgSubstanceUNNOs = undgSubstances.Select(s => s.DG_UNNO);

			var undgCountryReferencePivotQuery = new ZDBOnlyQuery(typeof(UNDGCountryReferencePivot));
			undgCountryReferencePivotQuery.AddToFilter(UNDGCountryReferencePivotSchema.DCP_UNNO, undgSubstanceUNNOs);

			var undgCountryReferencePivots = factory
			.Load<UNDGCountryReferencePivot>(undgCountryReferencePivotQuery)
			.ToArray();

			var referencePKs = undgCountryReferencePivots.Select(s => s.DCP_DCR);

			var referencesQuery = new ZDBOnlyQuery(typeof(UNDGCountryReference));
			referencesQuery.AddToFilter(UNDGCountryReferenceSchema.PK, referencePKs);

			return (factory.Load<UNDGCountryReference>(referencesQuery).ToArray(), undgCountryReferencePivots);
		}

		static WhsItemUNDGTotalsView[] GetUNDGTotalsView(UNDGDataItem[] dgDataItems, UNDGSubstance[] undgSubstances, UNDGCountryReference[] references, WhsWarehouse warehouse, IFactory factory)
		{
			var dgSubstancePKs = dgDataItems.Select(di => di.DI_DG);
			var dgBaseClasses = dgDataItems.Where(di => di.DI_IMOClass.Length > 0).Select(di => di.DI_IMOClass.Substring(0, 1)).Distinct().ToList();
			var referencePKs = references.Select(r => r.PK);

			var undgTotalsViewsQuery = new ZDBOnlyQuery(typeof(WhsItemUNDGTotalsView));

			var substanceQuery = new ZQuery().AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_Source, "DG").AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_DG_Substance, dgSubstancePKs);
			var countryReferenceQuery = new ZQuery().AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_Source, "DCR").AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_DCR_UNDGCountryReference, referencePKs);
			var baseClassesQuery = new ZQuery().AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_Source, "CLS").AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_Code, dgBaseClasses);

			var combinedSourceQuery = new ZQuery();
			combinedSourceQuery.AddToFilter(substanceQuery, JoinCondition.Or);
			combinedSourceQuery.AddToFilter(countryReferenceQuery, JoinCondition.Or);
			combinedSourceQuery.AddToFilter(baseClassesQuery, JoinCondition.Or);

			undgTotalsViewsQuery.AddToFilter(combinedSourceQuery);
			undgTotalsViewsQuery.AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_WW_Warehouse, warehouse.PK);
			undgTotalsViewsQuery.AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_IsLimited, true);

			return factory.Load<WhsItemUNDGTotalsView>(undgTotalsViewsQuery).ToArray();
		}

		static void CalculateCountryReferenceRelatedDataItemWeightAndVolume(IEnumerable<UNDGCountryReferencePivot> undgCountryReferencePivots, WhsItemUNDGTotalsView undgTotalsView, UNDGDataItem[] dgDataItems, UNDGSubstance[] undgSubstances, ref ZDecimal currentWeight, ref ZDecimal currentVolume)
		{
			foreach (var undgCountryReferencePivot in undgCountryReferencePivots)
			{
				var undgSubstance = undgSubstances.FirstOrDefault(s => s.DG_UNNO == undgCountryReferencePivot.DCP_UNNO);
				var matchedDGDataItems = dgDataItems.Where(d => undgSubstance.PK == d.DI_DG);
				if (matchedDGDataItems.Any())
				{
					foreach (var dgDataItem in matchedDGDataItems)
					{
						var weightConverted = ConvertWeightUnit(dgDataItem.DI_DGWeight, dgDataItem.DI_UnitOfWeight, undgTotalsView.WDT_TotalWeightLimitUQ);
						var volumeConverted = ConvertVolumeUnit(dgDataItem.DI_DGVolume, dgDataItem.DI_UnitOfVolume, undgTotalsView.WDT_TotalVolumeLimitUQ);

						currentWeight += weightConverted;
						currentVolume += volumeConverted;
					}
				}
			}
		}

		static ZDecimal ConvertWeightUnit(ZDecimal value, ZString fromUnit, ZString toUnit)
		{
			if (Constants.Weight.ContainsCode(fromUnit) && Constants.Weight.ContainsCode(toUnit))
			{
				return Constants.Weight.Convert(value, fromUnit, toUnit);
			}
			return value;
		}

		static ZDecimal ConvertVolumeUnit(ZDecimal value, ZString fromUnit, ZString toUnit)
		{
			if (Constants.Volume.ContainsCode(fromUnit) && Constants.Volume.ContainsCode(toUnit))
			{
				return Constants.Volume.Convert(value, fromUnit, toUnit);
			}
			return value;
		}

		static string dgCaption => Res.GetString("ba6016e7-9148-4c27-8f28-d05ff4f98b8c", "DG");

		static string dgClassCaption => Res.GetString("ad9afced-6f56-46b8-972b-ac4d274d9f48", "UNDG Class");

		#endregion

		#region Package Totals

		public static (decimal totalWeightInKG, decimal totalVolumeInM3, int totalQuantity) GetTotalsOfPackageStates(IEnumerable<WhsItemPackageState> packageStates)
		{
			if (packageStates == null || !packageStates.Any())
			{
				return (0m, 0m, 0);
			}

			var packages = packageStates.Select(wps => wps.Package);
			var totalWeightInKG = packages.Sum(pkg => Constants.Weight.Convert(pkg.KP_Weight, pkg.KP_WeightUQ, Constants.Weight.Kilograms));
			var totalVolumeInM3 = packages.Sum(pkg => Constants.Volume.Convert(pkg.KP_Volume, pkg.KP_VolumeUQ, Constants.Volume.CubicMetres));
			var totalQuantity = packages.Sum(pkg => pkg.KP_PackageQty);

			return (totalWeightInKG, totalVolumeInM3, totalQuantity);
		}

		public static IEnumerable<WhsItemPackageState> GetValidPackageStates(IEnumerable<WhsItemPackageState> packageStates)
		{
			return packageStates.Where(wps => wps.WPS_Status != TransitWarehouseStatuses.Codes.Departed && wps.WPS_Status != TransitWarehouseStatuses.Codes.AdjustedOut && wps.WPS_Status != TransitWarehouseStatuses.Codes.Finalized);
		}

		#endregion

		#region Validation Error Messages

		public static string GetErrorMessage(ValidationErrorCode? code, string existingErrorMessage)
		{
			var errorMessageBuilder = new StringBuilder($"{ErrorMessages[code]}");
			if (!string.IsNullOrEmpty(existingErrorMessage))
			{
				errorMessageBuilder.Append(System.Environment.NewLine);
				errorMessageBuilder.Append(existingErrorMessage);
			}

			return errorMessageBuilder.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message strings")]
		public static Dictionary<ValidationErrorCode?, string> ErrorMessages => new()
		{
			{ ValidationErrorCode.FNF, "Facility Not Found" },
			{ ValidationErrorCode.JNF, "Job or Instruction Not Found" },
			{ ValidationErrorCode.VDG, "Violation of Dangerous Goods Validation" },
			{ ValidationErrorCode.INV, "Invalid Reference Number Type" }
		};

		#endregion
	}
}
