using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	public interface IPackLineOverrider
	{
		void SetPackageOverride(PackLine packLine);
		void SetPackageCollectionOverride(List<PackLine> packLines);
	}
}
