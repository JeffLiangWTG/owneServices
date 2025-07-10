using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ContainerData),
	Enterprise.Core.Constants.DocManagerCodes.Container)]

namespace Enterprise.Freight.CFS.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class ContainerData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CFSContainer); } }
		protected override Type CollectionType
		{
			get { return typeof(CFSShipmentList); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CFSContainerRegistrationList(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.PackContainerRegistration; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("02a4edc0-c14b-4b3c-af85-1b6074bf02ab", "Container"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
