using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetDocManagerInfo : DocManagerInfo
	{
		public DtbConsignmentRunSheetDocManagerInfo(DtbConsignmentRunSheet runSheet)
			: base(runSheet, Constants.DocManagerCodes.DomesticTransportRunSheet)
		{
		}
	}
}
