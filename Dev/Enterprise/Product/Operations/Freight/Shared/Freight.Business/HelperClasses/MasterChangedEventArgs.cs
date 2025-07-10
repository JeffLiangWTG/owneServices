using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class MasterChangedEventArgs : EventArgs
	{
		public MasterChangedEventArgs(ZGuid oldMasterPK, ZGuid newMasterPK)
			: base()
		{
			this.OldMasterPK = oldMasterPK;
			this.NewMasterPK = newMasterPK;
		}

		public ZGuid OldMasterPK { get; private set; }
		public ZGuid NewMasterPK { get; private set; }
	}
}
