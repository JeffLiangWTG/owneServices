//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCPSCLabReportAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSCPSCLabReportAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCPSCLabReportAddInfoValidation : AutoUSCPSCLabReportAddInfoValidation
	{
		public USCPSCLabReportAddInfoValidation(AutoUSCPSCLabReportAddInfo parent) : base(parent)
		{
		}

		CPSCReport Report
		{
			get { return (CPSCReport)Parent.Parent; }
		}

		protected override void CheckUS_RemarksText()
		{
			base.CheckUS_RemarksText();
			if (Parent != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RemarksTextInfo);
			}
		}

		protected override void CheckUS_RemarksType()
		{
			base.CheckUS_RemarksType();
			if (Parent != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RemarksTypeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_RemarksTypeInfo, Report.AddInfoLookups.LabReportInformationTypeList);
			}
		}
	}
}
