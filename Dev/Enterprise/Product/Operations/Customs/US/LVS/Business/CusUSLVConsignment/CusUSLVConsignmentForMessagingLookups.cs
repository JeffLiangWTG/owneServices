using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVConsignmentForMessagingLookups : ZLookups
	{
		public CusUSLVConsignmentForMessagingLookups(CusUSLVConsignmentForMessaging parent)
			: base(parent)
		{
		}

		public ReasonCodeList ReasonCodeList => Factory.GetCachedValue<ReasonCodeList>();
	}
}
