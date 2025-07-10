using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.AMS;

namespace Enterprise.Customs.Business
{
	public class PackageTypeMapping : AMSConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZString GetPackageType(ZString unitCode)
		{
			ZString result = base.GetPackageType(unitCode);
			switch (unitCode)
			{
				case "CS":
					result = AMSConstants.PackageType.Case;
					break;
				case Constants.PkgUnit.Pallet:
					result = AMSConstants.PackageType.Pail;
					break;
			}
			return result;
		}
	}
}
