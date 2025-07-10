using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(UNLOCOData),
	Enterprise.Core.Constants.DocManagerCodes.UNLOCO)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class UNLOCOData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefUNLOCO); } }
		protected override Type CollectionType
		{
			get { return typeof(RefUNLOCOCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefUNLOCOCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefUNLOCO; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1335bdd0-98e1-4724-9314-f7ec545d55aa", "UNLOCO"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
