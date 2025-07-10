using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class BulkCommissionAgreementConflictEmailCreator
	{
		public BulkCommissionAgreementConflictEmailCreator(BusinessObjectFactory factory, IEnumerable<ICommissionAgreementConflict> agreementConflicts)
		{
			this.factory = factory;
			this.agreementConflicts = agreementConflicts;
		}

		readonly BusinessObjectFactory factory;
		readonly IEnumerable<ICommissionAgreementConflict> agreementConflicts;

		public ZString GetNotificationPromptMessage()
		{
			var isSendToAllRecipients = OrganisationRegistry.Instance.SendCommissionAgreementConflictEmailToAllRecipients.Value;
			var isSendToNotificationGroup = OrganisationRegistry.Instance.CommissionAgreementConflictNotificationGroup.Value != Guid.Empty;

			var notificationRecipients =
				(isSendToAllRecipients && isSendToNotificationGroup) ? Res.GetString("e30dee87-4206-4afc-aa94-54c73cd8794a", "a notification group and all members of the existing commission agreement wolf packs") :
				(isSendToAllRecipients && !isSendToNotificationGroup) ? Res.GetString("24c2766e-6172-4159-9a7d-7d69d8fff6ea", "all members of the existing commission agreement wolf packs") :
				(!isSendToAllRecipients && isSendToNotificationGroup) ? Res.GetString("b4b33df9-c433-468b-9b26-d1d7e3eb51f1", "a notification group") :
				string.Empty;

			if (string.IsNullOrEmpty(notificationRecipients))
			{
				return ZString.Empty;
			}

			return Res.GetString("c6426f8c-ef29-4a0b-b3ac-e09450cf24d3", @"You have added / modified commission agreement(s) which results in taking a subset commission from the following existing commission agreements:

{0}


An email will be sent to {1}. Are you sure you want to continue?",
					CommissionAgreementConflictsTextProvider.New().ToDisplayListGroupedByAgreement(agreementConflicts, true), notificationRecipients);
		}

		public void CreateAndSave()
		{
			var conflictsGroupedByCommissionAgreement =
				from conflict in agreementConflicts
				group conflict by new { conflict.LoserCommissionAgreement, conflict.WinnerCommissionAgreement } into grp
				select grp;

			foreach (var conflictGroup in conflictsGroupedByCommissionAgreement)
			{
				var loserCommissionAgreement = conflictGroup.Key.LoserCommissionAgreement;
				var winnerCommissionAgreement = conflictGroup.Key.WinnerCommissionAgreement;
				var emailCreator = new CommissionAgreementConflictEmailCreator(factory, winnerCommissionAgreement, loserCommissionAgreement, conflictGroup);
				emailCreator.CreateAndSave();
			}
		}
	}
}
