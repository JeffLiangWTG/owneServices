using System.Collections;

namespace Enterprise.Freight.Common.Business
{
	class LegTypesComparer : IComparer
	{
		int IComparer.Compare(object leg1, object leg2)
		{
			CommonCartageLegType l1 = (CommonCartageLegType)leg1;
			CommonCartageLegType l2 = (CommonCartageLegType)leg2;
			return l1.E4_DisplayOrder.CompareTo(l2.E4_DisplayOrder);
		}
	}
}
