using System.Collections;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecuritySortIndexCalculator : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			GlbSecurity security1 = (GlbSecurity)x;
			GlbSecurity security2 = (GlbSecurity)y;

			if (CalculateSortIndex(security1) == 1 && CalculateSortIndex(security2) != 1)
			{
				return -1;
			}
			else if (CalculateSortIndex(security2) == 1 && CalculateSortIndex(security1) != 1)
			{
				return 1;
			}

			if (security1.Group.GG_Code.CompareTo(security2.Group.GG_Code) == 0)
			{
				return CalculateSortIndex(security1).CompareTo(CalculateSortIndex(security2));
			}
			else
			{
				return security1.Group.GG_Code.CompareTo(security2.Group.GG_Code);
			}
		}

		public static int CalculateSortIndex(GlbSecurity security)
		{
			int sortIndex = 0;
			if (security.GU_GE.IsEmpty && security.GU_GB.IsEmpty && security.GU_GC.IsEmpty)
			{
				sortIndex = 1;
			}
			else if (security.GU_GE.IsEmpty && security.GU_GB.IsEmpty && !security.GU_GC.IsEmpty)
			{
				sortIndex = 2;
			}
			else if (security.GU_GE.IsEmpty && !security.GU_GB.IsEmpty && security.GU_GC.IsEmpty)
			{
				sortIndex = 3;
			}
			else if (!security.GU_GE.IsEmpty && security.GU_GB.IsEmpty && !security.GU_GC.IsEmpty)
			{
				sortIndex = 4;
			}
			else if (!security.GU_GE.IsEmpty && !security.GU_GB.IsEmpty && security.GU_GC.IsEmpty)
			{
				sortIndex = 5;
			}
			return sortIndex;
		}

		#endregion
	}
}
