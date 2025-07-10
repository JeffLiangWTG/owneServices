using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USAMSOR2AddInfoValidation : USAMSLineAddInfoValidation
	{
		public USAMSOR2AddInfoValidation(AutoUSAMSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_CertNumber()
		{
			base.CheckUS_CertNumber();

			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.US_CertNumberInfo);
			if (!parent.US_CertNumber.IsEmpty && !Regex.IsMatch(parent.US_CertNumber, @"(^[0-9]{3}-[0-9]{10}-[0-9]{6}$)|(^[0-9]{3}-[0-9]{3}-[A-Z]{1}$)", RegexOptions.IgnoreCase))
			{
				parent.US_CertNumberInfo.AddWarning(CertNumberFormatWarning);
			}
		}
		internal const string CertNumberFormatWarning = "Certificate Number should be in the format NNN-NNNNNNNNNN-NNNNNN or NNN-NNN-A where N is a numeric and A is alphabetic.";

		protected override void CheckUS_CertType()
		{
			base.CheckUS_CertType();

			var parent = Parent;
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.US_CertTypeInfo, AMSLineDetail.AddInfoLookups.CertTypeCodeList);
				if (!parent.US_CertNumber.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.US_CertTypeInfo);
				}
			}
		}
	}
}
