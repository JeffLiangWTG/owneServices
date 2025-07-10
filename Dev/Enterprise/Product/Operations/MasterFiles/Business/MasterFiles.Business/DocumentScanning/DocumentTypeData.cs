using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DocumentTypeData),
	Enterprise.Core.Constants.DocManagerCodes.DocumentType)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class DocumentTypeData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(RefDocType); } }
		protected override Type CollectionType
		{
			get { return typeof(RefDocTypeCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new RefDocTypeCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.RefDocType; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.GeneralReferenceTables; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ed99c910-a9d1-468d-ac99-d713fc9368c2", "Document Type"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
