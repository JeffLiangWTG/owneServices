using System;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Business
{
	[Serializable]
	public class RefComplianceListResponse
	{
		public bool RCL_IsActive { get; set; }
		public string RCL_ListCode { get; set; }
		public string RCL_ListName { get; set; }
		public string RCL_ListDescription { get; set; }
		public string RCL_ListPublisher { get; set; }
		public string RCL_ListType { get; set; }
		public string RCL_PublisherJurisdiction { get; set; }
		public string RCL_PublisherDescription { get; set; }
		public DateTime? RCL_IntegrationDate { get; set; }
		public string RCL_MainSourceURL { get; set; }
		public string RCL_SecondarySourceURL { get; set; }
		public DateTime RCL_LastUpdatedDate { get; set; }
	}
}
