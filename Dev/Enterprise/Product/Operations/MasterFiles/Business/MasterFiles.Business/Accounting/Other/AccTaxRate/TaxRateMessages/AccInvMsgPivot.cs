using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccInvMsgPivot : AutoAccInvMsgPivot, Integration.IAccInvMsgPivot
	{
		public AccInvMsgPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
