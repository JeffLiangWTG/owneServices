using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteRead : AutoGlbReleaseNoteRead
	{
		public GlbReleaseNoteRead(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnFactorySaving()
		{
			if (!IsInDatabase && !IsDeleted)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbReleaseNoteRead));
				query.AddToFilter(GlbReleaseNoteReadSchema.GR_GS_Staff, GR_GS_Staff);
				query.AddToFilter(GlbReleaseNoteReadSchema.GR_ReleaseNoteID, GR_ReleaseNoteID);

				if (Factory.LoadTop1<GlbReleaseNoteRead>(query) != null)
				{
					Delete();
				}
			}
			base.OnFactorySaving();
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new GlbReleaseNoteReadUniqueIndexFailureHandler(this); }
		}

		internal class GlbReleaseNoteReadUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public GlbReleaseNoteReadUniqueIndexFailureHandler(GlbReleaseNoteRead parent)
			{
				this.Parent = parent;
			}

			readonly GlbReleaseNoteRead Parent;

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				Parent.Delete();
			}

			public IEnumerable<string> HandledUniqueIndexNames => HandledUniqueIndexNamesCore;

			protected virtual IEnumerable<string> HandledUniqueIndexNamesCore
			{
				get { yield return GlbReleaseNoteReadSchema.Constants.Indexes.NR_UC__GR_GS_Staff_GR_ReleaseNoteID; }
			}
		}
	}
}
