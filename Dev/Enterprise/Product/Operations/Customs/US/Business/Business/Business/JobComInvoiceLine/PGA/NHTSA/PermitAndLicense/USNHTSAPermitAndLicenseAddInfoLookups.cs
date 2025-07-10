//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSAPermitAndLicenseAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSNHTSAPermitAndLicenseAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USNHTSAPermitAndLicenseAddInfoLookups : AutoUSNHTSAPermitAndLicenseAddInfoLookups
	{
		public USNHTSAPermitAndLicenseAddInfoLookups(AutoUSNHTSAPermitAndLicenseAddInfo parent) : base(parent)
		{
		}

		public NHTSALPCOTypeList LPCOTypes
		{
			get { return Factory.GetCachedValue<NHTSALPCOTypeList>(); }
		}

		public LPCODateQualifierList LPCODateTypes
		{
			get
			{
				return Factory.GetCachedValue("NHTSALPCODateTypes", delegate
			{
				var result = new LPCODateQualifierList();
				result.Sort();
				return result;
			});
			}
		}
	}
}
