using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveDetailProcessHandlingInfo : ProcessHandlingInfo
	{
		public CusInBondMoveDetailProcessHandlingInfo(CusInBondMoveDetail detail)
			: base(detail)
		{ }

		CusInBondMoveDetail MoveDetail
		{
			get { return (CusInBondMoveDetail)base.LogParent; }
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return CusInBondMoveHeaderProcessHandlingInfo.PopulateCascadingTargets(MoveDetail.Header, logBeingAdded);
		}
	}
}
