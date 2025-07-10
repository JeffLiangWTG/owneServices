using System;
using System.Collections;
using System.Globalization;

namespace Enterprise.Packing.Business
{
	public class PkgPackageComparer : ComparerWithCacheableSortProperties<PkgPackage>
	{
		CaseInsensitiveComparer CaseInsensitiveComparer { get; } = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);

		protected override int CompareCore(PkgPackage x, PkgPackage y)
		{
			var result = Compare(x, y, nameof(PkgPackage.KP_Sequence), package => (short)package.KP_Sequence, (valueX, valueY) =>
			{
				if (valueX <= 0 || valueY <= 0)
				{
					return 0;
				}
				else if (valueX < valueY)
				{
					return -1;
				}
				else if (valueX > valueY)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			});

			if (result == 0)
			{
				result = Compare(x, y, nameof(PkgPackage.Description), package => (string)package.Description, CaseInsensitiveComparer.Compare);
				if (result == 0) // secondary sort by PackageID, empty packages last.
				{
					result = Compare(x, y, nameof(PkgPackage.KP_PackageID), package => (string)package.KP_PackageID, (valueX, valueY) =>
					{
						if (valueX.Length == 0 && valueY.Length > 0)
						{
							return 1;
						}
						else if (valueX.Length > 0 && valueY.Length == 0)
						{
							return -1;
						}
						else
						{
							return CaseInsensitiveComparer.Compare(valueX, valueY);
						}
					});
				}
			}

			return result;
		}

		public IDisposable CacheSortingProperties() => CacheSortProperties();
	}
}
