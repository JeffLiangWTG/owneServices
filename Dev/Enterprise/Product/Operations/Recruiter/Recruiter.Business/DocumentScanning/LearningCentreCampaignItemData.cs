using System;
using Enterprise.Recruiter.Business.DocumentScanning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(LearningCentreCampaignItemData),
	Enterprise.Core.Constants.DocManagerCodes.LearningCentreCampaignItem)]

namespace Enterprise.Recruiter.Business.DocumentScanning
{
	public class LearningCentreCampaignItemData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(LearningCentreCampaignItem); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("63f3e7d8-28ee-4e29-9db4-bec6d59c06cd", "Learning Center Campaign Item"); } }
	}
}
