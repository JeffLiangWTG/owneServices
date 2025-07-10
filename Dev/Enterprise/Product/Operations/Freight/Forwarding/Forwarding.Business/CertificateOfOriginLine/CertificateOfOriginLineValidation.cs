//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCertificateOfOriginLineValidation
//
//    This class should be used for overriding validation in AutoCertificateOfOriginLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Business
{
	public class CertificateOfOriginLineValidation : AutoCertificateOfOriginLineValidation
	{
		public CertificateOfOriginLineValidation(AutoCertificateOfOriginLine parent) : base(parent)
		{
		}
	}
}
