using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobApplicationData),
	Enterprise.Core.Constants.DocManagerCodes.JobApplication)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class JobApplicationData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(HRJobApplication);
		protected override Type CollectionType => typeof(HRJobApplicationCollection);
		public override ModuleIdentifier ModuleID => ModuleIDs.HRJobApplication;
		public override string ReferenceType => Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("b00316d8-4a82-4567-8cd9-bdd66898b10f", "Job Application");
		public override bool IsAllowedForUnallocatedeDocs => true;
		public override ZString GetFriendlyName(BusinessObject businessObject)
		{
			var jobApplication = (HRJobApplication)businessObject;
			var applicationName = (jobApplication.JobOpening != null) ? jobApplication.JobOpening.HV_AdTitle.ToString() : (NoResString)"No Valid Campaign";
			return $"{applicationName} {jobApplication.HP_ApplicationNumber}";
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new HRJobApplicationCollection(factory);
		}
	}
}
