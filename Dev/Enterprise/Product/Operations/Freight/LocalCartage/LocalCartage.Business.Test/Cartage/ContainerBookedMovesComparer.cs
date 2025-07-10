using System.Collections.Generic;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class ContainerBookedMovesComparer : IComparer<CommonBookedCtgMove>
	{
		public int Compare(CommonBookedCtgMove x, CommonBookedCtgMove y)
		{
			return x.Container.JC_ContainerJobID.CompareTo(y.Container.JC_ContainerJobID);
		}
	}
}
