using System;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HVLVConsignmentData),
	Enterprise.Core.Constants.DocManagerCodes.HVLVConsignment)]

namespace Enterprise.eTail.Business
{
	class HVLVConsignmentData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HVLVConsignment);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("650577d8-43f9-45e0-9850-79be10bf5335", "Consignment");

		protected override Type CollectionType => typeof(HVLVShipmentConsignmentCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HVLVConsignmentAdhocEdocsSupportCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
