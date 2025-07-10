using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ContainerLoadListData),
	Constants.DocManagerCodes.ContainerLoadList)]

namespace Enterprise.Freight.Forwarding.Business
{
	public class ContainerLoadListData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CYContainerLoadList);

		public override string ReferenceType => Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(ContainerLoadListCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new ContainerLoadListCollection(factory);

		public override ModuleIdentifier ModuleID => null;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("4d6775ce-c91c-4471-ae8f-36f4d175516d", "Container Load List");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
