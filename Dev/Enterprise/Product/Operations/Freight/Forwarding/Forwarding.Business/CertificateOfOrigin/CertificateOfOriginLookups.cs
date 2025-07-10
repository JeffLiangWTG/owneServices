//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCertificateOfOriginLookups
//
//    This class should be used for overriding collections in AutoCertificateOfOriginLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Business
{
	public class CertificateOfOriginLookups : AutoCertificateOfOriginLookups
	{
		public CertificateOfOriginLookups(AutoCertificateOfOrigin parent) : base(parent)
		{
		}
	}
}
