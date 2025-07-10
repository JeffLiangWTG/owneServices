using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[DependentBusinessObject(typeof(GlbAccreditation), "Groups")]
	public class GlbAccreditationJobSkillGroup : AutoGlbAccreditationJobSkillGroup, IGlbAccreditationJobSkillGroup
	{
		public GlbAccreditationJobSkillGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HJG_Threshold = 1;
		}

		protected override ZString HumanReadableNameCore { get { return Res.GetString("6BCC4A26-7BC2-4D24-BD67-8AA1CDC81133", "Skill Group").Trim(); } }

		public GlbAccreditation ParentAccreditation
		{
			get
			{
				return HJG_ParentTableCode == GlbAccreditationSchema.Constants.Prefix
					? Factory.Load<GlbAccreditation>(HJG_ParentID)
					: null;
			}
		}

		public GlbAccreditationJobSkillGroup ParentGroup
		{
			get
			{
				return HJG_ParentTableCode == GlbAccreditationJobSkillGroupSchema.Constants.Prefix
					? Factory.Load<GlbAccreditationJobSkillGroup>(HJG_ParentID)
					: null;
			}
		}

		GlbAccreditationJobSkillGroupCollection groups;
		[ChildEditable]
		public GlbAccreditationJobSkillGroupCollection Groups
		{
			get
			{
				if (groups == null)
				{
					groups = new GlbAccreditationJobSkillGroupCollection(this);
					groups.Load();
					RegisterEditableChildObject(groups);
				}

				return groups;
			}
		}

		IGlbAccreditationJobSkillGroupCollection IGlbAccreditationJobSkillGroup.Groups
		{
			get { return Groups; }
		}

		GlbAccreditationJobSkillPivotCollection skillPivots;

		[ChildEditable]
		public GlbAccreditationJobSkillPivotCollection SkillPivots
		{
			get
			{
				if (skillPivots == null)
				{
					skillPivots = new GlbAccreditationJobSkillPivotCollection(this);
					skillPivots.Load();
					RegisterEditableChildObject(skillPivots);
				}

				return skillPivots;
			}
		}

		IGlbAccreditationJobSkillPivotCollection IGlbAccreditationJobSkillGroup.SkillPivots
		{
			get { return SkillPivots; }
		}

		public override void Delete()
		{
			base.Delete();
			SkillPivots.RemoveAndDeleteAll();
			Groups.RemoveAndDeleteAll();
		}

		readonly Dictionary<ZGuid, Dictionary<ZGuid, ZDateTime>> cache = new Dictionary<ZGuid, Dictionary<ZGuid, ZDateTime>>();

		public ZDateTime GetCompletedDateForPerson(IGlbPerson person, IGlbAccreditationAttempt attempt, bool useCache)
		{
			Dictionary<ZGuid, ZDateTime> forAttempt = null;
			if (useCache)
			{
				if (cache.TryGetValue(person.PK, out forAttempt))
				{
					var inner = cache[person.PK];
					ZDateTime value;
					if (inner.TryGetValue(attempt.PK, out value))
					{
						return value;
					}
				}
				else
				{
					forAttempt = new Dictionary<ZGuid, ZDateTime>();
					cache.Add(person.PK, forAttempt);
				}
			}

			var completionDates = new List<ZDateTime>();

			foreach (GlbAccreditationJobSkillGroup @group in Groups)
			{
				var date = @group.GetCompletedDateForPerson(person, attempt, useCache);
				if (!date.IsEmpty)
				{
					completionDates.Add(date);
				}
			}

			ZDateTime result = ZDateTime.Empty;

			if (completionDates.Count >= HJG_Threshold)
			{
				completionDates.Sort();
				result = HJG_Threshold > 0 ? completionDates[HJG_Threshold - 1] : completionDates[0];
			}

			if (!result.IsEmpty)
			{
				forAttempt?.Add(attempt.PK, result);
			}

			return result;
		}

		public ZDecimal GetAverageWeightedScoreForPerson(IGlbPerson person, ZDate commencementDate, ZDate completionDueDate)
		{
			var weightedScores = new List<Tuple<ZDecimal, ZShort>>();

			foreach (var @group in Groups.Cast<GlbAccreditationJobSkillGroup>())
			{
				var score = @group.GetAverageWeightedScoreForPerson(person, commencementDate, completionDueDate);
				weightedScores.Add(Tuple.Create(score, @group.HJG_Threshold));
			}

			var scoreSortedScores = weightedScores.OrderByDescending(s => s).ToList();
			var topScores = scoreSortedScores.Take(HJG_Threshold);
			decimal totalWeightedScore = 0;
			int totalWeight = 0;

			foreach (var scoreTuple in topScores)
			{
				totalWeightedScore += scoreTuple.Item1 * scoreTuple.Item2;
				totalWeight += scoreTuple.Item2;
			}

			return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
		}

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			HJG_ParentID = accred.PK;
			HJG_ParentTableCode = GlbAccreditationSchema.Constants.Prefix;
		}

#endif
		#endregion

		#region IGlbAccreditationTreeNodeItem

		bool IGlbAccreditationJobSkillGroup.IsDeleted
		{
			get { return base.IsDeleted; }
		}

		#endregion

		internal void ClearCache()
		{
			cache.Clear();
		}
	}
}
