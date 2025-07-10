using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(VesselData),
	Enterprise.Core.Constants.DocManagerCodes.Vessel)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class VesselData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefVessel); } }
		protected override Type CollectionType
		{
			get { return typeof(RefVesselCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefVesselCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefVessel; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("7185561b-934e-4dd9-9a4e-03ffded5d7b9", "Vessel"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
