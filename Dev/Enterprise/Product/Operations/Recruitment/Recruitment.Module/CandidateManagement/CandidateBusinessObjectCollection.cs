using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public class CandidateBusinessObjectCollection : NonPersistentBusinessObjectCollection<Candidate>
	{
		public CandidateBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static ICollection<Candidate> Load(BusinessObjectFactory factory, ZQuery filter)
		{
			var list = new List<Candidate>();
			foreach (var application in factory.Load<HRJobApplication>(filter))
			{
				factory.AddFetchHint(HRJobApplicantSchema.PK, application.HP_HA);
				list.Add(new Candidate(application));
			}
			return list;
		}

		public override void Load(ZQuery additionalFilter)
		{
			using (SuspendListChanged())
			{
				var items = Load(Factory, GetCompleteLoadFilter(additionalFilter)).ToList();
				var matchingPks = new HashSet<ZGuid>(items.Select(i => i.PK));
				var noLongerMatches = this.Where(item => !matchingPks.Contains(item.PK)).ToList();
				RemoveRange(noLongerMatches);
				AddRange(items);
			}

			OnLoaded();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter strings")]
		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			if (property.Name == "Stage")
			{
				return new StageComparer(direction);
			}

			return base.GetComparerForSort(property, direction);
		}

		protected override BusinessObject AddNewCore() => throw new NotSupportedException();
		protected override BusinessObject AddNewCore(Type t) => throw new NotSupportedException();
		protected override BusinessObject CreateNonPersistentBusinessObject() => Candidate.CreateUncommittedRow(Factory);

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public class StageComparer : IComparer<HRTaskStage>, IComparer
		{
			public StageComparer(ListSortDirection direction)
			{
				this.direction = direction;
			}

			int Compare(HRTaskStage x, HRTaskStage y)
			{
				int result = x.CompareTo(y);
				return direction == ListSortDirection.Ascending ? result : -result;
			}

			int IComparer<HRTaskStage>.Compare(HRTaskStage x, HRTaskStage y)
				=> Compare(x, y);

			int IComparer.Compare(object x, object y)
				=> Compare(((Candidate)x).Stage, ((Candidate)y).Stage);

			readonly ListSortDirection direction;
		}
	}
}
