using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(ContainerReferenceFilesData),
	Enterprise.Core.Constants.DocManagerCodes.ContainerReferenceFiles)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class ContainerReferenceFilesData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefContainer); } }
		protected override Type CollectionType
		{
			get { return typeof(RefContainerCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefContainerCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefContainer; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("9df63102-d69b-4480-b9a0-c9098339e05d", "Container (Reference Files)"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
