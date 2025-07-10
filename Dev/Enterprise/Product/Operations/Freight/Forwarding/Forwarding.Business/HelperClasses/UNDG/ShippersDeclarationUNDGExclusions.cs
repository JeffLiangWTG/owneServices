using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ShippersDeclarationUNDGExclusions
	{
		public static class UNNOCodes
		{
			public static HashSet<string> ExclusionList => exclusionList ?? (exclusionList = new HashSet<string>
				{
					UN1845, UN2807, UN3164, UN3245, UN3373,
					LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries,
					LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries,
					LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries,
					LithiumBatteryConstants.UNNOCodes.PackedLithiumMetalBatteries
				});

			[ThreadStatic]
			static HashSet<string> exclusionList;

			public const string UN1845 = "1845";
			public const string UN2807 = "2807";
			public const string UN3164 = "3164";
			public const string UN3245 = "3245";
			public const string UN3373 = "3373";

			public const string UNNOPrefix = "UN";
		}

		public static bool IsLithiumUNDGs(UNDGDataItem undg)
		{
			return undg.Substance != null && IsLithiumUNDGs(undg.Substance.DG_UNNO);
		}

		public static bool IsLithiumUNDGs(ZString unno)
		{
			return LithiumBatteryConstants.UNNOCodes.CodesList.Contains(unno);
		}

		public static bool DoesPackLineRequireDeclaration(PackLine packLine)
		{
			var packLineUNDGs = packLine.UNDGs.Where(undg => undg.Substance != null);

			var undgsPermittedInLimitedQuantities = packLineUNDGs.Where(undg => ExceptedQuantityUtilities.IsSubstancePermittedInLimitedQuantities(undg.Substance));

			foreach (var undg in packLineUNDGs)
			{
				var isIATA = undg.UNDGSubstance != null && undg.UNDGSubstance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				var isExcludedFromShipperDeclaration = UNNOCodes.ExclusionList.Contains(undg.Substance.DG_UNNO);
				var isPermittedInLimitedQuantities = undgsPermittedInLimitedQuantities.Contains(undg);

				if (isIATA && !(isExcludedFromShipperDeclaration || isPermittedInLimitedQuantities))
				{
					return true;
				}
			}

			return DoUNDGsExceedExceptedQuantities(undgsPermittedInLimitedQuantities);
		}

		static bool DoUNDGsExceedExceptedQuantities(IEnumerable<UNDGDataItem> undgsPermittedInLimitedQuantities)
		{
			if (undgsPermittedInLimitedQuantities.Any())
			{
				var summedResult = undgsPermittedInLimitedQuantities
					.GroupBy(x => x.SubstanceCode)
					.Select(x => new
					{
						sumOfUNDGWeights =
						x.Where(dg => Constants.Weight.ContainsCode(dg.DI_UnitOfWeight))
									.Sum(dg => Constants.Weight.Convert(dg.DI_DGWeight, dg.DI_UnitOfWeight, Constants.Weight.Kilograms)),
						sumOfUNDGVolumes =
						x.Where(dg => Constants.Volume.ContainsCode(dg.DI_UnitOfVolume))
									.Sum(dg => Constants.Volume.Convert(dg.DI_DGVolume, dg.DI_UnitOfVolume, Constants.Volume.Litre)),
						exceptedQuantityCode = x.Select(dg => dg.ExceptedQuantity).First()
					});

				foreach (var summedData in summedResult)
				{
					var maximumAcceptedQuantity = 0M;

					var allExceptedQuantityCodesAreIdentical = undgsPermittedInLimitedQuantities.All(
						undg => undg.Substance.DG_ExceptedQuantityCode == summedData.exceptedQuantityCode
					);

					if (allExceptedQuantityCodesAreIdentical)
					{
						ExceptedQuantityUtilities.MaximumInnerQuantityDictionary.TryGetValue(
							undgsPermittedInLimitedQuantities.First().Substance.DG_ExceptedQuantityCode,
							out maximumAcceptedQuantity
						);
					}
					else
					{
						ExceptedQuantityUtilities.MaximumOuterQuantityDictionary.TryGetValue(
							ExceptedQuantityUtilities.GetMostRestrictiveExceptedQuantityCodeOfSubstances(
								undgsPermittedInLimitedQuantities.Select(dg => dg.Substance)
							),
							out maximumAcceptedQuantity);
					}

					if (summedData.sumOfUNDGWeights > maximumAcceptedQuantity || summedData.sumOfUNDGVolumes > maximumAcceptedQuantity)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static int GetCountOfDangerousPacks(IEnumerable<PackLine> packLines)
		{
			return packLines.Sum(packLine => GetCountOfDangerousPacks(packLine));
		}

		public static int GetCountOfDangerousPacks(PackLine packLine)
		{
			if (packLine == null || packLine.UNDGs.Count == 0)
			{
				return 0;
			}

			if (DoesPackLineHaveExtraUNDGDetails(packLine))
			{
				if (!UNDGsRequiringDeclaration(packLine) && !RequireLithiumBatteriesUNDGsDeclaration(packLine))
				{
					return 0;
				}
			}
			return (int)packLine.JL_PackageCount;
		}

		public static string GetTypeOfDangerousPacks()
		{
			return Res.GetString("fc2fbe3d-73e8-4ccd-95ef-4c0a64b1c9fc", "Pack(s)");
		}

		public static bool DoesPackLineHaveExtraUNDGDetails(PackLine packLine)
		{
			return packLine.UNDGs.Count > 1 || packLine.UNDGs.FirstOrDefault()?.DI_PackageCount > 0;
		}

		static bool UNDGsRequiringDeclaration(PackLine packLine)
		{
			return packLine != null && packLine.UNDGs.Any(undg => undg.Substance != null && !UNNOCodes.ExclusionList.Contains(undg.Substance.DG_UNNO));
		}

		static readonly HashSet<(ZString unno, ZString section)> LithiumBatteriesWarningMap = new HashSet<(ZString unno, ZString section)>
		{
			(LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionIA),
			(LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionIB),
			(LithiumBatteryConstants.UNNOCodes.PackedLithiumMetalBatteries, PackingInstructionSectionTypeList.Codes.SectionI),
			(LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionIA),
			(LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionIB),
			(LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries, PackingInstructionSectionTypeList.Codes.SectionI)
		};

		public static bool RequireLithiumBatteriesUNDGsDeclaration(PackLine packLine)
		{
			if (packLine == null)
			{
				return false;
			}

			return packLine.UNDGs.Any(undg => undg.Substance != null && LithiumBatteriesWarningMap.Contains((undg.Substance.DG_UNNO, undg.DI_PackingInstructionSection)));
		}

		public static IEnumerable<UNDGDataItem> GetUNDGsRequiringForQValue(UNDGDataItemCollection undgs)
		{
			if (undgs.Count == 0)
			{
				return Enumerable.Empty<UNDGDataItem>();
			}

			var packType = undgs.Count == 1
				? ExceptedQuantityUtilities.UNDGPackType.SingleUNDGPack
				: ExceptedQuantityUtilities.UNDGPackType.MultiUNDGPack;

			var result = undgs.Where(undg =>
				undg.Substance != null
				&& undg.Substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA
				&& (!UNNOCodes.ExclusionList.Contains(undg.Substance.DG_UNNO) || LithiumBatteryConstants.UNNOCodes.CodesList.Contains(undg.Substance.DG_UNNO))
				&& (!ExceptedQuantityUtilities.IsSubstancePermittedInLimitedQuantities(undg.Substance) || ValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(undg, packType))).ToArray();

			return result;
		}
	}
}
