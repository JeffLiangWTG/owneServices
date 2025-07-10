//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportCustomsManifestLinesLookups
//
//    This class should be used for overriding collections in AutoExportCustomsManifestLinesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestLinesLookups : AutoExportCustomsManifestLinesLookups
	{
		public ExportCustomsManifestLinesLookups(AutoExportCustomsManifestLines parent) : base(parent)
		{
		}

		public virtual ICodeDescriptionPairList TypeOfCANs
		{
			get
			{
				if (fTypeOfCANs == null)
				{
					fTypeOfCANs = new CodeDescriptionPairList();
				}
				return fTypeOfCANs;
			}
		}
		ICodeDescriptionPairList fTypeOfCANs;

		public virtual ICodeDescriptionPairList ValidExemptionCodes
		{
			get
			{
				if (fValidExemptionCodes == null)
				{
					fValidExemptionCodes = new CodeDescriptionPairList();
				}
				return fValidExemptionCodes;
			}
		}
		ICodeDescriptionPairList fValidExemptionCodes;
	}
}
