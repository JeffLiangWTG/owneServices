using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class WebUserVisibleNotes
	{
		public WebUserVisibleNotes(IWebUserVisibleNotesSupport parent)
		{
			this.Parent = parent;
			Parent.NotesParentBO.Notes.ShowNotesForAllCompanies = true;
		}

		readonly IWebUserVisibleNotesSupport Parent;

		public IReadOnlyList<StmNote> VisibleNotes
		{
			get
			{
				var notes = new List<StmNote>();

				var siteUser = WebEnv.AppInstance.SiteUser;
				if (siteUser.IsLoggedIn)
				{
					var showAgentNotes = Parent.ShowAgentNotes && !((TrackingSiteUser)siteUser).IsShipmentQuickViewUser;
					notes.AddRange(Parent.NotesParentBO.Notes.VisibleNotes.Cast<StmNote>().Where(n => n.ST_NoteType == nameof(StmNoteVisibility.PUB) || (showAgentNotes && n.ST_NoteType == nameof(StmNoteVisibility.AGV))));
				}

				return notes.OrderBy(note => note.ST_CreatedDateUtc).ToArray();
			}
		}

		public ZString Top3JobNotes
		{
			get
			{
				var jobNotes = VisibleNotes.Where(x => x.ST_Description == PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description).Select(x => x.ST_NoteDataAsTextConcatenatedAndTrimmed);

				if (jobNotes.Count() > 3)
				{
					jobNotes = jobNotes.Take(3).Append("...");
				}

				var builder = new ZStringBuilder(jobNotes);

				return builder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
			}
		}
	}
}
