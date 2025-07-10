using System;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HiringRequestData),
	Enterprise.Core.Constants.DocManagerCodes.HiringRequest)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class HiringRequestData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HRHiringRequest);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
	}
}
