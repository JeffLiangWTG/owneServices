using System;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PkgPackageData),
	Enterprise.Core.Constants.DocManagerCodes.Package)]

namespace Enterprise.Packing.Business
{
	public class PkgPackageData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(PkgPackage);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(PkgPackageCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("ac2b3907-6511-405f-9e1a-acbb2861a432", "Package");
	}
}
