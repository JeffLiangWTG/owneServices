using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgCommissionAgreementUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public OrgCommissionAgreementUniqueIndexFailureHandler(OrgCommissionAgreement commissionAgreement)
		{
			this.commissionAgreement = commissionAgreement;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return OrgCommissionAgreementSchema.Constants.Indexes.FK_UX__CA0_CA0_ParentVersion; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			notifier.ReportError(
				Res.GetString("56421aff-12ef-4aec-a02f-dbe42caa89a3", "Another user has already made changes to {0}. You must re-open this form and re-apply your changes to continue.", commissionAgreement.HumanReadableName),
				Res.GetString("242346ff-9ba7-46e1-8770-fc22778082cc", "Another user has changed {0}", commissionAgreement.HumanReadableName));
		}

		readonly OrgCommissionAgreement commissionAgreement;
	}
}
