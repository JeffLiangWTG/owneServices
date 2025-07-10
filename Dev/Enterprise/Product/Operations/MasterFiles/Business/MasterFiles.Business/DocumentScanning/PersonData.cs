using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PersonData),
	Enterprise.Core.Constants.DocManagerCodes.Person)]

namespace Enterprise.MasterFiles.Business
{
	class PersonData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(GlbPerson);

		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }

		protected override Type CollectionType => typeof(GlbPersonCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbPersonCollection(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbPerson; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("3BE68619-0311-4FEB-A647-909F80BDB816", "Person"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
