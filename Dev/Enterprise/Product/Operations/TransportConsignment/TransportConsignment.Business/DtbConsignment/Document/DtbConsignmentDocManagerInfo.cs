using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentDocManagerInfo : DocManagerInfo
	{
		public DtbConsignmentDocManagerInfo(DtbConsignment consignment)
			: base(consignment, Constants.DocManagerCodes.LandTransportConsignment)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var actions = ((DtbConsignment)BusinessEntity).AllActions;
			return base.GetRelatedObjects().Concat(actions).ToArray();
		}
	}
}
