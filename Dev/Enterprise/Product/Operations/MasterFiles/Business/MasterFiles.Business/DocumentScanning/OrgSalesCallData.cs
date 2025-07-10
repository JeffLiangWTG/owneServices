using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(OrgSalesCallData),
	Enterprise.Core.Constants.DocManagerCodes.CommunicationManager)]

namespace Enterprise.MasterFiles.Business
{
	class OrgSalesCallData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(OrgSalesCall); }
		}

		protected override Type CollectionType
		{
			get { return typeof(OrgSalesCallCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new OrgSalesCallCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Communication; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("9130b2c3-f655-4086-8257-7901513bd040", "Communication"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}