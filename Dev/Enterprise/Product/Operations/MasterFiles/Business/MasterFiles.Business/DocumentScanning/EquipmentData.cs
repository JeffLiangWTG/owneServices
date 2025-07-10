using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(EquipmentData),
	Enterprise.Core.Constants.DocManagerCodes.Equipment)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class EquipmentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefEquipment); } }
		protected override Type CollectionType
		{
			get { return typeof(RefEquipmentCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefEquipmentCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefEquipment; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("d7cb96e1-4e50-4a6e-bdb2-cc7965b687ae", "Equipment"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
