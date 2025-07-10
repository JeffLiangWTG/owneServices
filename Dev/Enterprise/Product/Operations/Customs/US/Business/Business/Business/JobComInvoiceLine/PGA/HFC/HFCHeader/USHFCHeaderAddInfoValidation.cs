//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSHFCHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSHFCHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USHFCHeaderAddInfoValidation : AutoUSHFCHeaderAddInfoValidation
	{
		public USHFCHeaderAddInfoValidation(AutoUSHFCHeaderAddInfo parent) : base(parent)
		{
		}

		new USHFCHeaderAddInfo Parent
		{
			get { return (USHFCHeaderAddInfo)base.Parent; }
		}

		protected USHFCHeader Header
		{
			get { return Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var hfcHeader = Header;
				if (hfcHeader != null)
				{
					var invoiceLine = hfcHeader.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		protected override void CheckUS_ASHRAENumber()
		{
			base.CheckUS_ASHRAENumber();

			if (IsPGAValidation)
			{
				var parent = Parent;
				if (!parent.US_ASHRAENumber.IsEmpty)
				{
					if (!Regex.IsMatch(parent.US_ASHRAENumber, @"^R-[0-9]{1,3}[A-Z]{1,3}$"))
					{
						parent.US_ASHRAENumberInfo.AddWarning(ASHRAENumberFormat);
					}
				}
				else if (Header.USHFCDetails.Count == 0)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.US_ASHRAENumberInfo);
				}
			}
		}
		internal const string ASHRAENumberFormat = "The format for the ASHRAE Item Identity Number is: A-NNNAAA (where A=alpha and N=numeric) and the first ALPHA always = R with up to 3 number and up to 3 alpha characters.";

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();

			if (IsPGAValidation)
			{
				var parent = Parent;
				MandatoryValidation.CheckNotNegative(parent.US_NetWeightInfo);
				MandatoryValidation.MessageErrorIfNotEntered(parent.US_NetWeightInfo);
			}
		}

		protected override void CheckUS_CertifyingIndividual()
		{
			base.CheckUS_CertifyingIndividual();

			if (IsPGAValidation)
			{
				var parent = Parent;
				ListValidation.MessageErrorIfInvalidCode(parent.US_CertifyingIndividualInfo, parent.Lookups.HFCCertifyingIndividualList);
			}
		}

		protected override void CheckUS_HFCImageSent()
		{
			base.CheckUS_HFCImageSent();

			var parent = Parent;
			if (IsPGAValidation && !parent.US_HFCImageSent)
			{
				parent.US_HFCImageSentInfo.AddMessageError(LabelRequired);
			}
		}
		internal const string LabelRequired = "This indicator is mandatory, please confirm Electronic Image Submitted.";
	}
}
