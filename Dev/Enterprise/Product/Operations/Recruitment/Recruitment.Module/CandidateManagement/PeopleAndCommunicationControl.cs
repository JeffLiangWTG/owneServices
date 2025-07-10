using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.EConversation.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public partial class PeopleAndCommunicationControl : ZUserControl
	{
		public PeopleAndCommunicationControl()
		{
			InitializeComponent();
			permittedToContactReferencesCheckBox.CheckStateChanged += permittedToContactReferencesCheckBoxChecked;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void permittedToContactReferencesCheckBoxChecked(object sender, EventArgs e)
		{
			var candidate = DataSource as Candidate;
			if (candidate != null && permittedToContactReferencesCheckBox.Checked)
			{
				candidate.Application.Logs.AddNew(ZArchitecture.Business.AutoEvents.Authorised, new KeyValuePair<string, string>(nameof(EventLoggingKeys.Info), Res.GetString("acd59f4f-0deb-4d61-89bc-74011098fc5f", "Permission to contact references granted")));
			}
			else
			{
				candidate.Application.Logs.AddNew(ZArchitecture.Business.AutoEvents.AuthorisationWithdrawn, new KeyValuePair<string, string>(nameof(EventLoggingKeys.Info), Res.GetString("bad549d7-2391-4245-8e1b-ff3baf11fceb", "Permission to contact references revoked")));
			}
		}

		void OpenInMailClientButton_Click(object sender, EventArgs e)
		{
			var conversation = GenerateEConversation(DataSource as Candidate, permittedToContactReferencesCheckBox.Checked);

			if (conversation != null)
			{
				WebUrlLauncher.Launch(EmailBizoEncoder.GetMailToForBizo(conversation, RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value));
			}
		}

		public static JobConversation GenerateEConversation(Candidate candidate, bool isPermittedToContactReferences)
		{
			if (string.IsNullOrEmpty(RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value))
			{
				ShowErrorMessage(Res.GetString("08b614fc-e052-4fe3-b5c8-7125cfcaded9", "'{0}' must be set in the registry.", RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.GetLocation()));

				return null;
			}

			if (!candidate.CommunicationContactRows.AreAllRowsValid())
			{
				ShowErrorMessage(Res.GetString("7fd738ef-b87b-4f27-b555-fb15b383559e", "There are unresolved errors in the grid."));

				return null;
			}

			var selectedContacts = candidate.CommunicationContactRows.Cast<CommunicationContactRow>().Where(c => c.Selected);

			if (!isPermittedToContactReferences && selectedContacts.Any(c => IsContactAReference(c)))
			{
				ShowErrorMessage(Res.GetString("1f4fcdcc-2709-4150-8c26-b42f79a05633", "You have not indicated permission to contact references."));

				return null;
			}

			if (!selectedContacts.Any())
			{
				return null;
			}

			var conversation = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);

			// This change is for Winzor
			// To avoid ambiguous calling between the System.Linq.Enumerable.DistinctBy and CargoWise.Common.IEnumerableExtensions.DistinctBy.
			// We want to use CargoWise.Common.IEnumerableExtensions.DistinctBy.
			foreach (var row in IEnumerableExtensions.DistinctBy(selectedContacts, c => c.Contact.Email))
			{
				var glbStaff = row.Contact.AsGlbStaff;
				var participant = (glbStaff == null)
					? conversation.Participants.AddNewParticipant(row.Contact.Email)
					: conversation.Participants.AddNewParticipant(glbStaff);
				participant.JCP_Relation = row.Contact.Position;
			}

			candidate.Factory.Save();

			return conversation;
		}

		public static void ShowErrorMessage(string errorMessage)
			=> Globals.Message.ShowError(errorMessage, Res.GetString("ba5772b4-67b8-47e7-bf2b-2e4513fb3b2c", "Cannot create eConversation"));

		public static bool IsContactAReference(CommunicationContactRow row) => row.Contact.Position == CommunicationContactPositions.Reference.Description;
	}
}
