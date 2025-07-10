using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(SalesEnquiryData),
	Enterprise.Core.Constants.DocManagerCodes.SalesEnquiry)]

namespace Enterprise.MasterFiles.Business
{
	class SalesEnquiryData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(SalesEnquiry); }
		}

		protected override Type CollectionType
		{
			get { return typeof(SalesEnquiryCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new SalesEnquiryCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesEnquiry; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("a3c64d63-999c-4a9c-ab02-2919861bd8c9", "Inquiry"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}