using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Yard.Business
{
	public abstract class CYDAssemblyData<T> : AssemblyData
		where T : BusinessObject
	{
		public override Type BusinessObjectType => typeof(T);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
