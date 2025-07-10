using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class PackingLineExtensions
	{
		#region GetPackTypeCode

		public static string GetPackTypeCode(this IReadOnlyCollection<IPackingLine> packingLines)
		{
			if (packingLines == null
				|| packingLines.Count == 0)
			{
				return string.Empty;
			}

			if (!packingLines.TryGetPackTypeCode(out var packTypeCode))
			{
				packTypeCode = Core.Constants.PkgUnit.Package;
			}

			return packTypeCode;
		}

		public static bool TryGetPackTypeCode(this IReadOnlyCollection<IPackingLine> packingLines, out string packTypeCode)
		{
			if (packingLines == null
				|| packingLines.Count == 0)
			{
				packTypeCode = string.Empty;
				return false;
			}

			return packingLines
				.Select(p => p.PackageType)
				.ToArray()
				.TryGetPackTypeCode(out packTypeCode);
		}

		public static string GetPackTypeCode(this IReadOnlyCollection<ICodeDescription> packageTypes)
		{
			if (packageTypes == null
				|| packageTypes.Count == 0)
			{
				return string.Empty;
			}

			if (!packageTypes.TryGetPackTypeCode(out var packTypeCode))
			{
				packTypeCode = Core.Constants.PkgUnit.Package;
			}

			return packTypeCode;
		}

		public static bool TryGetPackTypeCode(this IReadOnlyCollection<ICodeDescription> packageTypes, out string packTypeCode)
		{
			if (packageTypes == null
				|| packageTypes.Count == 0)
			{
				packTypeCode = string.Empty;
				return false;
			}

			var res = string.Empty;

			foreach (var packType in packageTypes)
			{
				if (packType == null)
				{
					continue;
				}

				if (string.IsNullOrEmpty(res))
				{
					res = packType.Code;
				}
				else
				{
					if (string.CompareOrdinal(res, packType.Code) != 0)
					{
						packTypeCode = string.Empty;
						return false;
					}
				}
			}

			packTypeCode = res;
			return true;
		}

		#endregion

		#region GetPackTypeDescription

		public static string GetPackTypeDescription(this IReadOnlyCollection<IPackingLine> packingLines)
		{
			return packingLines
				?.Select(p => p.PackageType)
				.ToArray()
				.GetPackTypeDescription()
				?? string.Empty;
		}

		public static string GetPackTypeDescription(this IReadOnlyCollection<ICodeDescription> packageTypes)
		{
			var res = string.Empty;

			foreach (var packType in packageTypes)
			{
				if (packType == null)
				{
					continue;
				}

				if (string.IsNullOrEmpty(res))
				{
					res = packType.Description;
				}
				else
				{
					if (string.CompareOrdinal(res, packType.Description) != 0)
					{
						res = Core.Constants.PkgUnit.GetNonPluralDescription(Core.Constants.PkgUnit.Package)?.GetUnresolvedString();
						break;
					}
				}
			}

			return res;
		}

		#endregion

		#region DoAllPackingLinesHaveSameUnitOfMeasure (Weight/Volume)

		public static bool HaveSameUnitOfWeight(this IReadOnlyCollection<IPackingLine> packingLines) => DoAllPackingLinesHaveSameUnitOfMeasure(packingLines, GetPackingLineUnitOfWeight);
		public static bool HaveSameUnitOfVolume(this IReadOnlyCollection<IPackingLine> packingLines) => DoAllPackingLinesHaveSameUnitOfMeasure(packingLines, GetPackingLineUnitOfVolume);

		static string GetPackingLineUnitOfWeight(IPackingLine p) => p.Weight?.Unit?.Code;
		static string GetPackingLineUnitOfVolume(IPackingLine p) => p.Volume?.Unit?.Code;

		static bool DoAllPackingLinesHaveSameUnitOfMeasure(IReadOnlyCollection<IPackingLine> packingLines, Func<IPackingLine, string> unitProvider)
		{
			switch (packingLines?.Count ?? 0)
			{
				case 0:
					return false;

				case 1:
					return true;

				default:
					string firstPackingLineUnitOfMeasure = null;

					foreach (var packingLine in packingLines)
					{
						var packingLineUnitOfMeasure = unitProvider(packingLine);

						if (string.IsNullOrWhiteSpace(packingLineUnitOfMeasure))
						{
							return false;
						}

						if (firstPackingLineUnitOfMeasure == null)
						{
							firstPackingLineUnitOfMeasure = packingLineUnitOfMeasure;
						}
						else
						{
							if (string.Compare(firstPackingLineUnitOfMeasure, packingLineUnitOfMeasure, StringComparison.OrdinalIgnoreCase) != 0)
							{
								return false;
							}
						}
					}

					return true;
			}
		}

		#endregion
	}
}
