using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ContainerDetentionData),
	Enterprise.Core.Constants.DocManagerCodes.DetentionInvoice)]

namespace Enterprise.Freight.Agency.Business
{
	class ContainerDetentionData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ContainerDetention); } }
		protected override Type CollectionType
		{
			get { return typeof(ContainerDetentionCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ContainerDetentionCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AgencyContainerDetention; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a2f02f25-62f9-436d-beec-b432e3651f08", "Detention Invoice"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
