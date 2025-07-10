using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public static class CommonCartageLegExtensions
	{
		public static CommonCartageLeg[] OtherLegsInSameGroup(this CommonCartageLeg leg)
		{
			CommonCartageLeg[] runSheetLegs;

			if (leg.WorkSheet != null)
			{
				runSheetLegs = leg.WorkSheet.CartageLegs.Where(l => l.JU_RunSheetSequence == leg.JU_RunSheetSequence && l.PK != leg.PK).ToArray();
			}
			else
			{
				runSheetLegs = Array.Empty<CommonCartageLeg>();
			}

			return runSheetLegs;
		}
	}

	public class RunSheetSequenceHelper
	{
		public RunSheetSequenceHelper(CommonCartageLeg parentLeg)
		{
			if (parentLeg == null)
			{
				throw new ArgumentException("parentLeg can't be null.");
			}

			this.ParentLeg = parentLeg;
		}
		readonly CommonCartageLeg ParentLeg;

		public void GiveSequenceNumberToNewLegOnWorkSheet()
		{
			if (ParentLeg.WorkSheet == null)
			{
				throw new InvalidOperationException("RunSheet GUID is empty.");
			}

			CommonCartageLeg[] workSheetLegs = GetRunSheetLegs(ParentLeg.WorkSheet);
			ZInt lastLegSeq = workSheetLegs.Length > 0 ? workSheetLegs[workSheetLegs.Length - 1].JU_RunSheetSequence : ZInt.Zero;
			ParentLeg.JU_RunSheetSequence = lastLegSeq + 1;
		}

		public void RemoveSequenceNumberFromLegOnAWorkSheet()
		{
			if (ParentLeg.WorkSheet == null)
			{
				throw new InvalidOperationException("ParentLeg isn't allocated to a Work Sheet. Can't be removed.");
			}

			CommonCartageLeg[] groupedLegs = GetRunSheetLegs(ParentLeg.WorkSheet, ParentLeg.JU_RunSheetSequence);
			ParentLeg.JU_RunSheetSequence = 0;
			if (groupedLegs.Length == 1)
			{
				SetSequenceNumberBasedOnCurrentOrder(GetRunSheetLegs(ParentLeg.WorkSheet));
			}
		}

		public CommonCartageLeg[] MoveDown()
		{
			if (ParentLeg.WorkSheet == null)
			{
				throw new InvalidOperationException("ParentLeg isn't allocated to a Work Sheet. Can't be removed.");
			}

			ZInt seqToMoveDown = ParentLeg.JU_RunSheetSequence;
			CommonCartageLeg[] allLegs = GetRunSheetLegs(ParentLeg.WorkSheet);

			if (allLegs[allLegs.Length - 1].JU_RunSheetSequence != seqToMoveDown)
			{
				foreach (CommonCartageLeg leg in allLegs)
				{
					if (leg.JU_RunSheetSequence == seqToMoveDown)
					{
						leg.JU_RunSheetSequence = seqToMoveDown + 1;
					}
					else if (leg.JU_RunSheetSequence == seqToMoveDown + 1)
					{
						leg.JU_RunSheetSequence = seqToMoveDown;
					}
				}
			}

			return GetRunSheetLegs(ParentLeg.WorkSheet, ParentLeg.JU_RunSheetSequence);
		}

		public CommonCartageLeg[] MoveUp()
		{
			if (ParentLeg.WorkSheet == null)
			{
				throw new InvalidOperationException("ParentLeg isn't allocated to a Work Sheet. Can't be removed.");
			}

			ZInt seqToMoveUp = ParentLeg.JU_RunSheetSequence;
			CommonCartageLeg[] allLegs = GetRunSheetLegs(ParentLeg.WorkSheet);

			if (allLegs[0].JU_RunSheetSequence != seqToMoveUp)
			{
				foreach (CommonCartageLeg leg in allLegs)
				{
					if (leg.JU_RunSheetSequence == seqToMoveUp)
					{
						leg.JU_RunSheetSequence = seqToMoveUp - 1;
					}
					else if (leg.JU_RunSheetSequence == seqToMoveUp - 1)
					{
						leg.JU_RunSheetSequence = seqToMoveUp;
					}
				}
			}

			return GetRunSheetLegs(ParentLeg.WorkSheet, ParentLeg.JU_RunSheetSequence);
		}

		public void Group(CommonCartageLeg[] legsToGroup)
		{
			List<ZInt> sequencesToGroup = new List<ZInt>();
			foreach (CommonCartageLeg leg in legsToGroup)
			{
				sequencesToGroup.Add(leg.JU_RunSheetSequence);
			}

			CommonCartageLeg[] allLegsToGroup = GetRunSheetLegs(legsToGroup[0].WorkSheet, sequencesToGroup);
			int min = MinRunSheetSequence(allLegsToGroup);
			foreach (CommonCartageLeg leg in allLegsToGroup)
			{
				if (leg != min)
				{
					leg.JU_RunSheetSequence = min;
				}
			}

			CommonCartageLeg[] legsInOrder = GetRunSheetLegs(legsToGroup[0].WorkSheet);
			SetSequenceNumberBasedOnCurrentOrder(legsInOrder);
		}

		public string Split(CommonCartageLeg[] legsToSplit)
		{
			//All legs need to have the same sequence number
			int groupSequence = legsToSplit[0].JU_RunSheetSequence;
			foreach (CommonCartageLeg leg in legsToSplit)
			{
				if (leg.JU_RunSheetSequence != groupSequence)
				{
					return Res.GetString("629fce86-d3ca-47ca-8ad6-6d9af337b8fg", "Selected legs should all have the same sequence number.");
				}
			}

			CommonCartageLeg[] allLegsToGroup = GetRunSheetLegs(legsToSplit[0].WorkSheet, groupSequence);

			int add = 0;
			int includeFirst = 0;
			if (allLegsToGroup.Length > legsToSplit.Length)
			{
				add = legsToSplit.Length;
				includeFirst = 1;
			}
			else
			{
				add = legsToSplit.Length - 1;
			}

			CommonCartageLeg[] allLegs = GetRunSheetLegs(legsToSplit[0].WorkSheet);
			foreach (CommonCartageLeg leg in allLegs)
			{
				if (leg.JU_RunSheetSequence > groupSequence)
				{
					leg.JU_RunSheetSequence = leg.JU_RunSheetSequence + add;
				}
			}

			for (int i = 0; i < legsToSplit.Length; i++)
			{
				legsToSplit[i].JU_RunSheetSequence = legsToSplit[i].JU_RunSheetSequence + i + includeFirst;
			}

			return null;
		}

		int MinRunSheetSequence(CommonCartageLeg[] legs)
		{
			int min = legs[0].JU_RunSheetSequence;
			foreach (CommonCartageLeg leg in legs)
			{
				if (leg.JU_RunSheetSequence < min)
				{
					min = leg.JU_RunSheetSequence;
				}
			}
			return min;
		}

		static CommonCartageLeg[] GetRunSheetLegs(CommonWorkSheet runSheet)
		{
			return GetRunSheetLegs(runSheet, new List<ZInt>());
		}

		static CommonCartageLeg[] GetRunSheetLegs(CommonWorkSheet runSheet, ZInt groupSequence)
		{
			return GetRunSheetLegs(runSheet, new List<ZInt>(new ZInt[] { groupSequence }));
		}

		static CommonCartageLeg[] GetRunSheetLegs(CommonWorkSheet runSheet, List<ZInt> groupSequences)
		{
			return GetRunSheetLegs(runSheet, groupSequences, JobContainerLegsSchema.JU_RunSheetSequence);
		}

		public static CommonCartageLeg[] GetRunSheetLegs(CommonWorkSheet runSheet, SchemaColumn orderBy)
		{
			return GetRunSheetLegs(runSheet, new List<ZInt>(), orderBy);
		}

		static CommonCartageLeg[] GetRunSheetLegs(CommonWorkSheet runSheet, List<ZInt> groupSequences, SchemaColumn orderBy)
		{
			CommonCartageLeg[] runSheetLegs;

			if (runSheet != null)
			{
				runSheetLegs = runSheet.CartageLegs.Where(l => groupSequences.Count == 0 || groupSequences.Contains(l.JU_RunSheetSequence)).ToArray();
				Array.Sort(runSheetLegs, (l1, l2) => ((IZType)l1[orderBy.Name]).CompareTo(l2[orderBy.Name]));
			}
			else
			{
				runSheetLegs = Array.Empty<CommonCartageLeg>();
			}

			return runSheetLegs;
		}

		public static void SetSequenceNumberBasedOnCurrentOrder(CommonCartageLeg[] orderedWorkSheetLegs)
		{
			ZInt seq = 1;
			Dictionary<ZInt, ZInt> groupSeq = new Dictionary<ZInt, ZInt>();
			//Always keep groups together

			foreach (CommonCartageLeg leg in orderedWorkSheetLegs)
			{
				if (leg.JU_RunSheetSequence != 0)
				{
					ZInt value;
					if (groupSeq.TryGetValue(leg.JU_RunSheetSequence, out value))
					{
						leg.JU_RunSheetSequence = value;
					}
					else
					{
						groupSeq.Add(leg.JU_RunSheetSequence, seq);
						leg.JU_RunSheetSequence = seq;
						seq++;
					}
				}
			}
		}
	}
}
