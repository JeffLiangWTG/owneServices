using System;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HVLVOuterPackageData),
	Enterprise.Core.Constants.DocManagerCodes.HVLVOuterPackage)]

namespace Enterprise.eTail.Business
{
	class HVLVOuterPackageData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HVLVOuterPackage);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("e109d9bf-d32a-4d4a-8739-57b9892fa041", "Outer Package");

		protected override Type CollectionType => typeof(HVLVOuterPackageCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HVLVOuterPackageAdhocEdocsSupportCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
