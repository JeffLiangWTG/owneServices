using System;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(OnBoardingData),
	Enterprise.Core.Constants.DocManagerCodes.OnBoarding)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class OnBoardingData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HROnBoarding);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
	}
}
