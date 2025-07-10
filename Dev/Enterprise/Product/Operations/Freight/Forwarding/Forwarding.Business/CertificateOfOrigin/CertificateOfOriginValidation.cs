//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCertificateOfOriginValidation
//
//    This class should be used for overriding validation in AutoCertificateOfOriginValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Business
{
	public class CertificateOfOriginValidation : AutoCertificateOfOriginValidation
	{
		public CertificateOfOriginValidation(AutoCertificateOfOrigin parent) : base(parent)
		{
		}
	}
}
