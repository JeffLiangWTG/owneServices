using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruitment.Common
{
	public sealed class CommunicationContactRowBusinessObjectCollection : NonPersistentBusinessObjectCollection<CommunicationContactRow>
	{
		public CommunicationContactRowBusinessObjectCollection(BusinessObjectFactory factory, Candidate candidate)
			: base(factory)
		{
			ParentCandidate = candidate;

			if (!ReadOnly)
			{
				Load();
			}
		}

		public readonly Candidate ParentCandidate;

		public override bool ReadOnly => ParentCandidate.IsNull;

		public override void Load()
		{
			using (SuspendListChanged())
			{
				var communicationContactRows = new List<CommunicationContactRow>();

				if (!string.IsNullOrEmpty(ParentCandidate.Applicant?.HA_EmailAddress))
				{
					communicationContactRows.Add(new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Candidate, ParentCandidate.Applicant.HA_EmailAddress)));
				}

				if (!string.IsNullOrEmpty(ParentCandidate.Application?.AssignedTo?.GS_EmailAddress))
				{
					communicationContactRows.Add(new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Recruiter, ParentCandidate.Application.AssignedTo)));
				}

				foreach (var conversation in ParentCandidate.EConversation?.Conversations)
				{
					foreach (var participant in conversation.Participants)
					{
						if (!string.IsNullOrEmpty(participant.EmailAddress))
						{
							var contact = new CommunicationContact(Factory);
							var position = (CodeDescriptionPair)contact.Positions.ToArray().Where(x => x.Description.Equals(participant.JCP_Relation)).SingleOrDefault() ?? CommunicationContactPositions.OtherPosition;
							communicationContactRows.Add(new CommunicationContactRow(new CommunicationContact(Factory, position, participant.EmailAddress)));
						}
					}
				}

				// This change is for Winzor
				// To avoid ambiguous calling between the System.Linq.Enumerable.DistinctBy and CargoWise.Common.IEnumerableExtensions.DistinctBy.
				// We want to use CargoWise.Common.IEnumerableExtensions.DistinctBy.
				AddRange(communicationContactRows.DistinctBy(c => c.Contact.Email));
			}
		}

		public bool AreAllRowsValid()
		{
			var allRowsValid = true;

			foreach (CommunicationContactRow row in this)
			{
				row.Contact.Validation.ValidateAll();

				if (row.Contact.HasErrors)
				{
					allRowsValid = false;
				}
			}

			return allRowsValid;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => CommunicationContactRow.CreateUncommittedRow(Factory);
	}
}
