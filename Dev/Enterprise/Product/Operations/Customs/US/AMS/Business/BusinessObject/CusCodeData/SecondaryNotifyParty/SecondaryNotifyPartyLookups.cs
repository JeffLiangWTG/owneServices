using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class SecondaryNotifyPartyLookups : Customs.Business.CusCodeDataLookups
	{
		public SecondaryNotifyPartyLookups(SecondaryNotifyParty parent)
			: base(parent)
		{
		}

		public USCCarrierAndFIRMSCollection SCACOrFIRMSList
		{
			get { return new USCCarrierAndFIRMSCollection(Factory); }
		}
	}
}
