
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbCompanyCampaign : IBusiness
	{
		ZString CampaignID { get; }
		ZPropertyInfo CampaignIDInfo { get; }

		ZString G0_CampaignName { get; set; }
		ZString G0_CampaignID { get; set; }
		ZPropertyInfo G0_CampaignNameInfo { get; }

		ZString G0_BroadcastVoteSurveyExam { get; set; }
		ZPropertyInfo G0_BroadcastVoteSurveyExamInfo { get; }
		ZBool G0_IsSalesAndMarketing { get; set; }

		ZString G0_Stage { get; set; }
		ZPropertyInfo G0_StageInfo { get; }

		ZString G0_Type { get; set; }
		ZPropertyInfo G0_TypeInfo { get; }

		ZString G0_Category { get; set; }
		ZPropertyInfo G0_CategoryInfo { get; }

		ZDateTime G0_EstimatedStartedDate { get; set; }
		ZPropertyInfo G0_EstimatedStartedDateInfo { get; }

		ZDateTime G0_EstimatedCompletedDate { get; set; }
		ZPropertyInfo G0_EstimatedCompletedDateInfo { get; }

		ZDateTime G0_ActualStartedDate { get; set; }
		ZPropertyInfo G0_ActualStartedDateInfo { get; }

		ZDateTime G0_ActualCompletedDate { get; set; }
		ZPropertyInfo G0_ActualCompletedDateInfo { get; }

		ZString G0_GS_NKCampaignManager { get; set; }
		ZPropertyInfo G0_GS_NKCampaignManagerInfo { get; }

		ZString G0_GS_NKCampaignCoordinator { get; set; }
		ZPropertyInfo G0_GS_NKCampaignCoordinatorInfo { get; }

		ZString G0_EmailSubject { get; set; }
		ZPropertyInfo G0_EmailSubjectInfo { get; }

		ZGuid G0_G0_Master { get; set; }

		ZBlob HtmlDocumentBlob { get; set; }
		ZWrappedPropertyInfo HtmlDocumentBlobInfo { get; }

		ZDateTime G0_SystemCreateTimeUtc { get; set; }
		ZString G0_SystemCreateUser { get; set; }
		ZString G0_SystemCreateBranch { get; set; }
		ZString G0_SystemCreateDepartment { get; set; }
		ZDateTime G0_SystemLastEditTimeUtc { get; set; }
		ZString G0_SystemLastEditUser { get; set; }

		ZGuid PK { get; }

		ISalesRelationModel SalesRelationModel { get; }

		bool IsTargetList { get; }

		void SetDocumentData(ZBlob htmlDocument);
	}
}
