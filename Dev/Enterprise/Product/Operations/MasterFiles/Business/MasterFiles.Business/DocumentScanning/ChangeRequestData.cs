using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(ChangeRequestData), Enterprise.Core.Constants.DocManagerCodes.ChangeRequest)]

namespace Enterprise.MasterFiles.Business
{
	class ChangeRequestData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(GlbStaffChangeRequest);

		public override string ReferenceType => Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;

		protected override Type CollectionType => null;
	}
}
