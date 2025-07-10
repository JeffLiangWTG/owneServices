using System;
using Enterprise.Customs.US.InBond.Business;

namespace Enterprise.Customs.US.InBond.Module
{
	public class USInBondMoveHeaderOperationalActionSupporter : CusInBondHeaderOperationalActionSupporter
	{
		public override Type RootType => typeof(USInBondMoveHeader);
	}
}
