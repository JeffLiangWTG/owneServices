using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public interface IOrgMatchApprovalModule : IDisposable
	{
		bool IsUnmatchForCurrentUserOnly { get; }
		void RevertFilterToUnmatchedByCurrentUser();
		bool RevertFilterToUnmatchedByCurrentUserAfterWarningUser();
		void PerformSearch();
		OrgMatchApprovalForm ShowEditForm(OrgMatchApproval selectedBusinessObject);
		OrgMatchApprovalCollection GridCollection { get; }
	}
}
