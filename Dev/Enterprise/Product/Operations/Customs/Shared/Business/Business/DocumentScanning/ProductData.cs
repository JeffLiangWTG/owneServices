using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ProductData),
	Enterprise.Core.Constants.DocManagerCodes.Product)]

namespace Enterprise.Customs.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public sealed class ProductData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(OrgSupplierPart); } }
		protected override Type CollectionType
		{
			get { return typeof(OrgSupplierPartCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new OrgSupplierPartCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.SupplierPart; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("2b8ce775-295c-415c-baeb-c7c9dc84b302", "Product"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
