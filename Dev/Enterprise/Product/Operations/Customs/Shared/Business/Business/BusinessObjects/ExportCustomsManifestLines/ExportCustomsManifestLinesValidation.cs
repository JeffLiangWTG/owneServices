//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportCustomsManifestLinesValidation
//
//    This class should be used for overriding validation in AutoExportCustomsManifestLinesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestLinesValidation : AutoExportCustomsManifestLinesValidation
	{
		public ExportCustomsManifestLinesValidation(AutoExportCustomsManifestLines parent) : base(parent)
		{
		}
	}
}
