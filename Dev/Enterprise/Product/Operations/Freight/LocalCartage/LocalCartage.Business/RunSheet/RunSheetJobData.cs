using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(RunSheetJobData), Enterprise.Core.Constants.DocManagerCodes.JobCartageRunSheet)]

namespace Enterprise.Freight.LocalCartage.Business
{
	public class RunSheetJobData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CommonWorkSheet); } }
		protected override Type CollectionType
		{
			get { return typeof(ModuleCartageRunSheetCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ModuleCartageRunSheetCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CartageWorkSheet; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("d81cc08b-8c82-4ff5-acd4-36306cf4dfbf", "Port Transport Run Sheet"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
