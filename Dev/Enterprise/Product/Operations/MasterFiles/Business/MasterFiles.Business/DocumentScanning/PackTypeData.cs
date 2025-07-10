using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(PackTypeData),
	Enterprise.Core.Constants.DocManagerCodes.PackType)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class PackTypeData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefPackType); } }
		protected override Type CollectionType
		{
			get { return typeof(RefPackTypeCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefPackTypeCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefPackType; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("20ece634-8ee8-497c-abf6-632c1e878ae2", "Pack Type"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
