using System;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HVLVOriginLoadListData),
	Enterprise.Core.Constants.DocManagerCodes.HVLVOriginLoadList)]

namespace Enterprise.eTail.Business
{
	class HVLVOriginLoadListData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HVLVOriginLoadList);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("b0bff7f7-b4bb-4ca9-8cf5-8b6a7142795d1", "Load List");

		protected override Type CollectionType => typeof(HVLVOriginLoadListCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HVLVOriginLoadListAdhocEdocsSupportCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
