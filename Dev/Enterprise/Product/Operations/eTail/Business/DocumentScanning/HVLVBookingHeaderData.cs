using System;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HVLVBookingHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.HVLVBookingHeader)]

namespace Enterprise.eTail.Business
{
	class HVLVBookingHeaderData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HVLVBookingHeader);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("7218af9e-b18c-4802-a8b0-56721fce5776", "Booking Header");

		protected override Type CollectionType => typeof(HVLVBookingHeaderAdhocEdocsSupportCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HVLVBookingHeaderAdhocEdocsSupportCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
