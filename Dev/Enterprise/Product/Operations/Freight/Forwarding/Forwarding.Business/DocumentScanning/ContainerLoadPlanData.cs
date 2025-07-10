using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ContainerLoadPlanData),
	Constants.DocManagerCodes.ContainerLoadPlan)]

namespace Enterprise.Freight.Forwarding.Business
{
	public class ContainerLoadPlanData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CFSContainerLoadList);

		public override string ReferenceType => Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(ContainerLoadPlanCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new ContainerLoadPlanCollection(factory);

		public override ModuleIdentifier ModuleID => null;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("4d6775ce-c91c-4471-ae8f-36f4d175513d", "Container Load Plan");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
