using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TallyData),
	Enterprise.Core.Constants.DocManagerCodes.TallyContainer)]

namespace Enterprise.Freight.CFS.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	class TallyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(TallyContainer); } }
		protected override Type CollectionType
		{
			get { return typeof(TallyContainerCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new TallyContainerCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ManifestTally; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("f7516185-b63b-44b9-ab7c-179161e2c4b3", "Tally"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
