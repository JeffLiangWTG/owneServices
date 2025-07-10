using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using AutoEvents = Enterprise.ZArchitecture.Business.AutoEvents;

namespace Enterprise.Warehouse.Transit.Business.Common
{
	public static class TransitWarehouseNoteHelper
	{
		public static NoteTypeCollection GetNoteTypes()
		{
			var noteTypeCollection = new NoteTypeCollection
			{
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails
			};

			return noteTypeCollection;
		}

		public static List<ZString> GetNoteTypesNotPopulate()
		{
			return new List<ZString>
			{
				PredefinedNoteTypes.Instance.CIN750MessageNotes.Description,
				PredefinedNoteTypes.Instance.CRESAMessageNotes.Description
			};
		}

		#region CIN750Note

		public static StmNote FindOrCreateCIN750StmNote(this IStmNoteParent noteParent)
		{
			return noteParent.FindOrCreateNonCustomStmNoteByDescription(PredefinedNoteTypes.Instance.CIN750MessageNotes.Description);
		}

		public static void PopulateCIN750MessageNote(this IStmNoteParent noteParent, ZString noteText, ZString messageID, ZString failedReason)
		{
			PopulateStmNoteByNoteTypeDescription(noteParent, PredefinedNoteTypes.Instance.CIN750MessageNotes.Description, noteText, messageID, failedReason);
		}

		public static void UpdateCIN750MessageStatusNote(this IStmNoteParent parent, string messageID, string failedReason = "")
		{
			var note = parent.FindOrCreateCIN750StmNote();
			var noteText = note.ST_NoteText;

			var lines = noteText.Split(System.Environment.NewLine);
			var oldMessageStatus = string.Empty;
			var newMessageStatus = string.Empty;
			foreach (var line in lines)
			{
				if (line.Contains(messageID))
				{
					oldMessageStatus = line;
					if (failedReason.IsNullOrEmpty())
					{
						newMessageStatus = Res.GetString("0caa2591-2e75-4ccc-b6f1-8aab5a9bd361", "Message Status: {0} has been sent successfully.", messageID);
					}
					else
					{
						newMessageStatus = Res.GetString("baa7ef55-52e3-43be-9765-9331b5dd1ffc", "Message Status: {0} has been rejected. Reason: {1}.", messageID, failedReason);
					}
					break;
				}
			}

			if (!oldMessageStatus.IsNullOrEmpty())
			{
				note.ST_NoteText = noteText.Replace(oldMessageStatus, newMessageStatus);
			}
		}

		#endregion

		#region CRESANote

		public static StmNote FindOrCreateCRESAStmNote(this IStmNoteParent noteParent)
		{
			return noteParent.FindOrCreateNonCustomStmNoteByDescription(PredefinedNoteTypes.Instance.CRESAMessageNotes.Description);
		}

		public static void PopulateCRESAMessageNote(this IStmNoteParent noteParent, ZString noteText, ZString failedReason)
		{
			PopulateStmNoteByNoteTypeDescription(noteParent, PredefinedNoteTypes.Instance.CRESAMessageNotes.Description, noteText, ZString.Empty, failedReason);
		}

		public static void UpdateCRESAMessageStatusNote(this IStmNoteParent noteParent, string eventCode, string reason = "")
		{
			var eventNote = ZString.Empty;
			switch (eventCode)
			{
				case AutoEvents.ClearanceStatusChangedCode:
					eventNote = Res.GetString("ea3fd57c-67a9-4d3e-9852-31e6c3f7009f", "SCM - Clearance Completed");
					break;
				case AutoEvents.MessageRejectedCode:
					eventNote = Res.GetString("2fddda6d-bb47-465a-bd50-c8e220c17734", "MRJ - Message Rejected");
					break;
				case AutoEvents.MessageAcceptedCode:
					eventNote = Res.GetString("ae6a8bf5-de3b-4e25-8795-f53f56baf3fb", "MAA - Message Accepted");
					break;
				default:
					break;
			}

			if (!eventNote.IsEmpty)
			{
				var noteBuilder = new ZStringBuilder();
				noteBuilder.Append(Res.GetString("706622d0-a81c-492d-8aad-a52a83fc1dee", "Time: "));
				noteBuilder.AppendLine(ZDateTimeOffset.Now.ToString());

				noteBuilder.Append(Res.GetString("f03f3089-6f97-4d0a-b2b1-ca50b9250c28", "Event Code: "));
				noteBuilder.AppendLine(eventNote);

				if (!reason.IsNullOrEmpty())
				{
					noteBuilder.Append(Res.GetString("f472e91c-5909-4b53-9100-562e52fa37f5", "Failed Reason: "));
					noteBuilder.AppendLine(reason);
				}

				noteBuilder.AppendLine();

				var note = noteParent.FindOrCreateCRESAStmNote();
				note.ST_NoteText = note.ST_NoteText + noteBuilder.ToString();
			}
		}

		#endregion

		#region Implementation

		static StmNote FindOrCreateNonCustomStmNoteByDescription(this IStmNoteParent noteParent, string noteTypeDescription)
		{
			var note = noteParent.Notes.FindByDescription(noteTypeDescription).FirstOrDefault();
			if (note == null)
			{
				note = noteParent.Notes.AddNew();
				using (note.GetValidationSuspender())
				{
					note.ST_Description = noteTypeDescription;
					note.ST_IsCustomDescription = false;
					note.ST_NoteText = ZString.Empty;
				}
			}

			return note;
		}

		static void PopulateStmNoteByNoteTypeDescription(IStmNoteParent noteParent, string noteTypeDescription, ZString noteText, ZString messageID, ZString failedReason)
		{
			var addedNoteText = GetAddedNoteText(noteText, messageID, failedReason);

			var note = noteParent.FindOrCreateNonCustomStmNoteByDescription(noteTypeDescription);
			note.ST_NoteText = note.ST_NoteText + addedNoteText;
			note.Factory.Save();
		}

		static string GetAddedNoteText(ZString noteText, ZString messageID, ZString failedReason)
		{
			var noteHeaderStringBuilder = new ZStringBuilder();
			noteHeaderStringBuilder.Append(Res.GetString("427372e1-ae1e-41bc-8c28-abe6b76348a5", "User: "));
			noteHeaderStringBuilder.AppendLine(GlbStaff.CurrentUser.GS_FullName);

			noteHeaderStringBuilder.Append(Res.GetString("11d8929a-d75f-4193-a4c9-562340c053a5", "Time: "));
			noteHeaderStringBuilder.AppendLine(ZDateTimeOffset.Now.ToString());

			if (!failedReason.IsEmpty)
			{
				noteHeaderStringBuilder.Append(Res.GetString("6e6cb9cc-3ad7-4062-99e8-af6c200a9d62", "Failed reason: "));
				noteHeaderStringBuilder.AppendLine(failedReason);
			}
			else
			{
				noteHeaderStringBuilder.Append(Res.GetString("b4147847-7090-48cb-ad9f-0a260b900dad", "Message Status: "));
				noteHeaderStringBuilder.AppendLine(Res.GetString("ed7887dd-130f-415e-a621-4ac3549dda7c", "{0} has been sent and is waiting for response.", messageID.IsEmpty ? Res.GetString("3837185f-8b57-4cab-ac26-2922748a19b9", "CRESA Message") : messageID));
			}

			var addedNoteText = noteHeaderStringBuilder.ToString() + noteText;
			return addedNoteText;
		}

		#endregion
	}
}
