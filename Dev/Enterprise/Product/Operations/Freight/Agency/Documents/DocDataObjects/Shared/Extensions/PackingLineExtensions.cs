using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public static class PackingLineExtensions
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

		static bool TryGetPackTypeCode(this IReadOnlyCollection<ICodeDescription> packageTypes, out string packTypeCode)
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
	}
}
