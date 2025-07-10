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

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBAccountingInformationValidation : AutoExportAWBAccountingInformationValidation
	{
		public ExportAWBAccountingInformationValidation(AutoExportAWBAccountingInformation parent)
			: base(parent)
		{
		}

		public new ExportAWBAccountingInformation Parent
		{
			get { return (ExportAWBAccountingInformation)base.Parent; }
		}
	}
}
