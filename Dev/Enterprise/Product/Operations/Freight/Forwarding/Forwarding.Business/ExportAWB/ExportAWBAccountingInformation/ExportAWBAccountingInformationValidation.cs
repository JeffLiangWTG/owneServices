//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBAccountingInformationValidation
//
//    This class should be used for overriding validation in AutoExportAWBAccountingInformationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBAccountingInformationValidation : Forwarding.AWB.Business.ExportAWBAccountingInformationValidation
	{
		public ExportAWBAccountingInformationValidation(ExportAWBAccountingInformation parent)
			: base(parent)
		{
		}
	}
}
