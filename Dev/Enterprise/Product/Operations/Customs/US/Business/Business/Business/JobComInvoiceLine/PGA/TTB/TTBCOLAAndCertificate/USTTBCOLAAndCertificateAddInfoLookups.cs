//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSTTBCOLAAndCertificateAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSTTBCOLAAndCertificateAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USTTBCOLAAndCertificateAddInfoLookups : AutoUSTTBCOLAAndCertificateAddInfoLookups
	{
		public USTTBCOLAAndCertificateAddInfoLookups(AutoUSTTBCOLAAndCertificateAddInfo parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList COLAExemptionCodes
		{
			get { return TTBExemptionCodeList.GetListForCOLA(Factory, ProgramCode); }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		ZString ProgramCode
		{
			get
			{
				var parent = Parent.Parent;
				var ttbLine = parent.Parent;
				return ttbLine == null ? ZString.Empty : ttbLine.US_ProgramCode;
			}
		}

		protected new USTTBCOLAAndCertificateAddInfo Parent
		{
			get { return (USTTBCOLAAndCertificateAddInfo)base.Parent; }
		}
	}
}
