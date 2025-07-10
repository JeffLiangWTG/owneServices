//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBSecurityStatusLineValidation
//
//    This class should be used for overriding validation in AutoExportAWBSecurityStatusLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBSecurityStatusLineValidation : AutoExportAWBSecurityStatusLineValidation
	{
		public ExportAWBSecurityStatusLineValidation(AutoExportAWBSecurityStatusLine parent)
			: base(parent)
		{
		}
	}
}
