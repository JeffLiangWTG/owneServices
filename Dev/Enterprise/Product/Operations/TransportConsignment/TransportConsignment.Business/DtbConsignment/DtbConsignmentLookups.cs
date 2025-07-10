using Enterprise.TransportCommon.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLookups : AutoDtbConsignmentLookups
	{
		public DtbConsignmentLookups(AutoDtbConsignment parent) : base(parent)
		{
		}

		#region BindToLists

		public TransportBindToLists BindToLists
		{
			get { return Factory.GetCachedValue("DtbConsignmentLookups|BindToLists", () => new TransportBindToLists(Factory)); }
		}

		#endregion
	}
}
